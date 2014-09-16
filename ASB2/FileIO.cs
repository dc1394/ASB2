//-----------------------------------------------------------------------
// <copyright file="FileIO.cs" company="dc1394's software">
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace FileMemWork
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using MyLogic;

    /// <summary>
    /// ファイルIO用のクラス
    /// </summary>
    internal sealed class FileIO : IDisposable
    {
        #region フィールド

        /// <summary>
        /// 待機中のフラグ
        /// </summary>
        internal const Int32 待機中 = 0;

        /// <summary>
        /// 動作中のフラグ
        /// </summary>
        internal const Int32 動作中 = 1;

        /// <summary>
        /// ループした回数
        /// </summary>
        private static Int64 loopNum = 0;

        /// <summary>
        /// 書き込まれたバイト数の合計
        /// </summary>
        private static Int64 totalWroteBytes = 0;

        /// <summary>
        /// バッファのサイズ
        /// </summary>
        private Int32 bufSize;

        /// <summary>
        /// IOの終了指示のフラグ
        /// </summary>
        private Boolean endFlag = false;

        /// <summary>
        /// 並列化するかどうかを示すフラグ
        /// </summary>
        private Boolean isParallel;

        /// <summary>
        /// ベリファイするかどうかを示すフラグ
        /// </summary>
        private Boolean isVerify;

        /// <summary>
        /// メモリー確保用のオブジェクト
        /// </summary>
        private MemoryAllocate ma;

        /// <summary>
        /// 読み込み専用のメモリー確保用のオブジェクト
        /// </summary>
        private MemoryAllocate maread = null;

        /// <summary>
        /// バッファに書き込むデリゲート
        /// </summary>
        private Action<MemoryAllocate> bufferWriteAction;

        /// <summary>
        /// バッファの内容を比較するデリゲート
        /// </summary>
        private Func<MemoryAllocate, MemoryAllocate, Boolean> bufCompare = null;

        /// <summary>
        /// バッファとディスクとの間で処理を行うデリゲート
        /// </summary>
        private Action bufBetweenDisk;

        /// <summary>
        /// バッファに書き込まれたかどうかを示すフラグ
        /// </summary>
        private Boolean bufferWroteFlag = false;

        /// <summary>
        /// IOが終わったかどうかを示すフラグ
        /// </summary>
        /// <remarks>複数のスレッドからアクセスされる！</remarks>
        private Int32 isNow;

        #endregion フィールド
        
        #region 構築・破棄

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bufSize">バッファのサイズ</param>
        /// <param name="isParallel">並列化するかどうか</param>
        /// <param name="isVerify">ベリファイをかけるかどうか</param>
        public FileIO(Int32 bufSize, Boolean isParallel, Boolean isVerify)
        {
            this.bufSize = bufSize;

            this.isParallel = isParallel;

            this.isVerify = isVerify;

            if (Environment.Is64BitProcess)
            {
                this.bufferWriteAction = UnsafeNativeMethods.writeMemory64;
            }
            else
            {
                this.bufferWriteAction = UnsafeNativeMethods.writeMemory32;
            }

            this.bufBetweenDisk = this.BufferToDisk;

            if (this.isVerify)
            {
                if (Environment.Is64BitProcess && this.isParallel)
                {
                    this.bufCompare = UnsafeNativeMethods.MemoryCmp64Parallel;
                }
                else if (Environment.Is64BitProcess)
                {
                    this.bufCompare = UnsafeNativeMethods.MemoryCmp64;
                }
                else if (this.isParallel)
                {
                    this.bufCompare = UnsafeNativeMethods.MemoryCmp32Parallel;
                }
                else
                {
                    this.bufCompare = UnsafeNativeMethods.MemoryCmp32;
                }

                this.maread = new MemoryAllocate(this.bufSize);
                this.bufBetweenDisk += this.DiskVerify;
            }

            this.bufBetweenDisk +=
                () =>
                {
                    try
                    {
                        // もし一時ファイルが残っていたらそのファイルを消去
                        File.Delete(this.FileName);
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                };

            this.ma = new MemoryAllocate(this.bufSize);
        }

        #endregion 構築・破棄

        #region プロパティ

        /// <summary>
        /// ループした回数
        /// </summary>
        internal static Int64 LoopNum
        {
            get { return FileIO.loopNum; }
        }

        /// <summary>
        /// 書き込まれたバイト数の合計
        /// </summary>
        internal static Int64 TotalWroteBytes
        {
            get { return FileIO.totalWroteBytes; }
        }

        /// <summary>
        /// バッファの内容が全て書き込まれたかどうかを示すフラグ
        /// </summary>
        internal Boolean AllBufferWrote { get; private set; }

        /// <summary>
        /// 処理をキャンセルする場合のトークン
        /// </summary>
        internal CancellationTokenSource Cts { get; set; }

        /// <summary>
        /// 一時ファイルのファイル名
        /// </summary>
        internal String FileName { private get; set; }

        /// <summary>
        /// 一時ファイルのファイルサイズ
        /// </summary>
        internal Int64 FileSize { private get; set; }

        /// <summary>
        /// ループした回数
        /// </summary>
        internal Boolean IsLoop { private get; set; }
        
        /// <summary>
        /// IOが終わったかどうか
        /// </summary>
        /// <remarks>複数のスレッドからアクセスされる！</remarks>
        internal Int32 IsNow
        {
            get
            {
                return this.isNow;
            }

            set
            {
                Interlocked.Exchange(ref this.isNow, value);
            }
        }

        /// <summary>
        /// 読み込んだ合計のバイト数
        /// </summary>
        /// <remarks>必ず初期化すること！</remarks>
        internal Int64 ReadBytes { get; set; }

        /// <summary>
        /// 書き込んだ合計のバイト数
        /// </summary>
        /// <remarks>必ず初期化すること！</remarks>
        internal Int64 WroteBytes { get; private set; }
        
        #endregion

        #region 破棄

        /// <summary>
        /// Disposeメソッド
        /// </summary>
        public void Dispose()
        {
            this.ma.Dispose();
            
            this.ma = null;

            if (this.maread != null)
            {
                this.maread.Dispose();

                this.maread = null;
            }

            if (File.Exists(this.FileName))
            {
                var fileNotAccess = false;
                while (!fileNotAccess)
                {
                    try
                    {
                        File.Delete(this.FileName);
                    }
                    catch (IOException)
                    {
                        continue;
                    }

                    fileNotAccess = true;
                }
            }
        }

        #endregion 破棄

        #region メソッド

        /// <summary>
        /// メインスレッド以外で投げられた例外をメインスレッドで投げ返す
        /// </summary>
        /// <param name="arg">例外</param>
        /// <returns>なし</returns>
        internal static Object ThrowMainThreadException(Object arg)
        {
            var ex = arg as Exception;
            MyError.WriteAndThrow<SystemException>(ex.Message, ex);

            return null;
        }

        /// <summary>
        /// 一時ファイルの読み書きを行う
        /// </summary>
        internal async void FileIORun()
        {
            if (this.Cts == null)
            {
                this.Cts = new CancellationTokenSource();
            }

            this.IsNow = FileIO.動作中;

            await Task.Run(
                () =>
                {
                    if (this.IsLoop)
                    {
                        while (true)
                        {
                            this.bufBetweenDisk();

                            if (this.endFlag)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        this.bufBetweenDisk();
                    }

                    this.IsNow = FileIO.待機中;
                },
                this.Cts.Token);

            this.Cts.Dispose();
            this.Cts = null;
        }

        /// <summary>
        /// バッファの内容をディスクに書き込む
        /// </summary>
        private void BufferToDisk()
        {
            if (this.isVerify)
            {
                this.bufferWroteFlag = false;
                this.AllBufferWrote = false;
            }

            using (var bw = new BinaryWriter(new FileStream(this.FileName, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                var ct = this.Cts.Token;

                try
                {
                    var max = this.FileSize / this.bufSize;
                    for (var i = 0; i < max; i++)
                    {
                        if (!this.MyWrite(
                            () =>
                            {
                                if (!(this.isVerify && this.bufferWroteFlag))
                                {
                                    this.bufferWriteAction(this.ma);
                                    this.bufferWroteFlag = true;
                                }
                            },
                            bw,
                            this.bufSize))
                        {
                            return;
                        }

                        this.WroteBytes = (i + 1) * this.bufSize;
                    }

                    var residue = (Int32)(this.FileSize % (Int64)this.bufSize);
                    if (residue != 0)
                    {
                        if (!this.MyWrite(
                            () =>
                            {
                                if (!this.isVerify)
                                {
                                    this.bufferWriteAction(this.ma);
                                }
                            },
                            bw,
                            residue))
                        {
                            return;
                        }

                        this.WroteBytes += (Int64)residue;
                    }

                    if (!this.isVerify)
                    {
                        FileIO.loopNum++;
                    }
                    else
                    {
                        this.AllBufferWrote = true;
                    }
                }
                catch (Exception ex)
                {
                    // 例外をキャッチし、メイン UI スレッドのメソッドに例外を渡します。
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Send,
                        new DispatcherOperationCallback(FileIO.ThrowMainThreadException),
                        ex);
                }
            }
        }

        /// <summary>
        /// ディスクの内容のベリファイを行う
        /// </summary>
        private void DiskVerify()
        {
            if (!this.endFlag)
            {
                try
                {
                    using (var br = new BinaryReader(File.Open(this.FileName, FileMode.Open, FileAccess.Read, FileShare.None)))
                    {
                        var max = this.FileSize / this.bufSize;
                        for (var i = 0; i < max; i++)
                        {
                            if (!this.MyVerify(br, this.bufSize))
                            {
                                return;
                            }

                            this.ReadBytes = (i + 1) * this.bufSize;
                        }

                        var residue = (Int32)(this.FileSize % (Int64)this.bufSize);
                        if (residue != 0)
                        {
                            if (!this.MyVerify(br, residue))
                            {
                                return;
                            }

                            this.ReadBytes += residue;
                        }
                    }

                    FileIO.loopNum++;
                }
                catch (Exception ex)
                {
                    // 例外をキャッチし、メイン UI スレッドのメソッドに例外を渡します。
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Send,
                        new DispatcherOperationCallback(FileIO.ThrowMainThreadException),
                        ex);
                }
            }
        }

        /// <summary>
        /// 一時ファイルのベリファイ作業を行う
        /// </summary>
        /// <param name="br">BinaryReaderオブジェクト</param>
        /// <param name="count">BinaryReaderで読み込むバイト数</param>
        /// <returns>読み込みが正常に終了したならtrue、それ以外の場合はfalse</returns>
        private Boolean MyVerify(BinaryReader br, Int32 count)
        {
            try
            {
                var ct = this.Cts.Token;

                // キャンセルされた場合例外をスロー           
                ct.ThrowIfCancellationRequested();

                br.Read(this.maread.Buf, this.maread.Offset, count);

                // キャンセルされた場合例外をスロー           
                ct.ThrowIfCancellationRequested();

                if (!this.bufCompare(this.ma, this.maread))
                {
                    throw new InvalidDataException(
                        String.Format(
                            "ベリファイに失敗しました。{0}プログラムのバグか、SSD/HDDが壊れています。",
                            Environment.NewLine));
                }
            }
            catch (OperationCanceledException)
            {
                this.endFlag = true;

                return false;
            }
            catch (InvalidDataException e)
            {
                MyError.CallErrorMessageBox(e.Message);

                this.endFlag = true;

                return false;
            }
            catch (IOException e)
            {
                MyError.CallErrorMessageBox(e.Message);

                this.endFlag = true;

                return false;
            }

            return true;
        }

        /// <summary>
        /// 一時ファイルに書き込み作業を行う
        /// </summary>
        /// <param name="act">BinaryWriterによる書き込み作業用のデリゲート</param>
        /// <param name="bw">BinaryWriterオブジェクト</param>
        /// <param name="count">BinaryWriterで書き込むバイト数</param>
        /// <returns>書き込みが正常に終了したならtrue、それ以外の場合はfalse</returns>
        private Boolean MyWrite(Action act, BinaryWriter bw, Int32 count)
        {
            try
            {
                var ct = this.Cts.Token;

                // キャンセルされた場合例外をスロー           
                ct.ThrowIfCancellationRequested();

                act();

                // キャンセルされた場合例外をスロー           
                ct.ThrowIfCancellationRequested();

                bw.Write(this.ma.Buf, this.ma.Offset, count);

                // キャンセルされた場合例外をスロー           
                ct.ThrowIfCancellationRequested();

                // 強制的にバッファをクリア
                bw.Flush();
            }
            catch (OperationCanceledException)
            {
                this.endFlag = true;

                return false;
            }
            catch (IOException e)
            {
                MyError.CallErrorMessageBox(e.Message);

                this.endFlag = true;

                return false;
            }

            FileIO.totalWroteBytes += (Int64)count;

            return true;
        }

        #endregion メソッド
    }
}

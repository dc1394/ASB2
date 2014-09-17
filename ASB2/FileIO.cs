﻿//-----------------------------------------------------------------------
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
    using System.Runtime.InteropServices;
    using System.Diagnostics;

    /// <summary>
    /// ファイルIO用のクラス
    /// </summary>
    internal sealed class FileIO : IDisposable
    {
        #region フィールド

        /// <summary>
        /// 待機中の定数
        /// </summary>
        internal const Int32 待機中 = 0;
        
        /// <summary>
        /// 動作中の定数
        /// </summary>
        internal const Int32 書き込み中 = 1;

        /// <summary>
        /// 動作中の定数
        /// </summary>
        internal const Int32 ベリファイ中 = 2;

        /// <summary>
        /// 終了待機中の定数
        /// </summary>
        internal const Int32 終了待機中 = 3;

        /// <summary>
        /// 未終了の定数
        /// </summary>
        internal const Int32 未終了 = 1;

        /// <summary>
        /// 正常終了の定数
        /// </summary>
        internal const Int32 正常終了 = 0;

        /// <summary>
        /// 異常終了の定数
        /// </summary>
        internal const Int32 異常終了 = -1;

        /// <summary>
        /// ループした回数
        /// </summary>
        private static Int64 loopNum = 0;

        /// <summary>
        /// 書き込まれたバイト数の合計
        /// </summary>
        private static Int64 totalWroteBytes = 0;

        /// <summary>
        /// バッファとディスクとの間で処理を行うデリゲート
        /// </summary>
        private Action bufBetweenDisk;

        /// <summary>
        /// バッファのサイズ
        /// </summary>
        private Int32 bufferSize;

        /// <summary>
        /// バッファの内容を比較するデリゲート
        /// </summary>
        private Action<MemoryAllocate, MemoryAllocate> bufferCompare = null;

        /// <summary>
        /// バッファに書き込むデリゲート
        /// </summary>
        private Action<MemoryAllocate> bufferWrite;

        /// <summary>
        /// バッファに書き込まれたかどうかを示すフラグ
        /// </summary>
        private Boolean bufferWroteFlag = false;

        /// <summary>
        /// IOが終わったかどうかを示すフラグ
        /// </summary>
        /// <remarks>複数のスレッドからアクセスされる！</remarks>
        private Int32 errorCode = FileIO.未終了;

        /// <summary>
        /// IOが終わったかどうかを示すフラグ
        /// </summary>
        /// <remarks>複数のスレッドからアクセスされる！</remarks>
        private Int32 isNow = FileIO.待機中;

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

        #endregion フィールド
        
        #region 構築

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bufferSize">バッファのサイズ</param>
        /// <param name="isParallel">並列化するかどうか</param>
        /// <param name="isVerify">ベリファイをかけるかどうか</param>
        public FileIO(Int32 bufferSize, Boolean isParallel, Boolean isVerify)
        {
            this.bufferSize = bufferSize;

            this.isParallel = isParallel;

            this.isVerify = isVerify;

            if (Environment.Is64BitProcess)
            {
                this.bufferWrite = UnsafeNativeMethods.writeMemory64;
            }
            else
            {
                this.bufferWrite = UnsafeNativeMethods.writeMemory32;
            }

            this.bufBetweenDisk = this.BufferToDisk;

            if (this.isVerify)
            {
                if (Environment.Is64BitProcess && this.isParallel)
                {
                    this.bufferCompare = UnsafeNativeMethods.MemoryCmp64Parallel;
                }
                else if (Environment.Is64BitProcess)
                {
                    this.bufferCompare = UnsafeNativeMethods.MemoryCmp64;
                }
                else if (this.isParallel)
                {
                    this.bufferCompare = UnsafeNativeMethods.MemoryCmp32Parallel;
                }
                else
                {
                    this.bufferCompare = UnsafeNativeMethods.MemoryCmp32;
                }

                this.maread = new MemoryAllocate(this.bufferSize);
                this.bufBetweenDisk += this.DiskVerify;
            }

            this.bufBetweenDisk +=
                () =>
                {
                    try
                    {
                        // もし一時ファイルが残っていたらそのファイルを消去
                        File.Delete(this.Filename);
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                };

            this.ma = new MemoryAllocate(this.bufferSize);
        }

        #endregion 構築

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
        /// 処理をキャンセルする場合のトークン
        /// </summary>
        internal CancellationTokenSource Cts { get; private set; }

        /// <summary>
        /// 処理が終了した場合のエラーコード
        /// </summary>
        /// <remarks>複数のスレッドからアクセスされる！</remarks>
        internal Int32 ErrorCode
        {
            get
            {
                return this.errorCode;
            }

            private set
            {
                Interlocked.Exchange(ref this.errorCode, value);
            }
        }

        /// <summary>
        /// 一時ファイルのファイル名
        /// </summary>
        internal String Filename { private get; set; }

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

            private set
            {
                Interlocked.Exchange(ref this.isNow, value);
            }
        }

        /// <summary>
        /// 読み込んだ合計のバイト数
        /// </summary>
        /// <remarks>必ず初期化すること！</remarks>
        internal Int64 ReadBytes { get; private set; }

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

            if (File.Exists(this.Filename))
            {
                var fileDelete = false;
                while (!fileDelete)
                {
                    try
                    {
                        File.Delete(this.Filename);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }
                    catch (IOException)
                    {
                        continue;
                    }

                    fileDelete = true;
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

            this.IsNow = FileIO.書き込み中;

            await Task.Run(
                () =>
                {
                    if (this.IsLoop)
                    {
                        while (true)
                        {
                            this.bufBetweenDisk();

                            switch (this.IsNow)
                            {
                                case FileIO.待機中:
                                    Debug.Assert(false, "IsNowが「待機中」になっている！");
                                    break;

                                case FileIO.書き込み中:
                                case FileIO.ベリファイ中:
                                    continue;

                                case FileIO.終了待機中:
                                    break;

                                default:
                                    Debug.Assert(false, "IsNowがありえない値になっている！");
                                    break;
                            }

                            break;
                        }
                    }
                    else
                    {
                        this.bufBetweenDisk();
                    }

                    this.IsNow = FileIO.終了待機中;
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
            }

            using (var bw = new BinaryWriter(new FileStream(this.Filename, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                var ct = this.Cts.Token;

                try
                {
                    var max = this.FileSize / this.bufferSize;
                    for (var i = 0; i < max; i++)
                    {
                        if (!this.MyWrite(
                            () =>
                            {
                                if (!(this.isVerify && this.bufferWroteFlag))
                                {
                                    this.bufferWrite(this.ma);
                                    this.bufferWroteFlag = true;
                                }
                            },
                            bw,
                            this.bufferSize))
                        {
                            return;
                        }

                        this.WroteBytes = (i + 1) * this.bufferSize;
                    }

                    var residue = (Int32)(this.FileSize % (Int64)this.bufferSize);
                    if (residue != 0)
                    {
                        if (!this.MyWrite(
                            () =>
                            {
                                if (!this.isVerify)
                                {
                                    this.bufferWrite(this.ma);
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
                        this.IsNow = FileIO.ベリファイ中;
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
            switch (this.IsNow)
            {
                case FileIO.待機中:
                    Debug.Assert(false, "IsNowが「待機中」になっている！");
                    break;

                case FileIO.書き込み中:
                    Debug.Assert(false, "IsNowが「書き込み中」になっている！");
                    break;

                case FileIO.ベリファイ中:
                    try
                    {
                        using (var br = new BinaryReader(File.Open(this.Filename, FileMode.Open, FileAccess.Read, FileShare.None)))
                        {
                            var max = this.FileSize / this.bufferSize;
                            for (var i = 0; i < max; i++)
                            {
                                if (!this.MyVerify(br, this.bufferSize))
                                {
                                    return;
                                }

                                this.ReadBytes = (i + 1) * this.bufferSize;
                            }

                            var residue = (Int32)(this.FileSize % (Int64)this.bufferSize);
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
                    break;

                case FileIO.終了待機中:
                    break;

                default:
                    Debug.Assert(false, "IsNowがありえない状態になっている！");
                    break;
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

                this.bufferCompare(this.ma, this.maread);
            }
            catch (OperationCanceledException)
            {
                this.ErrorCode = FileIO.正常終了;

                this.IsNow = FileIO.終了待機中;

                return false;
            }
            catch (IOException e)
            {
                MyError.CallErrorMessageBox(e.Message);

                this.ErrorCode = FileIO.異常終了;

                this.IsNow = FileIO.終了待機中;

                return false;
            }
            catch (SEHException)
            {
                MyError.CallErrorMessageBox(String.Format(
                        "ベリファイに失敗しました。{0}プログラムのバグか、SSD/HDDが壊れています。",
                        Environment.NewLine));

                this.ErrorCode = FileIO.異常終了;

                this.IsNow = FileIO.終了待機中;

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
                this.ErrorCode = FileIO.正常終了;

                this.IsNow = FileIO.終了待機中;

                return false;
            }
            catch (IOException e)
            {
                MyError.CallErrorMessageBox(e.Message);

                this.ErrorCode = FileIO.異常終了;

                this.IsNow = FileIO.終了待機中;

                return false;
            }

            FileIO.totalWroteBytes += (Int64)count;

            return true;
        }

        #endregion メソッド
    }
}

﻿//-----------------------------------------------------------------------
// <copyright file="FileIO.cs" company="dc1394's software">
//     Copyright © 2014-2018 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ASB2
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using MyLogic;

    /// <summary>
    /// ファイルIO用のクラス
    /// </summary>
    internal sealed class FileIo : IDisposable
    {
        #region フィールド

        /// <summary>
        /// ループした回数
        /// </summary>
        private static Int64 loopNum;

        /// <summary>
        /// 書き込まれたバイト数の合計
        /// </summary>
        private static Int64 totalWroteBytes;

        /// <summary>
        /// バッファとディスクとの間で処理を行うデリゲート
        /// </summary>
        private readonly Action bufBetweenDisk;

        /// <summary>
        /// バッファのサイズ
        /// </summary>
        private readonly Int32 bufferSize;

        /// <summary>
        /// バッファの内容を比較するデリゲート
        /// </summary>
        private readonly Func<MemoryAllocate, MemoryAllocate, Boolean> bufferCompare;

        /// <summary>
        /// ベリファイするかどうかを示すフラグ
        /// </summary>
        private readonly Boolean isVerify;

        /// <summary>
        /// IOが終わったかどうかを示すフラグ
        /// </summary>
        /// <remarks>複数のスレッドからアクセスされる！</remarks>
        private Int32 isNow;

        /// <summary>
        /// メモリー確保用のオブジェクト
        /// </summary>
        private MemoryAllocate ma;

        /// <summary>
        /// 読み込み専用のメモリー確保用のオブジェクト
        /// </summary>
        private MemoryAllocate maread;

        /// <summary>
        /// IOが終わったかどうかを示すフラグ
        /// </summary>
        /// <remarks>複数のスレッドからアクセスされる！</remarks>
        private FileIo.終了状態 returnCode;

        #endregion フィールド
        
        #region 構築

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bufferSize">バッファのサイズ</param>
        /// <param name="isParallel">並列化するかどうか</param>
        /// <param name="isVerify">ベリファイをかけるかどうか</param>
        public FileIo(Int32 bufferSize, Boolean isParallel, Boolean isVerify)
        {
            this.bufferSize = bufferSize;

            this.isVerify = isVerify;

            this.bufBetweenDisk = this.BufferToDisk;
            this.ma = new MemoryAllocate(this.bufferSize);

            if (this.isVerify)
            {
                if (isParallel)
                {
                    this.bufferCompare = UnsafeNativeMethods.MyMemoryCompareParallel;
                }
                else
                {
                    this.bufferCompare = UnsafeNativeMethods.MyMemoryCompare;
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
        }

        #endregion 構築
        
        #region 列挙型

        /// <summary>
        /// 現在の動作状態を表す列挙型
        /// </summary>
        internal enum 動作状態
        {
            /// <summary>
            /// 待機中
            /// </summary>
            待機中 = 0,

            /// <summary>
            /// ディスクに書き込み中
            /// </summary>
            書込中 = 1,

            /// <summary>
            /// ディスクに書き込んだファイルをベリファイ中
            /// </summary>
            ベリファイ中 = 2,

            /// <summary>
            /// 終了を待機中
            /// </summary>
            終了待機中 = 3
        }

        /// <summary>
        /// 終了の理由を表す列挙型
        /// </summary>
        internal enum 終了状態
        {
            /// <summary>
            /// 正常に終了
            /// </summary>
            正常終了 = 0,

            /// <summary>
            /// キャンセルされた
            /// </summary>
            キャンセル終了 = 1,

            /// <summary>
            /// 何らかの異常があって終了した
            /// </summary>
            異常終了 = -1,
        }

        #endregion 列挙型

        #region プロパティ

        /// <summary>
        /// ループした回数
        /// </summary>
        internal static Int64 LoopNum => FileIo.loopNum;

        /// <summary>
        /// 書き込まれたバイト数の合計
        /// </summary>
        internal static Int64 TotalWroteBytes => FileIo.totalWroteBytes;

        /// <summary>
        /// 処理をキャンセルする場合のトークン
        /// </summary>
        internal CancellationTokenSource Cts { get; private set; }

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
            get => this.isNow;

            private set => Interlocked.Exchange(ref this.isNow, value);
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
            if (arg is Exception ex)
            {
                MyError.WriteAndThrow<SystemException>(ex.Message, ex);
            }

            return null;
        }

        /// <summary>
        /// 一時ファイルの読み書きを行う
        /// </summary>
        /// <returns>FileIoクラスの終了状態をTask型でラップしたクラス</returns>
        internal async Task<FileIo.終了状態> FileIoRunAsync()
        {
            using (this.Cts = this.Cts ?? new CancellationTokenSource())
            {
                this.IsNow = (Int32)FileIo.動作状態.書込中;

                return await Task.Run(
                    () =>
                    {
                        if (this.IsLoop)
                        {
                            while (true)
                            {
                                this.bufBetweenDisk();

                                switch ((動作状態)this.IsNow)
                                {
                                    case FileIo.動作状態.書込中:
                                    case FileIo.動作状態.ベリファイ中:
                                        continue;

                                    case FileIo.動作状態.終了待機中:
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

                        this.IsNow = (Int32)FileIo.動作状態.終了待機中;

                        return this.returnCode;
                    },
                    this.Cts.Token);
            }
        }

        /// <summary>
        /// バッファの内容をディスクに書き込む
        /// </summary>
        private void BufferToDisk()
        {
            using (var bw = new BinaryWriter(new FileStream(this.Filename, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                try
                {
                    var max = this.FileSize / this.bufferSize;
                    UnsafeNativeMethods.WriteMemory(this.ma);
                    for (var i = 0; i < max; i++)
                    {
                        if (!this.MyWrite(bw, this.bufferSize))
                        {
                            return;
                        }

                        this.WroteBytes = (Int64)(i + 1) * (Int64)this.bufferSize;
                    }

                    var residue = (Int32)(this.FileSize % (Int64)this.bufferSize);
                    if (residue != 0)
                    {
                        if (!this.MyWrite(bw, residue))
                        {
                            return;
                        }

                        this.WroteBytes += (Int64)residue;
                    }
                    
                    if (!this.isVerify)
                    {
                        FileIo.loopNum++;
                    }
                    else
                    {
                        this.IsNow = (Int32)FileIo.動作状態.ベリファイ中;
                    }
                }
                catch (Exception ex)
                {
                    // 例外をキャッチし、メイン UI スレッドのメソッドに例外を渡します。
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Send,
                        new DispatcherOperationCallback(FileIo.ThrowMainThreadException),
                        ex);
                }
            }
        }

        /// <summary>
        /// ディスクの内容のベリファイを行う
        /// </summary>
        private void DiskVerify()
        {
            switch ((動作状態)this.IsNow)
            {
                case FileIo.動作状態.ベリファイ中:
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

                                this.ReadBytes = (Int64)(i + 1) * (Int64)this.bufferSize;
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

                        if (this.IsLoop)
                        {
                            this.WroteBytes = 0;
                            this.IsNow = (Int32)FileIo.動作状態.書込中;
                        }

                        FileIo.loopNum++;
                    }
                    catch (Exception ex)
                    {
                        // 例外をキャッチし、メイン UI スレッドのメソッドに例外を渡します。
                        Application.Current.Dispatcher.Invoke(
                            DispatcherPriority.Send,
                            new DispatcherOperationCallback(FileIo.ThrowMainThreadException),
                            ex);
                    }

                    break;

                case FileIo.動作状態.終了待機中:
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

                br.Read(this.maread.Buffer, this.maread.Offset, count);

                // キャンセルされた場合例外をスロー           
                ct.ThrowIfCancellationRequested();

                if (!this.bufferCompare(this.ma, this.maread))
                {
                    MyError.CallErrorMessageBox($"ベリファイに失敗しました。{Environment.NewLine}プログラムのバグか、SSD/HDDが壊れています。");

                    this.returnCode = FileIo.終了状態.異常終了;

                    this.IsNow = (Int32)FileIo.動作状態.終了待機中;

                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                this.returnCode = FileIo.終了状態.キャンセル終了;

                this.IsNow = (Int32)FileIo.動作状態.終了待機中;

                return false;
            }
            catch (IOException e)
            {
                MyError.CallErrorMessageBox(e.Message);

                this.returnCode = FileIo.終了状態.異常終了;

                this.IsNow = (Int32)FileIo.動作状態.終了待機中;

                return false;
            }

            this.returnCode = (Int32)FileIo.終了状態.正常終了;
            
            return true;
        }

        /// <summary>
        /// 一時ファイルに書き込み作業を行う
        /// </summary>
        /// <param name="bw">BinaryWriterオブジェクト</param>
        /// <param name="count">BinaryWriterで書き込むバイト数</param>
        /// <returns>書き込みが正常に終了したならtrue、それ以外の場合はfalse</returns>
        private Boolean MyWrite(BinaryWriter bw, Int32 count)
        {
            try
            {
                var ct = this.Cts.Token;

                // キャンセルされた場合例外をスロー           
                ct.ThrowIfCancellationRequested();

                bw.Write(this.ma.Buffer, this.ma.Offset, count);

                // キャンセルされた場合例外をスロー           
                ct.ThrowIfCancellationRequested();

                // 強制的にバッファをクリア
                bw.Flush();
            }
            catch (OperationCanceledException)
            {
                this.returnCode = FileIo.終了状態.キャンセル終了;

                this.IsNow = (Int32)FileIo.動作状態.終了待機中;

                return false;
            }
            catch (IOException e)
            {
                MyError.CallErrorMessageBox(e.Message);

                this.returnCode = FileIo.終了状態.異常終了;

                this.IsNow = (Int32)FileIo.動作状態.終了待機中;

                return false;
            }

            if (!this.isVerify)
            {
                this.returnCode = FileIo.終了状態.正常終了;
            }

            FileIo.totalWroteBytes += (Int64)count;

            return true;
        }

        #endregion メソッド
    }
}

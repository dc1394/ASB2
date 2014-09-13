namespace FileMemWork
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using MyLogic;

    internal sealed class FileIO : IDisposable
    {
        #region フィールド

        /// <summary>
        /// 待機中のフラグ
        /// </summary>
        private const Int32 待機中 = 0;

        /// <summary>
        /// 失敗のフラグ
        /// </summary>
        private const Int32 動作中 = 1;

        /// <summary>
        /// 書き込まれたバイト数の合計
        /// </summary>
        static private Int64 totalWroteBytes = 0;

        /// <summary>
        /// ループした回数
        /// </summary>
        static private Int64 loopNum = 0;

        /// <summary>
        /// バッファのサイズ
        /// </summary>
        private Int32 bufSize;

        /// <summary>
        /// 処理をキャンセルする場合のトークン
        /// </summary>
        private CancellationTokenSource cts = null;

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
        /// メモリに書き込むデリゲート
        /// </summary>
        private Action<MemoryAllocate> memoryWriteAction;

        /// <summary>
        /// メモリの内容を比較するデリゲート
        /// </summary>
        private Func<MemoryAllocate, MemoryAllocate, Boolean> memcmp = null;

        /// <summary>
        /// 実際に処理を行うデリゲート
        /// </summary>
        private Action run;

        /// <summary>
        /// メモリに書き込まれたかどうかを示すフラグ
        /// </summary>
        private Boolean memoryWroteFlag = false;

        /// <summary>
        /// IOが終わったかどうかを示すフラグ
        /// </summary>
        /// <remarks>
        /// 複数のスレッドからアクセスされる！
        /// </remarks>
        private Int32 isnow;

        #endregion フィールド
        
        #region 構築・破棄

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="target"></param>
        public FileIO(Int32 bufSize, Boolean isVerify, Boolean isParallel)
        {
            if (Environment.Is64BitProcess)
            {
                this.memoryWriteAction = UnsafeNativeMethods.writeMemory64;
            }
            else
            {
                this.memoryWriteAction = UnsafeNativeMethods.writeMemory32;
            }

            this.run = this.write;

            if (isVerify)
            {
                if (Environment.Is64BitProcess && isParallel)
                {
                    this.memcmp = UnsafeNativeMethods.MemoryCmp64Parallel;
                }
                else if (Environment.Is64BitProcess)
                {
                    this.memcmp = UnsafeNativeMethods.MemoryCmp64;
                }
                else if (isParallel)
                {
                    this.memcmp = UnsafeNativeMethods.MemoryCmp32Parallel;
                }
                else
                {
                    this.memcmp = UnsafeNativeMethods.MemoryCmp32;
                }
      
                this.run += this.readandverify;
                this.maread = new MemoryAllocate(bufSize);
            }

            this.ma = new MemoryAllocate(bufSize);

            this.bufSize = bufSize;
            this.isVerify = isVerify;
            this.isParallel = isParallel;
        }

        #region IDisposable メンバ

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();

            if (maread != null)
            {
                maread.Dispose();
            }

            ma.Dispose();
        }

        #endregion

        #endregion 構築・破棄

        #region プロパティ
        
        internal Boolean IsLoop { private get; set; }

        /// <summary>
        /// IOが終わったかどうか
        /// </summary>
        /// <remarks>
        /// 複数のスレッドからアクセスされる！
        /// </remarks>
        private Int32 Isnow
        {
            get
            {
                return this.isnow;
            }

            set
            {
                Interlocked.Exchange(ref this.isnow, value);
            }
        }

        internal String FileName { private get; set; }
        internal Int64 FileSize { private get; set; }
        internal Int64 WroteSize { get; private set; }
        internal Int64 ReadSize { get; private set; }
        internal Boolean AllMemoryWrited { get; private set; }

        static public Int64 TotalWrote
        {
            get { return totalWroteBytes; }
        }

        static public Int64 LoopNum
        {
            get { return loopNum; }
        }

        #endregion

        #region メソッド

        internal async void fileIORun()
        {
            if (this.cts == null)
            {
                this.cts = new CancellationTokenSource();
            }

            await Task.Run(
                () =>
                {
                    if (this.IsLoop)
                    {
                        while (true)
                        {
                            this.run();
                            
                            // ファイル消去
                            File.Delete(FileName);
                            if (this.endFlag)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        this.run();

                        // ファイル消去
                        File.Delete(FileName);
                    }
                },
                this.cts.Token);

            this.cts.Dispose();
            this.cts = null;
        }

        private void write()
        {
            try
            {
                if (isVerify)
                {
                    this.memoryWroteFlag = false;
                    this.AllMemoryWrited = false;
                }

                using (var bw = new BinaryWriter(new FileStream(this.FileName, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    var ct = this.cts.Token;

                    Int64 max = FileSize / bufSize;
                    for (Int64 i = 0; i < max; i++)
                    {
                        try
                        {
                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            if (!this.isVerify || !this.memoryWroteFlag)
                            {
                                this.memoryWriteAction(this.ma);
                                this.memoryWroteFlag = true;
                            }

                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            bw.Write(this.ma.Buf, this.ma.Offset, this.bufSize);

                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            // 強制的にバッファをクリア
                            bw.Flush();

                            this.WroteSize = (i + 1) * bufSize;
                            
                            FileIO.totalWroteBytes += bufSize;
                        }
                        catch (OperationCanceledException)
                        {
                            this.endFlag = true;
                            return;
                        }
                    }

                    var residue = (Int32)(FileSize % (Int64)bufSize);
                    if (residue != 0)
                    {
                        try
                        {
                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            if (!isVerify)
                            {
                                memoryWriteAction(ma);
                            }

                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            bw.Write(this.ma.Buf, this.ma.Offset, residue);

                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            // 強制的にバッファをクリア
                            bw.Flush();

                            WroteSize += (Int64)residue;
                            FileIO.totalWroteBytes += (Int64)residue;
                        }
                        catch (OperationCanceledException)
                        {
                            this.endFlag = true;
                        }
                    }
                }

                if (!this.isVerify)
                {
                    FileIO.loopNum++;
                }
                else
                {
                    this.AllMemoryWrited = true;
                }
            }
            catch (Exception ex)
            {
                // 例外をキャッチし、メイン UI スレッドのメソッドに例外を渡します。
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Send,
                                                      new DispatcherOperationCallback(ThrowMainThreadException),
                                                      ex);
            }
        }

        private void readandverify()
        {
            try
            {
                using (BinaryReader br = new BinaryReader(File.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.None)))
                {
                    var ct = cts.Token;

                    Int64 max = FileSize / bufSize;
                    for (Int64 i = 0; i < max; i++)
                    {
                        try
                        {
                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            br.Read(maread.Buf, maread.Offset, bufSize);

                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            if (!memcmp(ma, maread))
                            {
                                throw new InvalidDataException(@"ベリファイに失敗しました。
プログラムのバグか、SSD/HDDが壊れています。");
                            }

                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            ReadSize = (i + 1) * bufSize;
                        }
                        catch (OperationCanceledException)
                        {
                            endFlag = true;
                            return;
                        }
                        catch (InvalidDataException e)
                        {
                            ErrorDispose.callError(e.Message);
                            endFlag = true;
                            return;
                        }
                    }

                    Int32 residue = (Int32)(FileSize % (Int64)bufSize);
                    if (residue != 0)
                    {
                        try
                        {
                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            br.Read(maread.Buf, maread.Offset, residue);

                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            if (!memcmp(ma, maread))
                            {
                                throw new InvalidDataException(@"ベリファイに失敗しました。
プログラムのバグか、SSD/HDDが壊れています。");
                            }

                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            ReadSize += (Int64)residue;
                        }
                        catch (OperationCanceledException)
                        {
                            endFlag = true;
                        }
                        catch (InvalidDataException e)
                        {
                            ErrorDispose.callError(e.Message);
                            endFlag = true;
                        }
                    }
                }

                FileIO.loopNum++;
            }
            catch (Exception ex)
            {
                // 例外をキャッチし、メイン UI スレッドのメソッドに例外を渡します。
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Send,
                                                      new DispatcherOperationCallback(ThrowMainThreadException),
                                                      ex);
            }
        }

        private Object ThrowMainThreadException(Object arg)
        {
            throw new Exception("FileIOクラスの例外です。", (Exception)arg);
        }

        #endregion メソッド
    }
}

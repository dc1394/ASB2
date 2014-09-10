using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;

using MyLogic;

namespace FileMemWork
{
    public sealed class FileIO : IDisposable
    {
        #region フィールド
        
        private Action<MemoryAllocate> memorywrite;
        private Func<MemoryAllocate, MemoryAllocate, Boolean> memcmp = null;
        private Action run;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Task tas = null;

        private MemoryAllocate maread = null;
        private MemoryAllocate ma;

        private Int32 bufSize;
        private Boolean isVerify;
        private Boolean isParallel;
        private Boolean bMemoryWrited = false;
        private Boolean bEnd = false;

        static public Int64 totalWrote = 0;
        static public Int64 loopNum = 0;

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
                memorywrite = new Action<MemoryAllocate>(UnsafeNativeMethods.writeMemory64);
            }
            else
            {
                memorywrite = new Action<MemoryAllocate>(UnsafeNativeMethods.writeMemory32);
            }

            run = new Action(write);

            if (isVerify)
            {
                if (Environment.Is64BitProcess && isParallel)
                {
                    memcmp = new Func<MemoryAllocate, MemoryAllocate, Boolean>(UnsafeNativeMethods.MemoryCmp64Parallel);
                }
                else if (Environment.Is64BitProcess)
                {
                    memcmp = new Func<MemoryAllocate, MemoryAllocate, Boolean>(UnsafeNativeMethods.MemoryCmp64);
                }
                else if (isParallel)
                {
                    memcmp = new Func<MemoryAllocate, MemoryAllocate, Boolean>(UnsafeNativeMethods.MemoryCmp32Parallel);
                }
                else
                {
                    memcmp = new Func<MemoryAllocate, MemoryAllocate, Boolean>(UnsafeNativeMethods.MemoryCmp32);
                }
      
                run += new Action(readandverify);
                maread = new MemoryAllocate(bufSize);
            }

            ma = new MemoryAllocate(bufSize);

            this.bufSize = bufSize;
            this.isVerify = isVerify;
            this.isParallel = isParallel;
        }

        #region IDisposable メンバ

        public void Dispose()
        {
            cts.Cancel();
            while (Tas.Status != TaskStatus.RanToCompletion)
            {
            }
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

        public CancellationTokenSource Cts { get { return cts; } }
        public Task Tas { get { return tas; } }
        public Boolean IsLoop { private get; set; }
        public String FileName { private get; set; }
        public Int64 FileSize { private get; set; }
        public Int64 WroteSize { get; private set; }
        public Int64 ReadSize { get; private set; }
        public Boolean AllMemoryWrited { get; private set; }

        static public Int64 TotalWrote
        {
            get { return totalWrote; }
        }
        static public Int64 LoopNum
        {
            get { return loopNum; }
        }

        #endregion

        #region メソッド

        public void fileIORun()
        {
            tas = Task.Factory.StartNew(() =>
                {
                    if (IsLoop)
                    {
                        while (true)
                        {
                            run();
                            // ファイル消去
                            File.Delete(FileName);
                            if (bEnd)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        run();
                        // ファイル消去
                        File.Delete(FileName);
                    }
                });
        }

        private void write()
        {
            try
            {
                if (isVerify)
                {
                    bMemoryWrited = false;
                    AllMemoryWrited = false;
                }
                using (BinaryWriter bw = new BinaryWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.None)))
                {
                    var ct = cts.Token;
                    
                    Int64 max = FileSize / bufSize;
                    for (Int64 i = 0; i < max; i++)
                    {
                        try
                        {
                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            if (!isVerify || !bMemoryWrited)
                            {
                                memorywrite(ma);
                                bMemoryWrited = true;
                            }

                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            bw.Write(ma.Buf, ma.Offset, bufSize);

                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            // 強制的にバッファをクリア
                            bw.Flush();
                            WroteSize = (i + 1) * bufSize;
                            FileIO.totalWrote += bufSize;
                        }
                        catch (OperationCanceledException)
                        {
                            bEnd = true;
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

                            if (!isVerify)
                            {
                                memorywrite(ma);
                            }

                            // キャンセルされた場合例外をスロー           
                            ct.ThrowIfCancellationRequested();

                            bw.Write(ma.Buf, ma.Offset, residue);
                            // 強制的にバッファをクリア
                            bw.Flush();

                            WroteSize += (Int64)residue;
                            FileIO.totalWrote += (Int64)residue;
                        }
                        catch (OperationCanceledException)
                        {
                            bEnd = true;
                        }
                    }
                }

                if (!isVerify)
                {
                    FileIO.loopNum++;
                }
                else
                {
                    AllMemoryWrited = true;
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
                            bEnd = true;
                            return;
                        }
                        catch (InvalidDataException e)
                        {
                            ErrorDispose.callError(e.Message);
                            bEnd = true;
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
                            bEnd = true;
                        }
                        catch (InvalidDataException e)
                        {
                            ErrorDispose.callError(e.Message);
                            bEnd = true;
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

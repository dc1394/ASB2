using System;
using System.Runtime;

using MyLogic;

namespace FileMemWork
{
    internal sealed class MemoryAllocate : IDisposable
    {
        #region フィールド

        private const Byte FIFTEEN = 15;
        private const UInt32 MASK_FFFFFFF0h = 0xFFFFFFF0;
        private const UInt64 MASK_FFFFFFFFFFFFFFF0h = 0xFFFFFFFFFFFFFFF0;
        
        private Int32 bufSize;

        private MemoryFailPoint mfp;

        #endregion フィールド

        #region 構築・破棄

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="target"></param>
        internal unsafe MemoryAllocate(Int32 bufSize)
        {
            Int32 i = bufSize / (Int32)Pow.pow(2L, 20U);
            mfp = (i != 0) ? new MemoryFailPoint(i) : null;
            this.bufSize = bufSize;
            Buf = new Byte[bufSize + FIFTEEN];

            fixed (Byte* p = &Buf[0])
            {
                if (Environment.Is64BitProcess)
                {
                    UInt64 ptr = (UInt64)p;
                    Address64 = ((ptr + (UInt64)FIFTEEN) & MASK_FFFFFFFFFFFFFFF0h);
                    Offset = (Int32)(Address64 - ptr);
                }
                else
                {
                    UInt32 ptr = (UInt32)p;
                    Address32 = ((ptr + (UInt32)FIFTEEN) & MASK_FFFFFFF0h);
                    Offset = (Int32)(Address32 - ptr);
                }
            }
        }

        #region IDisposable メンバ

        public void Dispose()
        {
            // メモリ解放
            if (mfp != null)
            {
                mfp.Dispose();
            }
        }

        #endregion

        #endregion 構築・破棄

        #region プロパティ

        internal Int32 BufSize
        {
            get { return bufSize; }
        }

        internal UInt64 Address64
        { get; private set; }
        
        internal UInt32 Address32
        { get; private set; }
        
        internal Int32 Offset
        { get; private set; }

        internal Byte[] Buf
        { get; private set; }

        #endregion
    }
}

//-----------------------------------------------------------------------
// <copyright file="MemoryAllocate.cs" company="dc1394's software">
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
{
    using System;
    using System.Runtime;
    using MyLogic;

    /// <summary>
    /// メモリのアロケートを行うクラス
    /// </summary>
    internal sealed class MemoryAllocate : IDisposable
    {
        #region フィールド

        /// <summary>
        /// 15を表す定数
        /// </summary>
        private const Byte SIXTYTHREE = 63;

        /// <summary>
        /// x86のときのビットマスク
        /// </summary>
        private const UInt32 MASKOfFFFFFFC0h = 0xFFFFFFC0;

        /// <summary>
        /// x64のときのビットマスク
        /// </summary>
        private const UInt64 MASKOfFFFFFFFFFFFFFFC0h = 0xFFFFFFFFFFFFFFC0;

        /// <summary>
        /// 1MByte以上のメモリを確保するときに使う
        /// MemoryFailPointオブジェクト
        /// </summary>
        private MemoryFailPoint mfp;

        #endregion フィールド

        #region 構築

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bufSize">メモリの大きさ</param>
        internal unsafe MemoryAllocate(Int32 bufSize)
        {
            this.BufferSize = bufSize;
            
            var i = this.BufferSize / (Int32)Pow.pow(2L, 20U);
            
            this.mfp = (i != 0) ? new MemoryFailPoint(i) : null;
            
            this.Buffer = new Byte[this.BufferSize + MemoryAllocate.SIXTYTHREE];

            fixed (Byte* p = &this.Buffer[0])
            {
                if (Environment.Is64BitProcess)
                {
                    var ptr = (UInt64)p;
                    this.Address64 = (ptr + (UInt64)MemoryAllocate.SIXTYTHREE) & MemoryAllocate.MASKOfFFFFFFFFFFFFFFC0h;
                    this.Offset = (Int32)(this.Address64 - ptr);
                }
                else
                {
                    var ptr = (UInt32)p;
                    this.Address32 = (ptr + (UInt32)MemoryAllocate.SIXTYTHREE) & MemoryAllocate.MASKOfFFFFFFC0h;
                    this.Offset = (Int32)(this.Address32 - ptr);
                }
            }
        }

        #endregion 構築

        #region プロパティ

        /// <summary>
        /// x86におけるメモリの先頭アドレス
        /// </summary>
        internal UInt32 Address32 { get; private set; }

        /// <summary>
        /// x64におけるメモリの先頭アドレス
        /// </summary>
        internal UInt64 Address64 { get; private set; }

        /// <summary>
        /// メモリ
        /// </summary>
        internal Byte[] Buffer { get; private set; }

        /// <summary>
        /// メモリの大きさ
        /// </summary>
        internal Int32 BufferSize { get; private set; }

        /// <summary>
        /// メモリのオフセット
        /// </summary>
        internal Int32 Offset { get; private set; }

        #endregion プロパティ

        #region 破棄

        /// <summary>
        /// Disposeメソッド
        /// </summary>
        public void Dispose()
        {
            // メモリ解放
            if (this.mfp != null)
            {
                this.mfp.Dispose();
                this.mfp = null;
            }
        }

        #endregion 破棄
    }
}

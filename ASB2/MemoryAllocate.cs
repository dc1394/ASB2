//-----------------------------------------------------------------------
// <copyright file="MemoryAllocate.cs" company="dc1394's software">
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace FileMemWork
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
        private const Byte FIFTEEN = 15;

        /// <summary>
        /// x86のときのビットマスク
        /// </summary>
        private const UInt32 MASKOfFFFFFFF0h = 0xFFFFFFF0;

        /// <summary>
        /// x64のときのビットマスク
        /// </summary>
        private const UInt64 MASKOfFFFFFFFFFFFFFFF0h = 0xFFFFFFFFFFFFFFF0;

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
        /// <param name="bufSize">バッファの大きさ</param>
        internal unsafe MemoryAllocate(Int32 bufSize)
        {
            this.BufferSize = bufSize;
            
            var i = this.BufferSize / (Int32)Pow.pow(2L, 20U);
            
            this.mfp = (i != 0) ? new MemoryFailPoint(i) : null;
            
            this.Buffer = new Byte[this.BufferSize + FIFTEEN];

            fixed (Byte* p = &this.Buffer[0])
            {
                if (Environment.Is64BitProcess)
                {
                    var ptr = (UInt64)p;
                    this.Address64 = (ptr + (UInt64)MemoryAllocate.FIFTEEN) & MemoryAllocate.MASKOfFFFFFFFFFFFFFFF0h;
                    this.Offset = (Int32)(this.Address64 - ptr);
                }
                else
                {
                    var ptr = (UInt32)p;
                    this.Address32 = (ptr + (UInt32)MemoryAllocate.FIFTEEN) & MemoryAllocate.MASKOfFFFFFFF0h;
                    this.Offset = (Int32)(this.Address32 - ptr);
                }
            }
        }

        #endregion 構築

        #region プロパティ

        /// <summary>
        /// x86におけるバッファの先頭アドレス
        /// </summary>
        internal UInt32 Address32 { get; private set; }

        /// <summary>
        /// x64におけるバッファの先頭アドレス
        /// </summary>
        internal UInt64 Address64 { get; private set; }

        /// <summary>
        /// バッファ
        /// </summary>
        internal Byte[] Buffer { get; private set; }

        /// <summary>
        /// バッファの大きさ
        /// </summary>
        internal Int32 BufferSize { get; private set; }

        /// <summary>
        /// バッファのオフセット
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

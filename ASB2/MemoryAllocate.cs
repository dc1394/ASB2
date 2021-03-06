﻿//-----------------------------------------------------------------------
// <copyright file="MemoryAllocate.cs" company="dc1394's software">
//     Copyright © 2014-2018 @dc1394 All Rights Reserved.
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
        /// 63を表す定数
        /// </summary>
        private const Byte Sixtythree = 63;

        /// <summary>
        /// x86のときのビットマスク
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private const UInt32 MaskOfFFFFFFC0h = 0xFFFFFFC0;

        /// <summary>
        /// x64のときのビットマスク
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private const UInt64 MaskOfFFFFFFFFFFFFFFC0h = 0xFFFFFFFFFFFFFFC0;

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
            
            this.Buffer = new Byte[this.BufferSize + MemoryAllocate.Sixtythree];

            fixed (Byte* p = &this.Buffer[0])
            {
                if (Environment.Is64BitProcess)
                {
                    var ptr = (UInt64)p;
                    var address = (ptr + (UInt64)MemoryAllocate.Sixtythree) & MemoryAllocate.MaskOfFFFFFFFFFFFFFFC0h;
                    this.Offset = (Int32)(address - ptr);
                }
                else
                {
                    var ptr = (UInt32)p;
                    var address = (ptr + (UInt32)MemoryAllocate.Sixtythree) & MemoryAllocate.MaskOfFFFFFFC0h;
                    this.Offset = (Int32)(address - ptr);
                }
            }
        }

        #endregion 構築

        #region プロパティ

        /// <summary>
        /// メモリ
        /// </summary>
        internal Byte[] Buffer { get; }

        /// <summary>
        /// メモリの大きさ
        /// </summary>
        internal Int32 BufferSize { get; }

        /// <summary>
        /// メモリのオフセット
        /// </summary>
        internal Int32 Offset { get; }

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

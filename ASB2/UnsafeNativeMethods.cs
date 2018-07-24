//-----------------------------------------------------------------------
// <copyright file="UnsafeNativeMethods.cs" company="dc1394's software">
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace FileMemWork
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// DLLから関数を呼び出すクラス
    /// </summary>
    internal static class UnsafeNativeMethods
    {
        #region メソッド

        /// <summary>
        /// 引数で指定された二つのバッファの内容を比較する（x86）
        /// </summary>
        /// <param name="ma1">バッファのラッパーオブジェクト1</param>
        /// <param name="ma2">バッファのラッパーオブジェクト2</param>
        internal static unsafe void MemoryCmp32(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufferSize == ma2.BufferSize, "バッファ1とバッファ2のサイズが等しくない！");

            memcmp128((Byte*)ma1.Address32, (Byte*)ma2.Address32, (UInt32)ma1.BufferSize);
        }

        /// <summary>
        /// 引数で指定された二つのバッファの内容を並列で比較する（x86）
        /// </summary>
        /// <param name="ma1">バッファのラッパーオブジェクト1</param>
        /// <param name="ma2">バッファのラッパーオブジェクト2</param>
        internal static unsafe void MemoryCmp32Parallel(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufferSize == ma2.BufferSize, "バッファ1とバッファ2のサイズが等しくない！");

            memcmpparallel128((Byte*)ma1.Address32, (Byte*)ma2.Address32, (UInt32)ma1.BufferSize);
        }

        /// <summary>
        /// 引数で指定された二つのバッファの内容を比較する（x64）
        /// </summary>
        /// <param name="ma1">バッファのラッパーオブジェクト1</param>
        /// <param name="ma2">バッファのラッパーオブジェクト2</param>
        internal static unsafe void MemoryCmp64(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufferSize == ma2.BufferSize, "バッファ1とバッファ2のサイズが等しくない！");

            memcmp128((Byte*)ma1.Address64, (Byte*)ma2.Address64, (UInt32)ma1.BufferSize);
        }

        /// <summary>
        /// 引数で指定された二つのバッファの内容を並列で比較する（x64）
        /// </summary>
        /// <param name="ma1">バッファのラッパーオブジェクト1</param>
        /// <param name="ma2">バッファのラッパーオブジェクト2</param>
        internal static unsafe void MemoryCmp64Parallel(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufferSize == ma2.BufferSize, "バッファ1とバッファ2のサイズが等しくない！");

            memcmpparallel128((Byte*)ma1.Address64, (Byte*)ma2.Address64, (UInt32)ma1.BufferSize);
        }

        /// <summary>
        /// 引数で指定されたバッファに書き込む（x86）
        /// </summary>
        /// <param name="ma">バッファのラッパーオブジェクト</param>
        internal static unsafe void WriteMemory32(MemoryAllocate ma)
        {
            memfill128((Byte*)ma.Address32, (UInt32)ma.BufferSize);
        }

        /// <summary>
        /// 引数で指定されたバッファに書き込む（x64）
        /// </summary>
        /// <param name="ma">バッファのラッパーオブジェクト</param>
        internal static unsafe void WriteMemory64(MemoryAllocate ma)
        {
            memfill128((Byte*)ma.Address64, (UInt32)ma.BufferSize);
        }

        [DllImport("MemWork.dll", EntryPoint = "memcmp128")]
        private static unsafe extern void memcmp128(Byte* p1, Byte* p2, UInt32 size);

        [DllImport("MemWork.dll", EntryPoint = "memcmpparallel128")]
        private static unsafe extern void memcmpparallel128(Byte* p1, Byte* p2, UInt32 size);

        [DllImport("MemWork.dll", EntryPoint = "memfill128")]
        private static unsafe extern void memfill128(Byte* p, UInt32 size);

        #endregion メソッド
    }
}

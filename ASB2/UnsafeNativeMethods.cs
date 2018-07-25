//-----------------------------------------------------------------------
// <copyright file="UnsafeNativeMethods.cs" company="dc1394's software">
//     Copyright ©  2014 @dc1394 All Rights Reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace ASB2
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
        /// <returns>比較した二つのバッファの内容が同じだったかどうか</returns>
        internal static Boolean MemoryCompare32(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufferSize == ma2.BufferSize, "バッファ1とバッファ2のサイズが等しくない！");

            return MemoryCmpSimd((IntPtr)ma1.Address32, (IntPtr)ma2.Address32, (UInt32)ma1.BufferSize);
        }

        /// <summary>
        /// 引数で指定された二つのバッファの内容を並列で比較する（x86）
        /// </summary>
        /// <param name="ma1">バッファのラッパーオブジェクト1</param>
        /// <param name="ma2">バッファのラッパーオブジェクト2</param>
        /// <returns>比較した二つのバッファの内容が同じだったかどうか</returns>
        internal static Boolean MemoryCompare32Parallel(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufferSize == ma2.BufferSize, "バッファ1とバッファ2のサイズが等しくない！");

            return MemoryCmpParallelSimd((IntPtr)ma1.Address32, (IntPtr)ma2.Address32, (UInt32)ma1.BufferSize);
        }

        /// <summary>
        /// 引数で指定された二つのバッファの内容を比較する（x64）
        /// </summary>
        /// <param name="ma1">バッファのラッパーオブジェクト1</param>
        /// <param name="ma2">バッファのラッパーオブジェクト2</param>
        /// <returns>比較した二つのバッファの内容が同じだったかどうか</returns>
        internal static Boolean MemoryCompare64(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufferSize == ma2.BufferSize, "バッファ1とバッファ2のサイズが等しくない！");

            return MemoryCmpSimd((IntPtr)ma1.Address64, (IntPtr)ma2.Address64, (UInt32)ma1.BufferSize);
        }

        /// <summary>
        /// 引数で指定された二つのバッファの内容を並列で比較する（x64）
        /// </summary>
        /// <param name="ma1">バッファのラッパーオブジェクト1</param>
        /// <param name="ma2">バッファのラッパーオブジェクト2</param>
        /// <returns>比較した二つのバッファの内容が同じだったかどうか</returns>
        internal static Boolean MemoryCompare64Parallel(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufferSize == ma2.BufferSize, "バッファ1とバッファ2のサイズが等しくない！");

            return MemoryCmpParallelSimd((IntPtr)ma1.Address64, (IntPtr)ma2.Address64, (UInt32)ma1.BufferSize);
        }

        /// <summary>
        /// 引数で指定されたバッファに書き込む（x86）
        /// </summary>
        /// <param name="ma">バッファのラッパーオブジェクト</param>
        internal static void WriteMemory32(MemoryAllocate ma)
        {
            MemoryFillSimd((IntPtr)ma.Address32, (UInt32)ma.BufferSize);
        }

        /// <summary>
        /// 引数で指定されたバッファに書き込む（x64）
        /// </summary>
        /// <param name="ma">バッファのラッパーオブジェクト</param>
        internal static unsafe void WriteMemory64(MemoryAllocate ma)
        {
            MemoryFillSimd((IntPtr)ma.Address64, (UInt32)ma.BufferSize);
        }

        [DllImport("memwork", EntryPoint = "memcmpsimd")]
        private static extern Boolean MemoryCmpSimd(IntPtr p1, IntPtr p2, UInt32 size);

        [DllImport("memwork", EntryPoint = "memcmpparallelsimd")]
        private static extern Boolean MemoryCmpParallelSimd(IntPtr p1, IntPtr p2, UInt32 size);

        [DllImport("memwork", EntryPoint = "memfillsimd")]
        private static extern void MemoryFillSimd(IntPtr p, UInt32 size);

        #endregion メソッド
    }
}

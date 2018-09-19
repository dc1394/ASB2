//-----------------------------------------------------------------------
// <copyright file="UnsafeNativeMethods.cs" company="dc1394's software">
//     Copyright © 2014-2018 @dc1394 All Rights Reserved.
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
        /// 引数で指定された二つのバッファの内容を比較する
        /// </summary>
        /// <param name="ma1">バッファのラッパーオブジェクト1</param>
        /// <param name="ma2">バッファのラッパーオブジェクト2</param>
        /// <returns>比較した二つのバッファの内容が同じだったかどうか</returns>
        internal static unsafe Boolean MyMemoryCompare(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufferSize == ma2.BufferSize, "バッファ1とバッファ2のサイズが等しくない！");

            fixed (Byte* p1 = &ma1.Buffer[0])
            {
                fixed (Byte* p2 = &ma2.Buffer[0])
                {
                    return MemoryCmpSimd((IntPtr)(p1 + ma1.Offset), (IntPtr)(p2 + ma2.Offset), (UInt32)ma1.BufferSize) == 1;
                }
            }
        }

        /// <summary>
        /// 引数で指定された二つのバッファの内容を並列で比較する
        /// </summary>
        /// <param name="ma1">バッファのラッパーオブジェクト1</param>
        /// <param name="ma2">バッファのラッパーオブジェクト2</param>
        /// <returns>比較した二つのバッファの内容が同じだったかどうか</returns>
        internal static unsafe Boolean MyMemoryCompareParallel(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufferSize == ma2.BufferSize, "バッファ1とバッファ2のサイズが等しくない！");

            fixed (Byte* p1 = &ma1.Buffer[0])
            {
                fixed (Byte* p2 = &ma2.Buffer[0])
                {
                    return MemoryCmpParallelSimd((IntPtr)(p1 + ma1.Offset), (IntPtr)(p2 + ma2.Offset), (UInt32)ma1.BufferSize) == 1;
                }
            }
        }

        /// <summary>
        /// 引数で指定されたバッファに書き込む
        /// </summary>
        /// <param name="ma">バッファのラッパーオブジェクト</param>
        internal static unsafe void WriteMemory(MemoryAllocate ma)
        {
            fixed (Byte* p = &ma.Buffer[0])
            {
                MemoryFillSimd((IntPtr)(p + ma.Offset), (UInt32)ma.BufferSize);
            }
        }

        [DllImport("memwork", EntryPoint = "memcmpsimd")]
        private static extern Int32 MemoryCmpSimd(IntPtr p1, IntPtr p2, UInt32 size);

        [DllImport("memwork", EntryPoint = "memcmpparallelsimd")]
        private static extern Int32 MemoryCmpParallelSimd(IntPtr p1, IntPtr p2, UInt32 size);

        [DllImport("memwork", EntryPoint = "memfillsimd")]
        private static extern void MemoryFillSimd(IntPtr p, UInt32 size);

        #endregion メソッド
    }
}

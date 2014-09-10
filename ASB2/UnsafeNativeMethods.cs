using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FileMemWork
{
    internal static class UnsafeNativeMethods
    {
        [DllImport("MemWork.dll")]
        unsafe static extern void memfill128(Byte* p, UInt32 size);
        [DllImport("MemWork.dll")]
        unsafe static extern Boolean memcmp128(Byte* p1, Byte* p2, UInt32 size);
        [DllImport("MemWork.dll")]
        unsafe static extern Boolean memcmpparallel128(Byte* p1, Byte* p2, UInt32 size);

        #region staticメソッド

        internal unsafe static void writeMemory64(MemoryAllocate ma)
        {
            memfill128((Byte*)ma.Address64, (UInt32)ma.BufSize);
        }

        internal unsafe static void writeMemory32(MemoryAllocate ma)
        {
            memfill128((Byte*)ma.Address32, (UInt32)ma.BufSize);
        }

        internal unsafe static Boolean MemoryCmp64Parallel(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufSize == ma2.BufSize, "バグってます！！");

            return memcmpparallel128((Byte*)ma1.Address64, (Byte*)ma2.Address64, (UInt32)ma1.BufSize);
        }

        internal unsafe static Boolean MemoryCmp64(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufSize == ma2.BufSize, "バグってます！！");
            
            return memcmp128((Byte*)ma1.Address64, (Byte*)ma2.Address64, (UInt32)ma1.BufSize);
        }

        internal unsafe static Boolean MemoryCmp32Parallel(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufSize == ma2.BufSize, "バグってます！！");

            return memcmpparallel128((Byte*)ma1.Address32, (Byte*)ma2.Address32, (UInt32)ma1.BufSize);
        }

        internal unsafe static Boolean MemoryCmp32(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufSize == ma2.BufSize, "バグってます！！");
            
            return memcmp128((Byte*)ma1.Address32, (Byte*)ma2.Address32, (UInt32)ma1.BufSize);
        }

        #endregion staticメソッド
    }
}

namespace FileMemWork
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class UnsafeNativeMethods
    {
        [DllImport("MemWork.dll")]
        unsafe static extern void memcmp128(Byte* p1, Byte* p2, UInt32 size);

        [DllImport("MemWork.dll")]
        unsafe static extern void memcmpparallel128(Byte* p1, Byte* p2, UInt32 size);
        
        [DllImport("MemWork.dll")]
        unsafe static extern void memfill128(Byte* p, UInt32 size);

        #region staticメソッド

        internal unsafe static void writeMemory64(MemoryAllocate ma)
        {
            memfill128((Byte*)ma.Address64, (UInt32)ma.BufSize);
        }

        internal unsafe static void writeMemory32(MemoryAllocate ma)
        {
            memfill128((Byte*)ma.Address32, (UInt32)ma.BufSize);
        }

        internal unsafe static void MemoryCmp64Parallel(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufSize == ma2.BufSize, "バグってます！！");

            memcmpparallel128((Byte*)ma1.Address64, (Byte*)ma2.Address64, (UInt32)ma1.BufSize);
        }

        internal unsafe static void MemoryCmp64(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufSize == ma2.BufSize, "バグってます！！");
            
            memcmp128((Byte*)ma1.Address64, (Byte*)ma2.Address64, (UInt32)ma1.BufSize);

        }

        internal unsafe static void MemoryCmp32Parallel(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufSize == ma2.BufSize, "バグってます！！");

            memcmpparallel128((Byte*)ma1.Address32, (Byte*)ma2.Address32, (UInt32)ma1.BufSize);
        }

        internal unsafe static void MemoryCmp32(MemoryAllocate ma1, MemoryAllocate ma2)
        {
            Debug.Assert(ma1.BufSize == ma2.BufSize, "バグってます！！");

            memcmp128((Byte*)ma1.Address32, (Byte*)ma2.Address32, (UInt32)ma1.BufSize);
        }

        #endregion staticメソッド
    }
}

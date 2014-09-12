#include <cstdint>
#include <intrin.h>

#ifndef _WIN64
	#pragma comment(linker, "/export:memfill128=_memfill128@8")
	#pragma comment(linker, "/export:memcmp128=_memcmp128@12")
	#pragma comment(linker, "/export:memcmpparallel128=_memcmpparallel128@12")
#endif

#ifdef __cplusplus
	#define DLLEXPORT extern "C" __declspec(dllexport)
#else
	#define DLLEXPORT __declspec(dllexport)
#endif

namespace {
#ifdef __INTEL_COMPILER
	static constexpr std::uint32_t SSE4_1_FLAG = 0x00080000;
	static constexpr std::uint32_t PREFETCHSIZE = 4096;
	static constexpr std::uint64_t FFFFFFFFFFFFFFFFh = 0xFFFFFFFFFFFFFFFF;
#else
	static const std::uint32_t SSE4_1_FLAG = 0x00080000;
	static const std::uint32_t PREFETCHSIZE = 4096;
	static const std::uint64_t FFFFFFFFFFFFFFFFh = 0xFFFFFFFFFFFFFFFF;
#endif
	inline bool availableSSE4_1()
	{
		int CPUInfo[4];
		::__cpuid(CPUInfo, 1);

		return static_cast<bool>(CPUInfo[2] & SSE4_1_FLAG);
	}
}

DLLEXPORT void __stdcall memfill128(std::uint8_t * p, std::uint32_t size);
DLLEXPORT bool __stdcall memcmp128(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size);
DLLEXPORT bool __stdcall memcmpparallel128(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size);

#ifndef _MEMWORK_H_
#define _MEMWORK_H_

#pragma once

#include <array>
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
	static auto constexpr SSE4_1_FLAG = 0x00080000;
	static auto constexpr PREFETCHSIZE = 4096;
	static auto constexpr FFFFFFFFFFFFFFFFh = 0xFFFFFFFFFFFFFFFF;

	inline bool availableSSE4_1()
	{
		std::array<std::int32_t, 4> CPUInfo;
		::__cpuid(CPUInfo.data(), 1);

		return static_cast<bool>(CPUInfo[2] & SSE4_1_FLAG);
	}
}

DLLEXPORT bool __stdcall memcmp128(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size);
DLLEXPORT bool __stdcall memcmpparallel128(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size);
DLLEXPORT void __stdcall memfill128(std::uint8_t * p, std::uint32_t size);

#endif  // _MEMWORK_H_

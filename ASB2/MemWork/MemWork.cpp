/*! \file memwork.cpp
    \brief メモリに書き込む関数群の実装

    Copyright © 2014-2018 @dc1394 All Rights Reserved.
    This software is released under the BSD 2-Clause License.
*/
#include "memwork.h"
#include "myrandom/myrand.h"
//#include "myrandom/myrandavx512.h"
//#include "myrandom/myrandsfmt.h"
#include <bitset>
#include <cassert>                  // for assert
#include <stdexcept>                // for std::runtime_error
#include <tuple>                    // for std::tuple
#include <vector>                   // for std::vector
#include <intrin.h>

#include <tbb/parallel_for.h>

void buffercompareuseAVX2(std::uint32_t index, std::uint8_t * p1, std::uint8_t * p2)
{
    auto const s1 = reinterpret_cast<char const *>(p1) + (index << 8);
    auto const s2 = reinterpret_cast<char const *>(p2) + (index << 8);

    // キャッシュ汚染を防ぐ
    ::_mm_prefetch(s1 + PREFETCHSIZE, _MM_HINT_NTA);

    // メモリからymm0～16にロード
    std::array<__m256i, 8> ymm, ymm2;
    for (auto j = 0; j < 8; j++) {
        ymm[j] = _mm256_loadu_si256(reinterpret_cast<__m256i const *>(s1 + j * sizeof(__m256i)));
        ymm2[j] = _mm256_loadu_si256(reinterpret_cast<__m256i const *>(s2 + j * sizeof(__m256i)));
    }

    // 結果を比較
    for (auto j = 0; j < 8; j++) {
        // 256ビット（16バイト）ごとに結果をチェック
        auto const subresult = _mm256_sub_epi64(ymm[j], ymm2[j]);
        if (!_mm256_testz_si256(subresult, subresult)) {
            throw std::runtime_error("");
        }
    }
}

void buffercompareuseSSE41(bool availableSSE41, std::uint32_t index, std::uint8_t * p1, std::uint8_t * p2)
{
    auto const s1 = reinterpret_cast<char const *>(p1) + (index << 6);
    auto const s2 = reinterpret_cast<char const *>(p2) + (index << 6);

    // キャッシュ汚染を防ぐ
    ::_mm_prefetch(s1 + PREFETCHSIZE, _MM_HINT_NTA);

    // メモリからxmm0～7にロード
    std::array<__m128i, 4> xmm, xmm2;
    for (auto j = 0; j < 4; j++) {
        xmm[j] = ::_mm_load_si128(reinterpret_cast<__m128i const *>(s1 + j * sizeof(__m128i)));
        xmm2[j] = ::_mm_load_si128(reinterpret_cast<__m128i const *>(s2 + j * sizeof(__m128i)));
    }

    // 結果を比較
    std::array<__m128i, 4> r;
    for (auto j = 0; j < 4; j++) {
        if (availableSSE41) {
            r[j] = ::_mm_cmpeq_epi64(xmm[j], xmm2[j]);
        }
        else {
            r[j] = ::_mm_cmpeq_epi32(xmm[j], xmm2[j]);
        }

        // 64ビット（8バイト）ごとに結果をチェック
        for (auto k = 0; k < 2; k++) {
            if (r[j].m128i_u64[k] != FFFFFFFFFFFFFFFFh)
                throw std::runtime_error("");
        }
    }
}

std::pair<AvailSIMDtype, std::uint32_t> check(std::uint8_t const * p1, std::uint8_t const * p2, std::uint32_t size)
{
    // アライメントはあっているか？
#ifdef _WIN64
        assert(!(reinterpret_cast<std::uint64_t>(p1)& 0x0F));
        assert(!(reinterpret_cast<std::uint64_t>(p2)& 0x0F));
#else
        assert(!(reinterpret_cast<std::uint32_t>(p1)& 0x0F));
        assert(!(reinterpret_cast<std::uint32_t>(p2)& 0x0F));
#endif
    // 書き込むバイト数は64バイトの倍数か？
    assert(!(size & 0x3F));

    // SIMD命令はどれが使用可能か
    auto const availsimdtype = isAvailableSIMDtype();

    // 比較する回数
    std::uint32_t compareloopnum;

    switch (availsimdtype) {
    case AvailSIMDtype::NOAVAIL:
    case AvailSIMDtype::AVAILSSE41:
        compareloopnum = size >> 6;
        break;

    case AvailSIMDtype::AVAILAVX2:
        compareloopnum = size >> 8;
        break;
    }
    
    return std::make_pair(availsimdtype, compareloopnum);
}

AvailSIMDtype isAvailableSIMDtype()
{
    std::bitset<32> f_1_ecx = { 0 };
    std::bitset<32> f_7_ebx = { 0 };
    std::vector< std::array<int, 4> > data{};
    std::vector< std::array<int, 4> > extdata{};

    std::array<int, 4> cpuinfo = { -1 };

    // Calling __cpuid with 0x0 as the function_id argument  
    // gets the number of the highest valid function ID.  
    __cpuid(cpuinfo.data(), 0);
    auto const nids = cpuinfo[0];

    for (auto i = 0; i <= nids; ++i) {
        __cpuidex(cpuinfo.data(), i, 0);
        data.push_back(cpuinfo);
    }

    // load bitset with flags for function 0x00000001  
    if (nids >= 1) {
        f_1_ecx = data[1][2];
    }

    // load bitset with flags for function 0x00000007  
    if (nids >= 7) {
        f_7_ebx = data[7][1];
    }

/*    if (f_7_ebx[16]) {
        return AvailSIMDtype::AVAILAVX512;
    }
    else*/ if (f_7_ebx[5]) {
        return AvailSIMDtype::AVAILAVX2;
    }
    else if (f_1_ecx[19]) {
        return AvailSIMDtype::AVAILSSE41;
    }
    else {
        return AvailSIMDtype::NOAVAIL;
    }
}

void memcmpAVX2(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size, std::uint32_t compareloopnum)
{
    // 実際に1回のループで256バイトずつ比較
    for (std::uint32_t i = 0; i < compareloopnum; i++) {
        buffercompareuseAVX2(i, p1, p2);
    }
}

DLLEXPORT void __stdcall memcmpsimd(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size)
{
    AvailSIMDtype availsimdtype;
    std::uint32_t compareloopnum;

    std::tie(availsimdtype, compareloopnum) = check(p1, p2, size);

    switch (availsimdtype) {
    case AvailSIMDtype::NOAVAIL:
        memcmpSSE(false, p1, p2, size, compareloopnum);
        break;

    case AvailSIMDtype::AVAILSSE41:
        memcmpSSE(true, p1, p2, size, compareloopnum);
        break;

    case AvailSIMDtype::AVAILAVX2:
        memcmpAVX2(p1, p2, size, compareloopnum);
        break;
    }
}

void memcmpSSE(bool availableSSE41, std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size, std::uint32_t compareloopnum)
{
    // 実際に1回のループで64バイトずつ比較
    for (std::uint32_t i = 0; i < compareloopnum; i++) {
        buffercompareuseSSE41(availableSSE41, i, p1, p2);
    }
}

DLLEXPORT void __stdcall memcmpparallelsimd(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size)
{
    AvailSIMDtype availsimdtype;
    std::uint32_t compareloopnum;

    std::tie(availsimdtype, compareloopnum) = check(p1, p2, size);

    switch (availsimdtype) {
    case AvailSIMDtype::NOAVAIL:
        tbb::parallel_for(
            std::uint32_t(0),
            compareloopnum,
            std::uint32_t(1),
            [=](std::uint32_t i) { buffercompareuseSSE41(false, i, p1, p2); });
        break;

    case AvailSIMDtype::AVAILSSE41:
        tbb::parallel_for(
            std::uint32_t(0),
            compareloopnum,
            std::uint32_t(1),
            [=](std::uint32_t i) { buffercompareuseSSE41(true, i, p1, p2); });
        break;

    case AvailSIMDtype::AVAILAVX2:
        tbb::parallel_for(
            std::uint32_t(0),
            compareloopnum,
            std::uint32_t(1),
            [=](std::uint32_t i) { buffercompareuseAVX2(i, p1, p2); });
        break;
    }
}

DLLEXPORT void __stdcall memfill128(std::uint8_t * p, std::uint32_t size)
{
    // 乱数オブジェクト生成
    myrandom::MyRand mr;

    // アライメントはあっているか？
#ifdef _WIN64
        assert(!(reinterpret_cast<std::uint64_t>(p) & 0x0F));
#else
        assert(!(reinterpret_cast<std::uint32_t>(p) & 0x0F));
#endif
    // 書き込むバイト数は64バイトの倍数か？
    assert(!(size & 0x3F));

    // 1回のループで書き込む回数（64バイト（512ビット）ずつ）
    auto const write512loop = size >> 6;

    // 実際に1回のループで64バイトずつ書き込む
    for (std::uint32_t i = 0; i < write512loop; i++) {
        std::array<__m128i, 4> xmm;
        // xmm0～3レジスタ上に乱数生成
        for (auto j = 0; j < 4; j++) {
            alignas(16) std::array<std::uint32_t, 4> xmmtemp = { mr.myrand(), mr.myrand(), mr.myrand(), mr.myrand() };
            xmm[j] = ::_mm_load_si128(reinterpret_cast<__m128i const *>(xmmtemp.data()));
        }

        // アドレス
        auto * const d = p + (i << 6);
        // 実際に書き込む
        for (auto j = 0; j < 4; j++) {
            ::_mm_stream_si128(reinterpret_cast<__m128i *>(d + j * sizeof(__m128i)), xmm[j]);
        }
    }
}


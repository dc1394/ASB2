﻿/*! \file memwork.cpp
    \brief メモリに書き込む関数群の実装

    Copyright © 2014-2018 @dc1394 All Rights Reserved.
    This software is released under the BSD 2-Clause License.
*/
#include "memwork.h"

#include "myrandom/myrandsfmt.h"
#include <array>                    // for std::array
#include <bitset>                   // for std::bitset
#include <cassert>                  // for assert
#include <functional>               // for std::plus
#include <tuple>                    // for std::tie
#include <vector>                   // for std::vector
#include <emmintrin.h>              // for _mm_cmpeq_epi32, _mm_load_si128, _mm_prefetch, _mm_stream_si128
#include <smmintrin.h>              // for _mm_cmpeq_epi64
#include <immintrin.h>              // for _mm256_load_si256, _mm256_store_si256, _mm256_sub_epi64, _mm256_testz_si256
#include <zmmintrin.h>              // for _mm512_cmpeq_epu64_mask, _mm512_load_si512, _mm512_store_si512

#include <tbb/parallel_for.h>       // for tbb::blocked_range
#include <tbb/parallel_reduce.h>    // for tbb::parallel_reduce

std::pair<AvailSIMDtype, std::uint32_t> getinfo(std::uint8_t const * p1, std::uint8_t const * p2, std::uint32_t size)
{
    // 比較する回数
    std::uint32_t compareloopnum;

    // SIMD命令はどれが使用可能か
    auto const availsimdtype = isAvailableSIMDtype();

    switch (availsimdtype) {
    case AvailSIMDtype::AVAILSSE2:
    case AvailSIMDtype::AVAILSSE41:
        compareloopnum = size >> 6;
        break;

    case AvailSIMDtype::AVAILAVX2:
        compareloopnum = size >> 8;
        break;

    case AvailSIMDtype::AVAILAVX512:
        compareloopnum = size >> 10;
        break;

    default:
        assert(!"switchのdefaultに来てしまった！");
        break;
    }
    
    return std::make_pair(availsimdtype, compareloopnum);
}

AvailSIMDtype isAvailableSIMDtype()
{
    std::bitset<32> f_1_ecx = { 0 };
    std::bitset<32> f_7_ebx = { 0 };
    std::vector< std::array<std::int32_t, 4> > data{};

    std::array<std::int32_t, 4> cpuinfo = { -1 };

    // Calling __cpuid with 0x0 as the function_id argument  
    // gets the number of the highest valid function ID.  
    __cpuid(cpuinfo.data(), 0);
    auto const nids = cpuinfo[0];

    for (auto i = 0; i <= nids; i++) {
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

    if (f_7_ebx[16]) {
        return AvailSIMDtype::AVAILAVX512;
    }

    if (f_7_ebx[5]) {
        return AvailSIMDtype::AVAILAVX2;
    }
    else if (f_1_ecx[19]) {
        return AvailSIMDtype::AVAILSSE41;
    }
    else {
        return AvailSIMDtype::AVAILSSE2;
    }
}

bool memcmpAVX2(std::uint32_t cmploopnum, std::uint8_t const * p1, std::uint8_t const * p2)
{
    // 実際に1回のループで256バイトずつ比較
    for (auto i = 0U; i < cmploopnum; i++) {
        if (!memcmpuseAVX2(i, p1, p2)) {
            return false;
        }
    }

    return true;
}

bool memcmpAVX512(std::uint32_t cmploopnum, std::uint8_t const * p1, std::uint8_t const * p2)
{
    // 実際に1回のループで1024バイトずつ比較
    for (auto i = 0U; i < cmploopnum; i++) {
        if (!memcmpuseAVX512(i, p1, p2)) {
            return false;
        }
    }

    return true;
}

DLLEXPORT std::int32_t __stdcall memcmpsimd(std::uint8_t const * p1, std::uint8_t const * p2, std::uint32_t size)
{
    AvailSIMDtype availsimdtype;
    std::uint32_t cmploopnum;

    std::tie(availsimdtype, cmploopnum) = getinfo(p1, p2, size);

    switch (availsimdtype) {
    case AvailSIMDtype::AVAILSSE2:
        return memcmpSSE(false, cmploopnum, p1, p2) ? 1 : 0;

    case AvailSIMDtype::AVAILSSE41:
        return memcmpSSE(true, cmploopnum, p1, p2) ? 1 : 0;

    case AvailSIMDtype::AVAILAVX2:
        return memcmpAVX2(cmploopnum, p1, p2) ? 1 : 0;

    case AvailSIMDtype::AVAILAVX512:
        return memcmpAVX512(cmploopnum, p1, p2) ? 1 : 0;

    default:
        assert(!"AvailSIMDtypeがあり得ない値になっている！");
        break;
    }

    return false;
}

bool memcmpSSE(bool availSSE41, std::uint32_t cmploopnum, std::uint8_t const * p1, std::uint8_t const * p2)
{
    // 実際に1回のループで64バイトずつ比較
    for (auto i = 0U; i < cmploopnum; i++) {
        if (!memcmpuseSSE(availSSE41, i, p1, p2)) {
            return false;
        }
    }

    return true;
}

DLLEXPORT std::int32_t __stdcall memcmpparallelsimd(std::uint8_t const * p1, std::uint8_t const * p2, std::uint32_t size)
{
    AvailSIMDtype availsimdtype;
    std::uint32_t cmploopnum;

    std::tie(availsimdtype, cmploopnum) = getinfo(p1, p2, size);

    auto result = 0U;

    switch (availsimdtype) {
    case AvailSIMDtype::AVAILSSE2:
        result = tbb::parallel_reduce(
            tbb::blocked_range<std::uint32_t>(0U, cmploopnum),
            0,
            [p1, p2](tbb::blocked_range<std::uint32_t> const & range, std::uint32_t sumlocal) {
                for (auto && i = range.begin(); i != range.end(); ++i) {
                    sumlocal += memcmpuseSSE(false, i, p1, p2) ? 1 : 0;
                }
                return sumlocal;
            },
            std::plus<>());
        break;

    case AvailSIMDtype::AVAILSSE41:
        result = tbb::parallel_reduce(
            tbb::blocked_range<std::uint32_t>(0, cmploopnum),
            0,
            [p1, p2](tbb::blocked_range<std::uint32_t> const & range, std::uint32_t sumlocal) {
                for (auto && i = range.begin(); i != range.end(); ++i) {
                    sumlocal += memcmpuseSSE(true, i, p1, p2) ? 1 : 0;
                }
                return sumlocal;
            },
            std::plus<>());
        break;

    case AvailSIMDtype::AVAILAVX2:
        result = tbb::parallel_reduce(
            tbb::blocked_range<std::uint32_t>(0, cmploopnum),
            0,
            [p1, p2](tbb::blocked_range<std::uint32_t> const & range, std::uint32_t sumlocal) {
                for (auto && i = range.begin(); i != range.end(); ++i) {
                    sumlocal += memcmpuseAVX2(i, p1, p2) ? 1 : 0;
                }
                return sumlocal;
            },
            std::plus<>());
        break;

    case AvailSIMDtype::AVAILAVX512:
        result = tbb::parallel_reduce(
            tbb::blocked_range<std::uint32_t>(0, cmploopnum),
            0,
            [p1, p2](tbb::blocked_range<std::uint32_t> const & range, std::uint32_t sumlocal) {
                for (auto && i = range.begin(); i != range.end(); ++i) {
                    sumlocal += memcmpuseAVX512(i, p1, p2) ? 1 : 0;
                }
                return sumlocal;
            },
            std::plus<>());
        break;

    default:
        assert(!"AvailSIMDtypeがあり得ない値になっている！");
        break;
    }

    return result == cmploopnum ? 1 : 0;
}

bool memcmpuseAVX2(std::uint32_t index, std::uint8_t const * p1, std::uint8_t const * p2)
{
    // 1回のループで256バイト（2048ビット）ずつ比較
    auto const s1 = reinterpret_cast<char const *>(p1) + (index << 8);
    auto const s2 = reinterpret_cast<char const *>(p2) + (index << 8);

    // キャッシュ汚染を防ぐ
    ::_mm_prefetch(s1 + PREFETCHSIZE, _MM_HINT_NTA);

    // メモリからymm0～16にロード
    std::array<__m256i, 8> ymm{}, ymm2{};
    for (auto j = 0; j < 8; j++) {
        ymm[j] = _mm256_load_si256(reinterpret_cast<__m256i const *>(s1 + j * sizeof(__m256i)));
        ymm2[j] = _mm256_load_si256(reinterpret_cast<__m256i const *>(s2 + j * sizeof(__m256i)));
    }

    // 結果を比較
    for (auto j = 0; j < 8; j++) {
        // 256ビット（32バイト）ごとに結果をチェック
        auto const subresult = _mm256_sub_epi64(ymm[j], ymm2[j]);
        if (!_mm256_testz_si256(subresult, subresult)) {
            return false;
        }
    }

    return true;
}

bool memcmpuseAVX512(std::uint32_t index, std::uint8_t const * p1, std::uint8_t const * p2)
{
    // 1回のループで1024バイト（8192ビット）ずつ比較
    auto const s1 = reinterpret_cast<char const *>(p1) + (index << 10);
    auto const s2 = reinterpret_cast<char const *>(p2) + (index << 10);

    // キャッシュ汚染を防ぐ
    ::_mm_prefetch(s1 + PREFETCHSIZE, _MM_HINT_NTA);

    // メモリからzmm0～32にロード
    std::array<__m512i, 16> zmm{}, zmm2{};
    for (auto j = 0; j < 16; j++) {
        zmm[j] = _mm512_load_si512(reinterpret_cast<__m512i const *>(s1 + j * sizeof(__m512i)));
        zmm2[j] = _mm512_load_si512(reinterpret_cast<__m512i const *>(s2 + j * sizeof(__m512i)));
    }

    // 結果を比較
    for (auto j = 0; j < 16; j++) {
        // 512ビット（64バイト）ごとに結果をチェック
        auto const mask = _mm512_cmpeq_epu64_mask(zmm[j], zmm2[j]);
        if (mask != 0xFF) {
            return false;
        }
    }

    return true;
}

bool memcmpuseSSE(bool availableSSE41, std::uint32_t index, std::uint8_t const * p1, std::uint8_t const * p2)
{
    // 1回のループで64バイト（512ビット）ずつ比較
    auto const s1 = reinterpret_cast<char const *>(p1) + (index << 6);
    auto const s2 = reinterpret_cast<char const *>(p2) + (index << 6);

    // キャッシュ汚染を防ぐ
    ::_mm_prefetch(s1 + PREFETCHSIZE, _MM_HINT_NTA);

    // メモリからxmm0～7にロード
    std::array<__m128i, 4> xmm{}, xmm2{};
    for (auto j = 0; j < 4; j++) {
        xmm[j] = _mm_load_si128(reinterpret_cast<__m128i const *>(s1 + j * sizeof(__m128i)));
        xmm2[j] = _mm_load_si128(reinterpret_cast<__m128i const *>(s2 + j * sizeof(__m128i)));
    }

    // 結果を比較
    std::array<__m128i, 4> r{};
    for (auto j = 0; j < 4; j++) {
        // 128ビット（8バイト）ごとに結果をチェック
        {
            if (availableSSE41) {
                r[j] = _mm_cmpeq_epi64(xmm[j], xmm2[j]);
            }
            else {
                r[j] = _mm_cmpeq_epi32(xmm[j], xmm2[j]);
            }

            // 64ビット（8バイト）ごとに結果をチェック
            for (auto k = 0; k < 2; k++) {
                if (r[j].m128i_u64[k] != FFFFFFFFFFFFFFFFh) {
                    return false;
                }
            }
        }
    }

    return true;
}

void memfillAVX2(std::uint8_t * p, std::uint32_t size)
{
    // 1回のループで書き込む回数（512バイト（4096ビット）ずつ）
    auto const writeloop = size >> 9;

    // 乱数オブジェクト生成
    myrandom::MyRandSfmt mr;

    // 実際に1回のループで512バイトずつ書き込む
    for (auto i = 0U; i < writeloop; i++) {
        std::array<__m256i, 16> ymm{};
        
        // ymm0～15レジスタ上に乱数生成
        for (auto j = 0; j < 16; j++) {
            alignas(32) std::array<std::uint32_t, 8> ymmtemp{};
            for (auto && item : ymmtemp) {
                item = mr.myrand();
            }
            ymm[j] = _mm256_load_si256(reinterpret_cast<__m256i const *>(ymmtemp.data()));
        }

        // アドレス
        auto * const d = p + (i << 9);
        // 実際に書き込む
        for (auto j = 0; j < 16; j++) {
            _mm256_store_si256(reinterpret_cast<__m256i *>(d + j * sizeof(__m256i)), ymm[j]);
        }
    }
}

void memfillAVX512(std::uint8_t * p, std::uint32_t size)
{
    // 1回のループで書き込む回数（2048バイト（16384ビット）ずつ）
    auto const writeloop = size >> 11;

    // 乱数オブジェクト生成
    myrandom::MyRandSfmt mr;

    // 実際に1回のループで2048バイトずつ書き込む
    for (auto i = 0U; i < writeloop; i++) {
        std::array<__m512i, 32> zmm{};

        // zmm0～31レジスタ上に乱数生成
        for (auto j = 0; j < 32; j++) {
            alignas(64) std::array<std::uint32_t, 16> zmmtemp{};
            for (auto && item : zmmtemp) {
                item = mr.myrand();
            }
            zmm[j] = _mm512_load_si512(reinterpret_cast<__m512i const *>(zmmtemp.data()));
        }

        // アドレス
        auto * const d = p + (i << 11);
        // 実際に書き込む
        for (auto j = 0; j < 32; j++) {
            _mm512_store_si512(reinterpret_cast<__m512i *>(d + j * sizeof(__m512i)), zmm[j]);
        }
    }

    // sizeを2048バイトで割った余り
    auto const remainder = size & 0x07FF;

    // 余りの分のループ回数
    auto const loop = remainder / 4;

    // 余りの分の開始アドレス
    auto const address = p + (writeloop << 11);

    // 余りを4バイトずつ書き込む
    for (auto i = 0U; i < loop; i++) {
        auto * const d = reinterpret_cast<std::uint32_t * const>(address + i * 4);
        *d = mr.myrand();
    }
}

DLLEXPORT void __stdcall memfillsimd(std::uint8_t * p, std::uint32_t size)
{
    switch (isAvailableSIMDtype()) {
    case AvailSIMDtype::AVAILSSE2:
    case AvailSIMDtype::AVAILSSE41:
        memfillSSE2(p, size);
        break;

    case AvailSIMDtype::AVAILAVX2:
        memfillAVX2(p, size);
        break;

    case AvailSIMDtype::AVAILAVX512:
        memfillAVX512(p, size);
        break;

    default:
        assert(!"switchのdefaultに来てしまった！");
        break;
    }    
}

void memfillSSE2(std::uint8_t * p, std::uint32_t size)
{
    // 1回のループで書き込む回数（128バイト（1024ビット）ずつ）
    auto const writeloop = size >> 7;

    // 乱数オブジェクト生成
    myrandom::MyRandSfmt mr;

    // 実際に1回のループで128バイトずつ書き込む
    for (auto i = 0U; i < writeloop; i++) {
        std::array<__m128i, 8> xmm{};
        // xmm0～7レジスタ上に乱数生成
        for (auto j = 0; j < 8; j++) {
            alignas(16) std::array<std::uint32_t, 4> xmmtemp = { mr.myrand(), mr.myrand(), mr.myrand(), mr.myrand() };
            xmm[j] = _mm_load_si128(reinterpret_cast<__m128i const *>(xmmtemp.data()));
        }

        // アドレス
        auto * const d = p + (i << 7);
        // 実際に書き込む
        for (auto j = 0; j < 8; j++) {
            _mm_stream_si128(reinterpret_cast<__m128i *>(d + j * sizeof(__m128i)), xmm[j]);
        }
    }
}

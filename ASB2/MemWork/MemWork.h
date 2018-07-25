﻿/*! \file memwork.h
    \brief メモリに書き込む関数群の宣言

    Copyright © 2014-2018 @dc1394 All Rights Reserved.
*/
#ifndef _MEMWORK_H_
#define _MEMWORK_H_

#pragma once

#include <array>        // for std::array
#include <cstdint>      // for std::int32_t
#include <utility>      // for std::pair

#ifndef _WIN64
    #pragma comment(linker, "/export:memcmpsimd=_memcmpsimd@12")
    #pragma comment(linker, "/export:memcmpparallelsimd=_memcmpparallelsimd@12")
    #pragma comment(linker, "/export:memfill128=_memfill128@8")
#endif

#ifdef __cplusplus
    #define DLLEXPORT extern "C" __declspec(dllexport)
#else
    #define DLLEXPORT __declspec(dllexport)
#endif

//! A enumerated type
/*!
    SSE4.1、AVX2、AVX-512が使用可能かどうかの列挙型
*/
enum class AvailSIMDtype : std::int32_t {
    // どれも使用不能
    NOAVAIL = 0,

    // SSE4.1使用可能
    AVAILSSE41 = 1,

    // AVX2使用可能
    AVAILAVX2 = 2,

    // AVX-512使用可能
    AVAILAVX512 = 3
};

//! A static variable (constant expression).
/*!
    64ビット符号なし整数の最大値
*/
static auto constexpr FFFFFFFFFFFFFFFFh = 0xFFFFFFFFFFFFFFFF;

//! A static variable (constant expression).
/*!
    プリフェッチのサイズ
*/
static auto constexpr PREFETCHSIZE = 4096;

//! A global function.
/*!
    バッファを比較する（AVX2を使う）
    \param index バッファを比較するときに足すインデックス
    \param p1 比較するバッファ1の先頭アドレス
    \param p2 比較するバッファ2の先頭アドレス
*/
void buffercompareuseAVX2(std::uint32_t index, std::uint8_t * p1, std::uint8_t * p2);

//! A global function.
/*!
    バッファを比較する（SSE4.1を使う）
    \param availableSSE41 SSE4.1が使用可能かどうか
    \param index バッファを比較するときに足すインデックス
    \param p1 比較するバッファ1の先頭アドレス
    \param p2 比較するバッファ2の先頭アドレス
*/
void buffercompareuseSSE41(bool availableSSE41, std::uint32_t index, std::uint8_t * p1, std::uint8_t * p2);

//! A global function.
/*!
    各種条件を確認する
    \param p1 比較するバッファ1の先頭アドレス
    \param p2 比較するバッファ2の先頭アドレス
    \param size 比較するバッファのサイズ
    \return SSE4.1、AVX2およびAVX-512が使用可能かどうかと、比較する回数をセットにしたstd::pair
*/
std::pair<AvailSIMDtype, std::uint32_t> check(std::uint8_t const * p1, std::uint8_t const * p2, std::uint32_t size);

//! A global function.
/*!
    SSE4.1、AVX2、AVX-512が使用可能かどうかチェックする
    \return SSE4.1、AVX2、AVX-512が使用可能かどうか
*/
AvailSIMDtype isAvailableSIMDtype();

void memcmpAVX2(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size, std::uint32_t compareloopnum);

//! A global function.
/*!
    バッファを比較する
    \param p1 比較するバッファ1の先頭アドレス
    \param p2 比較するバッファ2の先頭アドレス
    \param size 比較するバッファのサイズ
*/
DLLEXPORT void __stdcall memcmpsimd(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size);

void memcmpSSE(bool availableSSE41, std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size, std::uint32_t compareloopnum);

//! A global function.
/*!
    バッファを比較する（並列化あり）
    \param p1 比較するバッファ1の先頭アドレス
    \param p2 比較するバッファ2の先頭アドレス
    \param size 比較するバッファのサイズ
*/
DLLEXPORT void __stdcall memcmpparallelsimd(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size);

//! A global function.
/*!
    バッファの内容を乱数で埋める
    \param p 比較するバッファ1の先頭アドレス
    \param size 比較するバッファのサイズ
*/
DLLEXPORT void __stdcall memfill128(std::uint8_t * p, std::uint32_t size);

#endif  // _MEMWORK_H_

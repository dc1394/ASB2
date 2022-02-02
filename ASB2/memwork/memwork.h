/*! \file memwork.h
    \brief メモリに書き込む関数群の宣言

    Copyright © 2014-2018 @dc1394 All Rights Reserved.
    This software is released under the BSD 2-Clause License.
*/
#ifndef _MEMWORK_H_
#define _MEMWORK_H_

#pragma once

#include <cstdint>  // for std::int32_t
#include <utility>  // for std::pair

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
    // SSE2使用可能
    AVAILSSE2 = 0,

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
    各種条件を確認する
    \param size 比較するメモリのサイズ
    \return SSE4.1、AVX2およびAVX-512が使用可能かどうかと、比較する回数をセットにしたstd::pair
*/
std::pair<AvailSIMDtype, std::uint32_t> getinfo(std::uint8_t const * p1, std::uint8_t const * p2, std::uint32_t size);

//! A global function.
/*!
    SSE4.1、AVX2、AVX-512が使用可能かどうかチェックする
    \return SSE4.1、AVX2、AVX-512が使用可能かどうかの列挙型
*/
AvailSIMDtype isAvailableSIMDtype();

//! A global function.
/*!
    AVX2命令を使ってメモリの内容を比較する
    \param cmploopnum メモリを比較する際のループ回数
    \param p1 比較するメモリ1の先頭アドレス
    \param p2 比較するメモリ2の先頭アドレス
    \return メモリの内容が一致したかどうか
*/
bool memcmpAVX2(std::uint32_t cmploopnum, std::uint8_t const * p1, std::uint8_t const * p2);

//! A global function.
/*!
    AVX-512命令を使ってメモリの内容を比較する
    \param cmploopnum メモリを比較する際のループ回数
    \param p1 比較するメモリ1の先頭アドレス
    \param p2 比較するメモリ2の先頭アドレス
    \return メモリの内容が一致したかどうか
*/
bool memcmpAVX512(std::uint32_t cmploopnum, std::uint8_t const * p1, std::uint8_t const * p2);

//! A global function.
/*!
    SIMDを使ってメモリの内容を比較する
    \param p1 比較するメモリ1の先頭アドレス
    \param p2 比較するメモリ2の先頭アドレス
    \param size 比較するメモリのサイズ
    \return メモリの内容が一致したら1、一致しなかったら0
*/
DLLEXPORT std::int32_t __stdcall memcmpsimd(std::uint8_t const * p1, std::uint8_t const * p2, std::uint32_t size);

//! A global function.
/*!
    SSE命令を使ってメモリの内容を比較する
    \param availSSE41 SSE4.1が使用可能かどうか
    \param cmploopnum メモリの内容を比較する際のループ回数
    \param p1 比較するメモリ1の先頭アドレス
    \param p2 比較するメモリ2の先頭アドレス
    \return メモリの内容が一致したかどうか
*/
bool memcmpSSE(bool availSSE41, std::uint32_t cmploopnum, std::uint8_t const * p1, std::uint8_t const * p2);

//! A global function.
/*!
    SIMDを使ってメモリの内容を比較する（スレッド並列化あり）
    \param p1 比較するメモリ1の先頭アドレス
    \param p2 比較するメモリ2の先頭アドレス
    \param size 比較するメモリのサイズ
    \return メモリの内容が一致したら1、一致しなかったら0
*/
DLLEXPORT std::int32_t __stdcall memcmpparallelsimd(std::uint8_t const * p1, std::uint8_t const * p2, std::uint32_t size);

//! A global function.
/*!
    メモリの内容を、AVX2命令を使って比較する
    \param index メモリの内容を比較するときに足すインデックス
    \param p1 比較するメモリ1の先頭アドレス
    \param p2 比較するメモリ2の先頭アドレス
    \return メモリの内容が一致したかどうか
*/
bool memcmpuseAVX2(std::uint32_t index, std::uint8_t const * p1, std::uint8_t const * p2);

//! A global function.
/*!
    メモリの内容を、AVX-512命令を使って比較する
    \param index メモリの内容を比較するときに足すインデックス
    \param p1 比較するメモリ1の先頭アドレス
    \param p2 比較するメモリ2の先頭アドレス
    \return メモリの内容が一致したかどうか
*/
bool memcmpuseAVX512(std::uint32_t index, std::uint8_t const * p1, std::uint8_t const * p2);


//! A global function.
/*!
    メモリの内容を、SSE4.1命令を使って比較する
    \param availableSSE41 SSE4.1が使用可能かどうか
    \param index メモリの内容を比較するときに足すインデックス
    \param p1 比較するメモリ1の先頭アドレス
    \param p2 比較するメモリ2の先頭アドレス
    \return メモリの内容が一致したかどうか
*/
bool memcmpuseSSE(bool availableSSE41, std::uint32_t index, std::uint8_t const * p1, std::uint8_t const * p2);

//! A global function.
/*!
    メモリの内容をAVX2命令を使い乱数で埋める
    \param p 比較するメモリ1の先頭アドレス
    \param size 比較するメモリのサイズ
*/
void memfillAVX2(std::uint8_t * p, std::uint32_t size);

//! A global function.
/*!
    メモリの内容をAVX512命令を使い乱数で埋める
    \param p 比較するメモリ1の先頭アドレス
    \param size 比較するメモリのサイズ
*/
void memfillAVX512(std::uint8_t * p, std::uint32_t size);

//! A global function.
/*!
    メモリの内容をSSE命令を使い乱数で埋める
    \param p 比較するメモリ1の先頭アドレス
    \param size 比較するメモリのサイズ
*/
DLLEXPORT void __stdcall memfillsimd(std::uint8_t * p, std::uint32_t size);

//! A global function.
/*!
    メモリの内容をSSE2命令を使い乱数で埋める
    \param p 比較するメモリ1の先頭アドレス
    \param size 比較するメモリのサイズ
*/
void memfillSSE2(std::uint8_t * p, std::uint32_t size);

#endif  // _MEMWORK_H_

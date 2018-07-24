﻿/*! \file MemWork.h
    \brief メモリに書き込む関数群の宣言

    Copyright © 2014 @dc1394 All Rights Reserved.
*/
#ifndef _MEMWORK_H_
#define _MEMWORK_H_

#pragma once

#include <array>        // for std::array
#include <cstdint>      // for std::int32_t
#include <utility>      // for std::pair
#include <intrin.h>

#ifndef _WIN64
    #pragma comment(linker, "/export:memcmp128=_memcmp128@12")
    #pragma comment(linker, "/export:memcmpparallel128=_memcmpparallel128@12")
    #pragma comment(linker, "/export:memfill128=_memfill128@8")
#endif

#ifdef __cplusplus
    #define DLLEXPORT extern "C" __declspec(dllexport)
#else
    #define DLLEXPORT __declspec(dllexport)
#endif

namespace {
    //! A static variable (constant expression).
    /*!
        64ビット符号なし整数の最大値
    */
    static auto constexpr FFFFFFFFFFFFFFFFh = 0xFFFFFFFFFFFFFFFF;

    //! A static variable (constant expression).
    /*!
        SSE4.1が使用できるかどうかに使うフラグ
    */
    static auto constexpr SSE4_1_FLAG = 0x00080000;

    //! A static variable (constant expression).
    /*!
        プリフェッチのサイズ
    */
    static auto constexpr PREFETCHSIZE = 4096;

    //! A function.
    /*!
        バッファを比較する（SSE4.1を使う）
        \param availableSSE4_1 SSE4.1が使用可能かどうか
        \param index バッファを比較するときに足すインデックス
        \param p1 比較するバッファ1の先頭アドレス
        \param p2 比較するバッファ2の先頭アドレス
    */
    void buffercompareuseSSE4_1(bool availableSSE4_1, std::uint32_t index, std::uint8_t * p1, std::uint8_t * p2);

    //! A function.
    /*!
        各種条件を確認する
        \param p1 比較するバッファ1の先頭アドレス
        \param p2 比較するバッファ2の先頭アドレス
        \param size 比較するバッファのサイズ
        \return SSE4.1が使用可能かどうかと、比較する回数をセットにしたstd::pair
    */
    std::pair<bool, std::uint32_t> check(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size);

    //! A function (inline).
    /*!
        SSE4.1が使用可能かどうかチェックする
        \return SSE4.1が使用可能かどうか
    */
    inline bool isAvailableSSE4_1()
    {
        std::array<std::int32_t, 4> CPUInfo;
        ::__cpuid(CPUInfo.data(), 1);

        return static_cast<bool>(CPUInfo[2] & SSE4_1_FLAG);
    }
}

extern "C" {
    //! A global function.
    /*!
        バッファを比較する
        \param p1 比較するバッファ1の先頭アドレス
        \param p2 比較するバッファ2の先頭アドレス
        \param size 比較するバッファのサイズ
    */
    DLLEXPORT void __stdcall memcmp128(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size);

    //! A global function.
    /*!
        バッファを比較する（並列化あり）
        \param p1 比較するバッファ1の先頭アドレス
        \param p2 比較するバッファ2の先頭アドレス
        \param size 比較するバッファのサイズ
    */
    DLLEXPORT void __stdcall memcmpparallel128(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size);

    //! A global function.
    /*!
        バッファの内容を乱数で埋める
        \param p 比較するバッファ1の先頭アドレス
        \param size 比較するバッファのサイズ
    */
    DLLEXPORT void __stdcall memfill128(std::uint8_t * p, std::uint32_t size);
}

#endif  // _MEMWORK_H_

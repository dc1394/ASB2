/*! \file Xor514.h
    \brief SSE2を使って高速化したXorshiftによる乱数生成クラス
    
    but this is originally adapted by 和田維作 さま
    cf. http://www001.upp.so-net.ne.jp/isaku/xor.c.html
    
    Copyright ©  2014 @dc1394 All Rights Reserved.
*/

#ifndef _XOR514_H_
#define _XOR514_H_

#pragma once

// 以下はオリジナルのソースコードのコメント
/* xor.c : coded by isaku@pb4.so-net.ne.jp 2008/12～2009/1,  2010/10, 2012/6
乱数 Xorshift のＳＳＥ２を使った高速化(整数版)

xor128 は論文に書いてあった 128 bit Xorshift である。

以前の xor130 は SSE2 を使って、１２８ビット演算を行うものであった。
一度に４個の整数を得るので、周期は 2^{130}-4 であった。
１ビット単位のシフト演算は SSE2 ではサポートされていないので、
他の演算を組み合わせることで１２８ビットシフト演算を実現した。

最初のバージョンでは

enum { a=11,b=23 };
x^=x<<a; x^=x>>b; return x;

のように、二つの Xorshift 演算を行っていた。
Diehard テストを行ったところ、良好な結果を得たが、
Dieharder テストでは必ず不合格が出るので、
次のバージョンでは、三つの Xorshift 演算を行うように改良した。

enum { a=8,b=69,c=75 };
x^=x<<a; x^=x>>b; x^=x<<c; return x;

高速に計算できるパラメータを選んだので、
古いバージョンと同じスピードで実行できた。
古い Dieharder テストで不合格がでなくなったが、
新しい Dieharder テスト(3.31.1)では不合格になってしまった。

そこで、

enum { a=101,b=99,c=8 };
t=x^x<<a; x=y; y=z; z=w; t^=t>>b; w^=w>>c^t; return w;
とした xor514 にしたところ、新しい Dieharder テストでも不合格が出なくなった。
周期は 2^{514}-4 であ

以下のプログラムは xor128 , xor514 のそれぞれについて、
１０億個 × ８の乱数を足し合わせて、最速の実行時間を比較する。

１０億回の生成時間(単位 秒)の結果

Core i7 2600(turbo boost off)
+--------------------------------------------------+--------+--------+
| compiler and system                              | xor128 | xor514 |
+--------------------------------------------------+--------+--------+
| VC++2010 64bit cl /O2 /GL /W4  on Windows7 64bit |  1.856 |  1.326 |
| VC++2010 32bit cl /O2 /GL /W4  on Windows7 64bit |  2.262 |  1.554 |
| gcc -O4 -Wall              on Ubuntu 12.04 64bit |  1.800 |  1.230 |
| gcc -O4 -Wall -m32 -msse2  on Ubuntu 12.04 64bit |  2.320 |  1.040 |
+--------------------------------------------------+--------+--------+
*/

#include <cstdint>
#include <emmintrin.h>

#ifdef __GNUC__
    #define align32 __attribute__((aligned(32)))
#else
    #define align32 _declspec(align(32))
#endif

namespace memwork {
    namespace myrandom {
        //! A class.
        /*!
        SSE2を使って高速化したXorshiftによる乱数生成クラス
        */
        class Xor514 final
        {
        public:
            // #region 共用体

            align32 union xor514_t {
                std::uint32_t u[20];
                __m128i m[5];
            };

            // #endregion 共用体

            // #region コンストラクタ

            //! A constructor.
            /*!
            \param seed 乱数のシード
            */
            explicit Xor514(std::uint32_t seed);

            // #endregion コンストラクタ

            // #region メンバ関数

            //! A public member function.
            /*!
            乱数を生成する
            \return 乱数
            */
            __m128i rand128();

            // #endregion メンバ関数

        private:
            // #region メンバ変数

            //! A private variable (constant expression).
            /*!
            U0の最大値
            */
            static auto constexpr U0MAX = 20;

            //! A private variable (constant expression).
            /*!
            次の乱数を生成するときのバイトの閾値
            */
            static auto constexpr XOR514HEAD = 16;

            
            xor514_t p;
            void xor514sub();

            Xor514() = delete;
            Xor514(Xor514 const &) = delete;
            Xor514 & operator=(Xor514 const &) = delete;
        };
    }
}

#endif  // _XOR514_H_

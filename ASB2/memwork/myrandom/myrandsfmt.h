/*! \file myrandsfmt.h
    \brief SFMTを使った自作乱数クラスの宣言

    Copyright © 2017 @dc1394 All Rights Reserved.
    This software is released under the BSD 2-Clause License.
*/

#ifndef _MYRANDSFMT_H_
#define _MYRANDSFMT_H_

#pragma once

#include "../../SFMT-src-1.5.1/SFMT.h"
#include <algorithm>					// for std::generate
#include <cstdint>						// for std::uint32_t, std::uint_least32_t
#include <functional>					// for std::ref
#include <vector>						// for std::vector

namespace myrandom {
    //! A class.
    /*!
        自作乱数クラス
    */
    class MyRandSfmt final {
        // #region コンストラクタ・デストラクタ

    public:
        //! A constructor.
        /*!
            唯一のコンストラクタ
        */
        MyRandSfmt();

        //! A destructor.
        /*!
            デフォルトデストラクタ
        */
        ~MyRandSfmt() = default;

        // #endregion コンストラクタ・デストラクタ

        // #region メンバ関数

        //!  A public member function.
        /*!
            [0, UINT32_MAX]の閉区間で一様乱数を生成する
        */
        std::uint32_t myrand()
        {
            return sfmt_genrand_uint32(&sfmt);
        }

        // #endregion メンバ関数

        // #region メンバ変数

    private:
        //! A private static member variable (constant expression).
        /*!
            初期乱数生成用のstd::vectorのサイズ
        */
        static std::vector<std::uint_least32_t>::size_type const SIZE = 1;

        //! A private member variable.
        /*!
            乱数エンジン
        */
		sfmt_t sfmt;

        // #region 禁止されたコンストラクタ・メンバ関数

        //! A private copy constructor (deleted).
        /*!
            コピーコンストラクタ（禁止）
        */
        MyRandSfmt(const MyRandSfmt &) = delete;

        //! A private member function (deleted).
        /*!
            operator=()の宣言（禁止）
            \param コピー元のオブジェクト（未使用）
            \return コピー元のオブジェクト
        */
        MyRandSfmt & operator=(const MyRandSfmt &) = delete;

        // #endregion 禁止されたコンストラクタ・メンバ関数
    };

    MyRandSfmt::MyRandSfmt()
    {
        // ランダムデバイス
        std::random_device rnd;

        // 初期化用ベクタ
        std::vector<std::uint_least32_t> v(SIZE);

        // ベクタの初期化
        // 非決定的な乱数でシード列を構築する
        std::generate(v.begin(), v.end(), std::ref(rnd));

        // 乱数エンジン
		sfmt_init_gen_rand(&sfmt, v[0]);
    }
}

#endif  // _MYRANDSFMT_H_

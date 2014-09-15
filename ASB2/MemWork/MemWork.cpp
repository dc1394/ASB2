#include "MemWork.h"
#include "Xor514.h"
#include <cassert>
#include <ctime>

#ifdef __cilk
	#include <cilk/cilk.h>
#else
	#define cilk_for for
#endif

DLLEXPORT bool __stdcall memcmp128(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size)
{
	// アライメントはあっているか？
#ifdef _WIN64
	assert(!(reinterpret_cast<std::uint64_t>(p1) & 0x0F));
	assert(!(reinterpret_cast<std::uint64_t>(p2) & 0x0F));
#else
	assert(!(reinterpret_cast<std::uint32_t>(p1) & 0x0F));
	assert(!(reinterpret_cast<std::uint32_t>(p2) & 0x0F));
#endif
	// 書き込むバイト数は64バイトの倍数か？
	assert(!(size & 0x3F));
	
	// 比較する回数（1回のループで64バイト（512ビット）ずつ比較）
	auto const write512loop = size >> 6;

	// SSE4.1命令は使用可能か
	auto const supposedSSE4_1 = availableSSE4_1();

	// 実際に1回のループで64バイトずつ比較
	for (std::uint32_t i = 0; i < write512loop; i++) {
		auto const s1 = reinterpret_cast<char const *>(p1) + (i << 6);
		auto const s2 = reinterpret_cast<char const *>(p2) + (i << 6);

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
			if (supposedSSE4_1)
				r[j] = ::_mm_cmpeq_epi64(xmm[j], xmm2[j]);
			else
				r[j] = ::_mm_cmpeq_epi32(xmm[j], xmm2[j]);
			
			// 64ビット（8バイト）ごとに結果をチェック
			for (auto k = 0; k < 2; k++) {
				if (r[j].m128i_u64[k] != FFFFFFFFFFFFFFFFh)
					return false;
			}
		}

		return true;
	}

	return true;
}

DLLEXPORT bool __stdcall memcmpparallel128(std::uint8_t * p1, std::uint8_t * p2, std::uint32_t size)
{
	// アライメントはあっているか？
#ifdef _WIN64
	assert(!(reinterpret_cast<std::uint64_t>(p1) & 0x0F));
	assert(!(reinterpret_cast<std::uint64_t>(p2) & 0x0F));
#else
	assert(!(reinterpret_cast<std::uint32_t>(p1) & 0x0F));
	assert(!(reinterpret_cast<std::uint32_t>(p2) & 0x0F));
#endif
	// 書き込むバイト数は64バイトの倍数か？
	assert(!(size & 0x3F));
	
	// 比較する回数（1回のループで64バイト（512ビット）ずつ比較）
	auto const write512loop = size >> 6;
	
	// SSE4.1命令は使用可能か
	auto const supposedSSE4_1 = availableSSE4_1();

	// 結果格納用変数
	auto res = true;

	// 実際に1回のループで64バイトずつ比較
	cilk_for (std::uint32_t i = 0; i < write512loop; i++) {
		if (res) {
			char const * s1 = reinterpret_cast<char const *>(p1) + (i << 6);
			char const * s2 = reinterpret_cast<char const *>(p2) + (i << 6);

			// キャッシュ汚染を防ぐ
			_mm_prefetch(s1 + PREFETCHSIZE, _MM_HINT_NTA);

			// メモリからxmm0～7にロード
			std::array<__m128i, 4> xmm, xmm2;
			for (auto j = 0; j < 4; j++) {
				xmm[j] = ::_mm_load_si128(reinterpret_cast<__m128i const *>(s1 + j * sizeof(__m128i)));
				xmm2[j] = ::_mm_load_si128(reinterpret_cast<__m128i const *>(s2 + j * sizeof(__m128i)));
			}

			// 結果を比較
			std::array<__m128i, 4> r;
			for (auto j = 0; j < 4; j++) {
				if (supposedSSE4_1)
					r[j] = ::_mm_cmpeq_epi64(xmm[j], xmm2[j]);
				else
					r[j] = ::_mm_cmpeq_epi32(xmm[j], xmm2[j]);

				// 64ビット（8バイト）ごとに結果をチェック
				for (auto k = 0; k < 2; k++) {
					if (r[j].m128i_u64[k] != FFFFFFFFFFFFFFFFh)
						res = false;
				}
			}
		}
	}

	return res;
}

DLLEXPORT void __stdcall memfill128(std::uint8_t * p, std::uint32_t size)
{
    // 乱数オブジェクト生成
    memwork::myrandom::Xor514 pxor(static_cast<std::uint32_t>(std::time(NULL)));

    // アライメントはあっているか？
#ifdef _WIN64
    assert(!(reinterpret_cast<std::uint64_t>(p)& 0x0F));
#else
    assert(!(reinterpret_cast<std::uint32_t>(p)& 0x0F));
#endif
    // 書き込むバイト数は64バイトの倍数か？
    assert(!(size & 0x3F));

    // 1回のループで書き込む回数（64バイト（512ビット）ずつ）
    auto const write512loop = size >> 6;

    // 実際に1回のループで64バイトずつ書き込む
    for (std::uint32_t i = 0; i < write512loop; i++) {
        std::array<__m128i, 4> xmm;
        // xmm0～3レジスタ上に乱数生成
        for (auto j = 0; j < 4; j++)
            xmm[j] = pxor.rand128();

        // アドレス
        std::uint8_t * const d = p + (i << 6);
        // 実際に書き込む
        for (auto j = 0; j < 4; j++)
            ::_mm_stream_si128(reinterpret_cast<__m128i *>(d + j * sizeof(__m128i)), xmm[j]);
    }
}

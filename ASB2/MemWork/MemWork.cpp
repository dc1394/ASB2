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
	// �A���C�����g�͂����Ă��邩�H
#ifdef _WIN64
	assert(!(reinterpret_cast<std::uint64_t>(p1) & 0x0F));
	assert(!(reinterpret_cast<std::uint64_t>(p2) & 0x0F));
#else
	assert(!(reinterpret_cast<std::uint32_t>(p1) & 0x0F));
	assert(!(reinterpret_cast<std::uint32_t>(p2) & 0x0F));
#endif
	// �������ރo�C�g����64�o�C�g�̔{�����H
	assert(!(size & 0x3F));
	
	// ��r����񐔁i1��̃��[�v��64�o�C�g�i512�r�b�g�j����r�j
	auto const write512loop = size >> 6;

	// SSE4.1���߂͎g�p�\��
	auto const supposedSSE4_1 = availableSSE4_1();

	// ���ۂ�1��̃��[�v��64�o�C�g����r
	for (std::uint32_t i = 0; i < write512loop; i++) {
		auto const s1 = reinterpret_cast<char const *>(p1) + (i << 6);
		auto const s2 = reinterpret_cast<char const *>(p2) + (i << 6);

		// �L���b�V��������h��
		::_mm_prefetch(s1 + PREFETCHSIZE, _MM_HINT_NTA);

		// ����������xmm0�`7�Ƀ��[�h
		std::array<__m128i, 4> xmm, xmm2;
		for (auto j = 0; j < 4; j++) {
			xmm[j] = ::_mm_load_si128(reinterpret_cast<__m128i const *>(s1 + j * sizeof(__m128i)));
			xmm2[j] = ::_mm_load_si128(reinterpret_cast<__m128i const *>(s2 + j * sizeof(__m128i)));
		}

		// ���ʂ��r
		std::array<__m128i, 4> r;
		for (auto j = 0; j < 4; j++) {
			if (supposedSSE4_1)
				r[j] = ::_mm_cmpeq_epi64(xmm[j], xmm2[j]);
			else
				r[j] = ::_mm_cmpeq_epi32(xmm[j], xmm2[j]);
			
			// 64�r�b�g�i8�o�C�g�j���ƂɌ��ʂ��`�F�b�N
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
	// �A���C�����g�͂����Ă��邩�H
#ifdef _WIN64
	assert(!(reinterpret_cast<std::uint64_t>(p1) & 0x0F));
	assert(!(reinterpret_cast<std::uint64_t>(p2) & 0x0F));
#else
	assert(!(reinterpret_cast<std::uint32_t>(p1) & 0x0F));
	assert(!(reinterpret_cast<std::uint32_t>(p2) & 0x0F));
#endif
	// �������ރo�C�g����64�o�C�g�̔{�����H
	assert(!(size & 0x3F));
	
	// ��r����񐔁i1��̃��[�v��64�o�C�g�i512�r�b�g�j����r�j
	auto const write512loop = size >> 6;
	
	// SSE4.1���߂͎g�p�\��
	auto const supposedSSE4_1 = availableSSE4_1();

	// ���ʊi�[�p�ϐ�
	auto res = true;

	// ���ۂ�1��̃��[�v��64�o�C�g����r
	cilk_for (std::uint32_t i = 0; i < write512loop; i++) {
		if (res) {
			char const * s1 = reinterpret_cast<char const *>(p1) + (i << 6);
			char const * s2 = reinterpret_cast<char const *>(p2) + (i << 6);

			// �L���b�V��������h��
			_mm_prefetch(s1 + PREFETCHSIZE, _MM_HINT_NTA);

			// ����������xmm0�`7�Ƀ��[�h
			std::array<__m128i, 4> xmm, xmm2;
			for (auto j = 0; j < 4; j++) {
				xmm[j] = ::_mm_load_si128(reinterpret_cast<__m128i const *>(s1 + j * sizeof(__m128i)));
				xmm2[j] = ::_mm_load_si128(reinterpret_cast<__m128i const *>(s2 + j * sizeof(__m128i)));
			}

			// ���ʂ��r
			std::array<__m128i, 4> r;
			for (auto j = 0; j < 4; j++) {
				if (supposedSSE4_1)
					r[j] = ::_mm_cmpeq_epi64(xmm[j], xmm2[j]);
				else
					r[j] = ::_mm_cmpeq_epi32(xmm[j], xmm2[j]);

				// 64�r�b�g�i8�o�C�g�j���ƂɌ��ʂ��`�F�b�N
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
    // �����I�u�W�F�N�g����
    memwork::myrandom::Xor514 pxor(static_cast<std::uint32_t>(std::time(NULL)));

    // �A���C�����g�͂����Ă��邩�H
#ifdef _WIN64
    assert(!(reinterpret_cast<std::uint64_t>(p)& 0x0F));
#else
    assert(!(reinterpret_cast<std::uint32_t>(p)& 0x0F));
#endif
    // �������ރo�C�g����64�o�C�g�̔{�����H
    assert(!(size & 0x3F));

    // 1��̃��[�v�ŏ������މ񐔁i64�o�C�g�i512�r�b�g�j���j
    auto const write512loop = size >> 6;

    // ���ۂ�1��̃��[�v��64�o�C�g����������
    for (std::uint32_t i = 0; i < write512loop; i++) {
        std::array<__m128i, 4> xmm;
        // xmm0�`3���W�X�^��ɗ�������
        for (auto j = 0; j < 4; j++)
            xmm[j] = pxor.rand128();

        // �A�h���X
        std::uint8_t * const d = p + (i << 6);
        // ���ۂɏ�������
        for (auto j = 0; j < 4; j++)
            ::_mm_stream_si128(reinterpret_cast<__m128i *>(d + j * sizeof(__m128i)), xmm[j]);
    }
}

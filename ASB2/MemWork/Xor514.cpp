#include "Xor514.h"

namespace memwork {
    namespace myrandom {
        Xor514::Xor514(std::uint32_t seed)
        {
            p.u[0] = Xor514::U0MAX;
            p.u[4] = seed;
            for (std::uint32_t i = 1; i < 16; i++)
                p.u[i + 4] = seed = 1812433253 * (seed ^ (seed >> 30)) + i;
        }

        void Xor514::xor514sub()
        {
            enum { a = 101, b = 99, c = 8 };
            auto const x = p.m[1];
            auto const w = p.m[4];
            auto s = _mm_slli_si128(_mm_slli_epi64(x, a - 64), 8);
            auto t = _mm_xor_si128(x, s);
            p.m[1] = p.m[2];
            p.m[2] = p.m[3];
            p.m[3] = w;
            s = _mm_srli_si128(_mm_srli_epi64(t, b - 64), 8);
            t = _mm_xor_si128(t, s);
            s = _mm_srli_si128(w, c / 8);
            s = _mm_xor_si128(s, t);
            p.m[4] = _mm_xor_si128(w, s);
        }

        __m128i Xor514::rand128()
        {
            if (p.u[0] != Xor514::XOR514HEAD) {
                p.u[0] = Xor514::XOR514HEAD;
                xor514sub();
            }

            p.u[0] += 4;
            return ::_mm_load_si128(reinterpret_cast<__m128i const *>(p.u + Xor514::XOR514HEAD));
        }
    }
}

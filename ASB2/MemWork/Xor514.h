/* xor514.h
 * http://www001.upp.so-net.ne.jp/isaku/xor.c.html
 * Arranged by dc1394
 * �ȉ��͎Q�l�ɂ����t�@�C���̃R�����g
 */

/* xor.c : coded by isaku@pb4.so-net.ne.jp 2008/12�`2009/1,  2010/10, 2012/6
���� Xorshift �̂r�r�d�Q���g����������(������)

xor128 �͘_���ɏ����Ă����� 128 bit Xorshift �ł���B

�ȑO�� xor130 �� SSE2 ���g���āA�P�Q�W�r�b�g���Z���s�����̂ł������B
��x�ɂS�̐����𓾂�̂ŁA������ 2^{130}-4 �ł������B
�P�r�b�g�P�ʂ̃V�t�g���Z�� SSE2 �ł̓T�|�[�g����Ă��Ȃ��̂ŁA
���̉��Z��g�ݍ��킹�邱�ƂłP�Q�W�r�b�g�V�t�g���Z�����������B

�ŏ��̃o�[�W�����ł�

enum { a=11,b=23 };
x^=x<<a; x^=x>>b; return x;

�̂悤�ɁA��� Xorshift ���Z���s���Ă����B
Diehard �e�X�g���s�����Ƃ���A�ǍD�Ȍ��ʂ𓾂����A
Dieharder �e�X�g�ł͕K���s���i���o��̂ŁA
���̃o�[�W�����ł́A�O�� Xorshift ���Z���s���悤�ɉ��ǂ����B

enum { a=8,b=69,c=75 };
x^=x<<a; x^=x>>b; x^=x<<c; return x;

�����Ɍv�Z�ł���p�����[�^��I�񂾂̂ŁA
�Â��o�[�W�����Ɠ����X�s�[�h�Ŏ��s�ł����B
�Â� Dieharder �e�X�g�ŕs���i���łȂ��Ȃ������A
�V���� Dieharder �e�X�g(3.31.1)�ł͕s���i�ɂȂ��Ă��܂����B

�����ŁA

enum { a=101,b=99,c=8 };
t=x^x<<a; x=y; y=z; z=w; t^=t>>b; w^=w>>c^t; return w;
�Ƃ��� xor514 �ɂ����Ƃ���A�V���� Dieharder �e�X�g�ł��s���i���o�Ȃ��Ȃ����B
������ 2^{514}-4 �ł�

�ȉ��̃v���O������ xor128 , xor514 �̂��ꂼ��ɂ��āA
�P�O���� �~ �W�̗����𑫂����킹�āA�ő��̎��s���Ԃ��r����B

�P�O����̐�������(�P�� �b)�̌���

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


#ifndef __INTEL_COMPILER
	#include <boost/noncopyable.hpp>
#endif

#ifdef __GNUC__
	#define align32 __attribute__((aligned(32)))
#else
	#define align32 _declspec(align(32))
#endif

namespace memwork {
	namespace myrandom {
		class Xor514
#ifndef __INTEL_COMPILER
			: private boost::noncopyable
#endif
		{
#ifdef __INTEL_COMPILER
			static constexpr std::uint32_t XOR514HEAD = 16;
			static constexpr std::uint32_t U0MAX = 20;
			Xor514() = delete;
			Xor514(const Xor514 &) = delete;
			Xor514 & operator=(const Xor514 &) = delete;
#else
			static const std::uint32_t XOR514HEAD = 16;
			static const std::uint32_t U0MAX = 20;
			Xor514();
#endif
			align32 union xor514_t { std::uint32_t u[20];
									 __m128i m[5];		    };
			xor514_t p;
			void xor514sub();

		public:
			explicit Xor514(std::uint32_t seed);
			__m128i rand128();
		};
	}
}

================================================================================
【 ソフト名 】Aomin SSD Breaker 2 (ASB2)
【  作成者  】@dc1394
================================================================================

★これは何？
　SSD/HDDにデータ（ファイル）をひたすら、書き込んでは消し、書き込んでは消し…を
　繰り返すことでSSD/HDDの寿命を意図的に縮めるソフトです。不要になったSSD/HDDを
　処分するときに使えるかもしれません。一回だけの書き込みも出来るので、SSD/HDDの
　ちょっとしたベンチマークにも使えるかもしれません。

★インストール
　ASB2_x64_(バージョン番号).zip（x64版）またはASB2_x86_(バージョン番号).zip（x86
　版）を任意のフォルダに解凍してください。
　ただし、このソフトを動作させるためには、「Microsoft .NET Framework 4」
　（ http://bit.ly/16JtVz1 からダウンロード可能）と、「Visual Studio 2012 更新プ
　ログラム 1 の Visual C++ 再頒布可能パッケージ」（ http://bit.ly/13sbvSW からダ
　ウンロード可能）が必要です。なお、ASB2 x86版の動作には、「vcredist_x86.exe」が、
　ASB2 x64版の動作には「vcredist_x64.exe」が必要となります（OS自体のx86、x64バー
　ジョンとは関係が無いので注意して下さい。なお当然ですが、ASB2 x64版は、x86バー
　ジョンのWindowsでは動きません。）

★アンインストール
　解凍したフォルダを削除して下さい。
　このソフトはレジストリを変更しません。

★既知のバグについて
　現状では既知のバグとして、開発環境がWindows 10であるため、Windows 7ではレイア
  ウトが崩れるバグがあります。

★更新履歴
　2013/04/08 ver.0.0.1α   コードを全面的に書き直し。
　2013/04/09 ver.0.0.1α2  ドキュメントの整備。
　2013/04/10 ver.0.0.1α3  小修正とドキュメントの整備。
　2013/06/16 ver.0.0.1β1  「タイマの更新間隔」が反映されていなかったのを修正。
　　　　　　　　　　　　　　「タイマの更新間隔」を最大10秒にした。
						   Windows XP環境で起こるバグを修正。
　　　　　　　　　　　　　 SSE4.1命令が使えない環境で起こるバグを修正。
　2014/09/21 ver.0.1       コードを全面的に整理した。
　2018/08/02 ver.0.2       AVX2/AVX-512命令に対応。

★謝辞
　このソフトウェアのBindaleBaseクラスは、「id:minami_SC」様の「WPFでも
　BindableBaseを使ってINotifyPropertyChangedを実装する
　（ http://sourcechord.hatenablog.com/entry/20130303/1362315081 )」を、
　EnumBooleanConverterクラスは、「ba」様の「Enum と RadioButton をバインドする(
　http://frog.raindrop.jp/knowledge/archives/002200.html )」の記事を、
　NumericBehaviorsクラスは、「なわ」様の「数値入力用テキストボックス　添付ビヘイ
　ビア編」の記事を、最後に、TaskTrayIconクラスは、「ほげたん」様の「タスクトレイ
　にアイコンを表示する( http://hogetan.blogspot.com/2008/10/blog-post.html )」の
　記事を、それぞれ参考にさせて頂きました。この場をお借りしまして御礼申し上げます。

★ライセンス
　このソフトはフリーソフトウェアです（修正BSDライセンス）。
--------------------------------------------------------------------------------
　Aomin SSD Breaker 2
　Copyright (C) 2013-2018 @dc1394

　1.ソースコード形式であれバイナリ形式であれ、変更の有無に関わらず、以下の条件を
　満たす限りにおいて、再配布および利用を許可します。

　1-1.ソースコード形式で再配布する場合、上記著作権表示、 本条件書および第2項の責
　任限定規定を必ず含めてください。
　1-2.バイナリ形式で再配布する場合、上記著作権表示、 本条件書および下記責任限定
　規定を、配布物とともに提供される文書 および/または 他の資料に必ず含めてくださ
　い。

　2.本ソフトウェアは無保証です。自己責任で使用してください。

　3.著作権者の名前を、広告や宣伝に勝手に使用しないでください。

  Copyright (c) 2013-2018, @dc1394
  All rights reserved.
  
  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions are met:
  * Redistributions of source code must retain the above copyright notice, 
    this list of conditions and the following disclaimer.
  * Redistributions in binary form must reproduce the above copyright notice, 
    this list of conditions and the following disclaimer in the documentation 
    and/or other materials provided with the distribution.
  * Neither the name of the <organization> nor the　names of its contributors 
    may be used to endorse or promote products derived from this software 
    without specific prior written permission.
  
  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
  AND
  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
  DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
  DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
  LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
  ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
--------------------------------------------------------------------------------
　
　ASB2にはMutsuo Saito, Makoto Matsumoto, Hiroshima University and The
　University of TokyoによるSIMD-oriented Fast Mersenne Twister (SFMT)を使用して
　います。こちらのライセンスは The (modified) BSD License になります。

Copyright (c) 2006,2007 Mutsuo Saito, Makoto Matsumoto and Hiroshima
University.
Copyright (c) 2012 Mutsuo Saito, Makoto Matsumoto, Hiroshima University
and The University of Tokyo.
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are
met:

    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above
      copyright notice, this list of conditions and the following
      disclaimer in the documentation and/or other materials provided
      with the distribution.
    * Neither the names of Hiroshima University, The University of
      Tokyo nor the names of its contributors may be used to endorse
      or promote products derived from this software without specific
      prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;
using System.IO;

namespace Nekote
{
    public static class nString
    {
        // Sun, 31 Mar 2019 03:26:03 GMT
        // あれば便利とたまに思い、でも実装しようとしたら冗長と思い……というのを何度か繰り返したので実装する
        // たとえば xMessage.nOrNull () のような書き方で、"" のときに null に単純化できる
        // シンプルな実装だし、ループで呼ぶこともあり得るので IsNullOrEmpty をそれぞれに入れることも考えたが、
        // コーディングのガイドラインとしては、できるだけ他を使い回して実行速度より保守性を優先なので、それに従っている

        public static string nOrDefault (this string text, string value) =>
            string.IsNullOrEmpty (text) == false ? text : value;

        public static string nOrEmpty (this string text) =>
            text.nOrDefault (string.Empty);

        public static string nOrNull (this string text) =>
            text.nOrDefault (null);

        // Mon, 20 May 2019 21:12:39 GMT
        // 無害な決め打ちにより、文字列の可読性を高める
        // ?? "(null)" で済むことだが、各所にリテラルを埋没させたくない

        public static string nToFriendlyString (this string text, string nullText = "(null)", string emptyText = "(empty)")
        {
            if (text == null)
                return nullText;
            else if (text.Length == 0)
                return emptyText;
            else return text;
        }

        // Thu, 16 May 2019 07:11:21 GMT
        // 引数をゴソッとチェックするときにコードが短くなる

        public static bool IsAnyNullOrEmpty (params string [] values)
        {
            foreach (string xValue in values)
            {
                if (string.IsNullOrEmpty (xValue))
                    return true;
            }

            return false;
        }

        // Sun, 31 Mar 2019 05:38:42 GMT
        // 地味に役立ちそうなので、文字用と文字列用の両方を用意しておく
        // 文字用のものはラッパーだが、同じメソッド名で両方あった方が分かりやすい

        public static string Repeat (char character, int count) =>
            new string (character, count);

        public static string Repeat (string text, int count)
        {
            StringBuilder xBuilder = new StringBuilder ();

            for (int temp = 0; temp < count; temp ++)
                xBuilder.Append (text);

            return xBuilder.ToString ();
        }

        // Wikipedia にならい、whitespace を一つの単語として識別子に含めている
        // https://en.wikipedia.org/wiki/Whitespace_%28programming_language%29
        // 文字については、以下のページに If ECMAScript-compliant behavior is specified, \s is equivalent to [ \f\n\r\t\v] とある
        // 他にもマイナーな文字がいくつかあるようだが、これらに対応しておけば、通常のプログラミングにおいては不足がないだろう
        // https://msdn.microsoft.com/en-us/library/20bw873z.aspx#WhitespaceCharacter

        // 追記: Unicode において IDEOGRAPHIC SPACE とされる、いわゆる「全角空白」もホワイトスペースとして処理することにした
        // これで、全角空白（_ で表現）を使っての「正解は___→→→___○○○」のような裏技的な表記が、ノーマライズされる文字列では不可能となるが、
        // 元々、半角空白やタブ文字ではできないことが全角空白でのみ可能という、CJK 優遇かつ、知っていないとできないことをライブラリーに埋没させるべきでない
        // それに、ATOK も MS-IME もデフォルトでは空白が全角であり、洗わないままでは、空白の種類のみ異なる表記揺れへの対応がさらに必要となる
        // なお、ページによると他にも空白系文字はあり、マイナーな言語のものも想定するならもっとあるだろうが、今後も追加は慎重に行っていく
        // https://ja.wikipedia.org/wiki/%E3%82%B9%E3%83%9A%E3%83%BC%E3%82%B9

        // Fri, 31 Aug 2018 04:13:08 GMT
        // YouTube の音楽ビデオをダウンロードしてみたところ、ノーマライズで落ちない空白がファイル名の末尾にあり、U+200E だった
        // これは、アラビア語などのテキストにおいて「ここだけ左から右のテキスト」という指定を行うための文字のようである
        // YouTube の件は、アラビア語などのページから投稿者が曲名をコピペしたときにその文字まで入ったのだろう
        // これは Wikipedia の Whitespace character ページには入っておらず、一方、このページには他にも多数の文字がある
        // ノーマライズにおいて照合しなければならない文字を増やしても、そのコストに比してメリットが微妙なのでやはりしない
        // https://en.wikipedia.org/wiki/Left-to-right_mark
        // https://ja.wikipedia.org/wiki/%E5%8F%8C%E6%96%B9%E5%90%91%E3%83%86%E3%82%AD%E3%82%B9%E3%83%88
        // https://en.wikipedia.org/wiki/Whitespace_character

        public static readonly char [] WhitespaceChars = { ' ', '\f', '\n', '\r', '\t', '\v', '\u3000' };

        // inline という表現が微妙だが、HTML には inline elements と block elements がある
        // 横並びの一つの流れというニュアンスで inline を捉えるなら、ひどく外れているネーミングでもないだろう
        public static readonly char [] InlineWhitespaceChars = { ' ', '\f', '\t', '\v', '\u3000' };

        // 「改行」というと line breaks だが、ここでも Wikipedia にならい、メジャーそうな表現を採用
        // newline で「（次の）新しい行」でなく「改行」を表現するのが、英語圏の人の感覚なのだろう
        // https://en.wikipedia.org/wiki/Newline
        public static readonly char [] NewlineChars = { '\n', '\r' };

        // Array.IndexOf は、引数を object で受け取るし、引数のチェックもうるさい

        // Sat, 18 May 2019 23:49:58 GMT
        // LINQ の Contains を当時知らなかったのか、インテリセンスで出ていなかったのか、こういうものを実装した
        // iContainsChar (InlineWhitespaceChars, ... と InlineWhitespaceChars.Contains の速度差が気になっていたので調べたところ、
        // 見付からないと分かっている文字をループで InlineWhitespaceChars から1億回探す処理に、それぞれ3969ミリ秒、8438ミリ秒かかった
        // やはり速度差は大きいが、それでも1万回に1ミリ秒かからないため、積もっても塵のままであり、今すぐ書き直すほどのことでない

        private static bool iContainsChar (char [] values, char value)
        {
            foreach (char xValue in values)
            {
                if (xValue == value)
                    return true;
            }

            return false;
        }

        // 文字列中の空白系文字を整え、無駄を減らす処理を「ノーマライズ」と呼び、2種類のメソッドを用意しておく
        // 一つは、単一行だと分かっていて、そこに改行が混入すると問題につながりうる文字列を対象とし、
        // もう一つは、段落分けがあり、インデントもあっての通常の複数行の文字列を対象とする

        // 単一行の方は、一つ以上の連続する空白系文字（改行を含む）が半角空白一つに統合されて最後に Trim されるのと同じ結果になる
        // 実装の都合上、Trim に相当する処理はむしろ最初に行われるが、上記のようにイメージするのが分かりやすい
        // 複数行の文字列も、このメソッドに通すと強制的に単一行にされるため、処理の安全性が向上する

        public static void nNormalizeLine (this string text, StringBuilder builder)
        {
            if (string.IsNullOrEmpty (text))
                return;

            int xFirstCharIndex = 0,
                xLastCharIndex = text.Length - 1;

            // 末尾の空白系文字を削る

            while (xLastCharIndex >= 0 && iContainsChar (WhitespaceChars, text [xLastCharIndex]))
                xLastCharIndex --;

            // 少なくとも1文字、空白系文字でないものが見つかったとき
            if (xLastCharIndex >= 0)
            {
                // 先頭の空白系文字を数える
                // xLastCharIndex のところにあるものは空白系文字でないと分かっている

                while (xFirstCharIndex < xLastCharIndex && iContainsChar (WhitespaceChars, text [xFirstCharIndex]))
                    xFirstCharIndex ++;

                bool xIsWhitespaceDetected = false;

                for (int temp = xFirstCharIndex; temp <= xLastCharIndex; temp ++)
                {
                    char xCurrent = text [temp];

                    // 分かりやすさのため、条件分岐を少し冗長にしている

                    if (iContainsChar (WhitespaceChars, xCurrent))
                    {
                        if (xIsWhitespaceDetected)
                        {
                            // 既に空白系文字が見つかり、出力されたあとの空白系文字
                            // 何も出力しない
                        }

                        else
                        {
                            // まだ空白系文字が見つかっていないときに一つ見つかったのでフラグを立てて一つ出力

                            xIsWhitespaceDetected = true;
                            builder.Append (' ');
                        }
                    }

                    else
                    {
                        // 空白系でない文字は、フラグの状態に関わらず、必ず出力される
                        // その前に、フラグが立っていればオフにする
                        // 毎回 false を代入してもよいが、条件分岐を挟んだ方が処理の流れが分かりやすい

                        if (xIsWhitespaceDetected)
                            xIsWhitespaceDetected = false;

                        builder.Append (xCurrent);
                    }
                }
            }
        }

        public static string nNormalizeLine (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            text.nNormalizeLine (xBuilder);
            return xBuilder.ToString ();
        }

        // 複数行の方では、空白系でない文字を最初に含む行の以前の空行が削除され、引数によってインデントが保持あるいは削除され、
        // 行末の空白系文字は全て削除され、それによって空白系文字しか持たない行が空行となった上、二つ以上の連続する空行が一つに統合され、
        // たとえば文字列中の改行を <br /> にして全体を <div></div> に入れても <br /></div> とならないように文字列末尾の改行が全て削除される
        // 文字列末尾に改行が残らないというのは、さまざまなところでの出力において改行の数に注意を払う必要がなくて便利である

        // Sat, 18 May 2019 08:53:21 GMT
        // nNormalizeLine は、主にセキュリティーを目的として、\r\n も空白系文字とみなしての処理を行う
        // input などからの入力をこちらに通せば、不正な POST によって複数行の文字列が送り込まれてのバグを回避できる
        // 一方、nNormalize は、インデントを残す動作がデフォルトであり、旧版の Nekote ではできなかったこととして、先頭行のインデントも残す
        // そのため、元々1行しかなかったり、空白系文字が削られた結果として1行になったりの文字列でも、その唯一の行のインデントが残るという仕様にしていた
        // しかし、これは、1) コードを行単位で編集できる機能をどこかで実装するなどで、そのインデントが失われては困る、
        // 2) ユーザーが1行だけ入力する意図を持つときに指が当たったとかで先頭に余計な空白系文字が入る、
        // の二つを比較するにおいて、間違いなく後者の方が頻度が高く、「1行しかなくてもインデントを残すべきか」というのは、ずっと気になっていた
        // そのため、nNormalize を nNormalizeLegacy に改名した上、結果的に1行なら先頭の空白系文字を削る nNormalizeAuto を別に用意した
        // nNormalize を利用できなくしたのは、1) 各プログラムで改めてどちらが適するかを考えたいため、
        // 2) *Line が必要なのにインテリセンスで *Line のない方になり、そのまま見落としてしまうことがたまにあるため
        // *Auto は、ノーマライズしてから行数を見るため、StringBuilder を受け取る多重定義を用意しては、
        // 最後に StringBuilder において文字列全体をスライドさせるか、別の StringBuilder からコピーすることになる
        // いずれも非効率的だし、そもそも StringBuilder を与えて使ったことが過去に一度もないので、return のものだけ用意した

        // Sat, 18 May 2019 09:04:47 GMT
        // 戻り値は、改行の数でなく、改行「文字」の数なので、\r\n は2文字としてカウントされる
        // これを見ることで、最終的な文字列に改行が含まれているかどうかを調べられる

        // Fri, 27 Sep 2019 12:51:18 GMT
        // 使い込んでいるメソッドであり、たぶん大丈夫だが、久々に全体をチェックした
        // 良好に書けているが、パッと見で分かりやすいコードでないためコメントを書き足す
        // xNewlineCharCount は、ノーマライズ後の文字列に含まれる改行文字を数えるもので、これが1以上なら複数行扱いとなる
        // xHasAppendedSomething は、1文字でも出力が行われたかどうかであり、空行の出力の判断などに必要
        // xIsEmptyLineDetected は、空行が元の文字列に含まれているかどうかであり、これが最初に true になるときに出力にも空行が入る
        //
        // 読み込みは行単位で行われ、それぞれについて、先頭と末尾のインデックスを確定し、まず末尾の空白系文字を削る
        // その結果、全ての文字が削られ、末尾のインデックスが負になれば、それは空行なので、xIsEmptyLineDetected を見ながら出力を調整
        // 末尾のインデックスが0以上なら少なくとも1文字は空白系文字でなく、空行でないため、xIsEmptyLineDetected を false に戻し、
        // 今度は、その「少なくとも1文字」より前の文字について、どこまでがインデントか調べ、keepsIndents に基づき、インデントを残したり捨てたり
        // 残す場合、調べる必要すらなく、先頭のインデックスを0のままとしての丸ごとのコピーが通用するが、大きなコストでないし、枯れているコードなので放置
        //
        // xIsWhitespaceDetected は、空白系文字でない先頭と末尾を持つ部分文字列の中で空白系文字が少なくとも一つ見つかったかどうかであり、空白系文字の圧縮に使われる
        // この時点で xLastCharIndex は上記の「少なくとも1文字」に対応するため、for ループには xLastCharIndex も含める
        // それぞれの文字を見ながら、空白系文字かどうかで xIsWhitespaceDetected を切り替え、一つ以上の空白系文字を一つの半角空白に置き換える
        // そのブロックでは、少なくとも1文字の出力が確実に行われるため、改行をつけ、xHasAppendedSomething を true にする
        //
        // 改行が出力されるのは、少なくとも1文字のブロックおよび空行のブロックのみであり、それぞれ \r\n 分で2を足す
        //
        // 最後に xHasAppendedSomething を見るのは、よくある「バッファーに残った文字」の処理のためでなく、
        // 「xHasAppendedSomething が true なら少なくとも1回は改行が出力されている」という条件により、出力の末尾の改行の有無を調べるため
        // これが false なのに改行がついていることはなく、true なのに改行がついていないこともないため、一度で両方を確定できる
        // ノーマライズでは出力全体の末尾に改行をつけないため、\r と \n を個別に削り、その分、カウントを下げていく
        // 少なくとも1文字は空白系文字以外が出力されているため builder.Length > 0 の確認は不要と思うが、コストでないので放置
        //
        // これで builder にノーマライズ済みの文字列が入るため、改行文字の総数を返して終わり

        private static int iNormalize (string text, StringBuilder builder, bool keepsIndents)
        {
            int xNewlineCharCount = 0;
            // 空白系でない文字が初めて現れる行の以前の空行を消したり（厳密には、そういう改行の出力を回避したり）、
            // 実装の都合上、文字列末尾に入りうる最大2組の \r\n を消したりするために使われる
            bool xHasAppendedSomething = false;

            using (StringReader xReader = new StringReader (text))
            {
                string xLine;
                bool xIsEmptyLineDetected = false;

                while ((xLine = xReader.ReadLine ()) != null)
                {
                    // 以下の実装には nNormalizeLine との共通点が多い
                    // メソッド化する選択肢もあるが、頻繁に使うメソッドなので速度を優先

                    int xFirstCharIndex = 0,
                        xLastCharIndex = xLine.Length - 1;

                    while (xLastCharIndex >= 0 && iContainsChar (InlineWhitespaceChars, xLine [xLastCharIndex]))
                        xLastCharIndex --;

                    if (xLastCharIndex >= 0)
                    {
                        if (xIsEmptyLineDetected)
                            xIsEmptyLineDetected = false;

                        while (xFirstCharIndex < xLastCharIndex && iContainsChar (InlineWhitespaceChars, xLine [xFirstCharIndex]))
                            xFirstCharIndex ++;

                        // インデントを残すように指定されていれば、そのまま出力する
                        // 行中ならたとえばタブも半角空白になるが、行頭のものはそのまま残る

                        if (keepsIndents)
                            builder.Append (xLine, 0, xFirstCharIndex);

                        bool xIsWhitespaceDetected = false;

                        for (int temp = xFirstCharIndex; temp <= xLastCharIndex; temp ++)
                        {
                            char xCurrent = xLine [temp];

                            if (iContainsChar (InlineWhitespaceChars, xCurrent))
                            {
                                if (xIsWhitespaceDetected)
                                {
                                    // 何も出力しない
                                }

                                else
                                {
                                    xIsWhitespaceDetected = true;
                                    builder.Append (' ');
                                }
                            }

                            else
                            {
                                if (xIsWhitespaceDetected)
                                    xIsWhitespaceDetected = false;

                                builder.Append (xCurrent);
                            }
                        }

                        // Sat, 18 May 2019 09:06:38 GMT
                        // AppendLine の出力するものが \r\n だとしての決め打ちの実装が気になったが、
                        // .NET Core に移植して、モバイルでも走らせて……とかは、今すぐに必要なことでない
                        // いずれは避けられないとも分かっているが、今は、すぐに必要なものの完成を優先

                        builder.AppendLine ();
                        xNewlineCharCount += 2;

                        // has にしているため、フラグを立てるのは何となく最後がすっきり

                        if (xHasAppendedSomething == false)
                            xHasAppendedSomething = true;
                    }

                    else
                    {
                        if (xIsEmptyLineDetected)
                        {
                            // 空白系文字の連続時に最初の分しか出力しないのと同様、
                            // 空行も、二つ以上続いたときには二つ目以降の分を出力しない
                        }

                        else
                        {
                            xIsEmptyLineDetected = true;

                            // 空白系でない文字の含まれる行の出力を待つことにより、余計な空行が文字列の先頭に残るのを回避
                            // xHasAppendedSomething は一度 true になればずっとそのままであり、これは最初だけのことである

                            if (xHasAppendedSomething)
                            {
                                builder.AppendLine ();
                                xNewlineCharCount += 2;
                            }
                        }
                    }
                }
            }

            // 空白系でない文字の含まれる行が見つかり、出力されたなら、builder の末尾の空白系文字を削っても少なくともその文字で止まる
            // そうでなく、後ろから削ることで元々 builder 内にあった文字まで影響を受けうるときには、xHasAppendedSomething が false である

            if (xHasAppendedSomething)
            {
                while (builder.Length > 0 && iContainsChar (NewlineChars, builder [builder.Length - 1]))
                {
                    builder.Length --;
                    // Sat, 18 May 2019 09:09:14 GMT
                    // 1文字ずつ削っているので、引き算も1文字ずつ行う
                    // \r\n の環境なら問題がないはずだが、しばらく様子見
                    xNewlineCharCount --;
                }
            }

            return xNewlineCharCount;
        }

        public static void nNormalizeLegacy (this string text, StringBuilder builder, bool keepsIndents = true)
        {
            if (string.IsNullOrEmpty (text))
                return;

            // Sat, 18 May 2019 09:14:58 GMT
            // 元々 nNormalize だったものを iNormalize に変更し、改行文字のカウントを追記しただけ
            // text が null などでないかのチェックもこちらに移した上で、同じ引数でそのまま呼ぶ
            iNormalize (text, builder, keepsIndents);
        }

        public static string nNormalizeLegacy (this string text, bool keepsIndents = true)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            text.nNormalizeLegacy (xBuilder, keepsIndents);
            return xBuilder.ToString ();
        }

        // Sat, 18 May 2019 09:17:13 GMT
        // 上の方にも書いたが、ノーマライズの処理が終わってから先頭のインデントを削るため、
        // StringBuilder を受け取ってそこに書き込む多重定義を用意しても、効率的な実装ができない
        // そのため、自前で用意し、ノーマライズを行い、結果的に単一行の場合、インデントを残す動作のときに限って先頭部分を削る
        // keepsIndents が false なら、行数と関係なく、先頭行のインデントも必ず削られているので、xIndex が0のままでいい
        // 今後、ノーマライズにおいては、基本的に *Auto を常用し、1行でないといけないところだけ *Line を使う

        // Thu, 26 Sep 2019 17:09:51 GMT
        // 久々に読んだらコメントが分かりにくくて困惑したので補足
        // keepsIndents は iNormalize がインデントを残すかどうかを指定するもので、
        // それが true のときに限って1行でもインデントが残るから、それを削るということ
        // これが true なら削る、という書き方では、「残せと言われたら残さない」の意味になる
        // 「残せと言われたら残るから外側で削らないといけない」というのが正確

        public static string nNormalizeAuto (this string text, bool keepsIndents = true)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            int xIndex = 0;

            if (iNormalize (text, xBuilder, keepsIndents) == 0 && keepsIndents)
            {
                // Sat, 18 May 2019 23:48:47 GMT
                // パフォーマンスの違いについて iContainsChar のところに書いておく
                while (xIndex < xBuilder.Length && InlineWhitespaceChars.Contains (xBuilder [xIndex]))
                    xIndex ++;
            }

            return xBuilder.ToString (xIndex, xBuilder.Length - xIndex);
        }

        // 単一の文字列を行単位に分割したり、それらを単一の文字列に統合したりのメソッドを用意しておく
        // ループを回すのは List より配列の方が2倍ほど高速との情報が散見されるため、戻り値を配列にしている
        // 統合する方は、string.Join (Environment.NewLine ... とする人が多いようだが、
        // File.WriteAllLines において末尾に改行が出力されるため、そちらに合わせている

        // Mon, 03 Sep 2018 08:08:47 GMT
        // 文字列を受け取って文字列を返すメソッドでは基本的に先頭で null や "" を見ているが、
        // コレクションの操作だったり、行に分けるなどのしっかりした処理だったりなら、そういうことをしない
        // 上記のチェックは、そこに null が通るのが普通であるところのみの救済策である

        // Fri, 26 Apr 2019 02:21:19 GMT
        // 仕様を変更し、null や "" を見るようにした
        // 他でも見ることが多く、整合性が崩れてきていたため

        public static void nSplitIntoLines (this string text, List <string> lines)
        {
            if (string.IsNullOrEmpty (text))
                return;

            using (StringReader xReader = new StringReader (text))
            {
                string xLine;

                while ((xLine = xReader.ReadLine ()) != null)
                    lines.Add (xLine);
            }
        }

        public static string [] nSplitIntoLines (this string text)
        {
            List <string> xLines = new List <string> ();

            if (string.IsNullOrEmpty (text) == false)
                text.nSplitIntoLines (xLines);

            return xLines.ToArray ();
        }

        // ここに nSplitIntoLinesEnumerable の追加を考えたが、そもそもなくてよさそう
        // string [] は元々 IEnumerable で、それは List もそうなので、個別に用意する理由がない
        // ToArray を回避したいなら、呼び出し元で List を生成して引数として与えるだけのこと

        // 一つにまとめる方では、最初 string [] だったが、List <string> も通したくなったので IEnumerable <string> に変更した
        // string [] のメソッドも作って foreach でなく for を使う方が速いだろうが、そういう作り込みは Nekote では避けている

        public static void nJoinLines (this IEnumerable <string> lines, StringBuilder builder)
        {
            foreach (string xLine in lines)
                builder.AppendLine (xLine);
        }

        public static string nJoinLines (this IEnumerable <string> lines)
        {
            StringBuilder xBuilder = new StringBuilder ();
            lines.nJoinLines (xBuilder);
            return xBuilder.ToString ();
        }

        // 変換系の処理では、text が null であるたびにイチイチ落ちると開発効率の低下につながる
        // また、nChar.cs にも書いたが、Nekote では何事においても invariant の方がデフォルトである

        public static string nToLower (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            return text.ToLower (CultureInfo.InvariantCulture);
        }

        public static string nToUpper (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            return text.ToUpper (CultureInfo.InvariantCulture);
        }

        // char.IsDigit は、Unicode カテゴリーを認識するため、余計なチェックも行う
        // CompareNumerically において必要なのは、半角数字かどうか知ることのみ

        private static bool iIsDigit (char value)
        {
            return '0' <= value && value <= '9';
        }

        // 文字列の長さに注意を払いながら、連続する半角数字を数える
        // 一つ目が見つかってからの処理になるため、二つ目以降のインデックスを与える

        private static int iCountDigits (string text, int index)
        {
            int xCount = 0;

            while (index + xCount < text.Length && iIsDigit (text [index + xCount]))
                xCount ++;

            return xCount;
        }

        private static void iFillArray (char [] array, int index, char value, int length)
        {
            for (int temp = 0; temp < length; temp ++)
                array [index + temp] = value;
        }

        private static void iCopyChars (char [] array, int arrayIndex, string text, int textIndex, int length)
        {
            for (int temp = 0; temp < length; temp ++)
                array [arrayIndex + temp] = text [textIndex + temp];
        }

        public static int CompareNumerically (string text1, string text2, bool ignoresCase = false)
        {
            // 戻り値は、左から右を引いてみて、左の方があとに来るべきならプラス、そうでないならマイナス
            // 大小は、文字の比較においては文字コードの大小で決まり、文字列の長さの比較においては長い方が大きい
            // null は、文字列ですらないという点において、"" よりさらに短いとみなされる

            if (text1 == null)
            {
                if (text2 == null)
                    return 0;
                else return -1;
            }

            else
            {
                if (text2 == null)
                    return 1;

                else
                {
                    int xIndex1 = 0,
                        xIndex2 = 0;

                    while (true)
                    {
                        if (xIndex1 >= text1.Length)
                        {
                            if (xIndex2 >= text2.Length)
                            {
                                // 内容に違いがなく、両方が同時に終わった
                                return 0;
                            }

                            else
                            {
                                // 2番目の方はまだ終わっていない
                                return -1;
                            }
                        }

                        else
                        {
                            if (xIndex2 >= text2.Length)
                            {
                                // 1番目の方はまだ終わっていない
                                return 1;
                            }

                            else
                            {
                                // どちらにも文字があるため、比較を行う

                                char xCurrent1 = text1 [xIndex1],
                                    xCurrent2 = text2 [xIndex2];

                                if (iIsDigit (xCurrent1) && iIsDigit (xCurrent2))
                                {
                                    // いずれも数字なので、連続する数字を数え、多い方の長さの配列を二つ作り、左側を0詰めし、数字部分をコピーし、前から比較
                                    // マイナス、桁区切りのカンマ、小数点のドットといったものは、数字の一部と常に断定できるわけでないため対応しない
                                    // xCurrent1 と xCurrent2 のいずれかのみが、連続する数字の先頭でない可能性については考える必要がない
                                    // 片方のみ、連続する数字の二つ目以降に突入しているなら、一つ目の比較のときに文字列の大小が決まっていたからである

                                    int xDigitCount1 = 1 + iCountDigits (text1, xIndex1 + 1),
                                        xDigitCount2 = 1 + iCountDigits (text2, xIndex2 + 1),
                                        xMaxDigitCount = xDigitCount1 >= xDigitCount2 ? xDigitCount1 : xDigitCount2;

                                    char [] xDigits1 = new char [xMaxDigitCount],
                                        xDigits2 = new char [xMaxDigitCount];

                                    int xPaddingLength1 = xMaxDigitCount - xDigitCount1,
                                        xPaddingLength2 = xMaxDigitCount - xDigitCount2;

                                    iFillArray (xDigits1, 0, '0', xPaddingLength1);
                                    iCopyChars (xDigits1, xPaddingLength1, text1, xIndex1, xDigitCount1);
                                    iFillArray (xDigits2, 0, '0', xPaddingLength2);
                                    iCopyChars (xDigits2, xPaddingLength2, text2, xIndex2, xDigitCount2);

                                    for (int temp = 0; temp < xMaxDigitCount; temp ++)
                                    {
                                        if (xDigits1 [temp] > xDigits2 [temp])
                                            return 1;
                                        else if (xDigits1 [temp] < xDigits2 [temp])
                                            return -1;
                                    }

                                    // 数字部分が数値的に一致したため、数字の数だけインデックスを進めて比較を続行

                                    xIndex1 += xDigitCount1;
                                    xIndex2 += xDigitCount2;
                                }

                                else
                                {
                                    // ignoresCase が false なら CompareTo を直接呼んだ方が速いが、
                                    // true のときの処理にクセがあるので、サクッと nChar.Compare をかませている
                                    // 処理のクセについては、nChar.Compare のところのコメントが詳しい
                                    int xResult = nChar.Compare (xCurrent1, xCurrent2, ignoresCase);

                                    if (xResult != 0)
                                        return xResult;

                                    else
                                    {
                                        // 一致なので両方とも次の文字へ進む

                                        xIndex1 ++;
                                        xIndex2 ++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // 比較の処理を伴うメソッドは、多重定義を含めて数が多く、そのうち多くが単純なラッパーであり、コードの水増しになる
        // 一方で、使用頻度も高く、用意しておいて損のないラッパーであるのも確かなので、うるさくならないように #region に入れておく
        #region 比較の処理を伴うメソッド // OK
        // MSDN の string 型のページで CultureInfo を検索して関連性の認められる全メソッドをラップしておく
        // == が内部的には Equals とのことで、case-sensitive and culture-insensitive と書かれているため、
        // よく使う Compare (string, string, bool) などが uses the current culture というのはどうかと思う
        // https://msdn.microsoft.com/en-us/library/system.string.aspx
        // https://msdn.microsoft.com/en-us/library/system.string.op_equality.aspx
        // https://msdn.microsoft.com/en-us/library/1hkt4325.aspx
        // https://msdn.microsoft.com/en-us/library/zkcaxw5y.aspx

        public static int Compare (string text1, string text2, bool ignoresCase = false)
        {
            // Fri, 26 Apr 2019 03:14:20 GMT
            // 引数が null でも落ちないようなのでチェックしない

            return string.Compare (text1, text2, ignoresCase, CultureInfo.InvariantCulture);
        }

        // Fri, 26 Apr 2019 02:23:15 GMT
        // 周囲との仕様の整合性が乱れていたため、こちらでも null や "" を見る
        // null だと意識して false をもらいたくて呼ぶメソッドでないが、
        // 思わぬところから出てきた null でプログラムが落ちるリスクを減らしたい

        public static bool nStartsWith (this string text1, string text2, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text1))
                return false;

            return text1.StartsWith (text2, ignoresCase, CultureInfo.InvariantCulture);
        }

        public static bool nEndsWith (this string text1, string text2, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text1))
                return false;

            return text1.EndsWith (text2, ignoresCase, CultureInfo.InvariantCulture);
        }

        // IndexOf 系は、CultureInfo をとらないが、StringComparison の指定は受ける
        // いろいろあるうち、おそらく OrdinalIgnoreCase である、文字を探すものや *Any を除外し、
        // 以下では、カルチャーが InvariantCulture で決め打ちのものを一応は揃えておく

        public static int nIndexOf (this string text1, string text2, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text1))
                return -1;

            return text1.IndexOf (text2, ignoresCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }

        public static int nIndexOf (this string text1, int index, string text2, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text1))
                return -1;

            return text1.IndexOf (text2, index, ignoresCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }

        public static int nIndexOf (this string text1, int index, int length, string text2, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text1))
                return -1;

            return text1.IndexOf (text2, index, length, ignoresCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }

        // 文字列に文字列が含まれるかどうかのチェックは頻繁に行われる
        // そのため、nIndexOf で容易に行えることだが、コードの可読性向上も考えて専門のメソッドを用意
        // 探す範囲を指定してまで存在するかどうかだけを調べることは稀なので、そういうものは用意しない

        public static bool nContains (this string text1, string text2, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text1))
                return false;

            return text1.nIndexOf (text2, ignoresCase) >= 0;
        }

        public static int nLastIndexOf (this string text1, string text2, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text1))
                return -1;

            return text1.LastIndexOf (text2, ignoresCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }

        public static int nLastIndexOf (this string text1, int index, string text2, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text1))
                return -1;

            return text1.LastIndexOf (text2, index, ignoresCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }

        public static int nLastIndexOf (this string text1, int index, int length, string text2, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text1))
                return -1;

            return text1.LastIndexOf (text2, index, length, ignoresCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }

        // Replace は、This method performs an ordinal (case-sensitive and culture-insensitive) search to find oldValue となっていて、
        // 他のメソッドと異なり、デフォルトで ordinal だし、そもそも ordinal でないオーバーライドが用意されていない
        // しかし、すぐにそういったことも忘れ、Replace を呼ぼうとしては、「これ、カルチャー大丈夫かな」と気になるのが分かっているため、n* を用意しておく
        // https://msdn.microsoft.com/en-us/library/fk49wtc1.aspx

        // 追記: ignoresCase を追加し、おそらく Unicode テーブルを見るのであろう .NET の実装に頼っての大文字・小文字を区別しない置換も実装した
        // 明示的に引数をチェックしないため、null なら ArgumentNullException が投げられることなく落ちるが、Nekote ではそのあたりは若干手抜きしている

        public static string nReplace (this string text, string oldText, string newText, bool ignoresCase = false)
        {
            if (ignoresCase)
            {
                // 元の文字列または検索対象が空のときや、検索対象の方が長くて明らかに比較が行われないとき
                // そのうち元の文字列が空の場合は、検索対象が空でないなら、長さの比較のところで分かるためチェックを省略できるが、
                // 多数の文字列をループで nReplace に放り込むようなケースにおいては text の長さを一番に見てよいし、
                // Nekote では、他のメソッドでも最初に string.IsNullOrEmpty に通すことが多いため、それと整合させている

                if (text.Length == 0 || oldText.Length == 0 || text.Length < oldText.Length)
                    return text;

                StringBuilder xBuilder = new StringBuilder ();

                // 終了条件もインクリメントもないループなので必ず抜けるのかどうか懸念があるが、
                // nIndexOf は、見付けて次に進むか、そうでないなら return するかであり、おそらく大丈夫
                // String.IndexOf は検索対象が "" だと0を返すという理解しがたい仕様になっていて、
                // oldText が空だと、temp が進まないまま "" が見付かり続けるという無限ループになるが、
                // それは最初に oldText の長さを調べることで確実に回避できている
                // temp が進むなら、String.IndexOf が終わるか落ちるかするためデバッグできる

                for (int temp = 0; ; )
                {
                    // 検索対象が1文字で、それが末尾で見付かると、text.Length - temp が0になるが、
                    // nIndexOf が内部で呼ぶ IndexOf は、nReplace と異なり、検索範囲の長さを問わない
                    int xIndex = text.nIndexOf (temp, text.Length - temp, oldText, true);

                    if (xIndex >= 0)
                    {
                        // その回の検索開始位置から見付かった位置までを元の文字列からコピー
                        xBuilder.Append (text, temp, xIndex - temp);
                        xBuilder.Append (newText);
                        temp = xIndex + oldText.Length;
                    }

                    else
                    {
                        xBuilder.Append (text, temp, text.Length - temp);
                        return xBuilder.ToString ();
                    }
                }
            }

            else return text.Replace (oldText, newText);
        }

        // トリミングは nNormalize* と似ている処理であり、
        // 同じ文字を処理する Nekote 版の nTrim* があるべき

        public static string nTrim (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            return text.Trim (WhitespaceChars);
        }

        public static string nTrimStart (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            return text.TrimStart (WhitespaceChars);
        }

        public static string nTrimEnd (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            return text.TrimEnd (WhitespaceChars);
        }

        // nString では、過度なラップを避けるため、比較や検索の対象としては文字「列」だけを考えていたが、
        // 実際に nString を使うにおいて文字を扱うメソッドを探すことが何度かあったのでやはり揃える

        public static bool nStartsWith (this string text, char value, bool ignoresCase = false)
        {
            // 拡張メソッドはインスタンスメソッドのように表記されて呼ばれるため、text が null でも落ちないのは違和感がある
            // しかし、Nekote では、多くのメソッドにおいて、文字列が null や空ではむやみに落ちずにとりあえず処理が続くようにしている
            // 一方で、文字「列」を扱う nStartsWith などは、.NET のインスタンスメソッドを呼ぶためガッツリ落ちる
            // それを問題視し、それらのメソッドでも入力が null なら落ちなくするようなことは、利益の乏しい過実装になるためやめておく

            if (string.IsNullOrEmpty (text) == false &&
                    nChar.Compare (text [0], value, ignoresCase) == 0)
                return true;
            else return false;
        }

        public static bool nEndsWith (this string text, char value, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text) == false &&
                    nChar.Compare (text [text.Length - 1], value, ignoresCase) == 0)
                return true;
            else return false;
        }

        // 部分文字列から一つの文字を探すときに丸ごと小文字にしてしまうのは非常に無駄がある
        // 一方、value を小文字にした上、text も1文字ずつ小文字にして比較するのも、.NET なので無駄が多そう
        // また、メソッド間で、自ら処理するのか .NET に頼るのかの不整合があるのはよくても、単一メソッド内では動作を揃えたい
        // nReplace のように、そうせざるを得ないところでは不揃いになるのも仕方ないが、nIndexOf では揃えられる
        // さらに、nIndexOf で大文字・小文字を区別せずに部分文字列をループで検索するようなことは、その場しのぎのコードでしかやらない
        // 非効率的だと分かっていての実装なので、そういうのをするのは、パッとテストコードを書いて走らせて消すときくらいだろう
        // ちゃんとした実装では最初に全体を小文字にするため、以下では ignoresCase によらず IndexOf に頼る

        public static int nIndexOf (this string text, char value, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text))
                return -1;

            if (ignoresCase)
                return text.nToLower ().IndexOf (value.nToLower ());
            else return text.IndexOf (value);
        }

        public static int nIndexOf (this string text, int index, char value, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text))
                return -1;

            if (ignoresCase)
                return text.nToLower ().IndexOf (value.nToLower (), index);
            else return text.IndexOf (value, index);
        }

        public static int nIndexOf (this string text, int index, int length, char value, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text))
                return -1;

            if (ignoresCase)
                return text.nToLower ().IndexOf (value.nToLower (), index, length);
            else return text.IndexOf (value, index, length);
        }

        // params を受け取る以下のメソッドには ignoresCase を用意しない
        // 引数の順序が変わると使いにくいし、ignoresCase があるものとないものを用意するのも面倒だし、
        // そもそも、params で指定する程度の文字数なら大文字と小文字の両方を書けば済むため

        public static int nIndexOfAny (this string text, params char [] values)
        {
            if (string.IsNullOrEmpty (text))
                return -1;

            return text.IndexOfAny (values);
        }

        public static int nIndexOfAny (this string text, int index, params char [] values)
        {
            if (string.IsNullOrEmpty (text))
                return -1;

            return text.IndexOfAny (values, index);
        }

        public static int nIndexOfAny (this string text, int index, int length, params char [] values)
        {
            if (string.IsNullOrEmpty (text))
                return -1;

            return text.IndexOfAny (values, index, length);
        }

        // Contains* はよく使われるので、文字を検索対象とするものも用意しておく
        // nIndexOf* で代用可能なので、index / length をとるものまでは作らない

        public static bool nContains (this string text, char value, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text))
                return false;

            return text.nIndexOf (value, ignoresCase) >= 0;
        }

        public static bool nContainsAny (this string text, params char [] values)
        {
            if (string.IsNullOrEmpty (text))
                return false;

            return text.nIndexOfAny (values) >= 0;
        }

        public static int nLastIndexOf (this string text, char value, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text))
                return -1;

            if (ignoresCase)
                return text.nToLower ().LastIndexOf (value.nToLower ());
            else return text.LastIndexOf (value);
        }

        public static int nLastIndexOf (this string text, int index, char value, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text))
                return -1;

            if (ignoresCase)
                return text.nToLower ().LastIndexOf (value.nToLower (), index);
            else return text.LastIndexOf (value, index);
        }

        public static int nLastIndexOf (this string text, int index, int length, char value, bool ignoresCase = false)
        {
            if (string.IsNullOrEmpty (text))
                return -1;

            if (ignoresCase)
                return text.nToLower ().LastIndexOf (value.nToLower (), index, length);
            else return text.LastIndexOf (value, index, length);
        }

        public static int nLastIndexOfAny (this string text, params char [] values)
        {
            if (string.IsNullOrEmpty (text))
                return -1;

            return text.LastIndexOfAny (values);
        }

        public static int nLastIndexOfAny (this string text, int index, params char [] values)
        {
            if (string.IsNullOrEmpty (text))
                return -1;

            return text.LastIndexOfAny (values, index);
        }

        public static int nLastIndexOfAny (this string text, int index, int length, params char [] values)
        {
            if (string.IsNullOrEmpty (text))
                return -1;

            return text.LastIndexOfAny (values, index, length);
        }

        // 以下の二つの nReplace は、文字「列」をとるもののほぼコピペである
        // ignoresCase のある方のみ、効率化のために最初に text 全体を小文字にする

        public static string nReplace (this string text, char oldChar, char newChar, bool ignoresCase = false)
        {
            if (ignoresCase)
            {
                string xText = text.nToLower ();
                StringBuilder xBuilder = new StringBuilder ();

                for (int temp = 0; ; )
                {
                    // 最初に text 全体を小文字にしてから、ここでは ignoresCase == false で検索
                    // 文字「列」の方の nReplace のコメントにも書いたが、IndexOf は検索範囲の長さを問わない
                    int xIndex = xText.nIndexOf (temp, text.Length - temp, oldChar, false);

                    if (xIndex >= 0)
                    {
                        // 検索には xText を使うが、出力するのは元の文字列
                        xBuilder.Append (text, temp, xIndex - temp);
                        xBuilder.Append (newChar);
                        temp = xIndex + 1;
                    }

                    else
                    {
                        xBuilder.Append (text, temp, text.Length - temp);
                        return xBuilder.ToString ();
                    }
                }
            }

            else return text.Replace (oldChar, newChar);
        }

        // 他のメソッドと異なり、引数が右から左になっているため使用時には注意が必要
        public static string nReplaceAny (this string text, char newChar, params char [] oldChars)
        {
            // 文字や文字「列」をとる nReplace が、text が null や空である可能性を考えないため、
            // こちらでもそういうチェックを行わずに、いきなり StringBuilder を作ってループをまわす

            StringBuilder xBuilder = new StringBuilder ();

            for (int temp = 0; ; )
            {
                int xIndex = text.nIndexOfAny (temp, text.Length - temp, oldChars);

                if (xIndex >= 0)
                {
                    xBuilder.Append (text, temp, xIndex - temp);
                    xBuilder.Append (newChar);
                    temp = xIndex + 1;
                }

                else
                {
                    xBuilder.Append (text, temp, text.Length - temp);
                    return xBuilder.ToString ();
                }
            }
        }

        // Tue, 04 Dec 2018 15:36:50 GMT
        // 改行を置換するメソッドが意外となかったので今さら実装した
        // たいていのところで、ノーマライズをかけての \r\n の置換で済んでいたからだろう

        public static string nReplaceNewLines (this string text, string replacement)
        {
            // Tue, 04 Dec 2018 15:38:01 GMT
            // 他の置換系のメソッドと同様、text をチェックしない
            // text が null などなら、結果が不定としてそこで落ちるべき

            StringBuilder xBuilder = new StringBuilder ();

            for (int temp = 0; temp < text.Length; temp ++)
            {
                char xCurrent = text [temp];

                if (xCurrent == '\r')
                {
                    if (temp + 1 < text.Length && text [temp + 1] == '\n')
                        temp ++;

                    xBuilder.Append (replacement);
                }

                else if (xCurrent == '\n')
                    xBuilder.Append (replacement);
                else xBuilder.Append (xCurrent);
            }

            return xBuilder.ToString ();
        }

        // .NET のメソッドをそのままラップするだけだが、
        // カルチャーについて心配しなくてよいメソッドを nString にはいろいろ揃えたので、
        // いつも通り nString. まで書いてから必要なものが見付からないと困惑する

        public static string nTrim (this string text, params char [] chars)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            return text.Trim (chars);
        }

        public static string nTrimStart (this string text, params char [] chars)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            return text.TrimStart (chars);
        }

        public static string nTrimEnd (this string text, params char [] chars)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            return text.TrimEnd (chars);
        }
        #endregion

        // 文字列を段落分けしたり、段落の配列を文字列に戻したりできるようにしておく
        // 他の類似メソッドと同様、文字列を洗う類いのことは避けていて、半角空白一つでも「段落」となる
        // 空行が二つ以上連続するときには、それらをひとまとめにして段落の区切りとして処理する実装にしている
        // そのため、iAnalyzeNewlines により、少なくとも一つ見付かった改行文字が段落の終わりかどうかを判別する

        private static void iAnalyzeNewlines (string text, int index, out bool isEndOfParagraph, out int letterCount)
        {
            int xNewlineCount = 0,
                xLetterCount = 0;

            for (int temp = index; temp < text.Length; temp ++)
            {
                char xCurrent = text [temp];

                if (xCurrent == '\r' || xCurrent == '\n')
                {
                    if (xCurrent == '\r')
                    {
                        if (temp + 1 < text.Length && text [temp + 1] == '\n')
                        {
                            temp ++;
                            // このメソッドを呼ぶ側でも、ループの temp を飛ばす必要がある
                            // そのため、何文字で改行いくつなのかを内部的に数えていく
                            xLetterCount ++;
                        }
                    }

                    xNewlineCount ++;
                    xLetterCount ++;
                }

                else
                {
                    // 改行が一つで、直後に改行でない文字が続くなら（if の方）、それは段落中の改行である
                    // このメソッドは、改行文字が少なくとも一つ見付かったときに呼ばれるものであり、
                    // この下の else は、xNewlineCount が0となることはないため、改行が二つ以上のとき
                    // 改行が二つ以上、つまり、一つ以上の空行があり、改行でない文字が続くなら、それは段落の終わりである

                    if (xNewlineCount == 1)
                    {
                        isEndOfParagraph = false;
                        letterCount = xLetterCount;
                        return;
                    }

                    else
                    {
                        isEndOfParagraph = true;
                        letterCount = xLetterCount;
                        return;
                    }
                }
            }

            // index のところに少なくとも1文字は読めるものがあって呼ばれるメソッドなので、
            // その後、改行文字しか見付からず、for ループ内の else に入らずにループを抜けたなら、
            // 見付かった改行の数に関わりなく、それが最後の段落の終わりだと判断できる

            isEndOfParagraph = true;
            letterCount = xLetterCount;
        }

        public static void nSplitIntoParagraphs (this string text, List <string> paragraphs)
        {
            if (string.IsNullOrEmpty (text))
                return;

            StringBuilder xBuilder = new StringBuilder ();

            for (int temp = 0; temp < text.Length; temp ++)
            {
                char xCurrent = text [temp];

                if (xCurrent == '\r' || xCurrent == '\n')
                {
                    iAnalyzeNewlines (text, temp, out bool xIsEndOfParagraph, out int xLetterCount);

                    // 改行でない文字が見付かっていて、xBuilder が空でない場合のみ、処理が行われる
                    // 見付かった改行が一つだけなら xIsEndOfParagraph が false となり、段落中の改行となる
                    // 複数が見付かったか、文字列が終わったなら、その時点の xBuilder の内容を段落として出力

                    if (xBuilder.Length > 0)
                    {
                        if (xIsEndOfParagraph == false)
                            xBuilder.AppendLine ();

                        else
                        {
                            paragraphs.Add (xBuilder.ToString ());
                            xBuilder.Clear ();
                        }
                    }

                    // 連続する改行文字の分、ループを先に進める
                    // 文字列の末尾まで改行文字なら、このまま読み終わる
                    temp += xLetterCount - 1;
                }

                // インデックスを保持しての Substring も考えたが、
                // 他のところでも1文字ずつコピーしているため、一貫させた
                else xBuilder.Append (xCurrent);
            }

            // 改行でない文字および段落中の改行を読んでいるうちに文字列が終わることはよくある
            // そういったものが xBuilder に残っているなら段落として出力するが、Clear は不要

            if (xBuilder.Length > 0)
            {
                paragraphs.Add (xBuilder.ToString ());
                // xBuilder.Clear ();
            }
        }

        public static string [] nSplitIntoParagraphs (this string text)
        {
            List <string> xParagraphs = new List <string> ();

            if (string.IsNullOrEmpty (text) == false)
                text.nSplitIntoParagraphs (xParagraphs);

            return xParagraphs.ToArray ();
        }

        // 複数行を一つにする方と同じく、こちらも string [] から IEnumerable <string> に変更した
        // 行も段落も、億単位の数のものをループで何度も処理するようなことはまずないため、作り込みは生産的でない

        public static void nJoinParagraphs (this IEnumerable <string> paragraphs, StringBuilder builder)
        {
            bool xIsFirst = true;

            foreach (string xParagraph in paragraphs)
            {
                if (xIsFirst)
                    xIsFirst = false;
                else builder.AppendLine ();

                // このように実装すると、最後の段落の直後に改行が入るが、それでよい
                // .NET でも、行の配列をファイルに出力するなどすれば末尾に改行が一つ置かれる
                // 複数行を単一の文字列にするなら、間に改行を挟むだけでよいが、そういう実装にはなっていない
                // そのため、複数の改行を単一の文字にするところでも、間に空行を挟むだけの実装にはしない
                builder.AppendLine (xParagraph);
            }
        }

        public static string nJoinParagraphs (this IEnumerable <string> paragraphs)
        {
            StringBuilder xBuilder = new StringBuilder ();
            paragraphs.nJoinParagraphs (xBuilder);
            return xBuilder.ToString ();
        }

        // .NET の実装に依存しないハッシュ計算のアルゴリズムが欲しかったので、Java の実装をコピーした
        // Java もかつてハッシュ計算のアルゴリズムを刷新したことがあるようで、.NET でもそれが起こらないとは限らない
        // いろいろなアルゴリズムがある中で Java のものを選んだのは、有名言語かつ処理が極めてシンプルだからである
        // 最後のページにある "Welcome to Tutorialspoint.com" から同一の1186874997が得られるため、ちゃんと実装できているはず
        // https://en.wikipedia.org/wiki/List_of_hash_functions
        // https://en.wikipedia.org/wiki/Java_hashCode%28%29
        // https://docs.oracle.com/javase/7/docs/api/java/lang/String.html#hashCode%28%29
        // http://grepcode.com/file/repository.grepcode.com/java/root/jdk/openjdk/8u40-b25/java/lang/String.java#String.hashCode%28%29
        // http://hg.openjdk.java.net/jdk8/jdk8/jdk/file/687fd7c7986d/src/share/classes/java/lang/String.java
        // https://www.tutorialspoint.com/java/java_string_hashcode.htm

        public static int nGetHashCode (this string text)
        {
            // null だけ見れば足りるが、"" は頻出するので最初に蹴る
            // 文字列が短いときに上位ビットのほとんどが0のハッシュを返さないためには初期値を0以外にするのも選択肢だが、
            // null や "" が0になるのは仕様としてきれいだし、Java の実装との互換性が失われるのも避けたい
            // 最初から上位ビットまで埋めたいなら、text を prefix/text のようにするのが良いだろう
            // このフォーマットにする理由については、nImage.ToArgbColor のコメントに書いた

            // Sun, 31 Mar 2019 02:52:17 GMT
            // nHash.ComputeSimple に同様の実装を行ったので、こちらを更新するときには反映する

            if (string.IsNullOrEmpty (text))
                return 0;

            int xHash = 0;

            for (int temp = 0; temp < text.Length; temp ++)
                // すぐにオーバーフローするため、unchecked キーワードを指定する必要がある
                // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/unchecked
                xHash = unchecked (31 * xHash + text [temp]);

            return xHash;
        }
    }
}

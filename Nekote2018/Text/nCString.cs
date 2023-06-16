using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nCString
    {
        // C 言語でエスケープされる文字のうち、行単位・タブ区切りの表の出力に影響するものだけを扱う
        // 識別子に C と入れているが、全ての文字が処理されるわけでないため注意が必要である
        // https://en.wikipedia.org/wiki/Escape_sequences_in_C

        public static void nUnescapeC (this string text, StringBuilder builder)
        {
            if (string.IsNullOrEmpty (text))
                return;

            for (int temp = 0; temp < text.Length; temp ++)
            {
                char xCurrent = text [temp];

                if (xCurrent == '\\')
                {
                    temp ++;

                    if (temp < text.Length)
                    {
                        char xNext = text [temp];

                        if (xNext == 't')
                            builder.Append ('\t');
                        else if (xNext == 'r')
                            builder.Append ('\r');
                        else if (xNext == 'n')
                            builder.Append ('\n');
                        else if (xNext == '\\')
                            builder.Append ('\\');
                        else throw new nInvalidFormatException ();
                    }

                    else throw new nInvalidFormatException ();
                }

                else builder.Append (xCurrent);
            }
        }

        public static string nUnescapeC (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            text.nUnescapeC (xBuilder);
            return xBuilder.ToString ();
        }

        public static void nEscapeC (this string text, StringBuilder builder)
        {
            if (string.IsNullOrEmpty (text))
                return;

            foreach (char xChar in text)
            {
                if (xChar == '\t')
                    builder.Append (@"\t");
                else if (xChar == '\r')
                    builder.Append (@"\r");
                else if (xChar == '\n')
                    builder.Append (@"\n");
                else if (xChar == '\\')
                    builder.Append (@"\\");
                else builder.Append (xChar);
            }
        }

        public static string nEscapeC (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            text.nEscapeC (xBuilder);
            return xBuilder.ToString ();
        }

        #region 縦線の | も処理するメソッド
        // Thu, 25 Apr 2019 22:06:40 GMT
        // 複数の文字列を単一行にまとめるには、元々処理できるタブを区切り文字として使う選択肢もあるが、可読性に問題がある
        // CSV も、セルに普通に入る , を使ってでも TSV を回避することに比較的ユーザーの利益があるから非効率が無視されるのだと思う
        // Nekote でも同様の理由によってタブを避けたいが、内容によく使われる , を使うことにも抵抗があるため、
        // , ほど一般的ではないと Wikipedia に書かれているが、区切りに使われることもあると Delimiter のところにある | を採用
        // hoge,moge,poge と hoge|moge|poge を見比べるに、前者では、三つの単語が , でつなげられた単一のフィールドの可能性を否定できない
        // 仕様としての一般性はやや低下しようと、パッと見て明らかにフィールド区切りである文字を使うことに長期的なメリットが見える
        // どうエスケープするかについては、他に使われると知っていて \v も考えたが、やはり誤解につながるので \| にする
        // | は、主役級の記号なのに英語や日本語で全く使われないのが都合良く、ユーザーが \| を目にすることはほとんどないだろう
        // https://en.wikipedia.org/wiki/Vertical_bar
        // https://en.wikipedia.org/wiki/Escape_sequences_in_C

        // Thu, 25 Apr 2019 22:21:05 GMT
        // _ex というメソッド名を最初に考えたが、今後もこういうイレギュラーなことをしそうなので仕様をメソッド名に入れる
        // 単一の大文字に CamelCase で後続の単語をつなげるときには小文字にして _ で区切ることにしているので、ここでもそうしている
        // CAnd とする人も多いが、それは SQLite 的でもあって、単語の区切れを機械的に判断できないため避けたい

        // Thu, 25 Apr 2019 22:26:54 GMT
        // 他のメソッドと同じように拡張メソッドにしないのは、一般的なメソッドでないため
        // こういうものがインテリセンスで大量に出てきたら、それはそれで不便

        public static void UnescapeC_andVerticalBar (string text, StringBuilder builder)
        {
            if (string.IsNullOrEmpty (text))
                return;

            for (int temp = 0; temp < text.Length; temp ++)
            {
                char xCurrent = text [temp];

                if (xCurrent == '\\')
                {
                    temp ++;

                    if (temp < text.Length)
                    {
                        char xNext = text [temp];

                        if (xNext == 't')
                            builder.Append ('\t');
                        else if (xNext == '|')
                            builder.Append ('|');
                        else if (xNext == 'r')
                            builder.Append ('\r');
                        else if (xNext == 'n')
                            builder.Append ('\n');
                        else if (xNext == '\\')
                            builder.Append ('\\');
                        else throw new nInvalidFormatException ();
                    }

                    else throw new nInvalidFormatException ();
                }

                else builder.Append (xCurrent);
            }
        }

        public static string UnescapeC_andVerticalBar (string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            UnescapeC_andVerticalBar (text, xBuilder);
            return xBuilder.ToString ();
        }

        public static void EscapeC_andVerticalBar (string text, StringBuilder builder)
        {
            if (string.IsNullOrEmpty (text))
                return;

            foreach (char xChar in text)
            {
                if (xChar == '\t')
                    builder.Append (@"\t");
                else if (xChar == '|')
                    builder.Append (@"\|");
                else if (xChar == '\r')
                    builder.Append (@"\r");
                else if (xChar == '\n')
                    builder.Append (@"\n");
                else if (xChar == '\\')
                    builder.Append (@"\\");
                else builder.Append (xChar);
            }
        }

        public static string EscapeC_andVerticalBar (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            EscapeC_andVerticalBar (text, xBuilder);
            return xBuilder.ToString ();
        }
        #endregion
    }
}

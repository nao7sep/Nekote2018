using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nHtmlString
    {
        // .NET に用意されているメソッドを呼び出すラッパーメソッドにしても足りたが、
        // StringBuilder の単一のインスタンスへの連続的な書き込みのために新たに実装してみた
        // データベース内などの文字列を HTML ページ用にエスケープするのが主となるはずで、
        // それを元に戻すことはめったになく、他の文字を扱うことなどまずないため、
        // 必ず変換しなければならない5文字のみに処理の対象を絞り込んだ

        // Fri, 10 May 2019 20:05:41 GMT
        // なぜ比較をループにしなかったのか、久々に見ると謎のコードだが、
        // たぶん高速化を考えたのだろうし、今からループにする利益もないため放置

        public static void nUnescapeHtml (this string text, StringBuilder builder)
        {
            if (string.IsNullOrEmpty (text))
                return;

            for (int temp = 0; temp < text.Length; temp ++)
            {
                char xCurrent = text [temp];

                if (xCurrent == '&')
                {
                    int xRemainingLength = text.Length - temp - 1;

                    if (xRemainingLength >= 3)
                    {
                        if (text [temp + 1] == 'l' &&
                            text [temp + 2] == 't' &&
                            text [temp + 3] == ';')
                        {
                            temp += 3;
                            builder.Append ('<');
                            continue;
                        }

                        if (text [temp + 1] == 'g' &&
                            text [temp + 2] == 't' &&
                            text [temp + 3] == ';')
                        {
                            temp += 3;
                            builder.Append ('>');
                            continue;
                        }
                    }

                    if (xRemainingLength >= 4)
                    {
                        if (text [temp + 1] == 'a' &&
                            text [temp + 2] == 'm' &&
                            text [temp + 3] == 'p' &&
                            text [temp + 4] == ';')
                        {
                            temp += 4;
                            builder.Append ('&');
                            continue;
                        }
                    }

                    if (xRemainingLength >= 5)
                    {
                        if (text [temp + 1] == 'q' &&
                            text [temp + 2] == 'u' &&
                            text [temp + 3] == 'o' &&
                            text [temp + 4] == 't' &&
                            text [temp + 5] == ';')
                        {
                            temp += 5;
                            builder.Append ('"');
                            continue;
                        }

                        if (text [temp + 1] == 'a' &&
                            text [temp + 2] == 'p' &&
                            text [temp + 3] == 'o' &&
                            text [temp + 4] == 's' &&
                            text [temp + 5] == ';')
                        {
                            temp += 5;
                            builder.Append ('\'');
                            continue;
                        }
                    }

                    throw new nInvalidFormatException ();
                }

                else builder.Append (xCurrent);
            }
        }

        public static string nUnescapeHtml (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            text.nUnescapeHtml (xBuilder);
            return xBuilder.ToString ();
        }

        public static void nEscapeHtml (this string text, StringBuilder builder)
        {
            if (string.IsNullOrEmpty (text))
                return;

            foreach (char xChar in text)
            {
                if (xChar == '"')
                    builder.Append ("&quot;");
                else if (xChar == '&')
                    builder.Append ("&amp;");
                else if (xChar == '\'')
                    builder.Append ("&apos;");
                else if (xChar == '<')
                    builder.Append ("&lt;");
                else if (xChar == '>')
                    builder.Append ("&gt;");
                else builder.Append (xChar);
            }
        }

        public static string nEscapeHtml (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            text.nEscapeHtml (xBuilder);
            return xBuilder.ToString ();
        }

        // Sun, 31 Mar 2019 06:05:58 GMT
        // テキストを <br /> だけの HTML または <p> も含むものに変換するニーズがたまにあるので、ちゃんと実装しておく
        // インデントを残すかどうか、タブの幅、全角空白の幅、改行のタグ丸ごとを指定できるため、それなりに使えると思う

        public static void nEscapeHtmlLines (this string text, StringBuilder builder,
            bool keepsIndents = true, int tabWidth = 4, int fullWidthSpaceWidth = 2,
            string newLineTag = "<br />")
        {
            string [] xEscapedLines = text.nEscapeHtml ().nSplitIntoLines ();

            string xTabReplacement = nString.Repeat ("&nbsp;", tabWidth),
                xFullWidthSpaceReplacement = nString.Repeat ("&nbsp;", fullWidthSpaceWidth);

            for (int temp = 0; temp < xEscapedLines.Length; temp ++)
            {
                if (temp > 0)
                    builder.Append (newLineTag);

                bool xIsInIndent = true;

                foreach (char xChar in xEscapedLines [temp])
                {
                    if (xIsInIndent)
                    {
                        if (nString.InlineWhitespaceChars.Contains (xChar))
                        {
                            if (keepsIndents)
                            {
                                if (xChar == '\t')
                                    builder.Append (xTabReplacement);
                                else if (xChar == '\u3000')
                                    builder.Append (xFullWidthSpaceReplacement);
                                else builder.Append ("&nbsp;");
                            }
                        }

                        else
                        {
                            xIsInIndent = false;
                            builder.Append (xChar);
                        }
                    }

                    else builder.Append (xChar);
                }
            }
        }

        public static string nEscapeHtmlLines (this string text,
            bool keepsIndents = true, int tabWidth = 4, int fullWidthSpaceWidth = 2,
            string newLineTag = "<br />")
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            text.nEscapeHtmlLines (xBuilder, keepsIndents, tabWidth, fullWidthSpaceWidth, newLineTag);
            return xBuilder.ToString ();
        }

        public static void nEscapeHtmlParagraphs (this string text, StringBuilder builder,
            bool keepsIndents = true, int tabWidth = 4, int fullWidthSpaceWidth = 2,
            string paragraphStartTag = "<p>", string paragraphEndTag = "</p>", string newLineTag = "<br />")
        {
            // Sun, 31 Mar 2019 06:09:12 GMT
            // <br /> だけの方では最初に HTML エスケープを行うが、こちらでは nEscapeHtmlLines に任せる
            // 細かい作り込みをしていないが、実際の使用時には text に事前にノーマライズがかかっているのが普通なので問題ない

            foreach (string xParagraph in text.nSplitIntoParagraphs ())
            {
                builder.Append (paragraphStartTag);
                builder.Append (xParagraph.nEscapeHtmlLines (keepsIndents, tabWidth, fullWidthSpaceWidth, newLineTag));
                builder.Append (paragraphEndTag);
            }
        }

        public static string nEscapeHtmlParagraphs (this string text,
            bool keepsIndents = true, int tabWidth = 4, int fullWidthSpaceWidth = 2,
            string paragraphStartTag = "<p>", string paragraphEndTag = "</p>", string newLineTag = "<br />")
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            text.nEscapeHtmlParagraphs (xBuilder, keepsIndents, tabWidth, fullWidthSpaceWidth, paragraphStartTag, paragraphEndTag, newLineTag);
            return xBuilder.ToString ();
        }

        // Mon, 24 Sep 2018 06:54:39 GMT
        // 無条件で全ての文字を HTML エスケープするメソッド
        // a タグ内でメールアドレスを隠すとか、文字コードが分からないファイルに日本語を吐くとかに使える
        // *AllHtml を最初に考えたが、派生的なメソッドは、英語的に微妙でも接尾辞で粘った方がいい

        public static void nEscapeHtmlAll (this string text, StringBuilder builder)
        {
            if (string.IsNullOrEmpty (text))
                return;

            foreach (char xChar in text)
            {
                builder.Append ("&#x");
                // Mon, 24 Sep 2018 06:56:38 GMT
                // Unicode 決め打ちで、16進数あるいは10進数で吐くのが仕様
                // 10進数の方が x がないが、結局一つ桁が増えるし、文字コードは16進数であるべきと思う
                // char が符号なし16ビットで固定なので、int でなく ushort にしている
                // 今のところニーズが一切ないため、サロゲートペアには対応しない
                // https://docs.microsoft.com/en-us/dotnet/api/system.char
                builder.Append (((ushort) xChar).nToString ("x"));
                builder.Append (';');
            }
        }

        public static string nEscapeHtmlAll (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            text.nEscapeHtmlAll (xBuilder);
            return xBuilder.ToString ();
        }
    }
}

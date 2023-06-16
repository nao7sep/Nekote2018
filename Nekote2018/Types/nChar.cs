using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nChar
    {
        #region 使用頻度の高い配列
        // Fri, 27 Sep 2019 18:17:14 GMT
        // nRandom などで使うための、文字の集合としての配列を揃えておく
        // nBase36 のものなど、どちらかと言えば使用頻度の低いものまでは揃えない
        // ToCharArray で生成しても微々たるコストだろうが、
        // そうするにしてもリテラルは必要なので、最初から配列にしておく

        public static readonly char [] SmallLetters =
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        public static readonly char [] CapitalLetters =
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        public static readonly char [] AllLetters =
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        public static readonly char [] Digits =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public static readonly char [] AllLettersAndDigits =
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public static readonly char [] AsciiSymbols =
        {
            // Fri, 27 Sep 2019 21:40:04 GMT
            // 0x21 から 0x7e までのうち、AllLettersAndDigits に含まれないものを出力
            // その上、念のため二つのページと照合し、完全に一致するのを確認
            // https://www.cs.cmu.edu/~pattis/15-1XX/common/handouts/ascii.html
            // http://www.asciitable.com/
            '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~'
        };
        #endregion

        public static string nToString (this char value)
        {
            // 'a' が "a" となるような変換であり、フォーマットを指定できるものが用意されていない
            // https://msdn.microsoft.com/en-us/library/system.char.tostring.aspx
            return value.ToString (CultureInfo.InvariantCulture);
        }

        // ラウンドトリップが可能なところなので、形式的に用意しておく
        // ラッパーを使うことで型変換の責任を Nekote に丸投げできるようにしておく

        public static ushort nToUShort (this char value)
        {
            // (ushort) を書くとキャストが冗長だという警告が表示される
            return value;
        }

        // Mon, 29 Oct 2018 19:00:55 GMT
        // 何も考えずに nToString でラウンドトリップに期待したら豪快にミスった
        // nDateTime でも long の文字列にできるようにしているため、それに合わせる
        // この変換は絶対に失敗しないため、*OrDefault は不要である

        public static string nToUShortString (this char value)
        {
            return value.nToUShort ().nToString ();
        }

        // 文字列を解析する方では、フォーマットもカルチャーも指定できない

        public static char nToChar (this string text)
        {
            return char.Parse (text);
        }

        public static char nToCharOrDefault (this string text, char value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (char.TryParse (text, out char xResult))
                return xResult;

            return value;
        }

        // Mon, 29 Oct 2018 19:04:36 GMT
        // 他のクラスと同様、ラウンドトリップを可能にしておく

        public static char nUShortToChar (this string text)
        {
            return text.nToUShort ().nToChar ();
        }

        public static char nUShortToCharOrDefault (this string text, char value)
        {
            // Mon, 29 Oct 2018 19:04:54 GMT
            // value を変換するのが少しもったいないが、コストは低い
            return text.nToUShortOrDefault (value.nToUShort ()).nToChar ();
        }

        // char.ToLowerInvariant などが用意されているが、静的メソッドなので表記が長くなるし、
        // Nekote ベースのプログラミングでは invariant の方がデフォルトなので、拡張メソッドを用意しておく

        public static char nToLower (this char value)
        {
            return char.ToLower (value, CultureInfo.InvariantCulture);
        }

        public static char nToUpper (this char value)
        {
            return char.ToUpper (value, CultureInfo.InvariantCulture);
        }

        // .NET には、Unicode テーブルを見て文字と文字の大小関係を調べるメソッドが用意されていない
        // 文字「列」ではそれが可能であり、case-insensitive の比較であっても string.Compare は大小関係を定義するが、
        // 文字では、いったん大文字または小文字にする中途半端な実装で妥協するしかなく、そのときに情報の欠損が起こり、また、ソートの結果が不定となる
        // しかし、実際のコーディングにおいて、ASCII 文字だけを念頭に、入力が Y / y なのか N / n なのかを見たいようなことはよくある
        // また、既に CompareNumerically において、大文字・小文字を区別しない文字の比較が不可欠であり、それができないことにはメソッドが成立しない
        // そのため、理想的な実装ではないながらも役に立ち、実際の使用においてそれほど問題もないであろう妥協的な実装で Compare を用意した

        // 大文字と小文字のいずれに揃えるかについては、ソースコードをあさっていたときに NlsCompareInvariantNoCase のところに
        // WARNING: [\]^_` will be less than A-Z because we make everything lower case before comparing them
        // というコメントがあったため、理由としてやや不十分かもしれないが、暫定的に小文字に揃えている
        // https://github.com/dotnet/coreclr/blob/master/src/utilcode/sortversioning.cpp

        public static int Compare (char char1, char char2, bool ignoresCase = false)
        {
            if (ignoresCase)
                return char1.nToLower ().CompareTo (char2.nToLower ());
            else return char1.CompareTo (char2);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace Nekote
{
    // Sat, 04 May 2019 12:08:11 GMT
    // 不正な文字列を検証するコードにミスがあったらセキュリティーに響くので、
    // そういうコードをこのクラスにまとめ、各所で使い回し、集中的にデバッグする

    public static class nValidator
    {
        // Sat, 04 May 2019 12:19:41 GMT
        // 識別子は、基本的には、下線あるいは英字で始まり、下線と英数字が0個以上続くもの
        // 自分でフォーマットを決められるところでは、このシンプルなルールに従うことで実装を簡略化できる
        // パッと書いてみてからググったらいくらでも見付かったので、今後はまずググってみることにする
        // https://ufcpp.net/study/csharp/misc_identifier.html
        // https://stackoverflow.com/questions/14953861/representing-identifiers-using-regular-expression
        // https://rgxdb.com/r/1FYIQWRM

        public static bool IsIdentifier (string text)
        {
            if (string.IsNullOrEmpty (text))
                return false;

            return Regex.Match (text, "^[_a-z][_a-z0-9]*$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase) != Match.Empty;
        }

        // Sat, 04 May 2019 12:26:22 GMT
        // ユーザー名に下線はなくていいとも思ったが、ユーザーが増えたら必要かもしれない
        // 大文字・小文字の区別がされず、CamelCase にできないため、snake_case を可能にしておく
        // userName では name との違いが曖昧とも感じられ、より分かりやすくするために loginName も考えたが、
        // ログイン以外に使うこともあるし、他者のコードでほぼ確実に userName なので、それにならう

        public static bool IsUserName (string text) =>
            IsIdentifier (text);

        // Sun, 05 May 2019 20:39:47 GMT
        // いったん int や long に戻すなら安全性が高いと言えるが、コストの関係で、そこまでしないこともある
        // その場合においても、不正な文字が含まれていないことくらいは見ておいた方がマシである

        public static bool IsBase36 (string text)
        {
            if (string.IsNullOrEmpty (text))
                return false;

            return Regex.Match (text, "^[a-z0-9]+$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase) != Match.Empty;
        }

        // Sat, 28 Sep 2019 02:46:53 GMT
        // nSafeCode に書いた理由により、1文字目は0以外

        public static bool IsSafeCode (string text)
        {
            if (string.IsNullOrEmpty (text))
                return false;

            return Regex.Match (text, "^[1-9a-z][0-9a-z]{11}$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase) != Match.Empty;
        }
    }
}

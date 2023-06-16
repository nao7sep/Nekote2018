using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nBool
    {
        public static string nToString (this bool value)
        {
            // フォーマットを指定できるものが用意されていない
            // https://msdn.microsoft.com/en-us/library/system.boolean.tostring.aspx
            return value.ToString (CultureInfo.InvariantCulture);
        }

        // 一部の古いデータベースなど、bool 型を扱えないものがあった記憶がある
        // True / False より 1 / 0 の方が転送量をわずかに削減できるという利点もある

        public static int nToInt (this bool value)
        {
            return value ? 1 : 0;
        }

        // 文字列を解析する方では、フォーマットもカルチャーも指定できない

        public static bool nToBool (this string text)
        {
            if (bool.TryParse (text, out bool xResult))
                return xResult;

            return text.nToInt ().nToBool ();
        }

        public static bool nToBoolOrDefault (this string text, bool value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (bool.TryParse (text, out bool xResult))
                return xResult;

            return text.nToIntOrDefault (value.nToInt ()).nToBool ();
        }
    }
}

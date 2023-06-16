using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nUShort
    {
        public static string nToString (this ushort value)
        {
            return value.ToString (CultureInfo.InvariantCulture);
        }

        public static string nToString (this ushort value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        // ラウンドトリップが可能なところなので、形式的に用意しておく
        // ラッパーを使うことで型変換の責任を Nekote に丸投げできるようにしておく

        public static char nToChar (this ushort value)
        {
            return (char) value;
        }

        public static ushort nToUShort (this string text)
        {
            return ushort.Parse (text, CultureInfo.InvariantCulture);
        }

        public static ushort nToUShort (this string text, NumberStyles style)
        {
            return ushort.Parse (text, style, CultureInfo.InvariantCulture);
        }

        public static ushort nToUShortOrDefault (this string text, ushort value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // https://msdn.microsoft.com/en-us/library/wxy94716.aspx
            if (ushort.TryParse (text, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort xResult))
                return xResult;

            return value;
        }

        public static ushort nToUShortOrDefault (this string text, NumberStyles style, ushort value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (ushort.TryParse (text, style, CultureInfo.InvariantCulture, out ushort xResult))
                return xResult;

            return value;
        }
    }
}

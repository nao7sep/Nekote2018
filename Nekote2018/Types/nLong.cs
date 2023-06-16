using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nLong
    {
        public static string nToString (this long value)
        {
            return value.ToString (CultureInfo.InvariantCulture);
        }

        public static string nToString (this long value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        // ラウンドトリップが可能なところなので、形式的に用意しておく
        // ラッパーを使うことで型変換の責任を Nekote に丸投げできるようにしておく

        public static DateTime nToDateTime (this long value)
        {
            return new DateTime (value);
        }

        public static TimeSpan nToTimeSpan (this long value)
        {
            return new TimeSpan (value);
        }

        public static long nToLong (this string text)
        {
            return long.Parse (text, CultureInfo.InvariantCulture);
        }

        public static long nToLong (this string text, NumberStyles style)
        {
            return long.Parse (text, style, CultureInfo.InvariantCulture);
        }

        public static long nToLongOrDefault (this string text, long value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // https://msdn.microsoft.com/en-us/library/zc2x2b1h.aspx
            if (long.TryParse (text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long xResult))
                return xResult;

            return value;
        }

        public static long nToLongOrDefault (this string text, NumberStyles style, long value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (long.TryParse (text, style, CultureInfo.InvariantCulture, out long xResult))
                return xResult;

            return value;
        }
    }
}

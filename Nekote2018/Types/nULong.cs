using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nULong
    {
        public static string nToString (this ulong value)
        {
            return value.ToString (CultureInfo.InvariantCulture);
        }

        public static string nToString (this ulong value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        public static ulong nToULong (this string text)
        {
            return ulong.Parse (text, CultureInfo.InvariantCulture);
        }

        public static ulong nToULong (this string text, NumberStyles style)
        {
            return ulong.Parse (text, style, CultureInfo.InvariantCulture);
        }

        public static ulong nToULongOrDefault (this string text, ulong value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // https://msdn.microsoft.com/en-us/library/6sty5xhc.aspx
            if (ulong.TryParse (text, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong xResult))
                return xResult;

            return value;
        }

        public static ulong nToULongOrDefault (this string text, NumberStyles style, ulong value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (ulong.TryParse (text, style, CultureInfo.InvariantCulture, out ulong xResult))
                return xResult;

            return value;
        }
    }
}

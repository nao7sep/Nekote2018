using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nShort
    {
        public static string nToString (this short value)
        {
            return value.ToString (CultureInfo.InvariantCulture);
        }

        public static string nToString (this short value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        public static short nToShort (this string text)
        {
            return short.Parse (text, CultureInfo.InvariantCulture);
        }

        public static short nToShort (this string text, NumberStyles style)
        {
            return short.Parse (text, style, CultureInfo.InvariantCulture);
        }

        public static short nToShortOrDefault (this string text, short value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // https://msdn.microsoft.com/en-us/library/9hh1awhy.aspx
            if (short.TryParse (text, NumberStyles.Integer, CultureInfo.InvariantCulture, out short xResult))
                return xResult;

            return value;
        }

        public static short nToShortOrDefault (this string text, NumberStyles style, short value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (short.TryParse (text, style, CultureInfo.InvariantCulture, out short xResult))
                return xResult;

            return value;
        }
    }
}

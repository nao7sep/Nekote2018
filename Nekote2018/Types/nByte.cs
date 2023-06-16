using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nByte
    {
        public static string nToString (this byte value)
        {
            return value.ToString (CultureInfo.InvariantCulture);
        }

        public static string nToString (this byte value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        public static byte nToByte (this string text)
        {
            return byte.Parse (text, CultureInfo.InvariantCulture);
        }

        public static byte nToByte (this string text, NumberStyles style)
        {
            return byte.Parse (text, style, CultureInfo.InvariantCulture);
        }

        public static byte nToByteOrDefault (this string text, byte value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // https://msdn.microsoft.com/en-us/library/f78h0es7.aspx
            if (byte.TryParse (text, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte xResult))
                return xResult;

            return value;
        }

        public static byte nToByteOrDefault (this string text, NumberStyles style, byte value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (byte.TryParse (text, style, CultureInfo.InvariantCulture, out byte xResult))
                return xResult;

            return value;
        }
    }
}

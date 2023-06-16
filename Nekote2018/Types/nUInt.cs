using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nUInt
    {
        public static string nToString (this uint value)
        {
            return value.ToString (CultureInfo.InvariantCulture);
        }

        public static string nToString (this uint value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        public static uint nToUInt (this string text)
        {
            return uint.Parse (text, CultureInfo.InvariantCulture);
        }

        public static uint nToUInt (this string text, NumberStyles style)
        {
            return uint.Parse (text, style, CultureInfo.InvariantCulture);
        }

        public static uint nToUIntOrDefault (this string text, uint value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // https://msdn.microsoft.com/en-us/library/thh25zy8.aspx
            if (uint.TryParse (text, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint xResult))
                return xResult;

            return value;
        }

        public static uint nToUIntOrDefault (this string text, NumberStyles style, uint value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (uint.TryParse (text, style, CultureInfo.InvariantCulture, out uint xResult))
                return xResult;

            return value;
        }
    }
}

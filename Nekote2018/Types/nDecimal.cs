using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nDecimal
    {
        public static string nToString (this decimal value)
        {
            return value.ToString (CultureInfo.InvariantCulture);
        }

        public static string nToString (this decimal value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        public static decimal nToDecimal (this string text)
        {
            return decimal.Parse (text, CultureInfo.InvariantCulture);
        }

        public static decimal nToDecimal (this string text, NumberStyles style)
        {
            return decimal.Parse (text, style, CultureInfo.InvariantCulture);
        }

        public static decimal nToDecimalOrDefault (this string text, decimal value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // https://msdn.microsoft.com/en-us/library/9zbda557.aspx
            if (decimal.TryParse (text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal xResult))
                return xResult;

            return value;
        }

        public static decimal nToDecimalOrDefault (this string text, NumberStyles style, decimal value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (decimal.TryParse (text, style, CultureInfo.InvariantCulture, out decimal xResult))
                return xResult;

            return value;
        }
    }
}

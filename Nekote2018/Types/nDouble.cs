using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nDouble
    {
        public static string nToString (this double value)
        {
            return value.ToString (CultureInfo.InvariantCulture);
        }

        public static string nToString (this double value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        public static double nToDouble (this string text)
        {
            return double.Parse (text, CultureInfo.InvariantCulture);
        }

        public static double nToDouble (this string text, NumberStyles style)
        {
            return double.Parse (text, style, CultureInfo.InvariantCulture);
        }

        public static double nToDoubleOrDefault (this string text, double value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // https://msdn.microsoft.com/en-us/library/994c0zb1.aspx
            if (double.TryParse (text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double xResult))
                return xResult;

            return value;
        }

        public static double nToDoubleOrDefault (this string text, NumberStyles style, double value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (double.TryParse (text, style, CultureInfo.InvariantCulture, out double xResult))
                return xResult;

            return value;
        }
    }
}

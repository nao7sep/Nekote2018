using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nSByte
    {
        public static string nToString (this sbyte value)
        {
            return value.ToString (CultureInfo.InvariantCulture);
        }

        public static string nToString (this sbyte value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        public static sbyte nToSByte (this string text)
        {
            return sbyte.Parse (text, CultureInfo.InvariantCulture);
        }

        public static sbyte nToSByte (this string text, NumberStyles style)
        {
            return sbyte.Parse (text, style, CultureInfo.InvariantCulture);
        }

        public static sbyte nToSByteOrDefault (this string text, sbyte value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // https://msdn.microsoft.com/en-us/library/h9y629c8.aspx
            if (sbyte.TryParse (text, NumberStyles.Integer, CultureInfo.InvariantCulture, out sbyte xResult))
                return xResult;

            return value;
        }

        public static sbyte nToSByteOrDefault (this string text, NumberStyles style, sbyte value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (sbyte.TryParse (text, style, CultureInfo.InvariantCulture, out sbyte xResult))
                return xResult;

            return value;
        }
    }
}

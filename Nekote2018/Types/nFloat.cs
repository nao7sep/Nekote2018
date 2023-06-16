using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nFloat
    {
        public static string nToString (this float value)
        {
            return value.ToString (CultureInfo.InvariantCulture);
        }

        public static string nToString (this float value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        public static float nToFloat (this string text)
        {
            return float.Parse (text, CultureInfo.InvariantCulture);
        }

        public static float nToFloat (this string text, NumberStyles style)
        {
            return float.Parse (text, style, CultureInfo.InvariantCulture);
        }

        public static float nToFloatOrDefault (this string text, float value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // https://msdn.microsoft.com/en-us/library/26sxas5t.aspx
            if (float.TryParse (text, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float xResult))
                return xResult;

            return value;
        }

        public static float nToFloatOrDefault (this string text, NumberStyles style, float value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (float.TryParse (text, style, CultureInfo.InvariantCulture, out float xResult))
                return xResult;

            return value;
        }
    }
}

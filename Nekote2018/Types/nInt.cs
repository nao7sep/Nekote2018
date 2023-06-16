using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nInt
    {
        public static string nToString (this int value)
        {
            return value.ToString (CultureInfo.InvariantCulture);
        }

        public static string nToString (this int value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        // 用意した理由については nBool.cs を参照のこと

        public static bool nToBool (this int value)
        {
            return value != 0;
        }

        // nEnum.cs に書いた理由により、以下、IsDefined を多用する

        public static object nToEnum (this int value, Type type)
        {
            if (Enum.IsDefined (type, value) == false)
                throw new nBadOperationException ();

            // Enum.ToObject Method (Type, Int32)
            // https://msdn.microsoft.com/en-us/library/ksbe1e7h.aspx
            return Enum.ToObject (type, value);
        }

        public static object nToEnumOrDefault (this int value, Type type, object defaultValue)
        {
            if (Enum.IsDefined (type, value))
                return Enum.ToObject (type, value);

            return defaultValue;
        }

        // ジェネリックを使うものも用意しておく
        // 戻り値のキャストが不要なので通常はこちらを使う

        public static T nToEnum <T> (this int value)
        {
            if (Enum.IsDefined (typeof (T), value) == false)
                throw new nBadOperationException ();

            return (T) Enum.ToObject (typeof (T), value);
        }

        public static T nToEnumOrDefault <T> (this int value, T defaultValue)
        {
            if (Enum.IsDefined (typeof (T), value))
                return (T) Enum.ToObject (typeof (T), value);

            return defaultValue;
        }

        // bool や enum が便宜的に int に変換されることがあるが、
        // それらの文字列表現から int を取得するほどの構文解析は行わない

        public static int nToInt (this string text)
        {
            return int.Parse (text, CultureInfo.InvariantCulture);
        }

        public static int nToInt (this string text, NumberStyles style)
        {
            return int.Parse (text, style, CultureInfo.InvariantCulture);
        }

        public static int nToIntOrDefault (this string text, int value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // https://msdn.microsoft.com/en-us/library/f02979c7.aspx
            if (int.TryParse (text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int xResult))
                return xResult;

            return value;
        }

        public static int nToIntOrDefault (this string text, NumberStyles style, int value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (int.TryParse (text, style, CultureInfo.InvariantCulture, out int xResult))
                return xResult;

            return value;
        }
    }
}

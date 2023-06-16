using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nSqlString
    {
        // Replace でも足りるが、フォーマットのエラーを検出したいし、
        // StringBuilder への流し込みは、こちらでも行いたい

        public static void nUnescapeSql (this string text, StringBuilder builder)
        {
            if (string.IsNullOrEmpty (text))
                return;

            for (int temp = 0; temp < text.Length; temp ++)
            {
                char xCurrent = text [temp];

                if (xCurrent == '\'')
                {
                    temp ++;

                    if (temp < text.Length)
                    {
                        char xNext = text [temp];

                        if (xNext == '\'')
                            builder.Append ('\'');
                        else throw new nInvalidFormatException ();
                    }

                    else throw new nInvalidFormatException ();
                }

                else builder.Append (xCurrent);
            }
        }

        public static string nUnescapeSql (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            text.nUnescapeSql (xBuilder);
            return xBuilder.ToString ();
        }

        public static void nEscapeSql (this string text, StringBuilder builder)
        {
            if (string.IsNullOrEmpty (text))
                return;

            foreach (char xChar in text)
            {
                if (xChar == '\'')
                    builder.Append ("''");
                else builder.Append (xChar);
            }
        }

        public static string nEscapeSql (this string text)
        {
            if (string.IsNullOrEmpty (text))
                return text;

            StringBuilder xBuilder = new StringBuilder ();
            text.nEscapeSql (xBuilder);
            return xBuilder.ToString ();
        }
    }
}

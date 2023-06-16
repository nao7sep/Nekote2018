using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nTimeSpan
    {
        // "c" の Name 列は Constant (invariant) format とされている
        // nDateTime のメソッドとの一貫性も考えて、ここでは invariant を名前に含めた
        // https://msdn.microsoft.com/en-us/library/ee372286.aspx
        public static string nToInvariantString (this TimeSpan value)
        {
            // カルチャーのみを指定できるものがなく、フォーマットとして null または "" を指定すれば、
            // 内部的には "c" として解釈され、[-][d.]hh:mm:ss[.fffffff] が使われるとのことである
            // 日時と異なり、時間（差）には一つでどこでも大丈夫というようなフォーマットがないためデフォルトに任せる
            // https://msdn.microsoft.com/en-us/library/dd784379.aspx
            // https://msdn.microsoft.com/en-us/library/ee372286.aspx#Constant

            // 追記: invariant は、DateTime と TimeSpan の両方においてデフォルト的に使われるものであり、
            // 詳しくは DateTime の方のコメントに書くが、精度や可読性に不整合があるのがどうしても気になる
            // しかし、ORM には、効率を考えても Ticks を使うことになるため、invariant は今の仕様のままでよいだろう

            return value.ToString ("c", CultureInfo.InvariantCulture);
        }

        // nDateTime 同様、ラウンドトリップには invariant より long が適する
        // こちらの invariant は精度があるが、Ticks を使う方が仕様としての整合性が高い

        public static string nToLongString (this TimeSpan value)
        {
            return value.nToLong ().nToString ();
        }

        public static string nToString (this TimeSpan value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        // ラウンドトリップが可能なところなので、形式的に用意しておく
        // ラッパーを使うことで型変換の責任を Nekote に丸投げできるようにしておく

        public static long nToLong (this TimeSpan value)
        {
            return value.Ticks;
        }

        public static TimeSpan nInvariantToTimeSpan (this string text)
        {
            return TimeSpan.ParseExact (text, "c", CultureInfo.InvariantCulture);
        }

        public static TimeSpan nLongToTimeSpan (this string text)
        {
            // Ticks を使ってもよいが、一応コードを一貫させておく
            return text.nToLong ().nToTimeSpan ();
        }

        public static TimeSpan nToTimeSpan (this string text, string format)
        {
            return TimeSpan.ParseExact (text, format, CultureInfo.InvariantCulture);
        }

        public static TimeSpan nToTimeSpan (this string text, string format, TimeSpanStyles style)
        {
            return TimeSpan.ParseExact (text, format, CultureInfo.InvariantCulture, style);
        }

        public static TimeSpan nInvariantToTimeSpanOrDefault (this string text, TimeSpan value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // TimeSpanStyles を受け取らないオーバーライドでは内部的に None とされている
            // DateTime.TryParseExact の方では *Styles の指定が不可欠のようなので、こちらでも指定しておく
            // https://referencesource.microsoft.com/#mscorlib/system/timespan.cs
            if (TimeSpan.TryParseExact (text, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None, out TimeSpan xResult))
                return xResult;

            return value;
        }

        public static TimeSpan nLongToTimeSpanOrDefault (this string text, TimeSpan value)
        {
            // nToLongOrDefault がすぐに IsNullOrEmpty を呼ぶため、こちらでは省略
            return text.nToLongOrDefault (value.Ticks).nToTimeSpan ();
        }

        public static TimeSpan nToTimeSpanOrDefault (this string text, string format, TimeSpan value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (TimeSpan.TryParseExact (text, format, CultureInfo.InvariantCulture, TimeSpanStyles.None, out TimeSpan xResult))
                return xResult;

            return value;
        }

        public static TimeSpan nToTimeSpanOrDefault (this string text, string format, TimeSpanStyles style, TimeSpan value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (TimeSpan.TryParseExact (text, format, CultureInfo.InvariantCulture, style, out TimeSpan xResult))
                return xResult;

            return value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nGuid
    {
        // Wed, 07 Nov 2018 20:44:07 GMT
        // GUID であるかどうかの簡易的なチェックなどに使える長さ
        // containing や including も考えたが、nToStringWithoutHyphens と合わせておく

        public static readonly int LengthWithHyphens = 32 + 4;

        public static readonly int LengthWithoutHyphens = 32;

        // Wed, 07 Nov 2018 20:45:17 GMT
        // カルチャーだけを指定できるものがなく、英数字とハイフンだけなら null または "" を指定
        // https://msdn.microsoft.com/en-us/library/s6tk2z69.aspx

        public static string nToString (this Guid value) =>
            value.ToString (null, CultureInfo.InvariantCulture);

        public static string nToString (this Guid value, string format) =>
            value.ToString (format, CultureInfo.InvariantCulture);

        // Wed, 07 Nov 2018 20:46:04 GMT
        // ファイルを移動するときの一時的なパスなどにはハイフンがなくてもよいと思ったが、
        // ハイフンがあってこそ誰が見ても GUID であり、英数字だけ並んでいる文字列との混同が減る
        // 各位置のハイフンの存在をチェックすることもあり得るし、何が何でもケチらなければならない4バイトではない
        // それをするなら minimal な日時の区切り文字である T を削ることも考えることになる

        public static string nToStringWithoutHyphens (this Guid value) =>
            value.ToString ("n", CultureInfo.InvariantCulture);

        // Wed, 07 Nov 2018 20:48:38 GMT
        // フォーマットを指定しない方ではハイフンの有無に関わらず解析できるのを確認してある

        public static Guid nToGuid (this string text) =>
            Guid.Parse (text);

        public static Guid nToGuid (this string text, string format) =>
            Guid.ParseExact (text, format);

        public static Guid nToGuidOrDefault (this string text, Guid value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (Guid.TryParse (text, out Guid xResult))
                return xResult;

            return value;
        }

        public static Guid nToGuidOrDefault (this string text, string format, Guid value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (Guid.TryParseExact (text, format, out Guid xResult))
                return xResult;

            return value;
        }

        // Wed, 07 Nov 2018 20:49:18 GMT
        // プロパティーは、一度取得したら値がキャッシュされて何度でも低コストで取得できるようなところに使いたい
        // このメソッドは呼び出しのたびに計算が行われるし、毎回結果も異なると期待されるものなので、メソッドであるべき
        // 128ビットが全て0になることは可能性として低いが、default で得られるものが得られる可能性をゼロにしておく

        public static Guid New ()
        {
            while (true)
            {
                Guid xValue = Guid.NewGuid ();

                if (xValue != Guid.Empty)
                    return xValue;
            }
        }
    }
}

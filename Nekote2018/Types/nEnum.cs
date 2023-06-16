using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nEnum
    {
        // enum の扱いにおいては、不正な値が素通りしないように細心の注意を払う必要がある
        // 処理のモードやユーザーの権限といったものが不正な値では、セキュリティーに深刻な影響が及ぶ
        // そのため、Nekote では、enum の扱いにおいて冗長なまでに IsDefined を呼び、
        // 不正アクセスがあれば、データの漏洩を許すよりはプログラムを落とす

        // 追記: このクラスでは、nToEnum* が、文字列として照合してダメだったときに int の文字列としての照合も行う
        // たとえば、"Guest" などがダメだったなら、Guest = 1 などとされていての "1" などの方から Guest などを復元しようとする
        // enum の要素名はほぼ全く変わらないと言えるが、値はそうとも限らないため、値の方に頼るプログラミングには危険性がある
        // ただ、現実的にデータベースなどに入れるのは値の方であり、"Guest" などを入れて where 句で絞り込むようなことはあり得ない
        // また、設定内容、クエリー文字列、ユーザー入力などで iUserType などのセキュリティー的に重大なものの値を受け取る実装もあり得ない
        // 何となくそう思うほどのリスクはないなら、型変換の柔軟性は Nekote のコンセプトの一部であり、仕様の整合性だと評価してよい

        public static string nToString (this Enum value)
        {
            // https://msdn.microsoft.com/en-us/library/system.enum.isdefined.aspx
            if (Enum.IsDefined (value.GetType (), value) == false)
                throw new nBadOperationException ();

            // カルチャーを指定できるものは obsolete とのこと
            // https://msdn.microsoft.com/en-us/library/system.enum_methods.aspx
            return value.ToString ();
        }

        public static string nToString (this Enum value, string format)
        {
            if (Enum.IsDefined (value.GetType (), value) == false)
                throw new nBadOperationException ();

            // Enumeration Format Strings
            // https://msdn.microsoft.com/en-us/library/c3s1ez6e.aspx
            return value.ToString (format);
        }

        // 上記のものが落ちるメソッドなので、デフォルト値を返すものも用意
        // ユーザーの権限が不正ならとりあえずゲストに落とすような処理に使える

        public static string nToStringOrDefault (this Enum value, string text)
        {
            if (Enum.IsDefined (value.GetType (), value))
                return value.ToString ();

            return text;
        }

        public static string nToStringOrDefault (this Enum value, string format, string text)
        {
            if (Enum.IsDefined (value.GetType (), value))
                return value.ToString (format);

            return text;
        }

        public static int nToInt (this Enum value)
        {
            if (Enum.IsDefined (value.GetType (), value) == false)
                throw new nBadOperationException ();

            // こちらにはカルチャーを指定できそうだが、値を取り出すだけなのでそもそも不要
            // https://msdn.microsoft.com/en-us/library/ms130995.aspx
            return Convert.ToInt32 (value);
        }

        public static int nToIntOrDefault (this Enum value, int defaultValue)
        {
            if (Enum.IsDefined (value.GetType (), value))
                return Convert.ToInt32 (value);

            return defaultValue;
        }

        // Mon, 29 Oct 2018 17:42:47 GMT
        // あると思って呼ぼうとしたらなくて困ったため、とりあえず作ってみた
        // nDateTime.nToLongString 同様、2段階の変換にしている

        public static string nToIntString (this Enum value) =>
            value.nToInt ().nToString ();

        // Mon, 29 Oct 2018 17:44:05 GMT
        // nDateTime.nToLongString は絶対に成功するが、こちらはそうとは限らない
        // nToIntOrDefault を使っての2段階の変換では defaultValue をいったん int に戻すことになる
        // やはり無駄だし、かといって defaultValue を int にするのも不揃いなので、ベタ書きで実装

        public static string nToIntStringOrDefault (this Enum value, string defaultValue)
        {
            if (Enum.IsDefined (value.GetType (), value))
                return Convert.ToInt32 (value).nToString ();

            return defaultValue;
        }

        // Mon, 29 Oct 2018 17:49:46 GMT
        // nDateTime には、nLongToDateTime など、
        // 文字列を long としてパーズしてから DateTime に戻すものがあるが、
        // このクラスでは、int として読んでみるのは、以下のメソッドが自動的に試す

        public static object nToEnum (this string text, Type type)
        {
            if (Enum.IsDefined (type, text))
                // IsDefined には大文字・小文字を区別しない指定ができないようなので
                // それが可能な Parse でも三つ目の引数によって区別させている
                // https://msdn.microsoft.com/en-us/library/kxydatf9.aspx
                return Enum.Parse (type, text, false);

            return text.nToInt ().nToEnum (type);
        }

        public static object nToEnumOrDefault (this string text, Type type, object value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (Enum.IsDefined (type, text))
                return Enum.Parse (type, text, false);

            // value には *.* の表記によって有効な値が指定されると見込んでいる
            // ここでさらに IsDefined を呼ぶのは、パフォーマンスを考えない過剰チェック
            return text.nToIntOrDefault (Convert.ToInt32 (value)).nToEnum (type);
        }

        // ジェネリックを使うものも用意しておく
        // 戻り値のキャストが不要なので通常はこちらを使う

        public static T nToEnum <T> (this string text)
        {
            if (Enum.IsDefined (typeof (T), text))
                return (T) Enum.Parse (typeof (T), text, false);

            return text.nToInt ().nToEnum <T> ();
        }

        public static T nToEnumOrDefault <T> (this string text, T value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (Enum.IsDefined (typeof (T), text))
                return (T) Enum.Parse (typeof (T), text, false);

            return text.nToIntOrDefault (Convert.ToInt32 (value)).nToEnum <T> ();
        }
    }
}

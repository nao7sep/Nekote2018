using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Globalization;

namespace Nekote
{
    public static class nDateTime
    {
        // 今、DateTime.UtcNow.ToString (CultureInfo.InvariantCulture) の結果は、03/18/2017 09:45:58 となった
        // 一方、ISO 8601 でも .NET の SortableDateTimePattern や UniversalSortableDateTimePattern でも、
        // 日付は年月日の順に並び、ハイフンで区切られ、時刻はダブルコロンで区切られ、違いは、T や Z が入ったり入らなかったり程度である
        // そのため、それらをベースとし、多くの一般ユーザーが違和感を覚えるであろう T と Z を省いたものを、可読性があるべきところでは常用する
        // https://en.wikipedia.org/wiki/ISO_8601
        // https://msdn.microsoft.com/en-us/library/az4se3k1.aspx

        // 追記: 旧版の Nekote では GenerateInvariantStringFormat が dddd, d MMMM yyyy H':'mm':'ss 'GMT' などを返していた
        // 今回の実装における InvariantDateTimeFormat に相当するフォーマットは、GenerateSortableStringFormat が返していた
        // InvariantDateTimeFormat は確かに sortable であり、上記のように .NET でもそういう表現が用いられているため、悪い命名でない
        // ただ、nTimeSpan.nToInvariantString が使う "c" が [-][d.]hh:mm:ss[.fffffff] であることを考えると、
        // 人間が見るものとして自然でないことを共通点とするなら、nDateTime 側の invariant を以下のようにすることにこそ仕様としての整合性がある
        // invariant は、nDateTime と nTimeSpan の両方において、内部的で、情報が多く、汎用性が高く、それでいて最低限の可読性もあるべき
        // nTimeSpan と比べて nDateTime には可読性が要求されることが多いため、そのことについては、nDateTimeFormat クラスを別に作って対応する

        // さらに追記: DateTime と TimeSpan とで invariant の仕様に整合性が乏しいように感じたが、これは、このままでよいと思う
        // DateTime の方は、人間が見て自然で、精度が「秒」までだが、TimeSpan の方は、よりマシーン的（？）で、精度がより高い
        // この違いは、.NET の ToString をそのまま呼んだときのデフォルトのフォーマットの違いによるものであり、Nekote はそれを引き継いだだけである
        // invariant というと、汎用的で、ラウンドトリップにも強そうなイメージなので、DateTime でも精度を高めたくなるが、そうすると汎用性が一気に低下する
        // そもそも、日時と時間「差」の違いは、点を示すのか量を示すかの違いであり、共通点は多くとも種類が異なる二つの仕様を整合させる必要性は乏しい
        // ORM 的なことをするなら、いずれにせよ、Ticks を使うのが、文字列が短く、変換が効率的で、精度も最も高い

        public static readonly string InvariantDateTimeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";

        // 大勢が違和感なく受け入れる形で日時を出力したいときに迷いなく使えるメソッド
        // ラウンドトリップが確実に行えるという利点もある

        // 元々は nToString という名前にしていたが、特定の仕様に基づくメソッドに一般名をつけるのはやめる
        public static string nToInvariantString (this DateTime value)
        {
            return value.ToString (InvariantDateTimeFormat, CultureInfo.InvariantCulture);
        }

        // ラウンドトリップが必要なところでは、invariant より long を使うのが適する
        // 元々は invariant を使っていたが、それでは ORM 感覚で使うにおいて精度の不足があった

        public static string nToLongString (this DateTime value)
        {
            return value.nToLong ().nToString ();
        }

        public static string nToString (this DateTime value, string format)
        {
            return value.ToString (format, CultureInfo.InvariantCulture);
        }

        // ラウンドトリップが可能なところなので、形式的に用意しておく
        // ラッパーを使うことで型変換の責任を Nekote に丸投げできるようにしておく

        public static long nToLong (this DateTime value)
        {
            return value.Ticks;
        }

        public static DateTime nInvariantToDateTime (this string text)
        {
            return DateTime.ParseExact (text, InvariantDateTimeFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime nLongToDateTime (this string text)
        {
            return text.nToLong ().nToDateTime ();
        }

        public static DateTime nToDateTime (this string text, string format)
        {
            return DateTime.ParseExact (text, format, CultureInfo.InvariantCulture);
        }

        public static DateTime nToDateTime (this string text, string format, DateTimeStyles style)
        {
            return DateTime.ParseExact (text, format, CultureInfo.InvariantCulture, style);
        }

        public static DateTime nInvariantToDateTimeOrDefault (this string text, DateTime value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            // 複数のフォーマットを受け取る方には A typical value to specify is DateTimeStyles.None とある
            // DateTimeStyles を指定しないオーバーライドが用意されていないようなので、とりあえず None を指定する
            // https://msdn.microsoft.com/en-us/library/h9b85w22.aspx
            if (DateTime.TryParseExact (text, InvariantDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime xResult))
                return xResult;

            return value;
        }

        public static DateTime nLongToDateTimeOrDefault (this string text, DateTime value)
        {
            // IsNullOrEmpty は、nToLongOrDefault で呼ばれるので省略
            // いったん value.Ticks に展開して渡すのがやや無駄だが、コストは小さい
            return text.nToLongOrDefault (value.Ticks).nToDateTime ();
        }

        public static DateTime nToDateTimeOrDefault (this string text, string format, DateTime value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (DateTime.TryParseExact (text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime xResult))
                return xResult;

            return value;
        }

        public static DateTime nToDateTimeOrDefault (this string text, string format, DateTimeStyles style, DateTime value)
        {
            if (string.IsNullOrEmpty (text))
                return value;

            if (DateTime.TryParseExact (text, format, CultureInfo.InvariantCulture, style, out DateTime xResult))
                return xResult;

            return value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // nDateTime と nTimeSpan は、いずれも最小限のフォーマットのみ認識するべきで、それ以上ゴテゴテさせたくない
    // 日時は、nTimeSpan の扱う時間「差」と比べてフォーマットに多様性があり、そこにロケールも関連してはエラいことになる
    // そのため、nTo*String などを nDateTime に追加していくのでなく、フォーマットを扱う専門的なクラスを今のうちから用意する
    // これを Types ディレクトリーに入れるべきかどうか微妙だが、DateTime 型に関連するものなので今のところ妥協している
    // 今後、タイムゾーンやロケールを扱うクラスの追加時にそれらとまとめたくなるだろうが、それはそのときに考える

    public static class nDateTimeFormat
    {
        // フォーマットのプロパティー名においては、他と区別しやすいシンプルな単語を先頭に置き、
        // 日時のうちどの部分を含むかを続け、最後に必要に応じて Universal を置く
        // Universal を先頭に置かないのは、IntelliSense の使用において順序を分かりやすくしたいため
        // universal でない RFC1123 や universal な full など、一般的でないものは用意しない
        // そういったものは、数を揃えたくなることがあるが、少し足りないくらいの方が選択に困らない

        // 曜日の名前が入った universal といえば RFC1123 であり、可読性も高いため、微調整もすることなく枯れた仕様にのるのが良い
        // https://docs.microsoft.com/en-us/dotnet/api/system.globalization.datetimeformatinfo.rfc1123pattern
        public static readonly string Rfc1123DateTimeUniversal = "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'";

        // full は、RFC1123 をベースとし、en-US の "F" である dddd, MMMM dd, yyyy h:mm:ss tt をいくらか取り込んだもの
        // dddd 直後の , はなくても困らないが、曜日は日時に対して補足的に付与される情報であり、RFC1123 でも , が入るので、ここでも区切っておく
        // 多くのロケールにおいて「日」は「月」より先であり、「月」が先だと「日」と「年」の数字が並んで , が必要となるため、ここでも「日」が先
        // ゼロ詰めは、たとえばパッと6時を手書きするときに「06時」と書く人は稀なはずで、「日」と「時」には不要と思うため省いた
        // Standard Date and Time Format Strings のページで見る限り、時刻の区切り文字は : で決め打ちにして問題ないだろう
        // https://docs.microsoft.com/en-us/dotnet/api/system.globalization.datetimeformatinfo.fulldatetimepattern
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings

        public static readonly string FullDateTime = "dddd, d MMMM yyyy H':'mm':'ss";

        public static readonly string FullDate = "dddd, d MMMM yyyy";

        // FriendlyTime と同じだが、片方しかないのではコーディング時に困惑する
        public static readonly string FullTime = "H':'mm':'ss";

        // human readable で検索していたら、「読みやすい」のニュアンスで friendly をよく見たので採用
        // friendly は、full ほど情報がなく、むしろ invariant に近く、記号が「ふつう」で、不要なゼロ詰めもない
        // 日付の区切り文字も、Standard Date and Time Format Strings のページを見る限り、/ で特に問題はないはず
        // 使い分けとしては、RFC1123 と full はいずれも曜日が分かるので、曜日が必要なところで universal かどうかで使い分け、
        // invariant と friendly については、ユーザーに見えるところかどうかで使い分けるのが選択肢の一つである

        public static readonly string FriendlyDateTime = "yyyy'/'M'/'d H':'mm':'ss";

        public static readonly string FriendlyDate = "yyyy'/'M'/'d";

        public static readonly string FriendlyTime = "H':'mm':'ss";

        // minimal は、ファイル名やキーなど、（ほぼ）最小の長さと使用文字で日時などを表現する必要のあるところに適する
        // 似たような単語に minimum があるが、これは数値的・int 的に「小さい」というニュアンスのようである
        // 日付部分と時刻部分のいずれにも区切り文字がないし、ソート可能であるべきなので、ゼロ詰めが行われる
        // The Round-trip ("O", "o") Format Specifier のところに従い、日付と時刻を T で区切り、universal を意味する Z をつける
        // これは ISO 8601 の仕様の一部であり、当該 Wikipedia ページを開いたところ 20171126T223840Z が例の一つとして表示された
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#Roundtrip
        // https://www.w3.org/TR/NOTE-datetime
        // https://en.wikipedia.org/wiki/ISO_8601

        public static readonly string MinimalDateTimeUniversal = "yyyyMMdd'T'HHmmss'Z'";

        // Z を外すなら T を - にするのが、ファイル名などに実際に使われているところをイメージするにおいて自然
        // この例外を認めないなら Iso8601 を識別子に含めることも選択肢だが、それでは使えないプロパティーになるだろう
        public static readonly string MinimalDateTime = "yyyyMMdd'-'HHmmss";

        // 時刻部分がないと時差の情報としての有用性が乏しくなるため日付のみに Z をつけることには抵抗を覚えたが、
        // taskKiller が 201711Z などのディレクトリーを作って処理済みタスクのファイルを整理するなど、
        // universal な日時で処理されるものの分類・分割（ログファイルなど）には Z があった方が良い
        public static readonly string MinimalDateUniversal = "yyyyMMdd'Z'";

        // minimal な date か time かについては、桁数のみで区別を行うのが現実的だろう
        // Nekote のユーザーは、私も含めて2099年までに死ぬだろうからチェックを 20[0-9]{6} と少し厳密化するのもアリだが、
        // そこにあるのが日付または時刻であることが確実なら、桁数だけで見た方が2100年問題を無意味に埋め込まずに済む

        public static readonly string MinimalDate = "yyyyMMdd";

        public static readonly string MinimalTime = "HHmmss";
    }
}

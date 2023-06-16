using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Mon, 29 Oct 2018 19:13:13 GMT
    // SQL の insert や update を手書きするのがしんどいため、
    // 必要な部分のみ半自動でコーディングするクラスを作ってみた
    // データベースごとにうまく自分なりの落としどころを見つけてうまく変換するため、
    // これは SQLite 専用のものとして個別に作り、その他のデータベースもその都度そうする

    // Tue, 30 Oct 2018 04:36:38 GMT
    // 最初、string, string だったが、後者を Tuple に変更した
    // テスト時に適当に入れた値のラウンドトリップをあとで確認したかったため
    // object に入れるが、 Nullable も Enum も問題がないのを確認している
    // あくまで元の値を取り出すためのものなので、Ticks にして……のようなことを一切しない
    // ulong を long にして入れるところがあるが、ulong に戻して照合する

    public class nSqliteCommandPartsBuilder
    {
        public Dictionary <string, Tuple <object, string>> Dictionary { get; private set; }

        public nSqliteCommandPartsBuilder ()
        {
            // Mon, 29 Oct 2018 19:14:06 GMT
            // SQL 側で大文字・小文字が区別されないはずなので、キーも同様にしている
            Dictionary = new Dictionary <string, Tuple <object, string>> (StringComparer.InvariantCultureIgnoreCase);
        }

        // Mon, 29 Oct 2018 19:15:43 GMT
        // null になり得ないものを SetValue で扱い、null, enum, string のみ、固有の名前を与える
        // そうでないと、たとえば (nImageFormat?) null と (string) null をコンパイラーが区別できない

        public void SetNull (string key) =>
            Dictionary [key] = new Tuple <object, string> (null, "null");

        public void SetValue (string key, bool value) =>
            Dictionary [key] = new Tuple <object, string> (value, value ? "1" : "0");

        public void SetValue (string key, bool? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? (value.Value ? "1" : "0") : "null");

        public void SetValue (string key, byte value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToString ());

        public void SetValue (string key, byte? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToString () : "null");

        public void SetValue (string key, char value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToUShortString ());

        public void SetValue (string key, char? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToUShortString () : "null");

        public void SetValue (string key, DateTime value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToLongString ());

        public void SetValue (string key, DateTime? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToLongString () : "null");

        // Tue, 30 Oct 2018 04:41:48 GMT
        // decimal は、数値として SQLite に入れるなら、どう頑張っても精度が落ちる
        // 型として SQL に書けるものとしては real になるようだが、これは decimal の半分のビット数しかない
        // Type Affinity というのは内部的な切り分けのようで、numeric でもやはりビット数は全く足りない
        // real 自体、ラウンドトリップが怪しく、たとえば double.MaxValue は「無限」扱いになるし、
        // それを半分に割って入れても、GetDouble で読むだけの当たり前のコーディングでラウンドトリップに失敗する
        // あくまで SQLite は簡易データベースであり、絶対にラウンドトリップが必要なところでは、
        // decimal, double, float については、変数をバイト列にして blob で入れろということなのだろう
        // ラウンドトリップに失敗したままというのは気になるが、real ではどうやっても失敗するため妥協する
        // long は signed にして入れる unsigned でもうまく回るため、実用上は何とかなりそう
        // https://www.sqlite.org/datatype3.html

        public void SetValue (string key, decimal value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToString ());

        public void SetValue (string key, decimal? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToString () : "null");

        public void SetValue (string key, double value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToString ());

        public void SetValue (string key, double? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToString () : "null");

        // Mon, 29 Oct 2018 19:18:39 GMT
        // よく分かっていなくて、Enum? が通らないからと、T? にしたり dynamic にしたりした
        // string 同様、基底クラスは null が通るため、string 的に扱うだけでよさそう
        // dynamic にしても value.Value が動かず、((Enum) value).nToIntString は動いた
        // 知らず知らずのうちに string? のようなことを dynamic で頑張っていたのだろう

        public void SetEnum (string key, Enum value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.nToIntString () : "null");

        public void SetValue (string key, float value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToString ());

        public void SetValue (string key, float? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToString () : "null");

        // Mon, 29 Oct 2018 19:21:08 GMT
        // GUID に ' が含まれることはないが、エスケープを習慣にしておく
        // ここはする、ここはしないという判断をかませていたら、必ずミスをする

        public void SetValue (string key, Guid value) =>
            Dictionary [key] = new Tuple <object, string> (value, $"'{value.nToString ().nEscapeSql ()}'");

        public void SetValue (string key, Guid? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? $"'{value.Value.nToString ().nEscapeSql ()}'" : "null");

        public void SetValue (string key, int value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToString ());

        public void SetValue (string key, int? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToString () : "null");

        public void SetValue (string key, long value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToString ());

        public void SetValue (string key, long? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToString () : "null");

        public void SetValue (string key, sbyte value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToString ());

        public void SetValue (string key, sbyte? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToString () : "null");

        public void SetValue (string key, short value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToString ());

        public void SetValue (string key, short? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToString () : "null");

        // Mon, 29 Oct 2018 19:22:50 GMT
        // "" を null にするかどうか考えたが、無断でデータをいじるべきでない
        // UI 側で string.IsNullOrEmpty をかまして対処するべき

        public void SetString (string key, string value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? $"'{value.nEscapeSql ()}'" : "null");

        public void SetValue (string key, TimeSpan value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToLongString ());

        public void SetValue (string key, TimeSpan? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToLongString () : "null");

        public void SetValue (string key, uint value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToString ());

        public void SetValue (string key, uint? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToString () : "null");

        // Mon, 29 Oct 2018 21:03:23 GMT
        // ulong のまま文字列にすると、GetInt64 での読み出し時に SQLite 側で落ちる
        // 元々 unsigned での読み出しが想定されておらず ulong などのメソッドを揃えるのがおかしいのだろう
        // では、廃止するかと言えば、それは Nekote の型対応のガイドラインに反する
        // 仕方なく ulong を long にして格納しているが、それではソートが成立しないので注意
        // なお、他の unsigned の型については、動いているようなので余計なことをしない
        // ulong もしたくないが、これだけは実際に落ちるのだから仕方ない

        public void SetValue (string key, ulong value) =>
            Dictionary [key] = new Tuple <object, string> (value, ((long) value).nToString ());

        public void SetValue (string key, ulong? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? ((long) value.Value).nToString () : "null");

        public void SetValue (string key, ushort value) =>
            Dictionary [key] = new Tuple <object, string> (value, value.nToString ());

        public void SetValue (string key, ushort? value) =>
            Dictionary [key] = new Tuple <object, string> (value, value != null ? value.Value.nToString () : "null");

        // Mon, 29 Oct 2018 19:25:08 GMT
        // SQL を手書きするときには、識別子でないと分かっているものまで [*] にするのが好きでない
        // 走らせてみたら一発で特定できる問題なので、まずはそのまま書いてみるのが良いと思う
        // しかし、以下のようにプログラムで自動生成するところでは、当然 [*] をかます

        public string ToFirstPartOfInsert ()
        {
            StringBuilder xBuilder = new StringBuilder ();

            foreach (var xPair in Dictionary)
            {
                if (xBuilder.Length > 0)
                    xBuilder.Append (", ");

                xBuilder.AppendFormat ("[{0}]", xPair.Key);
            }

            return xBuilder.ToString ();
        }

        public string ToSecondPartOfInsert ()
        {
            StringBuilder xBuilder = new StringBuilder ();

            foreach (var xPair in Dictionary)
            {
                if (xBuilder.Length > 0)
                    xBuilder.Append (", ");

                xBuilder.Append (xPair.Value.Item2);
            }

            return xBuilder.ToString ();
        }

        public string ToPartOfUpdate ()
        {
            StringBuilder xBuilder = new StringBuilder ();

            foreach (var xPair in Dictionary)
            {
                if (xBuilder.Length > 0)
                    xBuilder.Append (", ");

                xBuilder.AppendFormat ("[{0}] = {1}", xPair.Key, xPair.Value.Item2);
            }

            return xBuilder.ToString ();
        }

        // Tue, 30 Oct 2018 04:40:31 GMT
        // 同じインスタンスで次々と insert や update をしていくのもありだが、
        // 高コストでないため、そのブロック内では全データを保持した方があとあと便利か

        public void Clear ()
        {
            Dictionary.Clear ();
        }
    }
}

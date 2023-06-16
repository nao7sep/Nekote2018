using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Nekote
{
    public static class nStringTableHelper
    {
        // 日本の中小企業では今でも CSV をよく使うため、読み書きのライブラリーを単体で配布することも考えたが、そういうのはおそらく既にある
        // また、CSV データを単一のオブジェクトとして扱うクラスの追加については、そのクラスのオブジェクトを一つ作ってデータをその中で作り込むというよりは、
        // 汎用的なジャグ配列のクラスである nStringTable またはその派生クラスで作り込んだデータを文字列と相互変換するだけの方がシンプルである

        // CSV は、仕様が曖昧で、「方言」が多数存在するが、このクラスでは以下のページを参考とした
        // https://en.wikipedia.org/wiki/Comma-separated_values
        // https://www.ietf.org/rfc/rfc4180.txt
        // http://www.creativyst.com/Doc/Articles/CSV/CSV01.htm
        // https://github.com/parsecsv/csv-spec

        // " のエスケープを行うだけでなく、引用が必要かどうかの判別も行い、そうでないなら null を返す
        // builder は、処理に必要な道具を追加的に借りている感じなので2番目の引数としている

        private static string iEscapeField (string value, StringBuilder builder)
        {
            if (string.IsNullOrEmpty (value))
                return null;

            bool xShouldBeQuoted = false;

            // 以下の英語のコメントは、全てが次のページからの引用である
            // http://www.creativyst.com/Doc/Articles/CSV/CSV01.htm

            // Fields with leading or trailing spaces must be delimited with double-quote characters
            // ここでは、半角空白だけでなく、後ほど検出される \r \n を除くいくつかの空白系文字を調べている

            if (nString.InlineWhitespaceChars.Contains (value [0]) ||
                    nString.InlineWhitespaceChars.Contains (value [value.Length - 1]))
                xShouldBeQuoted = true;

            builder.Clear ();

            foreach (char xChar in value)
            {
                // Fields with embedded commas must be delimited with double-quote characters
                // A field that contains embedded line-breaks must be surrounded by double-quotes

                if (xChar == ',' || xChar == '\r' || xChar == '\n')
                    xShouldBeQuoted = true;

                // Fields that contain double quote characters must be surrounded by double-quotes,
                // and the embedded double-quotes must each be represented by a pair of consecutive double quotes

                else if (xChar == '"')
                {
                    builder.Append ('"');
                    xShouldBeQuoted = true;
                }

                builder.Append (xChar);
            }

            if (xShouldBeQuoted)
                return builder.ToString ();
            else return null;
        }

        public static void nToCsvString (this nStringTable table, StringBuilder builder)
        {
            if (table.RowCount == 0)
                return;

            // フィールドを処理するたびにインスタンスを生成しないため
            StringBuilder xBuilder = new StringBuilder ();

            for (int xRowIndex = 0; xRowIndex < table.RowCount; xRowIndex ++)
            {
                // if (xRowIndex > 0) ...
                // 最初の実装では、最後の行の末尾には改行文字が添えられないようにしていた
                // 空行も「フィールドが一つもない行」が存在するものとして処理する実装だからである
                // しかし、RFC 4180 には The last record in the file may or may not have an ending line break とあり、
                // この仕様は最優先なので、読み込み時には、文字列末尾の空行は、行そのものが存在しないものとして処理する必要がある
                // そのため、各行の末尾に必ず改行文字を添えるというシンプルかつ一貫性の高い実装に変更した

                nStringTableRow xRow = table [xRowIndex];

                // 空のフィールドが一つだけ含まれる行の出力においては、CSV 側にはただの空行が入る
                // フィールドが二つなら、いずれも空でも、, で区切られることによって空のフィールドが二つ戻ってくるが、
                // 一つなら、一つだということで特別に引用されるわけでもないため、ラウンドトリップが不成立となる
                // これは仕様であり、Nekote の開発においては、実用性の乏しい作り込みに時間を割くことを極力避けている
                // CSV には方言が多く、以前に仕事で関わった人は、自分の構文解析の難易度を下げるため、全フィールドの引用を要求してきた
                // 通常のデータが正確に扱われる限り「動けばよい」でごまかされている部分が CSV には多々ある
                // そのため、Nekote でも、吐くときは「最小限」、読むときは「厳密」と、実装の基準を異ならせている
                // ラウンドトリップが成立しないデータを探せば見つかるが、通常のデータでは問題なく動作し、
                // Nekote の実装で問題があるなら、それはデータの方に改善の余地があるという程度にまとまっている

                for (int xFieldIndex = 0; xFieldIndex < xRow.FieldCount; xFieldIndex ++)
                {
                    if (xFieldIndex > 0)
                        builder.Append (',');

                    string xValue = xRow [xFieldIndex],
                        xEscaped = iEscapeField (xValue, xBuilder);

                    if (xEscaped != null)
                    {
                        builder.Append ('"');
                        builder.Append (xEscaped);
                        builder.Append ('"');
                    }

                    else builder.Append (xValue);
                }

                builder.AppendLine ();
            }
        }

        public static string nToCsvString (this nStringTable table)
        {
            StringBuilder xBuilder = new StringBuilder ();
            table.nToCsvString (xBuilder);
            return xBuilder.ToString ();
        }

        private static string iTrimField (string value, int quotationMarkCount)
        {
            // Leading and trailing space-characters adjacent to comma field separators are ignored
            // iEscapeField で nString.InlineWhitespaceChars が使われているため、トリミングにも nTrim が使われている

            if (quotationMarkCount == 0)
                return value.nTrim ();

            // " に続く , などは区切り文字としての効力を持たないが、CSV の末尾が ," で終わっているなどすれば、ここに到達する
            // 閉じタグのない HTML のように寛容に処理することも選択肢だが、データの信頼性の問題が示唆されるため、やはり例外が飛ぶべき

            else if (quotationMarkCount == 1)
                throw new nInvalidFormatException ();

            // Fields with leading or trailing spaces must be delimited with double-quote characters
            // こちらは、空白系文字を残すためにわざわざ引用されているフィールドなので、Excel などのような安易なトリミングはやめておく
            else return value;
        }

        public static void nCsvToStringTable (this string text, nStringTable table)
        {
            if (string.IsNullOrEmpty (text))
                return;

            // 行は、必要になってから NewRow で作られる
            nStringTableRow xRow = null;
            // 引用符を除く、エスケープが解除されたフィールドの値がここに入る
            StringBuilder xBuilder = new StringBuilder ();
            // 引用符の検出と同時に検証も行われ、引用の状態が把握される
            int xQuotationMarkCount = 0;

            for (int temp = 0; temp < text.Length; temp ++)
            {
                char xCurrent = text [temp];

                // 以下、フィールド内のもの、フィールド区切り、レコード区切り、それ以外の順で処理

                if (xCurrent == '"')
                {
                    if (xQuotationMarkCount == 0)
                    {
                        // フィールド内に引用符があるなら、そのうち一つが先頭にないことはありえない
                        // 引用なら最初と最後にあり、エスケープされた " は、フィールドの先頭に置かれない
                        // _ を空白系文字として _"hoge"_ のようなフィールドは、グレーのようだが Nekote では蹴る
                        // 敢えてそうする必要のない出力であり、CSV を生成する側のコードを改善すべきのため

                        if (xBuilder.Length > 0)
                            throw new nInvalidFormatException ();

                        // 引用符は xBuilder に追加されない
                        xQuotationMarkCount ++;
                    }

                    else if (xQuotationMarkCount == 1)
                    {
                        // 引用中の "" は、引用の終わりでなく、" がエスケープされたもの
                        // 次の文字を読み飛ばすために temp を進め、" 一つを xBuilder に入れる
                        // 次が " かどうか調べられるのは既に引用中のときのみであり、
                        // "" "hoge" """" は、いずれも問題なく処理される

                        if (temp + 1 < text.Length && text [temp + 1] == '"')
                        {
                            temp ++;
                            xBuilder.Append (xCurrent);
                        }

                        // 次が " でない " は、いかなる場合においても引用の終わり
                        // これが最後の文字で、次の文字がない場合も、そう処理して問題ない
                        // 次が a など、通常の文字なら、下の方の else できちんと例外が飛ぶ
                        else xQuotationMarkCount ++;
                    }

                    // ""hoge"" のような、引用部分が二つ以上あるフィールドなら例外を投げる
                    // これもグレーのようで、積極的にチェックしない実装があるようだが、
                    // 敢えてそうする必要のない出力なので Nekote では蹴る

                    // 追記: 引用符の数を数えなくても、後続の else のところで例外が飛ぶ
                    // つまり、""hoge" の最後の " を待たずとも ""h の時点でアウト
                    // それでも、チェックが多重なのは不足よりマシだろうからこのままにしておく

                    else throw new nInvalidFormatException ();
                }

                else if (xCurrent == ',')
                {
                    // 引用中でないのは、引用符の数が0または2のときに限られる
                    // 2のときに引用符が見付かれば例外が飛ぶため、3以上になることはない
                    if (xQuotationMarkCount == 0 || xQuotationMarkCount == 2)
                    {
                        // 以下、行は必要になったときに作られ、xBuilder の長さに関わらずフィールドが追加される
                        // xRow, xBuilder, xQuotationMarkCount は、必要に応じて初期化される
                        // , のとき、\r や \n のとき、末尾のときで処理が微妙に異なるため、
                        // それらの違いについては、\r や \n のところにまとめておく

                        if (xRow == null)
                            xRow = table.NewRow ();

                        xRow.AddField (iTrimField (xBuilder.ToString (), xQuotationMarkCount));

                        // xRow = null;
                        xBuilder.Clear ();
                        xQuotationMarkCount = 0;
                    }

                    // 引用中なら , がフィールドにそのまま含まれる
                    else xBuilder.Append (xCurrent);
                }

                // \r や \n は、「行を終える機能を持つ ,」くらいに考えられないわけでもないが、
                // 単独でフィールドの存在を確定できるかどうかの違いがあるため、コードは別々にしている
                // \r と \n の実装は、直後の \n の読み飛ばしを除いて同じなので共通化が可能

                else if (xCurrent == '\r' || xCurrent == '\n')
                {
                    // , と同様に \r と \n も引用中でないときだけ効力を持つ
                    if (xQuotationMarkCount == 0 || xQuotationMarkCount == 2)
                    {
                        if (xCurrent == '\r')
                        {
                            if (temp + 1 < text.Length && text [temp + 1] == '\n')
                                temp ++;
                        }

                        // まず、前提として、フィールドの存在を単独で確定できるものには、文字、引用、, の三つがある
                        // 改行や（文字列の）終端によってもフィールドが追加されることがあるが、これらには他の条件がついている
                        // そして、Nekote の実装においては、,、改行、終端の三つにおいてフィールドが追加される可能性がある
                        // 以下、それぞれについて、仕様の細かな違いとその理由をまとめておく

                        // , は、どこにあろうと前後のフィールドの存在を単独で確定する
                        // 前から読んでの構文解析なので , があった時点で次のフィールドの内容まで分かるわけでないが、
                        // 少なくとも直前にフィールドがあったのは（引用中でないなら）確かであり、
                        // よって、, が見付かれば、行がないなら新たに作ってでもフィールドを追加する
                        // その際には xBuilder や xQuotationMarkCount の内容が見られることはない
                        // , 単体で絶対にフィールドは存在するため

                        // 改行は、基本的には、ある行に含まれる最後のフィールドの終わりを意味するが、
                        // きちんと出力されていない CSV においては、データを含まない空行であることも考えられる
                        // そのため、xBuilder が空でなく、フィールドを定義する三つのうちの一つである「文字」があるとき、
                        // あるいは、xQuotationMarkCount が2であり、同じく三つに一つの「引用」があるときのみ、
                        // 行がないなら新たに作ってまでフィールドを追加する
                        // 前者が空で、後者が0なら、「文字」がなく、「引用」もないということなので、
                        // 既に行が作られているときのみ、空のフィールドをその行に追加する
                        // これは、, によって後続フィールドの存在が確定されており、それを読んでいるときのこと
                        // 既に見付かっている , によって作成された行への空のフィールドの追加である

                        // 終端は、ループを抜けたというトリガーを無視し、そこに見えない改行があるかのように考えてよい
                        // CSV の終端に , を並べたり、改行を織り交ぜたり、いろいろしてみたが、改行のところと同じ実装でうまくいった
                        // データがあれば、行を作ってでもフィールドを追加し、データがないなら、行がある場合のみ空のフィールドを追加する
                        // 先に xRow を見た方がコードが少し短くなるし、実際に最初はそう書いていたが、
                        // 処理の流れを人間が追いやすいのは今の書き方なので、そうしている
                        // else の方がなければ、たとえば a, という CSV はフィールドが一つとなる
                        // , は単独で前後のフィールドの存在を確定するので、後続のフィールドを拾ってやる必要がある

                        if (xBuilder.Length > 0 || xQuotationMarkCount == 2)
                        {
                            if (xRow == null)
                                xRow = table.NewRow ();

                            xRow.AddField (iTrimField (xBuilder.ToString (), xQuotationMarkCount));
                        }

                        else
                        {
                            if (xRow != null)
                                xRow.AddField (iTrimField (xBuilder.ToString (), xQuotationMarkCount));
                        }

                        xRow = null;
                        xBuilder.Clear ();
                        xQuotationMarkCount = 0;
                    }

                    // 引用中なら改行文字もフィールドに含まれるのが仕様
                    // 対応しているプログラムが少ないようだが、して損はない
                    else xBuilder.Append (xCurrent);
                }

                else
                {
                    // " 以外の文字に続く " と同様、引用が終わってからの文字も問題である
                    // 先述したが、空白系文字だとしても、Nekote では正確を期して蹴る

                    if (xQuotationMarkCount == 2)
                        throw new nInvalidFormatException ();

                    xBuilder.Append (xCurrent);
                }
            }

            // 最後のフィールドの読み込み中にファイルが終わった場合の処理
            // フィールド区切りや行区切りで終わらない CSV はよくある

            // [EOF] を改行のようにみなしての処理に過ぎないため、このチェックを省く理由はない
            if (xQuotationMarkCount == 0 || xQuotationMarkCount == 2)
            {
                // 以前は xBuilder しか見ていなかったが、xQuotationMarkCount も見るように変更
                // 最後の行に "" だけ書かれていて、それで「空のフィールド一つの行」が示されることはまずないだろうが、
                // フィールドを単独で定義するものとしては、文字、引用、, の三つを考えているため、実装に整合性を与えている
                // , のとき、\r や \n のとき、末尾のときの実装の違いについては \r や \n のところに書いておく

                if (xBuilder.Length > 0 || xQuotationMarkCount == 2)
                {
                    if (xRow == null)
                        xRow = table.NewRow ();

                    xRow.AddField (iTrimField (xBuilder.ToString (), xQuotationMarkCount));
                }

                else
                {
                    if (xRow != null)
                        xRow.AddField (iTrimField (xBuilder.ToString (), xQuotationMarkCount));
                }

                // xRow = null;
                // xBuilder.Clear ();
                // xQuotationMarkCount = 0;
            }

            // 終端だろうと、引用符の数がおかしければ投げる
            else throw new nInvalidFormatException ();

            // RFC 4180 には The last record in the file may or may not have an ending line break とあるが、
            // 改行文字で行が終わっただけでは次の行が作られない実装にしているため、最後の行が空なら消すような処理は不要
        }

        public static nStringTable nCsvToStringTable (this string text)
        {
            nStringTable xTable = new nStringTable ();
            text.nCsvToStringTable (xTable);
            return xTable;
        }
    }
}

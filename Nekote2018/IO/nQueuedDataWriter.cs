using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Mon, 06 May 2019 09:02:30 GMT
    // キューにデータを入れていき、特定の条件を満たしたらフラッシュするというのは、よくある実装である
    // それをシンプルなクラスとしてまとめ、まずは CSV での書き込みのみ実装した
    // ファイル名の決め方、毎回新しいファイルを作るのか追記するのか、書き込みのフォーマット、
    // 「自動的に書き込む」のフラグ、そのときの件数の条件（いくつで発動し、いくつまで減らすか）などをプロパティーに入れておけば、
    // 件数が増える、基本的には唯一のメソッドである Enqueue 内で自動的に書き込ませることができて楽だが、
    // そういったことは、nStatic にメソッドを用意し、nAutoLock で lock をつけてラップすれば、呼び出し側でも容易に可能
    // そのため、パスの決定、書き込みのフォーマットへの変換、いくつを上限に出力するのかを敢えて引数にしてみた
    // Write* と Append* を別々に用意するのは、nFile でもそうしているし、引数で指定しなければならないほど低頻度の区別でないため
    // フォーマットごとにメソッドを用意していくのは、フォーマットによって内部処理が大きく異なるため
    // 残す数でなく書き込む数の上限を指定するのは、書き込みのコストを分散化するにおいては件数にこそ比例するため
    // いくつ残すかというのは、そのためにいくつ書き込み、それと比例してどのくらいのコストがかかるかへの注意が乏しい

    // Wed, 08 May 2019 07:09:59 GMT
    // このクラスでは、まず、ログインとページアクセスのログを扱う
    // それらを CSV で出力するわけだが、nStringTable を CSV にする実装は、入念にチェックはしたが、テストが全く足りていない
    // それでも CSV を使うのは、1) KVP で「1エントリー1ファイル」にしてはファイル数が大変なことになる、
    // 2) CSV の実装をテストする機会だと前向きに考えられる、3) 実装に問題があって初期のデータを失ってもログなので困らない、
    // 4) 個人的にまだ実務経験が全くない SQLite については「上位」とみなしていて、Nekote の基本的な機能の動作に不可欠にしたくない、
    // あたりが理由である
    // 何度も何度もチェックしたし、分かりやすいコードをきれいに書けたはずだが、CSV の実装には今でも不安がある
    // そのため、今までも CSV を内部的に使うことは一度もなく、ただ存在するだけの機能になっていた
    // しかし、日本の中小企業とのデータのやり取りでは今後も CSV が使われそうだし、そろそろ本格的にデバッグをしたい

    // Mon, 06 May 2019 09:30:53 GMT
    // このクラスの使用において注意するべきは、データがソートされないことである
    // Queue なので、入れたものから出力されるが、nDateTimeBased* のように明示的にソートされるわけでない
    // そのため、このクラスが出力したものは、統計処理において改めてソートするのが無難である

    public class nQueuedDataWriter <T>
    {
        public Queue <T> Entries { get; private set; } = new Queue <T> ();

        public int EntryCount
        {
            get
            {
                return Entries.Count;
            }
        }

        // Mon, 06 May 2019 09:21:59 GMT
        // Entries.Enqueue が戻り値を持たないので、それに合わせている
        // entry を返した方がすぐに次の処理に移れて1行減るが、分かりにくくなるか
        public void Enqueue (T entry) =>
            Entries.Enqueue (entry);

        // Mon, 06 May 2019 09:23:01 GMT
        // Write* と Append* の二択であり、今後増えないので appends で区別
        // writes にしては、false のときに単純否定の「書かない」を連想しそうなので、appends にしている
        // path が一切チェックされないため、呼び出し側で lock を行うべき
        // 少しでも lock の時間を減らすなら、このメソッドには文字列化のみ lock なしで行わせ、
        // パスの決定とファイルへの出力のみ lock 内で行う選択肢もあるが、その場合、Entries でコリジョンが起こる
        // かといって、Entries をロックする lock とファイルシステムをロックする lock を併存させて……のようなことは、デッドロックのリスクを高める
        // 開発の生産性、今後の管理コスト、現実的なリスクなどを考えるなら、「大きな lock 内で小さく速く書く」というのが現実的な妥協だろう

        private int iWriteAsCsvToFile (bool appends, string path, Func <T, nStringTableRow> typeToStringTableRow, int maxEntryCountToWrite)
        {
            nStringTable xTable = new nStringTable ();

            for (int temp = 0; temp < maxEntryCountToWrite; temp ++)
            {
                if (EntryCount == 0)
                    break;

                xTable.AddRow (typeToStringTableRow (Entries.Dequeue ()));
            }

            // Mon, 06 May 2019 09:27:33 GMT
            // ここで xTable.RowCount を見れば無用の書き込みを防げるが、
            // それは呼び出し側が EntryCount を見て回避するべきこと

            string xContent = xTable.nToCsvString ();

            if (appends)
                nFile.AppendAllText (path, xContent);
            else nFile.WriteAllText (path, xContent);

            // Mon, 06 May 2019 09:28:40 GMT
            // 削ることも考えたが、実際の結果を返すメソッドは多い
            // 呼び出し側での計算と一致しないこともあるため
            return xTable.RowCount;
        }

        public int WriteAsCsvToFile (string path, Func <T, nStringTableRow> typeToStringTableRow, int maxEntryCountToWrite = int.MaxValue) =>
            iWriteAsCsvToFile (false, path, typeToStringTableRow, maxEntryCountToWrite);

        public int AppendAsCsvToFile (string path, Func <T, nStringTableRow> typeToStringTableRow, int maxEntryCountToWrite = int.MaxValue) =>
            iWriteAsCsvToFile (true, path, typeToStringTableRow, maxEntryCountToWrite);
    }
}

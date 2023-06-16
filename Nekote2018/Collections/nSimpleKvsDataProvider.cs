using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Sat, 04 May 2019 06:06:32 GMT
    // nSimpleLogDataProvider と同様のコンストラクターを持ち、string を扱う CRUD のメソッドを持ち、nDictionary の Set*, Get* を持つクラス
    // Kvs というディレクトリー名にした理由は memo.txt に、他クラスから仕様を引き継いだ部分についてはそれぞれのクラスにコメントがある

    // Sat, 04 May 2019 09:03:12 GMT
    // キーごとにファイルを作るという、単純さと堅牢性を重視しての仕様なので、
    // キーが過度に長かったり、ファイル名に使えない文字を含んでいたり、
    // 大文字・小文字の違い以外に他のキーと違いを持たなかったりのときに不具合が生じる
    // もっとも、それぞれはゆるい制限で、階層的なキーには、識別子との整合性を考えて _ を使えば問題はない
    // プロパティー名などに使えなくなってもいいなら . でつなげる選択肢もあり、何とでもなるだろう

    // Sat, 04 May 2019 10:05:29 GMT
    // Task.Run で A 個のスレッドを用意し、B 回の処理を行う二重の for ループにおいて、
    // まず nStatic.SetKvsStringAutoLock (nRandom.NextWord (), nRandom.NextSentence ()) を、
    // 続いて nStatic.GetKvsStringOrNullAutoLock (nRandom.NextWord ()) を行ってみた
    // x のところは、AxB のフォーマットになっていて、その積が総回数である
    // Set* (10x10): 669ms, Get* (10x100000): 1315ms
    // Set* (10x10): 621ms, Get* (100x10000): 1710ms
    // Set* (10x10): 606ms, Get* (1000x1000): 1710ms
    // Set* (10x100): 6051ms, Get* (1000x1000): 1738ms
    // Set* (100x10): 6129ms, Get* (1000x1000): 1729ms
    // Set* (100x100): 61072ms, Get* (1000x1000): 1788ms
    // 書き込みがやはり重たく、1回6ミリ秒くらいかかるため、数十回で遅延を体感するだろう
    // 読み込みは Dictionary 任せなので速く、lock ありなので、スレッド数による変化も小さかった
    // ASP.NET なら、たびたび起こる一括ロードのコストが気になるが、数百件なら問題なさそうである

    public class nSimpleKvsDataProvider: nKeyBasedSimpleDataProvider <nSimpleKvsEntry>
    {
        public nSimpleKvsDataProvider (string directoryPath): base (directoryPath, nSimpleKvsEntry.EntryToDictionary, nSimpleKvsEntry.DictionaryToEntry)
        {
        }

        // Sat, 04 May 2019 06:31:04 GMT
        // あくまで CRUD なので、string を扱うものも、まずは CRUD の命名にする
        // ただ、Read は、戻り値の型が異なるため、KVS の V である Value をつけるしかない
        // いずれも下位の CRUD のメソッドの組み合わせによって実装しているため、データの不整合は起こらない
        // ReadValue の実装においては、nDictionary を見ながらだったので、キーがないなら落とした方がいいのでないかと思ったが、
        // それを言うなら Update や Delete でも落とすべきで、しかし、そういう実装では try / catch だらけになるため、今のままでいい
        // Contains を見てから ReadValue を呼ぶなら Contains を二度呼ぶことになる無駄があるが、
        // 「落ちない CRUD」がもたらす安定性を損ねてまで気にするべきコストでない

        public bool Create (string key, string value) =>
            Create (key, new nSimpleKvsEntry (value));

        public string ReadValue (string key)
        {
            if (Contains (key))
                return Read (key).Value;
            else return null;
        }

        public bool Update (string key, string value)
        {
            if (Contains (key))
            {
                nSimpleKvsEntry xEntry = Read (key);
                xEntry.LastUpdateUtc = DateTime.UtcNow;
                xEntry.Value = value;
                Update (key, xEntry);
                return true;
            }

            else return false;
        }

        // Sat, 04 May 2019 06:36:57 GMT
        // CRUD は、存在するキーでエントリーを Create しないとか、ないキーなら Update できないとか、
        // データとしての整合性を重視する仕様になっていて、MVC に使いやすいだろうが、カジュアルさに乏しい
        // そのため、Fire-and-forget 的な使い方のできる Set* と Get* も用意しておく
        // KVS では CRUD よりこちらの使用頻度が間違いなく高いため、nStatic などではこちらだけラップする
        // CRUD は、Contains を見ながらの使用が前提であり、*AutoLock を作りにくいという問題が大きい

        // Sat, 04 May 2019 08:55:01 GMT
        // SetString は、キーがあれば Update し、なければ Create する
        // 上位のメソッドの組み合わせだけで動き、データの不整合のリスクもない

        public void SetString (string key, string value)
        {
            if (Contains (key))
                Update (key, value);
            else Create (key, value);
        }

        // Sat, 04 May 2019 08:55:56 GMT
        // 「落ちない CRUD」と異なり、nDictionary 的な GetString では、何か問題があれば落ちる
        // nDictionary でも落ちるため、ここだけ落ちないのでは、それぞれの仕様について分かりにくくなる
        // キーがないと落ちてくれないと困るため、意図的に下位のプロパティーを参照している
        // nStatic.Logs ["..."] もしたく、*OrNull で this [...] を実装することも考えたが、
        // それも、他では this [...] が落ちやすいこととの不整合になり、
        // 落ちる this [...] を作るのでは GetStringOr* の方が多様性があるため、
        // this [...] の実装そのものをこのクラスではやめておくことにした

        public string GetString (string key) =>
            Entries [key].Value;

        // Sat, 04 May 2019 08:59:48 GMT
        // nDictionary の御三家を nDictionary と同様に実装
        // KVS であり、書き込みも容易なので、*OrDefault の値を書き込んでしまうオプションも考えたが、
        // 同じことをしたければ1行の追加で足るし、過度な自動化でコードを分かりにくくしたくない

        public string GetStringOrDefault (string key, string value)
        {
            if (Contains (key))
            {
                string xValue = Entries [key].Value;

                if (string.IsNullOrEmpty (xValue) == false)
                    return xValue;
            }

            return value;
        }

        public string GetStringOrEmpty (string key) =>
            GetStringOrDefault (key, string.Empty);

        public string GetStringOrNull (string key) =>
            GetStringOrDefault (key, null);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace Nekote
{
    // Sun, 07 Apr 2019 14:03:24 GMT
    // SQLite の接続文字列については、たまに調べて、設定しないといけないところをザッと探して……というのを繰り返している
    // UTF-8 を使わないなど、毎回そうする設定というのも出てきているため、必要最小限のラッパーをかましてみる
    // https://www.connectionstrings.com/sqlite/
    // http://dotnetcsharptips.seesaa.net/article/446853333.html
    // https://github.com/haf/System.Data.SQLite/blob/master/System.Data.SQLite/SQLiteConnectionStringBuilder.cs

    public class nSqliteConnectionStringBuilder
    {
        public SQLiteConnectionStringBuilder Builder { get; private set; }

        // Sun, 07 Apr 2019 14:00:21 GMT
        // ConnectionString も DataSource もそうだが、初期値が null でなく "" なので注意
        // ConnectionString を直接設定することが自分はないのにプロパティーを用意するのは、コンストラクターに入っているため
        // おそらくパーザーとしての役目もあって、使うときにはかなり使うだろうから、ここはそのまま引き継いでいる

        public string ConnectionString
        {
            get
            {
                return Builder.ConnectionString;
            }

            set
            {
                Builder.ConnectionString = value;
            }
        }

        public string DataSource
        {
            get
            {
                return Builder.DataSource;
            }

            set
            {
                Builder.DataSource = value;
            }
        }

        // Sun, 07 Apr 2019 14:35:37 GMT
        // 読み取り専用で開くというのは、分かりやすいため、使うことがありそう
        // もちろん、デフォルトは false

        public bool ReadOnly
        {
            get
            {
                return Builder.ReadOnly;
            }

            set
            {
                Builder.ReadOnly = value;
            }
        }

        // Sun, 07 Apr 2019 14:37:05 GMT
        // いつ落ちるか分からないコンピューターでの集計処理などでは Full があり得る
        // パフォーマンスに響くのが間違いないが、これも分かりやすく、たぶん使うことがある

        /// <summary>
        /// 書き込みのたびにフラッシュしたければ、Full に設定する。
        /// </summary>
        public SynchronizationModes SyncMode
        {
            get
            {
                return Builder.SyncMode;
            }

            set
            {
                Builder.SyncMode = value;
            }
        }

        private void iSetOptimalValues ()
        {
            // Sun, 07 Apr 2019 14:39:23 GMT
            // 以下、アルファベット順にプロパティーを並べた上、めんどくさいので単一のコメントを書く
            // DateTime* の三つは、特に一つ目を Ticks にしようとするとインテリセンスが非推奨を言ってくるので、さわらないでおく
            // デフォルトの ISO 8601 でも「秒」未満が7～8桁保持されるようで、Ticks に迫る精度を期待できる
            // DefaultIsolationLevel や JournalMode については、単一プロセスが lock 多用でアクセスするならそのままでよさそう
            // 複数のプロセスがアクセスするようになったらコリジョンなどの問題も出てくるが、それは、ちゃんとした RDBMS の仕事
            // FailIfMissing は、デフォルトで false であり、ファイルが存在しないと作ってくれるので、そのままでいい
            // Pooling は、デフォルトが false なのが不思議で、理由を考えてしまうが、
            // ネットで調べた限り、特に注意すべき点はなさそうで、大勢が高速化を喜んでいるようである
            // UseUTF16Encoding はデフォルトが null で、その場合は UTF-8 のデータベースになるようだ
            // bool 型だが、その内部にたぶん Nullable 的なものが入っていて、いきなり読もうとすると null の例外が飛ぶ
            // 英語圏なら UTF-8 でよいが、文字の9割以上が全角の日本人がその設定のままでは非効率的

            // Builder.DateTimeFormat
            // Builder.DateTimeFormatString
            // Builder.DateTimeKind
            // Builder.DefaultIsolationLevel
            // Builder.FailIfMissing
            // Builder.JournalMode
            Builder.Pooling = true;
            Builder.UseUTF16Encoding = true;
        }

        public nSqliteConnectionStringBuilder ()
        {
            Builder = new SQLiteConnectionStringBuilder ();
            iSetOptimalValues ();
        }

        public nSqliteConnectionStringBuilder (string connectionString)
        {
            Builder = new SQLiteConnectionStringBuilder (connectionString);
            iSetOptimalValues ();
        }

        // Sun, 07 Apr 2019 15:38:55 GMT
        // nSmtpSettings と同じ感覚でデータをロードできるようにしておく
        // ConnectionString を細かく設定するときに ReadOnly などだけ別のキーで設定することがないため、
        // ConnectionString を取得できたなら、それ以外についてはロード自体を省くようにしている

        public void Load (nNameValueCollection collection, string keyPrefix = null, string keySuffix = null)
        {
            ConnectionString = collection.GetStringOrNull (keyPrefix + "SqliteConnectionString" + keySuffix);

            if (string.IsNullOrEmpty (ConnectionString))
            {
                DataSource = collection.GetStringOrNull (keyPrefix + "SqliteDataSource" + keySuffix);
                ReadOnly = collection.GetBoolOrDefault (keyPrefix + "SqliteReadOnly" + keySuffix, false);
                SyncMode = collection.GetEnumOrDefault <SynchronizationModes> (keyPrefix + "SqliteSyncMode" + keySuffix, SynchronizationModes.Normal);
            }
        }

        public void Load (nDictionary dictionary, string keyPrefix = null, string keySuffix = null)
        {
            ConnectionString = dictionary.GetStringOrNull (keyPrefix + "SqliteConnectionString" + keySuffix);

            if (string.IsNullOrEmpty (ConnectionString))
            {
                DataSource = dictionary.GetStringOrNull (keyPrefix + "SqliteDataSource" + keySuffix);
                ReadOnly = dictionary.GetBoolOrDefault (keyPrefix + "SqliteReadOnly" + keySuffix, false);
                SyncMode = dictionary.GetEnumOrDefault <SynchronizationModes> (keyPrefix + "SqliteSyncMode" + keySuffix, SynchronizationModes.Normal);
            }
        }

        public override string ToString () =>
            Builder.ToString ();
    }
}

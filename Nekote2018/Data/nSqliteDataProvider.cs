using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace Nekote
{
    // Sat, 06 Apr 2019 16:18:12 GMT
    // いろいろと普通に SQLite を使いたくなったので、コードを短縮化するためのラッパーを用意
    // Microsoft のものは命名が Sqlite* であり、本家と一致しないが、仕様に決定的な違いはなさそう
    // https://qiita.com/koshian2/items/63938474001c510d0b15
    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite

    // Sat, 06 Apr 2019 22:29:28 GMT
    // client というのも考えたが、server ありきのものなので provider の方がしっくりくる
    // さらに上位クラスを作って ORM をする可能性もあるため、最下層に provider を置くというイメージ

    public class nSqliteDataProvider: IDisposable
    {
        public string ConnectionString { get; private set; }

        // Sat, 06 Apr 2019 16:36:34 GMT
        // タイムアウトについては Microsoft と本家では実装が異なっていて、接続については本家の方が設定項目が多い
        // ただ、lock ができていて、対象がファイルなら、接続のタイムアウトについては気にする必要がなさそうで、
        // コマンドの方も、デフォルトの30秒を積極的に変更するべき理由がないため、タイムアウト関連のプロパティーを全て省く

        public SQLiteConnection Connection { get; private set; }

        // Sat, 06 Apr 2019 16:25:07 GMT
        // Microsoft の方では SqliteConnection.Transaction に入っているが、本家では存在しない
        // https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqliteconnection
        public SQLiteTransaction Transaction { get; private set; }

        // Sat, 06 Apr 2019 16:03:32 GMT
        // Cancel は does nothing とのことなので、どこでも呼ばない
        // https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitecommand.cancel

        public SQLiteCommand Command { get; private set; }

        private readonly List <SQLiteDataReader> mDataReaders = new List <SQLiteDataReader> ();

        public nSqliteDataProvider (string connectionString)
        {
            ConnectionString = connectionString;
            Connection = new SQLiteConnection (ConnectionString);
            Connection.Open ();
            Command = Connection.CreateCommand ();
        }

        // Sat, 06 Apr 2019 16:12:46 GMT
        // トランザクションは、new SQLiteCommand で指定できるようになっているが、
        // どちらかと言えば接続に関連付けられるもので、コマンドとは独立して動くと考えてよさそう
        // https://www.devart.com/dotconnect/sqlite/docs/Devart.Data.SQLite~Devart.Data.SQLite.SQLiteTransaction.html

        public void BeginTransaction ()
        {
            if (Transaction != null)
                Transaction.Dispose ();

            Transaction = Connection.BeginTransaction ();
        }

        public void RollbackTransaction () =>
            Transaction.Rollback ();

        public void CommitTransaction () =>
            Transaction.Commit ();

        // Sat, 06 Apr 2019 16:40:09 GMT
        // コマンドのインスタンスを使い回すのは、大勢がやっているので、たぶん大丈夫だろう
        // トランザクションは接続の方に関連付けられるので、コマンドを処理の回数だけ作ることも可能だろうし、
        // もしかするとそれが正しいコーディングなのかもしれないとも思うが、まずは使い回して様子を見る

        public int ExecuteNonQuery (string commandText)
        {
            Command.CommandText = commandText;
            return Command.ExecuteNonQuery ();
        }

        public object ExecuteScalar (string commandText)
        {
            Command.CommandText = commandText;
            return Command.ExecuteScalar ();
        }

        // Sat, 06 Apr 2019 16:04:11 GMT
        // 以前の実装では DataReader も内包し、Get* を全てラップしていたが、今回は SQLiteDataReader をもらう
        // その参照を取って普通にループで回した方が早いし、その機能性を高めるための拡張メソッドも用意したため
        // ただ、SQLiteDataReader は、Close を忘れやすいという問題がある
        // ADO.NET との整合性のために用意されているだけのような気もして、SQLite では問題でないかもしれないが、呼ばないのは気になる
        // またまた本家でないが、always call Close when done reading とするページもある
        // そのため、mDataReaders にインスタンスを入れていき、一応、IsClosed を見ながら閉じておく
        // これは内部的な処理なので、プロパティーも外部に公開されない
        // https://www.devart.com/dotconnect/sqlite/docs/Devart.Data.SQLite~Devart.Data.SQLite.SQLiteDataReader.html

        /// <summary>
        /// SQLiteDataReader は、Dispose 時に自動的に閉じられる。
        /// </summary>
        public SQLiteDataReader ExecuteReader (string commandText)
        {
            Command.CommandText = commandText;
            SQLiteDataReader xDataReader = Command.ExecuteReader ();
            mDataReaders.Add (xDataReader);
            return xDataReader;
        }

        public void Dispose ()
        {
            if (Connection != null)
            {
                // Sat, 06 Apr 2019 16:29:50 GMT
                // 内部的に Close が呼ばれるだろうから、トランザクションが存在すればロールバックされる
                // https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqliteconnection.dispose
                // https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqliteconnection.close
                Connection.Dispose ();
                Connection = null;
            }

            if (Transaction != null)
            {
                Transaction.Dispose ();
                Transaction = null;
            }

            if (Command != null)
            {
                Command.Dispose ();
                Command = null;
            }

            if (mDataReaders.Count > 0)
            {
                foreach (SQLiteDataReader xDataReader in mDataReaders)
                {
                    // Sat, 06 Apr 2019 16:33:43 GMT
                    // Microsoft の実装（？）の方には Dispose があるが、本家にはないので、できるだけのことをしている
                    // https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlite.sqlitedatareader.dispose

                    if (xDataReader.IsClosed == false)
                        xDataReader.Close ();
                }

                mDataReaders.Clear ();
            }
        }
    }
}

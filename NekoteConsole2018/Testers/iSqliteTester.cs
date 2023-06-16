using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nekote;
using System.Data.SQLite;
using System.Reflection;
using System.Threading;

namespace NekoteConsole
{
    internal static class iSqliteTester
    {
        // Sun, 28 Oct 2018 06:57:01 GMT
        // SQLite については、手軽だ、便利だ、意外と速いという情報が多く、ベタ褒めになっているが、同時アクセスが危ないとのこと
        // そこで、SQLite に頼らず、ReaderWriterLockSlim をゴリゴリ使って、とにかく確実に動くコードを書いてみた

        // Fri, 29 Mar 2019 00:45:52 GMT
        // ReaderWriterLockSlim は、何にアクセスするのか、読むのか書くのか、といったことをきちんと意識しないとバグにつながる
        // 仕組みとしては、読んでいる途中で書き込みロックを取れず、その逆も無理で、書いている途中での書き込みロックも無理だが、読んでいる途中での読み込みロックのみいける
        // そのため、書き込みより読み込みが圧倒的に多いとき、たとえば複数スレッドでデータベースにアクセスしてページを同時に生成するようなところでは、合理性が際立つ
        // その代わり、読み込みロックの中で、そのコードブロックはもちろん、呼ぶメソッドが内部で呼ぶメソッドでさえ書き込みが一切行われないのを保証する必要がある
        // それがしんどいなら、lock により、読み書きの両方を想定したクリティカルセクションにする方が、try / finally も不要でコードがシンプルになる
        // ReaderWriterLockSlim 自体は、合理的にできていて、バグもなさそうなので、今後も使うし、以下でも使っているのをそのままにするが、
        // 「読み込みロックの中で、末端の処理まで絶対に書き込んでいないか」を以前は十分に意識できていなかったため、バグを埋没させた可能性が出ている
        // そのため、注意が必要というのを念頭に置くためにこのコメントを残しておく

        // Sun, 31 Mar 2019 04:09:38 GMT
        // 以下では Thread クラスのインスタンスが直接作成されているが、スレッドプールを使うか Task を使うかが主流のようだ
        // こちらはテストコードなのでこのまま放置するが、他のところでは修正し、今後は基本的に Task を使っていく

        // Sun, 07 Apr 2019 12:44:57 GMT
        // nSqliteDataProvider を書いたので、それを使うコードに全面的に変更
        // 機能が同じなのでどちらでも動くだろうが、古いものが必要なら0.18のアーカイブを見る

        // Sun, 07 Apr 2019 14:26:58 GMT
        // nSqliteDataProvider に切り替えたついでに Thread を Task に変更
        // xRunningTaskCount を見ているところがまだ古いが、これは結果を左右しないので放置

        // Fri, 19 Apr 2019 03:00:12 GMT
        // じわじわと気になり続けたので xRunningTaskCount も廃止した

        // Fri, 27 Sep 2019 15:55:16 GMT
        // それぞれのテストの違いが記憶にないようになっていたので、メソッド名を分かりやすくした
        // 一つ目と二つ目が似ているが、一つ目は、行の追加と ExecuteScalar をそれぞれの複数のスレッドにて行い、
        // 二つ目は、まず行を一気に追加してから、読み込みにおいては全行を読み、同時に行を追加していくということをそれぞれ複数のスレッドで行う
        // そのため、一つ目では読み書きのスレッド名が混ざっていれば成功で、二つ目では、混ざりつつ、行数も増えていけば成功
        // 一つ目を書いてから CMS への同時アクセスなどを想定し、より実際の使い方に近い二つ目を書いたようで、一つ目はなくてもよさそう

        public static void TestSimultaneousWriteAndRead ()
        {
            int xErrorCount = 0;

            // Sun, 28 Oct 2018 06:59:55 GMT
            // 読みも書きも Try* でない方では無期限に待つとみなし、それで困るなら Try* を使うだけでよさそう
            // https://docs.microsoft.com/en-us/dotnet/api/system.threading.readerwriterlockslim
            using (ReaderWriterLockSlim xLocker = new ReaderWriterLockSlim ())
            {
                int xWritingTaskCount = 10,
                    xReadingTaskCount = 10,
                    xWriteCount = 100,
                    xReadCount = 100;

                nSqliteConnectionStringBuilder xBuilder = new nSqliteConnectionStringBuilder ();
                xBuilder.DataSource = nApplication.MapPath ("Test" + nSqlite.Extension);

                // Sun, 28 Oct 2018 07:02:41 GMT
                // 同時アクセスによる問題を回避する方法が SQLite 側にいろいろあるようだが、プロセスが単一ならプロセス側で対処した方が良い
                // lock を使っている人もいるが、読み込みは複数のスレッドから同時にやっても特に問題がないようなので ReaderWriterLockSlim を使う
                // SQLite 側では Write-Ahead Logging というのが今のところ最善のようだが、プロセス側でやれる限りは ReaderWriterLockSlim でカッチリいく
                // https://stackoverflow.com/questions/10325683/can-i-read-and-write-to-a-sqlite-database-concurrently-from-multiple-connections
                // https://www.sqlite.org/wal.html
                // xBuilder.JournalMode = SQLiteJournalModeEnum.Wal;

                // Mon, 29 Oct 2018 07:16:47 GMT
                // 接続のプールを使用すると速くなるが、差があまりないようなので処理の単純さを優先する
                // 1万件のデータが入っているデータベースファイルでも、空のものでも、1万回開くのにかかる時間は、
                // 接続プールなしで7秒、ありで5秒くらいで、0.7ミリ秒も0.5ミリ秒も「ミリ秒」なのでユーザーが体感しない
                // ウェブシステムの方では接続をキャッシュし、つなぎ直すということをゼロにするのも考えたが、
                // むしろ毎回つなぎ直し、例外が飛んだらその場で処理した方が障害耐性が高くなる
                // それに、つなぎっぱなしではバックアップをとるタイミングに困りそうである
                // xBuilder.Pooling = true;

                nFile.Delete (xBuilder.DataSource);
                // Sun, 28 Oct 2018 07:06:07 GMT
                // プログラムを途中で強制終了したら、このファイルが残る
                nFile.Delete (nPath.ChangeExtension (xBuilder.DataSource, nSqlite.JournalFileExtension));

                Task [] xWriters = new Task [xWritingTaskCount];

                for (int temp = 0; temp < xWritingTaskCount; temp ++)
                {
                    // Sun, 28 Oct 2018 07:07:15 GMT
                    // このタイミングでコピーしておかないと、temp ++ が行われる
                    // つまりスレッドのコード内で読む頃には他の値になっている
                    int xTaskIndex = temp;

                    xWriters [xTaskIndex] = Task.Run (() =>
                    {
                        // Sun, 28 Oct 2018 07:08:08 GMT
                        // いつの間にか、かなり使いやすい機能が追加されていた
                        // https://docs.microsoft.com/ja-jp/dotnet/csharp/language-reference/tokens/interpolated
                        string xTaskName = $"xWriters [{xTaskIndex.nToString ()}]";

                        // Sun, 28 Oct 2018 07:09:21 GMT
                        // 接続するだけでは同時アクセスの問題が起こらないはずなので毎回つなぎ直す必要はないが、
                        // このテストコードで見たいのは、「ものすごく忙しい CMS」のようなものが確実に動くかどうかである
                        // そのため、何千というユーザーが GET を繰り返すイメージで、意図的に毎回つなぎ直している

                        for (int temp1 = 0; temp1 < xWriteCount; temp1 ++)
                        {
                            xLocker.EnterWriteLock ();

                            try
                            {
                                using (nSqliteDataProvider xProvider = new nSqliteDataProvider (xBuilder.ToString ()))
                                {
                                    // Sun, 28 Oct 2018 07:12:06 GMT
                                    // autoincrement を入れないと、一度使われて消された値が再利用されるとのこと
                                    // また、integer は、設定した値に基づいて、自動的に1バイトから8バイトの幅になるらしい
                                    // https://www.sqlite.org/autoinc.html
                                    // https://stackoverflow.com/questions/8672473/is-there-type-long-in-sqlite
                                    xProvider.ExecuteNonQuery ("create table if not exists Test (Id integer primary key autoincrement, Content text)");
                                    // Sun, 28 Oct 2018 07:18:00 GMT
                                    // ' のエスケープはここでは不要だが、コードのコピペのときに忘れそうなので入れておく
                                    // 通常、書き込みは読み込みより頻度が低いため、文字列の内容に関わらずに一応エスケープするのが良い
                                    xProvider.ExecuteNonQuery ($"insert into Test (Content) values ('{nRandom.NextAscii ().nEscapeSql ()}')");
                                    Console.WriteLine (xTaskName);
                                }
                            }

                            catch
                            {
                                xErrorCount ++;
                            }

                            finally
                            {
                                xLocker.ExitWriteLock ();
                            }

                            // Sun, 28 Oct 2018 07:19:52 GMT
                            // これを行わないと、原因が ReaderWriterLockSlim なのか SQLite なのか不明だが、単一の書き込みスレッドばかり連続で処理を行う
                            // スーパーで、手に持っていた値引きの寿司をいったん戻した瞬間にもう一度つかむなら、それまで持っていた人間が一番有利というイメージでよいか
                            // 0ミリ秒の Sleep でも書き込みはスレッドがバラけるが、読み込みは100ミリ秒でも行われず、1000ミリ秒にして、やっと読み込みのスレッドも処理を行う
                            // 乱数を使っているのは、そうしないと1秒ごとに読み書きがブワーッと行われ、また1秒沈黙し……ということの繰り返しになるためである
                            Thread.Sleep (nRandom.Next (1000));
                        }
                    });
                }

                Task [] xReaders = new Task [xReadingTaskCount];

                for (int temp = 0; temp < xReadingTaskCount; temp ++)
                {
                    int xTaskIndex = temp;

                    xReaders [xTaskIndex] = Task.Run (() =>
                    {
                        string xTaskName = $"xReaders [{xTaskIndex.nToString ()}]";

                        for (int temp1 = 0; temp1 < xReadCount; temp1 ++)
                        {
                            xLocker.EnterReadLock ();

                            try
                            {
                                using (nSqliteDataProvider xProvider = new nSqliteDataProvider (xBuilder.ToString ()))
                                {
                                    // Sun, 28 Oct 2018 07:28:57 GMT
                                    // 条件を指定しない count なので、テーブルの属性情報がそのまま読まれるはず
                                    xProvider.ExecuteScalar ("select count (*) from Test");
                                    Console.WriteLine (xTaskName);
                                }
                            }

                            catch
                            {
                                xErrorCount ++;
                            }

                            finally
                            {
                                xLocker.ExitReadLock ();
                            }

                            // Sun, 28 Oct 2018 07:29:56 GMT
                            // こうでないといけない理由を書き込みの方に書いておいた
                            Thread.Sleep (nRandom.Next (1000));
                        }
                    });
                }

                List <Task> xTasks = new List <Task> ();
                xTasks.AddRange (xWriters);
                xTasks.AddRange (xReaders);
                Task.WaitAll (xTasks.ToArray ());
                xTasks.nDisposeAll ();
            }

            if (xErrorCount == 0)
                Console.WriteLine ("iSqliteTester.TestSimultaneousWriteAndRead: OK");
        }

        // Sun, 28 Oct 2018 17:17:51 GMT
        // 一つ目の Test テーブルの内容を変更せず、*Reader で複数行を読むテストコードも追加
        // しっかりロックするので同時アクセスになっていないはずだが、安定性が最優先である
        // 各部のコメントは、多くが TestSimultaneousWriteAndRead と共通なので、こちらでは省略されている

        public static void TestSimultaneousWriteAndReadLines ()
        {
            int xErrorCount = 0;

            using (ReaderWriterLockSlim xLocker = new ReaderWriterLockSlim ())
            {
                int xInitialRowCount = 10000,
                    xReadingTaskCount = 10,
                    xWritingTaskCount = 10,
                    xReadCount = 100,
                    xWriteCount = 100;

                nSqliteConnectionStringBuilder xBuilder = new nSqliteConnectionStringBuilder ();
                xBuilder.DataSource = nApplication.MapPath ("Test" + nSqlite.Extension);
                nFile.Delete (xBuilder.DataSource);
                nFile.Delete (nPath.ChangeExtension (xBuilder.DataSource, nSqlite.JournalFileExtension));

                try
                {
                    using (nSqliteDataProvider xProvider = new nSqliteDataProvider (xBuilder.ToString ()))
                    {
                        xProvider.ExecuteNonQuery ("create table if not exists Test (Id integer primary key autoincrement, Content text)");

                        // Sun, 28 Oct 2018 17:20:38 GMT
                        // SQLiteTransaction の変数を新たに作るサンプルコードをよく見るが、
                        // トランザクションは接続ごとに一つしか作られないそうなので、コマンドに関連付けると散らからない
                        // ロック一つ、接続一つ、コマンド一つを関連付けることによるシンプルな実装が分かりやすい

                        // Sun, 07 Apr 2019 12:53:32 GMT
                        // nSqliteDataProvider を書いたので上記コメントは不整合だが、一応残しておく

                        try
                        {
                            xProvider.BeginTransaction ();

                            for (int temp = 0; temp < xInitialRowCount; temp ++)
                                xProvider.ExecuteNonQuery ($"insert into Test (Content) values ('{nRandom.NextAscii ().nEscapeSql ()}')");

                            xProvider.CommitTransaction ();
                        }

                        catch
                        {
                            xProvider.RollbackTransaction ();
                            throw;
                        }
                    }
                }

                catch
                {
                    // Sun, 28 Oct 2018 17:23:50 GMT
                    // ここに到達するなら後続のコードで落ちまくるが、それでよい
                    // 後続のコードでもデータベース関連の処理は try / catch に入っている
                    xErrorCount ++;
                }

                Task [] xReaders = new Task [xReadingTaskCount];

                for (int temp = 0; temp < xReadingTaskCount; temp ++)
                {
                    int xTaskIndex = temp;

                    xReaders [xTaskIndex] = Task.Run (() =>
                    {
                        string xTaskName = $"xReaders [{xTaskIndex.nToString ()}]";

                        for (int temp1 = 0; temp1 < xReadCount; temp1 ++)
                        {
                            xLocker.EnterReadLock ();

                            try
                            {
                                using (nSqliteDataProvider xProvider = new nSqliteDataProvider (xBuilder.ToString ()))
                                {
                                    SQLiteDataReader xReader = xProvider.ExecuteReader ("select * from Test");
                                    int xReadRowCount = 0;

                                    while (xReader.Read ())
                                        // Mon, 29 Oct 2018 11:44:46 GMT
                                        // StepCount があるが、確認のため自ら数える
                                        xReadRowCount ++;

                                    // Mon, 29 Oct 2018 11:46:47 GMT
                                    // Dispose できないのに Close が必要なので注意
                                    // 忘れるとデータベースファイルのロックが解除されないまま
                                    xReader.Close ();
                                    Console.WriteLine ("{0} ({1} rows)", xTaskName, xReadRowCount.nToString ());
                                }
                            }

                            catch
                            {
                                xErrorCount ++;
                            }

                            finally
                            {
                                xLocker.ExitReadLock ();
                            }

                            Thread.Sleep (nRandom.Next (1000));
                        }
                    });
                }

                Task [] xWriters = new Task [xWritingTaskCount];

                for (int temp = 0; temp < xWritingTaskCount; temp ++)
                {
                    int xTaskIndex = temp;

                    xWriters [xTaskIndex] = Task.Run (() =>
                    {
                        string xTaskName = $"xWriters [{xTaskIndex.nToString ()}]";

                        for (int temp1 = 0; temp1 < xWriteCount; temp1 ++)
                        {
                            xLocker.EnterWriteLock ();

                            try
                            {
                                using (nSqliteDataProvider xProvider = new nSqliteDataProvider (xBuilder.ToString ()))
                                {
                                    xProvider.ExecuteNonQuery ($"insert into Test (Content) values ('{nRandom.NextAscii ().nEscapeSql ()}')");
                                    Console.WriteLine (xTaskName);
                                }
                            }

                            catch
                            {
                                xErrorCount ++;
                            }

                            finally
                            {
                                xLocker.ExitWriteLock ();
                            }

                            Thread.Sleep (nRandom.Next (1000));
                        }
                    });
                }

                List <Task> xTasks = new List <Task> ();
                xTasks.AddRange (xReaders);
                xTasks.AddRange (xWriters);
                Task.WaitAll (xTasks.ToArray ());
                xTasks.nDisposeAll ();
            }

            if (xErrorCount == 0)
                Console.WriteLine ("iSqliteTester.TestSimultaneousWriteAndReadLines: OK");
        }

        // Sun, 28 Oct 2018 18:50:08 GMT
        // データベースファイルをバックアップできるようにしたので、マルチスレッドでのテストを行う
        // といっても、制限のきつい書き込みロックをかけるため、実質的には単一スレッドに近い

        public static void TestSimultaneousBackup ()
        {
            int xErrorCount = 0;

            using (ReaderWriterLockSlim xLocker = new ReaderWriterLockSlim ())
            {
                int xInitialRowCount = 10000,
                    xTaskCount = 10,
                    xBackupCount = 10;

                nSqliteConnectionStringBuilder xBuilder = new nSqliteConnectionStringBuilder ();
                xBuilder.DataSource = nApplication.MapPath ("Test" + nSqlite.Extension);
                nFile.Delete (xBuilder.DataSource);
                nFile.Delete (nPath.ChangeExtension (xBuilder.DataSource, nSqlite.JournalFileExtension));

                try
                {
                    using (nSqliteDataProvider xProvider = new nSqliteDataProvider (xBuilder.ToString ()))
                    {
                        xProvider.ExecuteNonQuery ("create table if not exists Test (Id integer primary key autoincrement, Content text)");

                        try
                        {
                            xProvider.BeginTransaction ();

                            for (int temp = 0; temp < xInitialRowCount; temp ++)
                                xProvider.ExecuteNonQuery ($"insert into Test (Content) values ('{nRandom.NextAscii ().nEscapeSql ()}')");

                            xProvider.CommitTransaction ();
                        }

                        catch
                        {
                            xProvider.RollbackTransaction ();
                            throw;
                        }
                    }
                }

                catch
                {
                    xErrorCount ++;
                }

                // Sun, 28 Oct 2018 18:58:01 GMT
                // 正規表現で指定しやすいファイル名の三つのコピーを作成

                nFile.Copy (xBuilder.DataSource, nApplication.MapPath ("Hoge" + nSqlite.Extension), true);
                nFile.Copy (xBuilder.DataSource, nApplication.MapPath ("Moge" + nSqlite.Extension), true);
                nFile.Copy (xBuilder.DataSource, nApplication.MapPath ("Poge" + nSqlite.Extension), true);

                Task [] xTasks = new Task [xTaskCount];

                for (int temp = 0; temp < xTaskCount; temp ++)
                {
                    int xTaskIndex = temp;

                    xTasks [xTaskIndex] = Task.Run (() =>
                    {
                        string xTaskName = $"xTasks [{xTaskIndex.nToString ()}]";

                        for (int temp1 = 0; temp1 < xBackupCount; temp1 ++)
                        {
                            // Sun, 28 Oct 2018 19:01:52 GMT
                            // データベースファイルへの処理は読み込みだけだが、ZIP ファイルを作成するので書き込みロックをかける
                            // 読み込みロックでは、Ticks を使っていてもファイル名の重複が起こるようで、
                            // 10個のスレッドで各10回のバックアップでも ZIP ファイルは60個くらいしかできない
                            xLocker.EnterWriteLock ();

                            try
                            {
                                // Sun, 28 Oct 2018 19:02:33 GMT
                                // ファイル名のパターンを指定するにおいては、念のために先頭の ^ と末尾の $ を入れる
                                // nSqlite.Extension 部分が長くても、ジャーナルのファイルが入る可能性がある

                                nSqlite.Backup (nApplication.DirectoryPath, @"^[HMP]oge\" +
                                    nSqlite.Extension + '$', nApplication.MapPath ("Backups"));

                                Console.WriteLine (xTaskName);
                            }

                            catch
                            {
                                xErrorCount ++;
                            }

                            finally
                            {
                                xLocker.ExitWriteLock ();
                            }

                            // Sun, 28 Oct 2018 19:03:36 GMT
                            // バックアップだけでも、Sleep がないと特定のスレッドばかり処理を行うため、
                            // スーパーの理論は、ReaderWriterLockSlim に起因する可能性が高い
                            Thread.Sleep (nRandom.Next (1000));
                        }
                    });
                }

                Task.WaitAll (xTasks.ToArray ());
                xTasks.nDisposeAll ();
            }

            if (xErrorCount == 0)
                Console.WriteLine ("iSqliteTester.TestSimultaneousBackup: OK");
        }

        // Tue, 30 Oct 2018 07:09:20 GMT
        // データを読み出し、キャッシュされているものとのラウンドトリップをチェック
        // 動けばよいので雑な実装になっているが、処理には問題がなさそう
        // decimal, double, float のラウンドトリップがうまくいかないのは妥協
        // nSqliteCommandPartsBuilder のコメントにいろいろ書いてある

        private static void iReadAndCheckData (SQLiteDataReader reader, nSqliteCommandPartsBuilder builder)
        {
            reader.Read ();

            string [] xTypeNames = { "Bool", "Byte", "Char", "DateTime", "Decimal", "Double", "Enum", "Float",
                "Guid", "Int", "Long", "SByte", "Short", "String", "TimeSpan", "UInt", "ULong", "UShort" };

            // Tue, 30 Oct 2018 07:22:39 GMT
            // ラウンドトリップに失敗したら、黄色にしてから元の色に戻す
            ConsoleColor xForegroundColor = Console.ForegroundColor;

            foreach (string xTypeName in xTypeNames)
            {
                object xValue = builder.Dictionary [xTypeName].Item1;
                // Tue, 30 Oct 2018 07:12:38 GMT
                // object として受け取るため、Nullable つきのもの一つでテストに不足はない
                MethodInfo xMethod = typeof (nSqliteDataReaderHelper).GetMethod ("nGet" + xTypeName + "Nullable");
                object xResult;

                // Tue, 30 Oct 2018 07:13:23 GMT
                // インスタンスメソッドでない、つまり静的メソッドなら一つ目を null にする
                // Enum のときだけ、nImageFormat だと指定しないと例外が飛ぶ

                if (xTypeName != "Enum")
                    xResult = xMethod.Invoke (null, new object [] { reader, reader.GetOrdinal (xTypeName) });
                else xResult = xMethod.Invoke (null, new object [] { reader, reader.GetOrdinal (xTypeName), typeof (nImageFormat) });

                bool xEquals;

                if (xValue != null)
                    xEquals = xValue.Equals (xResult);
                else xEquals = xResult == null;

                if (xTypeName == "Char")
                {
                    // Tue, 30 Oct 2018 07:52:09 GMT
                    // char が ? になるなどで見にくいのも修正しておく

                    xValue = xValue != null ? ((char) xValue).nToUShortString () : xValue;
                    xResult = xResult != null ? ((char) xResult).nToUShortString () : xResult;
                }

                if (xTypeName == "String")
                {
                    // Tue, 30 Oct 2018 07:21:16 GMT
                    // 出力を少しだけ見やすくしておく

                    // Mon, 20 May 2019 21:17:01 GMT
                    // nToFriendlyString を追加したが、ここはこのままでよい

                    xValue = xValue != null && ((string) xValue).Length == 0 ? "(empty)" : xValue;
                    xResult = xResult != null && ((string) xResult).Length == 0 ? "(empty)" : xResult;
                }

                if (xEquals == false)
                    Console.ForegroundColor = ConsoleColor.Yellow;

                // Tue, 30 Oct 2018 07:25:15 GMT
                // ここでも null ならそのままにせずに分かりやすい表示にしている
                // == の代わりに => にした方が変化が分かりやすいが、
                // それだと不一致のときの表示に全角文字を使うことになるため却下

                Console.WriteLine ("[{0}] {1} {2} {3}", xTypeName,
                    xValue ?? "(null)", xEquals ? "==" : "!=", xResult ?? "(null)");

                if (xEquals == false)
                    Console.ForegroundColor = xForegroundColor;
            }

            Console.WriteLine ("----");
        }

        public static void TestRoundtrips ()
        {
            nSqliteConnectionStringBuilder xBuilder = new nSqliteConnectionStringBuilder ();
            xBuilder.DataSource = nApplication.MapPath ("Test" + nSqlite.Extension);
            nFile.Delete (xBuilder.DataSource);
            nFile.Delete (nPath.ChangeExtension (xBuilder.DataSource, nSqlite.JournalFileExtension));

            try
            {
                using (nSqliteDataProvider xProvider = new nSqliteDataProvider (xBuilder.ToString ()))
                {
                    // Tue, 30 Oct 2018 05:37:16 GMT
                    // 使う型を調べるときにはこの SQL を見ることが多いと思う
                    // 型指定は実際にはあってないようなもので、何も書かなくても動くようである
                    // また、どう頑張っても、decimal, double, float のラウンドトリップは不確実
                    // そのあたりについては、nSqliteCommandPartsBuilder のコメントが詳しい
                    // https://www.sqlite.org/datatype3.html

                    xProvider.ExecuteNonQuery (
                        "create table if not exists Test (" +
                        "Id integer primary key autoincrement, " +
                        "Bool integer, " +
                        "Byte integer, " +
                        "Char integer, " +
                        "DateTime integer, " +
                        "Decimal real, " +
                        "Double real, " +
                        // Tue, 30 Oct 2018 05:39:06 GMT
                        // テストには nImageFormat を使う
                        "Enum integer, " +
                        "Float real, " +
                        "Guid text, " +
                        "Int integer, " +
                        "Long integer, " +
                        "SByte integer, " +
                        "Short integer, " +
                        "String text, " +
                        "TimeSpan integer, " +
                        "UInt integer, " +
                        "ULong integer, " +
                        "UShort integer)");

                    // Tue, 30 Oct 2018 07:28:51 GMT
                    // Nullable でない値として、できるだけ最大値を入れる

                    List <nSqliteCommandPartsBuilder> xBuilders = new List <nSqliteCommandPartsBuilder> ();
                    nSqliteCommandPartsBuilder xParts = new nSqliteCommandPartsBuilder ();
                    xParts.SetValue ("Bool", true);
                    xParts.SetValue ("Byte", byte.MaxValue);
                    xParts.SetValue ("Char", char.MaxValue);
                    xParts.SetValue ("DateTime", DateTime.MaxValue);
                    xParts.SetValue ("Decimal", decimal.MaxValue);
                    xParts.SetValue ("Double", double.MaxValue);
                    xParts.SetEnum ("Enum", nImageFormat.Tiff);
                    xParts.SetValue ("Float", float.MaxValue);
                    xParts.SetValue ("Guid", nGuid.New ());
                    xParts.SetValue ("Int", int.MaxValue);
                    xParts.SetValue ("Long", long.MaxValue);
                    xParts.SetValue ("SByte", sbyte.MaxValue);
                    xParts.SetValue ("Short", short.MaxValue);
                    xParts.SetString ("String", nRandom.NextAscii ());
                    xParts.SetValue ("TimeSpan", TimeSpan.MaxValue);
                    xParts.SetValue ("UInt", uint.MaxValue);
                    xParts.SetValue ("ULong", ulong.MaxValue);
                    xParts.SetValue ("UShort", ushort.MaxValue);
                    string xCommandText = $"insert into Test ({xParts.ToFirstPartOfInsert ()}) values ({xParts.ToSecondPartOfInsert ()})";
                    Console.WriteLine (xCommandText);
                    Console.WriteLine ("----");
                    xProvider.ExecuteNonQuery (xCommandText);
                    xBuilders.Add (xParts);

                    // Tue, 30 Oct 2018 07:29:43 GMT
                    // 同じく Nullable にせずに中間的な値を入れる

                    xParts = new nSqliteCommandPartsBuilder ();
                    xParts.SetValue ("Bool", nRandom.Next () % 2 == 1);
                    xParts.SetValue ("Byte", (byte) nRandom.Next (byte.MaxValue));
                    xParts.SetValue ("Char", (char) nRandom.Next (char.MaxValue));
                    xParts.SetValue ("DateTime", new DateTime (nRandom.Next ()));
                    xParts.SetValue ("Decimal", (decimal) nRandom.NextDouble ());
                    xParts.SetValue ("Double", nRandom.NextDouble ());
                    xParts.SetEnum ("Enum", nRandom.Next (nImageFormat.Bmp.nToInt (), nImageFormat.Tiff.nToInt () + 1).nToEnum <nImageFormat> ());
                    xParts.SetValue ("Float", (float) nRandom.NextDouble ());
                    xParts.SetValue ("Guid", nGuid.New ());
                    xParts.SetValue ("Int", nRandom.Next ());
                    xParts.SetValue ("Long", (long) nRandom.Next ());
                    xParts.SetValue ("SByte", (sbyte) nRandom.Next (sbyte.MaxValue));
                    xParts.SetValue ("Short", (short) nRandom.Next (short.MaxValue));
                    xParts.SetString ("String", nRandom.NextAscii ());
                    xParts.SetValue ("TimeSpan", new TimeSpan (nRandom.Next ()));
                    xParts.SetValue ("UInt",(uint) nRandom.Next ());
                    xParts.SetValue ("ULong", (ulong) nRandom.Next ());
                    xParts.SetValue ("UShort", (ushort) nRandom.Next (ushort.MaxValue));
                    xCommandText = $"insert into Test ({xParts.ToFirstPartOfInsert ()}) values ({xParts.ToSecondPartOfInsert ()})";
                    Console.WriteLine (xCommandText);
                    Console.WriteLine ("----");
                    xProvider.ExecuteNonQuery (xCommandText);
                    xBuilders.Add (xParts);

                    // Tue, 30 Oct 2018 07:30:03 GMT
                    // 今度は Nullable で、できるだけ最小値を入れる

                    xParts = new nSqliteCommandPartsBuilder ();
                    xParts.SetValue ("Bool", (bool?) false);
                    xParts.SetValue ("Byte", (byte?) byte.MinValue);
                    xParts.SetValue ("Char", (char?) char.MinValue);
                    xParts.SetValue ("DateTime", (DateTime?) DateTime.MinValue);
                    xParts.SetValue ("Decimal", (decimal?) decimal.MinValue);
                    xParts.SetValue ("Double", (double?) double.MinValue);
                    xParts.SetEnum ("Enum", (nImageFormat?) nImageFormat.Bmp);
                    xParts.SetValue ("Float", (float?) float.MinValue);
                    xParts.SetValue ("Guid", (Guid?) Guid.Empty);
                    xParts.SetValue ("Int", (int?) int.MinValue);
                    xParts.SetValue ("Long", (long?) long.MinValue);
                    xParts.SetValue ("SByte", (sbyte?) sbyte.MinValue);
                    xParts.SetValue ("Short", (short?) short.MinValue);
                    xParts.SetString ("String", string.Empty);
                    xParts.SetValue ("TimeSpan", (TimeSpan?) TimeSpan.MinValue);
                    xParts.SetValue ("UInt", (uint?) uint.MinValue);
                    xParts.SetValue ("ULong", (ulong?) ulong.MinValue);
                    xParts.SetValue ("UShort", (ushort?) ushort.MinValue);
                    xCommandText = $"insert into Test ({xParts.ToFirstPartOfInsert ()}) values ({xParts.ToSecondPartOfInsert ()})";
                    Console.WriteLine (xCommandText);
                    Console.WriteLine ("----");
                    xProvider.ExecuteNonQuery (xCommandText);
                    xBuilders.Add (xParts);

                    // Tue, 30 Oct 2018 07:30:34 GMT
                    // 最後に Nullable で null を入れる

                    xParts = new nSqliteCommandPartsBuilder ();
                    xParts.SetValue ("Bool", (bool?) null);
                    xParts.SetValue ("Byte", (byte?) null);
                    xParts.SetValue ("Char", (char?) null);
                    xParts.SetValue ("DateTime", (DateTime?) null);
                    xParts.SetValue ("Decimal", (decimal?) null);
                    xParts.SetValue ("Double", (double?) null);
                    xParts.SetEnum ("Enum", null);
                    xParts.SetValue ("Float", (float?) null);
                    xParts.SetValue ("Guid", (Guid?) null);
                    xParts.SetValue ("Int", (int?) null);
                    xParts.SetValue ("Long", (long?) null);
                    xParts.SetValue ("SByte", (sbyte?) null);
                    xParts.SetValue ("Short", (short?) null);
                    xParts.SetString ("String", null);
                    xParts.SetValue ("TimeSpan", (TimeSpan?) null);
                    xParts.SetValue ("UInt", (uint?) null);
                    xParts.SetValue ("ULong", (ulong?) null);
                    xParts.SetValue ("UShort", (ushort?) null);
                    xCommandText = $"insert into Test ({xParts.ToFirstPartOfInsert ()}) values ({xParts.ToSecondPartOfInsert ()})";
                    Console.WriteLine (xCommandText);
                    Console.WriteLine ("----");
                    xProvider.ExecuteNonQuery (xCommandText);
                    xBuilders.Add (xParts);

                    // Tue, 30 Oct 2018 07:31:12 GMT
                    // 4件のデータをキャッシュ内のものと照合してみる
                    // object と object の比較になるが、意外とうまくいった
                    // real 系のラウンドトリップがうまくいかないのが気になっているが、
                    // これは SQLite の限界のようなので、使い方でしのぐしかない

                    xCommandText = "select * from Test";
                    SQLiteDataReader xReader = xProvider.ExecuteReader (xCommandText);
                    iReadAndCheckData (xReader, xBuilders [0]);
                    iReadAndCheckData (xReader, xBuilders [1]);
                    iReadAndCheckData (xReader, xBuilders [2]);
                    iReadAndCheckData (xReader, xBuilders [3]);
                    xReader.Close ();
                }

                Console.WriteLine ("iSqliteTester.TestRoundtrips: OK");
            }

            catch
            {
            }
        }
    }
}

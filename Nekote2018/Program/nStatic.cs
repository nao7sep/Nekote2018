using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Sun, 05 May 2019 07:12:09 GMT
    // nApplication に入れていた静的なものの多くを、新たに作ったこのクラスに移した
    // そうした理由については、たぶん長くなるので memo.txt に書いておく

    public static class nStatic
    {
        // Sun, 07 Apr 2019 15:40:51 GMT
        // 呼び出し側で lock を適切に行うならマルチスレッドで利用しても信頼性が高く、
        // いったん x* にインスタンスをつけているので、lock なしでもうまくいきそうな静的プロパティーを二つ
        // null だと判明してのインスタンスの生成と Load の途中で他のスレッドでも null だと判明したところで実害がない

        private static nSqliteConnectionStringBuilder mSqliteConnectionStringBuilder = null;

        public static nSqliteConnectionStringBuilder SqliteConnectionStringBuilder
        {
            get
            {
                if (mSqliteConnectionStringBuilder == null)
                {
                    nSqliteConnectionStringBuilder xBuilder = new nSqliteConnectionStringBuilder ();
                    xBuilder.Load (nApplication.Settings);
                    mSqliteConnectionStringBuilder = xBuilder;
                }

                return mSqliteConnectionStringBuilder;
            }
        }

        // Fri, 19 Apr 2019 02:52:50 GMT
        // コンストラクターにすぐに与えられるプロパティーも用意しておく
        // これも、理屈としてはマルチスレッドで使っても問題がないはずである
        // もっとも、マルチスレッドのプログラムならデータベース処理は lock 内なので、なおさらリスクが低い

        public static string ConnectionString
        {
            get
            {
                return SqliteConnectionStringBuilder.ToString ();
            }
        }

        // Thu, 25 Apr 2019 05:00:50 GMT
        // これを使うことで、メールもサクッと送れる
        // 決め打ちの静的インスタンスを用意するのはデザインとして妥当でないが、
        // 機能の使用頻度にも偏りがあり、あると便利なものはあっていい

        private static nSmtpSettings mSmtpSettings = null;

        public static nSmtpSettings SmtpSettings
        {
            get
            {
                if (mSmtpSettings == null)
                {
                    nSmtpSettings xSettings = new nSmtpSettings ();
                    xSettings.Load (nApplication.Settings);
                    mSmtpSettings = xSettings;
                }

                return mSmtpSettings;
            }
        }

        // Fri, 03 May 2019 11:37:14 GMT
        // 通知メールのひな形のようなものを生成するクラスの静的プロパティー
        // *.config の設定に問題がなければ、これを他と組み合わせることで通知メールを送れる
        // nSmtpSettings などを受け取り、送信まで行えるものを作る考えだったが、
        // そうやって団子にしていくと派生開発で必ず困るので、このクラスには作成のみ行わせる
        // 今後、仕様の変更が必要になれば、メールの作成において、これ以外のクラスを組み合わせたらいい

        private static nNotificationMailMessageCreator mNotificationMailMessageCreator = null;

        public static nNotificationMailMessageCreator NotificationMailMessageCreator
        {
            get
            {
                if (mNotificationMailMessageCreator == null)
                {
                    nNotificationMailMessageCreator xCreator = new nNotificationMailMessageCreator ();
                    xCreator.Load (nApplication.Settings);
                    mNotificationMailMessageCreator = xCreator;
                }

                return mNotificationMailMessageCreator;
            }
        }

        // Fri, 03 May 2019 12:05:14 GMT
        // To が空なら通知メールの機能そのものがオフになるという仕様
        // そこをいきなり思い出せないときのためにプロパティーをラップしておく

        public static bool IsNotificationMailEnabled
        {
            get
            {
                return NotificationMailMessageCreator.IsEnabled;
            }
        }

        // Fri, 03 May 2019 11:35:22 GMT
        // このパスは、決め打ちにすることに問題がないだろう
        // ローカルなら、どこに置いても見えるもので、ウェブなら、どこだろうと Web.config でアクセスが禁じられる
        // ファイル名がほぼランダムと言えるものになるし、拡張子の MIME 登録もされないため、何重もの安全策が存在する

        private static string mLogsDirectoryPath = null;

        public static string LogsDirectoryPath
        {
            get
            {
                if (mLogsDirectoryPath == null)
                    mLogsDirectoryPath = nApplication.MapPath ("Logs");

                return mLogsDirectoryPath;
            }
        }

        // Fri, 03 May 2019 12:06:01 GMT
        // Logs ディレクトリーに Ticks のファイル名でログを吐いていくクラスの静的プロパティー
        // こういうものを増やすことには懸念もあるが、以前のようにクラスそのものを静的にするのでなく、
        // ちゃんとインスタンスを作れるクラスを細かく分割した上で静的インスタンスを用意しているだけなので、
        // 使わない機能があったらオフにしたらいいし、将来的にクラスの組み合わせを変更することも可能

        private static nSimpleLogDataProvider mLogs = null;

        public static nSimpleLogDataProvider Logs
        {
            get
            {
                if (mLogs == null)
                    mLogs = new nSimpleLogDataProvider (LogsDirectoryPath);

                return mLogs;
            }
        }

        #region ログの書き込みと通知メールの送信
        // Fri, 03 May 2019 12:08:44 GMT
        // 静的プロパティーを組み合わせてログの書き込みと通知メールの送信を行うメソッドを用意しておく
        // 各所で呼ぶもので、呼び方もさまざまで、*AutoLock も用意したく、数が多くなるので、#region に入れた

        // Fri, 03 May 2019 12:27:48 GMT
        // mode を None として10のスレッドから各100回、Log を呼んでみたら、初回で「そのファイルは使用中だ」の例外が飛んだ
        // Ticks は、けっこうな桁数で出るが、細かく解像しているわけでなく、複数スレッドで生成したら容易にコリジョンを起こすようである
        // 同じ条件で LogAutoLock もテストしたが、こちらは、各1,000回、計1万回にしても難なく成功した

        // Sun, 05 May 2019 21:02:12 GMT
        // リスクのコントロールのため、*AutoLock を nAutoLock に移した

        public static void Log (nSimpleLogEntry entry, nSimpleLogNotificationMode mode = nSimpleLogNotificationMode.Errors)
        {
            // Fri, 03 May 2019 12:10:53 GMT
            // 高い確率で成功するので、まずはファイルへの書き込み
            Logs.Create (entry);

            if (IsNotificationMailEnabled &&
                entry.Type.nToInt () >= mode.nToInt ())
            {
                try
                {
                    // Fri, 03 May 2019 12:12:17 GMT
                    // エントリーを nDictionary に変換し、ユーザーフレンドリーな文字列にしたものをテキストの本文として設定
                    // 管理者のための通知メールなので HTML の本文は不要で、添付ファイルは、リスクが高まるためこのメソッドでは対応しない
                    using (nMimeMessage xMessage = NotificationMailMessageCreator.Create (nSimpleLogEntry.EntryToDictionary (entry).nToFriendlyString ()))
                    {
                        // Fri, 03 May 2019 12:14:06 GMT
                        // Create がこれを行わないのは、その後の調整を想定しての仕様
                        xMessage.BuildBody ();

                        using (nSmtpClient xClient = new nSmtpClient (SmtpSettings))
                        {
                            // Fri, 03 May 2019 12:14:33 GMT
                            // 設定に問題がなく、ネットにつながっていたら、まず落ちないが、
                            // ちょっとの設定ミスで容易に落ちるため、届いたらラッキーと考えておく
                            xClient.Send (xMessage);
                        }
                    }
                }

                catch (Exception xException)
                {
                    // Fri, 03 May 2019 12:17:03 GMT
                    // 通知メールを送れない場合、エラー扱いで、その旨を書き、例外を渡し、届かない通知メールをオフにしてログを吐く
                    // ローカルのアプリケーションなら UI でエラーに気付けるし、ウェブのものなら、サービスを利用できているなら通知メールもたいてい届く
                    // そのため、通知メールが届かなかったときに今度は SMS を……などの対策には、今すぐには開発リソースを割かないでおく
                    Log (nSimpleLogType.Error, "Failed to send a notification mail message.", xException, nSimpleLogNotificationMode.None);
                }
            }
        }

        public static void Log (nSimpleLogType type, string message, Exception exception = null, nSimpleLogNotificationMode mode = nSimpleLogNotificationMode.Errors) =>
            Log (new nSimpleLogEntry (type, message, exception), mode);

        public static void Log (nSimpleLogType type, Exception exception, nSimpleLogNotificationMode mode = nSimpleLogNotificationMode.Errors) =>
            Log (new nSimpleLogEntry (type, exception), mode);

        public static void Log (string message, Exception exception = null, nSimpleLogNotificationMode mode = nSimpleLogNotificationMode.Errors) =>
            Log (new nSimpleLogEntry (message, exception), mode);

        public static void Log (Exception exception, nSimpleLogNotificationMode mode = nSimpleLogNotificationMode.Errors) =>
            Log (new nSimpleLogEntry (exception), mode);
        #endregion

        // Sat, 04 May 2019 05:29:24 GMT
        // KVS は、データなので、デフォルトでは Data ディレクトリーのサブディレクトリーとなる
        // どこかのプロパティーに事前にパスを設定していればそれが引き継がれるような実装は、今は不要
        // 仕様を決め打ちにしていくことには強い抵抗があるが、それが開発の生産性につながるのも事実

        private static string mKvsDirectoryPath = null;

        public static string KvsDirectoryPath
        {
            get
            {
                if (mKvsDirectoryPath == null)
                    mKvsDirectoryPath = nApplication.MapPath (@"Data\Kvs");

                return mKvsDirectoryPath;
            }
        }

        // Sat, 04 May 2019 05:38:29 GMT
        // Data\Kvs にファイルを作っていく KVS もすぐに使える
        // SimpleLog と命名を揃えているので、今後も続ける

        // Mon, 06 May 2019 19:14:38 GMT
        // クラス名の単複や Simple という単語について紆余曲折があったので、memo.txt に書いておく

        private static nSimpleKvsDataProvider mKvs = null;

        public static nSimpleKvsDataProvider Kvs
        {
            get
            {
                if (mKvs == null)
                    mKvs = new nSimpleKvsDataProvider (KvsDirectoryPath);

                return mKvs;
            }
        }

        #region KVS への値の読み書き
        // Sat, 04 May 2019 05:44:13 GMT
        // データベースに依存せず、ローカルでもウェブでもすぐに使える KVS が欲しかったので実装し、メソッドも用意した
        // Contains を見ずに使えるように *Or* を用意し、*AutoLock も作ったので、変数と同じ感覚で使える
        // 1) 件数が多くても数千件ほどで、2) 書き込みより読み込みが圧倒的に多い、というところで気軽に使っていく
        // Set* と Get* しか用意しない理由については、nSimpleKvsDataProvider の方に書いておく

        // Sun, 05 May 2019 21:02:12 GMT
        // リスクのコントロールのため、*AutoLock を nAutoLock に移した

        public static void SetKvsString (string key, string value) =>
            Kvs.SetString (key, value);

        public static string GetKvsString (string key) =>
            Kvs.GetString (key);

        public static string GetKvsStringOrDefault (string key, string value) =>
            Kvs.GetStringOrDefault (key, value);

        public static string GetKvsStringOrEmpty (string key) =>
            Kvs.GetStringOrEmpty (key);

        public static string GetKvsStringOrNull (string key) =>
            Kvs.GetStringOrNull (key);
        #endregion

        // Sun, 05 May 2019 09:46:48 GMT
        // 同じことの繰り返しになっているので詳細なコメントを書かないが、
        // すぐに使えるユーザー管理の機能も静的に用意しておく
        // ログや KVS と異なり、*Entry の項目が多いため、メソッドを用意しない
        // ちゃんとインスタンスを作り、細かく中身を入れていくようなコーディングになる

        private static string mUsersDirectoryPath = null;

        public static string UsersDirectoryPath
        {
            get
            {
                if (mUsersDirectoryPath == null)
                    mUsersDirectoryPath = nApplication.MapPath (@"Data\Users");

                return mUsersDirectoryPath;
            }
        }

        private static nSimpleUserDataProvider mUsers = null;

        public static nSimpleUserDataProvider Users
        {
            get
            {
                if (mUsers == null)
                    mUsers = new nSimpleUserDataProvider (UsersDirectoryPath);

                return mUsers;
            }
        }

        // Fri, 10 May 2019 21:05:00 GMT
        // *.config の RootUserName などをロードする静的プロパティー
        // 元々は単体のメソッドだったが、デザインパターンに適合させるために変更

        private static nRootAuthentication mRootAuthentication = null;

        public static nRootAuthentication RootAuthentication
        {
            get
            {
                if (mRootAuthentication == null)
                {
                    nRootAuthentication xAuthentication = new nRootAuthentication ();
                    xAuthentication.Load (nApplication.Settings);
                    mRootAuthentication = xAuthentication;
                }

                return mRootAuthentication;
            }
        }
    }
}

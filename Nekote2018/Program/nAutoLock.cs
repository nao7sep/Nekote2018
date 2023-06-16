using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Sun, 05 May 2019 20:54:07 GMT
    // nStatic や nLiterals と同じく、リスクをコントロールするためのクラス
    // 旧来の *AutoLock がデッドロックを引き起こすリスクを懸念し、こちらに集めた
    // 移動した関係で、コメントの整合性が多少損なわれているが、分かる範囲なので放置する

    // Sun, 05 May 2019 22:14:56 GMT
    // どういったものをこのクラスに入れるか、基準を明確化しておく必要がある
    // 現時点では、1) 多くのところで使われる、2) 周囲に lock が不要な処理がない、あたりで考えている
    // そのうち2は、呼び出し側では他に lock が必要な処理がなく、よって何にも便乗できないということである
    // データベースファイルのバックアップや、存在しないランダムなパスの生成およびファイルの作成といった処理は、
    // 周囲にも lock を必要とする処理があることが多く、そちらに便乗すればいい

    public static class nAutoLock
    {
        #region ログの書き込みと通知メールの送信
        public static void Log (nSimpleLogEntry entry, nSimpleLogNotificationMode mode = nSimpleLogNotificationMode.Errors)
        {
            lock (nLocker.LocalFileSystem)
            {
                nStatic.Log (entry, mode);
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

        #region KVS への値の読み書き
        // Sat, 04 May 2019 05:50:09 GMT
        // 五つあるうち、最初の三つはそれぞれ内部的な処理が異なる
        // 四つ目と五つ目だけ三つ目を呼ぶのでは中途半端なので、
        // 全てで lock を行い、*AutoLock のついていないものを呼んだ

        public static void SetKvsString (string key, string value)
        {
            lock (nLocker.LocalFileSystem)
            {
                nStatic.SetKvsString (key, value);
            }
        }

        public static string GetKvsString (string key)
        {
            lock (nLocker.LocalFileSystem)
            {
                return nStatic.GetKvsString (key);
            }
        }

        public static string GetKvsStringOrDefault (string key, string value)
        {
            lock (nLocker.LocalFileSystem)
            {
                return nStatic.GetKvsStringOrDefault (key, value);
            }
        }

        public static string GetKvsStringOrEmpty (string key)
        {
            lock (nLocker.LocalFileSystem)
            {
                return nStatic.GetKvsStringOrEmpty (key);
            }
        }

        public static string GetKvsStringOrNull (string key)
        {
            lock (nLocker.LocalFileSystem)
            {
                return nStatic.GetKvsStringOrNull (key);
            }
        }
        #endregion

        #region ユーザー認証や種類の特定
        public static nAuthenticationResult Authenticate (string userName, string password)
        {
            // Thu, 16 May 2019 07:13:31 GMT
            // 他のメソッドを呼ぶメソッドだが、引数は、ここでも軽くチェックしておく
            // その方が後続の処理を回避できて、少しだけパフォーマンスが高まる

            if (nString.IsAnyNullOrEmpty (userName, password) ||
                    nValidator.IsUserName (userName) == false)
                return nAuthenticationResult.Unsuccessful;

            // Thu, 16 May 2019 06:10:47 GMT
            // LocalFileSystem をロックに使うのは、Users がファイルシステムベースであることを考えると妥当
            // プログラムの共通リソースにアクセスしているのだから、そういう名前のインスタンスを用意する時期だとも思うが、
            // ロックに必要なのは一つであり、ファイルシステムと全く無縁の処理にロックが必要になるまでは、ファイルシステムのものでよい
            // 簡易システムでは Root しかいないだろうから、まず Root から見て、もらった結果をそのまま返す
            // Authenticate を if に入れたらコードが短くなるが、もらった enum を保持し、条件を満たす場合のみそれを返すコーディングが望ましい
            // 最後の Unsuccessful を lock に入れないのは、「lock しての確認が済んでの後処理」というニュアンスがあるため
            // 細かいことであり、lock に入れても実害がないが、何でも lock に入れるのでは、それが癖になる
            // 厳密には、二度目の Successful も lock 外に置けるが、それは一度目とのコーディングの整合性を乱すのでやめておく
            // こういうところを少し考える習慣をつけるだけで読みやすいコードになると思う

            lock (nLocker.LocalFileSystem)
            {
                nAuthenticationResult xResult = nStatic.RootAuthentication.Authenticate (userName, password);

                if (xResult == nAuthenticationResult.Successful)
                    return xResult;

                xResult = nStatic.Users.Authenticate (userName, password);

                if (xResult == nAuthenticationResult.Successful)
                    return xResult;
            }

            return nAuthenticationResult.Unsuccessful;
        }

        public static nSimpleUserType GetUserType (string userName)
        {
            // Thu, 16 May 2019 07:15:31 GMT
            // userName が null なのは、認証されていないときに普通にあることだが、
            // 種類を見る段階でフォーマットが不正なのは明らかな問題なので例外を投げる

            if (string.IsNullOrEmpty (userName))
                return nSimpleUserType.Guest;

            if (nValidator.IsUserName (userName) == false)
                throw new nBadOperationException ();

            // Thu, 16 May 2019 06:18:03 GMT
            // Users にアクセスするので、引き続き LocalFileSystem で lock する
            // Users の方に else をつけられるが、それでは if が true のときと false のときの処理の違いが大きくなる
            // true ならこれを返し、false ならこれを返す、のように処理が類似しているなら if / else が適するが、
            // true なら抜けて、そうでないならこの処理にフォールバックする、という、
            // 「うまくいき次第、抜ける」のコーディングにおいては、不可欠でない else を省いた方が分かりやすいと感じる
            // このコーディングは、処理の流れを頭で追いやすいし、インデントが深くなりすぎない利点もある

            lock (nLocker.LocalFileSystem)
            {
                if (nStatic.RootAuthentication.IsRoot (userName))
                    return nSimpleUserType.Root;

                return nStatic.Users.GetUserType (userName);
            }
        }
        #endregion
    }
}

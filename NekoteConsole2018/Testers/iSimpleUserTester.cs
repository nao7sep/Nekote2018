using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nekote;

namespace NekoteConsole
{
    // Thu, 16 May 2019 09:21:24 GMT
    // 名前に反し、nSimpleUser* だけが対象でないが、とりあえず、ユーザー管理をザクッとテストする
    // 最初、NekoteWeb にいろいろと用意してテストしようとしたが、コンソールの方が圧倒的に楽

    internal static class iSimpleUserTester
    {
        public static void TestAuthentication ()
        {
            // Tue, 07 May 2019 21:59:51 GMT
            // EntryCount へのアクセスによって前回のデータが読み込まれないようにする
            // ちゃんと読み込まれること、その上での Create が落ちないことも確認した
            nDirectory.Delete (nStatic.UsersDirectoryPath, true);

            // Tue, 07 May 2019 22:01:13 GMT
            // データがないのを確認し、管理者と一般ユーザーを追加
            // 追加されたのをアカウント数によって確認

            Console.WriteLine ("nStatic.Users.EntryCount: " + nStatic.Users.EntryCount.nToString ());

            nStatic.Users.Create ("Administrator", new nSimpleUserEntry
            {
                IsEnabled = true,
                CreationUtc = DateTime.UtcNow,
                Type = nSimpleUserType.Administrator,
                Password = "administrator"
            });

            Console.WriteLine ("Created user: Administrator");

            nStatic.Users.Create ("User", new nSimpleUserEntry
            {
                IsEnabled = true,
                CreationUtc = DateTime.UtcNow,
                Type = nSimpleUserType.User,
                Password = "user"
            });

            Console.WriteLine ("Created user: User");

            Console.WriteLine ("nStatic.Users.EntryCount: " + nStatic.Users.EntryCount.nToString ());

            // Tue, 07 May 2019 22:02:08 GMT
            // ユーザー名の大文字・小文字が区別されず、パスワードなら区別されるのを確認
            // ユーザーの種類を取得するところでも同じことをして、最後に存在しないユーザーもチェック

            // Thu, 16 May 2019 09:28:59 GMT
            // userName が null でも GetUserType が落ちないようにしたので、それもチェック

            Console.WriteLine ("// IDs are case-insensitive while passwords are not.");
            Console.WriteLine ("nStatic.RootAuthentication.Authenticate (\"Hoge\", \"hoge\"): " + nStatic.RootAuthentication.Authenticate ("Hoge", "hoge").nToString ());
            Console.WriteLine ("nStatic.RootAuthentication.Authenticate (\"hoge\", \"hoge\"): " + nStatic.RootAuthentication.Authenticate ("hoge", "hoge").nToString ());
            Console.WriteLine ("nStatic.RootAuthentication.Authenticate (\"hoge\", \"Hoge\"): " + nStatic.RootAuthentication.Authenticate ("hoge", "Hoge").nToString ());
            Console.WriteLine ("nStatic.Users.Authenticate (\"Administrator\", \"administrator\"): " + nStatic.Users.Authenticate ("Administrator", "administrator").nToString ());
            Console.WriteLine ("nStatic.Users.Authenticate (\"administrator\", \"administrator\"): " + nStatic.Users.Authenticate ("administrator", "administrator").nToString ());
            Console.WriteLine ("nStatic.Users.Authenticate (\"administrator\", \"Administrator\"): " + nStatic.Users.Authenticate ("administrator", "Administrator").nToString ());
            Console.WriteLine ("nStatic.Users.Authenticate (\"User\", \"user\"): " + nStatic.Users.Authenticate ("User", "user").nToString ());
            Console.WriteLine ("nStatic.Users.Authenticate (\"user\", \"user\"): " + nStatic.Users.Authenticate ("user", "user").nToString ());
            Console.WriteLine ("nStatic.Users.Authenticate (\"user\", \"User\"): " + nStatic.Users.Authenticate ("user", "User").nToString ());

            Console.WriteLine ("// Nonexistent IDs and (null) must be guests.");
            Console.WriteLine ("nAutoLock.GetUserType (\"Hoge\"): " + nAutoLock.GetUserType ("Hoge").nToString ());
            Console.WriteLine ("nAutoLock.GetUserType (\"hoge\"): " + nAutoLock.GetUserType ("hoge").nToString ());
            Console.WriteLine ("nAutoLock.GetUserType (\"Administrator\"): " + nAutoLock.GetUserType ("Administrator").nToString ());
            Console.WriteLine ("nAutoLock.GetUserType (\"administrator\"): " + nAutoLock.GetUserType ("administrator").nToString ());
            Console.WriteLine ("nAutoLock.GetUserType (\"User\"): " + nAutoLock.GetUserType ("User").nToString ());
            Console.WriteLine ("nAutoLock.GetUserType (\"user\"): " + nAutoLock.GetUserType ("user").nToString ());
            Console.WriteLine ("nAutoLock.GetUserType (\"Moge\"): " + nAutoLock.GetUserType ("Moge").nToString ());
            Console.WriteLine ("nAutoLock.GetUserType (null): " + nAutoLock.GetUserType (null).nToString ());
        }
    }
}

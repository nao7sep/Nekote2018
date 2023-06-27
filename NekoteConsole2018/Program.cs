using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// よく使うものを入れたままコミット
using ImageMagick;
using Nekote;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace NekoteConsole
{
    class Program
    {
        // Fri, 27 Sep 2019 18:08:35 GMT
        // Clipboard.SetText を使うため
        [STAThread]
        static void Main (/* string [] args */)
        {
            #region 実行済みのテスト
            // iStringTableHelperTester.TestRoundtrips (); // OK
            // iSqliteTester.TestSimultaneousWriteAndRead (); // OK
            // iSqliteTester.TestSimultaneousWriteAndReadLines (); // OK
            // iSqliteTester.TestSimultaneousBackup (); // OK
            // iSqliteTester.TestRoundtrips (); // OK
            // iSimpleUserTester.TestAuthentication (); // OK
            // iDictionaryAndNameValueCollectionTester.TestRoundtrips (); // OK
            #endregion
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nekote;

namespace NekoteConsole
{
    // Fri, 31 Aug 2018 09:28:11 GMT
    // テストコードをそのときにパッと書き、走ったら消してしまうことが多いが、さすがに良くないため残す
    // クラス名や機能性（たとえば Db）を接頭辞として *Tester クラスを作り、Test, Test1, ... を入れていく
    // 結果の判断をユーザーに要求せず、メソッドが OK と言ったらそれを信じていいシンプルな作り方にする
    // データベースが用意できていませんよ、のようなメッセージも、Console.WriteLine で単純に出力してよいだろう
    // フリーズを心配せずに待てる上限として、数秒から長くても10秒くらいで処理が終わるようにパラメーターを調整

    // Fri, 27 Sep 2019 15:43:15 GMT
    // SQLite の方で何を試しているか分からなくなってきたため、メソッド名に意味をつけるようにした
    // あくまで区別のための命名であり、厳密にそれだけをテストするわけでないが、何もしないよりよほどマシ

    internal static class iStringTableHelperTester
    {
        public static void TestRoundtrips ()
        {
            int xTrialCount = 1000,
                xMaxRowCount = 100,
                xMaxFieldCount = 100,
                xMaxFieldLength = 10,
                xOkCount = 0;

            // Fri, 31 Aug 2018 09:22:52 GMT
            // CSV の処理においては、これらの文字を扱えるならテストとして足りるはず
            char [] xRelevantChars = { ' ', ',', '"', '\r', '\n', 'a', '0' };
            StringBuilder xBuilder = new StringBuilder ();

            for (int temp = 0; temp < xTrialCount; temp ++)
            {
                nStringTable xTable = new nStringTable ();
                int xRowCount = nRandom.Next (xMaxRowCount);

                for (int temp1 = 0; temp1 < xRowCount; temp1 ++)
                {
                    nStringTableRow xRow = xTable.NewRow ();
                    int xFieldCount = nRandom.Next (xMaxFieldCount);

                    for (int temp2 = 0; temp2 < xFieldCount; temp2 ++)
                    {
                        xBuilder.Clear ();
                        int xFieldLength = nRandom.Next (xMaxFieldLength);

                        for (int temp3 = 0; temp3 < xFieldLength; temp3 ++)
                            xBuilder.Append (xRelevantChars [nRandom.Next (xRelevantChars.Length)]);

                        xRow.AddField (xBuilder.ToString ());
                    }
                }

                string xCsvString = xTable.nToCsvString ();
                nStringTable xNewTable = xCsvString.nCsvToStringTable ();

                // Fri, 31 Aug 2018 09:46:10 GMT
                // この処理を行わないと、4割前後の確率で結果が不一致になる
                // これは、パラメーターが 1000, 100, 100, 10 で、文字が七つの場合である
                // Nekote の実装では、本来出力されるべきでない空行の扱いについてのみ、やや甘くしている
                // 出力データの方に問題がなければ空行が問題になることはないため、ここでも削っての比較
                xNewTable.RemoveEmptyRows ();
                xTable.RemoveEmptyRows ();

                if (xNewTable.RowCount != xTable.RowCount)
                    goto End;

                for (int temp4 = 0; temp4 < xNewTable.RowCount; temp4 ++)
                {
                    nStringTableRow xNewRow = xNewTable [temp4],
                        xRow = xTable [temp4];

                    if (xNewRow.FieldCount != xRow.FieldCount)
                        goto End;

                    for (int temp5 = 0; temp5 < xNewRow.FieldCount; temp5 ++)
                    {
                        if (xNewRow [temp5] != xRow [temp5])
                            goto End;
                    }
                }

                xOkCount ++;

            End:
                // Fri, 31 Aug 2018 09:52:05 GMT
                // 命令を置いておかないと goto 用のラベルを置けないか
                continue;
            }

            if (xOkCount == xTrialCount)
                // Fri, 31 Aug 2018 09:52:25 GMT
                // クラス名、メソッド名、OK というのを出力するのが最小限
                // successful, succeeded, failed などを吐かなくても困らない
                Console.WriteLine ("iStringTableHelperTester.TestRoundtrips: OK");
        }
    }
}

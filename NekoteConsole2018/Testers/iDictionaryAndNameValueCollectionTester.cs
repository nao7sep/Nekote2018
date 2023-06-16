using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nekote;

namespace NekoteConsole
{
    // Mon, 20 May 2019 13:22:13 GMT
    // nDictionary と nNameValueCollection のラウンドトリップのテスト
    // 今後、他にもいろいろと変換を試す可能性があるため、専用のクラスを用意

    internal static class iDictionaryAndNameValueCollectionTester
    {
        public static void TestRoundtrips ()
        {
            int xEntryCount = 10000;

            #region キーと値の生成 // OK
            // Mon, 20 May 2019 13:55:05 GMT
            // nDictionary ではキーに null を使えないので、キーには "" のみ入れる
            // 値なら問題ないので null を用意するが、"" がラウンドトリップ中に失われるため "" を省略
            // null と "" は、string.IsNullOrEmpty で見るのが普通であり、他方であることでエラーになる実装をしない
            // そのため、最初は "" を入れていたが、テストに不都合があると知っては、サクッと廃止

            // Fri, 27 Sep 2019 22:44:05 GMT
            // nRandom を強化したので、テストを厳しくするために NextString と NextAscii を使用
            // キーに NextAscii を使わないのは、使えないのが仕様で、: を検出できなくなるため

            string [] xKeys = new string [xEntryCount],
                xValues = new string [xEntryCount];

            xKeys [0] = string.Empty;
            xKeys [1] = nRandom.NextString ();

            xValues [0] = null;
            // xValues [1] = string.Empty;
            xValues [1] = nRandom.NextAscii ();

            for (int temp = 2; temp < xEntryCount; temp ++)
            {
                xKeys [temp] = nRandom.NextString ();
                xValues [temp] = nRandom.NextAscii ();
            }

            // Mon, 20 May 2019 13:57:29 GMT
            // NextWord を呼ぶだけでも重複の可能性があるが、100件、明示的に重複させる

            // Sat, 28 Sep 2019 00:13:10 GMT
            // NextString でキーを生成するようになったことで、大文字がキーに含まれるようになった
            // 結果として、ここで重複させても、ここをコメントアウトしても、新たな問題が起こるようになった
            // 詳細については、後続のラウンドトリップのところに書く

            for (int temp = 0; temp < 100; temp ++)
            {
                xKeys [101 + temp] = xKeys [1 + temp];
                xValues [102 + temp] = xValues [2 + temp];
            }

            nArray.Shuffle (xKeys);
            nArray.Shuffle (xValues);
            #endregion

            // Mon, 20 May 2019 13:58:23 GMT
            // 大文字・小文字の区別についてテストするなら overwrites も見たいが、
            // 分かりやすいコードで明確に実装できているので、問題が生じる可能性が低い

            bool [] xIgnoresCaseValues = new bool [] { false, true };

            int xBadCount = 0;

            for (int temp = 0; temp < 2; temp ++)
            {
                nDictionary xDictionary = new nDictionary (xIgnoresCaseValues [temp]);
                nNameValueCollection xCollection = new nNameValueCollection (xIgnoresCaseValues [temp]);

                for (int tempAlt = 0; tempAlt < xEntryCount; tempAlt ++)
                {
                    xDictionary.SetValue (xKeys [tempAlt], xValues [tempAlt]);
                    xCollection.SetValue (xKeys [tempAlt], xValues [tempAlt]);
                }

                // Mon, 20 May 2019 13:59:08 GMT
                // nNameValueCollection という名前が長いので便宜的に「コレクション」と表記するにおいて、
                // 「辞書 → KVP → コレクション → List → 辞書 → コレクション → KVP → 辞書 → List → コレクション」という変換を行い、
                // 文字列にしたものを比較し、一致しなければ、そのときの UTC の ticks を含む名前のファイルに、一致しなかった内容を出力

                // Mon, 20 May 2019 22:24:50 GMT
                // nNameValueCollection.nToFriendlyString の実装を変更し、値側が一つでなくても大丈夫なようにした
                // しかし、以下のコードではラウンドトリップにおいて overwrites が true であり、値が一つになるため問題ない
                // overwrites をテストするなら nToFriendlyString に頼らないテストのコードが必要になるだろう

                // Mon, 20 May 2019 21:05:21 GMT
                // . でつながるものを複数行に分けるときにどこで切るかについては、. を次の行に置く人が多いようだが、私は好まない
                // , + ? などはその行だし、行頭に . がないことで可読性が著しく低下するわけでもないため、なぜ醜くするのか

                // Sat, 28 Sep 2019 00:16:15 GMT
                // nDictionary と nNameValueCollection は、元々、大文字・小文字の比較についての初期設定が異なる
                // 前者はデフォルトで区別するが、後者はデフォルトで無視するため、ignoresCase を指定しないのではラウンドトリップがうまくいかない
                // 以前のテストで OK になっていたのは、キーを NextWord で生成していて、必ず小文字だったから
                // NextString での生成に変えたことで100％の確率で Bad になるようになり、diff をとったら相違点が毎回多かった
                // 片方がもう片方の部分文字列になっているとか、文字のエスケープができていないとかでなく、完全に異なる値の組み合わせだった
                // しばらく詰まったが、結局、キーの大文字・小文字がそれぞれの拡張メソッドにおいて区別されたり無視されたりとバラついての問題と判明
                // そのため、ignoresCase を設定しただけでサクッと成功し、その後は、どれだけテストを繰り返してもエラーが起きていない
                // 値を NextAscii で生成するように変更してもラウンドトリップが成功するため、構文解析もエスケープも大丈夫だと考えてよさそう

                nNameValueCollection xNewCollection =
                    xDictionary.nToKvpString ().nKvpToNameValueCollection (xIgnoresCaseValues [temp]).
                    nToStringList ().nToDictionary (xIgnoresCaseValues [temp]).
                    nToNameValueCollection (xIgnoresCaseValues [temp]).
                    nToKvpString ().nKvpToDictionary (xIgnoresCaseValues [temp]).
                    nToStringList ().nToNameValueCollection (xIgnoresCaseValues [temp]);

                if (xDictionary.nToFriendlyString () != xNewCollection.nToFriendlyString ())
                {
                    string xUtcString = DateTime.UtcNow.nToLongString (),
                        xDictionaryFilePath = nPath.Combine (nPath.DesktopDirectoryPath, $"Dictionary ({xUtcString}Z).txt"),
                        xCollectionFilePath = nPath.Combine (nPath.DesktopDirectoryPath, $"Collection ({xUtcString}Z).txt");

                    nFile.WriteAllText (xDictionaryFilePath, xDictionary.nToFriendlyString ());
                    nFile.WriteAllText (xCollectionFilePath, xNewCollection.nToFriendlyString ());
                    Console.WriteLine ("Bad");
                    xBadCount ++;
                }
            }

            if (xBadCount == 0)
                Console.WriteLine ("iDictionaryAndNameValueCollectionTester.TestRoundtrips: OK");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;

namespace Nekote
{
    public static class nSqlite
    {
        // Sat, 27 Oct 2018 19:14:25 GMT
        // 14年くらい 3.x のようなので、よく見る拡張子で決め打ちにしてよい
        // https://www.sqlite.org/chronology.html
        public static readonly string Extension = ".sqlite3";

        // Sun, 28 Oct 2018 06:55:59 GMT
        // テストコードにおいて不要なファイルを消すことがある
        public static readonly string JournalFileExtension = ".sqlite3-journal";

        // Sun, 28 Oct 2018 19:09:19 GMT
        // データベースファイルを ZIP ファイルとしてバックアップするメソッドを用意した
        // 多重定義の区別がつかないため、単一ファイルを対象とするものだけ *OneFile とした
        // これらの使用時には EnterReadLock を呼び出し側でかますのが良さそう
        // メソッドに locker を受け取らせることも考えたが、
        // バックアップをするかどうかの判断において上位で何らかのロックが行われるため冗長になる
        // なお、destFileNameFormat の {0} には、UtcNow.Ticks に Z をつなげたものが入る
        // また、空でないファイルが一つも見付からないときには、ZIP ファイルが作成されない

        // Sun, 05 May 2019 20:56:54 GMT
        // *AutoLock というメソッドをそれぞれに用意していたが、
        // データベースのファイルを操作するときに lock を行わないミスはあり得ず、
        // また、そういうときには他の処理も同じ lock 内で行うだろうから、
        // ファイルの圧縮のみ行う中途半端な *AutoLock をなくした
        // *AutoLock は、より完結性の高い処理のみに用意する

        public static void Backup (string [] sourceFilePaths,
            string destDirectoryPath, string destFileNameFormat = "Data ({0}).zip")
        {
            List <string> xValidPaths = new List <string> ();

            foreach (string xPath in sourceFilePaths)
            {
                if (nFile.IsNonexistentOrEmpty (xPath) == false)
                    xValidPaths.Add (xPath);
            }

            if (xValidPaths.Count > 0)
            {
                nDirectory.Create (destDirectoryPath);
                string xDestFilePath;

                // Mon, 29 Oct 2018 05:12:05 GMT
                // 最初の実装では minimal な文字列を使い、Sleep を行っていた
                // ファイル名だけで日時が分かるようにしたく、バックアップは秒未満の単位で作るものでもないためである
                // しかし、日時はタイムスタンプで分かるし、今後はデータベース内の日時も衝突回避のために Ticks にしていく
                // UI に Sleep はギリギリ分かるが、Nekote 側に Sleep はやはり避けるべきだと思う

                while (true)
                {
                    xDestFilePath = nPath.Combine (destDirectoryPath,
                        string.Format (destFileNameFormat, DateTime.UtcNow.Ticks.nToString () + 'Z'));

                    if (nPath.CanCreate (xDestFilePath))
                        break;

                    // Thread.Sleep (1000);
                }

                // Mon, 29 Oct 2018 05:09:47 GMT
                // ZipArchiveMode.Create は FileMode.CreateNew として動作するため、ファイルが既存なら IOException が飛ぶ
                // 対処も考えたが、Backup を呼ぶ側で何らかのロックを行うし、ファイル名が Ticks なので、過剰実装になりそう
                // https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.zipfile.open
                // https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.ziparchivemode
                using (ZipArchive xArchive = ZipFile.Open (xDestFilePath, ZipArchiveMode.Create))
                {
                    foreach (string xPath in xValidPaths)
                        // Sun, 28 Oct 2018 19:15:41 GMT
                        // 便利な拡張メソッドが用意されているので、それをそのまま使っている
                        // データベースファイルは、単一のディレクトリー内に単一階層で置かれるのが分かりやすいため今後そう実装する
                        // そのため、ZIP ファイルの作成時には、エントリー名にファイル名のみを使い、階層構造を落としている
                        // https://docs.microsoft.com/en-us/dotnet/api/system.io.compression.zipfileextensions.createentryfromfile
                        xArchive.CreateEntryFromFile (xPath, nPath.GetName (xPath));
                }
            }
        }

        public static void Backup (string sourceDirectoryPath, string sourceFileNamesPattern,
            string destDirectoryPath, string destFileNameFormat = "Data ({0}).zip")
        {
            if (nDirectory.Exists (sourceDirectoryPath))
            {
                List <string> xPaths = new List <string> ();

                foreach (FileInfo xFile in nDirectory.GetFiles (sourceDirectoryPath, SearchOption.TopDirectoryOnly))
                {
                    if (Regex.Match (xFile.Name, sourceFileNamesPattern,
                            RegexOptions.Compiled | RegexOptions.IgnoreCase) != Match.Empty)
                        xPaths.Add (xFile.FullName);
                }

                if (xPaths.Count > 0)
                    Backup (xPaths.ToArray (), destDirectoryPath, destFileNameFormat);
            }
        }

        // Mon, 29 Oct 2018 05:05:27 GMT
        // Firefox は複数のデータベースファイルを使用するが、
        // 小規模システムにおいては、単一ファイルで済ませることが多いと思う
        // そのたびに文字列の配列にするのもうるさいので、分かりやすいメソッドを用意

        // Sun, 31 Mar 2019 00:07:04 GMT
        // BackupOneFile という名称になっていたが、データベースファイルにアクセスするので、安全でないことに違いはない
        // そもそもどこが安全でないのかというのはきちんと考える必要があり、
        // Backup* の場合、Ticks によるファイル名の重複もあり得なくはないが、リスクはデータベースアクセスの方が格段に大きい
        // そのため、これら二つの処理を分離し、まず Ticks で出力先のファイル名のみ確定する仕様にしたとしても、圧縮のコードを安全とは見なせない
        // よって、全てまとめて単一のメソッドにしたものを *ThreadUnsafe としておく

        public static void BackupOneFile (string sourceFilePath,
                string destDirectoryPath, string destFileNameFormat = "Data ({0}).zip") =>
            Backup (new string [] { sourceFilePath }, destDirectoryPath, destFileNameFormat);
    }
}

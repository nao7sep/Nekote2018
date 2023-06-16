using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Web;

namespace Nekote
{
    // ディレクトリーやファイルの処理においては、同じようなミスをよく繰り返す
    // そのため、このクラスを使っておくことでミスが減るというラッパークラスを用意しておく
    // 多少のオーバーヘッドが生じるが、思わぬところでクラッシュするよりマシである

    // nDirectory.cs の方に詳しく書いたが、.NET の全てのメソッドをラップするわけでない
    // .NET のものを使うことに特に問題がないものは、今後もそちらをそのまま使う

    public static class nFile
    {
        // Sun, 31 Mar 2019 03:50:10 GMT
        // ファイルを小刻みに読み込んでの処理がたまにあるため、バッファーサイズをそろそろ決めておく
        // 以前は 16 MB とかにして、ゴソッと読み込むことでループの回転数を減らそうとしていたが、意味がないとのこと
        // OS がキャッシュをしているし、最近は SSD が当たり前だし、マルチスレッドもよくやるので、
        // .NET のコードを見ても決め打ちで使われている4096という値を普通に使っておく
        // https://stackoverflow.com/questions/3033771/file-i-o-with-streams-best-memory-buffer-size
        public static int BufferLength { get; set; } = 4096;

        public static bool Exists (string path)
        {
            return File.Exists (path);
        }

        // インスタンスを作る点に無駄や非効率を感じるが、
        // どこでも new FileInfo (*).Length と書くよりマシ
        public static long GetLength (string path)
        {
            // Mon, 03 Sep 2018 08:04:39 GMT
            // ファイルが存在しないなら -1 を返すことも考えたが、
            // そういう見えない仕様を埋没させ、そこに頼るようになるとあとで困る
            return new FileInfo (path).Length;
        }

        public static bool IsEmpty (string path)
        {
            return GetLength (path) == 0;
        }

        public static bool IsNonexistentOrEmpty (string path)
        {
            return Exists (path) == false || IsEmpty (path);
        }

        // ディレクトリーとファイルのいずれかのみ、存在するかどうかを調べたところで、
        // そのパスにディレクトリーまたはファイルを作れるかどうかは分からない
        // 存在するという状態にできるかでなく、作るという作業を行えるかというチェック

        public static bool CanCreate (string path)
        {
            return Directory.Exists (path) == false && File.Exists (path) == false;
        }

        // .NET のものはファイルを開いたままにするのが不便
        // すぐに閉じれば済むことだが、そういうコーディングそのものをなくしたい
        // また、ディレクトリーが存在していなくて落ちるのもよくあること
        // https://msdn.microsoft.com/en-us/library/d62kzs03.aspx

        public static void Create (string path)
        {
            // このメソッドは、「まだその名前のファイルがないならその名前は取られていない」という処理で使われることが多い
            // そのため、僅差で取られてしまったなら、遅かった方で例外が飛び、それ以降の処理が妨げられるべきである
            // 追記: メソッド名から分からない動作であり、安全性より使いにくさにつながるため、例外を投げないことにする

            // if (CanCreate (path) == false)
                // throw new nBadOperationException ();

            // 以前の実装ではいきなり File.WriteAllText を行っていたが、
            // Delete 同様、結果としてその状態にするという使い方をすることも想定し、
            // ファイルが存在しないときのみ作成の処理を行うように変更した

            if (Exists (path) == false)
            {
                nDirectory.CreateForFile (path);
                File.WriteAllText (path, string.Empty, Encoding.ASCII);
            }
        }

        // いつの間にかディレクトリーやファイルに ReadOnly がついていただけでクラッシュするようなことを避けたい
        // 他の属性も飛ばしてしまう点が乱暴だが、Windows 上の属性に依存するプログラムは少なく、実際には影響がなさそう
        // 隠されているものが見えるようになることも、Nekote によって扱う範囲内のものでは問題となりにくい

        public static void ResetAttributes (string path)
        {
            // ファイルなら、とりあえず Normal にしておけば良さそう
            // https://msdn.microsoft.com/en-us/library/system.io.fileattributes.aspx
            File.SetAttributes (path, FileAttributes.Normal);
        }

        // System.IO を using とせずにちょっとファイルシステムを操作したいときに便利
        // 一部流用が可能だが、ディレクトリー用とファイル用の両方を用意しておく

        public static FileAttributes GetAttributes (string path)
        {
            return File.GetAttributes (path);
        }

        public static void SetAttributes (string path, FileAttributes attributes)
        {
            File.SetAttributes (path, attributes);
        }

        // Nekote では UTC をメインで使っているため、
        // メソッド名に time と utc の両方を入れるのは冗長

        public static DateTime GetLastWriteUtc (string path)
        {
            return File.GetLastWriteTimeUtc (path);
        }

        public static void SetLastWriteUtc (string path, DateTime value)
        {
            File.SetLastWriteTimeUtc (path, value);
        }

        // Mon, 01 Apr 2019 10:07:11 GMT
        // MimeKit で添付ファイルをインスタンス化するにあたって Content-Type が必要なので、.NET の機能をラップ
        // 元々の引数名はファイル名だが、パスでも大丈夫のようで、
        // また、でたらめな拡張子なら application/octet-stream になるため、おそらく例外は飛ばない
        // https://docs.microsoft.com/en-us/dotnet/api/system.web.mimemapping.getmimemapping

        public static string GetMimeMapping (string path) =>
            MimeMapping.GetMimeMapping (path);

        // Sun, 31 Mar 2019 04:02:36 GMT
        // ファイルの比較や、ファイルから計算したハッシュの比較をたまにやるので、メソッドを揃えた
        // こちらは、長さが違うなら不一致、長さが同じで両方0ならファイルを開かずに一致として、
        // それら以外なら、4096をバッファーの大きさとしてループでファイル全体を比較
        // 長さが同じと分かっているなら xStream2.Read のところで長さを見る必要がないかもしれないが、
        // ファイルが壊れているとか、他のプロセスやスレッドが追記したとか、あり得なくもないため、一応は見ておく
        // エンコーディングを指定してテキストと比較するものも考えたが、
        // バイナリーでないなら高確率で小さいため、一括で読み込んだ方が分かりやすい

        public static bool Equals (string path1, string path2)
        {
            long xLength1 = GetLength (path1);

            if (xLength1 != GetLength (path2))
                return false;

            if (xLength1 == 0)
                return true;

            using (FileStream xStream1 = File.OpenRead (path1))
            using (FileStream xStream2 = File.OpenRead (path2))
            {
                byte [] xBytes1 = new byte [BufferLength],
                    xBytes2 = new byte [BufferLength];

                int xReadLength;

                while ((xReadLength = xStream1.Read (xBytes1, 0, BufferLength)) > 0)
                {
                    if (xStream2.Read (xBytes2, 0, BufferLength) != xReadLength ||
                            nArray.Equals (xBytes1, xBytes2) == false)
                        return false;
                }
            }

            return true;
        }

        public static void Move (string sourcePath, string destPath, bool overwrites = false)
        {
            // 上書きが可能かつ移動先にファイルが存在する場合、安全のため、移動先のものをいったん他のところに待避させる
            // 移動先のものを消してから移動を試みる単純な実装では、移動に失敗した場合、移動先のものが無駄に消されたことになる

            if (overwrites && Exists (destPath))
            {
                string xDestDirectoryPath = Path.GetDirectoryName (destPath),
                    xTempDestFilePath;

                while (true)
                {
                    // .tmp をつけることも選択肢だが、そうすると、消してよいものと解釈される危険性がある
                    // つけないのも不便だが、つけなければ、これは何だろうと疑問に思って調べてもらえる可能性が残る

                    // Sun, 05 May 2019 18:53:33 GMT
                    // 以前は GUID を使っていたが、長さが問題になるリスクがあった
                    // int を Base36 にしたものなら、場合によっては短いため衝突の可能性があり得るが、
                    // といっても、int.MaxValue 通りくらいの文字列が短く得られるため、メリットの方が勝る

                    // Sat, 28 Sep 2019 02:09:02 GMT
                    // SafeCode の概念を導入したため、そちらに切り替えた

                    xTempDestFilePath = Path.Combine (xDestDirectoryPath, nSafeCode.Next ());

                    if (CanCreate (xTempDestFilePath))
                        break;
                }

                ResetAttributes (destPath);
                File.Move (destPath, xTempDestFilePath);

                // 属性のリセットとファイルの移動のみ行い、
                // どこかで例外が飛べば、待避中のファイルを元に戻す
                // エラーが発生したことにかわりないため、それから例外を投げる

                try
                {
                    ResetAttributes (sourcePath);
                    File.Move (sourcePath, destPath);
                }

                catch
                {
                    File.Move (xTempDestFilePath, destPath);
                    throw;
                }

                Delete (xTempDestFilePath);
            }

            else
            {
                ResetAttributes (sourcePath);
                // こちらでは、移動先のファイルの属性をリセットしない
                // また、移動先のディレクトリーがないことがあるため作っておく
                nDirectory.CreateForFile (destPath);
                File.Move (sourcePath, destPath);
            }
        }

        public static void Copy (string sourcePath, string destPath, bool overwrites = false)
        {
            ResetAttributes (sourcePath);

            if (overwrites && Exists (destPath))
                ResetAttributes (destPath);

            nDirectory.CreateForFile (destPath);
            File.Copy (sourcePath, destPath, overwrites);
        }

        #region Read*, Write*, Append* を揃えておく // OK
        // Read*, Write*, Append* は、使用頻度が極めて高い
        // また、Write* や Append* を呼ぶときに、ディレクトリーが存在せずに落ちることがよくある
        // ディレクトリーがあるはずのときも、不正な操作などによって消えている可能性を排除できない
        // さらに、エンコーディングを指定しなければ BOM のない UTF-8 になるというのは、他との一貫性を欠く
        // そういった理由で、以下、Read*, Write*, Append* をラップしておく

        // 全てに All という表現が入ることには違和感を覚える
        // Read* は、最初から最後まで読むという意味合いで All なのも分かる
        // しかし、Write* や Append* は、範囲指定の引数がないなら、All とせずとも全ての書き込みであることが明確である
        // 実際のところ、全てに All が入っているのは、Read* には必要で、また、AppendText との名前の衝突の回避を考えてのことだろう
        // そういう事情があった中、All が必要なものにだけ All を入れるのではややこしいため、全てに入れて一貫させたのだと思う

        public static byte [] ReadAllBytes (string path)
        {
            return File.ReadAllBytes (path);
        }

        public static string ReadAllText (string path)
        {
            // Read* は、エンコーディングを指定しなければ、UTF-8 と UTF-32 の BOM を検出するとのこと
            // Write* や Append* では BOM を出力するために指定するが、こちらでは指定しないでおく
            // https://msdn.microsoft.com/en-us/library/ms143368.aspx
            return File.ReadAllText (path);
        }

        public static string ReadAllText (string path, Encoding encoding)
        {
            return File.ReadAllText (path, encoding);
        }

        public static string [] ReadAllLines (string path)
        {
            return File.ReadAllLines (path);
        }

        public static string [] ReadAllLines (string path, Encoding encoding)
        {
            return File.ReadAllLines (path, encoding);
        }

        // .NET では ReadLines だが、All の有無によって区別する命名には違和感がある
        // ここでは、Directory.EnumerateDirectories などにならうことにした

        // 追記: 以前は EnumerateAllLines という名称だったが、配列や List のものがメインで、
        // そこに追加的に IEnumerable のものも用意することが他にもあるため、追加分にあたるこちらを改名した
        // ReadAllLines as IEnumerable だったり、ReadAllLines (enumerable) だったりをペタッと潰した命名
        // 英語圏の人のコードを見ていると、必死になって as とか for とかを使って文法的な正しさを保たない命名を頻繁に目にする

        public static IEnumerable <string> ReadAllLinesEnumerable (string path)
        {
            return File.ReadLines (path);
        }

        public static IEnumerable <string> ReadAllLinesEnumerable (string path, Encoding encoding)
        {
            return File.ReadLines (path, encoding);
        }

        // ファイルへの書き込みにおいて、読み取り専用またはディレクトリーが存在しないことで落ちることがよくある
        // 毎回呼ぶと冗長な気もするメソッドだが、呼ばないとリスクが残るため、Nekote では安全性を優先する

        private static void iEnsureWritable (string path)
        {
            if (Exists (path))
                ResetAttributes (path);
            else nDirectory.CreateForFile (path);
        }

        public static void WriteAllBytes (string path, byte [] bytes)
        {
            iEnsureWritable (path);
            File.WriteAllBytes (path, bytes);
        }

        public static void WriteAllText (string path, string text)
        {
            iEnsureWritable (path);
            // Nekote では BOM つきの UTF-8 をテキストファイルのデフォルトのエンコーディングとしているが、
            // File.WriteAllText にエンコーディングを指定しないと、BOM のない UTF-8 になる
            // https://msdn.microsoft.com/en-us/library/ms143375.aspx
            File.WriteAllText (path, text, Encoding.UTF8);
        }

        public static void WriteAllText (string path, string text, Encoding encoding)
        {
            iEnsureWritable (path);
            File.WriteAllText (path, text, encoding);
        }

        public static void WriteAllLines (string path, string [] lines)
        {
            iEnsureWritable (path);
            File.WriteAllLines (path, lines, Encoding.UTF8);
        }

        public static void WriteAllLines (string path, string [] lines, Encoding encoding)
        {
            iEnsureWritable (path);
            File.WriteAllLines (path, lines, encoding);
        }

        public static void WriteAllLines (string path, IEnumerable <string> lines)
        {
            iEnsureWritable (path);
            File.WriteAllLines (path, lines, Encoding.UTF8);
        }

        public static void WriteAllLines (string path, IEnumerable <string> lines, Encoding encoding)
        {
            iEnsureWritable (path);
            File.WriteAllLines (path, lines, encoding);
        }

        // .NET には用意されていないが、使い道がないわけでもなさそうなので用意

        public static void AppendAllBytes (string path, byte [] bytes)
        {
            iEnsureWritable (path);

            using (FileStream xStream = File.OpenWrite (path))
            {
                xStream.Seek (0, SeekOrigin.End);
                xStream.Write (bytes, 0, bytes.Length);
            }
        }

        public static void AppendAllText (string path, string text)
        {
            iEnsureWritable (path);
            File.AppendAllText (path, text, Encoding.UTF8);
        }

        public static void AppendAllText (string path, string text, Encoding encoding)
        {
            iEnsureWritable (path);
            File.AppendAllText (path, text, encoding);
        }

        public static void AppendAllLines (string path, string [] lines)
        {
            iEnsureWritable (path);
            // .NET には string [] を受け取るものがないが、IEnumerable を受け取るものに通る
            // 自力で書き出すことも可能だが、ここだけ頑張ってもパフォーマンスを向上させる効果は限定的だろう
            File.AppendAllLines (path, lines, Encoding.UTF8);
        }

        public static void AppendAllLines (string path, string [] lines, Encoding encoding)
        {
            iEnsureWritable (path);
            File.AppendAllLines (path, lines, encoding);
        }

        public static void AppendAllLines (string path, IEnumerable <string> lines)
        {
            iEnsureWritable (path);
            File.AppendAllLines (path, lines, Encoding.UTF8);
        }

        public static void AppendAllLines (string path, IEnumerable <string> lines, Encoding encoding)
        {
            iEnsureWritable (path);
            File.AppendAllLines (path, lines, encoding);
        }
        #endregion

        public static void Delete (string path)
        {
            // File.Delete は、ファイルが存在しなくても落ちないが、そこまでディレクトリーが存在しないと落ちる
            // そのため、ファイルが存在するとき以外にはそもそも File.Delete を呼ばないのが分かりやすい
            // https://msdn.microsoft.com/en-us/library/system.io.file.delete.aspx

            if (Exists (path))
            {
                ResetAttributes (path);
                File.Delete (path);
            }
        }

        public static void DeleteIfEmpty (string path)
        {
            if (Exists (path) && IsEmpty (path))
                Delete (path);
        }
    }
}

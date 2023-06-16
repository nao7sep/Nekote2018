using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Tue, 25 Sep 2018 12:40:24 GMT
    // nTempFile 同様、Temp\(ProgramName) などを受け取ってその中に一時ディレクトリーを作るクラス
    // こちらも、対応するのは、ランダムなパスの確定、ディレクトリーの作成、Dispose のみであり、具体的なアクセスを補助しない
    // ただ、nPath.Combine (xDirectory.Path, ...) はさすがにうるさいため、MapPath のみ用意しておいた

    // Mon, 29 Oct 2018 06:44:28 GMT
    // ウェブシステムの開発に着手し、スレッドセーフかどうかにこだわるようになったため、
    // GetAvailablePathThreadUnsafe をコンストラクターで呼ぶのをやめ、パスを受け取る仕様に変更
    // コンストラクターに与えるパスを用意する時点で、マルチスレッドなら適切なロックをかけるべき

    // Sun, 31 Mar 2019 00:33:48 GMT
    // GetAvailablePathThreadUnsafe 付近に書いた理由により、Create* を用意したため、今後はそちらを使う
    // nTemp* は、ディレクトリーもファイルも、既に存在していても問題がないように実装されている

    // Thu, 02 May 2019 07:41:52 GMT
    // *ThreadUnsafe という命名を、memo.txt に書いた理由によってやめたが、
    // 上記のコメントについては、そのままでないと意味が通らないためそのままとする

    public class nTempDirectory: IDisposable
    {
        public string Path { get; private set; }

        public bool Exists
        {
            get
            {
                return nDirectory.Exists (Path);
            }
        }

        public bool CanCreate
        {
            get
            {
                return nDirectory.CanCreate (Path);
            }
        }

        // Tue, 25 Sep 2018 12:35:11 GMT
        // 後続の処理とのトラブルを避けるため、基本的にはすぐに作るべき
        // もっと確実にする方法もあるが、よほどのことがない限りこれで実用的のはず
        public nTempDirectory (string path, bool creates = true)
        {
            Path = path;

            if (creates)
                nDirectory.Create (Path);
        }

        public void Create ()
        {
            nDirectory.Create (Path);
        }

        public string MapPath (string path)
        {
            return nPath.Combine (Path, path);
        }

        public string MapPath (string path1, string path2)
        {
            return nPath.Combine (Path, path1, path2);
        }

        public string MapPath (string path1, string path2, string path3)
        {
            return nPath.Combine (Path, path1, path2, path3);
        }

        // Tue, 25 Sep 2018 12:35:55 GMT
        // nDirectory 同様、安全な方をデフォルトとしておく
        public void Delete (bool isRecursive = false)
        {
            nDirectory.Delete (Path, isRecursive);
        }

        public void Dispose ()
        {
            nDirectory.Delete (Path, true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Tue, 25 Sep 2018 12:37:33 GMT
    // Temp\(ProgramName) などを引数として与えてその中に指定したアルゴリズムで一時ファイルを作るクラス
    // ランダムなパスの確定、ファイルの作成、Dispose のみ責任を負うクラスで、ファイルへの具体的な処理を上位に任せる

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

    public class nTempFile: IDisposable
    {
        public string Path { get; private set; }

        public bool Exists
        {
            get
            {
                return nFile.Exists (Path);
            }
        }

        public bool CanCreate
        {
            get
            {
                return nFile.CanCreate (Path);
            }
        }

        // Tue, 25 Sep 2018 12:36:50 GMT
        // すぐにファイルを作った方が後続のコードとのトラブルを回避しやすい
        public nTempFile (string path, bool creates = true)
        {
            Path = path;

            if (creates)
                nFile.Create (Path);
        }

        public void Create ()
        {
            nFile.Create (Path);
        }

        public void Delete ()
        {
            nFile.Delete (Path);
        }

        public void Dispose ()
        {
            nFile.Delete (Path);
        }
    }
}

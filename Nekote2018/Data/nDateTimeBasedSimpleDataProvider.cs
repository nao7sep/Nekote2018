using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Nekote
{
    // Thu, 02 May 2019 09:40:19 GMT
    // nKeyBasedSimpleDataProvider とセットで「1エントリー・1ファイル」のクラスを用意した
    // 共通的なコメントが多いため、それらは先に実装したこちらに書く

    // Thu, 02 May 2019 10:04:38 GMT
    // 一応、スピードのテストもしたが、HDD のパソコンもまだ多い中、SSD でのテストであり、あまり参考にならない
    // ディスク IO が速い環境なら、イメージとしては「爆速ではないが、けっこう速い」くらいでよさそうで、
    // 遅い環境だろうと、100～1,000件くらいなら遅延をほぼ全く体感せずに使えるだろう

    // Sat, 04 May 2019 16:56:11 GMT
    // Ticks を文字列にするところで0詰めを行うべきか考えたときのことを memo.txt に書いておく
    // 結論としては、あと1,000年くらいは桁が上がらないため0詰めは不要である

    public class nDateTimeBasedSimpleDataProvider <T>
    {
        public string DirectoryPath { get; private set; }

        // Thu, 02 May 2019 09:41:57 GMT
        // モデル的なイメージで入れ物のクラスを用意し、それと nDictionary との相互変換のメソッドをラムダ式で指定する
        // いったん nDictionary にするのは、文字列にしやすいからだけでなく、nDictionary 自体が各所で容易に使い回せるため

        // Fri, 03 May 2019 02:37:53 GMT
        // Encoder だったものを TypeToDictionary に変更するなどした
        // 今後ずっと引き継ぐ仕様が新たに生じたため、memo.txt に書いておく

        public Func <T, nDictionary> TypeToDictionary { get; private set; }

        public Func <nDictionary, T> DictionaryToType { get; private set; }

        // Thu, 02 May 2019 09:43:58 GMT
        // { get; private set; } ですぐに new にしてしまう選択肢もあったが、
        // 初期化とロードのタイミングを呼び出し側で決めることもできるようにした

        private SortedDictionary <long, T> mEntries = null;

        public SortedDictionary <long, T> Entries
        {
            get
            {
                if (mEntries == null)
                    EnsureLoaded ();

                return mEntries;
            }
        }

        public int EntryCount
        {
            get
            {
                return Entries.Count;
            }
        }

        public nDateTimeBasedSimpleDataProvider (string directoryPath, Func <T, nDictionary> typeToDictionary, Func <nDictionary, T> dictionaryToType)
        {
            DirectoryPath = directoryPath;
            TypeToDictionary = typeToDictionary;
            DictionaryToType = dictionaryToType;
        }

        // Thu, 02 May 2019 10:02:47 GMT
        // 以下、マルチスレッドに対応していないメソッドが並ぶが、*ThreadUnsafe という命名を廃止したので、以下でも行っていない
        // また、スレッドセーフにしたところでプロセスセーフにはできないし、そもそもクラス全体がスレッドセーフでないなら無理をできないため、
        // memo.txt に書いた理由にもより、*AutoLock という命名のメソッドをこのクラスには用意しない

        public void EnsureLoaded ()
        {
            mEntries = new SortedDictionary <long, T> ();

            if (nDirectory.Exists (DirectoryPath))
            {
                // Thu, 02 May 2019 09:45:09 GMT
                // IIS などでも最初からアクセスが禁じられている拡張子にしておく
                // さらに Web.config などで明示的にアクセスを禁じるのが良い
                foreach (FileInfo xFile in nDirectory.GetFiles (DirectoryPath, "*.dat"))
                {
                    try
                    {
                        // Thu, 02 May 2019 09:46:50 GMT
                        // ローカルの日時を使っては環境依存になるため UTC を使うのは当然で、
                        // その場合、'Z' をつけて UTC だと示したいが、パフォーマンスに影響するためやめておく
                        // このクラスによって読み書きされるのが明確なファイルなので、Z がなくても実害は考えにくい
                        long xTicks = nPath.GetNameWithoutExtension (xFile.Name).nToLong ();
                        T xEntry = DictionaryToType (nFile.ReadAllText (xFile.FullName).nKvpToDictionary ());
                        mEntries.Add (xTicks, xEntry);
                    }

                    catch
                    {
                        // Thu, 02 May 2019 09:49:19 GMT
                        // CRUD のうち、R はメモリー上のデータだけで完結する処理で、U と D も落ちにくい
                        // インプットをいじりやすく、落ちやすいのは C だけなので、そこが問題にならないようにしている
                        // 読めないファイルがディレクトリー内に存在しても、少しの無駄な処理になる以外は問題がない
                    }
                }
            }
        }

        public bool Contains (long ticks) =>
            Entries.ContainsKey (ticks);

        public string iGetFilePath (long ticks) =>
            nPath.Combine (DirectoryPath, ticks.nToString () + ".dat");

        private void iWriteToFile (string path, T entry) =>
            nFile.WriteAllText (path, TypeToDictionary (entry).nToKvpString ());

        public long Create (T entry)
        {
            // Thu, 02 May 2019 09:51:54 GMT
            // lock に入れたらスレッドセーフになるが、プロセスセーフもできるだけ実現したく、ファイルの存在も見る
            // Ticks なので Sleep は不要で、メモリーにもファイルシステムにも同じ Ticks がない場合に書き込む
            // このメソッドは、基本的に100％成功し、キーの代わりとして Ticks を返す

            while (true)
            {
                long xTicks = DateTime.UtcNow.Ticks;
                string xFilePath = iGetFilePath (xTicks);

                if (Contains (xTicks) == false &&
                    nFile.Exists (xFilePath) == false)
                {
                    // Thu, 02 May 2019 10:00:54 GMT
                    // ファイルの存在のみで重複チェックを行う仕様とした上、
                    // Entries.Add より先にファイルへの書き込みを行うようにすれば、
                    // 書き込んでからようやく Entries の初期化が行われてのエントリーの重複が起こる
                    // 以前、ahoList をパッと書いたときにハマったことがある
                    Entries.Add (xTicks, entry);
                    iWriteToFile (xFilePath, entry);
                    return xTicks;
                }
            }
        }

        public T Read (long ticks)
        {
            if (Contains (ticks))
                return Entries [ticks];
            else return default;
        }

        // Thu, 02 May 2019 09:55:09 GMT
        // Contains を見て Create なのか Update なのかを決める使い方を想定している
        // どちらも可能な Set の用意も考えたが、CRUD は、データの整合性が大事なので、ゆるくできない
        // たとえばユーザーを Create したときにその ID が既存だったらエラーになる必要があるし、
        // Update を試みたときに既にその ID が Delete されていても、それは同じである
        // こちらは Ticks であり、キーではないが、CRUD としての厳密性は同一とする

        public bool Update (long ticks, T entry)
        {
            if (Contains (ticks))
            {
                Entries [ticks] = entry;
                iWriteToFile (iGetFilePath (ticks), entry);
                return true;
            }

            else return false;
        }

        public bool Delete (long ticks)
        {
            if (Contains (ticks))
            {
                Entries.Remove (ticks);
                nFile.Delete (iGetFilePath (ticks));
                return true;
            }

            else return false;
        }
    }
}

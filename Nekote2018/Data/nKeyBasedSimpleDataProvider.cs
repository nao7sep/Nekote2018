using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Nekote
{
    // Thu, 02 May 2019 09:41:17 GMT
    // nDateTimeBasedSimpleDataProvider とセットで「1エントリー・1ファイル」のクラスを用意した
    // 共通的なコメントが多いため、それらは nDateTimeBasedSimpleDataProvider の方に書く

    // Sat, 04 May 2019 03:52:02 GMT
    // 実装のミスで SortedDictionary になっていたが、それは nDateTimeBased* の方だけでよい
    // こちらは、キーの順序は不定であり、Create の戻り値以外では、追加したエントリーのキーが分からない

    public class nKeyBasedSimpleDataProvider <T>
    {
        public string DirectoryPath { get; private set; }

        public Func <T, nDictionary> TypeToDictionary { get; private set; }

        public Func <nDictionary, T> DictionaryToType { get; private set; }

        private Dictionary <string, T> mEntries = null;

        public Dictionary <string, T> Entries
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

        public nKeyBasedSimpleDataProvider (string directoryPath, Func <T, nDictionary> typeToDictionary, Func <nDictionary, T> dictionaryToType)
        {
            DirectoryPath = directoryPath;
            TypeToDictionary = typeToDictionary;
            DictionaryToType = dictionaryToType;
        }

        public void EnsureLoaded ()
        {
            // Thu, 02 May 2019 10:49:53 GMT
            // キーとファイル名が一対一の仕様なので、大文字・小文字についてはファイルシステムと同じ扱いになる
            mEntries = new Dictionary <string, T> (StringComparer.InvariantCultureIgnoreCase);

            if (nDirectory.Exists (DirectoryPath))
            {
                foreach (FileInfo xFile in nDirectory.GetFiles (DirectoryPath, "*.dat"))
                {
                    try
                    {
                        string xKey = nPath.GetNameWithoutExtension (xFile.Name);
                        T xEntry = DictionaryToType (nFile.ReadAllText (xFile.FullName).nKvpToDictionary ());
                        mEntries.Add (xKey, xEntry);
                    }

                    catch
                    {
                    }
                }
            }
        }

        public bool Contains (string key) =>
            Entries.ContainsKey (key);

        public string iGetFilePath (string key) =>
            nPath.Combine (DirectoryPath, key + ".dat");

        private void iWriteToFile (string path, T entry) =>
            nFile.WriteAllText (path, TypeToDictionary (entry).nToKvpString ());

        public bool Create (string key, T entry)
        {
            // Thu, 02 May 2019 11:07:04 GMT
            // nDateTimeBasedSimpleDataProvider の方は、ループを内包し、基本的に100％の成功率だが、
            // こちらは、ファイル名に使えない文字が key に入っていたら例外が飛ぶし、
            // メモリーまたはファイルシステムにおいてキーの重複が起これば、何も行われずに false が返る
            // 重複時に上書きしない理由については、nDateTimeBasedSimpleDataProvider の Update のところに書いた

            string xFilePath = iGetFilePath (key);

            if (Contains (key) == false &&
                nFile.Exists (xFilePath) == false)
            {
                Entries.Add (key, entry);
                iWriteToFile (xFilePath, entry);
                return true;
            }

            else return false;
        }

        public T Read (string key)
        {
            if (Contains (key))
                return Entries [key];
            else return default;
        }

        public bool Update (string key, T entry)
        {
            if (Contains (key))
            {
                Entries [key] = entry;
                iWriteToFile (iGetFilePath (key), entry);
                return true;
            }

            else return false;
        }

        public bool Delete (string key)
        {
            if (Contains (key))
            {
                Entries.Remove (key);
                nFile.Delete (iGetFilePath (key));
                return true;
            }

            else return false;
        }
    }
}

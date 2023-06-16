using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Mon, 06 May 2019 13:35:33 GMT
    // ジェネリッククラスに型を与え、書き込みのメソッドにも Func を与えただけのもの
    // これだけでは、どういうタイミングでどのファイルにどのくらい書き込むのかの指定が不可能だが、
    // そういったことは、クラス内に無理やり詰め込むのでなく、呼び出し側に上位メソッドを作ってまとめるべき

    public class nSimplePageAccessLogDataWriter: nQueuedDataWriter <nSimplePageAccessLogEntry>
    {
        public int WriteAsCsvToFile (string path, int maxEntryCountToWrite = int.MaxValue) =>
            WriteAsCsvToFile (path, nSimplePageAccessLogEntry.EntryToStringTableRow, maxEntryCountToWrite);

        public int AppendAsCsvToFile (string path, int maxEntryCountToWrite = int.MaxValue) =>
            AppendAsCsvToFile (path, nSimplePageAccessLogEntry.EntryToStringTableRow, maxEntryCountToWrite);
    }
}

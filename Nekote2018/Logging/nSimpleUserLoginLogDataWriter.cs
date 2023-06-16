using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Tue, 07 May 2019 18:01:11 GMT
    // コメントは nSimplePageAccessLogDataWriter と共通なので割愛

    public class nSimpleUserLoginLogDataWriter: nQueuedDataWriter <nSimpleUserLoginLogEntry>
    {
        public int WriteAsCsvToFile (string path, int maxEntryCountToWrite = int.MaxValue) =>
            WriteAsCsvToFile (path, nSimpleUserLoginLogEntry.EntryToStringTableRow, maxEntryCountToWrite);

        public int AppendAsCsvToFile (string path, int maxEntryCountToWrite = int.MaxValue) =>
            AppendAsCsvToFile (path, nSimpleUserLoginLogEntry.EntryToStringTableRow, maxEntryCountToWrite);
    }
}

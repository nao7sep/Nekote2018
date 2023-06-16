using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Tue, 01 Oct 2019 02:32:21 GMT
    // nSimplePageAccessLogEntry を CSV にするためにまず nStringTableRow にするところで使うクラス

    public static class nSimplePageAccessLogEntryCsvIndices
    {
        public static readonly int Utc = 0;

        public static readonly int UserHostName = 1;

        public static readonly int UserHostAddress = 2;

        public static readonly int UserName = 3;

        public static readonly int UserAgent = 4;

        public static readonly int UserLanguages = 5;

        public static readonly int UrlReferrer = 6;

        public static readonly int RawUrl = 7;
    }
}

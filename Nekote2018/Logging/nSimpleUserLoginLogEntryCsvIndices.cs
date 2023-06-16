using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Tue, 07 May 2019 17:19:46 GMT
    // 列の説明は nSimplePageAccessLogEntry と nSimpleUserLoginLogEntry に、
    // const にする理由は nSimplePageAccessLogEntryCsvIndices に書いた

    // Tue, 01 Oct 2019 02:34:28 GMT
    // 定数の扱い方について変更し、「定数の扱い方.txt」を書いた

    public static class nSimpleUserLoginLogEntryCsvIndices
    {
        public static readonly int Utc = 0;

        public static readonly int UserHostName = 1;

        public static readonly int UserHostAddress = 2;

        public static readonly int UserName = 3;

        public static readonly int TypedPassword = 4;

        public static readonly int Result = 5;
    }
}

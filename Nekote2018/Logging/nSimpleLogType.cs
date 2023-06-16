using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public enum nSimpleLogType
    {
        // Thu, 02 May 2019 13:20:56 GMT
        // 5～10段階の仕様が散見されるが、ここでは「どういう対処が必要か」に基づいてシンプルに三つにする
        // Info は、問題でないため何もする必要がなく、
        // Warning は、非効率や処理の続行に支障のない事象などについての対処不要の警告であり、
        // Error のみ、処理の続行に問題があるから対処が要求されるものである
        // EventLogEntryType も *Audit を除けば三つであり、合理的にできていると思う
        // int にしての比較があり得るものなので念のために値を10単位にしているが、ずっとこのままだろう
        // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.eventlogentrytype

        Info = 10,
        Warning = 20,
        Error = 30
    }
}

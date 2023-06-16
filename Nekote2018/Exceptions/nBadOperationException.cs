using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Wed, 24 Apr 2019 00:17:44 GMT
    // invalid operation では invalid だらけだったので、data を corrupt にしたのに合わせて bad に変更
    // 「やってはいけないこと」というカジュアルなニュアンスで bad という言葉を採用し、呼び出し側のミスに広く使っていく

    [Serializable]
    public class nBadOperationException: Exception
    {
        public nBadOperationException ()
        {
        }

        public nBadOperationException (string message): base (message)
        {
        }

        public nBadOperationException (string message, Exception inner): base (message, inner)
        {
        }
    }
}

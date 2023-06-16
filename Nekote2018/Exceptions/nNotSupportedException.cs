using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // .NET には NotSupportedException があるが、Nekote では以下のものを使う
    // これは、NotSupportedException と NotImplementedException を合わせたものとみなされる
    // サポートしないという判断が下っているのか、サポートしたいが時間がなくて間に合っていないのかは、
    // Nekote を使う側には重要でなく、二つの例外クラスによって区別されるほどのことでないため

    [Serializable]
    public class nNotSupportedException: Exception
    {
        public nNotSupportedException ()
        {
        }

        public nNotSupportedException (string message): base (message)
        {
        }

        public nNotSupportedException (string message, Exception inner): base (message, inner)
        {
        }
    }
}

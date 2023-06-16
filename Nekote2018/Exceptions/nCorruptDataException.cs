using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Wed, 24 Apr 2019 00:21:54 GMT
    // invalid data というと、フォーマットがおかしくてもそうだし、存在するデータが何らかの条件を満たさなくなっていてもそうで、
    // インスタンスがあって、「その段階でまだそのメソッドを呼ばないでほしい」だろうと、内部の整合性に重きを置くなら data が invalid である
    // という点において invalid data は、意味が広く、invalid operation との使い分けが曖昧で、それが仕様のゆるさにつながっていた
    // そのため、data が corrupt だと、つまり、破損しているのだと意味を狭め、各部において例外クラスの使い分けを見直した

    [Serializable]
    public class nCorruptDataException: Exception
    {
        public nCorruptDataException ()
        {
        }

        public nCorruptDataException (string message): base (message)
        {
        }

        public nCorruptDataException (string message, Exception inner): base (message, inner)
        {
        }
    }
}

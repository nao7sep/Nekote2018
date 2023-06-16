using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Wed, 24 Apr 2019 00:19:09 GMT
    // invalid data を corrupt に、invalid operation を bad に変更したが、これは invalid のまま
    // これにより、「やってはいけないことをやった」「フォーマットの解析において問題があった」「データが破損している」の区別が少し分かりやすくなった
    // そのうち「データが破損している」については、時期ごとの解釈の違いによって仕様の曖昧さがあったため、そちらのクラスにコメントをまとめておく

    [Serializable]
    public class nInvalidFormatException: Exception
    {
        public nInvalidFormatException ()
        {
        }

        public nInvalidFormatException (string message): base (message)
        {
        }

        public nInvalidFormatException (string message, Exception inner): base (message, inner)
        {
        }
    }
}

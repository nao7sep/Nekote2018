using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Thu, 25 Apr 2019 23:04:29 GMT
    // nStrings.nJoin などが複数の文字列を単一のものにするときの区切り
    // タブと改行は C 言語のエスケープの機能でいけるが、縦線は独自仕様に偏る
    // しかし、可読性が低いタブに比べて縦線の方がトラブルが少なそう

    // Fri, 26 Apr 2019 00:51:04 GMT
    // カンマ、スラッシュ、バックスラッシュなども考えたが、
    // カンマは内容によく使われるもので、スラッシュ系は区切りとして一般的でない
    // 縦線の強みは内容にほとんど使われないことで、
    // C 言語でエスケープ対象とならない文字の使用はこれだけで足りる

    public enum nStringsSeparator
    {
        Tab = 1,
        NewLine = 2,
        VerticalBar = 3
    }
}

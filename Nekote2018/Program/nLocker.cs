using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Wed, 24 Apr 2019 00:48:52 GMT
    // どういうロックが必要か明確で、Nekote にやってほしいところをやるためのインスタンスを集める
    // Nekote では、全ての lock にこのクラスが使われるため、nLocker で検索してのデバッグが容易

    public static class nLocker
    {
        // Wed, 24 Apr 2019 00:50:32 GMT
        // まだ存在しないファイル名を探すなどの、ローカルのファイルシステム関連のロックに使われるもの
        // local とするのは、LAN 上のものやリモートのものには使われないというのを明示するため
        public static object LocalFileSystem = new object ();
    }
}

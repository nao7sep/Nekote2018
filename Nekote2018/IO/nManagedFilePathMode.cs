using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Sun, 05 May 2019 19:16:33 GMT
    // 管理ファイルのパスに GUID を2回も入れるというのは、冷静に考えたら長すぎる
    // セキュリティーを考え、それ以外を深く考えずにそう実装したが、文字数が足りなくなるリスクがある
    // 6文字の Base36 では int.MaxValue に近い精度があるため、そちらでの動作をデフォルトに変更
    // これはけっこう大きな変更であり、nikoP などの既存のシステムが大きな影響を受けるため、
    // nManagedFileUtility.PathMode を変更することで、とりあえず古い動作も継続できるようにする
    // しかし、もうデフォルトは Base36 であり、戻る可能性が低いため、値はそちらが1である

    // Sat, 28 Sep 2019 02:22:32 GMT
    // Base36 を大いに気に入っていて、各所で使いまくる意気込みだったが、より良い SafeCode を思いついた
    // これは Base36 のサブセットの仕様だが、Base36 より積極的に使いたいため、ラッパークラスを別に用意した
    // なぜ SafeCode が Base36 より優れるのかについては、nSafeCode のコメントに書いておく

    public enum nManagedFilePathMode
    {
        Base36 = 1,
        Guid = 2,
        SafeCode = 3
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Sat, 04 May 2019 12:35:06 GMT
    // ユーザーの分類においては、PowerUser 的な、Administrator と User の間のものも考えていたが、サクッと削る
    // システムの設定を管理者だけに許可するなら、「設定はさわれないが、一般ユーザーにできない高度なことができる人」も定義したかった
    // しかし、1) 高度でない一般ユーザーにも使える機能だけでシステムを構成するべき、2) 設定は管理者、操作はユーザーという区別が一番シンプル、
    // という二つの点を考慮し、そもそも、1に反し、高度でないと操作できないのではシステム自体に問題があると思うので、2段階で何とかする
    // Guest は、Anonymous なユーザーがシステムを利用するにおいて自動的に得る type であり、これがあれば Nullable が不要だろう
    // なお、値を10単位にしておくのは、ログのところなどと同様、将来的に間に何か挟む可能性がゼロでないため
    // https://docs.microsoft.com/en-us/previous-versions/windows/server-essentials/gg496135%28v%3dmsdn.10%29

    // Fri, 10 May 2019 18:21:02 GMT
    // .config に1組だけユーザー名とパスワードを入れられるようにし、それも Administrator 扱いする考えだったが、
    // それでは、.config と .dat の両方に Administrator が存在することになり、メソッド名などが衝突するようになった
    // そのため、.config のものを Root とし、これだけ別格とみなすものの、権限は Administrator と同等という実装に切り替える
    // 今後もこういう変更があるとは考えにくいため、値は、10単位で連続させての40とし、IsAdministratorOrRoot のようなものを用意する

    public enum nSimpleUserType
    {
        Guest = 10,
        User = 20,
        Administrator = 30,
        Root = 40
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Tue, 23 Apr 2019 08:31:01 GMT
    // IEnumerable に対する決まりきった処理を個別のクラスにまとめていく
    // 汎用性の低いものも入るだろうが、それが自分の癖なのだろうから受け入れていく

    public static class nEnumerable
    {
        // Tue, 23 Apr 2019 08:33:52 GMT
        // List <Task> を念頭に、そういうものを一気に Dispose するメソッドを用意した
        // Task の Dispose は不要だという情報が散見されるが、存在する Dispose を呼ばないのは気持ちが悪い
        // 複数の Task を作るところでは、たいてい List に入れて ToArray で WaitAll なので、
        // ほぼ間違いなく生じる List から拡張メソッドで全ての Dispose ができたら便利
        // Dispose 済みのところに null を設定することはできないので、
        // List の Clear を呼ぶなど、呼び出し側での注意が必要

        public static void nDisposeAll <T> (this IEnumerable <T> elements) where T: IDisposable
        {
            foreach (T xElement in elements)
            {
                if (xElement != null)
                    xElement.Dispose ();
            }
        }
    }
}

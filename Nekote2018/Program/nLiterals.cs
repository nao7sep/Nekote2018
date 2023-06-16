using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Fri, 03 May 2019 07:17:40 GMT
    // MVC には、ViewBag だったと思うが、多種多様なデータをカジュアルにやり取りする仕組みがあり、非常に便利だった
    // そういうことは、仕様の不整合やセキュリティーの低下につながるため、ライブラリーの設計においては基本的には避けるべきである
    // しかし、単一のクラスに全てをまとめ、初期化のタイミングを明確化して使うなら、利益の大きさも無視できない
    // nLiterals は、そういうものを選び抜いて入れていくクラスであり、リスクを認識した上でなおコードの簡略化を図るものである

    // Sun, 05 May 2019 22:13:14 GMT
    // 最初、nShared というクラス名だったが、各部のコメントで nLiterals と誤って書いていたので、そちらに合わせた
    // 確かに、nStatic, nAutoLock に比べると、nShared というのは、共有されるのはどれも同じなのだから、一意性を欠いた

    public static class nLiterals
    {
        // Fri, 03 May 2019 07:20:57 GMT
        // nApplication.NameAndVersion の上位版のような位置付けで、そういったところに使われる
        // 通知メールを送るときも、これがあるならこれ、ないなら nApplication.NameAndVersion が件名に使われる
        // ローカルのアプリケーションにもウェブのものにも初期化のメソッドがあるため、そこで一度だけ設定するならトラブルは少ない
        public static string ApplicationTitle { get; set; }
    }
}

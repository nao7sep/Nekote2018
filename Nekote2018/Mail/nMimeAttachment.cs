using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Nekote
{
    // Mon, 01 Apr 2019 08:53:12 GMT
    // メールの添付ファイルを List で管理するにおいて Tuple を使いたくなかったので作ったクラス
    // DisplayName は、本来のファイル名でなく、メールの表示における名称だと明示する命名
    // string path によって両方に値を設定できるコンストラクターなども考えたが、シンプルさを保つ
    // DisplayName は null のままでよく、そうなら FileInfo.Name が使われる

    public class nMimeAttachment
    {
        public string DisplayName { get; set; }

        public FileInfo FileInfo { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Fri, 03 May 2019 10:03:14 GMT
    // nStatic.Log にログを渡すにおいて、通知メールはどうするかを指定するための enum
    // 指定した値とログ側の Type の値を数値的に比較しての絞り込みができるように10単位にしている
    // Errors には Only をつけることも考えたが、「だけ」のニュアンスを持つのは二つ目も同じなのでやめておいた

    // Fri, 03 May 2019 11:24:50 GMT
    // 送らないという指定も必要なので、おそらく到達しない100を設定した

    public enum nSimpleLogNotificationMode
    {
        Everything = 10,
        WarningsAndErrors = 20,
        Errors = 30,
        None = 100
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Fri, 10 May 2019 19:41:27 GMT
    // 最初、nStatic.AuthenticateAsAdmin を用意し、接頭辞などを与えられるようにしたが、
    // 「インスタンスごとにカスタマイズ可能なクラスを作り、できるだけデフォルトの設定でその静的インスタンスを nStatic に入れる」というデザインパターンに反していたため消した
    // これは、適合するクラスであり、Load などの仕様についても nSmtpSettings などと酷似するため、コメントの大部分を割愛する

    public class nRootAuthentication
    {
        // Thu, 16 May 2019 08:06:38 GMT
        // Root を使わないシステムもあるだろうから、これらが null になるのは想定内とする
        // よって、null になっていても例外を投げず、Root としての認証や照合を全面的に回避する

        public string UserName { get; private set; }

        public string Password { get; private set; }

        // Fri, 10 May 2019 19:48:30 GMT
        // 類似クラスと異なり、UserName などが private set なので、Load による設定しか行えない
        // これは仕様であり、そうでないと、呼び出し側が容易に書き換えて Authenticate を突破できてしまう

        public void Load (nNameValueCollection collection, string keyPrefix = null, string keySuffix = null)
        {
            UserName = collection.GetStringOrNull (keyPrefix + "RootUserName" + keySuffix);
            Password = collection.GetStringOrNull (keyPrefix + "RootPassword" + keySuffix);
        }

        public void Load (nDictionary dictionary, string keyPrefix = null, string keySuffix = null)
        {
            UserName = dictionary.GetStringOrNull (keyPrefix + "RootUserName" + keySuffix);
            Password = dictionary.GetStringOrNull (keyPrefix + "RootPassword" + keySuffix);
        }

        public nAuthenticationResult Authenticate (string userName, string password)
        {
            // Thu, 16 May 2019 08:13:29 GMT
            // => で書けるが、うるさくなるので敢えて二つの if 文に分けた
            // 一つ目では、Root として認めるわけにはいかない条件を全てチェック
            // IsUserName が false になるのは、この段階では「任意入力のチェック」なので例外を投げない
            // 二つ目の if 文では、大文字・小文字の比較を区別しながら照合を行って結果を返す

            if (nString.IsAnyNullOrEmpty (UserName, Password, userName, password) ||
                    nValidator.IsUserName (userName) == false)
                return nAuthenticationResult.Unsuccessful;

            if (nString.Compare (userName, UserName, true) == 0 &&
                    nString.Compare (password, Password, false) == 0)
                return nAuthenticationResult.Successful;
            else return nAuthenticationResult.Unsuccessful;
        }

        public bool IsRoot (string userName)
        {
            // Thu, 16 May 2019 08:07:45 GMT
            // UserName が null なのは想定内で、userName が null なら認証されていない
            // どちらの場合も Root でないことに違いはないため、false を返す
            // 続いて、userName のフォーマットを確認し、問題があれば例外を投げる
            // セキュリティー関連のコードなので、このチェックは、うるさいくらい各所で行う
            // フォーマットに問題があっても、UserName が null なら例外が飛ばないが、
            // だからこそ各所でチェックをするわけで、どうせどこかで飛ぶ

            if (nString.IsAnyNullOrEmpty (UserName, userName))
                return false;

            if (nValidator.IsUserName (userName) == false)
                throw new nBadOperationException ();

            return nString.Compare (userName, UserName, true) == 0;
        }
    }
}

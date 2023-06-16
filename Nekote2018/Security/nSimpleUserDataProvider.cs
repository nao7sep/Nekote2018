using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Sat, 04 May 2019 12:45:15 GMT
    // Data\Users などにファイルとしてユーザー情報を入れて CRUD でアクセスするためのクラス
    // nSimpleLogDataProvider, nSimpleKvsDataProvider などと共通点が多いため、コメントの一部を省略

    public class nSimpleUserDataProvider: nKeyBasedSimpleDataProvider <nSimpleUserEntry>
    {
        public nSimpleUserDataProvider (string directoryPath): base (directoryPath, nSimpleUserEntry.EntryToDictionary, nSimpleUserEntry.DictionaryToEntry)
        {
        }

        public nAuthenticationResult Authenticate (string userName, string password)
        {
            // Thu, 16 May 2019 08:27:03 GMT
            // nAutoLock.Authenticate と同様、null や不正なフォーマットなら前半で返す
            // キーが存在する場合にパスワードが null などでないかチェックすることについては、迷いもある
            // そういう値設定を呼び出し側で防ぐ前提なので、そもそもパスワードが null などになることが考えにくいが、
            // Root の方では UserName と Password の両方について null チェックを行っているため、一応、それに整合させた
            // セキュリティーの向上につながるわけではないが、入れておくと、実装例として今後の参考になりそうだとは思う

            if (nString.IsAnyNullOrEmpty (userName, password) ||
                    nValidator.IsUserName (userName) == false)
                return nAuthenticationResult.Unsuccessful;

            if (Contains (userName))
            {
                nSimpleUserEntry xEntry = Read (userName);

                if (string.IsNullOrEmpty (xEntry.Password) == false &&
                        nString.Compare (password, xEntry.Password, false) == 0)
                    return nAuthenticationResult.Successful;
            }

            return nAuthenticationResult.Unsuccessful;
        }

        // Fri, 10 May 2019 22:20:04 GMT
        // これを最初は nStatic に入れたが、明らかに間違っていた
        // ちょっと静的なプロパティーを用意したら、すぐに関数型言語のプログラミングをする
        // データと処理をひとまとめにするオブジェクト指向の基本すら忘れていた

        public nSimpleUserType GetUserType (string userName)
        {
            // Thu, 16 May 2019 08:23:02 GMT
            // nAutoLock.GetUserType と同様、null などなら Guest を最初に返し、
            // そうでなく、フォーマットが不正なら、この段階でそうなるのは問題なので例外を投げる
            // その後は、キーが存在するなら Type を返し、どこにも引っ掛からないなら Guest とみなす

            if (string.IsNullOrEmpty (userName))
                return nSimpleUserType.Guest;

            if (nValidator.IsUserName (userName) == false)
                throw new nBadOperationException ();

            if (Contains (userName))
                return Read (userName).Type;

            return nSimpleUserType.Guest;
        }
    }
}

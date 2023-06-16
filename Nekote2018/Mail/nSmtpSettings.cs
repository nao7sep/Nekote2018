using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nSmtpSettings
    {
        // Mon, 01 Apr 2019 05:35:59 GMT
        // いわゆる「オレオレ証明書」に使われる
        // http://www.mimekit.net/docs/html/P_MailKit_MailService_ServerCertificateValidationCallback.htm
        public bool RequiresServerCertificateValidation { get; set; } = true;

        public bool RequiresSsl { get; set; } = true;

        // Mon, 01 Apr 2019 05:36:47 GMT
        // localhost でテストすることや、サーバーなら実運用も localhost のことが多い
        // 初期値を与えないことも考えたが、たいていこれで動くのだから入れてよいと思う
        public string Host { get; set; } = "localhost";

        // Mon, 01 Apr 2019 05:37:23 GMT
        // ポートの方も、たいていこれなので初期値として入れておく
        // https://www.mailgun.com/blog/which-smtp-port-understanding-ports-25-465-587
        public int Port { get; set; } = 587;

        public bool RequiresAuthentication { get; set; } = true;

        public string UserName { get; set; }

        public string Password { get; set; }

        // Mon, 01 Apr 2019 05:43:02 GMT
        // *.config や KVP ファイルからデータを読めるようにし、書き込みもついでに実装
        // ファイルからとか、文字列からとかは、面倒でない実装の無駄なラッパーになるためやめておく
        // 最初、get / set の名称で考えていたが、それではプロパティーのそれらと意味が逆転するため却下し、
        // 読み込むということ、保存を目的として書き込むということが分かりやすいメソッド名にした

        // Sat, 06 Apr 2019 23:26:15 GMT
        // LoadData / SaveData という命名にしていたが、目的語を入れる理由が見えなかった
        // 目的語は、それがないとメソッドの動作が分からなかったり、他との区別が必要だったりのときのもの

        // Sun, 07 Apr 2019 15:12:01 GMT
        // nSqliteConnectionStringBuilder の実装に伴い、こちらも仕様を見直した
        // Save の必要性が感じられなかったので消し、Load の方も、"Smtp" をキーの本体に含めた
        // 接頭辞としてアカウント名を、接尾辞として添え字を指定するような想定であり、"Smtp" は固定でよい

        public void Load (nNameValueCollection collection, string keyPrefix = null, string keySuffix = null)
        {
            RequiresServerCertificateValidation = collection.GetBoolOrDefault (keyPrefix + "SmtpRequiresServerCertificateValidation" + keySuffix, true);
            RequiresSsl = collection.GetBoolOrDefault (keyPrefix + "SmtpRequiresSsl" + keySuffix, true);
            Host = collection.GetStringOrDefault (keyPrefix + "SmtpHost" + keySuffix, "localhost");
            Port = collection.GetIntOrDefault (keyPrefix + "SmtpPort" + keySuffix, 587);
            RequiresAuthentication = collection.GetBoolOrDefault (keyPrefix + "SmtpRequiresAuthentication" + keySuffix, true);
            UserName = collection.GetStringOrNull (keyPrefix + "SmtpUserName" + keySuffix);
            Password = collection.GetStringOrNull (keyPrefix + "SmtpPassword" + keySuffix);
        }

        public void Load (nDictionary dictionary, string keyPrefix = null, string keySuffix = null)
        {
            RequiresServerCertificateValidation = dictionary.GetBoolOrDefault (keyPrefix + "SmtpRequiresServerCertificateValidation" + keySuffix, true);
            RequiresSsl = dictionary.GetBoolOrDefault (keyPrefix + "SmtpRequiresSsl" + keySuffix, true);
            Host = dictionary.GetStringOrDefault (keyPrefix + "SmtpHost" + keySuffix, "localhost");
            Port = dictionary.GetIntOrDefault (keyPrefix + "SmtpPort" + keySuffix, 587);
            RequiresAuthentication = dictionary.GetBoolOrDefault (keyPrefix + "SmtpRequiresAuthentication" + keySuffix, true);
            UserName = dictionary.GetStringOrNull (keyPrefix + "SmtpUserName" + keySuffix);
            Password = dictionary.GetStringOrNull (keyPrefix + "SmtpPassword" + keySuffix);
        }
    }
}

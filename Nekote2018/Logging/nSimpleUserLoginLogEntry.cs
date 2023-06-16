using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Tue, 07 May 2019 17:52:31 GMT
    // nSimplePageAccessLogEntry の項目のうち、ログインの試みを追跡するのに必要なものを引き継ぐ
    // 入力されたパスワードの保存については、1) セキュリティーが低下する恐れ、2) むしろセキュリティーを強化できる可能性、の比較で悩んだが、
    // 入力内容が分かっていてこそ、ハッキングのためのリストに基づくものなのか、悪意のない間違いなのか、何らかのプログラムに古い設定が残っているのか、などが分かる
    // 一応、パスワードでなく「入力された」パスワードだということが明確な名前にしている
    // 結果については、マルかバツかなので bool で考えていたが、それでは true / false が CSV に入り、拡張性が失われるため、enum にした

    public class nSimpleUserLoginLogEntry
    {
        public DateTime Utc { get; set; }

        public string UserHostName { get; set; }

        public string UserHostAddress { get; set; }

        public string UserName { get; set; }

        public string TypedPassword { get; set; }

        public nAuthenticationResult Result { get; set; }

        // Tue, 07 May 2019 18:04:03 GMT
        // ここもコメントが nSimplePageAccessLogEntry と共通なので割愛
        // 後半で xRow.EnsureFieldExists を呼ばないのも同じ

        public static readonly Func <nSimpleUserLoginLogEntry, nStringTableRow> EntryToStringTableRow = (entry) =>
        {
            nStringTableRow xRow = new nStringTableRow ();
            xRow.SetDateTime (nSimpleUserLoginLogEntryCsvIndices.Utc, entry.Utc); // 必須

            if (string.IsNullOrEmpty (entry.UserHostName) == false)
                xRow.SetString (nSimpleUserLoginLogEntryCsvIndices.UserHostName, entry.UserHostName); // オプション

            if (string.IsNullOrEmpty (entry.UserHostAddress) == false)
                xRow.SetString (nSimpleUserLoginLogEntryCsvIndices.UserHostAddress, entry.UserHostAddress); // オプション

            if (string.IsNullOrEmpty (entry.UserName) == false)
                xRow.SetString (nSimpleUserLoginLogEntryCsvIndices.UserName, entry.UserName); // オプション

            if (string.IsNullOrEmpty (entry.TypedPassword) == false)
                xRow.SetString (nSimpleUserLoginLogEntryCsvIndices.TypedPassword, entry.TypedPassword); // オプション

            xRow.SetEnum (nSimpleUserLoginLogEntryCsvIndices.Result, entry.Result); // 必須

            return xRow;
        };
    }
}

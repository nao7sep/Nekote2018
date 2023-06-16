using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nSimpleUserEntry
    {
        // Sat, 04 May 2019 17:14:38 GMT
        // ユーザーに関連付けられる情報は、ユーザー管理の機能性を直接的に左右するため、取捨選択が難しい
        // 以下では、まず、オン・オフを切り替えるフラグを最初に置く
        // これがオフならそれ以降の全ての情報は不要である
        // モバイルでも、何かをオフにしたことでそれ以降の全てが非表示になることは多い
        // 続いて、日時に関連するものを一気に並べる
        // 一部を Nullable にするのは、更新やログインがまだなら「まだ」ということを明示したいため
        // nSimpleKvsEntry とも共通する仕様なので、そちらのコメントも参照
        // 続いて、Type により、管理者なのか一般ユーザーなのかの二択
        // Guest は、Anonymous のユーザーに自動的に設定される属性であり、このクラスで扱うことはない
        // ここまでの「オン・オフ」「日時関連」「種類や属性」という順序を今後のスキーマでも可能な限り引き継ぐ
        // UserName は nSimpleUserDataProvider でキーとなるため、*Entry には入れないでおく
        // Microsoft は、こういうときに入れるが、同じデータに複数の方法でアクセスできるとコードが甘くなる
        // 次の Password は、ハッシュで保存するべきものだろうが、「パスワードを教えて」のメールを想定し、平文で入れる
        // IT に疎い人は本当にものすごく疎く、パスワード再設定用のページへのリンクをメールで送り付けるようなことは野暮である
        // *.dat へのアクセスが IIS の設定で禁じられていて、レンタルサーバー業者が信頼できるなら、パスワードが無防備というわけでもない
        // メアドは、メインのもの一つと予備的なもの複数という組み合わせで考えていたが、設定を煩雑にしてはトラブルになるためシンプルにした
        // 「できればここに送ってほしいが、そちらに届かないときだけこちらに」のようなことを言われても、「無理」で押し通していい
        // 表示名は、メアドほど重要でなく、どちらかと言うと、次のコメントに近い補足的な情報なのでメアドの次に置く
        // コメントは、表示名より設定頻度が低く、空のことが多いだろうから、表示名の次とする
        // 以上に収まらない情報を AdditionalInfo に自由なキーで収める
        // データベースによらない KVS をデフォルトで用意し、ログにもユーザー情報にも nDictionary を入れるのは、
        // プログラム固有でない、そういった機能に最大限の汎用性を与え、Nekote をどこでもすぐに使えるようにするためである
        // このクラスの設計においてボツにしたプロパティーとしては、IsApproved, LastPasswordChangedDate, LastLockoutDate あたりを挙げておく
        // 登録申請があったものを管理者が承認するのか、メールのリンクをクリックしてもらっての承認なのかは不明だが、
        // システムを操作するユーザーの管理においては、そこまでする必要はない
        // 基本的に全員が社員であり、システム管理者には全員の情報が正確に把握されている
        // パスワードに有効期限を設けるとか、古くなってきたから変更するように言うとかも必要性が乏しく、
        // 一人か二人のシステム管理者が数名から数十名のユーザーを管理するだけなら、仕様のシンプルさを優先していい
        // ログインに一定時間内に何度かミスってのロックアウトも、アカウントをロックするより、IP アドレスをブロックするべきと思うためイマイチ
        // アカウントをロックするには UserName が一致する必要があるが、ロックしてしまうと、その UserName の存在を確認するのと同じ
        // ありそうな UserName を短時間に10回くらい試すだけでまず UserName を確認できるのは、攻撃者のメリットになる
        // 繰り返しになるが、アカウントのロックより、まずは IP アドレスをブロックし、DoS や DDoS への耐性をつけるべきである
        // といった、今回実装しないプロパティーがどうしても必要なら、AdditionalInfo を使うことで何とでもなる
        // https://docs.microsoft.com/en-us/windows/desktop/cimwin32prov/win32-useraccount
        // https://docs.microsoft.com/ja-jp/dotnet/api/system.web.security.activedirectorymembershipuser
        // https://docs.microsoft.com/en-us/previous-versions/windows/server-essentials/gg514748%28v%3dmsdn.10%29

        // Sat, 04 May 2019 17:46:27 GMT
        // LoginCount というのも元々は考えていたが、そもそもログインの回数を見て何になるのか不明なので却下
        // 簡易的なセキュリティーに必要なのは最後のログイン日時であり、勘の鋭い人は、それで不正アクセスに気付ける
        // そうでない人のためには、アクセスログとログインのログの両方を用意し、
        // IP アドレスのエリアが大きく変わったときに警告メッセージを表示するようなことが選択肢
        // そちらの仕様についても考え始めているため、特に役に立たない LoginCount は不要

        // Sun, 05 May 2019 05:31:47 GMT
        // LoginCount のようなものは、スレッドのメッセージ数や、カテゴリーのタスク数といったものの表示には役に立つ
        // 別のテーブルで行をカウントすれば得られる情報でも、毎回カウントするのではコストが大きいため、キャッシュに利益がある
        // しかし、ログインについては、誰が何回ログインしたかを表に入れる利益がなく、絶対にそうしないのだから、やはり不要

        // Sun, 05 May 2019 06:14:43 GMT
        // 必須なのかオプションなのか書き忘れていた
        // null にできないものは全て必須で、Nullable は全てオプションで、
        // 文字列は、DisplayName と Comment のみオプションで、
        // メアドは、通知が不要ならオプション扱いでも問題がないため呼び出し側に任せ、
        // AdditionalInfo は、その役割上、当然オプションである
        // オプションのものは、吐くものがないならキーそのものを出力しない

        public bool IsEnabled { get; set; }

        public DateTime CreationUtc { get; set; }

        public DateTime? LastUpdateUtc { get; set; }

        public DateTime? LastLoginUtc { get; set; }

        public nSimpleUserType Type { get; set; }

        public string Password { get; set; }

        public List <string> MailAddresses { get; private set; } = new List <string> ();

        public string DisplayName { get; set; }

        public string Comment { get; set; }

        public nDictionary AdditionalInfo { get; private set; } = new nDictionary ();

        // Sun, 05 May 2019 06:27:33 GMT
        // 類似クラスである nSimpleLogEntry に、以下に関連するコメントがある
        // キーに接頭辞をつける理由などは、そちらを参照

        public static readonly Func <nSimpleUserEntry, nDictionary> EntryToDictionary = (entry) =>
        {
            nDictionary xDictionary = new nDictionary ();
            xDictionary.SetBool ("IsEnabled", entry.IsEnabled); // 必須
            xDictionary.SetDateTime ("CreationUtc", entry.CreationUtc); // 必須

            if (entry.LastUpdateUtc != null)
                xDictionary.SetDateTime ("LastUpdateUtc", entry.LastUpdateUtc.Value); // オプション

            if (entry.LastLoginUtc != null)
                xDictionary.SetDateTime ("LastLoginUtc", entry.LastLoginUtc.Value); // オプション

            xDictionary.SetEnum ("Type", entry.Type); // 必須
            xDictionary.SetString ("Password", entry.Password); // 必須

            if (entry.MailAddresses.Count > 0)
                xDictionary.SetString ("MailAddresses", entry.MailAddresses.nJoin (nStringsSeparator.VerticalBar)); // オプション

            if (string.IsNullOrEmpty (entry.DisplayName) == false)
                xDictionary.SetString ("DisplayName", entry.DisplayName); // オプション

            if (string.IsNullOrEmpty (entry.Comment) == false)
                xDictionary.SetString ("Comment", entry.Comment); // オプション

            foreach (var xPair in entry.AdditionalInfo)
                xDictionary.SetString ("AdditionalInfo_" + xPair.Key, xPair.Value); // オプション

            return xDictionary;
        };

        public static readonly Func <nDictionary, nSimpleUserEntry> DictionaryToEntry = (dictionary) =>
        {
            nSimpleUserEntry xEntry = new nSimpleUserEntry ();
            xEntry.IsEnabled = dictionary.GetBool ("IsEnabled"); // 必須
            xEntry.CreationUtc = dictionary.GetDateTime ("CreationUtc"); // 必須

            if (dictionary.ContainsKey ("LastUpdateUtc"))
                xEntry.LastUpdateUtc = dictionary.GetDateTime ("LastUpdateUtc"); // オプション

            if (dictionary.ContainsKey ("LastLoginUtc"))
                xEntry.LastLoginUtc = dictionary.GetDateTime ("LastLoginUtc"); // オプション

            xEntry.Type = dictionary.GetEnum <nSimpleUserType> ("Type"); // 必須
            xEntry.Password = dictionary.GetString ("Password"); // 必須

            if (dictionary.ContainsKey ("MailAddresses"))
                xEntry.MailAddresses.AddRange (dictionary.GetString ("MailAddresses").nSplit (nStringsSeparator.VerticalBar)); // オプション

            xEntry.DisplayName = dictionary.GetStringOrNull ("DisplayName"); // オプション
            xEntry.Comment = dictionary.GetStringOrNull ("Comment"); // オプション

            foreach (var xPair in dictionary)
            {
                if (xPair.Key.nStartsWith ("AdditionalInfo_"))
                {
                    string xKey = xPair.Key.Substring ("AdditionalInfo_".Length);

                    if (string.IsNullOrEmpty (xKey) == false)
                        xEntry.AdditionalInfo.SetString (xKey, xPair.Value); // オプション
                }
            }

            return xEntry;
        };

        // Sun, 05 May 2019 06:41:29 GMT
        // ログや KVS と異なり、項目が多く、ごく一部を自動的に設定することの利益が乏しいため、コンストラクターを用意しない
        // 入力画面では、インスタンスを用意し、それぞれのコントロールの内容を見ながら値を細かく設定していくことになる
    }
}

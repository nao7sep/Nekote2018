using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MimeKit;
using MimeKit.Utils;
using System.IO;

namespace Nekote
{
    // Mon, 01 Apr 2019 10:48:43 GMT
    // MimeKit の MimeMessage は、高機能だが、UTF-8 以外のメールを送りにくく、また、添付ファイルをつけるのが面倒
    // 特に後者がややこしく、これでいいのか、何か見落としていないか、といったことを毎回考えているため、Nekote でラップしておく

    // Mon, 01 Apr 2019 12:12:05 GMT
    // CloseAttachmentsStreams の呼び忘れを防ぐために IDisposable にしておく

    public class nMimeMessage: IDisposable
    {
        // Mon, 01 Apr 2019 10:50:34 GMT
        // 各部のエンコーディングをそれぞれ異ならせるニーズを想定できないため、一つで全てに設定できるようにする
        // MimeKit でも UTF-8 がデフォルトであり、各部で決め打ちになっているため、今の時代、デフォルトはそれでいい

        // Mon, 01 Apr 2019 11:59:57 GMT
        // GetEncoding (932) で Shift-JIS を取得して送ったところ、Thunderbird では豪快に文字化けし、Shuriken では読めた
        // Shuriken では最初から読めて、Thunderbird では、エンコーディングを指定しても本文しか読めず、残りは化けたままだった
        // 生成されたソースを見る限り、MimeKit に問題はなく、Thunderbird がそこまで読まないだけのようなのでこれでいい

        public Encoding Encoding { get; private set; } = Encoding.UTF8;

        // Mon, 01 Apr 2019 10:51:59 GMT
        // プロパティーに直接設定できるようにすると整合性が乱れる
        // そのため、以下、主要な項目に Set* を用意しておく

        public void SetEncoding (Encoding encoding)
        {
            Encoding = encoding;
            IsBodyBuilt = false;
        }

        public MimeMessage Message { get; private set; } = new MimeMessage ();

        // Mon, 01 Apr 2019 10:53:51 GMT
        // 実装において参考にしたページのうち、まだブラウザーで開かれているものをザッとまとめておく
        // ドキュメントがマメに用意されているが、それでも細部までカバーしているわけでなく、
        // 実装においては、かなり多く部分についてコードを読む必要があった
        // https://www.atmarkit.co.jp/ait/articles/1811/21/news023.html
        // https://www.sukerou.com/2018/10/vb-c-mailkit.html
        // http://www.mimekit.net/docs/html/R_Project_Documentation.htm
        // https://github.com/jstedfast/MimeKit/blob/master/MimeKit/BodyBuilder.cs
        // https://github.com/jstedfast/MimeKit/blob/master/MimeKit/ContentType.cs
        // https://github.com/jstedfast/MimeKit/blob/master/MimeKit/InternetAddress.cs
        // https://github.com/jstedfast/MimeKit/blob/master/MimeKit/MailboxAddress.cs
        // https://github.com/jstedfast/MimeKit/blob/master/MimeKit/MimeContent.cs
        // https://github.com/jstedfast/MimeKit/blob/master/MimeKit/MimeEntity.cs
        // https://github.com/jstedfast/MimeKit/blob/master/MimeKit/MimeMessage.cs
        // https://github.com/jstedfast/MimeKit/blob/master/MimeKit/MimePart.cs
        // https://github.com/jstedfast/MimeKit/blob/master/MimeKit/TextPart.cs
        // http://www.mimekit.net/docs/html/Creating-Messages.htm

        // Mon, 01 Apr 2019 11:03:00 GMT
        // 自分が使うのは、from, to, subject, body, attachments くらいなので、
        // from / to 系は全てカバーしつつ、ヘッダー系、属性系をサクッと Message プロパティーに任せる
        // 省略したのは、具体的には、Date, Headers, Importance, InReplyTo, MessageId, MimeVersion, Priority, XPriority と思う
        // Sender なども私は今まで使ったことがないので省略を考えたが、頻度で考えると迷うため、「メアド系」と考えたものはとりあえず全て残した

        public void SetSender (string address, string name = null) =>
            Message.Sender = new MailboxAddress (Encoding, name, address);

        public void AddFrom (string address, string name = null) =>
            Message.From.Add (new MailboxAddress (Encoding, name, address));

        public void AddReplyTo (string address, string name = null) =>
            Message.ReplyTo.Add (new MailboxAddress (Encoding, name, address));

        public void AddTo (string address, string name = null) =>
            Message.To.Add (new MailboxAddress (Encoding, name, address));

        public void AddCc (string address, string name = null) =>
            Message.Cc.Add (new MailboxAddress (Encoding, name, address));

        public void AddBcc (string address, string name = null) =>
            Message.Bcc.Add (new MailboxAddress (Encoding, name, address));

        public void SetSubject (string subject) =>
            // Mon, 01 Apr 2019 11:07:24 GMT
            // Subject に直接設定するのではエンコーディングを指定できない
            // この方法でも内部的に同期されるようで、Subject で件名を取得できる
            Message.Headers.Replace (HeaderId.Subject, Encoding, subject);

        // Mon, 01 Apr 2019 11:11:51 GMT
        // alternative, mixed など、いろいろ関わってくるため、
        // プロパティーにデータを集めて最後に BuildBody でまとめる仕様にした

        public string TextBody { get; private set; }

        public void SetTextBody (string body)
        {
            TextBody = body;
            IsBodyBuilt = false;
        }

        public string HtmlBody { get; private set; }

        public void SetHtmlBody (string body)
        {
            HtmlBody = body;
            IsBodyBuilt = false;
        }

        public List <nMimeAttachment> Attachments { get; private set; } = new List <nMimeAttachment> ();

        public void AddAttachment (string path, string displayName = null)
        {
            // Mon, 01 Apr 2019 11:22:22 GMT
            // 存在チェック、拡張子の正しさ、DisplayName がファイル名としてどうかなどは、
            // このメソッドがチェック・対処することではない

            Attachments.Add (new nMimeAttachment
            {
                DisplayName = displayName,
                FileInfo = new FileInfo (path)
            });

            IsBodyBuilt = false;
        }

        // Mon, 01 Apr 2019 11:38:35 GMT
        // MimeContent のコンストラクターで指定する FileStream を MimeKit が閉じるのを期待したが、
        // 試してみたところ、全て開きっぱなしにしてくれていたので、List に入れていき、最後にユーザーに閉じてもらう
        // private にすることも考えたが、様子見が必要なところで、外部から見えることに特にリスクもない

        public List <FileStream> AttachmentsStreams { get; private set; } = new List <FileStream> ();

        private MimePart iToMimeKitAttachment (nMimeAttachment attachment)
        {
            // MimeKit が使いにくいのは、添付ファイルをインスタンス化するところがややこしいから
            // 以下、暫定的なコードを書いてみたが、これでいいのかどうかは、まだまだ分からない
            // http://www.mimekit.net/docs/html/Creating-Messages.htm

            FileStream xStream = attachment.FileInfo.OpenRead ();
            AttachmentsStreams.Add (xStream);

            // Mon, 01 Apr 2019 11:43:27 GMT
            // サンプルコードでは MimePart に二つの引数を指定しているが、
            // あとで / でつなげているコードがあるため、つながっているものを渡しても問題ないだろう
            // DisplayName があるならそちらを使うのは、そちらの方がユーザーの関与度が高く、より正確と見込めるため
            // 存在するファイルが JPEG なのに拡張子が PNG になっているから JPEG のファイル名で……のようなことがあり得る

            return new MimePart (nFile.GetMimeMapping (
                string.IsNullOrEmpty (attachment.DisplayName) == false ?
                attachment.DisplayName : attachment.FileInfo.Name))
            {
                Content = new MimeContent (xStream),
                ContentDisposition = new ContentDisposition (ContentDisposition.Attachment),

                // Mon, 01 Apr 2019 11:45:35 GMT
                // サンプルコードでは、添付ファイルには設定していないが、HTML の方では設定している
                // また、他者のコードを見ると設定していることが多いため、こちらでも設定しておく

                // Fri, 05 Apr 2019 04:27:00 GMT
                // テキストにいらないのは確実として、HTML や添付ファイルに Content-ID がいるのかいらないのかについては、よく分からない
                // inline の画像を <img src="cid:..." /> として HTML で表示するのでないなら不要と言う人がいる一方、
                // RFC やそれをベースとするページでは、HTML 間のリンク、区別、キャッシングなどのために必要とされているようである部分がある
                // HTML の方で必要なのは、HTML から HTML にリンクするときのように限られるようでもあって、情報が錯綜していると感じる
                // 現実的には、あっても不要なら無視されるが、必要なのに存在しなければトラブルになり得ると考えてよい
                // そのため、間違いなく不要なテキスト以外では、「あって損はない」という考え方でつけておく
                // https://serverfault.com/questions/398962/does-the-presence-of-a-content-id-header-in-an-email-mime-mean-that-the-attachme
                // https://tools.ietf.org/html/rfc2045
                // https://tools.ietf.org/html/rfc2046

                ContentId = MimeUtils.GenerateMessageId (),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = string.IsNullOrEmpty (attachment.DisplayName) == false ?
                    attachment.DisplayName : attachment.FileInfo.Name // ,
                // Fri, 05 Apr 2019 04:03:38 GMT
                // Content-Disposition の方が明確に attachment になっているため、設定する意味がなさそう
                // はっきりと「不要」とするドキュメントが見当たらないが、サンプルコードが設定していないため大丈夫だろう
                // IsAttachment = true
            };
        }

        // Wed, 24 Apr 2019 00:31:24 GMT
        // メールを送ってみて、本文が空で、何がおかしいのか探すようなことが何度かあった
        // BuildBody の呼び忘れだとすぐに思い出せずに困ることがあるため、フラグで例外を投げる
        // すぐに気付けるうっかりミスだが、気付けるまでの労力が無駄になるのはやはり避けたい

        public bool IsBodyBuilt { get; private set; } = false;

        // Mon, 01 Apr 2019 11:48:51 GMT
        // これを呼ばないと、本文の組み立てが行われない
        // 呼び忘れているとメールがおかしくなって必ず気付くため、自動化をしない
        // これを自動化するために AddAttachment などのたびに呼び出すと処理の重複が大きい

        // Thu, 26 Sep 2019 12:17:03 GMT
        // nSmtpClient で自動的に呼ぶことを改めて考えたが、
        // それで分かるのは「少なくとも一度呼ばれたかどうか」であり、
        // 「最後にメールの内容を更新したあとに呼ばれたか」までは分からない
        // そして、Message を公開しているため、あらゆる更新を捕捉することはできない

        public void BuildBody ()
        {
            // Mon, 01 Apr 2019 11:50:30 GMT
            // 平文と HTML の両方があれば、alternative に両方を入れて、alternative を body に設定
            // 片方しかないなら body にそれを設定
            // 平文には ContentId が不要で、サンプルコードを見る限り、HTML には、あった方が良さそう
            // SetText を使うのは、Text への直接設定では UTF-8 が決め打ちになっているから

            MimeEntity xBody = null;
            MultipartAlternative xAlternative = null;

            if (string.IsNullOrEmpty (TextBody) == false &&
                string.IsNullOrEmpty (HtmlBody) == false)
            {
                xAlternative = new MultipartAlternative ();
                xBody = xAlternative;
            }

            if (string.IsNullOrEmpty (TextBody) == false)
            {
                TextPart xPart = new TextPart ("plain");
                xPart.SetText (Encoding, TextBody);

                if (xAlternative != null)
                    xAlternative.Add (xPart);
                else xBody = xPart;
            }

            if (string.IsNullOrEmpty (HtmlBody) == false)
            {
                TextPart xPart = new TextPart ("html");
                // Fri, 19 Apr 2019 02:45:15 GMT
                // 詳しくは iToMimeKitAttachment のコメントに書いたが、
                // 要不要がよく分からないものなので、「あって損はない」の考えでつけておく
                xPart.ContentId = MimeUtils.GenerateMessageId ();
                xPart.SetText (Encoding, HtmlBody);

                if (xAlternative != null)
                    xAlternative.Add (xPart);
                else xBody = xPart;
            }

            if (Attachments.Count > 0)
            {
                // Mon, 01 Apr 2019 11:53:38 GMT
                // body が null、つまり、平文も HTML もなく、おまけに添付ファイルが一つだけなら、
                // 平文または HTML のどちらかだけあるときと同様、body に直接それだけを設定
                // それ以外の場合、1) テキスト系＋添付ファイル、2) 二つ以上の添付ファイル、のいずれであろうと、
                // 直接設定はできず、テキスト系だけでないので alternative にも入らず、mixed が確定
                // そのときに前半で body に何か入っていたら、それを mixed に取り込む
                // 続いて添付ファイルを全て mixed に入れ、最後に mixed を body に設定して終わり

                if (xBody == null && Attachments.Count == 1)
                    xBody = iToMimeKitAttachment (Attachments [0]);

                else
                {
                    Multipart xParts = new Multipart ("mixed");

                    if (xBody != null)
                        xParts.Add (xBody);

                    foreach (nMimeAttachment xAttachment in Attachments)
                        xParts.Add (iToMimeKitAttachment (xAttachment));

                    xBody = xParts;
                }
            }

            // Mon, 01 Apr 2019 11:57:44 GMT
            // テキスト系または添付ファイルが一つでもあれば、body は null にならない
            // null なら何もないということなので、サンプルコードにならって "" を設定しておく
            Message.Body = xBody ?? new TextPart ("plain") { Text = string.Empty };
            IsBodyBuilt = true;
        }

        // Mon, 01 Apr 2019 11:59:38 GMT
        // SMTP のログを読む方では、Encoding プロパティーのようなものがないためデフォルト値を UTF-8 としたが、
        // こちらは、指定したエンコーディングで送信されるし、Encoding を利用できるため、それで読んでいる
        // 実際に試してみたら、SMTP のログの方は、転送時にさらに Base64 でエンコードされていて、全体が7ビットになっていた
        // そのため、Shift-JIS で送ったメッセージは、ソースでは日本語に戻り、ログでは Base64 で表示された

        public string GetEntireSource (Encoding encoding = null)
        {
            using (MemoryStream xStream = new MemoryStream ())
            {
                Message.WriteTo (xStream);
                return (encoding != null ? encoding : Encoding).GetString (xStream.ToArray ());
            }
        }

        // Fri, 05 Apr 2019 04:45:18 GMT
        // 添付ファイルの方で FileStream を開きっぱなすので WriteTo もチェックしたが、そちらは大丈夫だった
        // それでも、メール全体をバイナリーのまま取得する機能は存在するべきと思うので、とりあえず実装しておく

        public byte [] GetEntireSourceInBinary ()
        {
            using (MemoryStream xStream = new MemoryStream ())
            {
                Message.WriteTo (xStream);
                return xStream.ToArray ();
            }
        }

        // Fri, 05 Apr 2019 04:52:42 GMT
        // ClearTo がないと、結局 MimeKit を NuGet で入れることになる
        // どうせ Nekote の構成の一部として DLL ファイルが入るが、簡単なプログラミングなら手間を惜しみたい
        // sender のみ、リストになっていなくて、ClearSender を用意すると仕様について誤解を招くことになるためやめておく

        public void ClearFrom () =>
            Message.From.Clear ();

        public void ClearReplyTo () =>
            Message.ReplyTo.Clear ();

        public void ClearTo () =>
            Message.To.Clear ();

        public void ClearCc () =>
            Message.Cc.Clear ();

        public void ClearBcc () =>
            Message.Bcc.Clear ();

        public void CloseAttachmentsStreams ()
        {
            foreach (FileStream xStream in AttachmentsStreams)
            {
                xStream.Close ();
                xStream.Dispose ();
            }

            AttachmentsStreams.Clear ();
        }

        // Fri, 05 Apr 2019 04:55:16 GMT
        // Attachments が公開されているためこちらは不要とも思えるが、
        // リストを空にするメソッドを他に用意したので、一貫性があった方が使いやすい
        // 念のためにストリームを閉じるが、Body の再構築を自動的に行うことはやめておく
        // それは、Set*Body でもやっていないことであり、無駄が大きいため

        public void ClearAttachments ()
        {
            Attachments.Clear ();
            CloseAttachmentsStreams ();
            IsBodyBuilt = false;
        }

        public void Dispose () =>
            // Mon, 01 Apr 2019 12:10:04 GMT
            // これだけでも、複数回実行しても問題ない
            CloseAttachmentsStreams ();
    }
}

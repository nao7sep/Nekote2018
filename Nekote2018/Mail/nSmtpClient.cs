using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using System.IO;

namespace Nekote
{
    // Mon, 01 Apr 2019 06:55:38 GMT
    // MailKit の SmtpClient は極めて良くできているが、ちょっとメールを送るだけでコードが長くなる
    // そのため、1) 現状最小限で、2) 非同期もいったん見送り、3) とにかく短いコードで使えるラッパーを作っておく

    public class nSmtpClient: IDisposable
    {
        public nSmtpSettings Settings { get; private set; }

        // Mon, 01 Apr 2019 06:57:09 GMT
        // Stream だけでは分かりにくいので、これだけ修飾しておく
        public MemoryStream LoggingStream { get; private set; }

        // Mon, 01 Apr 2019 06:57:32 GMT
        // 外部で使うことがなさそうだが、他に合わせて一応公開
        public ProtocolLogger Logger { get; private set; }

        public SmtpClient Client { get; private set; }

        public nSmtpClient (nSmtpSettings settings, bool connectsAndAuthenticates = true)
        {
            Settings = settings;
            LoggingStream = new MemoryStream ();
            Logger = new ProtocolLogger (LoggingStream);
            Client = new SmtpClient (Logger);

            if (Settings.RequiresServerCertificateValidation == false)
                // Mon, 01 Apr 2019 06:58:11 GMT
                // 引数名が元々のものの頭文字になっていないためこうする理由がよく分からないが、とりあえずこれで「オレオレ証明書」を使える
                // http://www.mimekit.net/docs/html/P_MailKit_MailService_ServerCertificateValidationCallback.htm
                Client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            if (connectsAndAuthenticates)
            {
                Connect ();

                // Mon, 01 Apr 2019 07:47:03 GMT
                // フォールバック先を減らすことでログインを高速化する（？）コードが散見されるが、
                // 私の環境では、AuthenticationMechanisms を見ても "LOGIN" しか入っていない
                // Gmail などなら有効なのかもしれないが、今の私には不要で、なおかつ急ぐ機能でないため見送る
                // https://github.com/jstedfast/MailKit/issues/304
                // Client.AuthenticationMechanisms.Remove ("XOAUTH2");

                if (Settings.RequiresAuthentication)
                    Authenticate ();
            }
        }

        // Mon, 01 Apr 2019 06:59:28 GMT
        // 非同期には今のところ対応しない
        // すごい数のメールを送るプログラムを書くなら、Send 単位で非同期の処理を行うのでなく、
        // 送りっぱなしのスレッドを二つ三つ作り、それらにメールを流し込むキューを私なら作りそうである
        // 一方、送る数が少ないなら、特に、それがウェブシステムなら、どうせ await で送信終了を待つことになる

        public void Connect () =>
            Client.Connect (Settings.Host, Settings.Port, Settings.RequiresSsl);

        public void Authenticate () =>
            Client.Authenticate (Settings.UserName, Settings.Password);

        public void Send (MimeMessage message) =>
            Client.Send (message);

        public void Send (nMimeMessage message)
        {
            // Wed, 24 Apr 2019 00:38:04 GMT
            // 簡単なことなのに、BuildBody を呼び忘れるミスが多い
            // 気付くまでの労力を無駄にしないため、送信時に例外を投げる

            if (message.IsBodyBuilt == false)
                throw new nBadOperationException ();

            Client.Send (message.Message);
        }

        // Mon, 01 Apr 2019 07:02:19 GMT
        // Logger を経由して LoggingStream に入っている全データを文字列として取得する
        // 内部で使われているエンコーディングについての情報がなかったので、一応 UTF-8 を当てておく
        // たぶん、サーバーとの7ビットでのやり取りをそのままバイナリーとして書き出す
        // そのため、理屈としては ANSI で足りるはずだが、UTF-8 のメールもないことはない
        // ToArray は、Writes the stream contents to a byte array, regardless of the Position property とのこと
        // 接続と認証が終わった時点で Position を見たら数百バイト進んでいるが、ToArray を何度呼んでもログを取れた
        // 得られる結果には、末尾に余分な改行がつくので、必要に応じてノーマライズなどを行う
        // ファイルへの追記などを繰り返すことを想定し、改行を除くことはしていない
        // メソッド名に entire を入れるのは、ReadAllText にならってのこと
        // ただ、不可算な text に対し、log は、1行ずつ log と呼び、全体を logs とみなすのか、全体で log なのかが難しい
        // 個人的には、ログは、ファイル一つで log、ファイルが複数あれば logs と思え、
        // 件数を明示的に数えるなら、そもそも複数件のログを単一のファイルに入れるべきでないと思う
        // そのため、このクラスでも、サーバーにつなぎ、処理し、切るまでの一連の処理を log とみなした上、
        // all では複数形をつなぎたくなるので、entire によって「全体」を示している
        public string GetEntireLog (Encoding encoding = null) =>
            (encoding != null ? encoding : Encoding.UTF8).GetString (LoggingStream.ToArray ());

        // Fri, 05 Apr 2019 04:45:00 GMT
        // 使用頻度が低いだろうが、一応用意

#pragma warning disable IDE0060
        public byte [] GetEntireLogInBinary (Encoding encoding = null) =>
            LoggingStream.ToArray ();
#pragma warning restore IDE0060

        public void Dispose ()
        {
            if (LoggingStream != null)
            {
                LoggingStream.Dispose ();
                LoggingStream = null;
            }

            if (Logger != null)
            {
                Logger.Dispose ();
                Logger = null;
            }

            if (Client != null)
            {
                Client.Dispose ();
                Client = null;
            }
        }
    }
}

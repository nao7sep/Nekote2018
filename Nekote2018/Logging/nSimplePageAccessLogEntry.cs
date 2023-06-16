using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nSimplePageAccessLogEntry
    {
        // Mon, 06 May 2019 11:02:16 GMT
        // 以下、nSimpleUserEntry に書いたことを参考とし、まずは日時から始める
        // Date, Time, DateTime, AccessUtc, CreationUtc, UtcDateTime, UtcTicks, Ticks なども考えたが、
        // nSimplePageAccessLogEntry というクラス名が十分に「何の」UTC なのか示すため、Utc だけで足りる
        // 「日時」である点を強調するために DateTime のみというのも選択肢だが、それでは Utc に比べて情報量が落ちる
        // UserHostName, UserHostAddress は、Request プロパティーのメンバーの名前であり、情報量の多い方を先に置く
        // 続く UserName までがユーザーの身元に関する情報である
        // 続いて、そのユーザーがブラウザーをどう設定しているかの情報を、それぞれプロパティー名そのままで続ける
        // そこでも、情報量の多さから、まずは UserAgent を置いている
        // UserLanguages は、Request では null となり得る配列だが、Nekote では他のクラスとの整合性のために List とし、すぐに初期化する
        // そのため、Count を見ることでデータが存在するかどうかを調べられる
        // これでブラウザーの設定が終わったので、最後に、そのユーザーが、そのブラウザーで、どのページを開いたあとに何を開いたのかを扱う
        // UrlReferrer も RawUrl も Request にあるもので、有名なスペルミスについては、.NET が正しているのでそれに従う
        // リクエストを受け取ったときのみ "referer" で解析するが、あくまで転送時のフォーマットに過ぎないと解釈
        // 他にも、サーバーの名前や IP アドレス、送受信のバイト数、レスポンスのコードなどがあるが、それらは IIS のログに任せる
        // nSimplePageAccessLogEntry は、Google Analytics 的なことを、より厳密に製品や地域といったデータと関連付けて行うためのものであり、
        // たとえば、どの地域の顧客はどういう商品を好む傾向があるのか、といったことを多種多様なアルゴリズムでガリガリやりたいのである
        // Google Analytics を使えば、ページあたりのアクセス数などは容易に分かるだろうが、
        // 商品同士の関係性も内部的なデータによって認識した上で Amazon がやっているようなことまでやるには、不便が目立ちそうだ
        // 以下は、自分のやりたい分析に必要なものが全て含まれていながら、なくても困らないものは全て省かれていて、無駄がない
        // https://docs.microsoft.com/en-us/windows/desktop/http/iis-logging
        // https://docs.microsoft.com/en-us/windows/desktop/http/w3c-logging
        // https://kakunin.net/kun/

        // Tue, 07 May 2019 10:17:01 GMT
        // RawUrl を廃止し、UrlOriginalString に切り替える
        // 長くなりそうなので、コメントを memo.txt の方に書く

        // Tue, 07 May 2019 11:26:15 GMT
        // なかなかガッツリ紆余曲折中だが、やはり RawUrl を使う
        // その結論に至るまでに調べたことなども memo.txt に殴り書きした

        // Tue, 07 May 2019 11:37:26 GMT
        // Response.StatusCode を入れたくなったが、何についての統計をとるかというのがブレてくるので却下
        // システムの安定度や、クエリー文字列などによるパフォーマンスの変化を見たいのでなく、
        // ユーザーのふるまいや商品の人気といったものを、何へのアクセスが試みられたかで知りたい
        // ステータスコードは、実装が良好なら、ほぼ全てのレスポンスにおいて200であり、
        // 稀にそれ以外のときには、IIS のログあるいは Nekote のエラーログに何らかの情報が入る

        // Mon, 06 May 2019 12:08:54 GMT
        // ブラウザーが任意で送ってくる情報なので、大部分をオプション扱いする
        // たいてい届くものであっても、送ってこないブラウザーの存在を否定できない

        public DateTime Utc { get; set; }

        public string UserHostName { get; set; }

        // Mon, 06 May 2019 13:22:06 GMT
        // ユーザーがプロキシーを使っていたり、イントラネットでアクセスしていたりのときに IPv6 のアドレスになるとのこと
        // 私がこのクラスに求めるのは大まかな統計機能であり、それ以上の厳密性などを必要としないため、今のところ無視
        // https://stackoverflow.com/questions/1932843/iis-request-userhostaddress-returning-ipv6-1-even-when-ipv6-disabled
        // http://tutorialgenius.blogspot.com/2010/09/aspnet-get-ipv4-address-even-if-user-is.html
        public string UserHostAddress { get; set; }

        public string UserName { get; set; }

        public string UserAgent { get; set; }

        public List <string> UserLanguages { get; set; } = new List <string> ();

        public string UrlReferrer { get; set; }

        public string RawUrl { get; set; }

        public static readonly Func <nSimplePageAccessLogEntry, nStringTableRow> EntryToStringTableRow = (entry) =>
        {
            nStringTableRow xRow = new nStringTableRow ();
            xRow.SetDateTime (nSimplePageAccessLogEntryCsvIndices.Utc, entry.Utc); // 必須

            if (string.IsNullOrEmpty (entry.UserHostName) == false)
                xRow.SetString (nSimplePageAccessLogEntryCsvIndices.UserHostName, entry.UserHostName); // オプション

            if (string.IsNullOrEmpty (entry.UserHostAddress) == false)
                xRow.SetString (nSimplePageAccessLogEntryCsvIndices.UserHostAddress, entry.UserHostAddress); // オプション

            if (string.IsNullOrEmpty (entry.UserName) == false)
                xRow.SetString (nSimplePageAccessLogEntryCsvIndices.UserName, entry.UserName); // オプション

            if (string.IsNullOrEmpty (entry.UserAgent) == false)
                xRow.SetString (nSimplePageAccessLogEntryCsvIndices.UserAgent, entry.UserAgent); // オプション

            if (entry.UserLanguages.Count > 0)
                xRow.SetString (nSimplePageAccessLogEntryCsvIndices.UserLanguages, entry.UserLanguages.nJoin (nStringsSeparator.VerticalBar)); // オプション

            if (string.IsNullOrEmpty (entry.UrlReferrer) == false)
                xRow.SetString (nSimplePageAccessLogEntryCsvIndices.UrlReferrer, entry.UrlReferrer); // オプション

            if (string.IsNullOrEmpty (entry.RawUrl) == false)
                xRow.SetString (nSimplePageAccessLogEntryCsvIndices.RawUrl, entry.RawUrl); // オプション

            // Tue, 07 May 2019 07:51:09 GMT
            // RawUrl は、他と同様の安全な実装にしない理由が特にないためオプション扱いにしているが、ないとページを開けないのだから常に存在する
            // そのため、以下のコードによって CSV の最後の列まできっちり出力されることを保証する必要性はない
            // そもそも、今後、最初のいくつかの列だけが大事で、それ以降、値の設定されないことが多い列がいくつも続くようなスキーマを扱うこともあるだろう
            // といったことも勘案し、CSV の書き込みにおいては存在する列だけ書き、読み込みにおいても存在する列だけ読む実装を習慣化していく
            // xRow.EnsureFieldExists (nSimplePageAccessLogEntryCsvIndices.RawUrl);

            return xRow;
        };
    }
}

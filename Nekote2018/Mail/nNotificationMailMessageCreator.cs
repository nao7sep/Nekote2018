using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Fri, 03 May 2019 10:44:12 GMT
    // 通知メールの送信に必要な情報を集め、本文などを受け取ってメールを Create するクラス
    // 名前が長いが、Mail を省いては IM 的なニュアンスが出てくるし、Message がなくては他と不整合で、
    // ほぼ単機能クラスなので Creator を除いての一般化も不自然で、削れる単語がない

    public class nNotificationMailMessageCreator
    {
        // Fri, 03 May 2019 10:47:04 GMT
        // 管理者が使う機能であり、ユーザービリティーよりシンプルさと安定性なので、
        // それぞれのメアドについて表示名を受け取るようなことはしない

        public string From { get; set; }

        // Fri, 03 May 2019 10:47:46 GMT
        // これを List にしてすぐ初期化するか、初期化はオンデマンドにするか、コンストラクターで行うか、
        // あるいは Array にしておいて set も許可するか、といったことは、Nekote の全体的な設計に今後影響する
        // 結局のところ、「基本的には List をすぐに初期化」くらいで考えておき、しかし例外に寛容に、臨機応変に対処するしかない
        // オンデマンドは、null かどうかを見るときに無駄に初期化されるため、こういうところではコスト節約にならない
        // コンストラクターでやるなら、宣言と同じ行で初期化も行った方がコードの可読性が高い
        // Array にして set を許すことのメリットは、null チェックのコストが低いこと
        // 一方で、インスタンスを残しておいて各所で要素を追加していくコーディングが難しくなり、
        // どこかで List を作って要素を入れきったものについて ToArray を呼ばなければならない制約が生じる
        // ただ、少しずつ追加していくということが絶対になく、どこかで配列がポンと出てくるのを受ける場合には Array が適する
        // 実質的には、1) すぐに初期化する List、2) どこかでポンと設定する前提の set 可能な Array、の2択なのだろう

        // Fri, 03 May 2019 12:43:59 GMT
        // To には複数のメアドを設定できるが、個別送信にはならないので注意
        // つまり、管理者が複数いる場合、全員がお互いのメアドを To 欄で参照できてしまう
        // 個別送信にすると、そのうち一つのメアドがおかしいからエラーになって……のように問題が多層化する
        // 極端な話、システムが壊れかけていて、あと数秒しか処理を行えない状況もあるかもしれない
        // といったことを勘案するなら、通知メールは、一度の処理でサクッと全員に送ってしまった方が確実
        // システムを共同管理している人物のメアドくらい普通は知れ渡っているため、実害はないだろう

        public List <string> To { get; private set; } = new List <string> ();

        public bool IsEnabled
        {
            get
            {
                return To.Count > 0;
            }
        }

        // Fri, 03 May 2019 10:55:01 GMT
        // 以前なら、迷わずに "Notification ({0})" としていたし、ページの <title> タグにもそういうものを入れていたが、同じようなことをしている人を見たことがない
        // そのため、<title> タグに他社が何を設定しているか、パンくずリストはどうなのか、といったことを調べた上、ASCII に過度にこだわらず、« を採用する
        // 私は、<title> にもこれを使い、右向きになるパンくずリストには » を使おうと思うが、Google は、検索結果に全角の › を入れてくる
        // 「タイポグラフィー的に」という理由で ASCII の > を避ける人は多く、その場合、› か » では、> と区別がつきにくい方を敢えて使う合理性は乏しい
        // Nekote では、nDiffMatchPatch.cs には ⏎ を使っていて、かなりフォントを選ぶ冒険だと思っていたが、現行のブラウザーはどれもサクッと表示する
        // 決め打ちにはしていても、変更できないわけでないし、それは SubjectFormat も同じなので、そこそこ今風（？）なものを採用していい
        // https://stackoverflow.com/questions/16262480/how-to-properly-indicate-the-direction-within-a-breadcrumbs
        // https://css-tricks.com/markup-for-breadcrumbs/

        // Sat, 28 Sep 2019 21:39:28 GMT
        // « を埋め込んでいたのを nHtmlChars に移し、こちらでも3ヶ所で参照しないために iSubjectFormatDefaultValue を追加
        // プロパティーのデフォルト値を別のところに用意したことが今までないようで、const、プロパティー、readonly、メンバー変数のうちどれを使うか迷った
        // 結論としては、nSimplePageAccessLogEntryCsvIndices.cs のコメントに立ち返り、「const かプロパティーか」の二択において、
        // 定数でないため不可能な const の除外により、消去法でプロパティーとした
        // readonly は、上記のファイルのコメントに書いたとおり、IDE で見るときに分かりにくくて不便
        // メンバー変数は、通常、バリバリ変更し、使い込むものであり、定数「的」なものに使うには違和感を覚える

        // Tue, 01 Oct 2019 02:39:50 GMT
        // パッと次に進むつもりだったが、自分の知識不足を知り、いろいろと改善し、また、「定数の扱い方.txt」をまとめた
        // 以下の場合、変更できるべきでないが、定数によって初期化することもできないため、static readonly が適する
        // 名前については、private の readonly なので、上記のメモに基づき、i をつけるのが妥当

        private static readonly string iSubjectFormatDefaultValue = "Notification " + nHtmlChars.LeftPointingDoubleAngleQuotationMark + " {0}";

        public string SubjectFormat { get; set; } = iSubjectFormatDefaultValue;

        // Fri, 03 May 2019 11:05:26 GMT
        // 他のクラスと同様、Load を二つ用意しておく
        // WPF など、通知メールを送れるように構成することなく使うプログラムも多いため、いずれのキーもなくてもエラーにならず、
        // また、空の値の読み込み・分割によって To が空になった場合には、通知メールの機能そのものがオフになる
        // その場合、*Mode がどのようになっていようと、メール送信の処理のブロックに入ることすらない
        // それはそれでリスクもあるが、そのためにログファイルを吐くのであり、そちらが目に入るように工夫すればいい

        public void Load (nNameValueCollection collection, string keyPrefix = null, string keySuffix = null)
        {
            From = collection.GetStringOrNull (keyPrefix + "NotificationMailFrom" + keySuffix);
            To.Clear ();
            // Fri, 03 May 2019 11:16:28 GMT
            // | を区切り文字としての分割は不可欠だが、その後の nDistinct などは、この部分のコードの責務でない
            // また、空の文字列が含まれていてメール送信に失敗するのも設定側のミスなので、nSplit の引数によって洗うことをしない
            To.AddRange (collection.GetStringOrNull (keyPrefix + "NotificationMailTo" + keySuffix).nSplit (nStringsSeparator.VerticalBar));
            SubjectFormat = collection.GetStringOrDefault (keyPrefix + "NotificationMailSubjectFormat" + keySuffix, iSubjectFormatDefaultValue);
        }

        public void Load (nDictionary dictionary, string keyPrefix = null, string keySuffix = null)
        {
            From = dictionary.GetStringOrNull (keyPrefix + "NotificationMailFrom" + keySuffix);
            To.Clear ();
            To.AddRange (dictionary.GetStringOrNull (keyPrefix + "NotificationMailTo" + keySuffix).nSplit (nStringsSeparator.VerticalBar));
            SubjectFormat = dictionary.GetStringOrDefault (keyPrefix + "NotificationMailSubjectFormat" + keySuffix, iSubjectFormatDefaultValue);
        }

        // Fri, 03 May 2019 11:09:22 GMT
        // 元々は Build であり、BuildBody も呼んでいたが、それでは呼び出し側でメールの最終調整をしにくい
        // BuildBody を呼んだあと、処理を経てまた BuildBody を呼ぶのでは、添付ファイルがあるときの無駄が大きい
        // 一応、From などが null でも落ちないようにしているが、呼び出し側でのチェックも不可欠

        public nMimeMessage Create (string textBody, string htmlBody = null, params string [] attachmentsPaths)
        {
            nMimeMessage xMessage = new nMimeMessage ();

            if (string.IsNullOrEmpty (From) == false)
                xMessage.AddFrom (From);

            foreach (string xTo in To)
                xMessage.AddTo (xTo);

            if (string.IsNullOrEmpty (SubjectFormat) == false)
            {
                // Fri, 03 May 2019 11:13:19 GMT
                // ウェブアプリケーションでは nApplication.NameAndVersion を常に利用可能なわけでもないため、
                // Application_Start などで nLiterals.ApplicationTitle に固有のタイトルをコピーするのが良い
                string xTitle = string.IsNullOrEmpty (nLiterals.ApplicationTitle) == false ? nLiterals.ApplicationTitle : nApplication.NameAndVersion;
                xMessage.SetSubject (string.Format (SubjectFormat, xTitle));
            }

            if (string.IsNullOrEmpty (textBody) == false)
                xMessage.SetTextBody (textBody);

            if (string.IsNullOrEmpty (htmlBody) == false)
                xMessage.SetHtmlBody (htmlBody);

            // Fri, 03 May 2019 11:17:24 GMT
            // nStrings.ExtractExistingPaths を使えば安全性が少し高まるが、
            // Load のところと同様、そこまでする責務をこのメソッドは負わない
            // おかしなパスを渡すのは呼び出し側のミスであり、そちらでデバッグするべき
            foreach (string xPath in attachmentsPaths)
                xMessage.AddAttachment (xPath);

            // xMessage.BuildBody ();
            return xMessage;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace Nekote
{
    // Tue, 07 May 2019 23:08:40 GMT
    // 全てのページのクラスにこれを継承させる、というクラスをできれば作りたくなくて nWebAuthentication を作ってみたが、
    // ThreadStatic をつけても CanContinue の値がリロードのたびに変わり、ASP.NET では原理的に無理だと知った
    // 1) 1リクエスト1スレッド、ではあっても、2) 同一ユーザーによる連続する2回のリクエストが同じスレッドによって処理される保証がない、ようである
    // そういえば、旧版の Nekote をベースとしたページの実装では、Page_Load などで多数の静的プロパティーに初期値を再設定していた
    // 関数型言語のようにしたくて機能の大半を静的クラスの静的メソッドに入れてしまっていたので以前はそうするしかなかったが、
    // 現行の Nekote は、そのあたりの反省に基づいての設計になっているため、ここでもサクッと Page を継承し、各ページに継承させる
    // https://stackoverflow.com/questions/4791208/threadstaticattribute-in-asp-net

    public class nWebPage: Page
    {
        public bool CanContinue { get; private set; } = true;

        #region ユーザー認証関連
        // Thu, 16 May 2019 08:55:23 GMT
        // 以下、UserName, IsAuthenticated, UserType という順序だが、
        // UI に表示するときには、IsAuthenticated, UserType, UserName の順序が適する
        // 内部では、順序を左右するプロパティーの主従関係が、内容としての依存関係に基づいていて、
        // 最初に分かる UserName があり、そこから分かるもののうち、すぐ分かるもの、少し重たいものが続く
        // しかし、UI やモデル（的な）クラスでは、ユーザーの自然な認識に基づき、種類などが先行する
        // そのときに最初が IsAuthenticated なのは、ログインしているかどうかがユーザーにとっては第一であるため
        // 内部的にはユーザー名が第一であり、そこからログインしているかどうかが分かるが、そういう表示ではユーザーには分かりにくい

        // Tue, 07 May 2019 23:35:59 GMT
        // 認証されていないときに User.Identity.Name が "" になることがあるようなので、値を洗っておく
        // mUserName だけでは初期化が済んだかどうか分からないためフラグを使う
        // User はたぶん大丈夫だろうが、Identity は、null になる可能性がありそうで懸念がある
        // そのあたりを突き詰めるために .NET のソースを精読するより、null かどうか調べた方が早い

        // Thu, 16 May 2019 07:30:05 GMT
        // フラグ、値、プロパティーの三つを用意するのがうるさかったので、フラグと値を一つにした
        // Nullable 的な仕様にできず、new が必要なのもうるさいが、変数が増えて行数が増えるよりはマシ

        private nInitialized <string> mUserName = null;

        public string UserName
        {
            get
            {
                if (mUserName == null)
                {
                    // Thu, 16 May 2019 07:28:03 GMT
                    // string を mUserName に直接設定できる Nullable 的な仕様では、
                    // mUserName のインスタンスが null なのか中身がそうなのか曖昧になる
                    // nInitialized の方に、そのあたりの詳しいコメントを書いた

                    // Thu, 16 May 2019 07:40:51 GMT
                    // 「あるなら、洗って設定」のみのプロパティーなので、IsUserName までは呼ばない
                    // それは、種類の判別や、認証されているかどうかの判別といったところで行われるべきこと

                    if (User != null && User.Identity != null &&
                            string.IsNullOrEmpty (User.Identity.Name) == false)
                        mUserName = new nInitialized <string> (User.Identity.Name);
                    else mUserName = new nInitialized <string> (null);
                }

                return mUserName.Value;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                // Thu, 16 May 2019 07:33:34 GMT
                // UserType を見るのもアリだし、そちらの方が厳密だが、
                // lock するし、ループもあるし、そこまでのことが毎回必要なのかどうかは微妙
                // Authenticate を通っての文字列が UserName には入るため、
                // ここでは、null などでないかだけ見て、そうでないのにフォーマットがおかしいときには例外を投げる
                // どちらにも引っ掛からなければ、認証されているとみなして実害はなさそう

                if (string.IsNullOrEmpty (UserName))
                    return false;

                if (nValidator.IsUserName (UserName) == false)
                    throw new nBadOperationException ();

                return true;
            }
        }

        // Tue, 07 May 2019 23:31:41 GMT
        // null の場合、一度だけ nAutoLock によって初期化
        // たぶん大丈夫だと思うが、デッドロックにしばらく注意が必要か

        private nSimpleUserType? mUserType = null;

        public nSimpleUserType UserType
        {
            get
            {
                if (mUserType == null)
                    // Thu, 16 May 2019 06:52:55 GMT
                    // 認証されておらず、UserName が null でも落ちないように変更
                    mUserType = nAutoLock.GetUserType (UserName);

                return mUserType.Value;
            }
        }

        /// <summary>
        /// 匿名ユーザーでも管理者でもない一般ユーザー。
        /// </summary>
        public bool IsUser
        {
            get
            {
                return UserType == nSimpleUserType.User;
            }
        }

        /// <summary>
        /// .dat の Administrator または .config の Root ということ。
        /// </summary>
        public bool IsAdministratorOrRoot
        {
            get
            {
                return UserType == nSimpleUserType.Administrator ||
                    UserType == nSimpleUserType.Root;
            }
        }
        #endregion

        #region ページのリダイレクトなど
        // Tue, 07 May 2019 23:38:29 GMT
        // Redirect を呼んだらそこで全ての処理が打ち切られ、すぐさまリダイレクトのステータスコードが返るという実装ではないので、Schedule* とした
        // .NET の RedirectFromLoginPage を使わないのは、すぐに処理を打ち切るために例外を投げる方の Redirect が使われるようだからである
        // GetRedirectUrl の二つ目の引数は無視されるとのことだが、ちょうどいいものがあるので一応渡しておく
        // https://docs.microsoft.com/en-us/dotnet/api/system.web.security.formsauthentication.redirectfromloginpage
        // https://docs.microsoft.com/en-us/dotnet/api/system.web.security.formsauthentication.setauthcookie
        // https://docs.microsoft.com/en-us/dotnet/api/system.web.httpresponse.redirect
        // https://docs.microsoft.com/en-us/dotnet/api/system.web.security.formsauthentication.getredirecturl

        public void ScheduleRedirectFromLoginPage (string userName, bool createsPersistentCookie = true)
        {
            FormsAuthentication.SetAuthCookie (userName, createsPersistentCookie);
            Response.Redirect (FormsAuthentication.GetRedirectUrl (userName, createsPersistentCookie), false);
        }

        // Tue, 07 May 2019 23:58:10 GMT
        // ログインページに戻る方は、使ったことがないので実装しない
        // Login のようなリンクを用意することがなく、いきなり Admin へのリンクを作る
        // そのため、ログインしたら *From* の方ですぐに管理ページのトップが開く

        public void ScheduleRedirect (string url) =>
            Response.Redirect (url, false);

        // Tue, 07 May 2019 23:45:55 GMT
        // 認証のメソッドを含む静的プロパティーを nStatic などに入れ、ログインの試行の結果を必要に応じて内部で自動的にログに入れ、
        // 認証を通ったなら ScheduleRedirectFromLoginPage によってクッキーの設定とリダイレクトの「スケジュール」を行い、
        // その後も他に必要な処理があったら行い、最後に CompleteRequest で CanContinue への false の設定も行う
        // これらを一つのメソッドにまとめるのは容易だが、ログインの処理は各アプリケーションで基本的に一度なので、汎用性を優先
        // https://stackoverflow.com/questions/2777105/why-response-redirect-causes-system-threading-threadabortexception
        // https://docs.microsoft.com/en-us/dotnet/api/system.web.httpapplication.completerequest

        public void CompleteRequest ()
        {
            HttpContext.Current.ApplicationInstance.CompleteRequest ();
            CanContinue = false;
        }

        // Tue, 07 May 2019 23:50:29 GMT
        // ログアウトの処理はクッキーを消すだけだが、認証されていないと開けないページの処理の途中のこともあるだろうから CanContinue を false にする
        // 呼び出し側でも、すぐさまそのメソッドを抜け、後続のコードも CanContinue のチェックによって全てスキップするべき
        // .NET では SignOut だが、Login の対義語は Logout なので、そちらで整合させる
        // Log out だから LogOut だとする人もいるが少数派で、Logoff とするには Logon でないといけないが Login は .NET も使っている識別子
        // https://docs.microsoft.com/en-us/dotnet/api/system.web.security.formsauthentication.signout

        // Wed, 08 May 2019 00:11:01 GMT
        // ドキュメントを見ていて今さら知ったが、SignOut はリダイレクトのスケジュールも行うようである
        // リダイレクト先はログインページだと決まっていて、それ以外へのリダイレクトが必要なら呼び出し側がスケジュールを上書きする必要性がありそうだ
        // 感覚としては ScheduleRedirectFromLoginPage と同じで、使い方も同様である必要があるが、
        // 間違いなく忘れて Redirect と CompleteRequest を忘れるので、メソッド名を Logout のみから変更した

        // Sat, 18 May 2019 11:25:24 GMT
        // LogoutAndScheduleRedirect から Logout に名前を戻した
        // Microsoft のドキュメントでは、リダイレクトは常に起こるような書き方になっているが、念のために試したらそうでなかった
        // リダイレクトが行われるのは、フォーム認証がクッキーを使わないモードになっていて、リダイレクトしないとログイン情報を消せないときのようだ
        // それなら、CanContinue を戻し、ScheduleRedirect などと組み合わせての使用を想定する選択肢もあるが、
        // CompleteRequest を呼ぶことで CanContinue は変更されるため、リダイレクトのために結局そちらを呼ぶなら処理の重複になる
        // また、特定の条件において .NET の SignOut を呼び、そのままページの処理を続ける実装をする人もいるようである
        // 自分も今後そういうことをする可能性を考えるなら、SignOut は、名前だけ Login と整合するものがシンプルにラップされているのが良い
        // https://www.hanselman.com/blog/NewFormsAuthenticationSignOutBehaviorInASPNET20.aspx

        // Thu, 16 May 2019 07:43:14 GMT
        // nSimpleUserLoginLog* には、ログアウトについての情報は入らない
        // 入退室を全て記録する軍事施設のような実装にしたところで、ログアウトせずにブラウザーを閉じられては無意味
        // そもそもログインのログを取る目的は、IP アドレスなどの傾向を見て不正なログインがないか調べること
        // 不正なログインがあるなら、その時点で既に大きな問題であり、
        // その人がご丁寧にログアウトしてくれると期待してのログアウトのログ取りによってセキュリティーは向上しない

        public void Logout ()
        {
            FormsAuthentication.SignOut ();
            // CanContinue = false;
        }
        #endregion

        #region リクエスト関連の情報
        // Mon, 20 May 2019 21:22:44 GMT
        // nApplication.Settings と同様、nNameValueCollection でのアクセスを可能にしておく
        // ついでに nNameValueCollection を更新し、*Helper を追加したので、できることが多い

        private nNameValueCollection mQueryString = null;

        public nNameValueCollection QueryString
        {
            get
            {
                if (mQueryString == null)
                    mQueryString = new nNameValueCollection (Request.QueryString);

                return mQueryString;
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web;
using System.Web.Hosting;

namespace Nekote
{
    // Fri, 05 Apr 2019 10:42:22 GMT
    // nPath 同様、URL に関する処理を単一のクラスにまとめてみる
    // URL なのか URI なのかでは、
    // 1) MapRoute が url という引数をとる
    // 2) 最近、URL Standard というものが進んでいるようだ
    // 3) 自分の近くで URL を URI と呼んでいる人を見たことがない
    // あたりを理由として、「わざわざ URI とする理由がない」というのを結論とする
    // https://url.spec.whatwg.org/

    public static class nUrl
    {
        // Wed, 24 Apr 2019 22:20:18 GMT
        // nPath と nUrl の両方に用意するもので、内容の順序のみ異なる
        // それぞれどちらを使うか分かり切っているのにミスでの混同を目にする
        public static readonly char [] SeparatorChars = { '/', '\\' };

        public static readonly char DefaultSeparatorChar = '/';

        #region Path クラスのメソッドと似たもの
        public static string Combine (string url1, string url2)
        {
            // Fri, 05 Apr 2019 10:48:18 GMT
            // 片方が null または "" なら、もう片方をそのまま返す
            // 両方が有効なら、つなぎ目のところの / と \ を洗い、単一の / でつなぐ
            // 両方が無効のときには、null に決め打ちにするのでも、どちらかに決め打ちにするのでもなく、
            // どちらかが "" なら "" に、両方が null なら null になるようにしてみた

            if (string.IsNullOrEmpty (url1) == false)
            {
                if (string.IsNullOrEmpty (url2) == false)
                    return url1.nTrimEnd (SeparatorChars) + DefaultSeparatorChar + url2.nTrimStart (SeparatorChars);
                else return url1;
            }

            else
            {
                if (string.IsNullOrEmpty (url2) == false)
                    return url2;
                else return url1 ?? url2;
            }
        }

        public static string Combine (string url1, string url2, string url3) =>
            Combine (Combine (url1, url2), url3);

        public static string Combine (string url1, string url2, string url3, string url4) =>
            Combine (Combine (url1, url2, url3), url4);

        public static string Combine (params string [] urls)
        {
            // Fri, 05 Apr 2019 10:52:31 GMT
            // 空の string [] を渡せるため、あり得ないわけでない

            if (urls.Length == 0)
                return null;

            // Fri, 05 Apr 2019 10:53:17 GMT
            // StringBuilder を使ってゴリゴリやった方が間違いなく高速だが、
            // そもそも、5個も10個も部分をつなげて URL を作ることが稀

            string xUrl = urls [0];

            for (int temp = 1; temp < urls.Length; temp ++)
                xUrl = Combine (xUrl, urls [temp]);

            return xUrl;
        }
        #endregion

        // Thu, 25 Apr 2019 09:29:09 GMT
        // nPath にも同名のメソッドを実装したが、こちらは nTrimEnd に丸投げするので足りる
        // たとえば http:// を与えたら http: になるが、そもそも http:// は URL として有効なものでない
        // 一方、パスとしては、C:\ は有効であり、実際に TrimEnd に与えられる可能性があるため対処が必要

        public static string TrimEnd (string url) =>
            url.nTrimEnd (SeparatorChars);

        #region よく使う URL のプロパティー
        // Fri, 05 Apr 2019 10:57:58 GMT
        // アプリケーションのルート（に何かをつなげたもの）を通知メールなどに入れたいことがよくあるので、プロパティーを用意
        // たとえば、localhost:12345/app/home/index/?query=1#fragment が開かれている場合、
        // ApplicationDirectoryUrl: localhost:12345/app/
        // ServerDirectoryUrl: localhost:12345/
        // というようになる
        // index/ までのものは簡単に取れるし、他にも用意したいものがいくつかあったが、たぶん使わない
        // https://dobon.net/vb/dotnet/internet/analyzeurl.html
        // https://en.wikipedia.org/wiki/Uniform_Resource_Identifier

        private static string mApplicationDirectoryUrl = null;

        /// <summary>
        /// HttpContext.Current が使えないところでは利用不可。
        /// </summary>
        public static string ApplicationDirectoryUrl
        {
            get
            {
                if (mApplicationDirectoryUrl == null)
                {
                    // Fri, 05 Apr 2019 11:05:55 GMT
                    // かなり古い IIS では Application_Start などでも Request を参照できたそうだが、最近のバージョンでは無理である
                    // URL に関する情報は、大別すると、1) IIS での設定値、2) ユーザーから届いたもの、の二つがある
                    // 前者は、IIS 上での仮想パス、そこに割り当てられたファイルシステム上でのパスなどである
                    // 後者は、ユーザーが GET などとして送ってきたもので、
                    // そのうちホスト部分が IIS によって抽出されるため単一のサーバーで複数のサイトを運用できる
                    // 技術的には1でそのアプリケーションのルートまでの URL くらいは取れそうだが、
                    // とりあえず、現状はできなくなっているので、それに依存するプログラミング自体をやめるべき
                    // URL の末尾に / をつけるのは、ディレクトリーの URL ならそうであるべきと思うため

                    mApplicationDirectoryUrl = Combine (
                        HttpContext.Current.Request.Url.GetLeftPart (UriPartial.Authority),
                        HostingEnvironment.ApplicationVirtualPath,
                        "/");
                }

                return mApplicationDirectoryUrl;
            }
        }

        // Fri, 05 Apr 2019 11:27:56 GMT
        // server という表現は微妙で、素人ぽいっというか、技術的に正確というわけではない
        // 単一のサーバーが複数のホストを認識するのも当たり前だし、複数のサーバーで単一のホストを運用することもある
        // しかし、URL は、ユーザーから見たら一つのサーバーのようであり、そのルートを server と呼ぶのは分かりやすい

        private static string mServerDirectoryUrl = null;

        /// <summary>
        /// HttpContext.Current が使えないところでは利用不可。
        /// </summary>
        public static string ServerDirectoryUrl
        {
            get
            {
                if (mServerDirectoryUrl == null)
                {
                    // Fri, 05 Apr 2019 11:25:26 GMT
                    // Authority には / がつかないため、明示的に入れている
                    // 前述の通り、ディレクトリー的なものは / で終わるべきと考えている

                    mServerDirectoryUrl = Combine (
                        HttpContext.Current.Request.Url.GetLeftPart (UriPartial.Authority),
                        "/");
                }

                return mServerDirectoryUrl;
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;

namespace Nekote
{
    // Nekote では、program = application + libraries とみなし、Nekote は後者のうちの一つとみなす
    // そのため、実行ファイル（または ASP.NET の DLL など、それに近いもの）に関する機能を nApplication に集め、
    // library としての Nekote の機能を、接頭辞 n を名前に含む nLibrary に集めることにする

    public static class nApplication
    {
        // Thu, 25 Apr 2019 04:26:52 GMT
        // KVP ファイルの出力時など、プログラムの名前とバージョンを単一の文字列として取得したいときがある
        // システムからエラー報告メールを送るなら、ユーザー指定のタイトルとは別にプログラムの情報も不可欠
        // Assembly から AssemblyName を取り、Nekote 0.21 のような文字列にするのは、すぐにできることだが、毎回やるのは面倒
        // ウェブアプリケーションなら HttpContext.Current が使えなくて GetEntryAssembly が使えないという問題もある
        // そのため、ローカルでもウェブでも Assembly を得られ、そこから NameAndVersion も容易に取得できるようにしておく

        private static Assembly mAssembly = null;

        /// <summary>
        /// ウェブの場合、1) Global.asax が存在し、2) HttpContext.Current が使えるところ、というのが使用条件。
        /// </summary>
        public static Assembly Assembly
        {
            get
            {
                if (mAssembly == null)
                {
                    if (IsWeb == false)
                        mAssembly = Assembly.GetEntryAssembly ();

                    else
                    {
                        // Thu, 25 Apr 2019 04:30:04 GMT
                        // ウェブでも動くようにしたが、HttpContext.Current に依存するため、Application_Start などで使えないという痛い問題がある
                        // その日のデータベースファイルを朝一でバックアップするような処理は、ページ側のコードにトリガーをねじ込むことになる
                        // 以下のコードは借り物で、なぜそれで動くのか調べていないが、昔から動いているので、動かなくなるまでそのまま使う
                        // https://stackoverflow.com/questions/4277692/getentryassembly-for-web-applications

                        // Thu, 25 Apr 2019 08:53:04 GMT
                        // NekoteWeb のうち不要なコードをギリギリまで削ってみたところ、動かなくなった
                        // どうやら、HttpApplication を継承する NekoteWeb.Global を探すようで、
                        // Global.asax がなくては、NameAndVersion が "System.Web 4.0" などになるようである

                        Type xType = HttpContext.Current.ApplicationInstance.GetType ();

                        while (xType != null && xType.Namespace == "ASP")
                            xType = xType.BaseType;

                        mAssembly = xType.Assembly;
                    }
                }

                return mAssembly;
            }
        }

        // Thu, 25 Apr 2019 04:57:35 GMT
        // 元々の実装では、Assembly, AssemblyName をキャッシュし、Name, Version も用意した上で NameAndVersion で文字列を生成していた
        // しかし、Name だけを必要とするような状況が想定できず、必要以上のラップによる nApplication と nLibrary のコードの重複が気になったため、
        // 共通化できない Assembly だけをそれぞれにキャッシュした上、文字列を生成するメソッドを非公開で共有した

        internal static string iGetNameAndVersion (Assembly assembly)
        {
            AssemblyName xName = assembly.GetName ();
            Version xVersion = xName.Version;
            return $"{xName.Name} {xVersion.Major.nToString ()}.{xVersion.Minor.nToString ()}";
        }

        private static string mNameAndVersion = null;

        /// <summary>
        /// ウェブの場合、1) Global.asax が存在し、2) HttpContext.Current が使えるところ、というのが使用条件。
        /// </summary>
        public static string NameAndVersion
        {
            get
            {
                if (mNameAndVersion == null)
                    mNameAndVersion = iGetNameAndVersion (Assembly);

                return mNameAndVersion;
            }
        }

        // Fri, 05 Apr 2019 09:06:57 GMT
        // アプリケーションディレクトリー直下の "data" ディレクトリーなどのパスをパッと得たいため、
        // 実行中のプログラムがローカルのものなのか IIS によってホストされているのかを判別できるようにしておく
        // パスや URL については、同じような情報が得られるプロパティーやメソッドが多い
        // どれが一番オリジナルかを考えるなら、ローカルなのか hosted なのかの判別には HostingEnvironment を使うべき
        // https://stackoverflow.com/questions/10223799/how-can-my-code-find-if-its-running-inside-iis
        // https://stackoverflow.com/questions/7007440/how-to-find-out-if-the-current-application-is-an-asp-net-web-app

        private static bool? mIsWeb = null;

        public static bool IsWeb
        {
            get
            {
                if (mIsWeb == null)
                    mIsWeb = HostingEnvironment.IsHosted;

                return mIsWeb.Value;
            }
        }

        private static string mDirectoryPath = null;

        public static string DirectoryPath
        {
            get
            {
                if (mDirectoryPath == null)
                {
                    if (IsWeb == false)
                        mDirectoryPath = nPath.ApplicationDirectoryPath;
                    else mDirectoryPath = nPath.WebApplicationDirectoryPath;
                }

                return mDirectoryPath;
            }
        }

        // Combine が params のもの以外では四つまで引数をとるため、こちらは三つ用意
        // 四つ以上をマップすることはまずないため、params を強引に実装することはやめておく

        // Thu, 25 Apr 2019 07:33:01 GMT
        // nUrl.Combine は四つまで対応だが、ベースを持たないのでそれでいい
        // こちらは、基底ディレクトリーがあってのそこへのパスの連結なので三つまで

        public static string MapPath (string path)
        {
            return Path.Combine (DirectoryPath, path);
        }

        public static string MapPath (string path1, string path2)
        {
            return Path.Combine (DirectoryPath, path1, path2);
        }

        public static string MapPath (string path1, string path2, string path3)
        {
            return Path.Combine (DirectoryPath, path1, path2, path3);
        }

        // Mon, 24 Sep 2018 07:25:15 GMT
        // 小さなプログラムでは設定を App.config に頼るため、パッと使えると便利
        // ラッパークラスをかませているので、より高機能であり、コードの短縮化につながる
        // クラス名に App 部分が入っているため、名前は Settings のみ

        private static nNameValueCollection mSettings = null;

        public static nNameValueCollection Settings
        {
            get
            {
                if (mSettings == null)
                    mSettings = new nNameValueCollection (ConfigurationManager.AppSettings);

                return mSettings;
            }
        }
    }
}

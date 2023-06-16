using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;
using System.Web.Hosting;

namespace Nekote
{
    public static class nPath
    {
        // Wed, 24 Apr 2019 22:20:18 GMT
        // nPath と nUrl の両方に用意するもので、内容の順序のみ異なる
        // それぞれどちらを使うか分かり切っているのにミスでの混同を目にする
        public static readonly char [] SeparatorChars = { '\\', '/' };

        public static readonly char DefaultSeparatorChar = '\\';

        #region Path クラスのメソッドのラッパー
        // Mon, 24 Sep 2018 12:55:19 GMT
        // 自作クラスに Path プロパティーがあって、Path クラスを System.IO.Path と書かなければいけないときがある
        // かといって、それを回避するために FilePath などとするのも他との整合性の問題があるため、nPath で通るようにしておく
        // directory の name を取得するわけでないとか、name は file と限らないとか、以前から気になっていた点も整えた

        private static char [] mInvalidPathChars = null;

        public static char [] InvalidPathChars
        {
            get
            {
                if (mInvalidPathChars == null)
                    mInvalidPathChars = Path.GetInvalidPathChars ();

                return mInvalidPathChars;
            }
        }

        private static char [] mInvalidFileNameChars = null;

        public static char [] InvalidFileNameChars
        {
            get
            {
                if (mInvalidFileNameChars == null)
                    mInvalidFileNameChars = Path.GetInvalidFileNameChars ();

                return mInvalidFileNameChars;
            }
        }

        public static bool IsPathRooted (string path) =>
            Path.IsPathRooted (path);

        public static string GetPathRoot (string path) =>
            Path.GetPathRoot (path);

        public static string GetDirectoryPath (string path) =>
            Path.GetDirectoryName (path);

        public static string GetName (string path) =>
            Path.GetFileName (path);

        public static string GetNameWithoutExtension (string path) =>
            Path.GetFileNameWithoutExtension (path);

        public static bool HasExtension (string path) =>
            Path.HasExtension (path);

        public static string GetExtension (string path) =>
            Path.GetExtension (path);

        public static string ChangeExtension (string path, string extension) =>
            Path.ChangeExtension (path, extension);

        public static string Combine (string path1, string path2) =>
            Path.Combine (path1, path2);

        public static string Combine (string path1, string path2, string path3) =>
            Path.Combine (path1, path2, path3);

        public static string Combine (string path1, string path2, string path3, string path4) =>
            Path.Combine (path1, path2, path3, path4);

        public static string Combine (params string [] paths) =>
            Path.Combine (paths);

        public static string GetFullPath (string path) =>
            Path.GetFullPath (path);
        #endregion

        // Mon, 24 Sep 2018 09:29:33 GMT
        // nFile のものを nDirectory から呼んでいるが、厳密にはパスだけに関する処理なので、
        // それらを残したまま、ディレクトリーあるいはファイルに限定されないクラスにも用意しておく

        public static bool CanCreate (string path) =>
            Directory.Exists (path) == false && File.Exists (path) == false;

        public static string GetRelativePath (int directoryPathLength, string path) =>
            path.Substring (directoryPathLength).nTrimStart (SeparatorChars);

        // Thu, 25 Apr 2019 09:33:56 GMT
        // パスの末尾にたまに不要な \ がついているのを削るメソッドを書いてみた
        // Windows ではディレクトリーのパスでも末尾に \ がつかず、エクスプローラーなどでもそういう表示である
        // かといって、単純に nTrimEnd を呼ぶのでは、正しいパスである C:\ が C: となって問題になる
        // \\Server や \\Server\Shared も想定したが、これらはいずれも \ で終わることがないため画一的に処理できる
        // エクスプローラーでは、前者では共有されているものが一覧表示され、後者ではそのオブジェクトのルートなどが表示される
        // いずれも自分で末尾に \ をつけて開いてみても自動的に削られるため、それが正式な仕様なのだろう

        public static string TrimEnd (string path)
        {
            // Thu, 25 Apr 2019 09:40:58 GMT
            // nTrimEnd と同様、null または "" ならそのまま返す

            if (string.IsNullOrEmpty (path))
                return path;

            // Thu, 25 Apr 2019 09:42:13 GMT
            // 条件分岐をいろいろやりたくないため、結局何文字を消したらいいのかをまず調べる
            // その上で、IsPathRooted に基づき、何文字まで消せるかを調べて最初の値と Min で比較
            // 途中、消すものがないなら、その後の処理を省略するために return する

            int xLength = 0;

            for (int temp = path.Length - 1; temp >= 0; temp --)
            {
                if (SeparatorChars.Contains (path [temp]))
                    xLength ++;
                else break;
            }

            if (xLength == 0)
                return path;

            int xRemovableLength = IsPathRooted (path) ? path.Length - 3 : path.Length,
                xLengthToRemove = Math.Min (xRemovableLength, xLength);

            return path.Substring (0, path.Length - xLengthToRemove);
        }

        #region よく使うパスのプロパティー
        // Fri, 05 Apr 2019 09:21:37 GMT
        // IsWeb によって nApplication.DirectoryPath の返すものが変わるようにした
        // その実装のため、GetEntryAssembly を呼ぶ実装をこちらに移動した
        // アプリケーションは、ウェブかそうでないか、必ず二択であり、両方が必要になることはまずない

        // Sun, 05 May 2019 18:59:17 GMT
        // nApplication, nStatic にも同様のプロパティーが散在するようになっているが、
        // 1) nApplication は、プログラムに直接的に関連するもの、2) nStatic は、1のうち、独自仕様の色合いが濃いもの、
        // 3) nPath は、プログラムというより Windows やユーザー寄りで、プログラムの配置と関係性の乏しいもの、
        // という区別をゆるく念頭に置き、パッと思い付くところに入っているようにしておく

        // Thu, 25 Apr 2019 04:17:04 GMT
        // nApplication.Assembly などを用意したが、ここの実装はさわらないでおく
        // そちらは、ウェブアプリケーションなら特殊なことをするし、いろいろとややこしい

        private static string mApplicationDirectoryPath = null;

        public static string ApplicationDirectoryPath
        {
            get
            {
                if (mApplicationDirectoryPath == null)
                    mApplicationDirectoryPath = Path.GetDirectoryName (Assembly.GetEntryAssembly ().Location);

                return mApplicationDirectoryPath;
            }
        }

        // Fri, 05 Apr 2019 09:23:10 GMT
        // IsWeb のところにも書いたが、IIS 上でホスティングされているアプリケーションに関する情報を取るなら HostingEnvironment だろう
        // 他にも方法がいろいろあるようだが、たとえば HttpContext.Current は Request がないところやサブスレッドでは使えなかった記憶がある
        // HttpRuntime.AppDomainAppPath も使えそうだが、HostingEnvironment より優れる理由がないため却下

        private static string mWebApplicationDirectoryPath = null;

        public static string WebApplicationDirectoryPath
        {
            get
            {
                if (mWebApplicationDirectoryPath == null)
                    // Thu, 25 Apr 2019 09:28:04 GMT
                    // 手元の環境ではパスの末尾に \ が入るため、安全性の高いメソッドを追加してトリム
                    // 他にも .NET のクラスライブラリーからパスをもらうところが多々あるが、
                    // 全てに TrimEnd をかますのではチェックが面倒なので、他は困ってからにする
                    mWebApplicationDirectoryPath = TrimEnd (HostingEnvironment.ApplicationPhysicalPath);

                return mWebApplicationDirectoryPath;
            }
        }

        // Mon, 24 Sep 2018 06:22:22 GMT
        // テスト時もちゃんとした開発時もデスクトップに何かを吐くことは多い
        // CommonDesktopDirectory もあるが、そちらは使ったことがない

        private static string mDesktopDirectoryPath = null;

        public static string DesktopDirectoryPath
        {
            get
            {
                if (mDesktopDirectoryPath == null)
                    mDesktopDirectoryPath = Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory);

                return mDesktopDirectoryPath;
            }
        }

        // Fri, 05 Apr 2019 09:11:38 GMT
        // Desktop などのために Map* を細かく用意していたが、nPath.Combine で済むことなので消した
        // Desktop と Temp くらいなら残していてもよかったが、今後パスがいろいろと増えそう

        // Mon, 24 Sep 2018 06:29:20 GMT
        // Temp ディレクトリーも中間処理によく使うため対応しておく
        // プログラム名を入れるとか、ランダムな文字列を入れるとかはしない
        // プログラムの実行ファイルの横の Temp ディレクトリーも、
        // そこでないといけない理由がないため対応しない

        // Fri, 05 Apr 2019 22:44:44 GMT
        // temp というとアプリケーション直下のものを意味するようになってきている
        // たまに使うだろうから消すことはないが、AppData\Local\Temp なので、表現を増やした

        private static string mLocalTempDirectoryPath = null;

        public static string LocalTempDirectoryPath
        {
            get
            {
                if (mLocalTempDirectoryPath == null)
                    mLocalTempDirectoryPath = Path.GetTempPath ();

                return mLocalTempDirectoryPath;
            }
        }
        #endregion

        #region ランダムなパスの生成
        // Mon, 24 Sep 2018 09:54:13 GMT
        // nManagedFileUtility に同様のコードが含まれるが、
        // そちらは、既存のファイルを、パスを衝突させずに流し込むもので、縮小版に関する処理も行う
        // 一方、こちらは、ルート直下に置くもののパスを、必要に応じて拡張子も含めて取得する

        // Wed, 07 Nov 2018 19:43:19 GMT
        // パスの生成方法を指定できるように以前は実装していたが、GUID に一本化した
        // nManagedFile* の方も GUID に一本化することになり、それに合わせて単純化した
        // この実装ならスレッドセーフとみなしても死にはしないが、可能性としてはゼロでないためメソッド名をそのままとする
        // ちょっとの手間で回避できる「起こるはずがない」を看過していくプログラミングは、あとあとずっと気になる

        // Thu, 02 May 2019 07:35:20 GMT
        // *ThreadUnsafe としていたが、memo.txt に書いた理由によりやめる

        // Sun, 05 May 2019 19:02:00 GMT
        // GUID 入りのパスを生成する GetAvailablePath およびその *AutoLock を用意していたが、
        // Base36 を思い付いてしまい、「ランダム」を実現するだけで GUID は過剰と感じるようになったため、
        // 全てのメソッドを一新し、また、どうせ lock 圏内で使うため *AutoLock を廃止した

        // Sat, 28 Sep 2019 02:17:53 GMT
        // 今後は、Base36 より SafeCode を使うべき

        public static string CreateDateTimeBasedPath (string directoryPath, string fileNameFormat, string ticksFormat = null)
        {
            while (true)
            {
                // Sun, 05 May 2019 13:12:46 GMT
                // ticksFormat が null のときにそのまま与えても結果が同じかどうかは保証がない
                string xTicksString = ticksFormat != null ? DateTime.UtcNow.Ticks.nToString (ticksFormat) : DateTime.UtcNow.Ticks.nToString (),
                    xPath = nPath.Combine (directoryPath, string.Format (fileNameFormat, xTicksString));

                if (nPath.CanCreate (xPath))
                    return xPath;
            }
        }

        public static string CreateGuidBasedPath (string directoryPath, string fileNameFormat, string guidFormat = null)
        {
            while (true)
            {
                string xGuidString = guidFormat != null ? nGuid.New ().nToString (guidFormat) : nGuid.New ().nToString (),
                    xPath = nPath.Combine (directoryPath, string.Format (fileNameFormat, xGuidString));

                if (nPath.CanCreate (xPath))
                    return xPath;
            }
        }

        // Sun, 05 May 2019 13:09:11 GMT
        // GUID をパスに含めることにそもそもの違和感が生じ始めている
        // ちょうど6文字の "zzzzzz" が int.MaxValue をわずかに上回るくらいなので、
        // nRandom.Next の結果を Base36 にした方が、実用上の違いなく文字を大幅に減らせる

        // Sat, 28 Sep 2019 02:17:53 GMT
        // 今後は、Base36 より SafeCode を使うべき

        public static string CreateBase36BasedPath (string directoryPath, string fileNameFormat)
        {
            while (true)
            {
                string xPath = nPath.Combine (directoryPath, string.Format (fileNameFormat, nRandom.Next ().nToBase36String ()));

                if (nPath.CanCreate (xPath))
                    return xPath;
            }
        }

        // Sun, 05 May 2019 13:10:17 GMT
        // first, last, step という単語が使われているので参考にした
        // https://en.wikipedia.org/wiki/For_loop
        public static string CreateNumberBasedPath (string directoryPath, string fileNameFormat, int first = 1, int step = 1, string numberFormat = null)
        {
            for (int temp = first; ; temp += step)
            {
                string xNumberString = numberFormat != null ? temp.nToString (numberFormat) : temp.nToString (),
                    xPath = nPath.Combine (directoryPath, string.Format (fileNameFormat, xNumberString));

                if (nPath.CanCreate (xPath))
                    return xPath;
            }
        }

        // Sat, 28 Sep 2019 02:48:16 GMT
        // Base36 のものより、今後、基本的にはこちらを使うべき

        public static string CreateSafeCodeBasedPath (string directoryPath, string fileNameFormat)
        {
            while (true)
            {
                string xPath = nPath.Combine (directoryPath, string.Format (fileNameFormat, nSafeCode.Next ()));

                if (nPath.CanCreate (xPath))
                    return xPath;
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Nekote
{
    // ディレクトリーやファイルの処理においては、同じようなミスをよく繰り返す
    // そのため、このクラスを使っておくことでミスが減るというラッパークラスを用意しておく
    // 多少のオーバーヘッドが生じるが、思わぬところでクラッシュするよりマシである

    // .NET のクラスのメソッドをラップしたものが多いが、何もかもラップしているわけでなく、それなりの基準はある
    // それは、使用頻度が高いか、よくあるミスを防ぐ仕組みがあるか、名前や仕様が整えられているかあたりである
    // いずれにも該当せず、.NET のメソッドを呼ぶことに問題がないものは、わざわざラップしていない

    public static class nDirectory
    {
        public static bool Exists (string path)
        {
            return Directory.Exists (path);
        }

        public static bool IsEmpty (string path)
        {
            // Fri, 31 Aug 2018 04:40:26 GMT
            // IEnumerable のメソッドを使って高速化できそうだが、まあいい
            return Directory.GetFileSystemEntries (path).Length == 0;
        }

        public static bool IsNonexistentOrEmpty (string path)
        {
            return Exists (path) == false || IsEmpty (path);
        }

        #region Get* を揃えておく // OK
        // .NET の Directory クラスには EnumerateDirectories や GetDirectories などが用意されているが、
        // 名前は Get* が分かりやすく、戻り値は string [] でも IEnumerable <string> でもなくクラスの IEnumerable が良く、
        // pattern なしでも option を指定できるオーバーライドも欲しかったため、一通りラップしてしまうことにした
        // 戻り値は、foreach が低速というイメージのつきまとう IEnumerable より配列の方が高速に動作しそうなイメージがあるが、
        // .NET の実装の方が IEnumerable から List を経て配列にするため、IEnumerable の方が高速と考えられる

        public static IEnumerable <DirectoryInfo> GetDirectories (string path)
        {
            // Directory でも DirectoryInfo でも、パターンの初期値は "*" のようである
            // https://msdn.microsoft.com/en-us/library/c1sez4sc.aspx
            return new DirectoryInfo (path).EnumerateDirectories ("*", SearchOption.TopDirectoryOnly);
        }

        public static IEnumerable <DirectoryInfo> GetDirectories (string path, string pattern)
        {
            return new DirectoryInfo (path).EnumerateDirectories (pattern, SearchOption.TopDirectoryOnly);
        }

        public static IEnumerable <DirectoryInfo> GetDirectories (string path, string pattern, SearchOption option)
        {
            return new DirectoryInfo (path).EnumerateDirectories (pattern, option);
        }

        public static IEnumerable <DirectoryInfo> GetDirectories (string path, SearchOption option)
        {
            return new DirectoryInfo (path).EnumerateDirectories ("*", option);
        }

        public static IEnumerable <FileInfo> GetFiles (string path)
        {
            return new DirectoryInfo (path).EnumerateFiles ("*", SearchOption.TopDirectoryOnly);
        }

        public static IEnumerable <FileInfo> GetFiles (string path, string pattern)
        {
            return new DirectoryInfo (path).EnumerateFiles (pattern, SearchOption.TopDirectoryOnly);
        }

        public static IEnumerable <FileInfo> GetFiles (string path, string pattern, SearchOption option)
        {
            return new DirectoryInfo (path).EnumerateFiles (pattern, option);
        }

        public static IEnumerable <FileInfo> GetFiles (string path, SearchOption option)
        {
            return new DirectoryInfo (path).EnumerateFiles ("*", option);
        }

        public static IEnumerable <FileSystemInfo> GetFileSystemInfos (string path)
        {
            return new DirectoryInfo (path).EnumerateFileSystemInfos ("*", SearchOption.TopDirectoryOnly);
        }

        public static IEnumerable <FileSystemInfo> GetFileSystemInfos (string path, string pattern)
        {
            return new DirectoryInfo (path).EnumerateFileSystemInfos (pattern, SearchOption.TopDirectoryOnly);
        }

        public static IEnumerable <FileSystemInfo> GetFileSystemInfos (string path, string pattern, SearchOption option)
        {
            return new DirectoryInfo (path).EnumerateFileSystemInfos (pattern, option);
        }

        public static IEnumerable <FileSystemInfo> GetFileSystemInfos (string path, SearchOption option)
        {
            return new DirectoryInfo (path).EnumerateFileSystemInfos ("*", option);
        }
        #endregion

        // ディレクトリーとファイルのいずれかのみ存在するかどうかを調べたところで、
        // そのパスにディレクトリーまたはファイルを作れるかどうかは分からない

        public static bool CanCreate (string path)
        {
            return nFile.CanCreate (path);
        }

        // 地味に便利な CreateForFile を用意するなら、これも用意しておく必要がある
        // IntelliSense に頼っているため、類似メソッドが近くにないとコーディングが止まる

        public static void Create (string path)
        {
            // nFile.Create と異なり、こちらは既存でも落ちない
            // 追記: nFile.Create でも落ちないようにした
            Directory.CreateDirectory (path);
        }

        public static void CreateForFile (string path)
        {
            // path が C:\ など、ルートディレクトリーそのものなら null が返る
            // それ以外の絶対パスならディレクトリーのパスの部分が、
            // ファイル名のみなど、ディレクトリー情報が含まれていないなら "" が返る
            string xDirectoryPath = Path.GetDirectoryName (path);

            // null なら絶対に何もすることがないので除外
            // それが path として与えられるべきかどうか以前の問題
            if (xDirectoryPath != null)
            {
                if (xDirectoryPath.Length == 0)
                    // カレントディレクトリーにファイルを作成しようとしている可能性が高いため、一応それを作っておく
                    // Environment.CurrentDirectory は、存在しないディレクトリーのパスを設定しようとすると例外が飛ぶが、
                    // 存在するディレクトリーのパスが設定されてからそのディレクトリーが消されることもあり得ないわけでない
                    Create (Environment.CurrentDirectory);
                else Create (xDirectoryPath);
            }

            // 以前の実装では、ルートディレクトリーのファイルのパスが指定されたときには、例外を投げていた
            // 作る必要のないディレクトリーを作ろうとしていることを呼び出し側が把握できるようにするためである
            // しかし、nFile.WriteAllText などで呼ぶため、ルートだろうと問題にならないように変更した
        }

        // いつの間にかディレクトリーやファイルに ReadOnly がついていただけでクラッシュするようなことを避けたい
        // 他の属性も飛ばしてしまう点が乱暴だが、Windows 上の属性に依存するプログラムは少なく、実際には影響がなさそう
        // 隠されているものが見えるようになることも、Nekote によって扱う範囲内のものでは問題となりにくい

        public static void ResetAttributes (string path)
        {
            // ディレクトリーなら、とりあえず Directory にしておけば良さそう
            // https://msdn.microsoft.com/en-us/library/system.io.fileattributes.aspx
            File.SetAttributes (path, FileAttributes.Directory);
        }

        // System.IO を using とせずにちょっとファイルシステムを操作したいときに便利
        // 一部流用が可能だが、ディレクトリー用とファイル用の両方を用意しておく

        public static FileAttributes GetAttributes (string path)
        {
            return File.GetAttributes (path);
        }

        public static void SetAttributes (string path, FileAttributes attributes)
        {
            File.SetAttributes (path, attributes);
        }

        // Nekote では UTC をメインで使っているため、
        // メソッド名に time と utc の両方を入れるのは冗長

        public static DateTime GetLastWriteUtc (string path)
        {
            return Directory.GetLastWriteTimeUtc (path);
        }

        public static void SetLastWriteUtc (string path, DateTime value)
        {
            Directory.SetLastWriteTimeUtc (path, value);
        }

        // たまに便利なディレクトリーの移動とコピーのメソッドも用意しておく
        // アクセス権限的に問題のないところでちょっとした移動やコピーを行う程度の実装

        // Sat, 28 Sep 2019 00:27:07 GMT
        // 久々にコードを見ていて、overwrites が無視されているように感じて違和感を覚えた
        // 以下の実装は、Windows のエクスプローラーで言うなら、「フォルダーの結合の競合を非表示にする」がオンになっているに等しい
        // つまり、ファイルの移動やコピーの段階では overwrites が意識されるが、その前にディレクトリーが既存でも何も起こらない
        // ただ、実際の使用においては、移動先のパスについて CanCreate を呼ぶのが普通であり、実害はない
        // overwrites がディレクトリーの存在も確認しては、その分、コードがややこしくなるし、使い勝手も悪くなりそう

        private static void iMove (DirectoryInfo sourceDirectory, string destPath, bool overwrites)
        {
            Create (destPath);

            foreach (DirectoryInfo xSubdirectory in sourceDirectory.GetDirectories ())
                iMove (xSubdirectory, Path.Combine (destPath, xSubdirectory.Name), overwrites);

            foreach (FileInfo xFile in sourceDirectory.GetFiles ())
                nFile.Move (xFile.FullName, Path.Combine (destPath, xFile.Name), overwrites);

            // 何らかの理由でディレクトリーまたはファイルが一つでも残っていれば落ちる
            Delete (sourceDirectory.FullName);
        }

        public static void Move (string sourcePath, string destPath, bool overwrites = false)
        {
            iMove (new DirectoryInfo (sourcePath), destPath, overwrites);
        }

        private static void iCopy (DirectoryInfo sourceDirectory, string destPath, bool overwrites)
        {
            Create (destPath);

            foreach (DirectoryInfo xSubdirectory in sourceDirectory.GetDirectories ())
                iCopy (xSubdirectory, Path.Combine (destPath, xSubdirectory.Name), overwrites);

            foreach (FileInfo xFile in sourceDirectory.GetFiles ())
                nFile.Copy (xFile.FullName, Path.Combine (destPath, xFile.Name), overwrites);
        }

        public static void Copy (string sourcePath, string destPath, bool overwrites = false)
        {
            iCopy (new DirectoryInfo (sourcePath), destPath, overwrites);
        }

        // Thu, 08 Nov 2018 10:47:14 GMT
        // ディレクトリーを掃除するメソッドを用意しておく
        // 指定したディレクトリーそのものを消せないので、そちらには DeleteIfEmpty を使う
        // 二つの foreach のうち、前者は二つの bool のうち一つでも true なら必要で、後者は空のファイルを消すときだけ必要
        // bool が両方 false なら前者も不要になるが、それは操作の方に問題があるため対処しない

        private static void iClean (DirectoryInfo directory, int depth, bool deletesEmptyDirectories, bool deletesEmptyFiles)
        {
            foreach (DirectoryInfo xSubdirectory in directory.GetDirectories ())
                iClean (xSubdirectory, depth + 1, deletesEmptyDirectories, deletesEmptyFiles);

            if (deletesEmptyFiles)
            {
                foreach (FileInfo xFile in directory.GetFiles ())
                {
                    if (xFile.Length == 0)
                        nFile.Delete (xFile.FullName);
                }
            }

            if (depth > 0 && deletesEmptyDirectories && directory.GetFileSystemInfos ().Length == 0)
                Delete (directory.FullName);
        }

        public static void Clean (string path, bool deletesEmptyDirectories, bool deletesEmptyFiles)
        {
            iClean (new DirectoryInfo (path), 0, deletesEmptyDirectories, deletesEmptyFiles);
        }

        public static void Delete (string path, bool isRecursive = false)
        {
            // Directory.Delete は、File.Delete と異なり、対象が存在しないときに落ちる
            // また、二つ目の引数がないか、あるいは false のときにディレクトリーが空でなくても落ちる
            // https://msdn.microsoft.com/en-us/library/fxeahc5f.aspx

            if (Exists (path))
            {
                ResetAttributes (path);
                Directory.Delete (path, isRecursive);
            }
        }

        public static void DeleteIfEmpty (string path)
        {
            if (Exists (path) && IsEmpty (path))
                Delete (path);
        }
    }
}

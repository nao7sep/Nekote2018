using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nekote;
using System.Drawing;
using System.Drawing.Imaging;

namespace NekoteConsole
{
    internal static class iManagedFileTester
    {
        // Sat, 28 Sep 2019 13:50:57 GMT
        // nManaged* のコードは、久々に見るたびに不安を覚えるもので、とにかく分かりにくい
        // 書き直しにより改善されたが、それでも分かりにくいのは、それぞれのインスタンスが扱う情報が多いからと思う
        // そのせいで、細かいところまで仕様を覚えられず、「この場合、ここはどうなるか」を何度も繰り返し調べることになる
        // また、そのときの直感で「自分ならこう実装したはずだ」と思う仕様で動かなかったときに不安が強まる
        // そのたびに仕様変更を考えるのだが、コードやコメントを読み進めていくと、現行の仕様こそ合理的に思えてくる
        // といったことの繰り返しにより、どうしても使いにくいクラス群になっていたので、いっそのことテストコードを書いてみた
        // もっとも、重要なのはテストそのものでなく、その結果として生じるレポートのファイルこそが便利になるだろう

        public static void TestEverything ()
        {
            // Sat, 28 Sep 2019 13:56:42 GMT
            // ただのファイル、いくつかの縦横の画像を作り、特に後者を、拡張子を間違えて保存
            // 縦横は、冗長だが、画像としての最小、最小の縮小版周辺、よく使う縮小版周辺、大きいもの、を用意
            // 縦横を異ならせて計算結果を吐くのは、1) ログが見にくくなる、2) この計算はデバッグができている、ため却下

            string xFilePath = nApplication.MapPath ("File.txt");
            nFile.WriteAllText (xFilePath, "iManagedFileTester.TestEverything");

            int [] xWidthHeightArray = { 1, 199, 200, 201, 1599, 1600, 1601, 6000 };
            string [] xImageFilePaths = new string [xWidthHeightArray.Length];

            for (int temp = 0; temp < xWidthHeightArray.Length; temp ++)
            {
                int xWidthHeight = xWidthHeightArray [temp];

                using (Bitmap xImage = new Bitmap (xWidthHeight, xWidthHeight))
                {
                    xImageFilePaths [temp] = nApplication.MapPath ($"Image{xWidthHeight.nToString ()}.png");
                    xImage.Save (xImageFilePaths [temp], ImageFormat.Jpeg);
                }
            }

            nManagedFilePathMode [] xPathModes =
            {
                nManagedFilePathMode.Base36,
                nManagedFilePathMode.Guid,
                nManagedFilePathMode.SafeCode
            };

            string xDestDirectoryPath = nApplication.MapPath ("Test"),
                xUtcString = DateTime.UtcNow.nToString (nDateTimeFormat.MinimalDateTimeUniversal);

            foreach (nManagedFilePathMode xPathMode in xPathModes)
            {
                // Sat, 28 Sep 2019 13:59:39 GMT
                // それぞれのモードについて、それぞれのファイルの管理ファイルを作り、展開し、内容を吐き、消す
                // 画像の方では、縮小版を生成し、全て消してしまう Delete より先に DeleteAdditionalFiles を呼ぶ
                // 各回の Append で必ず空行がつく出力なので、最後に長さを2減らすことで余分を消す
                // その後、他のテストコードでは行わないことも多いが、行っても問題のないこととして掃除を行う
                // レポートのファイルに全てのパスが入るため、ファイルを見て得られる情報はない
                // 落ちなければ成功なので、なぜ表示されないのかあとで迷わないために OK を表示

                nManagedFileUtility.PathMode = xPathMode;
                StringBuilder xBuilder = new StringBuilder ();

                nManagedFile xFile = nManagedFileUtility.Manage (xDestDirectoryPath, xFilePath, true);
                xBuilder.Append ($"[{xFilePath}]\r\n{xFile.ToDebugString ()}\r\n");
                xFile.Delete ();

                for (int temp = 0; temp < xWidthHeightArray.Length; temp ++)
                {
                    nManagedFile xImageFile = nManagedFileUtility.Manage (xDestDirectoryPath, xImageFilePaths [temp], true);
                    xImageFile.CreateOrUpdateAdditionalImages ();
                    xBuilder.Append ($"[{xImageFilePaths [temp]}]\r\n{xImageFile.ToDebugString ()}\r\n");
                    xImageFile.DeleteAdditionalFiles ();
                    xImageFile.Delete ();
                }

                xBuilder.Length -= 2;
                string xFilePathAlt = nPath.Combine (nPath.DesktopDirectoryPath, $"{xPathMode.nToString ()} ({xUtcString}).txt");
                nFile.WriteAllText (xFilePathAlt, xBuilder.ToString ());
            }

            nDirectory.Delete (xDestDirectoryPath);
            nFile.Delete (xFilePath);

            foreach (string xImageFilePath in xImageFilePaths)
                nFile.Delete (xImageFilePath);

            Console.WriteLine ("iManagedFileTester.TestEverything: OK");
        }
    }
}

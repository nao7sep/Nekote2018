using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nManagedFileUtility
    {
        // Sun, 05 May 2019 19:21:49 GMT
        // 元々は GUID をパスに入れていたが、あまりに長いので Base36 を入れるのをデフォルトに変更
        // しかし、それでは古いプログラムの派生開発において困るため、PathMode で古い動作に戻せるようにする

        // Sat, 28 Sep 2019 02:20:02 GMT
        // nSafeCode に書いた理由により、デフォルトを SafeCode に変更

        public static nManagedFilePathMode PathMode { get; set; } = nManagedFilePathMode.SafeCode;

        private static string iGetRandomString ()
        {
            if (PathMode == nManagedFilePathMode.Base36)
                return nRandom.Next ().nToBase36String ();
            else if (PathMode == nManagedFilePathMode.Guid)
                return nGuid.New ().nToString ();
            else if (PathMode == nManagedFilePathMode.SafeCode)
                return nSafeCode.Next ();
            else throw new nBadOperationException ();
        }

        // Wed, 07 Nov 2018 20:58:26 GMT
        // 200, 400, 800, 1600, 3200 の五つという意味合いで5にしてあるが、
        // ここの画像の縮小版はこの大きさまで、あっちはこの大きさまで、という実装の区別もあり得るため、
        // MaxResizedImageCount を勝手に設定する多重定義は、作るのも使うのもやめておく
        // ただ、小規模のプログラムなら set でサクッと変更できると楽なので、一応は用意してある

        // Sat, 28 Sep 2019 03:22:13 GMT
        // 結局、maxResizedImageCount に初期値を与えた
        // 注意喚起のために同じことを毎回書かされるというのも不毛

        public static int MaxResizedImageCount { get; set; } = 5;

        // Wed, 07 Nov 2018 21:07:03 GMT
        // インデックスのように0からカウントするのでなく、1からなので注意
        // 実装においては、配列のうちインデックスが0のところには原版のパスなどを入れる

        // Mon, 24 Dec 2018 09:58:56 GMT
        // 原版が 100px のときに opt が生成されないバグがあったので修正した
        // 懐かしいが、縮小版が0個であることで2の0乗の100倍として 100px になっていた

        public static int GetResizedImageMaxWidthAndHeight (int number)
        {
            if (number >= 1)
                return (int) Math.Pow (2, number) * 100;
            else return 0;
        }

        // Wed, 07 Nov 2018 21:07:47 GMT
        // resized なので、「縮小化された画像」をいくつ作ることができるかを返す
        // たとえば原版が縦横200ピクセルなら、それ未満がないため0が返り、
        // 801ピクセルなら 200, 400, 800 が作られるため3が返る

        // Sat, 28 Sep 2019 15:09:52 GMT
        // nManaged* が分かりにくい原因の一つになっていたので、仕様を変更した
        // 今までは、たとえば原版が800ピクセルなら800ピクセルの縮小版をこのメソッドはカウントしなかったが、
        // 400ピクセルを表示したり原版を表示したりはできないため、結局800ピクセルのものは必要
        // それなら、「原版以下」をこのメソッドでは数え、呼び出し側で、
        // それらとは別に、原版の最適化のみ行ったものを追加で用意するべきか判断するべき
        // 原版が800ピクセルのときに縮小版を400ピクセルまでにするのでは、
        // その追加の一つと800ピクセルの縮小版のところが曖昧で分かりにくくなる

        public static int GetResizedImageCount (int width, int height, int maxCount)
        {
            int xCount = 0;

            for (int temp = 1; temp <= maxCount; temp ++)
            {
                int xMaxWidthAndHeight = GetResizedImageMaxWidthAndHeight (temp);

                if (xMaxWidthAndHeight <= width && xMaxWidthAndHeight <= height)
                {
                    xCount = temp;
                    continue;
                }

                break;
            }

            return xCount;
        }

        // Thu, 08 Nov 2018 01:56:07 GMT
        // nManagedFile としてファイルを配置すると同時に縮小版まで作っていたのをやめた
        // それは、CRUD 的に、より高機能なことを nManagedFile 側で何度でも行えるようにするべき
        // 他のところにも書いたが、GUID をファイルの配置に使うようになったので衝突の可能性は天文学的に低くなった
        // となると *ThreadUnsafe としなくてもいいが、可能性としてゼロでないことが絶対にないとみなすコーディングが好きでない

        // Mon, 24 Dec 2018 07:26:37 GMT
        // ファイル名を指定できないのが不便だったので、newFileName を途中に挟み込んだ
        // 元々のパラメーターのものを多重定義として用意し、こちらを呼ぶようにしてある

        // Sun, 31 Mar 2019 00:15:21 GMT
        // nikoP を走らせていて、写真のアップロードが終わってからのサーバー側での縮小版などの作成中に他のページを開けないのが気になった
        // ManageThreadUnsafe が画像処理まで行うからロックが長引いているのかと思って調べたら、
        // nikoP の方がロック内で CreateOrUpdateAdditionalImages を呼んでいた
        // ManageThreadUnsafe については、高速化するなら、while ループのところだけを *ThreadUnsafe として抜き出し、
        // それら以外の処理を他のメソッドにまとめ、取得したパスに対しての処理をロック外で行う選択肢がある
        // しかし、ManageThreadUnsafe は、move モードで呼ばれることが多く、その場合の処理時間は極めて短い
        // nikoP でも、一時パスの取得のためにまずロックし、ロック外でファイルの保存などを行い、またロックして Manage* を呼んでいる
        // その二つ目のロックの中でなぜか CreateOrUpdateAdditionalImages まで行うのは、たぶんバグなので修正を試みる

        // Sun, 05 May 2019 19:20:41 GMT
        // GUID で決め打ちにしていた実装を Base36 に変更した上、
        // PathMode 次第では旧来の動作にも戻せるようにした

        // Sat, 28 Sep 2019 02:21:05 GMT
        // より安全な SafeCode を導入したため、デフォルトをそちらに切り替えた
        // Guid も Base36 も引き続き使えるが、全面的に勝る SafeCode の使用が望ましい

        // Sat, 28 Sep 2019 03:22:13 GMT
        // 結局、maxResizedImageCount に初期値を与えた
        // 注意喚起のために同じことを毎回書かされるというのも不毛

        public static nManagedFile Manage (string destDirectoryPath, string sourceFilePath,
            string newFileName, bool makesCopy, int? maxResizedImageCount = null)
        {
            string xFirstRandomString;

            while (true)
            {
                xFirstRandomString = iGetRandomString ();

                if (nPath.CanCreate (nPath.Combine (destDirectoryPath, xFirstRandomString)))
                    break;
            }

            nImageInfo xImageInfo = nImage.GetImageInfo (sourceFilePath);

            string xNewFileName = string.IsNullOrEmpty (newFileName) == false ?
                    newFileName : nPath.GetName (sourceFilePath),
                xDestFilePath;

            if (xImageInfo != null)
                xDestFilePath = nPath.Combine (destDirectoryPath, xFirstRandomString, iGetRandomString (), xNewFileName);
            else xDestFilePath = nPath.Combine (destDirectoryPath, xFirstRandomString, xNewFileName);

            if (makesCopy)
                nFile.Copy (sourceFilePath, xDestFilePath);
            else nFile.Move (sourceFilePath, xDestFilePath);

            // Thu, 08 Nov 2018 02:00:14 GMT
            // iPhone でメールに画像を添付すると、JPEG なのに .png になることがあるようだ
            // BMP なのに .jpg とする人もいるし、拡張子を書き換えたらファイルの変換になると思う人もいるようだし、
            // なんだかんだ言って、拡張子のおかしいファイルがけっこう届くため、強引だが、ここで修正しておく

            // Sat, 28 Sep 2019 04:30:01 GMT
            // Copy や Move のあとにパスだけを変更しても……と思ったが、
            // どうやら、このメソッドは、名前からイメージしにくいが、改名も行うようである
            // Path.ChangeExtension のような感覚で使ってはいけないため注意

            if (xImageInfo != null)
                xDestFilePath = nImage.CorrectExtension (xDestFilePath, xImageInfo.Format);

            return new nManagedFile (xDestFilePath, xImageInfo, maxResizedImageCount);
        }

        // Sun, 05 May 2019 20:59:21 GMT
        // 呼び出し側で lock を行い、もっといろいろと行う可能性が高いため、
        // 中途半端に Manage のみ行う *AutoLock をここでは廃止した

        public static nManagedFile Manage (string destDirectoryPath,
                string sourceFilePath, bool makesCopy, int? maxResizedImageCount = null) =>
            Manage (destDirectoryPath, sourceFilePath, null, makesCopy, maxResizedImageCount);

        // Thu, 08 Nov 2018 10:50:49 GMT
        // CreateOrUpdateAdditionalImages のところにも書いたが、携帯で撮った写真がかなりのアンダーになりやすい
        // 最近の携帯ならマシなのだろうが、私のものは4、5年前のものであり、レベル補正のために画像を分析したときに maxValue が200を切るようなこともよくある
        // ウェブシステムにアップロードされた全ての画像に画一的な処理を施すようなことは、基本的にはやめておきたいが、上記についてはメリットが極めて大きい
        // 一方で、暗部をさわると、コントラストの低いテーブルで撮った印刷物に強い陰影が生じて不気味になるとか、肌がどす黒くなるとかなので、これはやめておいた方がいい
        // また、ちゃんとしたカメラで撮った写真をアップロードすることも多いため、変なエッジが生じたりしうるアンシャープマスクもデフォルトではオフがいい
        // JPEG 品質については、Magick.NET の方にコードのコメントとして75がデフォルト値であるかのようなことが書かれているし、.NET もそうだとどこかで読んだ記憶がある
        // MaxResizedImageCount 同様、プログラム全体に影響するプロパティーであり、set を公開しておくことには抵抗を感じるが、
        // こちらは引数が四つになって、全て指定するとコードがうるさくなるし、MaxResizedImageCount と異なり、
        // 暗部をさわらず、明部も少ししかさわらず、アンシャープマスクをかけず、デフォルト的な JPEG 品質にするという仕様が安定しているし、
        // 今回の実装では nManagedFile* で縮小版をいったん消したり作り直したりを自由に行えるようにしたため、
        // これら四つを自動的に設定する多重定義を用意することに大きな問題はない
        // https://www.imagemagick.org/script/command-line-options.php#quality
        // https://github.com/dlemstra/Magick.NET/blob/master/Source/Magick.NET/Core/MagickImage.cs
        // http://stackoverflow.com/questions/3957477/what-quality-level-does-image-save-use-for-jpeg-files

        // Sat, 28 Sep 2019 15:18:26 GMT
        // ImageMagick についていろいろと調べていると、自動コントラストのデフォルトのパラメーターについて情報が錯綜しているのが見受けられる
        // 結論として現行の初期値を変えないのが決まっているため具体例をググってまで示さないが、もっと強く補正すると言う人、もっと弱いと言う人など、バラバラである
        // ドキュメントとソースの不整合もあるようで、そのソースの方も時期によって紆余曲折があったようで、最新のソースのパラメーターが最善という保証もない
        // Photoshop の自動コントラストが優秀なので、以前には、同様の結果になるパラメーターを探したこともあるが、うまくいかなかった
        // あれは、ヒストグラム的な統計情報に基づいて下端と上端を決めるだけでなく、より複雑な分析によってトーンカーブもいじっているのだろうと思う
        // 簡単なレベル補正しか実装しない以上、全ての状況においてうまくいくパラメーターというものはなく、本家の「今の」値をそのまま導入するのも頼りない
        // そのため、nikoP の数千枚の写真をそれなりに良好に処理できていて、目立った不具合のない現行のパラメーターを今後も使っていく

        public static double BlackPointForLevel { get; set; } = -1;

        public static double WhitePointForLevel { get; set; } = 1;

        public static bool AppliesUnsharpMask { get; set; } = false;

        public static int JpegQuality { get; set; } = 75;
    }
}

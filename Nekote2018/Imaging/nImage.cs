using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;

namespace Nekote
{
    public static class nImage
    {
        // 以下、「ImageFormat <=> nImageFormat <=> 拡張子」の変換メソッドを揃えておく
        // Nekote がサポートしているフォーマット以外については nNotSupportedException を投げるため注意
        // 照合は、何となくそうだろうと思う使用頻度の順でなく、揺るぎないアルファベット順に行われる
        // switch や if / else は、今日びのコンピューターではコストとして無視できる

        public static ImageFormat nToImageFormat (this nImageFormat format)
        {
            switch (format)
            {
                case nImageFormat.Bmp:
                    return ImageFormat.Bmp;
                case nImageFormat.Gif:
                    return ImageFormat.Gif;
                case nImageFormat.Jpeg:
                    return ImageFormat.Jpeg;
                case nImageFormat.Png:
                    return ImageFormat.Png;
                case nImageFormat.Tiff:
                    return ImageFormat.Tiff;
            }

            throw new nNotSupportedException ();
        }

        public static ImageFormat nToImageFormatOrDefault (this nImageFormat format, ImageFormat defaultValue)
        {
            switch (format)
            {
                case nImageFormat.Bmp:
                    return ImageFormat.Bmp;
                case nImageFormat.Gif:
                    return ImageFormat.Gif;
                case nImageFormat.Jpeg:
                    return ImageFormat.Jpeg;
                case nImageFormat.Png:
                    return ImageFormat.Png;
                case nImageFormat.Tiff:
                    return ImageFormat.Tiff;
            }

            return defaultValue;
        }

        public static nImageFormat nToNImageFormat (this ImageFormat format)
        {
            if (format.Guid == ImageFormat.Bmp.Guid)
                return nImageFormat.Bmp;
            else if (format.Guid == ImageFormat.Gif.Guid)
                return nImageFormat.Gif;
            else if (format.Guid == ImageFormat.Jpeg.Guid)
                return nImageFormat.Jpeg;
            else if (format.Guid == ImageFormat.Png.Guid)
                return nImageFormat.Png;
            else if (format.Guid == ImageFormat.Tiff.Guid)
                return nImageFormat.Tiff;
            else throw new nNotSupportedException ();
        }

        public static nImageFormat nToNImageFormatOrDefault (this ImageFormat format, nImageFormat defaultValue)
        {
            if (format.Guid == ImageFormat.Bmp.Guid)
                return nImageFormat.Bmp;
            else if (format.Guid == ImageFormat.Gif.Guid)
                return nImageFormat.Gif;
            else if (format.Guid == ImageFormat.Jpeg.Guid)
                return nImageFormat.Jpeg;
            else if (format.Guid == ImageFormat.Png.Guid)
                return nImageFormat.Png;
            else if (format.Guid == ImageFormat.Tiff.Guid)
                return nImageFormat.Tiff;
            else return defaultValue;
        }

        // 拡張子から nImageFormat にするときにはエイリアス的なものにも対応するが、
        // その逆のときには、配列を返すような面倒なことをせず、もっとも一般的なものを決め打ちで返す

        public static nImageFormat FileExtensionToNImageFormat (string extension)
        {
            string xLowerExtension = extension.nToLower ();

            switch (xLowerExtension)
            {
                case ".bmp":
                    return nImageFormat.Bmp;
                case ".gif":
                    return nImageFormat.Gif;
                case ".jpeg":
                    return nImageFormat.Jpeg;
                case ".jpg":
                    return nImageFormat.Jpeg;
                case ".png":
                    return nImageFormat.Png;
                case ".tif":
                    return nImageFormat.Tiff;
                case ".tiff":
                    return nImageFormat.Tiff;
            }

            throw new nNotSupportedException ();
        }

        public static nImageFormat FileExtensionToNImageFormatOrDefault (string extension, nImageFormat format)
        {
            string xLowerExtension = extension.nToLower ();

            switch (xLowerExtension)
            {
                case ".bmp":
                    return nImageFormat.Bmp;
                case ".gif":
                    return nImageFormat.Gif;
                case ".jpeg":
                    return nImageFormat.Jpeg;
                case ".jpg":
                    return nImageFormat.Jpeg;
                case ".png":
                    return nImageFormat.Png;
                case ".tif":
                    return nImageFormat.Tiff;
                case ".tiff":
                    return nImageFormat.Tiff;
            }

            return format;
        }

        public static string nImageFormatToFileExtension (nImageFormat format)
        {
            switch (format)
            {
                case nImageFormat.Bmp:
                    return ".bmp";
                case nImageFormat.Gif:
                    return ".gif";
                case nImageFormat.Jpeg:
                    return ".jpg";
                case nImageFormat.Png:
                    return ".png";
                case nImageFormat.Tiff:
                    return ".tif";
            }

            throw new nNotSupportedException ();
        }

        public static string nImageFormatToFileExtensionOrDefault (nImageFormat format, string extension)
        {
            switch (format)
            {
                case nImageFormat.Bmp:
                    return ".bmp";
                case nImageFormat.Gif:
                    return ".gif";
                case nImageFormat.Jpeg:
                    return ".jpg";
                case nImageFormat.Png:
                    return ".png";
                case nImageFormat.Tiff:
                    return ".tif";
            }

            return extension;
        }

        // 縦横の指定された長方形に内接するように、縦横比を保って画像をリサイズした場合の新しい縦横を計算
        // 小数点以下を切り捨てるのではわずかに縦横比が変わる可能性を考え、念のために Math.Round をかましている
        // ランダムな引数によるテストを何億回も行ったが、fit* が max* を超えることは一度も確認されなかった

        // Thu, 08 Nov 2018 02:43:13 GMT
        // isEnlarged を追加し、拡大されたかどうかが分かるようにした
        // ついでに実装も見たが、xWidth < maxWidth && xHeight < maxHeight が成立するのは縦横が初めから要求に満たないときで、
        // まず横、次に縦を縮小化する前半のコードの結果として上記が成立することはないため、そこから拡大してのデータの欠損はない

        public static void GetFitSize (int maxWidth, int maxHeight, int width, int height,
            bool enlarges, out int fitWidth, out int fitHeight, out bool isEnlarged)
        {
            double xWidth = width,
                xHeight = height;

            if (xWidth > maxWidth)
            {
                xHeight = xHeight * maxWidth / xWidth;
                xWidth = maxWidth;
            }

            if (xHeight > maxHeight)
            {
                xWidth = xWidth * maxHeight / xHeight;
                xHeight = maxHeight;
            }

            isEnlarged = false;

            if (enlarges)
            {
                if (xWidth < maxWidth && xHeight < maxHeight)
                {
                    // 縦を最大にしたときに横が最大に満たないか、あるいは最大と一致するなら、
                    // 逆に、横を最大にすれば、縦は最大を超えるか最大と一致するということである

                    double xTempWidth = xWidth * maxHeight / xHeight;

                    if (xTempWidth <= maxWidth)
                    {
                        xWidth = xTempWidth;
                        xHeight = maxHeight;
                    }

                    else
                    {
                        xHeight = xHeight * maxWidth / xWidth;
                        xWidth = maxWidth;
                    }

                    isEnlarged = true;
                }
            }

            fitWidth = (int) Math.Round (xWidth);
            fitHeight = (int) Math.Round (xHeight);
        }

        // FromStream に validateImageData: false を指定することで画像の情報を高速に読み出す
        // 縦横の取得においては Exif による回転も考慮して完了なので、そういう戻り値も用意している
        // https://www.roelvanlisdonk.nl/2012/02/28/fastest-way-to-read-dimensions-from-a-picture-image-file-in-c/

        // テストのため、手元の全てのファイルをこのメソッドに通してみた
        // .svn 内も含めて、画像でないものが338039ファイル、画像が97514ファイルあり、
        // そのうち、GIF アニメーションが162個、回転の必要なものが15660個だった
        // 処理が滞ったり、例外が投げられたりすることは一度もなかった

        public static bool GetImageInfo (string path, out nImageFormat format,
            out bool isAnimated, out int orientation, out int width, out int height)
        {
            // ファイルが存在しないことによる例外は投げさせたいため try 外としている

            using (FileStream xStream = File.OpenRead (path))
            {
                // 以降、画像の扱いにおいて問題が一つでも起これば、画像でないものとみなす
                // 「画像が表示されてません」の報告が入ってからファイルと実装をチェックすればよい

                try
                {
                    using (Image xImage = Image.FromStream (xStream, false, false))
                    {
                        // ここで nImageFormat に変換できないフォーマットには対応しない
                        format = xImage.RawFormat.nToNImageFormat ();

                        if (format == nImageFormat.Gif)
                            // FrameDimension.Page でなく、FrameDimension.Time を指定するべきのようだ
                            // この実装であらゆる GIF ファイルに対応できるかは不明だが、敢えて try / catch に入れず様子を見る
                            // https://msdn.microsoft.com/en-us/library/system.drawing.image.getframecount.aspx
                            // https://msdn.microsoft.com/en-us/library/system.drawing.imaging.framedimension.aspx
                            // http://stackoverflow.com/questions/11856210/trying-to-iterate-over-the-frames-in-a-system-drawing-image-throws-invalid-para
                            isAnimated = xImage.GetFrameCount (FrameDimension.Time) > 1;
                        else isAnimated = false;

                        // まずは「回転なし」を設定
                        orientation = 0;

                        // Exif は、JPEG と TIFF のみ想定すれば足りるようだ
                        // https://en.wikipedia.org/wiki/Exif
                        if (format == nImageFormat.Jpeg || format == nImageFormat.Tiff)
                        {
                            // Orientation は 0x112 のところに int16u として格納されている
                            // http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/EXIF.html
                            // http://stackoverflow.com/questions/27835064/get-image-orientation-and-rotate-as-per-orientation

                            if (xImage.PropertyIdList.Contains (0x112))
                                orientation = (ushort) xImage.GetPropertyItem (0x112).Value [0];
                        }

                        width = xImage.Width;
                        height = xImage.Height;
                        return true;
                    }
                }

                catch
                {
                    // 参照されない戻り値なので default に丸投げしている
                    // format が Nekote としては不正な0となるのも問題なし

                    format = default;
                    isAnimated = default;
                    orientation = default;
                    width = default;
                    height = default;
                    return false;
                }
            }
        }

        // Wed, 07 Nov 2018 19:40:22 GMT
        // 取得した画像データを使い回すには、単一のインスタンスが返ってくるものが便利
        // ファイルの存在チェックなどを行わず、画像でないなら null を返す

        public static nImageInfo GetImageInfo (string path)
        {
            if (GetImageInfo (path, out nImageFormat xFormat, out bool xIsAnimated,
                out int xOrientation, out int xWidth, out int xHeight))
            {
                return new nImageInfo
                {
                    Format = xFormat,
                    IsAnimated = xIsAnimated,
                    Orientation = xOrientation,
                    Width = xWidth,
                    Height = xHeight
                };
            }

            else return null;
        }

        // Exif の Orientation に基づき、必要なら幅と高さを入れ替える

        public static void ApplyOrientation (ref int width, ref int height, int orientation)
        {
            // 1 = Horizontal (normal)
            // 2 = Mirror horizontal
            // 3 = Rotate 180
            // 4 = Mirror vertical
            // 5 = Mirror horizontal and rotate 270 CW
            // 6 = Rotate 90 CW
            // 7 = Mirror horizontal and rotate 90 CW
            // 8 = Rotate 270 CW
            // http://www.sno.phy.queensu.ca/~phil/exiftool/TagNames/EXIF.html
            if (5 <= orientation && orientation <= 8)
            {
                int xTemp = width;
                width = height;
                height = xTemp;
            }
        }

        // 回転後の縦横だけ欲しいときが多いため、そのためのメソッドを追加
        // やはり無駄の目立つ実装だが、実際のコストは微々たるものだろう

        public static bool GetImageSize (string path, out int width, out int height)
        {
#pragma warning disable IDE0059
            if (GetImageInfo (path, out nImageFormat xFormat,
                out bool xIsAnimated, out int xOrientation, out width, out height))
            {
                ApplyOrientation (ref width, ref height, xOrientation);
                return true;
            }
#pragma warning restore IDE0059

            return false;
        }

        // 画像であり、なおかつ Nekote がサポートするフォーマットかどうかを調べるメソッド
        // GetImageInfo のコードをコピーしてもよいが、不要な戻り値の取得はコストとして微々たるもの

        public static bool IsImage (string path)
        {
#pragma warning disable IDE0059
            return GetImageInfo (path, out nImageFormat xFormat,
                out bool xIsAnimated, out int xOrientation, out int xWidth, out int xHeight);
#pragma warning restore IDE0059
        }

        // Mon, 24 Sep 2018 08:24:04 GMT
        // 最初、BitmapMetadata だけ抜くメソッドと、そこから各項目を抜くメソッドに分けてみたが、
        // DateTaken などはデータの追加的ロードが必要で、ファイルが開かれていないと動かないようである

        public static DateTime? GetExifDateTaken (string path)
        {
            try
            {
                string xDateTakenString = null;

                // Sun, 31 Mar 2019 04:23:36 GMT
                // 何となく Thread を使っていたが、Task またはスレッドプールを使えとのこと
                // Thread は、一般的に思われているより作成コストが大きいそうである

                Task xTask = Task.Run (() =>
                {
                    try
                    {
                        using (FileStream xStream = File.OpenRead (path))
                        {
                            JpegBitmapDecoder xDecoder = new JpegBitmapDecoder (xStream,
                                BitmapCreateOptions.None, BitmapCacheOption.None);

                            // Mon, 24 Sep 2018 08:25:52 GMT
                            // DateTaken プロパティーは、Exif の仕様の形式になっている日時を読み出し、
                            // それをそのまま返してほしいのに、引数を指定しない ToString で文字列にしてくれる
                            // こうやってスレッドを作れば、ToString のためにカルチャーを変更しても、呼び出し側に影響しない
                            // なお、Thread.CurrentThread.ManagedThreadId により、上位スコープと違うスレッドなのも確認した
                            // http://www.dotnetframework.org/default.aspx/DotNET/DotNET/8@0/untmp/WIN_WINDOWS/lh_tools_devdiv_wpf/Windows/wcp/Core/System/Windows/Media/Imaging/BitmapMetadata@cs/1/BitmapMetadata@cs
                            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                            // Mon, 24 Sep 2018 08:32:45 GMT
                            // 上位スコープの変数へのアクセスについては、動く理由が分からず、やっていいことなのか不詳だった
                            // ちゃんと仕様として決まっていて、コリジョンの心配もなさそうなので、今後は普通にやっていく
                            // http://www.atmarkit.co.jp/fdotnet/extremecs/extremecs_06/extremecs_06_03.html
                            xDateTakenString = ((BitmapMetadata) xDecoder.Frames [0].Metadata).DateTaken;
                        }
                    }

                    catch
                    {
                        // Wed, 26 Sep 2018 16:34:41 GMT
                        // うっかりしていて、上位スコープの try / catch で足りると思っていた
                        // PNG を読もうとしたところ、JpegBitmapDecoder のところで豪快に落ちた
                    }
                });

                // Sun, 31 Mar 2019 04:24:30 GMT
                // Task は元々バックグラウンドとのことで、また、優先度を設定できない
                // 後者については、やろうと思えばできそうでもあるが、スレッドプールのデザイン上、やるべきでないとのこと

                // xThread.IsBackground = true;

                // Mon, 24 Sep 2018 08:30:25 GMT
                // こうすると、他のところが忙しいときにプログラムがほぼフリーズするか
                // xThread.Priority = ThreadPriority.BelowNormal;
                // xThread.Start ();
                // xThread.Join ();

                // Sun, 31 Mar 2019 04:25:56 GMT
                // Task に切り替えたので、Join から Wait に変更
                // Task は Thread と違って Dispose が可能だが、しなくてもいいという人が目立つ
                // また、Task.Run を using に入れたら、IDE が、書き間違えていないかの警告を出してきてうるさい
                // しかし、Dispose できるものを Dispose しないのは将来にわたって最善との保証がないため、一応やっておく

                xTask.Wait ();
                xTask.Dispose ();

                // Wed, 26 Sep 2018 16:35:23 GMT
                // ファイルが存在しないときや JPEG でないときにも null になる
                // 頻度が高そうなので、日時の構文解析より先にメソッドを抜ける

                if (xDateTakenString == null)
                    return null;

                if (DateTime.TryParseExact (xDateTakenString, "MM'/'dd'/'yyyy' 'HH':'mm':'ss",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime xDateTaken))
                    return xDateTaken;
            }

            catch
            {
            }

            return null;
        }

        // Tue, 04 Sep 2018 22:18:24 GMT
        // 既に画像であることが判明しているファイルの拡張子をファイルの内容に基づいて正す
        // iPhone が JPEG ファイルをなぜか .png として送ってくるなど、よく分からないことが最近多い
        // あるいはアプリに問題があるのかもしれないが、とりあえず、カジュアルに呼べるメソッドを置く
        // 形式の判別まで行うものを用意しないのは、それに依存しては呼び出し元が非効率的なロジックになるため

        public static string CorrectExtension (string path, nImageFormat format)
        {
            string xExtension = Path.GetExtension (path),
                xCorrectExtension = nImageFormatToFileExtension (format);

            if (nString.Compare (xExtension, xCorrectExtension, true) != 0)
            {
                // Tue, 04 Sep 2018 22:33:26 GMT
                // 拡張子が全くない可能性も想定し、大文字・小文字についても引き継ぐ
                // たまにある image.PNG などがそのまま image.JPG になることを考えても、
                // デジカメで撮って出しのファイル名の拡張子だけが小文字になるのを防ぐなどのメリットが大きい
                // 末尾しか見ないため image.pnG などにだまされるが、そういうのは大きな問題でない

                bool xIsUpper = string.IsNullOrEmpty (xExtension) == false &&
                    char.IsUpper (xExtension [xExtension.Length - 1]);

                string xCorrectPath = Path.ChangeExtension (path, xIsUpper ?
                    xCorrectExtension.nToUpper () : xCorrectExtension);

                nFile.Move (path, xCorrectPath);
                return xCorrectPath;
            }

            // Tue, 04 Sep 2018 22:36:29 GMT
            // とりあえず呼び、ファイル名が変更されたかどうか気にせず処理を続行したい
            return path;
        }

        // タスクリストのタイトルごとにウィンドウを色分けするようなことに使えるメソッドを用意しておく
        // 思い付いたときには Nekote に入れるべきかどうか迷ったが、よく考えると汎用性が低いわけでもない
        // ARGB を使うのは、.NET でよく使われているため、また、A を無視するときに RGB へのアクセスに無駄が少ないため
        // Wikipedia では RGBA が扱われているが、それでは B の抽出にもビットシフトが必要
        // 今後、ARGB と RGBA の両方の実装を揃える可能性があるため、メソッド名に Argb を入れておく
        // https://en.wikipedia.org/wiki/RGBA_color_space

        public static int ToArgbColor (string text)
        {
            // null または "" のときには、nGetHashCode にならって0を返す
            // ハッシュがうまくバラけるなら、0という色も立派な正常値である

            if (string.IsNullOrEmpty (text))
                return 0;

            // Java のアルゴリズムは、入力文字列が短いときに上位ビットがほとんど0のハッシュを返す
            // アルゴリズムをいじることなく1文字からでも多様な色を得るには、文字列を長くするのが仕様としてきれい
            // 最初は GUID をつなげることを考えたが、色をバラけさせるだけなら長すぎて無駄があるため却下
            // */t のようにするのは、ファイルシステムでも同名のファイルの衝突を避けるにはディレクトリーを区別するため
            // *:t や t+* なども選択肢だが、衝突の回避を念頭に置くなら、階層構造を与えるのが一番しっくりくる
            // キャッシュや KVS に使うキーも URL も / で階層構造が与えられるため、それらとも整合する
            return ("ToArgbColor/" + text).nGetHashCode ();
        }

        // 二つの色を、それぞれの使用量を指定してブレンドするメソッド
        // Math.Round も除算も使っていて遅いが、まずは精度を優先しての実装を行った
        // 高速化を考えるなら、二つの amount* の比率が制限されるが、ビット演算での実装も可能だろう

        public static int BlendArgbColors (int color1,
            double amount1, int color2, double amount2)
        {
            // もしかすると高速化にならないかもしれないが、とりあえず
            double xTotalAmount = amount1 + amount2;

            // 演算子の優先度は、>> や << が & や | より高い
            // https://ja.wikipedia.org/wiki/%E6%BC%94%E7%AE%97%E5%AD%90%E3%81%AE%E5%84%AA%E5%85%88%E9%A0%86%E4%BD%8D

            int xAlpha = (int) Math.Round (((color1 >> 24 & 0xff) * amount1 + (color2 >> 24 & 0xff) * amount2) / xTotalAmount),
                xRed = (int) Math.Round (((color1 >> 16 & 0xff) * amount1 + (color2 >> 16 & 0xff) * amount2) / xTotalAmount),
                xGreen = (int) Math.Round (((color1 >> 8 & 0xff) * amount1 + (color2 >> 8 & 0xff) * amount2) / xTotalAmount),
                xBlue = (int) Math.Round (((color1 & 0xff) * amount1 + (color2 & 0xff) * amount2) / xTotalAmount);

            return xAlpha << 24 | xRed << 16 | xGreen << 8 | xBlue;
        }

        // 決め打ちの仕様でパステルカラーを生成するメソッドを用意しておく
        // ネットでしっかり探したが、パステルカラーの明確な定義は存在しないようだった
        // 混合比としてはここでは 3:7 を採用しているが、これは、約1/3にあたる30％を可変とするという考え方であり、
        // だいたい何事においてもうまくいくという経験則のある3という数字にとりあえず依拠しての仕様である
        // 2:8 でもよいし、1:5 も悪くないが、そこまでいくと今度は色の区別が弱まり、ユーザービリティーの低下が起こりうる
        // 1:1 でもパステルカラーと呼ぶことにあまり抵抗のない色になるため、3に仕様としての安定性を期することにする

        public static int ToPastelArgbColor (int color)
        {
            // (int) -1 は、(uint) 0xffffffff である
            // ここでは unchecked なしで渡しても問題ないようだ
            return BlendArgbColors (color, 3, -1, 7);
        }

        // .NET には System.Drawing.Color と System.Windows.Media.Color の二つがある
        // 前者は int とのラウンドトリップが容易だが、後者はやや癖があるため、分かりやすく両方ともラップしておく

        // ToDrawingColor という名前を最初に考えたが、それではもう一つが ToWindowsColor なのか ToMediaColor なのか迷う
        // また、.NET の旧来の画像処理クラスは、System.Drawing と System.Drawing.Imaging に分散していて、
        // 今後、同様のクラスを追加していくにおいて、Drawing を接頭辞として使うことが必ずしも正しいとは限らない
        // そもそも、drawing color というのは何物なのかという、将来的な分かりにくさの問題もある
        // そのため、.NET に WPF が載っかっているという考え方で、Net / Wpf を接頭辞のように使っている

        public static Color ToNetColor (int argbColor)
        {
            // https://msdn.microsoft.com/en-us/library/2zys7833.aspx
            return Color.FromArgb (argbColor);
        }

        public static int NetColorToArgbColor (Color color)
        {
            // https://msdn.microsoft.com/en-us/library/system.drawing.color.toargb.aspx
            return color.ToArgb ();
        }

        public static System.Windows.Media.Color ToWpfColor (int argbColor)
        {
            // fixed でポインターが使われているため、int にビット演算を施し、それぞれを byte にキャストするより速そう
            // https://msdn.microsoft.com/en-us/library/system.bitconverter.aspx
            // http://referencesource.microsoft.com/#mscorlib/system/bitconverter.cs
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/fixed-statement
            byte [] xBytes = BitConverter.GetBytes (argbColor);

            // それぞれの引数が byte なので、BitConverter で取得した値をそれぞれ指定
            // 順序については、Windows 用のプログラムしか書いていないため決め打ちでも大丈夫だが、
            // なぜ 3, 2, 1, 0 なのかのコメントを書くより、後学につながるコード例を残す方が良さそう
            // https://msdn.microsoft.com/en-us/library/system.bitconverter.islittleendian.aspx
            // https://msdn.microsoft.com/en-us/library/system.windows.media.color.fromargb.aspx

            if (BitConverter.IsLittleEndian)
                return System.Windows.Media.Color.FromArgb (xBytes [3], xBytes [2], xBytes [1], xBytes [0]);
            else return System.Windows.Media.Color.FromArgb (xBytes [0], xBytes [1], xBytes [2], xBytes [3]);
        }

        public static int WpfColorToArgbColor (System.Windows.Media.Color color)
        {
            // こちらは論理的な順序なので、OS のバイトオーダーを気にする必要がない
            // A などは byte だが << を通るときに int として扱われるようだ
            return color.A << 24 | color.R << 16 | color.G << 8 | color.B;
        }

        // int と HTML のカラーコードの相互変換をできるようにしておく

        public static string ToHtmlColorCode (int argbColor)
        {
            // アルファーは、RGB にかけるなどするのでなく、単純に無視
            // 3桁にできる値でも、そうするコストに比べてメリットが小さいため6桁でいく
            return '#' + (argbColor & 0x00ffffff).nToString ("x6");
        }

        // MSDN を見ていても int の色の並びは ARGB がデフォルト扱いされているが、
        // それでも、いずれ RGBA 版を作ることになっても困らない命名としている

        public static int HtmlColorCodeToArgbColor (string text)
        {
            // 問題があるなら、null だとか範囲外だとかの例外が飛ぶに任せる
            // .NET 側が何かしら例外を投げる状況で Nekote も投げるのは、合理的でない
            // 開発効率も実行効率も低下する一方、通常、そこまで例外情報を見ないため
            string xText = text [0] == '#' ? text.Substring (1) : text;

            // 6桁の方をよく目にするため、先にそちらを扱う
            if (xText.Length == 6 || xText.Length == 3)
            {
                // 自前で構文解析した方が高速かもしれないが、頻繁に呼ぶメソッドでないためこのままでよい
                if (int.TryParse (xText, NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out int xColor))
                {
                    if (xText.Length == 6)
                        return
                            unchecked ((int) 0xff000000) |
                            xColor;

                    else
                    {
                        int xRed = xColor & 0x00000f00,
                            xGreen = xColor & 0x000000f0,
                            xBlue = xColor & 0x0000000f;

                        return
                            unchecked ((int) 0xff000000) |
                            xRed << 12 |
                            xRed << 8 |
                            xGreen << 8 |
                            xGreen << 4 |
                            xBlue << 4 |
                            xBlue;
                    }
                }
            }

            throw new nInvalidFormatException ();
        }
    }
}

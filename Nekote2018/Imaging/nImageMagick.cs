using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ImageMagick;
using System.Globalization;

namespace Nekote
{
    // Magick.NET を使っているが、クラス名は nImageMagick としている
    // Magick.NET の名前空間も ImageMagick だし、いかにラップされていようと、
    // Nekote が依存している機能は、最終的には ImageMagick のものであるため

    public static class nImageMagick
    {
        // MagickFormat には似ている項目が多数あり、一方でドキュメントに不足があるため、それらの使い分けが分かりにくい
        // そのため、まずは最もそれらしい最小限の実装にとどめ、今後の使用において不具合が発生するたびに対応する項目を増やしていく
        // https://github.com/dlemstra/Magick.NET/blob/master/Source/Magick.NET/Core/Enums/MagickFormat.cs

        public static MagickFormat nToMagickFormat (this nImageFormat format)
        {
            switch (format)
            {
                case nImageFormat.Bmp:
                    return MagickFormat.Bmp;
                case nImageFormat.Gif:
                    return MagickFormat.Gif;
                case nImageFormat.Jpeg:
                    return MagickFormat.Jpeg;
                case nImageFormat.Png:
                    return MagickFormat.Png;
                case nImageFormat.Tiff:
                    return MagickFormat.Tiff;
            }

            throw new nNotSupportedException ();
        }

        public static MagickFormat nToMagickFormatOrDefault (this nImageFormat format, MagickFormat defaultValue)
        {
            switch (format)
            {
                case nImageFormat.Bmp:
                    return MagickFormat.Bmp;
                case nImageFormat.Gif:
                    return MagickFormat.Gif;
                case nImageFormat.Jpeg:
                    return MagickFormat.Jpeg;
                case nImageFormat.Png:
                    return MagickFormat.Png;
                case nImageFormat.Tiff:
                    return MagickFormat.Tiff;
            }

            return defaultValue;
        }

        // 手元の全ての画像ファイルを MagickImage で開き、Format を確認したところ、
        // GIF, JPEG, PNG, TIFF は .NET や nImageFormat と同じ識別子の項目として検出されたが、
        // BMP は Bmp3 のみが得られ、インターネットで探しても Bmp となるファイルは見付からなかった
        // そこで、MagickImage.Format に Bmp を設定して Write したところ、このファイルは Bmp となった
        // これは、ACDSee や Windows でも表示に問題がなく、通常の BMP ファイルとして扱えそうだった
        // そのため、まずは Bmp を基本としつつ、既に存在の確認されている Bmp3 にも対応させる
        // ついでに Bmp2 にも対応させることが可能だが、これは存在が未確認なので様子を見る

        public static nImageFormat nToNImageFormat (this MagickFormat format)
        {
            if (format == MagickFormat.Bmp || format == MagickFormat.Bmp3)
                return nImageFormat.Bmp;
            else if (format == MagickFormat.Gif)
                return nImageFormat.Gif;
            else if (format == MagickFormat.Jpeg)
                return nImageFormat.Jpeg;
            else if (format == MagickFormat.Png)
                return nImageFormat.Png;
            else if (format == MagickFormat.Tiff)
                return nImageFormat.Tiff;
            else throw new nNotSupportedException ();
        }

        public static nImageFormat nToNImageFormatOrDefault (this MagickFormat format, nImageFormat defaultValue)
        {
            if (format == MagickFormat.Bmp || format == MagickFormat.Bmp3)
                return nImageFormat.Bmp;
            else if (format == MagickFormat.Gif)
                return nImageFormat.Gif;
            else if (format == MagickFormat.Jpeg)
                return nImageFormat.Jpeg;
            else if (format == MagickFormat.Png)
                return nImageFormat.Png;
            else if (format == MagickFormat.Tiff)
                return nImageFormat.Tiff;
            else return defaultValue;
        }

        // 以下、Magick.NET の関わる全てのメソッドで nImageMagickSettings.Apply を呼ぶ
        // 現時点では OpenCL くらいしかさわらないため、たとえば GetInfoForUnsafeLevel で呼ぶのは不要だが、
        // Apply 側を変更していく可能性もあるし、どれで呼んで、どれでは呼ばなくて、といった保守は利益が乏しい

        // MagickImage.Resize には、縦横の計算がおかしい点、無条件で拡大も行う点の二つの問題がある
        // 拡大は、よほど特殊なアルゴリズムを使うなどでないなら無駄に情報量を増やすだけのことが多く、
        // 「小さければ拡大も」という意図をもって resize を行うことは稀なので、デフォルトでオフにしている

        public static void nResize (this MagickImage image, int maxWidth, int maxHeight, bool enlarges = false)
        {
            nImageMagickSettings.Apply ();

            if ((image.Width > maxWidth || image.Height > maxHeight) ||
                (image.Width < maxWidth && image.Height < maxHeight && enlarges))
            {
                // Resize に縦横の計算を任せると、短辺の方が繰り上げられたかのようになることがある
                // ネットで調べても、この問題に困っている人は多いようで、クロップしろという提案すらあった
                // このメソッドでは、対策として、自前で計算した縦横への縮小化を行っている
                // そうしておけば、GetFitSize による結果が常に縮小版の大きさだという前提にも立てる

#pragma warning disable IDE0059
                nImage.GetFitSize (maxWidth, maxHeight, image.Width, image.Height, enlarges,
                    out int xWidth, out int xHeight, out bool xIsEnlarged);
#pragma warning restore IDE0059

                // ImageMagick では以下のフォーマットによって ! を添えることで縦横比を保た「ない」縮小化が可能
                // 計算に誤りがあり、むしろ ! を添えてこそ縦横比を保てているように思うが、とりあえず対策はできているはず
                // Windows のシェルでは ! のエスケープは基本的に不要のようで、そのあたりも問題はなさそうである
                // http://www.imagemagick.org/Usage/resize/
                // https://www.imagemagick.org/script/command-line-processing.php#geometry
                // http://stackoverflow.com/questions/6828751/batch-character-escaping
                // http://www.robvanderwoude.com/escapechars.php
                // http://www.robvanderwoude.com/variableexpansion.php
                // image.Resize (new MagickGeometry (string.Format (CultureInfo.InvariantCulture, "{0}x{1}!", xWidth, xHeight)));

                // 追記: 自ら ! を出力しなくても、プロパティーによってそれを行えることが判明したため、そちらを利用する実装に切り替えた
                // https://github.com/dlemstra/Magick.NET/blob/master/Source/Magick.NET/Core/Types/MagickGeometry.cs
                image.Resize (new MagickGeometry { Width = xWidth, Height = xHeight, IgnoreAspectRatio = true });
            }
        }

        // ImageMagick には、コントラストを調整するための機能がいくつか用意されている
        // そのうち、Nekote では、perfect mathematical normalization とされる auto-level を常用する
        // 通常、auto-level を適用するには、MagickImage.AutoLevel を呼ぶだけでよい
        // しかし、複数の縮小版を生成するときに AutoLevel を個別に呼ぶと、
        // 画素の少ないものほど、例外的に暗い点や明るい点が消失しやすく、
        // 同じ画像の縮小版なのに、大きさによって露出が異なるということが起こる
        // そういうことを避けるには、GetInfoForLevel を nLevel と組み合わせるべき
        // http://www.imagemagick.org/Usage/color_mods/#histogram

        // 追記: perfect を求めては、わずかな異常ピクセルでコントラスト不良が放置されることが相次いだ
        // JPEG で画像を扱っている時点でロスはあるため、総合的に考えたら良好な結果となる Unsafe* を追加した
        // 今後、「レベル補正」というと Unsafe* の方をデフォルトで使うようにしていく

        // 念のため、GetInfoForLevel + nLevel の結果が nAutoLevel と一致するのか調べた
        // GetPixels, ToArray による byte [] を比較したところ、いずれの画像でも完全に一致した

        public static void GetInfoForLevel (MagickImage image, out byte minValue, out byte maxValue)
        {
            nImageMagickSettings.Apply ();
            // Composite のコメントは Returns the statistics for all the channels となっている
            IChannelStatistics xStatistics = image.Statistics ().Composite ();

            // #region と同様、字下げを行わないことにする
            // #endif と対になってコードの一部を囲み、特別扱いする性質を持つものの、
            // あくまで補助的なものであり、{ } のようにコードの構造まで定義するものでない

            // Thu, 26 Sep 2019 23:18:51 GMT
            // こういうことをきっちり決める性格だが、忘れっぽいので、徹底はできない
            // いくつかのプログラムで #if DEBUG を字下げしてから、また上記のコメントを見た
            // 「プログラムの動作に影響するわけでないため、どっちでもいい」としか、もう言えない

            #if DEBUG
            // Q8 の Magick.NET なら Minimum などは byte の範囲に収まるはずなのに、なぜか double になっている
            // そのため、範囲外だったり、整数でなかったりのものがあるのかどうか、DEBUG のときだけ確認し、あれば例外を投げる

            if (xStatistics.Minimum < 0 || xStatistics.Minimum > byte.MaxValue ||
                    Math.Round (xStatistics.Minimum) != xStatistics.Minimum ||
                    xStatistics.Maximum < 0 || xStatistics.Maximum > byte.MaxValue ||
                    Math.Round (xStatistics.Maximum) != xStatistics.Maximum)
                throw new nDebugException ();
            #endif

            minValue = (byte) xStatistics.Minimum;
            maxValue = (byte) xStatistics.Maximum;
        }

        public static void nAutoLevel (this MagickImage image)
        {
            nImageMagickSettings.Apply ();
            image.AutoLevel (Channels.All);
        }

        public static void nLevel (this MagickImage image, byte minValue, byte maxValue)
        {
            nImageMagickSettings.Apply ();

            // 手元にある多数のランダムな画像でテストを行ってみて分かったが、ほとんどの画像は、最初から 0, 255 である
            // それはデジカメで撮って出しにしたものも同じで、ほぼ全ての写真で自動コントラストが効果的だった時代は終わったようだ
            // そのため、0, 255 なら一切の処理が行われないようにしている

            if (minValue > 0 || maxValue < byte.MaxValue)
                image.Level (minValue, maxValue, Channels.All);
        }

        // 画像データをバイト列にするだけの単純な処理だが、調べたことが多いのでメソッドを用意しておく
        // マッピングは、ARGB と RGBA がよく使われるが、Nekote では MSDN にならって ARGB をデフォルトとする
        // nImage の方のコメントにも書いたが、ARGB は、int で扱うときに B の読み出しにビットシフトが不要で少しだけ高速
        public static byte [] nToByteArray (this MagickImage image, string mapping = "ARGB")
        {
            nImageMagickSettings.Apply ();

            // GetPixels と GetPixelsUnsafe では、チェックの少ない後者の方が速いようだ
            // GetPixelsUnsafe は、PixelCollection を継承する UnsafePixelCollection のインスタンスを生成する
            // UnsafePixelCollection も PixelCollection も internal なので、参照の型には IPixelCollection を使う
            // → IUnsafePixelCollection <byte> で受けないと動かないようになったので変更した
            // https://github.com/dlemstra/Magick.NET/blob/master/Source/Magick.NET/Shared/MagickImage.cs
            // https://github.com/dlemstra/Magick.NET/blob/master/Source/Magick.NET/Shared/Pixels/UnsafePixelCollection.cs
            // https://github.com/dlemstra/Magick.NET/blob/master/Source/Magick.NET/Shared/Pixels/PixelCollection.cs
            using (IUnsafePixelCollection <byte> xPixels = image.GetPixelsUnsafe ())
            {
                // byte [] を返すものとしては、GetValues, ToArray, ToByteArray がある
                // GetValues は、GetAreaUnchecked → _nativeInstance.GetArea → QuantumConverter.ToArray となる
                // ToArray は GetValues を呼ぶだけ
                // ToByteArray はいきなり _nativeInstance.ToByteArray → ByteConverter.ToArray であり、
                // 結局、配列への変換が必要にはなるが、ImageMagick が mapping も受け取るため、おそらくこれが最速
                // なお、int length = width * height * Image.ChannelCount というコードが Magick.NET の他のところにあるため、
                // 画像は常に長方形（つまり、台形などはない）と考えてよく、ピクセル数は常に width * height で得られるだろう
                return xPixels.ToByteArray (mapping);
            }
        }

        // MagickImage には、ロスありでコントラストを最適化するメソッドもいくつかあり、十分に使い物になるが、
        // 同じ画像のサイズ違いの縮小版をいくつか生成するときには、縮小化の過程で異常ピクセルが消えたり消えなかったりに結果が影響される
        // そのため、データ量の最も多い原版で上下の切り取り幅を計算し、その結果を共通的に全ての縮小版に適用できるようにした

        // 追記: このメソッドでは、blackPoint が負なら暗部を絶対に削らず、0ならピクセルが全くない部分のみ削り、それより大きいなら％指定とみなす
        // 以前の実装では0なら全く削らない仕様にしていたが、GetInfoForLevel と同様の「安全な」処理にも対応したかったので、削らないのは負の指定に変更した

        public static void GetInfoForUnsafeLevel (MagickImage image,
            double blackPoint, double whitePoint, out byte minValue, out byte maxValue)
        {
            nImageMagickSettings.Apply ();

            // Histogram メソッドの返すものは、ラップまみれで、しかも Dictionary であり、やはり無駄が気になる
            // そのため、いったんは各色ごとのレベル補正を考えず、最小限のヒストグラム解析のみ行うコードをベタ書き

            // Nekote では ARGB がデフォルトだが、
            // 以下のコードにおいて効率的に処理できるのはこちら
            byte [] xBytes = image.nToByteArray ("RGBA");
            int [] xCounts = new int [256];
            // おそらく不要だが、初期化しないのは気になる
            nArray.Fill (xCounts, 0);
            // ミリオン単位で get のメソッドを呼びたくない
            int xLength = xBytes.Length;

            for (int temp = 0; temp < xLength; )
            {
                xCounts [xBytes [temp]] ++;
                temp ++;
                xCounts [xBytes [temp]] ++;
                temp ++;
                xCounts [xBytes [temp]] ++;
                temp += 2;
            }

            // ピクセルごとの L を HSL の算出によってとるようなことを考えていたが、
            // 実際には、RGB の関係性を無視して、展開した値をゴソッとひとまとめにしての処理で足りる
            // RGB 各色のヒストグラムを256段階それぞれで縦方向に足し算するところをイメージ

            // 追記: xTotalCount の * 3 を忘れていたなど、いろいろと質の低いコードがそのまま定着していた
            // 他にもいくつか改善すべき点があったため、これ以降のコードは、ゴッソリと書き直してみた

            int xTotalCount = image.Width * image.Height * 3,
                xLowCountToLose = (int) Math.Round (xTotalCount * blackPoint / 100),
                xHighCountToLose = (int) Math.Round (xTotalCount * whitePoint / 100);

            #if DEBUG
            // DEBUG モードのときに毎回チェックするようなことではなく、一度通ったらそれで OK だが、
            // 重い計算ではないため、和がこれと一致しなければならないんですよ、の確認を意図してノリで書いた

            if (xCounts.Sum () != xTotalCount)
                throw new nDebugException ();
            #endif

            // 理論的には、真っ黒や真っ白は50％のグレーになるべきだが、それはユーザーの感覚とズレた動作になる
            // Photoshop でいろいろと試したところ、白と黒だけはコントラスト最適化の影響を受けなかった
            // しかし、[255] でなく [254] に全ピクセルを割り振った、目視では同じく真っ白の画像は、50％のグレーになった
            // 一つの階調に全ピクセルがあろうと、ヒストグラム上で左端または右端でないものはガッツリ処理されるようである
            // 仕様として現実的だと思うので、このメソッドでも、真っ黒と真っ白だけは「変更なし」の戻り値にする

            if (xCounts [0] == xTotalCount ||
                xCounts [255] == xTotalCount)
            {
                minValue = 0;
                maxValue = 255;
                return;
            }

            // 残す暗部の最小値および明部の最大値を、以下、引数に基づいてそれぞれ求める
            // 暗部と明部の二つの処理には共通点が多いため、コメントは主に暗部の方にまとめる

            // null と比較して、必要に応じて例外を投げる
            // -1でもよいが、「未設定」を明示するならこの方が良い
            int? xMinValue = null;

            // Round で丸め込まれる分を考慮しても、-1くらいの値を blackPoint として指定しておけば、ここが true となる
            // この場合、xMinValue は0となり、暗部の側については [0] 以上の領域のデータが残されるという指定になる

            if (xLowCountToLose < 0)
                xMinValue = 0;

            else
            {
                // 以下、[0] から [temp] までの和を xSum とし、それが xLowCountToLose を超えたところで temp を xMinValue に設定する
                // 最初はサクッとそう実装していたが、コメントを怠ったからか、その後のチェック時に勘違いし、むしろ改悪してしまった
                // 具体的には、xSum > xLowCountToLose のみに任せず、xSum == xLowCountToLose も見て、
                // 後者の場合、[0] から [temp] までを消すわけだから、xMinValue には temp + 1 を設定するということをした
                // この実装の問題は、たとえば [0], [1], [2], [3] のピクセル数がそれぞれ 10, 20, 0, 30 のときに、
                // xLowCountToLose が30では、temp + 1 によって、ピクセルのない [2] のデータが残ることである
                // [temp - 1] までがぴったり一致だろうと、[temp - 1] が0だろうと、特定するべきは「これ以上は残すべき」の境界である

                int xSum = 0;

                for (int temp = 0; temp <= 255; temp ++)
                {
                    xSum += xCounts [temp];

                    if (xSum > xLowCountToLose)
                    {
                        xMinValue = temp;
                        break;
                    }
                }

                // [255] まで足しても xSum > xLowCountToLose が成立しないのは、blackPoint が100％以上のとき
                // 100％以上を削るというのは、画素情報を完全に欠落させてコントラスト最適化の処理の結果を不定とするということである
                // わざとそういうことをして利益のあるところでもないが、デバッグ時の利益なども考えて例外を投げておく

                if (xMinValue == null)
                    throw new nBadOperationException ();
            }

            int? xMaxValue = null;

            if (xHighCountToLose < 0)
                xMaxValue = 255;

            else
            {
                // 暗部のときと逆方向の処理になるが、「これ以上は残すべき」の境界を特定する処理は同じ
                // blackPoint と whitePoint の組み合わせに問題がある場合については、後ほど個別に対応

                int xSum = 0;

                for (int temp = 255; temp >= 0; temp --)
                {
                    xSum += xCounts [temp];

                    if (xSum > xHighCountToLose)
                    {
                        xMaxValue = temp;
                        break;
                    }
                }

                if (xMaxValue == null)
                    throw new nBadOperationException ();
            }

            // 理論上、1階調だけでも50％のところへの割り付けが可能なので、一致も容認している
            // 真っ黒や真っ白の検出のところと同様、OK なら return し、それ以外で例外を投げている

            if (xMinValue <= xMaxValue)
            {
                minValue = (byte) xMinValue.Value;
                maxValue = (byte) xMaxValue.Value;
                return;
            }

            throw new nBadOperationException ();
        }

        // Nekote を使用する各プロジェクトに 2, 1 を決め打ちで入れたくないため多重定義を用意した
        public static void GetInfoForUnsafeLevel (MagickImage image, out byte minValue, out byte maxValue)
        {
            nImageMagickSettings.Apply ();

            // 引数をこうする理由については、nUnsafeAutoLevel の方のコメントに書いた

            // Sat, 01 Dec 2018 20:52:39 GMT
            // 大きな仕様変更を行った
            // その理由についても nUnsafeAutoLevel のコメントに書いた
            // GetInfoForUnsafeLevel (image, 2, 1, out minValue, out maxValue);

            GetInfoForUnsafeLevel (image, -1, 1, out minValue, out maxValue);
        }

        // 同じ画像のサイズ違いの縮小版に同じ処理を施せるように GetInfoForUnsafeLevel を追加したため、
        // 今後のカスタマイズの可能性なども考えて、normalize と（ほぼ）同じことをそのあたりのコードで実装してみた
        // 内部で呼ばれる GetInfoForUnsafeLevel の結果を知りたいときがあるため、Tuple で返す
        // MinValue, MaxValue のみ持つ構造体またはクラスを用意しないのは、それでは構造体やクラスが増えすぎるため
        // ValueTuple を使う選択肢もあるが、Tuple の方が短いし、参照の中身を受け取り側で変更できた方が便利
        // https://msdn.microsoft.com/ja-jp/library/dd268536.aspx
        // https://msdn.microsoft.com/ja-jp/library/mt744804.aspx
        public static Tuple <byte, byte> nUnsafeAutoLevel (this MagickImage image)
        {
            nImageMagickSettings.Apply ();

            // -normalize is equivalent to -contrast-stretch 2%x1% とあるので、それにならっている
            // 3％も削るのは削りすぎとも思うため、0.2, 0.1 も考えたが、それではレベル補正の効果が認められない場合も出てくる
            // https://www.imagemagick.org/script/command-line-options.php#normalize

            // 実際にネットで low contrast で検索した多数の画像を処理して normalize と比較したところ、
            // たいてい同様の結果となりながらも、Nekote の方が暗かったり明るかったり、多少異なった特性の結果でもあった
            // 良好であることに変わりないため 2, 1 を引き続き使用するが、ピクセル単位での一致は起こらないので注意

            // 追記: normalize は contrast-stretch だが、私の実装は linear-stretch のようだ
            // となると、同じパラメーターを使うのもアレだが、多数の画像で試しての結果が良好なので、このまま使っていく

            // Sat, 01 Dec 2018 20:49:03 GMT
            // nManagedFile.CreateOrUpdateAdditionalImages のコメントに書いた理由により、こちらでもデフォルト値を変更
            // ImageMagick の仕様と異なるようになるし、片方だけ最適化するという仕様としてのアンバランスも気になるが、
            // イメージセンサーの特性、明るくするより暗くする方が（当然）容易であることなどを考えるなら、
            // 結果的にうまくいき、ハズレが少ないのは、アンダーに振れたものの救済のみ行う仕様である

            // return image.nUnsafeLevel (2, 1);
            return image.nUnsafeLevel (-1, 1);
        }

        // nLevel は切り取り幅を受け取るが、こちらは捨てる部分を％で受け取る
        // nUnsafeAutoLevel が返すためのインスタンスをこのメソッドが Create する
        public static Tuple <byte, byte> nUnsafeLevel (this MagickImage image, double blackPoint, double whitePoint)
        {
            nImageMagickSettings.Apply ();
            // 捨てる部分が％で指定されるため、そこから上下の切り取り幅を計算し、レベル補正には従来のメソッドを使う
            GetInfoForUnsafeLevel (image, blackPoint, whitePoint, out byte xMinValue, out byte xMaxValue);
            image.nLevel (xMinValue, xMaxValue);
            return Tuple.Create (xMinValue, xMaxValue);
        }

        public static void nUnsharpMask (this MagickImage image)
        {
            nImageMagickSettings.Apply ();
            // ImageMagick のデフォルトのパラメーターによってアンシャープマスクを適用
            // https://www.imagemagick.org/script/command-line-options.php#unsharp
            image.UnsharpMask (0, 1, 1, 0.05);
        }

        // これまで Write をそのまま呼んでいたが、nImageMagickSettings.Apply のためにラップした
        // Write が必ず上書きするのが気になっていたので対応し、さらに、必要に応じてディレクトリーを作るようにした

        public static void nWrite (this MagickImage image, string path, bool overwrites = false)
        {
            nImageMagickSettings.Apply ();

            if (overwrites || nFile.Exists (path) == false)
            {
                nDirectory.CreateForFile (path);
                image.Write (path);
            }

            // いくつかの例外クラスがあるが、ファイルが既存なのは「操作」の問題
            else throw new nBadOperationException ();
        }
    }
}

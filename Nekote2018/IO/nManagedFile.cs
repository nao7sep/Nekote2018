using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ImageMagick;
using System.IO;

namespace Nekote
{
    // Sun, 05 May 2019 19:34:46 GMT
    // GUID で決め打ちにしていた実装を Base36 に大胆に変更した
    // nManagedFileUtility.PathMode で古い動作も可能にしてあるので、古いプログラムの派生開発には問題がない
    // 以下、GUID だった頃のコメントが散見されるが、GUID を扱えなくなったわけでないため、そのまま残す
    // ただ、必要性が曖昧で、実際にこのクラス内でも使っていなかった FirstGuid などはなくなり、
    // それを文字列化しての FirstGuidString などだけが FirstRandomString などとして残されている

    // Sat, 28 Sep 2019 02:25:45 GMT
    // またまた変更し、今度はデフォルトを SafeCode に切り替えた
    // そちらの方が全面的に優れる理由などを nSafeCode に書いておく

    public class nManagedFile
    {
        public string Path { get; private set; }

        // Wed, 07 Nov 2018 21:27:03 GMT
        // ファイル名だけ取得するなど、いろいろと役に立つ

        private FileInfo mFileInfo = null;

        public FileInfo FileInfo
        {
            get
            {
                if (mFileInfo == null)
                    mFileInfo = new FileInfo (Path);

                return mFileInfo;
            }
        }

        private void iSetPathRelatedThings ()
        {
            // Wed, 07 Nov 2018 21:59:21 GMT
            // 既に Path には、ファイルか画像かによっていずれかのようなパスが設定されていて、
            // なおかつ、IsImage も設定されているか、そうでないならファイルを読み込めるものとする
            // C:\Parent\a05b95a1-4b6f-4326-acc9-1cdbde5d15af\File.txt
            // C:\Parent\51172235-52d4-4648-8124-21a2a5f2a975\e2e74869-de56-46d6-a1a3-e77c4aba592b\Image.jpg

            // Wed, 07 Nov 2018 22:12:43 GMT
            // 以下、軽いチェックを行っているが、Path はたいていデータベースから読まれるものなので過度なチェックにはしていない
            // 区切り文字を見つけ、IsImage を見ながら、条件が揃えば、落ちやすい GUID の解析から行い、落ちなければ抜ける
            // 画像でないとき、mSecondGuid は null のままとなるため、後続のコードで参照したら落ちるが、それは実装のミスなのでそれでいい
            // 条件が揃わず throw まで行ったなら、操作やデータの問題というよりフォーマットなので、この例外を投げている

            if (nManagedFileUtility.PathMode == nManagedFilePathMode.Base36) // OK
            {
                // Sun, 05 May 2019 20:35:18 GMT
                // デフォルトにした Base36 のモードのとき、ディレクトリー名の長さは不定である
                // int を Base36 に変換したものが使われるため、長さが1～6文字の間でランダムに変わる
                // それ自体は、ディレクトリーを名前で並び替えるわけでもないため問題でない
                // しかし、区切り文字が \ 二つになっているなど、ちょっとした工夫で攻撃が成立しうる
                // そのため、nValidator.IsBase36 に通し、問題があれば例外を投げる
                // プログラムがそこで止まるリスクがあろうと、セキュリティーの問題よりマシ

                int xIndex1 = Path.nLastIndexOfAny (nPath.SeparatorChars);

                // Sun, 05 May 2019 20:44:12 GMT
                // xIndex* を1と比較するのは、「一つ前もある」程度の考えによること
                // それは他のところも同じで、別に一つ前がなくてもいいところや、前に一つでは足りないところでも、
                // とりあえず、区切り文字が見付かったという判別のためにそうしている
                // どういう場合において、その前に何文字あるべきで……のようなことを突き詰めても利益がない

                if (xIndex1 >= 1)
                {
                    int xIndex2 = Path.nLastIndexOfAny (xIndex1 - 1, nPath.SeparatorChars);

                    if (xIndex2 >= 1)
                    {
                        // Sun, 05 May 2019 20:46:16 GMT
                        // 1文字が間に挟まるなら、インデックスは0と2のようになる
                        if (xIndex1 - xIndex2 >= 2)
                        {
                            if (IsImage == false)
                            {
                                FirstRandomString = Path.Substring (xIndex2 + 1, xIndex1 - xIndex2 - 1);

                                if (nValidator.IsBase36 (FirstRandomString) == false)
                                    throw new nInvalidFormatException ();

                                mParentDirectoryPath = Path.Substring (0, xIndex2);
                                mRelativePath = Path.Substring (xIndex2 + 1);
                                return;
                            }

                            int xIndex3 = Path.nLastIndexOfAny (xIndex2 - 1, nPath.SeparatorChars);

                            if (xIndex3 >= 1)
                            {
                                if (IsImage)
                                {
                                    FirstRandomString = Path.Substring (xIndex3 + 1, xIndex2 - xIndex3 - 1);

                                    if (nValidator.IsBase36 (FirstRandomString) == false)
                                        throw new nInvalidFormatException ();

                                    SecondRandomString = Path.Substring (xIndex2 + 1, xIndex1 - xIndex2 - 1);

                                    if (nValidator.IsBase36 (SecondRandomString) == false)
                                        throw new nInvalidFormatException ();

                                    mParentDirectoryPath = Path.Substring (0, xIndex3);
                                    mRelativePath = Path.Substring (xIndex3 + 1);
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            else if (nManagedFileUtility.PathMode == nManagedFilePathMode.Guid) // OK
            {
                // Sun, 05 May 2019 19:46:09 GMT
                // 以下のコードは、GUID がデフォルトの動作だったときのままである
                // mFirstGuid などを廃止したが、いったん GUID にしてみるのは、堅牢性につながる
                // そのため、自動変数にいったん入れてから FirstRandomString に戻している

                int xIndex = Path.nLastIndexOfAny (nPath.SeparatorChars) - nGuid.LengthWithHyphens - 1;

                // Sat, 28 Sep 2019 02:54:24 GMT
                // 他のところでは積極的に例外を投げているメソッドであり、
                // ここで xIndex の範囲を見ないのは問題でない
                // 各所が落ちやすくなっているのは、テストを意図してのことである

                if (nPath.SeparatorChars.Contains (Path [xIndex]))
                {
                    if (IsImage == false)
                    {
                        Guid xFirstGuid = Path.Substring (xIndex + 1, nGuid.LengthWithHyphens).nToGuid ();
                        FirstRandomString = xFirstGuid.nToString ();
                        mParentDirectoryPath = Path.Substring (0, xIndex);
                        mRelativePath = Path.Substring (xIndex + 1);
                        return;
                    }

                    int xIndexAlt = xIndex - nGuid.LengthWithHyphens - 1;

                    if (nPath.SeparatorChars.Contains (Path [xIndexAlt]))
                    {
                        if (IsImage)
                        {
                            Guid xFirstGuid = Path.Substring (xIndexAlt + 1, nGuid.LengthWithHyphens).nToGuid ();
                            FirstRandomString = xFirstGuid.nToString ();
                            Guid xSecondGuid = Path.Substring (xIndex + 1, nGuid.LengthWithHyphens).nToGuid ();
                            SecondRandomString = xSecondGuid.nToString ();
                            mParentDirectoryPath = Path.Substring (0, xIndexAlt);
                            mRelativePath = Path.Substring (xIndexAlt + 1);
                            return;
                        }
                    }
                }
            }

            else if (nManagedFileUtility.PathMode == nManagedFilePathMode.SafeCode) // OK
            {
                // Sat, 28 Sep 2019 02:51:20 GMT
                // Base36 のサブセットの仕様である SafeCode なので、コードも全く同じ
                // 検証のところで使う Is* のみ異なるが、メソッド化して切り分けて……というのも面倒
                // 各インデックスの範囲を見ながら部分文字列を検証にかけているため、うまく書けていると思う

                int xIndex1 = Path.nLastIndexOfAny (nPath.SeparatorChars);

                if (xIndex1 >= 1)
                {
                    int xIndex2 = Path.nLastIndexOfAny (xIndex1 - 1, nPath.SeparatorChars);

                    if (xIndex2 >= 1)
                    {
                        if (xIndex1 - xIndex2 >= 2)
                        {
                            if (IsImage == false)
                            {
                                FirstRandomString = Path.Substring (xIndex2 + 1, xIndex1 - xIndex2 - 1);

                                if (nValidator.IsSafeCode (FirstRandomString) == false)
                                    throw new nInvalidFormatException ();

                                mParentDirectoryPath = Path.Substring (0, xIndex2);
                                mRelativePath = Path.Substring (xIndex2 + 1);
                                return;
                            }

                            int xIndex3 = Path.nLastIndexOfAny (xIndex2 - 1, nPath.SeparatorChars);

                            if (xIndex3 >= 1)
                            {
                                if (IsImage)
                                {
                                    FirstRandomString = Path.Substring (xIndex3 + 1, xIndex2 - xIndex3 - 1);

                                    if (nValidator.IsSafeCode (FirstRandomString) == false)
                                        throw new nInvalidFormatException ();

                                    SecondRandomString = Path.Substring (xIndex2 + 1, xIndex1 - xIndex2 - 1);

                                    if (nValidator.IsSafeCode (SecondRandomString) == false)
                                        throw new nInvalidFormatException ();

                                    mParentDirectoryPath = Path.Substring (0, xIndex3);
                                    mRelativePath = Path.Substring (xIndex3 + 1);
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            // Sun, 05 May 2019 19:41:21 GMT
            // PathMode が正しくてフォーマットに問題があるのでなく、
            // ここに到達するのは、PathMode の指定がおかしいときである
            else throw new nBadOperationException ();

            throw new nInvalidFormatException ();
        }

        // Wed, 07 Nov 2018 21:34:07 GMT
        // 一つ目の GUID のさらに上の、Manage* のときに指定したディレクトリーのパス

        private string mParentDirectoryPath = null;

        public string ParentDirectoryPath
        {
            get
            {
                if (mParentDirectoryPath == null)
                    iSetPathRelatedThings ();

                return mParentDirectoryPath;
            }
        }

        // Sat, 28 Sep 2019 03:30:16 GMT
        // このメソッド名だけ単体で見たら一瞬分かりにくいのでコメント
        // これは、一つ目の GUID のさらに上の、Manage の destDirectoryPath へのマップ
        // path として relative path を指定したら、管理ファイルのフルパスになる
        // parent directory path + relative path = path というイメージ

        public string MapPathToParentDirectory (string path) =>
            nPath.Combine (ParentDirectoryPath, path);

        public string FirstRandomString { get; private set; }

        public string SecondRandomString { get; private set; }

        private string mRelativePath = null;

        public string RelativePath
        {
            get
            {
                if (mRelativePath == null)
                    iSetPathRelatedThings ();

                return mRelativePath;
            }
        }

        private string mRelativeUrl = null;

        public string RelativeUrl
        {
            get
            {
                if (mRelativeUrl == null)
                    mRelativeUrl = RelativePath.nReplace (nPath.DefaultSeparatorChar, nUrl.DefaultSeparatorChar);

                return mRelativeUrl;
            }
        }

        // Wed, 07 Nov 2018 23:03:44 GMT
        // これは nManagedFile のインスタンスを作る時点で定数扱いで入れてしまった方が良いと思う
        // 単一のシステムにおいても、たとえばプロフィール画像は 400px まで、記事への添付写真は 1600px まで、といった区別は十分にあり得る
        // では、動的に、今日は何となくノリでプロフィール画像を 800px まで、といった切り替えをするかと言えば、その可能性はない
        // また、端末の高解像度化への対応などで、ある日を境にデータを変換するなら、縮小版を作り直すのは容易
        // といったことを考えるなら、nManagedFileUtility.MaxResizedImageCount への依存より、よほどマシなコーディング

        // Sat, 28 Sep 2019 03:32:55 GMT
        // インスタンス生成時に設定が必須とされるプロパティー
        // 一度設定したら変更不可で、その管理ファイルの操作性を確定する

        public int MaxResizedImageCount { get; private set; }

        // Wed, 07 Nov 2018 22:44:10 GMT
        // 画像関連のフラグは、ちょっとややこしい
        // たとえば mImageInfo は、画像でないとき null のままなので、
        // 「null なら読み込みを試みる」という実装では、画像でないときに何度も読み込みを試みることになる
        // そこで、そういったプロパティーにおいては、まず自身の m* が null かどうか見て、
        // null であり、なおかつ mIsImage が null であるとき、つまり、画像かどうか白黒ついていないとき、
        // または、そちらは null でなく true で、画像だと分かっているのに m* が null のとき、画像の読み込みを試みる
        // コンストラクターで受け取るのは isImage と imageInfo だけなので、これ以上複雑にはならないはず

        // Thu, 08 Nov 2018 00:36:42 GMT
        // IsImage, ImageInfo, Images の全ての初期化に iSetImageRelatedThings を使う実装にしたところ、
        // RelativePath と IsImage が互いを参照し、久々にスタックオーバーフローになった
        // IsImage と ImageInfo は内部的にシンプルなので、Images の面倒までは見ないように変更する

        private void iSetImageRelatedThings ()
        {
            nImageInfo xInfo = nImage.GetImageInfo (Path);

            if (xInfo != null)
            {
                mIsImage = true;
                mImageInfo = xInfo;
            }

            else
            {
                mIsImage = false;

                // Thu, 08 Nov 2018 00:53:13 GMT
                // IsImage が false なら ImageInfo を見ないし、
                // mImageInfo が null 以外になっているなら呼び出し側のミスなので、
                // ここで mImageInfo を null に戻すことはわざわざしない
            }
        }

        private bool? mIsImage = null;

        public bool IsImage
        {
            get
            {
                if (mIsImage == null)
                    iSetImageRelatedThings ();

                return mIsImage.Value;
            }
        }

        private nImageInfo mImageInfo = null;

        public nImageInfo ImageInfo
        {
            get
            {
                if (mImageInfo == null)
                {
                    if (mIsImage == null || mIsImage.Value)
                        iSetImageRelatedThings ();
                }

                return mImageInfo;
            }
        }

        // Thu, 08 Nov 2018 01:10:04 GMT
        // プロパティーの中に書きたくない大きさの処理なのでメソッドとして分ける
        // 入力に問題がなければ確実に動き、入力に問題があれば確実に落ちるメソッドなので、例外を見ていない
        // テスト時には、二つの GUID をパスに入れておかないといけないことを忘れて、範囲外の例外が飛ぶことが多い

        // Mon, 24 Dec 2018 07:39:30 GMT
        // 何がそんなに悪いのか分からないが、nManaged* だけは、しばらく離れていて戻ってきたら不明点が多くて使いにくい
        // そのことを嫌って全体的に書き直したが、それでもまだ分かりにくいと感じるため、さらにコメントを追記する
        // Images は、IsImage が false のときには空でなく null になる配列で、使用時には、コードの明確化のため、まず IsImage を見るのが良い
        // それぞれの要素には、相対パス、絶対パス、相対 URL、回転後の縦横の長さなどが入っていて、さらに GetFitSize を呼ぶことで表示サイズなどを取得できる
        // 先頭には必ず原版の情報が入っていて、縦横も回転後のものだが、表示を担当するプログラムやライブラリーが Exif を見て回す保証がないため、GIF アニメーションでないなら後述の opt を使うべき
        // 二つ目以降には 200px からの縮小版が入っていて、重要なこととして、Images.Length から [0] の分で1を引いても、それが利用可能な縮小版の数と一致するとは限らない
        // たとえば、3200px まで縮小版を作れる状況で原版が 800px では、「縮小」版は 200px, 400px が作られるが、
        // それで 600px の表示を行いたいときに 400px を使うのではボケるし、800px の原版を使うのでは GPS 情報の漏洩などが考えられる
        // この問題の解決のため、「縮小版が規定数に満たず、縮小版のうち最大のものより原版が大きい」という状況では、原版の大きさのまま回転や Exif 除去が行われた画像 (opt) も用意される
        // 条件の前半の理由は、規定数に満ちていたら、つまり、3200px まで作られていたら、通常のシステムでは 3200px を「最も大きい画像」として表示することに不足がないため
        // たとえば、6000×4000px の撮って出し画像に対して回転や Exif 除去のみ行ったものへ 400px からリンクするようなシステムが必要なら、
        // 利用者の端末や回線への負荷があっても大きな画像を表示する点においてニーズが特殊であり、よっておそらく必要性の乏しい縮小版まで大量に作るのでなく、必要な画像だけ自前で作るべき
        // nManaged* は、あくまで「よくある画像の扱い」を想定したもので、通常のシステムでは、1) セキュリティーが必要なら 1600px または 3200px まで、2) 身内で使うものなら原版に直リンク、でよい
        // 条件の後半は論理的なもので、そもそも縮小版は原版「未満」になるように作られるので常に true だが、「原版がもっと大きいなら」というロジックが分かりやすいので入れている

        // Sat, 28 Sep 2019 15:32:50 GMT
        // ★ 上記のコメントは既に一部が不正確で、今回の更新により、原版と同じサイズの「縮小版」もカウントするようにした
        // 具体的には、原版が800ピクセルなら、縮小版も800ピクセルまで三つ作られ、上記の「原版の大きさのまま」のものは作られない
        // 結果は同じであり、今回の更新によって動作が変わるわけでないが、コードの読みやすさが大きく向上したと思う

        // Mon, 24 Dec 2018 09:34:25 GMT
        // 以下、原版のサイズをいろいろやってみて、Images などがどうなるかを実際に試した結果をまとめておく
        // 縮小版は原版「未満」になるように作られるため、200px までは作られず、opt のみが作られる
        // 撮って出しの画像をイメージしての 6000×4000px でのみ opt が作られていない
        // 試したときに GetResizedImageMaxWidthAndHeight に問題があること、
        // それにより 100px 以下では opt が作られないことが判明したが、修正が済んでいる
        //
        // [0]: 1 1 (Original)
        // [1]: 1 1 (Opt)
        // Resized Images: 0
        // Opt: Yes
        //
        // [0]: 100 100 (Original)
        // [1]: 100 100 (Opt)
        // Resized Images: 0
        // Opt: Yes
        //
        // [0]: 200 200 (Original)
        // [1]: 200 200 (Opt)
        // Resized Images: 0
        // Opt: Yes
        //
        // [0]: 799 799 (Original)
        // [1]: 200 200
        // [2]: 400 400
        // [3]: 799 799 (Opt)
        // Resized Images: 2
        // Opt: Yes
        //
        // [0]: 800 800 (Original)
        // [1]: 200 200
        // [2]: 400 400
        // [3]: 800 800 (Opt)
        // Resized Images: 2
        // Opt: Yes
        //
        // [0]: 801 801 (Original)
        // [1]: 200 200
        // [2]: 400 400
        // [3]: 800 800
        // [4]: 801 801 (Opt)
        // Resized Images: 3
        // Opt: Yes
        //
        // [0]: 6000 4000 (Original)
        // [1]: 200 133
        // [2]: 400 267
        // [3]: 800 533
        // [4]: 1600 1067
        // [5]: 3200 2133
        // Resized Images: 5
        // Opt: No

        private void iSetImages ()
        {
            List <nManagedImage> xImages = new List <nManagedImage> ();
            xImages.Add (new nManagedImage (this, RelativePath, ImageInfo.DisplayWidth, ImageInfo.DisplayHeight));
            // Thu, 08 Nov 2018 01:12:11 GMT
            // 以前の実装では何も考えずに ".jpg" と書いていたが、それはやはりやめた方がいいと思う
            string xFileName = nPath.GetNameWithoutExtension (Path) + nImage.nImageFormatToFileExtension (nImageFormat.Jpeg);

            int xResizedImageCount = ImageInfo.GetResizedImageCount (MaxResizedImageCount),
                xMaxResizedImageWidthAndHeight = nManagedFileUtility.GetResizedImageMaxWidthAndHeight (xResizedImageCount);

            // Thu, 08 Nov 2018 01:12:52 GMT
            // MaxResizedImageCount が十分に大きいとき、
            // たとえば、原版の縦横が 400px なら、GetResizedImageCount は1を返し、縮小版は 200px だけ作られる
            // これを 401x400px にすると2が返るため、「resize を行えること」というのが確定的な条件になっていることが分かる
            // では、たとえば 800px の縮小版を表示するブログにおいて、原版が 800px ぴったりだからということで縮小版が 400px までとなり、
            // 800px のものをそのまま見せてしまうのは GPS 情報の漏洩などのリスクがあって不可能なら、ユーザーは、特に理由なく 400px の閲覧を強いられる
            // これはやはりおかしいため、要求された数の縮小版を作れず（つまり、原版が十分に大きくない）、
            // なおかつ、最大の縮小版より原版が大きい場合には、原版をリサイズすることなく Exif だけ落とした画像を用意する

            // Sat, 28 Sep 2019 15:37:39 GMT
            // ★ GetResizedImageCount の仕様変更により計算方法が変わったが、結果は同じである
            // ここでは、原版が800ピクセルのときと801ピクセルのときの二つを考えてみる
            // 800ピクセルなら縮小版の数が3となり、xMax* が800となり、原版の縦横いずれもそれより大きくないので xMakes* は false になる
            // 801ピクセルでも縮小版の数は3で、原版の縦横が xMax* の800より大きいため xMakes* が true になり、801ピクセルのものが作られる
            // 以前は、800ピクセルのとき縮小版の数が2で、xMax* が400となり、原版の縦横がそれより大きいため xMakes* が true になっていた
            // つまり、今回の更新での変化は、「原版のサイズが、作られうる縮小版のサイズとたまたま一致したとき、
            // それが xMakes* と GetResizedImageCount のいずれによって作られるか」である
            // 結果は同じだが、直感的に分かりやすいのは現行のコードである

            bool xMakesOneMore = xResizedImageCount < MaxResizedImageCount &&
                (ImageInfo.DisplayWidth > xMaxResizedImageWidthAndHeight ||
                    ImageInfo.DisplayHeight > xMaxResizedImageWidthAndHeight);

            for (int temp = 1; temp <= xResizedImageCount; temp ++)
            {
                int xMaxWidthAndHeight = nManagedFileUtility.GetResizedImageMaxWidthAndHeight (temp);
                string xRelativePath;

                // Thu, 08 Nov 2018 01:21:18 GMT
                // コメントが冗長になってきている気がするが、しばらく経ったら自分が忘れて困るため冗長にしておくと、
                // たとえば原版が 800px ぴったりで、縮小版が 400px までしか作られないなら、
                // 200, 400 を名前とするディレクトリーが作られた上、その上位に 800px のものが置かれるべきである
                // 縮小版の数が十分なら xMakesOneMore が false になるので、上位のファイルへの上書きも起こり得ない
                // なお、以前の実装では thumbs ディレクトリーをかまし、その中に Hoge-200.jpg などをゴソッと入れていたが、
                // 今回の実装では nManagedFile ごとに GUID のディレクトリーを作り、その中にさらにサイズごとのディレクトリーを作る
                // 負荷が気になるが、(year)\(month)\(GUID) のように parent の方を切り分けることも可能で、何とかなるだろう

                // Fri, 16 Nov 2018 00:34:07 GMT
                // ディレクトリー名についての仕様を変更した
                // 200, 400, ... と作り、縮小版のうち最大のものを FirstGuid のディレクトリーのルートに置いていたが、
                // それでは、正規表現による ignore が今のところ（？）できない Subversion において、原版だけバージョン管理するのが難しい
                // 別プログラムでディレクトリー構造をスキャンして svn.exe を呼ぶなど、方法はないわけでもないだろうが、そのようなことに時間を割きたくない
                // そのため、いくらか汎用性の落ちる r200, r400, ... に縮小版を保存し、最大のものにも opt ディレクトリーをかますようにした
                // r は resized の略で、thumbnail の t も考えたが、800や1600はサムネイルと呼ぶほど小さくない
                // r は、コードの識別子とも整合するし、最短で、それ以上適合性の高い文字も見付からないため、妥当な仕様だと思う
                // opt は、2週間くらい前にそういうディレクトリーをかますべきかと考えたときに最初に思い付いたもので、
                // optimized と optimal の意味合いを持ち、そのうち後者を「ニーズに最も適している」のように私は解釈している
                // パッと思い付いたものだが、それから2週間が経っても明確にそれに勝るものが思い付かないため、そのまま定着させてしまう

                // Sat, 28 Sep 2019 15:43:31 GMT
                // ★ xMakesOneMore の値を決めるところに詳しく書いたが、今回の更新により処理の流れが変わった
                // それにより、この条件分岐の解釈も微妙に変わったため、結果は同じなのだが、コメントしておく
                // 結局のところ、管理ファイルは、原版がまずあり、縮小版がいくつあろうと、その中で最大のものは opt ディレクトリーに入れたい
                // GPS 情報などが含まれうる原版を表示することはできず、原版を最適化だけしたものは、
                // r801 のようなイレギュラーな名前のディレクトリーに入れるのも微妙だからである
                // そして、原版以外に作る画像の数を考えるにおいて、
                // 1) 縮小版が3200ピクセルまで作られるなら、実用上、それで困ることはない
                // 2) 縮小版が800ピクセルとかで止まっていて、原版がもう少し大きいなら、原版サイズのものを別に用意しておきたい
                // という二つのパターンだけが存在し、そのうち2を作るかどうかを判別しての結果が xMakesOneMore である
                // そのため、まずは temp を1から回し、最大の縮小版の一つ前までは r* に入れ、
                // その次の回において最大の縮小版について扱うときに xMakesOneMore が true なら、最大の縮小版も r* に入れる
                // else になるのは「最大の縮小版であり、xMakesOneMore が false」という AND 条件であり、具体的には3200ピクセルのものになる
                // それでループを抜けたあと、xMakesOneMore を改めて見て、true なら、サイズを考えず opt のディレクトリーパスを作る

                if (temp < xResizedImageCount || xMakesOneMore)
                    xRelativePath = $@"{FirstRandomString}\r{xMaxWidthAndHeight.nToString ()}\{xFileName}";
                else xRelativePath = $@"{FirstRandomString}\opt\{xFileName}";

                ImageInfo.GetFitSize (xMaxWidthAndHeight, xMaxWidthAndHeight, false, out int xWidth, out int xHeight, out bool xIsEnlarged);
                xImages.Add (new nManagedImage (this, xRelativePath, xWidth, xHeight));
            }

            if (xMakesOneMore)
                xImages.Add (new nManagedImage (this, $@"{FirstRandomString}\opt\{xFileName}",
                    ImageInfo.DisplayWidth, ImageInfo.DisplayHeight));

            mImages = xImages.ToArray ();
        }

        // Wed, 07 Nov 2018 23:14:43 GMT
        // 画像なら、[0] には必ず原版が、末尾には必ず (FirstGuid)\* の画像が入り、Images.Length が必ず2以上になる
        // 必要でないメンバー変数やプロパティーまでどんどん値を設定するコーディングなのが気に入らないが、
        // 必要なものだけカツカツのコーディングで設定しようとして、わけの分からない実装になったので、これが限界と思う
        // 私が勝手に気にしているだけで、画像のロードなどに比べたら計算量は屁みたいなものだろう

        // Fri, 16 Nov 2018 00:40:48 GMT
        // iSetImages のところに書いたが、パスについての仕様を変更した

        private nManagedImage [] mImages = null;

        /// <summary>
        /// Images.Length - 1 が縮小版の数とは限らないため注意。
        /// ソースを参照できるなら、iSetImages のコメントが詳しい。
        /// 縮小版の数の取得には、ImageInfo.GetResizedImageCount (MaxResizedImageCount) が選択肢。
        /// しかし、それでは GIF アニメーションの問題などがあるので、サムネイルやリンク先の決定には GetSuitableImage をかませるのが良い。
        /// 異なった条件でサムネイルとリンク先のインスタンスをそれぞれ取得し、それらが異なっていればリンクを張る。
        /// </summary>
        public nManagedImage [] Images
        {
            get
            {
                if (mImages == null)
                {
                    // Thu, 08 Nov 2018 01:02:17 GMT
                    // IsImage と ImageInfo では m* を直接見ているが、
                    // このプロパティーは、もう少し高水準なので IsImage を見る

                    if (IsImage)
                        iSetImages ();
                }

                return mImages;
            }
        }

        // Thu, 08 Nov 2018 04:00:40 GMT
        // 以前は、紆余曲折あって、最終的にはレベル補正もアンシャープマスクも行わない実装に落ち着いていたが、
        // 携帯で撮った写真を扱うにおいては、明らかに暗い写真が多く、そのくらいは直したいと思っていた
        // そこで、レベル補正とアンシャープマスクのパラメーターを受け取れるようにした上、
        // 変則的だが、nManagedFileUtility 側のデフォルト値では、BlackPointForLevel を-1とした
        // ImageMagick のデフォルト値は、下が2、上が1であり、nImageMagick.nUnsafeAutoLevel もそうしたが、
        // それでは、今度は紙や肌といった、ヒストグラムが右寄りになるものが左に転び、過度な陰影が出て不気味だった

        // Sat, 01 Dec 2018 20:53:14 GMT
        // nManagedFile* をベースとする nikoP がうまくいっていて、写真が300枚を超えた
        // その全てを -1, 1 で処理して、ただの一つも結果に不満がないため、nImageMagick でもそちらをデフォルトに変更した
        // 仕様としてのアンバランス感が気になるが、イメージセンサーの特性などを考えるなら、明るくするだけの仕様にこそ合理性がある

        // Thu, 08 Nov 2018 11:44:26 GMT
        // 蛇足だが、additional というのは、原版「以外」という意味合いでメソッド名に入れている
        // CreateThumbs や ResizeImages といった名前も考えたが、原版をそのままノーマライズすることもあるし、あとで誤解につながる
        // それなら、原版以外の作成または更新を行うという、うるさくても分かりやすいメソッド名がいい

        // Wed, 14 Nov 2018 01:57:20 GMT
        // 初回の実装では、眠かったのか、パラメーターや min / max の値を単一の文字列にまとめて返すようにしたが、
        // そういうのは、必要なら out をつけた引数で、データ型を引き継いで取得した方が、後続のコードにおける利便性が高い
        // 今回の更新により、このメソッドは、何か一つでも処理したのかどうかを bool で返すようになった

        public bool CreateOrUpdateAdditionalImages (double blackPointForLevel, double whitePointForLevel,
            out byte minValueForLevel, out byte maxValueForLevel, bool appliesUnsharpMask, int jpegQuality)
        {
            // Thu, 08 Nov 2018 04:05:02 GMT
            // 原版を除く全ての画像について、存在しないか、原版より古いかを調べ、処理を続行するかを決める
            // そういう実装にした上で毎回 Create* を呼んでおけば、ウェブシステムの運用において原版を差し替えることもできる
            // むろん、システム上の操作だけで何でもできるのが理想的だが、そこまで作り込まないシステムもある

            bool xProceeds = false;

            for (int temp = 1; temp < Images.Length; temp ++)
            {
                nManagedImage xImage = Images [temp];

                if (nFile.Exists (xImage.Path) == false ||
                    nFile.GetLastWriteUtc (xImage.Path) < FileInfo.LastWriteTimeUtc)
                {
                    xProceeds = true;
                    break;
                }
            }

            if (xProceeds == false)
            {
                minValueForLevel = default;
                maxValueForLevel = default;
                return false;
            }

            // Thu, 08 Nov 2018 11:37:35 GMT
            // いったん初期化しないとコンパイラーに怒られる

            // Wed, 14 Nov 2018 01:59:36 GMT
            // 処理が失敗した場合に一部の out 引数のみ既に値が変わっているようにしたくない
            // こういうものは、一時変数を作り、そちらで処理を終えてから、最後に out 引数に入れる

            byte xMinValue = default,
                xMaxValue = default;

            using (MagickImage xOriginalImage = new MagickImage (Path))
            {
                // Thu, 08 Nov 2018 04:08:30 GMT
                // 画像を回転してから Exif 情報を消す
                // 最近では携帯による GPS 情報が原版に入っていることが多く、大きな問題だった
                // 二つ目の GUID で原版を隠すのも、情報漏洩の回避が最重要の目的である

                xOriginalImage.AutoOrient ();
                xOriginalImage.Strip ();

                if (blackPointForLevel >= 0 || whitePointForLevel >= 0)
                    nImageMagick.GetInfoForUnsafeLevel (xOriginalImage,
                        blackPointForLevel, whitePointForLevel,
                        out xMinValue, out xMaxValue);

                for (int temp = 1; temp < Images.Length; temp ++)
                {
                    nManagedImage xImage = Images [temp];

                    if (nFile.Exists (xImage.Path) == false ||
                        nFile.GetLastWriteUtc (xImage.Path) < FileInfo.LastWriteTimeUtc)
                    {
                        using (MagickImage xNewImage = (MagickImage) xOriginalImage.Clone ())
                        {
                            // Thu, 08 Nov 2018 04:10:18 GMT
                            // nResize は、縦横の上限を指定し、内部で nImage.GetFitSize を呼ばせるものである
                            // こちらでは既に最終的な縦横が分かっているため、二度目の計算の丸め込みで誤差が生じるなどを回避したく、
                            // nResize 内のコードのうち、厳密な縦横を指定する部分のみコピーしている

                            xNewImage.Resize (new MagickGeometry
                            {
                                Width = xImage.DisplayWidth,
                                Height = xImage.DisplayHeight,
                                IgnoreAspectRatio = true
                            });

                            if (blackPointForLevel >= 0 || whitePointForLevel >= 0)
                                xNewImage.nLevel (xMinValue, xMaxValue);

                            if (appliesUnsharpMask)
                                xNewImage.nUnsharpMask ();

                            xNewImage.Format = MagickFormat.Jpeg;
                            xNewImage.Quality = jpegQuality;
                            // Thu, 08 Nov 2018 04:12:44 GMT
                            // create だけでなく update も行うメソッドなので上書きさせる
                            xNewImage.nWrite (xImage.Path, true);
                            // Thu, 08 Nov 2018 04:13:25 GMT
                            // 同じメソッドを何度も呼べるように、タイムスタンプを一致させておく
                            nFile.SetLastWriteUtc (xImage.Path, FileInfo.LastWriteTimeUtc);
                        }
                    }
                }
            }

            minValueForLevel = xMinValue;
            maxValueForLevel = xMaxValue;
            return true;
        }

        // Thu, 08 Nov 2018 04:19:36 GMT
        // MaxResizedImageCount を自動的に指定する多重定義は意地でも避けようと思っているが、
        // 画像の最適化や JPEG 品質の方は、プログラムごとに変更する余地が乏しく、決め打ちでも平均的にうまくいくはず
        // もちろん、さわらない方がいい画像も今後は多数出てくるが、パッと見る限り、さわった方がいい画像の方が圧倒的に多い

        // Sat, 28 Sep 2019 03:20:23 GMT
        // 結局、maxResizedImageCount に初期値を与えた
        // ここは関係ないようだが、コメントで言及されているため一応

        public bool CreateOrUpdateAdditionalImages () =>
            CreateOrUpdateAdditionalImages (
                nManagedFileUtility.BlackPointForLevel, nManagedFileUtility.WhitePointForLevel,
                out byte xMinValueForLevel, out byte xMaxValueForLevel,
                nManagedFileUtility.AppliesUnsharpMask, nManagedFileUtility.JpegQuality);

        // Thu, 08 Nov 2018 02:53:51 GMT
        // 画像を表示するエリアの縦横を指定することで、どれをどういった縦横指定で出力するべきか分かるメソッド
        // enlarges は、原版の縦横でも足りないときの苦肉の策であり、基本的には true で問題がない
        // 実装としては、拡大が関連してくるのは最後だけなので、それまでの分について、縦または横の片方でも要求を満たしていればそこで返る
        // 両方が満たすべきと思いがちだが、実際の表示は内接になるため、片方が「以上」になっていて、それをその辺の長さに縮小しての表示がうまくいく
        // それらで要求を満たせなかった場合には、最後のものを、今度は enlarges を反映して試し、唯一の選択肢なのだからそれを返す

        // Thu, 08 Nov 2018 12:16:34 GMT
        // アップロードされたものが GIF アニメーションなら、原版を表示しないことにはコンテンツが崩壊する
        // [0] の画像を出力しない考えだったが、元々は Exif 情報を漏洩させないためであり、GIF ならおそらく問題はない
        // 二つ目の GUID が見えることも問題でなく、そのことによって他のファイルに危険は及ばない

        public nManagedImage GetSuitableImage (int targetWidth, int targetHeight,
            bool prioritizesAnimation, bool enlarges, out int fitWidth, out int fitHeight, out bool isEnlarged)
        {
            if (prioritizesAnimation)
            {
                if (ImageInfo.IsAnimated)
                {
                    nManagedImage xFirstImage = Images [0];

                    xFirstImage.GetFitSize (targetWidth, targetHeight, enlarges,
                        out fitWidth, out fitHeight, out isEnlarged);

                    return xFirstImage;
                }
            }

            for (int temp = 1; temp < Images.Length - 1; temp ++)
            {
                nManagedImage xImage = Images [temp];

                if (xImage.DisplayWidth >= targetWidth ||
                    xImage.DisplayHeight >= targetHeight)
                {
                    xImage.GetFitSize (targetWidth, targetHeight, false,
                        out fitWidth, out fitHeight, out isEnlarged);

                    return xImage;
                }
            }

            nManagedImage xLastImage = Images [Images.Length - 1];

            xLastImage.GetFitSize (targetWidth, targetHeight, enlarges,
                out fitWidth, out fitHeight, out isEnlarged);

            return xLastImage;
        }

        // Thu, 08 Nov 2018 11:55:20 GMT
        // 元のファイル以外を、他の方法で追加されたものも含めてゴッソリ消すメソッド
        // 200などの、縮小版のためのディレクトリーだけでなく、あらゆるサブディレクトリーをスキャンする
        // 途中で Count をチェックするのは、その後の foreach で元のファイルはパスが一致して守られるのを確認するため
        // Count の結果が1にならないのは、nManagedFile* またはそれを使う側の実装に問題があるということなので例外を投げておく
        // Clean において空のファイルを消さないのは、元のファイルが空の可能性も想定しているため

        // Fri, 16 Nov 2018 00:40:48 GMT
        // iSetImages のところに書いたが、パスについての仕様を変更した

        // Wed, 14 Nov 2018 02:02:27 GMT
        // 戻り値がないメソッドだったが、bool で、一つでも消したかどうかを返すようにした
        // 消したファイルの数を返さないのは、「何を」消したか不明で数だけもらっても仕方ないため

        public bool DeleteAdditionalFiles ()
        {
            string xDirectoryPath = nPath.Combine (ParentDirectoryPath, FirstRandomString);
            IEnumerable <FileInfo> xFiles = nDirectory.GetFiles (xDirectoryPath, SearchOption.AllDirectories);
            int xDeletedFileCount = 0;

            if (xFiles.Count (x => nIgnoreCase.Compare (x.FullName, Path) == 0) == 1)
            {
                foreach (FileInfo xFile in xFiles)
                {
                    if (nIgnoreCase.Compare (xFile.FullName, Path) != 0)
                    {
                        nFile.Delete (xFile.FullName);
                        xDeletedFileCount ++;
                    }
                }

                nDirectory.Clean (xDirectoryPath, true, false);
                return xDeletedFileCount > 0;
            }

            // Wed, 24 Apr 2019 00:12:10 GMT
            // invalid data から corrupt data にクラスを変更したが、ここは、そのままでよい
            // GUID のディレクトリー内に原版があるとき「以外」であり、確かにデータが破損していると言えるため
            throw new nCorruptDataException ();
        }

        // Wed, 07 Nov 2018 23:10:02 GMT
        // コンストラクターで縮小版の数の最大値を受け取る理由については、MaxResizedImageCount のところに書いた
        // nManagedFileUtility.MaxResizedImageCount を自動設定する多重定義を用意したい衝動とちょっと戦っているが、
        // iUtility クラスでも作ってその中に入れたプロパティーを毎回渡した方が絶対に保守性の高いコードになる
        // あるいは、この値が単一かつ固定でよいなら、手作業で nManagedFileUtility.MaxResizedImageCount を与えたらいい

        // Sat, 28 Sep 2019 03:22:13 GMT
        // 結局、maxResizedImageCount に初期値を与えた
        // 注意喚起のために同じことを毎回書かされるというのも不毛

        public nManagedFile (string path, int? maxResizedImageCount = null)
        {
            Path = path;

            if (maxResizedImageCount != null)
                MaxResizedImageCount = maxResizedImageCount.Value;
            else MaxResizedImageCount = nManagedFileUtility.MaxResizedImageCount;
        }

        public nManagedFile (string path, bool isImage, int? maxResizedImageCount = null)
        {
            Path = path;
            mIsImage = isImage;

            // Wed, 07 Nov 2018 22:53:54 GMT
            // mImageInfo は null のままなので、isImage が true なら、
            // 「画像だと分かっているのに自分はまだ空だ」と思って JIT 的に初期化する

            if (maxResizedImageCount != null)
                MaxResizedImageCount = maxResizedImageCount.Value;
            else MaxResizedImageCount = nManagedFileUtility.MaxResizedImageCount;
        }

        public nManagedFile (string path, nImageInfo imageInfo, int? maxResizedImageCount = null)
        {
            Path = path;
            // Wed, 07 Nov 2018 22:55:03 GMT
            // imageInfo に基づき、画像かどうかはここで確定していい
            mIsImage = imageInfo != null;
            mImageInfo = imageInfo;

            if (maxResizedImageCount != null)
                MaxResizedImageCount = maxResizedImageCount.Value;
            else MaxResizedImageCount = nManagedFileUtility.MaxResizedImageCount;
        }

        // Sat, 28 Sep 2019 12:15:26 GMT
        // 見なければならないことが多いクラスなのでデバッグがややこしい
        // これまでは、その都度必要なところだけ見ていたが、丸ごと吐いた方が良さそう
        // 名前に debug を含めるが、リリース版が呼ぶ可能性もあるため DEBUG には入れない

        public string ToDebugString ()
        {
            StringBuilder xBuilder = new StringBuilder ();
            xBuilder.AppendLine ("Path: " + Path);
            xBuilder.AppendLine ("ParentDirectoryPath: " + ParentDirectoryPath);
            xBuilder.AppendLine ("FirstRandomString: " + FirstRandomString);
            xBuilder.AppendLine ("SecondRandomString: " + SecondRandomString.nToFriendlyString ());
            xBuilder.AppendLine ("RelativePath: " + RelativePath);
            xBuilder.AppendLine ("RelativeUrl: " + RelativeUrl);
            xBuilder.AppendLine ("IsImage: " + IsImage.nToString ());

            if (IsImage)
            {
                xBuilder.AppendLine ("ImageInfo");
                xBuilder.AppendLine ("\tFormat: " + ImageInfo.Format.nToString ());
                xBuilder.AppendLine ("\tIsAnimated: " + ImageInfo.IsAnimated.nToString ());
                xBuilder.AppendLine ("\tOrientation: " + ImageInfo.Orientation.nToString ());
                xBuilder.AppendLine ("\tWidth: " + ImageInfo.Width.nToString ());
                xBuilder.AppendLine ("\tHeight: " + ImageInfo.Height.nToString ());
                xBuilder.AppendLine ("\tDisplayWidth: " + ImageInfo.DisplayWidth.nToString ());
                xBuilder.AppendLine ("\tDisplayHeight: " + ImageInfo.DisplayHeight.nToString ());

                // Sat, 28 Sep 2019 12:20:47 GMT
                // MaxResizedImageCount は IsImage が false でも利用可能だが、
                // ファイルが画像でないなら処理に全く関係してこないため、ここで出力する
                xBuilder.AppendLine ("MaxResizedImageCount: " + MaxResizedImageCount.nToString ());

                xBuilder.AppendLine ("Images");
                xBuilder.AppendLine ("\tLength: " + Images.Length.nToString ());

                for (int temp = 0; temp < Images.Length; temp ++)
                {
                    // Sat, 28 Sep 2019 12:22:23 GMT
                    // key: value になっているところには普通に : を挟むが、値を持たないところには不要
                    // Visual Studio のデバッグ画面が配列の要素をこのように表示するため、それを参考にしている
                    xBuilder.AppendLine ($"\t[{temp.nToString ()}]");
                    xBuilder.AppendLine ("\t\tPath: " + Images [temp].Path);
                    xBuilder.AppendLine ("\t\tRelativePath: " + Images [temp].RelativePath);
                    xBuilder.AppendLine ("\t\tRelativeUrl: " + Images [temp].RelativeUrl);
                    xBuilder.AppendLine ("\t\tDisplayWidth: " + Images [temp].DisplayWidth.nToString ());
                    xBuilder.AppendLine ("\t\tDisplayHeight: " + Images [temp].DisplayHeight.nToString ());
                }
            }

            return xBuilder.ToString ();
        }

        // Mon, 12 Nov 2018 18:58:38 GMT
        // 実装したつもりの Delete がなかったのでサクッと実装
        // DeleteAdditionalFiles とセットで、消したファイルのパスを配列で受け取れるようにすることを考えたが、
        // .NET の Directory.Delete も行わないことであり、ニーズをよく考えない偏った仕様になるため却下

        // Wed, 14 Nov 2018 02:06:01 GMT
        // DeleteAdditionalFiles に戻り値をつけたが、こちらはそのままにしておく
        // .NET との整合性もそうだし、消したかどうかは、ディレクトリーがあったかどうかで分かるため
        // さらに言うと、「ない状態にする」というのが目的のメソッドであり、
        // その実現のために削除という処理が要されたかどうかはあまり重要でないため

        public void Delete ()
        {
            string xDirectoryPath = nPath.Combine (ParentDirectoryPath, FirstRandomString);
            nDirectory.Delete (xDirectoryPath, true);
        }
    }
}

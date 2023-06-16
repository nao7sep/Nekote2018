using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // テストコードを書くときに Random のインスタンスを生成するのが面倒だし、
    // ダイアログやページのデザインにおいてランダムな文や単語が欲しいときもあるため、
    // Random のラッパークラスを静的に用意しておく
    // ループなどで乱数が多数必要なら、Random のインスタンスを作るか、自前で生成した方が良いかもしれない
    // 一方、こういう静的クラスがあれば、今後いろいろと機能を追加できるし、Nekote で内部的に使えて便利である
    // https://msdn.microsoft.com/en-us/library/system.random.aspx
    // http://referencesource.microsoft.com/#mscorlib/system/random.cs

    public static class nRandom
    {
        private static Random mRandom = null;

        public static Random Random
        {
            get
            {
                if (mRandom == null)
                    // seed として何も指定しなければ、Environment.TickCount が使われるようだ
                    // 複数のインスタンスを同時に作るなら、lock を使いながら seed に連番を加えるのが良いか
                    mRandom = new Random ();

                return mRandom;
            }
        }

        // int.MaxValue が含まれないことを忘れることがあるため summary を設定

        // Mon, 24 Dec 2018 07:16:22 GMT
        // たぶんこれが最初の summary なのでここに書いておくが、
        // summary には IDE 側の都合で改行のみを入れられず、<para> では空行が入る
        // 量を書くことも今後あるため、ここだけは句点も使い、複数行で書いていく
        // するとインテリセンスでは句点の後ろに半角空白が入るが、そこは妥協してよい

        /// <summary>
        /// 0以上、int.MaxValue 未満であり、int.MaxValue は含まれない。
        /// </summary>
        public static int Next ()
        {
            // 内部的には InternalSample が呼ばれ、本当に乱数を生成するのはこのメソッドだけのようだ
            // つまり、それ以外は、double を生成するものですら、このメソッドの戻り値の割り算である
            return Random.Next ();
        }

        // 未満であることが分かりやすい引数名にしている
        // MSDN のページで使われている表現
        public static int Next (int exclusiveMaxValue)
        {
            // Next 内部の InternalSample を割って算出した Sample に引数をかける実装
            // 確認したわけでないが、精度の低下があるため、範囲内の全ての値が得られるわけでなさそう
            return Random.Next (exclusiveMaxValue);
        }

        public static int Next (int inclusiveMinValue, int exclusiveMaxValue)
        {
            // inclusiveMinValue が負数でも問題はなさそう
            // 引数が一つの Next と同様、全ての値が得られるわけではなさそう
            return Random.Next (inclusiveMinValue, exclusiveMaxValue);
        }

        // こちらでは1.0が含まれないことを忘れるため summary を設定しておく
        /// <summary>
        /// 0.0以上、1.0未満であり、1.0は含まれない。
        /// </summary>
        public static double NextDouble ()
        {
            // Next 内部の InternalSample を割り算する実装なので、64ビットの double にふさわしい精度が得られるわけでなさそう
            // Java の方はビット演算で生成されるようだが、.NET は割り算であり、精度を求めるなら、それ専用のライブラリーを探すべきだろう
            return Random.NextDouble ();
        }

        // .NET の NextBytes は、範囲を指定したり、新しい配列を作ったりできない
        // そのため、そういったことが可能なものを .NET と同様の実装で用意しておく

        public static void NextBytes (byte [] bytes, int index, int length)
        {
            for (int temp = 0; temp < length; temp ++)
                // .NET のコードでは Byte.MaxValue + 1 だが、定数にしておく
                bytes [index + temp] = (byte) (Next () % 256);
        }

        public static void NextBytes (byte [] bytes)
        {
            // インデックスの足し算やラッパーメソッドの呼び出しがないため、
            // .NET のものを使えるところでは、そうした方が速い
            Random.NextBytes (bytes);
        }

        public static byte [] NextBytes (int length)
        {
            byte [] xBytes = new byte [length];
            NextBytes (xBytes);
            return xBytes;
        }

        #region ランダムな文や単語の生成 // OK
        // 以下のメソッドで、ランダムな文や単語などを生成できる
        // 特定の言語を想定していないが、ヨーロッパ各国の文字を入れるなどすると Unicode 表を組み込むことになるし、
        // そこまでの作り込みは不要なので、ASCII のみとしている
        // 文法的には、「段落 (paragraph) > 文 (sentence) > 節 (clause) > 語 (word)」とのことだが、
        // 「語」は「言語」のイメージが強いため、コメントでは「単語」と表現している
        // https://en.wikipedia.org/wiki/Paragraph
        // https://en.wikipedia.org/wiki/Sentence_%28linguistics%29
        // https://en.wikipedia.org/wiki/Clause
        // https://en.wikipedia.org/wiki/Word

        // 単語の長さは、a が1文字なので最短を1とし、最長は、8, 10, 16 あたりを考えた末、
        // 人間が話す言語をイメージするならプログラミング寄りの長さにすることもないため10とした

        public static void NextWord (
            StringBuilder builder,
            int minLength = 1, int maxLength = 10)
        {
            int xLength = Next (minLength, maxLength + 1);

            for (int temp = 0; temp < xLength; temp ++)
                builder.Append ((char) ('a' + Next (26)));
        }

        // 言語系のメソッドのうち、NextWord だけは、
        // StringBuilder を受け取らないものが内部で StringBuilder のインスタンスを作らない
        // 長さ固定の char [] を作り、それを new string した方が速そうであるため

        public static string NextWord (
            int minLength = 1, int maxLength = 10)
        {
            int xLength = Next (minLength, maxLength + 1);
            char [] xChars = new char [xLength];

            for (int temp = 0; temp < xLength; temp ++)
                xChars [temp] = (char) ('a' + Next (26));

            return new string (xChars);
        }

        // 節については、最初の文字を大文字にするとか、最後にピリオドをつけるとかは不要
        // そういうのは、節が不可避的に , で連結される文以上において必要となってくる表記上のことで、
        // 意味を持つ節（あるいは phrase としての句）の成立要件に含まれるものでない
        // https://en.wikipedia.org/wiki/Phrase

        public static void NextClause (
            StringBuilder builder,
            int minWordCount = 1, int maxWordCount = 10,
            int minWordLength = 1, int maxWordLength = 10)
        {
            int xCount = Next (minWordCount, maxWordCount + 1);

            for (int temp = 0; temp < xCount; temp ++)
            {
                if (temp > 0)
                    builder.Append (' ');

                NextWord (builder, minWordLength, maxWordLength);
            }
        }

        public static string NextClause (
            int minWordCount = 1, int maxWordCount = 10,
            int minWordLength = 1, int maxWordLength = 10)
        {
            StringBuilder xBuilder = new StringBuilder ();
            NextClause (xBuilder, minWordCount, maxWordCount, minWordLength, maxWordLength);
            return xBuilder.ToString ();
        }

        // 長さが1～10文字の単語が1～10個含まれる節がさらに1～3個含まれるものを文としている
        // 実際には、単語の長さも単語数も節の数も偏りがあり、各値が均一に現れるのではむしろ違和感があるが、
        // 特定の言語を想定しない、あくまで表示サンプル程度のものなので、一切の作り込みを行わない

        public static void NextSentence (
            StringBuilder builder,
            int minClauseCount = 1, int maxClauseCount = 3,
            int minWordCount = 1, int maxWordCount = 10,
            int minWordLength = 1, int maxWordLength = 10)
        {
            // あとで先頭を大文字にするため位置をとっておく
            int xLength = builder.Length,
                xCount = Next (minClauseCount, maxClauseCount + 1);

            for (int temp = 0; temp < xCount; temp ++)
            {
                if (temp > 0)
                    builder.Append (", ");

                NextClause (builder, minWordCount, maxWordCount, minWordLength, maxWordLength);
            }

            builder [xLength] = builder [xLength].nToUpper ();
            // カンマは空白もつけるが、ピリオドはそうでないため注意
            // NextSentence のみ何度も呼ぶのでは、文がつながってしまう
            builder.Append ('.');
        }

        public static string NextSentence (
            int minClauseCount = 1, int maxClauseCount = 3,
            int minWordCount = 1, int maxWordCount = 10,
            int minWordLength = 1, int maxWordLength = 10)
        {
            StringBuilder xBuilder = new StringBuilder ();

            NextSentence (xBuilder, minClauseCount, maxClauseCount,
                minWordCount, maxWordCount, minWordLength, maxWordLength);

            return xBuilder.ToString ();
        }

        // 文が1～3個集まれば段落であるとしている
        // 最近、裁判所の書面を扱うことが多い関係で、長い段落を見慣れていて、1～10個にすることも考えたが、
        // アルファベットで1～300単語・平均150単語の段落というのは、実際に表示してみると長かった

        public static void NextParagraph (
            StringBuilder builder,
            int minSentenceCount = 1, int maxSentenceCount = 3,
            int minClauseCount = 1, int maxClauseCount = 3,
            int minWordCount = 1, int maxWordCount = 10,
            int minWordLength = 1, int maxWordLength = 10)
        {
            int xCount = Next (minSentenceCount, maxSentenceCount + 1);

            for (int temp = 0; temp < xCount; temp ++)
            {
                if (temp > 0)
                    builder.Append (' ');

                NextSentence (builder, minClauseCount, maxClauseCount,
                    minWordCount, maxWordCount, minWordLength, maxWordLength);
            }
        }

        public static string NextParagraph (
            int minSentenceCount = 1, int maxSentenceCount = 3,
            int minClauseCount = 1, int maxClauseCount = 3,
            int minWordCount = 1, int maxWordCount = 10,
            int minWordLength = 1, int maxWordLength = 10)
        {
            StringBuilder xBuilder = new StringBuilder ();

            NextParagraph (xBuilder, minSentenceCount, maxSentenceCount,
                minClauseCount, maxClauseCount, minWordCount, maxWordCount,
                minWordLength, maxWordLength);

            return xBuilder.ToString ();
        }
        #endregion

        #region テスト用の文字列の生成
        // Fri, 27 Sep 2019 22:10:21 GMT
        // テストコードに NextWord を使っていたが、それでは記号の扱いのチェックが甘い
        // そこで、より雑多な文字列を生成できるメソッドを追加した
        //
        // NextString は、追加の文字を char [] で指定できるため、呼び出し側で予めキャッシュするのが良い
        // params にしても additionalChars: で指定できるのは一つだけであり、結局 new char [] { ... } になる
        // そのため params にせず、追加の文字を指定しないなら null を与えるようにした
        //
        // NextAscii は、ASCII の全ての文字を含みうるデタラメな文字列をより高速に生成するためのもので、
        // AsciiChars などをローカルに持つため、毎回 List.AddRange を何度か呼ぶ NextString より間違いなく速い
        //
        // いずれも maxLength が100なのは、10では短く感じるし、
        // ちょうど NextClause が、maxWordCount * maxWordLength により100であるため
        // NextAscii を呼び、デフォルトでは半角空白以外の ASCII 文字1～100個というのを生成すれば、
        // どういったフォーマットを経るラウンドトリップであろうと、あらゆるエスケープをテストできる

        public static void NextString (
            StringBuilder builder,
            int minLength = 1, int maxLength = 100,
            bool canContainSmallLetters = true, bool canContainCapitalLetters = true,
            bool canContainDigits = true, char [] additionalChars = null)
        {
            int xLength = Next (minLength, maxLength + 1);

            // Fri, 27 Sep 2019 22:19:04 GMT
            // 8パターンの条件分岐も考えたが、どうせ additionalChars をつなげる
            // 8パターン程度なら作れるが、作ってしまうと16パターンでも作りたくなりそう
            // 速度が必要なら NextAscii を呼べばよく、こちらを呼ぶことは頻繁でない

            List <char> xCharsAlt = new List <char> ();

            if (canContainSmallLetters)
                xCharsAlt.AddRange (nChar.SmallLetters);

            if (canContainCapitalLetters)
                xCharsAlt.AddRange (nChar.CapitalLetters);

            if (canContainDigits)
                xCharsAlt.AddRange (nChar.Digits);

            if (additionalChars != null)
                xCharsAlt.AddRange (additionalChars);

            // Fri, 27 Sep 2019 22:20:30 GMT
            // いくつかのコレクションの実装が極めて複雑で非効率的なので調べたが、
            // List は、this [index] も Count も内部の配列や int への直接アクセスであり、無駄がない
            // そのため、いったん char [] に変換するようなことは必要でない
            // https://github.com/microsoft/referencesource/blob/master/mscorlib/system/collections/generic/list.cs

            for (int temp = 0; temp < xLength; temp ++)
                builder.Append (xCharsAlt [Next (xCharsAlt.Count)]);
        }

        public static string NextString (
            int minLength = 1, int maxLength = 100,
            bool canContainSmallLetters = true, bool canContainCapitalLetters = true,
            bool canContainDigits = true, char [] additionalChars = null)
        {
            StringBuilder xBuilder = new StringBuilder ();

            NextString (xBuilder, minLength, maxLength, canContainSmallLetters,
                canContainCapitalLetters, canContainDigits, additionalChars);

            return xBuilder.ToString ();
        }

        // Fri, 27 Sep 2019 22:21:58 GMT
        // nChar に入れることも考えたが、用途がやや特殊なので様子見
        // nChar でもどうしても使いたくなれば、そちらに参照だけコピーするのでもよい
        // 片方だけ without をつけるのは、GetFileNameWithoutExtension と同様の考え方による
        // これも、拡張子が含まれる方は GetFileName とシンプルである

        public static readonly char [] AsciiChars =
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~',
            ' '
        };

        public static readonly char [] AsciiCharsWithoutSpace =
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~'
        };

        // Fri, 27 Sep 2019 22:24:35 GMT
        // テストに通すからには、半角空白だけだとか、半角空白が二つ以上続くとかも稀に起こるべき
        // 何となく、目に見えないものを除外したかったが、それではテストの厳しさが低下する

        public static void NextAscii (
            StringBuilder builder,
            int minLength = 1, int maxLength = 100,
            bool canContainSpace = true)
        {
            int xLength = Next (minLength, maxLength + 1);

            if (canContainSpace)
            {
                for (int temp = 0; temp < xLength; temp ++)
                    builder.Append (AsciiChars [Next (AsciiChars.Length)]);
            }

            else
            {
                for (int temp = 0; temp < xLength; temp ++)
                    builder.Append (AsciiCharsWithoutSpace [Next (AsciiCharsWithoutSpace.Length)]);
            }
        }

        public static string NextAscii (
            int minLength = 1, int maxLength = 100,
            bool canContainSpace = true)
        {
            StringBuilder xBuilder = new StringBuilder ();
            NextAscii (xBuilder, minLength, maxLength, canContainSpace);
            return xBuilder.ToString ();
        }
        #endregion
    }
}

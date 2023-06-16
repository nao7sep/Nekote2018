using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Fri, 26 Apr 2019 00:58:28 GMT
    // 複数の文字列がセットになったものを扱うメソッドをこのクラスに集めていく
    // nString に入れることも考えたが、1) そちらは肥大化しつつある、2) 概念的に重きを置くべきは「複数の」文字列であること、と考え、別クラスを用意
    // ExtractExistingPaths は完全に nPath の仕事だし、目的や用途が特殊なメソッドが集まってきそうだが、実用性を優先

    public static class nStrings
    {
        // Fri, 26 Apr 2019 01:00:47 GMT
        // LINQ の Distinct よりパッと呼べて、null や "" も削れるメソッド
        // null や "" を削る処理を Distinct でやろうとしたらラムダでゴタゴタするか

        public static List <string> nDistinct (this IEnumerable <string> values, bool ignoresCase = false, bool keepsNullAndEmpty = true)
        {
            // Fri, 26 Apr 2019 01:02:16 GMT
            // .NET のちゃんとした実装では、遅延処理のために DistinctIterator をかませるだけのようだが、
            // nStrings は、ちゃんとしていなくていいから使いやすいものを目指すクラスであり、List <string> に全て入れて返す
            // https://github.com/Microsoft/referencesource/blob/master/System.Core/System/Linq/Enumerable.cs
            List <string> xValues = new List <string> ();

            foreach (string xValue in values)
            {
                if (keepsNullAndEmpty ||
                    string.IsNullOrEmpty (xValue) == false)
                {
                    if (xValues.Contains (xValue, ignoresCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture) == false)
                        xValues.Add (xValue);
                }
            }

            return xValues;
        }

        // Fri, 26 Apr 2019 01:04:12 GMT
        // 最初、nExtractExistingFileSystemObjectsPaths のような名称も考えたが、
        // 今のところ「パス」と言えばファイルシステムを連想するので、シンプルにした
        // Get も考えたが、抜き出すという意味合いを考えて Extract とした
        public static List <string> nExtractExistingPaths (this IEnumerable <string> values, bool canBeDirectory, bool canBeFile)
        {
            List <string> xValues = new List <string> ();

            foreach (string xValue in values)
            {
                if (string.IsNullOrEmpty (xValue) == false)
                {
                    if (canBeDirectory && nDirectory.Exists (xValue))
                        xValues.Add (xValue);
                    else if (canBeFile && nFile.Exists (xValue))
                        xValues.Add (xValue);
                }
            }

            return xValues;
        }

        // Fri, 26 Apr 2019 01:06:00 GMT
        // nJoinStrings / nSplitIntoStrings を最初は考えたが、必ず nStringsSeparator を取るため string.Split などとの混同は起こりにくい
        // むしろ、複数の文字列を統合し、複数の文字列に展開するのが分かり切っていて Strings を入れる野暮ったさの方が気になる
        public static void nJoin (this IEnumerable <string> values, StringBuilder builder, nStringsSeparator separator, bool keepsNullAndEmpty = true)
        {
            string xSeparator;

            switch (separator)
            {
                case nStringsSeparator.Tab:
                    xSeparator = "\t";
                    break;
                case nStringsSeparator.NewLine:
                    xSeparator = "\r\n";
                    break;
                case nStringsSeparator.VerticalBar:
                    xSeparator = "|";
                    break;
                default:
                    throw new nBadOperationException ();
            }

            int xCount = 0;

            foreach (string xValue in values)
            {
                if (keepsNullAndEmpty ||
                    string.IsNullOrEmpty (xValue) == false)
                {
                    if (xCount > 0)
                        builder.Append (xSeparator);

                    if (separator == nStringsSeparator.VerticalBar)
                        nCString.EscapeC_andVerticalBar (xValue, builder);
                    else xValue.nEscapeC (builder);

                    xCount ++;
                }
            }
        }

        public static string nJoin (this IEnumerable <string> values, nStringsSeparator separator, bool keepsNullAndEmpty = true)
        {
            StringBuilder xBuilder = new StringBuilder ();
            values.nJoin (xBuilder, separator, keepsNullAndEmpty);
            return xBuilder.ToString ();
        }

        // Fri, 26 Apr 2019 01:10:25 GMT
        // 展開時には null が出てこないので引数名が厳密には不正確だが、他と揃えるためそのままにしている
        /// <summary>
        /// 区切りによって得られる要素の数が異なることがあるため、詳しくはコメントを参照。
        /// </summary>
        public static List <string> nSplit (this string value, nStringsSeparator separator, bool keepsNullAndEmpty = true)
        {
            List <string> xValues = new List <string> ();

            // Fri, 26 Apr 2019 01:14:54 GMT
            // 他の多くの文字列系メソッドと同様、null または "" なら無難なものを返す
            // List として null を返すことも考えたが、そうすると呼び出し側のコードが増える

            if (string.IsNullOrEmpty (value))
                return xValues;

            if (separator == nStringsSeparator.Tab ||
                separator == nStringsSeparator.NewLine)
            {
                // Fri, 26 Apr 2019 01:21:32 GMT
                // タブと縦線なら、区切りを「★」として "★" を処理したら要素が二つ得られるが、
                // 改行のときだけ、StringReader の仕様により要素が一つしか得られない
                // これは、頑張ったらタブや縦線と揃えられるが、今度は、*AllLines のラウンドトリップが乱れてくる
                // そちらでは各行に改行がつくため、"★" が2行になる実装では、1行の出力が2行になって返ってくることになる
                // といったところでああだこうだして独自仕様に走って……というのは労力に見合わないため、
                // 文字で区切るなら「区切りの数＋一つ」が得られ、改行ならそれより一つ減ると意識して使い分けるのが良い
                // もっとも、個人的には keepsNullAndEmpty を false にすることが多そうで、それなら結果は同じである
                string [] xSplit = separator == nStringsSeparator.Tab ? value.Split ('\t') : value.nSplitIntoLines ();

                foreach (string xValue in xSplit)
                {
                    if (keepsNullAndEmpty ||
                            string.IsNullOrEmpty (xValue) == false)
                        xValues.Add (xValue);
                }
            }

            else if (separator == nStringsSeparator.VerticalBar)
            {
                // Fri, 26 Apr 2019 01:26:42 GMT
                // それぞれの文字を見ていき、\ なら、次の何かと合わせて xCurrent に入れ、それが正しいエスケープになっているかはそこでは評価しない
                // \ が見付かり、それが文字列の末尾なら、そのときのみフォーマットがおかしいという例外を投げる
                // | が見付かるのは、\ が先行しないときに限られるので、null や "" でも吐くか、xCurrent に何か溜まっているとき、縦線も含めてデコードして吐く
                // それとは別の処理として（つまり、else なしで）、それが文字列の末尾なら、"★" の二つ目を読む必要があるため、そのための処理を行う
                // これをループの外で行う場合、文字列が区切りで終わったのか、それ以外の文字が xCurrent に流れ込んだかの区別が必要で、余計なフラグが必要になる

                StringBuilder xCurrent = new StringBuilder ();

                for (int temp = 0; temp < value.Length; temp ++)
                {
                    char xChar = value [temp];

                    if (xChar == '\\')
                    {
                        if (temp + 1 == value.Length)
                            throw new nInvalidFormatException ();

                        xCurrent.Append ('\\');
                        xCurrent.Append (value [temp + 1]);
                        temp ++;
                    }

                    else if (xChar == '|')
                    {
                        if (keepsNullAndEmpty ||
                            xCurrent.Length > 0)
                        {
                            xValues.Add (nCString.UnescapeC_andVerticalBar (xCurrent.ToString ()));
                            xCurrent.Clear ();
                        }

                        if (keepsNullAndEmpty && temp + 1 == value.Length)
                            xValues.Add (string.Empty);
                    }

                    else xCurrent.Append (xChar);
                }

                // Fri, 26 Apr 2019 01:34:09 GMT
                // 文字列の末尾が区切りでないときのみ xCurrent に何か入るため、
                // temp + 1 == value.Length を見ての処理との衝突はない
                // 当然のことながら、こちらでは Clear は不要

                if (xCurrent.Length > 0)
                    xValues.Add (nCString.UnescapeC_andVerticalBar (xCurrent.ToString ()));
            }

            else throw new nBadOperationException ();

            return xValues;
        }
    }
}

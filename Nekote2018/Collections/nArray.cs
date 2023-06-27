using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // .NET の Array と一部の機能が重複するが、配列に対する典型的な処理をまとめたクラスを用意しておく
    // Array を使ったり、IEnumerable を使ったり、Nekote を使ったりの飛び飛びの実装をできるだけ避けたいため
    // 処理の見える化のため、実装もできるだけ自前で行い、*Comparer 関連で .NET の方が圧倒的に速いところだけ頼っている
    // メソッドの名前や仕様については、いくつかのプログラミング言語を参考とした
    // https://msdn.microsoft.com/en-us/library/system.array.aspx
    // https://msdn.microsoft.com/en-us/library/9eekhta0.aspx
    // https://msdn.microsoft.com/en-us/library/system.string.aspx
    // http://docs.oracle.com/javase/7/docs/api/java/util/Arrays.html
    // http://en.cppreference.com/w/cpp/algorithm

    public static class nArray
    {
        // 比較の処理を伴うメソッドは、多重定義を含めて数が多く、そのうち多くが単純なラッパーであり、コードの水増しになる
        // 一方で、使用頻度も高く、用意しておいて損のないラッパーであるのも確かなので、うるさくならないように #region に入れておく
        #region 比較の処理を伴うメソッド // OK
        public static bool Equals <T> (T [] array1, int index1, T [] array2, int index2, int length)
        {
            // ジェネリックメソッドの実装において、型に関わらずだいたい最適な比較を行いたいときには Default を使う
            // すると、内部で CreateComparer が呼ばれ、型に応じた比較のためのクラスのインスタンスが作られる
            // https://msdn.microsoft.com/en-us/library/ms132123.aspx
            // https://github.com/Microsoft/referencesource/blob/master/mscorlib/system/collections/generic/equalitycomparer.cs
            // https://stackoverflow.com/questions/5857654/equalitycomparert-default-vs-t-equals
            EqualityComparer <T> xComparer = EqualityComparer <T>.Default;

            for (int temp = 0; temp < length; temp ++)
            {
                if (xComparer.Equals (array1 [index1 + temp], array2 [index2 + temp]) == false)
                    return false;
            }

            return true;
        }

        public static bool Equals <T> (T [] array1, T [] array2)
        {
            if (array1.Length != array2.Length)
                return false;

            return Equals (array1, 0, array2, 0, array2.Length);
        }

        public static int Compare <T> (T [] array1, int index1, int length1, T [] array2, int index2, int length2)
        {
            // いずれが大きいか調べたいときには Comparer の Default を使う
            // https://msdn.microsoft.com/en-us/library/cfttsh47.aspx
            // https://github.com/Microsoft/referencesource/blob/master/mscorlib/system/collections/generic/comparer.cs
            Comparer <T> xComparer = Comparer <T>.Default;
            int xLength = Math.Min (length1, length2);

            for (int temp = 0; temp < xLength; temp ++)
            {
                int xResult = xComparer.Compare (array1 [index1 + temp], array2 [index2 + temp]);

                if (xResult != 0)
                    return xResult;
            }

            if (length1 > xLength)
                return 1;
            else if (length2 > xLength)
                return -1;
            else return 0;
        }

        public static int Compare <T> (T [] array1, T [] array2)
        {
            return Compare (array1, 0, array1.Length, array2, 0, array2.Length);
        }

        public static bool Contains <T> (T [] sourceArray, int sourceIndex, int sourceLength, T targetValue)
        {
            return IndexOf (sourceArray, sourceIndex, sourceLength, targetValue) >= 0;
        }

        public static bool Contains <T> (T [] sourceArray, T targetValue)
        {
            return Contains (sourceArray, 0, sourceArray.Length, targetValue);
        }

        public static bool Contains <T> (T [] sourceArray, int sourceIndex, int sourceLength, T [] targetArray, int targetIndex, int targetLength)
        {
            return IndexOf (sourceArray, sourceIndex, sourceLength, targetArray, targetIndex, targetLength) >= 0;
        }

        public static bool Contains <T> (T [] sourceArray, T [] targetArray)
        {
            return Contains (sourceArray, 0, sourceArray.Length, targetArray, 0, targetArray.Length);
        }

        public static bool ContainsAny <T> (T [] sourceArray, int sourceIndex, int sourceLength, T [] targetValues)
        {
            return IndexOfAny (sourceArray, sourceIndex, sourceLength, targetValues) >= 0;
        }

        public static bool ContainsAny <T> (T [] sourceArray, T [] targetValues)
        {
            return ContainsAny (sourceArray, 0, sourceArray.Length, targetValues);
        }

        public static int IndexOf <T> (T [] sourceArray, int sourceIndex, int sourceLength, T targetValue)
        {
            // EqualityComparer やその派生クラスには IndexOf があり、こちらで Default を取得してループを回すより4～5倍速い
            return Array.IndexOf (sourceArray, targetValue, sourceIndex, sourceLength);
        }

        public static int IndexOf <T> (T [] sourceArray, T targetValue)
        {
            return IndexOf (sourceArray, 0, sourceArray.Length, targetValue);
        }

        public static int IndexOf <T> (T [] sourceArray, int sourceIndex, int sourceLength, T [] targetArray, int targetIndex, int targetLength)
        {
            // 配列から配列を探すときには、.NET にそういう機能がないため、ループを回して Default を使う
            // 速度に期待できない実装だが、ちょっとしたところで使うくらいなら開発効率を優先してよい
            EqualityComparer <T> xComparer = EqualityComparer <T>.Default;
            int xCount = sourceLength - targetLength + 1;

            for (int temp = 0; temp < xCount; temp ++)
            {
                int xSourceIndex = sourceIndex + temp;
                bool xIsEqual = true;

                for (int temp1 = 0; temp1 < targetLength; temp1 ++)
                {
                    if (xComparer.Equals (sourceArray [xSourceIndex + temp1], targetArray [targetIndex + temp1]) == false)
                    {
                        xIsEqual = false;
                        break;
                    }
                }

                if (xIsEqual)
                    return xSourceIndex;
            }

            return -1;
        }

        public static int IndexOf <T> (T [] sourceArray, T [] targetArray)
        {
            return IndexOf (sourceArray, 0, sourceArray.Length, targetArray, 0, targetArray.Length);
        }

#pragma warning disable IDE0060
        public static int IndexOfAny <T> (T [] sourceArray, int sourceIndex, int sourceLength, T [] targetValues)
#pragma warning restore IDE0060
        {
            // これも .NET ではできないようなので、ループを回して Default で比較する
            EqualityComparer <T> xComparer = EqualityComparer <T>.Default;

            for (int temp = 0; temp < sourceLength; temp ++)
            {
                T xSourceValue = sourceArray [temp];

                for (int temp1 = 0; temp1 < targetValues.Length; temp1 ++)
                {
                    if (xComparer.Equals (xSourceValue, targetValues [temp1]))
                        return temp;
                }
            }

            return -1;
        }

        public static int IndexOfAny <T> (T [] sourceArray, T [] targetValues)
        {
            return IndexOfAny (sourceArray, 0, sourceArray.Length, targetValues);
        }

        public static int LastIndexOf <T> (T [] sourceArray, int sourceIndex, int sourceLength, T targetValue)
        {
            return Array.LastIndexOf (sourceArray, targetValue, sourceIndex, sourceLength);
        }

        public static int LastIndexOf <T> (T [] sourceArray, T targetValue)
        {
            return LastIndexOf (sourceArray, 0, sourceArray.Length, targetValue);
        }

        public static int LastIndexOf <T> (T [] sourceArray, int sourceIndex, int sourceLength, T [] targetArray, int targetIndex, int targetLength)
        {
            EqualityComparer <T> xComparer = EqualityComparer <T>.Default;
            int xCount = sourceLength - targetLength + 1;

            for (int temp = xCount - 1; temp >= 0; temp --)
            {
                int xSourceIndex = sourceIndex + temp;
                bool xIsEqual = true;

                for (int temp1 = 0; temp1 < targetLength; temp1 ++)
                {
                    if (xComparer.Equals (sourceArray [xSourceIndex + temp1], targetArray [targetIndex + temp1]) == false)
                    {
                        xIsEqual = false;
                        break;
                    }
                }

                if (xIsEqual)
                    return xSourceIndex;
            }

            return -1;
        }

        public static int LastIndexOf <T> (T [] sourceArray, T [] targetArray)
        {
            return LastIndexOf (sourceArray, 0, sourceArray.Length, targetArray, 0, targetArray.Length);
        }

#pragma warning disable IDE0060
        public static int LastIndexOfAny <T> (T [] sourceArray, int sourceIndex, int sourceLength, T [] targetValues)
#pragma warning restore IDE0060
        {
            EqualityComparer <T> xComparer = EqualityComparer <T>.Default;

            for (int temp = sourceLength - 1; temp >= 0; temp --)
            {
                T xSourceValue = sourceArray [temp];

                for (int temp1 = 0; temp1 < targetValues.Length; temp1 ++)
                {
                    if (xComparer.Equals (xSourceValue, targetValues [temp1]))
                        return temp;
                }
            }

            return -1;
        }

        public static int LastIndexOfAny <T> (T [] sourceArray, T [] targetValues)
        {
            return LastIndexOfAny (sourceArray, 0, sourceArray.Length, targetValues);
        }

        // Tue, 25 Sep 2018 13:11:14 GMT
        // ここに BinarySearch と Sort のラッパーを用意することを考えたが、やめておく
        // このクラスでラップしたメソッドの多くは、関連メソッドの一部を自分で実装しなければならないものだった
        // しかし、BinarySearch と Sort は、自分で書いたら極めて遅くなるだろうし、.NET のものが最初から充実している
        // 要素が見付からなかったときに、ではそれをどこに挿入するべきかを返す BinarySearch を用意したかったが、
        // その実装では、速いのは既存かどうかのチェックだけであり、挿入の方が、毎回スライドが起こるため遅い
        // 既存かどうかのチェックと挿入の両方を高速化するには、HashSet を使うべきだろう
        #endregion

        public static void Swap <T> (T [] array, int index1, int index2)
        {
            T xTemp = array [index1];
            array [index1] = array [index2];
            array [index2] = xTemp;
        }

        // 複数の配列を対象とするものも一応用意しておく

        public static void Swap <T> (T [] array1, int index1, T [] array2, int index2)
        {
            T xTemp = array1 [index1];
            array1 [index1] = array2 [index2];
            array2 [index2] = xTemp;
        }

        // Java の fill にならった命名

        public static void Fill <T> (T [] destArray, int destIndex, int destLength, T sourceValue)
        {
            for (int temp = 0; temp < destLength; temp ++)
                destArray [destIndex + temp] = sourceValue;
        }

        public static void Fill <T> (T [] destArray, T sourceValue)
        {
            Fill (destArray, 0, destArray.Length, sourceValue);
        }

        public static void Copy <T> (T [] destArray, int destIndex, T [] sourceArray, int sourceIndex, int sourceLength)
        {
            for (int temp = 0; temp < sourceLength; temp ++)
                destArray [destIndex + temp] = sourceArray [sourceIndex + temp];
        }

        // オーバーライドにおいて destIndex までなくすと、使い道のないメソッドになる

        public static void Copy <T> (T [] destArray, int destIndex, T [] sourceArray)
        {
            Copy (destArray, destIndex, sourceArray, 0, sourceArray.Length);
        }

        // やや内部的なメソッドだが、insert のような実装で多用するため用意しておく

        public static void CopyBackward <T> (T [] destArray, int destIndex, T [] sourceArray, int sourceIndex, int sourceLength)
        {
            for (int temp = sourceLength - 1; temp >= 0; temp --)
                destArray [destIndex + temp] = sourceArray [sourceIndex + temp];
        }

        public static void CopyBackward <T> (T [] destArray, int destIndex, T [] sourceArray)
        {
            CopyBackward (destArray, destIndex, sourceArray, 0, sourceArray.Length);
        }

        public static T [] Clone <T> (T [] array)
        {
            T [] xArray = new T [array.Length];
            Copy (xArray, 0, array);
            return xArray;
        }

        // 配列の一部を新しい配列として複製するメソッド
        // 他の配列への書き込みは、Copy で足りるため用意しない
        // Substring というメソッドがあるため、命名はこれでよいと思う

        public static T [] Subarray <T> (T [] array, int index, int length)
        {
            T [] xArray = new T [length];
            Copy (xArray, 0, array, index, length);
            return xArray;
        }

        public static void Reverse <T> (T [] array, int index, int length)
        {
            int xLastIndex = index + length - 1,
                // 偶数個なら全て Swap し、奇数個なら中央以外をそうする
                xCount = length / 2;

            for (int temp = 0; temp < xCount; temp ++)
                Swap (array, index + temp, xLastIndex - temp);
        }

        public static void Reverse <T> (T [] array)
        {
            Reverse (array, 0, array.Length);
        }

        public static void Rotate <T> (T [] array, int index, int length, int indexIncrement)
        {
            int xIndexIncrement = indexIncrement;

            // インクリメントがマイナスであり、左にスライドさせる指定になっていたら、
            // 割り算などでややこしくせず、長さを何度も足して右へのスライドに切り替える
            // たいてい絶対値が length 未満なので、これが最速の実装だろう

            while (xIndexIncrement < 0)
                xIndexIncrement += length;

            // 何周もスライドさせる指定になっていたら、こちらは割り算で減らす
            xIndexIncrement = xIndexIncrement % length;

            // 追記: コードを何となくチェックしていたときに引数に具体的な数字を当てはめて考えたら困惑したので、コメントを書き足しておく
            // Subarray は、index == 0 とみなすなら、len - inc から inc 分のコピーであり、対象範囲の末尾をいったん他のところにコピーする
            // CopyBackward は、そうやって穴があいたところのみ埋めるのでなく、index == 0 とみなすなら、inc 分以外の残り全てを先頭から右に inc 分ズラす
            // inc 分ズラしたあと、当然、先頭には inc 分の穴があるため、そこに、Subarray でいったんコピーしたものを書き込む

            T [] xTemp = Subarray (array, index + length - xIndexIncrement, xIndexIncrement);
            CopyBackward (array, index + xIndexIncrement, array, index, length - xIndexIncrement);
            Copy (array, index, xTemp);
        }

        public static void Rotate <T> (T [] array, int indexIncrement)
        {
            Rotate (array, 0, array.Length, indexIncrement);
        }

        public static void Shuffle <T> (T [] array, int index, int length)
        {
            // Fisher–Yates shuffle と呼ばれるものを採用
            // 配列をシャッフルしたときに先頭や末尾がそのままだとちゃんと混ざっていないと思い、全体をループに入れたくなるが、
            // 混ざっていないと感じるものを主観的に重く評価しているだけであり、統計的にはきちんと混ざっているようである
            // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle

            // 追記: これもチェック時に引数に具体的な数字を当てはめてみたら困惑したためコメントを書き足す
            // index == 0 とみなすなら、先頭から「末尾の一つ前」までについて、temp から末尾までのうちの一つと置換
            // 先頭から末尾までループを回さないのは、最後が「末尾から末尾までのうちの一つ」となるため
            // 先頭から末尾までを先頭から末尾までと置換した方が均一に混ざるのでないかと直感的には思うが、
            // それでは、混ぜられて移動したものがまた混ぜられて元の位置に戻る可能性が不均一になるような気がする

            for (int temp = 0; temp <= length - 2; temp ++)
                Swap (array, index + temp, index + nRandom.Next (temp, length));
        }

        public static void Shuffle <T> (T [] array)
        {
            Shuffle (array, 0, array.Length);
        }
    }
}

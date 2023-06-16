using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Wed, 24 Apr 2019 02:54:51 GMT
    // サイトに静的ファイルを置きたくて、GUID を URL に入れるものを作ったが、明らかに冗長だった
    // /static/f7ff4e20-949b-4541-83d4-5fea45d9277d/Something.pdf のようなものは、常軌を逸していた
    // そのため、シンプルに /static/ps4d/Something.pdf のようなものを吐くためのクラスを実装した
    // ランダムな文字列を全く入れないというのは、長期運用において同名のファイルの扱いに困るため現実的でないが、
    // Base36 と呼ぶことにする、0-9a-z による36進数を入れておけば、4桁の "zzzz" でも1,679,615まで扱えるため十分である

    // Thu, 25 Apr 2019 01:17:11 GMT
    // ネットでチラッと見た会話からの造語のつもりだったが、存在する言葉のようだ
    // こちらでも割り算をループで繰り返していて、おそらくこれ以上に効率的な実装はない
    // https://en.wikipedia.org/wiki/Base36

    // Wed, 24 Apr 2019 03:16:50 GMT
    // 文字列の長さごとの、整数型での最大値をまとめておく
    // long なら13桁の途中までいけるが、実用的なのは8桁くらい
    // それ以上ではユーザーの短期記憶に収まらず、メリットがなくなる
    // zz: 1295
    // zzz: 46655
    // zzzz: 1679615
    // zzzzz: 60466175
    // zzzzzz: 2176782335
    // zzzzzzz: 78364164095
    // zzzzzzzz: 2821109907455

    // Wed, 24 Apr 2019 03:23:17 GMT
    // xValue = nRandom.Next () をやりながら
    // if (xValue.nToBase36String ().nBase36ToInt () != xValue) を1,000万回見るのが、10年近く前のデスクトップで3,296ミリ秒
    // 3ミリ秒で1万回ということは、1ミリ秒で数千回ということであり、数万回程度なら、体感するほどの遅延にはならないだろう

    // Sat, 28 Sep 2019 02:18:43 GMT
    // Base36 自体は非常に良くできていると思うが、いくつかの問題があって SafeCode を定義
    // 今後、Base36 を使えるところには SafeCode の使用を第一に考えてみるのが良い

    public static class nBase36
    {
        // Fri, 27 Sep 2019 18:14:22 GMT
        // nChar にいろいろと用意したが、以下の組み合わせはないので自前で用意
        // nChar に入れることも考えたが、使用頻度の低いものまで全て揃えるべきでない
        // なお、こちらでは、36進数の順序で配列の内容が定義されている

        public static readonly char [] Chars =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        // Wed, 24 Apr 2019 03:01:14 GMT
        // int と long のコードを個別に実装するのがうるさいが、long 一つにして int からそちらを呼ぶのも気に入らない
        // おそらく64ビットのシステムでは既に速度差がないだろうが、それでも int で足りるところは int で書きたい

        public static string nToBase36String (this int value)
        {
            if (value < 0)
                throw new nNotSupportedException ();

            if (value == 0)
                return "0";

            int xValue = value;
            // Wed, 24 Apr 2019 03:02:33 GMT
            // int.MaxValue が zik0zj になるため、6桁で必ず足りる
            char [] xChars = new char [6];
            int xIndex = xChars.Length;

            while (xValue > 0)
            {
                xIndex --;
                xChars [xIndex] = Chars [xValue % 36];
                xValue /= 36;
            }

            return new string (xChars, xIndex, xChars.Length - xIndex);
        }

        public static string nToBase36String (this long value)
        {
            if (value < 0)
                throw new nNotSupportedException ();

            if (value == 0)
                return "0";

            long xValue = value;
            // Wed, 24 Apr 2019 03:02:40 GMT
            // long.MaxValue が 1y2p0ij32e8e7 になるため、13桁で必ず足りる
            char [] xChars = new char [13];
            int xIndex = xChars.Length;

            while (xValue > 0)
            {
                xIndex --;
                xChars [xIndex] = Chars [xValue % 36];
                xValue /= 36;
            }

            return new string (xChars, xIndex, xChars.Length - xIndex);
        }

        public static int nBase36ToInt (this string text)
        {
            int xSum = 0,
                xBase = 1;

            for (int temp = text.Length - 1; temp >= 0; temp --)
            {
                int xValue;
                char xChar = text [temp];

                if ('0' <= xChar && xChar <= '9')
                    xValue = xChar - '0';
                else if ('a' <= xChar && xChar <= 'z')
                    xValue = xChar - 'a' + 10;
                else if ('A' <= xChar && xChar <= 'Z')
                    xValue = xChar - 'A' + 10;
                else throw new nInvalidFormatException ();

                xSum += xBase * xValue;
                xBase *= 36;
            }

            return xSum;
        }

        public static int nBase36ToIntOrDefault (this string text, int value)
        {
            try
            {
                return text.nBase36ToInt ();
            }

            catch
            {
                return value;
            }
        }

        public static long nBase36ToLong (this string text)
        {
            long xSum = 0,
                xBase = 1;

            for (int temp = text.Length - 1; temp >= 0; temp --)
            {
                int xValue;
                char xChar = text [temp];

                if ('0' <= xChar && xChar <= '9')
                    xValue = xChar - '0';
                else if ('a' <= xChar && xChar <= 'z')
                    xValue = xChar - 'a' + 10;
                else if ('A' <= xChar && xChar <= 'Z')
                    xValue = xChar - 'A' + 10;
                else throw new nInvalidFormatException ();

                xSum += xBase * xValue;
                xBase *= 36;
            }

            return xSum;
        }

        public static long nBase36ToLongOrDefault (this string text, long value)
        {
            try
            {
                return text.nBase36ToLong ();
            }

            catch
            {
                return value;
            }
        }

        // Wed, 24 Apr 2019 03:05:50 GMT
        // 構文解析に失敗すると落ちるし、短い文字列でも long で比較するのがもったいないため、積極的に使わない方がいいかもしれない
        // しかし、構文解析が大丈夫と分かっているなら、long も int もぶっちゃけ速度差はないだろうから、気にすることでもないか

        public static int CompareNumerically (string text1, string text2) =>
            text1.nBase36ToLong ().CompareTo (text2.nBase36ToLong ());

        // Wed, 24 Apr 2019 03:07:12 GMT
        // int を nRandom でもらってきての変換では、"0000" から "zzzz" までとしてもらうことができない
        // int からの変換において、得られるランダムな Base36 文字列が数学的にちゃんと散らばっている確証もない
        // そのため、パフォーマンスは劣るだろうが、指定した長さの文字列を一発で得られるものも用意した

        public static string Next (int length)
        {
            char [] xChars = new char [length];

            for (int temp = 0; temp < length; temp ++)
                xChars [temp] = Chars [nRandom.Next (0, 36)];

            return new string (xChars);
        }
    }
}

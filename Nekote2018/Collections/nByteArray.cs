using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Thu, 27 Sep 2018 08:13:45 GMT
    // nBytes というのも考えたが、それでは List <byte> 用のクラスを作るとしたときに名前が不揃いになる
    // 「データの型＋入れ物の型」としておけば、nByteList も作れるし、長期的にネーミングがグダグダにならない

    public static class nByteArray
    {
        // Thu, 27 Sep 2018 08:15:33 GMT
        // これも nToString としては、16進数の文字列がデフォルトの仕様という扱いになる
        // 戻す方では、nInvariantToDateTime などにならい、「仕様」と「型」を名前に入れておく

        // Thu, 27 Sep 2018 08:39:39 GMT
        // さまざまな変換方法について真剣にベンチマークを行った人がいるようなのでリンクを貼っておく
        // 私の方法は、遅いのが分かり切っている byte.ToString を使っていないため、特に速くもなくても実用的なレベルだろう
        // https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa

        public static string nToHexString (this byte [] bytes)
        {
            char [] xChars = new char [bytes.Length * 2];

            for (int temp = 0; temp < bytes.Length; temp ++)
            {
                int xFirst = bytes [temp] >> 4,
                    xSecond = bytes [temp] & 0x0f;

                if (xFirst <= 9)
                    xFirst = xFirst + '0';
                else xFirst = xFirst + 'a' - 10;

                if (xSecond <= 9)
                    xSecond = xSecond + '0';
                else xSecond = xSecond + 'a' - 10;

                xChars [temp * 2] = (char) xFirst;
                xChars [temp * 2 + 1] = (char) xSecond;
            }

            return new string (xChars);
        }

        public static byte [] nHexToByteArray (this string text)
        {
            if (text.Length % 2 != 0)
                throw new nInvalidFormatException ();

            int xCount = text.Length / 2;
            byte [] xBytes = new byte [xCount];

            for (int temp = 0; temp < xCount; temp ++)
            {
                // Thu, 27 Sep 2018 08:29:23 GMT
                // char で受け取ると、後続の引き算のところで int となり、キャストが必要
                // キャストを最後に一度だけにするには、int で受け取ってしまうのが良い

                int xFirst = text [temp * 2],
                    xSecond = text [temp * 2 + 1];

                // Thu, 27 Sep 2018 08:30:32 GMT
                // 出力時には小文字で決め打ちだが、それは GUID なども同じ
                // 一方、構文解析時には、数の多さと出現頻度に応じて三つとも見ておく

                if ('0' <= xFirst && xFirst <= '9')
                    xFirst = xFirst - '0';
                else if ('a' <= xFirst && xFirst <= 'f')
                    xFirst = xFirst - 'a' + 10;
                else if ('A' <= xFirst && xFirst <= 'F')
                    xFirst = xFirst - 'A' + 10;
                else throw new nInvalidFormatException ();

                if ('0' <= xSecond && xSecond <= '9')
                    xSecond = xSecond - '0';
                else if ('a' <= xSecond && xSecond <= 'f')
                    xSecond = xSecond - 'a' + 10;
                else if ('A' <= xSecond && xSecond <= 'F')
                    xSecond = xSecond - 'A' + 10;
                else throw new nInvalidFormatException ();

                xBytes [temp] = (byte) (xFirst << 4 | xSecond);
            }

            return xBytes;
        }

        // Thu, 27 Sep 2018 08:31:49 GMT
        // バイト列を単一の int にできるメソッドが .NET にないようなので簡易的に実装
        // CRC-32 も考えたが、外部ライブラリーに頼ることになるし、
        // 重複してよいハッシュコードの生成には過剰で、かといってセキュリティー的には論外
        // nString でも実装した、Java の古いコードによるものでごまかしておく
        // unchecked についてのコメントは、そちらを参照

        public static int nGetHashCode (this byte [] bytes)
        {
            int xHash = 0;

            for (int temp = 0; temp < bytes.Length; temp ++)
                xHash = unchecked (31 * xHash + bytes [temp]);

            return xHash;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Security.Cryptography;

namespace Nekote
{
    public static class nHash
    {
        // Sun, 31 Mar 2019 03:16:02 GMT
        // パッと使えて分かりやすいものを用意しておく

        public static bool Equals (byte [] hash1, byte [] hash2) =>
            nArray.Equals (hash1, hash2);

        // Sun, 31 Mar 2019 02:37:38 GMT
        // SHA256 のハッシュ計算をよくやるので、そのためのメソッドを用意しておく
        // SHA256 クラスのインスタンスを使い回すのは、以下のページによると問題とのことで、試してみたらそうだった
        // ついでにマルチスレッドも試したく、1) 10のスレッドで、2) 10秒間、同じ文字列のハッシュを計算・比較してみたところ、
        // インスタンスの使い回しでは、例外は飛ばなかったが、ものすごい数の不一致が起こり、
        // インスタンスを使い回さない現行のコードでは、7773156回の計算において不一致が起こらなかった
        // 何度もインスタンスを生成するのが重たそうで使い回していたが、
        // だから一部のプログラムで、ハッシュが一致せず、ファイル本体の比較のコードに入り、そこで重たくなっていたのかもしれない
        // そちらの方は重要なプログラムでなく、一応の正常動作が可能なので正さないが、
        // インスタンスの生成が思っていたより低コストと知ったので、今後はこのクラスを常用する
        // https://medium.com/@jamesikanos/c-cautionary-tail-the-dangers-of-sha256-reuse-2b5bb9c6fde9

        public static byte [] ComputeSha256 (byte [] bytes)
        {
            using (SHA256 xSha256 = SHA256.Create ())
            {
                return xSha256.ComputeHash (bytes);
            }
        }

        public static byte [] ComputeSha256 (byte [] bytes, int index, int length)
        {
            using (SHA256 xSha256 = SHA256.Create ())
            {
                return xSha256.ComputeHash (bytes, index, length);
            }
        }

        public static byte [] ComputeSha256OfUtf8String (string text)
        {
            using (SHA256 xSha256 = SHA256.Create ())
            {
                // Sun, 31 Mar 2019 02:46:20 GMT
                // 長さ0のバイト列でもランダムなハッシュが計算されるため、文字列の方も null では落ちないようにしておく
                return xSha256.ComputeHash (Encoding.UTF8.GetBytes (text ?? string.Empty));
            }
        }

        public static byte [] ComputeSha256OfFile (string path)
        {
            using (SHA256 xSha256 = SHA256.Create ())
            using (FileStream xStream = File.OpenRead (path))
            {
                // Sun, 31 Mar 2019 02:47:31 GMT
                // 4096バイトずつの読み込みが行われるようである
                // http://www.dotnetframework.org/default.aspx/DotNET/DotNET/8@0/untmp/whidbey/REDBITS/ndp/clr/src/BCL/System/Security/Cryptography/HashAlgorithm@cs/1/HashAlgorithm@cs
                return xSha256.ComputeHash (xStream);
            }
        }

        // Sun, 31 Mar 2019 02:49:35 GMT
        // nString.GetHashCode と同様の実装でバイト列のハッシュを計算できるようにしておく
        // 精度がかなり下がるが、int の比較で済むというメリットがあり、比較回数が極めて多いときには有用性が出てくるか
        // ref int に結果を入れるので、0を与えて length が0なら、何もされずに0のままである

        private static void iComputeSimple (byte [] bytes, int index, int length, ref int hash)
        {
            for (int temp = 0; temp < length; temp ++)
                hash = unchecked (31 * hash + bytes [index + temp]);
        }

        public static int ComputeSimple (byte [] bytes)
        {
            // Sun, 31 Mar 2019 02:54:19 GMT
            // bytes の長さが0の場合、if 文で対応しなくても0が返る

            int xHash = 0;
            iComputeSimple (bytes, 0, bytes.Length, ref xHash);
            return xHash;
        }

        public static int ComputeSimple (byte [] bytes, int index, int length)
        {
            // Sun, 31 Mar 2019 02:55:48 GMT
            // length が0の場合、if 文で対応しなくても0が返る

            int xHash = 0;
            iComputeSimple (bytes, index, length, ref xHash);
            return xHash;
        }

        public static int ComputeSimpleOfUtf8String (string text) =>
            ComputeSimple (Encoding.UTF8.GetBytes (text ?? string.Empty));

        public static int ComputeSimpleOfFile (string path)
        {
            // Sun, 31 Mar 2019 02:57:09 GMT
            // ファイルが開けないなら、バッファーを生成する前に落ちるべき
            // 開けるなら、BufferLength に基づき、4096バイトずつ読み込んで計算
            // バッファーの大きさを4096バイトとする理由については、BufferLength の方に書いておく

            using (FileStream xStream = File.OpenRead (path))
            {
                int xHash = 0;
                byte [] xBytes = new byte [nFile.BufferLength];
                int xReadLength;

                while ((xReadLength = xStream.Read (xBytes, 0, nFile.BufferLength)) > 0)
                    iComputeSimple (xBytes, 0, xReadLength, ref xHash);

                return xHash;
            }
        }

        // Sun, 31 Mar 2019 03:18:41 GMT
        // 0x をつけたいことがたまにあるため、きれいに書けるようにしておく

        public static string ToHexString (byte [] hash, string prefix = null, string suffix = null) =>
            prefix + hash.nToHexString () + suffix;
    }
}

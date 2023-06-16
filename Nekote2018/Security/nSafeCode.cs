using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Sat, 28 Sep 2019 01:28:44 GMT
    // Base36 のサブセットの仕様による小さなクラスなので、コメントをここにまとめて書く
    //
    // int でも long でも使える Base36 を気に入っているが、nRandom.Next で int を得るのでは、
    // それを Base36 にしたものが短いとき、安全性が十分でない可能性およびパッと見で安全そうに見えない問題がある
    // たとえば、総当たり攻撃で ID を抜くとき、攻撃プログラムが数学的にランダムな ID を試すなら、探される側の ID の長さは安全性と無関係である
    // つまり、0から順番に文字数も内容も変えていくのでなく、文字数も内容もランダムなプログラムなら、ID が "0" だけでも、そこに至る確率は他の ID と異ならない
    // しかし、"" や数文字という手抜きの設定が残っていることを想定する賢いプログラムなら、まずそのあたりを調べてから、時間のかかる総当たり攻撃を始める
    // 見た目の問題もあり、「このコードがあなたのセキュリティーコードですよ」的なことを言われてのコードが "0" だけでは、それでいいのかと不安に感じる人もいるだろう
    //
    // そこで、"100000000000" 以上、"zzzzzzzzzzzz" 以下の12桁の Base36 文字列を SafeCode と呼び、それを、これまで Base36 を使っていたところで使う
    // 12桁なのは、long.MaxValue が Base36 では13桁の途中であり、long で埋められるのは12桁までで、11桁以下がなくても12桁のものだけで膨大な範囲だからである
    // YouTube は、パッと見た ID が、小文字、大文字、数字を含む11桁で、13桁もあった気がする
    // 13桁が勘違いだとしても、11桁の Base62 になるため、12桁の Base36 と比べても、なおのこと途方もない範囲である
    // しかし、SafeCode を私はファイルシステムのパスなどにも使いたいため、大文字・小文字の区別が強制される仕様を避けたい
    //
    // 12桁として "000000000000" も認め、0詰めを必須とすることや、long の全域を使い、0詰めありの13桁にすることなども考えたが、
    // どこかでミスがあって0詰めを忘れてしまっては SafeCode の目的の一つである「パッと見の安心感」が損なわれるし、セキュリティーのリスクも出てくる
    //
    // あくまでセキュリティーを目的とするものなので、各メソッドにおいて、SafeCode として正しい範囲内かどうかのチェックが徹底される
    // そのチェックを通らなかったなら、bad operation というより、SafeCode ではサポートされていない範囲という扱いで、not supported を投げる
    // 文字列化において *OrEmpty などまで用意するのは、try / catch なしで「無効」を調べたいことがあるかもしれないため
    // チェックが厳密なので CompareNumerically でも行われるが、これはさすがにパフォーマンスに影響があるため、Sort には渡さないのが良い
    // もっとも、普通にプログラミングをしていれば、SafeCode を long に変換してクラスのインスタンスに入れるだろうから、Sort ではそちらを比較するべき
    // Next が最初の文字を見るのは、上記の0詰めに頼らない仕様に基づき、12桁でしか表現しようのない SafeCode のみ使いたいからである
    //
    // 念のため、nSafeCode.Next ().nSafeCodeToLong ().nToSafeCode (); を100万回行うベンチマークを行ったところ、平均1,600ミリ秒くらいだった
    // そもそも Base36 の計算量がけっこうあり、それを引き継ぐ SafeCode も予想通り中途半端に遅くなったが、1ミリ秒で600回と考えるなら、使えないわけでもない
    // long にする必要がなく、Next で生成したものをそのままランダム文字列として使えるところも多いため、パフォーマンスについては大丈夫とみなす
    //
    // なお、実装においては、Base36 系のメソッドで足りるところにはできるだけそれらを使い、オーバーヘッドの削減に努めた

    public static class nSafeCode
    {
        public static readonly long MinLongValue = "100000000000".nBase36ToLong ();

        public static readonly long MaxLongValue = "zzzzzzzzzzzz".nBase36ToLong ();

        public static string nToSafeCode (this long value)
        {
            if (value < MinLongValue || value > MaxLongValue)
                throw new nNotSupportedException ();

            return value.nToBase36String ();
        }

        public static string nToSafeCodeOrDefault (this long value, string text)
        {
            if (value < MinLongValue || value > MaxLongValue)
                return text;

            return value.nToBase36String ();
        }

        public static string nToSafeCodeOrEmpty (this long value) =>
            nToSafeCodeOrDefault (value, string.Empty);

        public static string nToSafeCodeOrNull (this long value) =>
            nToSafeCodeOrDefault (value, null);

        public static long nSafeCodeToLong (this string text)
        {
            long xValue = text.nBase36ToLong ();

            if (xValue < MinLongValue || xValue > MaxLongValue)
                throw new nNotSupportedException ();

            return xValue;
        }

        public static long nSafeCodeToLongOrDefault (this string text, long value)
        {
            try
            {
                long xValue = text.nBase36ToLong ();

                if (xValue < MinLongValue || xValue > MaxLongValue)
                    return value;

                return xValue;
            }

            catch
            {
                return value;
            }
        }

        public static int CompareNumerically (string text1, string text2)
        {
            long xValue1 = text1.nBase36ToLong ();

            if (xValue1 < MinLongValue || xValue1 > MaxLongValue)
                throw new nNotSupportedException ();

            long xValue2 = text2.nBase36ToLong ();

            if (xValue2 < MinLongValue || xValue2 > MaxLongValue)
                throw new nNotSupportedException ();

            return xValue1.CompareTo (xValue2);
        }

        public static string Next ()
        {
            while (true)
            {
                string xCode = nRandom.NextString (12, 12, true, false, true);

                if (xCode [0] != '0')
                    return xCode;
            }
        }

        public static long NextLong () =>
            Next ().nBase36ToLong ();
    }
}

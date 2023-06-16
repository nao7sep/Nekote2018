using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Sat, 28 Sep 2019 17:54:01 GMT
    // 各所に文字やそれを HTML に埋め込むときの文字列を散在させ始めていたのを改善
    // 一貫性のあるルールにより、今後、使う文字だけをその都度ここに入れていく
    // https://www.compart.com/en/unicode/html

    // Sat, 28 Sep 2019 18:01:05 GMT
    // nSimplePageAccessLogEntryCsvIndices.cs に長ったらしく書いた理由により、const にした
    // 「const かプロパティーか」の二択において、周囲に char [] などもなく、クラス全体を const にできるため
    // { get; private set; } も考えたが、全体を const で統一できるなら、プロパティーでないといけない理由がない

    // Tue, 01 Oct 2019 02:45:23 GMT
    // 「定数の扱い方.txt」に書いた理由により、全体を static readonly に変更した
    // 以下は Unicode の仕様であり、変更の可能性が極めて低いが、それでも const を避ける
    // static readonly でいけない理由がなく、そちらの方が適用範囲が広いため

    public static class nHtmlChars
    {
        // Sat, 28 Sep 2019 17:55:16 GMT
        // 通知メールのタイトルのデフォルト値に « が必要で、それと対応する » は、パンくずリストに今後役立つ
        // この類いの文字は他にもあり、半角の <、その全角版、文字コード値がより大きい « の別文字、<<< のようになっているものが確認されている
        // そのうち、Nekote では、文字コード値が小さく、HTML の方でも固有の識別子が与えられているものを各部に一貫して使っていく
        // https://www.compart.com/en/unicode/U+00AB
        // https://www.compart.com/en/unicode/U+00BB

        public static readonly char LeftPointingDoubleAngleQuotationMark = '«';

        public static readonly string LeftPointingDoubleAngleQuotationMarkHtml = "&laquo;";

        public static readonly char RightPointingDoubleAngleQuotationMark = '»';

        public static readonly string RightPointingDoubleAngleQuotationMarkHtml = "&raquo;";

        // Sat, 28 Sep 2019 18:03:41 GMT
        // diff のところで、改行の追加や削除を示すときに使っている記号
        // 折れている矢印の記号もあるが、「改行」固有のものはこれだけのようである
        // https://www.compart.com/en/unicode/U+23CE

        public static readonly char ReturnSymbol = '⏎';

        public static readonly string ReturnSymbolHtml = "&#x23ce;";
    }
}

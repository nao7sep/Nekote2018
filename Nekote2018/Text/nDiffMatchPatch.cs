using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiffMatchPatch;

namespace Nekote
{
    public static class nDiffMatchPatch
    {
        // Tue, 04 Dec 2018 15:43:21 GMT
        // 内部的に呼ばれるメソッドなので、tag などの HTML エスケープを行わない
        // 不正な文字が流れ込んでいないかどうかは、呼び出し側でチェックする

        private static string iGenerateStartTag (string tag, string @class) =>
            $"<{tag}{(string.IsNullOrEmpty (@class) == false ? $" class=\"{@class}\"" : null)}>";

        private static string iGenerateEndTag (string tag) =>
            $"</{tag}>";

        // Tue, 04 Dec 2018 15:43:56 GMT
        // diff_match_patch.diff_prettyHtml の出力がイマイチなので、より良いものを自作
        // いろいろとカスタマイズできるようにしたが、配列を使っているので、そう遅くないはず

        // Fri, 29 Mar 2019 08:05:11 GMT
        // 最初、brTag のみ受け取っての処理だったが、それでは改行の増減が見えなくて不便だった
        // 増減の明示には、三つそれぞれについて記号および <br /> の表示を考える必要がある

        // Sun, 31 Mar 2019 03:43:49 GMT
        // 慣例を考えたら escapedEquTag のようにするのが適切と思われるが、
        // 要素名や属性名をユーザーに書き出させることはなく、escaped を入れても安全性の向上は見込めない
        // escaped の類いのものは、タイトルや本文といったものを受け取るところに入れるだけでいい

        public static string ToPrettierHtml (List <Diff> diffs,
            string equTag, string equClass, string equNewLine,
            string insTag, string insClass, string insNewLine,
            string delTag, string delClass, string delNewLine)
        {
            // Tue, 04 Dec 2018 15:51:22 GMT
            // diffs に問題があるときに null を返すようなことをしない
            // null がそのまま通ると保証されなければならないところではない

            StringBuilder xBuilder = new StringBuilder ();

            // Tue, 04 Dec 2018 15:45:24 GMT
            // 現時点では Operation の要素に値が指定されておらず、0, 1, 2 になっている
            // 枯れているコードなのでたぶんこのままだろうが、
            // 万が一にも変更されるとすれば 1, 2, 3 の明示的な指定なので、
            // 念のため、それでも配列の長さが足りるようにしておく

            string [] xStartTags = new string [3 + 1],
                xNewLines = new string [3 + 1],
                xEndTags = new string [3 + 1];

            if (string.IsNullOrEmpty (equTag) == false)
            {
                // Tue, 04 Dec 2018 15:49:34 GMT
                // enum でも nToInt を使うコーディングをよくするが、
                // 要素名でアクセスしていて、しかも速度が必要なところでは、(int) でもよい
                xStartTags [(int) Operation.EQUAL] = iGenerateStartTag (equTag, equClass);
                xEndTags [(int) Operation.EQUAL] = iGenerateEndTag (equTag);
            }

            else
            {
                // Tue, 04 Dec 2018 15:48:24 GMT
                // 初期化していない領域にアクセスするのは、どうも気になる
                xStartTags [(int) Operation.EQUAL] = null;
                xEndTags [(int) Operation.EQUAL] = null;
            }

            xStartTags [(int) Operation.INSERT] = iGenerateStartTag (insTag, insClass);
            xEndTags [(int) Operation.INSERT] = iGenerateEndTag (insTag);

            xStartTags [(int) Operation.DELETE] = iGenerateStartTag (delTag, delClass);
            xEndTags [(int) Operation.DELETE] = iGenerateEndTag (delTag);

            // Fri, 29 Mar 2019 08:06:55 GMT
            // equTag が null でも <br /> への変換は不可避なので equNewLine まで null ということはない
            // 初期値を使うという指定のために equTag に null を指定できるようにする選択肢もあったが、分かりにくくなりそう
            xNewLines [(int) Operation.EQUAL] = equNewLine;
            xNewLines [(int) Operation.INSERT] = insNewLine;
            xNewLines [(int) Operation.DELETE] = delNewLine;

            foreach (Diff xDiff in diffs)
            {
                xBuilder.Append (xStartTags [(int) xDiff.operation]);
                xBuilder.Append (xDiff.text.nEscapeHtml ().nReplaceNewLines (xNewLines [(int) xDiff.operation]));
                xBuilder.Append (xEndTags [(int) xDiff.operation]);
            }

            return xBuilder.ToString ();
        }

        // Fri, 29 Mar 2019 09:59:13 GMT
        // 多重定義の方では、デフォルト値を用意しながら、使う記号のみ文字列として受け取る
        // 「リターン記号」にするのか、&crarr; という文字参照が用意されている「キャリッジ・リターン」にするのか迷ったが、
        // どちらも Unicode でちゃんと定義されていて、最近のブラウザーが特殊文字だけでなく顔文字まで扱う以上、目立つ方が便利である
        // https://graphemica.com/%E2%8F%8E
        // https://ja.wikipedia.org/wiki/%E3%83%AA%E3%82%BF%E3%83%BC%E3%83%B3%E8%A8%98%E5%8F%B7
        // https://gray-code.com/html_css/list-of-symbols-and-special-characters/

        // Sun, 31 Mar 2019 03:46:08 GMT
        // デフォルト値を決め打ちで入れていたが、やはり気持ち悪くて変数に移した

        public static string ToPrettierHtml (List <Diff> diffs, string newLineSymbol = null) =>
            ToPrettierHtml (diffs,
                null, null, "<br />",
                "ins", "xInserted", (newLineSymbol ?? nHtmlChars.ReturnSymbolHtml) + "<br />",
                "del", "xDeleted", newLineSymbol ?? nHtmlChars.ReturnSymbolHtml);

        // Tue, 04 Dec 2018 15:53:46 GMT
        // 動詞として使うときには、cleanups でなく cleans up になるとのこと
        // https://grammarist.com/usage/cleanup-clean-up/
        public static string DiffToPrettierHtml (string text1, string text2, bool cleansUp,
            string equTag, string equClass, string equNewLine,
            string insTag, string insClass, string insNewLine,
            string delTag, string delClass, string delNewLine)
        {
            diff_match_patch xDmp = new diff_match_patch ();
            List <Diff> xDiffs = xDmp.diff_main (text1, text2);

            if (cleansUp)
            {
                // Tue, 04 Dec 2018 16:01:23 GMT
                // いくつかあるので、現時点におけるコード内のコメントをコピペしておく
                // 日本語は、何も呼ばなくてもうまく処理されるが、英語は、diff_cleanupSemantic が必要
                // 公式のサンプルでも呼ばれているし、これはオン・オフできるようにすることもなく、決め打ちで呼んでよい
                // diff_cleanupSemantic
                    // Reduce the number of edits by eliminating semantically trivial equalities.
                // diff_cleanupSemanticLossless
                    // Look for single edits surrounded on both sides by equalities which can be shifted sideways to align the edit to a word boundary.
                // diff_cleanupEfficiency
                    // Reduce the number of edits by eliminating operationally trivial equalities.
                // diff_cleanupMerge
                    // Reorder and merge like edit sections. Merge equalities. Any edit section can move as long as it doesn't cross an equality.

                // Fri, 29 Mar 2019 08:10:33 GMT
                // 一つ上のコメントに「オン・オフできるようにすることもなく」とあるが、そうできるようにしている
                // 半分寝ながら書いたコメントなのか、今の私が何か勘違いしているのか不明だが、とりあえず放置で様子を見る

                xDmp.diff_cleanupSemantic (xDiffs);
            }

            return ToPrettierHtml (xDiffs,
                equTag, equClass, equNewLine,
                insTag, insClass, insNewLine,
                delTag, delClass, delNewLine);
        }

        public static string DiffToPrettierHtml (string text1, string text2, bool cleansUp = true, string newLineSymbol = null) =>
            DiffToPrettierHtml (text1, text2, cleansUp,
                null, null, "<br />",
                "ins", "xInserted", (newLineSymbol ?? nHtmlChars.ReturnSymbolHtml) + "<br />",
                "del", "xDeleted", newLineSymbol ?? nHtmlChars.ReturnSymbolHtml);
    }
}

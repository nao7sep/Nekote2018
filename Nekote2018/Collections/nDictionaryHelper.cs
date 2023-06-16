using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.IO;

namespace Nekote
{
    public static class nDictionaryHelper
    {
        // キーと値のペアを、key:escapedValue のようにして各行に一つ置き、リストとしたものを KVP フォーマットと呼ぶ
        // INI と似ているが、使う記号は = でなく : であり、セクションの概念がなく、コメントを書けるが、そちらも書き方が異なる
        // : を使うのは、同種と言える , . ; : をうまく使い分けることで将来的に機能性を高めていく可能性があるため
        // たとえば、@ を at でなく as と読み、key@jagged:r1f1,r1f2;,,r2f3 のようにジャグ配列を表現できれば便利かもしれない
        // key@base64 も可能だし、Base64 では +, /, = しか使われないため、key@file:escapedName;... というのも可能

        public static void nToKvpString (this nDictionary dictionary, StringBuilder builder)
        {
            foreach (var xPair in dictionary)
            {
                builder.Append (xPair.Key);
                builder.Append (':');
                builder.AppendLine (xPair.Value.nEscapeC ());
            }
        }

        public static string nToKvpString (this nDictionary dictionary)
        {
            StringBuilder xBuilder = new StringBuilder ();
            dictionary.nToKvpString (xBuilder);
            return xBuilder.ToString ();
        }

        // overwrites は、同じキーによる値の上書きを認めるか、あるいは例外を投げるかである
        // 複数のファイルから情報をかき集めるようなことがよくあるため、デフォルトで true としている

        public static void nKvpToDictionary (this string text, nDictionary dictionary, bool overwrites = true)
        {
            // Fri, 26 Apr 2019 03:16:19 GMT
            // わざわざ落とすようなことでないためメソッドを抜ける

            if (string.IsNullOrEmpty (text))
                return;

            // 以前は nSplitIntoLines を使っていたが、Nekote 内ではできるだけ StringReader を使う
            // foreach は遅いようだし、たとえ読み出した複数行を List にいったん入れるだけでも無駄な処理である

            using (StringReader xReader = new StringReader (text))
            {
                string xLine;

                while ((xLine = xReader.ReadLine ()) != null)
                {
                    // 空行を無視

                    if (xLine.Length > 0)
                    {
                        if (xLine.nStartsWith ("//"))
                        {
                            // 先頭に // がある行全体をコメントとみなす
                            // パフォーマンスが低下するため、インデントは認めない
                        }

                        else
                        {
                            // 値が "" なら Key: とし、null なら Key のみ出力する選択肢もある
                            // それでもキーの復元は可能であり、"" と null の区別が役に立つこともあるだろう
                            // しかし、Key のみの行というのは、ユーザーにとって仕様として分かりにくい
                            // "" も null も null とした上、呼び出し側で工夫する方が賢明

                            int xIndex = xLine.IndexOf (':');

                            if (xIndex >= 0)
                            {
                                string xKey = xLine.Substring (0, xIndex);
                                int xValueLength = xLine.Length - (xIndex + 1);
                                string xValue = xValueLength > 0 ? xLine.Substring (xIndex + 1, xValueLength).nUnescapeC () : null;

                                if (overwrites)
                                    dictionary.SetValue (xKey, xValue);
                                else dictionary.AddValue (xKey, xValue);
                            }

                            else throw new nInvalidFormatException ();
                        }
                    }
                }
            }
        }

        public static nDictionary nKvpToDictionary (this string text, bool ignoresCase = false, bool overwrites = true)
        {
            nDictionary xDictionary = new nDictionary (ignoresCase);

            if (string.IsNullOrEmpty (text) == false)
                text.nKvpToDictionary (xDictionary, overwrites);

            return xDictionary;
        }

        // Fri, 26 Apr 2019 04:25:26 GMT
        // nDictionary の内容をメールの本文などとしてそのまま送りたいことが多いため、そのためのメソッドを用意しておく
        // 可読性を考えたフォーマットなので friendly という表現を用い、DateTime などが Ticks では分からないので、その対処もする

        // Fri, 26 Apr 2019 04:43:58 GMT
        // nNameValueCollectionHelper.nToFriendlyString の実装を始めたが、使う気がしなくて面倒になってやめた
        // 各キーに複数の値を設定できるのに、.NET でも、そのうち先頭の要素だけを使っていて、いろいろと設計がアホすぎる
        // 値が複数のときのノーマライズをどうするかとか、フォーマットはどうするかとかは、考えないと実装できないが、考えても利益がない
        // レガシーコードに入っているから仕方なく使うクラスであり、新規開発で積極的に使うものでない

        // Mon, 20 May 2019 20:44:16 GMT
        // なんだかんだ言って、nNameValueCollection にも用意した
        // クエリー文字列を一覧表示するなど、使い道がないわけでもなさそう

        public static void nToFriendlyString (this nDictionary dictionary, StringBuilder builder, bool keepsNullAndEmpty = true, string [] dateTimeKeys = null, string [] timeSpanKeys = null)
        {
            int xCount = 0;

            foreach (var xPair in dictionary.Dictionary)
            {
                string xNormalized = xPair.Value.nNormalizeAuto ();

                if (keepsNullAndEmpty ||
                    string.IsNullOrEmpty (xNormalized) == false)
                {
                    if (xCount > 0)
                        builder.AppendLine ();

                    builder.Append ('[');
                    builder.Append (xPair.Key.nToFriendlyString ());
                    builder.Append ("]\r\n");

                    // Fri, 26 Apr 2019 04:27:13 GMT
                    // null や "" をそのまま空行のように表示しても分かりにくいため、決め打ちの文字列にする
                    // DateTime などもキーだけ存在することがあり得るため、まず null などの処理を行う
                    // 値が空でないなら、DateTime などでないか調べ、そうなら Ticks より可読性の高いフォーマットにする
                    // UTC とは限らないため、Rfc1123DateTimeUniversal にせず、いろいろある中で最もユーザーフレンドリーな FriendlyDateTime を使う
                    // TimeSpan の方に選択肢が乏しいため、そちらと揃えるために nToInvariantString も考えたが、そもそもこれら二つの invariant には整合性がない
                    // 何か一つ invariant のフォーマットを用意しなければならなくてそうしただけであり、元々異なるものなのでメソッド名だけ揃えても仕方ない
                    // DateTime の方には FullDateTime などもあるが、nDictionary の内容を見るにおいて曜日は必ずしも必要なものでないため冗長
                    // DateTime と TimeSpan に共通することとして、ちゃんとした Ticks になっていなければ落ちるが、これは仕様である
                    // これらの型の値を想定するところがそうなっていないなら、呼び出し側にミスがあるか、データが破損しているかであり、対処が必要

                    if (string.IsNullOrEmpty (xNormalized))
                        builder.AppendLine (xNormalized.nToFriendlyString ());

                    else
                    {
                        if (dateTimeKeys != null && dateTimeKeys.Contains (xPair.Key, StringComparer.InvariantCultureIgnoreCase))
                            builder.AppendLine (xNormalized.nLongToDateTime ().nToString (nDateTimeFormat.FriendlyDateTime));
                        else if (timeSpanKeys != null && timeSpanKeys.Contains (xPair.Key, StringComparer.InvariantCultureIgnoreCase))
                            builder.AppendLine (xNormalized.nLongToTimeSpan ().nToInvariantString ());
                        else builder.AppendLine (xNormalized);
                    }

                    xCount ++;
                }
            }
        }

        public static string nToFriendlyString (this nDictionary dictionary, bool keepsNullAndEmpty = true, string [] dateTimeKeys = null, string [] timeSpanKeys = null)
        {
            StringBuilder xBuilder = new StringBuilder ();
            dictionary.nToFriendlyString (xBuilder, keepsNullAndEmpty, dateTimeKeys, timeSpanKeys);
            return xBuilder.ToString ();
        }

        // Sun, 19 May 2019 01:34:21 GMT
        // 辞書の内容をフラットにするメソッドが欲しくて実装した
        // この逆も用意し、1行で辞書を初期化できるようにする

        public static void nToStringList (this nDictionary dictionary, List <string> list)
        {
            // Sun, 19 May 2019 01:35:50 GMT
            // dictionary.Dictionary と書いてから nToKvpString を見て、dictionary だけで動いていることに疑問を持った
            // IEnumerable を継承し、必要な何かを実装しているクラスでないと foreach に入らない認識で何年も C# を書いてきた
            // 実際には、継承は不要のようで、その「何か」とは GetEnumerator のようである
            // nDictionary は、ラッパーであり、何も考えずに GetEnumerator もラップしたため動いていた
            // https://stackoverflow.com/questions/3679805/how-does-foreach-call-getenumerator-via-ienumerable-reference-or-via
            foreach (var xPair in dictionary)
            {
                list.Add (xPair.Key);
                list.Add (xPair.Value);
            }
        }

        public static List <string> nToStringList (this nDictionary dictionary)
        {
            List <string> xList = new List <string> ();
            dictionary.nToStringList (xList);
            return xList;
        }

        // Sun, 19 May 2019 06:47:52 GMT
        // 最初、nEnumerableToDictionary としたが、わざわざ左半分を入れなくても分かるので省略した
        // 一部の拡張メソッドにそういうものを入れるのは、型でなくフォーマットであり、区別に不可欠だから
        public static void nToDictionary (this IEnumerable <string> keyValuePairs, nDictionary dictionary, bool overwrites = true)
        {
            // Sun, 19 May 2019 01:43:50 GMT
            // IEnumerable は foreach で読むのが一番速そうなので、xKey をフラグにしての読み込みを行う
            // まず配列にしてしまうことも考えたが、そのコピーの処理で foreach が一度行われそう
            // ?? で throw できるのを知らず、IDE が教えてくれた
            // パッと見、よく分からないコードだが、自分が見慣れていないだけと思う
            // 都合良く nDictionary が null をキーとして認めないため、その点を利用している
            // keyValuePairs の要素数が奇数なら最後のキーが無視されるのは、仕様とする
            // 数を調べて例外を投げるとか、便宜的に値を null にするとかは、そこまですることもない

            string xKey = null;

            foreach (string xKeyOrValue in keyValuePairs)
            {
                if (xKey == null)
                    xKey = xKeyOrValue ?? throw new nBadOperationException ();

                else
                {
                    if (overwrites)
                        dictionary.SetValue (xKey, xKeyOrValue);
                    else dictionary.AddValue (xKey, xKeyOrValue);

                    xKey = null;
                }
            }
        }

        public static nDictionary nToDictionary (this IEnumerable <string> keyValuePairs, bool ignoresCase = false, bool overwrites = true)
        {
            nDictionary xDictionary = new nDictionary (ignoresCase);
            keyValuePairs.nToDictionary (xDictionary, overwrites);
            return xDictionary;
        }

        // Mon, 20 May 2019 11:48:40 GMT
        // nDictionary と nNameValueCollection の相互変換を可能にした
        // それぞれに *Helper クラスがあるため、往復のうち「行き」に相当するものだけ、それぞれに入れた
        // いずれのクラスでも ignoresCase は null が初期値であり、その場合、コピー元の Comparer が使われる

        public static void nToNameValueCollection (this nDictionary dictionary, nNameValueCollection collection, bool overwrites = true)
        {
            // Mon, 20 May 2019 11:51:42 GMT
            // この向きのコピーでは、気にするべきことが少ない
            // nNameValueCollection にはキーが既存だと落ちる Add* がないため、
            // 既存かどうか調べて呼び出し側で例外を投げるということだけ必要

            foreach (var xPair in dictionary)
            {
                if (overwrites)
                    collection.SetValue (xPair.Key, xPair.Value);

                else
                {
                    if (collection.ContainsKeyFast (xPair.Key))
                        throw new nBadOperationException ();

                    collection.SetValue (xPair.Key, xPair.Value);
                }
            }
        }

        public static nNameValueCollection nToNameValueCollection (this nDictionary dictionary, bool? ignoresCase = null, bool overwrites = true)
        {
            nNameValueCollection xCollection;

            if (ignoresCase != null)
                xCollection = new nNameValueCollection (ignoresCase.Value);
            else xCollection = new nNameValueCollection ((IEqualityComparer) dictionary.Comparer);

            dictionary.nToNameValueCollection (xCollection, overwrites);
            return xCollection;
        }
    }
}

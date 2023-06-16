using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Specialized;
using System.IO;

namespace Nekote
{
    // Mon, 20 May 2019 09:28:12 GMT
    // nNameValueCollection を他と相互変換する拡張メソッドについては、これまで必要性を認めていなかった
    // しかし、ConfigurationManager.AppSettings や Request.QueryString の内容のチェックに便利だし、
    // 相互変換の実装やラウンドトリップのテストによって nNameValueCollection の質も高まると考え、敢行した
    // 予想通り、nNameValueCollection には見直すべきところが多々あり、それなりに使い物になるようになったと思う

    // Mon, 20 May 2019 09:44:27 GMT
    // 基本、nDictionary の方を実装してから、ついでにこちらも実装するだけである
    // そのため、内容的に重複するコメントを nDictionary の方にまとめ、こちらでは省略

    public static class nNameValueCollectionHelper
    {
        public static void nToKvpString (this nNameValueCollection collection, StringBuilder builder)
        {
            // Mon, 20 May 2019 09:37:14 GMT
            // Count, GetKey, GetValues を使うので、近道を用意
            // 初めから NameValueCollection を受け取ることも考えたが、
            // このクラスにおいて Nekote が責任を負うのは、Nekote の仕様によるクラスの変換のみである
            // たまたまここでは NameValueCollection のメンバーだけで足りるからと言って、今後、他のところでもそうとは限らない
            // 足りるところでは .NET のクラスを使うか、多重定義を用意し、足りないところでは……といった仕様の不整合を避ける
            NameValueCollection xCollection = collection.Collection;

            for (int temp = 0; temp < xCollection.Count; temp ++)
            {
                // Mon, 20 May 2019 10:39:34 GMT
                // nDictionary と異なり、nNameValueCollection ではキーが null となり得る
                // それを KVP にしても、残念ながらキーのラウンドトリップは成立しない
                builder.Append (xCollection.GetKey (temp));
                builder.Append (':');

                // Mon, 20 May 2019 09:41:58 GMT
                // NameValueCollection では値側の ArrayList に0個以上の値を入れられるが、
                // Nekote が用意する Get* / Set* が扱うのは先頭だけであり、それで困ることもない
                // そのため、KVP などとの相互変換においては、値側の要素が一つでないときに例外を投げる
                // 一つでないものを仕様として認めないのでなく、認めつつ、「変換には適さない」という扱いである

                string [] xValues = xCollection.GetValues (temp);

                if (xValues == null || xValues.Length != 1)
                    throw new nBadOperationException ();

                builder.AppendLine (xValues [0].nEscapeC ());
            }
        }

        public static string nToKvpString (this nNameValueCollection collection)
        {
            StringBuilder xBuilder = new StringBuilder ();
            collection.nToKvpString (xBuilder);
            return xBuilder.ToString ();
        }

        public static void nKvpToNameValueCollection (this string text, nNameValueCollection collection, bool overwrites = true)
        {
            if (string.IsNullOrEmpty (text))
                return;

            using (StringReader xReader = new StringReader (text))
            {
                string xLine;

                while ((xLine = xReader.ReadLine ()) != null)
                {
                    if (xLine.Length > 0)
                    {
                        if (xLine.nStartsWith ("//"))
                        {
                        }

                        else
                        {
                            int xIndex = xLine.IndexOf (':');

                            if (xIndex >= 0)
                            {
                                string xKey = xLine.Substring (0, xIndex);
                                int xValueLength = xLine.Length - (xIndex + 1);
                                string xValue = xValueLength > 0 ? xLine.Substring (xIndex + 1, xValueLength).nUnescapeC () : null;

                                if (overwrites)
                                    collection.SetValue (xKey, xValue);

                                else
                                {
                                    // Mon, 20 May 2019 10:24:58 GMT
                                    // nNameValueCollection では Set* も Add* もキーが既存のときに例外を投げない
                                    // 通常、Set* は投げず、Add* は追加なのでキーが既存なら投げるが、
                                    // nNameValueCollection における Add* は、キーというより「値の」追加なので、例外を投げない
                                    // そのため、両方に Set* を使い、上書きをしないモードのときにキーが既存なら明示的に投げる

                                    if (collection.ContainsKeyFast (xKey))
                                        throw new nBadOperationException ();

                                    collection.SetValue (xKey, xValue);
                                }
                            }

                            else throw new nInvalidFormatException ();
                        }
                    }
                }
            }
        }

        // Mon, 20 May 2019 10:36:31 GMT
        // NameValueCollection のデフォルトの仕様にならい、大文字・小文字を区別しない方がデフォルト
        public static nNameValueCollection nKvpToNameValueCollection (this string text, bool ignoresCase = true, bool overwrites = true)
        {
            nNameValueCollection xCollection = new nNameValueCollection (ignoresCase);

            if (string.IsNullOrEmpty (text) == false)
                text.nKvpToNameValueCollection (xCollection, overwrites);

            return xCollection;
        }

        // Mon, 20 May 2019 10:37:36 GMT
        // 各所で nDictionary をエントリーに使うようになり、その内容を見たくて実装したが、
        // nNameValueCollection の方でも、ConfigurationManager.AppSettings や Request.QueryString に使える

        public static void nToFriendlyString (this nNameValueCollection collection, StringBuilder builder, bool keepsNullAndEmpty = true, string [] dateTimeKeys = null, string [] timeSpanKeys = null)
        {
            NameValueCollection xCollection = collection.Collection;

            int xCount = 0;

            for (int temp = 0; temp < xCollection.Count; temp ++)
            {
                // Mon, 20 May 2019 22:02:49 GMT
                // 最初の実装では、xValues が null または長さが1以外のときに nToDictionary などと同じく例外を投げていた
                // しかし、それは、nDictionary とのラウンドトリップを成り立たせるためのことで、このメソッドでは過剰なことだった
                // また、クエリー文字列をこのメソッドに通したところ、上記の条件に該当し、例外が飛び、文字列の生成に失敗した
                // このメソッドは、fire-and-forget 的なものなので、少々のことでは落ちるべきでない
                // そこで、上記の条件に該当するときには、xValues を長さ1にして、null だけ入れるようにした
                // そのままループに入り、値の数だけ同じキーを出力する
                // そうすることで、?hoge&moge=&=poge&=&= というクエリーが、nAutoLock.Log で出力すれば、
                // [(null)]\r\nhoge\r\n\r\n[moge]\r\n(empty)\r\n\r\n[(empty)]\r\npoge\r\n\r\n[(empty)]\r\n(empty)\r\n\r\n[(empty)]\r\n(empty)\r\n
                // という文字列として Message キーに設定される
                // hoge だけ書いたら、hoge[=""] のようにみなされると思っていたが、意外なことに [null=]hoge だった
                // = の左右がないのが "" になるのは想定内で、だから、なおさら、[null=]hoge が不思議だが、そういうものだと受け入れる
                // = だけなら [""]=[""] となるようで、それを複数回書けば、AddValue ("", "") を繰り返し呼ぶのと同じ結果のようだ
                // Add* なので、"" キーの値側の ArrayList に "" が追加されていき、=poge のところの分と合わせて、値側は "poge", "", "" となる
                // 値側が null または長さが0のときに { null } にするのは、そうでないと、Add (..., null) のときの、長さ0の ArrayList が無視され、キーが出力されないため
                // 基本的には、キーがあるなら値側は長さが1以上であるべきで、0や null にするくらいならキーそのものをなくすべきと思うが、
                // NameValueCollection は、値側が null や長さ0であることも想定したコーディングになっていて、そのうち長さ0については Add で容易に実現できる
                // 値側を null にする方法は今のところ確認できていないが、いずれにせよ、.NET 側がこれら二つに明示的に対応しているため、せめてキーだけでも出力しておく

                string [] xValues = xCollection.GetValues (temp);

                if (xValues == null || xValues.Length == 0)
                    xValues = new string [] { null };

                foreach (string xValue in xValues)
                {
                    string xNormalized = xValue.nNormalizeAuto ();

                    if (keepsNullAndEmpty ||
                        string.IsNullOrEmpty (xNormalized) == false)
                    {
                        if (xCount > 0)
                            builder.AppendLine ();

                        string xKey = xCollection.GetKey (temp);

                        builder.Append ('[');
                        builder.Append (xKey.nToFriendlyString ());
                        builder.Append ("]\r\n");

                        if (string.IsNullOrEmpty (xNormalized))
                            builder.AppendLine (xNormalized.nToFriendlyString ());

                        else
                        {
                            if (dateTimeKeys != null && dateTimeKeys.Contains (xKey, StringComparer.InvariantCultureIgnoreCase))
                                builder.AppendLine (xNormalized.nLongToDateTime ().nToString (nDateTimeFormat.FriendlyDateTime));
                            else if (timeSpanKeys != null && timeSpanKeys.Contains (xKey, StringComparer.InvariantCultureIgnoreCase))
                                builder.AppendLine (xNormalized.nLongToTimeSpan ().nToInvariantString ());
                            else builder.AppendLine (xNormalized);
                        }

                        xCount ++;
                    }
                }
            }
        }

        public static string nToFriendlyString (this nNameValueCollection collection, bool keepsNullAndEmpty = true, string [] dateTimeKeys = null, string [] timeSpanKeys = null)
        {
            StringBuilder xBuilder = new StringBuilder ();
            collection.nToFriendlyString (xBuilder, keepsNullAndEmpty, dateTimeKeys, timeSpanKeys);
            return xBuilder.ToString ();
        }

        public static void nToStringList (this nNameValueCollection collection, List <string> list)
        {
            NameValueCollection xCollection = collection.Collection;

            for (int temp = 0; temp < xCollection.Count; temp ++)
            {
                list.Add (xCollection.GetKey (temp));

                string [] xValues = xCollection.GetValues (temp);

                if (xValues == null || xValues.Length != 1)
                    throw new nBadOperationException ();

                list.Add (xValues [0]);
            }
        }

        public static List <string> nToStringList (this nNameValueCollection collection)
        {
            List <string> xList = new List <string> ();
            collection.nToStringList (xList);
            return xList;
        }

        public static void nToNameValueCollection (this IEnumerable <string> keyValuePairs, nNameValueCollection collection, bool overwrites = true)
        {
            // Mon, 20 May 2019 10:45:50 GMT
            // nNameValueCollection には null のキーも入るので、
            // nDictionary の方のように null かどうかをフラグに使えない
            nInitialized <string> xKey = null;

            // Fri, 27 Sep 2019 16:39:53 GMT
            // なぜ偶数と奇数の添え字でアクセスしないのか疑問に思ったが、IEnumerable だからである
            // また、nNameValueCollection の AddValue はキーでなく「値の」追加であり、キーが既存でも落ちないため、
            // nKvpToNameValueCollection と同様、キーが既存なら例外を投げる

            foreach (string xKeyOrValue in keyValuePairs)
            {
                if (xKey == null)
                    xKey = new nInitialized <string> (xKeyOrValue) ?? throw new nBadOperationException ();

                else
                {
                    if (overwrites)
                        collection.SetValue (xKey.Value, xKeyOrValue);

                    else
                    {
                        if (collection.ContainsKeyFast (xKey.Value))
                            throw new nBadOperationException ();

                        collection.SetValue (xKey.Value, xKeyOrValue);
                    }

                    xKey = null;
                }
            }
        }

        // Mon, 20 May 2019 10:47:19 GMT
        // 大文字・小文字を区別しない方をデフォルトとしている
        public static nNameValueCollection nToNameValueCollection (this IEnumerable <string> keyValuePairs, bool ignoresCase = true, bool overwrites = true)
        {
            nNameValueCollection xCollection = new nNameValueCollection (ignoresCase);
            keyValuePairs.nToNameValueCollection (xCollection, overwrites);
            return xCollection;
        }

        // Mon, 20 May 2019 11:48:40 GMT
        // nDictionary と nNameValueCollection の相互変換を可能にした
        // それぞれに *Helper クラスがあるため、往復のうち「行き」に相当するものだけ、それぞれに入れた
        // いずれのクラスでも ignoresCase は null が初期値であり、その場合、コピー元の Comparer が使われる

        public static void nToDictionary (this nNameValueCollection collection, nDictionary dictionary, bool overwrites = true)
        {
            NameValueCollection xCollection = collection.Collection;

            // Mon, 20 May 2019 11:54:13 GMT
            // この向きのコピーでは、添え字でキーと string [] の値側にシーケンシャルにアクセスし、
            // キーが null のとき、値側が null または空のときに、「変換には適さない」という意味合いで例外を投げ、
            // overwrites が false なら、キーが既存のときに落ちる AddValue による設定を行う

            for (int temp = 0; temp < xCollection.Count; temp ++)
            {
                string xKey = xCollection.GetKey (temp);

                if (xKey == null)
                    throw new nBadOperationException ();

                string [] xValues = xCollection.GetValues (temp);

                if (xValues == null || xValues.Length != 1)
                    throw new nBadOperationException ();

                if (overwrites)
                    dictionary.SetValue (xKey, xValues [0]);
                else dictionary.AddValue (xKey, xValues [0]);
            }
        }

        public static nDictionary nToDictionary (this nNameValueCollection collection, bool? ignoresCase = null, bool overwrites = true)
        {
            nDictionary xDictionary;

            if (ignoresCase != null)
                xDictionary = new nDictionary (ignoresCase.Value);
            else xDictionary = new nDictionary ((IEqualityComparer <string>) collection.Comparer);

            collection.nToDictionary (xDictionary, overwrites);
            return xDictionary;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Collections.Specialized;

namespace Nekote
{
    // NameValueCollection は実装が古く、無駄だらけであり、使い勝手も悪い
    // しかし、.NET の中核的なところで今でも使われているため、少しでも使いやすくラップしておく
    // 値は配列で、キーと組み合わされて Hashtable に収められ、値の取得にはそちらが使われるようである
    // 加えて、キーと値は、インデックスによるシーケンシャルアクセスのために ArrayList にも収められる
    // https://referencesource.microsoft.com/#System/compmod/system/collections/specialized/namevaluecollection.cs
    // https://referencesource.microsoft.com/#System/compmod/system/collections/specialized/nameobjectcollectionbase.cs

    // Sun, 19 May 2019 06:00:49 GMT
    // 全体的に実装を見直すにおいて、ソースを精読した結果を memo.txt に書いた
    // 元がグチャグチャなクラスなので限界があるが、可能な限り Dictionary に近付ける
    // ただ、null をキーとして使えてしまうのは、禁止すると、アクセスできないデータが生じうる
    // クエリー文字列でもそういうのがあった気がするし、Nekote だけが見られないのは問題

    // Sun, 19 May 2019 00:21:59 GMT
    // Dictionary では null をキーにできないが、NameValueCollection では可能
    // できない方が良い仕様と思うが、後者で、できないとみなしてのコードを書くべきでない
    // たまに、どちらがどうだったか迷うため、このコメントを両方のクラスに貼っておく

    public class nNameValueCollection
    {
        public NameValueCollection Collection { get; private set; }

        // コンストラクターに IEqualityComparer を指定できるが、
        // インスタンスの生成時に何を指定したかをあとで調べる方法はなさそうである
        // そのため、Dictionary.Comparer のようなものは用意できない

        // Sun, 19 May 2019 02:46:48 GMT
        // 無理やり用意することに決めた
        // どうせ元がグチャグチャな仕様のクラス
        // Dictionary と異なり、こちらのものはジェネリックでない

        // Sun, 19 May 2019 07:30:39 GMT
        // 設定するのが StringComparer だと分かっているため、プロパティーもそうしてもよいが、
        // 基底の NameObjectCollectionBase の Comparer に合わせて IEqualityComparer にしている

        public IEqualityComparer Comparer { get; private set; }

        // NameValueCollection には AllKeys というものもあるが、これは ArrayList 内のキーを配列にコピーするため非効率的
        // Keys は、KeysCollection というラッパークラスによって ArrayList のデータにアクセスするもので、無駄はなさそう

        // Sun, 19 May 2019 03:51:11 GMT
        // string [] の AllKeys の方では LINQ の拡張メソッドを使えるのでそちらに切り替えたいが、
        // ソースを見ても、キャッシュである _allKeys が有効なのは InvalidateCachedArrays が呼ばれるまでで、
        // 「キーがなければ追加」のようなことをループで繰り返すと全く無意味なので、Keys のままとする

        public NameObjectCollectionBase.KeysCollection Keys
        {
            get
            {
                return Collection.Keys;
            }
        }

        public int Count
        {
            get
            {
                // ArrayList の長さの取得なので高速
                return Collection.Count;
            }
        }

        #region キーの存在の高速なチェックのため
        // Mon, 20 May 2019 05:34:43 GMT
        // ContainsKey を実装してみたが、ループで全てを見るので、やはり非常に遅かった
        // NameValueCollection には Hashtable が使われているのでそちらのキーを見たいが、それは明示的に禁じられている
        // そのため、さらにキーだけ別の HashSet に入れるという、デザインとしては最悪のコーディングを敢行
        // そうでもしないと、「キーの存在をチェックしながら追加」のような処理が加速度的に遅くなる
        // NameValueCollection には、Get と GetValues も用意されているが、いずれも値側について余計なことをする
        // mFastKeys に頼るコーディングでは、キーの追加・削除のために nNameValueCollection のメソッドだけを使うことになるが、
        // そもそも NameValueCollection のメソッドは、使いにくかったり、仕様が微妙だったりなので、大きな問題ではない

        // Mon, 20 May 2019 12:54:42 GMT
        // 早期に調べていたがコメントを書き忘れていたこととして、HashSet には null のキーを使える
        // nDictionary では使えないのが不思議だが、一対一の関係を見るにおいて分かりにくいからだろうか
        // この件についてはいろいろな考えがあるが、効率を言われてもデザインを言われても、不整合の理由として納得できない
        // NHibernate には NullableDictionary というベタな実装のクラスがあり、やはりニーズはあるのだと思う
        // https://stackoverflow.com/questions/2174692/why-cant-you-use-null-as-a-key-for-a-dictionarybool-string
        // https://stackoverflow.com/questions/4632945/dictionary-with-null-key
        // https://github.com/nhibernate/nhibernate-core/blob/master/src/NHibernate/Util/NullableDictionary.cs

        private HashSet <string> mFastKeys = null;

        // Mon, 20 May 2019 05:39:54 GMT
        // Comparer は、まだ実装に確信がないためコンストラクターで積極的に初期化し、デバッグしていくが、
        // こちらは、動くのが分かっているため、ContainsKeyFast が最初に呼ばれるまで初期化されない
        // AllKeys は非効率的だが、一度きりの使用なので問題でなく、Comparer は、型が異なるが、キャストでいける

        private void iInitializeFastKeys () =>
            mFastKeys = new HashSet <string> (Collection.AllKeys, (IEqualityComparer <string>) Comparer);

        // Mon, 20 May 2019 05:42:09 GMT
        // インスタンスがあるときに限って処理するメソッドをいくつか
        // いずれも、キーが既存だったり、既になかったりしても例外が飛ばない
        // Clear の代わりに null を設定することも可能だが、実装として正しいのは Clear
        // Collection が Clear されているのだから、キーの方もそれと整合させるべき

        private void iSetFastKey (string key)
        {
            if (mFastKeys != null)
                // Fri, 27 Sep 2019 14:34:47 GMT
                // 追加されたら true、既存なら false が返るため、重複チェックが不要
                // 既存ならエラーになる add* が多い一方、ここでは実質的に set* の動作
                // だからメソッド名にも set を含めたのだろう
                mFastKeys.Add (key);
        }

        private void iRemoveFastKey (string key)
        {
            if (mFastKeys != null)
                mFastKeys.Remove (key);
        }

        private void iClearFastKeys ()
        {
            if (mFastKeys != null)
                mFastKeys.Clear ();
        }
        #endregion

        // 値の方が配列であり、Dictionary.Values のようなものは、本質的に意味を成さない
        // それぞれの値の配列の先頭の文字列のみ集めることも考えたが、使いもしないものをイレギュラーな方法で実装するだけとなる

        // 値の方が配列になっているコレクションなので、this では生データにできるだけ近い配列を返すべき
        // NameValueCollection の実装では配列の内容をカンマでつなげたものが得られるが、何を考えていたのか
        // それでは、たとえば配列の内容の方にもカンマが含まれているだけでラウンドトリップが成立しなくなる

        /// <summary>
        /// 元の実装がひどいため、get は、毎回 GetAsStringArray で CopyTo を行うし、set は、仕方なくループで Add を呼ぶ。
        /// </summary>
        public string [] this [string key]
        {
            get
            {
                return GetValues (key);
            }

            set
            {
                SetValues (key, value);
            }
        }

        // this [index] は実装しないでおく
        // 元々が、配列の内容をカンマでつなげたものを返すダメな仕様であり、
        // キーによって（順不同の）値にアクセスするというコンセプトとも一貫しないため

        // Dictionary では StringComparer.InvariantCulture に相当する動作がデフォルトだが、
        // こちらでは StringComparer.InvariantCultureIgnoreCase なので、ignoresCase のデフォルト値もそれに合わせている

        // NameObjectCollectionBase 内に defaultComparer = StringComparer.InvariantCultureIgnoreCase とある
        // MSDN には、それがデフォルトであるかのようなニュアンスで、in .NET Framework version 1.1 and later,
        // this class uses CultureInfo.InvariantCulture when comparing strings と書かれている
        // これは defaultComparer に値を設定するコードと矛盾し、結局どちらか試したところ、大文字・小文字の区別はされなかった
        // https://msdn.microsoft.com/en-us/library/system.collections.specialized.namevaluecollection.aspx

        // Mon, 20 May 2019 11:44:12 GMT
        // IEqualityComparer を受け取れるコンストラクターを追加し、コードを整えた
        // this を使うなら ; で定義も終わってほしいが、それはできないようである

        public nNameValueCollection (bool ignoresCase = true): this (ignoresCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture)
        {
        }

        public nNameValueCollection (IEqualityComparer comparer)
        {
            Comparer = comparer;
            // Sun, 19 May 2019 06:07:41 GMT
            // Comparer を確定してからの初期化なので、こちらは処理が正確
            Collection = new NameValueCollection (Comparer);
        }

        // Sun, 19 May 2019 06:08:14 GMT
        // もらった NameValueCollection のインスタンスのみコピーしたあと、
        // そこにダミーの値を入れてみて Comparer を推測する、あまり良くない実装

        // Sun, 19 May 2019 09:04:18 GMT
        // 読み取り専用の NameValueCollection があるようで、ConfigurationManager.AppSettings がそうだった
        // 判別のためのプロパティーなどがないようで、try / catch を推奨する人が多い
        // どんどんムチャクチャなメソッドになっているが、やれるだけのことをやる実装にした
        // https://www.reddit.com/r/dotnet/comments/1szhbp/how_do_you_check_if_a_namevaluecollection_is/

        // Sun, 19 May 2019 07:40:10 GMT
        // 以下、テストを行った部分には簡易的に OK をつけておく

        private IEqualityComparer iGetComparerDirty ()
        {
            // Sun, 19 May 2019 07:25:27 GMT
            // ContainsKey をまだ使えないので、nImage.ToArgbColor と同様のフォーマットのランダムなキーを、
            // まずは、大文字・小文字を区別せずにベタ書きで探し、そのキーの追加・削除がデータに影響しないのを確認
            // 大丈夫なキーが見付かれば、"" を設定し、大文字にしたキーで読もうとしてみる
            // 値が null でないと分かっているので、読めたら大文字・小文字が区別されないことになる

            // Mon, 20 May 2019 05:58:51 GMT
            // 補足だが、値に関わりなく、GetValues によって読んだものが null でないならキーは存在する
            // 問題なのは、「null が返ったからといって、キーがないとは限らない」という逆の方で、
            // 「null 以外が返ったからキーが存在する」というのは、常に成立する

            while (true)
            {
                string xRandomKey = "Key/" + nRandom.Next ().nToString ();

                for (int temp = 0; temp < Collection.Count; temp ++)
                {
                    if (nIgnoreCase.Compare (Collection.GetKey (temp), xRandomKey) == 0)
                    {
                        xRandomKey = null;
                        break;
                    }
                }

                if (xRandomKey != null)
                {
                    try
                    {
                        // Sun, 19 May 2019 07:34:38 GMT
                        // Set に対応するものが GetValues だというのは、元々の仕様の不整合である
                        // SetValues がなく、Get は GetAsOneString を呼ぶため、Set と GetValues を使うのが最善
                        // Remove は、ループを後ろから回し、「一致したら消す」というのを回数制限なく行うようなので安全

                        // Sun, 19 May 2019 10:13:09 GMT
                        // 詳しくは後述したが、ConfigurationManager.AppSettings では、Set には成功してしまう
                        // そこで、消すものがない Remove を最初に呼び、ConfigurationErrorsException を飛ばしてみる
                        // 都合良く Remove 内の BaseRemove でも最初に読み取り専用かどうかを見てくれるので、
                        // NotSupportedException が飛ぶ NameValueCollection でも、この実装でうまくいく
                        // それなら Set は不要かもしれないが、一応、残しておく
                        // 何もしない Remove がダメなのに、値を書き込む Set がいけるダメな実装のインスタンスが確認されている
                        // 逆に、Set がいけるのに Remove がダメなインスタンスもどこかにある可能性はゼロでない

                        Collection.Remove (xRandomKey);
                        Collection.Set (xRandomKey, string.Empty);
                        bool xIgnoresCase = Collection.GetValues (xRandomKey.nToUpper ()) != null;
                        Collection.Remove (xRandomKey);

                        if (xIgnoresCase)
                            return StringComparer.InvariantCultureIgnoreCase; // OK
                        else return StringComparer.InvariantCulture; // OK
                    }

                    catch
                    {
                        // Sun, 19 May 2019 09:06:17 GMT
                        // コレクションが読み取り専用なら、Set で NotSupportedException が飛び、ここに至る
                        // 要素が一つもないなら、書き込めないのだから他にできることは何もなく、デフォルトのようである InvariantCultureIgnoreCase を返す
                        // 要素があるなら、大文字・小文字を区別しない List にまとめ直し、要素数が減るかそのままか調べる
                        // 減るなら、大文字・小文字のみ異なる二つ以上のキーがあるということで、大文字・小文字が区別されているということ
                        // 減らないなら、1) 大文字・小文字が区別されないコレクション、2) 大文字・小文字以上の違いが全てのキーにある、のいずれか
                        // 大文字にしたときに変化するキーがあるなら、大文字のキーでも値を取得できるか調べることで、1なのかどうか分かる
                        // 繰り返しになるが、List にまとめたときに、大文字・小文字の違いしかないキーがないことが確認できているため
                        // 大文字にすることで、そのキーでは値を取得できなくなるなら、その時点で大文字・小文字の区別が確定
                        // そういう都合の良いキー（大文字にすることで変化するもの）がないなら、同じくデフォルトのようである InvariantCultureIgnoreCase を返す
                        // 無理やりな実装だが、ConfigurationManager.AppSettings も Request.QueryString も扱えるので、何とかなるだろう

                        // Sun, 19 May 2019 09:58:17 GMT
                        // ConfigurationManager.AppSettings では、Set には成功していて、Remove で ConfigurationErrorsException が飛んでいると判明
                        // Request.QueryString では、普通（？）に Set で NotSupportedException が飛ぶのを確認した
                        // 後者のチェックにおいて、NotSupportedException だけ捕捉する catch の中で nAutoLock.Log を呼んだところ、
                        // どこかで ConfigurationManager.AppSettings へのアクセスがあり、ConfigurationErrorsException を拾えずに落ちた
                        // ConfigurationErrorsException がどこから飛んでいるかは、コードをザッと眺めただけでは分からなかった
                        // 基底の NameObjectCollectionBase が複雑だし、ConfigurationManager とも密結合なので、調べる気にならない
                        // とりあえず、読み取り専用だということに起因すること以外で例外が飛ぶ部分ではなさそうなので、全てを捕捉する catch に変更した

                        if (Collection.Count == 0)
                            return StringComparer.InvariantCultureIgnoreCase; // OK

                        List <string> xKeys = new List <string> ();

                        for (int temp = 0; temp < Collection.Count; temp ++)
                        {
                            string xKey = Collection.GetKey (temp);

                            if (xKeys.Contains (xKey, StringComparer.InvariantCultureIgnoreCase) == false)
                                xKeys.Add (xKey);
                        }

                        if (xKeys.Count < Collection.Count)
                            return StringComparer.InvariantCulture; // OK

                        for (int temp = 0; temp < Collection.Count; temp ++)
                        {
                            string xKey = Collection.GetKey (temp),
                                xCapitalized = xKey.nToUpper ();

                            if (xCapitalized != xKey)
                            {
                                if (Collection.GetValues (xCapitalized) != null)
                                    return StringComparer.InvariantCultureIgnoreCase; // OK
                                else return StringComparer.InvariantCulture; // OK
                            }
                        }

                        return StringComparer.InvariantCultureIgnoreCase; // OK
                    }
                }
            }
        }

        public nNameValueCollection (NameValueCollection collection)
        {
            // 元々の実装は全ての値をコピーするが、こちらはラッパークラスなのでそうはしない
            // https://msdn.microsoft.com/en-us/library/2s711zk3.aspx
            Collection = collection;

            // Sun, 19 May 2019 06:09:56 GMT
            // もらった collection に基づく推測なので、こちらは100％でない

            // Sun, 19 May 2019 10:29:17 GMT
            // 強引な実装になったので、Comparer が参照されてからの初期化に切り替えることも考えたが、
            // 強引な実装だからこそ、コンストラクターで毎回呼び、デバッグに徹した方が、コードの質が高まる
            // 重たいメソッドではなく、今日び、このコストを気にしなければならないパソコンはない

            Comparer = iGetComparerDirty ();
        }

        // Add* は、キーがなければ作り、あれば、値の既存の配列の末尾に value または values を追加
        // Set* は、キーがなければ作って値を設定し、あれば既存の配列を捨てて新たな配列のインスタンスを設定

        // Sun, 19 May 2019 06:11:24 GMT
        // 以下、冗長だが、summary としても同様のコメントを書いておく

        // Fri, 27 Sep 2019 14:31:36 GMT
        // 久々に見ていて、nDictionary なら重複があるときに落ちるのにこちらでは落ちないことに違和感を覚えた
        // しかし、こちらでは「値」の方が複数であり、そこに足していけるのが元々のクラスの仕様なので、この違いは仕方ない

        /// <summary>
        /// キーが既存でも落ちず、その場合、値側の ArrayList の末尾に追加される。値は、null だと string.Empty とみなされる。
        /// </summary>
        public void AddValue (string key, string value) // OK
        {
            // Add は value が null なら key のみ追加し、Set は null である value も設定するという謎の仕様になっている
            // 前者においては、Keys にもキーが追加されるのに GetValues を呼ぶと null が返るわけで、わけが分からない
            // 放置するのが気になるイレギュラーだが、SetValue の方を使う限り大丈夫なようなので、まあ仕方ないか

            // Sun, 19 May 2019 06:20:04 GMT
            // 値が null でも、キーが追加されるなら、null が追加されたのだろうと推測は可能で、何とかやり過ごせる
            // しかし、AddValues の実装において、null 詰め（？）されている各値の位置が変わるのは、場合によっては致命的
            // では、AddValues の方だけ ?? string.Empty にするべきかと言うと、それでは二つの Add* の動作が違ってバグになる
            // そのため、どちらにおいても null が "" になるようにした
            // null を入れることは稀だし、読むときには string.IsNullOrEmpty に通すことが多いため、これが最善

            // Mon, 20 May 2019 06:25:20 GMT
            // 古いコメントを鵜呑みにして確認していなかったが、Add に null を与えたら、値側は、空の ArrayList になる
            // 「key のみ追加」から、値側が (ArrayList) null になるイメージだったが、一応、インスタンスは作られるようである
            // どうせ読むのは GetAsStringArray であり、このメソッドが違いをなくしてくれるが、違いを念頭には置いておく

            Collection.Add (key, value ?? string.Empty);
            iSetFastKey (key);
        }

        /// <summary>
        /// キーが既存でも落ちず、その場合、値側の ArrayList の末尾に追加される。各値は、null だと string.Empty とみなされる。
        /// </summary>
        public void AddValues (string key, string [] values) // OK
        {
            foreach (string xValue in values)
                Collection.Add (key, xValue ?? string.Empty);

            // Mon, 20 May 2019 06:03:50 GMT
            // values が空なら何も起こらないメソッド

            if (values.Length > 0)
                iSetFastKey (key);
        }

        /// <summary>
        /// キーが既存なら上書きされる。Add* と異なり、値が null でもそのまま出力される。
        /// </summary>
        public void SetValue (string key, string value) // OK
        {
            // こちらは value が null でも問題なし

            // Sun, 19 May 2019 06:44:44 GMT
            // ここで ?? string.Empty を適用すると、null を設定する方法が一切なくなる
            // 単一の値なら、null でもラウンドトリップが成立しなければならない

            Collection.Set (key, value);
            iSetFastKey (key);
        }

        // Mon, 20 May 2019 06:11:15 GMT
        // このクラスでは、1) キーがない、2) キーがあるが値側が (ArrayList) null、3) キーがあるが値側が空、4) キーがあるが値側の中身が null、あたりを想定する必要がある
        // Add* は、ループで何度も呼びうるものなので、「初回はキーだけ追加し（実際には3になる）、2回目以降は何もしませんよ」では、null 詰め（？）効果がなくて問題
        // そのため、?? string.Empty によって便宜的に "" を出力してでも、呼んだ回数と値側の要素数が一致するようにした
        // SetValue は、null を与えたらそのまま出力し、2や3でなく4になり、要するに、{ null } になる
        // そこから Add で null を入れようとしても、{ null, null, ... } とすることはできないのが、NameValueCollection の問題
        // では、SetValues の仕様はどうするべきか考えたが、foreach の部分だけでは、values が空のときに無駄にキーが消える
        // かといって、values が空でないときだけキーを消す仕様にしては、「上書き」のメソッドにならない
        // File.WriteAllText に null を与えると、「null だから既存のファイルをそのまま残しますよ」とはならない
        // ということを考えるなら、values が空のときの結果は、キーがそのまま残るのでも、1でもなく、2または3が適する
        // そのうち、NameValueCollection のメソッドで実現可能なのは3だけなので、苦肉の策だが Add を呼ぶ
        // これは、空の string [] が空の ArrayList になる点において、結果的には、そう悪くもないか

        /// <summary>
        /// キーが既存なら上書きされる。Add* と同様、値が null のところは string.Empty とみなされる。
        /// </summary>
        public void SetValues (string key, string [] values) // OK
        {
            // string [] を設定できる Set がないため、
            // To append the new value to the existing list of values, use the Add method というのに従い、
            // まずはキーによってエントリーそのものを消してから、Add を繰り返し呼んでいる
            // 削除時にキーが存在しない場合については、If the NameValueCollection does not contain an element with the specified key,
            // the NameValueCollection remains unchanged. No exception is thrown とのこと
            // https://msdn.microsoft.com/en-us/library/system.collections.specialized.namevaluecollection.set.aspx
            // https://msdn.microsoft.com/en-us/library/xsc9a449.aspx
            // https://msdn.microsoft.com/en-us/library/system.collections.specialized.namevaluecollection.remove.aspx

            Collection.Remove (key);

            if (values.Length > 0)
            {
                foreach (string xValue in values)
                    Collection.Add (key, xValue ?? string.Empty);
            }

            else Collection.Add (key, null);

            iSetFastKey (key);
        }

        // Dictionary にならって ContainsKey を実装することは難しい
        // キーへのシーケンシャルアクセスは効率的に行えそうだが、
        // comparer が不明なので、スマートな方法でキーの一致を調べられない
        // GUID のキーで大文字・小文字が区別されるかどうか調べてその結果をキャッシュするなども選択肢だが、
        // そもそも ContainsKey があるとそれを使うのが作法と思ってしまうため、ミスリードを避ける

        // Sun, 19 May 2019 06:30:04 GMT
        // Comparer と同様、ContainsKey も無理やり実装してみる
        // 想定するべきケースが多くなく、高確率で正しい結果を得られるため
        // 後述する理由で、効率的なメソッドではないが、ないよりマシだとは思う

        // Mon, 20 May 2019 06:47:07 GMT
        // 速度に問題があったので ContainsKeyFast を追加し、ContainsKey を改名
        // 古いものを誤って使わないためだが、正確なのはこちらなので、今のところは削除しない
        // また、古いコメントは古いメソッド名のままだが、そのままでいい
        // ContainsKeyFast は、Collection プロパティーの方のメソッドを使えば、
        // キーの整合性が失われて、必ずしも正確な結果を返さないようになる

        public bool ContainsKeyLegacy (string key)
        {
            // Sun, 19 May 2019 10:47:30 GMT
            // Hashtable.FindEntry を呼ぶため、圧倒的に高速
            // ただ、1) キーがないとき、2) あるが、値側の ArrayList が null のとき、3) 2の ArrayList が null でないが、要素が全くないとき、
            // の三つにおいて null を返してくるため、「null でないなら、1～3のいずれにも該当しない」という判断しか、ここでは下せない
            // ContainsKey は、キーがたいてい見付かるところで使うなら高速だが、ほぼ見付からないところで使うなら、要素数に比例して遅くなる

            if (Collection.GetValues (key) != null)
                return true; // OK

            // Sun, 19 May 2019 07:20:20 GMT
            // NameValueCollection を受け取る Add にならい、要素数をコピーしてから GetKey でキーを取得
            // それらを、引数が object なのが痛いが、Comparer.Equals で比較し、目的のキーを探す

            // Mon, 20 May 2019 08:09:16 GMT
            // 要素数をいったん xCount にコピーするのは、コードの整合性を乱すのでやめておく
            // そういうことを気にするならそもそもプロパティーを使えず、public 変数を使いたくなる

            for (int temp = 0; temp < Collection.Count; temp ++)
            {
                // Sun, 19 May 2019 10:57:04 GMT
                // xCollection.Collection.Add ("...", null) のようにしないと、ここには引っ掛からない
                // nNameValueConnection の Add* では ?? string.Empty になるし、まずあり得ないこと

                if (Comparer.Equals (Collection.GetKey (temp), key))
                    return true; // OK
            }

            return false; // OK
        }

        // Mon, 20 May 2019 06:53:11 GMT
        // 重複のない100万のキーを設定し、見付からないと分かっているキーを二つのメソッドで探した
        // 古い方は、引数が object 型の Equals を100万件×100回なので、かなり時間がかかった
        // 新しい方は、HashSet なので、100万件×1000万回でも、古い方100回より速かった
        // SetValue (1000000 times): 2703ms
        // ContainsKeyLegacy (100 times): 13548ms
        // ContainsKeyFast (10000000 times): 3562ms

        public bool ContainsKeyFast (string key)
        {
            // Mon, 20 May 2019 06:57:24 GMT
            // 初期化のメソッドなので、それが必要なときだけ呼ぶ
            // メソッド内に null のチェックを入れると、名前と不整合になる
            // Comparer が HashSet 側に設定されるため、Contains には不要

            if (mFastKeys == null)
                iInitializeFastKeys ();

            return mFastKeys.Contains (key);
        }

        // ContainsValue は、上の方に書いた、Values を用意しないのと同じ理由で用意しない

        /// <summary>
        /// キーが存在しなければ落ちる。
        /// </summary>
        public string GetValue (string key)
        {
            // This method returns null in the following cases:
            // 1) if the specified key is not found; and
            // 2) if the specified key is found and its associated value is null とあり、
            // どちらの場合においても落ちる実装だが、意図的にそうしている
            // https://msdn.microsoft.com/en-us/library/4ba5htte.aspx
            return Collection.GetValues (key) [0];
        }

        /// <summary>
        /// キーが存在しなければ落ちる。
        /// </summary>
        public string [] GetValues (string key)
        {
            // キーが null なら、_nullKeyEntry という個別の変数から値を返してくる
            // キーが存在しなくても落ちない仕様については、敢えてどうこうするほどのことはない
            // ArrayList から配列へのコピーが毎回行われるのがもったいないが、
            // まず使うことのない ArrayList で返ってきても扱いに困るため仕方ない
            return Collection.GetValues (key);
        }

        public bool TryGetValue (string key, out string value)
        {
            string [] xValues = Collection.GetValues (key);

            // キーが存在し、配列が空でなければ、先頭の文字列が null でも返す
            // これは Dictionary と同様の仕様である

            if (xValues != null && xValues.Length > 0)
            {
                value = xValues [0];
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetValues (string key, out string [] values)
        {
            string [] xValues = Collection.GetValues (key);

            if (xValues != null)
            {
                values = xValues;
                return true;
            }

            values = default;
            return false;
        }

        public string GetValueOrDefault (string key, string value)
        {
            string [] xValues = Collection.GetValues (key);

            if (xValues != null && xValues.Length > 0)
                return xValues [0];

            return value;
        }

        public string GetNotNullValueOrDefault (string key, string value)
        {
            string [] xValues = Collection.GetValues (key);

            if (xValues != null && xValues.Length > 0)
            {
                string xValue = xValues [0];

                if (xValue != null)
                    return xValue;
            }

            return value;
        }

        public string GetNotEmptyValueOrDefault (string key, string value)
        {
            string [] xValues = Collection.GetValues (key);

            if (xValues != null && xValues.Length > 0)
            {
                string xValue = xValues [0];

                if (string.IsNullOrEmpty (xValue) == false)
                    return xValue;
            }

            return value;
        }

        public string [] GetValuesOrDefault (string key, string [] values)
        {
            string [] xValues = Collection.GetValues (key);

            if (xValues != null)
                return xValues;

            return values;
        }

        #region それぞれの型に対応する Set* / Get* の糖衣メソッド // OK
        // Mon, 20 May 2019 07:01:32 GMT
        // mFastKeys の更新が必要になったので、
        // 各所の Collection.Set を SetValue に変更した

        public void SetString (string key, string value) =>
            SetValue (key, value);

        // 落ちるべきなので、this の中身である GetValues を呼んで一つ目を取得
        public string GetString (string key) =>
            Collection.GetValues (key) [0];

        // 落ちてはならず、"" を得ても使い物にならないことが多いので、not empty で取得
        public string GetStringOrDefault (string key, string value) =>
            GetNotEmptyValueOrDefault (key, value);

        public string GetStringOrEmpty (string key) =>
            GetStringOrDefault (key, string.Empty);

        public string GetStringOrNull (string key) =>
            GetStringOrDefault (key, null);

        public void SetBool (string key, bool value) =>
            SetValue (key, value.nToString ());

        public bool GetBool (string key) =>
            Collection.GetValues (key) [0].nToBool ();

        // null を得て bool への変換を試みても問題なし
        public bool GetBoolOrDefault (string key, bool value) =>
            GetValueOrDefault (key, null).nToBoolOrDefault (value);

        public void SetByte (string key, byte value) =>
            SetValue (key, value.nToString ());

        public byte GetByte (string key) =>
            Collection.GetValues (key) [0].nToByte ();

        public byte GetByteOrDefault (string key, byte value) =>
            GetValueOrDefault (key, null).nToByteOrDefault (value);

        public void SetChar (string key, char value) =>
            SetValue (key, value.nToUShortString ());

        public char GetChar (string key) =>
            Collection.GetValues (key) [0].nUShortToChar ();

        public char GetCharOrDefault (string key, char value) =>
            GetValueOrDefault (key, null).nUShortToCharOrDefault (value);

        public void SetDateTime (string key, DateTime value) =>
            SetValue (key, value.nToLongString ());

        public DateTime GetDateTime (string key) =>
            Collection.GetValues (key) [0].nLongToDateTime ();

        public DateTime GetDateTimeOrDefault (string key, DateTime value) =>
            GetValueOrDefault (key, null).nLongToDateTimeOrDefault (value);

        public void SetDecimal (string key, decimal value) =>
            SetValue (key, value.nToString ());

        public decimal GetDecimal (string key) =>
            Collection.GetValues (key) [0].nToDecimal ();

        public decimal GetDecimalOrDefault (string key, decimal value) =>
            GetValueOrDefault (key, null).nToDecimalOrDefault (value);

        public void SetDouble (string key, double value) =>
            SetValue (key, value.nToString ());

        public double GetDouble (string key) =>
            Collection.GetValues (key) [0].nToDouble ();

        public double GetDoubleOrDefault (string key, double value) =>
            GetValueOrDefault (key, null).nToDoubleOrDefault (value);

        public void SetEnum (string key, Enum value) =>
            SetValue (key, value.nToString ());

        public object GetEnum (string key, Type type) =>
            Collection.GetValues (key) [0].nToEnum (type);

        public object GetEnumOrDefault (string key, Type type, object value) =>
            GetValueOrDefault (key, null).nToEnumOrDefault (type, value);

        // 通常は、こちらのジェネリックの方を使う

        public T GetEnum <T> (string key) =>
            Collection.GetValues (key) [0].nToEnum <T> ();

        public T GetEnumOrDefault <T> (string key, T value) =>
            GetValueOrDefault (key, null).nToEnumOrDefault (value);

        public void SetFloat (string key, float value) =>
            SetValue (key, value.nToString ());

        public float GetFloat (string key) =>
            Collection.GetValues (key) [0].nToFloat ();

        public float GetFloatOrDefault (string key, float value) =>
            GetValueOrDefault (key, null).nToFloatOrDefault (value);

        public void SetGuid (string key, Guid value) =>
            SetValue (key, value.nToString ());

        public Guid GetGuid (string key) =>
            Collection.GetValues (key) [0].nToGuid ();

        public Guid GetGuidOrDefault (string key, Guid value) =>
            GetValueOrDefault (key, null).nToGuidOrDefault (value);

        public void SetInt (string key, int value) =>
            SetValue (key, value.nToString ());

        public int GetInt (string key) =>
            Collection.GetValues (key) [0].nToInt ();

        public int GetIntOrDefault (string key, int value) =>
            GetValueOrDefault (key, null).nToIntOrDefault (value);

        public void SetLong (string key, long value) =>
            SetValue (key, value.nToString ());

        public long GetLong (string key) =>
            Collection.GetValues (key) [0].nToLong ();

        public long GetLongOrDefault (string key, long value) =>
            GetValueOrDefault (key, null).nToLongOrDefault (value);

        public void SetSByte (string key, sbyte value) =>
            SetValue (key, value.nToString ());

        public sbyte GetSByte (string key) =>
            Collection.GetValues (key) [0].nToSByte ();

        public sbyte GetSByteOrDefault (string key, sbyte value) =>
            GetValueOrDefault (key, null).nToSByteOrDefault (value);

        public void SetShort (string key, short value) =>
            SetValue (key, value.nToString ());

        public short GetShort (string key) =>
            Collection.GetValues (key) [0].nToShort ();

        public short GetShortOrDefault (string key, short value) =>
            GetValueOrDefault (key, null).nToShortOrDefault (value);

        public void SetTimeSpan (string key, TimeSpan value) =>
            SetValue (key, value.nToLongString ());

        public TimeSpan GetTimeSpan (string key) =>
            Collection.GetValues (key) [0].nLongToTimeSpan ();

        public TimeSpan GetTimeSpanOrDefault (string key, TimeSpan value) =>
            GetValueOrDefault (key, null).nLongToTimeSpanOrDefault (value);

        public void SetUInt (string key, uint value) =>
            SetValue (key, value.nToString ());

        public uint GetUInt (string key) =>
            Collection.GetValues (key) [0].nToUInt ();

        public uint GetUIntOrDefault (string key, uint value) =>
            GetValueOrDefault (key, null).nToUIntOrDefault (value);

        public void SetULong (string key, ulong value) =>
            SetValue (key, value.nToString ());

        public ulong GetULong (string key) =>
            Collection.GetValues (key) [0].nToULong ();

        public ulong GetULongOrDefault (string key, ulong value) =>
            GetValueOrDefault (key, null).nToULongOrDefault (value);

        public void SetUShort (string key, ushort value) =>
            SetValue (key, value.nToString ());

        public ushort GetUShort (string key) =>
            Collection.GetValues (key) [0].nToUShort ();

        public ushort GetUShortOrDefault (string key, ushort value) =>
            GetValueOrDefault (key, null).nToUShortOrDefault (value);
        #endregion

        // 使うことがなさそうだが、Dictionary と一応は揃えておく
        // https://msdn.microsoft.com/en-us/library/system.collections.specialized.nameobjectcollectionbase.getenumerator.aspx

        // Tue, 21 May 2019 11:28:34 GMT
        // 私が知らなかっただけで、foreach に不可欠のメソッド
        // ただ、NameValueCollection の場合、foreach で得られるのがキーなので使いにくい

        public IEnumerator GetEnumerator ()
        {
            return Collection.GetEnumerator ();
        }

        public void Remove (string key)
        {
            // キーが存在しなくても何も起こらない
            Collection.Remove (key);
            iRemoveFastKey (key);
        }

        public void Clear ()
        {
            Collection.Clear ();
            iClearFastKeys ();
        }
    }
}

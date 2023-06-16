using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Thu, 02 May 2019 13:29:13 GMT
    // ログ1件のデータを格納し、nDictionary と相互変換するためのクラス
    // クラスを用意した理由などは nSimpleLogDataProvider の方に書いておく

    public class nSimpleLogEntry
    {
        // Thu, 02 May 2019 13:30:40 GMT
        // 必須であり、デフォルトのままでは EntryToDictionary で落ちる
        public nSimpleLogType Type { get; set; }

        // Thu, 02 May 2019 13:31:27 GMT
        // Message と ExceptionString はオプションであり、
        // あれば出力されるが、なくても問題にならない
        // 後者を *String に変換するのは、Exception のままではラウンドトリップができないため
        // 一時的な保持はできるが、書いて読み込んだときには、もう Exception には戻せない

        public string Message { get; set; }

        public string ExceptionString { get; set; }

        // Thu, 02 May 2019 13:31:56 GMT
        // 変数の値などをザッと出力しておくのに役立つプロパティー
        // "Type", "Message", "ExceptionString" をキーとしても落ちないが、
        // そういうことをすると、Type プロパティーなどに設定した値が EntryToDictionary の実行時に上書きされる
        // 落とさないのは、ログ出力などの、場合によっては危機的な状況における処理で落ちるべきでないため
        // データの整合性の問題も気にはなるが、まずはデータをファイルに出力することが先決である

        // Sun, 05 May 2019 05:39:33 GMT
        // ログを吐くだけのこのクラスでは、重複キーによる値の上書きは大きな問題にならない
        // しかし、ユーザー管理においては、たとえば "Type" に文字列として "Administrator" を設定することで権限の昇格が起こる
        // そういったリスクを完全になくす目的において、このクラスでも、キー名に接頭辞をつける仕様に変更する

        // Fri, 03 May 2019 08:06:34 GMT
        // nDateTimeBasedSimpleDataProvider では、ticks が内部的に取得・衝突確認されるため、
        // このクラスにもそういうプロパティーを用意したところで、キーとしての ticks と一致することはない
        // 「衝突確認後の ticks をこのプロパティーに設定しろ」的な指定も可能だが、そこまでする利益が見えない
        // それは、Application, Library, Url, Location といったプロパティーについても同様で、
        // そのうち Location には、ログを書くメソッドに与える静的な GUID を設定することを考えたが、
        // いずれも、なくてもログの利用価値が大きく落ちるわけでなく、仕様や実装が複雑になるリスクの方が目立つ
        // といったことを考えた上で Message と ExceptionString に絞り込んだため、安易に項目を増やさない

        public nDictionary Values { get; private set; } = new nDictionary ();

        // Fri, 03 May 2019 04:21:18 GMT
        // こちらを TypeToDictionary でなく Entry* とするのは、こちらはジェネリックでないため
        // クラス名が *Entry であり、そのものを nDictionary と相互変換するイメージが明確なので、この命名でよい
        // このあたりは、きちんとしたルールを定めることが難しく、臨機応変に最善の仕様を考えることになる

        public static readonly Func <nSimpleLogEntry, nDictionary> EntryToDictionary = (entry) =>
        {
            // Fri, 03 May 2019 04:24:09 GMT
            // プログラムが読み書きするものなので、キーの大文字・小文字を区別してよい
            // デバッグがちゃんと行われていれば、書いたものを読めない可能性は基本的にない

            nDictionary xDictionary = new nDictionary ();
            // Thu, 02 May 2019 13:37:03 GMT
            // デフォルトのままではここで例外が飛ぶのを確認した
            xDictionary.SetEnum ("Type", entry.Type); // 必須

            if (string.IsNullOrEmpty (entry.Message) == false)
                xDictionary.SetString ("Message", entry.Message); // オプション

            if (string.IsNullOrEmpty (entry.ExceptionString) == false)
                xDictionary.SetString ("ExceptionString", entry.ExceptionString); // オプション

            foreach (var xPair in entry.Values)
            {
                // Thu, 02 May 2019 13:37:45 GMT
                // Message などはプロパティーが最初から存在するため、null を設定したのか、全くさわっていないのかを、値を見ることで判別できない
                // 一方、Values の方は、さわらなければキーが存在しないため、キーが存在し、その値が null なら、null を設定したということ
                // Type を除いて全てをオプションと考えているため、前者では値を見るが、後者では、キーがあるなら値を設定したということなので無条件でコピー

                // Sun, 05 May 2019 05:42:47 GMT
                // 上述した理由により、キーに接頭辞をつけている
                // この接頭辞は、nValidator.IsIdentifier を通る

                xDictionary.SetString ("Values_" + xPair.Key, xPair.Value); // オプション
            }

            return xDictionary;
        };

        public static readonly Func <nDictionary, nSimpleLogEntry> DictionaryToEntry = (dictionary) =>
        {
            nSimpleLogEntry xEntry = new nSimpleLogEntry
            {
                // Fri, 03 May 2019 04:31:25 GMT
                // 前半にも書いたが、Type が未設定ならここで落ちる
                // これはデバッグで落とせる問題であり、ランタイムでは起こらないので、この方がいい
                Type = dictionary.GetEnum <nSimpleLogType> ("Type"), // 必須
                Message = dictionary.GetStringOrNull ("Message"), // オプション
                ExceptionString = dictionary.GetStringOrNull ("ExceptionString") // オプション
            };

            foreach (var xPair in dictionary)
            {
                // Sun, 05 May 2019 05:46:31 GMT
                // プログラムが読み書きするものなので、先頭部分の比較においても大文字・小文字が区別される
                // キーが "Values_" で始まるため、たとえば呼び出し側が "Type" としても "Values_Type" が Type プロパティーに影響することはない
                // nDictionary は、空のキーが通った記憶があるが、通ってしまうとややこしいので、通さない前提のコーディングにしている
                // "Values_" で始まらず、プロパティー名とも一致しないキーは、Nekote が書いたものではあり得ないので無視される

                if (xPair.Key.nStartsWith ("Values_"))
                {
                    // Sun, 05 May 2019 05:53:18 GMT
                    // パッとググった程度では情報が見付からなかったが、
                    // コンパイル時に定数に最適化されるはず
                    string xKey = xPair.Key.Substring ("Values_".Length);

                    if (string.IsNullOrEmpty (xKey) == false)
                        xEntry.Values.SetString (xKey, xPair.Value); // オプション
                }
            }

            return xEntry;
        };

        // Fri, 03 May 2019 09:57:06 GMT
        // nSimpleLogDataProvider.cs の方に書いた理由により、そちらの Create をこちらのコンストラクターに変更した
        // そのため、以下のコメントは不整合だが、大筋がそのまま通用するため、特に書き直すことなくそのまま残しておく

        // Fri, 03 May 2019 07:37:33 GMT
        // 下位の Create を拡張し、主要な値を直接受け取れるようにしておく
        // type は必須で、message と exception はオプションなので、後者については常に null も想定する
        // Create から Create を呼ぶところでは、二つ目から四つ目の全てで一つ目を呼び、コードの見通しを良くしている
        // 四つ目が三つ目、三つ目が二つ目……としていくことでギリギリまでコードを削ることがあるが、
        // 加齢で頭の回転が鈍くなってきたこともあってか、少し長くしてでも、分かりやすいコードに逃げたい気持ちが最近は強い
        // type を受け取らないところでは、例外があるなら Error、ないなら Info とみなすという合理的な処理をしている
        // そのことを忘れないように、一応、<summary> によるコメントもつけている

        public nSimpleLogEntry (nSimpleLogType type, string message, Exception exception = null)
        {
            Type = type;
            Message = message;
            // Fri, 03 May 2019 08:01:57 GMT
            // 例外を文字列化すると末尾に不要な改行がつくので、一応ノーマライズ
            // HTML 化するときに余計な <br /> が入りうるなど、些細なデメリットがある
            ExceptionString = exception != null ? exception.ToString ().nNormalizeAuto () : null;
        }

        public nSimpleLogEntry (nSimpleLogType type, Exception exception): this (type, null, exception)
        {
        }

        /// <summary>
        /// 例外の有無により、内部的に Error なのか Info なのかが切り替わる。
        /// </summary>
        public nSimpleLogEntry (string message, Exception exception = null): this (exception != null ? nSimpleLogType.Error : nSimpleLogType.Info, message, exception)
        {
        }

        /// <summary>
        /// 例外しか引数がないため、内部的には Error として扱われる。
        /// </summary>
        public nSimpleLogEntry (Exception exception): this (nSimpleLogType.Error, null, exception)
        {
        }

        public nSimpleLogEntry ()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nSimpleKvsEntry
    {
        // Sat, 04 May 2019 09:12:57 GMT
        // Create と Update の日時を UTC で保持する
        // 後者を Nullable にしないなら、Create と同時に LastUpdateUtc も設定すればいいが、
        // 未更新なら後者が null の方が仕様としての整合性があり、メモリーの消費量も少ないだろう
        // 未更新の判別そのものは、二つの値を比較することで可能だが、仕様として何となく気に入らない

        // Sat, 04 May 2019 09:18:16 GMT
        // CreatedAtUtc 方式の方が、1) 英語では短くなることが多い、2) CreatedBy などの類似プロパティーを作りやすい、というメリットがある
        // しかし、動詞がまずあると、そこにつながるのは副詞がメインで、ある程度以上の表現力が必要なところで英語の問題になる
        // 形容詞、名詞、前置詞を組み合わせていく方が、意味が複雑になってきてからも対応力が高い
        // https://davidappleyard.com/english/jgrammar.htm

        // Sun, 05 May 2019 06:23:09 GMT
        // 「値を設定しない」「値が存在しない」といった意味で Value に明示的に null に設定することも可能
        // そして、値が存在しないなら、キーそのものを出力しないのが慣例なので、ここでもそれに従う

        public DateTime CreationUtc { get; set; }

        public DateTime? LastUpdateUtc { get; set; }

        public string Value { get; set; }

        public static readonly Func <nSimpleKvsEntry, nDictionary> EntryToDictionary = (entry) =>
        {
            nDictionary xDictionary = new nDictionary ();
            xDictionary.SetDateTime ("CreationUtc", entry.CreationUtc); // 必須

            if (entry.LastUpdateUtc != null)
                xDictionary.SetDateTime ("LastUpdateUtc", entry.LastUpdateUtc.Value); // オプション

            if (string.IsNullOrEmpty (entry.Value) == false)
                xDictionary.SetString ("Value", entry.Value); // オプション

            return xDictionary;
        };

        public static readonly Func <nDictionary, nSimpleKvsEntry> DictionaryToEntry = (dictionary) =>
        {
            nSimpleKvsEntry xEntry = new nSimpleKvsEntry ();
            xEntry.CreationUtc = dictionary.GetDateTime ("CreationUtc"); // 必須

            if (dictionary.ContainsKey ("LastUpdateUtc"))
                xEntry.LastUpdateUtc = dictionary.GetDateTime ("LastUpdateUtc"); // オプション

            xEntry.Value = dictionary.GetStringOrNull ("Value"); // オプション
            return xEntry;
        };

        public nSimpleKvsEntry (DateTime creationUtc, DateTime? lastUpdateUtc, string value)
        {
            CreationUtc = creationUtc;
            LastUpdateUtc = lastUpdateUtc;
            Value = value;
        }

        /// <summary>
        /// CreationUtc は、自動的に DateTime.UtcNow になる。
        /// </summary>
        public nSimpleKvsEntry (string value): this (DateTime.UtcNow, null, value)
        {
        }

        public nSimpleKvsEntry ()
        {
        }
    }
}

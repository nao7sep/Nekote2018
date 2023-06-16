using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // キーと値の組み合わせを扱うことはかなり頻繁なので、毎回 Dictionary <string, string> と書かずに済むようにしておく
    // また、このクラスには、GetNotEmptyValueOrDefault など、地味に便利なメソッドがあり、今後も必要に応じて追加していく

    // Sun, 19 May 2019 00:21:59 GMT
    // Dictionary では null をキーにできないが、NameValueCollection では可能
    // できない方が良い仕様と思うが、後者で、できないとみなしてのコードを書くべきでない
    // たまに、どちらがどうだったか迷うため、このコメントを両方のクラスに貼っておく

    public class nDictionary
    {
        public Dictionary <string, string> Dictionary { get; private set; }

        public IEqualityComparer <string> Comparer
        {
            get
            {
                return Dictionary.Comparer;
            }
        }

        public Dictionary <string, string>.KeyCollection Keys
        {
            get
            {
                return Dictionary.Keys;
            }
        }

        public Dictionary <string, string>.ValueCollection Values
        {
            get
            {
                return Dictionary.Values;
            }
        }

        public int Count
        {
            get
            {
                return Dictionary.Count;
            }
        }

        /// <summary>
        /// get は、キーが null または存在しないときに落ち、set は、キーが存在しなくても落ちない。
        /// </summary>
        public string this [string key]
        {
            get
            {
                return Dictionary [key];
            }

            set
            {
                Dictionary [key] = value;
            }
        }

        // キーの大文字と小文字を区別しない Dictionary を作ることが多いため、コンストラクターを用意
        // InvariantCulture* に決め打ちだが、通常の使用においてはそれで問題ない

        // comparer を指定しない場合、EqualityComparer <T>.Default が使われる
        // これは、uses the overrides of Object.Equals とのことで、
        // string.Equals は、performs an ordinal (case-sensitive and culture-insensitive) comparison とのこと
        // つまり、StringComparer.InvariantCulture が指定されたかのように動作するということである
        // そこまで判明しているなら指定しない理由もないため、ignoresCase の値に関わらず、StringComparer を指定している
        // https://msdn.microsoft.com/en-us/library/ms132092.aspx
        // https://msdn.microsoft.com/en-us/library/ms224763.aspx
        // https://msdn.microsoft.com/en-us/library/858x0yyx.aspx

        // Mon, 20 May 2019 11:44:12 GMT
        // IEqualityComparer を受け取れるコンストラクターを追加し、コードを整えた
        // this を使うなら ; で定義も終わってほしいが、それはできないようである

        public nDictionary (bool ignoresCase = false): this (ignoresCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture)
        {
        }

        public nDictionary (IEqualityComparer <string> comparer): this (new Dictionary <string, string> (comparer))
        {
        }

        public nDictionary (Dictionary <string, string> dictionary) =>
            Dictionary = dictionary;

        // Sun, 19 May 2019 02:40:41 GMT
        // Add だったものを AddValue に変更した
        // Value と Values の区別が必要な nNameValueCollection との整合性のため
        // Get* に Value が入るため、そちらと整合させる目的もあってのこと

        /// <summary>
        /// キーが既に存在すると落ちるため、重複チェックに使える。
        /// </summary>
        public void AddValue (string key, string value) =>
            Dictionary.Add (key, value);

        // Sun, 19 May 2019 02:43:19 GMT
        // ないと、なぜないか気になることがあったため用意した
        // this [key] と同じラッパーであり、コストも同じ

        /// <summary>
        /// キーが既存でも落ちず、なければ作られる。
        /// </summary>
        public void SetValue (string key, string value) =>
            Dictionary [key] = value;

        public bool ContainsKey (string key)
        {
            return Dictionary.ContainsKey (key);
        }

        public bool ContainsValue (string value)
        {
            return Dictionary.ContainsValue (value);
        }

        // Sun, 19 May 2019 02:45:15 GMT
        // SetValue 同様、ないとその理由が気になるので用意

        /// <summary>
        /// キーが null または存在しないときに落ちる。
        /// </summary>
        public string GetValue (string key) =>
            Dictionary [key];

        public bool TryGetValue (string key, out string value)
        {
            // キーが存在するかどうかのみを見る
            return Dictionary.TryGetValue (key, out value);
        }

        // 以下、キーが存在しないか、値が null または "" のときにはデフォルト値を返すメソッドを用意しておく
        // このクラスの使用においては、基本的には this [key] で値を設定し、GetNot* で取得するのがシンプルかつ安全

        public string GetValueOrDefault (string key, string value)
        {
            if (Dictionary.ContainsKey (key))
                return Dictionary [key];

            return value;
        }

        public string GetNotNullValueOrDefault (string key, string value)
        {
            if (Dictionary.ContainsKey (key))
            {
                string xValue = Dictionary [key];

                if (xValue != null)
                    return xValue;
            }

            return value;
        }

        public string GetNotEmptyValueOrDefault (string key, string value)
        {
            if (Dictionary.ContainsKey (key))
            {
                string xValue = Dictionary [key];

                if (string.IsNullOrEmpty (xValue) == false)
                    return xValue;
            }

            return value;
        }

        #region それぞれの型に対応する Set* / Get* の糖衣メソッド // OK
        // 落ちてはならないが、this が元々落ちないためそのまま使う
        public void SetString (string key, string value) =>
            Dictionary [key] = value;

        // 落ちるべきで、こちらも this が最初からそうなっているのでそのまま
        public string GetString (string key) =>
            Dictionary [key];

        // 落ちるべきでなく、"" が得られても使い物にならないことが多いため、not empty で取得
        public string GetStringOrDefault (string key, string value) =>
            GetNotEmptyValueOrDefault (key, value);

        public string GetStringOrEmpty (string key) =>
            GetStringOrDefault (key, string.Empty);

        public string GetStringOrNull (string key) =>
            GetStringOrDefault (key, null);

        public void SetBool (string key, bool value) =>
            Dictionary [key] = value.nToString ();

        public bool GetBool (string key) =>
            Dictionary [key].nToBool ();

        // null を得てから bool に変換しても問題がない
        public bool GetBoolOrDefault (string key, bool value) =>
            GetValueOrDefault (key, null).nToBoolOrDefault (value);

        public void SetByte (string key, byte value) =>
            Dictionary [key] = value.nToString ();

        public byte GetByte (string key) =>
            Dictionary [key].nToByte ();

        public byte GetByteOrDefault (string key, byte value) =>
            GetValueOrDefault (key, null).nToByteOrDefault (value);

        public void SetChar (string key, char value) =>
            Dictionary [key] = value.nToUShortString ();

        public char GetChar (string key) =>
            Dictionary [key].nUShortToChar ();

        public char GetCharOrDefault (string key, char value) =>
            GetValueOrDefault (key, null).nUShortToCharOrDefault (value);

        public void SetDateTime (string key, DateTime value) =>
            Dictionary [key] = value.nToLongString ();

        public DateTime GetDateTime (string key) =>
            Dictionary [key].nLongToDateTime ();

        public DateTime GetDateTimeOrDefault (string key, DateTime value) =>
            GetValueOrDefault (key, null).nLongToDateTimeOrDefault (value);

        public void SetDecimal (string key, decimal value) =>
            Dictionary [key] = value.nToString ();

        public decimal GetDecimal (string key) =>
            Dictionary [key].nToDecimal ();

        public decimal GetDecimalOrDefault (string key, decimal value) =>
            GetValueOrDefault (key, null).nToDecimalOrDefault (value);

        public void SetDouble (string key, double value) =>
            Dictionary [key] = value.nToString ();

        public double GetDouble (string key) =>
            Dictionary [key].nToDouble ();

        public double GetDoubleOrDefault (string key, double value) =>
            GetValueOrDefault (key, null).nToDoubleOrDefault (value);

        public void SetEnum (string key, Enum value) =>
            Dictionary [key] = value.nToString ();

        public object GetEnum (string key, Type type) =>
            Dictionary [key].nToEnum (type);

        public object GetEnumOrDefault (string key, Type type, object value) =>
            GetValueOrDefault (key, null).nToEnumOrDefault (type, value);

        // 普通はジェネリックの方を使うのが便利

        public T GetEnum <T> (string key) =>
            Dictionary [key].nToEnum <T> ();

        public T GetEnumOrDefault <T> (string key, T value) =>
            GetValueOrDefault (key, null).nToEnumOrDefault (value);

        public void SetFloat (string key, float value) =>
            Dictionary [key] = value.nToString ();

        public float GetFloat (string key) =>
            Dictionary [key].nToFloat ();

        public float GetFloatOrDefault (string key, float value) =>
            GetValueOrDefault (key, null).nToFloatOrDefault (value);

        public void SetGuid (string key, Guid value) =>
            Dictionary [key] = value.nToString ();

        public Guid GetGuid (string key) =>
            Dictionary [key].nToGuid ();

        public Guid GetGuidOrDefault (string key, Guid value) =>
            GetValueOrDefault (key, null).nToGuidOrDefault (value);

        public void SetInt (string key, int value) =>
            Dictionary [key] = value.nToString ();

        public int GetInt (string key) =>
            Dictionary [key].nToInt ();

        public int GetIntOrDefault (string key, int value) =>
            GetValueOrDefault (key, null).nToIntOrDefault (value);

        public void SetLong (string key, long value) =>
            Dictionary [key] = value.nToString ();

        public long GetLong (string key) =>
            Dictionary [key].nToLong ();

        public long GetLongOrDefault (string key, long value) =>
            GetValueOrDefault (key, null).nToLongOrDefault (value);

        public void SetSByte (string key, sbyte value) =>
            Dictionary [key] = value.nToString ();

        public sbyte GetSByte (string key) =>
            Dictionary [key].nToSByte ();

        public sbyte GetSByteOrDefault (string key, sbyte value) =>
            GetValueOrDefault (key, null).nToSByteOrDefault (value);

        public void SetShort (string key, short value) =>
            Dictionary [key] = value.nToString ();

        public short GetShort (string key) =>
            Dictionary [key].nToShort ();

        public short GetShortOrDefault (string key, short value) =>
            GetValueOrDefault (key, null).nToShortOrDefault (value);

        public void SetTimeSpan (string key, TimeSpan value) =>
            Dictionary [key] = value.nToLongString ();

        public TimeSpan GetTimeSpan (string key) =>
            Dictionary [key].nLongToTimeSpan ();

        public TimeSpan GetTimeSpanOrDefault (string key, TimeSpan value) =>
            GetValueOrDefault (key, null).nLongToTimeSpanOrDefault (value);

        public void SetUInt (string key, uint value) =>
            Dictionary [key] = value.nToString ();

        public uint GetUInt (string key) =>
            Dictionary [key].nToUInt ();

        public uint GetUIntOrDefault (string key, uint value) =>
            GetValueOrDefault (key, null).nToUIntOrDefault (value);

        public void SetULong (string key, ulong value) =>
            Dictionary [key] = value.nToString ();

        public ulong GetULong (string key) =>
            Dictionary [key].nToULong ();

        public ulong GetULongOrDefault (string key, ulong value) =>
            GetValueOrDefault (key, null).nToULongOrDefault (value);

        public void SetUShort (string key, ushort value) =>
            Dictionary [key] = value.nToString ();

        public ushort GetUShort (string key) =>
            Dictionary [key].nToUShort ();

        public ushort GetUShortOrDefault (string key, ushort value) =>
            GetValueOrDefault (key, null).nToUShortOrDefault (value);
        #endregion

        public Dictionary <string, string>.Enumerator GetEnumerator ()
        {
            return Dictionary.GetEnumerator ();
        }

        public bool Remove (string key)
        {
            // キーが存在しなくても false が返るだけで落ちない
            return Dictionary.Remove (key);
        }

        public void Clear ()
        {
            Dictionary.Clear ();
        }
    }
}

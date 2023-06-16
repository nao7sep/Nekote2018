using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // nStringTable にて使われる「行」のクラス
    // string row では少し分かりにくいため string table row にすることも考えたが、しつこさも感じるため却下
    // List に nStringField を用意して入れる選択肢もあるが、Content しかプロパティーを持たないクラスになるため冗長
    // .NET の DataRow も内部的には ArrayList で object の配列を扱っているようであり、「セル」のクラスはない
    // それらしきものとして DataColumn があるが、これはスキーマとしての「列」の定義を扱うもののようだ

    // 「表」は、nStringData では分かりにくいため普通に table とし、「行」は、record では「録る」のニュアンスがあるため row とした
    // 「列」は、column では表の1行目の、「この行のこの項目は何々ですよ」のキー的なものになるらしく、行内データを columns とするコードを目にしない
    // そのため、row には fields が入っているものとし、そのインデックスも、columnIndex でなく fieldIndex とすることでジャグ配列であることを強調している
    // https://github.com/parsecsv/csv-spec

    // Sun, 05 May 2019 22:53:11 GMT
    // 久々に他のところで仕様に組み込もうとして、nStringTableRow だと思っていたものが nStringRow で困惑した
    // しばらく時間が経ったことで、自分で使おうとしても分かりにくかったので、nStringTableRow に変更した

    public class nStringTableRow
    {
        public List <string> Fields { get; private set; }

        public int FieldCount
        {
            get
            {
                return Fields.Count;
            }
        }

        public string this [int index]
        {
            get
            {
                return Fields [index];
            }

            set
            {
                Fields [index] = value;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Fields.Any (x => string.IsNullOrEmpty (x) == false) == false;
            }
        }

        public int NotEmptyFieldCount
        {
            get
            {
                return Fields.Count (x => string.IsNullOrEmpty (x) == false);
            }
        }

        public nStringTableRow ()
        {
            Fields = new List <string> ();
        }

        public void AddField (string value)
        {
            Fields.Add (value);
        }

        public void InsertField (int index, string value)
        {
            Fields.Insert (index, value);
        }

        public bool Contains (string value)
        {
            return Fields.Contains (value);
        }

        public void EnsureFieldExists (int index)
        {
            for (int temp = FieldCount; temp <= index; temp ++)
                Fields.Add (null);
        }

        /// <summary>
        /// 存在しないフィールドを指定しても、それまでが作成されるため落ちない。
        /// </summary>
        public void SetField (int index, string value)
        {
            if (index < FieldCount)
                Fields [index] = value;

            else
            {
                // index のところに書けないからその一つ前までの要素を存在させ、
                // 最後の一つは、Add によって場所の確保と書き込みを同時に行っている
                // index - 1 がマイナスになることにも問題はない
                EnsureFieldExists (index - 1);
                Fields.Add (value);
            }
        }

        public string GetField (int index)
        {
            return Fields [index];
        }

        public string GetFieldOrDefault (int index, string value)
        {
            if (index < FieldCount)
                return Fields [index];

            return value;
        }

        public string GetNotNullFieldOrDefault (int index, string value)
        {
            if (index < FieldCount)
            {
                string xField = Fields [index];

                if (xField != null)
                    return xField;
            }

            return value;
        }

        public string GetNotEmptyFieldOrDefault (int index, string value)
        {
            if (index < FieldCount)
            {
                string xField = Fields [index];

                if (string.IsNullOrEmpty (xField) == false)
                    return xField;
            }

            return value;
        }

        #region それぞれの型に対応する Set* / Get* の糖衣メソッド // OK
        // Mon, 06 May 2019 12:03:08 GMT
        // 行単位で値を読み書きすることが出てきたので、以前は nStringTable だけに存在したメソッドを移植
        // 実装が非常に似ていたので、インデックスを減らし、Rows を Fields にするというのを単純な置換で行った
        // ザッと目を通す程度のチェックを行うが、たぶん動くので、全てのメソッドのテストまでは省略

        // 落ちてはいけないので SetField を使う
        // これ以上の展開はできない
        public void SetString (int index, string value) =>
            SetField (index, value);

        // 落ちるべきなので this の中身を使って高速化
        public string GetString (int index) =>
            Fields [index];

        // "" が得られても使い物にならないところが多いため、not empty で取得
        public string GetStringOrDefault (int index, string value) =>
            GetNotEmptyFieldOrDefault (index, value);

        public string GetStringOrEmpty (int index) =>
            GetStringOrDefault (index, string.Empty);

        public string GetStringOrNull (int index) =>
            GetStringOrDefault (index, null);

        public void SetBool (int index, bool value) =>
            SetField (index, value.nToString ());

        public bool GetBool (int index) =>
            Fields [index].nToBool ();

        // null を得て nToBoolOrDefault に渡すのも問題がない
        public bool GetBoolOrDefault (int index, bool value) =>
            GetFieldOrDefault (index, null).nToBoolOrDefault (value);

        public void SetByte (int index, byte value) =>
            SetField (index, value.nToString ());

        public byte GetByte (int index) =>
            Fields [index].nToByte ();

        public byte GetByteOrDefault (int index, byte value) =>
            GetFieldOrDefault (index, null).nToByteOrDefault (value);

        public void SetChar (int index, char value) =>
            SetField (index, value.nToUShortString ());

        public char GetChar (int index) =>
            Fields [index].nUShortToChar ();

        public char GetCharOrDefault (int index, char value) =>
            GetFieldOrDefault (index, null).nUShortToCharOrDefault (value);

        public void SetDateTime (int index, DateTime value) =>
            SetField (index, value.nToLongString ());

        public DateTime GetDateTime (int index) =>
            Fields [index].nLongToDateTime ();

        public DateTime GetDateTimeOrDefault (int index, DateTime value) =>
            GetFieldOrDefault (index, null).nLongToDateTimeOrDefault (value);

        public void SetDecimal (int index, decimal value) =>
            SetField (index, value.nToString ());

        public decimal GetDecimal (int index) =>
            Fields [index].nToDecimal ();

        public decimal GetDecimalOrDefault (int index, decimal value) =>
            GetFieldOrDefault (index, null).nToDecimalOrDefault (value);

        public void SetDouble (int index, double value) =>
            SetField (index, value.nToString ());

        public double GetDouble (int index) =>
            Fields [index].nToDouble ();

        public double GetDoubleOrDefault (int index, double value) =>
            GetFieldOrDefault (index, null).nToDoubleOrDefault (value);

        public void SetEnum (int index, Enum value) =>
            SetField (index, value.nToString ());

        public object GetEnum (int index, Type type) =>
            Fields [index].nToEnum (type);

        public object GetEnumOrDefault (int index, Type type, object value) =>
            GetFieldOrDefault (index, null).nToEnumOrDefault (type, value);

        // 通常は、ジェネリックのものを使う

        public T GetEnum <T> (int index) =>
            Fields [index].nToEnum <T> ();

        public T GetEnumOrDefault <T> (int index, T value) =>
            GetFieldOrDefault (index, null).nToEnumOrDefault (value);

        public void SetFloat (int index, float value) =>
            SetField (index, value.nToString ());

        public float GetFloat (int index) =>
            Fields [index].nToFloat ();

        public float GetFloatOrDefault (int index, float value) =>
            GetFieldOrDefault (index, null).nToFloatOrDefault (value);

        public void SetGuid (int index, Guid value) =>
            SetField (index, value.nToString ());

        public Guid GetGuid (int index) =>
            Fields [index].nToGuid ();

        public Guid GetGuidOrDefault (int index, Guid value) =>
            GetFieldOrDefault (index, null).nToGuidOrDefault (value);

        public void SetInt (int index, int value) =>
            SetField (index, value.nToString ());

        public int GetInt (int index) =>
            Fields [index].nToInt ();

        public int GetIntOrDefault (int index, int value) =>
            GetFieldOrDefault (index, null).nToIntOrDefault (value);

        public void SetLong (int index, long value) =>
            SetField (index, value.nToString ());

        public long GetLong (int index) =>
            Fields [index].nToLong ();

        public long GetLongOrDefault (int index, long value) =>
            GetFieldOrDefault (index, null).nToLongOrDefault (value);

        public void SetSByte (int index, sbyte value) =>
            SetField (index, value.nToString ());

        public sbyte GetSByte (int index) =>
            Fields [index].nToSByte ();

        public sbyte GetSByteOrDefault (int index, sbyte value) =>
            GetFieldOrDefault (index, null).nToSByteOrDefault (value);

        public void SetShort (int index, short value) =>
            SetField (index, value.nToString ());

        public short GetShort (int index) =>
            Fields [index].nToShort ();

        public short GetShortOrDefault (int index, short value) =>
            GetFieldOrDefault (index, null).nToShortOrDefault (value);

        public void SetTimeSpan (int index, TimeSpan value) =>
            SetField (index, value.nToLongString ());

        public TimeSpan GetTimeSpan (int index) =>
            Fields [index].nLongToTimeSpan ();

        public TimeSpan GetTimeSpanOrDefault (int index, TimeSpan value) =>
            GetFieldOrDefault (index, null).nLongToTimeSpanOrDefault (value);

        public void SetUInt (int index, uint value) =>
            SetField (index, value.nToString ());

        public uint GetUInt (int index) =>
            Fields [index].nToUInt ();

        public uint GetUIntOrDefault (int index, uint value) =>
            GetFieldOrDefault (index, null).nToUIntOrDefault (value);

        public void SetULong (int index, ulong value) =>
            SetField (index, value.nToString ());

        public ulong GetULong (int index) =>
            Fields [index].nToULong ();

        public ulong GetULongOrDefault (int index, ulong value) =>
            GetFieldOrDefault (index, null).nToULongOrDefault (value);

        public void SetUShort (int index, ushort value) =>
            SetField (index, value.nToString ());

        public ushort GetUShort (int index) =>
            Fields [index].nToUShort ();

        public ushort GetUShortOrDefault (int index, ushort value) =>
            GetFieldOrDefault (index, null).nToUShortOrDefault (value);
        #endregion

        public void RemoveField (string value)
        {
            Fields.Remove (value);
        }

        public void RemoveFieldAt (int index)
        {
            Fields.RemoveAt (index);
        }

        public void Clear ()
        {
            Fields.Clear ();
        }
    }
}

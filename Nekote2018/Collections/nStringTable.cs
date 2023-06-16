using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // まだ存在しないセルの行インデックスと列インデックスを指定してそこにアクセスすることも可能なジャグ配列のクラス
    // 構造の分からないデータを CSV ファイルから読み出すなどにおいて DataTable などが使いにくいため用意した
    // データのリレーションを扱う DataTable のような上等なクラスでは全くないため、string table という表現を採用
    // 「行」に含まれるものを column でなく field と表現する理由については、nStringTableRow.cs に書いておく

    public class nStringTable
    {
        public List <nStringTableRow> Rows { get; private set; }

        public int RowCount
        {
            get
            {
                return Rows.Count;
            }
        }

        // 通常の this では、範囲外のインデックスで落とす

        public nStringTableRow this [int index]
        {
            get
            {
                return Rows [index];
            }

            set
            {
                Rows [index] = value;
            }
        }

        public string this [int rowIndex, int fieldIndex]
        {
            get
            {
                return Rows [rowIndex] [fieldIndex];
            }

            set
            {
                Rows [rowIndex] [fieldIndex] = value;
            }
        }

        // CSV ファイルでは、null と "" が区別されない
        // 特殊なエスケープシーケンスなどによって null を表現することも可能だが、あまり独自仕様に走りたくない
        // そのため、null と "" は両方とも「空」とみなし、IsEmpty では全てのセルがそうであるかを調べる

        public bool IsEmpty
        {
            get
            {
                return Rows.Any (x => x.IsEmpty == false) == false;
            }
        }

        public int TotalFieldCount
        {
            get
            {
                return Rows.Sum (x => x.FieldCount);
            }
        }

        public int TotalNotEmptyFieldCount
        {
            get
            {
                return Rows.Sum (x => x.NotEmptyFieldCount);
            }
        }

        // ジャグ配列を普通の二次元配列にするときに確保するべきメモリー領域を調べるためのプロパティー
        // 最大と最小を意味する表現としては、largest / smallest のペアを使っていくことも選択肢だが、
        // > オペレーターが greater で、< は less なので、命名にはそれらの最上級を使っていく
        // max / min は、いくつかの要素のうち最大・最小というより、取り得る値の範囲の上限・下限だと思う

        public int GreatestFieldCount
        {
            get
            {
                return Rows.Max (x => x.FieldCount);
            }
        }

        public nStringTable ()
        {
            Rows = new List <nStringTableRow> ();
        }

        public void AddRow (nStringTableRow row)
        {
            Rows.Add (row);
        }

        // 行の作成、参照の取得、表への追加を1行で行うためのメソッド
        // スキーマを扱う DataTable などとは根本的に違うが、同名のメソッドがあることを軽く意識している
        // https://msdn.microsoft.com/en-us/library/system.data.datatable.newrow.aspx

        public nStringTableRow NewRow ()
        {
            nStringTableRow xRow = new nStringTableRow ();
            Rows.Add (xRow);
            return xRow;
        }

        public void InsertRow (int index, nStringTableRow row)
        {
            Rows.Insert (index, row);
        }

        public bool Contains (nStringTableRow row)
        {
            return Rows.Contains (row);
        }

        // 容量でなく、nStringTableRow のインスタンスが必要数に足りていて、インデックスでのアクセスで落ちないことを確実とするメソッド
        // このメソッドによって生じた配列の要素に対しては、インスタンスであるという前提でのアクセスがすぐに行われるため、new している
        // null を詰めておくだけでは後続のコードで落ちる

        public void EnsureRowExists (int index)
        {
            for (int temp = RowCount; temp <= index; temp ++)
                Rows.Add (new nStringTableRow ());
        }

        public void EnsureFieldExists (int rowIndex, int fieldIndex)
        {
            EnsureRowExists (rowIndex);
            Rows [rowIndex].EnsureFieldExists (fieldIndex);
        }

        /// <summary>
        /// 存在しないフィールドを指定しても、それまでが作成されるため落ちない。
        /// </summary>
        public void SetField (int rowIndex, int fieldIndex, string value)
        {
            EnsureRowExists (rowIndex);
            Rows [rowIndex].SetField (fieldIndex, value);
        }

        // 構造を持つデータを扱うクラスに、自身のデータを一括処理させるメソッドを追加するのは、あまり好ましいことでないと思う
        // そのたびに新しいインスタンスを作ってデータをコピーするなら話は別だが、そうでないならループなどで呼びすぎになるリスクがある
        // ただ、CSV などとの相互変換が想定されるこのクラスにおいて、null と "" をどちらかに統一できるのは、後続の処理の効率性を高める
        // また、その他の形式でデータを出力するときに null と "" のいずれかに値を統一することは、出力データの品質向上につながる

        public void SetEmptyToNullFields ()
        {
            foreach (nStringTableRow xRow in Rows)
            {
                // xRow.FieldCount の値のキャッシュを考えたが、こういう無駄は .NET につきもの
                // 10列のテーブルが100行とか1,000行とか程度なら、そもそも負荷ですらない
                for (int temp = 0; temp < xRow.FieldCount; temp ++)
                {
                    if (xRow [temp] == null)
                        xRow [temp] = string.Empty;
                }
            }
        }

        public void SetNullToEmptyFields ()
        {
            foreach (nStringTableRow xRow in Rows)
            {
                for (int temp = 0; temp < xRow.FieldCount; temp ++)
                {
                    // 一応確認したが、左辺が null でも問題はない
                    if (xRow [temp] == string.Empty)
                        xRow [temp] = null;
                }
            }
        }

        // 落ちる

        public string GetField (int rowIndex, int fieldIndex)
        {
            return Rows [rowIndex] [fieldIndex];
        }

        public string GetFieldOrDefault (int rowIndex, int fieldIndex, string value)
        {
            if (rowIndex < RowCount)
            {
                nStringTableRow xRow = Rows [rowIndex];

                if (fieldIndex < xRow.FieldCount)
                    return xRow [fieldIndex];
            }

            return value;
        }

        public string GetNotNullFieldOrDefault (int rowIndex, int fieldIndex, string value)
        {
            if (rowIndex < RowCount)
            {
                nStringTableRow xRow = Rows [rowIndex];

                if (fieldIndex < xRow.FieldCount)
                {
                    string xField = xRow [fieldIndex];

                    if (xField != null)
                        return xField;
                }
            }

            return value;
        }

        public string GetNotEmptyFieldOrDefault (int rowIndex, int fieldIndex, string value)
        {
            if (rowIndex < RowCount)
            {
                nStringTableRow xRow = Rows [rowIndex];

                if (fieldIndex < xRow.FieldCount)
                {
                    string xField = xRow [fieldIndex];

                    if (string.IsNullOrEmpty (xField) == false)
                        return xField;
                }
            }

            return value;
        }

        #region それぞれの型に対応する Set* / Get* の糖衣メソッド // OK
        // 落ちてはいけないので SetField を使う
        // これ以上の展開はできない
        public void SetString (int rowIndex, int fieldIndex, string value) =>
            SetField (rowIndex, fieldIndex, value);

        // 落ちるべきなので this の中身を使って高速化
        public string GetString (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex];

        // "" が得られても使い物にならないところが多いため、not empty で取得
        public string GetStringOrDefault (int rowIndex, int fieldIndex, string value) =>
            GetNotEmptyFieldOrDefault (rowIndex, fieldIndex, value);

        public string GetStringOrEmpty (int rowIndex, int fieldIndex) =>
            GetStringOrDefault (rowIndex, fieldIndex, string.Empty);

        public string GetStringOrNull (int rowIndex, int fieldIndex) =>
            GetStringOrDefault (rowIndex, fieldIndex, null);

        public void SetBool (int rowIndex, int fieldIndex, bool value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public bool GetBool (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToBool ();

        // null を得て nToBoolOrDefault に渡すのも問題がない
        public bool GetBoolOrDefault (int rowIndex, int fieldIndex, bool value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToBoolOrDefault (value);

        public void SetByte (int rowIndex, int fieldIndex, byte value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public byte GetByte (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToByte ();

        public byte GetByteOrDefault (int rowIndex, int fieldIndex, byte value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToByteOrDefault (value);

        public void SetChar (int rowIndex, int fieldIndex, char value) =>
            SetField (rowIndex, fieldIndex, value.nToUShortString ());

        public char GetChar (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nUShortToChar ();

        public char GetCharOrDefault (int rowIndex, int fieldIndex, char value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nUShortToCharOrDefault (value);

        public void SetDateTime (int rowIndex, int fieldIndex, DateTime value) =>
            SetField (rowIndex, fieldIndex, value.nToLongString ());

        public DateTime GetDateTime (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nLongToDateTime ();

        public DateTime GetDateTimeOrDefault (int rowIndex, int fieldIndex, DateTime value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nLongToDateTimeOrDefault (value);

        public void SetDecimal (int rowIndex, int fieldIndex, decimal value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public decimal GetDecimal (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToDecimal ();

        public decimal GetDecimalOrDefault (int rowIndex, int fieldIndex, decimal value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToDecimalOrDefault (value);

        public void SetDouble (int rowIndex, int fieldIndex, double value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public double GetDouble (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToDouble ();

        public double GetDoubleOrDefault (int rowIndex, int fieldIndex, double value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToDoubleOrDefault (value);

        public void SetEnum (int rowIndex, int fieldIndex, Enum value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public object GetEnum (int rowIndex, int fieldIndex, Type type) =>
            Rows [rowIndex] [fieldIndex].nToEnum (type);

        public object GetEnumOrDefault (int rowIndex, int fieldIndex, Type type, object value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToEnumOrDefault (type, value);

        // 通常は、ジェネリックのものを使う

        public T GetEnum <T> (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToEnum <T> ();

        public T GetEnumOrDefault <T> (int rowIndex, int fieldIndex, T value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToEnumOrDefault (value);

        public void SetFloat (int rowIndex, int fieldIndex, float value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public float GetFloat (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToFloat ();

        public float GetFloatOrDefault (int rowIndex, int fieldIndex, float value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToFloatOrDefault (value);

        public void SetGuid (int rowIndex, int fieldIndex, Guid value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public Guid GetGuid (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToGuid ();

        public Guid GetGuidOrDefault (int rowIndex, int fieldIndex, Guid value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToGuidOrDefault (value);

        public void SetInt (int rowIndex, int fieldIndex, int value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public int GetInt (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToInt ();

        public int GetIntOrDefault (int rowIndex, int fieldIndex, int value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToIntOrDefault (value);

        public void SetLong (int rowIndex, int fieldIndex, long value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public long GetLong (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToLong ();

        public long GetLongOrDefault (int rowIndex, int fieldIndex, long value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToLongOrDefault (value);

        public void SetSByte (int rowIndex, int fieldIndex, sbyte value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public sbyte GetSByte (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToSByte ();

        public sbyte GetSByteOrDefault (int rowIndex, int fieldIndex, sbyte value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToSByteOrDefault (value);

        public void SetShort (int rowIndex, int fieldIndex, short value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public short GetShort (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToShort ();

        public short GetShortOrDefault (int rowIndex, int fieldIndex, short value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToShortOrDefault (value);

        public void SetTimeSpan (int rowIndex, int fieldIndex, TimeSpan value) =>
            SetField (rowIndex, fieldIndex, value.nToLongString ());

        public TimeSpan GetTimeSpan (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nLongToTimeSpan ();

        public TimeSpan GetTimeSpanOrDefault (int rowIndex, int fieldIndex, TimeSpan value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nLongToTimeSpanOrDefault (value);

        public void SetUInt (int rowIndex, int fieldIndex, uint value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public uint GetUInt (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToUInt ();

        public uint GetUIntOrDefault (int rowIndex, int fieldIndex, uint value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToUIntOrDefault (value);

        public void SetULong (int rowIndex, int fieldIndex, ulong value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public ulong GetULong (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToULong ();

        public ulong GetULongOrDefault (int rowIndex, int fieldIndex, ulong value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToULongOrDefault (value);

        public void SetUShort (int rowIndex, int fieldIndex, ushort value) =>
            SetField (rowIndex, fieldIndex, value.nToString ());

        public ushort GetUShort (int rowIndex, int fieldIndex) =>
            Rows [rowIndex] [fieldIndex].nToUShort ();

        public ushort GetUShortOrDefault (int rowIndex, int fieldIndex, ushort value) =>
            GetFieldOrDefault (rowIndex, fieldIndex, null).nToUShortOrDefault (value);
        #endregion

        public void RemoveRow (nStringTableRow row)
        {
            Rows.Remove (row);
        }

        public void RemoveRowAt (int index)
        {
            Rows.RemoveAt (index);
        }

        // SetEmptyToNullFields のところに書いた理由により、いずれのフィールドにも有効なデータの含まれない行を扱うメソッドも二つ用意しておく
        // Remove* は、List の Rows 内で空でない行のみ自分自身にコピーして最後に長さを調整し、Clear* は、空の行からフィールドを全て除く
        // データを集めていく時点で、こういうメソッドがあとあと必要になるような実装を避けるべきだが、あって便利なときもあると思う

        public void RemoveEmptyRows ()
        {
            int xLength = 0;

            for (int temp = 0; temp < RowCount; temp ++)
            {
                if (Rows [temp].IsEmpty == false)
                {
                    Rows [xLength] = Rows [temp];
                    xLength ++;
                }
            }

            // StringBuilder のように長さを設定できないようだ
            Rows.RemoveRange (xLength, RowCount - xLength);
        }

        public void ClearEmptyRows ()
        {
            foreach (nStringTableRow xRow in Rows)
            {
                if (xRow.IsEmpty)
                    xRow.Clear ();
            }
        }

        public void Clear ()
        {
            Rows.Clear ();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace Nekote
{
    // Tue, 30 Oct 2018 04:59:08 GMT
    // まず SQLite の LINQ を試したが、
    // SQL Server 用の SQL が吐かれる問題を解決できなかったため、
    // Dapper も試し、こちらも、なんだかんだ言って「やりたいことだけ」ではなかった
    // ORM は、小規模なデータベースなら、補助的なメソッドがあれば特に困らない
    // 外部 ORM を使うと、「この値がこうなっていたら、こっちをこう解釈する」ようなことに困る
    // Entity Framework もそうだが、過度な自動化を目的とするものを好きになれない
    // 自分だけ新技術を拒否って頑固な人間になっているのでないかと不安になるが、
    // 何度学んでも、「自分でパッと書いた方が早いし、小回りが利く」という事実を否定できない
    // HTML を手書きするか、ホームページビルダーのようなものを使うかの違いだろうか
    // 私の答えはやはり「補助メソッドを使った小回りの利く実装」になるようである

    public static class nSqliteDataReaderHelper
    {
        public static bool nGetBool (this SQLiteDataReader reader, int index) =>
            reader.GetInt32 (index) != 0 ? true : false;

        public static bool? nGetBoolNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (bool?) null : reader.nGetBool (index);

        public static bool nGetBoolOrDefault (this SQLiteDataReader reader, int index, bool defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetBool (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static byte nGetByte (this SQLiteDataReader reader, int index) =>
            reader.GetByte (index);

        public static byte? nGetByteNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (byte?) null : reader.nGetByte (index);

        public static byte nGetByteOrDefault (this SQLiteDataReader reader, int index, byte defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetByte (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        // Tue, 30 Oct 2018 05:06:54 GMT
        // nToString でデータベースに入れると文字が一つの文字列になる
        // ushort にして入れているため、short として読んでから char に戻す
        // いったん ushort にするのは、一応の作法のようなものである

        public static char nGetChar (this SQLiteDataReader reader, int index) =>
            ((ushort) reader.nGetShort (index)).nToChar ();

        public static char? nGetCharNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (char?) null : reader.nGetChar (index);

        public static char nGetCharOrDefault (this SQLiteDataReader reader, int index, char defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetChar (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static DateTime nGetDateTime (this SQLiteDataReader reader, int index) =>
            reader.GetInt64 (index).nToDateTime ();

        public static DateTime? nGetDateTimeNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (DateTime?) null : reader.nGetDateTime (index);

        public static DateTime nGetDateTimeOrDefault (this SQLiteDataReader reader, int index, DateTime defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetDateTime (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        // Tue, 30 Oct 2018 05:08:39 GMT
        // nSqliteCommandPartsBuilder の方に詳しく書いたが、
        // decimal, double, float は、SQLite ではラウンドトリップが不確実
        // 特に decimal は、128ビットが64ビットにされるため、後半がゴソッと0になる
        // それを避けるには、変数をバイナリーにして blob に入れるなどが必要
        // ラウンドトリップができないままだが、数値としての扱いができてほしいため、
        // 標準的な実装では、数値を文字列化して入れ、数値として読み出す

        public static decimal nGetDecimal (this SQLiteDataReader reader, int index) =>
            reader.GetDecimal (index);

        public static decimal? nGetDecimalNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (decimal?) null : reader.nGetDecimal (index);

        public static decimal nGetDecimalOrDefault (this SQLiteDataReader reader, int index, decimal defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetDecimal (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static double nGetDouble (this SQLiteDataReader reader, int index) =>
            reader.GetDouble (index);

        public static double? nGetDoubleNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (double?) null : reader.nGetDouble (index);

        public static double nGetDoubleOrDefault (this SQLiteDataReader reader, int index, double defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetDouble (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        // Tue, 30 Oct 2018 05:12:25 GMT
        // Set* ならまだしも、Get* ではジェネリックを使えないため呼び出し側がうるさくなる
        // 一応、nImageFormat, nImageFormat? の両方で動くことは確認できている

        public static object nGetEnum (this SQLiteDataReader reader, int index, Type type) =>
            reader.GetInt32 (index).nToEnum (type);

        public static object nGetEnumNullable (this SQLiteDataReader reader, int index, Type type) =>
            reader.IsDBNull (index) ? null : reader.nGetEnum (index, type);

        public static object nGetEnumOrDefault (this SQLiteDataReader reader, int index, Type type, object defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetEnum (index, type);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static float nGetFloat (this SQLiteDataReader reader, int index) =>
            reader.GetFloat (index);

        public static float? nGetFloatNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (float?) null : reader.nGetFloat (index);

        public static float nGetFloatOrDefault (this SQLiteDataReader reader, int index, float defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetFloat (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static Guid nGetGuid (this SQLiteDataReader reader, int index) =>
            reader.GetString (index).nToGuid ();

        public static Guid? nGetGuidNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (Guid?) null : reader.nGetGuid (index);

        public static Guid nGetGuidOrDefault (this SQLiteDataReader reader, int index, Guid defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetGuid (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static int nGetInt (this SQLiteDataReader reader, int index) =>
            reader.GetInt32 (index);

        public static int? nGetIntNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (int?) null : reader.nGetInt (index);

        public static int nGetIntOrDefault (this SQLiteDataReader reader, int index, int defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetInt (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static long nGetLong (this SQLiteDataReader reader, int index) =>
            reader.GetInt64 (index);

        public static long? nGetLongNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (long?) null : reader.nGetLong (index);

        public static long nGetLongOrDefault (this SQLiteDataReader reader, int index, long defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetLong (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static sbyte nGetSByte (this SQLiteDataReader reader, int index) =>
            (sbyte) reader.GetByte (index);

        public static sbyte? nGetSByteNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (sbyte?) null : reader.nGetSByte (index);

        public static sbyte nGetSByteOrDefault (this SQLiteDataReader reader, int index, sbyte defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetSByte (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static short nGetShort (this SQLiteDataReader reader, int index) =>
            reader.GetInt16 (index);

        public static short? nGetShortNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (short?) null : reader.nGetShort (index);

        public static short nGetShortOrDefault (this SQLiteDataReader reader, int index, short defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetShort (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static string nGetString (this SQLiteDataReader reader, int index) =>
            reader.GetString (index);

        public static string nGetStringNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? null : reader.nGetString (index);

        public static string nGetStringOrDefault (this SQLiteDataReader reader, int index, string defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetString (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static string nGetStringOrEmpty (this SQLiteDataReader reader, int index) =>
            reader.nGetStringOrDefault (index, string.Empty);

        public static string nGetStringOrNull (this SQLiteDataReader reader, int index) =>
            reader.nGetStringOrDefault (index, null);

        public static TimeSpan nGetTimeSpan (this SQLiteDataReader reader, int index) =>
            reader.GetInt64 (index).nToTimeSpan ();

        public static TimeSpan? nGetTimeSpanNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (TimeSpan?) null : reader.nGetTimeSpan (index);

        public static TimeSpan nGetTimeSpanOrDefault (this SQLiteDataReader reader, int index, TimeSpan defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetTimeSpan (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        // Tue, 30 Oct 2018 05:18:22 GMT
        // SQLite の integer は、入力に応じてビット数が決まるとのことで、
        // uint は Nekote が uint のまま入れるため、まずは内部的に4バイトになるはず
        // それを GetInt32 で読めば、32ビットのバイナリーが signed int として解釈されて戻り、
        // それをさらに uint にキャストするため、結果的には期待通りの動作になる

        public static uint nGetUInt (this SQLiteDataReader reader, int index) =>
            (uint) reader.GetInt32 (index);

        public static uint? nGetUIntNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (uint?) null : reader.nGetUInt (index);

        public static uint nGetUIntOrDefault (this SQLiteDataReader reader, int index, uint defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetUInt (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static ulong nGetULong (this SQLiteDataReader reader, int index) =>
            (ulong) reader.GetInt64 (index);

        public static ulong? nGetULongNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (ulong?) null : reader.nGetULong (index);

        public static ulong nGetULongOrDefault (this SQLiteDataReader reader, int index, ulong defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetULong (index);
            }

            catch
            {
                return defaultValue;
            }
        }

        public static ushort nGetUShort (this SQLiteDataReader reader, int index) =>
            (ushort) reader.GetInt16 (index);

        public static ushort? nGetUShortNullable (this SQLiteDataReader reader, int index) =>
            reader.IsDBNull (index) ? (ushort?) null : reader.nGetUShort (index);

        public static ushort nGetUShortOrDefault (this SQLiteDataReader reader, int index, ushort defaultValue)
        {
            if (reader.IsDBNull (index))
                return defaultValue;

            try
            {
                return reader.nGetUShort (index);
            }

            catch
            {
                return defaultValue;
            }
        }
    }
}

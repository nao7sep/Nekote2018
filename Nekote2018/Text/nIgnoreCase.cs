using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Wed, 05 Sep 2018 12:02:47 GMT
    // 文字列の処理において大文字・小文字を比較しない指定を true 一つで行うのでは、丸ごと書き忘れていることに気付けなかったり、
    // そのあたりのコードブロックでは比較しているのかしていないのかの意識が抜けての甘いコーディングになったりする
    // 丸ごとラッパークラスなのでパフォーマンスに軽微な影響があるだろうが、ミスが間違いなく減るメリットの方がはるかに大きい
    // メソッドの順序については、同期のしやすさを考えて nString のものをそのまま引き継いでいる

    public static class nIgnoreCase
    {
        public static int CompareNumerically (string text1, string text2)
        {
            return nString.CompareNumerically (text1, text2, true);
        }

        public static int Compare (string text1, string text2)
        {
            return nString.Compare (text1, text2, true);
        }

        public static bool StartsWith (string text1, string text2)
        {
            return nString.nStartsWith (text1, text2, true);
        }

        public static bool EndsWith (string text1, string text2)
        {
            return nString.nEndsWith (text1, text2, true);
        }

        public static int IndexOf (string text1, string text2)
        {
            return nString.nIndexOf (text1, text2, true);
        }

        public static int IndexOf (string text1, int index, string text2)
        {
            return nString.nIndexOf (text1, index, text2, true);
        }

        public static int IndexOf (string text1, int index, int length, string text2)
        {
            return nString.nIndexOf (text1, index, length, text2, true);
        }

        public static bool Contains (string text1, string text2)
        {
            return nString.nContains (text1, text2, true);
        }

        public static int LastIndexOf (string text1, string text2)
        {
            return nString.nLastIndexOf (text1, text2, true);
        }

        public static int LastIndexOf (string text1, int index, string text2)
        {
            return nString.nLastIndexOf (text1, index, text2, true);
        }

        public static int LastIndexOf (string text1, int index, int length, string text2)
        {
            return nString.nLastIndexOf (text1, index, length, text2);
        }

        public static string Replace (string text, string oldText, string newText)
        {
            return nString.nReplace (text, oldText, newText, true);
        }

        public static bool StartsWith (string text, char value)
        {
            return nString.nStartsWith (text, value, true);
        }

        public static bool EndsWith (string text, char value)
        {
            return nString.nEndsWith (text, value, true);
        }

        public static int IndexOf (string text, char value)
        {
            return nString.nIndexOf (text, value, true);
        }

        public static int IndexOf (string text, int index, char value)
        {
            return nString.nIndexOf (text, index, value, true);
        }

        public static int IndexOf (string text, int index, int length, char value)
        {
            return nString.nIndexOf (text, index, length, value, true);
        }

        public static bool Contains (string text, char value)
        {
            return nString.nContains (text, value, true);
        }

        public static int LastIndexOf (string text, char value)
        {
            return nString.nLastIndexOf (text, value, true);
        }

        public static int LastIndexOf (string text, int index, char value)
        {
            return nString.nLastIndexOf (text, index, value, true);
        }

        public static int LastIndexOf (string text, int index, int length, char value)
        {
            return nString.nLastIndexOf (text, index, length, value, true);
        }

        public static string Replace (string text, char oldChar, char newChar)
        {
            return nString.nReplace (text, oldChar, newChar, true);
        }
    }
}

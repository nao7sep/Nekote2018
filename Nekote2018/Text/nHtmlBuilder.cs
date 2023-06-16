using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Fri, 05 Apr 2019 13:27:50 GMT
    // $"" を使えば HTML の生成がだいぶ楽になるが、それでもしんどいときがあるのでクラスを用意
    // $"" とうまく使い分けて、できるだけ短いコードで、保守性の高いコードを書いていく

    public class nHtmlBuilder
    {
        public StringBuilder Builder { get; private set; } = new StringBuilder ();

        public int MaxCapacity
        {
            get
            {
                return Builder.MaxCapacity;
            }
        }

        public int Capacity
        {
            get
            {
                return Builder.Capacity;
            }

            set
            {
                Builder.Capacity = value;
            }
        }

        public int Length
        {
            get
            {
                return Builder.Length;
            }

            set
            {
                Builder.Length = value;
            }
        }

        public char this [int index]
        {
            get
            {
                return Builder [index];
            }

            set
            {
                Builder [index] = value;
            }
        }

        public void EnsureCapacity (int capacity) =>
            Builder.EnsureCapacity (capacity);

        // Fri, 05 Apr 2019 13:28:55 GMT
        // 要素や属性の name は決め打ちで書かれることが多く、ランタイムで何度もチェックするのは無駄
        // かといって、全くチェックしないのではセキュリティーが低下するため、DEBUG で見ておく

        #if DEBUG
        private static readonly char [] iInvalidChars = { '"', '&', '\'', '<', '>' };

        // Fri, 05 Apr 2019 13:29:55 GMT
        // 投げるものが例外なのは明らかだし、catch 内での throw もあるため、exception を省略
        // 最初、iThrowExceptionIfNameIsInvalid を考えたが、内部的なものだし、シンプルな方がいい

        private static void iThrowIfInvalidName (string name)
        {
            if (string.IsNullOrEmpty (name) ||
                    name.nContainsAny (iInvalidChars))
                throw new nDebugException ();
        }
        #endif

        /// <summary>
        /// 属性については、name [0], value [0], name [1], ... のように指定
        /// </summary>
        public void OpenElement (string name, params string [] attributesNamesAndValues)
        {
            #if DEBUG
            iThrowIfInvalidName (name);
            #endif

            Builder.Append ('<');
            Builder.Append (name);

            for (int temp = 0; temp < attributesNamesAndValues.Length; temp += 2)
            {
                #if DEBUG
                iThrowIfInvalidName (attributesNamesAndValues [temp * 2]);
                #endif

                Builder.Append (' ');
                Builder.Append (attributesNamesAndValues [temp * 2]);
                Builder.Append ("=\"");
                Builder.Append (attributesNamesAndValues [temp * 2 + 1].nEscapeHtml ());
                Builder.Append ('"');
            }

            Builder.Append ('>');
        }

        // Fri, 05 Apr 2019 13:32:41 GMT
        // 最初、スタックを用意して引数なしで呼べるようにする考えだったが、
        // open と close が対応している方がコードが分かりやすいし、速度も出る

        public void CloseElement (string name)
        {
            #if DEBUG
            iThrowIfInvalidName (name);
            #endif

            Builder.Append ("</");
            Builder.Append (name);
            Builder.Append ('>');
        }

        /// <summary>
        /// 属性については、name [0], value [0], name [1], ... のように指定
        /// </summary>
        public void AddElementRaw (string name, string content, params string [] attributesNamesAndValues)
        {
            #if DEBUG
            iThrowIfInvalidName (name);
            #endif

            // Fri, 05 Apr 2019 13:35:02 GMT
            // content がないなら /> で閉じるのでベタ書き

            Builder.Append ('<');
            Builder.Append (name);

            for (int temp = 0; temp < attributesNamesAndValues.Length; temp += 2)
            {
                #if DEBUG
                iThrowIfInvalidName (attributesNamesAndValues [temp * 2]);
                #endif

                Builder.Append (' ');
                Builder.Append (attributesNamesAndValues [temp * 2]);
                Builder.Append ("=\"");
                Builder.Append (attributesNamesAndValues [temp * 2 + 1].nEscapeHtml ());
                Builder.Append ('"');
            }

            if (string.IsNullOrEmpty (content) == false)
            {
                Builder.Append ('>');
                Builder.Append (content);
                Builder.Append ("</");
                Builder.Append (name);
                Builder.Append ('>');
            }

            else Builder.Append (" />");
        }

        public void AddElement (string name, string content, params string [] attributesNamesAndValues) =>
            AddElementRaw (name, content.nEscapeHtml (), attributesNamesAndValues);

        // Fri, 05 Apr 2019 13:37:02 GMT
        // Append だけでは、StringBuilder のものと同名で動作が異なるということになる
        // タグを自由に書くとか、コメントを書くとか、不可欠なので、text を入れてとりあえず用意

        public void AppendTextRaw (string text) =>
            Builder.Append (text);

        public void AppendText (string text) =>
            AppendTextRaw (text.nEscapeHtml ());

        public override string ToString () =>
            Builder.ToString ();

        public void Clear () =>
            Builder.Clear ();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace Nekote
{
    public static class nLibrary
    {
        private static Assembly mAssembly = null;

        public static Assembly Assembly
        {
            get
            {
                if (mAssembly == null)
                    mAssembly = Assembly.GetExecutingAssembly ();

                return mAssembly;
            }
        }

        private static string mNameAndVersion = null;

        public static string NameAndVersion
        {
            get
            {
                if (mNameAndVersion == null)
                    // Thu, 25 Apr 2019 04:25:02 GMT
                    // Nekote ではイレギュラーなことだが、他のクラスの internal のメソッドを借りる
                    // そうする理由などについては、nApplication の方にいろいろと書いておく
                    mNameAndVersion = nApplication.iGetNameAndVersion (Assembly);

                return mNameAndVersion;
            }
        }
    }
}

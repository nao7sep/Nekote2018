using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // デバッグ用のコードは、必ずしも具体的な問題を検出して例外を投げるのでなく、
    // もしかするとパフォーマンス低下につながるかもしれない程度のところもチェックしていく
    // そういうところで、どう悪いのかを定義して通常の例外クラスを選択するのは難しい

    [Serializable]
    public class nDebugException: Exception
    {
        public nDebugException ()
        {
        }

        public nDebugException (string message): base (message)
        {
        }

        public nDebugException (string message, Exception inner): base (message, inner)
        {
        }
    }
}

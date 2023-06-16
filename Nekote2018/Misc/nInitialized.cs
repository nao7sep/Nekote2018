using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Fri, 10 May 2019 17:58:23 GMT
    // 値の型なら Nullable によって初期化が済んでいるかどうかが分かるが、string などの参照型は、そうでない
    // string に一度だけ初期化のチャンスがあり、そのときに null になったらずっと null のままというようなケースがある
    // string は、初期値も null なので、今 null になっているのが、単体では、その一度の初期化を経てのことなのかどうか分からない
    // そのため、mIsInitialized, mValue, Value のように、二つのメンバー変数と一つのプロパティーを用意することがあるが、うるさく感じる
    // そのため、インスタンスが存在していたら初期化が済んでいるんですよ、の意味合いで nInitialized クラスを用意した
    // Nullable と同じことを、よりシンプルに、よりゆるい制限で行うものであり、分かりやすさのために敢えてこの仕様にした
    // https://docs.microsoft.com/en-us/dotnet/api/system.nullable-1
    // https://github.com/Microsoft/referencesource/blob/master/mscorlib/system/nullable.cs

    public class nInitialized <T>
    {
        public T Value { get; private set; }

        // Fri, 10 May 2019 18:01:45 GMT
        // 値を設定できるのは、コンストラクターでの一度に限られる

        public nInitialized (T value) =>
            Value = value;

        // Fri, 10 May 2019 18:02:19 GMT
        // implicit は、明示的でないキャストにおける自動変換で、explicit は、明示的なキャストによるもの
        // Age 型の xAge に Number 型の xNumber を、xAge = xNumber として代入するのが前者のようだ
        // Nullable について言えば、私は、前者は知っていたし、使っていたが、後者が可能ということは知らなかった
        // nInitialized は極めてシンプルなクラスなので両方を implicit にしてもよいが、一応、Nullable の仕様にならった
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/implicit
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/explicit

        // Thu, 16 May 2019 07:22:59 GMT
        // xHoge = null で初期化しておいたものを if (xHoge == null) のコードブロックで初期化する考えだが、
        // 以下のオペレーターを用意しておくと、xHoge = null の時点で new されてインスタンスが生じる
        // Nullable が値型にしか使えないのは、中身に null を設定できず、xHoge = null が意図通りに動くからというのもあるか
        // やりたいことは、初期化されたかどうかのフラグをなくすことだけなので、new を呼び出し側で行う使い方に変更

        // public static implicit operator nInitialized <T> (T value) =>
            // new nInitialized <T> (value);

        // public static explicit operator T (nInitialized <T> hoge) =>
            // hoge.Value;
    }
}

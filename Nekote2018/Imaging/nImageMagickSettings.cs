using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ImageMagick;

namespace Nekote
{
    // Magick.NET に関する設定を扱うクラスを個別に用意しておく
    // デフォルトでよく動くライブラリーだが、OpenCL については深刻な問題が認められる
    // 今後もいろいろ問題が見付かるなら、そのたびに nImageMagick で対応しては設定項目が埋没していく
    // config でなく settings なのは、ランタイムで変更される項目を主に想定しているため

    public static class nImageMagickSettings
    {
        // Magick.NET の OpenCL 対応については、ユーザーからの質問に答える形で開発者が以下のコメントを残している
        // The first time you use an OpenCL operation a benchmark will run to determine if it is better to use the GPU or the CPU
        // It is possible that the benchmark already ran on your machine and decided that the CPU is faster
        // This means that even though OpenCL is enabled the CPU will be used
        // これは、2017年秋現在においては実装に問題があるのか、少なくとも二つの、GPU を持たないラップトップで Magick.NET の動作に問題がある
        // 片方では、レベル補正時に画像が激しく白飛びし、もう片方では、コンデジによる写真の Resize でも各1～2分かかり、その間は過負荷でマウスカーソルすら動かない
        // 同じく GPU を持たない別のラップトップではこういった問題は起こらず、OpenCL.IsEnabled が false を返す中、処理は高速かつ正確に行われる
        // また、GPU 搭載のデスクトップでは、OpenCL.IsEnabled が true を返し、わざわざ速度を比較していないが、処理は同じく高速かつ正確である
        // さらに、マウスカーソルまで動かなくなる方でも、理由や条件が分からないが、特に問題なく処理が行われることがたまにある
        // それでも、まるで気まぐれのように処理がうまくいったり問題だらけだったりというのは信頼性に影響することで、ライブラリーでは看過できない
        // ラップトップやタブレットの多くに GPU が搭載されていないことを考えても、OpenCL はデフォルトでオフになっているべき
        // https://stackoverflow.com/questions/42050790/enable-opencl-with-magick-net
        // https://ja.wikipedia.org/wiki/OpenCL

        // IsOpenClEnabled の set でいきなり OpenCL.IsEnabled を更新することも可能だが、
        // それではデフォルト値を置きにくいし、どうせ Apply のようなメソッドは必要なので、コードがうるさくなる
        // OpenCL.IsEnabled は ThreadStatic でないようなので、ASP.NET などで使うなら最初にどこかで Apply を呼んでおきたい
        // lock を使って OpenCL.IsEnabled を更新したところで Magick.NET が内部的にマルチスレッド対応かどうかは不明
        // そのため、IsOpenClEnabled は、特別な理由があるときを除いて false のままにしておくのがよく、
        // lock がなくても OpenCL.IsEnabled = IsOpenClEnabled は何度行われても問題がないというのがシンプル

        public static bool IsOpenClEnabled { get; set; } = false;

        // Magick.NET と関わりのある、Nekote の全てのメソッドで形式的にこれを呼ぶことにする
        // 派生開発においても、直接 Magick.NET を使用する場合もとりあえず呼べるメソッドにしていく
        public static void Apply ()
        {
            OpenCL.IsEnabled = IsOpenClEnabled;
        }
    }
}

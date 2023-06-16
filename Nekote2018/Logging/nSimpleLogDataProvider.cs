using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Fri, 03 May 2019 09:49:35 GMT
    // 元々はこのクラスに四つの Create を入れていて、type, message, exception の三つについて、あったりなかったりを個別に考えて6パターンを選んで多重定義を行った
    // しかし、nStatic.Log の実装において、また6パターンの実装が必要となり、共通化のために、コードを nSimpleLogEntry のコンストラクターの方に移した
    // そのため、以下のコメントは部分的に不正確だが、Create がコンストラクターになった点を除けば有効だし、面倒なので、そのまま残しておく

    // Fri, 03 May 2019 09:53:21 GMT
    // Create がないなら空っぽのクラスであり、消してしまった方がいいのでないかとも思ったが、
    // そうすると nDateTimeBasedSimpleDataProvider <nSimpleLogEntry> を各所に書くことになるし、
    // コンストラクターに EntryToDictionary などを与える手間も出てくるため、分かりやすさのために現状のままとする

    // Fri, 03 May 2019 07:28:20 GMT
    // Create で文字列などを受け取り、nSimpleLogEntry のインスタンスにまとめ、nDateTimeBasedSimpleDataProvider で管理するクラス
    // nDateTimeBasedSimpleDataProvider は、ファイルへの書き込みおよびメモリー上でのインスタンスの保持を行うため、CRUD がすぐに行える
    // nDateTimeBasedSimpleDataProvider のインスタンスを DataProvider などのプロパティーにすることも考えたが、
    // それでは、nDateTimeBasedSimpleDataProvider が持つ CRUD のメソッドなどが遠く、かといってメソッドをラップしまくるのもアレである
    // 個人的には、クラスの継承によって下位のプロパティーやメソッドが必要以上に見えてしまい、親クラスの全体像が見えにくくなったり、
    // 下位のものを呼ぶのではデータなどに不整合が生じるから上位のものを用意したのに下位のものを呼んでしまって……といったことが起こったりが嫌だが、
    // 1) 上下ともにシンプルで、継承しても全体像が見えにくくならない、2) 上で下のものを呼んでも不整合などが起こらない、という二つの条件が満たされるなら、
    // 1) コーディングの手間を削減できる、2) デバッグの効率も高まる、の二つを考え、クラスの継承をもっと積極的に行っていいかもしれない

    // Fri, 03 May 2019 07:34:49 GMT
    // 元々は、手抜きのため、また、キーと値のペアで何でも入れるために nDictionary の使用を考えていた
    // しかし、それでは Type による抽出が重たいし、Message と ExceptionString の二つで済むケースが多いだろうから、nSimpleLogEntry を用意した
    // 何でも入れられる拡張性は、Values を用意することで実現できたため、これで、処理速度と拡張性のバランスが取れたと考えている

    // Fri, 03 May 2019 07:51:23 GMT
    // ログの出力はプログラムの各所で行う処理であり、そのたびに lock を書くのではコードがうるさくなる
    // そのため、このクラスに *AutoLock のメソッドを用意することも考えたが、それは、より上位のメソッドに任せるべき
    // 具体的には、nStatic に「ログを書く」や「通知メールを送る」を用意し、さらにそれらを一度に行うメソッドまで用意するにおいて、
    // それだけに lock を内包させ、その旨をメソッド名で分かりやすくするのが、ライブラリー側の lock まみれの回避につながる

    // Sat, 04 May 2019 02:08:30 GMT
    // 元々は、上位のクラスであると示す目的もあって nSimpleLogger という、内部感（？）の若干低い名前にしていたが、
    // まず仕様名を決めて、*DataProvider, *Entry, *Type などを用意していく慣例ができそうなので、こちらもそれに合わせた

    public class nSimpleLogDataProvider: nDateTimeBasedSimpleDataProvider <nSimpleLogEntry>
    {
        public nSimpleLogDataProvider (string directoryPath): base (directoryPath, nSimpleLogEntry.EntryToDictionary, nSimpleLogEntry.DictionaryToEntry)
        {
        }
    }
}

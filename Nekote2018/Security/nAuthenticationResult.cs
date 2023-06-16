using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Tue, 07 May 2019 17:39:59 GMT
    // 今のところログインの結果については成功と失敗の二つしか想定していないが、bool にするといつか困るかもしれないので、enum を用意
    // 英語でかなり詰まり、success, succeeded, allowed, accepted, OK, failure, failed, faultful, denied, rejected など、ネットでもいろいろ見たが、
    // 1) ユーザーでなく it を主語にできる表現、2) よって is をつけて識別子にできる、の条件を満たす、相反する表現として、以下のものを選んだ
    // こういう enum は他でも使えるため、より汎用的な名前で作ることも考えたが、派生開発における拡張性を考えて固有名にした
    // https://www.thesaurus.com/browse/successful

    public enum nAuthenticationResult
    {
        Successful = 1,
        Unsuccessful = 2
    }
}

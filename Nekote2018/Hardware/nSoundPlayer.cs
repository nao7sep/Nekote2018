using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace Nekote
{
    // Tue, 25 Sep 2018 11:36:58 GMT
    // ちょっとした音を鳴らしたいときに役立つクラスを用意しておく
    // 手元のファイルはたまたま .ogg が多いが、再生に必要なパッケージは NAudio の開発者によるものでない
    // いろいろ混ぜておかしくなるのも避けたいため、.mp3 か .wav への変換をユーザーに強いる仕様とする

    public class nSoundPlayer: IDisposable
    {
        public string Path { get; private set; }

        public WaveOutEvent WaveOut { get; private set; }

        public MediaFoundationReader Reader { get; private set; }

        // Tue, 25 Sep 2018 11:39:50 GMT
        // 当たり前だが、オーディオデバイスを持たないサーバーではきっちり false になる
        // null から始まり、再生できていれば true で、一度でも失敗すれば不可逆的に false でよい

        public bool? IsPlayable { get; private set; }

        public nSoundPlayer (string path)
        {
            Path = path;
        }

        // Tue, 25 Sep 2018 11:41:37 GMT
        // 再生できない環境が普通にあるため、try / catch を上位でやりたくなくて TryPlay とした
        // 注意すべきは、オーディオデバイスへのデータの送信が終わった時点（？）ですぐ return になることである
        // あるいは、別スレッドでの再生が始まるのかもしれないが、いずれにしてもメソッドを抜けた時点ではまだ音が鳴り始めてすらいない
        // そのため、using ですぐに Dispose するのでなく、Dispose は最後に他のところで行い、それまでインスタンスを放置するべき
        // どうしてもすぐに Dispose したいなら、IsPlaying を while に入れるのもアリだが、あまりきれいなコーディングでない

        public bool TryPlay ()
        {
            if (IsPlayable == null)
            {
                try
                {
                    WaveOut = new WaveOutEvent ();
                    Reader = new MediaFoundationReader (Path);
                    WaveOut.Init (Reader);
                    // Tue, 25 Sep 2018 11:41:06 GMT
                    // 初回は不要だろうが、入れて損はない
                    Reader.Position = 0;
                    WaveOut.Play ();
                    IsPlayable = true;
                    return true;
                }

                catch
                {
                    IsPlayable = false;
                    return false;
                }
            }

            else if (IsPlayable == true)
            {
                try
                {
                    Reader.Position = 0;
                    WaveOut.Play ();
                    return true;
                }

                catch
                {
                    IsPlayable = false;
                    return false;
                }
            }

            else return false;
        }

        public bool IsPlaying
        {
            get
            {
                return WaveOut != null && WaveOut.PlaybackState == PlaybackState.Playing;
            }
        }

        public void Dispose ()
        {
            if (WaveOut != null)
            {
                WaveOut.Dispose ();
                WaveOut = null;
            }

            if (Reader != null)
            {
                Reader.Dispose ();
                Reader = null;
            }
        }
    }
}

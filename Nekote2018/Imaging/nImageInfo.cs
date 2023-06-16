using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Wed, 07 Nov 2018 19:47:13 GMT
    // いくつもの引数を out で指定して nImage.GetImageInfo を呼ぶのが面倒だったので、
    // 単一のインスタンスに結果が全て入り、Display* なども利用できるクラスを用意した

    public class nImageInfo
    {
        public nImageFormat Format { get; set; }

        public bool IsAnimated { get; set; }

        public int Orientation { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        private void iSetDisplayWidthAndHeight ()
        {
            int xWidth = Width,
                xHeight = Height;

            nImage.ApplyOrientation (ref xWidth, ref xHeight, Orientation);
            mDisplayWidth = xWidth;
            mDisplayHeight = xHeight;
        }

        private int? mDisplayWidth = null;

        public int DisplayWidth
        {
            get
            {
                if (mDisplayWidth == null)
                    iSetDisplayWidthAndHeight ();

                return mDisplayWidth.Value;
            }
        }

        private int? mDisplayHeight = null;

        public int DisplayHeight
        {
            get
            {
                if (mDisplayHeight == null)
                    iSetDisplayWidthAndHeight ();

                return mDisplayHeight.Value;
            }
        }

        // Wed, 07 Nov 2018 20:10:57 GMT
        // 画像関連のプログラミングでは、変数が多くなってしんどくなってくることが多い
        // そういうのを少しでも回避するため、より少ない変数で呼べるメソッドを少しでも用意していく

        public void GetFitSize (int maxWidth, int maxHeight, bool enlarges, out int fitWidth, out int fitHeight, out bool isEnlarged) =>
            nImage.GetFitSize (maxWidth, maxHeight, DisplayWidth, DisplayHeight, enlarges, out fitWidth, out fitHeight, out isEnlarged);

        // Wed, 07 Nov 2018 21:22:14 GMT
        // GetFitSize では Display* を使っているが、こちらは縦横が同じなのでそうしなくていい
        // これらは初期化の時点で設定されるプロパティーなので、この方が計算を節約できる

        public int GetResizedImageCount (int maxCount) =>
            nManagedFileUtility.GetResizedImageCount (Width, Height, maxCount);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Nekote
{
    public class nManagedImage
    {
        // Wed, 07 Nov 2018 21:24:56 GMT
        // MapPath などに必要なので、サクッとキャッシュしておく

        public nManagedFile OriginalFile { get; private set; }

        public string RelativePath { get; private set; }

        private string mPath = null;

        public string Path
        {
            get
            {
                if (mPath == null)
                    mPath = OriginalFile.MapPathToParentDirectory (RelativePath);

                return mPath;
            }
        }

        // Wed, 07 Nov 2018 21:27:03 GMT
        // ファイル名だけ取得するなど、いろいろと役に立つ

        private FileInfo mFileInfo = null;

        public FileInfo FileInfo
        {
            get
            {
                if (mFileInfo == null)
                    mFileInfo = new FileInfo (Path);

                return mFileInfo;
            }
        }

        private readonly string mRelativeUrl = null;

        public string RelativeUrl
        {
            get
            {
                if (mRelativeUrl == null)
                    throw new NotSupportedException ();

                return mRelativeUrl;
            }
        }

        // Wed, 07 Nov 2018 21:27:21 GMT
        // 縮小版の生成時に回転をかけるため、こちらには Display* のみ用意

        public int DisplayWidth { get; private set; }

        public int DisplayHeight { get; private set; }

        public nManagedImage (nManagedFile originalFile, string relativePath, int displayWidth, int displayHeight)
        {
            OriginalFile = originalFile;
            RelativePath = relativePath;
            DisplayWidth = displayWidth;
            DisplayHeight = displayHeight;
        }

        public void GetFitSize (int maxWidth, int maxHeight, bool enlarges, out int fitWidth, out int fitHeight, out bool isEnlarged) =>
            nImage.GetFitSize (maxWidth, maxHeight, DisplayWidth, DisplayHeight, enlarges, out fitWidth, out fitHeight, out isEnlarged);
    }
}

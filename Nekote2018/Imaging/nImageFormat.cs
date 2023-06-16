using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // .NET の ImageFormat はクラスであり、照合は GUID によって行われ、データベースへの出し入れに不都合がある
    // nImageFormat は、Nekote がサポートするフォーマットのみ含み、int やその他の形に変換可能で、今後の拡張性もある
    // https://msdn.microsoft.com/en-us/library/system.drawing.imaging.imageformat.aspx

    public enum nImageFormat
    {
        Bmp = 1,
        Gif = 2,
        Jpeg = 3,
        Png = 4,
        Tiff = 5,
    }
}

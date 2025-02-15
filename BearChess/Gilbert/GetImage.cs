using System.IO;
using System.Reflection;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.Gilbert
{
    internal class GetImage : IGetImage
    {
        public GetImage()
        {
            byte[] buffer = null;
            try
            {
                var assembly = GetType().GetTypeInfo().Assembly;
                
                using (var imageStream =
                    assembly.GetManifestResourceStream("www.SoLaNoSoft.com.BearChess.Engine.logo.jpg"))
                {
                    if (imageStream != null && imageStream.Length > 0)
                    {
                        using (var br = new BinaryReader(imageStream))
                        {
                            buffer = br.ReadBytes((int)imageStream.Length);
                        }
                    }
                }
                Image = buffer;
            }
            catch
            {
                // ignored
            }
        }
        
        public byte[] Image { get; }
    }
}

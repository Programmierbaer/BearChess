using System.Text;

namespace www.SoLaNoSoft.com.BearChess.BearChessCommunication
{
    public static class CRCConversions
    {

        public static byte AddOddPar(string c)
        {
            int result = Encoding.ASCII.GetBytes(c)[0];
            result = result & 127;
            int par = 1;
            for (int i = 0; i < 8; i++)
            {
                var bit = result & 1;
                result = result >> 1;
                par = par ^ bit;

            }

            if (par == 1)
            {
                result = Encoding.ASCII.GetBytes(c)[0] | 128;
            }
            else
            {
                result = Encoding.ASCII.GetBytes(c)[0] & 127;
            }
            return (byte)result;
        }

        public static string AddBlockCrc(string message)
        {
            var bytes = Encoding.ASCII.GetBytes(message);
            var gpar = 0;
            for (var i = 0; i < bytes.Length; i++)
            {
                gpar ^= bytes[i];
            }

            return message + gpar.ToString("X2");
        }
    }
}

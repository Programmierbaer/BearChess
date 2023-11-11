using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Timers;

namespace www.SoLaNoSoft.com.BearChessBase
{
    public static class MoveLineHelper
    {
        public static T Swap<T>(this T x, ref T y)
        {
            T t = y;
            y = x;
            return t;
        }


        public static string[] GetMoveLine(string fieldNameFrom, string fieldNameTo)
        {
            if (fieldNameFrom.CompareTo(fieldNameTo)>0)
            {
                fieldNameFrom = fieldNameFrom.Swap(ref fieldNameTo);
            }
            List<string> moveLine = new List<string>();
            moveLine.Add(fieldNameFrom);
            moveLine.Add(fieldNameTo);

            if (!fieldNameFrom[0].Equals(fieldNameTo[0]) && (fieldNameFrom[1].Equals(fieldNameTo[1])))
            {
                // e.G A1 H1
                int f = (int)fieldNameFrom[0];
                int t = (int)fieldNameTo[0];
                if (f > t)
                {
                    f = f.Swap(ref t);
                }

                f++;
                for (int i = f; i < t; i++)
                {
                    moveLine.Add($"{(char)(i)}{fieldNameFrom[1]}");
                }
                return moveLine.ToArray();
            }
            if (fieldNameFrom[0].Equals(fieldNameTo[0]) && (!fieldNameFrom[1].Equals(fieldNameTo[1])))
            {
                // E.g. A1 A8
                int f = (int)fieldNameFrom[1];
                int t = (int)fieldNameTo[1];
                if (f > t)
                {
                    f = f.Swap(ref t);
                }

                f++;
                for (int i = f; i < t; i++)
                {
                    moveLine.Add($"{fieldNameFrom[0]}{(char)(i)}");
                }
                return moveLine.ToArray();
            }
            // A1 H8
            int f1 = (int)fieldNameFrom[0];
            int t1 = (int)fieldNameTo[0];
            int f2 = (int)fieldNameFrom[1];
            int t2 = (int)fieldNameTo[1];
            List<string> line1 = new List<string>();
            List<string> line2 = new List<string>();
            
            if (f1 > t1)
            {
                for (int i = f1-1; i > t1; i--)
                {
                    line1.Add($"{(char)(i)}");
                }
            }
            else
            {
                for (int i = f1+1; i < t1; i++)
                {
                    line1.Add($"{(char)(i)}");
                }
            }

            if (f2 > t2)
            {
                for (int i = f2 - 1; i > t2; i--)
                {
                    line2.Add($"{(char)(i)}");
                }
            }
            else
            {
                for (int i = f2+1; i < t2; i++)
                {
                    line2.Add($"{(char)(i)}");
                }
            }

            if (line1.Count == 0 && line2.Count > 0)
            {
                line1.Add(fieldNameFrom[0].ToString());
            }
            if (line2.Count == 0 && line1.Count > 0)
            {
                line2.Add(fieldNameTo[1].ToString());
            }
            int l = line1.Count > line2.Count ? line1.Count : line2.Count;
            for (int i = 0; i < l; i++)
            {
                if (i < line1.Count && i < line2.Count)
                {
                    moveLine.Add(line1[i] + line2[i]);
                }
            }

            return moveLine.ToArray();
        }
    }
}
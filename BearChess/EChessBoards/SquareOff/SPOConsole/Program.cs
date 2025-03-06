using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using www.SoLaNoSoft.com.BearChess.SquareOffChessBoard;

namespace SPOConsole
{
    class Program
    {
        private static void DrawBoard(string[] pieces)
        {
            var hs = new HashSet<string>(pieces);
            string fA1, fA2, fA3, fA4, fA5, fA6, fA7, fA8;
            string fB1, fB2, fB3, fB4, fB5, fB6, fB7, fB8;
            string fC1, fC2, fC3, fC4, fC5, fC6, fC7, fC8;
            string fD1, fD2, fD3, fD4, fD5, fD6, fD7, fD8;
            string fE1, fE2, fE3, fE4, fE5, fE6, fE7, fE8;
            string fF1, fF2, fF3, fF4, fF5, fF6, fF7, fF8;
            string fG1, fG2, fG3, fG4, fG5, fG6, fG7, fG8;
            string fH1, fH2, fH3, fH4, fH5, fH6, fH7, fH8;
            fA1 = hs.Contains("A1") ? "p" : " ";
            fA2 = hs.Contains("A2") ? "p" : " ";
            fA3 = hs.Contains("A3") ? "p" : " ";
            fA4 = hs.Contains("A4") ? "p" : " ";
            fA5 = hs.Contains("A5") ? "p" : " ";
            fA6 = hs.Contains("A6") ? "p" : " ";
            fA7 = hs.Contains("A7") ? "p" : " ";
            fA8 = hs.Contains("A8") ? "p" : " ";
            fB1 = hs.Contains("B1") ? "p" : " ";
            fB2 = hs.Contains("B2") ? "p" : " ";
            fB3 = hs.Contains("B3") ? "p" : " ";
            fB4 = hs.Contains("B4") ? "p" : " ";
            fB5 = hs.Contains("B5") ? "p" : " ";
            fB6 = hs.Contains("B6") ? "p" : " ";
            fB7 = hs.Contains("B7") ? "p" : " ";
            fB8 = hs.Contains("B8") ? "p" : " ";
            fC1 = hs.Contains("C1") ? "p" : " ";
            fC2 = hs.Contains("C2") ? "p" : " ";
            fC3 = hs.Contains("C3") ? "p" : " ";
            fC4 = hs.Contains("C4") ? "p" : " ";
            fC5 = hs.Contains("C5") ? "p" : " ";
            fC6 = hs.Contains("C6") ? "p" : " ";
            fC7 = hs.Contains("C7") ? "p" : " ";
            fC8 = hs.Contains("C8") ? "p" : " ";
            fD1 = hs.Contains("D1") ? "p" : " ";
            fD2 = hs.Contains("D2") ? "p" : " ";
            fD3 = hs.Contains("D3") ? "p" : " ";
            fD4 = hs.Contains("D4") ? "p" : " ";
            fD5 = hs.Contains("D5") ? "p" : " ";
            fD6 = hs.Contains("D6") ? "p" : " ";
            fD7 = hs.Contains("D7") ? "p" : " ";
            fD8 = hs.Contains("D8") ? "p" : " ";
            fE1 = hs.Contains("E1") ? "p" : " ";
            fE2 = hs.Contains("E2") ? "p" : " ";
            fE3 = hs.Contains("E3") ? "p" : " ";
            fE4 = hs.Contains("E4") ? "p" : " ";
            fE5 = hs.Contains("E5") ? "p" : " ";
            fE6 = hs.Contains("E6") ? "p" : " ";
            fE7 = hs.Contains("E7") ? "p" : " ";
            fE8 = hs.Contains("E8") ? "p" : " ";
            fF1 = hs.Contains("F1") ? "p" : " ";
            fF2 = hs.Contains("F2") ? "p" : " ";
            fF3 = hs.Contains("F3") ? "p" : " ";
            fF4 = hs.Contains("F4") ? "p" : " ";
            fF5 = hs.Contains("F5") ? "p" : " ";
            fF6 = hs.Contains("F6") ? "p" : " ";
            fF7 = hs.Contains("F7") ? "p" : " ";
            fF8 = hs.Contains("F8") ? "p" : " ";
            fG1 = hs.Contains("G1") ? "p" : " ";
            fG2 = hs.Contains("G2") ? "p" : " ";
            fG3 = hs.Contains("G3") ? "p" : " ";
            fG4 = hs.Contains("G4") ? "p" : " ";
            fG5 = hs.Contains("G5") ? "p" : " ";
            fG6 = hs.Contains("G6") ? "p" : " ";
            fG7 = hs.Contains("G7") ? "p" : " ";
            fG8 = hs.Contains("G8") ? "p" : " ";
            fH1 = hs.Contains("H1") ? "p" : " ";
            fH2 = hs.Contains("H2") ? "p" : " ";
            fH3 = hs.Contains("H3") ? "p" : " ";
            fH4 = hs.Contains("H4") ? "p" : " ";
            fH5 = hs.Contains("H5") ? "p" : " ";
            fH6 = hs.Contains("H6") ? "p" : " ";
            fH7 = hs.Contains("H7") ? "p" : " ";
            fH8 = hs.Contains("H8") ? "p" : " ";
            Console.WriteLine("+---+---+---+---+---+---+---+---+");
            Console.WriteLine($"! {fA8} ! {fB8} ! {fC8} ! {fD8} ! {fE8} ! {fF8} ! {fG8} ! {fH8} ! 8");
            Console.WriteLine($"! {fA7} ! {fB7} ! {fC7} ! {fD7} ! {fE7} ! {fF7} ! {fG7} ! {fH7} ! 7");
            Console.WriteLine($"! {fA6} ! {fB6} ! {fC6} ! {fD6} ! {fE6} ! {fF6} ! {fG6} ! {fH6} ! 6");
            Console.WriteLine($"! {fA5} ! {fB5} ! {fC5} ! {fD5} ! {fE5} ! {fF5} ! {fG5} ! {fH5} ! 5");
            Console.WriteLine($"! {fA4} ! {fB4} ! {fC4} ! {fD4} ! {fE4} ! {fF4} ! {fG4} ! {fH4} ! 4");
            Console.WriteLine($"! {fA3} ! {fB3} ! {fC3} ! {fD3} ! {fE3} ! {fF3} ! {fG3} ! {fH3} ! 3");
            Console.WriteLine($"! {fA2} ! {fB2} ! {fC2} ! {fD2} ! {fE2} ! {fF2} ! {fG2} ! {fH2} ! 2");
            Console.WriteLine($"! {fA1} ! {fB1} ! {fC1} ! {fD1} ! {fE1} ! {fF1} ! {fG1} ! {fH1} ! 1");
            Console.WriteLine("+---+---+---+---+---+---+---+---+");
            Console.WriteLine("  A   B   C   D   E   F   G   H  ");
        }

        public static void Main(string[] args)
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (line != null && line.Equals("x"))
                {
                    break;
                }
                var bc = new BoardCodeConverter(line);                
                Console.WriteLine(string.Join(" ", bc.GetFieldsWithPieces()));
                Console.WriteLine();
                DrawBoard(bc.GetFieldsWithPieces());
            }
        }
    }
}

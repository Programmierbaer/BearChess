using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable InconsistentNaming

namespace www.SoLaNoSoft.com.BearChessBase.Definitions
{
    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }

    public static class Fields
    {
        public enum Lines
        {
            A,
            B,
            C,
            D,
            E,
            F,
            G,
            H
        }

        public const int MAX_FIELD = 120;

        public const int FA1 = 21;
        public const int FB1 = 22;
        public const int FC1 = 23;
        public const int FD1 = 24;
        public const int FE1 = 25;
        public const int FF1 = 26;
        public const int FG1 = 27;
        public const int FH1 = 28;
        public const int FA2 = 31;
        public const int FB2 = 32;
        public const int FC2 = 33;
        public const int FD2 = 34;
        public const int FE2 = 35;
        public const int FF2 = 36;
        public const int FG2 = 37;
        public const int FH2 = 38;
        public const int FA3 = 41;
        public const int FB3 = 42;
        public const int FC3 = 43;
        public const int FD3 = 44;
        public const int FE3 = 45;
        public const int FF3 = 46;
        public const int FG3 = 47;
        public const int FH3 = 48;
        public const int FA4 = 51;
        public const int FB4 = 52;
        public const int FC4 = 53;
        public const int FD4 = 54;
        public const int FE4 = 55;
        public const int FF4 = 56;
        public const int FG4 = 57;
        public const int FH4 = 58;
        public const int FA5 = 61;
        public const int FB5 = 62;
        public const int FC5 = 63;
        public const int FD5 = 64;
        public const int FE5 = 65;
        public const int FF5 = 66;
        public const int FG5 = 67;
        public const int FH5 = 68;
        public const int FA6 = 71;
        public const int FB6 = 72;
        public const int FC6 = 73;
        public const int FD6 = 74;
        public const int FE6 = 75;
        public const int FF6 = 76;
        public const int FG6 = 77;
        public const int FH6 = 78;
        public const int FA7 = 81;
        public const int FB7 = 82;
        public const int FC7 = 83;
        public const int FD7 = 84;
        public const int FE7 = 85;
        public const int FF7 = 86;
        public const int FG7 = 87;
        public const int FH7 = 88;
        public const int FA8 = 91;
        public const int FB8 = 92;
        public const int FC8 = 93;
        public const int FD8 = 94;
        public const int FE8 = 95;
        public const int FF8 = 96;
        public const int FG8 = 97;
        public const int FH8 = 98;

        public const int COLOR_WHITE = 1;
        public const int COLOR_BLACK = 0;
        public const int COLOR_EMPTY = -1;
        public const int COLOR_OUTSIDE = -2;

        public static readonly int[] OutsideFields =
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 29, 30, 39, 40, 49, 50, 59, 60,
            69, 70, 79, 80, 89, 90, 99, 100,
            101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114,
            115, 116, 117, 118, 119
        };

        public static readonly int[] BoardFields =
        {
            21, 22, 23, 24, 25, 26, 27, 28, 31, 32, 33, 34, 35, 36, 37, 38, 41, 42, 43, 44, 45, 46, 47, 48, 51, 52, 53,
            54, 55, 56, 57, 58, 61, 62, 63, 64, 65,
            66, 67, 68, 71, 72, 73, 74, 75, 76, 77, 78,
            81, 82, 83, 84, 85, 86, 87, 88, 91, 92, 93, 94, 95, 96, 97, 98
        };

        public static readonly HashSet<int> WhiteFields = new HashSet<int> { FB1,FD1,FF1,FH1,
                                                                             FA2,FC2,FE2,FG2,
                                                                             FB3,FD3,FF3,FH3,
                                                                             FA4,FC4,FE4,FG4,
                                                                             FB5,FD5,FF5,FH5,
                                                                             FA6,FC6,FE6,FG6,
                                                                             FB7,FD7,FF7,FH7,
                                                                             FA8,FC8,FE8,FG8,
        };

        private static readonly HashSet<int> LineA = new HashSet<int> { FA1, FA2, FA3, FA4, FA5, FA6, FA7, FA8 };
        private static readonly HashSet<int> LineB = new HashSet<int> { FB1, FB2, FB3, FB4, FB5, FB6, FB7, FB8 };
        private static readonly HashSet<int> LineC = new HashSet<int> { FC1, FC2, FC3, FC4, FC5, FC6, FC7, FC8 };
        private static readonly HashSet<int> LineD = new HashSet<int> { FD1, FD2, FD3, FD4, FD5, FD6, FD7, FD8 };
        private static readonly HashSet<int> LineE = new HashSet<int> { FE1, FE2, FE3, FE4, FE5, FE6, FE7, FE8 };
        private static readonly HashSet<int> LineF = new HashSet<int> { FF1, FF2, FF3, FF4, FF5, FF6, FF7, FF8 };
        private static readonly HashSet<int> LineG = new HashSet<int> { FG1, FG2, FG3, FG4, FG5, FG6, FG7, FG8 };
        private static readonly HashSet<int> LineH = new HashSet<int> { FH1, FH2, FH3, FH4, FH5, FH6, FH7, FH8 };

        private static readonly HashSet<int> Row1 = new HashSet<int> { FA1, FB1, FC1, FD1, FE1, FF1, FG1, FH1 };
        private static readonly HashSet<int> Row2 = new HashSet<int> { FA2, FB2, FC2, FD2, FE2, FF2, FG2, FH2 };
        private static readonly HashSet<int> Row3 = new HashSet<int> { FA3, FB3, FC3, FD3, FE3, FF3, FG3, FH3 };
        private static readonly HashSet<int> Row4 = new HashSet<int> { FA4, FB4, FC4, FD4, FE4, FF4, FG4, FH4 };
        private static readonly HashSet<int> Row5 = new HashSet<int> { FA5, FB5, FC5, FD5, FE5, FF5, FG5, FH5 };
        private static readonly HashSet<int> Row6 = new HashSet<int> { FA6, FB6, FC6, FD6, FE6, FF6, FG6, FH6 };
        private static readonly HashSet<int> Row7 = new HashSet<int> { FA7, FB7, FC7, FD7, FE7, FF7, FG7, FH7 };
        private static readonly HashSet<int> Row8 = new HashSet<int> { FA8, FB8, FC8, FD8, FE8, FF8, FG8, FH8 };

        public static readonly Dictionary<int, string> ColorToName = new Dictionary<int, string>
                                                                         {
                                                                             { COLOR_WHITE, "white" },
                                                                             { COLOR_BLACK, "black" },
                                                                             { COLOR_EMPTY, "empty" },
                                                                             { COLOR_OUTSIDE, "outside" }
                                                                         };
        public static readonly Dictionary<int, Lines> FieldToLines = new Dictionary<int, Lines>
                                                                     {
                                                                         {FA1, Lines.A},
                                                                         {FA2, Lines.A},
                                                                         {FA3, Lines.A},
                                                                         {FA4, Lines.A},
                                                                         {FA5, Lines.A},
                                                                         {FA6, Lines.A},
                                                                         {FA7, Lines.A},
                                                                         {FA8, Lines.A},
                                                                         {FB1, Lines.B},
                                                                         {FB2, Lines.B},
                                                                         {FB3, Lines.B},
                                                                         {FB4, Lines.B},
                                                                         {FB5, Lines.B},
                                                                         {FB6, Lines.B},
                                                                         {FB7, Lines.B},
                                                                         {FB8, Lines.B},
                                                                         {FC1, Lines.C},
                                                                         {FC2, Lines.C},
                                                                         {FC3, Lines.C},
                                                                         {FC4, Lines.C},
                                                                         {FC5, Lines.C},
                                                                         {FC6, Lines.C},
                                                                         {FC7, Lines.C},
                                                                         {FC8, Lines.C},
                                                                         {FD1, Lines.D},
                                                                         {FD2, Lines.D},
                                                                         {FD3, Lines.D},
                                                                         {FD4, Lines.D},
                                                                         {FD5, Lines.D},
                                                                         {FD6, Lines.D},
                                                                         {FD7, Lines.D},
                                                                         {FD8, Lines.D},
                                                                         {FE1, Lines.E},
                                                                         {FE2, Lines.E},
                                                                         {FE3, Lines.E},
                                                                         {FE4, Lines.E},
                                                                         {FE5, Lines.E},
                                                                         {FE6, Lines.E},
                                                                         {FE7, Lines.E},
                                                                         {FE8, Lines.E},
                                                                         {FF1, Lines.F},
                                                                         {FF2, Lines.F},
                                                                         {FF3, Lines.F},
                                                                         {FF4, Lines.F},
                                                                         {FF5, Lines.F},
                                                                         {FF6, Lines.F},
                                                                         {FF7, Lines.F},
                                                                         {FF8, Lines.F},
                                                                         {FG1, Lines.G},
                                                                         {FG2, Lines.G},
                                                                         {FG3, Lines.G},
                                                                         {FG4, Lines.G},
                                                                         {FG5, Lines.G},
                                                                         {FG6, Lines.G},
                                                                         {FG7, Lines.G},
                                                                         {FG8, Lines.G},
                                                                         {FH1, Lines.H},
                                                                         {FH2, Lines.H},
                                                                         {FH3, Lines.H},
                                                                         {FH4, Lines.H},
                                                                         {FH5, Lines.H},
                                                                         {FH6, Lines.H},
                                                                         {FH7, Lines.H},
                                                                         {FH8, Lines.H}
                                                                     };

        private static readonly Dictionary<string, string> AdpatedFieldName = new Dictionary<string, string>
        {
            {"A1","H8"},
            {"B1","G8"},
            {"C1","F8"},
            {"D1","E8"},
            {"E1","D8"},
            {"F1","C8"},
            {"G1","B8"},
            {"H1","A8"},
            {"A2","H7"},
            {"B2","G7"},
            {"C2","F7"},
            {"D2","E7"},
            {"E2","D7"},
            {"F2","C7"},
            {"G2","B7"},
            {"H2","A7"},
            {"A3","H6"},
            {"B3","G6"},
            {"C3","F6"},
            {"D3","E6"},
            {"E3","D6"},
            {"F3","C6"},
            {"G3","B6"},
            {"H3","A6"},
            {"A4","H5"},
            {"B4","G5"},
            {"C4","F5"},
            {"D4","E5"},
            {"E4","D5"},
            {"F4","C5"},
            {"G4","B5"},
            {"H4","A5"},
            {"A5","H4"},
            {"B5","G4"},
            {"C5","F4"},
            {"D5","E4"},
            {"E5","D4"},
            {"F5","C4"},
            {"G5","B4"},
            {"H5","A4"},
            {"A6","H3"},
            {"B6","G3"},
            {"C6","F3"},
            {"D6","E3"},
            {"E6","D3"},
            {"F6","C3"},
            {"G6","B3"},
            {"H6","A3"},
            {"A7","H2"},
            {"B7","G2"},
            {"C7","F2"},
            {"D7","E2"},
            {"E7","D2"},
            {"F7","C2"},
            {"G7","B2"},
            {"H7","A2"},
            {"A8","H1"},
            {"B8","G1"},
            {"C8","F1"},
            {"D8","E1"},
            {"E8","D1"},
            {"F8","C1"},
            {"G8","B1"},
            {"H8","A1"},
        };
        public static readonly Dictionary<int, int> FieldToRow = new Dictionary<int, int>
                                                                  {
                                                                      {FA1, 1},
                                                                      {FB1, 1},
                                                                      {FC1, 1},
                                                                      {FD1, 1},
                                                                      {FE1, 1},
                                                                      {FF1, 1},
                                                                      {FG1, 1},
                                                                      {FH1, 1},
                                                                      {FA2, 2},
                                                                      {FB2, 2},
                                                                      {FC2, 2},
                                                                      {FD2, 2},
                                                                      {FE2, 2},
                                                                      {FF2, 2},
                                                                      {FG2, 2},
                                                                      {FH2, 2},
                                                                      {FA3, 3},
                                                                      {FB3, 3},
                                                                      {FC3, 3},
                                                                      {FD3, 3},
                                                                      {FE3, 3},
                                                                      {FF3, 3},
                                                                      {FG3, 3},
                                                                      {FH3, 3},
                                                                      {FA4, 4},
                                                                      {FB4, 4},
                                                                      {FC4, 4},
                                                                      {FD4, 4},
                                                                      {FE4, 4},
                                                                      {FF4, 4},
                                                                      {FG4, 4},
                                                                      {FH4, 4},
                                                                      {FA5, 5},
                                                                      {FB5, 5},
                                                                      {FC5, 5},
                                                                      {FD5, 5},
                                                                      {FE5, 5},
                                                                      {FF5, 5},
                                                                      {FG5, 5},
                                                                      {FH5, 5},
                                                                      {FA6, 6},
                                                                      {FB6, 6},
                                                                      {FC6, 6},
                                                                      {FD6, 6},
                                                                      {FE6, 6},
                                                                      {FF6, 6},
                                                                      {FG6, 6},
                                                                      {FH6, 6},
                                                                      {FA7, 7},
                                                                      {FB7, 7},
                                                                      {FC7, 7},
                                                                      {FD7, 7},
                                                                      {FE7, 7},
                                                                      {FF7, 7},
                                                                      {FG7, 7},
                                                                      {FH7, 7},
                                                                      {FA8, 8},
                                                                      {FB8, 8},
                                                                      {FC8, 8},
                                                                      {FD8, 8},
                                                                      {FE8, 8},
                                                                      {FF8, 8},
                                                                      {FG8, 8},
                                                                      {FH8, 8}
                                                                  };

        private static readonly Dictionary<string, string> BlindFieldNames = new Dictionary<string, string>
        {
             {"A","Anna"},
             {"B","Bella"},
             {"C","Cesar"},
             {"D","David"},
             {"E","Eva"},
             {"F","Felix"},
             {"G","Gustav"},
             {"H","Hector"},
        };
        private static readonly Dictionary<string, string> BlindFieldRankDe = new Dictionary<string, string>
        {
            {"1","eins"},
            {"2","zwei"},
            {"3","drei"},
            {"4","vier"},
            {"5","funf"},
            {"6","sechs"},
            {"7","sieben"},
            {"8","acht"},
        };

        private static readonly Dictionary<string, string> BlindFieldRank = new Dictionary<string, string>
        {
            {"1","1"},
            {"2","2"},
            {"3","3"},
            {"4","4"},
            {"5","5"},
            {"6","6"},
            {"7","7"},
            {"8","8"},
        };

        public static Lines GetLine(int field)
        {
            return FieldToLines[field];
        }

        public static int GetRow(int field)
        {
            return FieldToRow[field];
        }

        public static int[] RowFields(int row)
        {
            switch (row)
            {
                case 1:
                    return Row1.ToArray();
                case 2:
                    return Row2.ToArray();
                case 3:
                    return Row3.ToArray();
                case 4:
                    return Row4.ToArray();
                case 5:
                    return Row5.ToArray();
                case 6:
                    return Row6.ToArray();
                case 7:
                    return Row7.ToArray();
                case 8:
                    return Row8.ToArray();
                default:
                    return Array.Empty<int>();
            }
        }

        public static bool InRow(int row, int field)
        {
            switch (row)
            {
                case 1:
                    return Row1.Contains(field);
                case 2:
                    return Row2.Contains(field);
                case 3:
                    return Row3.Contains(field);
                case 4:
                    return Row4.Contains(field);
                case 5:
                    return Row5.Contains(field);
                case 6:
                    return Row6.Contains(field);
                case 7:
                    return Row7.Contains(field);
                case 8:
                    return Row8.Contains(field);
                default:
                    return false;
            }
        }

        public static bool InLine(Lines line, int field)
        {
            switch (line)
            {
                case Lines.A:
                    return LineA.Contains(field);
                case Lines.B:
                    return LineB.Contains(field);
                case Lines.C:
                    return LineC.Contains(field);
                case Lines.D:
                    return LineD.Contains(field);
                case Lines.E:
                    return LineE.Contains(field);
                case Lines.F:
                    return LineF.Contains(field);
                case Lines.G:
                    return LineG.Contains(field);
                case Lines.H:
                    return LineH.Contains(field);
                default:
                    return false;
            }
        }

        public static int[] LineFields(Lines line)
        {
            switch (line)
            {
                case Lines.A:
                    return LineA.ToArray();
                case Lines.B:
                    return LineB.ToArray();
                case Lines.C:
                    return LineC.ToArray();
                case Lines.D:
                    return LineD.ToArray();
                case Lines.E:
                    return LineE.ToArray();
                case Lines.F:
                    return LineF.ToArray();
                case Lines.G:
                    return LineG.ToArray();
                case Lines.H:
                    return LineH.ToArray();
                default:
                    return Array.Empty<int>();
            }
        }

        public static Lines[] NeighbourLines(Lines line)
        {
            switch (line)
            {
                case Lines.A:
                    return new[] { Lines.B };
                case Lines.B:
                    return new[] { Lines.A, Lines.C };
                case Lines.C:
                    return new[] { Lines.B, Lines.D };
                case Lines.D:
                    return new[] { Lines.C, Lines.E };
                case Lines.E:
                    return new[] { Lines.D, Lines.F };
                case Lines.F:
                    return new[] { Lines.E, Lines.G };
                case Lines.G:
                    return new[] { Lines.F, Lines.H };
                case Lines.H:
                    return new[] { Lines.G };
                default:
                    return Array.Empty<Lines>();
            }
        }

        public static string GetBlindFieldName(string fieldName)
        {
            if (!Configuration.Instance.GetBoolValue("blindUserSayFideRules", true))
            {
                return fieldName;
            }
            string row, col;
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return string.Empty;
            }
            if (fieldName.Length > 1)
            {
                row = fieldName[0].ToString().ToUpper();
                col = fieldName[1].ToString().ToUpper();
            }
            else
            {
                row = fieldName.ToUpper();
                col = string.Empty;
            }

            BlindFieldRank.TryGetValue(col, out  col);
            return BlindFieldNames.TryGetValue(row, out var name) ? $"{name} {col}" : fieldName;
        }

        public static string GetFieldName(int field)
        {
            string[] row = { "", "A", "B", "C", "D", "E", "F", "G", "H" };
            var zehner = field / 10;
            var einer = field - zehner * 10;
            zehner--;
            return row[einer] + zehner;
        }

      

        public static string GetAdaptedFieldName(string fieldName, bool playingWhite)
        {
            if (playingWhite)
            {
                return fieldName;
            }

            return AdpatedFieldName[fieldName];
        }

        public static int GetFieldNumber(string field)
        {
            if (string.IsNullOrWhiteSpace(field) || field == "-" )
            {
                return Fields.COLOR_EMPTY;
            }

            try
            {
                const string row = " ABCDEFGH";
                var s = Convert.ToInt32(field.Substring(1, 1)) * 10 + 10;
                var r = row.IndexOf(field.Substring(0, 1), StringComparison.OrdinalIgnoreCase);
                if (r < 0)
                {
                    return Fields.COLOR_EMPTY;
                }
                return s + r;
            }
            catch
            {
                return Fields.COLOR_EMPTY;
            }
        }
    }
}

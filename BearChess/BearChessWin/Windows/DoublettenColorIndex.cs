using System.Collections.Generic;
using System.Windows.Media;

namespace www.SoLaNoSoft.com.BearChessWin
{
    public static class DoublettenColorIndex
    {
        public static readonly Dictionary<int, SolidColorBrush> colorIndex = new Dictionary<int, SolidColorBrush>()
                                                                             {
                                                                                 {0, new SolidColorBrush(Colors.LightBlue)},
                                                                                 {1, new SolidColorBrush(Colors.LightGreen)},
                                                                                 {2, new SolidColorBrush(Colors.LightSalmon)},
                                                                                 {3, new SolidColorBrush(Colors.CadetBlue)},
                                                                                 {4, new SolidColorBrush(Colors.DarkSeaGreen)},
                                                                                 {5, new SolidColorBrush(Colors.Gold)},
                                                                                 {6, new SolidColorBrush(Colors.CornflowerBlue)}
                                                                             };

        public static SolidColorBrush GetColorOfIndex(int index)
        {
            if (index >= colorIndex.Keys.Count)
            {
                index = 0;
            }
            return colorIndex[index];
        }
    }
}

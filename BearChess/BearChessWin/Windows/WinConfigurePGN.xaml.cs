using System.Windows;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für WinConfigurePGN.xaml
    /// </summary>
    public partial class WinConfigurePGN : Window
    {
        private readonly Configuration _configuration;

        public WinConfigurePGN(Configuration configuration)
        {
            _configuration = configuration;
            InitializeComponent();
            var pgnConfiguration = configuration.GetPgnConfiguration();
            CheckBoxPurePGN.IsChecked = pgnConfiguration.PurePgn;
            CheckBoxComment.IsChecked = pgnConfiguration.IncludeComment;
            CheckBoxEvaluation.IsChecked = pgnConfiguration.IncludeEvaluation;
            CheckBoxMoveTime.IsChecked = pgnConfiguration.IncludeMoveTime;
            CheckBoxSymbols.IsChecked = pgnConfiguration.IncludeSymbols;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            _configuration.SavePgnConfiguration(new PgnConfiguration()
            {
                PurePgn = CheckBoxPurePGN.IsChecked.HasValue && CheckBoxPurePGN.IsChecked.Value,
                IncludeComment = CheckBoxComment.IsChecked.HasValue && CheckBoxComment.IsChecked.Value,
                IncludeEvaluation = CheckBoxEvaluation.IsChecked.HasValue && CheckBoxEvaluation.IsChecked.Value,
                IncludeMoveTime = CheckBoxMoveTime.IsChecked.HasValue && CheckBoxMoveTime.IsChecked.Value,
                IncludeSymbols = CheckBoxSymbols.IsChecked.HasValue && CheckBoxSymbols.IsChecked.Value,
            });
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CheckBoxPurePGN_OnChecked(object sender, RoutedEventArgs e)
        {
            GroupBoxInclude.IsEnabled = false;
        }

        private void CheckBoxPurePGN_OnUnchecked(object sender, RoutedEventArgs e)
        {
            GroupBoxInclude.IsEnabled = true;
            CheckBoxComment.IsChecked = true;
            CheckBoxEvaluation.IsChecked = true;
            CheckBoxMoveTime.IsChecked = true;
            CheckBoxSymbols.IsChecked = true;
        }
    }
}
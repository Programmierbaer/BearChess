using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    ///     Interaktionslogik für ParameterSelectionWindow.xaml
    /// </summary>
    public partial class ParameterSelectionWindow : Window
    {
        private readonly List<ParameterSelection> _parameterSelections = new List<ParameterSelection>();
        private List<ParameterSelection> _selections = new List<ParameterSelection>();

        public ParameterSelectionWindow()
        {
            InitializeComponent();
            textBoxText.Clear();
            SelectedFile = string.Empty;
            buttonParameterFile.Visibility = Visibility.Collapsed;
        }

        public ParameterSelection SelectedEngine => _parameterSelections.First();

        public ParameterSelection[] SelectedEngines => _parameterSelections.ToArray();

        public bool SkipWarnings => checkBoxSkipWarning.IsChecked.HasValue && checkBoxSkipWarning.IsChecked.Value;

        public string SelectedFile { get; private set; }

        public void SetLabel(string label)
        {
            textBlockText.Text = label;
        }

        public void SetMultiSelectionMode()
        {
            listBoxEngines.SelectionMode = SelectionMode.Extended;
            textBoxFilter.Visibility = Visibility.Visible;
            textBlockFilter.Visibility = Visibility.Visible;
        }

        public void ShowList(string[] listValues)
        {
            _selections.Clear();
          
            foreach (var value in listValues)
            {
                _selections.Add(new ParameterSelection(value));
            }
            BuildFilter();

           
        }

        private void BuildFilter()
        {
            _parameterSelections.Clear();
            if (textBoxFilter.Text.Trim().Length > 0)
            {

                listBoxEngines.ItemsSource = _selections
                                             .Where(f => f.ParameterDisplay.ToLower()
                                                          .Contains(textBoxFilter.Text.Trim().ToLower()))
                                             .OrderBy(s => s.ParameterDisplay);
            }
            else
            {
                listBoxEngines.ItemsSource = _selections.OrderBy(s => s.ParameterDisplay);
            }
            
            listBoxEngines.SelectedIndex = -1;
            listBoxEngines.SelectedIndex = 0;
        }

        public void ShowParameterButton(bool show)
        {
            buttonParameterFile.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ListBoxEngines_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _parameterSelections.Clear();
            foreach (var selectedItem in listBoxEngines.SelectedItems)
            {
                _parameterSelections.Add((ParameterSelection)selectedItem);
            }

            textBoxText.Text = _parameterSelections.Count>0 ?  _parameterSelections.First().ParameterDisplay : string.Empty;
            SelectedFile = string.Empty;
            if (!_parameterSelections.Any() && listBoxEngines.Items.Count>0)
            {
                _parameterSelections.Add((ParameterSelection)listBoxEngines.Items[0]);
            }
        }

        private void ButtonParameter_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "Avatar Engine|*.zip|All Files|*.*" };
            var showDialogAvatar = openFileDialog.ShowDialog(this);
            if (showDialogAvatar.Value && !string.IsNullOrWhiteSpace(openFileDialog.FileName))
            {
                SelectedFile = openFileDialog.FileName;
                if (openFileDialog.FileName.Contains(@"\"))
                {
                    textBoxText.Text = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf(@"\") + 1);
                    _parameterSelections.Clear();
                    _parameterSelections.Add(new ParameterSelection(textBoxText.Text));
                }
            }
        }

      

        private void TextBoxFilter_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            BuildFilter();
        }

        private void ListBoxEngines_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedEngines.Length>0)
            {
                DialogResult = true;
            }
        }
    }
}
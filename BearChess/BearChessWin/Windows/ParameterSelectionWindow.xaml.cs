using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace www.SoLaNoSoft.com.BearChessWin.Windows
{
    /// <summary>
    /// Interaktionslogik für ParameterSelectionWindow.xaml
    /// </summary>
    public partial class ParameterSelectionWindow : Window
    {

        public string SelectedEngine => textBoxText.Text;
        public string SelectedFile { get; private set; }

        public ParameterSelectionWindow()
        {
            InitializeComponent();
            textBoxText.Clear();
            SelectedFile = string.Empty;
            buttonParameterFile.Visibility = Visibility.Collapsed;
        }

        public void SetLabel(string label)
        {
            textBlockText.Text = label;
        }

        public void ShowList(string[] listValues)
        {
            listBoxEngines.ItemsSource = listValues;
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
            textBoxText.Text = listBoxEngines.SelectedItem.ToString();
            SelectedFile = string.Empty;
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
                }
            }
        }
    }
}

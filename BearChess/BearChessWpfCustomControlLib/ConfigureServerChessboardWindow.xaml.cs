using System;
using System.Collections.Generic;
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
using System.IO;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChess.EChessBoard;
using www.SoLaNoSoft.com.BearChess.CertaboLoader;
using www.SoLaNoSoft.com.BearChess.IChessOneLoader;
using www.SoLaNoSoft.com.BearChess.TabuTronic.Tactum.Loader;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChess.DGTLoader;
using www.SoLaNoSoft.com.BearChess.ChessnutAirLoader;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;
using www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.Loader;
using www.SoLaNoSoft.com.BearChess.Tabutronic.Sentio.Loader;

namespace www.SoLaNoSoft.com.BearChessWpfCustomControlLib
{
    /// <summary>
    /// Interaktionslogik für ConfigureServerChessboardWindow.xaml
    /// </summary>
    public partial class ConfigureServerChessboardWindow : Window
    {

        private string _configPathWhite;
        private string _configPathBlack;
        private bool _isInitialized = false;

        public string WhiteConnectionId { get; private set; }
        public string BlackConnectionId { get; private set; }

        public IElectronicChessBoard WhiteEboard { get; private set; }
        public IElectronicChessBoard BlackEboard { get; private set; }

        public ConfigureServerChessboardWindow(string serverBoardId, BearChessClientInformation[] bcClientToken)
        {
            InitializeComponent();
            var serverPath = Path.Combine(Configuration.Instance.FolderPath,"server");
            var configPath = Path.Combine(serverPath, "boards", serverBoardId);
            _configPathWhite = Path.Combine(configPath,"white");
            _configPathBlack = Path.Combine(configPath, "black");
            comboboxWhiteEBoardNames.Items.Clear();
            comboboxWhiteEBoardNames.Items.Add(Constants.Certabo);
            comboboxWhiteEBoardNames.Items.Add(Constants.ChessnutAir);
            comboboxWhiteEBoardNames.Items.Add(Constants.ChessnutEvo);            
            comboboxWhiteEBoardNames.Items.Add(Constants.DGT);
            comboboxWhiteEBoardNames.Items.Add(Constants.IChessOne);
            comboboxWhiteEBoardNames.Items.Add(Constants.MChessLink);
            comboboxWhiteEBoardNames.Items.Add(Constants.TabutronicCerno);
            comboboxWhiteEBoardNames.Items.Add(Constants.TabutronicSentio);
            comboboxWhiteEBoardNames.Items.Add(Constants.TabutronicTactum);
            comboboxBlackEBoardNames.Items.Clear();
            comboboxBlackEBoardNames.Items.Add(Constants.Certabo);
            comboboxBlackEBoardNames.Items.Add(Constants.ChessnutAir);
            comboboxBlackEBoardNames.Items.Add(Constants.ChessnutEvo);
            comboboxBlackEBoardNames.Items.Add(Constants.DGT);
            comboboxBlackEBoardNames.Items.Add(Constants.IChessOne);
            comboboxBlackEBoardNames.Items.Add(Constants.MChessLink);
            comboboxBlackEBoardNames.Items.Add(Constants.TabutronicCerno);
            comboboxBlackEBoardNames.Items.Add(Constants.TabutronicSentio);
            comboboxBlackEBoardNames.Items.Add(Constants.TabutronicTactum);
            for (int i=0; i< bcClientToken.Length; i++)
            {
                comboboxWhiteBCNames.Items.Add(bcClientToken[i]);
                comboboxBlackBCNames.Items.Add(bcClientToken[i]);
            }
            WhiteEboard = null;
            BlackEboard = null;
            WhiteConnectionId = string.Empty;
            BlackConnectionId = string.Empty;
            _isInitialized = true;
            CheckBoxSameConnection.IsChecked = true;
            radioButtonWhiteDirectConnected.IsChecked = true;
        }


        private void ConfigureBoardConnection(string selectedBoard, IElectronicChessBoard eBoard, string configPath, bool forWhite)
        {
            if (selectedBoard.Equals(Constants.IChessOne))
            {
                var configIChessOne = new ConfigureIChessOneWindow(Configuration.Instance, false, configPath);
                var dialogResult = configIChessOne.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (forWhite)
                        WhiteConnectionId = configIChessOne.SelectedPortName;
                    else
                        BlackConnectionId = configIChessOne.SelectedPortName;
                    eBoard = new IChessOneLoader(configPath);
                }
                return;
            }
            if (selectedBoard.Equals(Constants.Certabo))
            {
                var configCertabo = new ConfigureCertaboWindow(Configuration.Instance, false, false, false, configPath);
                var dialogResult = configCertabo.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {                    
                    if (forWhite)
                        WhiteConnectionId = configCertabo.SelectedPortName;
                    else
                        BlackConnectionId = configCertabo.SelectedPortName;
                    eBoard = new IChessOneLoader(configPath);
                }
                return;
            }
            if (selectedBoard.Equals(Constants.DGT))
            {
                var configDGT = new ConfigureDGTWindow(Configuration.Instance, false, configPath);
                var dialogResult = configDGT.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {                    
                    if (forWhite)
                        WhiteConnectionId = configDGT.SelectedPortName;
                    else
                        BlackConnectionId = configDGT.SelectedPortName;
                    eBoard = new DGTLoader(configPath);
                }
                return;
            }
            if (selectedBoard.Equals(Constants.ChessnutAir))
            {
                var configChessnut = new ConfigureChessnutWindow(Constants.ChessnutAir, Configuration.Instance, false, configPath);
                var dialogResult = configChessnut.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {                 
                    if (forWhite)
                        WhiteConnectionId = configChessnut.SelectedPortName;
                    else
                        BlackConnectionId = configChessnut.SelectedPortName;
                    eBoard = new ChessnutAirLoader(configPath);
                }
                return;
            }
            if (selectedBoard.Equals(Constants.ChessnutEvo))
            {
                var configEvo = new ConfigureChessnutEvoWindow(Configuration.Instance, configPath);
                var dialogResult = configEvo.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    WhiteConnectionId = configEvo.SelectedPortName;
                    if (forWhite)
                        WhiteConnectionId = configEvo.SelectedPortName;
                    else
                        BlackConnectionId = configEvo.SelectedPortName;
                    eBoard = new ChessnutEvoLoader(configPath);
                }
                return;
            }
            if (selectedBoard.Equals(Constants.MChessLink))
            {
                var configWindow = new ConfigureMChessLinkWindow(Configuration.Instance,false,false,false,false, configPath);
                var dialogResult = configWindow.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    WhiteConnectionId = configWindow.SelectedPortName;
                    if (forWhite)
                        WhiteConnectionId = configWindow.SelectedPortName;
                    else
                        BlackConnectionId = configWindow.SelectedPortName;
                    eBoard = new MChessLinkLoader(configPath);
                }
                return;
            }
            if (selectedBoard.Equals(Constants.TabutronicCerno))
            {
                var configWindow = new ConfigureCernoWindow(Configuration.Instance, false, false, configPath);
                var dialogResult = configWindow.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    WhiteConnectionId = configWindow.SelectedPortName;
                    if (forWhite)
                        WhiteConnectionId = configWindow.SelectedPortName;
                    else
                        BlackConnectionId = configWindow.SelectedPortName;
                    eBoard = new TabutronicCernoLoader(configPath);
                }
                return;
            }
            if (selectedBoard.Equals(Constants.TabutronicSentio))
            {
                var configWindow = new ConfigureSentioWindow(Configuration.Instance, false, false, configPath);
                var dialogResult = configWindow.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    WhiteConnectionId = configWindow.SelectedPortName;
                    if (forWhite)
                        WhiteConnectionId = configWindow.SelectedPortName;
                    else
                        BlackConnectionId = configWindow.SelectedPortName;
                    eBoard = new TabutronicSentioLoader(configPath);
                }
                return;
            }
            if (selectedBoard.Equals(Constants.TabutronicTactum))
            {
                var configWindow = new ConfigureTactumWindow(Configuration.Instance, false, configPath);
                var dialogResult = configWindow.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    WhiteConnectionId = configWindow.SelectedPortName;
                    if (forWhite)
                        WhiteConnectionId = configWindow.SelectedPortName;
                    else
                        BlackConnectionId = configWindow.SelectedPortName;
                    eBoard = new TabuTronicTactumLoader(configPath);
                }
                return;
            }
        }

        private void ButtonConfigureWhiteConnection_Click(object sender, RoutedEventArgs e)
        {
            if (comboboxWhiteEBoardNames.SelectedItem==null)
            {
                return;
            }
            var selectedBoard = comboboxWhiteEBoardNames.SelectedItem as string;
            ConfigureBoardConnection(selectedBoard,WhiteEboard,_configPathWhite, true);
            
        }

        private void ButtonConfigureBlackConnection_Click(object sender, RoutedEventArgs e)
        {
            if (comboboxBlackEBoardNames.SelectedItem == null)
            {
                return;
            }
            var selectedBoard = comboboxBlackEBoardNames.SelectedItem as string;
            ConfigureBoardConnection(selectedBoard, BlackEboard, _configPathBlack, false);
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (radioButtonWhiteConnectedViaBC.IsChecked.HasValue && radioButtonWhiteConnectedViaBC.IsChecked.Value)
            {
                WhiteConnectionId = (comboboxWhiteBCNames.SelectedItem as BearChessClientInformation).Address;
            }
            if (CheckBoxSameConnection.IsChecked.HasValue && CheckBoxSameConnection.IsChecked.Value)
            {
                BlackConnectionId = WhiteConnectionId;
            }
            else
            {
                if (radioButtonBlackConnectedViaBC.IsChecked.HasValue && radioButtonBlackConnectedViaBC.IsChecked.Value)
                {
                    BlackConnectionId = (comboboxBlackBCNames.SelectedItem as BearChessClientInformation).Address;
                }
            }
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult= false;
        }

        private void radioButtonWhiteDirectConnected_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                comboboxWhiteBCNames.IsEnabled = false;
                comboboxBlackEBoardNames.IsEnabled = true;
                ButtonConfigureWhiteConnection.IsEnabled = true;
            }
        }

        private void radioButtonWhiteConnectedViaBC_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                comboboxWhiteBCNames.IsEnabled = true;
                comboboxBlackEBoardNames.IsEnabled = false;
                ButtonConfigureWhiteConnection.IsEnabled = false;
            }
        }
    }
}

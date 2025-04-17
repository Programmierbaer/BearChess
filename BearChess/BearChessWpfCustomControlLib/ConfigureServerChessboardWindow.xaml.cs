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
using www.SoLaNoSoft.com.BearChess.HoSLoader;
using www.SoLaNoSoft.com.BearChess.MChessLinkLoader;
using www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.Loader;
using www.SoLaNoSoft.com.BearChess.Tabutronic.Sentio.Loader;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

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
        private readonly ILogging _logging;

        public string WhiteConnectionId { get; private set; }
        public string BlackConnectionId { get; private set; }

        public IElectronicChessBoard WhiteEboard { get; private set; }
        public IElectronicChessBoard BlackEboard { get; private set; }

        public ConfigureServerChessboardWindow(string serverBoardId, BearChessClientInformation[] bcClientToken, ILogging logging)
        {
            InitializeComponent();
            var serverPath = Path.Combine(Configuration.Instance.FolderPath,"server");
            var configPath = Path.Combine(serverPath, "boards", serverBoardId);
            _logging = logging;
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
            comboboxWhiteEBoardNames.Items.Add(Constants.TabutronicCernoSpectrum);
            comboboxWhiteEBoardNames.Items.Add(Constants.TabutronicSentio);
            comboboxWhiteEBoardNames.Items.Add(Constants.TabutronicTactum);
            comboboxWhiteEBoardNames.Items.Add(Constants.Zmartfun);
            comboboxBlackEBoardNames.Items.Clear();
            comboboxBlackEBoardNames.Items.Add(Constants.Certabo);
            comboboxBlackEBoardNames.Items.Add(Constants.ChessnutAir);
            comboboxBlackEBoardNames.Items.Add(Constants.ChessnutEvo);
            comboboxBlackEBoardNames.Items.Add(Constants.DGT);
            comboboxBlackEBoardNames.Items.Add(Constants.IChessOne);
            comboboxBlackEBoardNames.Items.Add(Constants.MChessLink);
            comboboxBlackEBoardNames.Items.Add(Constants.TabutronicCerno);
            comboboxBlackEBoardNames.Items.Add(Constants.TabutronicCernoSpectrum);
            comboboxBlackEBoardNames.Items.Add(Constants.TabutronicSentio);
            comboboxBlackEBoardNames.Items.Add(Constants.TabutronicTactum);
            comboboxBlackEBoardNames.Items.Add(Constants.Zmartfun);
            for (int i=0; i< bcClientToken.Length; i++)
            {
                comboboxWhiteBCNames.Items.Add(bcClientToken[i]);
                comboboxBlackBCNames.Items.Add(bcClientToken[i]);
            }
            WhiteEboard = null;
            BlackEboard = null;
            WhiteConnectionId = string.Empty;
            BlackConnectionId = string.Empty;
            comboboxWhiteBCNames.IsEnabled = false;
            comboboxBlackEBoardNames.IsEnabled = true;
            ButtonConfigureWhiteConnection.IsEnabled = true;
            CheckBoxSameConnection.IsChecked = true;
            radioButtonWhiteDirectConnected.IsChecked = true;
            BorderBlack.IsEnabled = false;
            _isInitialized = true;
        }


        private bool ConfigureBoardConnection(string selectedBoard, string configPath, bool forWhite)
        {
            _logging.LogDebug($"Configure {selectedBoard}");
            if (selectedBoard.Equals(Constants.IChessOne))
            { 
                var configIChessOne = new ConfigureIChessOneWindow(Configuration.Instance, false, configPath) { Owner = this};
                var dialogResult = configIChessOne.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (forWhite)
                    {
                        
                        WhiteEboard = new IChessOneLoader(configPath);
                        WhiteEboard.Identification = Guid.NewGuid().ToString("N");
                        WhiteConnectionId = WhiteEboard.Identification;
                    }
                    else
                    {
                   
                        BlackEboard = new IChessOneLoader(configPath);
                        BlackEboard.Identification = Guid.NewGuid().ToString("N");
                        BlackConnectionId = BlackEboard.Identification;
                    }
                    
                    return true;
                }

                return false;
            }

            if (selectedBoard.Equals(Constants.Certabo))
            {
                var configCertabo = new ConfigureCertaboWindow(Configuration.Instance, false, false, false, configPath) { Owner = this };
                var dialogResult = configCertabo.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (forWhite)
                    {
                        WhiteEboard = new CertaboLoader(configPath);
                        WhiteEboard.Identification = Guid.NewGuid().ToString("N");
                        WhiteConnectionId = WhiteEboard.Identification;
                    }
                    else
                    {
                        BlackEboard = new CertaboLoader(configPath);
                        BlackEboard.Identification = Guid.NewGuid().ToString("N");
                        BlackConnectionId = BlackEboard.Identification;
                    }

                    return true;
                }

                return false;
            }

            if (selectedBoard.Equals(Constants.DGT))
            {
                var configDGT = new ConfigureDGTWindow(Configuration.Instance, false, configPath) { Owner = this };
                var dialogResult = configDGT.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (forWhite)
                    {
                        
                        WhiteEboard = new DGTLoader(configPath);
                        WhiteEboard.Identification = Guid.NewGuid().ToString("N");
                        WhiteConnectionId = WhiteEboard.Identification;
                    }
                    else
                    {
                        BlackEboard = new DGTLoader(configPath);
                        BlackEboard.Identification = Guid.NewGuid().ToString("N");
                        BlackConnectionId = BlackEboard.Identification;
                    }

                    return true;

                }

                return false;
            }

            if (selectedBoard.Equals(Constants.ChessnutAir))
            {
                var configChessnut =
                    new ConfigureChessnutWindow(Constants.ChessnutAir, Configuration.Instance, false, configPath) { Owner = this };
                var dialogResult = configChessnut.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (forWhite)
                    {
                        WhiteEboard = new ChessnutAirLoader(configPath);
                        WhiteEboard.Identification = Guid.NewGuid().ToString("N");
                        WhiteConnectionId = WhiteEboard.Identification;
                    }
                    else
                    {
                        BlackEboard = new ChessnutAirLoader(configPath);
                        BlackEboard.Identification = Guid.NewGuid().ToString("N");
                        BlackConnectionId = BlackEboard.Identification;
                    }

                    return true;

                }

                return false;
            }

            if (selectedBoard.Equals(Constants.ChessnutEvo))
            {
                var configEvo = new ConfigureChessnutEvoWindow(Configuration.Instance, configPath) { Owner = this };
                var dialogResult = configEvo.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (forWhite)
                    {
                        WhiteEboard = new ChessnutEvoLoader(configPath);
                        WhiteEboard.Identification = Guid.NewGuid().ToString("N");
                        WhiteConnectionId = WhiteEboard.Identification;
                    }
                    else
                    {
                        BlackEboard = new ChessnutEvoLoader(configPath);
                        BlackEboard.Identification = Guid.NewGuid().ToString("N");
                        BlackConnectionId = BlackEboard.Identification;
                    }

                    return true;
                }

                return false;
            }

            if (selectedBoard.Equals(Constants.MChessLink))
            {
                var configWindow =
                    new ConfigureMChessLinkWindow(Configuration.Instance, false, true, false, false, configPath) { Owner = this };
                var dialogResult = configWindow.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (forWhite)
                    {
                        WhiteEboard = new MChessLinkLoader(configPath);
                        WhiteEboard.Identification = Guid.NewGuid().ToString("N");
                        WhiteConnectionId = WhiteEboard.Identification;
                    }
                    else
                    {
                        BlackEboard = new MChessLinkLoader(configPath);
                        BlackEboard.Identification = Guid.NewGuid().ToString("N");
                        BlackConnectionId = BlackEboard.Identification;
                    }

                    return true;
                }

                return false;
            }

            if (selectedBoard.Equals(Constants.TabutronicCerno))
            {
                var configWindow = new ConfigureCernoWindow(Configuration.Instance, false, false, configPath) { Owner = this };
                var dialogResult = configWindow.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (forWhite)
                    {
                        WhiteEboard = new TabutronicCernoLoader(configPath);
                        WhiteEboard.Identification = Guid.NewGuid().ToString("N");
                        WhiteConnectionId = WhiteEboard.Identification;
                    }
                    else
                    {
                        BlackEboard = new TabutronicCernoLoader(configPath);
                        BlackEboard.Identification = Guid.NewGuid().ToString("N");
                        BlackConnectionId = BlackEboard.Identification;
                    }

                    return true;
                }

                return false;
            }

            if (selectedBoard.Equals(Constants.TabutronicCernoSpectrum))
            {
                var configWindow = new ConfigureCernoSpectrumWindow(Configuration.Instance, false, false, configPath) { Owner = this };
                var dialogResult = configWindow.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (forWhite)
                    {
                        WhiteEboard = new TabutronicCernoSpectrumLoader(configPath);
                        WhiteEboard.Identification = Guid.NewGuid().ToString("N");
                        WhiteConnectionId = WhiteEboard.Identification;
                    }
                    else
                    {
                        BlackEboard = new TabutronicCernoSpectrumLoader(configPath);
                        BlackEboard.Identification = Guid.NewGuid().ToString("N");
                        BlackConnectionId = BlackEboard.Identification;
                    }
                    return true;
                }
                return false;
            }

            if (selectedBoard.Equals(Constants.TabutronicSentio))
            {
                var configWindow = new ConfigureSentioWindow(Configuration.Instance, false, false, configPath) { Owner = this };
                var dialogResult = configWindow.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (forWhite)
                    {
                        WhiteEboard = new TabutronicSentioLoader(configPath);
                        WhiteEboard.Identification = Guid.NewGuid().ToString("N");
                        WhiteConnectionId = WhiteEboard.Identification;
                    }
                    else
                    {
                        BlackEboard = new TabutronicSentioLoader(configPath);
                        BlackEboard.Identification = Guid.NewGuid().ToString("N");
                        BlackConnectionId = BlackEboard.Identification;
                    }
                    return true;
                }
                return false;
            }

            if (selectedBoard.Equals(Constants.TabutronicTactum))
            {
                var configWindow = new ConfigureTactumWindow(Configuration.Instance, false, configPath) { Owner = this };
                var dialogResult = configWindow.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (forWhite)
                    {
                        WhiteEboard = new TabuTronicTactumLoader(configPath);
                        WhiteEboard.Identification = Guid.NewGuid().ToString("N");
                        WhiteConnectionId = WhiteEboard.Identification;
                    }
                    else
                    {
                        BlackEboard = new TabuTronicTactumLoader(configPath);
                        BlackEboard.Identification = Guid.NewGuid().ToString("N");
                        BlackConnectionId = BlackEboard.Identification;
                    }

                    return true;
                }

                return false;
            }

            if (selectedBoard.Equals(Constants.Zmartfun))
            {
                var configWindow = new ConfigureHoSWindow(Configuration.Instance, configPath) { Owner = this };
                var dialogResult = configWindow.ShowDialog();
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (forWhite)
                    {
                        WhiteEboard = new HoSLoader(configPath);
                        WhiteEboard.Identification = Guid.NewGuid().ToString("N");
                        WhiteConnectionId = WhiteEboard.Identification;
                    }
                    else
                    {
                        BlackEboard = new HoSLoader(configPath);
                        BlackEboard.Identification = Guid.NewGuid().ToString("N");
                        BlackConnectionId = BlackEboard.Identification;
                    }

                    return true;
                }

                return false;
            }

            return false;
        }

        private void ButtonConfigureWhiteConnection_Click(object sender, RoutedEventArgs e)
        {
            if (comboboxWhiteEBoardNames.SelectedItem==null)
            {
                return;
            }
            var selectedBoard = comboboxWhiteEBoardNames.SelectedItem as string;
            if (ConfigureBoardConnection(selectedBoard, _configPathWhite, true))
            {
                TextBlockPortNameWhite.Text = WhiteConnectionId;
            }

        }

        private void ButtonConfigureBlackConnection_Click(object sender, RoutedEventArgs e)
        {
            if (comboboxBlackEBoardNames.SelectedItem == null)
            {
                return;
            }
            var selectedBoard = comboboxBlackEBoardNames.SelectedItem as string;
            if (ConfigureBoardConnection(selectedBoard, _configPathBlack, false))
            {
                TextBlockPortNameBlack.Text = BlackConnectionId;
            }
        }

        private void ButtonOk_OnClick(object sender, RoutedEventArgs e)
        {
            if (radioButtonWhiteConnectedViaBC.IsChecked.HasValue && radioButtonWhiteConnectedViaBC.IsChecked.Value)
            {
                if (comboboxWhiteBCNames?.SelectedItem != null)
                {
                    WhiteConnectionId = (comboboxWhiteBCNames.SelectedItem as BearChessClientInformation).Address;
                }
            }
            if (CheckBoxSameConnection.IsChecked.HasValue && CheckBoxSameConnection.IsChecked.Value)
            {
                BlackConnectionId = WhiteConnectionId;
            }
            else
            {
                if (radioButtonBlackConnectedViaBC.IsChecked.HasValue && radioButtonBlackConnectedViaBC.IsChecked.Value)
                {
                    if (comboboxBlackBCNames?.SelectedItem != null)
                    {
                        BlackConnectionId = (comboboxBlackBCNames.SelectedItem as BearChessClientInformation).Address;
                    }
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
                comboboxWhiteEBoardNames.IsEnabled = true;
                ButtonConfigureWhiteConnection.IsEnabled = true;
            }
        }

        private void radioButtonWhiteConnectedViaBC_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                comboboxWhiteBCNames.IsEnabled = true;
                comboboxWhiteEBoardNames.IsEnabled = false;
                ButtonConfigureWhiteConnection.IsEnabled = false;
            }
        }

        private void CheckBoxSameConnection_OnChecked(object sender, RoutedEventArgs e)
        {
            BorderBlack.IsEnabled = false;
        }

        private void CheckBoxSameConnection_OnUnchecked(object sender, RoutedEventArgs e)
        {
            BorderBlack.IsEnabled = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;
using www.SoLaNoSoft.com.BearChessTools;

namespace www.SoLaNoSoft.com.BearChessWin
{
    /// <summary>
    /// Interaktionslogik für MaterialWindow.xaml
    /// </summary>
    public partial class MaterialWindow : Window, IMaterialUserControl
    {
        private readonly Configuration _configuration;


        public MaterialWindow(Configuration configuration, ILogging logger)
        {
            InitializeComponent();
            _configuration = configuration;

            Top = _configuration.GetWinDoubleValue("MaterialWindowTop", Configuration.WinScreenInfo.Top,
                SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            Left = _configuration.GetWinDoubleValue("MaterialWindowLeft", Configuration.WinScreenInfo.Left,
                SystemParameters.VirtualScreenHeight, SystemParameters.VirtualScreenWidth);
            
            ChangeSize(_configuration.GetBoolValue("MaterialWindowSmall", true));
            MaterialUserControl.ShowDifference(_configuration.GetBoolValue("MaterialWindowDifference", false));
            MaterialUserControl.SetLogger(logger);
        }


        public bool GetSmall() => MaterialUserControl.GetSmall();

        public bool GetShowDifference() => MaterialUserControl.GetShowDifference();

        public void Clear()
        {
            MaterialUserControl.Clear();
        }

        public void ChangeSize(bool small)
        {
            MaterialUserControl.ChangeSize(small);
            Width = small ? 380 : 520;
        }

        public void ShowDifference(bool showDifference) => throw new NotImplementedException();

        public void ShowMaterial(IChessFigure[] topFigures, IChessFigure[] bottomFigures, Move[] playedMoveList)
        {
            MaterialUserControl.ShowMaterial(topFigures, bottomFigures, playedMoveList);
        }

        public void SetSdiLayout(bool sdiLayout)
        {
            MaterialUserControl.SetSdiLayout(sdiLayout);
        }

        public void SetLogger(ILogging logger)
        {
            MaterialUserControl.SetLogger(logger);
        }


        private void MaterialWindow_OnClosing(object sender, CancelEventArgs e)
        {
            _configuration.SetDoubleValue("MaterialWindowTop", Top);
            _configuration.SetDoubleValue("MaterialWindowLeft", Left);
            _configuration.SetBoolValue("MaterialWindowSmall",  MaterialUserControl.GetSmall());
            _configuration.SetBoolValue("MaterialWindowDifference", MaterialUserControl.GetShowDifference());
        }
    }
}
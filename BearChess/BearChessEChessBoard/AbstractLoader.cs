﻿using System;
using System.IO;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;

namespace www.SoLaNoSoft.com.BearChess.EChessBoard
{
    public abstract class AbstractLoader : IElectronicChessBoard
    {
        private IEBoardWrapper _eChessBoard;
        private  string _configFile;
        protected string Name { get; }

        public event EventHandler<string> MoveEvent;
        public event EventHandler<string> FenEvent;
        public event EventHandler AwaitedPosition;

        public bool IsInDemoMode => _eChessBoard.IsInDemoMode;
        public bool IsConnected => _eChessBoard.IsConnected;

        protected AbstractLoader(string folderPath, string name)
        {
            Name = name;
            var basePath = Path.Combine(folderPath, name);
            Init(basePath);

        }

        protected AbstractLoader(string name)
        {
            Name = name;
            var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BearChess", name);
            Init(basePath);

        }

        private void Init(string basePath)
        {
            try
            {
                Directory.CreateDirectory(basePath);
                Directory.CreateDirectory(Path.Combine(basePath, "log"));
            }
            catch
            {
                return;
            }
            _configFile = Path.Combine(basePath, $"{Name}Cfg.xml");
            EChessBoardConfiguration configuration = ReadConfiguration();
            // ReSharper disable once VirtualMemberCallInConstructor
            _eChessBoard = GetEBoardImpl(basePath, configuration);
            _eChessBoard.FenEvent += this._eChessBoard_FenEvent;
            _eChessBoard.MoveEvent += this._eChessBoard_MoveEvent;
            _eChessBoard.AwaitedPosition += _eChessBoard_AwaitedPosition;
            
        }

       

        protected abstract IEBoardWrapper GetEBoardImpl(string basePath, EChessBoardConfiguration configuration);

        /// <inheritdoc />
        public void SetDemoMode(bool inDemoMode)
        {
            _eChessBoard.SetDemoMode(inDemoMode);
        }

        /// <inheritdoc />
        public void ShowMove(string allMoves)
        {
            _eChessBoard.ShowMove(allMoves);
        }

        /// <inheritdoc />
        public void ShowMove(string fromField, string toField)
        {
            _eChessBoard.ShowMove(fromField, toField);
        }

        /// <inheritdoc />
        public void SetLedsFor(string[] fields)
        {
            _eChessBoard.SetLedsFor(fields);
        }

        /// <inheritdoc />
        public void SetAllLedsOff()
        {
            _eChessBoard.SetAllLedsOff();
        }

        /// <inheritdoc />
        public void SetAllLedsOn()
        {
            _eChessBoard.SetAllLedsOn();
        }

        /// <inheritdoc />
        public DataFromBoard GetPiecesFen()
        {
            return _eChessBoard.GetPiecesFen();
        }

        /// <inheritdoc />
        public string GetFen()
        {
            return _eChessBoard.GetFen();
        }

        /// <inheritdoc />
        public void NewGame()
        {
            _eChessBoard.NewGame();
        }

        /// <inheritdoc />
        public void SetFen(string fen, string allMoves)
        {
            _eChessBoard.SetFen(fen, allMoves);
        }

        /// <inheritdoc />
        public void Close()
        {
            _eChessBoard.Close();
        }

        /// <inheritdoc />
        public void Stop()
        {
            _eChessBoard.Stop();
        }

        /// <inheritdoc />
        public void Calibrate()
        {
            _eChessBoard.Calibrate();
        }

        /// <inheritdoc />
        public void PlayWithWhite(bool withWhite)
        {
            _eChessBoard.PlayWithWhite(withWhite);
        }

        /// <inheritdoc />
        public string GetBestMove()
        {
            return _eChessBoard.GetBestMove();
        }

        /// <inheritdoc />
        public void SetComPort(string portName)
        {
            _eChessBoard.SetCOMPort(portName);
        }

        /// <inheritdoc />
        public void DimLeds(bool dimLeds)
        {
            _eChessBoard.DimLeds(dimLeds);
        }

        /// <inheritdoc />
        public void FlashInSync(bool flashSync)
        {
            _eChessBoard.FlashInSync(flashSync);
        }

        /// <inheritdoc />
        public EChessBoardConfiguration GetEChessBoardConfiguration()
        {
            return ReadConfiguration();
        }

        /// <inheritdoc />
        public void SaveEChessBoardConfiguration(EChessBoardConfiguration configuration)
        {
            SaveConfiguration(configuration);
        }

        /// <inheritdoc />
        public void SetLedCorner(bool upperLeft, bool upperRight, bool lowerLeft, bool lowerRight)
        {
            _eChessBoard.SetLedCorner(upperLeft,upperRight,lowerLeft,lowerRight);
        }

        #region private

        private void _eChessBoard_MoveEvent(object sender, string move)
        {
            MoveEvent?.Invoke(this, move);
        }

        private EChessBoardConfiguration ReadConfiguration()
        {
            return EChessBoardConfiguration.Load(_configFile);
        }

        private void SaveConfiguration(EChessBoardConfiguration configuration)
        {
            EChessBoardConfiguration.Save(configuration,_configFile);
        }

        private void _eChessBoard_FenEvent(object sender, string fenPosition)
        {
            FenEvent?.Invoke(this, fenPosition);
        }

        private void _eChessBoard_AwaitedPosition(object sender, EventArgs e)
        {
            AwaitedPosition?.Invoke(sender,e);
        }

        #endregion
    }
}

﻿using System.Threading;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.PegasusEBoardWrapper
{
    public class PegasusImpl : AbstractEBoardWrapper
    {


        public PegasusImpl(string name, string basePath) : base(name, basePath)
        {
        }

        public PegasusImpl(string name, string basePath, bool isFirstInstance, string comPortName, bool useBluetooth) : base(
            name, basePath, isFirstInstance, comPortName, useBluetooth)
        {

        }

        public override void FlashInSync(bool flashInSync)
        {
            _board.FlashSync(flashInSync);
        }

        protected override IEBoard GetEBoard()
        {
            return new PegasusChessBoard.EChessBoard(basePath: _basePath, logger: _fileLogger, isFirstInstance: _isFirstInstance,portName: _comPortName, _useBluetooth);
        }

        protected override IEBoard GetEBoard(bool check)
        {
            
            return new PegasusChessBoard.EChessBoard(logger: _fileLogger);
        }

        public override void Calibrate()
        {
            _stop = true;
            SetAllLedsOn();
            Thread.Sleep(1000);
            _board.Calibrate();
            SetAllLedsOff();
            _stop = false;
        }

        public override void DimLeds(bool _)
        {
            // Ignore
        }

        public override void DimLeds(int _)
        {
            // Ignore
        }
    }
}
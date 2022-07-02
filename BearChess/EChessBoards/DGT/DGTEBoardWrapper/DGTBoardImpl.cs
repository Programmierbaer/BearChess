using System;
using www.SoLaNoSoft.com.BearChess.EChessBoard;

namespace www.SoLaNoSoft.com.BearChess.DGTEBoardWrapper
{
    public class DGTBoardImpl :AbstractEBoardWrapper
    {

        public DGTBoardImpl(string name, string basePath) : base(name, basePath)
        {
        }

     
        public DGTBoardImpl(string name, string basePath, bool isFirstInstance, string comPortName, bool useBluetooth, bool useClock, bool showOnlyMoves, bool switchClockSide) : base(
            name, basePath, isFirstInstance, comPortName, useBluetooth, useClock, showOnlyMoves, switchClockSide)
        {

        }


        public override void Calibrate()
        {
            _board.Calibrate();
        }

        public override void SendInformation(string message)
        {
            _board.SendInformation(message);
        }

        public override void DimLEDs(bool dimLeds)
        {
           // throw new NotImplementedException();
        }

        public override void DimLEDs(int level)
        {
           //  throw new NotImplementedException();
        }

        public override void FlashInSync(bool flashSync)
        {
            // throw new NotImplementedException();
        }

        protected override IEBoard GetEBoard()
        {
            return new DGTChessBoard.EChessBoard(basePath: _basePath, logger: _fileLogger,
                                                 isFirstInstance: _isFirstInstance, portName: _comPortName,
                                                 _useBluetooth, _useClock, _showMovesOnly, _switchClockSide);
        }

        protected override IEBoard GetEBoard(bool check)
        {

            return new DGTChessBoard.EChessBoard(logger: _fileLogger);
        }
    }
}

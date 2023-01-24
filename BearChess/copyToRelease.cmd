@echo off
cd C:\Users\larsn\source\github\BearChess\BearChess
copy C:\Users\larsn\source\repos\BearChess\BearChess\BearChessBase\*.* BearChessBase /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\BearChessBTLETools\*.* BearChessBTLETools /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\BearChessBTTools\*.* BearChessBTTools /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\BearChessCommunication\*.* BearChessCommunication /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\BearChessDatabase\*.* BearChessDatabase /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\BearChessEChessBoard\*.* BearChessEChessBoard /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\BearChessTools\*.* BearChessTools /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\BearChessTournament\*.* BearChessTournament /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\BearChessWin\*.* BearChessWin /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\CertaboLoader\*.* CertaboLoader /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\EChessBoards\*.* EChessBoards /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\Engine\*.* Engine /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\FICS\*.* FICS /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\MChessLinkLoader\*.* MChessLinkLoader /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\packages\*.* packages /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\PegasusChessBoard\*.* PegasusChessBoard /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\PegasusEboardWrapper\*.* PegasusEboardWrapper /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\PegasusLoader\*.* PegasusLoader /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\SquareOffChessBoard\*.* SquareOffChessBoard /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\SquareOffEBoardWrapper\*.* SquareOffEBoardWrapper /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\SquareOffProLoader\*.* SquareOffProLoader /s /y
copy C:\Users\larsn\source\repos\BearChess\BearChess\Teddy\*.* Teddy /s /y
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" BearChess.sln
cd C:\Users\larsn\source\repos\BearChess\BearChess
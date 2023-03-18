@echo off
cd C:\Users\larsn\source\github\BearChess\BearChess
copy C:\Users\larsn\source\repos\BearChess\BearChess\*.* /s /y
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" BearChess.sln
cd C:\Users\larsn\source\repos\BearChess\BearChess
﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using www.SoLaNoSoft.com.BearChess.CommonUciWrapper;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChess.IChessOneChessBoard
{
    public class SerialCommunication : AbstractSerialCommunication
    {
        private Thread _readingThread;
        public SerialCommunication(ILogging logger, string portName, string boardName) : base(logger, portName, boardName)
        {
        }

        public SerialCommunication(ILogging logger, string portName, bool useBluetooth) : base(logger, portName, Constants.IChessOne)
        {
            _useBluetooth = useBluetooth;
            _useHID = !useBluetooth;
        }

        public override string GetRawFromBoard(string param)
        {
            try
            {
                return _comPort.ReadLine();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex.Message);
                return string.Empty;
            }
        }

        public override void SendRawToBoard(string param)
        {
            var convertToSend = ConvertToArray(param);
            _comPort.Write(convertToSend, 0, convertToSend.Length);
        }

        public override void SendRawToBoard(byte[] param)
        {
            _comPort.Write(param, 0, param.Length);
        }

        public override string GetCalibrateData()
        {
            return string.Empty;
        }

        protected override void Communicate()
        {
            _logger?.LogDebug("SC: Communicate");
            IsCommunicating = true;
            bool withConnection = true;
            string lastReadToSend = string.Empty;
            string convertFromRead = string.Empty;
            _readingThread = new Thread(ReadingFromBoard) { IsBackground = true };
            _readingThread.Start();
            while (!_stopReading)
            {
                try
                {
                    if (!withConnection)
                    {
                        withConnection = Connect();
                    }

                    if (withConnection && !_pauseReading)
                    {
                        
                        if (_stringDataToBoard.TryDequeue(out string stringData))
                        {
                            {
                                _forcedSend = false;
                                _logger?.LogDebug($"SC: Send {stringData}");
                                var convertToArray = ConvertToArray($"{stringData}{Environment.NewLine}");
                                _comPort.Write(convertToArray,0,convertToArray.Length);
                                //_comPort.WriteLine(UTF8toASCII(stringData));
                                //_comPort.WriteLine($"{stringData}{Environment.NewLine}");
                                lastReadToSend = convertFromRead;
                            }
                        }

                    }

                    Thread.Sleep(5);
                }
                catch (Exception ex)
                {
                    withConnection = false;
                    string port = _useBluetooth ? "BTLE" : "USB";
                    _logger?.LogError($"SC: Error with {port} port: {convertFromRead} ");
                    _logger?.LogError($"SC: Error with {port} port: {ex.Message} ");

                    //break;
                }
            }
            IsCommunicating = false;
            _logger?.LogDebug("SC: Exit Communicate");
        }

        public static string UTF8toASCII(string text)
        {
            System.Text.Encoding utf8 = System.Text.Encoding.UTF8;
            Byte[] encodedBytes = utf8.GetBytes(text);
            Byte[] convertedBytes =
                Encoding.Convert(Encoding.UTF8, Encoding.ASCII, encodedBytes);
            System.Text.Encoding ascii = System.Text.Encoding.ASCII;

            return ascii.GetString(convertedBytes);
        }

        private byte[] ConvertToArray(string param)
        {
         
            return Encoding.ASCII.GetBytes(param);
        }

        private string ConvertFromArray(byte[] bArray)
        {
            var r = string.Empty;
            foreach (var b in bArray)
            {
                r = r + b + " ";
            }

            return r;
        }

        private string ConvertFromRead(byte[] bArray)
        {
            string r = Encoding.UTF8.GetString(bArray, 0, bArray.Length);
           
            return r;
        }


        private void ReadingFromBoard()
        {
            bool withConnection = true;
            string readLine = string.Empty;
            string prevLine = string.Empty;
            while (!_stopReading)
            {
                Thread.Sleep(5);
                try
                {
                    if (!withConnection)
                    {
                        withConnection = Connect();
                    }
                    if (withConnection && !_pauseReading)
                    {
                        readLine = string.Empty;
                        try
                        {
                            {
                                readLine = _comPort.ReadLine();
                                byte[] readByte = _comPort.ReadByteArray();
                                if (readByte.Length > 0)
                                {
                                    var convertFromRead = ConvertFromRead(readByte);
                                    readLine = convertFromRead;
                                }

                            }
                        }
                        catch
                        {
                            // _logger?.LogDebug("SC: Catch");
                        }


                        if (string.IsNullOrWhiteSpace(readLine))
                        {
                            continue;
                        }
                        if (!readLine.Equals(prevLine))
                            _logger?.LogDebug($"SC: Read {readLine.Length} bytes from board: {readLine}");
                        prevLine = readLine;
                        _dataFromBoard.Enqueue(readLine);



                    }

                }
                catch (Exception ex)
                {
                    withConnection = false;
                    _logger?.LogError($"SC: Error with BTLE port: {readLine} ");
                    _logger?.LogError($"SC: Error with BTLE port: {ex.Message} ");
                    //break;
                }
            }
        }
    }
}
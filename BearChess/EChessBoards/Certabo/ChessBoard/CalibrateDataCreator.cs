using System;
using System.Collections.Generic;

namespace www.SoLaNoSoft.com.BearChess.CertaboChessBoard
{
    public class CalibrateDataCreator
    {
        private int count = 0;
        
        private readonly Dictionary<string, int> calibrationHelper = new Dictionary<string, int>();

        public bool LimitExceeds { get; private set; }
        public string CalibrateCodes { get; private set; }

        public CalibrateDataCreator()
        {
            LimitExceeds = false;
            CalibrateCodes = string.Empty;
        }

        public bool NewDataFromBoard(string fromBoard)
        {
            count++;
            
            LimitExceeds = count > 200;
            var cleanCodes = RemoveNoise(fromBoard);
            if (string.IsNullOrWhiteSpace(cleanCodes))
            {
                return false;
            }
            if (!calibrationHelper.ContainsKey(cleanCodes))
            {
                calibrationHelper[cleanCodes] = 1;
            }
            else
            {
                calibrationHelper[cleanCodes] = calibrationHelper[cleanCodes] + 1;
            }
            // More than 10 times same result => should be the right codes
            if (calibrationHelper[cleanCodes] >= 10)
            {
                CalibrateCodes = cleanCodes;
                return true;
            }
            return false;
        }



        private string RemoveNoise(string codes)
        {
            var dataArray = codes.Replace('\0', ' ').Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (dataArray.Length < 320)
            {
                return string.Empty;
            }
            for (int i=80; i<95; i++)
              dataArray[i] = "0";
            for (int i = 100; i < 215; i++)
                dataArray[i] = "0";
            for (int i = 220; i < 240; i++)
                dataArray[i] = "0";
            return string.Join(" ", dataArray);
        }
    }
}
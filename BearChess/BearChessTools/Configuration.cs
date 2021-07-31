using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using InTheHand.Net;
using www.SoLaNoSoft.com.BearChessBase;
using www.SoLaNoSoft.com.BearChessBase.Implementations;

namespace www.SoLaNoSoft.com.BearChessTools
{
    public class Configuration
    {
        public enum WinScreenInfo
        {
            Top,
            Width,
            Height,
            Left
        }

        private static Configuration _instance;
        private static readonly object Locker = new object();
        private readonly ConfigurationSettings<string, string> _appSettings;
        private string ConfigFileName { get; }
        private string BtConfigFileName { get; }
        private string TimeControlFileName { get; }
        private string StartupTimeControlFileName { get; }
        private string DatabaseFilterFileName { get; }

        public const string STARTUP_WHITE_ENGINE_ID = "startupWhite.uci";
        public const string STARTUP_BLACK_ENGINE_ID = "startupBlack.uci";

        public string FolderPath { get; }

        public static Configuration Instance
        {
            get
            {
                lock (Locker)
                {
                    return _instance ?? (_instance = new Configuration());
                }
            }
        }

        private Configuration()
        {
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BearChess");
            if (!Directory.Exists(FolderPath))
            {
                try
                {
                    Directory.CreateDirectory(FolderPath);
                }
                catch
                {
                    FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BearChess");
                }
            }
            TimeControlFileName = Path.Combine(FolderPath, "bearchess_tc.xml");
            StartupTimeControlFileName = Path.Combine(FolderPath, "bearchess_start_tc.xml");
            ConfigFileName = Path.Combine(FolderPath, "bearchess.xml");
            BtConfigFileName = Path.Combine(FolderPath, "bearchess_bt.xml");
            DatabaseFilterFileName = Path.Combine(FolderPath, "bearchess_dbfilter.xml");
            try
            {
                if (File.Exists(ConfigFileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ConfigurationSettings<string, string>));
                    TextReader textReader = new StreamReader(ConfigFileName);
                    _appSettings = (ConfigurationSettings<string, string>) serializer.Deserialize(textReader);
                    textReader.Close();
                }
                else
                {
                    _appSettings = new ConfigurationSettings<string, string>();
                }
            }
            catch
            {
                _appSettings = new ConfigurationSettings<string, string>();
            }
        }

        public double GetWinDoubleValue(string winName, WinScreenInfo screenInfo, double vScreenHeight, double vScreenWidth,  string defaultValue="0")
        {
            var winDoubleValue = double.Parse(GetConfigValue(_appSettings, winName, defaultValue), CultureInfo.InvariantCulture);
            double winValue;
            switch (screenInfo)
            {
                case WinScreenInfo.Top: winValue = vScreenHeight;
                    if (winDoubleValue > winValue - 50)
                    {
                        winDoubleValue = 0;
                    }
                    break;
                case WinScreenInfo.Height:
                    winValue = vScreenHeight;
                    if (winDoubleValue > winValue - 50)
                    {
                        winDoubleValue = winValue - 50;
                    }
                    break;
                case WinScreenInfo.Left:
                    winValue = vScreenWidth;
                    if (winDoubleValue > winValue - 50)
                    {
                        winDoubleValue = 0;
                    }
                    break; 
                case WinScreenInfo.Width:
                    winValue = vScreenWidth;
                    if (winDoubleValue > winValue - 50)
                    {
                        winDoubleValue = winValue - 50;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(screenInfo), screenInfo, null);
            }
            return winDoubleValue;
        }

        public double GetDoubleValue(string winName,  string defaultValue = "0")
        {
            return double.Parse(GetConfigValue(_appSettings, winName, defaultValue), CultureInfo.InvariantCulture);
        }

        public void SetDoubleValue(string winName, double position)
        {
            SetConfigValue(_appSettings, winName, position.ToString(CultureInfo.InvariantCulture));
        }

        public void SaveGamesFilter(GamesFilter gamesFilter)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GamesFilter));
                TextWriter textWriter = new StreamWriter(DatabaseFilterFileName, false);
                serializer.Serialize(textWriter, gamesFilter);
                textWriter.Close();
            }
            catch
            {
                //
            }
        }

        public GamesFilter LoadGamesFilter()
        {
            if (!File.Exists(DatabaseFilterFileName))
            {
                return new GamesFilter() {FilterIsActive = false};
            }
            XmlSerializer serializer = new XmlSerializer(typeof(GamesFilter));
            TextReader textReader = new StreamReader(DatabaseFilterFileName);
            GamesFilter gamesFilter = (GamesFilter)serializer.Deserialize(textReader);
            textReader.Close();
            return gamesFilter;
        }

        public void Save(BluetoothAddress btAddress)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(BluetoothAddress));
                TextWriter textWriter = new StreamWriter(BtConfigFileName, false);
                serializer.Serialize(textWriter, btAddress);
                textWriter.Close();
            }
            catch
            {
                //
            }
        }

        public BluetoothAddress LoadBtAddress()
        {
            if (!File.Exists(BtConfigFileName))
            {
                return null;
            }
            XmlSerializer serializer = new XmlSerializer(typeof(BluetoothAddress));
            TextReader textReader = new StreamReader(BtConfigFileName);
            BluetoothAddress btAddress = (BluetoothAddress)serializer.Deserialize(textReader);
            textReader.Close();
            return btAddress;
        }

        public void Save()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ConfigurationSettings<string, string>));
                TextWriter textWriter = new StreamWriter(ConfigFileName, false);
                serializer.Serialize(textWriter, _appSettings);
                textWriter.Close();
            }
            catch
            {
                //
            }
        }

        /// <summary>
        /// Save the <paramref name="timeControl"/> as <paramref name="asStartup"/> or just as last configured time control
        /// </summary>
        /// <param name="timeControl"></param>
        /// <param name="asStartup"></param>
        public void Save(TimeControl timeControl, bool asStartup)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TimeControl));
                TextWriter textWriter =
                    new StreamWriter(asStartup ? StartupTimeControlFileName : TimeControlFileName, false);
                serializer.Serialize(textWriter, timeControl);
                textWriter.Close();
            }
            catch
            {
                //
            }
        }

        public TimeControl LoadTimeControl(bool asStartup)
        {
            if (!File.Exists(asStartup ? StartupTimeControlFileName : TimeControlFileName))
            {
                return null;
            }
            XmlSerializer serializer = new XmlSerializer(typeof(TimeControl));
            TextReader textReader = new StreamReader(asStartup ? StartupTimeControlFileName : TimeControlFileName);
            TimeControl timeControl = (TimeControl)serializer.Deserialize(textReader);
            textReader.Close();
            return timeControl;
        }



        public void SetConfigValue(string key, string value)
        {
            SetConfigValue(_appSettings, key, value);
        }

        public string GetConfigValue(string key, string defaultValue)
        {
            return _appSettings.ContainsKey(key) ? _appSettings[key] : defaultValue;
        }

        private string GetConfigValue(Dictionary<string, string> settings, string key, string defaultValue)
        {
            return settings.ContainsKey(key) ? settings[key] : defaultValue;
        }

        private void SetConfigValue(Dictionary<string, string> settings, string key, string value)
        {
            settings[key] = value;
        }
    }
}
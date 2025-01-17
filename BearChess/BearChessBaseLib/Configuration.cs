using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using InTheHand.Net;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations;
using www.SoLaNoSoft.com.BearChessBase.Implementations.pgn;

namespace www.SoLaNoSoft.com.BearChessBase
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

        private string ConfigFileName
        {
            get;
        }

        private string BTConfigFileName
        {
            get;
        }

        private string TimeControlFileName
        {
            get;
        }

        private string TimeControlBlackFileName
        {
            get;
        }

        private string FicTimeControlFileName
        {
            get;
        }

        private string StartupTimeControlFileName
        {
            get;
        }

        private string StartupTimeControlBlackFileName
        {
            get;
        }

        private string DatabaseFilterFileName
        {
            get;
        }

        public const string STARTUP_WHITE_ENGINE_ID = "startupWhite.uci";
        public const string STARTUP_BLACK_ENGINE_ID = "startupBlack.uci";

        public const string MESSCHESS_LEVELS_FILE = "MessChessLevels.txt";

        public string FolderPath
        {
            get;
        }

        public string BinPath
        {
            get;
        }

        public bool RunOn64Bit
        {
            get;
        }

        public bool Standalone
        {
            get;
        }

        public PgnConfiguration GetPgnConfiguration()
        {
            return new PgnConfiguration()
            {
                PurePgn = bool.Parse(GetConfigValue("gamesPurePGNExport", "true")),
                IncludeComment = bool.Parse(GetConfigValue("gamesPGNExportComment", "true")),
                IncludeEvaluation = bool.Parse(GetConfigValue("gamesPGNExportEvaluation", "true")),
                IncludeMoveTime = bool.Parse(GetConfigValue("gamesPGNExportMoveTime", "true")),
                IncludeSymbols = bool.Parse(GetConfigValue("gamesPGNExportSymbols", "true")),
            };
        }

        public void SavePgnConfiguration(PgnConfiguration pgnConfiguration)
        {
            SetConfigValue("gamesPurePGNExport", pgnConfiguration.PurePgn.ToString());
            SetConfigValue("gamesPGNExportComment", pgnConfiguration.IncludeComment.ToString());
            SetConfigValue("gamesPGNExportEvaluation", pgnConfiguration.IncludeEvaluation.ToString());
            SetConfigValue("gamesPGNExportMoveTime", pgnConfiguration.IncludeMoveTime.ToString());
            SetConfigValue("gamesPGNExportSymbols", pgnConfiguration.IncludeSymbols.ToString());
        }

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

        public CultureInfo SystemCultureInfo
        {
            get;
        }

        private Configuration()
        {
            RunOn64Bit = Environment.Is64BitProcess;
            SystemCultureInfo = Thread.CurrentThread.CurrentUICulture;
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            BinPath = fileInfo.DirectoryName;
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),  Constants.BearChess);
            var args = Environment.GetCommandLineArgs();
            Standalone = false;
            for (var i = 1; i < args.Length; i++)
            {
                if (args[i].Equals("-path", StringComparison.InvariantCultureIgnoreCase) && !Standalone)
                {
                    if (i < args.Length - 1)
                    {
                        try
                        {
                            var pathValue = args[i + 1];
                            if (pathValue.Equals("doc", StringComparison.InvariantCultureIgnoreCase) ||
                                pathValue.Equals("dok", StringComparison.InvariantCultureIgnoreCase))
                            {
                                FolderPath =
                                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                        Constants.BearChess);
                            }

                            else
                            {
                                var pathName = Path.Combine(pathValue, Constants.BearChess);
                                Directory.Exists(pathName);
                                FolderPath = pathName;
                            }
                        }
                        catch
                        {
                            //
                        }
                    }
                }

                if (args[i].Equals("-standalone", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (BinPath != null)
                    {
                        FolderPath = Path.Combine(BinPath, Constants.BearChess);
                    }

                    Standalone = true;
                }
            }

            if (!Directory.Exists(FolderPath))
            {
                try
                {
                    Directory.CreateDirectory(FolderPath);
                }
                catch
                {
                    FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        Constants.BearChess);
                }
            }

            TimeControlFileName = Path.Combine(FolderPath, "bearchess_tc.xml");
            TimeControlBlackFileName = Path.Combine(FolderPath, "bearchess_tc2.xml");
            FicTimeControlFileName = Path.Combine(FolderPath, "bearchess_ficstc.xml");
            StartupTimeControlFileName = Path.Combine(FolderPath, "bearchess_start_tc.xml");
            StartupTimeControlBlackFileName = Path.Combine(FolderPath, "bearchess_start_tc2.xml");
            ConfigFileName = Path.Combine(FolderPath, "bearchess.xml");
            BTConfigFileName = "bearchess_bt.xml";
            DatabaseFilterFileName = Path.Combine(FolderPath, "bearchess_dbfilter.xml");
            try
            {
                if (File.Exists(ConfigFileName))
                {
                    var serializer = new XmlSerializer(typeof(ConfigurationSettings<string, string>));
                    TextReader textReader = new StreamReader(ConfigFileName);
                    _appSettings = (ConfigurationSettings<string, string>)serializer.Deserialize(textReader);
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

        public double GetWinDoubleValue(string winName, WinScreenInfo screenInfo, double vScreenHeight,
            double vScreenWidth, string defaultValue = "0")
        {
            var winDoubleValue = double.Parse(GetConfigValue(_appSettings, winName, defaultValue),
                CultureInfo.InvariantCulture);
            if (winDoubleValue < 0)
            {
                winDoubleValue = 0;
            }
            double winValue;
            switch (screenInfo)
            {
                case WinScreenInfo.Top:
                    winValue = vScreenHeight;
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

        public double GetDoubleValue(string winName, string defaultValue = "0")
        {
            return double.Parse(GetConfigValue(_appSettings, winName, defaultValue), CultureInfo.InvariantCulture);
        }

        public double GetDoubleValue(string winName, double defaultValue = 0)
        {
            return double.Parse(GetConfigValue(_appSettings, winName, defaultValue.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture);
        }

        public void SetDoubleValue(string winName, double position)
        {
            SetConfigValue(_appSettings, winName, position.ToString(CultureInfo.InvariantCulture));
        }

        public bool GetBoolValue(string key, bool defaultValue)
        {
            return bool.Parse(GetConfigValue(key, defaultValue.ToString()));
        }

        public void SetBoolValue(string key, bool value)
        {
            SetConfigValue(key, value.ToString());
        }

        public int GetIntValue(string key, int defaultValue)
        {
            return int.Parse(GetConfigValue(key, defaultValue.ToString()));
        }

        public void SetIntValue(string key, int value)
        {
            SetConfigValue(key, value.ToString());
        }

        public void SaveGamesFilter(GamesFilter gamesFilter)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(GamesFilter));
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
                return new GamesFilter() { FilterIsActive = false };
            }

            try
            {
                var serializer = new XmlSerializer(typeof(GamesFilter));
                TextReader textReader = new StreamReader(DatabaseFilterFileName);
                var gamesFilter = (GamesFilter)serializer.Deserialize(textReader);
                textReader.Close();
                return gamesFilter;
            }
            catch
            {
                //
            }

            return new GamesFilter() { FilterIsActive = false };
        }

        public void SaveBTAddress(string boardName, BluetoothAddress btAddress, string identifier = "0")
        {
            try
            {
                var fileName = Path.Combine(FolderPath, boardName, $"{boardName}_{identifier}_bt.xml");
                var serializer = new XmlSerializer(typeof(BluetoothAddress));
                TextWriter textWriter = new StreamWriter(fileName, false);
                serializer.Serialize(textWriter, btAddress);
                textWriter.Close();
            }
            catch
            {
                //
            }
        }

        public BluetoothAddress LoadBTAddress(string boardName, string identifier = "0")
        {
            var migrateConfig = false;
            var fileName = Path.Combine(FolderPath, boardName, $"{boardName}_{identifier}_bt.xml");
            if (!File.Exists(fileName))
            {
                fileName = Path.Combine(FolderPath, BTConfigFileName);
                if (!File.Exists(fileName))
                {
                    return null;
                }

                migrateConfig = true;
            }

            try
            {
                var serializer = new XmlSerializer(typeof(BluetoothAddress));
                TextReader textReader = new StreamReader(fileName);
                var btAddress = (BluetoothAddress)serializer.Deserialize(textReader);
                textReader.Close();
                if (migrateConfig)
                {
                    SaveBTAddress(boardName, btAddress);
                }

                return btAddress;
            }
            catch
            {
                //
            }

            return null;
        }

        public void Save()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ConfigurationSettings<string, string>));
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
        /// <param name="white"></param>
        /// <param name="asStartup"></param>
        public void Save(TimeControl timeControl, bool white, bool asStartup)
        {
            try
            {
                if (white)
                {
                    var serializer = new XmlSerializer(typeof(TimeControl));
                    TextWriter textWriter =
                        new StreamWriter(asStartup ? StartupTimeControlFileName : TimeControlFileName, false);
                    serializer.Serialize(textWriter, timeControl);
                    textWriter.Close();
                }
                else
                {
                    var serializer = new XmlSerializer(typeof(TimeControl));
                    TextWriter textWriter =
                        new StreamWriter(asStartup ? StartupTimeControlBlackFileName : TimeControlBlackFileName, false);
                    serializer.Serialize(textWriter, timeControl);
                    textWriter.Close();
                }
            }
            catch
            {
                //
            }
        }


        public TimeControl LoadTimeControl(bool white, bool asStartup)
        {
            return white ? LoadWhiteTimeControl(asStartup) : LoadBlackTimeControl(asStartup);
        }

        private TimeControl LoadWhiteTimeControl(bool asStartup)
        {
            if (!File.Exists(asStartup ? StartupTimeControlFileName : TimeControlFileName))
            {
                return null;
            }

            var serializer = new XmlSerializer(typeof(TimeControl));
            TextReader textReader = new StreamReader(asStartup ? StartupTimeControlFileName : TimeControlFileName);
            var timeControl = (TimeControl)serializer.Deserialize(textReader);
            textReader.Close();
            return timeControl;
        }

        private TimeControl LoadBlackTimeControl(bool asStartup)
        {
            if (!File.Exists(asStartup ? StartupTimeControlBlackFileName : TimeControlBlackFileName))
            {
                return LoadWhiteTimeControl(asStartup);
            }

            var serializer = new XmlSerializer(typeof(TimeControl));
            TextReader textReader =
                new StreamReader(asStartup ? StartupTimeControlBlackFileName : TimeControlBlackFileName);
            var timeControl = (TimeControl)serializer.Deserialize(textReader);
            textReader.Close();
            return timeControl;
        }

        public void Save(FicsTimeControl timeControl, int number)
        {
            try
            {
                var fileName = FicTimeControlFileName.Replace("ficstc", $"ficstc_{number}");
                var serializer = new XmlSerializer(typeof(FicsTimeControl));
                TextWriter textWriter = new StreamWriter(fileName, false);
                serializer.Serialize(textWriter, timeControl);
                textWriter.Close();
            }
            catch
            {
                //
            }
        }

        public FicsTimeControl LoadFicsTimeControl(int number)
        {
            var fileName = FicTimeControlFileName.Replace("ficstc", $"ficstc_{number}");
            if (!File.Exists(fileName))
            {
                return new FicsTimeControl(number);
            }

            var serializer = new XmlSerializer(typeof(FicsTimeControl));
            TextReader textReader = new StreamReader(fileName);
            var timeControl = (FicsTimeControl)serializer.Deserialize(textReader);
            textReader.Close();
            return timeControl;
        }


        public void SetSecureConfigValue(string key, string value)
        {
            var encryptPlainTextToCipherText = EnDeCryption.EncryptPlainTextToCipherText(value);
            SetConfigValue(_appSettings, key, encryptPlainTextToCipherText);
        }


        public void SetConfigValue(string key, string value)
        {
            SetConfigValue(_appSettings, key, value);
        }

        public void DeleteConfigValue(string key)
        {
            if (_appSettings.ContainsKey(key))
            {
                _appSettings.Remove(key);
            }
        }

        public string GetConfigValue(string key, string defaultValue)
        {
            return _appSettings.TryGetValue(key, out var setting) ? setting : defaultValue;
        }

        public string GetSecureConfigValue(string key, string defaultValue)
        {
            return _appSettings.TryGetValue(key, out var appSetting)
                ? EnDeCryption.DecryptCipherTextToPlainText(appSetting)
                : defaultValue;
        }


        private string GetConfigValue(IReadOnlyDictionary<string, string> settings, string key, string defaultValue)
        {
            return settings.TryGetValue(key, out var setting) ? setting : defaultValue;
        }

        private void SetConfigValue(IDictionary<string, string> settings, string key, string value)
        {
            settings[key] = value;
        }
    }
}
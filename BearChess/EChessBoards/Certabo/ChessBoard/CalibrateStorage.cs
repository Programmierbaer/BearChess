using System.IO;
using System.Xml.Serialization;

namespace www.SoLaNoSoft.com.BearChess.CertaboChessBoard
{
    public class CalibrateStorage : ICalibrateStorage
    {
        private readonly bool _useChesstimation;
        private readonly string _oldStorageFile = "calibrate.dat";
        private readonly string _storageFile = "calibrate.xml";

        private CalibrateData _calibrateData;

        public CalibrateStorage(string basePath, bool useChesstimation)
        {
            _useChesstimation = useChesstimation;
            try
            {
                _storageFile = Path.Combine(basePath, _storageFile);
                _oldStorageFile = Path.Combine(basePath, _oldStorageFile);
                Load();
            }
            catch
            {
                //
            }
        }

        /// <inheritdoc />
        public bool SaveCalibrationData(CalibrateData codes)
        {
            try
            {
                _calibrateData.BasePositionCodes = codes.BasePositionCodes;
                _calibrateData.WhiteQueenCodes = codes.WhiteQueenCodes;
                _calibrateData.BlackQueenCodes = codes.BlackQueenCodes;
                return Save();
            }
            catch
            {
                //
            }

            return false;
        }

        private bool Save()
        {
            if (_useChesstimation)
            {
                return true;
            }
            try
            {
                var serializer = new XmlSerializer(typeof(CalibrateData));
                TextWriter textWriter = new StreamWriter(_storageFile, false);
                serializer.Serialize(textWriter, _calibrateData);
                textWriter.Close();
                return true;
            }
            catch
            {
                //
            }

            return false;
        }

        /// <inheritdoc />
        public CalibrateData GetCalibrationData()
        {
            try
            {
                if (_calibrateData == null)
                {
                    Load();
                }

                return _calibrateData;
            }
            catch
            {
                //
            }

            return new CalibrateData()
                   {
                       BasePositionCodes = string.Empty,
                       WhiteQueenCodes = string.Empty,
                       BlackQueenCodes = string.Empty
                   };
        }


        private void Load()
        {
            if (_useChesstimation)
            {
                _calibrateData = new CalibrateData
                                 {
                                     BasePositionCodes = "0 0 0 0 5 0 0 0 0 7 0 0 0 0 9 0 0 0 0 11 0 0 0 0 13 0 0 0 0 25 0 0 0 0 23 0 0 0 0 21 0 0 0 0 3 0 0 0 0 19 0 0 0 0 35 0 0 0 0 51 0 0 0 0 67 0 0 0 0 83 0 0 0 0 99 0 0 0 0 115 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 2 0 0 0 0 18 0 0 0 0 34 0 0 0 0 50 0 0 0 0 66 0 0 0 0 82 0 0 0 0 98 0 0 0 0 114 0 0 0 0 4 0 0 0 0 6 0 0 0 0 8 0 0 0 0 10 0 0 0 0 12 0 0 0 0 24 0 0 0 0 22 0 0 0 0 20",
                                     WhiteQueenCodes = "0 0 0 0 10",
                                     BlackQueenCodes = "0 0 0 0 11"
                };
                return;
            }
            try
            {
                if (File.Exists(_storageFile))
                {
                    var serializer = new XmlSerializer(typeof(CalibrateData));
                    TextReader textReader = new StreamReader(_storageFile);
                    CalibrateData savedConfig = (CalibrateData) serializer.Deserialize(textReader);
                    _calibrateData = new CalibrateData
                    {
                        BasePositionCodes = savedConfig.BasePositionCodes,
                        WhiteQueenCodes = savedConfig.WhiteQueenCodes,
                        BlackQueenCodes = savedConfig.BlackQueenCodes
                    };
                    textReader.Close();
                }
                else
                {
                    _calibrateData = new CalibrateData()
                    {
                        BasePositionCodes = string.Empty,
                        WhiteQueenCodes = string.Empty,
                        BlackQueenCodes = string.Empty
                    };
                    if (File.Exists(_oldStorageFile))
                    {
                        _calibrateData.BasePositionCodes = File.ReadAllText(_oldStorageFile);
                        if (Save())
                        {
                            File.Delete(_oldStorageFile);
                        }
                    }
                }
            }
            catch
            {
                _calibrateData = new CalibrateData()
                {
                    BasePositionCodes = string.Empty,
                    WhiteQueenCodes = string.Empty,
                    BlackQueenCodes = string.Empty
                };
            }
        }

        /// <inheritdoc />
        public string GetQueensCodes(bool white)
        {
            if (_calibrateData == null)
            {
                Load();
            }

            try
            {
                return white ? _calibrateData.WhiteQueenCodes : _calibrateData.BlackQueenCodes;
            }
            catch
            {
                //
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public bool SaveQueensCodes(bool white, string codes)
        {
            try
            {
                if (_calibrateData == null)
                {
                    Load();
                }

                if (white)
                {
                    _calibrateData.WhiteQueenCodes = codes;
                }
                else
                {
                    _calibrateData.BlackQueenCodes = codes;
                }

                return Save();
            }
            catch
            {
                //
            }

            return false;
        }
    }
}
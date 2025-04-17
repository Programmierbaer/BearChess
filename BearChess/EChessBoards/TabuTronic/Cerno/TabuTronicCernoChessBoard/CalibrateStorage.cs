using System.IO;
using System.Xml.Serialization;

namespace  www.SoLaNoSoft.com.BearChess.Tabutronic.Cerno.ChessBoard
{
    public class CalibrateStorage : ICalibrateStorage
    {
        private readonly string _oldStorageFile = "calibrate.dat";
        private readonly string _storageFile = "calibrate.xml";

        private CalibrateData _calibrateData;

        public CalibrateStorage(string basePath)
        {
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

        public void DeleteCalibrationData()
        {
            if (File.Exists(_storageFile))
            {
                try
                {
                    File.Delete(_storageFile);
                }
                catch
                {
                    //
                }
            }
        }


        private void Load()
        {
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
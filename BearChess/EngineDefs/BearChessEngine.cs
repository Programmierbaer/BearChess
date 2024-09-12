using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase;

namespace www.SoLaNoSoft.com.BearChess.Engine
{
    public static class BearChessEngine
    {

        private static void InstallInternalBearChessEngine(string binPath, string uciPath)
        {
            try
            {
                string sourcePath = Path.Combine(binPath, Constants.InternalBearChessEngineGUID);
                if (!Directory.Exists(sourcePath))
                {
                    return;
                }
                string targetPath = Path.Combine(uciPath, Constants.InternalBearChessEngineGUID);
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                var dir = new DirectoryInfo(sourcePath);
                foreach (FileInfo file in dir.GetFiles())
                {
                    string targetFilePath = Path.Combine(targetPath, file.Name);
                    if (file.Extension.Equals(".uci"))
                    {
                        var serializer = new XmlSerializer(typeof(UciInfo));
                        TextReader textReader = new StreamReader(file.FullName);
                        var origConfig = (UciInfo)serializer.Deserialize(textReader);
                        origConfig.FileName = Path.Combine(binPath, Constants.InternalBearChessEngineGUID, Constants.InternalChessEngineFileName);
                        origConfig.LogoFileName = Path.Combine(binPath, Constants.InternalBearChessEngineGUID, Constants.InternalChessEngineLogoFileName);
                        origConfig.IsInternalBearChessEngine = true;
                        origConfig.IsInternalChessEngine = false;
                        textReader.Close();
                        TextWriter textWriter = new StreamWriter(file.FullName, false);
                        serializer.Serialize(textWriter, origConfig);
                        textWriter.Close();

                    }
                    file.CopyTo(targetFilePath, true);
                }
            }
            catch
            {
                //
            }
        }

        private static void InstallInternalChessEngine(string binPath, string uciPath)
        {
            try
            {
                string sourcePath = Path.Combine(binPath, Constants.InternalChessEngineGUID);
                if (!Directory.Exists(sourcePath))
                {
                    return;
                }
                string targetPath = Path.Combine(uciPath, Constants.InternalChessEngineGUID);
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                var dir = new DirectoryInfo(sourcePath);
                foreach (FileInfo file in dir.GetFiles())
                {
                    string targetFilePath = Path.Combine(targetPath, file.Name);
                    if (file.Extension.Equals(".uci"))
                    {
                        var serializer = new XmlSerializer(typeof(UciInfo));
                        TextReader textReader = new StreamReader(file.FullName);
                        var origConfig = (UciInfo)serializer.Deserialize(textReader);
                        origConfig.FileName = Path.Combine(binPath, Constants.InternalChessEngineGUID, Constants.InternalChessEngineFileName);
                        origConfig.LogoFileName = Path.Combine(binPath, Constants.InternalChessEngineGUID, Constants.InternalChessEngineLogoFileName);
                        origConfig.IsInternalChessEngine = true;
                        origConfig.IsInternalBearChessEngine = false;
                        textReader.Close();
                        TextWriter textWriter = new StreamWriter(file.FullName, false);
                        if (File.Exists(targetFilePath))
                        {
                            textReader = new StreamReader(targetFilePath);
                            var savedConfig = (UciInfo)serializer.Deserialize(textReader);
                            textReader.Close();
                            savedConfig.IsInternalChessEngine = true;
                            savedConfig.IsInternalBearChessEngine = false;
                            savedConfig.FileName = origConfig.FileName;
                            savedConfig.LogoFileName = origConfig.LogoFileName;
                            serializer.Serialize(textWriter, savedConfig);
                        }
                        else
                        {
                            serializer.Serialize(textWriter, origConfig);
                        }

                        textWriter.Close();

                    }
                    file.CopyTo(targetFilePath, true);
                }
            }
            catch
            {
                //
            }
        }


        public static void InstallBearChessEngine(string binPath,  string uciPath)
        {
           InstallInternalBearChessEngine(binPath, uciPath);
           InstallInternalChessEngine(binPath, uciPath);
        }
    }
}

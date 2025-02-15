using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase.Definitions;
using www.SoLaNoSoft.com.BearChessBase.Implementations.CTG;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public static class OpeningBookLoader
    {
        private static string _bookPath;
        private static string _binPath;
        private static Dictionary<string, BookInfo> _installedBooks;


        public static void Init(string bookPath, string binPath, FileLogger fileLogger)
        {
            _bookPath = bookPath;
            _binPath = binPath;
            _installedBooks = new Dictionary<string, BookInfo>();
            InstallInternalBook(fileLogger);
            InstallInternalHiddenBook(fileLogger);
            ReadInstalledBooks(fileLogger);
        }

        public static OpeningBook LoadBook(string bookName, bool checkFile)
        {
            var openingBook = new OpeningBook();
            if (openingBook.LoadBook(_installedBooks[bookName].FileName, checkFile))
            {
                return openingBook;
            }

            return null;
        }

        public static string[] GetInstalledBooks()
        {
            return _installedBooks.Keys.ToArray();
        }

        public static BookInfo[] GetInstalledBookInfos()
        {
            return _installedBooks.Values.ToArray();
        }

        private static void InstallInternalBook(FileLogger fileLogger)
        {
            try
            {
                var sourcePath = Path.Combine(_binPath, Constants.InternalBookGUIDPerfectCTG);
                if (!Directory.Exists(sourcePath))
                {
                    return;
                }

                var sourceFile = Path.Combine(_binPath, Constants.InternalBookGUIDPerfectCTG, $"{Constants.InternalBookGUIDPerfectCTG}.book");
                if (!File.Exists(sourceFile))
                {
                    return;
                }

                var targetFile = Path.Combine(_bookPath, $"{Constants.InternalBookGUIDPerfectCTG}.book");
                var file = new FileInfo(sourceFile);
                var serializer = new XmlSerializer(typeof(BookInfo));
                TextReader textReader = new StreamReader(file.FullName);
                var origConfig = (BookInfo)serializer.Deserialize(textReader);
                origConfig.FileName =
                    Path.Combine(_binPath, Constants.InternalBookGUIDPerfectCTG, Constants.InternalBookFileNamePerfectCTG);
                origConfig.Name = "Perfect 2023";
                origConfig.IsInternalBook = true;
                origConfig.IsHidddenInternalBook = false;
                textReader.Close();
                TextWriter textWriter = new StreamWriter(file.FullName, false);
                serializer.Serialize(textWriter, origConfig);
                textWriter.Close();
                File.Copy(sourceFile, targetFile, true);
            }
            catch (Exception ex)
            {
                fileLogger?.LogError(ex);
            }
        }

        private static void InstallInternalHiddenBook(FileLogger fileLogger)
        {
            try
            {
                var sourcePath = Path.Combine(_binPath, Constants.InternalBookGUIDPerfectBIN);
                if (!Directory.Exists(sourcePath))
                {
                    return;
                }

                var sourceFile = Path.Combine(_binPath, Constants.InternalBookGUIDPerfectBIN, $"{Constants.InternalBookGUIDPerfectBIN}.book");
                if (!File.Exists(sourceFile))
                {
                    return;
                }

                var targetFile = Path.Combine(_bookPath, $"{Constants.InternalBookGUIDPerfectBIN}.book");
                var file = new FileInfo(sourceFile);
                var serializer = new XmlSerializer(typeof(BookInfo));
                TextReader textReader = new StreamReader(file.FullName);
                var origConfig = (BookInfo)serializer.Deserialize(textReader);
                origConfig.FileName =
                    Path.Combine(_binPath, Constants.InternalBookGUIDPerfectBIN, Constants.InternalBookFileNamePerfectBIN);
                origConfig.Name = "Perfect 2023 Polyglot";
                origConfig.IsInternalBook = true;
                origConfig.IsHidddenInternalBook = true;
                textReader.Close();
                TextWriter textWriter = new StreamWriter(file.FullName, false);
                serializer.Serialize(textWriter, origConfig);
                textWriter.Close();
                File.Copy(sourceFile, targetFile, true);
            }
            catch (Exception ex)
            {
                fileLogger?.LogError(ex);
            }
        }

        private static void ReadInstalledBooks(FileLogger fileLogger)
        {
            try
            {
                int invalidBooks = 0;
                fileLogger?.LogInfo("Read installed books...");
                _installedBooks.Clear();
                var fileNames = Directory.GetFiles(_bookPath, "*.book", SearchOption.TopDirectoryOnly);
                foreach (var fileName in fileNames)
                {
                    try
                    {
                        fileLogger?.LogInfo($"  File {fileName}");
                        var serializer = new XmlSerializer(typeof(BookInfo));
                        TextReader textReader = new StreamReader(fileName);
                        var savedBook = (BookInfo)serializer.Deserialize(textReader);
                        if (!File.Exists(savedBook.FileName))
                        {
                            fileLogger?.LogWarning($"  Book file {savedBook.FileName} not found");
                            invalidBooks++;
                            continue;
                        }

                        if (_installedBooks.ContainsKey(savedBook.Name))
                        {
                            fileLogger?.LogWarning($" Book file {savedBook.Name} already installed");
                            invalidBooks++;
                            continue;
                        }

                        fileLogger?.LogInfo($"   Add book {savedBook.FileName} as {savedBook.Name}");
                        _installedBooks.Add(savedBook.Name, savedBook);
                    }
                    catch (Exception ex)
                    {
                        fileLogger?.LogError(ex);
                    }
                }

                fileLogger?.LogInfo($" {_installedBooks.Count} books read");
                if (invalidBooks > 0)
                {
                    fileLogger?.LogWarning($" {invalidBooks} books could not read");
                }
            }
            catch (Exception ex)
            {
                fileLogger?.LogError("Read installed books", ex);
            }
        }
    }
}
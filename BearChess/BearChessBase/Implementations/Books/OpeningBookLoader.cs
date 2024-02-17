using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using www.SoLaNoSoft.com.BearChessBase.Implementations.CTG;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    public static class OpeningBookLoader
    {
        private static string _bookPath;
        private static Dictionary<string, BookInfo> _installedBooks;


        public static void Init(string bookPath, FileLogger fileLogger)
        {
            _bookPath = bookPath;
            _installedBooks = new Dictionary<string, BookInfo>();
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
                        fileLogger?.LogInfo($"  Reading {fileName}");
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

                        fileLogger?.LogInfo($"  Add book {savedBook.FileName} as {savedBook.Name}");
                        _installedBooks.Add(savedBook.Name, savedBook);
                    }
                    catch (Exception ex)
                    {
                        fileLogger?.LogError(ex);
                    }
                }

                fileLogger?.LogInfo($"{_installedBooks.Count} books read");
                if (invalidBooks > 0)
                {
                    fileLogger?.LogWarning($"{invalidBooks} books could not read");
                }
            }
            catch (Exception ex)
            {
                fileLogger?.LogError("Read installed books", ex);
            }
        }
    }
}
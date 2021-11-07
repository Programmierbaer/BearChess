using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

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
            fileLogger?.LogInfo("Read installed books...");
            ReadInstalledBooks(fileLogger);
            fileLogger?.LogInfo("---Read installed books");
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
                _installedBooks.Clear();
                var fileNames = Directory.GetFiles(_bookPath, "*.book", SearchOption.TopDirectoryOnly);
                foreach (var fileName in fileNames)
                {
                    fileLogger?.LogInfo($"Reading {fileName}");
                    var serializer = new XmlSerializer(typeof(BookInfo));
                    TextReader textReader = new StreamReader(fileName);
                    var savedBook = (BookInfo)serializer.Deserialize(textReader);
                    if (File.Exists(savedBook.FileName) && !_installedBooks.ContainsKey(savedBook.Name))
                    {
                        fileLogger?.LogInfo($"Add book {savedBook.Name}");
                        _installedBooks.Add(savedBook.Name, savedBook);
                    }
                }
                fileLogger?.LogInfo($" {_installedBooks.Count} books read");

            }
            catch (Exception ex)
            {
                fileLogger?.LogError("Read installed books", ex);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;

namespace www.SoLaNoSoft.com.BearChessTools
{
    public class FileDownload
    {
        public static void Download(string url, string fileName, bool overrideExisting)
        {
            if (!overrideExisting && File.Exists(fileName)) {
                return;
            };
            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, fileName);
                }
            }
            catch
            {
                return;
            }


        }

        public static string UnzipFile(string zipFile, string pathName, bool overrideExisting)
        {
            ZipArchive archive = ZipFile.Open(zipFile, ZipArchiveMode.Read);
            ZipArchiveEntry firstEntry = archive.Entries.FirstOrDefault();
            if (firstEntry != null)
            {

                string fileName = firstEntry.Name;
                string fullName = Path.Combine(pathName, fileName);
                if (File.Exists(fullName))
                {
                    if (!overrideExisting)
                    {
                        throw new Exception($"File {fullName} already exists");
                    }
                    File.Delete(fullName);
                }
                ZipFile.ExtractToDirectory(zipFile, pathName);
                return fileName;
            }
            throw new Exception("Zip file is empty");
        }
    }
}

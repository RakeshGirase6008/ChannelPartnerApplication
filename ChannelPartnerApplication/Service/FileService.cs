using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ChannelPartnerApplication.Service
{
    public class FileService
    {
        public string SaveFile(List<IFormFile> files, string subDirectory)
        {
            subDirectory ??= string.Empty;
            var target = Path.Combine(Directory.GetCurrentDirectory() + "\\wwwroot\\", subDirectory);
            Directory.CreateDirectory(target);
            string filePath = string.Empty;
            string retunFilePath = string.Empty;
            if (files != null && files.Count() > 0)
                files.ForEach(async file =>
                {
                    if (file == null || file.Length <= 0) return;
                    filePath = Path.Combine(target, file.FileName);
                    retunFilePath = Path.Combine(subDirectory, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                });
            
            return retunFilePath;
        }

        public (string fileType, byte[] archiveData, string archiveName) FetechFiles(string subDirectory)
        {
            var zipName = $"archive-{DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss")}.zip";

            var files = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory() + "\\", subDirectory)).ToList();

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    files.ForEach(file =>
                    {
                        var theFile = archive.CreateEntry(file);
                        using (var streamWriter = new StreamWriter(theFile.Open()))
                        {
                            streamWriter.Write(File.ReadAllText(file));
                        }

                    });
                }
                return ("application/zip", memoryStream.ToArray(), zipName);
            }
        }

        public static string SizeConverter(long bytes)
        {
            var fileSize = new decimal(bytes);
            var kilobyte = new decimal(1024);
            var megabyte = new decimal(1024 * 1024);
            var gigabyte = new decimal(1024 * 1024 * 1024);

            switch (fileSize)
            {
                case var _ when fileSize < kilobyte:
                    return $"Less then 1KB";
                case var _ when fileSize < megabyte:
                    return $"{Math.Round(fileSize / kilobyte, 0, MidpointRounding.AwayFromZero):##,###.##}KB";
                case var _ when fileSize < gigabyte:
                    return $"{Math.Round(fileSize / megabyte, 2, MidpointRounding.AwayFromZero):##,###.##}MB";
                case var _ when fileSize >= gigabyte:
                    return $"{Math.Round(fileSize / gigabyte, 2, MidpointRounding.AwayFromZero):##,###.##}GB";
                default:
                    return "n/a";
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;

namespace MarketDataRecorder
{
    public class DataExporter
    {
        private readonly string _folderDirectory;

        public DataExporter(string folderDirectory)
        {
            _folderDirectory = folderDirectory;
            if (!Directory.Exists(_folderDirectory))
            {
                Directory.CreateDirectory(_folderDirectory);
            }
        }

        public string Export<T>(IEnumerable<T> items)
        {
            var filename = $"{_folderDirectory}/{Guid.NewGuid()}.csv";
            using (var writer = new StreamWriter(filename))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(items);
            }
            return filename;
        }
    }
}

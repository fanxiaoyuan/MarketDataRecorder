using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using MarketDataRecorder.DataModel;
using NUnit.Framework;

namespace MarketDataRecorder.Tests
{
    [TestFixture]
    public class DataExporterTest
    {
        private DataExporter _objectToTest;
        private string _directory;

        [SetUp]
        public void SetUp()
        {
            _directory = AppDomain.CurrentDomain.BaseDirectory;
            _objectToTest = new DataExporter(_directory);
        }

        [TestCase("snapshot1.csv")]
        public void TestSnapshotExport(string filename)
        {
            Test<SnapshotData>(filename);
        }

        [TestCase("streaming1.csv")]
        public void TestStreamingExport(string filename)
        {
            Test<StreamingData>(filename);
        }

        private void Test<T>(string filename)
        {
            IEnumerable<T> records = null;
            var sampleFilename = $"{_directory}sample\\{filename}";
            var generatedFilename = string.Empty;
            using (var reader = new StreamReader(sampleFilename))
            using (var csv = new CsvReader(reader))
            {
                records = csv.GetRecords<T>();
                generatedFilename = _objectToTest.Export(records);
            }
            Assert.That(File.ReadAllBytes(generatedFilename), Is.EquivalentTo(File.ReadAllBytes(sampleFilename)));
        }
    }
}

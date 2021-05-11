using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRREvaporationReaderTest
    {
        [Test]
        public void ReadTholenFile()
        {
            var text = @"*Name of this file: \SOBEK212\FIXED\THOLEN_2.EVP" + Environment.NewLine +
            @"*Date and time of construction: 27-06-2011   12:39:59" + Environment.NewLine +
            @"*Verdampingsfile" + Environment.NewLine +
            @"*Meteo data: evaporation intensity in mm/day" + Environment.NewLine +
            @"*First record: start date, data in mm/day" + Environment.NewLine +
            @"*Datum (year month day), verdamping (mm/dag) voor elk weerstation" + Environment.NewLine +
            @"*jaar maand dag verdamping[mm]" + Environment.NewLine +
            @" 2009  12  29 0.1" + Environment.NewLine +
            @" 2009  12  30 0.1" + Environment.NewLine +
            @" 2009  12  31 0.1" + Environment.NewLine +
            @" 2010  1  1   0.4" + Environment.NewLine +
            @" 2010  1  2   0.2" + Environment.NewLine +
            @" 2010  1  3   0.4" + Environment.NewLine +
            @" 2010  1  4   0.2" + Environment.NewLine +
            @" 2010  1  5   0.3" + Environment.NewLine +
            @" 2010  1  6   0.4" + Environment.NewLine +
            @" 2010  1  7   0.4" + Environment.NewLine +
            @" 2010  1  8   0.4" + Environment.NewLine +
            @" 2010  1  9   0.2" + Environment.NewLine +
            @" 2010  1  10  0.2" + Environment.NewLine +
            @" 2010  1  11  0.1" + Environment.NewLine +
            @" 2010  1  12  0.1" + Environment.NewLine +
            @" 2010  1  13  0.2" + Environment.NewLine +
            @" 2010  1  14  0.4" + Environment.NewLine +
            @" 2010  1  15  0.1" + Environment.NewLine +
            @" 2010  1  16  0.1" + Environment.NewLine +
            @" 2010  1  17  0.6" + Environment.NewLine +
            @" 2010  1  18  0.2" + Environment.NewLine +
            @" 2010  1  19  0.2" + Environment.NewLine +
            @" 2010  1  20  0.4" + Environment.NewLine +
            @" 2010  1  21  0.3" + Environment.NewLine +
            @" 2010  1  22  0.5" + Environment.NewLine +
            @" 2010  1  23  0.1" + Environment.NewLine +
            @" 2010  1  24  0.1" + Environment.NewLine +
            @" 2010  1  25  0.1" + Environment.NewLine +
            @" 2010  1  26  0.5" + Environment.NewLine +
            @" 2010  1  27  0.2" + Environment.NewLine +
            @" 2010  1  28  0.3" + Environment.NewLine +
            @" 2010  1  29  0.2" + Environment.NewLine +
            @" 2010  1  30  0.8" + Environment.NewLine +
            @" 2010  1  31  0.5";

            var sobekEvaporationTable = SobekRREvaporationReader.ParseEvaporationData(text).FirstOrDefault();

            Assert.IsNotNull(sobekEvaporationTable);
            Assert.AreEqual(34, sobekEvaporationTable.Rows.Count);
            var lastRow = sobekEvaporationTable.Rows[33];
            Assert.AreEqual(2010, (int)lastRow[0]);
            Assert.AreEqual(1, (int)lastRow[1]);
            Assert.AreEqual(31, (int)lastRow[2]);
            Assert.AreEqual(0.5, (double)lastRow[3]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadEvaporationFileTholenPlvExtension()
        {
            string plvFilePath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"FIXED\EVAPOR.PLV");
            var lstEvaporationData = SobekRREvaporationReader.ReadEvaporationData(plvFilePath);
            Assert.AreEqual(366, lstEvaporationData.FirstOrDefault().Rows.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadEvaporationFileTholenGemExtension()
        {
            string gemFilePath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"FIXED\EVAPOR.GEM");
            var lstEvaporationData = SobekRREvaporationReader.ReadEvaporationData(gemFilePath);
            Assert.AreEqual(366, lstEvaporationData.FirstOrDefault().Rows.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadEvaporationFileTholenNoExtensionMultipleStations()
        {
            string filePath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"FIXED\EVAPOR");
            var lstEvaporationData = SobekRREvaporationReader.ReadEvaporationData(filePath);
            Assert.AreEqual(13, lstEvaporationData.FirstOrDefault().Columns.Count);
            Assert.AreEqual(16437, lstEvaporationData.FirstOrDefault().Rows.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadEvaporationFileWithLongtimeAverage()
        {
            string gemFilePath = TestHelper.GetTestFilePath(@"Evaporation\EVAPOR_with_longtime_average.GEM");
            var lstEvaporationData = SobekRREvaporationReader.ReadEvaporationData(gemFilePath);
            var dataTable = lstEvaporationData.FirstOrDefault();
            Assert.NotNull(dataTable);
            Assert.AreEqual(366, dataTable.Rows.Count);
            var startDateTime = DateTime.Today;
            var stopDateTime = startDateTime.AddDays(2);
            lstEvaporationData = SobekRREvaporationReader.ReadEvaporationData(gemFilePath, startDateTime, stopDateTime);
            
            dataTable = lstEvaporationData.FirstOrDefault();
            Assert.NotNull(dataTable);
            Assert.AreEqual(366, dataTable.Rows.Count);
            Assert.That(dataTable.Rows[0]["Year"], Is.EqualTo(startDateTime.Year));
            Assert.That(dataTable.Rows[0]["Month"], Is.EqualTo(startDateTime.Month));
            Assert.That(dataTable.Rows[0]["Day"], Is.EqualTo(startDateTime.Day));

            Assert.That(dataTable.Rows[2]["Year"], Is.EqualTo(stopDateTime.Year));
            Assert.That(dataTable.Rows[2]["Month"], Is.EqualTo(stopDateTime.Month));
            Assert.That(dataTable.Rows[2]["Day"], Is.EqualTo(stopDateTime.Day));
        }
    }
}

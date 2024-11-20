using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class DelwaqMapFileReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestMapFileReaderReadMetaData()
        {
            string mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "deltashell.map");
            MapFileMetaData mapFileMetaData = DelwaqMapFileReader.ReadMetaData(mapFilePath);

            Assert.AreEqual(25, mapFileMetaData.NumberOfTimeSteps);
            Assert.AreEqual(2, mapFileMetaData.NumberOfSegments);
            Assert.AreEqual(6, mapFileMetaData.NumberOfSubstances);
            Assert.AreEqual(288, mapFileMetaData.DataBlockOffsetInBytes);

            Assert.AreEqual("Salinity", mapFileMetaData.Substances[0]);
            Assert.AreEqual("Temperature", mapFileMetaData.Substances[1]);
            Assert.AreEqual("OXY", mapFileMetaData.Substances[2]);
            Assert.AreEqual("AAP", mapFileMetaData.Substances[3]);
            Assert.AreEqual("SOD", mapFileMetaData.Substances[4]);
            Assert.AreEqual("IM1S1", mapFileMetaData.Substances[5]);

            Assert.AreEqual(mapFileMetaData.NumberOfTimeSteps, mapFileMetaData.Times.Count);
            Assert.IsTrue(mapFileMetaData.Times[0].CompareTo(new DateTime(2010, 1, 1, 0, 0, 0)) == 0);
            Assert.IsTrue(mapFileMetaData.Times[1].CompareTo(new DateTime(2010, 1, 1, 1, 0, 0)) == 0);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestMapFileReaderReadTimeStep()
        {
            string mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "deltashell.map");
            MapFileMetaData mapFileMetaData = DelwaqMapFileReader.ReadMetaData(mapFilePath);
            List<double> valuesSalinity = DelwaqMapFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 6, "Salinity");

            Assert.AreEqual(2, valuesSalinity.Count);
            Assert.AreEqual(19.767536163330078, valuesSalinity[0]);
            Assert.AreEqual(27.733558654785156, valuesSalinity[1]);

            List<double> valuesOxy = DelwaqMapFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 6, "OXY");

            Assert.AreEqual(2, valuesOxy.Count);
            Assert.AreEqual(1.97675359249115, valuesOxy[0]);
            Assert.AreEqual(2.7733557224273682, valuesOxy[1]);

            // Check the substance values for the last time step for two random substances
            valuesSalinity = DelwaqMapFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 24, "Salinity");

            Assert.AreEqual(2, valuesSalinity.Count);
            Assert.AreEqual(5.6551790237426758, valuesSalinity[0]);
            Assert.AreEqual(14.770989418029785, valuesSalinity[1]);

            valuesOxy = DelwaqMapFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 24, "OXY");

            Assert.AreEqual(2, valuesOxy.Count);
            Assert.AreEqual(0.56551802158355713, valuesOxy[0]);
            Assert.AreEqual(1.4770994186401367, valuesOxy[1]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestMapFileReaderReadTimeStepForOneSegment()
        {
            string mapFilePath = Path.Combine(TestHelper.GetTestDataDirectory(), "IO", "deltashell.map");
            MapFileMetaData mapFileMetaData = DelwaqMapFileReader.ReadMetaData(mapFilePath);
            List<double> valuesSalinity = DelwaqMapFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 6, "Salinity", 1);

            Assert.AreEqual(1, valuesSalinity.Count);
            Assert.AreEqual(27.733558654785156, valuesSalinity[0]);

            List<double> valuesOxy = DelwaqMapFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 6, "OXY", 1);

            Assert.AreEqual(1, valuesOxy.Count);
            Assert.AreEqual(2.7733557224273682, valuesOxy[0]);

            // Check the substance values for the last time step for two random substances
            valuesSalinity = DelwaqMapFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 24, "Salinity", 0);

            Assert.AreEqual(1, valuesSalinity.Count);
            Assert.AreEqual(5.6551790237426758, valuesSalinity[0]);

            valuesOxy = DelwaqMapFileReader.GetTimeStepData(mapFilePath, mapFileMetaData, 24, "OXY", 0);

            Assert.AreEqual(1, valuesOxy.Count);
            Assert.AreEqual(0.56551802158355713, valuesOxy[0]);
        }
    }
}
using System;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRIniSettingsReaderTest
    {
        [Test]
        public void ImportSettingsTholen()
        {
            string path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly, @"Tholen.lit\29\DELFT_3B.INI");
            SobekRRIniSettings settings = new SobekRRIniSettingsReader().GetSobekRRIniSettings(path);

            // General settings
            Assert.AreEqual(new DateTime(2006, 12, 22), settings.StartTime);
            Assert.AreEqual(new DateTime(2007, 2, 6), settings.EndTime);
            Assert.AreEqual(new TimeSpan(0, 0, 15, 0), settings.TimestepSize);
            Assert.AreEqual(true, settings.PeriodFromEvent);
            Assert.AreEqual(1, settings.OutputTimestepMultiplier);
            Assert.AreEqual(-1, settings.UnsaturatedZone);
            Assert.AreEqual(1994, settings.GreenhouseYear);
            Assert.AreEqual(1, settings.InitCapsimOption);
            Assert.AreEqual(false, settings.CapsimPerCropAreaIsDefined);
            Assert.AreEqual(0, settings.CapsimPerCropArea);

            // Output settings
            Assert.AreEqual(2, settings.AggregationOptions);
            Assert.AreEqual(true, settings.OutputRRPaved);
            Assert.AreEqual(true, settings.OutputRRUnpaved);
            Assert.AreEqual(false, settings.OutputRRGreenhouse);
            Assert.AreEqual(false, settings.OutputRROpenWater);
            Assert.AreEqual(false, settings.OutputRRStructure);
            Assert.AreEqual(true, settings.OutputRRBoundary);
            Assert.AreEqual(false, settings.OutputRRNWRW);
            Assert.AreEqual(false, settings.OutputRRWWTP);
            Assert.AreEqual(false, settings.OutputRRIndustry);
            Assert.AreEqual(false, settings.OutputRRSacramento);
            Assert.AreEqual(false, settings.OutputRRRunoff);
            Assert.AreEqual(true, settings.OutputRRLinkFlows);
            Assert.AreEqual(true, settings.OutputRRBalance);
        }
    }
}
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
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowFMModelImporterTest).Assembly,@"Tholen.lit\29\DELFT_3B.INI");
            var settings = new SobekRRIniSettingsReader().GetSobekRRIniSettings(path);

            Assert.AreEqual(true, settings.PeriodFromEvent);
            Assert.AreEqual(1, settings.OutputTimestepMultiplier);
            Assert.AreEqual(new DateTime(2006,12,22),settings.StartTime);
            Assert.AreEqual(new DateTime(2007,2,6), settings.EndTime);
            Assert.AreEqual(new TimeSpan(0,0,15,0), settings.TimestepSize);
            Assert.AreEqual(-1, settings.UnsaturatedZone);
            Assert.AreEqual(1, settings.InitCapsimOption);
            Assert.AreEqual(0, settings.CapsimPerCropArea);
            Assert.AreEqual(1994, settings.GreenhouseYear);
        }
    }
}
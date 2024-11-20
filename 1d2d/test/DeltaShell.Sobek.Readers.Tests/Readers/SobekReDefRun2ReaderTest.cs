using System;
using System.Linq;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekReDefRun2ReaderTest
    {
        [Test]
        public void ParseDefRun2RecordWithoutXr()
        {
            var source =
                @"FLNM g_ 19.81 th 0.55 ps 0.5 rh 1000 ur 0.5 mi 50 sw 0.01 sd 0.1 cm 1 er 0 us 1" + Environment.NewLine +
                    @" in 0.001 pc 1000 xn 50 sm 0.01 dt 1 flnm";
            var sobekReDefRun2Reader = new SobekReDefRun2Reader {SobekCaseSettingsInstance = new SobekCaseSettings()};
            var settings = sobekReDefRun2Reader.Parse(source).First();
            sobekReDefRun2Reader.ApplyAdaptedSettingsToExistingSettings(settings);
            Assert.AreEqual(19.81, sobekReDefRun2Reader.SobekCaseSettingsInstance.GravityAcceleration);
            Assert.AreEqual(false, sobekReDefRun2Reader.SobekCaseSettingsInstance.UseKsiForExtraResistance);
        }

        [Test]
        public void ParseDefRun2RecordWithXr()
        {
            var source =
                @"FLNM g_ 29.81 th 0.55 ps 0.5 rh 1000 ur 0.5 mi 50 sw 0.01 sd 0.1 cm 1 er 0 us 1" + Environment.NewLine +
                    @" in 0.001 pc 1000 xn 50 sm 0.01 dt 1 xr 1 flnm";

            var sobekReDefRun2Reader = new SobekReDefRun2Reader { SobekCaseSettingsInstance = new SobekCaseSettings() };
            var settings = sobekReDefRun2Reader.Parse(source).First();
            sobekReDefRun2Reader.ApplyAdaptedSettingsToExistingSettings(settings);
            Assert.AreEqual(29.81, sobekReDefRun2Reader.SobekCaseSettingsInstance.GravityAcceleration);
            // 0 = ksi, 1 = eta
            Assert.AreEqual(false, sobekReDefRun2Reader.SobekCaseSettingsInstance.UseKsiForExtraResistance);
        }
    }
}

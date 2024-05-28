using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekReDefRun1ReaderTest
    {
        [Test]
        public void ParseDefRun1Record()
        {
            const string source = 
                @"FLTM bt '2000/01/01;00:00:00' et '2000/02/02;00:00:00' ct '01:10:10' cd 0 tp '12:25:00' dp 0 ba '' tt 75 tf 1 nt 1 if 0 im 1 ri 0 fltm";

            var settings = SobekReDefRun1Reader.ParseSobekCaseSettings(source);
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0), settings.StartTime);
            Assert.AreEqual(new DateTime(2000, 2, 2, 0, 0, 0), settings.StopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 10, 10), settings.TimeStep);
            Assert.AreEqual(new TimeSpan(0, 1, 10, 10), settings.OutPutTimeStep);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ParseDefRun1File()
        {
            var settingsFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"ReModels\RIJN301.SBK\8\defrun.1");
            var settings = new SobekReDefRun1Reader().Read(settingsFile).First();
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0), settings.StartTime);
            Assert.AreEqual(new DateTime(2000, 2, 1, 0, 0, 0), settings.StopTime);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), settings.TimeStep);
            Assert.AreEqual(new TimeSpan(0, 1, 0, 0), settings.OutPutTimeStep);
        }
    }
}
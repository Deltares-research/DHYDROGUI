using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class FmCrossSectionDefinitionExporterTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void GivenArrayOfCrossSectionDefinitions_WhenWritingToFile_ThenIniFileIsWritten()
        {
            var filePath = Path.Combine(FileUtils.CreateTempDirectory(), "crsdef.ini");
            var network = new HydroNetwork();
            network.SharedCrossSectionDefinitions.Add(new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle()));
            FmCrossSectionDefinitionWriter.WriteFile(filePath, network);

            Assert.That(File.Exists(filePath));
        }
    }
}
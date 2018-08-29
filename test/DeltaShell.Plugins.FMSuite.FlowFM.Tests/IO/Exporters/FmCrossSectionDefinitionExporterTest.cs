using System.IO;
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
        public void GivenArrayOfCrossSectionDefinitions_WhenExporting_ThenIniFileIsWritten()
        {
            var filePath = Path.Combine(FileUtils.CreateTempDirectory(), "crsdef.ini");
            var crossSectionDefinitions = new[] { new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle())};
            FmCrossSectionDefinitionExporter.Export(filePath, crossSectionDefinitions);

            Assert.That(File.Exists(filePath));
        }
    }
}
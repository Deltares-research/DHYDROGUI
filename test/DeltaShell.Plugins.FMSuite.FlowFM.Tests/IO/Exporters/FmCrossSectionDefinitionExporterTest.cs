using System.IO;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using NUnit.Framework;

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
            var fmModel = new WaterFlowFMModel();
            //fmModel.Network.SharedCrossSectionDefinitions.Add(new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle()));
            CrossSectionDefinitionFileWriter.WriteFile(filePath, fmModel.Network, fmModel.RoughnessSections);

            Assert.That(File.Exists(filePath));
        }

        
    }
}
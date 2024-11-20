using System.IO;
using System.Linq;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Networks;
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
            var filePath = Path.Combine(FileUtils.CreateTempDirectory(), FeatureFile1D2DConstants.DefaultCrossDefFileName);

            var fmModel = new WaterFlowFMModel();
            var channelFrictionDefinitionPerBranchLookup = fmModel.ChannelFrictionDefinitions.ToDictionary(cfd => (IBranch) cfd.Channel, cfd => cfd);

            CrossSectionDefinitionFileWriter.WriteFile(filePath, fmModel.Network, branch =>
                {
                    var channelFrictionDefinition = channelFrictionDefinitionPerBranchLookup[branch];

                    return channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.RoughnessSections
                           || channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.CrossSectionFrictionDefinitions;
                }, "Channels");

            Assert.That(File.Exists(filePath));
        }
    }
}
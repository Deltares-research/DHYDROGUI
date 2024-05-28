using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Tests.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FeatureFile1D2DWriterTest
    {
        private const string CrossSectionDefinitionFileName = FeatureFile1D2DWriter.CROSS_SECTION_DEFINITION_FILE_NAME;
        private const string CrossSectionLocationFileName = FeatureFile1D2DWriter.CROSS_SECTION_LOCATION_FILE_NAME;
        private const string StructuresFileName = FeatureFile1D2DWriter.STRUCTURES_FILE_NAME;
        private const string NodeFileName = FeatureFile1D2DWriter.NODE_FILE_NAME;

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWithSewerNetworkWithCompartments_WhenWritingMduFile_ThenNodeFileIsWrittenAndIsReferencedInTheMduFile()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var nodeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, NodeFileName);

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };

            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<ICompartment> { new Compartment("myCompartment") }
            };
            fmModel.Network.Nodes.Add(manhole);

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            Assert.IsTrue(File.Exists(nodeFilePath));

            var nodeFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StorageNodeFile);
            Assert.That(nodeFileProperty.GetValueAsString(), Is.EqualTo(NodeFileName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWithSewerNetworkWithoutCompartments_WhenWritingMduFileWithNodeFileAlreadyExisting_ThenExistingNodeFileIsDeleted()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var nodeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, NodeFileName);

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.NodeFile, "someText");

            var fileStream = File.Create(nodeFilePath);
            fileStream.Close();

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            Assert.IsFalse(File.Exists(nodeFilePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWith1DNetworkThatHasCrossSections_WhenWritingMduFile_ThenCrossSectionDataIsWrittenToFile()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var crossSectionDefinitionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, CrossSectionDefinitionFileName);
            var crossSectionLocationFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, CrossSectionLocationFileName);

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var crossSectionDefinitionZw = CrossSectionDefinitionZW.CreateDefault();
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(network.Channels.First(), crossSectionDefinitionZw, 50);
            network.SharedCrossSectionDefinitions.Add(crossSectionDefinitionZw);
            Assert.IsNotEmpty(network.SharedCrossSectionDefinitions);

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Network = network
            };

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            Assert.IsTrue(File.Exists(crossSectionDefinitionFilePath), "Cross section definition file was not written");
            Assert.IsTrue(File.Exists(crossSectionLocationFilePath), "Cross section location file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossDefFile);
            var crossSectionLocationFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossLocFile);

            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(CrossSectionDefinitionFileName));
            Assert.That(crossSectionLocationFileProperty.GetValueAsString(), Is.EqualTo(CrossSectionLocationFileName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWithSewerNetworkThatHasCrossSections_WhenWritingMduFile_ThenCrossSectionDataIsWrittenToFile()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var crossSectionDefinitionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, CrossSectionDefinitionFileName);
            var crossSectionLocationFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, CrossSectionLocationFileName);

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOnePipeWithACrossSection();

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Network = network
            };

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            Assert.IsTrue(File.Exists(crossSectionDefinitionFilePath), "Cross section definition file was not written");
            Assert.IsTrue(File.Exists(crossSectionLocationFilePath), "Cross section location file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossDefFile);
            var crossSectionLocationFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossLocFile);

            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(CrossSectionDefinitionFileName));
            Assert.That(crossSectionLocationFileProperty.GetValueAsString(), Is.EqualTo(CrossSectionLocationFileName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWith1DNetworkWithoutCrossSections_WhenWritingMduFile_ThenCrossSectionFilesAreNotWrittenAndMduReferencesAreRemoved()
        {
            var crossSectionDefinitionFileKey = KnownProperties.CrossDefFile;
            var crossSectionLocationFileKey = KnownProperties.CrossLocFile;
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var crossSectionDefinitionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, CrossSectionDefinitionFileName);
            var crossSectionLocationFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, CrossSectionLocationFileName);

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Network = new HydroNetwork()
            };

            fmModel.ModelDefinition.SetModelProperty(crossSectionDefinitionFileKey, "someText");
            fmModel.ModelDefinition.SetModelProperty(crossSectionLocationFileKey, "someText");

            var definitionFileStream = File.Create(crossSectionDefinitionFilePath);
            var locationFileStream = File.Create(crossSectionLocationFilePath);
            definitionFileStream.Close();
            locationFileStream.Close();

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            Assert.IsFalse(File.Exists(crossSectionDefinitionFilePath), "Cross section definition file was written, but should not have been written");
            Assert.IsFalse(File.Exists(crossSectionDefinitionFilePath), "Cross section location file was written, but should not have been written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(crossSectionDefinitionFileKey);
            var crossSectionLocationFileProperty = fmModel.ModelDefinition.GetModelProperty(crossSectionLocationFileKey);

            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(string.Empty));
            Assert.That(crossSectionLocationFileProperty.GetValueAsString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GivenFmModelWithSewerNetworkWithAPump_WhenWritingMduFile_ThenStructureFilesAreWrittenAndMduReferenceIsCorrect()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var structuresFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, StructuresFileName);

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOnePump();

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Network = network
            };

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            Assert.IsTrue(File.Exists(structuresFilePath), "Structures file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile);
            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(StructuresFileName));
        }

        [Test]
        public void GivenFmModelWith1DNetworkWithAPump_WhenWritingMduFile_ThenStructureFilesAreWrittenAndMduReferenceIsCorrect()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var structuresFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, StructuresFileName);

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(new Pump(), network.Channels.First());

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Network = network
            };

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            Assert.IsTrue(File.Exists(structuresFilePath), "Structures file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile);
            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(StructuresFileName));
        }

        [Test]
        public void GivenFmModelWithEmptyNetworkAndEmptyHydroArea_WhenWritingMduFile_ThenMduReferenceToStructuresFileIsRemoved()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.StructuresFile, "someText");

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile);
            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(string.Empty));
        }

        [TestCase(RoughnessDataSet.SewerSectionTypeName)]
        [TestCase(RoughnessDataSet.MainSectionTypeName)]
        public void GivenFmModelWithSewerRoughness_WhenWriting1D2DFeatures_ThenSewerRoughnessFileIsWritten(string roughnessSectionName)
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var sewerRoughnessFileName = $"roughness-{roughnessSectionName}.ini";
            var sewerRoughnessFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, sewerRoughnessFileName);

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var sewerRoughnessSection = fmModel.RoughnessSections.FirstOrDefault(rs => rs.Name == roughnessSectionName);
            Assert.IsNotNull(sewerRoughnessSection);

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);
            Assert.That(File.Exists(sewerRoughnessFilePath), $"{roughnessSectionName} roughness file was not written");
        }

        [TestCase(RoughnessDataSet.SewerSectionTypeName)]
        [TestCase(RoughnessDataSet.MainSectionTypeName)]
        public void GivenFmModelWithSewerRoughness_WhenWriting1D2DFeatures_ThenSewerRoughnessFilesAreReferencedInTheMduFile(string roughnessSectionName)
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var sewerRoughnessFileName = $"roughness-{roughnessSectionName}.ini";

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var sewerRoughnessSection = fmModel.RoughnessSections.FirstOrDefault(rs => rs.Name == roughnessSectionName);
            Assert.IsNotNull(sewerRoughnessSection);

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            var frictionProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.FrictFile);
            Assert.Contains(sewerRoughnessFileName, frictionProperty.GetValueAsString().Split(';'));
        }

        [Test]
        public void GivenFmModelWithChannel_WhenWriting1D2DFeatures_ThenFrictionFileIsReferencedInTheMduFile()
        {
            var expectedFileName = Properties.Resources.Roughness_Main_Channels_Filename;
            var tempFolder = FileUtils.CreateTempDirectory();
            try
            {
                var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
                using (var fmModel = new WaterFlowFMModel() { MduFilePath = mduFilePath })
                {
                    var channel = new Channel();
                    fmModel.Network.Branches.Add(channel);
                    Assert.That(fmModel.Network.Channels.Count(), Is.EqualTo(1));

                    FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);
                    var frictionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.FrictFile);
                    var actualFileNames = frictionFileProperty.GetValueAsString().Split(';');
                    Assert.Contains(expectedFileName, actualFileNames);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }
    }
}
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
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FeatureFile1D2DWriterTest
    {
        private FeatureFile1D2DWriter featureFileWriter;

        [SetUp]
        public void SetUp()
        {
            featureFileWriter = new FeatureFile1D2DWriter(new InitialFieldFile());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWithSewerNetworkWithCompartments_WhenWritingMduFile_ThenNodeFileIsWrittenAndIsReferencedInTheMduFile()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var nodeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, NetworkPropertiesHelper.StorageNodeFileName);

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };

            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<ICompartment> { new Compartment("myCompartment") }
            };
            fmModel.Network.Nodes.Add(manhole);

            featureFileWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            Assert.IsTrue(File.Exists(nodeFilePath));

            var nodeFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StorageNodeFile);
            Assert.That(nodeFileProperty.GetValueAsString(), Is.EqualTo(NetworkPropertiesHelper.StorageNodeFileName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWithSewerNetworkWithoutCompartments_WhenWritingMduFile_ThenNodeFileIsNotWrittenAndMduReferenceIsCleared()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.StorageNodeFile, "someText");

            featureFileWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            var nodeFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StorageNodeFile);

            Assert.That(nodeFileProperty.GetValueAsString(), Is.EqualTo(string.Empty));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWith1DNetworkThatHasCrossSections_WhenWritingMduFile_ThenCrossSectionDataIsWrittenToFile()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var crossSectionDefinitionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, FeatureFile1D2DConstants.DefaultCrossDefFileName);
            var crossSectionLocationFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, FeatureFile1D2DConstants.DefaultCrossLocFileName);

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

            featureFileWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            Assert.IsTrue(File.Exists(crossSectionDefinitionFilePath), "Cross section definition file was not written");
            Assert.IsTrue(File.Exists(crossSectionLocationFilePath), "Cross section location file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossDefFile);
            var crossSectionLocationFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossLocFile);

            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(FeatureFile1D2DConstants.DefaultCrossDefFileName));
            Assert.That(crossSectionLocationFileProperty.GetValueAsString(), Is.EqualTo(FeatureFile1D2DConstants.DefaultCrossLocFileName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWithSewerNetworkThatHasCrossSections_WhenWritingMduFile_ThenCrossSectionDataIsWrittenToFile()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var crossSectionDefinitionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, FeatureFile1D2DConstants.DefaultCrossDefFileName);
            var crossSectionLocationFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, FeatureFile1D2DConstants.DefaultCrossLocFileName);

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOnePipeWithACrossSection();

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Network = network
            };

            featureFileWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            Assert.IsTrue(File.Exists(crossSectionDefinitionFilePath), "Cross section definition file was not written");
            Assert.IsTrue(File.Exists(crossSectionLocationFilePath), "Cross section location file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossDefFile);
            var crossSectionLocationFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossLocFile);

            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(FeatureFile1D2DConstants.DefaultCrossDefFileName));
            Assert.That(crossSectionLocationFileProperty.GetValueAsString(), Is.EqualTo(FeatureFile1D2DConstants.DefaultCrossLocFileName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWith1DNetworkWithoutCrossSections_WhenWritingMduFile_ThenCrossSectionFilesAreNotWrittenAndMduReferencesAreCleared()
        {
            var crossSectionDefinitionFileKey = KnownProperties.CrossDefFile;
            var crossSectionLocationFileKey = KnownProperties.CrossLocFile;
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Network = new HydroNetwork()
            };

            fmModel.ModelDefinition.SetModelProperty(crossSectionDefinitionFileKey, "someText");
            fmModel.ModelDefinition.SetModelProperty(crossSectionLocationFileKey, "someText");

            featureFileWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(crossSectionDefinitionFileKey);
            var crossSectionLocationFileProperty = fmModel.ModelDefinition.GetModelProperty(crossSectionLocationFileKey);

            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(string.Empty));
            Assert.That(crossSectionLocationFileProperty.GetValueAsString(), Is.EqualTo(string.Empty));
        }
        
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWith1DNetworkThatHasObservationPoints_WhenWritingMduFile_ThenObservationPointDataIsWrittenToFile()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var obsFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, FeatureFile1D2DConstants.DefaultObsFileName);

            var model = new WaterFlowFMModel { MduFilePath = mduFilePath };
            var branch = new Channel();
            model.Network.Branches.Add(branch);

            var observationPoint = new ObservationPoint { Geometry = new Point(1, 1) };
            branch.BranchFeatures.Add(observationPoint);

            model.ModelDefinition.SetModelProperty(KnownProperties.ObsFile, "obs_2D_obs.xyn");

            featureFileWriter.Write1D2DFeatures(model.MduFilePath, model.ModelDefinition, model.Network, model.Area, model.RoughnessSections, model.ChannelFrictionDefinitions, model.ChannelInitialConditionDefinitions);

            Assert.IsTrue(File.Exists(obsFilePath), "Cross section definition file was not written");

            var obsFileProperty = model.ModelDefinition.GetModelProperty(KnownProperties.ObsFile);

            Assert.That(obsFileProperty.GetValueAsString(), Is.EqualTo($"obs_2D_obs.xyn {FeatureFile1D2DConstants.DefaultObsFileName}"));
        }
        
        [Test]
        public void GivenFmModelWith1DNetworkWithoutObservationPoints_WhenWritingMduFile_ThenMduReferenceToExistingObsFile2DIsNotCleared()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");

            var model = new WaterFlowFMModel { MduFilePath = mduFilePath };
            model.ModelDefinition.SetModelProperty(KnownProperties.ObsFile, "obs_2D_obs.xyn");

            featureFileWriter.Write1D2DFeatures(model.MduFilePath, model.ModelDefinition, model.Network, model.Area, model.RoughnessSections, model.ChannelFrictionDefinitions, model.ChannelInitialConditionDefinitions);

            var crossSectionDefinitionFileProperty = model.ModelDefinition.GetModelProperty(KnownProperties.ObsFile);
            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo("obs_2D_obs.xyn"));
        }
        
        [Test]
        public void GivenFmModelWithSewerNetworkWithAPump_WhenWritingMduFile_ThenStructureFilesAreWrittenAndMduReferenceIsCorrect()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var structuresFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, FeatureFile1D2DConstants.DefaultStructuresFileName);

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOnePump();

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Network = network
            };

            featureFileWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            Assert.IsTrue(File.Exists(structuresFilePath), "Structures file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile);
            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(FeatureFile1D2DConstants.DefaultStructuresFileName));
        }

        [Test]
        public void GivenFmModelWith1DNetworkWithAPump_WhenWritingMduFile_ThenStructureFilesAreWrittenAndMduReferenceIsCorrect()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var structuresFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, FeatureFile1D2DConstants.DefaultStructuresFileName);

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(new Pump(), network.Channels.First());

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Network = network
            };

            featureFileWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

            Assert.IsTrue(File.Exists(structuresFilePath), "Structures file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile);
            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(FeatureFile1D2DConstants.DefaultStructuresFileName));
        }

        [Test]
        public void GivenFmModelWithEmptyNetworkAndEmptyHydroArea_WhenWritingMduFile_ThenMduReferenceToStructuresFileIsCleared()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.StructuresFile, "someText");

            featureFileWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

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

            featureFileWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);
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

            featureFileWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);

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

                    featureFileWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel.ModelDefinition, fmModel.Network, fmModel.Area, fmModel.RoughnessSections, fmModel.ChannelFrictionDefinitions, fmModel.ChannelInitialConditionDefinitions);
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
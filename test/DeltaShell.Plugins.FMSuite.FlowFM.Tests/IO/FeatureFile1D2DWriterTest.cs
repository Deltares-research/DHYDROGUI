using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Tests.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FeatureFile1D2DWriterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWithSewerNetworkWithCompartments_WhenWritingMduFile_ThenNodeFileIsWrittenAndIsReferencedInTheMduFile()
        {
            var nodeFileName = "nodeFile.ini";
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var nodeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, nodeFileName);

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };

            var manhole = new Manhole("myManhole")
            {
                Compartments = new EventedList<Compartment> { new Compartment("myCompartment") }
            };
            fmModel.Network.Nodes.Add(manhole);

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel);

            Assert.IsTrue(File.Exists(nodeFilePath));

            var nodeFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.NodeFile);
            Assert.That(nodeFileProperty.GetValueAsString(), Is.EqualTo(nodeFileName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWithSewerNetworkWithoutCompartments_WhenWritingMduFile_ThenNodeFileIsNotWrittenAndMduFileDoesNotReferenceANodeFile()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var nodeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, "nodeFile.ini");

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.NodeFile, "someText");

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel);

            Assert.IsFalse(File.Exists(nodeFilePath));

            var nodeFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.NodeFile);
            Assert.That(nodeFileProperty.GetValueAsString(), Is.EqualTo(string.Empty));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWithSewerNetworkWithoutCompartments_WhenWritingMduFileWithNodeFileAlreadyExisting_ThenExistingNodeFileIsDeleted()
        {
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var nodeFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, "nodeFile.ini");

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.NodeFile, "someText");

            var fileStream = File.Create(nodeFilePath);
            fileStream.Close();

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel);

            Assert.IsFalse(File.Exists(nodeFilePath));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWith1DNetworkThatHasCrossSections_WhenWritingMduFile_ThenCrossSectionDataIsWrittenToFile()
        {
            var crossSectionDefinitionFileName = "crsdef.ini";
            var crossSectionLocationFileName = "crsloc.ini";
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var crossSectionDefinitionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, crossSectionDefinitionFileName);
            var crossSectionLocationFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, crossSectionLocationFileName);

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

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel);

            Assert.IsTrue(File.Exists(crossSectionDefinitionFilePath), "Cross section definition file was not written");
            Assert.IsTrue(File.Exists(crossSectionLocationFilePath), "Cross section location file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossDefFile);
            var crossSectionLocationFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossLocFile);

            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(crossSectionDefinitionFileName));
            Assert.That(crossSectionLocationFileProperty.GetValueAsString(), Is.EqualTo(crossSectionLocationFileName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWithSewerNetworkThatHasCrossSections_WhenWritingMduFile_ThenCrossSectionDataIsWrittenToFile()
        {
            var crossSectionDefinitionFileName = "crsdef.ini";
            var crossSectionLocationFileName = "crsloc.ini";
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var crossSectionDefinitionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, crossSectionDefinitionFileName);
            var crossSectionLocationFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, crossSectionLocationFileName);

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOnePipeWithACrossSection();

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Network = network
            };

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel);

            Assert.IsTrue(File.Exists(crossSectionDefinitionFilePath), "Cross section definition file was not written");
            Assert.IsTrue(File.Exists(crossSectionLocationFilePath), "Cross section location file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossDefFile);
            var crossSectionLocationFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.CrossLocFile);

            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(crossSectionDefinitionFileName));
            Assert.That(crossSectionLocationFileProperty.GetValueAsString(), Is.EqualTo(crossSectionLocationFileName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenFmModelWith1DNetworkWithoutCrossSections_WhenWritingMduFile_ThenCrossSectionFilesAreNotWrittenAndMduReferencesAreRemoved()
        {
            var crossSectionDefinitionFileKey = KnownProperties.CrossDefFile;
            var crossSectionLocationFileKey = KnownProperties.CrossLocFile;
            var crossSectionDefinitionFileName = "crsdef.ini";
            var crossSectionLocationFileName = "crsloc.ini";
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var crossSectionDefinitionFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, crossSectionDefinitionFileName);
            var crossSectionLocationFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, crossSectionLocationFileName);

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

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel);

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
            var structureFileName = "structures.ini";
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var structuresFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, structureFileName);

            var network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOnePump();

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Network = network
            };

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel);

            Assert.IsTrue(File.Exists(structuresFilePath), "Structures file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile);
            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(structureFileName));
        }

        [Test]
        public void GivenFmModelWith1DNetworkWithAPump_WhenWritingMduFile_ThenStructureFilesAreWrittenAndMduReferenceIsCorrect()
        {
            var structureFileName = "structures.ini";
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var structuresFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, structureFileName);

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(new Pump(), network.Channels.First());

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Network = network
            };

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel);

            Assert.IsTrue(File.Exists(structuresFilePath), "Structures file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile);
            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(structureFileName));
        }

        [Test]
        public void GivenFmModelWith2DPump_WhenWritingMduFile_ThenStructureFilesAreWrittenAndMduReferenceIsCorrect()
        {
            var pumpName = "my2DPump";
            var structureFileName = "structures.ini";
            var polylineFileName = $"{pumpName}.pli";
            var tempFolder = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(tempFolder, "myModel.mdu");
            var structuresFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, structureFileName);
            var pliFilePath = IoHelper.GetFilePathToLocationInSameDirectory(mduFilePath, polylineFileName);

            var pump2D = new Pump2D(pumpName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(10, 10) })
            };
            var area = new HydroArea();
            area.Pumps.Add(pump2D);

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                Area = area
            };

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel);

            Assert.IsTrue(File.Exists(structuresFilePath), "Structures file was not written");
            Assert.IsTrue(File.Exists(pliFilePath), "Polyline file was not written");

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile);
            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(structureFileName));
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

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel);

            var crossSectionDefinitionFileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile);
            Assert.That(crossSectionDefinitionFileProperty.GetValueAsString(), Is.EqualTo(string.Empty));
        }

        [TestCase("Sewer")]
        [TestCase("Main")]
        public void GivenFmModelWithSewerRoughness_WhenWritingMduFile_ThenSewerRoughnessFileIsWritten(string roughnessSectionName)
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

            FeatureFile1D2DWriter.Write1D2DFeatures(fmModel.MduFilePath, fmModel);

            Assert.That(File.Exists(sewerRoughnessFilePath), $"{roughnessSectionName} roughness file was not written");

            var roughnessModelProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.RoughnessFile);
            Assert.Contains(sewerRoughnessFileName, roughnessModelProperty.GetValueAsString().Split(' '));
        }
    }
}
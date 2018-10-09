using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Tests.Helpers;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor.Tests.Helpers;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    public class WaterFlowFMModelWriterReaderTest
    {
        private string tempDirectory;

        [SetUp]
        public void SetUp()
        {
            tempDirectory = FileUtils.CreateTempDirectory();
        }

        [TearDown]
        public void TearDown()
        {
            FileUtils.DeleteIfExists(tempDirectory);
        }

        [Test]
        public void GivenOnePipeTwoManholes_WhenWritingAndReading_ThenNetworksAreTheSame()
        {
            var pipeName = "myPipe";
            var mduPath = Path.Combine(tempDirectory, "FlowFM.mdu");
            var model = new WaterFlowFMModel
            {
                MduFilePath = mduPath
            };

            var geometry = new LineString(new[]{ new Coordinate(0, 0), new Coordinate(0, 100) });
            SewerFactory.AddDefaultPipeToNetwork(new Pipe { Name = pipeName, Geometry = geometry, Material = SewerProfileMapping.SewerProfileMaterial.Polyester}, model.Network);

            //action write
            WaterFlowFMModelWriter.Write(model);

            var ugridPath = Path.Combine(tempDirectory, model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).GetValueAsString());
            Assert.True(File.Exists(ugridPath)); //UGrid file

            var retrievedFmModel = WaterFlowFMModelReader.Read(model.MduFilePath);
            var retrievedNetwork = retrievedFmModel.Network;
            
            HydroNetworkTestHelper.CompareNetworks(model.Network, retrievedNetwork);
            HydroNetworkTestHelper.CompareDiscretisations(model.NetworkDiscretization, retrievedFmModel.NetworkDiscretization);
        }

        [Test]
        public void GivenFmModel_WhenWritingFmModel_ThenCorrectFilesAreWritten()
        {
            var mduFilePath = Path.Combine(tempDirectory, "myFmModel.mdu");
            var fmModel = new WaterFlowFMModel(mduFilePath)
            {
                Network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOneOrifice()
            };

            WaterFlowFMModelWriter.Write(fmModel);
        }

        [Test]
        public void GivenFmModelWith1DRoughness_WhenWritingFmModel_ThenCorrectFilesAreWritten()
        {
            var mduFilePath = Path.Combine(tempDirectory, "myFmModel.mdu");
            var fmModel = new WaterFlowFMModel(mduFilePath)
            {
                Network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOneOrifice()
            };
            var sewerRoughness = fmModel.RoughnessSections.FirstOrDefault(rs => rs.Name == "Sewer");
            Assert.IsNotNull(sewerRoughness);
            var branch1 = fmModel.Network.Branches.OfType<SewerConnection>().FirstOrDefault();
            Assert.IsNotNull(branch1);
            var waterLevelfunction = RoughnessSection.DefineFunctionOfH();
            waterLevelfunction[0.0, 0.0] = 4.1;
            waterLevelfunction[0.0, 1000.0] = 5.1;
            waterLevelfunction[0.0, 5000.0] = 6.1;
            waterLevelfunction[0.0, 10000.0] = 5.1;

            // add functions
            sewerRoughness.AddHRoughnessFunctionToBranch(branch1, waterLevelfunction);
            /*var functionOfQ = new Function("functionOfQ");
            functionOfQ.Arguments.Add(new Variable<double>() { Values = new MultiDimensionalArray<double>() { 2.0, 6.0, 10.0 } });
            functionOfQ.Components.Add(new Variable<double>() { Values = new MultiDimensionalArray<double>() { 3.0, 5.0, 7.0 } });

            sewerRoughness.AddQRoughnessFunctionToBranch(branch1, functionOfQ);*/


            WaterFlowFMModelWriter.Write(fmModel);
            var directory = Path.GetDirectoryName(fmModel.MduFilePath);
            Assert.IsTrue(File.Exists(Path.Combine(directory, "roughness-" + sewerRoughness.Name+".ini")));
        }

        [Test, Ignore("Issue FM1D2D-189")]
        public void GivenFmModel_With1DAnd2DStructures_CheckStructureIniFile()
        {
            var mduFilePath = Path.Combine(tempDirectory, "myFmModel.mdu");

            var network = new HydroNetwork();
            var fmModel = new WaterFlowFMModel(mduFilePath)
            {
                Network = network
            };

            //1d pump branch
            var fromNode = new HydroNode("from") {Geometry = new Point(0, 0)};
            var toNode = new HydroNode("to") { Geometry = new Point(100, 0) };
            network.Nodes.Add(fromNode);
            network.Nodes.Add(toNode);
            var channel = new Channel("channel",fromNode, toNode);
            network.Branches.Add(channel);
            var pump1 = new Pump("pump1");
            NetworkHelper.AddBranchFeatureToBranch(pump1, channel, 50.0);

            Assert.IsTrue(network.Pumps.Any(p => p.Name.Equals("pump1")));

            //1d pump manhole
            var manhole = SewerFactory.CreateDefaultManholeAndAddToNetwork(network, new Coordinate(0, 10));
            //var pump2 = SewerFactory.CreateConnectionWithStructure<Pump>(manhole); why can't i use it? why two IManholes? mmm
            var fromCompartment = new Compartment("cmp1");
            var toCompartment = new Compartment("cmp2");
            manhole.Compartments.Add(fromCompartment);
            manhole.Compartments.Add(toCompartment);
            network.Nodes.Add(manhole);

            var pump2 = new Pump("pump2");

            var pumpConnection = new SewerConnection("pumpConnection")
            {
                SourceCompartment = fromCompartment,
                TargetCompartment = toCompartment,
                LevelSource = 0.0,
                LevelTarget = 0.0,
                WaterType = SewerConnectionWaterType.None,
            };
            pumpConnection.AddStructureToBranch(pump2);
            network.Branches.Add(pumpConnection);
            //Assert.IsTrue(network.Pumps.Any(p => p.Name.Equals("pump2")));

            //2d pump 
            fmModel.Area.Pumps.Add(new Pump2D("pump3") {Geometry = new LineString(new [] {new Coordinate(0, 20), new Coordinate(100, 20)})});
            
            WaterFlowFMModelWriter.Write(fmModel);

            var fileProperty = fmModel.ModelDefinition.GetModelProperty(KnownProperties.StructuresFile);
            var structureFile = Path.Combine(tempDirectory, fileProperty.GetValueAsString());

            Assert.IsTrue(File.Exists(structureFile));
            var contentFile = File.ReadAllText(structureFile);

            Assert.IsTrue(contentFile.Contains("pump1"));
            Assert.IsTrue(contentFile.Contains("pump2"));
            Assert.IsTrue(contentFile.Contains("pump3"));
        }
    }
}

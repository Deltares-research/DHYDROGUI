using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Tests.Helpers;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor.Tests.Helpers;
using GeoAPI.Geometries;
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
    }
}

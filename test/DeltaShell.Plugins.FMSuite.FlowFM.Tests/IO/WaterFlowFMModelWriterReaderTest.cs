using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Tests.Helpers;
using DelftTools.Utils.Collections.Generic;
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
                Network = TestSewerNetworkProvider.CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOnePipeWithACrossSection()
            };

            WaterFlowFMModelWriter.Write(fmModel);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Tests.Helpers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
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
            var mduPath = Path.Combine(tempDirectory, "MyModel.mdu");
            var model = new WaterFlowFMModel
            {
                MduFilePath = mduPath
            };

            var geometry1 = new LineString(new[]{ new Coordinate(0, 0), new Coordinate(0, 100) });
            SewerFactory.SetDefaultSettingPipeAndAddToNetwork(model.Network, new Pipe { Name = pipeName, Geometry = geometry1, Material = SewerProfileMapping.SewerProfileMaterial.Polyester});

            //action write
            WaterFlowFMModelWriter.Write(model);

            var ugridPath = Path.Combine(tempDirectory, model.Name + "_net.nc");
            Assert.True(File.Exists(ugridPath)); //UGrid file

            var retrievedFmModel = WaterFlowFMModelReader.Read(model.MduFilePath);
            var retrievedNetwork = retrievedFmModel.Network;
            
            HydroNetworkTestHelper.CompareNetworks(model.Network, retrievedNetwork);
            HydroNetworkTestHelper.CompareDiscretisations(model.NetworkDiscretization, retrievedFmModel.NetworkDiscretization);
        }
    }
}

using System.IO;
using BruTile.Wmts;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.IntegrationTestUtils.NHibernate;
using DeltaShell.NGHS.Common.Gui.MapLayers;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests
{
    [TestFixture]
    public class WmtsGroupLayerTest : NHibernateIntegrationTestBase
    {
        [Test]
        public void GivenWmtsGroupLayer_ParsingOfUrl_ShouldGiveCorrectLayers()
        {
            //Arrange
            var url = "https://service.pdok.nl/brt/achtergrondkaart/wmts/v2_0";
            var capabilitiesText = File.ReadAllText(TestHelper.GetTestFilePath("Capabilities.xml"));
            
            var requestHandler = Substitute.For<IRequestHandler>();
            requestHandler.DoRequest(Arg.Any<string>()).Returns(s => capabilitiesText);

            // Act
            var layer = new WmtsGroupLayer(requestHandler) { Url = url };

            // Assert
            Assert.AreEqual("brtachtergrondkaart",  layer.Name);
            Assert.AreEqual(4, layer.Layers.Count);
            
            Assert.IsAssignableFrom<WmtsLayer>(layer.Layers[0]);
            
            var firstLayer = (WmtsLayer)layer.Layers[0];
            Assert.AreEqual("standaard", firstLayer.Name);
            Assert.AreEqual(3, firstLayer.TileSources.Count);
            
            var schema = firstLayer.SelectedTileSource.Schema as WmtsTileSchema;
            Assert.NotNull(schema);
            Assert.AreEqual("image/png", schema.Format);
            Assert.AreEqual("standaard(EPSG:28992)", schema.Identifier);
            Assert.AreEqual("EPSG:28992", schema.Name);
            Assert.AreEqual("EPSG:28992", schema.Srs);
            Assert.AreEqual(15, schema.Resolutions.Count);
            Assert.AreEqual("EPSG:28992", schema.TileMatrixSet);
        }

        [Test]
        public void GivenWmtsGroupLayer_SaveLoad_ShouldWork()
        {
            var url = "https://service.pdok.nl/brt/achtergrondkaart/wmts/v2_0";
            var capabilitiesText = File.ReadAllText(TestHelper.GetTestFilePath("Capabilities.xml"));

            var requestHandler = Substitute.For<IRequestHandler>();
            requestHandler.DoRequest(Arg.Any<string>()).Returns(s => capabilitiesText);

            // Act
            var layer = new WmtsGroupLayer(requestHandler) { Url = url };

            // Act
            var loadedLayer = SaveAndRetrieveObject(layer);

            // Assert
            Assert.AreEqual(url, loadedLayer.Url);
        }
        
        protected override NHibernateProjectRepository CreateProjectRepository()
        {
            var projectRepository = base.CreateProjectRepository();
            projectRepository.AddMappingStreams(AssemblyUtils.GetAssemblyResourceStreams(typeof(WmtsGroupLayer).Assembly, s => s.EndsWith(".hbm.xml")));
            return projectRepository;
        }
    }
}
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerManholeGeneratorTest : SewerFeatureFactoryTestHelper
    {
        [Test]
        public void SewerManholeGeneratorCreatesManholeWithGivenCoordinates()
        {
            var manholeId = "manhole123";
            var nodeType = EnumDescriptionAttributeTypeConverter.GetEnumDescription(ManholeMapping.NodeType.Manhole);
            var coordX = 10.0;
            var coordY = 20.0;
            var str = string.Empty;
            var dbl = 0.0;
            var gwswElement = GetNodeGwswElement(str, manholeId, nodeType, coordX, coordY, dbl, dbl, str, dbl, dbl,
                dbl);

            var createdFeature = new SewerManholeGenerator().Generate(gwswElement, null);
            Assert.IsNotNull(createdFeature);

            var manhole = createdFeature as Manhole;
            Assert.IsNotNull(manhole);

            Assert.AreEqual(manholeId, manhole.Name);
            Assert.AreEqual(coordX, manhole.XCoordinate);
            Assert.AreEqual(coordY, manhole.YCoordinate);
        }

        [Test]
        public void SewerManholeGeneratorExtendsExistingManholeWithGivenCoordinates()
        {
            var manholeId = "manhole123";
            var nodeType = EnumDescriptionAttributeTypeConverter.GetEnumDescription(ManholeMapping.NodeType.Manhole);
            var coordX = 10.0;
            var coordY = 20.0;
            var str = string.Empty;
            var dbl = 0.0;
            var gwswElement = GetNodeGwswElement(str, manholeId, nodeType, coordX, coordY, dbl, dbl, str, dbl, dbl,
                dbl);

            var network = new HydroNetwork();
            network.Nodes.Add(new Manhole(manholeId));
            Assert.IsTrue(network.Manholes.Any( m => m.Name.Equals(manholeId)));

            var createdFeature = new SewerManholeGenerator().Generate(gwswElement, null);
            Assert.IsNotNull(createdFeature);

            var manhole = createdFeature as Manhole;
            Assert.IsNotNull(manhole);

            Assert.AreEqual(manholeId, manhole.Name);
            Assert.AreEqual(coordX, manhole.XCoordinate);
            Assert.AreEqual(coordY, manhole.YCoordinate);
        }

        [Test]
        public void SewerManholeGeneratorReplacesExistingManholeCoordinates()
        {
            var manholeId = "manhole123";
            var oldCoordX = 10.0;
            var oldCoordY = 20.0;
            var nodeType = EnumDescriptionAttributeTypeConverter.GetEnumDescription(ManholeMapping.NodeType.Manhole);
            var newCoordX = 100.0;
            var newCoordY = 200.0;
            var str = string.Empty;
            var dbl = 0.0;
            var gwswElement = GetNodeGwswElement(str, manholeId, nodeType, newCoordX, newCoordY, dbl, dbl, str, dbl, dbl,
                dbl);

            var network = new HydroNetwork();
            network.Nodes.Add(new Manhole(manholeId){ Geometry = new Point(oldCoordX, oldCoordY)});
            Assert.IsTrue(network.Manholes.Any(m => m.Name.Equals(manholeId)));

            var createdFeature = new SewerManholeGenerator().Generate(gwswElement, null);
            Assert.IsNotNull(createdFeature);

            var manhole = createdFeature as Manhole;
            Assert.IsNotNull(manhole);

            Assert.AreEqual(manholeId, manhole.Name);
            Assert.AreEqual(newCoordX, manhole.XCoordinate);
            Assert.AreEqual(newCoordY, manhole.YCoordinate);
        }

        [Test]
        public void SewerManholeGeneratorReturnsNullIfNodeTypeIsWrong()
        {
            var manholeId = "manhole123";
            var nodeType = EnumDescriptionAttributeTypeConverter.GetEnumDescription(ManholeMapping.NodeType.Outlet);
            var coordX = 10.0;
            var coordY = 20.0;
            var str = string.Empty;
            var dbl = 0.0;
            var gwswElement = GetNodeGwswElement(str, manholeId, nodeType, coordX, coordY, dbl, dbl, str, dbl, dbl,
                dbl);

            var createdFeature = new SewerManholeGenerator().Generate(gwswElement, null);
            Assert.IsNull(createdFeature);
        }

        [Test]
        public void SewerManholeGeneratorReturnsNullIfManholeNameIsMissing()
        {
            var nodeType = EnumDescriptionAttributeTypeConverter.GetEnumDescription(ManholeMapping.NodeType.Outlet);
            var coordX = 10.0;
            var coordY = 20.0;
            var str = string.Empty;
            var dbl = 0.0;
            var gwswElement = GetNodeGwswElement(str, str, nodeType, coordX, coordY, dbl, dbl, str, dbl, dbl,
                dbl);

            var createdFeature = new SewerManholeGenerator().Generate(gwswElement, null);
            Assert.IsNull(createdFeature);
        }

        [TestCase("01FA", "23.6")]
        [TestCase("23.6", "01FA")]
        public void GivenGwswElementWithBadEntriesForCoordinateValues_WhenCreatingWithFactory_ThenLogMessageIsShownButDefaultCompartmentIsGivenBack(string xStringValue, string yStringValue)
        {
            var manholeId = "01001";
            var nodeType = EnumDescriptionAttributeTypeConverter.GetEnumDescription(ManholeMapping.NodeType.Manhole);
            var badGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList =
                {
                    new GwswAttribute
                    {
                        ValueAsString = nodeType,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.NodeType, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = manholeId,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "string",
                            ManholeMapping.PropertyKeys.ManholeId, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = xStringValue,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double",
                            ManholeMapping.PropertyKeys.XCoordinate, "MyDescription", null, null)
                    },
                    new GwswAttribute
                    {
                        ValueAsString = yStringValue,
                        GwswAttributeType = new GwswAttributeType("Knooppunt.csv", 2, "MyColumnName", "double",
                            ManholeMapping.PropertyKeys.YCoordinate, "MyDescription", null, null)
                    }
                }
            };

            var expectedPartOfMessage = "It was not possible to parse attribute";
            GenerateManholeCheckLogMessagesAndValidation(badGwswElement, expectedPartOfMessage, manholeId);
        }

        private void GenerateManholeCheckLogMessagesAndValidation(GwswElement gwswElement, string expectedMsg, string manholeName)
        {
            INetworkFeature feature = null;

            TestHelper.AssertAtLeastOneLogMessagesContains(() => feature = new SewerManholeGenerator().Generate(gwswElement, null),
                expectedMsg);

            // Check compartment
            var manhole = feature as Manhole;
            Assert.NotNull(manhole);
            Assert.That(manhole.Name, Is.EqualTo(manholeName));
        }
    }
}
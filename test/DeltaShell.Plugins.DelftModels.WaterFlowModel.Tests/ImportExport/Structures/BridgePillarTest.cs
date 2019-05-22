using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class BridgePillarTest : StructureConverterTestHelper
    {
        private const string BridgeName = "myBridge";
        private const string BridgeLongName = "myBridge_longName";
        private const string ChainageAsString = "2.0";

        [SetUp]
        public void Setup()
        {
            Network = mocks.DynamicMock<INetwork>();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void GivenBridgeStructureIniCategoryWithMatchingBranch_WhenConvertingToStructure1D_ThenBridgeWithCommonPropertyValuesIsReturned()
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();
            var branch = GetMockedBranch();

            // When
            var bridge = ConvertAndCheckForNull<BridgePillarConverter, Bridge>(category, branch);

            // Then
            IsBridgePillar(bridge);
            Assert.That(bridge.Name, Is.EqualTo(BridgeName));
            Assert.That(bridge.LongName, Is.EqualTo(BridgeLongName));
            Assert.That(bridge.Chainage, Is.EqualTo(ParseToDouble(ChainageAsString)));
            Assert.That(bridge.Geometry, Is.EqualTo(new Point(2, 0)));
            Assert.That(bridge.Branch, Is.EqualTo(branch));
            Assert.That(bridge.Network, Is.EqualTo(Network));
        }

        [TestCase("0", FlowDirection.Both)]
        [TestCase("1", FlowDirection.Positive)]
        [TestCase("2", FlowDirection.Negative)]
        [TestCase("3", FlowDirection.None)]
        public void GivenBridgeStructureIniCategoryWithCommonBridgeSpecificPropertyValues_WhenConvertingToStructure1D_ThenBridgeWithCommonBridgePropertyValuesIsReturned
            (string allowedFlowDirValue, FlowDirection expectedFlowDirection)
        {
            // Given
            var bedLevel = "3.0";

            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.AllowedFlowDir.Key, "0");
            category.SetProperty(StructureRegion.BedLevel.Key, bedLevel);

            var branch = GetMockedBranch();

            // When
            var bridge = ConvertAndCheckForNull<BridgePillarConverter, Bridge>(category, branch);

            // Then
            IsBridgePillar(bridge);
            Assert.That(bridge.FlowDirection, Is.EqualTo(FlowDirection.Both));
            Assert.That(bridge.BottomLevel, Is.EqualTo(ParseToDouble(bedLevel)));
        }

        [Test]
        public void GivenBridgeStructureIniCategoryWithBridgePillarSpecificPropertyValues_WhenConvertingToStructure1D_ThenBridgePillarWithSpecificPropertyValuesIsReturned()
        {
            // Given
            var pillarWidth = "10.0";
            var formFactor = "0.25";

            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.PillarWidth.Key, pillarWidth);
            category.SetProperty(StructureRegion.FormFactor.Key, formFactor);

            var branch = GetMockedBranch();

            // When
            var bridge = ConvertAndCheckForNull<BridgePillarConverter, Bridge>(category, branch);

            // Then
            IsBridgePillar(bridge);
            Assert.That(bridge.PillarWidth, Is.EqualTo(ParseToDouble(pillarWidth)));
            Assert.That(bridge.ShapeFactor, Is.EqualTo(ParseToDouble(formFactor)));
        }

        private static IDelftIniCategory GetStructureCategoryWithBasicProperties()
        {
            var category = new DelftIniCategory(StructureRegion.Header);
            category.AddProperty(StructureRegion.Id.Key, BridgeName);
            category.AddProperty(StructureRegion.Name.Key, BridgeLongName);
            category.AddProperty(StructureRegion.Chainage.Key, ChainageAsString);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");
            category.AddProperty(StructureRegion.BedLevel.Key, "0.0");
            category.AddProperty(StructureRegion.PillarWidth.Key, "1.0");
            category.AddProperty(StructureRegion.FormFactor.Key, "1.0");

            return category;
        }

        private static void IsBridgePillar(IBridge bridge)
        {
            Assert.IsTrue(bridge.IsPillar);
            Assert.That(bridge.GetStructureType(), Is.EqualTo(StructureType.BridgePillar));
        }
    }
}
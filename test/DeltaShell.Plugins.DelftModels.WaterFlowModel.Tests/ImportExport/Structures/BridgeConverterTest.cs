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
    public class BridgeConverterTest : StructureConverterTestHelper
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
            var bridge = ConvertAndCheckForNull<BridgeConverter, Bridge>(category, branch);

            // Then
            IsBridge(bridge);
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
            var bridge = ConvertAndCheckForNull<BridgeConverter, Bridge>(category, branch);

            // Then
            IsBridge(bridge);
            Assert.That(bridge.FlowDirection, Is.EqualTo(FlowDirection.Both));
            Assert.That(bridge.BottomLevel, Is.EqualTo(ParseToDouble(bedLevel)));
        }

        [Test]
        public void GivenBridgeStructureIniCategoryWithStandardBridgeSpecificPropertyValues_WhenConvertingToStructure1D_ThenBridgeWithSpecificPropertyValuesIsReturned()
        {
            // Given
            var length = "10.0";
            var inletLossCoefficient = "0.25";
            var outletLossCoefficient = "0.33";

            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.Length.Key, length);
            category.SetProperty(StructureRegion.InletLossCoeff.Key, inletLossCoefficient);
            category.SetProperty(StructureRegion.OutletLossCoeff.Key, outletLossCoefficient);

            var branch = GetMockedBranch();

            // When
            var bridge = ConvertAndCheckForNull<BridgeConverter, Bridge>(category, branch);

            // Then
            IsBridge(bridge);
            Assert.That(bridge.CrossSectionDefinition.Name, Is.EqualTo(BridgeName));
            Assert.That(bridge.Length, Is.EqualTo(ParseToDouble(length)));
            Assert.That(bridge.InletLossCoefficient, Is.EqualTo(ParseToDouble(inletLossCoefficient)));
            Assert.That(bridge.OutletLossCoefficient, Is.EqualTo(ParseToDouble(outletLossCoefficient)));
        }

        [TestCase("1", BridgeFrictionType.Chezy, Friction.Chezy)]
        [TestCase("4", BridgeFrictionType.Manning, Friction.Mannings)]
        [TestCase("5", BridgeFrictionType.StricklerKn, Friction.Nikuradse)]
        [TestCase("6", BridgeFrictionType.StricklerKs, Friction.Strickler)]
        [TestCase("7", BridgeFrictionType.WhiteColebrook, Friction.WhiteColebrook)]
        public void GivenBridgeStructureIniCategoryWithBedFrictionEntries_WhenConvertingToStructure1D_ThenBridgeWithExpectedFrictionValuesIsReturned
            (string frictionTypeValue, BridgeFrictionType expectedBridgeFrictionType, Friction expectedFrictionDataType)
        {
            // Given
            var friction = "10.0";
            var category = GetStructureCategoryWithBasicProperties();
            // Same values for bed friction and ground friction
            category.SetProperty(StructureRegion.BedFrictionType.Key, frictionTypeValue);
            category.SetProperty(StructureRegion.BedFriction.Key, friction);
            category.SetProperty(StructureRegion.GroundFrictionType.Key, frictionTypeValue);
            category.SetProperty(StructureRegion.GroundFriction.Key, friction);

            var branch = GetMockedBranch();

            // When
            var bridge = ConvertAndCheckForNull<BridgeConverter, Bridge>(category, branch);

            // Then
            IsBridge(bridge);
            Assert.That(bridge.FrictionType, Is.EqualTo(expectedBridgeFrictionType));
            Assert.That(bridge.FrictionDataType, Is.EqualTo(expectedFrictionDataType));
            Assert.That(bridge.Friction, Is.EqualTo(ParseToDouble(friction)));
            Assert.That(bridge.GroundLayerRoughness, Is.EqualTo(0.0));

            mocks.VerifyAll();
        }

        [TestCase("1", BridgeFrictionType.Chezy, Friction.Chezy)]
        [TestCase("4", BridgeFrictionType.Manning, Friction.Mannings)]
        [TestCase("5", BridgeFrictionType.StricklerKn, Friction.Nikuradse)]
        [TestCase("6", BridgeFrictionType.StricklerKs, Friction.Strickler)]
        [TestCase("7", BridgeFrictionType.WhiteColebrook, Friction.WhiteColebrook)]
        public void GivenBridgeStructureIniCategoryWithBedFrictionEntriesThatAreDifferentFromGroundLayerSettings_WhenConvertingToStructure1D_ThenBridgeWithExpectedFrictionAndGroundLayerValuesIsReturned
            (string frictionTypeValue, BridgeFrictionType expectedBridgeFrictionType, Friction expectedFrictionDataType)
        {
            // Given
            var friction = "10.0";
            var groundFriction = "7.0";
            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.BedFrictionType.Key, frictionTypeValue);
            category.SetProperty(StructureRegion.BedFriction.Key, friction);
            category.SetProperty(StructureRegion.GroundFrictionType.Key, frictionTypeValue);
            category.SetProperty(StructureRegion.GroundFriction.Key, groundFriction);

            var branch = GetMockedBranch();

            // When
            var bridge = ConvertAndCheckForNull<BridgeConverter, Bridge>(category, branch);

            // Then
            IsBridge(bridge);
            Assert.That(bridge.FrictionType, Is.EqualTo(expectedBridgeFrictionType));
            Assert.That(bridge.FrictionDataType, Is.EqualTo(expectedFrictionDataType));
            Assert.That(bridge.Friction, Is.EqualTo(ParseToDouble(friction)));
            Assert.That(bridge.GroundLayerRoughness, Is.EqualTo(ParseToDouble(groundFriction)));

            mocks.VerifyAll();
        }

        private static IDelftIniCategory GetStructureCategoryWithBasicProperties()
        {
            var category = new DelftIniCategory(StructureRegion.Header);
            category.AddProperty(StructureRegion.Id.Key, BridgeName);
            category.AddProperty(StructureRegion.Name.Key, BridgeLongName);
            category.AddProperty(StructureRegion.Chainage.Key, ChainageAsString);
            category.AddProperty(StructureRegion.AllowedFlowDir.Key, "0");
            category.AddProperty(StructureRegion.BedLevel.Key, "0.0");
            category.AddProperty(StructureRegion.CsDefId.Key, BridgeName);
            category.AddProperty(StructureRegion.Length.Key, "1.0");
            category.AddProperty(StructureRegion.InletLossCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.OutletLossCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.BedFrictionType.Key, "1");
            category.AddProperty(StructureRegion.BedFriction.Key, "45.0");
            category.AddProperty(StructureRegion.GroundFrictionType.Key, "1");
            category.AddProperty(StructureRegion.GroundFriction.Key, "45.0");

            return category;
        }

        private static void IsBridge(IBridge bridge)
        {
            Assert.IsFalse(bridge.IsPillar);
            Assert.That(bridge.GetStructureType(), Is.EqualTo(StructureType.Bridge));
        }
    }
}
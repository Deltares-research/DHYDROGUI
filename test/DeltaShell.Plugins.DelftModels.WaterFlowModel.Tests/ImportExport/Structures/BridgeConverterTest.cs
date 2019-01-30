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

        [Test]
        public void GivenBridgeStructureIniCategoryWithMatchingBranch_WhenConvertingToStructure1D_ThenBridgeWithCommonPropertyValuesIsReturned()
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();
            var branch = GetMockedBranch();

            // When
            var bridge = ConvertAndCheckForNull<BridgeConverter, Bridge>(category, branch);

            // Then
            Assert.IsFalse(bridge.IsPillar);
            Assert.That(bridge.GetStructureType(), Is.EqualTo(StructureType.Bridge));

            Assert.That(bridge.Name, Is.EqualTo(BridgeName));
            Assert.That(bridge.LongName, Is.EqualTo(BridgeLongName));
            Assert.That(bridge.Chainage, Is.EqualTo(ParseToDouble(ChainageAsString)));
            Assert.That(bridge.Geometry, Is.EqualTo(new Point(2, 0)));
            Assert.That(bridge.Branch, Is.EqualTo(branch));
            Assert.That(bridge.Network, Is.EqualTo(Network));

            mocks.VerifyAll();
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
            Assert.IsFalse(bridge.IsPillar);
            Assert.That(bridge.GetStructureType(), Is.EqualTo(StructureType.Bridge));

            Assert.That(bridge.CrossSectionDefinition.Name, Is.EqualTo(BridgeName));
            Assert.That(bridge.Length, Is.EqualTo(ParseToDouble(length)));
            Assert.That(bridge.InletLossCoefficient, Is.EqualTo(ParseToDouble(inletLossCoefficient)));
            Assert.That(bridge.OutletLossCoefficient, Is.EqualTo(ParseToDouble(outletLossCoefficient)));

            mocks.VerifyAll();
        }

        private static IDelftIniCategory GetStructureCategoryWithBasicProperties()
        {
            var category = new DelftIniCategory(StructureRegion.Header);
            category.AddProperty(StructureRegion.Id.Key, BridgeName);
            category.AddProperty(StructureRegion.Name.Key, BridgeLongName);
            category.AddProperty(StructureRegion.Chainage.Key, ChainageAsString);
            category.AddProperty(StructureRegion.CsDefId.Key, BridgeName);
            category.AddProperty(StructureRegion.Length.Key, "1.0");
            category.AddProperty(StructureRegion.InletLossCoeff.Key, "1.0");
            category.AddProperty(StructureRegion.OutletLossCoeff.Key, "1.0");

            return category;
        }
    }
}
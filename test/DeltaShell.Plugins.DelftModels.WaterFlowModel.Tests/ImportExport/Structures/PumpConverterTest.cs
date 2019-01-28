using System.Globalization;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    [TestFixture]
    public class PumpConverterTest
    {
        private const string PumpName = "myPump";
        private const string PumpLongName = "myPump_longName";
        private const string ChainageAsString = "2.0";

        private MockRepository mocks = new MockRepository();
        private readonly ILineString geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(10, 0) });

        [Test]
        public void GivenPumpStructureIniCategoryWithMatchingChannel_WhenConvertingToStructure1D_ThenPumpIsReturnedWithCommonPropertyValues()
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();

            var branch = mocks.DynamicMock<IBranch>();
            var network = mocks.DynamicMock<INetwork>();
            SetBranchMockProperties(branch, network);
            mocks.ReplayAll();

            // When
            var structure = new PumpConverter().ConvertToStructure1D(category, branch);
            var pump = structure as Pump;

            // Then
            Assert.IsNotNull(pump, "PumpConverter did not return a Pump object.");
            Assert.That(pump.Name, Is.EqualTo(PumpName));
            Assert.That(pump.LongName, Is.EqualTo(PumpLongName));
            Assert.That(pump.Chainage, Is.EqualTo(double.Parse(ChainageAsString, CultureInfo.InvariantCulture)));
            Assert.That(pump.Geometry, Is.EqualTo(new Point(2, 0)));
            Assert.That(pump.Branch, Is.EqualTo(branch));
            Assert.That(pump.Network, Is.EqualTo(network));
            
            mocks.VerifyAll();
        }

        private void SetBranchMockProperties(IBranch branch, INetwork network)
        {
            branch.Expect(b => b.Length).Return(10.0).Repeat.Any();
            branch.Expect(b => b.Geometry).Return(geometry).Repeat.Any();
            branch.Expect(b => b.Network).Return(network).Repeat.Any();
        }

        [TestCase("1", PumpControlDirection.SuctionSideControl, true)]
        [TestCase("2", PumpControlDirection.DeliverySideControl, true)]
        [TestCase("3", PumpControlDirection.SuctionAndDeliverySideControl, true)]
        [TestCase("-1", PumpControlDirection.SuctionSideControl, false)]
        [TestCase("-2", PumpControlDirection.DeliverySideControl, false)]
        [TestCase("-3", PumpControlDirection.SuctionAndDeliverySideControl, false)]
        public void GivenPumpStructureIniCategoryWithDirection_WhenConvertingToStructure1D_ThenPumpWithSpecificControlDirectionIsReturned
            (string valueAsString, PumpControlDirection expectedDirection, bool expectedIsDirectionPositive)
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();
            category.SetProperty(StructureRegion.Direction.Key, valueAsString);

            var branch = mocks.DynamicMock<IBranch>();
            var network = mocks.DynamicMock<INetwork>();
            SetBranchMockProperties(branch, network);
            mocks.ReplayAll();

            // When
            var structure = new PumpConverter().ConvertToStructure1D(category, branch);
            var pump = structure as Pump;

            // Then
            Assert.IsNotNull(pump, "PumpConverter did not return a Pump object.");
            Assert.That(pump.ControlDirection, Is.EqualTo(expectedDirection));
            Assert.That(pump.DirectionIsPositive, Is.EqualTo(expectedIsDirectionPositive));

            mocks.VerifyAll();
        }

        private static IDelftIniCategory GetStructureCategoryWithBasicProperties()
        {
            var category = new DelftIniCategory(StructureRegion.Header);
            category.AddProperty(StructureRegion.Id.Key, PumpName);
            category.AddProperty(StructureRegion.Name.Key, PumpLongName);
            category.AddProperty(StructureRegion.Chainage.Key, ChainageAsString);
            category.AddProperty(StructureRegion.Direction.Key, "1");

            return category;
        }
    }
}
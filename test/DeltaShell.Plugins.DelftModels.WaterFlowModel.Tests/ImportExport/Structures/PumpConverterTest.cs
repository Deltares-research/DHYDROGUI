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

        [Test]
        public void GivenPumpStructureIniCategoryWithMatchingChannel_WhenConvertingToStructure1D_ThenPumpIsReturnedWithCommonPropertyValues()
        {
            // Given
            var category = new DelftIniCategory(StructureRegion.Header);
            category.AddProperty(StructureRegion.DefinitionType.Key, StructureRegion.StructureTypeName.Pump);
            category.AddProperty(StructureRegion.Id.Key, PumpName);
            category.AddProperty(StructureRegion.Name.Key, PumpLongName);
            category.AddProperty(StructureRegion.Chainage.Key, ChainageAsString);
            category.AddProperty(StructureRegion.BranchId.Key, "myBranch");

            var geometry = new LineString(new[]{new Coordinate(0, 0), new Coordinate(10, 0) });

            var mocks = new MockRepository();
            var branch = mocks.DynamicMock<IBranch>();
            var network = mocks.DynamicMock<INetwork>();

            branch.Expect(b => b.Length).Return(10.0).Repeat.Any();
            branch.Expect(b => b.Geometry).Return(geometry).Repeat.Any();
            branch.Expect(b => b.Network).Return(network).Repeat.Any();
            mocks.ReplayAll();

            // When
            var structure = new PumpConverter().ConvertToStructure1D(category, branch);

            // Then
            var pump = structure as Pump;
            Assert.IsNotNull(pump, "PumpConverter did not return a Pump object.");
            Assert.That(pump.Name, Is.EqualTo(PumpName));
            Assert.That(pump.LongName, Is.EqualTo(PumpLongName));
            Assert.That(pump.Chainage, Is.EqualTo(double.Parse(ChainageAsString, CultureInfo.InvariantCulture)));
            Assert.That(pump.Geometry, Is.EqualTo(new Point(2, 0)));
            Assert.That(pump.Branch, Is.EqualTo(branch));
            Assert.That(pump.Network, Is.EqualTo(network));
            
            mocks.VerifyAll();
        }
    }
}
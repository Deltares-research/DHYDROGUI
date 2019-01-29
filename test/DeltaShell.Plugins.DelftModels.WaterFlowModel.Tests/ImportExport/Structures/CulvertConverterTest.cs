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
    public class CulvertConverterTest
    {
        private const string CulvertName = "myCulvert";
        private const string CulvertLongName = "myCulvert_longName";
        private const string ChainageAsString = "2.0";

        private MockRepository mocks = new MockRepository();
        private INetwork network;
        private readonly ILineString branchGeometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(10, 0) });

        [SetUp]
        public void Setup()
        {
            network = mocks.DynamicMock<INetwork>();
        }

        [Test]
        public void GivenCulvertStructureIniCategoryWithMatchingBranch_WhenConvertingToStructure1D_ThenCulvertIsReturnedWithCommonPropertyValues()
        {
            // Given
            var category = GetStructureCategoryWithBasicProperties();

            var branch = mocks.DynamicMock<IBranch>();
            SetBranchMockProperties(branch, network);
            mocks.ReplayAll();

            // When
            var structure = new CulvertConverter().ConvertToStructure1D(category, branch);
            var culvert = structure as Culvert;

            // Then
            Assert.IsNotNull(culvert, "CulvertConverter did not return a Culvert object.");
            Assert.That(culvert.Name, Is.EqualTo(CulvertName));
            Assert.That(culvert.LongName, Is.EqualTo(CulvertLongName));
            Assert.That(culvert.Chainage, Is.EqualTo(double.Parse(ChainageAsString, CultureInfo.InvariantCulture)));
            Assert.That(culvert.Geometry, Is.EqualTo(new Point(2, 0)));
            Assert.That(culvert.Branch, Is.EqualTo(branch));
            Assert.That(culvert.Network, Is.EqualTo(network));

            mocks.VerifyAll();
        }

        private static IDelftIniCategory GetStructureCategoryWithBasicProperties()
        {
            var category = new DelftIniCategory(StructureRegion.Header);
            category.AddProperty(StructureRegion.Id.Key, CulvertName);
            category.AddProperty(StructureRegion.Name.Key, CulvertLongName);
            category.AddProperty(StructureRegion.Chainage.Key, ChainageAsString);

            return category;
        }

        private void SetBranchMockProperties(IBranch branch, INetwork network)
        {
            branch.Expect(b => b.Length).Return(10.0).Repeat.Any();
            branch.Expect(b => b.Geometry).Return(branchGeometry).Repeat.Any();
            branch.Expect(b => b.Network).Return(network).Repeat.Any();
        }
    }
}
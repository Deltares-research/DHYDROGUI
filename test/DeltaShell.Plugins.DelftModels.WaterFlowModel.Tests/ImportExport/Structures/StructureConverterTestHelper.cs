using System.Globalization;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Structures
{
    public abstract class StructureConverterTestHelper
    {
        protected readonly MockRepository mocks = new MockRepository();
        private readonly ILineString branchGeometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(10, 0) });
        protected INetwork Network;

        protected IBranch GetMockedBranch()
        {
            var branch = mocks.DynamicMock<IBranch>();
            SetBranchMockProperties(branch);
            mocks.ReplayAll();
            return branch;
        }

        private void SetBranchMockProperties(IBranch branch)
        {
            branch.Expect(b => b.Length).Return(10.0).Repeat.Any();
            branch.Expect(b => b.Geometry).Return(branchGeometry).Repeat.Any();
            branch.Expect(b => b.Network).Return(Network).Repeat.Any();
        }

        protected static TType ConvertAndCheckForNull<TConverter, TType>(IDelftIniCategory category, IBranch branch)
            where TConverter : IStructureConverter, new()
            where TType : class, IStructure1D
        {
            var structure = new TConverter().ConvertToStructure1D(category, branch);
            var culvert = structure as TType;
            Assert.IsNotNull(culvert, $"Converter did not return a {typeof(TConverter)} object.");

            return culvert;
        }

        protected static double ParseToDouble(string bendLossCoefficient)
        {
            return double.Parse(bendLossCoefficient, CultureInfo.InvariantCulture);
        }
    }
}
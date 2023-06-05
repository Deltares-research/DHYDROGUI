using DelftTools.Hydro;
using DelftTools.TestUtils;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid.DeltaresUGrid
{
    [TestFixture]
    public class GeneratedObjectsForLinksTests
    {
        [Test]
        public void GivenEmptyGeneratedObjectsForLinksAndNoApiWhenRead1D2DLinksThrowsArgumentNullException()
        {
            var generatedObjectsForLinks = new GeneratedObjectsForLinks();
            Assert.That(() => generatedObjectsForLinks.Read1D2DLinks("", null), Throws.ArgumentNullException);
        }
        
        [Test]
        public void GivenEmptyGeneratedObjectsForLinksAndApiWhenRead1D2DLinksThrowsArgumentNullException()
        {
            var generatedObjectsForLinks = new GeneratedObjectsForLinks();
            var api = Substitute.For<IUGridApi>();
            Assert.That(() => generatedObjectsForLinks.Read1D2DLinks("", api), Throws.Nothing);
        }
        
        [Test]
        public void GivenEmptyGeneratedObjectsForLinksAndApiWhenRead1D2DLinksAndFileNotUgridThenExpectLogMessage()
        {
            var generatedObjectsForLinks = new GeneratedObjectsForLinks();
            var api = Substitute.For<IUGridApi>();
            api.IsUGridFile().Returns(false);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => generatedObjectsForLinks.Read1D2DLinks("", api), "This is not a UGrid file.");
        }
        
        [Test]
        public void GivenGeneratedObjectsForLinksWithMesh2DAndApiWhenRead1D2DLinksAndNoDataValueFromApiThenExpectFillValueSetOtherThanDefault()
        {
            var disposable2DMeshGeometry = Substitute.For<Disposable2DMeshGeometry>();
            disposable2DMeshGeometry.Name = "myMesh2d";
            var generatedObjectsForLinks = new GeneratedObjectsForLinks()
            {
                Mesh2d = disposable2DMeshGeometry
            };
            var api = Substitute.For<IUGridApi>();
            api.IsUGridFile().Returns(true);
            api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[]{1});
            api.GetVariableNoDataValue(disposable2DMeshGeometry.Name + "_face_nodes", 1, GridLocationType.None).Returns(801.0);
            generatedObjectsForLinks.Read1D2DLinks("", api);
            Assert.That(generatedObjectsForLinks.FillValueMesh2DFaceNodes, Is.EqualTo(801));
        }
        
        [Test]
        public void GivenGeneratedObjectsForLinksWithGridAndApiWhenRead1D2DLinksAndNeedToReadMesh2DThenExpectMesh2DSetToOurInstance()
        {
            var grid = Substitute.For<UnstructuredGrid>();
            var generatedObjectsForLinks = new GeneratedObjectsForLinks()
            {
                Grid = grid
            };
            var disposable2DMeshGeometry = Substitute.For<Disposable2DMeshGeometry>();
            
            var api = Substitute.For<IUGridApi>();
            api.IsUGridFile().Returns(true);
            api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D).Returns(new[]{1});
            api.GetMesh2D(1).Returns(disposable2DMeshGeometry);
            generatedObjectsForLinks.Read1D2DLinks("", api);
            Assert.That(generatedObjectsForLinks.Mesh2d, Is.EqualTo(disposable2DMeshGeometry));
        }
        
        [Test]
        public void GivenGeneratedObjectsForLinksWithGridAndApiWhenRead1D2DLinksAndNeedToReadMesh1DThenExpectMesh1DSetToOurInstance()
        {
            var grid = Substitute.For<UnstructuredGrid>();
            var generatedObjectsForLinks = new GeneratedObjectsForLinks()
            {
                Grid = grid
            };
            var disposable1DMeshGeometry = Substitute.For<Disposable1DMeshGeometry>();
            
            var api = Substitute.For<IUGridApi>();
            api.IsUGridFile().Returns(true);
            api.GetMeshIdsByMeshType(UGridMeshType.Mesh1D).Returns(new[]{1});
            api.GetMesh1D(1).Returns(disposable1DMeshGeometry);
            generatedObjectsForLinks.Read1D2DLinks("", api);
            Assert.That(generatedObjectsForLinks.Mesh1d, Is.EqualTo(disposable1DMeshGeometry));
        }
        
        [Test]
        public void GivenGeneratedObjectsForLinksWithDiscretizationWithNetworkAndApiWhenRead1D2DLinksAndNeedToReadNetworkGeometryThenExpectNetworkGeometrySetToOurInstance()
        {
            var hydroNetwork = Substitute.For<IHydroNetwork>();
            var discretization = Substitute.For<IDiscretization>();
            discretization.Network = hydroNetwork;
            var generatedObjectsForLinks = new GeneratedObjectsForLinks()
            {
                Discretization = discretization
            };
            var networkGeometry = Substitute.For<DisposableNetworkGeometry>();
            
            var api = Substitute.For<IUGridApi>();
            api.IsUGridFile().Returns(true);
            api.GetNetworkIds().Returns(new[]{1});
            api.GetNetworkGeometry(1).Returns(networkGeometry);
            generatedObjectsForLinks.Read1D2DLinks("", api);
            Assert.That(generatedObjectsForLinks.NetworkGeometry, Is.EqualTo(networkGeometry));
        }

        [Test]
        public void GivenEmptyGeneratedObjectsForLinksAndApiWhenRead1D2DLinksAndLinkIdIsOneThenExpectLinksGeometrySetToOurInstance()
        {
            var generatedObjectsForLinks = new GeneratedObjectsForLinks();
            var disposableLinksGeometry = Substitute.For<DisposableLinksGeometry>();
            
            var api = Substitute.For<IUGridApi>();
            api.IsUGridFile().Returns(true);
            api.GetLinksId().Returns(1);
            api.GetLinks(1).Returns(disposableLinksGeometry);
            generatedObjectsForLinks.Read1D2DLinks("", api);
            Assert.That(generatedObjectsForLinks.LinksGeometry, Is.EqualTo(disposableLinksGeometry));
        }
        [Test]
        public void GivenEmptyGeneratedObjectsForLinksAndApiWhenRead1D2DLinksAndLinkIdIsMinusOneThenExpectLinksGeometrySetToNull()
        {
            var generatedObjectsForLinks = new GeneratedObjectsForLinks();
            var disposableLinksGeometry = Substitute.For<DisposableLinksGeometry>();
            
            var api = Substitute.For<IUGridApi>();
            api.IsUGridFile().Returns(true);
            api.GetLinksId().Returns(-1);
            generatedObjectsForLinks.Read1D2DLinks("", api);
            Assert.That(generatedObjectsForLinks.LinksGeometry, Is.Null);
        }

    }
}
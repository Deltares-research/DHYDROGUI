using System.Collections.Generic;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Utils.Collections.Generic;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGridFileHelper1D2DLinksTests
    {
        [Test]
        public void SetLinksTest_DoingCreateLinks_ShouldCreateValidLinks()
        {
            //Arrange
            int oneToOne = (int)LinkStorageType.Embedded;

            DisposableLinksGeometry linkGeometry = CreateLinkGeometry(oneToOne);
            Disposable1DMeshGeometry mesh1d = CreateMesh1d();
            DisposableNetworkGeometry networkGeometry = CreateNetworkGeometry();
            Disposable2DMeshGeometry mesh2d = CreateMesh2d();

            // generate grid with 3 cells of size 20x20, starting at 0,0
            UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(3, 1, 20, 20);
            Discretization discretization = CreateDiscretizationAndNetwork();

            GeneratedObjectsForLinks generatedObjectsForLinks = new GeneratedObjectsForLinks()
            {
                Grid = grid,
                Mesh2d = mesh2d,
                Mesh1d = mesh1d,
                NetworkGeometry = networkGeometry,
                Discretization = discretization,
                FillValueMesh2DFaceNodes = -999,
                LinksGeometry = linkGeometry,
                Links1D2D = new List<ILink1D2D>()
            };

            // Act
            UGridFileHelper1D2DLinks helper1D2DLinks = new UGridFileHelper1D2DLinks();
            helper1D2DLinks.SetLinks(generatedObjectsForLinks);

            IList<ILink1D2D> links = generatedObjectsForLinks.Links1D2D;
            Links1D2DHelper.SetGeometry1D2DLinks(links, discretization.Locations, grid.Cells);

            // Assert
            Assert.AreEqual(3, links.Count);
            Assert.AreEqual("link1", links[0].Name);
            Assert.AreEqual("link1_long", links[0].LongName);
            Assert.AreEqual(2, links[0].FaceIndex);
            Assert.AreEqual(0, links[0].DiscretisationPointIndex);
            Assert.AreEqual(LinkStorageType.Embedded, links[0].TypeOfLink);
        }

        private static Discretization CreateDiscretizationAndNetwork()
        {
            Branch branch = new Branch
            {
                Name = "branch1",
                Geometry = new LineString(new[] { new Coordinate(0.0, 25.0), new Coordinate(70.0, 60.0) })
            };
            Discretization discretization = new Discretization
            {
                Network = new Network() { Branches = new EventedList<IBranch>() { branch } },
                Locations =
                {
                    Values = new MultiDimensionalArray<INetworkLocation>()
                    {
                        new NetworkLocation(branch, 10),
                        new NetworkLocation(branch, 30),
                        new NetworkLocation(branch, 50)
                    }
                }
            };
            return discretization;
        }

        private static Disposable2DMeshGeometry CreateMesh2d()
        {
            return new Disposable2DMeshGeometry
            {
                FaceX = new[] { 010.0, 30.0, 50.0 },
                FaceY = new[] { 010.0, 010.0, 010.0 },
                MaxNumberOfFaceNodes = 4,
                NodesX = new[] { 0.0, 20.0, 20.0, 0.0, 40.0, 40.0, 60.0, 60.0 },
                NodesY = new[] { 0.0, 0.0, 20.0, 20.0, 0.0, 20.0, 0.0, 20.0 },
                FaceNodes = new[] { 0, 1, 2, 3, 1, 4, 5, 2, 4, 6, 7, 5 }
            };
        }

        private static DisposableNetworkGeometry CreateNetworkGeometry()
        {
            return new DisposableNetworkGeometry
            {
                BranchIds = new[] { "branch1" },
            };
        }

        private static Disposable1DMeshGeometry CreateMesh1d()
        {
            return new Disposable1DMeshGeometry
            {
                NodeIds = new[] { "node1", "node2", "node3", },
                NodesX = new[] { 10.0, 30.0, 50.0, },
                NodesY = new[] { 30.0, 40.0, 50.0, },
                BranchIDs = new[] { 0, 0, 0 },
                BranchOffsets = new[] { 10.0, 30.0, 50.0, },
            };
        }

        private static DisposableLinksGeometry CreateLinkGeometry(int oneToOne)
        {
            return new DisposableLinksGeometry
            {
                LinkId = new string[] { "link1", "link2", "link3" },
                LinkLongName = new string[] { "link1_long", "link2_long", "link3_long" },
                Mesh1DFrom = new int[] { 0, 1, 2 },
                Mesh2DTo = new int[] { 2, 1, 0 },
                LinkType = new int[] { oneToOne, oneToOne, oneToOne }
            };
        }
    }
}
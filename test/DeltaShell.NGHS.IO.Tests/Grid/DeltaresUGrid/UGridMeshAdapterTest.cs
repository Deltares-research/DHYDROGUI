using System.Collections.Generic;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid.DeltaresUGrid
{
    [TestFixture]
    public class UGridMeshAdapterTest
    {
        [Test]
        public void GivenUGridMeshAdapter_DoingCreateUnstructuredGrid_ShouldCreateValidGrid()
        {
            //
            //          7
            //    6.----.----. 8
            //     |    |    | 
            //     |    |    | 
            //    3.----.----. 5
            //     |   4|    | 
            //     |    |    | 
            //     .----.----.
            //     0    1    2
            //
            // 

            //Arrange
            var disposable2DMeshGeometry = new Disposable2DMeshGeometry
            {
                Name = "Mesh2d",
                NodesX = new double[] { 1, 2, 3, 1, 2, 3, 1, 2, 3 },
                NodesY = new double[] { 1, 1, 1, 2, 2, 2, 3, 3, 3 },
                EdgeNodes = new[] { 0, 1, 1, 2, 0, 3, 1, 4, 2, 5, 3, 4, 4, 5, 3, 6, 4, 7, 5, 8, 6, 7, 7, 8 },
                FaceNodes = new[] { 0, 1, 3, 4, 1, 2, 4, 5, 3, 4, 6, 7, 4, 5, 7, 8 },
                FaceX = new double[] { 1.5, 1.5, 2.5, 2.5 },
                FaceY = new double[] { 1.5, 2.5, 1.5, 2.5 },
                MaxNumberOfFaceNodes = 4
            };

            // Act

            var grid = disposable2DMeshGeometry.CreateUnstructuredGrid();

            // Assert
            Assert.AreEqual(9, grid.Vertices.Count);

            Assert.AreEqual(1, grid.Vertices[0].X);
            Assert.AreEqual(1, grid.Vertices[0].Y);

            Assert.AreEqual(3, grid.Vertices[8].X);
            Assert.AreEqual(3, grid.Vertices[8].Y);

            Assert.AreEqual(12, grid.Edges.Count);

            Assert.AreEqual(0, grid.Edges[0].VertexFromIndex);
            Assert.AreEqual(1, grid.Edges[0].VertexToIndex);

            Assert.AreEqual(7, grid.Edges[11].VertexFromIndex);
            Assert.AreEqual(8, grid.Edges[11].VertexToIndex);

            Assert.AreEqual(4, grid.Cells.Count);

            Assert.AreEqual(new[] { 0, 1, 3, 4}, grid.Cells[0].VertexIndices);
            Assert.AreEqual(new[] { 4, 5, 7, 8 }, grid.Cells[3].VertexIndices);

        }

        [Test]
        public void GivenUGridMeshAdapter_DoingCreateDisposable2DMeshGeometry_ShouldGiveValidCreateDisposable2DMeshGeometry()
        {
            //Arrange

            //var grid = UnstructuredGridFactory
            /*var grid = new UnstructuredGrid()
            {
                Vertices = new List<Coordinate>(),
                Edges = new List<Edge>()
                Cells = 
            }
*/

            // Act


            // Assert

        }
    }
}

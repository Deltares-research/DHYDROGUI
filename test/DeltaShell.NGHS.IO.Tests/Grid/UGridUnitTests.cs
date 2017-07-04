using System;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGridUnitTests
    {
        private const string UGRID_TEST_FILE = @"ugrid\Dummy.nc";
        private MockRepository mocks;
        private IUGridApi uGridApi;
        private UGrid grid;
        private const string standardErrorMessage = ", because of error number: -1";
        private int errorValue = -1;
        private int noErrorValue = GridApiDataSet.GridConstants.IONC_NOERR;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uGridApi = mocks.DynamicMock<IUGridApi>();
            grid = new UGrid(TestHelper.GetTestFilePath(UGRID_TEST_FILE))
            {
                GridApi = uGridApi
            };
            SetExpectanciesSuchThatGridNetworkApiIsValid();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        private void SetExpectanciesSuchThatGridNetworkApiIsValid()
        {
            uGridApi.Expect(api => api.Initialized).Return(true).Repeat.Any();
            uGridApi.Expect(api => api.GetConvention()).Return(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID).Repeat
                .Any();
            uGridApi.Expect(api => api.GetVersion()).Return(GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION).Repeat
                .Any();
        }

        [Test]
        public void WhenGetting_gridZCoordinateFillValue_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            uGridApi.Expect(api => api.zCoordinateFillValue).Return(2).Repeat.Once();

            mocks.ReplayAll();

            var gridZCoordinateFillValue = grid.ZCoordinateFillValue;
            Assert.That(gridZCoordinateFillValue, Is.EqualTo(2));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get the number of 2D meshes" + standardErrorMessage)]
        public void WhenInvoking_NumberOf2DMeshes_AndApiReturnsAnErrorValueThenThrowException()
        {
            int numMeshes = 2;
            uGridApi.Expect(api => api.GetNumberOfMeshByType(Arg<UGridMeshType>.Is.Anything, out Arg<int>.Out(numMeshes).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetNumberOf2DMeshes();
        }

        [Test]
        public void WhenInvoking_NumberOf2DMeshes_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int numMeshes = 2;
            uGridApi.Expect(api => api.GetNumberOfMeshByType(Arg<UGridMeshType>.Is.Anything, out Arg<int>.Out(numMeshes).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var nMeshes = grid.GetNumberOf2DMeshes();
            Assert.That(nMeshes, Is.EqualTo(numMeshes));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get the number of nodes" + standardErrorMessage)]
        public void WhenInvoking_NumberOfNodes_AndApiReturnsAnErrorValueThenThrowException()
        {
            int numNodes = 2;
            uGridApi.Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(numNodes).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetNumberOfNodesForMeshId(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_NumberOfNodes_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int numNodes = 2;
            uGridApi.Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(numNodes).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var nNodes = grid.GetNumberOfNodesForMeshId(Arg<int>.Is.Anything);
            Assert.That(nNodes, Is.EqualTo(numNodes));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get number of edges" + standardErrorMessage)]
        public void WhenInvoking_NumberOfEdges_AndApiReturnsAnErrorValueThenThrowException()
        {
            int numEdges = 2;
            uGridApi.Expect(api => api.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(numEdges).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetNumberOfEdgesForMeshId(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_NumberOfEdges_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int numEdges = 2;
            uGridApi.Expect(api => api.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(numEdges).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var nEdges = grid.GetNumberOfEdgesForMeshId(Arg<int>.Is.Anything);
            Assert.That(nEdges, Is.EqualTo(numEdges));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get number of faces" + standardErrorMessage)]
        public void WhenInvoking_NumberOfFaces_AndApiReturnsAnErrorValueThenThrowException()
        {
            int numFaces = 2;
            uGridApi.Expect(api => api.GetNumberOfFaces(Arg<int>.Is.Anything, out Arg<int>.Out(numFaces).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetNumberOfFacesForMeshId(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_NumberOfFaces_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int numFaces = 2;
            uGridApi.Expect(api => api.GetNumberOfFaces(Arg<int>.Is.Anything, out Arg<int>.Out(numFaces).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var nFaces = grid.GetNumberOfFacesForMeshId(Arg<int>.Is.Anything);
            Assert.That(nFaces, Is.EqualTo(numFaces));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get max face nodes" + standardErrorMessage)]
        public void WhenInvoking_NumberOfMaxFaceNodes_AndApiReturnsAnErrorValueThenThrowException()
        {
            int numMaxFaceNodes = 2;
            uGridApi.Expect(api => api.GetMaxFaceNodes(Arg<int>.Is.Anything, out Arg<int>.Out(numMaxFaceNodes).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetNumberOfMaxFaceNodesForMeshId(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_NumberOfMaxFaceNodes_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int numMaxFaceNodes = 2;
            uGridApi.Expect(api => api.GetMaxFaceNodes(Arg<int>.Is.Anything, out Arg<int>.Out(numMaxFaceNodes).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var nMaxFaceNodes = grid.GetNumberOfMaxFaceNodesForMeshId(Arg<int>.Is.Anything);
            Assert.That(nMaxFaceNodes, Is.EqualTo(numMaxFaceNodes));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get x node coordinates" + standardErrorMessage)]
        public void WhenInvoking_GetAllNodeCoordinates_AndApiReturnsAnErrorValueForXCoordinatesThenThrowException()
        {
            double[] xCoordinates = { 2.0 , 3.4 };
            uGridApi.Expect(api => api.GetNodeXCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(xCoordinates).Dummy))
                .Return(errorValue).Repeat.Once();
            uGridApi.Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(2).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetAllNodeCoordinatesForMeshId(Arg<int>.Is.Anything);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get y node coordinates" + standardErrorMessage)]
        public void WhenInvoking_GetAllNodeCoordinates_AndApiReturnsAnErrorValueForYCoordinatesThenThrowException()
        {
            double[] coordinates = { 2.0, 3.4 };
            uGridApi.Expect(api => api.GetNodeXCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(coordinates).Dummy))
                .Return(noErrorValue).Repeat.Once();
            uGridApi.Expect(api => api.GetNodeYCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(coordinates).Dummy))
                .Return(errorValue).Repeat.Once();
            uGridApi.Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(2).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetAllNodeCoordinatesForMeshId(Arg<int>.Is.Anything);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get z node coordinates" + standardErrorMessage)]
        public void WhenInvoking_GetAllNodeCoordinates_AndApiReturnsAnErrorValueForZCoordinatesThenThrowException()
        {
            double[] coordinates = { 2.0, 3.4 };
            uGridApi.Expect(api => api.GetNodeXCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(coordinates).Dummy))
                .Return(noErrorValue).Repeat.Once();
            uGridApi.Expect(api => api.GetNodeYCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(coordinates).Dummy))
                .Return(noErrorValue).Repeat.Once();
            uGridApi.Expect(api => api.GetNodeZCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(coordinates).Dummy))
                .Return(errorValue).Repeat.Once();
            uGridApi.Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(2).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetAllNodeCoordinatesForMeshId(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetAllNodeCoordinates_AndNumberOfNodesIsEqualTo0ThenReturnEmptyArray()
        {
            uGridApi.Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(0).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            Coordinate[] result = grid.GetAllNodeCoordinatesForMeshId(Arg<int>.Is.Anything);
            Assert.That(result, Is.EqualTo(new Coordinate[0]));
        }

        [Test]
        public void WhenInvoking_GetAllNodeCoordinates_ThenMethodSavesTheCoordinatesInTheRightWay()
        {
            int meshId = 1;
            int nNodes = 2;
            double[] xCoordinates = { 2.0, 3.4 };
            double[] yCoordinates = { -1.0, 8.4 };
            double[] zCoordinates = { -1.1, -2.3 };
            Coordinate[] expectedResult = { new Coordinate(2.0, -1.0, -1.1), new Coordinate(3.4, 8.4, -2.3) };

            uGridApi.Expect(api => api.GetNodeXCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(xCoordinates).Dummy))
                .Return(noErrorValue).Repeat.Once();
            uGridApi.Expect(api => api.GetNodeYCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(yCoordinates).Dummy))
                .Return(noErrorValue).Repeat.Once();
            uGridApi.Expect(api => api.GetNodeZCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(zCoordinates).Dummy))
                .Return(noErrorValue).Repeat.Once();
            uGridApi.Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(nNodes).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            Coordinate[] result = grid.GetAllNodeCoordinatesForMeshId(meshId);
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get edge nodes of the mesh" + standardErrorMessage)]
        public void WhenInvoking_GetEdgeNodesForMesh_AndApiReturnsAnErrorValueThenThrowException()
        {
            int[,] edgeNodes = { { 0, 1 }, { 1, 2 } };
            uGridApi.Expect(api => api.GetEdgeNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(edgeNodes).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetEdgeNodesForMeshId(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetEdgeNodesForMesh_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int[,] edgeNodes = { { 0, 1 }, { 1, 2 } };
            int meshId = 1;
            int numMeshes = 2;
            uGridApi.Expect(api => api.GetEdgeNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(edgeNodes).Dummy))
                .Return(noErrorValue).Repeat.Once();
            uGridApi.Expect(api => api.GetNumberOfMeshByType(Arg<UGridMeshType>.Is.Anything, out Arg<int>.Out(numMeshes).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetEdgeNodesForMeshId(meshId);
            var nodes = grid.EdgeNodesByMeshId[meshId - 1];

            Assert.That(nodes[0,0], Is.EqualTo(0));
            Assert.That(nodes[0,1], Is.EqualTo(1));
            Assert.That(nodes[1,0], Is.EqualTo(1));
            Assert.That(nodes[1,1], Is.EqualTo(2));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get face nodes of the mesh" + standardErrorMessage)]
        public void WhenInvoking_GetFaceNodesForMesh_AndApiReturnsAnErrorValueThenThrowException()
        {
            int[,] faceNodes = { { 0, 1 }, { 1, 2 } };
            uGridApi.Expect(api => api.GetFaceNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(faceNodes).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetFaceNodesForMeshId(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetFaceNodesForMesh_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int[,] faceNodes = { { 0, 1 }, { 1, 2 } };
            int meshId = 1;
            int numMeshes = 2;
            uGridApi.Expect(api => api.GetFaceNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(faceNodes).Dummy))
                .Return(noErrorValue).Repeat.Once();
            uGridApi.Expect(api => api.GetNumberOfMeshByType(Arg<UGridMeshType>.Is.Anything, out Arg<int>.Out(numMeshes).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetFaceNodesForMeshId(meshId);
            var nodes = grid.FaceNodesByMeshId[meshId - 1];

            Assert.That(nodes[0, 0], Is.EqualTo(0));
            Assert.That(nodes[0, 1], Is.EqualTo(1));
            Assert.That(nodes[1, 0], Is.EqualTo(1));
            Assert.That(nodes[1, 1], Is.EqualTo(2));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get the number of names at location" + standardErrorMessage)]
        public void WhenInvoking_NumberOfNamesAtLocation_AndApiReturnsAnErrorValueThenThrowException()
        {
            int nCount = 33;
            uGridApi.Expect(api => api.GetVarCount(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, out Arg<int>.Out(nCount).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.NumberOfNamesForLocationType(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_NumberOfNamesAtLocation_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int nCount = 33;
            uGridApi.Expect(api => api.GetVarCount(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, out Arg<int>.Out(nCount).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var varCount = grid.NumberOfNamesForLocationType(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything);
            Assert.That(varCount, Is.EqualTo(nCount));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get the names at location" + standardErrorMessage)]
        public void WhenInvoking_GetNamesAtLocation_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridApi.Expect(api => api.GetVarNames(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, out Arg<int[]>.Out(null).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetNamesAtLocation(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetNamesAtLocation_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int[] varIds = { 1, 1, 2, 3, 5, 8 };
            int meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            uGridApi.Expect(api => api.GetVarNames(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, out Arg<int[]>.Out(varIds).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetNamesAtLocation(meshId, locationType);
            var varNameIds = grid.VarNameIdsAtLocationInMesh[meshId - 1];

            Assert.That(varNameIds[locationType], Is.EqualTo(varIds));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't rewrite grid coordinates" + standardErrorMessage)]
        public void WhenInvoking_RewriteGridCoordinates_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridApi.Expect(api => api.WriteXYCoordinateValues(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.RewriteGridCoordinatesForMeshId(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_RewriteGridCoordinates_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            uGridApi.Expect(api => api.WriteXYCoordinateValues(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.RewriteGridCoordinatesForMeshId(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get z values at mesh faces" + standardErrorMessage)]
        public void WhenInvoking_WriteZValuesAtFaces_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridApi.Expect(api => api.WriteZCoordinateValues(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<double[]>.Is.Anything))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.WriteZValuesAtFacesForMeshId(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_WriteZValuesAtFaces_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            uGridApi.Expect(api => api.WriteZCoordinateValues(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<double[]>.Is.Anything))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.WriteZValuesAtFacesForMeshId(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't write z values at mesh nodes" + standardErrorMessage)]
        public void WhenInvoking_WriteZValuesAtNodes_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridApi.Expect(api => api.WriteZCoordinateValues(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<double[]>.Is.Anything))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.WriteZValuesAtNodesForMeshId(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_WriteZValuesAtNodes_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            uGridApi.Expect(api => api.WriteZCoordinateValues(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<double[]>.Is.Anything))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.WriteZValuesAtNodesForMeshId(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get meshname" + standardErrorMessage)]
        public void WhenInvoking_GetMeshName_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridApi.Expect(api => api.GetMeshName(Arg<int>.Is.Anything, out Arg<string>.Out(null).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            grid.GetMeshName(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetMeshName_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            string name = "MyNetwork";
            uGridApi.Expect(api => api.GetMeshName(Arg<int>.Is.Anything, out Arg<string>.Out(name).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var networkName = grid.GetMeshName(Arg<int>.Is.Anything);
            Assert.That(networkName, Is.EqualTo(name));
        }
    }
}
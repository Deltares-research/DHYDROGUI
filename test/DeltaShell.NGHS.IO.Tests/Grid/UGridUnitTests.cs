using System;
using System.Collections.Generic;
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
        private const string standardErrorMessage = ", because of error number: -1";
        private int errorValue = -1;
        private int noErrorValue = GridApiDataSet.GridConstants.NOERR;

        [Test]
        public void WhenGetting_gridZCoordinateFillValue_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            DoWithMockedUGridApi(
                uGridApi => uGridApi
                            .Expect(api => api.ZCoordinateFillValue)
                            .Return(2).Repeat.Once(),
                grid =>
                {
                    double gridZCoordinateFillValue = grid.ZCoordinateFillValue;
                    Assert.That(gridZCoordinateFillValue, Is.EqualTo(2));
                });
        }

        [Test]
        public void WhenInvoking_NumberOf2DMeshes_AndApiReturnsAnErrorValueThenThrowException()
        {
            var numMeshes = 2;

            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi => uGridApi
                                .Expect(api => api.GetNumberOfMeshByType(Arg<UGridMeshType>.Is.Anything, out Arg<int>.Out(numMeshes).Dummy))
                                .Return(errorValue).Repeat.Once(),
                    grid => { grid.GetNumberOf2DMeshes(); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get the number of 2D meshes" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_NumberOf2DMeshes_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            var numMeshes = 2;
            DoWithMockedUGridApi(
                uGridApi => uGridApi
                            .Expect(api => api.GetNumberOfMeshByType(Arg<UGridMeshType>.Is.Anything, out Arg<int>.Out(numMeshes).Dummy))
                            .Return(noErrorValue).Repeat.Once(),
                grid =>
                {
                    int nMeshes = grid.GetNumberOf2DMeshes();
                    Assert.That(nMeshes, Is.EqualTo(numMeshes));
                });
        }

        [Test]
        public void WhenInvoking_NumberOfNodes_AndApiReturnsAnErrorValueThenThrowException()
        {
            var numNodes = 2;

            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi => uGridApi
                                .Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(numNodes).Dummy))
                                .Return(errorValue).Repeat.Once(),
                    grid => { grid.GetNumberOfNodesForMeshId(Arg<int>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get the number of nodes" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_NumberOfNodes_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            var numNodes = 2;
            DoWithMockedUGridApi(
                uGridApi => uGridApi
                            .Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(numNodes).Dummy))
                            .Return(noErrorValue).Repeat.Once(),
                grid =>
                {
                    int nNodes = grid.GetNumberOfNodesForMeshId(Arg<int>.Is.Anything);
                    Assert.That(nNodes, Is.EqualTo(numNodes));
                });
        }

        [Test]
        public void WhenInvoking_NumberOfEdges_AndApiReturnsAnErrorValueThenThrowException()
        {
            var numEdges = 2;

            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi => uGridApi
                                .Expect(api => api.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(numEdges).Dummy))
                                .Return(errorValue).Repeat.Once(),
                    grid => { grid.GetNumberOfEdgesForMeshId(Arg<int>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get number of edges" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_NumberOfEdges_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            var numEdges = 2;
            DoWithMockedUGridApi(
                uGridApi => uGridApi
                            .Expect(api => api.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(numEdges).Dummy))
                            .Return(noErrorValue).Repeat.Once(),
                grid =>
                {
                    int nEdges = grid.GetNumberOfEdgesForMeshId(Arg<int>.Is.Anything);
                    Assert.That(nEdges, Is.EqualTo(numEdges));
                });
        }

        [Test]
        public void WhenInvoking_NumberOfFaces_AndApiReturnsAnErrorValueThenThrowException()
        {
            var numFaces = 2;

            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi => uGridApi
                                .Expect(api => api.GetNumberOfFaces(Arg<int>.Is.Anything, out Arg<int>.Out(numFaces).Dummy))
                                .Return(errorValue).Repeat.Once(),
                    grid => { grid.GetNumberOfFacesForMeshId(Arg<int>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get number of faces" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_NumberOfFaces_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            var numFaces = 2;
            DoWithMockedUGridApi(
                uGridApi => uGridApi
                            .Expect(api => api.GetNumberOfFaces(Arg<int>.Is.Anything, out Arg<int>.Out(numFaces).Dummy))
                            .Return(noErrorValue).Repeat.Once(),
                grid =>
                {
                    int nFaces = grid.GetNumberOfFacesForMeshId(Arg<int>.Is.Anything);
                    Assert.That(nFaces, Is.EqualTo(numFaces));
                });
        }

        [Test]
        public void WhenInvoking_NumberOfMaxFaceNodes_AndApiReturnsAnErrorValueThenThrowException()
        {
            var numMaxFaceNodes = 2;

            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi => uGridApi
                                .Expect(api => api.GetMaxFaceNodes(Arg<int>.Is.Anything, out Arg<int>.Out(numMaxFaceNodes).Dummy))
                                .Return(errorValue).Repeat.Once(),
                    grid => { grid.GetNumberOfMaxFaceNodesForMeshId(Arg<int>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get max face nodes" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_NumberOfMaxFaceNodes_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            var numMaxFaceNodes = 2;
            DoWithMockedUGridApi(
                uGridApi => uGridApi
                            .Expect(api => api.GetMaxFaceNodes(Arg<int>.Is.Anything, out Arg<int>.Out(numMaxFaceNodes).Dummy))
                            .Return(noErrorValue).Repeat.Once(),
                grid =>
                {
                    int nMaxFaceNodes = grid.GetNumberOfMaxFaceNodesForMeshId(Arg<int>.Is.Anything);
                    Assert.That(nMaxFaceNodes, Is.EqualTo(numMaxFaceNodes));
                });
        }

        [Test]
        public void WhenInvoking_GetAllNodeCoordinates_AndApiReturnsAnErrorValueForXCoordinatesThenThrowException()
        {
            double[] xCoordinates = {2.0, 3.4};

            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        uGridApi
                            .Expect(api => api.GetNodeXCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(xCoordinates).Dummy))
                            .Return(errorValue).Repeat.Once();
                        uGridApi
                            .Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(2).Dummy))
                            .Return(noErrorValue).Repeat.Once();
                    },
                    grid => { grid.GetAllNodeCoordinatesForMeshId(Arg<int>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get x node coordinates" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_GetAllNodeCoordinates_AndApiReturnsAnErrorValueForYCoordinatesThenThrowException()
        {
            double[] coordinates = {2.0, 3.4};

            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        uGridApi
                            .Expect(api => api.GetNodeXCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(coordinates).Dummy))
                            .Return(noErrorValue).Repeat.Once();
                        uGridApi
                            .Expect(api => api.GetNodeYCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(coordinates).Dummy))
                            .Return(errorValue).Repeat.Once();
                        uGridApi
                            .Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(2).Dummy))
                            .Return(noErrorValue).Repeat.Once();
                    },
                    grid => { grid.GetAllNodeCoordinatesForMeshId(Arg<int>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get y node coordinates" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_GetAllNodeCoordinates_AndApiReturnsAnErrorValueForZCoordinatesThenThrowException()
        {
            double[] coordinates = {2.0, 3.4};

            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        uGridApi
                            .Expect(api => api.GetNodeXCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(coordinates).Dummy))
                            .Return(noErrorValue).Repeat.Once();
                        uGridApi
                            .Expect(api => api.GetNodeYCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(coordinates).Dummy))
                            .Return(noErrorValue).Repeat.Once();
                        uGridApi
                            .Expect(api => api.GetNodeZCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(coordinates).Dummy))
                            .Return(errorValue).Repeat.Once();
                        uGridApi
                            .Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(2).Dummy))
                            .Return(noErrorValue).Repeat.Once();
                    },
                    grid => { grid.GetAllNodeCoordinatesForMeshId(Arg<int>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get z node coordinates" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_GetAllNodeCoordinates_AndNumberOfNodesIsEqualTo0ThenReturnEmptyArray()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi
                        .Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(0).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                },
                grid =>
                {
                    Coordinate[] result = grid.GetAllNodeCoordinatesForMeshId(Arg<int>.Is.Anything);
                    Assert.That(result, Is.EqualTo(new Coordinate[0]));
                });
        }

        [Test]
        public void WhenInvoking_GetAllNodeCoordinates_ThenMethodSavesTheCoordinatesInTheRightWay()
        {
            var meshId = 1;
            var nNodes = 2;
            double[] xCoordinates = {2.0, 3.4};
            double[] yCoordinates = {-1.0, 8.4};
            double[] zCoordinates = {-1.1, -2.3};
            Coordinate[] expectedResult = {new Coordinate(2.0, -1.0, -1.1), new Coordinate(3.4, 8.4, -2.3)};

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi
                        .Expect(api => api.GetNodeXCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(xCoordinates).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                    uGridApi
                        .Expect(api => api.GetNodeYCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(yCoordinates).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                    uGridApi
                        .Expect(api => api.GetNodeZCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(zCoordinates).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                    uGridApi
                        .Expect(api => api.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(nNodes).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                },
                grid =>
                {
                    Coordinate[] result = grid.GetAllNodeCoordinatesForMeshId(meshId);
                    Assert.That(result, Is.EqualTo(expectedResult));
                });
        }

        [Test]
        public void WhenInvoking_GetEdgeNodesForMesh_AndApiReturnsAnErrorValueThenThrowException()
        {
            int[,] edgeNodes = {{0, 1}, {1, 2}};

            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        uGridApi
                            .Expect(api => api.GetEdgeNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(edgeNodes).Dummy))
                            .Return(errorValue).Repeat.Once();
                    },
                    grid => { grid.GetEdgeNodesForMeshId(Arg<int>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get edge nodes of the mesh" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_GetEdgeNodesForMesh_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int[,] edgeNodes = {{0, 1}, {1, 2}};
            var meshId = 1;
            var numMeshes = 2;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi
                        .Expect(api => api.GetEdgeNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(edgeNodes).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                    uGridApi
                        .Expect(api => api.GetNumberOfMeshByType(Arg<UGridMeshType>.Is.Anything, out Arg<int>.Out(numMeshes).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                },
                grid =>
                {
                    grid.GetEdgeNodesForMeshId(meshId);
                    int[,] nodes = grid.EdgeNodesByMeshId[meshId - 1];
                    Assert.That(nodes[0, 0], Is.EqualTo(0));
                    Assert.That(nodes[0, 1], Is.EqualTo(1));
                    Assert.That(nodes[1, 0], Is.EqualTo(1));
                    Assert.That(nodes[1, 1], Is.EqualTo(2));
                });
        }

        [Test]
        public void WhenInvoking_GetFaceNodesForMesh_AndApiReturnsAnErrorValueThenThrowException()
        {
            int[,] faceNodes = {{0, 1}, {1, 2}};

            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        uGridApi
                            .Expect(api => api.GetFaceNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(faceNodes).Dummy))
                            .Return(errorValue).Repeat.Once();
                    },
                    grid => { grid.GetFaceNodesForMeshId(Arg<int>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get face nodes of the mesh" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_GetFaceNodesForMesh_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int[,] faceNodes = {{0, 1}, {1, 2}};
            var meshId = 1;
            var numMeshes = 2;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi
                        .Expect(api => api.GetFaceNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(faceNodes).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                    uGridApi
                        .Expect(api => api.GetNumberOfMeshByType(Arg<UGridMeshType>.Is.Anything, out Arg<int>.Out(numMeshes).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                },
                grid =>
                {
                    grid.GetFaceNodesForMeshId(meshId);
                    int[,] nodes = grid.FaceNodesByMeshId[meshId - 1];

                    Assert.That(nodes[0, 0], Is.EqualTo(0));
                    Assert.That(nodes[0, 1], Is.EqualTo(1));
                    Assert.That(nodes[1, 0], Is.EqualTo(1));
                    Assert.That(nodes[1, 1], Is.EqualTo(2));
                });
        }

        [Test]
        public void WhenInvoking_NumberOfNamesAtLocation_AndApiReturnsAnErrorValueThenThrowException()
        {
            var nCount = 33;

            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        uGridApi
                            .Expect(api => api.GetVarCount(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, out Arg<int>.Out(nCount).Dummy))
                            .Return(errorValue).Repeat.Once();
                    },
                    grid => { grid.NumberOfNamesForLocationType(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get the number of names for location type" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_NumberOfNamesAtLocation_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            var nCount = 33;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi
                        .Expect(api => api.GetVarCount(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, out Arg<int>.Out(nCount).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                },
                grid =>
                {
                    int varCount = grid.NumberOfNamesForLocationType(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything);
                    Assert.That(varCount, Is.EqualTo(nCount));
                });
        }

        [Test]
        public void WhenInvoking_GetNamesAtLocation_AndApiReturnsAnErrorValueThenThrowException()
        {
            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        uGridApi
                            .Expect(api => api.GetVarNames(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, out Arg<int[]>.Out(null).Dummy))
                            .Return(errorValue).Repeat.Once();
                    },
                    grid => { grid.GetNamesAtLocation(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get the names at location" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_GetNamesAtLocation_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int[] varIds = {1, 1, 2, 3, 5, 8};
            var meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi
                        .Expect(api => api.GetVarNames(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, out Arg<int[]>.Out(varIds).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                },
                grid =>
                {
                    grid.GetNamesAtLocation(meshId, locationType);
                    Dictionary<GridApiDataSet.LocationType, int[]> varNameIds = grid.VarNameIdsByLocationTypeByMeshId[meshId - 1];

                    Assert.That(varNameIds[locationType], Is.EqualTo(varIds));
                });
        }

        [Test]
        public void WhenInvoking_RewriteGridCoordinates_AndApiReturnsAnErrorValueThenThrowException()
        {
            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        uGridApi
                            .Expect(api => api.WriteXYCoordinateValues(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything))
                            .Return(errorValue).Repeat.Once();
                    },
                    grid => { grid.RewriteGridCoordinatesForMeshId(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't rewrite grid coordinates" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_RewriteGridCoordinates_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi
                        .Expect(api => api.WriteXYCoordinateValues(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything))
                        .Return(noErrorValue).Repeat.Once();
                },
                grid => { grid.RewriteGridCoordinatesForMeshId(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything); });
        }

        [Test]
        public void WhenInvoking_GetMeshName_AndApiReturnsAnErrorValueThenThrowException()
        {
            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        uGridApi
                            .Expect(api => api.GetMeshName(Arg<int>.Is.Anything, out Arg<string>.Out(null).Dummy))
                            .Return(errorValue).Repeat.Once();
                    },
                    grid => { grid.GetMeshName(Arg<int>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Couldn't get meshname" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_GetMeshName_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            var name = "MyNetwork";
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi
                        .Expect(api => api.GetMeshName(Arg<int>.Is.Anything, out Arg<string>.Out(name).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                },
                grid =>
                {
                    string networkName = grid.GetMeshName(Arg<int>.Is.Anything);
                    Assert.That(networkName, Is.EqualTo(name));
                });
        }

        private static void DoWithMockedUGridApi(Action<IUGridApi> addExpectations, Action<UGrid> gridAction)
        {
            var uGridApi = MockRepository.GenerateMock<IUGridApi>();

            using (var grid = new UGrid(TestHelper.GetTestFilePath(UGRID_TEST_FILE)) {GridApi = uGridApi})
            {
                //Expectations setup
                uGridApi
                    .Expect(api => api.Initialized).Return(true).Repeat.Any();
                uGridApi
                    .Expect(api => api.GetConvention())
                    .Return(GridApiDataSet.DataSetConventions.CONV_UGRID).Repeat.Any();
                uGridApi
                    .Expect(api => api.GetVersion())
                    .Return(GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION).Repeat.Any();

                addExpectations?.Invoke(uGridApi);
                uGridApi.Replay();

                //Replay
                gridAction?.Invoke(grid);

                Assert.IsNotNull(uGridApi);
                Assert.IsTrue(uGridApi.Initialized);
            }

            uGridApi.VerifyAllExpectations();
        }

        #region TestWriteZValues

        [Test]
        public void WhenInvoking_WriteZValuesAtFaces_AndApiReturnsAnErrorValueThenThrowException()
        {
            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        uGridApi
                            .Expect(api => api.WriteZCoordinateValues(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<double[]>.Is.Anything))
                            .Return(errorValue).Repeat.Once();
                    },
                    grid => { grid.WriteZValuesAtFacesForMeshId(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Error writing z values at mesh faces" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_WriteZValuesAtFaces_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi
                        .Expect(api => api.WriteZCoordinateValues(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<double[]>.Is.Anything))
                        .Return(noErrorValue).Repeat.Once();
                },
                grid => { grid.WriteZValuesAtFacesForMeshId(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything); });
        }

        [Test]
        public void WhenInvoking_WriteZValuesAtNodes_AndApiReturnsAnErrorValueThenThrowException()
        {
            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        uGridApi
                            .Expect(api => api.WriteZCoordinateValues(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<double[]>.Is.Anything))
                            .Return(errorValue).Repeat.Once();
                    },
                    grid => { grid.WriteZValuesAtNodesForMeshId(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Error writing z values at mesh nodes" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_WriteZValuesAtNodes_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi
                        .Expect(api => api.WriteZCoordinateValues(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<double[]>.Is.Anything))
                        .Return(noErrorValue).Repeat.Once();
                },
                grid => { grid.WriteZValuesAtNodesForMeshId(Arg<int>.Is.Anything, Arg<double[]>.Is.Anything); });
        }

        #endregion

        #region TestReadZValues

        [Test]
        public void WhenInvoking_ReadZValuesAtFaces_AndApiReturnsAnErrorValueThenThrowException()
        {
            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        double[] zValues = null;

                        uGridApi
                            .Expect(api => api.ReadZCoordinateValues(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, Arg<string>.Is.Anything, out Arg<double[]>.Out(zValues).Dummy))
                            .Return(errorValue).Repeat.Once();
                    },
                    grid => { grid.ReadZValuesAtFacesForMeshId(Arg<int>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Error reading z values at mesh faces" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_ReadZValuesAtFaces_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    double[] zValues = null;

                    uGridApi
                        .Expect(api => api.ReadZCoordinateValues(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, Arg<string>.Is.Anything, out Arg<double[]>.Out(zValues).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                },
                grid => { grid.ReadZValuesAtFacesForMeshId(Arg<int>.Is.Anything); });
        }

        [Test]
        public void WhenInvoking_ReadZValuesAtNodes_AndApiReturnsAnErrorValueThenThrowException()
        {
            void Test()
            {
                DoWithMockedUGridApi(
                    uGridApi =>
                    {
                        double[] zValues = null;

                        uGridApi
                            .Expect(api => api.ReadZCoordinateValues(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, Arg<string>.Is.Anything, out Arg<double[]>.Out(zValues).Dummy))
                            .Return(errorValue).Repeat.Once();
                    },
                    grid => { grid.ReadZValuesAtNodesForMeshId(Arg<int>.Is.Anything); });
            }

            Assert.That(Test, Throws.Exception.With.Message.EqualTo("Error reading z values at mesh nodes" + standardErrorMessage));
        }

        [Test]
        public void WhenInvoking_ReadZValuesAtNodes_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    double[] zValues = null;

                    uGridApi
                        .Expect(api => api.ReadZCoordinateValues(Arg<int>.Is.Anything, Arg<GridApiDataSet.LocationType>.Is.Anything, Arg<string>.Is.Anything, out Arg<double[]>.Out(zValues).Dummy))
                        .Return(noErrorValue).Repeat.Once();
                },
                grid => { grid.ReadZValuesAtNodesForMeshId(Arg<int>.Is.Anything); });
        }

        #endregion
    }
}
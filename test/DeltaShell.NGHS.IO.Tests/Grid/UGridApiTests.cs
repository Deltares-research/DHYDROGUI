using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGridApiTests
    {
        private IUGridApi uGridApi;
        private IUGridApi uRemoteGridApi;
        private MockRepository mocks;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            uGridApi = mocks.DynamicMock<UGridApi>();
            uRemoteGridApi = mocks.DynamicMock<RemoteUGridApi>();
            TypeUtils.SetField(uRemoteGridApi, "api", uGridApi);
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void UGridApiTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(0.0d, TypeUtils.GetField<UGridApi, double>(uGridApi, "fillValue"), 0.001d);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi, "nEdges"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi, "nFaces"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi, "nMaxFaceNodes"));
        }

        [Test]
        public void RemoteUGridApiTest()
        {
            mocks.ReplayAll();
            var api = TypeUtils.GetField(uRemoteGridApi, "api");
            var ugridApi = api as IUGridApi;
            Assert.That(api != null);
            Assert.That(ugridApi != null);

            Assert.AreEqual(0.0d, TypeUtils.GetField<UGridApi, double>(ugridApi, "fillValue"), 0.001d);
            Assert.AreEqual(-1, TypeUtils.GetField(ugridApi, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(ugridApi, "nEdges"));
            Assert.AreEqual(-1, TypeUtils.GetField(ugridApi, "nFaces"));
            Assert.AreEqual(-1, TypeUtils.GetField(ugridApi, "nMaxFaceNodes"));
        }

        [Test]
        public void WriteXYCoordinateValuesInvalidInitializationTest()
        {
            mocks.ReplayAll();
        }

        [Test]
        public void WriteXYCoordinateValuesTest()
        {
            //TypeUtils.SetField(uGridApi, "ioncid", 1);
            mocks.ReplayAll();
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_put_node_coordinates
        }

        [Test]
        public void WriteXYCoordinateValuesApiCallFailedTest()
        {
            mocks.ReplayAll();
        }

        [Test]
        public void WriteXYCoordinateValuesExceptionTest()
        {
            mocks.ReplayAll();
        }


        [Test]
        public void WriteZCoordinateValuesInvalidInitializationTest()
        {
            mocks.ReplayAll();
        }

        [Test]
        public void WriteZCoordinateValuesTest()
        {
            mocks.ReplayAll();
        }

        [Test]
        public void WriteZCoordinateValuesApiCallFailedTest()
        {
            mocks.ReplayAll();
        }

        [Test]
        public void WriteZCoordinateValuesExceptionTest()
        {
            mocks.ReplayAll();
        }

        [Test]
        public void GetMeshNameInvalidInitializationTest()
        {
            mocks.ReplayAll();
        }

        [Test]
        public void GetMeshNameTest()
        {
            mocks.ReplayAll();
            TypeUtils.SetField(uGridApi, "ioncid", 0);
            string name;
            var ierr = uGridApi.GetMeshName(1, out name);
            Assert.AreEqual(string.Empty, name);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            //TypeUtils.SetField(uGridApi, "ioncid", 1);
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_get_mesh_name
        }

        [Test]
        public void GetMeshNameApiCallFailedTest()
        {
            mocks.ReplayAll();
        }

        [Test]
        public void GetMeshNameExceptionTest()
        {
            mocks.ReplayAll();
        }


        [Test]
        public void GetNumberOfNodesInitializationFailedTest()
        {
            // uGridApi
            

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNumberOfNodesTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            int nNodes;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            TypeUtils.SetField(uGridApi, "ioncid", 1);
            TypeUtils.SetField(uGridApi, "nNodes", 2);
            var ierr = uGridApi.GetNumberOfNodes(1, out nNodes);
            Assert.AreEqual(2, nNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_get_node_count


            // uRemoteGridApi
        }

        [Test]
        public void GetNumberOfNodesApiCallFailedTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNumberOfNodesExceptionTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNumberOfEdgesInitializationFailedTest()
        {
            // uGridApi
            

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNumberOfEdgesTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            //    uGridApi.Expect(a => a.GetNumberOfEdges(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            //    mocks.ReplayAll();
            //    TypeUtils.SetField(uGridApi, "ioncid", 1);
            //    TypeUtils.SetField(uGridApi, "nEdges", 2);

            //    Assert.AreEqual(2, uGridApi.GetNumberOfEdges(1));
            //    //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_get_edge_count
            
            // uRemoteGridApi
        }

        [Test]
        public void GetNumberOfEdgesApiCallFailedTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNumberOfEdgesExceptionTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNumberOfFacesInitializationFailedTest()
        {
            // uGridApi
            

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNumberOfFacesTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            //    uGridApi.Expect(a => a.GetNumberOfFaces(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            //    mocks.ReplayAll();
            //    TypeUtils.SetField(uGridApi, "ioncid", 1);
            //    TypeUtils.SetField(uGridApi, "nFaces", 2);

            //    Assert.AreEqual(2, uGridApi.GetNumberOfFaces(1));
            //    //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_get_face_count

            // uRemoteGridApi
        }

        [Test]
        public void GetNumberOfFacesApiCallFailedTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNumberOfFacesExceptionTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetMaxFaceNodesInitializationFailedTest()
        {
            // uGridApi
            

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetMaxFaceNodesTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            //    uGridApi.Expect(a => a.GetMaxFaceNodes(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            //    mocks.ReplayAll();
            //    TypeUtils.SetField(uGridApi, "ioncid", 1);
            //    TypeUtils.SetField(uGridApi, "nMaxFaceNodes", 2);

            //    Assert.AreEqual(2, uGridApi.GetMaxFaceNodes(1));
            //    //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_get_max_face_nodes


            // uRemoteGridApi
        }

        [Test]
        public void GetMaxFaceNodesApiCallFailedTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetMaxFaceNodesExceptionTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNodeXCoordinatesInitializationFailedTest()
        {
            // uGridApi
            

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNodeXCoordinatesTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNodeXCoordinatesApiCallFailedTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNodeXCoordinatesExceptionTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNodeYCoordinatesInitializationFailedTest()
        {
            // uGridApi
           
            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNodeYCoordinatesTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNodeYCoordinatesApiCallFailedTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNodeYCoordinatesExceptionTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNodeZCoordinatesInitializationFailedTest()
        {
            // uGridApi
            

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNodeZCoordinatesTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNodeZCoordinatesApiCallFailedTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetNodeZCoordinatesExceptionTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetEdgeNodesForMeshInitializationFailedTest()
        {
            // uGridApi
            

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetEdgeNodesForMeshTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetEdgeNodesForMeshApiCallFailedTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetEdgeNodesForMeshExceptionTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetFaceNodesForMeshInitializationFailedTest()
        {
            // uGridApi
            

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetFaceNodesForMeshTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetFaceNodesForMeshApiCallFailedTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetFaceNodesForMeshExceptionTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }


        [Test]
        public void GetVarCountInitializationFailedTest()
        {
            // uGridApi
            

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetVarCountTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetVarCountApiCallFailedTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetVarCountExceptionTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetVarNamesInitializationFailedTest()
        {
            // uGridApi
            
            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetVarNamesTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetVarNamesApiCallFailedTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }

        [Test]
        public void GetVarNamesExceptionTest()
        {
            // uGridApi

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi

            // uRemoteGridApi
        }
    }
}
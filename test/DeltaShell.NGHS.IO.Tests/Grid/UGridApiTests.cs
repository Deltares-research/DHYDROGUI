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
        }

        [Test]
        public void WriteXYCoordinateValuesTest()
        {
            //TypeUtils.SetField(uGridApi, "ioncid", 1);
            mocks.ReplayAll();
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_put_node_coordinates
        }

        [Test]
        public void WriteZCoordinateValuesTest()
        {
            //TypeUtils.SetField(uGridApi, "ioncid", 1);
            mocks.ReplayAll();
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_put_var (voor "node_z")
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
        public void GetNumberOfNodesTest()
        {
            int nNodes;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            TypeUtils.SetField(uGridApi, "ioncid", 1);
            TypeUtils.SetField(uGridApi, "nNodes", 2);
            var ierr = uGridApi.GetNumberOfNodes(1, out nNodes);
            Assert.AreEqual(2, nNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_get_node_count
        }

        //[Test]
        //public void GetNumberOfEdgesTest()
        //{
        //    uGridApi.Expect(a => a.GetNumberOfEdges(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
        //    mocks.ReplayAll();
        //    TypeUtils.SetField(uGridApi, "ioncid", 1);
        //    TypeUtils.SetField(uGridApi, "nEdges", 2);

        //    Assert.AreEqual(2, uGridApi.GetNumberOfEdges(1));
        //    //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_get_edge_count
        //}

        //[Test]
        //public void GetNumberOfFacesTest()
        //{
        //    uGridApi.Expect(a => a.GetNumberOfFaces(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
        //    mocks.ReplayAll();
        //    TypeUtils.SetField(uGridApi, "ioncid", 1);
        //    TypeUtils.SetField(uGridApi, "nFaces", 2);

        //    Assert.AreEqual(2, uGridApi.GetNumberOfFaces(1));
        //    //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_get_face_count
        //}

        //[Test]
        //public void GetMaxFaceNodesTest()
        //{
        //    uGridApi.Expect(a => a.GetMaxFaceNodes(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
        //    mocks.ReplayAll();
        //    TypeUtils.SetField(uGridApi, "ioncid", 1);
        //    TypeUtils.SetField(uGridApi, "nMaxFaceNodes", 2);

        //    Assert.AreEqual(2, uGridApi.GetMaxFaceNodes(1));
        //    //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_get_max_face_nodes
        //}

        [Test]
        public void GetNodeXCoordinatesTest()
        {
            mocks.ReplayAll();
        }

        [Test]
        public void GetNodeYCoordinatesTest()
        {
            mocks.ReplayAll();

        }

        [Test]
        public void GetNodeZCoordinatesTest()
        {
            mocks.ReplayAll();

        }

        [Test]
        public void GetEdgeNodesForMeshTest()
        {
            mocks.ReplayAll();

        }

        [Test]
        public void GetFaceNodesForMeshTest()
        {
            mocks.ReplayAll();

        }

        [Test]
        public void GetVarCountTest()
        {
            mocks.ReplayAll();

        }

        [Test]
        public void GetVarNamesTest()
        {
            mocks.ReplayAll();

        }
    }
}
using System;
using System.Runtime.InteropServices;
using System.Text;
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
        private UGridApi uGridApi;
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
        // ReSharper disable once InconsistentNaming
        public void WriteXYCoordinateValuesInvalidInitializationTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();
           
            mocks.ReplayAll();
            // uGridApi
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi.WriteXYCoordinateValues(1, new []{0.0}, new[]{0.0}));

            // uRemoteGridApi
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 }));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void WriteXYCoordinateValuesGetNodesErrorTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out int nodes)).Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Twice();
            
            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void WriteXYCoordinateValuesTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr xPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
            IntPtr yPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
            int nNodes = 0;
            wrapper.Expect(w => w.ionc_put_node_coordinates(ref id, ref meshid, ref xPtr, ref yPtr, ref nNodes))
                .IgnoreArguments()
                .OutRef(id, meshid, xPtr, yPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();
            
            // uGridApi
            var ierr = uGridApi.WriteXYCoordinateValues(1, new[] {0.0}, new[] {0.0});
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.WriteXYCoordinateValues(1, new[] {0.0}, new[] {0.0});
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);

        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void WriteXYCoordinateValuesApiCallFailedTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr xPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
            IntPtr yPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
            int nNodes = 0;
            wrapper.Expect(w => w.ionc_put_node_coordinates(ref id, ref meshid, ref xPtr, ref yPtr, ref nNodes))
                .IgnoreArguments()
                .OutRef(id, meshid, xPtr, yPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void WriteXYCoordinateValuesExceptionTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr xPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
            IntPtr yPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
            int nNodes = 0;
            wrapper.Expect(w => w.ionc_put_node_coordinates(ref id, ref meshid, ref xPtr, ref yPtr, ref nNodes))
                .IgnoreArguments()
                .OutRef(id, meshid, xPtr, yPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("testException"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void WriteZCoordinateValuesInvalidInitializationTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            mocks.ReplayAll();
            // uGridApi
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi.WriteZCoordinateValues(1, new[] { 0.0 }));

            // uRemoteGridApi
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGridApi.WriteZCoordinateValues(1, new[] { 0.0 }));
        }

        [Test]
        public void WriteZCoordinateValuesGetNodesFailedTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out int nodes)).Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Twice();

            mocks.ReplayAll();
            // uGridApi
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi.WriteZCoordinateValues(1, new[] { 0.0 }));

            // uRemoteGridApi
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGridApi.WriteZCoordinateValues(1, new[] { 0.0 }));
        }

        [Test]
        public void WriteZCoordinateValuesTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr zPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
            int nVal = 0;
            int locationId = 0;
            string varName = "";
            wrapper.Expect(w => w.ionc_put_var(ref id, ref meshid, ref locationId, varName, ref zPtr, ref nVal))
                .IgnoreArguments()
                .OutRef(id, meshid, locationId, zPtr, nVal)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.WriteZCoordinateValues(1, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.WriteZCoordinateValues(1, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);

        }

        [Test]
        public void WriteZCoordinateValuesApiCallFailedTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr zPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
            int nVal = 0;
            int locationId = 0;
            string varName = "";
            wrapper.Expect(w => w.ionc_put_var(ref id, ref meshid, ref locationId, varName, ref zPtr, ref nVal))
                .IgnoreArguments()
                .OutRef(id, meshid, locationId, zPtr, nVal)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.WriteZCoordinateValues(1, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.WriteZCoordinateValues(1, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
        }

        [Test]
        public void WriteZCoordinateValuesExceptionTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr zPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
            int nVal = 0;
            int locationId = 0;
            string varName = "";
            wrapper.Expect(w => w.ionc_put_var(ref id, ref meshid, ref locationId, varName, ref zPtr, ref nVal))
                .IgnoreArguments()
                .OutRef(id, meshid, locationId, zPtr, nVal)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Throw(new Exception("testException"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.WriteZCoordinateValues(1, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.WriteZCoordinateValues(1, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void GetMeshNameInvalidInitializationTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();
            mocks.ReplayAll();

            // uGridApi
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi.GetMeshName(1,out string name));

            // uRemoteGridApi
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGridApi.GetMeshName(1, out name));
        }

        [Test]
        public void GetMeshNameTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int meshId = 0;
            StringBuilder meshName = new StringBuilder("");
            wrapper.Expect(w => w.ionc_get_mesh_name(ref id, ref meshId, meshName)).OutRef(id, meshId)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            mocks.ReplayAll();

            // uGridApi
            string name;
            var ierr = uGridApi.GetMeshName(1, out name);
            Assert.AreEqual(meshName.ToString(), name);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            
            // uRemoteGridApi
            ierr = uRemoteGridApi.GetMeshName(1, out name);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.AreEqual(meshName.ToString(), name);
        }

        [Test]
        public void GetMeshNameApiCallFailedTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int meshId = 0;
            StringBuilder meshName = new StringBuilder("");
            wrapper.Expect(w => w.ionc_get_mesh_name(ref id, ref meshId, meshName)).OutRef(id, meshId)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            mocks.ReplayAll();

            // uGridApi
            string name;
            var ierr = uGridApi.GetMeshName(1, out name);
            Assert.AreEqual(meshName.ToString(), name);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetMeshName(1, out name);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.AreEqual(meshName.ToString(), name);
        }

        [Test]
        public void GetMeshNameExceptionTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int meshId = 0;
            StringBuilder meshName = new StringBuilder("");
            wrapper.Expect(w => w.ionc_get_mesh_name(ref id, ref meshId, meshName)).OutRef(id, meshId)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Throw(new Exception("testException")).Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            mocks.ReplayAll();

            // uGridApi
            string name;
            var ierr = uGridApi.GetMeshName(1, out name);
            Assert.AreEqual(meshName.ToString(), name);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetMeshName(1, out name);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(meshName.ToString(), name);
        }


        [Test]
        [TestCase(false, 1, 1)]
        [TestCase(true, -1, 1)]
        [TestCase(true, 1, -1)]
        public void GetNumberOfNodesInitializationFailedTest(bool initialized, int meshId, int nNodes)
        {
            int nodes;

            // uGridApi
            uGridApi.Expect(a => a.GetNumberOfNodes(meshId, out nodes))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi.Expect(a => a.Initialized).Return(initialized).Repeat.Twice();
            TypeUtils.SetField(uGridApi, "nNodes", nNodes);

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.ionc_get_node_count(ref id, ref nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfNodes(meshId, out nodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.AreEqual(nNetworkNodes, nodes);
            Assert.AreEqual(nNetworkNodes, TypeUtils.GetField(uGridApi, "nNodes"));

            // uRemoteGridApi
            int rNodes;
            TypeUtils.SetField(uGridApi, "nNodes", nNodes);
            ierr = uRemoteGridApi.GetNumberOfNodes(meshId, out rNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.AreEqual(nNetworkNodes, rNodes);
            Assert.AreEqual(nNetworkNodes, TypeUtils.GetField(uGridApi, "nNodes"));
        }

        [Test]
        public void GetNumberOfNodesTest()
        {
            // uGridApi
            int nNodes;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi, "nNodes", 8);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfNodes(1, out nNodes);
            Assert.AreEqual(8, nNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            
            // uRemoteGridApi
            int rnNodes;
            ierr = uRemoteGridApi.GetNumberOfNodes(1, out rnNodes);
            Assert.AreEqual(8, rnNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
        }

        [Test]
        public void GetNumberOfNodesApiCallFailedTest()
        {
            int nodes;

            // uGridApi
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nodes))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.ionc_get_node_count(ref id, ref nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes).Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            // uRemoteGridApi
            int rNodes;
            //uRemoteGridApi.Expect(a => a.GetNumberOfNodes(meshId, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfNodes(1, out nodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.AreEqual(-1, nodes);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi, "nNodes"));

            // uRemoteGridApi
            TypeUtils.SetField(uGridApi, "nNodes", -1);
            ierr = uRemoteGridApi.GetNumberOfNodes(1, out rNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.AreEqual(-1, rNodes);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi, "nNodes"));
        }

        [Test]
        public void GetNumberOfNodesExceptionTest()
        {
            int nodes;

            // uGridApi
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nodes))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.ionc_get_node_count(ref id, ref nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes).Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("testException")).Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            // uRemoteGridApi
            int rNodes;
            //uRemoteGridApi.Expect(a => a.GetNumberOfNodes(meshId, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfNodes(1, out nodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(-1, nodes);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi, "nNodes"));

            // uRemoteGridApi
            TypeUtils.SetField(uGridApi, "nNodes", -1);
            ierr = uRemoteGridApi.GetNumberOfNodes(1, out rNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(-1, rNodes);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi, "nNodes"));
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
            int meshId = 1;
            int locationId = 1;
            int nCount;

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetVarCount(meshId, locationId, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetVarCount(meshId, locationId, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void GetVarCountTest()
        {
            var ioncid = 1;
            var meshId = 1;
            var locationId = 1;
            var nCount = 0;

            // wrapper
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            wrapper.Expect(w => w.ionc_get_var_count(ref ioncid, ref meshId, ref locationId, ref nCount)).IgnoreArguments()
                .OutRef(ioncid, meshId, locationId, nCount).Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetVarCount(meshId, locationId, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetVarCount(meshId, locationId, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
        }

        [Test]
        public void GetVarCountApiCallFailedTest()
        {
            var ioncid = 1;
            var meshId = 1;
            var locationId = 1;
            var nCount = 0;

            // wrapper
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            wrapper.Expect(w => w.ionc_get_var_count(ref ioncid, ref meshId, ref locationId, ref nCount)).IgnoreArguments()
                .OutRef(ioncid, meshId, locationId, nCount).Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetVarCount(meshId, locationId, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetVarCount(meshId, locationId, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void GetVarCountExceptionTest()
        {
            var ioncid = 1;
            var meshId = 1;
            var locationId = 1;
            var nCount = 0;

            // wrapper
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            wrapper.Expect(w => w.ionc_get_var_count(ref ioncid, ref meshId, ref locationId, ref nCount)).IgnoreArguments()
                .OutRef(ioncid, meshId, locationId, nCount).Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("TestException")).Repeat.Twice();

            TypeUtils.SetField(uGridApi, "wrapper", wrapper);

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetVarCount(meshId, locationId, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetVarCount(meshId, locationId, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void GetVarNamesInitializationFailedTest()
        {
            int meshId = 1;
            int locationId = 1;
            int[] varIds;
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Once();

            // uRemoteGridApi
            uRemoteGridApi.Expect(a => a.Initialized).Return(false).Repeat.Once();

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetVarNames(meshId, locationId, out varIds);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetVarNames(meshId, locationId, out varIds);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }
        
        [Test]
        public void GetVarNamesTest()
        {
            //int meshId = 1;
            //int locationId = 1;
            //int[] varIds;
            //int varCount = 0;

            //// uGridApi
            //uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            //uGridApi.Expect(a => a.GetVarCount(meshId, locationId, out varCount)).OutRef(varCount).IgnoreArguments().Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Once();

            //// uRemoteGridApi

            //mocks.ReplayAll();

            //// uGridApi
            //var ierr = uGridApi.GetVarNames(meshId, locationId, out varIds);

            //// uRemoteGridApi
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
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

        // UGridApi field names
        private const string NumFacesVarName = "nFaces";
        private const string NumMaxFaceNodesVarName = "nMaxFaceNodes";
        private const string WrapperVarName = "wrapper";
        private const string ApiVarName = "api";
        private const string FillValueVarName = "fillValue";

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            uGridApi = mocks.DynamicMock<UGridApi>();
            uRemoteGridApi = mocks.DynamicMock<RemoteUGridApi>();
            TypeUtils.SetField(uRemoteGridApi, ApiVarName, uGridApi);
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
            Assert.AreEqual(0.0d, TypeUtils.GetField<UGridApi, double>(uGridApi, FillValueVarName), 0.001d);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi, NumFacesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi, NumMaxFaceNodesVarName));
        }

        [Test]
        public void RemoteUGridApiTest()
        {
            mocks.ReplayAll();
            var api = TypeUtils.GetField(uRemoteGridApi, ApiVarName);
            var ugridApi = api as IUGridApi;
            Assert.That(api != null);
            Assert.That(ugridApi != null);

            Assert.AreEqual(0.0d, TypeUtils.GetField<UGridApi, double>(ugridApi, FillValueVarName), 0.001d);
            Assert.AreEqual(-1, TypeUtils.GetField(ugridApi, NumFacesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(ugridApi, NumMaxFaceNodesVarName));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void WriteXYCoordinateValuesInvalidInitializationTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            mocks.ReplayAll();
            // uGridApi
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 }));

            // uRemoteGridApi
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 }));
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void WriteXYCoordinateValuesGetNodesErrorTest()
        {
            // uGridApi
            int nodes;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nodes)).Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
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

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

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

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

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
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, "", "", new[] { 0.0 }));

            // uRemoteGridApi
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, "", "", new[] { 0.0 }));
        }
        
        [Test]
        public void WriteZCoordinateValuesTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr zPtr = IntPtr.Zero;
            int nVal = 0;
            GridApiDataSet.LocationType locationType = GridApiDataSet.LocationType.UG_LOC_NONE;
            string varName = "";
            wrapper.Expect(w => w.ionc_put_var(ref id, ref meshid, locationType, varName, ref zPtr, ref nVal))
                .IgnoreArguments()
                .OutRef(id, meshid, zPtr, nVal)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, "", "", new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, "", "", new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);

        }

        [Test]
        public void WriteZCoordinateValuesApiCallFailedTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int ioncid = 0;
            int meshId = 0;
            IntPtr zPtr = IntPtr.Zero;
            int nVal = 0;
            GridApiDataSet.LocationType locationType = GridApiDataSet.LocationType.UG_LOC_NONE;
            string varName = "";
            wrapper.Expect(w => w.ionc_put_var(ref ioncid, ref meshId, locationType, varName, ref zPtr, ref nVal))
                .IgnoreArguments()
                .OutRef(ioncid, meshId, zPtr, nVal)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, "", "", new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, "", "", new[] { 0.0 });
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
            IntPtr zPtr = IntPtr.Zero;
            int nVal = 0;
            GridApiDataSet.LocationType locationType = GridApiDataSet.LocationType.UG_LOC_NONE;
            string varName = "";
            wrapper.Expect(w => w.ionc_put_var(ref id, ref meshid, locationType, varName, ref zPtr, ref nVal))
                .IgnoreArguments()
                .OutRef(id, meshid, locationType, zPtr, nVal)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Throw(new Exception("testException"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, "", "", new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, "", "", new[] { 0.0 });
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void GetMeshNameInvalidInitializationTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();
            mocks.ReplayAll();

            // uGridApi
            string name;
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi.GetMeshName(1, out name));

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

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

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

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

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

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

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
        public void GetNumberOfNodesTest()
        {
            int ioncid = 1;
            int networkId = 1;
            int nNetworkNodes = 8;

            // wrapper
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
            wrapper.Expect(w => w.ionc_get_node_count(ref ioncid, ref networkId, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(ioncid, networkId, nNetworkNodes).Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            // uRemoteGridApi
            uRemoteGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi
            int numberOfNodes;
            var ierr = uGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out numberOfNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.That(numberOfNodes, Is.EqualTo(nNetworkNodes));

            // uRemoteGridApi
            numberOfNodes = -1;
            ierr = uRemoteGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out numberOfNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.That(numberOfNodes, Is.EqualTo(nNetworkNodes));
        }

        [Test]
        public void GivenUGridApiWhenGettingNumberOfNodesNotInitializedThenReturnFatalErrorValue()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            // uRemoteGridApi
            uRemoteGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void GivenUGridApiWhenApiCallThrosExceptionThenReturnFatalErrorValue()
        {
            int ioncid = 1;
            int networkId = 1;
            int nNetworkNodes = 8;

            // wrapper
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
            wrapper.Expect(w => w.ionc_get_node_count(ref ioncid, ref networkId, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(ioncid, networkId, nNetworkNodes)
                .Throw(new Exception("testException")).Repeat.Twice();

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            // uRemoteGridApi
            uRemoteGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }
        
        [Test]
        public void GivenUGridApiWhenGettingNumberOfEdgesAndNotInitializedThenReturnFatalErrorValue()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            // uRemoteGridApi
            uRemoteGridApi.Expect(a => a.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void GivenUGridApiWhenGettingNumberOfEdgesAndApiCallThrowsExceptionThenReturnFatalErrorValue()
        {
            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshEdges = 8;

            // wrapper
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
            wrapper.Expect(w => w.ionc_get_edge_count(ref ioncid, ref networkId, ref numberOfMeshEdges)).IgnoreArguments()
                .OutRef(ioncid, networkId, numberOfMeshEdges)
                .Throw(new Exception("testException")).Repeat.Twice();

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            // uRemoteGridApi
            uRemoteGridApi.Expect(a => a.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        [TestCase(false, 1, 1)]
        [TestCase(true, -1, 1)]
        [TestCase(true, 1, -1)]
        public void GetNumberOfFacesInitializationFailedTest(bool initialized, int meshId, int nFaces)
        {
            int faces;
            // uGridApi
            uGridApi.Expect(a => a.GetNumberOfFaces(meshId, out faces))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi.Expect(a => a.Initialized).Return(initialized).Repeat.Twice();
            TypeUtils.SetField(uGridApi, NumFacesVarName, nFaces);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int meshid = 0;
            int numberOfFaces = 5;
            wrapper.Expect(w => w.ionc_get_face_count(ref id, ref meshid, ref numberOfFaces)).IgnoreArguments()
                .OutRef(id, meshid, numberOfFaces)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfFaces(meshId, out faces);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.AreEqual(numberOfFaces, faces);
            // uRemoteGridApi

            TypeUtils.SetField(uGridApi, NumFacesVarName, nFaces);
            ierr = uRemoteGridApi.GetNumberOfFaces(meshid, out faces);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.AreEqual(numberOfFaces, faces);
        }

        [Test]
        public void GetNumberOfFacesTest()
        {
            // uGridApi
            int nFaces;
            uGridApi.Expect(a => a.GetNumberOfFaces(1, out nFaces)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi, NumFacesVarName, 8);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfFaces(1, out nFaces);
            Assert.AreEqual(8, nFaces);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNumberOfFaces(1, out nFaces);
            Assert.AreEqual(8, nFaces);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
        }

        [Test]
        public void GetNumberOfFacesApiCallFailedTest()
        {
            // uGridApi
            int nFaces;
            uGridApi.Expect(a => a.GetNumberOfFaces(1, out nFaces)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int meshid = 0;
            int numberOfFaces = 8;
            wrapper.Expect(w => w.ionc_get_face_count(ref id, ref meshid, ref numberOfFaces)).IgnoreArguments()
                .OutRef(id, meshid, numberOfFaces)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfFaces(meshid, out nFaces);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.AreEqual(-1, nFaces);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNumberOfFaces(meshid, out nFaces);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.AreEqual(-1, nFaces);
        }

        [Test]
        public void GetNumberOfFacesExceptionTest()
        {
            // uGridApi
            int nFaces;
            uGridApi.Expect(a => a.GetNumberOfFaces(1, out nFaces)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int meshid = 0;
            int numberOfFaces = 8;
            wrapper.Expect(w => w.ionc_get_face_count(ref id, ref meshid, ref numberOfFaces)).IgnoreArguments()
                .OutRef(id, meshid, numberOfFaces)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("testException"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfFaces(meshid, out nFaces);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(-1, nFaces);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNumberOfFaces(meshid, out nFaces);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(-1, nFaces);
        }

        [Test]
        [TestCase(false, 1, 1)]
        [TestCase(true, -1, 1)]
        [TestCase(true, 1, -1)]
        public void GetMaxFaceNodesInitializationFailedTest(bool initialized, int meshId, int nMaxFaceNodes)
        {
            // uGridApi
            int maxFaceNodes;
            uGridApi.Expect(a => a.GetMaxFaceNodes(meshId, out maxFaceNodes))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi.Expect(a => a.Initialized).Return(initialized).Repeat.Twice();
            TypeUtils.SetField(uGridApi, NumMaxFaceNodesVarName, nMaxFaceNodes);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int meshid = 0;
            int numberOfMaxFaceNodes = 8;
            wrapper.Expect(w => w.ionc_get_max_face_nodes(ref id, ref meshid, ref numberOfMaxFaceNodes))
                .IgnoreArguments().OutRef(id, meshid, numberOfMaxFaceNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
            

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetMaxFaceNodes(meshId, out maxFaceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.AreEqual(numberOfMaxFaceNodes, maxFaceNodes);

            // uRemoteGridApi
            TypeUtils.SetField(uGridApi, NumMaxFaceNodesVarName, nMaxFaceNodes);
            ierr = uRemoteGridApi.GetMaxFaceNodes(meshId, out maxFaceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.AreEqual(numberOfMaxFaceNodes, maxFaceNodes);
        }

        [Test]
        public void GetMaxFaceNodesTest()
        {
            // uGridApi
            int maxFaceNodes;
            uGridApi.Expect(a => a.GetMaxFaceNodes(1, out maxFaceNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi, NumMaxFaceNodesVarName, 8);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetMaxFaceNodes(1, out maxFaceNodes);
            Assert.AreEqual(8, maxFaceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetMaxFaceNodes(1, out maxFaceNodes);
            Assert.AreEqual(8, maxFaceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
        }

        [Test]
        public void GetMaxFaceNodesApiCallFailedTest()
        {
            // uGridApi
            int maxFaceNodes;
            uGridApi.Expect(a => a.GetMaxFaceNodes(1, out maxFaceNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int meshid = 0;
            int nMaxFaceNodes = 8;
            wrapper.Expect(w => w.ionc_get_max_face_nodes(ref id, ref meshid, ref nMaxFaceNodes)).IgnoreArguments()
                .OutRef(id, meshid, nMaxFaceNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetMaxFaceNodes(meshid, out maxFaceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.AreEqual(-1, maxFaceNodes);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetMaxFaceNodes(meshid, out maxFaceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.AreEqual(-1, maxFaceNodes);
        }

        [Test]
        public void GetMaxFaceNodesExceptionTest()
        {
            // uGridApi
            int maxFaceNodes;
            uGridApi.Expect(a => a.GetMaxFaceNodes(1, out maxFaceNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int meshid = 0;
            int nMaxFaceNodes = 8;
            wrapper.Expect(w => w.ionc_get_max_face_nodes(ref id, ref meshid, ref nMaxFaceNodes)).IgnoreArguments()
                .OutRef(id, meshid, nMaxFaceNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Throw(new Exception("testException"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetMaxFaceNodes(meshid, out maxFaceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(-1, maxFaceNodes);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetMaxFaceNodes(meshid, out maxFaceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(-1, maxFaceNodes);
        }

        [Test]
        public void GetNodeXCoordinatesInitializationFailedTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            double[] xCoordinates;
            var ierr = uGridApi.GetNodeXCoordinates(1, out xCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNodeXCoordinates(1, out xCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void GetNodeXCoordinatesGetNodesFailedTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            int nNodes;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Twice();
            
            mocks.ReplayAll();
            
            // uGridApi
            double[] xCoordinates;
            var ierr = uGridApi.GetNodeXCoordinates(1, out xCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            
            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNodeXCoordinates(1, out xCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeXCoordinatesTest(bool useLocalApi)
        {
            // uGridApi
            double[] xCoordinates;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();

            int nNodes = 3;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            nNodes = 3;
            wrapper.Expect(w => w.ionc_get_node_coordinates(ref id, ref meshid, ref xPtr, ref yPtr, ref nNodes))
                .IgnoreArguments().OutRef(id, meshid, xPtr, yPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
            
            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetNodeXCoordinates(1, out xCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.That(xCoordinates.Length == nNodes);
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeXCoordinatesApiCallFailedTest(bool useLocalApi)
        {
            // uGridApi
            double[] xCoordinates;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();

            int nNodes = 3;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            nNodes = 3;
            wrapper.Expect(w => w.ionc_get_node_coordinates(ref id, ref meshid, ref xPtr, ref yPtr, ref nNodes))
                .IgnoreArguments().OutRef(id, meshid, xPtr, yPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetNodeXCoordinates(1, out xCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.That(xCoordinates.Length == nNodes);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeXCoordinatesExceptionTest(bool useLocalApi)
        {
            // uGridApi
            double[] xCoordinates;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();

            int nNodes = 3;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            nNodes = 3;
            wrapper.Expect(w => w.ionc_get_node_coordinates(ref id, ref meshid, ref xPtr, ref yPtr, ref nNodes))
                .IgnoreArguments().OutRef(id, meshid, xPtr, yPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("testException"))
                .Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetNodeXCoordinates(1, out xCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.That(xCoordinates.Length == nNodes);
        }

        [Test]
        public void GetNodeYCoordinatesInitializationFailedTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            double[] yCoordinates;
            var ierr = uGridApi.GetNodeYCoordinates(1, out yCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNodeYCoordinates(1, out yCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void GetNodeYCoordinatesGetNodesFailedTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            int nNodes;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Twice();

            mocks.ReplayAll();

            // uGridApi
            double[] yCoordinates;
            var ierr = uGridApi.GetNodeYCoordinates(1, out yCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNodeYCoordinates(1, out yCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeYCoordinatesTest(bool useLocalApi)
        {
            // uGridApi
            double[] yCoordinates;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();

            int nNodes = 3;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            nNodes = 3;
            wrapper.Expect(w => w.ionc_get_node_coordinates(ref id, ref meshid, ref xPtr, ref yPtr, ref nNodes))
                .IgnoreArguments().OutRef(id, meshid, xPtr, yPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetNodeYCoordinates(1, out yCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.That(yCoordinates.Length == nNodes);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeYCoordinatesApiCallFailedTest(bool useLocalApi)
        {
            // uGridApi
            double[] yCoordinates;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();

            int nNodes = 3;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            nNodes = 3;
            wrapper.Expect(w => w.ionc_get_node_coordinates(ref id, ref meshid, ref xPtr, ref yPtr, ref nNodes))
                .IgnoreArguments().OutRef(id, meshid, xPtr, yPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetNodeYCoordinates(1, out yCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.That(yCoordinates.Length == nNodes);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeYCoordinatesExceptionTest(bool useLocalApi)
        {
            // uGridApi
            double[] yCoordinates;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();

            int nNodes = 3;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            nNodes = 3;
            wrapper.Expect(w => w.ionc_get_node_coordinates(ref id, ref meshid, ref xPtr, ref yPtr, ref nNodes))
                .IgnoreArguments().OutRef(id, meshid, xPtr, yPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("testException"))
                .Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetNodeYCoordinates(1, out yCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.That(yCoordinates.Length == nNodes);
        }

        [Test]
        public void GetNodeZCoordinatesInitializationFailedTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();
            
            mocks.ReplayAll();

            // uGridApi
            double[] zCoordinates;
            var ierr = uGridApi.GetNodeZCoordinates(1, out zCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.That(zCoordinates.Length == 0);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNodeZCoordinates(1, out zCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.That(zCoordinates.Length == 0);
        }

        [Test]
        public void GetNodeZCoordinatesGetNodesFailedTest()
        {
            int nNodes = 4;

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(nNodes).Dummy))
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Twice();

            mocks.ReplayAll();

            // uGridApi
            double[] zCoordinates = {2.0, 4.0};
            var ierr = uGridApi.GetNodeZCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(zCoordinates).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNodeZCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(zCoordinates).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeZCoordinates_NodeZ_Test(bool useLocalApi)
        {
            // uGridApi
            double[] zCoordinates;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();

            int nNodes = 3;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            nNodes = 3;
            int location = 0;
            string varName = "";
            double fillValue = 0;
            wrapper.Expect(w => w.ionc_get_var(ref id, ref meshid, ref location, varName, ref zPtr, ref nNodes, ref fillValue))
                .IgnoreArguments().OutRef(id, meshid, location, zPtr, nNodes, fillValue)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetNodeZCoordinates(1, out zCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.That(zCoordinates.Length == 3);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeZCoordinates_NetNodeZ_Test(bool useLocalApi)
        {
            var nNodes = 3;
            double[] zCoordinates;

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            uGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(nNodes).Dummy))
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            int location = 0;
            string varName = "";
            double fillValue = 0;

            wrapper.Expect(w => w.ionc_get_var(ref id, ref meshid, ref location, varName, ref zPtr, ref nNodes, ref fillValue))
                .IgnoreArguments().OutRef(id, meshid, location, zPtr, nNodes, fillValue)
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Once();

            wrapper.Expect(w => w.ionc_get_var(ref id, ref meshid, ref location, varName, ref zPtr, ref nNodes, ref fillValue))
                .IgnoreArguments().OutRef(id, meshid, location, zPtr, nNodes, fillValue)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetNodeZCoordinates(1, out zCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.That(zCoordinates.Length == 3);
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeZCoordinatesApiCallFailedTest(bool useLocalApi)
        {
            int nNodes = 3;
            double[] zCoordinates;

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            uGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(nNodes).Dummy))
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            int location = 0;
            string varName = "";
            double fillValue = 0;

            wrapper.Expect(w => w.ionc_get_var(ref id, ref meshid, ref location, varName, ref zPtr, ref nNodes, ref fillValue))
                .IgnoreArguments().OutRef(id, meshid, location, zPtr, nNodes, fillValue)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Once();

            wrapper.Expect(w => w.ionc_get_var(ref id, ref meshid, ref location, varName, ref zPtr, ref nNodes, ref fillValue))
                .IgnoreArguments().OutRef(id, meshid, location, zPtr, nNodes, fillValue)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetNodeZCoordinates(1, out zCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.That(zCoordinates.Length == 0);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeZCoordinatesExceptionTest(bool useLocalApi)
        {
            // uGridApi
            double[] zCoordinates;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();

            int nNodes = 3;
            uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            nNodes = 3;
            int location = 0;
            string varName = "";
            double fillValue = 0;

            wrapper.Expect(w => w.ionc_get_var(ref id, ref meshid, ref location, varName, ref zPtr, ref nNodes, ref fillValue))
                .IgnoreArguments().OutRef(id, meshid, location, zPtr, nNodes, fillValue)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("testException"))
                .Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetNodeZCoordinates(1, out zCoordinates);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.That(zCoordinates.Length == 0);
        }

        [Test]
        public void GetEdgeNodesForMeshInitializationFailedTest()
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi
            int[,] edgeNodes;
            var ierr = uGridApi.GetEdgeNodesForMesh(1, out edgeNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetEdgeNodesForMesh(1, out edgeNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void GetEdgeNodesForMeshGetEdgesFailedTest()
        {
            // uGridApi
            int nEdges = 3;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi.Expect(a => a.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(nEdges).Dummy))
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Twice();

            mocks.ReplayAll();

            // uGridApi
            var edgeNodes = new int[0, 0];
            var ierr = uGridApi.GetEdgeNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(edgeNodes).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetEdgeNodesForMesh(1, out edgeNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetEdgeNodesForMeshTest(bool useLocalApi)
        {
            // uGridApi
            int nEdges = 5;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            uGridApi.Expect(a => a.GetNumberOfEdges(1, out nEdges)).IgnoreArguments().OutRef(nEdges)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int meshid = 0;
            int numberOfEdges = nEdges;
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfEdges * GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_AN_EDGE);

            wrapper.Expect(w => w.ionc_get_edge_nodes(ref id, ref meshid, ref ptr, ref numberOfEdges)).IgnoreArguments()
                .OutRef(id, meshid, ptr, numberOfEdges).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            int[,] edgeNodes;
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetEdgeNodesForMesh(1, out edgeNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.That(edgeNodes.GetLength(0) == nEdges);
            Assert.That(edgeNodes.GetLength(1) == GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_AN_EDGE);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetEdgeNodesForMeshApiCallFailedTest(bool useLocalApi)
        {
            // uGridApi
            int nEdges = 5;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            uGridApi.Expect(a => a.GetNumberOfEdges(1, out nEdges)).IgnoreArguments().OutRef(nEdges)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int meshid = 0;
            int numberOfEdges = nEdges;
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfEdges * GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_AN_EDGE);

            wrapper.Expect(w => w.ionc_get_edge_nodes(ref id, ref meshid, ref ptr, ref numberOfEdges)).IgnoreArguments()
                .OutRef(id, meshid, ptr, numberOfEdges).Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            int[,] edgeNodes;
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetEdgeNodesForMesh(1, out edgeNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.That(edgeNodes.GetLength(0) == 0);
            Assert.That(edgeNodes.GetLength(1) == 0);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetEdgeNodesForMeshExceptionTest(bool useLocalApi)
        {
            // uGridApi
            int nEdges = 5;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            uGridApi.Expect(a => a.GetNumberOfEdges(1, out nEdges)).IgnoreArguments().OutRef(nEdges)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int meshid = 0;
            int numberOfEdges = nEdges;
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfEdges * GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_AN_EDGE);

            wrapper.Expect(w => w.ionc_get_edge_nodes(ref id, ref meshid, ref ptr, ref numberOfEdges)).IgnoreArguments()
                .OutRef(id, meshid, ptr, numberOfEdges).Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("testException"))
                .Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uRemoteGridApi

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            int[,] edgeNodes;
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetEdgeNodesForMesh(1, out edgeNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.That(edgeNodes.GetLength(0) == 0);
            Assert.That(edgeNodes.GetLength(1) == 0);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetFaceNodesForMeshInitializationFailedTest(bool useLocalApi)
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Once();

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            int[,] faceNodes;
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetFaceNodesForMesh(1, out faceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.That(faceNodes.GetLength(0) == 0);
            Assert.That(faceNodes.GetLength(1) == 0);

            }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetFaceNodesForMeshGetFacesFailedTest(bool useLocalApi)
        {
            // uGridApi
            int nFaces = 2;
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            uGridApi.Expect(a => a.GetNumberOfFaces(Arg<int>.Is.Anything, out Arg<int>.Out(nFaces).Dummy))
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Once();

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            int[,] faceNodes;
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetFaceNodesForMesh(1, out faceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.That(faceNodes.GetLength(0) == 0);
            Assert.That(faceNodes.GetLength(1) == 0);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetFaceNodesForMeshGetFaceNodesFailedTest(bool useLocalApi)
        {
            var nFaces = 1;
            var maxFaceNodes = 1;

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            uGridApi.Expect(a => a.GetNumberOfFaces(Arg<int>.Is.Anything, out Arg<int>.Out(nFaces).Dummy))
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();
            uGridApi.Expect(a => a.GetMaxFaceNodes(Arg<int>.Is.Anything, out Arg<int>.Out(maxFaceNodes).Dummy))
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Once();

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            int[,] faceNodes = new int[0, 0]; ;
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetFaceNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(faceNodes).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.That(faceNodes.GetLength(0) == 0);
            Assert.That(faceNodes.GetLength(1) == 0);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetFaceNodesForMeshTest(bool useLocalApi)
        {
            int nFaces = 4;
            int maxFaceNodes = 3;

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            uGridApi.Expect(a => a.GetNumberOfFaces(Arg<int>.Is.Anything, out Arg<int>.Out(nFaces).Dummy))
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();
            uGridApi.Expect(a => a.GetMaxFaceNodes(Arg<int>.Is.Anything, out Arg<int>.Out(maxFaceNodes).Dummy))
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFaces * maxFaceNodes);
            int fillValue = 0;
            wrapper.Expect(w => w.ionc_get_face_nodes(ref id, ref meshid, ref ptr, ref nFaces, ref maxFaceNodes,
                    ref fillValue)).IgnoreArguments().OutRef(id, meshid, ptr, nFaces, maxFaceNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            int[,] faceNodes;
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetFaceNodesForMesh(1, out faceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.That(faceNodes.GetLength(0) == nFaces);
            Assert.That(faceNodes.GetLength(1) == maxFaceNodes);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetFaceNodesForMeshApiCallFailedTest(bool useLocalApi)
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            int nFaces = 4;
            uGridApi.Expect(a => a.GetNumberOfFaces(1, out nFaces)).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();
            int maxFaceNodes = 3;
            uGridApi.Expect(a => a.GetMaxFaceNodes(1, out maxFaceNodes)).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFaces * maxFaceNodes);
            int fillValue = 0;
            wrapper.Expect(w => w.ionc_get_face_nodes(ref id, ref meshid, ref ptr, ref nFaces, ref maxFaceNodes,
                    ref fillValue)).IgnoreArguments().OutRef(id, meshid, ptr, nFaces, maxFaceNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            int[,] faceNodes;
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetFaceNodesForMesh(1, out faceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.That(faceNodes.GetLength(0) == 0);
            Assert.That(faceNodes.GetLength(1) == 0);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetFaceNodesForMeshExceptionTest(bool useLocalApi)
        {
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            int nFaces = 4;
            uGridApi.Expect(a => a.GetNumberOfFaces(1, out nFaces)).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();
            int maxFaceNodes = 3;
            uGridApi.Expect(a => a.GetMaxFaceNodes(1, out maxFaceNodes)).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int meshid = 0;
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFaces * maxFaceNodes);
            int fillValue = 0;
            wrapper.Expect(w => w.ionc_get_face_nodes(ref id, ref meshid, ref ptr, ref nFaces, ref maxFaceNodes,
                    ref fillValue)).IgnoreArguments().OutRef(id, meshid, ptr, nFaces, maxFaceNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Throw(new Exception("testException"))
                .Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            int[,] faceNodes;
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetFaceNodesForMesh(1, out faceNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.That(faceNodes.GetLength(0) == 0);
            Assert.That(faceNodes.GetLength(1) == 0);
        }


        [Test]
        public void GetVarCountInitializationFailedTest()
        {
            var meshId = 0;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NONE;
            int nCount;

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetVarCount(meshId, locationType, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetVarCount(meshId, locationType, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void GetVarCountTest()
        {
            var ioncid = 1;
            var meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            var nCount = 0;

            // wrapper
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            wrapper.Expect(w => w.ionc_get_var_count(ref ioncid, ref meshId, locationType, ref nCount)).IgnoreArguments()
                .OutRef(ioncid, meshId, locationType, nCount).Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetVarCount(meshId, locationType, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetVarCount(meshId, locationType, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
        }

        [Test]
        public void GetVarCountApiCallFailedTest()
        {
            var ioncid = 1;
            var meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            var nCount = 0;

            // wrapper
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            wrapper.Expect(w => w.ionc_get_var_count(ref ioncid, ref meshId, locationType, ref nCount)).IgnoreArguments()
                .OutRef(ioncid, meshId, locationType, nCount).Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetVarCount(meshId, locationType, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetVarCount(meshId, locationType, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
        }

        [Test]
        public void GetVarCountExceptionTest()
        {
            var ioncid = 1;
            var meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            var nCount = 0;

            // wrapper
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            wrapper.Expect(w => w.ionc_get_var_count(ref ioncid, ref meshId, locationType, ref nCount)).IgnoreArguments()
                .OutRef(ioncid, meshId, locationType, nCount).Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("TestException")).Repeat.Twice();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetVarCount(meshId, locationType, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetVarCount(meshId, locationType, out nCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        public void GetVarNamesInitializationFailedTest()
        {
            int meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            int[] varIds;
            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Once();

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetVarNames(meshId, locationType, out varIds);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetVarNames(meshId, locationType, out varIds);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetVarNamesGetVarCountFailedTest(bool useLocalApi)
        {
            int meshId = 0;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NONE;
            int[] varIds;

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nCount = 5;
            wrapper.Expect(w => w.ionc_get_var_count(ref id, ref meshId, locationType, ref nCount)).IgnoreArguments()
                .OutRef(id, meshId, locationType, nCount).Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Once();
            
            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetVarNames(meshId, locationType, out varIds);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.That(varIds.Length == 0);
        }
        
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetVarNamesTest(bool useLocalApi)
        {
            int meshId = 0;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            int[] varIds;

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nCount = 5;
            wrapper.Expect(w => w.ionc_get_var_count(ref id, ref meshId, locationType, ref nCount)).IgnoreArguments()
                .OutRef(id, meshId, nCount).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nCount);
            int nVar = nCount;
            wrapper.Expect(w => w.ionc_inq_varids(ref id, ref meshId, locationType, ref ptr, ref nVar))
                .IgnoreArguments().OutRef(id, meshId, ptr, nVar).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();
            
            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetVarNames(meshId, locationType, out varIds);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, ierr);
            Assert.That(varIds.Length == nCount);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetVarNamesApiCallFailedTest(bool useLocalApi)
        {
            int meshId = 0;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            int[] varIds;

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nCount = 5;
            wrapper.Expect(w => w.ionc_get_var_count(ref id, ref meshId, locationType, ref nCount)).IgnoreArguments()
                .OutRef(id, meshId, locationType, nCount).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nCount);
            int nVar = nCount;
            wrapper.Expect(w => w.ionc_inq_varids(ref id, ref meshId, locationType, ref ptr, ref nVar))
                .IgnoreArguments().OutRef(nVar).Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetVarNames(meshId, locationType, out varIds);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
            Assert.That(varIds.Length == 0);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetVarNamesExceptionTest(bool useLocalApi)
        {
            int meshId = 0;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            int[] varIds;

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nCount = 5;
            wrapper.Expect(w => w.ionc_get_var_count(ref id, ref meshId, locationType, ref nCount)).IgnoreArguments()
                .OutRef(id, meshId, locationType, nCount).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nCount);
            int nVar = nCount;
            wrapper.Expect(w => w.ionc_inq_varids(ref id, ref meshId, locationType, ref ptr, ref nVar))
                .IgnoreArguments().OutRef(nVar)
                .Throw(new Exception("testException"))
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();

            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

            mocks.ReplayAll();

            // uGridApi : uRemoteGridApi
            var api = useLocalApi ? uGridApi : uRemoteGridApi;
            var ierr = api.GetVarNames(meshId, locationType, out varIds);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, ierr);
            Assert.That(varIds.Length == 0);
        }
    }
}
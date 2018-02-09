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
    [Category("DoNotRunForCodeCoverage")]
    public class UGridApiTests
    {
        // UGridApi field names
        private const string WrapperVarName = "wrapper";
        private const string ApiVarName = "api";
        private const string FillValueVarName = "fillValue";

        private static void DoWithMockedUGridApi(Action<UGridApi> uGridApiAction, Action<IUGridApi> uRemoteGridApiAction)
        {
            var uGridApi = MockRepository.GenerateMock<UGridApi>();
            var uRemoteGridApi = MockRepository.GenerateMock<RemoteUGridApi>();

            TypeUtils.SetField(uRemoteGridApi, ApiVarName, uGridApi);

            uGridApiAction?.Invoke(uGridApi);
            uRemoteGridApiAction?.Invoke(uRemoteGridApi);

            uGridApi.Replay();
            uRemoteGridApi.Replay();

            uGridApi.VerifyAllExpectations();
            uRemoteGridApi.VerifyAllExpectations();
        }

        [Test]
        public void UGridApiTest()
        {
            DoWithMockedUGridApi(
                uGridApi => { Assert.AreEqual(0.0d, TypeUtils.GetField<UGridApi, double>(uGridApi, FillValueVarName), 0.001d); },
                uRemoteGridApi => {});
        }

        [Test]
        public void RemoteUGridApiTest()
        {
            DoWithMockedUGridApi(
                uGridApi => { },
                uRemoteGridApi =>
                {
                    var api = TypeUtils.GetField(uRemoteGridApi, ApiVarName);
                    var ugridApi = api as IUGridApi;
                    Assert.That(api != null);
                    Assert.That(ugridApi != null);

                    Assert.AreEqual(0.0d, TypeUtils.GetField<UGridApi, double>(ugridApi, FillValueVarName), 0.001d);
                });            
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void WriteXYCoordinateValuesInvalidInitializationTest()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 }));
                },
                uRemoteGridApi =>
                {
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uRemoteGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 }));
                });
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void WriteXYCoordinateValuesGetNodesErrorTest()
        {
            int nodes;
            DoWithMockedUGridApi(
                uGridApi => 
                {
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
                    uGridApi.Expect(a => a.GetNumberOfNodes(1, out nodes)).Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR).Repeat.Twice();

                    var ierr = uGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    var ierr = uRemoteGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void WriteXYCoordinateValuesTest()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
                    //
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr xPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
                    IntPtr yPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
                    int nNodes = 0;
                    wrapper.Expect(w => w.PutNodeCoordinates(id, meshId, xPtr, yPtr, nNodes))
                        .IgnoreArguments()
                        .OutRef(id, meshId, xPtr, yPtr, nNodes)
                        .Return(GridApiDataSet.GridConstants.NOERR)
                        .Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
                    //
                    var ierr = uGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                },
                uRemoteGridApi =>
                {
                    var ierr = uRemoteGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                });
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void WriteXYCoordinateValuesApiCallFailedTest()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr xPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
                    IntPtr yPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
                    int nNodes = 0;
                    wrapper.Expect(w => w.PutNodeCoordinates(id, meshId, xPtr, yPtr, nNodes))
                        .IgnoreArguments()
                        .OutRef(id, meshId, xPtr, yPtr, nNodes)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                        .Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    // uGridApi
                    var ierr = uGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                });
        }

        [Test]
        // ReSharper disable once InconsistentNaming
        public void WriteXYCoordinateValuesExceptionTest()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr xPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
                    IntPtr yPtr = IntPtr.Zero;// Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 1); 
                    int nNodes = 0;
                    wrapper.Expect(w => w.PutNodeCoordinates(id, meshId, xPtr, yPtr, nNodes))
                        .IgnoreArguments()
                        .OutRef(id, meshId, xPtr, yPtr, nNodes)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                        .Throw(new Exception("testException"))
                        .Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
                    // uGridApi
                    var ierr = uGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.WriteXYCoordinateValues(1, new[] { 0.0 }, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }

        [Test]
        public void WriteZCoordinateValuesInvalidInitializationTest()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, string.Empty, string.Empty, new[] { 0.0 }));
                },
                uRemoteGridApi =>
                {
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uRemoteGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, string.Empty, string.Empty, new[] { 0.0 }));
                });
        }

        [Test]
        public void WriteZCoordinateValuesTest()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr zPtr = IntPtr.Zero;
                    int nVal = 0;
                    GridApiDataSet.LocationType locationType = GridApiDataSet.LocationType.UG_LOC_NONE;
                    string varName = string.Empty;
                    wrapper.Expect(w => w.PutVariable(id, meshId, locationType, varName, zPtr, nVal))
                        .IgnoreArguments()
                        .OutRef(id, meshId, zPtr, nVal)
                        .Return(GridApiDataSet.GridConstants.NOERR)
                        .Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    // uGridApi
                    var ierr = uGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, string.Empty, string.Empty, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, string.Empty, string.Empty, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                });

        }

        [Test]
        public void WriteZCoordinateValuesApiCallFailedTest()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int ioncId = 0;
                    int meshId = 0;
                    IntPtr zPtr = IntPtr.Zero;
                    int nVal = 0;
                    GridApiDataSet.LocationType locationType = GridApiDataSet.LocationType.UG_LOC_NONE;
                    string varName = string.Empty;
                    wrapper.Expect(w => w.PutVariable(ioncId, meshId, locationType, varName, zPtr, nVal))
                        .IgnoreArguments()
                        .OutRef(ioncId, meshId, zPtr, nVal)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                        .Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    // uGridApi
                    var ierr = uGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, string.Empty, string.Empty, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, string.Empty, string.Empty, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                });
        }

        [Test]
        public void WriteZCoordinateValuesExceptionTest()
        {
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr zPtr = IntPtr.Zero;
                    int nVal = 0;
                    GridApiDataSet.LocationType locationType = GridApiDataSet.LocationType.UG_LOC_NONE;
                    string varName = string.Empty;
                    wrapper.Expect(w => w.PutVariable(id, meshId, locationType, varName, zPtr, nVal))
                        .IgnoreArguments()
                        .OutRef(id, meshId, locationType, zPtr, nVal)
                        .Return(GridApiDataSet.GridConstants.NOERR)
                        .Throw(new Exception("testException"))
                        .Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    // uGridApi
                    var ierr = uGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, string.Empty, string.Empty, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.WriteZCoordinateValues(1, GridApiDataSet.LocationType.UG_LOC_NODE, string.Empty, string.Empty, new[] { 0.0 });
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }
        [Test]
        public void GetMeshNameInvalidInitializationTest()
        {
            string name;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uGridApi.GetMeshName(1, out name));
                },
                uRemoteGridApi =>
                {
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uRemoteGridApi.GetMeshName(1, out name));
                });
        }

        [Test]
        public void GetMeshNameTest()
        {
            string name;
            StringBuilder meshName = new StringBuilder(string.Empty);

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();

                    int id = 0;
                    int meshId = 0;
                    wrapper.Expect(w => w.GetMeshName(id, meshId, meshName)).IgnoreArguments()
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
                    
                    // uGridApi
                    var ierr = uGridApi.GetMeshName(1, out name);
                    Assert.AreEqual(meshName.ToString(), name);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetMeshName(1, out name);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.AreEqual(meshName.ToString(), name);
                });
        }

        [Test]
        public void GetMeshNameApiCallFailedTest()
        {
            StringBuilder meshName = new StringBuilder(string.Empty);
            string name;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();

                    int id = 0;
                    int meshId = 0;

                    wrapper.Expect(w => w.GetMeshName(id, meshId, meshName)).IgnoreArguments()
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
                    
                    // uGridApi
                    var ierr = uGridApi.GetMeshName(1, out name);
                    Assert.AreEqual(meshName.ToString(), name);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetMeshName(1, out name);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.AreEqual(meshName.ToString(), name);
                });
        }

        [Test]
        public void GetMeshNameExceptionTest()
        {
            string name;
            StringBuilder meshName = new StringBuilder(string.Empty);
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();

                    int id = 0;
                    int meshId = 0;
                    wrapper.Expect(w => w.GetMeshName(id, meshId, meshName)).IgnoreArguments()
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Throw(new Exception("testException")).Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    // uGridApi
                    var ierr = uGridApi.GetMeshName(1, out name);
                    Assert.AreEqual(meshName.ToString(), name);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetMeshName(1, out name);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.AreEqual(meshName.ToString(), name);
                });
        }

        [Test]
        public void GetNumberOfNodesTest()
        {
            int ioncId = 1;
            int networkId = 1;
            int nNetworkNodes = 8;
            int numberOfNodes;

            var mocks = new MockRepository();
            var uGridApi = mocks.DynamicMock<UGridApi>();
            var uRemoteGridApi = mocks.DynamicMock<RemoteUGridApi>();
            TypeUtils.SetField(uRemoteGridApi, ApiVarName, uGridApi);

            // wrapper
            var wrapper = MockRepository.GenerateMock<GridWrapper>();
            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
            wrapper.Expect(w => w.GetNodeCount(ioncId, networkId, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(nNetworkNodes).Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            // uRemoteGridApi
            uRemoteGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out numberOfNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
            Assert.That(numberOfNodes, Is.EqualTo(nNetworkNodes));

            // uRemoteGridApi
            numberOfNodes = -1;
            ierr = uRemoteGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out numberOfNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
            Assert.That(numberOfNodes, Is.EqualTo(nNetworkNodes));

            mocks.VerifyAll();

        }

        [Test]
        public void GivenUGridApiWhenGettingNumberOfNodesNotInitializedThenReturnFatalErrorValue()
        {
            var mocks = new MockRepository();
            var uGridApi = mocks.DynamicMock<UGridApi>();
            var uRemoteGridApi = mocks.DynamicMock<RemoteUGridApi>();
            TypeUtils.SetField(uRemoteGridApi, ApiVarName, uGridApi);

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();
            // uRemoteGridApi
            uRemoteGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);

            mocks.VerifyAll();

//            DoWithMockedUGridApi(
//                uGridApi =>
//                {
//                    // uGridApi
//                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();
//                    // uGridApi
//                    var ierr = uGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
//                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
//                },
//                uRemoteGridApi =>
//                {
//                    // uRemoteGridApi
//                    uRemoteGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
//                    // uRemoteGridApi
//                    var ierr = uRemoteGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
//                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
//                });
        }

        [Test]
        public void GivenUGridApiWhenApiCallThrowsExceptionThenReturnFatalErrorValue()
        {
            var mocks = new MockRepository();
            var uGridApi = mocks.DynamicMock<UGridApi>();
            var uRemoteGridApi = mocks.DynamicMock<RemoteUGridApi>();
            TypeUtils.SetField(uRemoteGridApi, ApiVarName, uGridApi);

            int ioncId = 1;
            int networkId = 1;
            int nNetworkNodes = 8;

            // wrapper
            var wrapper = MockRepository.GenerateMock<GridWrapper>();
            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
            wrapper.Expect(w => w.GetNodeCount(ioncId, networkId, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(ioncId, networkId, nNetworkNodes)
                .Throw(new Exception("testException")).Repeat.Twice();

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            // uRemoteGridApi
            uRemoteGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);

            mocks.VerifyAll();

        }
        
        [Test]
        public void GivenUGridApiWhenGettingNumberOfEdgesAndNotInitializedThenReturnFatalErrorValue()
        {
            var mocks = new MockRepository();
            var uGridApi = mocks.DynamicMock<UGridApi>();
            var uRemoteGridApi = mocks.DynamicMock<RemoteUGridApi>();
            TypeUtils.SetField(uRemoteGridApi, ApiVarName, uGridApi);

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();
            // uRemoteGridApi
            uRemoteGridApi.Expect(a => a.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);

            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);

            mocks.VerifyAll();
        }

        [Test]
        public void GivenUGridApiWhenGettingNumberOfEdgesAndApiCallThrowsExceptionThenReturnFatalErrorValue()
        {
            var mocks = new MockRepository();
            var uGridApi = mocks.DynamicMock<UGridApi>();
            var uRemoteGridApi = mocks.DynamicMock<RemoteUGridApi>();
            TypeUtils.SetField(uRemoteGridApi, ApiVarName, uGridApi);

            int ioncId = 1;
            int networkId = 1;
            int numberOfMeshEdges = 8;

            // wrapper
            var wrapper = MockRepository.GenerateMock<GridWrapper>();
            TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
            wrapper.Expect(w => w.GetEdgeCount(ioncId, networkId, ref numberOfMeshEdges)).IgnoreArguments()
                .OutRef(ioncId, networkId, numberOfMeshEdges)
                .Throw(new Exception("testException")).Repeat.Twice();

            // uGridApi
            uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            // uRemoteGridApi
            uRemoteGridApi.Expect(a => a.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi
            var ierr = uGridApi.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);


            // uRemoteGridApi
            ierr = uRemoteGridApi.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);

            mocks.VerifyAll();
        }

        [Test]
        public void GetNumberOfFacesInitializationFailedTest()
        {
            int faces;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.GetNumberOfFaces(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy))
                        .CallOriginalMethod(OriginalCallOptions.NoExpectation);
                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();
                    // uGridApi
                    var ierr = uGridApi.GetNumberOfFaces(Arg<int>.Is.Anything, out faces);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    var ierr = uRemoteGridApi.GetNumberOfFaces(Arg<int>.Is.Anything, out faces);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }

        [Test]
        public void GetNumberOfFacesTest()
        {
            int nFaces;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.GetNumberOfFaces(1, out nFaces)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    // wrapper
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    int ioncId = 0;
                    int networkId = 0;
                    nFaces = 8;
                    wrapper.Expect(w => w.GetFaceCount(ioncId, networkId, ref nFaces)).IgnoreArguments()
                        .OutRef(nFaces).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();

                    // uGridApi
                    var ierr = uGridApi.GetNumberOfFaces(Arg<int>.Is.Anything, out nFaces);
                    Assert.AreEqual(8, nFaces);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetNumberOfFaces(Arg<int>.Is.Anything, out nFaces);
                    Assert.AreEqual(8, nFaces);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                });
        }

        [Test]
        public void GetNumberOfFacesApiCallFailedTest()
        {
            int nFaces;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.GetNumberOfFaces(1, out nFaces)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();

                    int id = 0;
                    int meshId = 0;
                    int numberOfFaces = 8;
                    wrapper.Expect(w => w.GetFaceCount(id, meshId, ref numberOfFaces)).IgnoreArguments()
                        .OutRef(id, meshId, numberOfFaces)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                        .Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    // uGridApi
                    var ierr = uGridApi.GetNumberOfFaces(Arg<int>.Is.Anything, out nFaces);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetNumberOfFaces(Arg<int>.Is.Anything, out nFaces);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                });
        }

        [Test]
        public void GetNumberOfFacesExceptionTest()
        {
            int nFaces;
            int meshId = 0;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.GetNumberOfFaces(1, out nFaces)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();

                    int id = 0;
                    int numberOfFaces = 8;
                    wrapper.Expect(w => w.GetFaceCount(id, meshId, ref numberOfFaces)).IgnoreArguments()
                        .OutRef(id, meshId, numberOfFaces)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                        .Throw(new Exception("testException"))
                        .Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                     // uGridApi
                    var ierr = uGridApi.GetNumberOfFaces(meshId, out nFaces);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.AreEqual(-1, nFaces);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetNumberOfFaces(meshId, out nFaces);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.AreEqual(-1, nFaces);
                });
            


        }

        [Test]
        public void GetMaxFaceNodesInitializationFailedTest()
        {
            int maxFaceNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.GetMaxFaceNodes(Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy))
                        .CallOriginalMethod(OriginalCallOptions.NoExpectation);
                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

                    var ierr = uGridApi.GetMaxFaceNodes(Arg<int>.Is.Anything, out maxFaceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetMaxFaceNodes(Arg<int>.Is.Anything, out maxFaceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }

        [Test]
        public void GetMaxFaceNodesTest()
        {
            int maxFaceNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.GetMaxFaceNodes(1, out maxFaceNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int ioncId = 0;
                    int meshId = 0;
                    maxFaceNodes = 8;
                    wrapper.Expect(w => w.GetMaxFaceNodes(ioncId, meshId, ref maxFaceNodes)).IgnoreArguments()
                        .OutRef(maxFaceNodes).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    // uGridApi
                    var ierr = uGridApi.GetMaxFaceNodes(Arg<int>.Is.Anything, out maxFaceNodes);
                    Assert.AreEqual(8, maxFaceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetMaxFaceNodes(Arg<int>.Is.Anything, out maxFaceNodes);
                    Assert.AreEqual(8, maxFaceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                });
        }

        [Test]
        public void GetMaxFaceNodesApiCallFailedTest()
        {
            int maxFaceNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.GetMaxFaceNodes(1, out maxFaceNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();

                    int id = 0;
                    int meshId = 0;
                    int nMaxFaceNodes = 8;
                    wrapper.Expect(w => w.GetMaxFaceNodes(id, meshId, ref nMaxFaceNodes)).IgnoreArguments()
                        .OutRef(id, meshId, nMaxFaceNodes)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                        .Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    // uGridApi
                    var ierr = uGridApi.GetMaxFaceNodes(Arg<int>.Is.Anything, out maxFaceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetMaxFaceNodes(Arg<int>.Is.Anything, out maxFaceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                });
        }

        [Test]
        public void GetMaxFaceNodesExceptionTest()
        {
            int maxFaceNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.GetMaxFaceNodes(1, out maxFaceNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();

                    int id = 0;
                    int meshId = 0;
                    int nMaxFaceNodes = 8;
                    wrapper.Expect(w => w.GetMaxFaceNodes(id, meshId, ref nMaxFaceNodes)).IgnoreArguments()
                        .OutRef(id, meshId, nMaxFaceNodes)
                        .Return(GridApiDataSet.GridConstants.NOERR)
                        .Throw(new Exception("testException"))
                        .Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
                    
                    // uGridApi
                    var ierr = uGridApi.GetMaxFaceNodes(Arg<int>.Is.Anything, out maxFaceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetMaxFaceNodes(Arg<int>.Is.Anything, out maxFaceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }

        [Test]
        public void GetNodeXCoordinatesInitializationFailedTest()
        {
            double[] xCoordinates;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

                    var ierr = uGridApi.GetNodeXCoordinates(1, out xCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetNodeXCoordinates(1, out xCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }

        [Test]
        public void GetNodeXCoordinatesGetNodesFailedTest()
        {
            double[] xCoordinates;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
                    int nNodes;
                    uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments()
                        .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR).Repeat.Twice();

                    var ierr = uGridApi.GetNodeXCoordinates(1, out xCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);

                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetNodeXCoordinates(1, out xCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeXCoordinatesTest(bool useLocalApi)
        {
            double[] xCoordinates;
            int nNodes = 3;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    nNodes = 3;
                    wrapper.Expect(w => w.GetNodeCoordinates(id, meshId, ref xPtr, ref yPtr, nNodes))
                        .IgnoreArguments().OutRef(xPtr, yPtr)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetNodeXCoordinates(1, out xCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(xCoordinates.Length == nNodes);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetNodeXCoordinates(1, out xCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(xCoordinates.Length == nNodes);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeXCoordinatesApiCallFailedTest(bool useLocalApi)
        {
            double[] xCoordinates;
            int nNodes = 3;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    nNodes = 3;
                    wrapper.Expect(w => w.GetNodeCoordinates(id, meshId, ref xPtr, ref yPtr, nNodes))
                        .IgnoreArguments().OutRef(xPtr, yPtr)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetNodeXCoordinates(1, out xCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.That(xCoordinates.Length == nNodes);

                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetNodeXCoordinates(1, out xCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.That(xCoordinates.Length == nNodes);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeXCoordinatesExceptionTest(bool useLocalApi)
        {
            double[] xCoordinates;
            int nNodes = 3;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    nNodes = 3;
                    wrapper.Expect(w => w.GetNodeCoordinates(id, meshId, ref xPtr, ref yPtr, nNodes))
                        .IgnoreArguments().OutRef(id, meshId, xPtr, yPtr, nNodes)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                        .Throw(new Exception("testException"))
                        .Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetNodeXCoordinates(1, out xCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(xCoordinates.Length == nNodes);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetNodeXCoordinates(1, out xCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(xCoordinates.Length == nNodes);
                });
        }

        [Test]
        public void GetNodeYCoordinatesInitializationFailedTest()
        {
            double[] yCoordinates;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

                    var ierr = uGridApi.GetNodeYCoordinates(1, out yCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetNodeYCoordinates(1, out yCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });

        }

        [Test]
        public void GetNodeYCoordinatesGetNodesFailedTest()
        {
            double[] yCoordinates;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
                    int nNodes;
                    uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments()
                        .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR).Repeat.Twice();

                    // uGridApi
                    var ierr = uGridApi.GetNodeYCoordinates(1, out yCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetNodeYCoordinates(1, out yCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeYCoordinatesTest(bool useLocalApi)
        {
            double[] yCoordinates;
            int nNodes = 3;
            DoWithMockedUGridApi(
                uGridApi =>
                {

                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    nNodes = 3;
                    wrapper.Expect(w => w.GetNodeCoordinates(id, meshId, ref xPtr, ref yPtr, nNodes))
                        .IgnoreArguments().OutRef(xPtr, yPtr)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetNodeYCoordinates(1, out yCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(yCoordinates.Length == nNodes);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetNodeYCoordinates(1, out yCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(yCoordinates.Length == nNodes);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeYCoordinatesApiCallFailedTest(bool useLocalApi)
        {
            double[] yCoordinates;
            int nNodes = 3;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    nNodes = 3;
                    wrapper.Expect(w => w.GetNodeCoordinates(id, meshId, ref xPtr, ref yPtr, nNodes))
                        .IgnoreArguments().OutRef(xPtr, yPtr)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetNodeYCoordinates(1, out yCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.That(yCoordinates.Length == nNodes);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetNodeYCoordinates(1, out yCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.That(yCoordinates.Length == nNodes);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeYCoordinatesExceptionApiTest(bool useLocalApi )
        {
            double[] yCoordinates;
            int nNodes = 3;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr xPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    IntPtr yPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    nNodes = 3;
                    wrapper.Expect(w => w.GetNodeCoordinates(id, meshId, ref xPtr, ref yPtr, nNodes))
                        .IgnoreArguments().OutRef(id, meshId, xPtr, yPtr, nNodes)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                        .Throw(new Exception("testException"))
                        .Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetNodeYCoordinates(1, out yCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(yCoordinates.Length == nNodes);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetNodeYCoordinates(1, out yCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(yCoordinates.Length == nNodes);

                });
        }

        [Test]
        public void GetNodeZCoordinatesInitializationFailedTest()
        {
            double[] zCoordinates;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

                    var ierr = uGridApi.GetNodeZCoordinates(1, out zCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(zCoordinates.Length == 0);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetNodeZCoordinates(1, out zCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(zCoordinates.Length == 0);
                });
        }

        [Test]
        public void GetNodeZCoordinatesGetNodesFailedTest()
        {
            double[] zCoordinates = { 2.0, 4.0 };
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    int nNodes = 4;
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
                    uGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(nNodes).Dummy))
                        .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR).Repeat.Twice();

                    // uGridApi
                    var ierr = uGridApi.GetNodeZCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(zCoordinates).Dummy);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetNodeZCoordinates(Arg<int>.Is.Anything, out Arg<double[]>.Out(zCoordinates).Dummy);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeZCoordinates_NodeZ_Test(bool useLocalApi)
        {
            double[] zCoordinates;
            int nNodes = 3;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    nNodes = 3;
                    int location = 0;
                    string varName = string.Empty;
                    double fillValue = 0;
                    wrapper.Expect(w => w.GetVariable(id, meshId, location, varName, ref zPtr, nNodes, ref fillValue))
                        .IgnoreArguments().OutRef(zPtr, fillValue)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetNodeZCoordinates(1, out zCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(zCoordinates.Length == 3);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetNodeZCoordinates(1, out zCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(zCoordinates.Length == 3);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeZCoordinates_NetNodeZ_Test(bool useLocalApi)
        {
            var nNodes = 3;
            double[] zCoordinates;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(nNodes).Dummy))
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    int location = 0;
                    string varName = string.Empty;
                    double fillValue = 0;

                    wrapper.Expect(w => w.GetVariable(id, meshId, location, varName, ref zPtr, nNodes, ref fillValue))
                        .IgnoreArguments().OutRef(zPtr, fillValue)
                        .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR).Repeat.Once();

                    wrapper.Expect(w => w.GetVariable(id, meshId, location, varName, ref zPtr, nNodes, ref fillValue))
                        .IgnoreArguments().OutRef(zPtr, fillValue)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetNodeZCoordinates(1, out zCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(zCoordinates.Length == 3);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetNodeZCoordinates(1, out zCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(zCoordinates.Length == 3);
                });
        }


        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeZCoordinatesApiCallFailedTest(bool useLocalApi)
        {
            int nNodes = 3;
            double[] zCoordinates;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfNodes(Arg<int>.Is.Anything, out Arg<int>.Out(nNodes).Dummy))
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    int location = 0;
                    string varName = string.Empty;
                    double fillValue = 0;

                    wrapper.Expect(w => w.GetVariable(id, meshId, location, varName, ref zPtr, nNodes, ref fillValue))
                        .IgnoreArguments().OutRef(zPtr, fillValue)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetNodeZCoordinates(1, out zCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.That(zCoordinates.Length == 0);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetNodeZCoordinates(1, out zCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.That(zCoordinates.Length == 0);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetNodeZCoordinatesExceptionTest(bool useLocalApi)
        {
            double[] zCoordinates;
            int nNodes = 3;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfNodes(1, out nNodes)).IgnoreArguments().OutRef(nNodes)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
                    nNodes = 3;
                    int location = 0;
                    string varName = string.Empty;
                    double fillValue = 0;

                    wrapper.Expect(w => w.GetVariable(id, meshId, location, varName, ref zPtr, nNodes, ref fillValue))
                        .IgnoreArguments().OutRef(zPtr, fillValue)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                        .Throw(new Exception("testException"))
                        .Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    // uGridApi : uRemoteGridApi
                    if (!useLocalApi) return;

                    var ierr = uGridApi.GetNodeZCoordinates(1, out zCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(zCoordinates.Length == 0);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;

                    var ierr = uRemoteGridApi.GetNodeZCoordinates(1, out zCoordinates);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(zCoordinates.Length == 0);
                });            
        }

        [Test]
        public void GetEdgeNodesForMeshInitializationFailedTest()
        {
            int[,] edgeNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

                    var ierr = uGridApi.GetEdgeNodesForMesh(1, out edgeNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetEdgeNodesForMesh(1, out edgeNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }

        [Test]
        public void GetEdgeNodesForMeshGetEdgesFailedTest()
        {
            var edgeNodes = new int[0, 0];
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    int nEdges = 3;
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
                    uGridApi.Expect(a => a.GetNumberOfEdges(Arg<int>.Is.Anything, out Arg<int>.Out(nEdges).Dummy))
                        .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR).Repeat.Twice();

                    // uGridApi
                    var ierr = uGridApi.GetEdgeNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(edgeNodes).Dummy);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetEdgeNodesForMesh(1, out edgeNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
            
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetEdgeNodesForMeshTest(bool useLocalApi)
        {
            int nEdges = 5;
            int[,] edgeNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfEdges(1, out nEdges)).IgnoreArguments().OutRef(nEdges)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();

                    int id = 0;
                    int meshId = 0;
                    int numberOfEdges = nEdges;
                    IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfEdges * GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_AN_EDGE);

                    wrapper.Expect(w => w.GetEdgeNodes(id, meshId, ref ptr, numberOfEdges)).IgnoreArguments()
                        .OutRef(ptr).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetEdgeNodesForMesh(1, out edgeNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(edgeNodes.GetLength(0) == nEdges);
                    Assert.That(edgeNodes.GetLength(1) == GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_AN_EDGE);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetEdgeNodesForMesh(1, out edgeNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(edgeNodes.GetLength(0) == nEdges);
                    Assert.That(edgeNodes.GetLength(1) == GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_AN_EDGE);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetEdgeNodesForMeshApiCallFailedTest(bool useLocalApi)
        {
            int[,] edgeNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    int nEdges = 5;
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfEdges(1, out nEdges)).IgnoreArguments().OutRef(nEdges)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();

                    int id = 0;
                    int meshId = 0;
                    int numberOfEdges = nEdges;
                    IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfEdges * GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_AN_EDGE);

                    wrapper.Expect(w => w.GetEdgeNodes(id, meshId, ref ptr, numberOfEdges)).IgnoreArguments()
                        .OutRef(ptr).Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetEdgeNodesForMesh(1, out edgeNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.That(edgeNodes.GetLength(0) == 0);
                    Assert.That(edgeNodes.GetLength(1) == 0);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetEdgeNodesForMesh(1, out edgeNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.That(edgeNodes.GetLength(0) == 0);
                    Assert.That(edgeNodes.GetLength(1) == 0);
                });
            
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetEdgeNodesForMeshExceptionTest(bool useLocalApi)
        {
            int[,] edgeNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    int nEdges = 5;
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfEdges(1, out nEdges)).IgnoreArguments().OutRef(nEdges)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();

                    int id = 0;
                    int meshId = 0;
                    int numberOfEdges = nEdges;
                    IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfEdges * GridApiDataSet.GridConstants.NUMBER_OF_NODES_ON_AN_EDGE);

                    wrapper.Expect(w => w.GetEdgeNodes(id, meshId, ref ptr, numberOfEdges)).IgnoreArguments()
                        .OutRef(id, meshId, ptr, numberOfEdges).Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                        .Throw(new Exception("testException"))
                        .Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetEdgeNodesForMesh(1, out edgeNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(edgeNodes.GetLength(0) == 0);
                    Assert.That(edgeNodes.GetLength(1) == 0);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetEdgeNodesForMesh(1, out edgeNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(edgeNodes.GetLength(0) == 0);
                    Assert.That(edgeNodes.GetLength(1) == 0);
                });
           
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetFaceNodesForMeshInitializationFailedTest(bool useLocalApi)
        {
            int[,] faceNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Once();

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetFaceNodesForMesh(1, out faceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(faceNodes.GetLength(0) == 0);
                    Assert.That(faceNodes.GetLength(1) == 0);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetFaceNodesForMesh(1, out faceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(faceNodes.GetLength(0) == 0);
                    Assert.That(faceNodes.GetLength(1) == 0);
                });
            }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetFaceNodesForMeshGetFacesFailedTest(bool useLocalApi)
        {
            int[,] faceNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    int nFaces = 2;
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfFaces(Arg<int>.Is.Anything, out Arg<int>.Out(nFaces).Dummy))
                        .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR).Repeat.Once();

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetFaceNodesForMesh(1, out faceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(faceNodes.GetLength(0) == 0);
                    Assert.That(faceNodes.GetLength(1) == 0);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetFaceNodesForMesh(1, out faceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(faceNodes.GetLength(0) == 0);
                    Assert.That(faceNodes.GetLength(1) == 0);
                });
            
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetFaceNodesForMeshGetFaceNodesFailedTest(bool useLocalApi)
        {
            var nFaces = 1;
            var maxFaceNodes = 1;
            int[,] faceNodes = new int[0, 0];
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfFaces(Arg<int>.Is.Anything, out Arg<int>.Out(nFaces).Dummy))
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();
                    uGridApi.Expect(a => a.GetMaxFaceNodes(Arg<int>.Is.Anything, out Arg<int>.Out(maxFaceNodes).Dummy))
                        .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR).Repeat.Once();

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetFaceNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(faceNodes).Dummy);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(faceNodes.GetLength(0) == 0);
                    Assert.That(faceNodes.GetLength(1) == 0);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetFaceNodesForMesh(Arg<int>.Is.Anything, out Arg<int[,]>.Out(faceNodes).Dummy);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(faceNodes.GetLength(0) == 0);
                    Assert.That(faceNodes.GetLength(1) == 0);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetFaceNodesForMeshTest(bool useLocalApi)
        {
            int nFaces = 4;
            int maxFaceNodes = 3;
            int[,] faceNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    uGridApi.Expect(a => a.GetNumberOfFaces(Arg<int>.Is.Anything, out Arg<int>.Out(nFaces).Dummy))
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();
                    uGridApi.Expect(a => a.GetMaxFaceNodes(Arg<int>.Is.Anything, out Arg<int>.Out(maxFaceNodes).Dummy))
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFaces * maxFaceNodes);
                    int fillValue = 0;
                    wrapper.Expect(w => w.GetFaceNodes(id, meshId, ref ptr, nFaces, maxFaceNodes,
                            ref fillValue)).IgnoreArguments().OutRef(ptr, fillValue)
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetFaceNodesForMesh(1, out faceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(faceNodes.GetLength(0) == nFaces);
                    Assert.That(faceNodes.GetLength(1) == maxFaceNodes);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetFaceNodesForMesh(1, out faceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(faceNodes.GetLength(0) == nFaces);
                    Assert.That(faceNodes.GetLength(1) == maxFaceNodes);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetFaceNodesForMeshApiCallFailedTest(bool useLocalApi)
        {
            int[,] faceNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    int nFaces = 4;
                    uGridApi.Expect(a => a.GetNumberOfFaces(1, out nFaces)).IgnoreArguments()
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();
                    int maxFaceNodes = 3;
                    uGridApi.Expect(a => a.GetMaxFaceNodes(1, out maxFaceNodes)).IgnoreArguments()
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFaces * maxFaceNodes);
                    int fillValue = 0;
                    wrapper.Expect(w => w.GetFaceNodes(id, meshId, ref ptr, nFaces, maxFaceNodes,
                            ref fillValue)).IgnoreArguments().OutRef(ptr, fillValue)
                        .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetFaceNodesForMesh(1, out faceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.That(faceNodes.GetLength(0) == 0);
                    Assert.That(faceNodes.GetLength(1) == 0);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetFaceNodesForMesh(1, out faceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.That(faceNodes.GetLength(0) == 0);
                    Assert.That(faceNodes.GetLength(1) == 0);
                });
            
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetFaceNodesForMeshExceptionTest(bool useLocalApi)
        {
            int[,] faceNodes;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
                    int nFaces = 4;
                    uGridApi.Expect(a => a.GetNumberOfFaces(1, out nFaces)).IgnoreArguments()
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();
                    int maxFaceNodes = 3;
                    uGridApi.Expect(a => a.GetMaxFaceNodes(1, out maxFaceNodes)).IgnoreArguments()
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int meshId = 0;
                    IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nFaces * maxFaceNodes);
                    int fillValue = 0;
                    wrapper.Expect(w => w.GetFaceNodes(id, meshId, ref ptr, nFaces, maxFaceNodes,
                            ref fillValue)).IgnoreArguments().OutRef(id, meshId, ptr, nFaces, maxFaceNodes)
                        .Return(GridApiDataSet.GridConstants.NOERR)
                        .Throw(new Exception("testException"))
                        .Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetFaceNodesForMesh(1, out faceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(faceNodes.GetLength(0) == 0);
                    Assert.That(faceNodes.GetLength(1) == 0);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetFaceNodesForMesh(1, out faceNodes);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(faceNodes.GetLength(0) == 0);
                    Assert.That(faceNodes.GetLength(1) == 0);
                });
            
        }


        [Test]
        public void GetVarCountInitializationFailedTest()
        {
            var meshId = 0;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NONE;
            int nCount;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

                    // uGridApi
                    var ierr = uGridApi.GetVarCount(meshId, locationType, out nCount);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetVarCount(meshId, locationType, out nCount);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }

        [Test]
        public void GetVarCountTest()
        {
            var ioncId = 1;
            var meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            var nCount = 0;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // wrapper
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    wrapper.Expect(w => w.GetVariablesCount(ioncId, meshId, locationType, ref nCount)).IgnoreArguments()
                        .OutRef(ioncId, meshId, locationType, nCount).Return(GridApiDataSet.GridConstants.NOERR)
                        .Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    // uGridApi
                    var ierr = uGridApi.GetVarCount(meshId, locationType, out nCount);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetVarCount(meshId, locationType, out nCount);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                });
        }

        [Test]
        public void GetVarCountApiCallFailedTest()
        {
            var ioncId = 1;
            var meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            var nCount = 0;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // wrapper
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    wrapper.Expect(w => w.GetVariablesCount(ioncId, meshId, locationType, ref nCount)).IgnoreArguments()
                        .OutRef(ioncId, meshId, locationType, nCount).Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                        .Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    // uGridApi
                    var ierr = uGridApi.GetVarCount(meshId, locationType, out nCount);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetVarCount(meshId, locationType, out nCount);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                });
        }

        [Test]
        public void GetVarCountExceptionTest()
        {
            var ioncId = 1;
            var meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            var nCount = 0;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // wrapper
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    wrapper.Expect(w => w.GetVariablesCount(ioncId, meshId, locationType, ref nCount)).IgnoreArguments()
                        .OutRef(ioncId, meshId, locationType, nCount).Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                        .Throw(new Exception("TestException")).Repeat.Twice();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);

                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
                    
                    // uGridApi
                    var ierr = uGridApi.GetVarCount(meshId, locationType, out nCount);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetVarCount(meshId, locationType, out nCount);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });
        }

        [Test]
        public void GetVarNamesInitializationFailedTest()
        {
            int meshId = 1;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            int[] varIds;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    uGridApi.Expect(a => a.Initialized).Return(false).Repeat.Once();
                    // uGridApi
                    var ierr = uGridApi.GetVarNames(meshId, locationType, out varIds);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                },
                uRemoteGridApi =>
                {
                    // uRemoteGridApi
                    var ierr = uRemoteGridApi.GetVarNames(meshId, locationType, out varIds);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                });

        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetVarNamesGetVarCountFailedTest(bool useLocalApi)
        {
            int meshId = 0;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NONE;
            int[] varIds;
            int nCount = 5;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;

                    wrapper.Expect(w => w.GetVariablesCount(id, meshId, locationType, ref nCount)).IgnoreArguments()
                        .OutRef(id, meshId, locationType, nCount).Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetVarNames(meshId, locationType, out varIds);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(varIds.Length == 0);

                },
                uRemoteGridApi =>
                {
                    if(useLocalApi) return;
                    var ierr = uRemoteGridApi.GetVarNames(meshId, locationType, out varIds);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(varIds.Length == 0);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetVarNamesTest(bool useLocalApi)
        {
            int meshId = 0;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            int[] varIds;
            int nCount = 5;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    wrapper.Expect(w => w.GetVariablesCount(id, meshId, locationType, ref nCount)).IgnoreArguments()
                        .OutRef(nCount).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nCount);
                    int nVar = nCount;
                    wrapper.Expect(w => w.InqueryVariableIds(id, meshId, locationType, ref ptr, nVar))
                        .IgnoreArguments().OutRef(ptr).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetVarNames(meshId, locationType, out varIds);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(varIds.Length == nCount);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetVarNames(meshId, locationType, out varIds);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.That(varIds.Length == nCount);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetVarNamesApiCallFailedTest(bool useLocalApi)
        {
            int meshId = 0;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            int[] varIds;
            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int nCount = 5;
                    wrapper.Expect(w => w.GetVariablesCount(id, meshId, locationType, ref nCount)).IgnoreArguments()
                        .OutRef(nCount).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nCount);
                    int nVar = nCount;
                    wrapper.Expect(w => w.InqueryVariableIds(id, meshId, locationType, ref ptr, nVar))
                        .IgnoreArguments().OutRef(ptr).Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetVarNames(meshId, locationType, out varIds);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.That(varIds.Length == 0);
                },
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetVarNames(meshId, locationType, out varIds);
                    Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, ierr);
                    Assert.That(varIds.Length == 0);
                });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetVarNamesExceptionTest(bool useLocalApi)
        {
            int meshId = 0;
            var locationType = GridApiDataSet.LocationType.UG_LOC_NODE;
            int[] varIds;

            DoWithMockedUGridApi(
                uGridApi =>
                {
                    // uGridApi
                    uGridApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    int id = 0;
                    int nCount = 5;
                    wrapper.Expect(w => w.GetVariablesCount(id, meshId, locationType, ref nCount)).IgnoreArguments()
                        .OutRef(id, meshId, locationType, nCount).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nCount);
                    int nVar = nCount;
                    wrapper.Expect(w => w.InqueryVariableIds(id, meshId, locationType, ref ptr, nVar))
                        .IgnoreArguments().OutRef(nVar)
                        .Throw(new Exception("testException"))
                        .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

                    TypeUtils.SetField(uGridApi, WrapperVarName, wrapper);
                    if (!useLocalApi) return;
                    var ierr = uGridApi.GetVarNames(meshId, locationType, out varIds);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(varIds.Length == 0);
                },
                
                uRemoteGridApi =>
                {
                    if (useLocalApi) return;
                    var ierr = uRemoteGridApi.GetVarNames(meshId, locationType, out varIds);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.That(varIds.Length == 0);
                });
        }
    }
}
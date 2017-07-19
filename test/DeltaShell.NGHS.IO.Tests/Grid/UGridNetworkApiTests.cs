using System;
using System.Runtime.InteropServices;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGridNetworkApiTests
    {
        private UGridNetworkApi uGridNetworkApi;
        private UGridNetworkDiscretisationApi uGridNetworkDiscretisationApi;
        private RemoteUGridNetworkApi uRemoteUGridNetworkApi;
        private RemoteUGridNetworkDiscretisationApi uRemoteUGridNetworkDiscretisationApi;
        private MockRepository mocks;

        private const string ApiVarName = "api";

        // UGridNetworkApi field names
        private const string NetworkIdForWritingVarName = "networkIdForWriting";
        private const string NumNodesVarName = "nNodes";
        private const string NumBranchesVarName = "nBranches";
        private const string NumGeometryPointsVarName = "nGeometryPoints";

        // UGridNetworkDiscretisationApi field names
        private const string NumNetworkPointsVarName = "nNetworkPoints";
        private const string MeshIdForWritingVarName = "meshIdForWriting";
        private const string WrapperFieldName = "wrapper";


        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            uGridNetworkApi = mocks.DynamicMock<UGridNetworkApi>();
            uGridNetworkDiscretisationApi = mocks.DynamicMock<UGridNetworkDiscretisationApi>();
            uRemoteUGridNetworkApi = mocks.DynamicMock<RemoteUGridNetworkApi>();
            uRemoteUGridNetworkDiscretisationApi = mocks.DynamicMock<RemoteUGridNetworkDiscretisationApi>();
            TypeUtils.SetField(uRemoteUGridNetworkApi, ApiVarName, uGridNetworkApi);
            TypeUtils.SetField(uRemoteUGridNetworkDiscretisationApi, ApiVarName, uGridNetworkDiscretisationApi);
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void UGridNetworkApiTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NetworkIdForWritingVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));
        }
        
        [Test]
        public void UGridNetworkDiscretisationApiTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkDiscretisationApi, MeshIdForWritingVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));
        }

        [Test]
        public void RemoteUGridNetworkApiTest()
        {
            mocks.ReplayAll();
            var api = TypeUtils.GetField(uRemoteUGridNetworkApi, ApiVarName);
            var uGridNetworkApi = api as IUGridNetworkApi;
            Assert.That(api != null);
            Assert.That(uGridNetworkApi != null);

            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NetworkIdForWritingVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));
        }
        
        [Test]
        public void RemoteUGridNetworkDiscretisationApiTest()
        {
            mocks.ReplayAll();
            var api = TypeUtils.GetField(uRemoteUGridNetworkDiscretisationApi, ApiVarName);
            var UGridNetworkDiscretisationApi = api as IUGridNetworkDiscretisationApi;
            Assert.That(api != null);
            Assert.That(UGridNetworkDiscretisationApi != null);
            
            Assert.AreEqual(-1, TypeUtils.GetField(UGridNetworkDiscretisationApi, MeshIdForWritingVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(UGridNetworkDiscretisationApi, NumNetworkPointsVarName));
        }

        #region Write Network

        [Test]
        public void CreateNetworkInvalidInitializationTest()
        {
            int networkId;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.CreateNetwork("", 0, 0, 0, out networkId)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uGridNetworkApi.CreateNetwork("", 0, 0, 0, out networkId));

            // uRemoteGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uRemoteUGridNetworkApi.CreateNetwork("", 0, 0, 0, out networkId));
        }

        [Test]
        public void CreateNetworkTest()
        {
            // arrange
            string name = "Network name";
            int nNodes = 1;
            int nBranches = 2;
            int nGeoPoints = 3;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nnodes = 1;
            int nbranches = 2;
            int ngeoPoints = 3;

            wrapper
                .Expect(w => w.create_1d_network(id, ref nwid, "", nnodes, nbranches, ngeoPoints))
                .IgnoreArguments()
                .OutRef(nwid)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation)
                .Repeat.Once();

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));

            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, uGridNetworkApi.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(nnodes, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(nbranches, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(ngeoPoints, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));

            // uRemoteGridNetworkApi
            TypeUtils.SetField(uGridNetworkApi, NumNodesVarName, -1);
            TypeUtils.SetField(uGridNetworkApi, NumBranchesVarName, -1);
            TypeUtils.SetField(uGridNetworkApi, NumGeometryPointsVarName, -1);

            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, uRemoteUGridNetworkApi.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(nnodes, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(nbranches, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(ngeoPoints, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));
        }

        [Test]
        public void CreateNetworkApiCallFailedTest()
        {
            // arrange
            string name = "Network name";
            int nNodes = 0;
            int nBranches = 0;
            int nGeoPoints = 0;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nnodes = 1;
            int nbranches = 2;
            int ngeoPoints = 3;
            wrapper
                .Expect(w => w.create_1d_network(id, ref nwid, "", nnodes, nbranches, ngeoPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nnodes, nbranches, ngeoPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();


            // uGridNetworkApi
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));

            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGridNetworkApi.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));

            // uRemoteGridNetworkApi
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));

            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uRemoteUGridNetworkApi.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));

        }

        [Test]
        public void CreateNetworkExceptionTest()
        {
            // arrange
            string name = "Network name";
            int nNodes = 0;
            int nBranches = 0;
            int nGeoPoints = 0;

            //uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nnodes = 1;
            int nbranches = 2;
            int ngeoPoints = 3;
            wrapper
                .Expect(w => w.create_1d_network(id, ref nwid, "", nnodes, nbranches, ngeoPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nnodes, nbranches, ngeoPoints)
                .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));

            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uGridNetworkApi.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));

            // uRemoteGridNetworkApi
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));

            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uRemoteUGridNetworkApi.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void WriteNetworkNodesInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkNodes(new double[0], new double[0], new string[0], new string[0])).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.WriteNetworkNodes(new double[0], new double[0], new string[0], new string[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.WriteNetworkNodes(new double[0], new double[0], new string[0], new string[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        public void WriteNetworkNodesTest()
        {
            // arrange
            var nNodes = 2;
            double[] nodesX = new double[nNodes];
            double[] nodesY = new double[nNodes];
            string[] nodesIds = new[] { "node 1", "node 2" };
            string[] nodesLongnames = new[] { "long name", "long name 2" };

            // uGridNetworkApi
            int nodes;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).IgnoreArguments().OutRef(nNodes).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr nodesXPtr = IntPtr.Zero;
            IntPtr nodesYPtr = IntPtr.Zero;
            var nodesInfo = new GridWrapper.interop_charinfo[nNodes];

            wrapper.Expect(w => w.write_1d_network_nodes(id, nwid, nodesXPtr, nodesYPtr,
                    nodesInfo, nNodes))
                .IgnoreArguments()
                .OutRef(id, nwid, nodesXPtr, nodesYPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, remoteResult);
        }

        [Test]
        public void WriteNetworkNodesApiCallFailedTest()
        {
            // arrange
            var nNodes = 2;
            double[] nodesX = new double[nNodes];
            double[] nodesY = new double[nNodes];
            string[] nodesIds = new[] { "node 1", "node 2" };
            string[] nodesLongnames = new[] { "long name", "long name 2" };

            // uGridNetworkApi
            int nodes;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).IgnoreArguments().OutRef(nNodes).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr nodesXPtr = IntPtr.Zero;
            IntPtr nodesYPtr = IntPtr.Zero;
            var nodesInfo = new GridWrapper.interop_charinfo[nNodes];

            wrapper.Expect(w => w.write_1d_network_nodes(id, nwid, nodesXPtr, nodesYPtr,
                    nodesInfo, nNodes))
                .IgnoreArguments()
                .OutRef(id, nwid, nodesXPtr, nodesYPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR) // Return an arbitrary error
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi

            var result = uGridNetworkApi.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void WriteNetworkNodesExceptionTest()
        {
            // arrange
            var nNodes = 2;
            double[] nodesX = new double[nNodes];
            double[] nodesY = new double[nNodes];
            string[] nodesIds = new[] { "node 1", "node 2" };
            string[] nodesLongnames = new[] { "long name", "long name 2" };

            // uGridNetworkApi
            int nodes;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).IgnoreArguments().OutRef(nNodes).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr nodesXPtr = IntPtr.Zero;
            IntPtr nodesYPtr = IntPtr.Zero;
            var nodesInfo = new GridWrapper.interop_charinfo[nNodes];

            wrapper.Expect(w => w.write_1d_network_nodes(id, nwid, nodesXPtr, nodesYPtr,
                    nodesInfo, nNodes))
                .IgnoreArguments()
                .OutRef(id, nwid, nodesXPtr, nodesYPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(-1, new[] { 0.0 }, new[] { 0.0 }, new[] { "" }, new[] { "" })]
        [TestCase(0, new[] { 0.0 }, new[] { 0.0 }, new[] { "" }, new[] { "" })]
        [TestCase(0, new[] { 0.0, 0.0 }, new[] { 0.0 }, new[] { "" }, new[] { "" })]
        [TestCase(0, new[] { 0.0 }, new[] { 0.0 }, new[] { "", "" }, new[] { "" })]
        [TestCase(0, new[] { 0.0 }, new[] { 0.0 }, new[] { "" }, new[] { "", "" })]
        public void WriteNetworkNodesInitializedButArrayNotCorrectTest(int numberOfNetworkNodes, double[] nodesX, double[] nodesY, string[] nodesIds, string[] nodesLongnames)
        {
            // check with :
            // node Of numbers; 
            // Length xArray; 
            // Length yArray; 
            // Length idArray; 
            // Length desrcArray; 

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();
            int nodes;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).IgnoreArguments().OutRef(numberOfNetworkNodes).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR, uGridNetworkApi.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames));

            // uRemoteGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR, uRemoteUGridNetworkApi.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames));
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void WriteNetworkBranchesInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkBranches(new int[0], new int[0], new double[0], new int[0],
                new string[0], new string[0], new int[0]))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi    
            var result = uGridNetworkApi.WriteNetworkBranches(new int[0], new int[0], new double[0], new int[0],
                new string[0], new string[0], new int[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);

            // uRemoteGridNetworkApi    
            var remoteResult = uRemoteUGridNetworkApi.WriteNetworkBranches(new int[0], new int[0], new double[0], new int[0],
                new string[0], new string[0], new int[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        public void WriteNetworkBranchesTest()
        {
            // arrange
            int nBranches = 2;
            var sourceNodeId = new int[nBranches];
            var targetNodeId = new int[nBranches];
            var branghLength = new double[nBranches];
            var nBranchGeoPoints = new int[nBranches];
            var branchId = new[] { "branch 1", "branch 2" };
            var branchLongnames = new[] { "long name", "long name 2" };
            var branchOrderNumbers = new int[nBranches];

            // uGridNetworkApi
            int branches;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).IgnoreArguments().OutRef(nBranches).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            mocks.ReplayAll();

            int id = 0;
            int nwid = 0;
            IntPtr sourceNodesPtr = IntPtr.Zero;
            IntPtr targetNodesPtr = IntPtr.Zero;
            IntPtr branchLengthPtr = IntPtr.Zero;
            IntPtr branchGeoPtr = IntPtr.Zero;
            var branchesInfo = new GridWrapper.interop_charinfo[nBranches];

            wrapper.Expect(w => w.write_1d_network_branches(id, nwid, sourceNodesPtr, targetNodesPtr, branchesInfo, branchLengthPtr, branchGeoPtr, nBranches))
                .IgnoreArguments()
                .OutRef(id, nwid, sourceNodesPtr, targetNodesPtr, branchLengthPtr, branchGeoPtr, nBranches)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi
                .Expect(a => a.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints,
                    branchId, branchLongnames, branchOrderNumbers))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames, branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames, branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, remoteResult);
        }

        [Test]
        public void WriteNetworkBranchesApiCallFailedTest()
        {
            // arrange
            int nBranches = 2;
            var sourceNodeId = new int[nBranches];
            var targetNodeId = new int[nBranches];
            var branghLength = new double[nBranches];
            var nBranchGeoPoints = new int[nBranches];
            var branchId = new[] { "branch 1", "branch 2" };
            var branchLongnames = new[] { "long name", "long name 2" };
            var branchOrderNumbers = new int[nBranches];

            // uGridNetworkApi
            int branches;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).IgnoreArguments().OutRef(nBranches).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            mocks.ReplayAll();

            int id = 0;
            int nwid = 0;
            IntPtr sourceNodesPtr = IntPtr.Zero;
            IntPtr targetNodesPtr = IntPtr.Zero;
            IntPtr branchLengthPtr = IntPtr.Zero;
            IntPtr branchGeoPtr = IntPtr.Zero;
            var branchesInfo = new GridWrapper.interop_charinfo[nBranches];

            wrapper.Expect(w => w.write_1d_network_branches(id, nwid, sourceNodesPtr, targetNodesPtr, branchesInfo, branchLengthPtr, branchGeoPtr, nBranches))
                .IgnoreArguments()
                .OutRef(id, nwid, sourceNodesPtr, targetNodesPtr, branchLengthPtr, branchGeoPtr, nBranches)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames, branchOrderNumbers)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames, branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames, branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void WriteNetworkBranchesExceptionTest()
        {
            // arrange
            int nBranches = 2;
            var sourceNodeId = new int[nBranches];
            var targetNodeId = new int[nBranches];
            var branghLength = new double[nBranches];
            var nBranchGeoPoints = new int[nBranches];
            var branchId = new[] { "branch 1", "branch 2" };
            var branchLongnames = new[] { "long name", "long name 2" };
            var branchOrderNumbers = new int[nBranches];

            // uGridNetworkApi
            int branches;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).IgnoreArguments().OutRef(nBranches).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr sourceNodesPtr = IntPtr.Zero;
            IntPtr targetNodesPtr = IntPtr.Zero;
            IntPtr branchLengthPtr = IntPtr.Zero;
            IntPtr branchGeoPtr = IntPtr.Zero;
            var branchesInfo = new GridWrapper.interop_charinfo[nBranches];

            wrapper.Expect(w => w.write_1d_network_branches(id, nwid, sourceNodesPtr, targetNodesPtr, branchesInfo, branchLengthPtr, branchGeoPtr, nBranches))
                .IgnoreArguments()
                .OutRef(id, nwid, sourceNodesPtr, targetNodesPtr, branchLengthPtr, branchGeoPtr, nBranches)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames, branchOrderNumbers)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames, branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames, branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(-1, new[] { 1 }, new[] { 1 }, new[] { 1.0 }, new[] { 1 }, new[] { "1" }, new[] { "1" }, new[] { -1 })]
        [TestCase(1, new[] { 1, 2 }, new[] { 1 }, new[] { 1.0 }, new[] { 1 }, new[] { "1" }, new[] { "1" }, new[] { -1 })]
        [TestCase(1, new[] { 1 }, new[] { 1, 2 }, new[] { 1.0 }, new[] { 1 }, new[] { "1" }, new[] { "1" }, new[] { -1 })]
        [TestCase(1, new[] { 1 }, new[] { 1 }, new[] { 1.0, 2.0 }, new[] { 1 }, new[] { "1" }, new[] { "1" }, new[] { -1 })]
        [TestCase(1, new[] { 1 }, new[] { 1 }, new[] { 1.0 }, new[] { 1, 2 }, new[] { "1" }, new[] { "1" }, new[] { -1 })]
        [TestCase(1, new[] { 1 }, new[] { 1 }, new[] { 1.0 }, new[] { 1 }, new[] { "1", "2" }, new[] { "1" }, new[] { -1 })]
        [TestCase(1, new[] { 1 }, new[] { 1 }, new[] { 1.0 }, new[] { 1 }, new[] { "1" }, new[] { "1", "2" }, new[] { -1 })]
        [TestCase(1, new[] { 1 }, new[] { 1 }, new[] { 1.0 }, new[] { 1 }, new[] { "1" }, new[] { "1" }, new[] { -1, -1 })]
        public void WriteNetworkBranchesInitializedButArrayNotCorrectTest(int nBranches, int[] sourceNodeId, int[] targetNodeId, double[] branchLength, int[] nBranchGeoPoints, string[] branchId, string[] branchLongname, int [] branchOrderNumbers)
        {
            // check with :
            // Branch Of numbers; 
            // Length srcIds; 
            // Length targetIds; 
            // Length lengths; 
            // Length geoPts; 
            // Length idsArray; 
            // Length desrcArray; 

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();
            int branches;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).IgnoreArguments().OutRef(nBranches).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkBranches(sourceNodeId, targetNodeId, branchLength, nBranchGeoPoints, branchId, branchLongname, branchOrderNumbers)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR, uGridNetworkApi.WriteNetworkBranches(sourceNodeId, targetNodeId, branchLength, nBranchGeoPoints, branchId, branchLongname, branchOrderNumbers));

            // uRemoteGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR, uRemoteUGridNetworkApi.WriteNetworkBranches(sourceNodeId, targetNodeId, branchLength, nBranchGeoPoints, branchId, branchLongname, branchOrderNumbers));
        }

        [Test]
        public void WriteNetworkGeometryTest()
        {
            // arrange
            int nGeoPoints = 2;
            var geopointsX = new[] { 0.0, 1.0 };
            var geopointsY = new[] { 0.0, 1.0 };

            // uGridNetworkApi
            int geopoints;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).IgnoreArguments().OutRef(nGeoPoints).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr geopointsXptr = IntPtr.Zero;
            IntPtr geopointsYptr = IntPtr.Zero;

            wrapper.Expect(w => w.write_1d_network_branches_geometry(id, nwid, geopointsXptr,
                    geopointsYptr, nGeoPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, geopointsXptr, geopointsYptr, nGeoPoints)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.WriteNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.WriteNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, remoteResult);
        }

        [Test]
        public void WriteNetworkGeometryApiCallFailedTest()
        {
            // arrange
            int nGeoPoints = 2;
            var geopointsX = new[] { 0.0, 1.0 };
            var geopointsY = new[] { 0.0, 1.0 };

            // uGridNetworkApi
            int geopoints;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).IgnoreArguments().OutRef(nGeoPoints).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr geopointsXptr = IntPtr.Zero;
            IntPtr geopointsYptr = IntPtr.Zero;

            wrapper.Expect(w => w.write_1d_network_branches_geometry(id, nwid, geopointsXptr,
                    geopointsYptr, nGeoPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, geopointsXptr, geopointsYptr, nGeoPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.WriteNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.WriteNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void WriteNetworkGeometryExceptionTest()
        {
            // arrange
            int nGeoPoints = 2;
            var geopointsX = new[] { 0.0, 1.0 };
            var geopointsY = new[] { 0.0, 1.0 };

            // uGridNetworkApi
            int geopoints;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).IgnoreArguments().OutRef(nGeoPoints).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr geopointsXptr = IntPtr.Zero;
            IntPtr geopointsYptr = IntPtr.Zero;

            wrapper.Expect(w => w.write_1d_network_branches_geometry(id, nwid, geopointsXptr,
                    geopointsYptr, nGeoPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, geopointsXptr, geopointsYptr, nGeoPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.WriteNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.WriteNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void WriteNetworkGeometryInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkGeometry(new double[0], new double[0])).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.WriteNetworkGeometry(new double[0], new double[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.WriteNetworkGeometry(new double[0], new double[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(-1, new[] { 1.0 }, new[] { 1.0 })]
        [TestCase(1, new[] { 1.0, 2.0 }, new[] { 1.0 })]
        [TestCase(1, new[] { 1.0 }, new[] { 1.0, 2.0 })]
        public void WriteNetworkNetworkGeometryInitializedButArrayNotCorrectTest(int nGeopoints, double[] geopointsX, double[] geopointsY)
        {
            // check with :
            // number of geopoints; 
            // Length xArray; 
            // Length yArray; 

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();
            int geopoints;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).IgnoreArguments().OutRef(nGeopoints).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.WriteNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR, uGridNetworkApi.WriteNetworkGeometry(geopointsX, geopointsY));

            // uRemoteGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR, uRemoteUGridNetworkApi.WriteNetworkGeometry(geopointsX, geopointsY));
        }

        #endregion

        #region Read network 

        [Test]
        public void GetNumberOfNetworkNodesTest()
        {
            int nNodes = 9;

            // uGridNetworkApi
            int nodes;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumNodesVarName, nNodes);

            // uRemoteGridNetworkApi
            int rNodes;
            uRemoteUGridNetworkApi.Expect(a => a.GetNumberOfNetworkNodes(1, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, uGridNetworkApi.GetNumberOfNetworkNodes(1, out nodes));
            Assert.AreEqual(nNodes, nodes);
            // uRemoteGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, uRemoteUGridNetworkApi.GetNumberOfNetworkNodes(1, out rNodes));
            Assert.AreEqual(nNodes, rNodes);
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void GetNumberOfNetworkNodesInvalidInitialization(bool isInitialized, int nNodes)
        {
            // uGridNetworkApi
            int nodes;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridNetworkApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumNodesVarName, nNodes);

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.get_1d_network_nodes_count(id, nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(nNetworkNodes)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            int rNodes;
            uRemoteUGridNetworkApi.Expect(a => a.GetNumberOfNetworkNodes(1, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.GetNumberOfNetworkNodes(1, out nodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, result);
            Assert.AreEqual(nNetworkNodes, nodes);
            Assert.AreEqual(nNetworkNodes, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));

            // uRemoteGridNetworkApi
            TypeUtils.SetField(uGridNetworkApi, NumNodesVarName, nNodes);
            var remoteResult = uRemoteUGridNetworkApi.GetNumberOfNetworkNodes(1, out rNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, remoteResult);
            Assert.AreEqual(nNetworkNodes, rNodes);
            Assert.AreEqual(nNetworkNodes, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));

        }

        [Test]
        public void GetNumberOfNetworkNodesApiCallFailedTest()
        {
            // uGridNetworkApi
            int nodes;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.get_1d_network_nodes_count(id, nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            int rNodes;
            uRemoteUGridNetworkApi.Expect(a => a.GetNumberOfNetworkNodes(1, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.GetNumberOfNetworkNodes(1, out nodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.GetNumberOfNetworkNodes(1, out rNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));

        }

        [Test]
        public void GetNumberOfNetworkNodesExceptionTest()
        {
            // uGridNetworkApi
            int nodes;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.get_1d_network_nodes_count(id, nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            int rNodes;
            uRemoteUGridNetworkApi.Expect(a => a.GetNumberOfNetworkNodes(1, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.GetNumberOfNetworkNodes(1, out nodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.GetNumberOfNetworkNodes(1, out rNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumNodesVarName));
        }

        [Test]
        public void GetNumberOfNetworkBranchesTest()
        {
            int nBranches = 1;
            // uGridNetworkApi
            int branches;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumBranchesVarName, nBranches);

            // uRemoteGridNetworkApi
            int rbranches;
            uRemoteUGridNetworkApi.Expect(a => a.GetNumberOfNetworkBranches(1, out rbranches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, uGridNetworkApi.GetNumberOfNetworkBranches(1, out branches));
            Assert.AreEqual(nBranches, branches);

            // uRemoteGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, uRemoteUGridNetworkApi.GetNumberOfNetworkBranches(1, out rbranches));
            Assert.AreEqual(nBranches, rbranches);
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void GetNumberOfNetworkBranchesInvalidInitialization(bool isInitialized, int nBranches)
        {
            // uGridNetworkApi
            int branches;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridNetworkApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumBranchesVarName, nBranches);

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nwid = 0;
            int nNetworkBranches = 6;
            wrapper.Expect(w => w.get_1d_network_branches_count(id, nwid,
                        ref nNetworkBranches))
                .IgnoreArguments()
                .OutRef(nNetworkBranches)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            int rbranches;
            uRemoteUGridNetworkApi.Expect(a => a.GetNumberOfNetworkBranches(1, out rbranches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, uGridNetworkApi.GetNumberOfNetworkBranches(1, out branches));
            Assert.AreEqual(nNetworkBranches, branches);
            Assert.AreEqual(nNetworkBranches, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));

            // uRemoteGridNetworkApi
            TypeUtils.SetField(uGridNetworkApi, NumBranchesVarName, nBranches);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, uRemoteUGridNetworkApi.GetNumberOfNetworkBranches(1, out rbranches));
            Assert.AreEqual(nNetworkBranches, rbranches);
            Assert.AreEqual(nNetworkBranches, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
        }

        [Test]
        public void GetNumberOfNetworkBranchesApiCallFailedTest()
        {
            // uGrid
            int branches;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridNetworkApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nwid = 0;
            int nNetworkBranches = 6;
            wrapper
                .Expect(
                    w => w.get_1d_network_branches_count(id, nwid,
                        ref nNetworkBranches))
                .IgnoreArguments()
                .OutRef(id, nwid, nNetworkBranches)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            int rbranches;
            uRemoteUGridNetworkApi.Expect(a => a.GetNumberOfNetworkBranches(1, out rbranches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.GetNumberOfNetworkBranches(1, out branches);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(-1, branches);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.GetNumberOfNetworkBranches(1, out rbranches);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(-1, rbranches);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
        }

        [Test]
        public void GetNumberOfNetworkBranchesExceptionTest()
        {
            // uGrid
            int branches;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridNetworkApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nwid = 0;
            int nNetworkBranches = 6;
            wrapper
                .Expect(
                    w => w.get_1d_network_branches_count(id, nwid,
                        ref nNetworkBranches))
                .IgnoreArguments()
                .OutRef(id, nwid, nNetworkBranches)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            int rbranches;
            uRemoteUGridNetworkApi.Expect(a => a.GetNumberOfNetworkBranches(1, out rbranches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.GetNumberOfNetworkBranches(1, out branches);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, branches);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.GetNumberOfNetworkBranches(1, out rbranches);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(-1, rbranches);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumBranchesVarName));
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsTest()
        {
            int nGeometryPoints = 11;

            // uGridNetworkApi
            int geopoints;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumGeometryPointsVarName, nGeometryPoints);

            // uRemoteGridNetworkApi
            int rgeopoints;
            uRemoteUGridNetworkApi.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out rgeopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, uGridNetworkApi.GetNumberOfNetworkGeometryPoints(1, out geopoints));
            Assert.AreEqual(nGeometryPoints, geopoints);

            // uRemoteGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, uRemoteUGridNetworkApi.GetNumberOfNetworkGeometryPoints(1, out rgeopoints));
            Assert.AreEqual(nGeometryPoints, rgeopoints);
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void GetNumberOfNetworkGeometryPointsInvalidInitialization(bool isInitialized, int nGeoPoints)
        {
            // uGridNetworkApi
            int geopoints;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridNetworkApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumGeometryPointsVarName, nGeoPoints);

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nwid = 0;
            int nGeometryPoints = 4;
            wrapper
                .Expect(
                    w => w.get_1d_network_branches_geometry_coordinate_count(id, nwid,
                        ref nGeometryPoints))
                .IgnoreArguments()
                .OutRef(nGeometryPoints)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            int rgeopoints;
            uRemoteUGridNetworkApi.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out rgeopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, uGridNetworkApi.GetNumberOfNetworkGeometryPoints(1, out geopoints));
            Assert.AreEqual(nGeometryPoints, geopoints);
            Assert.AreEqual(nGeometryPoints, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));

            // uRemoteGridNetworkApi
            TypeUtils.SetField(uGridNetworkApi, NumGeometryPointsVarName, nGeoPoints);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, uGridNetworkApi.GetNumberOfNetworkGeometryPoints(1, out rgeopoints));
            Assert.AreEqual(nGeometryPoints, rgeopoints);
            Assert.AreEqual(nGeometryPoints, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsApiCallFailed()
        {
            // uGridNetworkApi
            int geopoints;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridNetworkApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nwid = 0;
            int nGeometryPoints = 4;
            wrapper
                .Expect(
                    w => w.get_1d_network_branches_geometry_coordinate_count(id, nwid,
                        ref nGeometryPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nGeometryPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            int rgeopoints;
            uRemoteUGridNetworkApi.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out rgeopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGridNetworkApi.GetNumberOfNetworkGeometryPoints(1, out geopoints));
            Assert.AreEqual(-1, geopoints);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));

            // uRemoteGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGridNetworkApi.GetNumberOfNetworkGeometryPoints(1, out rgeopoints));
            Assert.AreEqual(-1, rgeopoints);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsExceptionTest()
        {
            // uGridNetworkApi
            int geopoints;
            uGridNetworkApi.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridNetworkApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nwid = 0;
            int nGeometryPoints = 4;
            wrapper
                .Expect(
                    w => w.get_1d_network_branches_geometry_coordinate_count(id, nwid,
                        ref nGeometryPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nGeometryPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            int rgeopoints;
            uRemoteUGridNetworkApi.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out rgeopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uGridNetworkApi.GetNumberOfNetworkGeometryPoints(1, out geopoints));
            Assert.AreEqual(-1, geopoints);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));

            // uRemoteGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uGridNetworkApi.GetNumberOfNetworkGeometryPoints(1, out rgeopoints));
            Assert.AreEqual(-1, rgeopoints);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkApi, NumGeometryPointsVarName));
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void ReadNetworkNodesInvalidInitializationTest(bool isInitialized, int nNodes)
        {
            // arrange
            double[] nodesX;
            double[] nodesY;
            string[] nodesIds;
            string[] nodesLongnames;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumNodesVarName, nNodes);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uGridNetworkApi.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames));

            // uRemoteGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uRemoteUGridNetworkApi.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames));
        }

        [Test]
        public void ReadNetworkNodesTest()
        {
            // arrange
            double[] nodesX;
            double[] nodesY;
            string[] nodesIds;
            string[] nodesLongnames;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumNodesVarName, 4);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nNodes = 4;

            IntPtr nodesXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr nodesYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);

            GridWrapper.interop_charinfo[] nodesinfo = new GridWrapper.interop_charinfo[nNodes];
            wrapper.Expect(w => w.read_1d_network_nodes(id, nwid, ref nodesXPtr, ref nodesYPtr, nodesinfo,
                    nNodes))
                .OutRef(nodesXPtr, nodesYPtr)
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Any();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, remoteResult);
        }

        [Test]
        public void ReadNetworkNodesApiCallFailedTest()
        {
            // arrange
            double[] nodesX;
            double[] nodesY;
            string[] nodesIds;
            string[] nodesLongnames;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumNodesVarName, 1);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nNodes = 0;

            IntPtr nodesXPtr = IntPtr.Zero;
            IntPtr nodesYPtr = IntPtr.Zero;

            GridWrapper.interop_charinfo[] nodesinfo = new GridWrapper.interop_charinfo[nNodes];
            wrapper.Expect(w => w.read_1d_network_nodes(id, nwid, ref nodesXPtr, ref nodesYPtr, nodesinfo,
                    nNodes))
                .OutRef(nodesXPtr, nodesYPtr)
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Any();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void ReadNetworkNodesExceptionTest()
        {
            // arrange
            double[] nodesX;
            double[] nodesY;
            string[] nodesIds;
            string[] nodesLongnames;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumNodesVarName, 1);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nNodes = 0;

            IntPtr nodesXPtr = IntPtr.Zero;
            IntPtr nodesYPtr = IntPtr.Zero;

            GridWrapper.interop_charinfo[] nodesinfo = new GridWrapper.interop_charinfo[nNodes];
            wrapper.Expect(w => w.read_1d_network_nodes(id, nwid, ref nodesXPtr, ref nodesYPtr, nodesinfo,
                    nNodes))
                .OutRef(id, nwid, nodesXPtr, nodesYPtr, nodesinfo, nNodes)
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Any();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void ReadNetworkBranchesInvalidInitializationTest(bool isInitialized, int nBranches)
        {
            // arrange
            int[] sourceNodes;
            int[] targetNodes;
            double[] branchLengths;
            int[] branchGeoPoints;
            string[] branchIds;
            string[] branchLongnames;
            int[] branchOrderNumbers;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumBranchesVarName, nBranches);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
        }


        [Test]
        public void ReadNetworkBranchesLocalTest()
        {
            // arrange
            int[] sourceNodes;
            int[] targetNodes;
            double[] branchLengths;
            int[] branchGeoPoints;
            string[] branchIds;
            string[] branchLongnames;
            int[] branchOrderNumbers;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGridNetworkApi, NumBranchesVarName, nBranches);

            IntPtr sourceNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr targetNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr branchLengthPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nBranches);
            IntPtr branchGeoPointsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);

            GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[nBranches];
            wrapper.Expect(w => w.read_1d_network_branches(id, nwid, ref sourceNodePtr,
                    ref targetNodePtr, ref branchLengthPtr, branchinfo, ref branchGeoPointsPtr, nBranches)).IgnoreArguments()
                .OutRef(sourceNodePtr, targetNodePtr, branchLengthPtr, branchGeoPointsPtr)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Any();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi
                .Expect(a => a.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, result);
            Assert.AreEqual(nBranches, sourceNodes.Length);
            Assert.AreEqual(nBranches, targetNodes.Length);
            Assert.AreEqual(nBranches, branchLengths.Length);
            Assert.AreEqual(nBranches, branchGeoPoints.Length);
            Assert.AreEqual(nBranches, branchIds.Length);
            Assert.AreEqual(nBranches, branchLongnames.Length);
        }

        [Test]
        public void ReadNetworkBranchesRemoteTest()
        {
            // arrange
            int[] sourceNodes;
            int[] targetNodes;
            double[] branchLengths;
            int[] branchGeoPoints;
            string[] branchIds;
            string[] branchLongnames;
            int[] branchOrderNumbers;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGridNetworkApi, NumBranchesVarName, nBranches);

            IntPtr sourceNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr targetNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr branchLengthPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nBranches);
            IntPtr branchGeoPointsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);

            GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[nBranches];
            wrapper.Expect(w => w.read_1d_network_branches(id, nwid, ref sourceNodePtr,
                    ref targetNodePtr, ref branchLengthPtr, branchinfo, ref branchGeoPointsPtr, nBranches)).IgnoreArguments()
                .OutRef(sourceNodePtr, targetNodePtr, branchLengthPtr, branchGeoPointsPtr)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Once();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi
                .Expect(a => a.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, remoteResult);
            Assert.AreEqual(nBranches, sourceNodes.Length);
            Assert.AreEqual(nBranches, targetNodes.Length);
            Assert.AreEqual(nBranches, branchLengths.Length);
            Assert.AreEqual(nBranches, branchGeoPoints.Length);
            Assert.AreEqual(nBranches, branchIds.Length);
            Assert.AreEqual(nBranches, branchLongnames.Length);
        }

        [Test]
        public void ReadNetworkBranchesApiCallFailedTest()
        {
            // arrange
            int[] sourceNodes;
            int[] targetNodes;
            double[] branchLengths;
            int[] branchGeoPoints;
            string[] branchIds;
            string[] branchLongnames;
            int[] branchOrderNumbers;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGridNetworkApi, NumBranchesVarName, nBranches);

            IntPtr sourceNodePtr = IntPtr.Zero;
            IntPtr targetNodePtr = IntPtr.Zero;
            IntPtr branchLengthPtr = IntPtr.Zero;
            IntPtr branchGeoPointsPtr = IntPtr.Zero;

            GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[nBranches];
            wrapper.Expect(w => w.read_1d_network_branches(id, nwid, ref sourceNodePtr,
                    ref targetNodePtr, ref branchLengthPtr, branchinfo, ref branchGeoPointsPtr, nBranches)).IgnoreArguments()
                .OutRef(sourceNodePtr, targetNodePtr, branchLengthPtr, branchGeoPointsPtr)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Any();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi
                .Expect(a => a.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(0, sourceNodes.Length);
            Assert.AreEqual(0, targetNodes.Length);
            Assert.AreEqual(0, branchLengths.Length);
            Assert.AreEqual(0, branchGeoPoints.Length);
            Assert.AreEqual(0, branchIds.Length);
            Assert.AreEqual(0, branchLongnames.Length);

            // uGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(0, sourceNodes.Length);
            Assert.AreEqual(0, targetNodes.Length);
            Assert.AreEqual(0, branchLengths.Length);
            Assert.AreEqual(0, branchGeoPoints.Length);
            Assert.AreEqual(0, branchIds.Length);
            Assert.AreEqual(0, branchLongnames.Length);
        }

        [Test]
        public void ReadNetworkBranchesExceptionTest()
        {
            // arrange
            int[] sourceNodes;
            int[] targetNodes;
            double[] branchLengths;
            int[] branchGeoPoints;
            string[] branchIds;
            string[] branchLongnames;
            int[] branchOrderNumbers;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGridNetworkApi, NumBranchesVarName, nBranches);

            IntPtr sourceNodePtr = IntPtr.Zero;
            IntPtr targetNodePtr = IntPtr.Zero;
            IntPtr branchLengthPtr = IntPtr.Zero;
            IntPtr branchGeoPointsPtr = IntPtr.Zero;

            GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[nBranches];
            wrapper.Expect(w => w.read_1d_network_branches(id, nwid, ref sourceNodePtr,
                    ref targetNodePtr, ref branchLengthPtr, branchinfo, ref branchGeoPointsPtr, nBranches)).IgnoreArguments()
                .OutRef(id, nwid, sourceNodePtr, targetNodePtr, branchLengthPtr, branchinfo, branchGeoPointsPtr, nBranches)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Any();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi
                .Expect(a => a.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);

            // uGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames, out branchOrderNumbers);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(0, sourceNodes.Length);
            Assert.AreEqual(0, targetNodes.Length);
            Assert.AreEqual(0, branchLengths.Length);
            Assert.AreEqual(0, branchGeoPoints.Length);
            Assert.AreEqual(0, branchIds.Length);
            Assert.AreEqual(0, branchLongnames.Length);
        }

        [Test]
        [TestCase(false, true, 1)]
        [TestCase(true, false, 1)]
        [TestCase(true, true, -1)]
        public void ReadNetworkGeometryInvalidInitializationTest(bool isInitialized, bool isReady, int nNodes)
        {
            double[] geopointsX;
            double[] geopointsY;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            TypeUtils.SetField(uGridNetworkApi, NumNodesVarName, nNodes);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.ReadNetworkGeometry(1, out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uGridNetworkApi.ReadNetworkGeometry(1, out geopointsX, out geopointsY));
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);

            // uRemoteNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uRemoteUGridNetworkApi.ReadNetworkGeometry(1, out geopointsX, out geopointsY));
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);
        }

        [Test]
        public void ReadNetworkGeometryTest()
        {
            double[] geopointsX;
            double[] geopointsY;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumGeometryPointsVarName, 4);
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nGeoPoints = 4;

            IntPtr geopointsXptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nGeoPoints);
            IntPtr geopointsYptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nGeoPoints);

            wrapper.Expect(w => w.read_1d_network_branches_geometry(id, nwid, ref geopointsXptr,
                    ref geopointsYptr, nGeoPoints)).IgnoreArguments()
                .OutRef(geopointsXptr, geopointsYptr)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.ReadNetworkGeometry(1, out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.ReadNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, result);
            Assert.AreEqual(nGeoPoints, geopointsX.Length);
            Assert.AreEqual(nGeoPoints, geopointsY.Length);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.ReadNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, remoteResult);
            Assert.AreEqual(nGeoPoints, geopointsX.Length);
            Assert.AreEqual(nGeoPoints, geopointsY.Length);
        }

        [Test]
        public void ReadNetworkGeometryApiCallFailedTest()
        {
            double[] geopointsX;
            double[] geopointsY;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumGeometryPointsVarName, 1);
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nGeoPoints = 0;

            IntPtr geopointsXptr = IntPtr.Zero;
            IntPtr geopointsYptr = IntPtr.Zero;

            wrapper.Expect(w => w.read_1d_network_branches_geometry(id, nwid, ref geopointsXptr,
                    ref geopointsYptr, nGeoPoints)).IgnoreArguments()
                .OutRef(geopointsXptr, geopointsYptr)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.ReadNetworkGeometry(1,out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.ReadNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.ReadNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);
        }

        [Test]
        public void ReadNetworkGeometryExceptionTest()
        {
            double[] geopointsX;
            double[] geopointsY;

            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkApi, NumGeometryPointsVarName, 1);
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nGeoPoints = 0;

            IntPtr geopointsXptr = IntPtr.Zero;
            IntPtr geopointsYptr = IntPtr.Zero;

            wrapper.Expect(w => w.read_1d_network_branches_geometry(id, nwid, ref geopointsXptr,
                    ref geopointsYptr, nGeoPoints)).IgnoreArguments()
                .OutRef(id, nwid, geopointsXptr, geopointsYptr, nGeoPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.ReadNetworkGeometry(1, out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkApi.ReadNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkApi.ReadNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);
        }
        #endregion

        #region Network initialization

        [Test]
        public void NetworkReadyTrueTest()
        {
            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.NetworkReadyForWriting).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGridNetworkApi, NetworkIdForWritingVarName, 1);

            // uGridNetworkApi
            Assert.AreEqual(true, uGridNetworkApi.NetworkReadyForWriting);

            // uRemoteNetworkApi
            Assert.AreEqual(true, uRemoteUGridNetworkApi.NetworkReadyForWriting);
        }

        [Test]
        public void NetworkReadyFalseBecauseNetworkIdNotSetTest()
        {
            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.NetworkReadyForWriting).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.NetworkReadyForWriting).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGridNetworkApi, NetworkIdForWritingVarName, -1);

            // uGridNetworkApi
            Assert.AreEqual(false, uGridNetworkApi.NetworkReadyForWriting);

            // uRemoteNetworkApi
            Assert.AreEqual(false, uRemoteUGridNetworkApi.NetworkReadyForWriting);
        }

        [Test]
        public void NetworkInitializedTrueTest()
        {
            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGridNetworkApi, "ioncId", 1);

            // uGridNetworkApi
            Assert.AreEqual(true, uGridNetworkApi.Initialized);

            // uRemoteNetworkApi
            Assert.AreEqual(true, uRemoteUGridNetworkApi.Initialized);
        }

        [Test]
        public void NetworkInitializedFalseBecauseIoncidNotSetTest()
        {
            // uGridNetworkApi
            uGridNetworkApi.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteNetworkApi
            uRemoteUGridNetworkApi.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGridNetworkApi, "ioncId", -1);

            // uGridNetworkApi
            Assert.AreEqual(false, uGridNetworkApi.Initialized);

            // uRemoteNetworkApi
            Assert.AreEqual(false, uRemoteUGridNetworkApi.Initialized);
        }

        #endregion

        #region Write network discretisation
        
        [Test]
        public void CreateNetworkUninitialized()
        {
            //uRemoteGridNetworkApi
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.CreateNetworkDiscretisation("", 0, 0, 0)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();
            
            // uGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uGridNetworkDiscretisationApi.CreateNetworkDiscretisation("", 0, 0, 0));
            
            // uRemoteGridNetworkApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, uRemoteUGridNetworkDiscretisationApi.CreateNetworkDiscretisation("", 0, 0, 0));
        }
        
        [Test]
        public void CreateNetworkDiscretisationTest()
        {
            int nmeshedges = 12;
            int nmeshpts = 31;
            int networkId = 1;
            int id = 1;
            int meshId = 0;
            string meshName = "myMesh";
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            // uGridNetworkApi
            uGridNetworkDiscretisationApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkDiscretisationApi, WrapperFieldName, wrapper);
            TypeUtils.SetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName, -1);

            wrapper.Expect(w => w.create_1d_mesh(id, networkId, ref meshId, meshName, nmeshpts, nmeshedges))
                .OutRef(id, networkId, meshId, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.CreateNetworkDiscretisation(meshName, nmeshpts, nmeshedges, networkId)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkDiscretisationApi.CreateNetworkDiscretisation(meshName, nmeshpts, nmeshedges, networkId);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, result);
            Assert.AreEqual(nmeshpts, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkDiscretisationApi.CreateNetworkDiscretisation(meshName, nmeshpts, nmeshedges, networkId);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, remoteResult);
            Assert.AreEqual(nmeshpts, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));
        }
        
        [Test]
        public void CreateNetworkDiscretisationApiCallFailedTest()
        {
            int nmeshedges = 12;
            int nmeshpts = 31;
            int networkId = 1;
            int id = 1;
            int meshId = 0;
            string meshName = "myMesh";
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            // uGridNetworkApi
            uGridNetworkDiscretisationApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            // wrapper
            wrapper.Expect(w => w.create_1d_mesh(id, networkId, ref meshId, meshName, nmeshpts, nmeshedges))
                .OutRef(id, networkId, meshId, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkDiscretisationApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.CreateNetworkDiscretisation(meshName, nmeshpts, nmeshedges, networkId)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();
            
            // uGridNetworkApi
            var result = uGridNetworkDiscretisationApi.CreateNetworkDiscretisation(meshName, nmeshpts, nmeshedges, networkId);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkDiscretisationApi.CreateNetworkDiscretisation(meshName, nmeshpts, nmeshedges, networkId);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));
        }

        [Test]
        public void CreateNetworkDiscretisationExceptionTest()
        {
            int nmeshedges = 12;
            int nmeshpts = 31;
            int networkId = 1;
            int id = 1;
            int meshId = 0;
            string meshName = "myMesh";
            
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            // uGridNetworkApi
            uGridNetworkDiscretisationApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();

            wrapper.Expect(w => w.create_1d_mesh(id, networkId, ref meshId, meshName, nmeshpts, nmeshedges))
                .OutRef(id, networkId, meshId, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();
            TypeUtils.SetField(uGridNetworkDiscretisationApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.CreateNetworkDiscretisation(meshName, nmeshpts, nmeshedges, networkId)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkDiscretisationApi.CreateNetworkDiscretisation(meshName, nmeshpts, nmeshedges, networkId);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkDiscretisationApi.CreateNetworkDiscretisation(meshName, nmeshpts, nmeshedges, networkId);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void WriteNetworkDiscretisationPointsInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            int[] branchIdx = new int[1];
            double[] offset = new double[1];

            // uGridNetworkApi
            uGridNetworkDiscretisationApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridNetworkDiscretisationApi.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            // uRemoteGridApNetwork
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.WriteNetworkDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkDiscretisationApi.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);

            // uRemoteNetworkApi
            var remoteResult = uRemoteUGridNetworkDiscretisationApi.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(-1, new[] { 1, 2 }, new[] { 1.0, 2.0 })]
        [TestCase(1, new[] { 1, }, new[] { 1.0, 2.0 })]
        [TestCase(1, new[] { 1, 2 }, new[] { 1.0 })]
        public void WriteNetworkDiscretisationInitializedButArrayNotCorrectTest(int nDiscPoints, int[] branchIdx, double[] offset)
        {
            // check with :
            // number of Discretisation points; 
            // Length branchIds; 
            // Length offsets; 

            // uGridNetworkApi
            uGridNetworkDiscretisationApi.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGridNetworkDiscretisationApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nNetworkPoints = nDiscPoints;

            TypeUtils.SetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName, nNetworkPoints);
            TypeUtils.SetField(uGridNetworkDiscretisationApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.WriteNetworkDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            var result = uGridNetworkDiscretisationApi.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR, result);

            // uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkDiscretisationApi.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR, remoteResult);
        }

        [Test]
        public void WriteNetworkDiscretisationTest()
        {
            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };
            int nNetworkPoints = 2;

            // uGridNetworkApi
            uGridNetworkDiscretisationApi.Expect(a => a.Initialized).Return(true).Repeat.Times(2);
            uGridNetworkDiscretisationApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Times(2);
            var wrapper = mocks.StrictMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;

            wrapper.Expect(w => w.write_1d_mesh_discretisation_points(id, nwid, branchIdxPtr,
                    offsetPtr, nNetworkPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, branchIdxPtr, offsetPtr, nNetworkPoints)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName, nNetworkPoints);
            TypeUtils.SetField(uGridNetworkDiscretisationApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.WriteNetworkDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(nNetworkPoints, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));
            var result = uGridNetworkDiscretisationApi.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, result);

            // uRemoteGridNetworkApi
            TypeUtils.SetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName, nNetworkPoints);
            var remoteResult = uRemoteUGridNetworkDiscretisationApi.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, remoteResult);
        }

        [Test]
        public void WriteNetworkMeshDiscretisationApiCallFailedTest()
        {
            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };
            int nNetworkPoints = 2;

            // uGridNetworkApi
            uGridNetworkDiscretisationApi.Expect(a => a.Initialized).Return(true).Repeat.Times(2);
            uGridNetworkDiscretisationApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Times(2);
            var wrapper = mocks.StrictMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;

            wrapper.Expect(w => w.write_1d_mesh_discretisation_points(id, nwid, branchIdxPtr,
                    offsetPtr, nNetworkPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, branchIdxPtr, offsetPtr, nNetworkPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();


            TypeUtils.SetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName, branchIdx.Length);
            TypeUtils.SetField(uGridNetworkDiscretisationApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.WriteNetworkDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            int nPoints;
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.GetNumberOfNetworkDiscretisationPoints(1, out nPoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(branchIdx.Length, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));
            var result = uGridNetworkDiscretisationApi.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridNetworkApi
            TypeUtils.SetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName, branchIdx.Length);
            var remoteResult = uRemoteUGridNetworkDiscretisationApi.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void WriteNetworkDiscretisationExceptionTest()
        {
            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };
            int nNetworkPoints = 2;

            // uGridNetworkApi
            uGridNetworkDiscretisationApi.Expect(a => a.Initialized).Return(true).Repeat.Times(2);
            uGridNetworkDiscretisationApi.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Times(2);
            var wrapper = mocks.StrictMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;

            wrapper.Expect(w => w.write_1d_mesh_discretisation_points(id, nwid, branchIdxPtr,
                    offsetPtr, nNetworkPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, branchIdxPtr, offsetPtr, nNetworkPoints)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName, branchIdx.Length);
            TypeUtils.SetField(uGridNetworkDiscretisationApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.WriteNetworkDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridNetworkApi
            Assert.AreEqual(branchIdx.Length, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));
            var result = uGridNetworkDiscretisationApi.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);

            // uRemoteGridNetworkApi
            TypeUtils.SetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName, branchIdx.Length);
            var remoteResult = uRemoteUGridNetworkDiscretisationApi.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
        }

        #endregion

        #region Read network discretisation

        [Test]
        public void GetNumberOfMeshDiscretisationPointsTest()
        {
            int nNetworkPoints = 5;

            // uGrdiNetworkApi
            uGridNetworkDiscretisationApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName, nNetworkPoints);

            //uRemoteGridNetworkApi
            int nPoints;
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.GetNumberOfNetworkDiscretisationPoints(1, out nPoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGridNetworkApi
            var ierr = uGridNetworkDiscretisationApi.GetNumberOfNetworkDiscretisationPoints(1, out nPoints);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
            Assert.AreEqual(nNetworkPoints, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));

            //uRemoteGridNetworkApi
            ierr = uRemoteUGridNetworkDiscretisationApi.GetNumberOfNetworkDiscretisationPoints(1, out nPoints);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
            Assert.AreEqual(nNetworkPoints, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void GetNumberOfMeshDiscretisationPointsApiCallTest(bool isInitialized, int nNetworkPoints)
        {
            //uGridNetworkApi
            uGridNetworkDiscretisationApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName, nNetworkPoints);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncId = 1;
            int networkId = 1;
            int numberOfMeshPoints = 30;
            wrapper.Expect(
                    w => w.get_1d_mesh_discretisation_points_count(ioncId, networkId, ref numberOfMeshPoints)).IgnoreArguments()
                .OutRef(numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkDiscretisationApi, WrapperFieldName, wrapper);

            //uRemoteGridNetworkApi
            int nPoints;
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.GetNumberOfNetworkDiscretisationPoints(1, out nPoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGridNetworkApi
            var ierr = uGridNetworkDiscretisationApi.GetNumberOfNetworkDiscretisationPoints(1, out nPoints);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
            Assert.AreEqual(numberOfMeshPoints, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));

            //uRemoteGridNetworkApi
            TypeUtils.SetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName, nNetworkPoints);
            ierr = uRemoteUGridNetworkDiscretisationApi.GetNumberOfNetworkDiscretisationPoints(1, out nPoints);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
            Assert.AreEqual(numberOfMeshPoints, TypeUtils.GetField(uGridNetworkDiscretisationApi, NumNetworkPointsVarName));
        }
        
        [Test]
        public void GetNumberOfMeshDiscretisationPointsApiCallFailedTest()
        {
            //uGridNetworkApi
            uGridNetworkDiscretisationApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncId = 1;
            int networkId = 1;
            int numberOfMeshPoints = -1;
            wrapper.Expect(
                    w => w.get_1d_mesh_discretisation_points_count(ioncId, networkId, ref numberOfMeshPoints))
                .OutRef(ioncId, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkDiscretisationApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            int nPoints;
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.GetNumberOfNetworkDiscretisationPoints(1, out nPoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGridNetworkApi
            var result = uGridNetworkDiscretisationApi.GetNumberOfNetworkDiscretisationPoints(1, out nPoints);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            //uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkDiscretisationApi.GetNumberOfNetworkDiscretisationPoints(1, out nPoints);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void GetNumberOfMeshDiscretisationPointsExceptionTest()
        {
            //uGridNetworkApi
            uGridNetworkDiscretisationApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncId = 1;
            int networkId = 1;
            int numberOfMeshPoints = -1;
            wrapper.Expect(
                    w => w.get_1d_mesh_discretisation_points_count(ioncId, networkId, ref numberOfMeshPoints))
                .OutRef(ioncId, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridNetworkDiscretisationApi, WrapperFieldName, wrapper);

            // uRemoteGridNetworkApi
            int nPoints;
            uRemoteUGridNetworkDiscretisationApi.Expect(a => a.GetNumberOfNetworkDiscretisationPoints(1, out nPoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGridNetworkApi
            var result = uGridNetworkDiscretisationApi.GetNumberOfNetworkDiscretisationPoints(1, out nPoints);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, result);

            //uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridNetworkDiscretisationApi.GetNumberOfNetworkDiscretisationPoints(1, out nPoints);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteResult);
        }

        #endregion
    }
}
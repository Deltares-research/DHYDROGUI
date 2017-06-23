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
    public class UGridApiNetworkTests
    {
        private UGridApiNetwork uGridApiNetwork;
        private UGridApiNetworkDiscretisation uGridApiNetworkDiscretisation;
        private RemoteUGridApiNetwork uRemoteUGridApiNetwork;
        private RemoteUGridApiNetworkDiscretisation uRemoteUGridApiNetworkDiscretisation;
        private MockRepository mocks;
        
        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            uGridApiNetwork = mocks.DynamicMock<UGridApiNetwork>();
            uGridApiNetworkDiscretisation = mocks.DynamicMock<UGridApiNetworkDiscretisation>();
            uRemoteUGridApiNetwork = mocks.DynamicMock<RemoteUGridApiNetwork>();
            uRemoteUGridApiNetworkDiscretisation = mocks.DynamicMock<RemoteUGridApiNetworkDiscretisation>();
            TypeUtils.SetField(uRemoteUGridApiNetwork, "api", uGridApiNetwork);
            TypeUtils.SetField(uRemoteUGridApiNetworkDiscretisation, "api", uGridApiNetworkDiscretisation);
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void UGridApiNetworkTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "networkIdForWriting"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));
        }
        
        [Test]
        public void UGridApiNetworkDiscretisationTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetworkDiscretisation, "meshIdForWriting"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nMeshEdges"));
        }

        [Test]
        public void RemoteUGridApiNetworkTest()
        {
            mocks.ReplayAll();
            var api = TypeUtils.GetField(uRemoteUGridApiNetwork, "api");
            var ugridApiNetwork = api as IUGridApiNetwork;
            Assert.That(api != null);
            Assert.That(ugridApiNetwork != null);

            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "networkIdForWriting"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));
        }
        
        [Test]
        public void RemoteUGridApiNetworkDiscretisationTest()
        {
            mocks.ReplayAll();
            var api = TypeUtils.GetField(uRemoteUGridApiNetworkDiscretisation, "api");
            var UGridApiNetworkDiscretisation = api as IUGridApiNetworkDiscretisation;
            Assert.That(api != null);
            Assert.That(UGridApiNetworkDiscretisation != null);
            
            Assert.AreEqual(-1, TypeUtils.GetField(UGridApiNetworkDiscretisation, "meshIdForWriting"));
            Assert.AreEqual(-1, TypeUtils.GetField(UGridApiNetworkDiscretisation, "nNetworkPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(UGridApiNetworkDiscretisation, "nMeshEdges"));
        }

        #region Write Network

        [Test]
        public void CreateNetworkInvalidInitializationTest()
        {
            int networkId;

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.CreateNetwork("", 0, 0, 0, out networkId)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApiNetwork.CreateNetwork("", 0, 0, 0, out networkId));

            // uRemoteGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteUGridApiNetwork.CreateNetwork("", 0, 0, 0, out networkId));
        }

        [Test]
        public void CreateNetworkTest()
        {
            // arrange
            string name = "Network name";
            int nNodes = 0;
            int nBranches = 0;
            int nGeoPoints = 0;

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nnodes = 1;
            int nbranches = 2;
            int ngeoPoints = 3;

            wrapper
                .Expect(w => w.ionc_create_1d_network(ref id, ref nwid, "", ref nnodes, ref nbranches, ref ngeoPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nnodes, nbranches, ngeoPoints)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation)
                .Repeat.Once();

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApiNetwork.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(nnodes, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(nbranches, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(ngeoPoints, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));

            // uRemoteGridApiNetwork
            TypeUtils.SetField(uGridApiNetwork, "nNodes", -1);
            TypeUtils.SetField(uGridApiNetwork, "nBranches", -1);
            TypeUtils.SetField(uGridApiNetwork, "nGeometryPoints", -1);

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uRemoteUGridApiNetwork.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(nnodes, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(nbranches, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(ngeoPoints, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));
        }

        [Test]
        public void CreateNetworkApiCallFailedTest()
        {
            // arrange
            string name = "Network name";
            int nNodes = 0;
            int nBranches = 0;
            int nGeoPoints = 0;

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nnodes = 1;
            int nbranches = 2;
            int ngeoPoints = 3;
            wrapper
                .Expect(w => w.ionc_create_1d_network(ref id, ref nwid, "", ref nnodes, ref nbranches, ref ngeoPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nnodes, nbranches, ngeoPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();


            // uGridApiNetwork
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGridApiNetwork.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));

            // uRemoteGridApiNetwork
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uRemoteUGridApiNetwork.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));

        }

        [Test]
        public void CreateNetworkExceptionTest()
        {
            // arrange
            string name = "Network name";
            int nNodes = 0;
            int nBranches = 0;
            int nGeoPoints = 0;

            //uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nnodes = 1;
            int nbranches = 2;
            int ngeoPoints = 3;
            wrapper
                .Expect(w => w.ionc_create_1d_network(ref id, ref nwid, "", ref nnodes, ref nbranches, ref ngeoPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nnodes, nbranches, ngeoPoints)
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApiNetwork.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));

            // uRemoteGridApiNetwork
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteUGridApiNetwork.CreateNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void WriteNetworkNodesInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkNodes(new double[0], new double[0], new string[0], new string[0])).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.WriteNetworkNodes(new double[0], new double[0], new string[0], new string[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.WriteNetworkNodes(new double[0], new double[0], new string[0], new string[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
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

            // uGridApiNetwork
            int nodes;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).IgnoreArguments().OutRef(nNodes).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr nodesXPtr = IntPtr.Zero;
            IntPtr nodesYPtr = IntPtr.Zero;
            var nodesInfo = new GridWrapper.interop_charinfo[nNodes];

            wrapper.Expect(w => w.ionc_write_1d_network_nodes(ref id, ref nwid, ref nodesXPtr, ref nodesYPtr,
                    nodesInfo, ref nNodes))
                .IgnoreArguments()
                .OutRef(id, nwid, nodesXPtr, nodesYPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
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

            // uGridApiNetwork
            int nodes;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).IgnoreArguments().OutRef(nNodes).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr nodesXPtr = IntPtr.Zero;
            IntPtr nodesYPtr = IntPtr.Zero;
            var nodesInfo = new GridWrapper.interop_charinfo[nNodes];

            wrapper.Expect(w => w.ionc_write_1d_network_nodes(ref id, ref nwid, ref nodesXPtr, ref nodesYPtr,
                    nodesInfo, ref nNodes))
                .IgnoreArguments()
                .OutRef(id, nwid, nodesXPtr, nodesYPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR) // Return an arbitrary error
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork

            var result = uGridApiNetwork.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
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

            // uGridApiNetwork
            int nodes;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).IgnoreArguments().OutRef(nNodes).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr nodesXPtr = IntPtr.Zero;
            IntPtr nodesYPtr = IntPtr.Zero;
            var nodesInfo = new GridWrapper.interop_charinfo[nNodes];

            wrapper.Expect(w => w.ionc_write_1d_network_nodes(ref id, ref nwid, ref nodesXPtr, ref nodesYPtr,
                    nodesInfo, ref nNodes))
                .IgnoreArguments()
                .OutRef(id, nwid, nodesXPtr, nodesYPtr, nNodes)
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
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

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();
            int nodes;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).IgnoreArguments().OutRef(numberOfNetworkNodes).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGridApiNetwork.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames));

            // uRemoteGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uRemoteUGridApiNetwork.WriteNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames));
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void WriteNetworkBranchesInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkBranches(new int[0], new int[0], new double[0], new int[0],
                new string[0], new string[0]))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork    
            var result = uGridApiNetwork.WriteNetworkBranches(new int[0], new int[0], new double[0], new int[0],
                new string[0], new string[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApiNetwork    
            var remoteResult = uRemoteUGridApiNetwork.WriteNetworkBranches(new int[0], new int[0], new double[0], new int[0],
                new string[0], new string[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
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

            // uGridApiNetwork
            int branches;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).IgnoreArguments().OutRef(nBranches).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            mocks.ReplayAll();

            int id = 0;
            int nwid = 0;
            IntPtr sourceNodesPtr = IntPtr.Zero;
            IntPtr targetNodesPtr = IntPtr.Zero;
            IntPtr branchLengthPtr = IntPtr.Zero;
            IntPtr branchGeoPtr = IntPtr.Zero;
            var branchesInfo = new GridWrapper.interop_charinfo[nBranches];

            wrapper.Expect(w => w.ionc_write_1d_network_branches(ref id, ref nwid, ref sourceNodesPtr, ref targetNodesPtr, branchesInfo, ref branchLengthPtr, ref branchGeoPtr, ref nBranches))
                .IgnoreArguments()
                .OutRef(id, nwid, sourceNodesPtr, targetNodesPtr, branchLengthPtr, branchGeoPtr, nBranches)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork
                .Expect(a => a.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints,
                    branchId, branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
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

            // uGridApiNetwork
            int branches;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).IgnoreArguments().OutRef(nBranches).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            mocks.ReplayAll();

            int id = 0;
            int nwid = 0;
            IntPtr sourceNodesPtr = IntPtr.Zero;
            IntPtr targetNodesPtr = IntPtr.Zero;
            IntPtr branchLengthPtr = IntPtr.Zero;
            IntPtr branchGeoPtr = IntPtr.Zero;
            var branchesInfo = new GridWrapper.interop_charinfo[nBranches];

            wrapper.Expect(w => w.ionc_write_1d_network_branches(ref id, ref nwid, ref sourceNodesPtr, ref targetNodesPtr, branchesInfo, ref branchLengthPtr, ref branchGeoPtr, ref nBranches))
                .IgnoreArguments()
                .OutRef(id, nwid, sourceNodesPtr, targetNodesPtr, branchLengthPtr, branchGeoPtr, nBranches)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
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

            // uGridApiNetwork
            int branches;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).IgnoreArguments().OutRef(nBranches).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr sourceNodesPtr = IntPtr.Zero;
            IntPtr targetNodesPtr = IntPtr.Zero;
            IntPtr branchLengthPtr = IntPtr.Zero;
            IntPtr branchGeoPtr = IntPtr.Zero;
            var branchesInfo = new GridWrapper.interop_charinfo[nBranches];

            wrapper.Expect(w => w.ionc_write_1d_network_branches(ref id, ref nwid, ref sourceNodesPtr, ref targetNodesPtr, branchesInfo, ref branchLengthPtr, ref branchGeoPtr, ref nBranches))
                .IgnoreArguments()
                .OutRef(id, nwid, sourceNodesPtr, targetNodesPtr, branchLengthPtr, branchGeoPtr, nBranches)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.WriteNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(-1, new[] { 1 }, new[] { 1 }, new[] { 1.0 }, new[] { 1 }, new[] { "1" }, new[] { "1" })]
        [TestCase(1, new[] { 1, 2 }, new[] { 1 }, new[] { 1.0 }, new[] { 1 }, new[] { "1" }, new[] { "1" })]
        [TestCase(1, new[] { 1 }, new[] { 1, 2 }, new[] { 1.0 }, new[] { 1 }, new[] { "1" }, new[] { "1" })]
        [TestCase(1, new[] { 1 }, new[] { 1 }, new[] { 1.0, 2.0 }, new[] { 1 }, new[] { "1" }, new[] { "1" })]
        [TestCase(1, new[] { 1 }, new[] { 1 }, new[] { 1.0 }, new[] { 1, 2 }, new[] { "1" }, new[] { "1" })]
        [TestCase(1, new[] { 1 }, new[] { 1 }, new[] { 1.0 }, new[] { 1 }, new[] { "1", "2" }, new[] { "1" })]
        [TestCase(1, new[] { 1 }, new[] { 1 }, new[] { 1.0 }, new[] { 1 }, new[] { "1" }, new[] { "1", "2" })]
        public void WriteNetworkBranchesInitializedButArrayNotCorrectTest(int nBranches, int[] sourceNodeId, int[] targetNodeId, double[] branchLength, int[] nBranchGeoPoints, string[] branchId, string[] branchLongname)
        {
            // check with :
            // Branch Of numbers; 
            // Length srcIds; 
            // Length targetIds; 
            // Length lengths; 
            // Length geoPts; 
            // Length idsArray; 
            // Length desrcArray; 

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();
            int branches;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).IgnoreArguments().OutRef(nBranches).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkBranches(sourceNodeId, targetNodeId, branchLength, nBranchGeoPoints, branchId, branchLongname)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGridApiNetwork.WriteNetworkBranches(sourceNodeId, targetNodeId, branchLength, nBranchGeoPoints, branchId, branchLongname));

            // uRemoteGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uRemoteUGridApiNetwork.WriteNetworkBranches(sourceNodeId, targetNodeId, branchLength, nBranchGeoPoints, branchId, branchLongname));
        }

        [Test]
        public void WriteNetworkGeometryTest()
        {
            // arrange
            int nGeoPoints = 2;
            var geopointsX = new[] { 0.0, 1.0 };
            var geopointsY = new[] { 0.0, 1.0 };

            // uGridApiNetwork
            int geopoints;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).IgnoreArguments().OutRef(nGeoPoints).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr geopointsXptr = IntPtr.Zero;
            IntPtr geopointsYptr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_write_1d_network_branches_geometry(ref id, ref nwid, ref geopointsXptr,
                    ref geopointsYptr, ref nGeoPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, geopointsXptr, geopointsYptr, nGeoPoints)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.WriteNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.WriteNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
        }

        [Test]
        public void WriteNetworkGeometryApiCallFailedTest()
        {
            // arrange
            int nGeoPoints = 2;
            var geopointsX = new[] { 0.0, 1.0 };
            var geopointsY = new[] { 0.0, 1.0 };

            // uGridApiNetwork
            int geopoints;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).IgnoreArguments().OutRef(nGeoPoints).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr geopointsXptr = IntPtr.Zero;
            IntPtr geopointsYptr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_write_1d_network_branches_geometry(ref id, ref nwid, ref geopointsXptr,
                    ref geopointsYptr, ref nGeoPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, geopointsXptr, geopointsYptr, nGeoPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.WriteNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.WriteNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void WriteNetworkGeometryExceptionTest()
        {
            // arrange
            int nGeoPoints = 2;
            var geopointsX = new[] { 0.0, 1.0 };
            var geopointsY = new[] { 0.0, 1.0 };

            // uGridApiNetwork
            int geopoints;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).IgnoreArguments().OutRef(nGeoPoints).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            IntPtr geopointsXptr = IntPtr.Zero;
            IntPtr geopointsYptr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_write_1d_network_branches_geometry(ref id, ref nwid, ref geopointsXptr,
                    ref geopointsYptr, ref nGeoPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, geopointsXptr, geopointsYptr, nGeoPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.WriteNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.WriteNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void WriteNetworkGeometryInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkGeometry(new double[0], new double[0])).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.WriteNetworkGeometry(new double[0], new double[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.WriteNetworkGeometry(new double[0], new double[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
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

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();
            int geopoints;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).IgnoreArguments().OutRef(nGeopoints).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.WriteNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGridApiNetwork.WriteNetworkGeometry(geopointsX, geopointsY));

            // uRemoteGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uRemoteUGridApiNetwork.WriteNetworkGeometry(geopointsX, geopointsY));
        }

        #endregion

        #region Read network 

        [Test]
        public void GetNumberOfNetworkNodesTest()
        {
            int nNodes = 9;

            // uGridApiNetwork
            int nodes;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nNodes", nNodes);

            // uRemoteGridApiNetwork
            int rNodes;
            uRemoteUGridApiNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApiNetwork.GetNumberOfNetworkNodes(1, out nodes));
            Assert.AreEqual(nNodes, nodes);
            // uRemoteGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uRemoteUGridApiNetwork.GetNumberOfNetworkNodes(1, out rNodes));
            Assert.AreEqual(nNodes, rNodes);
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void GetNumberOfNetworkNodesInvalidInitialization(bool isInitialized, int nNodes)
        {
            // uGridApiNetwork
            int nodes;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApiNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nNodes", nNodes);

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.ionc_get_1d_network_nodes_count(ref id, ref nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            int rNodes;
            uRemoteUGridApiNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.GetNumberOfNetworkNodes(1, out nodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);
            Assert.AreEqual(nNetworkNodes, nodes);
            Assert.AreEqual(nNetworkNodes, TypeUtils.GetField(uGridApiNetwork, "nNodes"));

            // uRemoteGridApiNetwork
            TypeUtils.SetField(uGridApiNetwork, "nNodes", nNodes);
            var remoteResult = uRemoteUGridApiNetwork.GetNumberOfNetworkNodes(1, out rNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
            Assert.AreEqual(nNetworkNodes, rNodes);
            Assert.AreEqual(nNetworkNodes, TypeUtils.GetField(uGridApiNetwork, "nNodes"));

        }

        [Test]
        public void GetNumberOfNetworkNodesApiCallFailedTest()
        {
            // uGridApiNetwork
            int nodes;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.ionc_get_1d_network_nodes_count(ref id, ref nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            int rNodes;
            uRemoteUGridApiNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.GetNumberOfNetworkNodes(1, out nodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.GetNumberOfNetworkNodes(1, out rNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));

        }

        [Test]
        public void GetNumberOfNetworkNodesExceptionTest()
        {
            // uGridApiNetwork
            int nodes;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.ionc_get_1d_network_nodes_count(ref id, ref nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            int rNodes;
            uRemoteUGridApiNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.GetNumberOfNetworkNodes(1, out nodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.GetNumberOfNetworkNodes(1, out rNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nNodes"));
        }

        [Test]
        public void GetNumberOfNetworkBranchesTest()
        {
            int nBranches = 1;
            // uGridApiNetwork
            int branches;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nBranches", nBranches);

            // uRemoteGridApiNetwork
            int rbranches;
            uRemoteUGridApiNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out rbranches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApiNetwork.GetNumberOfNetworkBranches(1, out branches));
            Assert.AreEqual(nBranches, branches);

            // uRemoteGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uRemoteUGridApiNetwork.GetNumberOfNetworkBranches(1, out rbranches));
            Assert.AreEqual(nBranches, rbranches);
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void GetNumberOfNetworkBranchesInvalidInitialization(bool isInitialized, int nBranches)
        {
            // uGridApiNetwork
            int branches;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApiNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nBranches", nBranches);

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nwid = 0;
            int nNetworkBranches = 6;
            wrapper.Expect(w => w.ionc_get_1d_network_branches_count(ref id, ref nwid,
                        ref nNetworkBranches))
                .IgnoreArguments()
                .OutRef(id, nwid, nNetworkBranches)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            int rbranches;
            uRemoteUGridApiNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out rbranches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApiNetwork.GetNumberOfNetworkBranches(1, out branches));
            Assert.AreEqual(nNetworkBranches, branches);
            Assert.AreEqual(nNetworkBranches, TypeUtils.GetField(uGridApiNetwork, "nBranches"));

            // uRemoteGridApiNetwork
            TypeUtils.SetField(uGridApiNetwork, "nBranches", nBranches);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uRemoteUGridApiNetwork.GetNumberOfNetworkBranches(1, out rbranches));
            Assert.AreEqual(nNetworkBranches, rbranches);
            Assert.AreEqual(nNetworkBranches, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
        }

        [Test]
        public void GetNumberOfNetworkBranchesApiCallFailedTest()
        {
            // uGrid
            int branches;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApiNetwork.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nwid = 0;
            int nNetworkBranches = 6;
            wrapper
                .Expect(
                    w => w.ionc_get_1d_network_branches_count(ref id, ref nwid,
                        ref nNetworkBranches))
                .IgnoreArguments()
                .OutRef(id, nwid, nNetworkBranches)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            int rbranches;
            uRemoteUGridApiNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out rbranches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.GetNumberOfNetworkBranches(1, out branches);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(-1, branches);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.GetNumberOfNetworkBranches(1, out rbranches);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(-1, rbranches);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
        }

        [Test]
        public void GetNumberOfNetworkBranchesExceptionTest()
        {
            // uGrid
            int branches;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApiNetwork.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nwid = 0;
            int nNetworkBranches = 6;
            wrapper
                .Expect(
                    w => w.ionc_get_1d_network_branches_count(ref id, ref nwid,
                        ref nNetworkBranches))
                .IgnoreArguments()
                .OutRef(id, nwid, nNetworkBranches)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            int rbranches;
            uRemoteUGridApiNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out rbranches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.GetNumberOfNetworkBranches(1, out branches);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, branches);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.GetNumberOfNetworkBranches(1, out rbranches);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(-1, rbranches);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nBranches"));
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsTest()
        {
            int nGeometryPoints = 11;

            // uGridApiNetwork
            int geopoints;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nGeometryPoints", nGeometryPoints);

            // uRemoteGridApiNetwork
            int rgeopoints;
            uRemoteUGridApiNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out rgeopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApiNetwork.GetNumberOfNetworkGeometryPoints(1, out geopoints));
            Assert.AreEqual(nGeometryPoints, geopoints);

            // uRemoteGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uRemoteUGridApiNetwork.GetNumberOfNetworkGeometryPoints(1, out rgeopoints));
            Assert.AreEqual(nGeometryPoints, rgeopoints);
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void GetNumberOfNetworkGeometryPointsInvalidInitialization(bool isInitialized, int nGeoPoints)
        {
            // uGridApiNetwork
            int geopoints;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApiNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nGeometryPoints", nGeoPoints);

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nwid = 0;
            int nGeometryPoints = 4;
            wrapper
                .Expect(
                    w => w.ionc_get_1d_network_branches_geometry_coordinate_count(ref id, ref nwid,
                        ref nGeometryPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nGeometryPoints)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            int rgeopoints;
            uRemoteUGridApiNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out rgeopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApiNetwork.GetNumberOfNetworkGeometryPoints(1, out geopoints));
            Assert.AreEqual(nGeometryPoints, geopoints);
            Assert.AreEqual(nGeometryPoints, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));

            // uRemoteGridApiNetwork
            TypeUtils.SetField(uGridApiNetwork, "nGeometryPoints", nGeoPoints);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApiNetwork.GetNumberOfNetworkGeometryPoints(1, out rgeopoints));
            Assert.AreEqual(nGeometryPoints, rgeopoints);
            Assert.AreEqual(nGeometryPoints, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsApiCallFailed()
        {
            // uGridApiNetwork
            int geopoints;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApiNetwork.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nwid = 0;
            int nGeometryPoints = 4;
            wrapper
                .Expect(
                    w => w.ionc_get_1d_network_branches_geometry_coordinate_count(ref id, ref nwid,
                        ref nGeometryPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nGeometryPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            int rgeopoints;
            uRemoteUGridApiNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out rgeopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGridApiNetwork.GetNumberOfNetworkGeometryPoints(1, out geopoints));
            Assert.AreEqual(-1, geopoints);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));

            // uRemoteGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGridApiNetwork.GetNumberOfNetworkGeometryPoints(1, out rgeopoints));
            Assert.AreEqual(-1, rgeopoints);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsExceptionTest()
        {
            // uGridApiNetwork
            int geopoints;
            uGridApiNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApiNetwork.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 0;
            int nwid = 0;
            int nGeometryPoints = 4;
            wrapper
                .Expect(
                    w => w.ionc_get_1d_network_branches_geometry_coordinate_count(ref id, ref nwid,
                        ref nGeometryPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nGeometryPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            int rgeopoints;
            uRemoteUGridApiNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out rgeopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApiNetwork.GetNumberOfNetworkGeometryPoints(1, out geopoints));
            Assert.AreEqual(-1, geopoints);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));

            // uRemoteGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApiNetwork.GetNumberOfNetworkGeometryPoints(1, out rgeopoints));
            Assert.AreEqual(-1, rgeopoints);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetwork, "nGeometryPoints"));
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

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nNodes", nNodes);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApiNetwork.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames));

            // uRemoteGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteUGridApiNetwork.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames));
        }

        [Test]
        public void ReadNetworkNodesTest()
        {
            // arrange
            double[] nodesX;
            double[] nodesY;
            string[] nodesIds;
            string[] nodesLongnames;

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nNodes", 4);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nNodes = 4;

            IntPtr nodesXPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr nodesYPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);

            GridWrapper.interop_charinfo[] nodesinfo = new GridWrapper.interop_charinfo[nNodes];
            wrapper.Expect(w => w.ionc_read_1d_network_nodes(ref id, ref nwid, ref nodesXPtr, ref nodesYPtr, nodesinfo,
                    ref nNodes))
                .OutRef(id, nwid, nodesXPtr, nodesYPtr, nodesinfo, nNodes)
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Any();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
        }

        [Test]
        public void ReadNetworkNodesApiCallFailedTest()
        {
            // arrange
            double[] nodesX;
            double[] nodesY;
            string[] nodesIds;
            string[] nodesLongnames;

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nNodes", 1);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nNodes = 0;

            IntPtr nodesXPtr = IntPtr.Zero;
            IntPtr nodesYPtr = IntPtr.Zero;

            GridWrapper.interop_charinfo[] nodesinfo = new GridWrapper.interop_charinfo[nNodes];
            wrapper.Expect(w => w.ionc_read_1d_network_nodes(ref id, ref nwid, ref nodesXPtr, ref nodesYPtr, nodesinfo,
                    ref nNodes))
                .OutRef(id, nwid, nodesXPtr, nodesYPtr, nodesinfo, nNodes)
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Any();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
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

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nNodes", 1);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nNodes = 0;

            IntPtr nodesXPtr = IntPtr.Zero;
            IntPtr nodesYPtr = IntPtr.Zero;

            GridWrapper.interop_charinfo[] nodesinfo = new GridWrapper.interop_charinfo[nNodes];
            wrapper.Expect(w => w.ionc_read_1d_network_nodes(ref id, ref nwid, ref nodesXPtr, ref nodesYPtr, nodesinfo,
                    ref nNodes))
                .OutRef(id, nwid, nodesXPtr, nodesYPtr, nodesinfo, nNodes)
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Any();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.ReadNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
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

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nBranches", nBranches);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
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

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGridApiNetwork, "nBranches", nBranches);

            IntPtr sourceNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr targetNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr branchLengthPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nBranches);
            IntPtr branchGeoPointsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);

            GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[nBranches];
            wrapper.Expect(w => w.ionc_read_1d_network_branches(ref id, ref nwid, ref sourceNodePtr,
                    ref targetNodePtr, ref branchLengthPtr, branchinfo, ref branchGeoPointsPtr, ref nBranches)).IgnoreArguments()
                .OutRef(id, nwid, sourceNodePtr, targetNodePtr, branchLengthPtr, branchinfo, branchGeoPointsPtr, nBranches)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Any();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork
                .Expect(a => a.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);
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

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Once();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGridApiNetwork, "nBranches", nBranches);

            IntPtr sourceNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr targetNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr branchLengthPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nBranches);
            IntPtr branchGeoPointsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);

            GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[nBranches];
            wrapper.Expect(w => w.ionc_read_1d_network_branches(ref id, ref nwid, ref sourceNodePtr,
                    ref targetNodePtr, ref branchLengthPtr, branchinfo, ref branchGeoPointsPtr, ref nBranches)).IgnoreArguments()
                .OutRef(id, nwid, sourceNodePtr, targetNodePtr, branchLengthPtr, branchinfo, branchGeoPointsPtr, nBranches)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Once();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork
                .Expect(a => a.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
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

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGridApiNetwork, "nBranches", nBranches);

            IntPtr sourceNodePtr = IntPtr.Zero;
            IntPtr targetNodePtr = IntPtr.Zero;
            IntPtr branchLengthPtr = IntPtr.Zero;
            IntPtr branchGeoPointsPtr = IntPtr.Zero;

            GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[nBranches];
            wrapper.Expect(w => w.ionc_read_1d_network_branches(ref id, ref nwid, ref sourceNodePtr,
                    ref targetNodePtr, ref branchLengthPtr, branchinfo, ref branchGeoPointsPtr, ref nBranches)).IgnoreArguments()
                .OutRef(id, nwid, sourceNodePtr, targetNodePtr, branchLengthPtr, branchinfo, branchGeoPointsPtr, nBranches)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Any();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork
                .Expect(a => a.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(0, sourceNodes.Length);
            Assert.AreEqual(0, targetNodes.Length);
            Assert.AreEqual(0, branchLengths.Length);
            Assert.AreEqual(0, branchGeoPoints.Length);
            Assert.AreEqual(0, branchIds.Length);
            Assert.AreEqual(0, branchLongnames.Length);

            // uGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
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

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGridApiNetwork, "nBranches", nBranches);

            IntPtr sourceNodePtr = IntPtr.Zero;
            IntPtr targetNodePtr = IntPtr.Zero;
            IntPtr branchLengthPtr = IntPtr.Zero;
            IntPtr branchGeoPointsPtr = IntPtr.Zero;

            GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[nBranches];
            wrapper.Expect(w => w.ionc_read_1d_network_branches(ref id, ref nwid, ref sourceNodePtr,
                    ref targetNodePtr, ref branchLengthPtr, branchinfo, ref branchGeoPointsPtr, ref nBranches)).IgnoreArguments()
                .OutRef(id, nwid, sourceNodePtr, targetNodePtr, branchLengthPtr, branchinfo, branchGeoPointsPtr, nBranches)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Any();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork
                .Expect(a => a.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.ReadNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
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

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            TypeUtils.SetField(uGridApiNetwork, "nNodes", nNodes);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.ReadNetworkGeometry(1, out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApiNetwork.ReadNetworkGeometry(1, out geopointsX, out geopointsY));
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);

            // uRemoteApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteUGridApiNetwork.ReadNetworkGeometry(1, out geopointsX, out geopointsY));
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);
        }

        [Test]
        public void ReadNetworkGeometryTest()
        {
            double[] geopointsX;
            double[] geopointsY;

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nGeometryPoints", 1);
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nGeoPoints = 4;

            IntPtr geopointsXptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nGeoPoints);
            IntPtr geopointsYptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nGeoPoints);

            wrapper.Expect(w => w.ionc_read_1d_network_branches_geometry(ref id, ref nwid, ref geopointsXptr,
                    ref geopointsYptr, ref nGeoPoints)).IgnoreArguments()
                .OutRef(id, nwid, geopointsXptr, geopointsYptr, nGeoPoints)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.ReadNetworkGeometry(1, out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.ReadNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);
            Assert.AreEqual(nGeoPoints, geopointsX.Length);
            Assert.AreEqual(nGeoPoints, geopointsY.Length);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.ReadNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
            Assert.AreEqual(nGeoPoints, geopointsX.Length);
            Assert.AreEqual(nGeoPoints, geopointsY.Length);
        }

        [Test]
        public void ReadNetworkGeometryApiCallFailedTest()
        {
            double[] geopointsX;
            double[] geopointsY;

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nGeometryPoints", 1);
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nGeoPoints = 0;

            IntPtr geopointsXptr = IntPtr.Zero;
            IntPtr geopointsYptr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_read_1d_network_branches_geometry(ref id, ref nwid, ref geopointsXptr,
                    ref geopointsYptr, ref nGeoPoints)).IgnoreArguments()
                .OutRef(id, nwid, geopointsXptr, geopointsYptr, nGeoPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.ReadNetworkGeometry(1,out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.ReadNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.ReadNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);
        }

        [Test]
        public void ReadNetworkGeometryExceptionTest()
        {
            double[] geopointsX;
            double[] geopointsY;

            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetwork, "nGeometryPoints", 1);
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nGeoPoints = 0;

            IntPtr geopointsXptr = IntPtr.Zero;
            IntPtr geopointsYptr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_read_1d_network_branches_geometry(ref id, ref nwid, ref geopointsXptr,
                    ref geopointsYptr, ref nGeoPoints)).IgnoreArguments()
                .OutRef(id, nwid, geopointsXptr, geopointsYptr, nGeoPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetwork, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.ReadNetworkGeometry(1, out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetwork.ReadNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetwork.ReadNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);
        }
        #endregion

        #region Network initialization

        [Test]
        public void NetworkReadyTrueTest()
        {
            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.NetworkReadyForWriting).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGridApiNetwork, "networkIdForWriting", 1);

            // uGridApiNetwork
            Assert.AreEqual(true, uGridApiNetwork.NetworkReadyForWriting);

            // uRemoteApiNetwork
            Assert.AreEqual(true, uRemoteUGridApiNetwork.NetworkReadyForWriting);
        }

        [Test]
        public void NetworkReadyFalseBecauseNetworkIdNotSetTest()
        {
            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.NetworkReadyForWriting).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.NetworkReadyForWriting).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGridApiNetwork, "networkIdForWriting", -1);

            // uGridApiNetwork
            Assert.AreEqual(false, uGridApiNetwork.NetworkReadyForWriting);

            // uRemoteApiNetwork
            Assert.AreEqual(false, uRemoteUGridApiNetwork.NetworkReadyForWriting);
        }

        [Test]
        public void NetworkInitializedTrueTest()
        {
            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGridApiNetwork, "ioncid", 1);

            // uGridApiNetwork
            Assert.AreEqual(true, uGridApiNetwork.Initialized);

            // uRemoteApiNetwork
            Assert.AreEqual(true, uRemoteUGridApiNetwork.Initialized);
        }

        [Test]
        public void NetworkInitializedFalseBecauseIoncidNotSetTest()
        {
            // uGridApiNetwork
            uGridApiNetwork.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteApiNetwork
            uRemoteUGridApiNetwork.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGridApiNetwork, "ioncid", -1);

            // uGridApiNetwork
            Assert.AreEqual(false, uGridApiNetwork.Initialized);

            // uRemoteApiNetwork
            Assert.AreEqual(false, uRemoteUGridApiNetwork.Initialized);
        }

        #endregion

        #region Write network discretisation
        
        [Test]
        public void CreateNetworkUninitialized()
        {
            //uRemoteGridNetworkApi
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.CreateNetworkDiscretisation("", 0, 0, 0)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();
            
            // uGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApiNetworkDiscretisation.CreateNetworkDiscretisation("", 0, 0, 0));
            
            // uRemoteGridApiNetwork
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteUGridApiNetworkDiscretisation.CreateNetworkDiscretisation("", 0, 0, 0));
        }
        
        [Test]
        public void CreateNetworkDiscretisationTest()
        {
            // uGridApiNetwork
            uGridApiNetworkDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            
            int nmeshedges = 12;
            int nmeshpts = 31;
            int nwid = 1;
            int id = 1;
            int meshId = 0;
            wrapper.Expect(w => w.ionc_create_1d_mesh(ref id, ref nwid, ref meshId, "myMesh", ref nmeshpts, ref nmeshedges))
                .OutRef(id, nwid, meshId, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "wrapper", wrapper);
            
            // uRemoteGridApiNetwork
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.CreateNetworkDiscretisation("myMesh", nmeshpts, nmeshedges, nwid)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();
            
            // uGridApiNetwork
            var result = uGridApiNetworkDiscretisation.CreateNetworkDiscretisation("myMesh", nmeshpts, nmeshedges, nwid);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);
            Assert.AreEqual(nmeshpts, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));
            Assert.AreEqual(nmeshedges, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nMeshEdges"));
            
            // uRemoteGridApiNetwork
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "nMeshEdges", -1);
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "nNetworkPoints", -1);
            var remoteResult = uRemoteUGridApiNetworkDiscretisation.CreateNetworkDiscretisation("myMesh", nmeshpts, nmeshedges, nwid);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
            Assert.AreEqual(nmeshpts, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));
            Assert.AreEqual(nmeshedges, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nMeshEdges"));
        }
        
        [Test]
        public void CreateNetworkDiscretisationApiCallFailedTest()
        {
            // uGridApiNetwork
            uGridApiNetworkDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            
            int nmeshedges = 12;
            int nmeshpts = 31;
            int nwid = 1;
            int id = 1;
            int meshId = 0;
            wrapper.Expect(w => w.ionc_create_1d_mesh(ref id, ref nwid, ref meshId, "myMesh", ref nmeshpts, ref nmeshedges))
                .OutRef(id, nwid, meshId, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "wrapper", wrapper);
            
            // uRemoteGridApiNetwork
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.CreateNetworkDiscretisation("myMesh", nmeshpts, nmeshedges, nwid)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();
            
            // uGridApiNetwork
            var result = uGridApiNetworkDiscretisation.CreateNetworkDiscretisation("myMesh", nmeshpts, nmeshedges, nwid);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nMeshEdges"));

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetworkDiscretisation.CreateNetworkDiscretisation("myMesh", nmeshpts, nmeshedges, nwid);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nMeshEdges"));
        }

        [Test]
        public void CreateNetworkDiscretisationExceptionTest()
        {
            // uGridApiNetwork
            uGridApiNetworkDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int nmeshedges = 12;
            int nmeshpts = 31;
            int nwid = 1;
            int id = 1;
            int meshId = 0;
            wrapper.Expect(w => w.ionc_create_1d_mesh(ref id, ref nwid, ref meshId, "myMesh", ref nmeshpts, ref nmeshedges))
                .OutRef(id, nwid, meshId, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.CreateNetworkDiscretisation("myMesh", nmeshpts, nmeshedges, nwid)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetworkDiscretisation.CreateNetworkDiscretisation("myMesh", nmeshpts, nmeshedges, nwid);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nMeshEdges"));

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetworkDiscretisation.CreateNetworkDiscretisation("myMesh", nmeshpts, nmeshedges, nwid);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nMeshEdges"));
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void WriteNetworkDiscretisationPointsInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            int[] branchIdx = new int[1];
            double[] offset = new double[1];

            // uGridApiNetwork
            uGridApiNetworkDiscretisation.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridApiNetworkDiscretisation.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            // uRemoteGridApNetwork
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.WriteNetworkDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetworkDiscretisation.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteApiNetwork
            var remoteResult = uRemoteUGridApiNetworkDiscretisation.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
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

            // uGridApiNetwork
            uGridApiNetworkDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGridApiNetworkDiscretisation.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nNetworkPoints = nDiscPoints;

            TypeUtils.SetField(uGridApiNetworkDiscretisation, "nNetworkPoints", nNetworkPoints);
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.WriteNetworkDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            var result = uGridApiNetworkDiscretisation.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, result);

            // uRemoteGridApiNetwork
            var remoteResult = uRemoteUGridApiNetworkDiscretisation.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, remoteResult);
        }

        [Test]
        public void WriteNetworkDiscretisationTest()
        {
            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };
            int nNetworkPoints = 2;

            // uGridApiNetwork
            uGridApiNetworkDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Times(2);
            uGridApiNetworkDiscretisation.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Times(2);
            var wrapper = mocks.StrictMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_write_1d_mesh_discretisation_points(ref id, ref nwid, ref branchIdxPtr,
                    ref offsetPtr, ref nNetworkPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, branchIdxPtr, offsetPtr, nNetworkPoints)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetworkDiscretisation, "nNetworkPoints", nNetworkPoints);
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.WriteNetworkDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(nNetworkPoints, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));
            var result = uGridApiNetworkDiscretisation.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApiNetwork
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "nNetworkPoints", nNetworkPoints);
            var remoteResult = uRemoteUGridApiNetworkDiscretisation.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
        }

        [Test]
        public void WriteNetworkMeshDiscretisationApiCallFailedTest()
        {
            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };
            int nNetworkPoints = 2;

            // uGridApiNetwork
            uGridApiNetworkDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Times(2);
            uGridApiNetworkDiscretisation.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Times(2);
            var wrapper = mocks.StrictMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_write_1d_mesh_discretisation_points(ref id, ref nwid, ref branchIdxPtr,
                    ref offsetPtr, ref nNetworkPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, branchIdxPtr, offsetPtr, nNetworkPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();


            TypeUtils.SetField(uGridApiNetworkDiscretisation, "nNetworkPoints", branchIdx.Length);
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.WriteNetworkDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.GetNumberOfNetworkDiscretisationPoints(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(branchIdx.Length, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));
            var result = uGridApiNetworkDiscretisation.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApiNetwork
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "nNetworkPoints", branchIdx.Length);
            var remoteResult = uRemoteUGridApiNetworkDiscretisation.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void WriteNetworkDiscretisationExceptionTest()
        {
            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };
            int nNetworkPoints = 2;

            // uGridApiNetwork
            uGridApiNetworkDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Times(2);
            uGridApiNetworkDiscretisation.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Times(2);
            var wrapper = mocks.StrictMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_write_1d_mesh_discretisation_points(ref id, ref nwid, ref branchIdxPtr,
                    ref offsetPtr, ref nNetworkPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, branchIdxPtr, offsetPtr, nNetworkPoints)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetworkDiscretisation, "nNetworkPoints", branchIdx.Length);
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.WriteNetworkDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApiNetwork
            Assert.AreEqual(branchIdx.Length, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));
            var result = uGridApiNetworkDiscretisation.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApiNetwork
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "nNetworkPoints", branchIdx.Length);
            var remoteResult = uRemoteUGridApiNetworkDiscretisation.WriteNetworkDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        #endregion

        #region Read network discretisation

        [Test]
        public void GetNumberOfMeshDiscretisationPointsTest()
        {
            int nNetworkPoints = 5;

            // uGrdiNetworkApi
            uGridApiNetworkDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "nNetworkPoints", nNetworkPoints);

            //uRemoteGridNetworkApi
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.GetNumberOfNetworkDiscretisationPoints(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGridNetworkApi
            var result = uGridApiNetworkDiscretisation.GetNumberOfNetworkDiscretisationPoints(1);
            Assert.AreEqual(nNetworkPoints, result);
            Assert.AreEqual(nNetworkPoints, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));

            //uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridApiNetworkDiscretisation.GetNumberOfNetworkDiscretisationPoints(1);
            Assert.AreEqual(nNetworkPoints, remoteResult);
            Assert.AreEqual(nNetworkPoints, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void GetNumberOfMeshDiscretisationPointsApiCallTest(bool isInitialized, int nNetworkPoints)
        {
            //uGridNetworkApi
            uGridApiNetworkDiscretisation.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "nNetworkPoints", nNetworkPoints);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshPoints = 30;
            wrapper.Expect(
                    w => w.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints)).IgnoreArguments()
                .OutRef(ioncid, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetworkDiscretisation, "wrapper", wrapper);

            //uRemoteGridNetworkApi
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.GetNumberOfNetworkDiscretisationPoints(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGridNetworkApi
            var result = uGridApiNetworkDiscretisation.GetNumberOfNetworkDiscretisationPoints(1);
            Assert.AreEqual(numberOfMeshPoints, result);
            Assert.AreEqual(numberOfMeshPoints, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));

            //uRemoteGridNetworkApi
            TypeUtils.SetField(uGridApiNetworkDiscretisation, "nNetworkPoints", nNetworkPoints);
            var remoteResult = uRemoteUGridApiNetworkDiscretisation.GetNumberOfNetworkDiscretisationPoints(1);
            Assert.AreEqual(numberOfMeshPoints, remoteResult);
            Assert.AreEqual(numberOfMeshPoints, TypeUtils.GetField(uGridApiNetworkDiscretisation, "nNetworkPoints"));
        }
        
        [Test]
        public void GetNumberOfMeshDiscretisationPointsApiCallFailedTest()
        {
            //uGridNetworkApi
            uGridApiNetworkDiscretisation.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshPoints = -1;
            wrapper.Expect(
                    w => w.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints))
                .OutRef(ioncid, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetworkDiscretisation, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.GetNumberOfNetworkDiscretisationPoints(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGridNetworkApi
            var result = uGridApiNetworkDiscretisation.GetNumberOfNetworkDiscretisationPoints(1);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            //uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridApiNetworkDiscretisation.GetNumberOfNetworkDiscretisationPoints(1);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void GetNumberOfMeshDiscretisationPointsExceptionTest()
        {
            //uGridNetworkApi
            uGridApiNetworkDiscretisation.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshPoints = -1;
            wrapper.Expect(
                    w => w.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints))
                .OutRef(ioncid, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApiNetworkDiscretisation, "wrapper", wrapper);

            // uRemoteGridApiNetwork
            uRemoteUGridApiNetworkDiscretisation.Expect(a => a.GetNumberOfNetworkDiscretisationPoints(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGridNetworkApi
            var result = uGridApiNetworkDiscretisation.GetNumberOfNetworkDiscretisationPoints(1);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            //uRemoteGridNetworkApi
            var remoteResult = uRemoteUGridApiNetworkDiscretisation.GetNumberOfNetworkDiscretisationPoints(1);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        #endregion
    }
}
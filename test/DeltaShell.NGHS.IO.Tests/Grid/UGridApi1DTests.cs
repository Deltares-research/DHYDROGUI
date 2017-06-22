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
    public class UGridApi1DTests
    {
        private UGridApi1DNetwork uGridApi1DNetwork;
        private UGridApi1DDiscretisation uGridApi1DDiscretisation;
        private RemoteUGridApi1DNetwork uRemoteUGridApi1DNetwork;
        private RemoteUGridApi1DDiscretisation uRemoteUGridApi1DDiscretisation;
        private MockRepository mocks;
        
        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            uGridApi1DNetwork = mocks.DynamicMock<UGridApi1DNetwork>();
            uGridApi1DDiscretisation = mocks.DynamicMock<UGridApi1DDiscretisation>();
            uRemoteUGridApi1DNetwork = mocks.DynamicMock<RemoteUGridApi1DNetwork>();
            uRemoteUGridApi1DDiscretisation = mocks.DynamicMock<RemoteUGridApi1DDiscretisation>();
            TypeUtils.SetField(uRemoteUGridApi1DNetwork, "api", uGridApi1DNetwork);
            TypeUtils.SetField(uRemoteUGridApi1DDiscretisation, "api", uGridApi1DDiscretisation);
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void UGridApi1DTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "networkIdForWriting"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));
        }
        
        [Test]
        public void UGridApi1DDiscretisationTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DDiscretisation, "meshIdForWriting"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshEdges"));
        }

        [Test]
        public void RemoteUGridApi1DTest()
        {
            mocks.ReplayAll();
            var api = TypeUtils.GetField(uRemoteUGridApi1DNetwork, "api");
            var ugridApi1D = api as IUGridApi1DNetwork;
            Assert.That(api != null);
            Assert.That(ugridApi1D != null);

            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "networkIdForWriting"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));
        }
        
        [Test]
        public void RemoteUGridApi1DDiscretisationTest()
        {
            mocks.ReplayAll();
            var api = TypeUtils.GetField(uRemoteUGridApi1DDiscretisation, "api");
            var UGridApi1DDiscretisation = api as IUGridApi1DDiscretisation;
            Assert.That(api != null);
            Assert.That(UGridApi1DDiscretisation != null);
            
            Assert.AreEqual(-1, TypeUtils.GetField(UGridApi1DDiscretisation, "meshIdForWriting"));
            Assert.AreEqual(-1, TypeUtils.GetField(UGridApi1DDiscretisation, "nMeshPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(UGridApi1DDiscretisation, "nMeshEdges"));
        }

        #region Write 1DNetwork

        [Test]
        public void Create1DNetworkInvalidInitializationTest()
        {
            int networkId;

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Create1DNetwork("", 0, 0, 0, out networkId)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi1DNetwork.Create1DNetwork("", 0, 0, 0, out networkId));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteUGridApi1DNetwork.Create1DNetwork("", 0, 0, 0, out networkId));
        }

        [Test]
        public void Create1DNetworkTest()
        {
            // arrange
            string name = "Network name";
            int nNodes = 0;
            int nBranches = 0;
            int nGeoPoints = 0;

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Create1DNetwork(name, nNodes, nBranches, nGeoPoints, out nwid))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation)
                .Repeat.Once();

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApi1DNetwork.Create1DNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(nnodes, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(nbranches, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(ngeoPoints, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));

            // uRemoteGridApi1D
            TypeUtils.SetField(uGridApi1DNetwork, "nNodes", -1);
            TypeUtils.SetField(uGridApi1DNetwork, "nBranches", -1);
            TypeUtils.SetField(uGridApi1DNetwork, "nGeometryPoints", -1);

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uRemoteUGridApi1DNetwork.Create1DNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(nnodes, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(nbranches, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(ngeoPoints, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));
        }

        [Test]
        public void Create1DNetworkApiCallFailedTest()
        {
            // arrange
            string name = "Network name";
            int nNodes = 0;
            int nBranches = 0;
            int nGeoPoints = 0;

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Create1DNetwork(name, nNodes, nBranches, nGeoPoints, out nwid)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();


            // uGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGridApi1DNetwork.Create1DNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));

            // uRemoteGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uRemoteUGridApi1DNetwork.Create1DNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));

        }

        [Test]
        public void Create1DNetworkExceptionTest()
        {
            // arrange
            string name = "Network name";
            int nNodes = 0;
            int nBranches = 0;
            int nGeoPoints = 0;

            //uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Create1DNetwork(name, nNodes, nBranches, nGeoPoints, out nwid)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi1DNetwork.Create1DNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));

            // uRemoteGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteUGridApi1DNetwork.Create1DNetwork(name, nNodes, nBranches, nGeoPoints, out nwid));

            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void Write1DNetworkNodesInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            // uGrid1DApi
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkNodes(new double[0], new double[0], new string[0], new string[0])).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Write1DNetworkNodes(new double[0], new double[0], new string[0], new string[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Write1DNetworkNodes(new double[0], new double[0], new string[0], new string[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        public void Write1DNetworkNodesTest()
        {
            // arrange
            var nNodes = 2;
            double[] nodesX = new double[nNodes];
            double[] nodesY = new double[nNodes];
            string[] nodesIds = new[] { "node 1", "node 2" };
            string[] nodesLongnames = new[] { "long name", "long name 2" };

            // uGridApi1D
            int nodes;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).IgnoreArguments().OutRef(nNodes).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
        }

        [Test]
        public void Write1DNetworkNodesApiCallFailedTest()
        {
            // arrange
            var nNodes = 2;
            double[] nodesX = new double[nNodes];
            double[] nodesY = new double[nNodes];
            string[] nodesIds = new[] { "node 1", "node 2" };
            string[] nodesLongnames = new[] { "long name", "long name 2" };

            // uGridApi1D
            int nodes;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).IgnoreArguments().OutRef(nNodes).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D

            var result = uGridApi1DNetwork.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void Write1DNetworkNodesExceptionTest()
        {
            // arrange
            var nNodes = 2;
            double[] nodesX = new double[nNodes];
            double[] nodesY = new double[nNodes];
            string[] nodesIds = new[] { "node 1", "node 2" };
            string[] nodesLongnames = new[] { "long name", "long name 2" };

            // uGridApi1D
            int nodes;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).IgnoreArguments().OutRef(nNodes).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(-1, new[] { 0.0 }, new[] { 0.0 }, new[] { "" }, new[] { "" })]
        [TestCase(0, new[] { 0.0 }, new[] { 0.0 }, new[] { "" }, new[] { "" })]
        [TestCase(0, new[] { 0.0, 0.0 }, new[] { 0.0 }, new[] { "" }, new[] { "" })]
        [TestCase(0, new[] { 0.0 }, new[] { 0.0 }, new[] { "", "" }, new[] { "" })]
        [TestCase(0, new[] { 0.0 }, new[] { 0.0 }, new[] { "" }, new[] { "", "" })]
        public void Write1DNetworkNodesInitializedButArrayNotCorrectTest(int numberOfNetworkNodes, double[] nodesX, double[] nodesY, string[] nodesIds, string[] nodesLongnames)
        {
            // check with :
            // node Of numbers; 
            // Length xArray; 
            // Length yArray; 
            // Length idArray; 
            // Length desrcArray; 

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();
            int nodes;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).IgnoreArguments().OutRef(numberOfNetworkNodes).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGridApi1DNetwork.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uRemoteUGridApi1DNetwork.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames));
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void Write1DNetworkBranchesInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkBranches(new int[0], new int[0], new double[0], new int[0],
                new string[0], new string[0]))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D    
            var result = uGridApi1DNetwork.Write1DNetworkBranches(new int[0], new int[0], new double[0], new int[0],
                new string[0], new string[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D    
            var remoteResult = uRemoteUGridApi1DNetwork.Write1DNetworkBranches(new int[0], new int[0], new double[0], new int[0],
                new string[0], new string[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        public void Write1DNetworkBranchesTest()
        {
            // arrange
            int nBranches = 2;
            var sourceNodeId = new int[nBranches];
            var targetNodeId = new int[nBranches];
            var branghLength = new double[nBranches];
            var nBranchGeoPoints = new int[nBranches];
            var branchId = new[] { "branch 1", "branch 2" };
            var branchLongnames = new[] { "long name", "long name 2" };

            // uGridApi1D
            int branches;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).IgnoreArguments().OutRef(nBranches).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork
                .Expect(a => a.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints,
                    branchId, branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
        }

        [Test]
        public void Write1DNetworkBranchesApiCallFailedTest()
        {
            // arrange
            int nBranches = 2;
            var sourceNodeId = new int[nBranches];
            var targetNodeId = new int[nBranches];
            var branghLength = new double[nBranches];
            var nBranchGeoPoints = new int[nBranches];
            var branchId = new[] { "branch 1", "branch 2" };
            var branchLongnames = new[] { "long name", "long name 2" };

            // uGridApi1D
            int branches;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).IgnoreArguments().OutRef(nBranches).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void Write1DNetworkBranchesExceptionTest()
        {
            // arrange
            int nBranches = 2;
            var sourceNodeId = new int[nBranches];
            var targetNodeId = new int[nBranches];
            var branghLength = new double[nBranches];
            var nBranchGeoPoints = new int[nBranches];
            var branchId = new[] { "branch 1", "branch 2" };
            var branchLongnames = new[] { "long name", "long name 2" };

            // uGridApi1D
            int branches;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).IgnoreArguments().OutRef(nBranches).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
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
        public void Write1DNetworkBranchesInitializedButArrayNotCorrectTest(int nBranches, int[] sourceNodeId, int[] targetNodeId, double[] branchLength, int[] nBranchGeoPoints, string[] branchId, string[] branchLongname)
        {
            // check with :
            // Branch Of numbers; 
            // Length srcIds; 
            // Length targetIds; 
            // Length lengths; 
            // Length geoPts; 
            // Length idsArray; 
            // Length desrcArray; 

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();
            int branches;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).IgnoreArguments().OutRef(nBranches).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkBranches(sourceNodeId, targetNodeId, branchLength, nBranchGeoPoints, branchId, branchLongname)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGridApi1DNetwork.Write1DNetworkBranches(sourceNodeId, targetNodeId, branchLength, nBranchGeoPoints, branchId, branchLongname));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uRemoteUGridApi1DNetwork.Write1DNetworkBranches(sourceNodeId, targetNodeId, branchLength, nBranchGeoPoints, branchId, branchLongname));
        }

        [Test]
        public void Write1DNetworkGeometryTest()
        {
            // arrange
            int nGeoPoints = 2;
            var geopointsX = new[] { 0.0, 1.0 };
            var geopointsY = new[] { 0.0, 1.0 };

            // uGridApi1D
            int geopoints;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).IgnoreArguments().OutRef(nGeoPoints).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Write1DNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Write1DNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
        }

        [Test]
        public void Write1DNetworkGeometryApiCallFailedTest()
        {
            // arrange
            int nGeoPoints = 2;
            var geopointsX = new[] { 0.0, 1.0 };
            var geopointsY = new[] { 0.0, 1.0 };

            // uGridApi1D
            int geopoints;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).IgnoreArguments().OutRef(nGeoPoints).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Write1DNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Write1DNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void Write1DNetworkGeometryExceptionTest()
        {
            // arrange
            int nGeoPoints = 2;
            var geopointsX = new[] { 0.0, 1.0 };
            var geopointsY = new[] { 0.0, 1.0 };

            // uGridApi1D
            int geopoints;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).IgnoreArguments().OutRef(nGeoPoints).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Write1DNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Write1DNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void Write1DNetworkGeometryInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkGeometry(new double[0], new double[0])).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Write1DNetworkGeometry(new double[0], new double[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Write1DNetworkGeometry(new double[0], new double[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(-1, new[] { 1.0 }, new[] { 1.0 })]
        [TestCase(1, new[] { 1.0, 2.0 }, new[] { 1.0 })]
        [TestCase(1, new[] { 1.0 }, new[] { 1.0, 2.0 })]
        public void Write1DNetworkGeometryInitializedButArrayNotCorrectTest(int nGeopoints, double[] geopointsX, double[] geopointsY)
        {
            // check with :
            // number of geopoints; 
            // Length xArray; 
            // Length yArray; 

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Twice();
            int geopoints;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).IgnoreArguments().OutRef(nGeopoints).Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Write1DNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGridApi1DNetwork.Write1DNetworkGeometry(geopointsX, geopointsY));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uRemoteUGridApi1DNetwork.Write1DNetworkGeometry(geopointsX, geopointsY));
        }

        #endregion

        #region Read 1D network 

        [Test]
        public void GetNumberOfNetworkNodesTest()
        {
            int nNodes = 9;

            // uGridApi1D
            int nodes;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nNodes", nNodes);

            // uRemoteGridApi1D
            int rNodes;
            uRemoteUGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApi1DNetwork.GetNumberOfNetworkNodes(1, out nodes));
            Assert.AreEqual(nNodes, nodes);
            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uRemoteUGridApi1DNetwork.GetNumberOfNetworkNodes(1, out rNodes));
            Assert.AreEqual(nNodes, rNodes);
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void GetNumberOfNetworkNodesInvalidInitialization(bool isInitialized, int nNodes)
        {
            // uGridApi1D
            int nodes;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nNodes", nNodes);

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.ionc_get_1d_network_nodes_count(ref id, ref nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            int rNodes;
            uRemoteUGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.GetNumberOfNetworkNodes(1, out nodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);
            Assert.AreEqual(nNetworkNodes, nodes);
            Assert.AreEqual(nNetworkNodes, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));

            // uRemoteGridApi1D
            TypeUtils.SetField(uGridApi1DNetwork, "nNodes", nNodes);
            var remoteResult = uRemoteUGridApi1DNetwork.GetNumberOfNetworkNodes(1, out rNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
            Assert.AreEqual(nNetworkNodes, rNodes);
            Assert.AreEqual(nNetworkNodes, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));

        }

        [Test]
        public void GetNumberOfNetworkNodesApiCallFailedTest()
        {
            // uGridApi1D
            int nodes;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.ionc_get_1d_network_nodes_count(ref id, ref nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            int rNodes;
            uRemoteUGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.GetNumberOfNetworkNodes(1, out nodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.GetNumberOfNetworkNodes(1, out rNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));

        }

        [Test]
        public void GetNumberOfNetworkNodesExceptionTest()
        {
            // uGridApi1D
            int nodes;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out nodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.ionc_get_1d_network_nodes_count(ref id, ref nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            int rNodes;
            uRemoteUGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkNodes(1, out rNodes)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.GetNumberOfNetworkNodes(1, out nodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.GetNumberOfNetworkNodes(1, out rNodes);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nNodes"));
        }

        [Test]
        public void GetNumberOfNetworkBranchesTest()
        {
            int nBranches = 1;
            // uGridApi1D
            int branches;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nBranches", nBranches);

            // uRemoteGridApi1D
            int rbranches;
            uRemoteUGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out rbranches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApi1DNetwork.GetNumberOfNetworkBranches(1, out branches));
            Assert.AreEqual(nBranches, branches);

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uRemoteUGridApi1DNetwork.GetNumberOfNetworkBranches(1, out rbranches));
            Assert.AreEqual(nBranches, rbranches);
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void GetNumberOfNetworkBranchesInvalidInitialization(bool isInitialized, int nBranches)
        {
            // uGridApi1D
            int branches;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nBranches", nBranches);

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            int rbranches;
            uRemoteUGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out rbranches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApi1DNetwork.GetNumberOfNetworkBranches(1, out branches));
            Assert.AreEqual(nNetworkBranches, branches);
            Assert.AreEqual(nNetworkBranches, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));

            // uRemoteGridApi1D
            TypeUtils.SetField(uGridApi1DNetwork, "nBranches", nBranches);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uRemoteUGridApi1DNetwork.GetNumberOfNetworkBranches(1, out rbranches));
            Assert.AreEqual(nNetworkBranches, rbranches);
            Assert.AreEqual(nNetworkBranches, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
        }

        [Test]
        public void GetNumberOfNetworkBranchesApiCallFailedTest()
        {
            // uGrid
            int branches;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(false).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            int rbranches;
            uRemoteUGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out rbranches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.GetNumberOfNetworkBranches(1, out branches);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(-1, branches);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.GetNumberOfNetworkBranches(1, out rbranches);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(-1, rbranches);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
        }

        [Test]
        public void GetNumberOfNetworkBranchesExceptionTest()
        {
            // uGrid
            int branches;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out branches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(false).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            int rbranches;
            uRemoteUGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkBranches(1, out rbranches)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.GetNumberOfNetworkBranches(1, out branches);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, branches);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.GetNumberOfNetworkBranches(1, out rbranches);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(-1, rbranches);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nBranches"));
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsTest()
        {
            int nGeometryPoints = 11;

            // uGridApi1D
            int geopoints;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nGeometryPoints", nGeometryPoints);

            // uRemoteGridAp1D
            int rgeopoints;
            uRemoteUGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out rgeopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApi1DNetwork.GetNumberOfNetworkGeometryPoints(1, out geopoints));
            Assert.AreEqual(nGeometryPoints, geopoints);

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uRemoteUGridApi1DNetwork.GetNumberOfNetworkGeometryPoints(1, out rgeopoints));
            Assert.AreEqual(nGeometryPoints, rgeopoints);
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void GetNumberOfNetworkGeometryPointsInvalidInitialization(bool isInitialized, int nGeoPoints)
        {
            // uGridApi1D
            int geopoints;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nGeometryPoints", nGeoPoints);

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            int rgeopoints;
            uRemoteUGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out rgeopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApi1DNetwork.GetNumberOfNetworkGeometryPoints(1, out geopoints));
            Assert.AreEqual(nGeometryPoints, geopoints);
            Assert.AreEqual(nGeometryPoints, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));

            // uRemoteGridApi1D
            TypeUtils.SetField(uGridApi1DNetwork, "nGeometryPoints", nGeoPoints);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGridApi1DNetwork.GetNumberOfNetworkGeometryPoints(1, out rgeopoints));
            Assert.AreEqual(nGeometryPoints, rgeopoints);
            Assert.AreEqual(nGeometryPoints, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsApiCallFailed()
        {
            // uGridApi1D
            int geopoints;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(false).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            int rgeopoints;
            uRemoteUGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out rgeopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGridApi1DNetwork.GetNumberOfNetworkGeometryPoints(1, out geopoints));
            Assert.AreEqual(-1, geopoints);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGridApi1DNetwork.GetNumberOfNetworkGeometryPoints(1, out rgeopoints));
            Assert.AreEqual(-1, rgeopoints);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsExceptionTest()
        {
            // uGridApi1D
            int geopoints;
            uGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out geopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(false).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            int rgeopoints;
            uRemoteUGridApi1DNetwork.Expect(a => a.GetNumberOfNetworkGeometryPoints(1, out rgeopoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi1DNetwork.GetNumberOfNetworkGeometryPoints(1, out geopoints));
            Assert.AreEqual(-1, geopoints);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi1DNetwork.GetNumberOfNetworkGeometryPoints(1, out rgeopoints));
            Assert.AreEqual(-1, rgeopoints);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DNetwork, "nGeometryPoints"));
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

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nNodes", nNodes);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Read1DNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi1DNetwork.Read1DNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteUGridApi1DNetwork.Read1DNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames));
        }

        [Test]
        public void ReadNetworkNodesTest()
        {
            // arrange
            double[] nodesX;
            double[] nodesY;
            string[] nodesIds;
            string[] nodesLongnames;

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nNodes", 4);

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Read1DNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Read1DNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Read1DNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
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

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nNodes", 1);

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Read1DNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Read1DNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Read1DNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
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

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nNodes", 1);

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Read1DNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Read1DNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Read1DNetworkNodes(1, out nodesX, out nodesY, out nodesIds, out nodesLongnames);
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

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nBranches", nBranches);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
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

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGridApi1DNetwork, "nBranches", nBranches);

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork
                .Expect(a => a.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
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

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Once();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGridApi1DNetwork, "nBranches", nBranches);

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork
                .Expect(a => a.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
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

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGridApi1DNetwork, "nBranches", nBranches);

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork
                .Expect(a => a.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(0, sourceNodes.Length);
            Assert.AreEqual(0, targetNodes.Length);
            Assert.AreEqual(0, branchLengths.Length);
            Assert.AreEqual(0, branchGeoPoints.Length);
            Assert.AreEqual(0, branchIds.Length);
            Assert.AreEqual(0, branchLongnames.Length);

            // uGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
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

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGridApi1DNetwork, "nBranches", nBranches);

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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork
                .Expect(a => a.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Read1DNetworkBranches(1, out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
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

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            TypeUtils.SetField(uGridApi1DNetwork, "nNodes", nNodes);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Read1DNetworkGeometry(1, out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi1DNetwork.Read1DNetworkGeometry(1, out geopointsX, out geopointsY));
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);

            // uRemoteApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteUGridApi1DNetwork.Read1DNetworkGeometry(1, out geopointsX, out geopointsY));
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);
        }

        [Test]
        public void ReadNetworkGeometryTest()
        {
            double[] geopointsX;
            double[] geopointsY;

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nGeometryPoints", 1);
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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Read1DNetworkGeometry(1, out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Read1DNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);
            Assert.AreEqual(nGeoPoints, geopointsX.Length);
            Assert.AreEqual(nGeoPoints, geopointsY.Length);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Read1DNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
            Assert.AreEqual(nGeoPoints, geopointsX.Length);
            Assert.AreEqual(nGeoPoints, geopointsY.Length);
        }

        [Test]
        public void ReadNetworkGeometryApiCallFailedTest()
        {
            double[] geopointsX;
            double[] geopointsY;

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nGeometryPoints", 1);
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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Read1DNetworkGeometry(1,out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Read1DNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Read1DNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);
        }

        [Test]
        public void ReadNetworkGeometryExceptionTest()
        {
            double[] geopointsX;
            double[] geopointsY;

            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DNetwork, "nGeometryPoints", 1);
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

            TypeUtils.SetField(uGridApi1DNetwork, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Read1DNetworkGeometry(1, out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DNetwork.Read1DNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DNetwork.Read1DNetworkGeometry(1, out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);
        }
        #endregion

        #region 1D network initialization

        [Test]
        public void NetworkReadyTrueTest()
        {
            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGridApi1DNetwork, "networkIdForWriting", 1);

            // uGridApi1D
            Assert.AreEqual(true, uGridApi1DNetwork.NetworkReadyForWriting);

            // uRemoteApi1D
            Assert.AreEqual(true, uRemoteUGridApi1DNetwork.NetworkReadyForWriting);
        }

        [Test]
        public void NetworkReadyFalseBecauseNetworkIdNotSetTest()
        {
            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.NetworkReadyForWriting).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGridApi1DNetwork, "networkIdForWriting", -1);

            // uGridApi1D
            Assert.AreEqual(false, uGridApi1DNetwork.NetworkReadyForWriting);

            // uRemoteApi1D
            Assert.AreEqual(false, uRemoteUGridApi1DNetwork.NetworkReadyForWriting);
        }

        [Test]
        public void NetworkInitializedTrueTest()
        {
            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGridApi1DNetwork, "ioncid", 1);

            // uGridApi1D
            Assert.AreEqual(true, uGridApi1DNetwork.Initialized);

            // uRemoteApi1D
            Assert.AreEqual(true, uRemoteUGridApi1DNetwork.Initialized);
        }

        [Test]
        public void NetworkInitializedFalseBecauseIoncidNotSetTest()
        {
            // uGridApi1D
            uGridApi1DNetwork.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteApi1D
            uRemoteUGridApi1DNetwork.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGridApi1DNetwork, "ioncid", -1);

            // uGridApi1D
            Assert.AreEqual(false, uGridApi1DNetwork.Initialized);

            // uRemoteApi1D
            Assert.AreEqual(false, uRemoteUGridApi1DNetwork.Initialized);
        }

        #endregion

        #region Write 1D network discretisation
        
        [Test]
        public void Create1DMeshUninitialized()
        {
            //uRemoteGrid1DApi
            uRemoteUGridApi1DDiscretisation.Expect(a => a.Create1dDiscretisation("", 0, 0, 0)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();
            
            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGridApi1DDiscretisation.Create1dDiscretisation("", 0, 0, 0));
            
            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteUGridApi1DDiscretisation.Create1dDiscretisation("", 0, 0, 0));
        }
        
        [Test]
        public void Create1DMeshTest()
        {
            // uGridApi1D
            uGridApi1DDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            
            int nmeshedges = 12;
            int nmeshpts = 31;
            int nwid = 1;
            int id = 1;
            int meshId = 0;
            wrapper.Expect(w => w.ionc_create_1d_mesh(ref id, ref nwid, ref meshId, "myMesh", ref nmeshpts, ref nmeshedges))
                .OutRef(id, nwid, meshId, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DDiscretisation, "wrapper", wrapper);
            
            // uRemoteGridApi1D
            uRemoteUGridApi1DDiscretisation.Expect(a => a.Create1dDiscretisation("myMesh", nmeshpts, nmeshedges, nwid)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();
            
            // uGridApi1D
            var result = uGridApi1DDiscretisation.Create1dDiscretisation("myMesh", nmeshpts, nmeshedges, nwid);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);
            Assert.AreEqual(nmeshpts, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));
            Assert.AreEqual(nmeshedges, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshEdges"));
            
            // uRemoteGridApi1D
            TypeUtils.SetField(uGridApi1DDiscretisation, "nMeshEdges", -1);
            TypeUtils.SetField(uGridApi1DDiscretisation, "nMeshPoints", -1);
            var remoteResult = uRemoteUGridApi1DDiscretisation.Create1dDiscretisation("myMesh", nmeshpts, nmeshedges, nwid);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
            Assert.AreEqual(nmeshpts, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));
            Assert.AreEqual(nmeshedges, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshEdges"));
        }
        
        [Test]
        public void Create1DMeshApiCallFailedTest()
        {
            // uGridApi1D
            uGridApi1DDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            
            int nmeshedges = 12;
            int nmeshpts = 31;
            int nwid = 1;
            int id = 1;
            int meshId = 0;
            wrapper.Expect(w => w.ionc_create_1d_mesh(ref id, ref nwid, ref meshId, "myMesh", ref nmeshpts, ref nmeshedges))
                .OutRef(id, nwid, meshId, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DDiscretisation, "wrapper", wrapper);
            
            // uRemoteGridApi1D
            uRemoteUGridApi1DDiscretisation.Expect(a => a.Create1dDiscretisation("myMesh", nmeshpts, nmeshedges, nwid)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();
            
            // uGridApi1D
            var result = uGridApi1DDiscretisation.Create1dDiscretisation("myMesh", nmeshpts, nmeshedges, nwid);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshEdges"));

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DDiscretisation.Create1dDiscretisation("myMesh", nmeshpts, nmeshedges, nwid);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshEdges"));
        }

        [Test]
        public void Create1DMeshExceptionTest()
        {
            // uGridApi1D
            uGridApi1DDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Twice();
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
            TypeUtils.SetField(uGridApi1DDiscretisation, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DDiscretisation.Expect(a => a.Create1dDiscretisation("myMesh", nmeshpts, nmeshedges, nwid)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DDiscretisation.Create1dDiscretisation("myMesh", nmeshpts, nmeshedges, nwid);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshEdges"));

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DDiscretisation.Create1dDiscretisation("myMesh", nmeshpts, nmeshedges, nwid);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshEdges"));
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void Write1dDiscretisationPointsInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            int[] branchIdx = new int[1];
            double[] offset = new double[1];

            // uGridApi1D
            uGridApi1DDiscretisation.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGridApi1DDiscretisation.Expect(a => a.NetworkReadyForWriting).Return(isReady).Repeat.Any();

            // uRemoteGridAp1D
            uRemoteUGridApi1DDiscretisation.Expect(a => a.Write1dDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DDiscretisation.Write1dDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteApi1D
            var remoteResult = uRemoteUGridApi1DDiscretisation.Write1dDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(-1, new[] { 1, 2 }, new[] { 1.0, 2.0 })]
        [TestCase(1, new[] { 1, }, new[] { 1.0, 2.0 })]
        [TestCase(1, new[] { 1, 2 }, new[] { 1.0 })]
        public void Write1DMeshDiscretisationInitializedButArrayNotCorrectTest(int nDiscPoints, int[] branchIdx, double[] offset)
        {
            // check with :
            // number of Discretisation points; 
            // Length branchIds; 
            // Length offsets; 

            // uGridApi1D
            uGridApi1DDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGridApi1DDiscretisation.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nMeshPoints = nDiscPoints;

            TypeUtils.SetField(uGridApi1DDiscretisation, "nMeshPoints", nMeshPoints);
            TypeUtils.SetField(uGridApi1DDiscretisation, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DDiscretisation.Expect(a => a.Write1dDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGridApi1DDiscretisation.Write1dDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteUGridApi1DDiscretisation.Write1dDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, remoteResult);
        }

        [Test]
        public void Write1DMeshDiscretisationTest()
        {
            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };
            int nMeshPoints = 2;

            // uGridApi1D
            uGridApi1DDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Times(2);
            uGridApi1DDiscretisation.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Times(2);
            var wrapper = mocks.StrictMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_write_1d_mesh_discretisation_points(ref id, ref nwid, ref branchIdxPtr,
                    ref offsetPtr, ref nMeshPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, branchIdxPtr, offsetPtr, nMeshPoints)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi1DDiscretisation, "nMeshPoints", nMeshPoints);
            TypeUtils.SetField(uGridApi1DDiscretisation, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DDiscretisation.Expect(a => a.Write1dDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(nMeshPoints, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));
            var result = uGridApi1DDiscretisation.Write1dDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApi1D
            TypeUtils.SetField(uGridApi1DDiscretisation, "nMeshPoints", nMeshPoints);
            var remoteResult = uRemoteUGridApi1DDiscretisation.Write1dDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
        }

        [Test]
        public void Write1DMeshDiscretisationApiCallFailedTest()
        {
            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };
            int nMeshPoints = 2;

            // uGridApi1D
            uGridApi1DDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Times(2);
            uGridApi1DDiscretisation.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Times(2);
            var wrapper = mocks.StrictMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_write_1d_mesh_discretisation_points(ref id, ref nwid, ref branchIdxPtr,
                    ref offsetPtr, ref nMeshPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, branchIdxPtr, offsetPtr, nMeshPoints)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();


            TypeUtils.SetField(uGridApi1DDiscretisation, "nMeshPoints", branchIdx.Length);
            TypeUtils.SetField(uGridApi1DDiscretisation, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DDiscretisation.Expect(a => a.Write1dDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uRemoteUGridApi1DDiscretisation.Expect(a => a.GetNumberOf1dDiscretisationPoints(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(branchIdx.Length, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));
            var result = uGridApi1DDiscretisation.Write1dDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApi1D
            TypeUtils.SetField(uGridApi1DDiscretisation, "nMeshPoints", branchIdx.Length);
            var remoteResult = uRemoteUGridApi1DDiscretisation.Write1dDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void Write1DMeshDiscretisationExceptionTest()
        {
            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };
            int nMeshPoints = 2;

            // uGridApi1D
            uGridApi1DDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Times(2);
            uGridApi1DDiscretisation.Expect(a => a.NetworkReadyForWriting).Return(true).Repeat.Times(2);
            var wrapper = mocks.StrictMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_write_1d_mesh_discretisation_points(ref id, ref nwid, ref branchIdxPtr,
                    ref offsetPtr, ref nMeshPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, branchIdxPtr, offsetPtr, nMeshPoints)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi1DDiscretisation, "nMeshPoints", branchIdx.Length);
            TypeUtils.SetField(uGridApi1DDiscretisation, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DDiscretisation.Expect(a => a.Write1dDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(branchIdx.Length, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));
            var result = uGridApi1DDiscretisation.Write1dDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            TypeUtils.SetField(uGridApi1DDiscretisation, "nMeshPoints", branchIdx.Length);
            var remoteResult = uRemoteUGridApi1DDiscretisation.Write1dDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        #endregion

        #region Read 1D network discretisation

        [Test]
        public void GetNumberOfMeshDiscretisationPointsTest()
        {
            int nMeshPoints = 5;

            // uGrdi1DApi
            uGridApi1DDiscretisation.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DDiscretisation, "nMeshPoints", nMeshPoints);

            //uRemoteGrid1DApi
            uRemoteUGridApi1DDiscretisation.Expect(a => a.GetNumberOf1dDiscretisationPoints(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGrid1DApi
            var result = uGridApi1DDiscretisation.GetNumberOf1dDiscretisationPoints(1);
            Assert.AreEqual(nMeshPoints, result);
            Assert.AreEqual(nMeshPoints, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));

            //uRemoteGrid1DApi
            var remoteResult = uRemoteUGridApi1DDiscretisation.GetNumberOf1dDiscretisationPoints(1);
            Assert.AreEqual(nMeshPoints, remoteResult);
            Assert.AreEqual(nMeshPoints, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, -1)]
        public void GetNumberOfMeshDiscretisationPointsApiCallTest(bool isInitialized, int nMeshPoints)
        {
            //uGrid1DApi
            uGridApi1DDiscretisation.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            TypeUtils.SetField(uGridApi1DDiscretisation, "nMeshPoints", nMeshPoints);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshPoints = 30;
            wrapper.Expect(
                    w => w.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints)).IgnoreArguments()
                .OutRef(ioncid, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi1DDiscretisation, "wrapper", wrapper);

            //uRemoteGrid1DApi
            uRemoteUGridApi1DDiscretisation.Expect(a => a.GetNumberOf1dDiscretisationPoints(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGrid1DApi
            var result = uGridApi1DDiscretisation.GetNumberOf1dDiscretisationPoints(1);
            Assert.AreEqual(numberOfMeshPoints, result);
            Assert.AreEqual(numberOfMeshPoints, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));

            //uRemoteGrid1DApi
            TypeUtils.SetField(uGridApi1DDiscretisation, "nMeshPoints", nMeshPoints);
            var remoteResult = uRemoteUGridApi1DDiscretisation.GetNumberOf1dDiscretisationPoints(1);
            Assert.AreEqual(numberOfMeshPoints, remoteResult);
            Assert.AreEqual(numberOfMeshPoints, TypeUtils.GetField(uGridApi1DDiscretisation, "nMeshPoints"));
        }
        
        [Test]
        public void GetNumberOfMeshDiscretisationPointsApiCallFailedTest()
        {
            //uGrid1DApi
            uGridApi1DDiscretisation.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshPoints = -1;
            wrapper.Expect(
                    w => w.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints))
                .OutRef(ioncid, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGridApi1DDiscretisation, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DDiscretisation.Expect(a => a.GetNumberOf1dDiscretisationPoints(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGrid1DApi
            var result = uGridApi1DDiscretisation.GetNumberOf1dDiscretisationPoints(1);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            //uRemoteGrid1DApi
            var remoteResult = uRemoteUGridApi1DDiscretisation.GetNumberOf1dDiscretisationPoints(1);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void GetNumberOfMeshDiscretisationPointsExceptionTest()
        {
            //uGrid1DApi
            uGridApi1DDiscretisation.Expect(a => a.Initialized).Return(false).Repeat.Twice();

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

            TypeUtils.SetField(uGridApi1DDiscretisation, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteUGridApi1DDiscretisation.Expect(a => a.GetNumberOf1dDiscretisationPoints(1)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGrid1DApi
            var result = uGridApi1DDiscretisation.GetNumberOf1dDiscretisationPoints(1);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            //uRemoteGrid1DApi
            var remoteResult = uRemoteUGridApi1DDiscretisation.GetNumberOf1dDiscretisationPoints(1);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        #endregion
    }
}
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
        private UGridApi1D uGrid1DApi;
        private RemoteUGridApi1D uRemoteGrid1DApi;
        private MockRepository mocks;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            uGrid1DApi = mocks.DynamicMock<UGridApi1D>();
            uRemoteGrid1DApi = mocks.DynamicMock<RemoteUGridApi1D>();
            TypeUtils.SetField(uRemoteGrid1DApi, "api", uGrid1DApi);

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
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "networkId"));

            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshEdges"));
        }

        [Test]
        public void RemoteUGridApi1DTest()
        {
            mocks.ReplayAll();
            var api = TypeUtils.GetField(uRemoteGrid1DApi, "api");
            var ugridApi1D = api as IUGridApi1D;
            Assert.That(api != null);
            Assert.That(ugridApi1D != null);
        }
        #region Write 1DNetwork

        [Test]
        public void Create1DNetworkInvalidInitializationTest()
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Create1DNetwork("", 0, 0, 0)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            
            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Create1DNetwork("", 0, 0, 0));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGrid1DApi.Create1DNetwork("", 0, 0, 0));
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Create1DNetwork(name, nNodes, nBranches, nGeoPoints))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation)
                .Repeat.Once();
            
            mocks.ReplayAll();
            
            // uGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uGrid1DApi.Create1DNetwork(name, nNodes, nBranches, nGeoPoints));

            Assert.AreEqual(nnodes, TypeUtils.GetField(uGrid1DApi, "nNodes"));
            Assert.AreEqual(nbranches, TypeUtils.GetField(uGrid1DApi, "nBranches"));
            Assert.AreEqual(ngeoPoints, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

            // uRemoteGridApi1D
            TypeUtils.SetField(uGrid1DApi, "nNodes", -1);
            TypeUtils.SetField(uGrid1DApi, "nBranches", -1);
            TypeUtils.SetField(uGrid1DApi, "nGeometryPoints", -1);

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, uRemoteGrid1DApi.Create1DNetwork(name, nNodes, nBranches, nGeoPoints));

            Assert.AreEqual(nnodes, TypeUtils.GetField(uGrid1DApi, "nNodes"));
            Assert.AreEqual(nbranches, TypeUtils.GetField(uGrid1DApi, "nBranches"));
            Assert.AreEqual(ngeoPoints, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Create1DNetwork(name, nNodes, nBranches, nGeoPoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            
            // uGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGrid1DApi.Create1DNetwork(name, nNodes, nBranches, nGeoPoints));

            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

            // uRemoteGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uRemoteGrid1DApi.Create1DNetwork(name, nNodes, nBranches, nGeoPoints));

            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Create1DNetwork(name, nNodes, nBranches, nGeoPoints)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Create1DNetwork(name, nNodes, nBranches, nGeoPoints));

            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

            // uRemoteGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGrid1DApi.Create1DNetwork(name, nNodes, nBranches, nGeoPoints));

            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));
        }
        
        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void Write1DNetworkNodesInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            // uGrid1DApi
            uGrid1DApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(isReady).Repeat.Any();

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkNodes(new double[0], new double[0], new string[0], new string[0])).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Write1DNetworkNodes(new double[0], new double[0], new string[0], new string[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DNetworkNodes(new double[0], new double[0], new string[0], new string[0]);
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
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).Return(nNodes).Repeat.Twice();
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
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
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).Return(nNodes).Repeat.Twice();
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D

            var result = uGrid1DApi.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
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
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).Return(nNodes).Repeat.Twice();
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a=> a.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames);
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).Return(numberOfNetworkNodes).Repeat.Twice();

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uRemoteGrid1DApi.Write1DNetworkNodes(nodesX, nodesY, nodesIds, nodesLongnames));
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void Write1DNetworkBranchesInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(isReady).Repeat.Any();

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkBranches(new int[0], new int[0], new double[0], new int[0],
                new string[0], new string[0]))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D    
            var result = uGrid1DApi.Write1DNetworkBranches(new int[0], new int[0], new double[0], new int[0],
                new string[0], new string[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D    
            var remoteResult = uRemoteGrid1DApi.Write1DNetworkBranches(new int[0], new int[0], new double[0], new int[0],
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
            var branchId = new[] {"branch 1", "branch 2"};
            var branchLongnames = new[] {"long name", "long name 2"};

            // uGridApi1D
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).Return(2).Repeat.Twice();
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi
                .Expect(a => a.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints,
                    branchId, branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
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
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).Return(2).Repeat.Twice();
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
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
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).Return(2).Repeat.Twice();
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DNetworkBranches(sourceNodeId, targetNodeId, branghLength, nBranchGeoPoints, branchId, branchLongnames);
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).Return(nBranches).Repeat.Twice();

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkBranches(sourceNodeId, targetNodeId, branchLength, nBranchGeoPoints, branchId, branchLongname)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkBranches(sourceNodeId, targetNodeId, branchLength, nBranchGeoPoints, branchId, branchLongname));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uRemoteGrid1DApi.Write1DNetworkBranches(sourceNodeId, targetNodeId, branchLength, nBranchGeoPoints, branchId, branchLongname));
        }

        [Test]
        public void Write1DNetworkGeometryTest()
        {
            // arrange
            int nGeoPoints = 2;
            var geopointsX = new[] {0.0, 1.0}; 
            var geopointsY = new[] {0.0, 1.0};

            // uGridApi1D
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).Return(nGeoPoints).Repeat.Twice();
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Write1DNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DNetworkGeometry(geopointsX, geopointsY);
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
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).Return(nGeoPoints).Repeat.Twice();
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Write1DNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DNetworkGeometry(geopointsX, geopointsY);
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
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).Return(nGeoPoints).Repeat.Twice();
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Write1DNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DNetworkGeometry(geopointsX, geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void Write1DNetworkGeometryInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(isReady).Repeat.Any();

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkGeometry(new double[0], new double[0])).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Write1DNetworkGeometry(new double[0], new double[0]);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DNetworkGeometry(new double[0], new double[0]);
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).Return(nGeopoints).Repeat.Twice();

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DNetworkGeometry(geopointsX, geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkGeometry(geopointsX, geopointsY));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uRemoteGrid1DApi.Write1DNetworkGeometry(geopointsX, geopointsY));
        }

        #endregion

        #region Read 1D network 

        [Test]
        public void GetNumberOfNetworkNodesTest()
        {
            int nNodes = 9;

            // uGridApi1D
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "nNodes", nNodes);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();
            
            // uGridApi1D
            Assert.AreEqual(nNodes, uGrid1DApi.GetNumberOfNetworkNodes());

            // uRemoteGridApi1D
            Assert.AreEqual(nNodes, uRemoteGrid1DApi.GetNumberOfNetworkNodes());
        }

        [Test]
        [TestCase(false, true, 1)]
        [TestCase(true, false, 1)]
        [TestCase(true, true, -1)]
        public void GetNumberOfNetworkNodesInvalidInitialization(bool isInitialized, bool isReady, int nNodes)
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(isReady).Repeat.Any();
            TypeUtils.SetField(uGrid1DApi, "nNodes", nNodes);

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.ionc_get_1d_network_nodes_count(ref id, ref nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.GetNumberOfNetworkNodes();
            Assert.AreEqual(nNetworkNodes, result);
            Assert.AreEqual(nNetworkNodes, TypeUtils.GetField(uGrid1DApi, "nNodes"));

            // uRemoteGridApi1D
            TypeUtils.SetField(uGrid1DApi, "nNodes", nNodes);
            var remoteResult = uRemoteGrid1DApi.GetNumberOfNetworkNodes();
            Assert.AreEqual(nNetworkNodes, remoteResult);
            Assert.AreEqual(nNetworkNodes, TypeUtils.GetField(uGrid1DApi, "nNodes"));

        }

        [Test]
        public void GetNumberOfNetworkNodesApiCallFailedTest()
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Any();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.ionc_get_1d_network_nodes_count(ref id, ref nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.GetNumberOfNetworkNodes();
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.GetNumberOfNetworkNodes();
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));

        }

        [Test]
        public void GetNumberOfNetworkNodesExceptionTest()
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Any();

            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nNetworkNodes = 8;
            wrapper.Expect(w => w.ionc_get_1d_network_nodes_count(ref id, ref nwid, ref nNetworkNodes)).IgnoreArguments()
                .OutRef(id, nwid, nNetworkNodes)
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.GetNumberOfNetworkNodes();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.GetNumberOfNetworkNodes();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nNodes"));
        }

        [Test]
        public void GetNumberOfNetworkBranchesTest()
        {
            int nBranches = 1;
            // uGridApi1D
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "nBranches", nBranches);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(nBranches, uGrid1DApi.GetNumberOfNetworkBranches());

            // uRemoteGridApi1D
            Assert.AreEqual(nBranches, uRemoteGrid1DApi.GetNumberOfNetworkBranches());
        }

        [Test]
        [TestCase(false, true, 1)]
        [TestCase(true, false, 1)]
        [TestCase(true, true, -1)]
        public void GetNumberOfNetworkBranchesInvalidInitialization(bool isInitialized, bool isReady, int nBranches)
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(isReady).Repeat.Any();
            TypeUtils.SetField(uGrid1DApi, "nBranches", nBranches);

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(nNetworkBranches, uGrid1DApi.GetNumberOfNetworkBranches());
            Assert.AreEqual(nNetworkBranches, TypeUtils.GetField(uGrid1DApi, "nBranches"));

            // uRemoteGridApi1D
            TypeUtils.SetField(uGrid1DApi, "nBranches", nBranches);
            Assert.AreEqual(nNetworkBranches, uRemoteGrid1DApi.GetNumberOfNetworkBranches());
            Assert.AreEqual(nNetworkBranches, TypeUtils.GetField(uGrid1DApi, "nBranches"));
        }

        [Test]
        public void GetNumberOfNetworkBranchesApiCallFailedTest()
        {
            // uGrid
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.GetNumberOfNetworkBranches();
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.GetNumberOfNetworkBranches();
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));
        }

        [Test]
        public void GetNumberOfNetworkBranchesExceptionTest()
        {
            // uGrid
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.GetNumberOfNetworkBranches();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.GetNumberOfNetworkBranches();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nBranches"));
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsTest()
        {
            int nGeometryPoints = 11;

            // uGridApi1D
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "nGeometryPoints", nGeometryPoints);

            // uRemoteGridAp1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(nGeometryPoints, uGrid1DApi.GetNumberOfNetworkGeometryPoints());

            // uRemoteGridApi1D
            Assert.AreEqual(nGeometryPoints, uRemoteGrid1DApi.GetNumberOfNetworkGeometryPoints());
        }

        [Test]
        [TestCase(false, true, 1)]
        [TestCase(true, false, 1)]
        [TestCase(true, true, -1)]
        public void GetNumberOfNetworkGeometryPointsInvalidInitialization(bool isInitialized, bool isReady, int nGeoPoints)
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(isReady).Repeat.Any();
            TypeUtils.SetField(uGrid1DApi, "nGeometryPoints", nGeoPoints);

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(4, uGrid1DApi.GetNumberOfNetworkGeometryPoints());
            Assert.AreEqual(nGeometryPoints, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

            // uRemoteGridApi1D
            TypeUtils.SetField(uGrid1DApi, "nGeometryPoints", nGeoPoints);
            Assert.AreEqual(4, uGrid1DApi.GetNumberOfNetworkGeometryPoints());
            Assert.AreEqual(nGeometryPoints, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsApiCallFailed()
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();
            
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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGrid1DApi.GetNumberOfNetworkGeometryPoints());
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGrid1DApi.GetNumberOfNetworkGeometryPoints());
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsExceptionTest()
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.GetNumberOfNetworkGeometryPoints());
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.GetNumberOfNetworkGeometryPoints());
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nGeometryPoints"));
        }

        [Test]
        [TestCase(false, true, 1)]
        [TestCase(true, false, 1)]
        [TestCase(true, true, -1)]
        public void ReadNetworkNodesInvalidInitializationTest(bool isInitialized, bool isReady, int nNodes)
        {
            // arrange
            double[] nodesX;
            double[] nodesY;
            string[] nodesIds;
            string[] nodesLongnames;

            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(isReady).Repeat.Any();
            TypeUtils.SetField(uGrid1DApi, "nNodes", nNodes);
            
            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGrid1DApi.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames));
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "nNodes", 4);

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames);
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "nNodes", 1);

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames);
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "nNodes", 1);

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Read1DNetworkNodes(out nodesX, out nodesY, out nodesIds, out nodesLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        [Test]
        [TestCase(false, true, 1)]
        [TestCase(true, false, 1)]
        [TestCase(true, true, -1)]
        public void ReadNetworkBranchesInvalidInitializationTest(bool isInitialized, bool isReady, int nBranches)
        {
            // arrange
            int[] sourceNodes;
            int[] targetNodes;
            double[] branchLengths;
            int[] branchGeoPoints;
            string[] branchIds;
            string[] branchLongnames;
            
            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(isReady).Repeat.Any();
            TypeUtils.SetField(uGrid1DApi, "nBranches", nBranches);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGrid1DApi, "nBranches", nBranches);

            IntPtr sourceNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr targetNodePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr branchLengthPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nBranches);
            IntPtr branchGeoPointsPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);

            GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[nBranches];
            wrapper.Expect(w => w.ionc_read_1d_network_branches(ref id, ref nwid, ref sourceNodePtr,
                    ref targetNodePtr, ref branchLengthPtr, branchinfo, ref branchGeoPointsPtr, ref nBranches)).IgnoreArguments()
                .OutRef(id, nwid, sourceNodePtr, targetNodePtr, branchLengthPtr, branchinfo, branchGeoPointsPtr,nBranches)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Any();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi
                .Expect(a => a.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Once();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGrid1DApi, "nBranches", nBranches);

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi
                .Expect(a => a.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();
            
            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGrid1DApi, "nBranches", nBranches);

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi
                .Expect(a => a.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(0, sourceNodes.Length);
            Assert.AreEqual(0, targetNodes.Length);
            Assert.AreEqual(0, branchLengths.Length);
            Assert.AreEqual(0, branchGeoPoints.Length);
            Assert.AreEqual(0, branchIds.Length);
            Assert.AreEqual(0, branchLongnames.Length);

            // uGridApi1D
            var remoteResult = uRemoteGrid1DApi.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nBranches = 4;

            TypeUtils.SetField(uGrid1DApi, "nBranches", nBranches);

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi
                .Expect(a => a.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths,
                    out branchGeoPoints, out branchIds, out branchLongnames))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uGridApi1D
            var remoteResult = uRemoteGrid1DApi.Read1DNetworkBranches(out sourceNodes, out targetNodes, out branchLengths, out branchGeoPoints, out branchIds, out branchLongnames);
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
            uGrid1DApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(isReady).Repeat.Any();

            TypeUtils.SetField(uGrid1DApi, "nNodes", nNodes);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Read1DNetworkGeometry(out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Read1DNetworkGeometry(out geopointsX, out geopointsY));
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);

            // uRemoteApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGrid1DApi.Read1DNetworkGeometry(out geopointsX, out geopointsY));
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);
        }

        [Test]
        public void ReadNetworkGeometryTest()
        {
            double[] geopointsX;
            double[] geopointsY;

            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "nGeometryPoints", 1);
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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Read1DNetworkGeometry(out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Read1DNetworkGeometry(out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);
            Assert.AreEqual(nGeoPoints, geopointsX.Length);
            Assert.AreEqual(nGeoPoints, geopointsY.Length);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Read1DNetworkGeometry(out geopointsX, out geopointsY);
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "nGeometryPoints", 1);
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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Read1DNetworkGeometry(out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Read1DNetworkGeometry(out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Read1DNetworkGeometry(out geopointsX, out geopointsY);
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
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "nGeometryPoints", 1);
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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Read1DNetworkGeometry(out geopointsX, out geopointsY)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Read1DNetworkGeometry(out geopointsX, out geopointsY);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(0, geopointsX.Length);
            Assert.AreEqual(0, geopointsY.Length);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Read1DNetworkGeometry(out geopointsX, out geopointsY);
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
            uGrid1DApi.Expect(a => a.NetworkReady).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteApi1D
            uRemoteGrid1DApi.Expect(a => a.NetworkReady).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGrid1DApi, "networkId", 1);

            // uGridApi1D
            Assert.AreEqual(true, uGrid1DApi.NetworkReady);

            // uRemoteApi1D
            Assert.AreEqual(true, uRemoteGrid1DApi.NetworkReady);
        }

        [Test]
        public void NetworkReadyFalseBecauseNetworkIdNotSetTest()
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.NetworkReady).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteApi1D
            uRemoteGrid1DApi.Expect(a => a.NetworkReady).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGrid1DApi, "networkId", -1);

            // uGridApi1D
            Assert.AreEqual(false, uGrid1DApi.NetworkReady);

            // uRemoteApi1D
            Assert.AreEqual(false, uRemoteGrid1DApi.NetworkReady);
        }

        [Test]
        public void NetworkInitializedTrueTest()
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteApi1D
            uRemoteGrid1DApi.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGrid1DApi, "ioncid", 1);

            // uGridApi1D
            Assert.AreEqual(true, uGrid1DApi.Initialized);

            // uRemoteApi1D
            Assert.AreEqual(true, uRemoteGrid1DApi.Initialized);
        }

        [Test]
        public void NetworkInitializedFalseBecauseIoncidNotSetTest()
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            // uRemoteApi1D
            uRemoteGrid1DApi.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            TypeUtils.SetField(uGrid1DApi, "ioncid", -1);

            // uGridApi1D
            Assert.AreEqual(false, uGrid1DApi.Initialized);

            // uRemoteApi1D
            Assert.AreEqual(false, uRemoteGrid1DApi.Initialized);
        }

        #endregion

        #region Write 1D network discretisation

        [Test]
        public void Create1DMeshUninitialized()
        {
            //uRemoteGrid1DApi
            uRemoteGrid1DApi.Expect(a => a.Create1DMesh("", 0, 0)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Create1DMesh("", 0, 0));

            // uRemoteGridApi1D
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGrid1DApi.Create1DMesh("", 0, 0));
        }

        [Test]
        public void Create1DMeshTest()
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int nmeshedges = 12;
            int nmeshpts = 31;
            int nwid = 1;
            int id = 1;
            wrapper.Expect(w => w.ionc_create_1d_mesh(ref id, ref nwid, "myMesh", ref nmeshpts, ref nmeshedges))
                .OutRef(id, nwid, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Create1DMesh("myMesh", nmeshpts, nmeshedges)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Create1DMesh("myMesh", nmeshpts, nmeshedges);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);
            Assert.AreEqual(nmeshpts, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            Assert.AreEqual(nmeshedges, TypeUtils.GetField(uGrid1DApi, "nMeshEdges"));

            // uRemoteGridApi1D
            TypeUtils.SetField(uGrid1DApi, "nMeshEdges", -1);
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", -1);
            var remoteResult = uRemoteGrid1DApi.Create1DMesh("myMesh", nmeshpts, nmeshedges);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
            Assert.AreEqual(nmeshpts, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            Assert.AreEqual(nmeshedges, TypeUtils.GetField(uGrid1DApi, "nMeshEdges"));
        }

        [Test]
        public void Create1DMeshApiCallFailedTest()
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int nmeshedges = 12;
            int nmeshpts = 31;
            int nwid = 1;
            int id = 1;
            wrapper.Expect(w => w.ionc_create_1d_mesh(ref id, ref nwid, "myMesh", ref nmeshpts, ref nmeshedges))
                .OutRef(id, nwid, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR).Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Create1DMesh("myMesh", nmeshpts, nmeshedges)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Create1DMesh("myMesh", nmeshpts, nmeshedges);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshEdges"));

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Create1DMesh("myMesh", nmeshpts, nmeshedges);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshEdges"));
        }

        [Test]
        public void Create1DMeshExceptionTest()
        {
            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int nmeshedges = 12;
            int nmeshpts = 31;
            int nwid = 1;
            int id = 1;
            wrapper.Expect(w => w.ionc_create_1d_mesh(ref id, ref nwid, "myMesh", ref nmeshpts, ref nmeshedges))
                .OutRef(id, nwid, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("myTest"))
                .Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Create1DMesh("myMesh", nmeshpts, nmeshedges)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Create1DMesh("myMesh", nmeshpts, nmeshedges);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshEdges"));

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Create1DMesh("myMesh", nmeshpts, nmeshedges);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshEdges"));
        }

        [Test]
        [TestCase(false, true)]
        [TestCase(true, false)]
        public void Write1DMeshDiscretisationPointsInvalidInitializationTest(bool isInitialized, bool isReady)
        {
            int[] branchIdx = new int[1];
            double[] offset = new double[1];

            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(isReady).Repeat.Any();

            // uRemoteGridAp1D
            uRemoteGrid1DApi.Expect(a => a.Write1DMeshDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            var result = uGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
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
            //uGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).Return(nDiscPoints).Repeat.Any();
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Any();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int id = 0;
            int nwid = 0;
            int nMeshPoints = nDiscPoints;

            wrapper.Expect(w => w.ionc_get_1d_mesh_discretisation_points_count(ref id, ref nwid, ref nMeshPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nMeshPoints)
                .Return(nMeshPoints)
                .Repeat.Twice();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DMeshDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            
            mocks.ReplayAll();
            
            // uGridApi1D
            var result = uGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, result);

            // uRemoteGridApi1D
            var remoteResult = uRemoteGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, remoteResult);
        }

        [Test]
        public void Write1DMeshDiscretisationTest()
        {
            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };
            int nMeshPoints = 2;
            
            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Times(3);
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Times(3);
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

            wrapper.Expect(w => w.ionc_get_1d_mesh_discretisation_points_count(ref id, ref nwid, ref nMeshPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nMeshPoints)
                .Return(nMeshPoints)
                .Repeat.Twice();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DMeshDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            var result = uGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, result);

            // uRemoteGridApi1D
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", -1);
            var remoteResult = uRemoteGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_NOERR, remoteResult);
        }

        [Test]
        public void Write1DMeshDiscretisationApiCallFailedTest()
        {
            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };
            int nMeshPoints = 2;

            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Times(3);
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Times(3);
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

            wrapper.Expect(w => w.ionc_get_1d_mesh_discretisation_points_count(ref id, ref nwid, ref nMeshPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nMeshPoints)
                .Return(nMeshPoints)
                .Repeat.Twice();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DMeshDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            var result = uGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            // uRemoteGridApi1D
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", -1);
            var remoteResult = uRemoteGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void Write1DMeshDiscretisationExceptionTest()
        {
            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };
            int nMeshPoints = 2;

            // uGridApi1D
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Times(3);
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Times(3);
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

            wrapper.Expect(w => w.ionc_get_1d_mesh_discretisation_points_count(ref id, ref nwid, ref nMeshPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, nMeshPoints)
                .Return(nMeshPoints)
                .Repeat.Twice();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.Write1DMeshDiscretisationPoints(branchIdx, offset)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGridApi1D
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            var result = uGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            // uRemoteGridApi1D
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", -1);
            var remoteResult = uRemoteGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }
        
        #endregion

        #region Read 1D network discretisation

        [Test]
        public void GetNumberOfMeshDiscretisationPointsTest()
        {
            int nMeshPoints = 5;

            // uGrdi1DApi
            uGrid1DApi.Expect(a => a.Initialized).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(true).Repeat.Twice();
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", nMeshPoints);
            
            //uRemoteGrid1DApi
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGrid1DApi
            var result = uGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(nMeshPoints, result);
            Assert.AreEqual(nMeshPoints, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));

            //uRemoteGrid1DApi
            var remoteResult = uRemoteGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(nMeshPoints, remoteResult);
            Assert.AreEqual(nMeshPoints, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
        }

        [Test]
        [TestCase(false, true, 1)]
        [TestCase(true, false, 1)]
        [TestCase(true, true, -1)]
        public void GetNumberOfMeshDiscretisationPointsApiCallTest(bool isInitialized, bool isReady, int nMeshPoints)
        {
            //uGrid1DApi
            uGrid1DApi.Expect(a => a.Initialized).Return(isInitialized).Repeat.Twice();
            uGrid1DApi.Expect(a => a.NetworkReady).Return(isReady).Repeat.Any();
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", nMeshPoints);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshPoints = 30;
            wrapper.Expect(
                    w => w.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints)).IgnoreArguments()
                .OutRef(ioncid, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            //uRemoteGrid1DApi
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGrid1DApi
            var result = uGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(numberOfMeshPoints, result);
            Assert.AreEqual(numberOfMeshPoints, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));

            //uRemoteGrid1DApi
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", nMeshPoints);
            var remoteResult = uRemoteGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(numberOfMeshPoints, remoteResult);
            Assert.AreEqual(numberOfMeshPoints, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
        }
        
        [Test]
        public void GetNumberOfMeshDiscretisationPointsApiCallFailedTest()
        {
            //uGrid1DApi
            uGrid1DApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshPoints = -1;
            wrapper.Expect(
                    w => w.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints))
                .OutRef(ioncid, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Twice();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGrid1DApi
            var result = uGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, result);

            //uRemoteGrid1DApi
            var remoteResult = uRemoteGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteResult);
        }

        [Test]
        public void GetNumberOfMeshDiscretisationPointsExceptionTest()
        {
            //uGrid1DApi
            uGrid1DApi.Expect(a => a.Initialized).Return(false).Repeat.Twice();

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

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            // uRemoteGridApi1D
            uRemoteGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            //uGrid1DApi
            var result = uGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);

            //uRemoteGrid1DApi
            var remoteResult = uRemoteGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, remoteResult);
        }

        #endregion
    }
}
using System;
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
        public void Create1DNetworkUnInitializedTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Create1DNetwork("", 0, 0, 0));
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGrid1DApi.Create1DNetwork("", 0, 0, 0));
        }

        [Test]
        public void Create1DNetworkTest()
        {
            mocks.ReplayAll();
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_create_1d_network

            /*
            uGrid1DApi.Expect(a => uGrid1DApi.Initialized).Return(true).Repeat.Once();
            mocks.ReplayAll();
            TypeUtils.SetField(uGrid1DApi, "ioncid", 1);
            TypeUtils.SetField(uGrid1DApi, "networkId", 1);
            TypeUtils.SetField(uRemoteGrid1DApi, "api", uGrid1DApi);
            Assert.AreEqual(-1, uRemoteGrid1DApi.Create1DNetwork("", 0, 0, 0));
            */

        }

        [Test]
        public void Write1DNetworkNodesTest()
        {
            mocks.ReplayAll();
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_write_1d_network_nodes
        }

        [Test]
        public void Write1DNetworkNodesUnInitializedTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Write1DNetworkNodes(new[] { 0.0 }, new[] { 0.0 }, new[] { string.Empty }, new[] { string.Empty }));
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGrid1DApi.Write1DNetworkNodes(new[] { 0.0 }, new[] { 0.0 }, new[] { string.Empty }, new[] { string.Empty }));
        }

        [Test]
        public void Write1DNetworkNodesInitializedButArrayNotCorrectTest()
        {
            // check with :
            // node Of numbers = -1; 
            // Length xArray = 1; 
            // Length yArray = 1; 
            // Length idArray = 1; 
            // Length desrcArray = 1; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).Return(-1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkNodes(new[] { 0.0 }, new[] { 0.0 }, new[] { string.Empty }, new[] { string.Empty }));

            mocks.BackToRecordAll();
            
            // check with :
            // node Of numbers = 1; 
            // Length xArray = 2; 
            // Length yArray = 1; 
            // Length idArray = 1; 
            // Length desrcArray = 1; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkNodes(new[] { 0.0, 0.0 }, new[] { 0.0 }, new[] { string.Empty }, new[] { string.Empty }));

            mocks.BackToRecordAll();

            // check with :
            // node Of numbers = 1; 
            // Length xArray = 1; 
            // Length yArray = 2; 
            // Length idArray = 1; 
            // Length desrcArray = 1; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkNodes(new[] { 0.0 }, new[] { 0.0, 0.0 }, new[] { string.Empty }, new[] { string.Empty }));

            mocks.BackToRecordAll();
            
            // check with :
            // node Of numbers = 1; 
            // Length xArray = 1; 
            // Length yArray = 1; 
            // Length idArray = 2; 
            // Length desrcArray = 1; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkNodes(new[] { 0.0 }, new[] { 0.0 }, new[] { string.Empty, string.Empty }, new[] { string.Empty }));

            mocks.BackToRecordAll();
            
            // check with :
            // node Of numbers = 1; 
            // Length xArray = 1; 
            // Length yArray = 1; 
            // Length idArray = 1; 
            // Length desrcArray = 2; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkNodes(new[] { 0.0 }, new[] { 0.0 }, new[] { string.Empty }, new[] { string.Empty, string.Empty }));
        }

        [Test]
        public void Write1DNetworkBranchesUnInitializedTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Write1DNetworkBranches(new[] { 0 }, new[] { 0 }, new[] { 0.0 }, new[] { 0 }, new[] { string.Empty }, new[] { string.Empty }));
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGrid1DApi.Write1DNetworkBranches(new[] { 0 }, new[] { 0 }, new[] { 0.0 }, new[] { 0 }, new[] { string.Empty }, new[] { string.Empty }));
        }

        [Test]
        public void Write1DNetworkBranchesTest()
        {
            mocks.ReplayAll();
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_write_1d_network_branches
        }

        [Test]
        public void Write1DNetworkBranchesInitializedButArrayNotCorrectTest()
        {
            // check with :
            // Branch Of numbers = -1; 
            // Length srcIds = 1; 
            // Length targetIds = 1; 
            // Length lengths = 1; 
            // Length geoPts = 1; 
            // Length idsArray = 1; 
            // Length desrcArray = 1; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).Return(-1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkBranches(new[] { 0 }, new[] { 0 }, new[] { 0.0 }, new[] { 0 }, new[] { string.Empty }, new[] { string.Empty }));

            mocks.BackToRecordAll();

            // check with :
            // Branch Of numbers = 1; 
            // Length srcIds = 2; 
            // Length targetIds = 1; 
            // Length lengths = 1; 
            // Length geoPts = 1; 
            // Length idsArray = 1; 
            // Length desrcArray = 1; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkBranches(new[] { 0, 0 }, new[] { 0 }, new[] { 0.0 }, new[] { 0 }, new[] { string.Empty }, new[] { string.Empty }));

            mocks.BackToRecordAll();

            // check with :
            // Branch Of numbers = 1; 
            // Length srcIds = 1; 
            // Length targetIds = 2; 
            // Length lengths = 1; 
            // Length geoPts = 1; 
            // Length idsArray = 1; 
            // Length desrcArray = 1; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkBranches(new[] { 0 }, new[] { 0, 0 }, new[] { 0.0 }, new[] { 0 }, new[] { string.Empty }, new[] { string.Empty }));

            mocks.BackToRecordAll();

            // check with :
            // Branch Of numbers = 1; 
            // Length srcIds = 1; 
            // Length targetIds = 1; 
            // Length lengths = 2; 
            // Length geoPts = 1; 
            // Length idsArray = 1; 
            // Length desrcArray = 1; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkBranches(new[] { 0 }, new[] { 0 }, new[] { 0.0, 0.0 }, new[] { 0 }, new[] { string.Empty }, new[] { string.Empty }));

            mocks.BackToRecordAll();
            
            // check with :
            // Branch Of numbers = 1; 
            // Length srcIds = 1; 
            // Length targetIds = 1; 
            // Length lengths = 1; 
            // Length geoPts = 2; 
            // Length idsArray = 1; 
            // Length desrcArray = 1; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkBranches(new[] { 0 }, new[] { 0 }, new[] { 0.0 }, new[] { 0, 0 }, new[] { string.Empty }, new[] { string.Empty }));

            mocks.BackToRecordAll();
            
            // check with :
            // Branch Of numbers = 1; 
            // Length srcIds = 1; 
            // Length targetIds = 1; 
            // Length lengths = 1; 
            // Length geoPts = 1; 
            // Length idsArray = 2; 
            // Length desrcArray = 1; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkBranches(new[] { 0 }, new[] { 0 }, new[] { 0.0 }, new[] { 0 }, new[] { string.Empty, string.Empty }, new[] { string.Empty }));

            mocks.BackToRecordAll();
            
            // check with :
            // Branch Of numbers = 1; 
            // Length srcIds = 1; 
            // Length targetIds = 1; 
            // Length lengths = 1; 
            // Length geoPts = 1; 
            // Length idsArray = 1; 
            // Length desrcArray = 2; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkBranches(new[] { 0 }, new[] { 0 }, new[] { 0.0 }, new[] { 0 }, new[] { string.Empty }, new[] { string.Empty, string.Empty }));
        }

        [Test]
        public void Write1DNetworkGeometryTest()
        {
            mocks.ReplayAll();
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_write_1d_network_branches_geometry
        }

        [Test]
        public void Write1DNetworkGeometryUnInitializedTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Write1DNetworkGeometry(new[] { 0.0 }, new[] { 0.0 }));
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGrid1DApi.Write1DNetworkGeometry(new[] { 0.0 }, new[] { 0.0 }));
        }

        [Test]
        public void Write1DNetworkGeometryInitializedButArrayNotCorrectTest()
        {
            // check with :
            // node Of numbers = -1; 
            // Length xArray = 1; 
            // Length yArray = 1; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).Return(-1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkGeometry(new[] { 0.0 }, new[] { 0.0 }));

            mocks.BackToRecordAll();
            
            // check with :
            // node Of numbers = 1; 
            // Length xArray = 1; 
            // Length yArray = 1; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkGeometry(new[] { 0.0, 0.0 }, new[] { 0.0 }));

            mocks.BackToRecordAll();

            // check with :
            // node Of numbers = 1; 
            // Length xArray = 1; 
            // Length yArray = 2; 
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkGeometry(new[] { 0.0 }, new[] { 0.0, 0.0 }));
        }

        [Test]
        public void GetNumberOfNetworkNodesTest()
        {
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            mocks.ReplayAll();
            TypeUtils.SetField(uGrid1DApi, "nNodes", 1);

            Assert.AreEqual(1, uGrid1DApi.GetNumberOfNetworkNodes());
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_get_1d_network_nodes_count
        }

        [Test]
        public void GetNumberOfNetworkBranchesTest()
        {
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            mocks.ReplayAll();
            TypeUtils.SetField(uGrid1DApi, "nBranches", 1);

            Assert.AreEqual(1, uGrid1DApi.GetNumberOfNetworkBranches());
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_get_1d_network_branches_count
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsTest()
        {
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            mocks.ReplayAll();
            TypeUtils.SetField(uGrid1DApi, "nGeometryPoints", 1);

            Assert.AreEqual(1, uGrid1DApi.GetNumberOfNetworkGeometryPoints());
            //Cannot create unit test because cannot mock the static method : GridWrapper.ionc_get_1d_network_branches_geometry_coordinate_count
        }

        [Test]
        public void NetworkReadyTrueTest()
        {
            uGrid1DApi.Expect(a => a.IsNetworkReady()).CallOriginalMethod(OriginalCallOptions.NoExpectation).Repeat.Once();
            mocks.ReplayAll();
            TypeUtils.SetField(uGrid1DApi, "networkId", 1);
            Assert.AreEqual(true, uGrid1DApi.IsNetworkReady());
        }

        [Test]
        public void NetworkReadyFalseBecauseNetworkIdNotSetTest()
        {
            uGrid1DApi.Expect(a => a.IsNetworkReady()).CallOriginalMethod(OriginalCallOptions.NoExpectation).Repeat.Once();
            mocks.ReplayAll();
            TypeUtils.SetField(uGrid1DApi, "networkId", -1);
            Assert.AreEqual(false, uGrid1DApi.IsNetworkReady());
        }

        [Test]
        public void NetworkInitializedTrueTest()
        {
            uGrid1DApi.Expect(a => a.IsInitialized())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation)
                .Repeat.Once();
            mocks.ReplayAll();
            TypeUtils.SetField(uGrid1DApi, "ioncid", 1);
            Assert.AreEqual(true, uGrid1DApi.IsInitialized());
        }

        [Test]
        public void NetworkInitializedFalseBecauseIoncidNotSetTest()
        {
            uGrid1DApi.Expect(a => a.IsInitialized())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation)
                .Repeat.Once();
            mocks.ReplayAll();
            TypeUtils.SetField(uGrid1DApi, "ioncid", -1);
            Assert.AreEqual(false, uGrid1DApi.IsInitialized());

        }

        [Test]
        public void Create1DMeshUninitialized()
        {
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Create1DMesh("", 0, 0));
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGrid1DApi.Create1DMesh("", 0, 0));
        }

        [Test]
        public void Create1DMeshTest()
        {
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int nmeshedges = 12;
            int nmeshpts = 31;
            int nwid = 1;
            int id = 1;
            wrapper.Expect(w => w.ionc_create_1d_mesh(ref id, ref nwid, "myMesh", ref nmeshpts, ref nmeshedges))
                .OutRef(id, nwid, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR).Repeat.Once();
            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            mocks.ReplayAll();

            uGrid1DApi.Create1DMesh("myMesh", nmeshpts, nmeshedges);
            Assert.AreEqual(nmeshpts, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            Assert.AreEqual(nmeshedges, TypeUtils.GetField(uGrid1DApi, "nMeshEdges"));
        }

        [Test]
        public void Create1DMeshInvalidPointsTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int nmeshedges = -3;
            int nmeshpts = -16;
            int nwid = 1;
            int id = 1;
            wrapper.Expect(w => w.ionc_create_1d_mesh(ref id, ref nwid, "myMesh", ref nmeshpts, ref nmeshedges))
                .OutRef(id, nwid, nmeshpts, nmeshedges).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR).Repeat.Once();
            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Create1DMesh("myMesh", nmeshpts, nmeshedges));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshEdges"));
        }

        [Test]
        public void Write1DMeshDiscretisationPointsUninitializedTest()
        {
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(false).Repeat.Any();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Any();
            mocks.ReplayAll();
           
            int[] branchIdx = new int[] { 1 };
            double[] offset = new double[] { 0.5 };
            var result = uGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
        }

        [Test]
        public void Write1DMeshDiscretisationPointsNetworkNotReadyTest()
        {
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Any();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(false).Repeat.Any();
            mocks.ReplayAll();
            
            int[] branchIdx = new int[] { 1 };
            double[] offset = new double[] { 0.5 };
            var result = uGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
        }

        [Test]
        public void Write1DMeshDiscretisationInvalidArrayLengthTest()
        {
            uGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).Return(2).Repeat.Twice();
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Twice();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Twice();
            mocks.ReplayAll();

            int[] invalidBranchIdx = new int[] { 1 };
            double[] offset = new double[] { 0.5, 1.3 };

            var resultInvalidBranchIdx = uGrid1DApi.Write1DMeshDiscretisationPoints(invalidBranchIdx, offset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, resultInvalidBranchIdx);

            int[] branchIdx = new int[] { 1, 2 };
            double[] invalidOffset = new double[] { 2, 3, 4 };

            var resultInvalidOffset = uGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, invalidOffset);
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, resultInvalidOffset);
        }

        [Test]
        public void Write1DMeshDiscretisationTest()
        {
            uGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).Return(2).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            var wrapper = mocks.StrictMock<IGridWrapper>();
            
            int id = 1;
            int nwid = 1;
            int nMeshPoints = 2;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_write_1d_mesh_discretisation_points(ref id, ref nwid, ref branchIdxPtr,
                    ref offsetPtr, ref nMeshPoints))
                    .IgnoreArguments()
                    .OutRef(id, nwid, branchIdxPtr, offsetPtr, nMeshPoints)
                    .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Once();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            mocks.ReplayAll();

            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };

            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            var ierr = uGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(0, ierr);
        }

        [Test]
        public void Write1DMeshDiscretisationExceptionTest()
        {
            uGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).Return(2).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();

            var wrapper = mocks.StrictMock<IGridWrapper>();
            int id = 1;
            int nwid = 1;
            int nMeshPoints = 2;

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_write_1d_mesh_discretisation_points(ref id, ref nwid, ref branchIdxPtr,
                    ref offsetPtr, ref nMeshPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, branchIdxPtr, offsetPtr, nMeshPoints)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Throw(new Exception("mytest"))
                .Repeat.Once();
            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);
            mocks.ReplayAll();

            int[] branchIdx = new int[] { 1, 1, };
            double[] offset = new double[] { 0.5, 1.2, };

            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset));
            
        }


        [Test]
        [Ignore("In progress")]
        public void Write1DMeshDiscretisationFreeMemoryTest()
        {
            int size = 260000000;
            uGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).Return(size).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            var wrapper = mocks.StrictMock<IGridWrapper>();

            int id = 1;
            int nwid = 1;
            int nMeshPoints = size;

            long startMemory = GC.GetTotalMemory(false);
            string startMemoryString = @"Start memory: " + (startMemory) / (1024.0 * 1000.0) + @" Mb";

            IntPtr branchIdxPtr = IntPtr.Zero;
            IntPtr offsetPtr = IntPtr.Zero;

            wrapper.Expect(w => w.ionc_write_1d_mesh_discretisation_points(ref id, ref nwid, ref branchIdxPtr,
                    ref offsetPtr, ref nMeshPoints))
                .IgnoreArguments()
                .OutRef(id, nwid, branchIdxPtr, offsetPtr, nMeshPoints)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Once();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            mocks.ReplayAll();

            int[] branchIdx = new int[nMeshPoints];
            branchIdx[0] = 1;
            double[] offset = new double[nMeshPoints];
            offset[0] = 1;

            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
            var ierr = uGrid1DApi.Write1DMeshDiscretisationPoints(branchIdx, offset);
            Assert.AreEqual(0, ierr);

            // TODO: Something with the memory check. Will it be freed?
            long endMemory = GC.GetTotalMemory(true);
            long leakedMemory = endMemory - startMemory;
            string leakedMemoryString = @"Total memory leak after test: " + (leakedMemory) / (1024.0 * 1000.0) + @" Mb";

            Assert.LessOrEqual(leakedMemory, 130000L, "Memory consumption should not be higher than 130 MB for a text file of size 48MB.");
        }


        [Test]
        public void GetNumberOfMeshDiscretisationPointsInitializedReadyKnownNumberTest()
        {
            uGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", 3);

            mocks.ReplayAll();

            var result = uGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(3, result);
            Assert.AreEqual(3, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));

        }

        [Test]
        public void GetNumberOfMeshDiscretisationPointsUninitializedTest()
        {
            uGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(false).Repeat.Once();
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", 1);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshPoints = 30;
            wrapper.Expect(
                w => w.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints))
                .OutRef(ioncid, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Once();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);

            mocks.ReplayAll();

            var result = uGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(30, result);
            Assert.AreEqual(30, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));

        }
        [Test]
        public void GetNumberOfMeshDiscretisationPointsNotReadyTest()
        {
            uGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(false).Repeat.Once();
            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshPoints = 15;
            wrapper.Expect(
                    w => w.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints))
                .OutRef(ioncid, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Once();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", 1);

            mocks.ReplayAll();

            var result = uGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(15, result);
            Assert.AreEqual(15, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));

        }

        [Test]
        public void GetNumberOfMeshDiscretisationPointsUnknownNumberTest()
        {
            uGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", -1);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshPoints = 10;
            wrapper.Expect(
                    w => w.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints))
                .OutRef(ioncid, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Once();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);
            mocks.ReplayAll();
            
            var result = uGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(10, result);
            Assert.AreEqual(10, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
        }

        [Test]
        public void GetNumberOfMeshDiscretisationPointsErrorTest()
        {
            uGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", -1);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshPoints = -1;
            wrapper.Expect(
                    w => w.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints))
                .OutRef(ioncid, networkId, numberOfMeshPoints).IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR)
                .Repeat.Once();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);
            mocks.ReplayAll();

            var result = uGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
        }

        [Test]
        public void GetNumberOfMeshDiscretisationPointsExceptionTest()
        {
            uGrid1DApi.Expect(a => a.GetNumberOfMeshDiscretisationPoints()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            uGrid1DApi.Expect(a => a.IsInitialized()).Return(true).Repeat.Once();
            uGrid1DApi.Expect(a => a.IsNetworkReady()).Return(true).Repeat.Once();
            TypeUtils.SetField(uGrid1DApi, "nMeshPoints", -1);

            var wrapper = mocks.DynamicMock<IGridWrapper>();

            int ioncid = 1;
            int networkId = 1;
            int numberOfMeshPoints = -1;
            wrapper.Expect(
                    w => w.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkId, ref numberOfMeshPoints))
                .OutRef(ioncid, networkId, numberOfMeshPoints).IgnoreArguments()
                .Throw(new Exception("myTestException"))
                .Repeat.Once();

            TypeUtils.SetField(uGrid1DApi, "wrapper", wrapper);
            mocks.ReplayAll();

            var result = uGrid1DApi.GetNumberOfMeshDiscretisationPoints();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, result);
            Assert.AreEqual(-1, TypeUtils.GetField(uGrid1DApi, "nMeshPoints"));
        }
    }
}
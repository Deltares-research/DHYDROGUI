using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGridApi1DTests
    {
        private IUGridApi1D uGrid1DApi;
        private IUGridApi1D uRemoteGrid1DApi;
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
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Write1DNetworkNodes(new []{0.0}, new[] { 0.0 }, new[] { string.Empty }, new[] { string.Empty }));
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uRemoteGrid1DApi.Write1DNetworkNodes(new []{0.0}, new[] { 0.0 }, new[] { string.Empty }, new[] { string.Empty }));
        }

        [Test]
        public void Write1DNetworkNodesInitializedButArrayNotCorrectTest()
        {
            TypeUtils.SetField(uGrid1DApi, "ioncid", 1);
            TypeUtils.SetField(uGrid1DApi, "networkId", 1);
            
            // check with :
            // node Of numbers = -1; 
            // Length xArray = 1; 
            // Length yArray = 1; 
            // Length idArray = 1; 
            // Length desrcArray = 1; 
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).Return(-1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkNodes(new []{0.0}, new[] { 0.0 }, new[] { string.Empty }, new[] { string.Empty }));

            mocks.BackToRecordAll();
            
            // check with :
            // node Of numbers = 1; 
            // Length xArray = 2; 
            // Length yArray = 1; 
            // Length idArray = 1; 
            // Length desrcArray = 1; 
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
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkNodes()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkNodes(new[] { 0.0 }, new[] { 0.0 }, new[] { string.Empty }, new[] { string.Empty, string.Empty }));
        }

        [Test]
        public void Write1DNetworkBranchesUnInitializedTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR, uGrid1DApi.Write1DNetworkBranches(new []{ 0 }, new[] { 0 }, new []{ 0.0 }, new []{ 0 }, new[] { string.Empty }, new[] { string.Empty }));
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
            TypeUtils.SetField(uGrid1DApi, "ioncid", 1);
            TypeUtils.SetField(uGrid1DApi, "networkId", 1);

            // check with :
            // Branch Of numbers = -1; 
            // Length srcIds = 1; 
            // Length targetIds = 1; 
            // Length lengths = 1; 
            // Length geoPts = 1; 
            // Length idsArray = 1; 
            // Length desrcArray = 1; 
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
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkBranches()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkBranches(new[] { 0 }, new[] { 0 }, new[] { 0.0 }, new[] { 0 }, new[] { string.Empty, string.Empty }, new[] { string.Empty}));

            mocks.BackToRecordAll();
            
            // check with :
            // Branch Of numbers = 1; 
            // Length srcIds = 1; 
            // Length targetIds = 1; 
            // Length lengths = 1; 
            // Length geoPts = 1; 
            // Length idsArray = 1; 
            // Length desrcArray = 2; 
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
            TypeUtils.SetField(uGrid1DApi, "ioncid", 1);
            TypeUtils.SetField(uGrid1DApi, "networkId", 1);

            // check with :
            // node Of numbers = -1; 
            // Length xArray = 1; 
            // Length yArray = 1; 
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).Return(-1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkGeometry(new[] { 0.0 }, new[] { 0.0 }));

            mocks.BackToRecordAll();

            // check with :
            // node Of numbers = 1; 
            // Length xArray = 1; 
            // Length yArray = 1; 
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkGeometry(new[] { 0.0, 0.0 }, new[] { 0.0 }));

            mocks.BackToRecordAll();

            // check with :
            // node Of numbers = 1; 
            // Length xArray = 1; 
            // Length yArray = 2; 
            uGrid1DApi.Expect(a => a.GetNumberOfNetworkGeometryPoints()).Return(1).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR, uGrid1DApi.Write1DNetworkGeometry(new[] { 0.0 }, new[] { 0.0, 0.0 }));
        }

        [Test()]
        public void GetNumberOfNetworkNodesTest()
        {
            mocks.ReplayAll();
        }

        [Test()]
        public void GetNumberOfNetworkBranchesTest()
        {
            mocks.ReplayAll();
        }

        [Test()]
        public void GetNumberOfNetworkGeometryPointsTest()
        {
            mocks.ReplayAll();
        }

        [Test]
        public void NetworkReadyTrueTest()
        {
            mocks.ReplayAll();
            TypeUtils.SetField(uGrid1DApi, "networkId", 1);
            Assert.AreEqual(true, uGrid1DApi.NetworkReady);
        }

        [Test]
        public void NetworkReadyFalseBecauseNetworkIdNotSetTest()
        {
            mocks.ReplayAll();
            TypeUtils.SetField(uGrid1DApi, "networkId", -1);
            Assert.AreEqual(false, uGrid1DApi.NetworkReady);
        }
    }
}
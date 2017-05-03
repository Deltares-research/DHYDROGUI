using System;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    
    [TestFixture]
    public class Ugrid1DTests
    {
        private IUGrid1D uGrid1D;
        private MockRepository mocks;
        
        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            uGrid1D = mocks.DynamicMock<UGrid1D>();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void Create1DNetworkTest()
        {
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a => 
                        a.Create1DNetwork(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)
                      ).IgnoreArguments().Return(0).Repeat.Once();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Once();
            mocks.ReplayAll();

            var networkName = "Network1";
            var numberOfNodes = 1;
            var numberOfBranches = 2;
            var totalNumberOfGeometryPoints = 3;
            uGrid1D.Create1DGridInFile(networkName, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints);
        }
        
        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Couldn't create new 1d network Network1 with number of nodes 1, number of branches 2, number of geometry points 3 because of error number -1")]
        public void Create1DNetworkFailsTest()
        {
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a => 
                        a.Create1DNetwork(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)
                      ).IgnoreArguments().Return(-1).Repeat.Once();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Once();
            mocks.ReplayAll();

            var networkName = "Network1";
            var numberOfNodes = 1;
            var numberOfBranches = 2;
            var totalNumberOfGeometryPoints = 3;
            uGrid1D.Create1DGridInFile(networkName, numberOfNodes, numberOfBranches, totalNumberOfGeometryPoints);
        }

        [Test]
        public void Write1DNetworkNodesTest()
        {
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a =>
                        a.Write1DNetworkNodes(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything)
                      ).IgnoreArguments().Return(0).Repeat.Once();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Once();
            uGrid1D.Expect(g => g.IsInitialized()).Return(true).Repeat.Once();
            mocks.ReplayAll();
            uGrid1D.Write1DNetworkNodes(new []{ 10.0 }, new []{ 5.0 }, new []{ "Node1" }, new []{ string.Empty });
        }

        [Test]
        public void Write1DNetworkNodesButUninitializedTest()
        {
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a =>
                        a.Write1DNetworkNodes(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything)
                      ).IgnoreArguments().Return(0).Repeat.Never();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Never();
            uGrid1D.Expect(g => g.IsInitialized()).Return(false).Repeat.Once();
            mocks.ReplayAll();
            uGrid1D.Write1DNetworkNodes(new []{ 10.0 }, new []{ 5.0 }, new []{ "Node1" }, new []{ string.Empty });
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Couldn't write 1d network nodes because of error number -1")]
        public void Write1DNetworkNodesFailsTest()
        {
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a =>
                        a.Write1DNetworkNodes(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything)
                      ).IgnoreArguments().Return(-1).Repeat.Once();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Once();
            uGrid1D.Expect(g => g.IsInitialized()).Return(true).Repeat.Once();
            mocks.ReplayAll();
            uGrid1D.Write1DNetworkNodes(new []{ 10.0 }, new []{ 5.0 }, new []{ "Node1" }, new []{ string.Empty });
        }

        [Test]
        public void Write1DNetworkBranchesTest()
        {
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a =>
                        a.Write1DNetworkBranches(Arg<int[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything)
                      ).IgnoreArguments().Return(0).Repeat.Once();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Once();
            uGrid1D.Expect(g => g.IsInitialized()).Return(true).Repeat.Once();
            mocks.ReplayAll();

            uGrid1D.Write1DNetworkBranches(new[] { 0 }, new[] { 1 }, new[] { 100.0 }, new[] { 3 }, new[] { "Branch1" }, new[] { string.Empty });
        }

        [Test]
        public void Write1DNetworkBranchesUninitializedTest()
        {
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a =>
                        a.Write1DNetworkBranches(Arg<int[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything)
                      ).IgnoreArguments().Return(0).Repeat.Never();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Never();
            uGrid1D.Expect(g => g.IsInitialized()).Return(false).Repeat.Once();
            mocks.ReplayAll();

            uGrid1D.Write1DNetworkBranches(new[] { 0 }, new[] { 1 }, new[] { 100.0 }, new[] { 3 }, new[] { "Branch1" }, new[] { string.Empty });
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Couldn't write 1d network branches because of error number -1")]
        public void Write1DNetworkBranchesFailsTest()
        {
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a =>
                        a.Write1DNetworkBranches(Arg<int[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything)
                      ).IgnoreArguments().Return(-1).Repeat.Once();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Once();
            uGrid1D.Expect(g => g.IsInitialized()).Return(true).Repeat.Once();
            mocks.ReplayAll();

            uGrid1D.Write1DNetworkBranches(new[] { 0 }, new[] { 1 }, new[] { 100.0 }, new[] { 3 }, new[] { "Branch1" }, new[] { string.Empty });
        }

        [Test]
        public void Write1DNetworkGeometryTest()
        {
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a =>
                        a.Write1DNetworkGeometry(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything)
                      ).IgnoreArguments().Return(0).Repeat.Once();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Once();
            uGrid1D.Expect(g => g.IsInitialized()).Return(true).Repeat.Once();
            mocks.ReplayAll();

            uGrid1D.Write1DNetworkGeometry(new[] { 10.0 }, new[] { 5.0 });
        }

        [Test]
        public void Write1DNetworkGeometryUninitializedTest()
        {
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a =>
                        a.Write1DNetworkGeometry(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything)
                      ).IgnoreArguments().Return(0).Repeat.Never();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Never();
            uGrid1D.Expect(g => g.IsInitialized()).Return(false).Repeat.Once();
            mocks.ReplayAll();

            uGrid1D.Write1DNetworkGeometry(new[] { 10.0 }, new[] { 5.0 });
        }

        [Test]
        public void GetNumberOfNetworkNodesNoApiSetTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(-1, uGrid1D.GetNumberOfNetworkNodes());
        }

        [Test]
        public void GetNumberOfNetworkNodesTest()
        {
            var nrOfNodes = 801;
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a => a.GetNumberOfNetworkNodes()).Return(nrOfNodes).Repeat.Once();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(nrOfNodes, uGrid1D.GetNumberOfNetworkNodes());
        }

        [Test]
        public void GetNumberOfNetworkBranchesNoApiSetTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(-1, uGrid1D.GetNumberOfNetworkBranches());
        }

        [Test]
        public void GetNumberOfNetworkBranchesTest()
        {
            var nrOfBranches = 801;
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a => a.GetNumberOfNetworkBranches()).Return(nrOfBranches).Repeat.Once();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(nrOfBranches, uGrid1D.GetNumberOfNetworkBranches());
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsNoApiSetTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(-1, uGrid1D.GetNumberOfNetworkGeometryPoints());
        }

        [Test]
        public void GetNumberOfNetworkGeometryPointsTest()
        {
            var nrOfGeometryPoints = 801;
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a => a.GetNumberOfNetworkGeometryPoints()).Return(nrOfGeometryPoints).Repeat.Once();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(nrOfGeometryPoints, uGrid1D.GetNumberOfNetworkGeometryPoints());
        }
        /*
        [Test]
        public void IsInitialized()
        {
            var api = mocks.DynamicMock<IUGridApi1D>();
            api.Expect(a => a.NetworkReady).Return(true).Repeat.Once();
            api.Expect(a => a.Initialized).Return(true).Repeat.Once();

            uGrid1D.Expect(g => g.GridApi).Return(api).Repeat.Once();
            mocks.ReplayAll();

            var isInitialized = uGrid1D.IsInitialized();
            Assert.IsTrue(isInitialized);
        }*/

    }
}

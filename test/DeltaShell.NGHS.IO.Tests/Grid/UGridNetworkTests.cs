using System;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGridNetworkTests
    {
        private const string UGRID_TEST_FILE = @"ugrid\Dummy.nc";
        private MockRepository mocks;
        private IUGridNetworkApi uGridNetworkApi;
        private UGridNetwork gridNetwork;
        private const string standardErrorMessage = " because of error number: -1";
        private int errorValue = -1;
        private int noErrorValue = GridApiDataSet.GridConstants.IONC_NOERR;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uGridNetworkApi = mocks.DynamicMock<IUGridNetworkApi>();
            gridNetwork = new UGridNetwork(TestHelper.GetTestFilePath(UGRID_TEST_FILE))
            {
                GridApi = uGridNetworkApi
            };
            SetExpectanciesSuchThatGridNetworkApiIsValid();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        private void SetExpectanciesSuchThatGridNetworkApiIsValid()
        {
            uGridNetworkApi.Expect(api => api.Initialized).Return(true).Repeat.Any();
            uGridNetworkApi.Expect(api => api.GetConvention()).Return(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID).Repeat
                .Once();
            uGridNetworkApi.Expect(api => api.GetVersion()).Return(GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION).Repeat
                .Once();
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't create new network ", MatchType = MessageMatch.StartsWith)]
        public void WhenInvokingCreateNetworkInFileAndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkApi.Expect(api => api.CreateNetwork(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.CreateNetworkInFile(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
        }

        [Test]
        public void WhenInvokingCreateNetworkInFileAndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            uGridNetworkApi.Expect(api => api.CreateNetwork(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.CreateNetworkInFile(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything, out Arg<int>.Out(1).Dummy);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't write network nodes" + standardErrorMessage)]
        public void WhenInvoking_WriteNetworkNodes_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkApi.Expect(api => api.WriteNetworkNodes(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.WriteNetworkNodes(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_WriteNetworkNodes_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            uGridNetworkApi.Expect(api => api.WriteNetworkNodes(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.WriteNetworkNodes(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't write network branches" + standardErrorMessage)]
        public void WhenInvoking_WriteNetworkBranches_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkApi.Expect(api => api.WriteNetworkBranches(Arg<int[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.WriteNetworkBranches(Arg<int[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_WriteNetworkBranches_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            uGridNetworkApi.Expect(api => api.WriteNetworkBranches(Arg<int[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.WriteNetworkBranches(Arg<int[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<int[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't write network geometry" + standardErrorMessage)]
        public void WhenInvoking_WriteNetworkGeometry_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkApi.Expect(api => api.WriteNetworkGeometry(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.WriteNetworkGeometry(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_WriteNetworkGeometry_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            uGridNetworkApi.Expect(api => api.WriteNetworkGeometry(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.WriteNetworkGeometry(Arg<double[]>.Is.Anything, Arg<double[]>.Is.Anything);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't obtain the network name" + standardErrorMessage)]
        public void WhenInvoking_GetNetworkName_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkApi.Expect(api => api.GetNetworkName(Arg<int>.Is.Anything, out Arg<string>.Out("MyNetwork").Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.GetNetworkName(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetNetworkName_AndApiReturnsNoErrorValueThenReturnNameValue()
        {
            var name = "MyNetwork";
            uGridNetworkApi.Expect(api => api.GetNetworkName(Arg<int>.Is.Anything, out Arg<string>.Out(name).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var networkName = gridNetwork.GetNetworkName(Arg<int>.Is.Anything);
            Assert.That(networkName, Is.EqualTo(name));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get number of network nodes" + standardErrorMessage)]
        public void WhenInvoking_GetNumberOfNetworkNodes_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkApi.Expect(api => api.GetNumberOfNetworkNodes(Arg<int>.Is.Anything, out Arg<int>.Out(52).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.GetNumberOfNetworkNodes(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetNumberOfNetworkNodes_AndApiReturnsNoErrorValueThenReturnValue()
        {
            var nNodes = 52;
            uGridNetworkApi.Expect(api => api.GetNumberOfNetworkNodes(Arg<int>.Is.Anything, out Arg<int>.Out(nNodes).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var numberOfNetworkNodes = gridNetwork.GetNumberOfNetworkNodes(Arg<int>.Is.Anything);
            Assert.That(numberOfNetworkNodes, Is.EqualTo(nNodes));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get the number of network branches" + standardErrorMessage)]
        public void WhenInvoking_GetNumberOfNetworkBranches_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkApi.Expect(api => api.GetNumberOfNetworkBranches(Arg<int>.Is.Anything, out Arg<int>.Out(33).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.GetNumberOfNetworkBranches(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetNumberOfNetworkBranches_AndApiReturnsNoErrorValueThenReturnValue()
        {
            var nBranches = 33;
            uGridNetworkApi.Expect(api => api.GetNumberOfNetworkBranches(Arg<int>.Is.Anything, out Arg<int>.Out(nBranches).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var numberOfBranches = gridNetwork.GetNumberOfNetworkBranches(Arg<int>.Is.Anything);
            Assert.That(numberOfBranches, Is.EqualTo(nBranches));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get the number of network geometry points" + standardErrorMessage)]
        public void WhenInvoking_GetNumberOfNetworkGeometryPoints_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkApi.Expect(api => api.GetNumberOfNetworkGeometryPoints(Arg<int>.Is.Anything, out Arg<int>.Out(33).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.GetNumberOfNetworkGeometryPoints(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetNumberOfNetworkGeometryPoints_AndApiReturnsNoErrorValueThenReturnValue()
        {
            var nGeometryPoints = 33;
            uGridNetworkApi.Expect(api => api.GetNumberOfNetworkGeometryPoints(Arg<int>.Is.Anything, out Arg<int>.Out(nGeometryPoints).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var numberOfBranches = gridNetwork.GetNumberOfNetworkGeometryPoints(Arg<int>.Is.Anything);
            Assert.That(numberOfBranches, Is.EqualTo(nGeometryPoints));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't read network nodes" + standardErrorMessage)]
        public void WhenInvoking_ReadNetworkNodes_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkApi.Expect(api => api.ReadNetworkNodes(Arg<int>.Is.Anything, out Arg<double[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.ReadNetworkNodes(Arg<int>.Is.Anything, out Arg<double[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy);
        }

        [Test]
        public void WhenInvoking_ReadNetworkNodes_AndApiReturnsNoErrorValueThenMethodCompletes()
        {
            uGridNetworkApi.Expect(api => api.ReadNetworkNodes(Arg<int>.Is.Anything, out Arg<double[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.ReadNetworkNodes(Arg<int>.Is.Anything, out Arg<double[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't read network branches" + standardErrorMessage)]
        public void WhenInvoking_ReadNetworkBranches_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkApi.Expect(api => api.ReadNetworkBranches(Arg<int>.Is.Anything, out Arg<int[]>.Out(null).Dummy, out Arg<int[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy, out Arg<int[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.ReadNetworkBranches(Arg<int>.Is.Anything, out Arg<int[]>.Out(null).Dummy, out Arg<int[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy, out Arg<int[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy);
        }

        [Test]
        public void WhenInvoking_ReadNetworkBranches_AndApiReturnsNoErrorValueThenMethodCompletes()
        {
            uGridNetworkApi.Expect(api => api.ReadNetworkBranches(Arg<int>.Is.Anything, out Arg<int[]>.Out(null).Dummy, out Arg<int[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy, out Arg<int[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.ReadNetworkBranches(Arg<int>.Is.Anything, out Arg<int[]>.Out(null).Dummy, out Arg<int[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy, out Arg<int[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't read network geometry" + standardErrorMessage)]
        public void WhenInvoking_ReadNetworkGeometry_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkApi.Expect(api => api.ReadNetworkGeometry(Arg<int>.Is.Anything, out Arg<double[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.ReadNetworkGeometry(Arg<int>.Is.Anything, out Arg<double[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy);
        }

        [Test]
        public void WhenInvoking_ReadNetworkGeometry_AndApiReturnsNoErrorValueThenMethodCompletes()
        {
            uGridNetworkApi.Expect(api => api.ReadNetworkGeometry(Arg<int>.Is.Anything, out Arg<double[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetwork.ReadNetworkGeometry(Arg<int>.Is.Anything, out Arg<double[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy);
        }
    }
}
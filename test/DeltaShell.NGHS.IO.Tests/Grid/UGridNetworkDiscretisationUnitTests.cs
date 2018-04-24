using System;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGridNetworkDiscretisationUnitTests
    {
        private const string UGRID_TEST_FILE = @"ugrid\Dummy.nc";
        private MockRepository mocks;
        private IUGridNetworkDiscretisationApi uGridNetworkDiscretisationApi;
        private UGridNetworkDiscretisation gridNetworkDiscretisation;
        private int errorValue = -1;
        private int noErrorValue = GridApiDataSet.GridConstants.NOERR;
        private const string standardErrorMessage = ", because of error number: -1";

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uGridNetworkDiscretisationApi = mocks.DynamicMock<IUGridNetworkDiscretisationApi>();
            gridNetworkDiscretisation = new UGridNetworkDiscretisation(TestHelper.GetTestFilePath(UGRID_TEST_FILE))
            {
                GridApi = uGridNetworkDiscretisationApi
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
            uGridNetworkDiscretisationApi.Expect(api => api.Initialized).Return(true).Repeat.Any();
            uGridNetworkDiscretisationApi.Expect(api => api.GetConvention()).Return(GridApiDataSet.DataSetConventions.CONV_UGRID).Repeat
                .Once();
            uGridNetworkDiscretisationApi.Expect(api => api.GetVersion()).Return(GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION).Repeat
                .Once();
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't create new network in", MatchType = MessageMatch.StartsWith)]
        public void WhenInvoking_CreateNetworkDiscretisationInFile_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkDiscretisationApi.Expect(api => api.CreateNetworkDiscretisation(Arg<int>.Is.Anything))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetworkDiscretisation.CreateNetworkDiscretisationInFile(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_CreateNetworkDiscretisationInFile_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            uGridNetworkDiscretisationApi.Expect(api => api.CreateNetworkDiscretisation(Arg<int>.Is.Anything))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetworkDiscretisation.CreateNetworkDiscretisationInFile(Arg<int>.Is.Anything);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't write the network discretisation points" + standardErrorMessage)]
        public void WhenInvoking_WriteNetworkDiscretisationPoints_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkDiscretisationApi.Expect(api => api.WriteNetworkDiscretisationPoints(Arg<int[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetworkDiscretisation.WriteNetworkDiscretisationPoints(Arg<int[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_WriteNetworkDiscretisationPoints_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            uGridNetworkDiscretisationApi.Expect(api => api.WriteNetworkDiscretisationPoints(Arg<int[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetworkDiscretisation.WriteNetworkDiscretisationPoints(Arg<int[]>.Is.Anything, Arg<double[]>.Is.Anything, Arg<string[]>.Is.Anything, Arg<string[]>.Is.Anything);
        }
        
        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get the mesh discretisation name" + standardErrorMessage)]
        public void WhenInvoking_GetNetworkDiscretisationName_AndApiReturnsAnErrorValueThenThrowException()
        {
            string name = "MyName";
            uGridNetworkDiscretisationApi.Expect(api => api.GetNetworkDiscretisationName(Arg<int>.Is.Anything, out Arg<string>.Out(name).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetworkDiscretisation.GetNetworkDiscretisationNameForMeshId(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetNetworkDiscretisationName_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            string name = "MyName";
            uGridNetworkDiscretisationApi.Expect(api => api.GetNetworkDiscretisationName(Arg<int>.Is.Anything, out Arg<string>.Out(name).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var nName = gridNetworkDiscretisation.GetNetworkDiscretisationNameForMeshId(Arg<int>.Is.Anything);
            Assert.That(nName, Is.EqualTo(name));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get the network Id corresponding to the network discretisation" + standardErrorMessage)]
        public void WhenInvoking_GetNetworkId_AndApiReturnsAnErrorValueThenThrowException()
        {
            int id = 1;
            uGridNetworkDiscretisationApi.Expect(api => api.GetNetworkIdFromMeshId(Arg<int>.Is.Anything, out Arg<int>.Out(id).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetworkDiscretisation.GetNetworkIdForMeshId(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetNetworkId_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int id = 1;
            uGridNetworkDiscretisationApi.Expect(api => api.GetNetworkIdFromMeshId(Arg<int>.Is.Anything, out Arg<int>.Out(id).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var networkId = gridNetworkDiscretisation.GetNetworkIdForMeshId(Arg<int>.Is.Anything);
            Assert.That(networkId, Is.EqualTo(id));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get the number of network discretisations" + standardErrorMessage)]
        public void WhenInvoking_GetNumberOfNetworkDiscretisations_AndApiReturnsAnErrorValueThenThrowException()
        {
            int nDiscretisations = 33;
            uGridNetworkDiscretisationApi.Expect(api => api.GetNumberOfMeshByType(Arg<UGridMeshType>.Is.Anything, out Arg<int>.Out(nDiscretisations).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetworkDiscretisation.GetNumberOfNetworkDiscretisations();
        }

        [Test]
        public void WhenInvoking_GetNumberOfNetworkDiscretisations_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int nDiscretisations = 33;
            uGridNetworkDiscretisationApi.Expect(api => api.GetNumberOfMeshByType(Arg<UGridMeshType>.Is.Anything, out Arg<int>.Out(nDiscretisations).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var nNetworkDiscretisation = gridNetworkDiscretisation.GetNumberOfNetworkDiscretisations();
            Assert.That(nNetworkDiscretisation, Is.EqualTo(nDiscretisations));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get the network discretisation IDs" + standardErrorMessage)]
        public void WhenInvoking_GetNetworkDiscretisationIds_AndApiReturnsAnErrorValueThenThrowException()
        {
            int[] ids = {1, 1, 2, 3, 5, 8};
            uGridNetworkDiscretisationApi.Expect(api => api.GetMeshIdsByMeshType(Arg<UGridMeshType>.Is.Anything, Arg<int>.Is.Anything, out Arg<int[]>.Out(ids).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetworkDiscretisation.GetNetworkDiscretisationIds(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetNetworkDiscretisationIds_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int[] ids = { 1, 1, 2, 3, 5, 8 };
            uGridNetworkDiscretisationApi.Expect(api => api.GetMeshIdsByMeshType(Arg<UGridMeshType>.Is.Anything, Arg<int>.Is.Anything, out Arg<int[]>.Out(ids).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var networkDiscretisationIds = gridNetworkDiscretisation.GetNetworkDiscretisationIds(Arg<int>.Is.Anything);
            Assert.That(networkDiscretisationIds, Is.EqualTo(ids));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get the number of network discretisation points" + standardErrorMessage)]
        public void WhenInvoking_GetNumberOfNetworkDiscretisationPoints_AndApiReturnsAnErrorValueThenThrowException()
        {
            int nPoints = 33;
            uGridNetworkDiscretisationApi.Expect(api => api.GetNumberOfNetworkDiscretisationPoints(Arg<int>.Is.Anything, out Arg<int>.Out(nPoints).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetworkDiscretisation.GetNumberOfNetworkDiscretisationPointsForMeshId(Arg<int>.Is.Anything);
        }

        [Test]
        public void WhenInvoking_GetNumberOfNetworkDiscretisationPoints_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            int nPoints = 33;
            uGridNetworkDiscretisationApi.Expect(api => api.GetNumberOfNetworkDiscretisationPoints(Arg<int>.Is.Anything, out Arg<int>.Out(nPoints).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            var numberOfDiscretisationPoints = gridNetworkDiscretisation.GetNumberOfNetworkDiscretisationPointsForMeshId(Arg<int>.Is.Anything);
            Assert.That(numberOfDiscretisationPoints, Is.EqualTo(nPoints));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't read the network discretisation points" + standardErrorMessage)]
        public void WhenInvoking_ReadNetworkDiscretisationPoints_AndApiReturnsAnErrorValueThenThrowException()
        {
            uGridNetworkDiscretisationApi.Expect(api => api.ReadNetworkDiscretisationPoints(Arg<int>.Is.Anything, out Arg<int[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy))
                .Return(errorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetworkDiscretisation.ReadNetworkDiscretisationPointsForMeshId(Arg<int>.Is.Anything, out Arg<int[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy);
        }

        [Test]
        public void WhenInvoking_ReadNetworkDiscretisationPoints_AndApiReturnsNoErrorValueThenMethodCompletesWithoutErrors()
        {
            uGridNetworkDiscretisationApi.Expect(api => api.ReadNetworkDiscretisationPoints(Arg<int>.Is.Anything, out Arg<int[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy))
                .Return(noErrorValue).Repeat.Once();

            mocks.ReplayAll();

            gridNetworkDiscretisation.ReadNetworkDiscretisationPointsForMeshId(Arg<int>.Is.Anything, out Arg<int[]>.Out(null).Dummy, out Arg<double[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy, out Arg<string[]>.Out(null).Dummy);
        }
    }
}
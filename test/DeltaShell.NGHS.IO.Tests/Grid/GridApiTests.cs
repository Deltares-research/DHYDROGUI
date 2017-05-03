using System;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class GridApiTests
    {
        private IGridApi gridApi;
        private IGridApi remoteGridApi;
        private MockRepository mocks;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            gridApi = mocks.DynamicMock<GridApi>();
            remoteGridApi = mocks.DynamicMock<RemoteGridApi>();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void GetConventionWithNullOrEmptyStringFileNameTest()
        {
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, gridApi.GetConvention(null));
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, gridApi.GetConvention(string.Empty));
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, remoteGridApi.GetConvention(null));
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, remoteGridApi.GetConvention(string.Empty));
        }

        [Test]
        public void GetConventionFailsButSucceedInFallBackTest()
        {
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .WhenCalled(invocation =>
                    { throw new Exception("test") ; });
            gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything)).Return(GridApiDataSet.DataSetConventions.IONC_CONV_TEST).Repeat.Once();
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_TEST, gridApi.GetConvention("test"));
        }
        
        [Test]
        public void GetConventionFailsAndFailsInFallBackTest()
        {
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .WhenCalled(invocation =>
                    { throw new Exception("test") ; });
            gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                .WhenCalled(invocation =>
                { throw new Exception("test2"); });
            mocks.ReplayAll();
            TestHelper.AssertLogMessageIsGenerated(() =>
            {
                Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, gridApi.GetConvention("test"));
            }, "Couldn't open nc grid file : test to determine what the convention in the nc file was. test2test"); 

        }

        [Test()]
        public void adherestoConventionsTest()
        {

        }

        [Test()]
        public void OpenTest()
        {

        }

        [Test()]
        public void CloseTest()
        {

        }

        [Test()]
        public void GetMeshCountTest()
        {

        }

        [Test()]
        public void GetCoordinateSystemCodeTest()
        {

        }

        [Test()]
        public void GetConventionTest1()
        {

        }

        [Test()]
        public void GetVersionTest()
        {

        }

        [Test()]
        public void InitializeTest()
        {

        }
    }
}
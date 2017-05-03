using System;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using log4net.Core;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

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
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_NULL, remoteGridApi.GetConvention(null));
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_NULL, remoteGridApi.GetConvention(string.Empty));
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
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
            LogHelper.ConfigureLogging(Level.Warn);
            try
            {
                gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                    .WhenCalled(invocation =>
                    { throw new Exception("Opening in with UGrid format failed."); });
                gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                    .WhenCalled(invocation =>
                    { throw new Exception("test2"); });
                mocks.ReplayAll();
                TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, gridApi.GetConvention("test"));
                }, "Couldn't open nc grid file : test to determine what the convention in the nc file was. Method 'GridApi.GetConventionViaDSFramework(anything);' requires a return value or an exception to throw. Opening in with UGrid format failed.");
            }
            finally
            {
                LogHelper.ResetLogging();
            }
        }

        [Test]
        public void GetConventionAgainViaLegacyWay()
        {
            
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .WhenCalled(invocation => { TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.IONC_CONV_NULL); }).Repeat.Once();
            gridApi.Expect(a => a.Close())
                .WhenCalled(invocation => { }).Repeat.Once();
            gridApi.Expect(a => ((GridApi) a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, gridApi.GetConvention("test"));
        }

        [Test]
        public void GetConventionClosingFails()
        {
            LogHelper.ConfigureLogging(Level.Warn);

            try
            {
                gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .WhenCalled(invocation => { TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.IONC_CONV_TEST);  }).Repeat.Once();
                gridApi.Expect(a => a.Close())
                    .WhenCalled(invocation => { throw new Exception("Closing failed"); }).Repeat.Once();
                
                mocks.ReplayAll();
                TestHelper.AssertLogMessageIsGenerated(() =>
                    {
                        Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_TEST, gridApi.GetConvention("test"));
                    },
                    "Closing failed");
            }
            finally
            {
                LogHelper.ResetLogging();
            }
            
        }

        [Test]
        public void GetConventionWithToLowConvensionNumber()
        {

            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .WhenCalled(invocation => { TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.IONC_CONV_UGRID); }).Repeat.Once();
            gridApi.Expect(a => a.Close())
                .WhenCalled(invocation => { }).Repeat.Once();
            gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, gridApi.GetConvention("test"));
        }

        [Test]
        public void GetConvention()
        {

            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .WhenCalled(invocation =>
                {
                    TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.IONC_CONV_UGRID);
                    TypeUtils.SetField(gridApi, "convversion", 1.0d);
                }).Repeat.Once();
            gridApi.Expect(a => a.Close())
                .WhenCalled(invocation => { }).Repeat.Once();
            gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID, gridApi.GetConvention("test"));
        }

        [Test]
        public void GetConventionTestWithoutFilename()
        {
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.IONC_CONV_UGRID);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_NULL, gridApi.GetConvention());
            TypeUtils.SetField(gridApi, "ioncid", 1);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID, gridApi.GetConvention());
        }

        [Test]
        public void adherestoConventionsTest()
        {
            mocks.ReplayAll();
            Assert.IsTrue(gridApi.adherestoConventions(GridApiDataSet.DataSetConventions.IONC_CONV_NULL));
            Assert.IsFalse(gridApi.adherestoConventions(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER));
            // cannot mock GridWrapper.ionc_adheresto_conventions
        }

        [Test]
        public void OpenTest()
        {
            mocks.ReplayAll();
            // cannot mock GridWrapper.ionc_open
        }

        [Test]
        public void CloseTest()
        {
            mocks.ReplayAll();
            // cannot mock GridWrapper.ionc_close
        }

        [Test]
        public void GetMeshCountTest()
        {
            mocks.ReplayAll();
            // cannot mock GridWrapper.ionc_get_mesh_count
        }

        [Test]
        public void GetCoordinateSystemCodeTest()
        {
            mocks.ReplayAll();
            // cannot mock GridWrapper.ionc_get_coordinate_system
        }

        [Test]
        public void GetVersionTest()
        {
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "convversion", 1.0d);
            Assert.AreEqual(double.NaN, gridApi.GetVersion(), 0.001d);
            TypeUtils.SetField(gridApi, "ioncid", 1);
            Assert.AreEqual(1.0d, gridApi.GetVersion(), 0.001d);
        }

        [Test]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void InitializeTest(int id, bool expectation)
        {
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "ioncid", id);
            Assert.AreEqual(expectation, gridApi.Initialized);
        }
    }
}
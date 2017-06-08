using System;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
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
            gridApi.Expect(a => a.Initialized)
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
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
            gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything)).Return(GridApiDataSet.DataSetConventions.IONC_CONV_TEST).Repeat.Twice();
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_TEST, gridApi.GetConvention("test"));
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_TEST, remoteGridApi.GetConvention("test"));
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
                TypeUtils.SetField(remoteGridApi, "api", gridApi);
                mocks.ReplayAll();
                TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, gridApi.GetConvention("test"));
                }, "Couldn't open nc grid file : test to determine what the convention in the nc file was. Method 'GridApi.GetConventionViaDSFramework(anything);' requires a return value or an exception to throw. Opening in with UGrid format failed.");

                TestHelper.AssertLogMessageIsGenerated(() =>
                {
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, remoteGridApi.GetConvention("test"));
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
                .WhenCalled(invocation => { TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.IONC_CONV_NULL); }).Repeat.Twice();
            gridApi.Expect(a => a.Close())
                .WhenCalled(invocation => { }).Repeat.Twice();
            gridApi.Expect(a => ((GridApi) a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, gridApi.GetConvention("test"));
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, remoteGridApi.GetConvention("test"));
        }

        [Test]
        public void GetConventionClosingFails()
        {
            LogHelper.ConfigureLogging(Level.Warn);

            try
            {
                gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .WhenCalled(invocation => { TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.IONC_CONV_TEST);  }).Repeat.Twice();
                gridApi.Expect(a => a.Close())
                    .WhenCalled(invocation => { throw new Exception("Closing failed"); }).Repeat.Twice();
                TypeUtils.SetField(remoteGridApi, "api", gridApi);
                mocks.ReplayAll();
                TestHelper.AssertLogMessageIsGenerated(() =>
                    {
                        Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_TEST, gridApi.GetConvention("test"));
                    },
                    "Closing failed");
                TestHelper.AssertLogMessageIsGenerated(() =>
                    {
                        Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_TEST, remoteGridApi.GetConvention("test"));
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
                .WhenCalled(invocation => { TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.IONC_CONV_UGRID); }).Repeat.Twice();
            gridApi.Expect(a => a.Close())
                .WhenCalled(invocation => { }).Repeat.Twice();
            gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, gridApi.GetConvention("test"));
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, remoteGridApi.GetConvention("test"));
        }

        [Test]
        public void GetConvention()
        {

            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .WhenCalled(invocation =>
                {
                    TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.IONC_CONV_UGRID);
                    TypeUtils.SetField(gridApi, "convversion", GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION);
                }).Repeat.Twice();
            gridApi.Expect(a => a.Close())
                .WhenCalled(invocation => { }).Repeat.Twice();
            gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID, gridApi.GetConvention("test"));
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID, remoteGridApi.GetConvention("test"));
        }

        [Test]
        public void GetConventionTestWithoutFilename()
        {
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.IONC_CONV_UGRID);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_NULL, gridApi.GetConvention());
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_NULL, remoteGridApi.GetConvention());
            TypeUtils.SetField(gridApi, "ioncid", 1);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID, gridApi.GetConvention());
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID, remoteGridApi.GetConvention());
        }

        [Test]
        public void adherestoConventionsTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int ioncid = 0;
            int iconvtype = (int) GridApiDataSet.DataSetConventions.IONC_CONV_NULL;

            wrapper.Expect(w =>
                    w.ionc_adheresto_conventions(
                        ref ioncid,
                        ref iconvtype))
                .IgnoreArguments()
                .OutRef(0, 0)
                .Return(true)
                .Repeat.Never();
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            Assert.IsTrue(gridApi.adherestoConventions(GridApiDataSet.DataSetConventions.IONC_CONV_NULL));
            Assert.IsTrue(remoteGridApi.adherestoConventions(GridApiDataSet.DataSetConventions.IONC_CONV_NULL));
            Assert.IsFalse(gridApi.adherestoConventions(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER));
            Assert.IsFalse(remoteGridApi.adherestoConventions(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER));

            mocks.BackToRecordAll();
        
            wrapper.Expect(w =>
                    w.ionc_adheresto_conventions(
                    ref ioncid,
                    ref iconvtype))
                .IgnoreArguments()
                .OutRef(1, 0)
                .Return(true)
                .Repeat.Twice();
            gridApi.Expect(a => a.Initialized)
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncid", 1);
            mocks.ReplayAll();

            Assert.IsTrue(gridApi.adherestoConventions(GridApiDataSet.DataSetConventions.IONC_CONV_TEST));
            Assert.IsTrue(remoteGridApi.adherestoConventions(GridApiDataSet.DataSetConventions.IONC_CONV_TEST));
        }

        [Test]
        public void OpenWithoutErrorButToLowConvensionVersionTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int mode = (int)GridApiDataSet.NetcdfOpenMode.nf90_nowrite;
            int ioncid = 0 ;
            int iconvtype = (int)GridApiDataSet.DataSetConventions.IONC_CONV_NULL;
            double l = 0.0d;
            wrapper.Expect(w =>
                    w.ionc_open("",
                    ref mode,
                    ref ioncid,
                    ref iconvtype,
                    ref l))
                .IgnoreArguments()
                .OutRef(0, 0, (int)GridApiDataSet.DataSetConventions.IONC_CONV_UGRID, 0.9d)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            gridApi.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, TypeUtils.GetField(gridApi, "iconvtype"));
            TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.IONC_CONV_TEST);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_TEST, TypeUtils.GetField(gridApi, "iconvtype"));

            remoteGridApi.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, TypeUtils.GetField(gridApi, "iconvtype"));
        }

        [Test]
        public void OpenWithoutErrorTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int mode = (int)GridApiDataSet.NetcdfOpenMode.nf90_nowrite;
            int ioncid = 0 ;
            int iconvtype = (int)GridApiDataSet.DataSetConventions.IONC_CONV_NULL;
            double l = 0.0d;
            wrapper.Expect(w =>
                    w.ionc_open("",
                    ref mode,
                    ref ioncid,
                    ref iconvtype,
                    ref l))
                .IgnoreArguments()
                .OutRef(0, 0, (int)GridApiDataSet.DataSetConventions.IONC_CONV_UGRID, GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION)
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            gridApi.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID, TypeUtils.GetField(gridApi, "iconvtype"));
            TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.IONC_CONV_TEST);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_TEST, TypeUtils.GetField(gridApi, "iconvtype"));

            remoteGridApi.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID, TypeUtils.GetField(gridApi, "iconvtype"));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't open grid nc file : test.nc because of err nr : -1000")]
        public void OpenWithErrorTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            int j = 0 ;
            int k = 0;
            double l = 0.0d;

            wrapper.Expect(w =>
                    w.ionc_open("",
                            ref i,
                            ref j,
                            ref k,
                            ref l))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            gridApi.Open("test.nc", Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't open grid nc file : test.nc because of err nr : -1000")]
        public void OpenInRemoteWithErrorTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            int j = 0 ;
            int k = 0;
            double l = 0.0d;

            wrapper.Expect(w =>
                    w.ionc_open("",
                            ref i,
                            ref j,
                            ref k,
                            ref l))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            remoteGridApi.Open("test.nc", Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
        }

        [Test]
        public void CloseUninitializedTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            wrapper.Expect(w =>
                    w.ionc_close(ref i))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)
                .Repeat.Never();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Close())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            gridApi.Close();
            remoteGridApi.Close();
        }

        [Test]
        public void CloseTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            wrapper.Expect(w =>
                    w.ionc_close(ref i))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .Repeat.Twice();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Close())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "ioncid", 1);
            gridApi.Close();
            Assert.AreEqual(0, TypeUtils.GetField(gridApi, "ioncid"));
            TypeUtils.SetField(gridApi, "ioncid", 1);
            Assert.AreEqual(1, TypeUtils.GetField(gridApi, "ioncid"));
            remoteGridApi.Close();
            Assert.AreEqual(0, TypeUtils.GetField(gridApi, "ioncid"));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't close grid nc file because of err nr : -1000")]
        public void CloseWithErrorTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            wrapper.Expect(w =>
                    w.ionc_close(ref i))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Close())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "ioncid", 1);
            gridApi.Close();
            Assert.AreEqual(1, TypeUtils.GetField(gridApi, "ioncid"));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't close grid nc file because of err nr : -1000")]
        public void CloseInRemoteWithErrorTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            wrapper.Expect(w =>
                    w.ionc_close(ref i))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Close())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "ioncid", 1);

            remoteGridApi.Close();
            Assert.AreEqual(1, TypeUtils.GetField(gridApi, "ioncid"));
        }

        [Test]
        public void GetMeshCountUninitializedTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            int j = 0;
            
            wrapper.Expect(w =>
                    w.ionc_get_mesh_count(
                            ref i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .OutRef(0, 10)
                .Repeat.Never();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            Assert.AreEqual(0, gridApi.GetMeshCount());
            Assert.AreEqual(0, remoteGridApi.GetMeshCount());
        }

        [Test]
        public void GetMeshCountTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            int j = 0;
            
            wrapper.Expect(w =>
                    w.ionc_get_mesh_count(
                            ref i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .OutRef(1, 10)
                .Repeat.Twice();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncid", 1);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            Assert.AreEqual(10, gridApi.GetMeshCount());
            Assert.AreEqual(10, remoteGridApi.GetMeshCount());
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get number of meshes because of err nr : -1000")]
        public void GetMeshCountWithExceptionTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            int j = 0;
            
            wrapper.Expect(w =>
                    w.ionc_get_mesh_count(
                            ref i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)
                .OutRef(0, 10)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncid", 1);
            mocks.ReplayAll();
            Assert.AreEqual(10, gridApi.GetMeshCount());
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get number of meshes because of err nr : -1000")]
        public void GetMeshCountInRemoteWithExceptionTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            int j = 0;
            
            wrapper.Expect(w =>
                    w.ionc_get_mesh_count(
                            ref i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)
                .OutRef(0, 10)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncid", 1);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            Assert.AreEqual(10, remoteGridApi.GetMeshCount());
        }

        [Test]
        public void GetCoordinateSystemCodeUninitializedTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            int j = 0;

            wrapper.Expect(w =>
                    w.ionc_get_coordinate_system(
                            ref i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .OutRef(0, 2887)
                .Repeat.Never();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            Assert.AreEqual(0, gridApi.GetCoordinateSystemCode());
            Assert.AreEqual(0, remoteGridApi.GetCoordinateSystemCode());
        }

        [Test]
        public void GetCoordinateSystemCodeTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            int j = 0;

            wrapper.Expect(w =>
                    w.ionc_get_coordinate_system(
                            ref i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_NOERR)
                .OutRef(1, 2887)
                .Repeat.Twice();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncid", 1);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            Assert.AreEqual(2887, gridApi.GetCoordinateSystemCode());
            Assert.AreEqual(2887, remoteGridApi.GetCoordinateSystemCode());
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get coordinate system code because of err nr : -1000")]
        public void GetCoordinateSystemCodeWithExceptionTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            int j = 0;

            wrapper.Expect(w =>
                    w.ionc_get_coordinate_system(
                            ref i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)
                .OutRef(0, 2887)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncid", 1);
            mocks.ReplayAll();
            Assert.AreEqual(2887, gridApi.GetCoordinateSystemCode());
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't get coordinate system code because of err nr : -1000")]
        public void GetCoordinateSystemCodeInRemoteWithExceptionTest()
        {
            var wrapper = mocks.DynamicMock<IGridWrapper>();
            int i = 0;
            int j = 0;

            wrapper.Expect(w =>
                    w.ionc_get_coordinate_system(
                            ref i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)
                .OutRef(0, 2887)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncid", 1);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            Assert.AreEqual(2887, remoteGridApi.GetCoordinateSystemCode());
        }

        [Test]
        public void GetVersionTest()
        {
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "convversion", GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION);
            Assert.AreEqual(double.NaN, gridApi.GetVersion(), 0.001d);
            Assert.AreEqual(double.NaN, remoteGridApi.GetVersion(), 0.001d);
            TypeUtils.SetField(gridApi, "ioncid", 1);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            Assert.AreEqual(GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION, gridApi.GetVersion(), 0.001d);
            Assert.AreEqual(GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION, remoteGridApi.GetVersion(), 0.001d);
        }

        [Test]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void InitializeTest(int id, bool expectation)
        {
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "ioncid", id);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            Assert.AreEqual(expectation, gridApi.Initialized);
            Assert.AreEqual(expectation, remoteGridApi.Initialized);
        }

        [Test]
        public void CreateEmptyNetCdfFileTest()
        {
            // Create GridWrapper and set it as field for gridApi
            var wrapper = new GridWrapper();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);

            // Construct a filePath and make sure it does not exist anymore
            string c_path = TestHelper.GetTestFilePath(@"ugrid\emptyWrite1d.nc");
            c_path = TestHelper.CreateLocalCopy(c_path);
            FileUtils.DeleteIfExists(c_path);
            Assert.IsFalse(File.Exists(c_path));

            gridApi.Expect(api => api.Close()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            
            // Create the nc-file and close the connection to it
            var metaData = new UGridGlobalMetaData("My Model", "My Source", "1.0");
            gridApi.CreateFile(c_path, metaData);
            Assert.IsTrue(File.Exists(c_path));
            gridApi.Close();
        }

        [Test]
        [ExpectedException(typeof(AccessViolationException))]
        public void CreateFileNullExceptionTest()
        {
            var wrapper = new GridWrapper();
            mocks.ReplayAll();

            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            var metaData = new UGridGlobalMetaData("My Model", "My Source", "1.0");
            gridApi.CreateFile(null, metaData);
        }

        [Test]
        [TestCase(GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR)]
        [TestCase(GridApiDataSet.GridConstants.IONC_GENERAL_ARRAY_LENGTH_FATAL_ERR)]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Couldn't create new NetCDF file at location", MatchType = MessageMatch.Contains)]
        public void CreateFileFailedTest(int apiCallReturnValue)
        {
            var wrapper = mocks.StrictMock<IGridWrapper>();
            int mode = (int)GridApiDataSet.NetcdfOpenMode.nf90_nowrite;
            int ioncid = 0;
            int iconvtype = 0;

            wrapper.Expect(a => a.ionc_create("", ref mode, ref ioncid))
                .IgnoreArguments()
                .OutRef(0, 0)
                .Return(apiCallReturnValue).Repeat.Once();
            mocks.ReplayAll();

            TypeUtils.SetField(gridApi, "wrapper", wrapper);

            gridApi.CreateFile(Arg<string>.Is.Anything, Arg<UGridGlobalMetaData>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
        }
    }
}
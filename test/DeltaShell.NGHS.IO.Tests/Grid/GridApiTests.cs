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
        private GridApi gridApi;
        private RemoteGridApi remoteGridApi;
        private MockRepository mocks;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            gridApi = mocks.DynamicMock<GridApi>();
            remoteGridApi = mocks.DynamicMock<RemoteGridApi>();
            gridApi.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);
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
            GridApiDataSet.DataSetConventions convention;

            gridApi.GetConvention(null, out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);

            gridApi.GetConvention(string.Empty, out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);

            remoteGridApi.GetConvention(null, out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_NULL, convention);

            remoteGridApi.GetConvention(string.Empty, out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_NULL, convention);

            TypeUtils.SetField(remoteGridApi, "api", gridApi);

            remoteGridApi.GetConvention(null, out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);

            remoteGridApi.GetConvention(string.Empty, out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);
        }

        [Test]
        public void GetConventionFailsButSucceedInFallBackTest()
        {
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR);
            gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything)).Return(GridApiDataSet.DataSetConventions.CONV_TEST).Repeat.Twice();
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();

            GridApiDataSet.DataSetConventions convention;
            gridApi.GetConvention("test", out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_TEST, convention);

            remoteGridApi.GetConvention("test", out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_TEST, convention);
        }

        [Test]
        public void GetConventionFailsAndFailsInFallBackTest()
        {
                gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                    .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR);
                gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                    .WhenCalled(invocation =>
                    { throw new Exception("test2"); });
                TypeUtils.SetField(remoteGridApi, "api", gridApi);
                mocks.ReplayAll();

                GridApiDataSet.DataSetConventions convention;
                gridApi.GetConvention("test", out convention);
                Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);

                remoteGridApi.GetConvention("test", out convention);
                Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);
        }

        [Test]
        public void GetConventionAgainViaLegacyWay()
        {
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .WhenCalled(invocation => { TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_NULL); }).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            gridApi.Expect(a => a.Close())
                .WhenCalled(invocation => { }).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            GridApiDataSet.DataSetConventions convention;
            gridApi.GetConvention("test", out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);

            remoteGridApi.GetConvention("test", out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);
        }

        [Test]
        public void GetConventionClosingFails()
        {
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
            .WhenCalled(invocation => { TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_TEST); }).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            gridApi.Expect(a => a.Close()).Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                .Repeat.Twice();
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();

            GridApiDataSet.DataSetConventions convention;
            gridApi.GetConvention("test", out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_TEST, convention);

            remoteGridApi.GetConvention("test", out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_TEST, convention);
        }

        [Test]
        public void GetConventionWithToLowConvensionNumber()
        {

            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .WhenCalled(invocation => { TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_UGRID); }).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            gridApi.Expect(a => a.Close()).Return(GridApiDataSet.GridConstants.NOERR)
                .WhenCalled(invocation => { }).Repeat.Twice();
            gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();

            GridApiDataSet.DataSetConventions convention;
            gridApi.GetConvention("test", out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);

            remoteGridApi.GetConvention("test", out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);
        }

        [Test]
        public void GetConvention()
        {

            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .WhenCalled(invocation =>
                {
                    TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_UGRID);
                    TypeUtils.SetField(gridApi, "convversion", GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION);
                }).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
            gridApi.Expect(a => a.Close()).Return(GridApiDataSet.GridConstants.NOERR)
                .WhenCalled(invocation => { }).Repeat.Twice();
            gridApi.Expect(a => ((GridApi)a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();

            GridApiDataSet.DataSetConventions convention;
            gridApi.GetConvention("test", out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_UGRID, convention);

            remoteGridApi.GetConvention("test", out convention);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_UGRID, convention);
        }

        [Test]
        public void GetConventionTestWithoutFilename()
        {
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_UGRID);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_NULL, gridApi.GetConvention());
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_NULL, remoteGridApi.GetConvention());
            TypeUtils.SetField(gridApi, "ioncId", 1);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_UGRID, gridApi.GetConvention());
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_UGRID, remoteGridApi.GetConvention());
        }

        [Test]
        public void adherestoConventionsTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int ioncId = 0;
            int iconvtype = (int)GridApiDataSet.DataSetConventions.CONV_NULL;

            wrapper.Expect(w =>
                    w.AdherestoConventions(
                        ioncId,
                        iconvtype))
                .IgnoreArguments()
                .OutRef(0, 0)
                .Return(true)
                .Repeat.Never();
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            Assert.IsTrue(gridApi.adherestoConventions(GridApiDataSet.DataSetConventions.CONV_NULL));
            Assert.IsTrue(remoteGridApi.adherestoConventions(GridApiDataSet.DataSetConventions.CONV_NULL));
            Assert.IsFalse(gridApi.adherestoConventions(GridApiDataSet.DataSetConventions.CONV_OTHER));
            Assert.IsFalse(remoteGridApi.adherestoConventions(GridApiDataSet.DataSetConventions.CONV_OTHER));

            mocks.BackToRecordAll();

            wrapper.Expect(w =>
                    w.AdherestoConventions(
                    ioncId,
                    iconvtype))
                .IgnoreArguments()
                .OutRef(1, 0)
                .Return(true)
                .Repeat.Twice();
            gridApi.Expect(a => a.Initialized)
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncId", 1);
            mocks.ReplayAll();

            Assert.IsTrue(gridApi.adherestoConventions(GridApiDataSet.DataSetConventions.CONV_TEST));
            Assert.IsTrue(remoteGridApi.adherestoConventions(GridApiDataSet.DataSetConventions.CONV_TEST));
        }

        [Test]
        public void OpenWithoutErrorButToLowConvensionVersionTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int mode = (int)GridApiDataSet.NetcdfOpenMode.nf90_nowrite;
            int ioncId = 0;
            int iconvtype = (int)GridApiDataSet.DataSetConventions.CONV_NULL;
            double l = 0.0d;
            wrapper.Expect(w =>
                    w.Open("",
                    mode,
                    ref ioncId,
                    ref iconvtype,
                    ref l))
                .IgnoreArguments()
                .OutRef(0, (int)GridApiDataSet.DataSetConventions.CONV_UGRID, 0.9d)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            gridApi.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, TypeUtils.GetField(gridApi, "iconvtype"));
            TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_TEST);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_TEST, TypeUtils.GetField(gridApi, "iconvtype"));

            remoteGridApi.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, TypeUtils.GetField(gridApi, "iconvtype"));
        }

        [Test]
        public void OpenWithoutErrorTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int mode = (int)GridApiDataSet.NetcdfOpenMode.nf90_nowrite;
            int ioncId = 0;
            int iconvtype = (int)GridApiDataSet.DataSetConventions.CONV_NULL;
            double l = 0.0d;
            wrapper.Expect(w =>
                    w.Open("",
                    mode,
                    ref ioncId,
                    ref iconvtype,
                    ref l))
                .IgnoreArguments()
                .OutRef( 0, (int)GridApiDataSet.DataSetConventions.CONV_UGRID, GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            gridApi.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_UGRID, TypeUtils.GetField(gridApi, "iconvtype"));
            TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_TEST);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_TEST, TypeUtils.GetField(gridApi, "iconvtype"));

            remoteGridApi.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_UGRID, TypeUtils.GetField(gridApi, "iconvtype"));
        }

        [Test]
        public void OpenWithErrorTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            int j = 0;
            int k = 0;
            double l = 0.0d;

            wrapper.Expect(w =>
                    w.Open("",
                            i,
                            ref j,
                            ref k,
                            ref l))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            var ierr = gridApi.Open("test.nc", Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.AreEqual(-1000, ierr);
        }

        [Test]
        public void OpenInRemoteWithErrorTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            int j = 0;
            int k = 0;
            double l = 0.0d;

            wrapper.Expect(w =>
                    w.Open("",
                            i,
                            ref j,
                            ref k,
                            ref l))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            var ierr = remoteGridApi.Open("test.nc", Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.AreEqual(-1000, ierr);
        }

        [Test]
        public void CloseUninitializedTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            wrapper.Expect(w =>
                    w.Close(i))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
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
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            wrapper.Expect(w =>
                    w.Close(i))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Twice();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Close())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "ioncId", 1);
            gridApi.Close();
            Assert.AreEqual(0, TypeUtils.GetField(gridApi, "ioncId"));
            TypeUtils.SetField(gridApi, "ioncId", 1);
            Assert.AreEqual(1, TypeUtils.GetField(gridApi, "ioncId"));
            remoteGridApi.Close();
            Assert.AreEqual(0, TypeUtils.GetField(gridApi, "ioncId"));
        }

        [Test]
        public void CloseWithErrorTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            wrapper.Expect(w =>
                    w.Close(i))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Close())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "ioncId", 1);
            var ierr = gridApi.Close();
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(1, TypeUtils.GetField(gridApi, "ioncId"));
        }

        [Test]
        public void CloseInRemoteWithErrorTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            wrapper.Expect(w =>
                    w.Close(i))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            gridApi.Expect(a => a.Close())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "ioncId", 1);

            var ierr = remoteGridApi.Close();
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(1, TypeUtils.GetField(gridApi, "ioncId"));
        }

        [Test]
        public void GetMeshCountUninitializedTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            int j = 0;

            wrapper.Expect(w =>
                    w.GetMeshCount(
                            i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.NOERR)
                .OutRef(0, 10)
                .Repeat.Never();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            int numberOfMeshes;
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, gridApi.GetMeshCount(out numberOfMeshes));
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteGridApi.GetMeshCount(out numberOfMeshes));
        }

        [Test]
        public void GetMeshCountTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            int j = 0;

            wrapper.Expect(w =>
                    w.GetMeshCount(
                            i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.NOERR)
                .OutRef(10)
                .Repeat.Twice();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncId", 1);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            int numberOfMeshes;
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, gridApi.GetMeshCount(out numberOfMeshes));
            Assert.AreEqual(10, numberOfMeshes);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, remoteGridApi.GetMeshCount(out numberOfMeshes));
            Assert.AreEqual(10, numberOfMeshes);

        }

        [Test]
        public void GetMeshCountWithExceptionTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            int j = 0;

            wrapper.Expect(w =>
                    w.GetMeshCount(
                            i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                .OutRef(0, 10)
                .Throw(new Exception("exception"))
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncId", 1);
            mocks.ReplayAll();
            int numberOfMeshes;
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, gridApi.GetMeshCount(out numberOfMeshes));
            Assert.AreEqual(0, numberOfMeshes);
        }

        [Test]
        public void GetMeshCountInRemoteWithExceptionTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            int j = 0;

            wrapper.Expect(w =>
                    w.GetMeshCount(
                            i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                .OutRef(0, 10)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncId", 1);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            int numberOfMeshes;
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteGridApi.GetMeshCount(out numberOfMeshes));
        }

        [Test]
        public void GetCoordinateSystemCodeUninitializedTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            int j = 0;

            wrapper.Expect(w =>
                    w.GetCoordinateSystem(
                            i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.NOERR)
                .OutRef(2887)
                .Repeat.Never();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            int coordinateSystemCode;
            var ierr = gridApi.GetCoordinateSystemCode(out coordinateSystemCode);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(0, coordinateSystemCode);

            ierr = remoteGridApi.GetCoordinateSystemCode(out coordinateSystemCode);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(0, coordinateSystemCode);
        }

        [Test]
        public void GetCoordinateSystemCodeTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            int j = 0;

            wrapper.Expect(w =>
                    w.GetCoordinateSystem(
                            i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.NOERR)
                .OutRef(2887)
                .Repeat.Twice();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncId", 1);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            int coordinateSystemCode;
            var ierr = gridApi.GetCoordinateSystemCode(out coordinateSystemCode);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
            Assert.AreEqual(2887, coordinateSystemCode);
            ierr = remoteGridApi.GetCoordinateSystemCode(out coordinateSystemCode);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
            Assert.AreEqual(2887, coordinateSystemCode);
        }

        [Test]
        public void GetCoordinateSystemCodeWithExceptionTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            int j = 0;

            wrapper.Expect(w =>
                    w.GetCoordinateSystem(
                            i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                .Throw(new Exception())
                .OutRef(2887)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncId", 1);
            mocks.ReplayAll();
            int coordinateSystemCode;
            var ierr = gridApi.GetCoordinateSystemCode(out coordinateSystemCode);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(0, coordinateSystemCode);
        }

        [Test]
        public void GetCoordinateSystemCodeInRemoteWithExceptionTest()
        {
            var wrapper = mocks.DynamicMock<GridWrapper>();
            int i = 0;
            int j = 0;

            wrapper.Expect(w =>
                    w.GetCoordinateSystem(
                            i,
                            ref j
                            ))
                .IgnoreArguments()
                .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                .Throw(new Exception())
                .OutRef(2887)
                .Repeat.Once();
            TypeUtils.SetField(gridApi, "wrapper", wrapper);
            TypeUtils.SetField(gridApi, "ioncId", 1);
            TypeUtils.SetField(remoteGridApi, "api", gridApi);
            mocks.ReplayAll();
            int coordinateSystemCode;
            var ierr = remoteGridApi.GetCoordinateSystemCode(out coordinateSystemCode);
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
            Assert.AreEqual(0, coordinateSystemCode);
        }

        [Test]
        public void GetVersionTest()
        {
            mocks.ReplayAll();
            TypeUtils.SetField(gridApi, "convversion", GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION);
            Assert.AreEqual(double.NaN, gridApi.GetVersion(), 0.001d);
            Assert.AreEqual(double.NaN, remoteGridApi.GetVersion(), 0.001d);
            TypeUtils.SetField(gridApi, "ioncId", 1);
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
            TypeUtils.SetField(gridApi, "ioncId", id);
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
        [TestCase(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)]
        [TestCase(GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR)]
        public void CreateFileFailedTest(int apiCallReturnValue)
        {
            var wrapper = mocks.StrictMock<GridWrapper>();
            int mode = (int)GridApiDataSet.NetcdfOpenMode.nf90_nowrite;
            int ioncId = 0;
            int iconvtype = 0;

            wrapper.Expect(a => a.create("", mode, ref ioncId))
                .IgnoreArguments()
                .OutRef(0, 0)
                .Return(apiCallReturnValue).Repeat.Once();
            mocks.ReplayAll();

            TypeUtils.SetField(gridApi, "wrapper", wrapper);

            var ierr = gridApi.CreateFile(Arg<string>.Is.Anything, Arg<UGridGlobalMetaData>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.AreEqual(apiCallReturnValue, ierr);
        }
    }
}
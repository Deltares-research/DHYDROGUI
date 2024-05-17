using System;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Remoting;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class GridApiTests
    {
        private const string ApiVarName = "api";
        private const string WrapperVarName = "wrapper";
        private const string DataSetIdVarName = "ioncId";

        [Test]
        public void GetConventionWithNullOrEmptyStringFileNameTest()
        {
            GridApiDataSet.DataSetConventions convention;
            DoWithMockedGridApi(
                gridApi =>
                {
                    gridApi.GetConvention(null, out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);

                    gridApi.GetConvention(string.Empty, out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);
                },
                remoteGridApi =>
                {
                    object apiValue = TypeUtils.GetField(remoteGridApi, ApiVarName);
                    TypeUtils.SetField(remoteGridApi, ApiVarName, null);

                    remoteGridApi.GetConvention(null, out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_NULL, convention);

                    remoteGridApi.GetConvention(string.Empty, out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_NULL, convention);

                    TypeUtils.SetField(remoteGridApi, ApiVarName, apiValue);
                    remoteGridApi.GetConvention(null, out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);

                    remoteGridApi.GetConvention(string.Empty, out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);
                });
        }

        [Test]
        public void GetConventionFailsButSucceedInFallBackTest()
        {
            GridApiDataSet.DataSetConventions convention;
            DoWithMockedGridApi(
                gridApi =>
                {
                    gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                           .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR);
                    gridApi.Expect(a => ((GridApi) a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                           .Return(GridApiDataSet.DataSetConventions.CONV_TEST).Repeat.Twice();
                    gridApi.GetConvention("test", out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_TEST, convention);
                },
                remoteGridApi =>
                {
                    remoteGridApi.GetConvention("test", out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_TEST, convention);
                });
        }

        [Test]
        public void GetConventionFailsAndFailsInFallBackTest()
        {
            GridApiDataSet.DataSetConventions convention;
            DoWithMockedGridApi(
                gridApi =>
                {
                    gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                           .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR);
                    gridApi.Expect(a => ((GridApi) a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                           .WhenCalled(invocation => { throw new Exception("test2"); });

                    gridApi.GetConvention("test", out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);
                },
                remoteGridApi =>
                {
                    remoteGridApi.GetConvention("test", out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);
                });
        }

        [Test]
        public void GetConventionAgainViaLegacyWay()
        {
            GridApiDataSet.DataSetConventions convention;
            DoWithMockedGridApi(
                gridApi =>
                {
                    gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                           .WhenCalled(invocation => { TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_NULL); }).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
                    gridApi.Expect(a => a.Close())
                           .WhenCalled(invocation => {}).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
                    gridApi.Expect(a => ((GridApi) a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                           .CallOriginalMethod(OriginalCallOptions.NoExpectation);

                    gridApi.GetConvention("test", out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);
                },
                remoteGridApi =>
                {
                    remoteGridApi.GetConvention("test", out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);
                });
        }

        [Test]
        public void GetConventionClosingFails()
        {
            GridApiDataSet.DataSetConventions convention;
            DoWithMockedGridApi(
                gridApi =>
                {
                    gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                           .WhenCalled(invocation => { TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_TEST); }).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
                    gridApi.Expect(a => a.Close()).Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                           .Repeat.Twice();
                    gridApi.GetConvention("test", out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_TEST, convention);
                },
                remoteGridApi =>
                {
                    remoteGridApi.GetConvention("test", out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_TEST, convention);
                });
        }

        [Test]
        public void GetConventionWithToLowConvensionNumber()
        {
            GridApiDataSet.DataSetConventions convention;
            DoWithMockedGridApi(
                gridApi =>
                {
                    gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                           .WhenCalled(invocation => { TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_UGRID); }).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
                    gridApi.Expect(a => a.Close()).Return(GridApiDataSet.GridConstants.NOERR)
                           .WhenCalled(invocation => {}).Repeat.Twice();
                    gridApi.Expect(a => ((GridApi) a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                           .CallOriginalMethod(OriginalCallOptions.NoExpectation);

                    gridApi.GetConvention("test", out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);
                },
                remoteGridApi =>
                {
                    remoteGridApi.GetConvention("test", out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_OTHER, convention);
                });
        }

        [Test]
        public void GetConvention()
        {
            GridApiDataSet.DataSetConventions convention;
            DoWithMockedGridApi(
                gridApi =>
                {
                    gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                           .WhenCalled(invocation =>
                           {
                               TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_UGRID);
                               TypeUtils.SetField(gridApi, "convversion", GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION);
                           }).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Twice();
                    gridApi.Expect(a => a.Close()).Return(GridApiDataSet.GridConstants.NOERR)
                           .WhenCalled(invocation => {}).Repeat.Twice();
                    gridApi.Expect(a => ((GridApi) a).GetConventionViaDSFramework(Arg<string>.Is.Anything))
                           .CallOriginalMethod(OriginalCallOptions.NoExpectation);

                    gridApi.GetConvention("test", out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_UGRID, convention);
                },
                remoteGridApi =>
                {
                    remoteGridApi.GetConvention("test", out convention);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_UGRID, convention);
                });
        }

        [Test]
        public void GetConventionTestWithoutFilename()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_UGRID);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_NULL, gridApi.GetConvention());
                },
                remoteGridApi => { Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_NULL, remoteGridApi.GetConvention()); });
        }

        [Test]
        public void GetConventionTestWithoutFilename_2()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    TypeUtils.SetField(gridApi, "iconvtype", GridApiDataSet.DataSetConventions.CONV_UGRID);
                    TypeUtils.SetField(gridApi, DataSetIdVarName, 1);
                    Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_UGRID, gridApi.GetConvention());
                },
                remoteGridApi => { Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_UGRID, remoteGridApi.GetConvention()); });
        }

        [Test]
        public void OpenWithErrorTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    var j = 0;
                    var k = 0;
                    var l = 0.0d;

                    wrapper.Expect(w =>
                                       w.Open(string.Empty,
                                              i,
                                              ref j,
                                              ref k,
                                              ref l))
                           .IgnoreArguments()
                           .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                           .Repeat.Once();
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                    gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                           .CallOriginalMethod(OriginalCallOptions.NoExpectation);
                    int ierr = gridApi.Open("test.nc", Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
                    Assert.AreEqual(-1000, ierr);
                },
                remoteGridApi => {});
        }

        [Test]
        public void OpenInRemoteWithErrorTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    var j = 0;
                    var k = 0;
                    var l = 0.0d;

                    wrapper.Expect(w =>
                                       w.Open(string.Empty,
                                              i,
                                              ref j,
                                              ref k,
                                              ref l))
                           .IgnoreArguments()
                           .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                           .Repeat.Once();
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                    gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                           .CallOriginalMethod(OriginalCallOptions.NoExpectation);
                },
                remoteGridApi =>
                {
                    int ierr = remoteGridApi.Open("test.nc", Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
                    Assert.AreEqual(-1000, ierr);
                });
        }

        [Test]
        public void CloseUninitializedTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    wrapper.Expect(w =>
                                       w.Close(i))
                           .IgnoreArguments()
                           .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                           .Repeat.Never();
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                    gridApi.Expect(a => a.Close())
                           .CallOriginalMethod(OriginalCallOptions.NoExpectation);
                    gridApi.Close();
                },
                remoteGridApi => { remoteGridApi.Close(); });
        }

        [Test]
        public void CloseTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    wrapper.Expect(w =>
                                       w.Close(i))
                           .IgnoreArguments()
                           .Return(GridApiDataSet.GridConstants.NOERR)
                           .Repeat.Twice();
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                    gridApi.Expect(a => a.Close())
                           .CallOriginalMethod(OriginalCallOptions.NoExpectation);
                    TypeUtils.SetField(gridApi, DataSetIdVarName, 1);
                    gridApi.Close();
                    Assert.AreEqual(0, TypeUtils.GetField(gridApi, DataSetIdVarName));
                    TypeUtils.SetField(gridApi, DataSetIdVarName, 1);
                    Assert.AreEqual(1, TypeUtils.GetField(gridApi, DataSetIdVarName));
                },
                remoteGridApi =>
                {
                    remoteGridApi.Close();
                    object gridApi = TypeUtils.GetField(remoteGridApi, ApiVarName);
                    Assert.AreEqual(0, TypeUtils.GetField(gridApi, DataSetIdVarName));
                });
        }

        [Test]
        public void CloseWithErrorTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    wrapper.Expect(w =>
                                       w.Close(i))
                           .IgnoreArguments()
                           .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                           .Repeat.Once();
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                    gridApi.Expect(a => a.Close())
                           .CallOriginalMethod(OriginalCallOptions.NoExpectation);

                    TypeUtils.SetField(gridApi, DataSetIdVarName, 1);
                    int ierr = gridApi.Close();
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.AreEqual(1, TypeUtils.GetField(gridApi, DataSetIdVarName));
                },
                remoteGridApi => {});
        }

        [Test]
        public void CloseInRemoteWithErrorTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    wrapper.Expect(w =>
                                       w.Close(i))
                           .IgnoreArguments()
                           .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                           .Repeat.Once();
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                    gridApi.Expect(a => a.Close())
                           .CallOriginalMethod(OriginalCallOptions.NoExpectation);
                    TypeUtils.SetField(gridApi, DataSetIdVarName, 1);
                },
                remoteGridApi =>
                {
                    int ierr = remoteGridApi.Close();
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    object gridApi = TypeUtils.GetField(remoteGridApi, ApiVarName);
                    Assert.AreEqual(1, TypeUtils.GetField(gridApi, DataSetIdVarName));
                });
        }

        [Test]
        public void GetMeshCountUninitializedTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    var j = 0;

                    wrapper.Expect(w =>
                                       w.GetMeshCount(
                                           i,
                                           ref j
                                       ))
                           .IgnoreArguments()
                           .Return(GridApiDataSet.GridConstants.NOERR)
                           .OutRef(0, 10)
                           .Repeat.Never();
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);

                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, gridApi.GetMeshCount(out int _));
                },
                remoteGridApi => { Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteGridApi.GetMeshCount(out int _)); });
        }

        [Test]
        public void GetMeshCountTest()
        {
            int numberOfMeshes;
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    var j = 0;

                    wrapper.Expect(w =>
                                       w.GetMeshCount(
                                           i,
                                           ref j
                                       ))
                           .IgnoreArguments()
                           .Return(GridApiDataSet.GridConstants.NOERR)
                           .OutRef(10)
                           .Repeat.Twice();
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                    TypeUtils.SetField(gridApi, DataSetIdVarName, 1);

                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, gridApi.GetMeshCount(out numberOfMeshes));
                    Assert.AreEqual(10, numberOfMeshes);
                },
                remoteGridApi =>
                {
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, remoteGridApi.GetMeshCount(out numberOfMeshes));
                    Assert.AreEqual(10, numberOfMeshes);
                });
        }

        [Test]
        public void GetMeshCountWithExceptionTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    var j = 0;

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
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                    TypeUtils.SetField(gridApi, DataSetIdVarName, 1);

                    int numberOfMeshes;
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, gridApi.GetMeshCount(out numberOfMeshes));
                    Assert.AreEqual(0, numberOfMeshes);
                },
                remoteGridApi => {});
        }

        [Test]
        public void GetMeshCountInRemoteWithExceptionTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    var j = 0;

                    wrapper.Expect(w =>
                                       w.GetMeshCount(
                                           i,
                                           ref j
                                       ))
                           .IgnoreArguments()
                           .Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)
                           .OutRef(0, 10)
                           .Repeat.Once();
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                    TypeUtils.SetField(gridApi, DataSetIdVarName, 1);
                },
                remoteGridApi =>
                {
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, remoteGridApi.GetMeshCount(out int _));
                });
        }

        [Test]
        public void GetCoordinateSystemCodeUninitializedTest()
        {
            int coordinateSystemCode;
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    var j = 0;

                    wrapper.Expect(w =>
                                       w.GetCoordinateSystem(
                                           i,
                                           ref j
                                       ))
                           .IgnoreArguments()
                           .Return(GridApiDataSet.GridConstants.NOERR)
                           .OutRef(2887)
                           .Repeat.Never();
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);

                    int ierr = gridApi.GetCoordinateSystemCode(out coordinateSystemCode);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.AreEqual(0, coordinateSystemCode);
                },
                remoteGridApi =>
                {
                    int ierr = remoteGridApi.GetCoordinateSystemCode(out coordinateSystemCode);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.AreEqual(0, coordinateSystemCode);
                });
        }

        [Test]
        public void GetCoordinateSystemCodeTest()
        {
            int coordinateSystemCode;
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    var j = 0;

                    wrapper.Expect(w =>
                                       w.GetCoordinateSystem(
                                           i,
                                           ref j
                                       ))
                           .IgnoreArguments()
                           .Return(GridApiDataSet.GridConstants.NOERR)
                           .OutRef(2887)
                           .Repeat.Twice();
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                    TypeUtils.SetField(gridApi, DataSetIdVarName, 1);

                    int ierr = gridApi.GetCoordinateSystemCode(out coordinateSystemCode);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.AreEqual(2887, coordinateSystemCode);
                },
                remoteGridApi =>
                {
                    int ierr = remoteGridApi.GetCoordinateSystemCode(out coordinateSystemCode);
                    Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
                    Assert.AreEqual(2887, coordinateSystemCode);
                });
        }

        [Test]
        public void GetCoordinateSystemCodeWithExceptionTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    var j = 0;

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
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                    TypeUtils.SetField(gridApi, DataSetIdVarName, 1);

                    int coordinateSystemCode;
                    int ierr = gridApi.GetCoordinateSystemCode(out coordinateSystemCode);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.AreEqual(0, coordinateSystemCode);
                },
                remoteGridApi => {});
        }

        [Test]
        public void GetCoordinateSystemCodeInRemoteWithExceptionTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var i = 0;
                    var j = 0;

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
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                    TypeUtils.SetField(gridApi, DataSetIdVarName, 1);
                },
                remoteGridApi =>
                {
                    int coordinateSystemCode;
                    int ierr = remoteGridApi.GetCoordinateSystemCode(out coordinateSystemCode);
                    Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR, ierr);
                    Assert.AreEqual(0, coordinateSystemCode);
                });
        }

        [Test]
        public void GetVersionTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    TypeUtils.SetField(gridApi, "convversion", GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION);
                    Assert.AreEqual(double.NaN, gridApi.GetVersion(), 0.001d);

                    TypeUtils.SetField(gridApi, DataSetIdVarName, 1);
                    Assert.AreEqual(GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION, gridApi.GetVersion(), 0.001d);
                },
                remoteGridApi =>
                {
                    object gridApi = TypeUtils.GetField(remoteGridApi, ApiVarName);
                    TypeUtils.SetField(remoteGridApi, ApiVarName, null);
                    Assert.AreEqual(double.NaN, remoteGridApi.GetVersion(), 0.001d);
                    TypeUtils.SetField(remoteGridApi, ApiVarName, gridApi);
                    Assert.AreEqual(GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION, remoteGridApi.GetVersion(), 0.001d);
                });
        }

        [Test]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void InitializeTest(int id, bool expectation)
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    TypeUtils.SetField(gridApi, DataSetIdVarName, id);
                    Assert.AreEqual(expectation, gridApi.Initialized);
                },
                remoteGridApi => { Assert.AreEqual(expectation, remoteGridApi.Initialized); });
        }

        [Test]
        public void CreateEmptyNetCdfFileTest()
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    // Create GridWrapper and set it as field for gridApi
                    var wrapper = new GridWrapper();
                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);

                    // Construct a filePath and make sure it does not exist anymore
                    string c_path = TestHelper.GetTestFilePath(@"ugrid\emptyWrite1d.nc");
                    c_path = TestHelper.CreateLocalCopy(c_path);
                    FileUtils.DeleteIfExists(c_path);
                    Assert.IsFalse(File.Exists(c_path));

                    gridApi.Expect(api => api.Close()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

                    // Create the nc-file and close the connection to it
                    var metaData = new UGridGlobalMetaData("My Model", "My Source", "1.0");
                    gridApi.CreateFile(c_path, metaData);
                    Assert.IsTrue(File.Exists(c_path));
                    gridApi.Close();
                },
                remoteGridApi => {});
        }

        [Test]
        public void CreateFileNullExceptionTest()
        {
            void Test() => DoWithMockedGridApi(gridApi =>
            {
                var wrapper = new GridWrapper();

                TypeUtils.SetField(gridApi, WrapperVarName, wrapper);
                var metaData = new UGridGlobalMetaData("My Model", "My Source", "1.0");
                gridApi.CreateFile(null, metaData);
            }, remoteGridApi => {});

            Assert.That(Test, Throws.InstanceOf<AccessViolationException>());
        }

        [Test]
        [TestCase(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR)]
        [TestCase(GridApiDataSet.GridConstants.GENERAL_ARRAY_LENGTH_FATAL_ERR)]
        public void CreateFileFailedTest(int apiCallReturnValue)
        {
            DoWithMockedGridApi(
                gridApi =>
                {
                    var wrapper = MockRepository.GenerateMock<GridWrapper>();
                    var mode = (int) GridApiDataSet.NetcdfOpenMode.nf90_nowrite;
                    var ioncId = 0;

                    wrapper.Expect(a => a.create(string.Empty, mode, ref ioncId))
                           .IgnoreArguments()
                           .OutRef(0, 0)
                           .Return(apiCallReturnValue).Repeat.Once();

                    TypeUtils.SetField(gridApi, WrapperVarName, wrapper);

                    int ierr = gridApi.CreateFile(Arg<string>.Is.Anything, Arg<UGridGlobalMetaData>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
                    Assert.AreEqual(apiCallReturnValue, ierr);
                },
                remoteGridApi => {});
        }

        private static void DoWithMockedGridApi(Action<GridApi> gridApiAction, Action<RemoteGridApi> remoteGridApiAction)
        {
            var gridApi = MockRepository.GenerateMock<GridApi>();
            var remoteGridApi = MockRepository.GenerateMock<RemoteUGridApi>();

            // get old api field value for disposing (killing remote process)
            var oldApiField = (IGridApi) TypeUtils.GetField(remoteGridApi, ApiVarName);

            TypeUtils.SetField(remoteGridApi, ApiVarName, gridApi);

            // dispose old api instance
            oldApiField.Close();
            RemoteInstanceContainer.RemoveInstance(oldApiField);

            gridApi.Expect(a => a.Initialized).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            gridApiAction?.Invoke(gridApi);
            remoteGridApiAction?.Invoke(remoteGridApi);

            gridApi.Replay();
            remoteGridApi.Replay();

            gridApi.VerifyAllExpectations();
            remoteGridApi.VerifyAllExpectations();
        }
    }
}
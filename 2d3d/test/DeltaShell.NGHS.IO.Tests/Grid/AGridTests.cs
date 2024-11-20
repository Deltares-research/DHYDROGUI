using System;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class AGridTests
    {
        private const string DUMMY_TEST_FILE = @"ugrid\Dummy.nc";
        private AGrid<IGridApi> grid;
        private MockRepository mocks;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            grid = mocks.DynamicMock<AGrid<IGridApi>>();
        }

        [Test]
        public void IsValidTest()
        {
            grid.Expect(g => g.IsValid()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.GridApi).Return(null).Repeat.Once();
            mocks.ReplayAll();
            //no api set, so false
            Assert.IsFalse(grid.IsValid());

            mocks.BackToRecordAll();

            var gridApi = mocks.DynamicMock<IGridApi>();
            gridApi.Expect(a => a.GetConvention())
                   .Return(GridApiDataSet.DataSetConventions.CONV_UGRID)
                   .Repeat.Once();
            gridApi.Expect(a => a.GetVersion())
                   .Return(0.0)
                   .Repeat.Once();
            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Times(3);
            grid.Expect(g => g.IsValid()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            //api set, convention is ok but version is not ok so false
            Assert.IsFalse(grid.IsValid());

            mocks.BackToRecordAll();
            gridApi.Expect(a => a.GetConvention())
                   .Return(GridApiDataSet.DataSetConventions.CONV_UGRID)
                   .Repeat.Once();
            gridApi.Expect(a => a.GetVersion())
                   .Return(GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION)
                   .Repeat.Once();
            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Times(3);
            grid.Expect(g => g.IsValid()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            //api set, convention is ok and version is ok so True
            Assert.IsTrue(grid.IsValid());

            mocks.VerifyAll();
        }

        [Test]
        public void CreateFileTest()
        {
            // Construct a filePath and make sure it does not exist anymore
            string filePath = TestHelper.GetTestFilePath(DUMMY_TEST_FILE);
            filePath = TestHelper.CreateLocalCopy(filePath);
            FileUtils.DeleteIfExists(filePath);
            Assert.IsFalse(File.Exists(filePath));

            // gridApi
            var gridApi = mocks.DynamicMock<IGridApi>();
            gridApi.Expect(a => a.CreateFile(Arg<string>.Is.Anything, Arg<UGridGlobalMetaData>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything)).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();

            // grid
            grid.Expect(g => g.CreateFile()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Once();
            TypeUtils.SetField(grid, "filename", filePath);

            mocks.ReplayAll();

            grid.CreateFile();

            mocks.VerifyAll();
        }

        [Test]
        public void InitializeCouldNotOpenExceptionTest()
        {
            var gridApi = mocks.DynamicMock<IGridApi>();

            grid.Expect(g => g.Initialize()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(false).Repeat.Once();
            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Times(2);

            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything)).Return(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR).Repeat.Once();

            mocks.ReplayAll();

            Assert.That(() => grid.Initialize(), Throws.TypeOf<GridApiException>().With.Message.Contains("Couldn't open grid nc file: "));

            mocks.VerifyAll();
        }

        [Test]
        public void InitializeAndGetCoordinateSystemTest()
        {
            var coordinateSystemCode = 3819;
            var gridApi = mocks.DynamicMock<IGridApi>();

            // grid
            grid.Expect(g => g.Initialize()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(true).Repeat.Once();
            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Times(3);
            ((AGrid<IGridApi>) grid).Expect(g => g.CoordinateSystem)
                                    .CallOriginalMethod(OriginalCallOptions.NoExpectation)
                                    .PropertyBehavior();

            // gridApi
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything)).Return(GridApiDataSet.GridConstants.NOERR)
                   .Repeat.Once();
            gridApi.Expect(a => a.GetCoordinateSystemCode(out coordinateSystemCode)).OutRef(coordinateSystemCode).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();
            TypeUtils.SetField(grid, "disposed", true);

            mocks.ReplayAll();

            grid.Initialize();
            Assert.AreEqual(3819, ((AGrid<IGridApi>) grid).CoordinateSystem.AuthorityCode);
            Assert.IsFalse((bool) TypeUtils.GetField(grid, "disposed"));

            mocks.VerifyAll();
        }

        [Test]
        public void InitializeAndGetCoordinateSystemWithExceptionTest()
        {
            var coordinateSystemCode = 3819;
            var gridApi = mocks.DynamicMock<IGridApi>();

            // grid
            grid.Expect(g => g.Initialize())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(true).Repeat.Twice();
            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Times(3);
            grid.Expect(g => g.CoordinateSystem)
                .CallOriginalMethod(OriginalCallOptions.NoExpectation)
                .PropertyBehavior();

            // gridApi
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything)).Return(GridApiDataSet.GridConstants.NOERR)
                   .Repeat.Once();
            gridApi.Expect(a => a.GetCoordinateSystemCode(out coordinateSystemCode)).OutRef(coordinateSystemCode).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once().Throw(new Exception());
            TypeUtils.SetField(grid, "disposed", true);

            mocks.ReplayAll();

            grid.Initialize();
            Assert.IsNull(grid.CoordinateSystem);
            Assert.IsTrue((bool) TypeUtils.GetField(grid, "disposed"));

            mocks.VerifyAll();
        }

        [Test]
        public void InitializeAndCleanUpWhileDisposedTest()
        {
            grid.Expect(g => g.Initialize())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(true).Repeat.Once();
            TypeUtils.SetField(grid, "disposed", true);
            mocks.ReplayAll();

            grid.Initialize();

            mocks.VerifyAll();
        }

        [Test]
        public void InitializeAndCleanUpWhileNotDisposedTest()
        {
            grid.Expect(g => g.Initialize())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(true);
            TypeUtils.SetField(grid, "disposed", false);
            mocks.ReplayAll();

            grid.Initialize();
            Assert.IsFalse((bool) TypeUtils.GetField(grid, "disposed"));

            mocks.VerifyAll();
        }

        [Test]
        public void InitializeAndCleanUpWhileNotDisposedAndNotInitializedTest()
        {
            grid.Expect(g => g.Initialize())
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            TypeUtils.SetField(grid, "disposed", false);
            mocks.ReplayAll();

            grid.Initialize();
            Assert.IsFalse((bool) TypeUtils.GetField(grid, "disposed"));

            mocks.VerifyAll();
        }

        [Test]
        public void GetDataSetConventionTest()
        {
            var gridApi = mocks.DynamicMock<IGridApi>();

            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Once();
            grid.Expect(g => g.IsInitialized()).Return(true).Repeat.Once();
            grid.Expect(g => g.GetDataSetConvention()).CallOriginalMethod(OriginalCallOptions.NoExpectation);

            gridApi.Expect(a => a.GetConvention()).Return(GridApiDataSet.DataSetConventions.CONV_TEST).Repeat.Once();
            mocks.ReplayAll();

            Assert.AreEqual(GridApiDataSet.DataSetConventions.CONV_TEST, grid.GetDataSetConvention());

            mocks.VerifyAll();
        }

        [Test]
        public void IsInitializedTest()
        {
            var gridApi = mocks.DynamicMock<IGridApi>();
            gridApi.Expect(a => a.Initialized).Return(false).Repeat.Once();
            gridApi.Expect(a => a.Initialized).Return(true).Repeat.Once();

            grid.Expect(g => g.GridApi).Return(null).Repeat.Once();
            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Times(4);
            grid.Expect(g => g.IsInitialized()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            Assert.IsFalse(grid.IsInitialized()); // gridapi = null
            Assert.IsFalse(grid.IsInitialized()); // gridapi != null, but not initialized
            Assert.IsTrue(grid.IsInitialized());  // gridapi != null, initialized

            mocks.VerifyAll();
        }

        [Test]
        public void GivenUgrid_WhenDisposing_ThenInternalGridApiIsNull()
        {
            // Given
            var ugrid = new UGrid("myFile");

            // When
            ugrid.Dispose();

            // Then
            Assert.IsNull(ugrid.GridApi);
            Assert.That(ugrid.IsInitialized(), Is.False);
        }
    }
}
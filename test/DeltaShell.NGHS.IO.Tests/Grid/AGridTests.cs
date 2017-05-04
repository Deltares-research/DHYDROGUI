using System;
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
        private IGrid grid;
        private MockRepository mocks;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            grid = mocks.DynamicMock<AGrid>();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
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
                .Return(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID)
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
                .Return(GridApiDataSet.DataSetConventions.IONC_CONV_UGRID)
                .Repeat.Once();
            gridApi.Expect(a => a.GetVersion())
                .Return(GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION)
                .Repeat.Once();
            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Times(3);
            grid.Expect(g => g.IsValid()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            //api set, convention is ok and version is ok so True
            Assert.IsTrue(grid.IsValid());
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Couldn't open grid nc file : ")]
        public void InitializeCouldNotOpenExceptionTest()
        {
            grid.Expect(g => g.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(false).Repeat.Once();

            var gridApi = mocks.DynamicMock<IGridApi>();
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .Repeat.Once();
            gridApi.Expect(a => a.Initialized).Return(false)
                .Repeat.Once();
            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Times(3);
            mocks.ReplayAll();

            grid.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
        }

        [Test]
        public void InitializeAndGetCoordinateSystemTest()
        {
            grid.Expect(g => g.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(true).Repeat.Once();

            var gridApi = mocks.DynamicMock<IGridApi>();
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .Repeat.Once();
            gridApi.Expect(a => a.Initialized).Return(true)
                .Repeat.Once();
            gridApi.Expect(a => a.GetCoordinateSystemCode()).Return(3819).Repeat.Once();
            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Times(4);
            ((AGrid) grid).Expect(g => g.CoordinateSystem)
                .CallOriginalMethod(OriginalCallOptions.NoExpectation)
                .PropertyBehavior();
            TypeUtils.SetField(grid, "disposed", true);
            mocks.ReplayAll();

            grid.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.AreEqual(3819, ((AGrid) grid).CoordinateSystem.AuthorityCode);
            Assert.IsFalse((bool)TypeUtils.GetField(grid, "disposed"));
        }
        [Test]
        public void InitializeAndGetCoordinateSystemWithExceptionTest()
        {
            grid.Expect(g => g.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(true).Repeat.Twice();

            var gridApi = mocks.DynamicMock<IGridApi>();
            gridApi.Expect(a => a.Open(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .Repeat.Once();
            gridApi.Expect(a => a.Initialized).Return(true)
                .Repeat.Once();
            gridApi.Expect(a => a.GetCoordinateSystemCode()).Return(3819).Repeat.Once().Throw(new Exception());
            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Times(4);
            ((AGrid) grid).Expect(g => g.CoordinateSystem)
                .CallOriginalMethod(OriginalCallOptions.NoExpectation)
                .PropertyBehavior();
            TypeUtils.SetField(grid, "disposed", true);
            mocks.ReplayAll();

            grid.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.IsNull(((AGrid) grid).CoordinateSystem);
            Assert.IsTrue((bool)TypeUtils.GetField(grid, "disposed"));
        }

        [Test]
        public void InitializeAndCleanUpWhileDisposedTest()
        {
            grid.Expect(g => g.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(true).Repeat.Once();
            TypeUtils.SetField(grid, "disposed", true);
            mocks.ReplayAll();

            grid.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
        }

        [Test]
        public void InitializeAndCleanUpWhileNotDisposedTest()
        {
            grid.Expect(g => g.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(true).Repeat.Twice();
            TypeUtils.SetField(grid, "disposed", false);
            mocks.ReplayAll();

            grid.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.IsTrue((bool) TypeUtils.GetField(grid, "disposed"));
        }

        [Test]
        public void InitializeAndCleanUpWhileNotDisposedAndNotInitializedTest()
        {
            grid.Expect(g => g.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(true).Repeat.Once();
            grid.Expect(g => g.IsInitialized()).Return(false).Repeat.Once();
            TypeUtils.SetField(grid, "disposed", false);
            mocks.ReplayAll();

            grid.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.IsTrue((bool) TypeUtils.GetField(grid, "disposed"));
        }

        [Test]
        public void InitializeAndCleanUp()
        {
            grid.Expect(g => g.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(true).Repeat.Twice();
            TypeUtils.SetField(grid, "disposed", false);
            var gridApi = mocks.DynamicMock<IGridApi>();
            gridApi.Expect(a => a.Close()).Repeat.Once();

            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Times(3);
            mocks.ReplayAll();

            grid.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.IsNull(grid.GridApi);
            Assert.IsTrue((bool)TypeUtils.GetField(grid, "disposed"));
        }

        [Test]
        public void InitializeAndCleanUpWithException()
        {
            grid.Expect(g => g.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            grid.Expect(g => g.IsInitialized()).Return(true).Repeat.Twice();
            TypeUtils.SetField(grid, "disposed", false);
            var gridApi = mocks.DynamicMock<IGridApi>();
            gridApi.Expect(a => a.Close()).Repeat.Once().Throw(new Exception());

            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Times(2);
            mocks.ReplayAll();
            
            grid.Initialize(Arg<string>.Is.Anything, Arg<GridApiDataSet.NetcdfOpenMode>.Is.Anything);
            Assert.IsNull(grid.GridApi);
            Assert.IsFalse((bool)TypeUtils.GetField(grid, "disposed"));
        }

        [Test]
        public void GetDataSetConventionTest()
        {
            grid.Expect(g => g.GridApi).Return(null).Repeat.Once();
            grid.Expect(g => g.GetDataSetConvention()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_OTHER, grid.GetDataSetConvention());
            mocks.BackToRecordAll();
            var gridApi = mocks.DynamicMock<IGridApi>();
            gridApi.Expect(a => a.GetConvention()).Return(GridApiDataSet.DataSetConventions.IONC_CONV_TEST).Repeat.Once();

            grid.Expect(g => g.GridApi).Return(gridApi).Repeat.Twice();
            grid.Expect(g => g.GetDataSetConvention()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            mocks.ReplayAll();
            Assert.AreEqual(GridApiDataSet.DataSetConventions.IONC_CONV_TEST, grid.GetDataSetConvention());
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
            Assert.IsTrue(grid.IsInitialized()); // gridapi != null, initialized
        }
    }
}
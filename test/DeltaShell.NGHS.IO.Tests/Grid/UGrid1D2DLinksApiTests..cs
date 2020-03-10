using System;
using System.Runtime.InteropServices;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGrid1D2DLinksApiTests
    {

        private UGrid1D2DLinksApi uGrid1D2DLinksApi;
        private RemoteUGrid1D2DLinksApi remoteUGrid1D2DLinksApi;
        private MockRepository mocks;

        private const string ApiVarName = "api";
        private const string WrapperFieldName = "wrapper";
        private const string fileIdFieldName = "ioncId";
        private const string linkIdFieldName = "meshLinks1D2DIdx";
        private const int fileId = 1; //> 0

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            uGrid1D2DLinksApi = mocks.DynamicMock<UGrid1D2DLinksApi>();
            remoteUGrid1D2DLinksApi = mocks.DynamicMock<RemoteUGrid1D2DLinksApi>();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test, RequiresThread]
        [TestCase(false)]
        [TestCase(true)]
        [Ignore("Seems not to work on the build server!")]
        [Category("ToCheck")]
        public void Create1D2DLinksApiCallTest(bool remote)
        {
            // UGrid1D2DLinksApi
            uGrid1D2DLinksApi.Expect(a => a.Initialized).Return(true).Repeat.Any();
            var wrapper = mocks.StrictMock<GridWrapper>();

            var mesh1DId = 3;
            var mesh2DId = 3;
            var numberOfLinks = 5;

            var type1 = (int)GridApiDataSet.LocationType.UG_LOC_NODE;
            var type2 = (int)GridApiDataSet.LocationType.UG_LOC_FACE;
            var contactsmesh = -1;
            wrapper.Expect(w => w.Create1D2DLinks(Arg<int>.Is.Equal(fileId), ref Arg<int>.Ref(Rhino.Mocks.Constraints.Is.Anything(), 1).Dummy, Arg<string>.Is.Equal(GridApiDataSet.DataSetNames.Links1D2D), Arg<int>.Is.Equal(numberOfLinks), Arg<int>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Equal(type1), Arg<int>.Is.Equal(type2)))
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Repeat.Any();

            wrapper.Expect(
                w => w.GetNumberOfMeshes(Arg<int>.Is.Anything, Arg<int>.Is.Anything, ref Arg<int>.Ref(Rhino.Mocks.Constraints.Is.Anything(), 1).Dummy))
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Any();

            var intPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * 1);
            Marshal.Copy(new []{2}, 0, intPtr, 1);
            wrapper.Expect(
                w => w.GetMeshIds(Arg<int>.Is.Anything, Arg<UGridMeshType>.Is.Anything, ref Arg<IntPtr>.Ref(Rhino.Mocks.Constraints.Is.Anything(),intPtr).Dummy, Arg<int>.Is.Anything))
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Any();

            TypeUtils.SetField(uGrid1D2DLinksApi, fileIdFieldName, fileId);
            TypeUtils.SetField(uGrid1D2DLinksApi, WrapperFieldName, wrapper);

            if (remote)
            {
                TypeUtils.SetField(remoteUGrid1D2DLinksApi, ApiVarName, uGrid1D2DLinksApi);
                remoteUGrid1D2DLinksApi.Expect(a => a.Create1D2DLinks(numberOfLinks)).
                                CallOriginalMethod(OriginalCallOptions.NoExpectation).Repeat.Once();

            }

            mocks.ReplayAll();

            if (remote)
            {
                Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, remoteUGrid1D2DLinksApi.Create1D2DLinks(numberOfLinks));
            }
            else
            {
                Assert.AreEqual(GridApiDataSet.GridConstants.TESTING_ERROR, uGrid1D2DLinksApi.Create1D2DLinks(numberOfLinks));
            }
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Create1D2DLinksExceptionTest(bool remote)
        {

            var wrapper = mocks.StrictMock<GridWrapper>();

            var mesh1DId = 3;
            var mesh2DId = 4;
            var numberOfLinks = 5;

            var type1 = (int)GridApiDataSet.LocationType.UG_LOC_NODE;
            var type2 = (int)GridApiDataSet.LocationType.UG_LOC_FACE;
            var contactsmesh = -1;
            wrapper.Expect(w => w.Create1D2DLinks(fileId, ref contactsmesh, GridApiDataSet.DataSetNames.Links1D2D, numberOfLinks, mesh1DId, mesh2DId, type1, type2))
                .Return(GridApiDataSet.GridConstants.TESTING_ERROR)
                .Throw(new Exception("Haha"))
                .Repeat.Any();

            TypeUtils.SetField(uGrid1D2DLinksApi, WrapperFieldName, wrapper);

            // uGrid1D2DLinksApi
            uGrid1D2DLinksApi.Expect(a => a.Create1D2DLinks(numberOfLinks))
                .CallOriginalMethod(OriginalCallOptions.NoExpectation);

            mocks.ReplayAll();

            // uGrid1D2DLinksApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR,
                uGrid1D2DLinksApi.Create1D2DLinks(numberOfLinks));

            // remoteUGrid1D2DLinksApi
            Assert.AreEqual(GridApiDataSet.GridConstants.GENERAL_FATAL_ERR,
                remoteUGrid1D2DLinksApi.Create1D2DLinks(numberOfLinks));
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Write1D2DLinksApiCallTest(bool remote)
        {
            
            // arrange
            var nLinks = 2;
            var mesh1DIndexes = new[] { 1, 2};
            var mesh2DIndexes = new[] { 3, 4 };
            var linkTypes = new[] { 5, 6 };
            var linkIds = new[] { "link 1", "link 2" };
            var linkLongnames = new[] { "long name", "long name 2" };

            int links1D2DIdx = -1;
            IntPtr mesh1DIndexesXPtr = IntPtr.Zero;
            IntPtr mesh2DIndexesXPtr = IntPtr.Zero;
            IntPtr linkTypesXPtr = IntPtr.Zero;
            IntPtr idsPtr = IntPtr.Zero;
            IntPtr longnamesPtr = IntPtr.Zero;

            uGrid1D2DLinksApi.Expect(a => a.Initialized).Return(true).Repeat.Any();
            uGrid1D2DLinksApi.Expect(a => a.Links1D2DReadyForWritingOrReading).Return(true).Repeat.Any();

            var wrapper = mocks.DynamicMock<GridWrapper>();

            wrapper.Expect(w => w.Write1D2DLinks(fileId, links1D2DIdx, mesh1DIndexesXPtr, mesh2DIndexesXPtr, linkTypesXPtr, idsPtr, longnamesPtr, nLinks))
                .IgnoreArguments()
                .OutRef(mesh1DIndexesXPtr, mesh2DIndexesXPtr, linkTypesXPtr)
                .Return(GridApiDataSet.GridConstants.NOERR)
                .Repeat.Once();

            int outNumberOfLinks;
            int fileIdTmp = fileId;
            uGrid1D2DLinksApi.Expect(a => a.GetNumberOf1D2DLinks(out outNumberOfLinks)).OutRef(nLinks).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            wrapper.Expect(w => w.GetNumberOf1D2DLinks(ref fileIdTmp, ref links1D2DIdx, ref nLinks)).IgnoreArguments().OutRef(fileIdTmp, links1D2DIdx, nLinks).Return(GridApiDataSet.GridConstants.NOERR).Repeat.Once();


            TypeUtils.SetField(uGrid1D2DLinksApi, fileIdFieldName, fileId);
            TypeUtils.SetField(uGrid1D2DLinksApi, linkIdFieldName, 2);
            TypeUtils.SetField(uGrid1D2DLinksApi, WrapperFieldName, wrapper);

            if (remote)
            {
                TypeUtils.SetField(remoteUGrid1D2DLinksApi, ApiVarName, uGrid1D2DLinksApi);
                remoteUGrid1D2DLinksApi.Expect(a => a.Write1D2DLinks(mesh1DIndexes, mesh2DIndexes, linkTypes, linkIds, linkLongnames, nLinks)).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            }

            mocks.ReplayAll();

            if (remote)
            {
                var remoteResult = remoteUGrid1D2DLinksApi.Write1D2DLinks(mesh1DIndexes, mesh2DIndexes, linkTypes, linkIds, linkLongnames, nLinks);
                Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, remoteResult);
            }
            else
            {
                var result = uGrid1D2DLinksApi.Write1D2DLinks(mesh1DIndexes, mesh2DIndexes, linkTypes, linkIds, linkLongnames, nLinks);
                Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, result);
            }
            
        }

    }
}

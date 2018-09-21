using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils.Interop;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    public class Grid1d2dTest
    {
        const double MissingValue = -999.0;

        static Grid1d2dTest()
        {
            NativeLibrary.LoadNativeDll(GridApiDataSet.GRIDDLL_NAME, DimrApiDataSet.SharedDllPath);
            NativeLibrary.LoadNativeDll(GridGeomApi.LIB_DLL_NAME, DimrApiDataSet.SharedDllPath);
        }
        
        [Test]
        public void Create1D2DLinksForAreaCompleteMesh1D()
        {
            var caseMaking1D2DLinks = new TestCaseMaking1D2DLinks();

            //mesh1D coordinates
            //meshXCoords = { -6, 5, 23, 34 };
            //meshYCoords = { 22, 16, 16, 7 };
            var areaX = new double[] { -10.0, -10.0, 40.0, 40.0, -10.0 };
            var areaY = new double[] { 30.0, 0.0, 0.0, 30.0, 30.0 };

            //expected links
            var expectedLinksFrom1DIndexes = new int[] { 2, 8, 7 };
            var expectedLinksTo2DIndexes = new int[] { 2, 3, 4 };

            var linkIndexes = caseMaking1D2DLinks.MakeEmbeddedOrLateralLinksForAreaAndReturnFromToOfAllLInks(areaX, areaY);

            //check links
            Assert.AreEqual(expectedLinksFrom1DIndexes.Length, linkIndexes.Count);

            if (linkIndexes.Count == expectedLinksFrom1DIndexes.Length)
            {
                for (int i = 0; i < linkIndexes.Count; i++)
                {
                    Assert.That(linkIndexes[i].Item1, Is.EqualTo(expectedLinksFrom1DIndexes[i]));
                    Assert.That(linkIndexes[i].Item2, Is.EqualTo(expectedLinksTo2DIndexes[i]));
                }
            }
        }

        [Test]
        public void Create1D2DLinksForAreaPartialMesh1D()
        {
            var caseMaking1D2DLinks = new TestCaseMaking1D2DLinks();

            //mesh1D coordinates
            //meshXCoords = { -6, 5, 23, 34 };
            //meshYCoords = { 22, 16, 16, 7 };
            var areaX = new double[] { 4.0, 4.0, 6.0, 6.0, 4.0 };
            var areaY = new double[] { 17.0, 15.0, 15.0, 17.0, 17.0 };

            //expected links
            var expectedLinksFrom1DIndexes = new int[] { 2 };
            var expectedLinksTo2DIndexes = new int[] { 2 };

            var linkIndexes = caseMaking1D2DLinks.MakeEmbeddedOrLateralLinksForAreaAndReturnFromToOfAllLInks(areaX, areaY);

            //check links
            Assert.AreEqual(expectedLinksFrom1DIndexes.Length, linkIndexes.Count);

            if (linkIndexes.Count == expectedLinksFrom1DIndexes.Length)
            {
                for (int i = 0; i < linkIndexes.Count; i++)
                {
                    Assert.That(linkIndexes[i].Item1, Is.EqualTo(expectedLinksFrom1DIndexes[i]));
                    Assert.That(linkIndexes[i].Item2, Is.EqualTo(expectedLinksTo2DIndexes[i]));
                }
            }
        }

        [Test]
        public void Create1D2DLinksForAreaNotAPartOfTheMesh1D()
        {
            var caseMaking1D2DLinks = new TestCaseMaking1D2DLinks();

            //mesh1D coordinates
            //meshXCoords = { -6, 5, 23, 34 };
            //meshYCoords = { 22, 16, 16, 7 };
            var areaX = new double[] { 100.0, 100.0, 140.0, 140.0, 100.0 };
            var areaY = new double[] { 130.0, 100.0, 100.0, 130.0, 130.0 };

            var linkIndexes = caseMaking1D2DLinks.MakeEmbeddedOrLateralLinksForAreaAndReturnFromToOfAllLInks(areaX, areaY);

            //check links
            Assert.AreEqual(0, linkIndexes.Count);
        }

        [Test]
        public void Get1d2dLinksFromApi()
        {
            string netFilePath = TestHelper.GetTestFilePath(@"flow1d2dLinks\SimpleModel\2d_ugrid_net.nc");
            Assert.IsTrue(File.Exists(netFilePath));
            netFilePath = TestHelper.CreateLocalCopy(netFilePath);
            Assert.IsTrue(File.Exists(netFilePath));

            // 0.1 Set Network Discretization.
            var testNetwork = new HydroNetwork();
            WaterFlowFMTestHelper.ConfigureDemoNetworkAtGivenCoordinates(testNetwork);
            Assert.NotNull(testNetwork);

            var testNetworkDiscretization = new Discretization
            {
                Name = WaterFlowFMModel.DiscretizationObjectName,
                Network = testNetwork,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };
            Assert.NotNull(testNetworkDiscretization);

            var offSet = new double[] { 0, 5, 10, 36.8337027874097 };
            HydroNetworkHelper.GenerateDiscretization(testNetworkDiscretization, (IChannel)testNetwork.Branches[0], offSet);

            var geomWrapper = new GridGeomApi();
            var linksFrom = new List<int>();
            var linksTo = new List<int>();
            var startIndex = 0;
            var linksCount = 0;
            var ierr = geomWrapper.Get1D2DLinksFrom1DTo2D(netFilePath, testNetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
            Assert.AreNotEqual(0, linksCount);
            Assert.IsNotEmpty(linksFrom);
            Assert.IsNotEmpty(linksTo);
        }

        [Test]
        public void CreateGullyLinks()
        {
            var caseMaking1D2DLinks = new TestCaseMaking1D2DLinks();

            //mesh1D coordinates
            //meshXCoords = { -6, 5, 23, 34 };
            //meshYCoords = { 22, 16, 16, 7 };
            var geoX = new double[] { 10.0 , MissingValue};
            var geoY = new double[] { 10.0, MissingValue};

            //expected links
            var expectedLinksFrom1DIndexes = new int[] { 2 };
            var expectedLinksTo2DIndexes = new int[] { 2 };

            var linkIndexes = caseMaking1D2DLinks.MakeGullyOrRoofLinksForAreaAndReturnFromToOfAllLInks(geoX, geoY);

            //check links
            Assert.AreEqual(expectedLinksFrom1DIndexes.Length, linkIndexes.Count);

            if (linkIndexes.Count == expectedLinksFrom1DIndexes.Length)
            {
                for (int i = 0; i < linkIndexes.Count; i++)
                {
                    Assert.That(linkIndexes[i].Item1, Is.EqualTo(expectedLinksFrom1DIndexes[i]));
                    Assert.That(linkIndexes[i].Item2, Is.EqualTo(expectedLinksTo2DIndexes[i]));
                }
            }
        }

        [Test]
        public void CreateRoofLinks()
        {
            var caseMaking1D2DLinks = new TestCaseMaking1D2DLinks();

            //mesh1D coordinates
            //meshXCoords = { -6, 5, 23, 34 };
            //meshYCoords = { 22, 16, 16, 7 };
            var geoX = new double[] { 10.0, 15.0, 15.0, 10.0, MissingValue, 20.0, 25.0, 25.0, 20.0, MissingValue };
            var geoY = new double[] { 10.0, 10.0, 15.0, 10.0, MissingValue, 20.0, 20.0, 25.0, 20.0, MissingValue };

            //expected links
            var expectedLinksFrom1DIndexes = new int[] { 2 };
            var expectedLinksTo2DIndexes = new int[] { 2 };

            var linkIndexes = caseMaking1D2DLinks.MakeGullyOrRoofLinksForAreaAndReturnFromToOfAllLInks(geoX, geoY, false);

            //check links
            Assert.AreEqual(expectedLinksFrom1DIndexes.Length, linkIndexes.Count);

            if (linkIndexes.Count == expectedLinksFrom1DIndexes.Length)
            {
                for (int i = 0; i < linkIndexes.Count; i++)
                {
                    Assert.That(linkIndexes[i].Item1, Is.EqualTo(expectedLinksFrom1DIndexes[i]));
                    Assert.That(linkIndexes[i].Item2, Is.EqualTo(expectedLinksTo2DIndexes[i]));
                }
            }
        }

        [Test]
        [Ignore("For some reason it hangs.")]
        public void Get1d2dLinksShouldNotCrashWhenRunningTwice()
        {
            string netFilePath = TestHelper.GetTestFilePath(@"flow1d2dLinks\SimpleModel\2d_ugrid_net.nc");
            Assert.IsTrue(File.Exists(netFilePath));
            netFilePath = TestHelper.CreateLocalCopy(netFilePath);
            Assert.IsTrue(File.Exists(netFilePath));

            // 0.1 Set Network Discretization.
            var testNetwork = new HydroNetwork();
            WaterFlowFMTestHelper.ConfigureDemoNetworkAtGivenCoordinates(testNetwork);
            Assert.NotNull(testNetwork);

            var testNetworkDiscretization = new Discretization
            {
                Name = WaterFlowFMModel.DiscretizationObjectName,
                Network = testNetwork,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };
            Assert.NotNull(testNetworkDiscretization);

            var geomWrapper = new GridGeomApi();
            var linksFrom = new List<int>();
            var linksTo = new List<int>();
            var startIndex = 0;
            var linksCount = 0;
            try
            {
                geomWrapper.Get1D2DLinksFrom1DTo2D(netFilePath, testNetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount);
                geomWrapper.Get1D2DLinksFrom1DTo2D(netFilePath, testNetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount);
            }
            catch (Exception e)
            {
                Assert.Fail("Did not run twice: {0}", e.Message);
            }
        }

        private class TestCaseMaking1D2DLinks : IDisposable
        {
            private GridGeomWrapper gridGeomWrapper;
            private GridWrapper gridWrapper;
            private string filePath;
            private int ierr;
            private GridWrapper.meshgeomdim meshtwoddim;
            private GridWrapper.meshgeom meshtwod;
            IntPtr c_meshXCoords;
            IntPtr c_meshYCoords;
            IntPtr c_branchids;
            IntPtr c_branchoffset;
            IntPtr c_sourcenodeid;
            IntPtr c_targetnodeid;
            IntPtr c_branchlength;

            int[] branchids = { 1, 1, 1, 1 };
            double[] meshXCoords = { -6, 5, 23, 34 };
            double[] meshYCoords = { 22, 16, 16, 7 };
            double[] branchoffset = { 0, 10, 20, 100 }; /// important are the first and last offset
            double[] branchlength = { 100 };
            int[] sourcenodeid = { 1, 2, 3 };
            int[] targetnodeid = { 2, 3, 4 };

            //mesh1d
            //discretization points information
            int nmeshpoints = 4;
            int nbranches = 1;
            int twodnumnode = 16;

            public TestCaseMaking1D2DLinks()
            {
                gridWrapper = new GridWrapper();
                gridGeomWrapper = new GridGeomWrapper();

                //mesh2d
                int twoddim = 2;
                twodnumnode = 16;
                int twodnumedge = 24;
                int twodnumface = 9;
                int twodmaxnumfacenodes = 4;
                int twodnumlayer = -1;
                int twodlayertype = -1;
                int startIndex = 1; // the indexes in the array are zero based

                //1. open the file with the 2d mesh
                var orgFilePath = TestHelper.GetTestFilePath(@"flow1d2dLinks\2d_ugrid_net.nc");
                Assert.IsTrue(File.Exists(orgFilePath));

                filePath = TestHelper.CreateLocalCopy(orgFilePath);
                Assert.IsTrue(File.Exists(filePath));

                int ioncid = 0; //file variable 
                int mode = 0; //create in read mode
                int iconvtype = 2;
                double convversion = 0.0;

                var ierr = gridWrapper.Open(filePath, mode, ref ioncid, ref iconvtype, ref convversion);
                Assert.That(ierr, Is.EqualTo(0));

                //2. get the 2d mesh id
                int meshid = 1;
                ierr = gridWrapper.Get2DMeshId(ref ioncid, ref meshid);
                Assert.That(ierr, Is.EqualTo(0));

                //3. get the dimensions of the 2d mesh
                meshtwoddim = new GridWrapper.meshgeomdim();
                ierr = gridWrapper.get_meshgeom_dim(ref ioncid, ref meshid, ref meshtwoddim);
                Assert.That(ierr, Is.EqualTo(0));

                Assert.That(meshtwoddim.dim, Is.EqualTo(twoddim));
                Assert.That(meshtwoddim.numnode, Is.EqualTo(twodnumnode));
                Assert.That(meshtwoddim.numedge, Is.EqualTo(twodnumedge));
                Assert.That(meshtwoddim.numface, Is.EqualTo(twodnumface));
                Assert.That(meshtwoddim.maxnumfacenodes, Is.EqualTo(twodmaxnumfacenodes));
                Assert.That(meshtwoddim.numlayer, Is.EqualTo(twodnumlayer));
                Assert.That(meshtwoddim.layertype, Is.EqualTo(twodlayertype));

                //4. allocate the arrays in meshgeom for storing the 2d mesh coordinates, edge_nodes
                meshtwod = new GridWrapper.meshgeom();
                meshtwod.nodex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * twodnumnode);
                meshtwod.nodey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * twodnumnode);
                meshtwod.nodez = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * twodnumnode);
                meshtwod.edge_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * twodnumedge * 2);

                //5. get the meshgeom arrays
                bool includeArrays = true;
                ierr = gridWrapper.get_meshgeom(ref ioncid, ref meshid, ref meshtwod, ref startIndex, includeArrays);
                Assert.That(ierr, Is.EqualTo(0));

 
            }

            public IList<Tuple<int,int>> MakeEmbeddedOrLateralLinksForAreaAndReturnFromToOfAllLInks(double[] areaXValues, double[] areaYValues, bool bEmbedded = true)
            {
                var result = new List<Tuple<int, int>>();

                double[] rc_twodnodex = new double[twodnumnode];
                double[] rc_twodnodey = new double[twodnumnode];
                double[] rc_twodnodez = new double[twodnumnode];

                Marshal.Copy(meshtwod.nodex, rc_twodnodex, 0, twodnumnode);
                Marshal.Copy(meshtwod.nodey, rc_twodnodey, 0, twodnumnode);
                Marshal.Copy(meshtwod.nodez, rc_twodnodez, 0, twodnumnode);

                //6. allocate the 1d arrays for storing the 1d coordinates and edge_nodes
                c_meshXCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
                c_meshYCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
                c_branchids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nmeshpoints);
                c_branchoffset = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
                c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nbranches);
                c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nbranches);
                c_branchlength = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nbranches);

                Marshal.Copy(branchids, 0, c_branchids, nmeshpoints);
                Marshal.Copy(meshXCoords, 0, c_meshXCoords, nmeshpoints);
                Marshal.Copy(meshYCoords, 0, c_meshYCoords, nmeshpoints);
                Marshal.Copy(sourcenodeid, 0, c_sourcenodeid, nbranches);
                Marshal.Copy(targetnodeid, 0, c_targetnodeid, nbranches);
                Marshal.Copy(branchoffset, 0, c_branchoffset, nmeshpoints);
                Marshal.Copy(branchlength, 0, c_branchlength, nbranches);

                //7. fill kn (Herman datastructure) for creating the links
                int start_index = 1; //the smallest integer in sourcenodeid/targetnodeid is 1
                ierr = gridGeomWrapper.Convert1dArray(ref c_meshXCoords, ref c_meshYCoords, ref c_branchoffset, ref c_branchlength,
                    ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nbranches, ref nmeshpoints, ref start_index);
                Assert.That(ierr, Is.EqualTo(0));
                ierr = gridGeomWrapper.Convert(ref meshtwod, ref meshtwoddim, ref start_index);
                Assert.That(ierr, Is.EqualTo(0));

                IntPtr intPtrXValuesSelectedArea;
                IntPtr intPtrYValuesSelectedArea;
                IntPtr intPtrZValuesSelectedArea;
                IntPtr intPtrfilterMesh1DPoints;

                //1. make area
                var filterMesh1DPoints = Enumerable.Repeat(1, nmeshpoints).ToArray();
                var nFilterMesh1DPoints = filterMesh1DPoints.Length;
                int nCoordinates = areaXValues.Count();
                intPtrXValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinates);
                intPtrYValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinates);
                intPtrZValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinates);
                intPtrfilterMesh1DPoints = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nmeshpoints);

                var selectedAreaXCoords = areaXValues;
                var selectedAreaYCoords = areaYValues;
                var selectedAreaZCoords = Enumerable.Repeat(0.0,nCoordinates).ToArray();

                Marshal.Copy(selectedAreaXCoords, 0, intPtrXValuesSelectedArea, nCoordinates);
                Marshal.Copy(selectedAreaYCoords, 0, intPtrYValuesSelectedArea, nCoordinates);
                Marshal.Copy(selectedAreaZCoords, 0, intPtrZValuesSelectedArea, nCoordinates);
                Marshal.Copy(filterMesh1DPoints, 0, intPtrfilterMesh1DPoints, nmeshpoints);


                //2. generate links
                if (bEmbedded)
                { 
                    ierr = gridGeomWrapper.Make1D2DInternalNetlinks(ref nCoordinates, ref intPtrXValuesSelectedArea,
                        ref intPtrYValuesSelectedArea, ref intPtrZValuesSelectedArea, ref nFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
                    Assert.That(ierr, Is.EqualTo(0));
                }
                else
                {
                    ierr = gridGeomWrapper.Make1D2DLateralInternalNetlinks(ref nCoordinates, ref intPtrXValuesSelectedArea,
                        ref intPtrYValuesSelectedArea, ref intPtrZValuesSelectedArea, ref nFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
                    Assert.That(ierr, Is.EqualTo(0));
                }

                //3. get the number of links
                var linkType = (int) GridApiDataSet.LinkType.Embedded;
                int n1d2dlinks = 0;
                ierr = gridGeomWrapper.GetLinkCount(ref n1d2dlinks, ref linkType);
                Assert.That(ierr, Is.EqualTo(0));

                //4. get the links: arrayfrom = 2d cell index, arrayto = 1d node index 
                IntPtr c_arrayfrom = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * n1d2dlinks); //2d cell number
                IntPtr c_arrayto = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * n1d2dlinks); //1d node
                ierr = gridGeomWrapper.Get1d2dLinks(ref c_arrayfrom, ref c_arrayto, ref n1d2dlinks, ref linkType);
                Assert.That(ierr, Is.EqualTo(0));

                int[] rc_arrayfrom = new int[n1d2dlinks];
                int[] rc_arrayto = new int[n1d2dlinks];
                Marshal.Copy(c_arrayfrom, rc_arrayfrom, 0, n1d2dlinks);
                Marshal.Copy(c_arrayto, rc_arrayto, 0, n1d2dlinks);

                for (var i = 0; i < n1d2dlinks; i++)
                {
                    result.Add(new Tuple<int, int>(rc_arrayfrom[i], rc_arrayto[i]));
                }

                //Free from and to arrays describing the links 
                Marshal.FreeCoTaskMem(c_arrayfrom);
                Marshal.FreeCoTaskMem(c_arrayto);

                //selected area
                Marshal.FreeCoTaskMem(intPtrXValuesSelectedArea);
                Marshal.FreeCoTaskMem(intPtrYValuesSelectedArea);
                Marshal.FreeCoTaskMem(intPtrZValuesSelectedArea);
                Marshal.FreeCoTaskMem(intPtrfilterMesh1DPoints);

                gridGeomWrapper.DeallocateMemory();

                return result;
            }

            public IList<Tuple<int, int>> MakeGullyOrRoofLinksForAreaAndReturnFromToOfAllLInks(double[] geometryXValues, double[] geometryYValues, bool bGully = true)
            {
                var result = new List<Tuple<int, int>>();

                double[] rc_twodnodex = new double[twodnumnode];
                double[] rc_twodnodey = new double[twodnumnode];
                double[] rc_twodnodez = new double[twodnumnode];

                Marshal.Copy(meshtwod.nodex, rc_twodnodex, 0, twodnumnode);
                Marshal.Copy(meshtwod.nodey, rc_twodnodey, 0, twodnumnode);
                Marshal.Copy(meshtwod.nodez, rc_twodnodez, 0, twodnumnode);

                //6. allocate the 1d arrays for storing the 1d coordinates and edge_nodes
                c_meshXCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
                c_meshYCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
                c_branchids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nmeshpoints);
                c_branchoffset = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
                c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nbranches);
                c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nbranches);
                c_branchlength = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nbranches);

                Marshal.Copy(branchids, 0, c_branchids, nmeshpoints);
                Marshal.Copy(meshXCoords, 0, c_meshXCoords, nmeshpoints);
                Marshal.Copy(meshYCoords, 0, c_meshYCoords, nmeshpoints);
                Marshal.Copy(sourcenodeid, 0, c_sourcenodeid, nbranches);
                Marshal.Copy(targetnodeid, 0, c_targetnodeid, nbranches);
                Marshal.Copy(branchoffset, 0, c_branchoffset, nmeshpoints);
                Marshal.Copy(branchlength, 0, c_branchlength, nbranches);

                //7. fill kn (Herman datastructure) for creating the links
                int start_index = 1; //the smallest integer in sourcenodeid/targetnodeid is 1
                ierr = gridGeomWrapper.Convert1dArray(ref c_meshXCoords, ref c_meshYCoords, ref c_branchoffset, ref c_branchlength,
                    ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nbranches, ref nmeshpoints, ref start_index);
                Assert.That(ierr, Is.EqualTo(0));
                ierr = gridGeomWrapper.Convert(ref meshtwod, ref meshtwoddim, ref start_index);
                Assert.That(ierr, Is.EqualTo(0));

                IntPtr intPtrXValuesSelectedArea;
                IntPtr intPtrYValuesSelectedArea;
                IntPtr intPtrZValuesSelectedArea;
                IntPtr intPtrfilterMesh1DPoints;

                //1. make area
                var filterMesh1DPoints = Enumerable.Repeat(1, nmeshpoints).ToArray();
                var nFilterMesh1DPoints = filterMesh1DPoints.Length;
                int nCoordinates = geometryXValues.Count();
                intPtrXValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinates);
                intPtrYValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinates);
                intPtrZValuesSelectedArea = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCoordinates);
                intPtrfilterMesh1DPoints = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nmeshpoints);

                var gullyOrRoofXCoords = geometryXValues;
                var gullyOrRoofYCoords = geometryYValues;
                var gullyOrRoofZCoords = Enumerable.Repeat(0.0, nCoordinates).ToArray();

                Marshal.Copy(gullyOrRoofXCoords, 0, intPtrXValuesSelectedArea, nCoordinates);
                Marshal.Copy(gullyOrRoofYCoords, 0, intPtrYValuesSelectedArea, nCoordinates);
                Marshal.Copy(gullyOrRoofZCoords, 0, intPtrZValuesSelectedArea, nCoordinates);
                Marshal.Copy(filterMesh1DPoints, 0, intPtrfilterMesh1DPoints, nmeshpoints);


                //2. generate links

                if (bGully)
                {
                    ierr = gridGeomWrapper.Make1D2DGullyLinks(ref nCoordinates, ref intPtrXValuesSelectedArea,
                        ref intPtrYValuesSelectedArea, ref intPtrZValuesSelectedArea, ref nFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
                    Assert.That(ierr, Is.EqualTo(0));
                }
                else
                {
                    ierr = gridGeomWrapper.Make1D2DRoofLinks(ref nCoordinates, ref intPtrXValuesSelectedArea,
                        ref intPtrYValuesSelectedArea, ref intPtrZValuesSelectedArea, ref nFilterMesh1DPoints, ref intPtrfilterMesh1DPoints);
                    Assert.That(ierr, Is.EqualTo(0));
                }

                //3. get the number of links
                var linkType = bGully ? (int) GridApiDataSet.LinkType.GullySewer : (int) GridApiDataSet.LinkType.RoofSewer;
                int n1d2dlinks = 0;
                ierr = gridGeomWrapper.GetLinkCount(ref n1d2dlinks, ref linkType);
                Assert.That(ierr, Is.EqualTo(0));

                //4. get the links: arrayfrom = 2d cell index, arrayto = 1d node index 
                IntPtr c_arrayfrom = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * n1d2dlinks); //2d cell number
                IntPtr c_arrayto = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * n1d2dlinks); //1d node
                ierr = gridGeomWrapper.Get1d2dLinks(ref c_arrayfrom, ref c_arrayto, ref n1d2dlinks, ref linkType);
                Assert.That(ierr, Is.EqualTo(0));

                int[] rc_arrayfrom = new int[n1d2dlinks];
                int[] rc_arrayto = new int[n1d2dlinks];
                Marshal.Copy(c_arrayfrom, rc_arrayfrom, 0, n1d2dlinks);
                Marshal.Copy(c_arrayto, rc_arrayto, 0, n1d2dlinks);

                for (var i = 0; i < n1d2dlinks; i++)
                {
                    result.Add(new Tuple<int, int>(rc_arrayfrom[i], rc_arrayto[i]));
                }

                //Free from and to arrays describing the links 
                Marshal.FreeCoTaskMem(c_arrayfrom);
                Marshal.FreeCoTaskMem(c_arrayto);

                //selected area
                Marshal.FreeCoTaskMem(intPtrXValuesSelectedArea);
                Marshal.FreeCoTaskMem(intPtrYValuesSelectedArea);
                Marshal.FreeCoTaskMem(intPtrZValuesSelectedArea);
                Marshal.FreeCoTaskMem(intPtrfilterMesh1DPoints);

                gridGeomWrapper.DeallocateMemory();

                return result;
            }

            public void Dispose()
            {
                gridWrapper = null;
                gridGeomWrapper = null;

                //Free 2d arrays
                Marshal.FreeCoTaskMem(meshtwod.nodex);
                Marshal.FreeCoTaskMem(meshtwod.nodey);
                Marshal.FreeCoTaskMem(meshtwod.nodez);
                Marshal.FreeCoTaskMem(meshtwod.edge_nodes);

                //Free 1d arrays
                Marshal.FreeCoTaskMem(c_meshXCoords);
                Marshal.FreeCoTaskMem(c_meshYCoords);
                Marshal.FreeCoTaskMem(c_branchids);
                Marshal.FreeCoTaskMem(c_sourcenodeid);
                Marshal.FreeCoTaskMem(c_targetnodeid);
                Marshal.FreeCoTaskMem(c_branchlength);
                Marshal.FreeCoTaskMem(c_branchoffset);

                if (File.Exists(filePath))
                {
                    //File.Delete(filePath);
                }

            }
        }
    }
}
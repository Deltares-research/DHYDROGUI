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
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    public class Grid1d2dTest
    {
        static Grid1d2dTest()
        {
            NativeLibrary.LoadNativeDll(GridApiDataSet.GRIDDLL_NAME, DimrApiDataSet.SharedDllPath);
            NativeLibrary.LoadNativeDll(GridGeomApi.LIB_DLL_NAME, DimrApiDataSet.SharedDllPath);
        }
        
        [Test]
        public void CreateLinksFrom2dFileGridGeomWrapper()
        {
            /* This is a 'copy' of the original test in the FlowFM kernel made by Luca. It should work here as long as it works in the kernel. */
            var gridWrapper = new GridWrapper();
            var gridGeomWrapper = new GridGeomWrapper();

            //mesh2d
            int twoddim = 2;
            int twodnumnode = 16;
            int twodnumedge = 24;
            int twodnumface = 9;
            int twodmaxnumfacenodes = 4;
            int twodnumlayer = -1;
            int twodlayertype = -1;
            int startIndex = 1; // the indexes in the array are zero based


            //mesh1d
            //discretization points information
            int nmeshpoints = 4;
            int nbranches = 1;
            int[] branchids = { 1, 1, 1, 1 };
            double[] meshXCoords = { -6, 5, 23, 34 };
            double[] meshYCoords = { 22, 16, 16, 7 };
            double[] branchoffset = { 0, 10, 20, 100 }; /// important are the first and last offset
            double[] branchlength = { 100 };
            int[] sourcenodeid = { 1 };
            int[] targetnodeid = { 2 };

            //links
            int[] arrayfrom = { 2, 8, 7 };
            int[] arrayto = { 2, 3, 4 };

            //1. open the file with the 2d mesh
            string c_path = TestHelper.GetTestFilePath(@"flow1d2dLinks\2d_ugrid_net.nc");
            Assert.IsTrue(File.Exists(c_path));

            c_path = TestHelper.CreateLocalCopy(c_path);
            Assert.IsTrue(File.Exists(c_path));

            int ioncid = 0; //file variable 
            int mode = 0;   //create in read mode
            int iconvtype = 2;
            double convversion = 0.0;

            var ierr = gridWrapper.Open(c_path, mode, ref ioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            //2. get the 2d mesh id
            int meshid = 1;
            ierr = gridWrapper.Get2DMeshId(ref ioncid, ref meshid);
            Assert.That(ierr, Is.EqualTo(0));

            //3. get the dimensions of the 2d mesh
            GridWrapper.meshgeomdim meshtwoddim = new GridWrapper.meshgeomdim();
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
            GridWrapper.meshgeom meshtwod = new GridWrapper.meshgeom();
            meshtwod.nodex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * twodnumnode);
            meshtwod.nodey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * twodnumnode);
            meshtwod.nodez = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * twodnumnode);
            meshtwod.edge_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * twodnumedge * 2);

            //5. get the meshgeom arrays
            bool includeArrays = true;
            ierr = gridWrapper.get_meshgeom(ref ioncid, ref meshid, ref meshtwod, ref startIndex, includeArrays);
            Assert.That(ierr, Is.EqualTo(0));
            double[] rc_twodnodex = new double[twodnumnode];
            double[] rc_twodnodey = new double[twodnumnode];
            double[] rc_twodnodez = new double[twodnumnode];
            Marshal.Copy(meshtwod.nodex, rc_twodnodex, 0, twodnumnode);
            Marshal.Copy(meshtwod.nodey, rc_twodnodey, 0, twodnumnode);
            Marshal.Copy(meshtwod.nodez, rc_twodnodez, 0, twodnumnode);

            //6. allocate the 1d arrays for storing the 1d coordinates and edge_nodes
            IntPtr c_meshXCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
            IntPtr c_meshYCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
            IntPtr c_branchids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nmeshpoints);
            IntPtr c_branchoffset = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
            IntPtr c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nbranches);
            IntPtr c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nbranches);
            IntPtr c_branchlength = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nbranches);

            Marshal.Copy(branchids, 0, c_branchids, nmeshpoints);
            Marshal.Copy(meshXCoords, 0, c_meshXCoords, nmeshpoints);
            Marshal.Copy(meshYCoords, 0, c_meshYCoords, nmeshpoints);
            Marshal.Copy(sourcenodeid, 0, c_sourcenodeid, nbranches);
            Marshal.Copy(targetnodeid, 0, c_targetnodeid, nbranches);
            Marshal.Copy(branchoffset, 0, c_branchoffset, nmeshpoints);
            Marshal.Copy(branchlength, 0, c_branchlength, nbranches);

            //7. fill kn (Herman datastructure) for creating the links
            int start_index = 1; //the smallest integer in sourcenodeid/targetnodeid is 1
            ierr = gridGeomWrapper.Convert1dArray(ref c_meshXCoords, ref c_meshYCoords, ref c_branchoffset, ref c_branchlength, ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nbranches, ref nmeshpoints, ref start_index);
            Assert.That(ierr, Is.EqualTo(0));
            ierr = gridGeomWrapper.Convert(ref meshtwod, ref meshtwoddim,ref start_index);
            Assert.That(ierr, Is.EqualTo(0));

            //9. make the links
            ierr = gridGeomWrapper.Make1d2dInternalnetlinks();
            Assert.That(ierr, Is.EqualTo(0));

            //10. get the number of links
            int n1d2dlinks = 0;
            ierr = gridGeomWrapper.GetLinkCount(ref n1d2dlinks);
            Assert.That(ierr, Is.EqualTo(0));

            //11. get the links: arrayfrom = 2d cell index, arrayto = 1d node index 
            IntPtr c_arrayfrom = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * n1d2dlinks); //2d cell number
            IntPtr c_arrayto = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * n1d2dlinks); //1d node
            ierr = gridGeomWrapper.Get1d2dLinks(ref c_arrayfrom, ref c_arrayto, ref n1d2dlinks);
            Assert.That(ierr, Is.EqualTo(0));


            int[] rc_arrayfrom = new int[n1d2dlinks];
            int[] rc_arrayto = new int[n1d2dlinks];
            Marshal.Copy(c_arrayfrom, rc_arrayfrom, 0, n1d2dlinks);
            Marshal.Copy(c_arrayto, rc_arrayto, 0, n1d2dlinks);
            for (int i = 0; i < n1d2dlinks; i++)
            {
                Assert.That(rc_arrayfrom[i], Is.EqualTo(arrayfrom[i]));
                Assert.That(rc_arrayto[i], Is.EqualTo(arrayto[i]));
            }
            //for writing the links look io_netcdf ionc_def_mesh_contact, ionc_put_mesh_contact 

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

            //Free from and to arrays describing the links 
            Marshal.FreeCoTaskMem(c_arrayfrom);
            Marshal.FreeCoTaskMem(c_arrayto);
        }

        [Test]
        public void Get1d2dLinksFromEvent()
        {
            string mduPath = TestHelper.GetTestFilePath(@"flow1d2dLinks\SimpleModel\FlowFM.mdu");
            Assert.IsTrue(File.Exists(mduPath));
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            Assert.IsTrue(File.Exists(mduPath));

            /* Note, we would like to load everything directly from the MDU, but the previous implementation is wrong and does not load the 1d network */
            string netFilePath = TestHelper.GetTestFilePath(@"flow1d2dLinks\SimpleModel\2d_ugrid_net.nc");
            Assert.IsTrue(File.Exists(netFilePath));
            netFilePath = TestHelper.CreateLocalCopy(netFilePath);
            Assert.IsTrue(File.Exists(netFilePath));

            var model = new WaterFlowFMModel(mduPath);

            // 0.1 Set Network Discretization.
            model.Network = new HydroNetwork();
            WaterFlowFMTestHelper.ConfigureDemoNetworkAtGivenCoordinates(model.Network);
            Assert.NotNull(model.Network);

            model.NetworkDiscretization = new Discretization
            {
                Name = WaterFlowFMModel.DiscretizationObjectName,
                Network = model.Network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };
            Assert.NotNull(model.NetworkDiscretization);

            // first offest always equal to 0 last offset equal to branch length
            var offSet = new double[] { 0, 5, 10, 36.8337027874097 };
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, (IChannel)model.Network.Branches[0], offSet);

            // 0.2 Set grid.
            model.Grid = UnstructuredGridFileHelper.LoadFromFile(netFilePath, true);
            Assert.NotNull(model.Grid);

            //Mode 1. Make sure the event propagation when changing the grid triggers the link generation.
            Assert.IsFalse(model.NetworkDiscretization == null || !model.NetworkDiscretization.Locations.AllValues.Any());
            Assert.NotNull(model.Links);
            Assert.AreNotEqual(0, model.Links.Count);
            foreach (var link in model.Links)
            {
                Assert.NotNull( link.Geometry );
                Assert.AreEqual(typeof(LineString), link.Geometry.GetType());
            }
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
            var ierr = geomWrapper.Get1d2dLinksFromGridAndNetwork(netFilePath, testNetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount);
            Assert.AreEqual(GridApiDataSet.GridConstants.NOERR, ierr);
            Assert.AreNotEqual(0, linksCount);
            Assert.IsNotEmpty(linksFrom);
            Assert.IsNotEmpty(linksTo);
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
                geomWrapper.Get1d2dLinksFromGridAndNetwork(netFilePath, testNetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount);
                geomWrapper.Get1d2dLinksFromGridAndNetwork(netFilePath, testNetworkDiscretization, ref linksFrom, ref linksTo, ref startIndex, ref linksCount);
            }
            catch (Exception e)
            {
                Assert.Fail("Did not run twice: {0}", e.Message);
            }
        }
    }
}
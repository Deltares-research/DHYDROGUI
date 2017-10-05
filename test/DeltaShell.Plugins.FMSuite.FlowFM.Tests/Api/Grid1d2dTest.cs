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
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation;
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
            int twodnumlayer = 0;
            int twodlayertype = 0;

            //mesh1d
            //discretization points information
            int nmeshpoints = 4;
            int nbranches = 1;
            int[] branchids = { 1, 1, 1, 1 };
            double[] meshXCoords = { -6, 5, 23, 34 };
            double[] meshYCoords = { 22, 16, 16, 7 };
            int[] sourcenodeid = { 1 };
            int[] targetnodeid = { 2 };

            //links
            int[] arrayfrom = { 2, 8 };
            int[] arrayto = { 2, 3 };

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
            ierr = gridWrapper.get_meshgeom(ref ioncid, ref meshid, ref meshtwod, includeArrays);
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
            IntPtr c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nbranches);
            IntPtr c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nbranches);

            Marshal.Copy(branchids, 0, c_branchids, nmeshpoints);
            Marshal.Copy(meshXCoords, 0, c_meshXCoords, nmeshpoints);
            Marshal.Copy(meshYCoords, 0, c_meshYCoords, nmeshpoints);
            Marshal.Copy(sourcenodeid, 0, c_sourcenodeid, nbranches);
            Marshal.Copy(targetnodeid, 0, c_targetnodeid, nbranches);

            //7. fill kn (Herman datastructure) for creating the links
            ierr = gridGeomWrapper.Convert1dArray(ref c_meshXCoords, ref c_meshYCoords, ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nbranches, ref nmeshpoints);
            Assert.That(ierr, Is.EqualTo(0));
            ierr = gridGeomWrapper.Convert(ref meshtwod, ref meshtwoddim);
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

            //Free from and to arrays describing the links 
            Marshal.FreeCoTaskMem(c_arrayfrom);
            Marshal.FreeCoTaskMem(c_arrayto);
        }

        [Test]
        public void Get1d2dLinksFromGridGeomApi()
        {
            /* This test is in practice, the pure implementation of the Method in GeomApi but with the given data. */

            var gridWrapper = new GridWrapper();
            var gridGeomWrapper = new GridGeomWrapper();
            var geomApi = new GridGeomApi();

            //mesh2d
            int twoddim = 2;
            int twodnumnode = 16;
            int twodnumedge = 24;
            int twodnumface = 9;
            int twodmaxnumfacenodes = 4;
            int twodnumlayer = 0;
            int twodlayertype = 0;

            string c_path = TestHelper.GetTestFilePath(@"flow1d2dLinks\2d_ugrid_net.nc");
            Assert.IsTrue(File.Exists(c_path));

            c_path = TestHelper.CreateLocalCopy(c_path);
            Assert.IsTrue(File.Exists(c_path));

            //mesh1d
            //discretization points information
            int nmeshpoints = 4;
            int nbranches = 1;
            int[] branchids = { 1, 1, 1, 1 };
            double[] meshXCoords = { -6, 5, 23, 34 };
            double[] meshYCoords = { 22, 16, 16, 7 };
            int[] sourcenodeid = { 1 };
            int[] targetnodeid = { 2 };

            //links
            int[] arrayfrom = { 2, 8 };
            int[] arrayto = { 2, 3 };

            //1. open the file with the 2d mesh
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
            ierr = gridWrapper.get_meshgeom(ref ioncid, ref meshid, ref meshtwod, includeArrays);
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
            IntPtr c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nbranches);
            IntPtr c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nbranches);

            Marshal.Copy(branchids, 0, c_branchids, nmeshpoints);
            Marshal.Copy(meshXCoords, 0, c_meshXCoords, nmeshpoints);
            Marshal.Copy(meshYCoords, 0, c_meshYCoords, nmeshpoints);
            Marshal.Copy(sourcenodeid, 0, c_sourcenodeid, nbranches);
            Marshal.Copy(targetnodeid, 0, c_targetnodeid, nbranches);

            //7. fill kn (Herman datastructure) for creating the links
            ierr = gridGeomWrapper.Convert1dArray(ref c_meshXCoords, ref c_meshYCoords, ref c_branchids, ref c_sourcenodeid, ref c_targetnodeid, ref nbranches, ref nmeshpoints);
            Assert.That(ierr, Is.EqualTo(0));
            ierr = gridGeomWrapper.Convert(ref meshtwod, ref meshtwoddim);
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

            //Free from and to arrays describing the links 
            Marshal.FreeCoTaskMem(c_arrayfrom);
            Marshal.FreeCoTaskMem(c_arrayto);
        }

        [Test]
        public void Get1d2dLinksDirectly()
        {
            
        }
    }
}
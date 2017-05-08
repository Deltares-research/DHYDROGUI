using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.InteropServices;
using DelftTools.Utils.Interop;
using DelftTools.Utils.IO;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class Ugrid1DTests
    {
        //network name
        private string networkName = "network";

        int networkid = 1; // we are assuming only one meshids is present

        //dimension info
        private int nNodes = 4;

        private int nBranches = 3;

        private int nGeometry = 7;

        //node info
        private double[] nodesX = {1.0, 5.0, 5.0, 8.0};

        private double[] nodesY = {4.0, 1.0, 4.0, 4.0};
        private string[] nodesids = {"node1", "node2", "node3", "node4"};
        private string[] nodeslongNames = {"nodelong1", "nodelong2", "nodelong3", "nodelong4"};
        private int[] sourcenodeid = {1, 3, 2};

        private int[] targetnodeid = {2, 2, 4};

        //branches info
        private double[] branchlengths = {4.0, 3.0, 3.0};

        private int[] nbranchgeometrypoints = {2, 3, 2};
        private string[] branchids = {"branch1", "branch2", "branch3"};

        private string[] branchlongNames = {"branchlong1", "branchlong2", "branchlong3"};

        //geometry info
        private double[] geopointsX = {1.0, 3.0, 5.0, 7.0, 8.0, 5.0, 5.0};

        private double[] geopointsY = {4.0, 4.0, 4.0, 4.0, 4.0, 1.0, 2.0};

        //mesh name
        private string meshname = "1dmesh";

        //mesh dimension
        private int nmeshpoints = 10;

        private int nmeshedges = 14;

        //mesh geometry
        private int[] branchidx = {1, 1, 1, 1, 2, 2, 2, 3, 3, 3};

        private double[] offset = {0.5, 1.0, 1.0, 1.0, 0.5, 1.0, 1.0, 0.5, 1.0, 1.0};

        //netcdf file specifications 
        private int iconvtype = 2;

        private double convversion = 0.0;

        static Ugrid1DTests()
        {
            NativeLibrary.LoadNativeDllForCurrentPlatform(GridApiDataSet.GRIDDLL_NAME, GridApiDataSet.DllDirectory);
        }

        ////create the netcdf files
        [Test]
        [Category(TestCategory.DataAccess)]
        public void create1dUGRIDNetcdf()
        {
            IntPtr c_nodesX = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr c_nodesY = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr c_branchlengths = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nBranches);
            IntPtr c_nbranchgeometrypoints = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr c_geopointsX = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nGeometry);
            IntPtr c_geopointsY = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nGeometry);
            IntPtr c_branchidx = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nmeshpoints);
            IntPtr c_offset = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
            try
            {
                //1. create a netcdf file 
                int ioncid = 0; //file variable 
                int mode = 1; //create in write mode
                var ierr = -1;
                string tmpstring; //temporary string for several operations
                string c_path = TestHelper.GetTestFilePath(@"ugrid\write1d.nc");
                c_path = TestHelper.CreateLocalCopy(c_path);
                FileUtils.DeleteIfExists(c_path);
                Assert.IsFalse(File.Exists(c_path));
                var wrapper = new GridWrapper();
                // create the file, will not add any dataset (iconvtype maybe not necessary)
                ierr = wrapper.ionc_create(c_path, ref mode, ref ioncid, ref iconvtype);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.IsTrue(File.Exists(c_path));

                // For reading the grid later on i need to add metadata to the netcdf file. 
                // The function ionc_add_global_attributes adds to the netCDF file the UGRID convention
                GridWrapper.interop_metadata metadata;
                tmpstring = "Deltares";
                tmpstring = tmpstring.PadRight(GridWrapper.metadatasize, ' ');
                metadata.institution = tmpstring.ToCharArray();
                tmpstring = "Unknown";
                tmpstring = tmpstring.PadRight(GridWrapper.metadatasize, ' ');
                metadata.source = tmpstring.ToCharArray();
                tmpstring = "Unknown";
                tmpstring = tmpstring.PadRight(GridWrapper.metadatasize, ' ');
                metadata.references = tmpstring.ToCharArray();
                tmpstring = "Unknown";
                tmpstring = tmpstring.PadRight(GridWrapper.metadatasize, ' ');
                metadata.version = tmpstring.ToCharArray();
                tmpstring = "Unknown";
                tmpstring = tmpstring.PadRight(GridWrapper.metadatasize, ' ');
                metadata.modelname = tmpstring.ToCharArray();
                ierr = wrapper.ionc_add_global_attributes(ref ioncid, metadata);
                Assert.That(ierr, Is.EqualTo(0));

                //2. create a 1d network
                ierr = wrapper.ionc_create_1d_network(ref ioncid, ref networkid, networkName, ref nNodes,
                    ref nBranches, ref nGeometry);
                Assert.That(ierr, Is.EqualTo(0));

                //3. write 1d network network nodes
                Marshal.Copy(nodesX, 0, c_nodesX, nNodes);
                Marshal.Copy(nodesY, 0, c_nodesY, nNodes);
                GridWrapper.interop_charinfo[] nodesinfo = new GridWrapper.interop_charinfo[4];
                for (int i = 0; i < nNodes; i++)
                {
                    tmpstring = nodesids[i];
                    tmpstring = tmpstring.PadRight(GridWrapper.idssize, ' ');
                    nodesinfo[i].ids = tmpstring.ToCharArray();
                    tmpstring = nodeslongNames[i];
                    tmpstring = tmpstring.PadRight(GridWrapper.longnamessize, ' ');
                    nodesinfo[i].longnames = tmpstring.ToCharArray();
                }
                ierr = wrapper.ionc_write_1d_network_nodes(ref ioncid, ref networkid, ref c_nodesX, ref c_nodesY,
                    nodesinfo, ref nNodes);
                Assert.That(ierr, Is.EqualTo(0));

                //4. write 1d network branches
                Marshal.Copy(sourcenodeid, 0, c_sourcenodeid, nBranches);
                Marshal.Copy(targetnodeid, 0, c_targetnodeid, nBranches);
                Marshal.Copy(branchlengths, 0, c_branchlengths, nBranches);
                Marshal.Copy(nbranchgeometrypoints, 0, c_nbranchgeometrypoints, nBranches);
                GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[3];
                for (int i = 0; i < nBranches; i++)
                {
                    tmpstring = branchids[i];
                    tmpstring = tmpstring.PadRight(GridWrapper.idssize, ' ');
                    branchinfo[i].ids = tmpstring.ToCharArray();
                    tmpstring = branchlongNames[i];
                    tmpstring = tmpstring.PadRight(GridWrapper.longnamessize, ' ');
                    branchinfo[i].longnames = tmpstring.ToCharArray();
                }
                ierr = wrapper.ionc_write_1d_network_branches(ref ioncid, ref networkid, ref c_sourcenodeid,
                    ref c_targetnodeid, branchinfo, ref c_branchlengths, ref c_nbranchgeometrypoints, ref nBranches);
                Assert.That(ierr, Is.EqualTo(0));

                //5. write 1d network geometry
                Marshal.Copy(geopointsX, 0, c_geopointsX, nGeometry);
                Marshal.Copy(geopointsY, 0, c_geopointsY, nGeometry);
                ierr = wrapper.ionc_write_1d_network_branches_geometry(ref ioncid, ref networkid, ref c_geopointsX,
                    ref c_geopointsY, ref nGeometry);
                Assert.That(ierr, Is.EqualTo(0));

                //6. write the 1d mesh topology
                ierr = wrapper.ionc_create_1d_mesh(ref ioncid, ref networkid, meshname, ref nmeshpoints,
                    ref nmeshedges);
                Assert.That(ierr, Is.EqualTo(0));

                //7. write the 1d mesh geometry
                Marshal.Copy(branchidx, 0, c_branchidx, nmeshpoints);
                Marshal.Copy(offset, 0, c_offset, nmeshpoints);
                ierr = wrapper.ionc_write_1d_mesh_discretisation_points(ref ioncid, ref networkid, ref c_branchidx,
                    ref c_offset, ref nmeshpoints);
                Assert.That(ierr, Is.EqualTo(0));

                //8. close the file
                ierr = wrapper.ionc_close(ref ioncid);
            }
            finally
            {
                Marshal.FreeHGlobal(c_nodesX);
                Marshal.FreeHGlobal(c_nodesY);
                Marshal.FreeHGlobal(c_sourcenodeid);
                Marshal.FreeHGlobal(c_targetnodeid);
                Marshal.FreeHGlobal(c_branchlengths);
                Marshal.FreeHGlobal(c_nbranchgeometrypoints);
                Marshal.FreeHGlobal(c_geopointsX);
                Marshal.FreeHGlobal(c_geopointsY);
                Marshal.FreeHGlobal(c_branchidx);
                Marshal.FreeHGlobal(c_offset);
            }
        }

        // read the netcdf file created in the test above
        [Test]
        [Category(TestCategory.DataAccess)]
        public void read1dUGRIDNetcdf()
        {
            IntPtr c_nodesX = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr c_nodesY = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nNodes);
            IntPtr c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr c_branchlengths = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nBranches);
            IntPtr c_nbranchgeometrypoints = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nBranches);
            IntPtr c_geopointsX = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nGeometry);
            IntPtr c_geopointsY = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nGeometry);
            IntPtr c_branchidx = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nmeshpoints);
            IntPtr c_offset = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
            try
            {
                //1. create a netcdf file 
                string c_path = TestHelper.GetTestFilePath(@"ugrid\write1d.nc");
                c_path = TestHelper.CreateLocalCopy(c_path);
                Assert.IsTrue(File.Exists(c_path));
                int ioncid = 0; //file variable 
                int mode = 0; //create in read mode
                var wrapper = new GridWrapper();
                var ierr = wrapper.ionc_open(c_path, ref mode, ref ioncid, ref iconvtype, ref convversion);
                Assert.That(ierr, Is.EqualTo(0));

                //2. get the node count
                int networkid = 1;
                int rnNodes = -1;
                ierr = wrapper.ionc_get_1d_network_nodes_count(ref ioncid, ref networkid, ref rnNodes);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnNodes, Is.EqualTo(nNodes));

                //3. get the number of branches
                int rnBranches = -1;
                ierr = wrapper.ionc_get_1d_network_branches_count(ref ioncid, ref networkid, ref rnBranches);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnBranches, Is.EqualTo(nBranches));

                //4. get the number of geometry points
                int rnGeometry = -1;
                ierr =
                    wrapper.ionc_get_1d_network_branches_geometry_coordinate_count(ref ioncid, ref networkid,
                        ref rnGeometry);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnGeometry, Is.EqualTo(nGeometry));

                //5. read nodes info and coordinates
                GridWrapper.interop_charinfo[] nodesinfo = new GridWrapper.interop_charinfo[4];
                ierr = wrapper.ionc_read_1d_network_nodes(ref ioncid, ref networkid, ref c_nodesX, ref c_nodesY,
                    nodesinfo, ref rnNodes);
                Assert.That(ierr, Is.EqualTo(0));

                double[] rc_nodesX = new double[4];
                double[] rc_nodesY = new double[4];
                Marshal.Copy(c_nodesX, rc_nodesX, 0, 4);
                Marshal.Copy(c_nodesY, rc_nodesY, 0, 4);
                for (int i = 0; i < rnNodes; i++)
                {
                    string tmpstring = new string(nodesinfo[i].ids);
                    Assert.That(tmpstring.Trim(), Is.EqualTo(nodesids[i]));
                    tmpstring = new string(nodesinfo[i].longnames);
                    Assert.That(tmpstring.Trim(), Is.EqualTo(nodeslongNames[i]));
                    Assert.That(rc_nodesX[i], Is.EqualTo(nodesX[i]));
                    Assert.That(rc_nodesY[i], Is.EqualTo(nodesY[i]));
                }

                //6. read the branch info and coordinates
                GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[3];
                ierr = wrapper.ionc_read_1d_network_branches(ref ioncid, ref networkid, ref c_sourcenodeid,
                    ref c_targetnodeid,
                    ref c_branchlengths, branchinfo, ref c_nbranchgeometrypoints, ref rnBranches);
                Assert.That(ierr, Is.EqualTo(0));

                int[] rc_targetnodeid = new int[3];
                int[] rc_sourcenodeid = new int[3];
                double[] rc_branchlengths = new double[3];
                int[] rc_nbranchgeometrypoints = new int[3];
                Marshal.Copy(c_targetnodeid, rc_targetnodeid, 0, 3);
                Marshal.Copy(c_sourcenodeid, rc_sourcenodeid, 0, 3);
                Marshal.Copy(c_branchlengths, rc_branchlengths, 0, 3);
                Marshal.Copy(c_nbranchgeometrypoints, rc_nbranchgeometrypoints, 0, 3);

                for (int i = 0; i < rnBranches; i++)
                {
                    string tmpstring = new string(branchinfo[i].ids);
                    Assert.That(tmpstring.Trim(), Is.EqualTo(branchids[i]));
                    tmpstring = new string(branchinfo[i].longnames);
                    Assert.That(tmpstring.Trim(), Is.EqualTo(branchlongNames[i]));
                    Assert.That(rc_targetnodeid[i], Is.EqualTo(targetnodeid[i]));
                    Assert.That(rc_sourcenodeid[i], Is.EqualTo(sourcenodeid[i]));
                    Assert.That(rc_branchlengths[i], Is.EqualTo(branchlengths[i]));
                    Assert.That(rc_nbranchgeometrypoints[i], Is.EqualTo(nbranchgeometrypoints[i]));
                }

                //7. read the 1d branch geometry
                ierr = wrapper.ionc_read_1d_network_branches_geometry(ref ioncid, ref networkid, ref c_geopointsX,
                    ref c_geopointsY, ref rnGeometry);
                Assert.That(ierr, Is.EqualTo(0));

                double[] rc_geopointsX = new double[rnGeometry];
                double[] rc_geopointsY = new double[rnGeometry];
                Marshal.Copy(c_geopointsX, rc_geopointsX, 0, rnGeometry);
                Marshal.Copy(c_geopointsY, rc_geopointsY, 0, rnGeometry);
                for (int i = 0; i < rnGeometry; i++)
                {
                    Assert.That(rc_geopointsX[i], Is.EqualTo(geopointsX[i]));
                    Assert.That(rc_geopointsY[i], Is.EqualTo(geopointsY[i]));
                }

                //8. read the number of mesh points
                int rnmeshpoints = -1;
                ierr =
                    wrapper.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkid,
                        ref rnmeshpoints);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnmeshpoints, Is.EqualTo(nmeshpoints));


                //9. read the coordinates of the mesh points
                ierr = wrapper.ionc_read_1d_mesh_discretisation_points(ref ioncid, ref networkid, ref c_branchidx,
                    ref c_offset, ref rnmeshpoints);
                Assert.That(ierr, Is.EqualTo(0));
                int[] rc_branchidx = new int[rnmeshpoints];
                double[] rc_offset = new double[rnmeshpoints];
                Marshal.Copy(c_branchidx, rc_branchidx, 0, rnmeshpoints);
                Marshal.Copy(c_offset, rc_offset, 0, rnmeshpoints);
                for (int i = 0; i < rnmeshpoints; i++)
                {
                    Assert.That(rc_branchidx[i], Is.EqualTo(branchidx[i]));
                    Assert.That(rc_offset[i], Is.EqualTo(offset[i]));
                }

                //10. close the file
                ierr = wrapper.ionc_close(ref ioncid);
            }
            finally
            {
                Marshal.FreeHGlobal(c_nodesX);
                Marshal.FreeHGlobal(c_nodesY);
                Marshal.FreeHGlobal(c_sourcenodeid);
                Marshal.FreeHGlobal(c_targetnodeid);
                Marshal.FreeHGlobal(c_branchlengths);
                Marshal.FreeHGlobal(c_nbranchgeometrypoints);
                Marshal.FreeHGlobal(c_geopointsX);
                Marshal.FreeHGlobal(c_geopointsY);
                Marshal.FreeHGlobal(c_branchidx);
                Marshal.FreeHGlobal(c_offset);

            }
        }
    }
}

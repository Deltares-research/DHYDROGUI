using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Grid;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using DelftTools.Utils.Interop;
using DelftTools.Utils.IO;
using DeltaShell.Dimr;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class Ugrid1DTests
    {
        //network name
        private string networkName = "network";

        //dimension info
        private int nNodes = 4;

        private int nBranches = 3;

        private int nGeometry = 7;

        //node info
        private double[] nodesX = {1.0, 5.0, 5.0, 8.0};

        private double[] nodesY = {4.0, 4.0, 1.0, 4.0};
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

        private double[] offset = {0.0, 2.0, 3.0, 4.0, 0.0, 1.5, 3.0, 0.0, 1.5, 3.0};

        //netcdf file specifications 
        private int iconvtype = 2;

        private double convversion = 0.0;

        //mesh links
        private string linkmeshname = "links";

        private int nlinks = 3;
        private int linkmesh1 = 1;
        private int linkmesh2 = 2;
        private int locationType1 = 1;
        private int locationType2 = 1;
        private int[] mesh1indexes = {1, 2, 3};
        private int[] mesh2indexes = {1, 2, 3};
        private string[] linksids = {"link1", "link2", "link3"};
        private string[] linkslongnames = {"linklong1", "linklong2", "linklong3"};

        static Ugrid1DTests()
        {
            NativeLibrary.LoadNativeDll(GridApiDataSet.GRIDDLL_NAME, DimrApiDataSet.SharedDllPath);
        }

        //////create the netcdf files
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
            // Links variables
            IntPtr c_mesh1indexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nlinks);
            IntPtr c_mesh2indexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nlinks);
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
                // create the file, will not add any dataset 
                ierr = wrapper.ionc_create(c_path, ref mode, ref ioncid);
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
                int networkid = -1;
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

                //6. write the 1d mesh topology.
                //Assume mesh and network are saved in the same table 
                int meshid = networkid;
                ierr = wrapper.ionc_create_1d_mesh(ref ioncid, ref meshid, meshname, ref nmeshpoints,
                    ref nmeshedges);
                Assert.That(ierr, Is.EqualTo(0));

                //7. write the 1d mesh geometry
                Marshal.Copy(branchidx, 0, c_branchidx, nmeshpoints);
                Marshal.Copy(offset, 0, c_offset, nmeshpoints);
                ierr = wrapper.ionc_write_1d_mesh_discretisation_points(ref ioncid, ref meshid, ref c_branchidx,
                    ref c_offset, ref nmeshpoints);
                Assert.That(ierr, Is.EqualTo(0));

                //8. write links attributes
                Marshal.Copy(mesh1indexes, 0, c_mesh1indexes, nlinks);
                Marshal.Copy(mesh2indexes, 0, c_mesh2indexes, nlinks);
                int linkmesh = -1;
                ierr = wrapper.ionc_def_mesh_contact(ref ioncid, ref linkmesh, linkmeshname, ref nlinks, ref linkmesh1,
                    ref linkmesh2, ref locationType1, ref locationType2);
                Assert.That(ierr, Is.EqualTo(0));
                GridWrapper.interop_charinfo[] linksinfo = new GridWrapper.interop_charinfo[nlinks];

                for (int i = 0; i < nlinks; i++)
                {
                    tmpstring = linksids[i];
                    tmpstring = tmpstring.PadRight(GridWrapper.idssize, ' ');
                    linksinfo[i].ids = tmpstring.ToCharArray();
                    tmpstring = linkslongnames[i];
                    tmpstring = tmpstring.PadRight(GridWrapper.longnamessize, ' ');
                    linksinfo[i].longnames = tmpstring.ToCharArray();
                }

                //9. write the mesh links
                ierr = wrapper.ionc_put_mesh_contact(ref ioncid, ref linkmesh, ref c_mesh1indexes, ref c_mesh2indexes,
                    linksinfo, ref nlinks);
                Assert.That(ierr, Is.EqualTo(0));

                //10. close the file
                ierr = wrapper.ionc_close(ref ioncid);
            }
            finally
            {
                Marshal.FreeCoTaskMem(c_nodesX);
                Marshal.FreeCoTaskMem(c_nodesY);
                Marshal.FreeCoTaskMem(c_sourcenodeid);
                Marshal.FreeCoTaskMem(c_targetnodeid);
                Marshal.FreeCoTaskMem(c_branchlengths);
                Marshal.FreeCoTaskMem(c_nbranchgeometrypoints);
                Marshal.FreeCoTaskMem(c_geopointsX);
                Marshal.FreeCoTaskMem(c_geopointsY);
                Marshal.FreeCoTaskMem(c_branchidx);
                Marshal.FreeCoTaskMem(c_offset);
                Marshal.FreeCoTaskMem(c_mesh1indexes);
                Marshal.FreeCoTaskMem(c_mesh2indexes);
            }
        }

        ////// read the netcdf file created in the test above
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
            IntPtr c_geopointsX = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nGeometry);
            IntPtr c_geopointsY = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nGeometry);
            IntPtr c_branchidx = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nmeshpoints);
            IntPtr c_offset = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
            // Links variables
            IntPtr c_mesh1indexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nlinks);
            IntPtr c_mesh2indexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nlinks);
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

                //2. get the 1D network and mesh ids
                int networkid = -1;
                ierr = wrapper.ionc_get_1d_network_id(ref ioncid, ref networkid);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(networkid, Is.EqualTo(1));
                int meshid = -1;
                ierr = wrapper.ionc_get_1d_mesh_id(ref ioncid, ref meshid);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(meshid, Is.EqualTo(1));
                

                //3. get the node count
                int rnNodes = -1;
                ierr = wrapper.ionc_get_1d_network_nodes_count(ref ioncid, ref networkid, ref rnNodes);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnNodes, Is.EqualTo(nNodes));

                //4. get the number of branches
                int rnBranches = -1;
                ierr = wrapper.ionc_get_1d_network_branches_count(ref ioncid, ref networkid, ref rnBranches);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnBranches, Is.EqualTo(nBranches));

                //5. get the number of geometry points
                int rnGeometry = -1;
                ierr = wrapper.ionc_get_1d_network_branches_geometry_coordinate_count(ref ioncid, ref networkid,
                        ref rnGeometry);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnGeometry, Is.EqualTo(nGeometry));

                //6. read nodes info and coordinates
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

                //7. read the branch info and coordinates
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

                //8. read the 1d branch geometry
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
                
                //9. get the mesh name
                var rnetworkName = new StringBuilder(GridApiDataSet.GridConstants.MAXSTRLEN);
                ierr = wrapper.ionc_get_mesh_name(ref ioncid, ref networkid, rnetworkName);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnetworkName.ToString().Trim(), Is.EqualTo(meshname));

                //10. read the number of mesh points
                int rnmeshpoints = -1;
                ierr =
                    wrapper.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref networkid,
                        ref rnmeshpoints);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnmeshpoints, Is.EqualTo(nmeshpoints));

                //11. read the coordinates of the mesh points
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

                //12. read the number of links 
                int linkmesh = 1;
                int r_nlinks = -1;
                ierr = wrapper.ionc_get_contacts_count(ref ioncid, ref linkmesh, ref r_nlinks);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(r_nlinks, Is.EqualTo(nlinks));
                GridWrapper.interop_charinfo[] linksinfo = new GridWrapper.interop_charinfo[nlinks];

                //13. read the links values back in
                ierr = wrapper.ionc_get_mesh_contact(ref ioncid, ref linkmesh, ref c_mesh1indexes, ref c_mesh2indexes,
                    linksinfo, ref nlinks);
                Assert.That(ierr, Is.EqualTo(0));
                int[] rc_mesh1indexes = new int[nlinks];
                int[] rc_mesh2indexes = new int[nlinks];
                Marshal.Copy(c_mesh1indexes, rc_mesh1indexes, 0, nlinks);
                Marshal.Copy(c_mesh2indexes, rc_mesh2indexes, 0, nlinks);
                for (int i = 0; i < nlinks; i++)
                {
                    string tmpstring = new string(linksinfo[i].ids);
                    Assert.That(tmpstring.Trim(), Is.EqualTo(linksids[i]));
                    tmpstring = new string(linksinfo[i].longnames);
                    Assert.That(tmpstring.Trim(), Is.EqualTo(linkslongnames[i]));
                    Assert.That(rc_mesh1indexes[i], Is.EqualTo(mesh1indexes[i]));
                    Assert.That(rc_mesh2indexes[i], Is.EqualTo(mesh2indexes[i]));
                }

                //14. close the file
                ierr = wrapper.ionc_close(ref ioncid);
            }
            finally
            {
                Marshal.FreeCoTaskMem(c_nodesX);
                Marshal.FreeCoTaskMem(c_nodesY);
                Marshal.FreeCoTaskMem(c_sourcenodeid);
                Marshal.FreeCoTaskMem(c_targetnodeid);
                Marshal.FreeCoTaskMem(c_branchlengths);
                Marshal.FreeCoTaskMem(c_nbranchgeometrypoints);
                Marshal.FreeCoTaskMem(c_geopointsX);
                Marshal.FreeCoTaskMem(c_geopointsY);
                Marshal.FreeCoTaskMem(c_branchidx);
                Marshal.FreeCoTaskMem(c_offset);
            }
        }

        //// clone a specific mesh from one file to another
        [Test]
        [Category(TestCategory.DataAccess)]
        public void DeltaShellToRGFGrid()
        {
            var wrapper = new GridWrapper();

            //1. Delta shell creates a 1d file with mesh1d and links. RGF grid opens it and store the info in memory
            string sourceoned_path = TestHelper.GetTestFilePath(@"ugrid\write1d.nc");
            sourceoned_path = TestHelper.CreateLocalCopy(sourceoned_path);
            Assert.IsTrue(File.Exists(sourceoned_path));
            int source1dioncid = -1;  //file id  
            int source1dmode = 0;  //read mode
            int ierr = wrapper.ionc_open(sourceoned_path, ref source1dmode, ref source1dioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            //2. RGF grid creates a 2d mesh. The info is in memory, here simulated by opening a second file with a mesh2d
            string sourcetwod_path = TestHelper.GetTestFilePath(@"ugrid\FlowFM_net.nc");
            sourcetwod_path = TestHelper.CreateLocalCopy(sourcetwod_path);
            Assert.IsTrue(File.Exists(sourcetwod_path));
            int sourcetwodioncid = -1;   //file id 
            int sourcetwomode = 0;   //read mode
            ierr = wrapper.ionc_open(sourcetwod_path, ref sourcetwomode, ref sourcetwodioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            //3. Now we create a new empty file where to save 1d part from deltashell, links and mesh2d
            int targetioncid = -1;  //file id  
            int targetmode = 1;  //create in write mode
            string target_path = TestHelper.GetTestFilePath(@"ugrid\target.nc");
            target_path = TestHelper.CreateLocalCopy(target_path);
            FileUtils.DeleteIfExists(target_path);
            Assert.IsFalse(File.Exists(target_path));
            ierr = wrapper.ionc_create(target_path, ref targetmode, ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.IsTrue(File.Exists(target_path));

            //4. Get the ids of the 1d mesh 2d mesh from the loaded meshes 
            int source1dgeom = -1;
            ierr = wrapper.ionc_get_1d_network_id(ref source1dioncid, ref source1dgeom);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(source1dgeom, Is.EqualTo(1));
            int source1dmesh = -1;
            ierr = wrapper.ionc_get_1d_mesh_id(ref source1dioncid, ref source1dmesh);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(source1dmesh, Is.EqualTo(1));


            int sourcemesh2d = -1;
            ierr = wrapper.ionc_get_2d_mesh_id(ref sourcetwodioncid, ref  sourcemesh2d);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(sourcemesh2d, Is.EqualTo(1));

            //5. Start cloning the data in the new file, first 1d, then the 2d
            int target1dmesh = -1;
            ierr = wrapper.ionc_clone_mesh_definition(ref source1dioncid, ref targetioncid, ref source1dmesh, ref target1dmesh);
            Assert.That(ierr, Is.EqualTo(0));
            int target2dmesh = -1;
            ierr = wrapper.ionc_clone_mesh_definition(ref sourcetwodioncid, ref targetioncid, ref sourcemesh2d, ref target2dmesh);
            Assert.That(ierr, Is.EqualTo(0));

            //6. Clone the mesh data
            ierr = wrapper.ionc_clone_mesh_data(ref source1dioncid, ref targetioncid, ref source1dmesh, ref target1dmesh);
            Assert.That(ierr, Is.EqualTo(0));
            ierr = wrapper.ionc_clone_mesh_data(ref sourcetwodioncid, ref targetioncid, ref sourcemesh2d, ref target2dmesh);
            Assert.That(ierr, Is.EqualTo(0));

            //7. Close all files 
            ierr = wrapper.ionc_close(ref source1dioncid);
            Assert.That(ierr, Is.EqualTo(0));
            ierr = wrapper.ionc_close(ref sourcetwodioncid);
            Assert.That(ierr, Is.EqualTo(0));
            ierr = wrapper.ionc_close(ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));

            //8. Now open the file with cloned meshes and check if the data written there are correct
            targetioncid = -1;  //file id  
            targetmode = 0;     //open in write mode
            ierr = wrapper.ionc_open(target_path, ref targetmode, ref targetioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            //start checking

            //xx. Now DeltaShell edits the 1d file, we need to re-write the target file in a temporary file.
            // We can not delete the target after openenig it, because is needed for retriving the data to be stored in the new file. 
            int newtargetioncid = -1;  //file id  
            int newtargetmode = 1;  //create in write mode
            string newtarget_path = TestHelper.GetTestFilePath(@"ugrid\newtarget.nc");
            newtarget_path = TestHelper.CreateLocalCopy(newtarget_path);
            FileUtils.DeleteIfExists(newtarget_path);
            Assert.IsFalse(File.Exists(newtarget_path));
            ierr = wrapper.ionc_create(newtarget_path, ref targetmode, ref newtargetioncid);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.IsTrue(File.Exists(newtarget_path));


            ////xx. Get the ids from the old target file 
            //source1dgeom = -1;
            //ierr = wrapper.ionc_get_1d_network_id(ref targetioncid, ref source1dgeom);
            //Assert.That(ierr, Is.EqualTo(0));
            //Assert.That(source1dgeom, Is.EqualTo(1));
            //source1dmesh = -1;
            //ierr = wrapper.ionc_get_1d_mesh_id(ref targetioncid, ref source1dmesh);
            //Assert.That(ierr, Is.EqualTo(0));
            //Assert.That(source1dmesh, Is.EqualTo(1));

            //sourcemesh2d = -1;
            //ierr = wrapper.ionc_get_2d_mesh_id(ref targetioncid, ref  sourcemesh2d);
            //Assert.That(ierr, Is.EqualTo(0));
            //Assert.That(sourcemesh2d, Is.EqualTo(2));

            ////xx. Start cloning the data in the new file, first 1d, then the 2d
            //int newtarget1dmesh = -1;
            //ierr = wrapper.ionc_clone_mesh_definition(ref targetioncid, ref newtargetioncid, ref target1dmesh, ref newtarget1dmesh);
            //Assert.That(ierr, Is.EqualTo(0));
            //int newtarget2dmesh = -1;
            //ierr = wrapper.ionc_clone_mesh_definition(ref targetioncid, ref newtargetioncid, ref target2dmesh, ref newtarget2dmesh);
            //Assert.That(ierr, Is.EqualTo(0));

            ////xx. Clone the mesh data in the new target
            //ierr = wrapper.ionc_clone_mesh_data(ref targetioncid, ref newtargetioncid, ref target1dmesh, ref newtarget1dmesh);
            //Assert.That(ierr, Is.EqualTo(0));
            //ierr = wrapper.ionc_clone_mesh_data(ref targetioncid, ref newtargetioncid, ref target2dmesh, ref newtarget2dmesh);
            //Assert.That(ierr, Is.EqualTo(0));

            ////xx. Close the old
            //ierr = wrapper.ionc_close(ref targetioncid);
            //Assert.That(ierr, Is.EqualTo(0));

            ////xx. Check if the new target are correct

        }
    }
}

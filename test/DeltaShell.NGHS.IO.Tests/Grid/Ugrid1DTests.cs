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
        private string[] meshpointsids = { "meshpoint1", "meshpoint2", "meshpoint3", "meshpoint4", "meshpoint5", "meshpoint6", "meshpoint7", "meshpoint8", "meshpoint9", "meshpoint10" };
        private string[] meshpointslongnames = { "meshpointlongname1", "meshpointlongname2", "meshpointlongname3", "meshpointlongname4", "meshpointlongname5", "meshpointlongname6", "meshpointlongname7", "meshpointlongname8", "meshpointlongname9", "meshpointlongname10" };

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

        // mesh2d
        private int numberOf2DNodes = 5;
        private int numberOfFaces = 2;
        private int numberOfMaxFaceNodes = 4;
        private double[] mesh2d_nodesX    = {0, 10, 15, 10, 5};
        private double[] mesh2d_nodesY    = {0, 0, 5, 10, 5};
        private double[,] mesh2d_face_nodes= {{1, 2, 5, -999},{2, 3, 4, 5 }};

        //function to check mesh1d data
        private void check1dmesh(int ioncId, int networkId, ref GridWrapper wrapper) 
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
            IntPtr c_mesh1indexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nlinks);
            IntPtr c_mesh2indexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nlinks);
            try
            {
                //1. Get the node count
                int rnNodes = -1;
                int ierr = wrapper.Get1DNetworkNodesCount(ioncId, networkId, ref rnNodes);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnNodes, Is.EqualTo(nNodes));

                //2. Get the number of branches
                int rnBranches = -1;
                ierr = wrapper.Get1DNetworkBranchesCount(ioncId, networkId, ref rnBranches);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnBranches, Is.EqualTo(nBranches));

                //3. Get the number of geometry points
                int rnGeometry = -1;
                ierr = wrapper.Get1DNetworkBranchesGeometryCoordinateCount(ioncId, networkId,
                    ref rnGeometry);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnGeometry, Is.EqualTo(nGeometry));

                //4. Get nodes info and coordinates
                GridWrapper.interop_charinfo[] nodesinfo = new GridWrapper.interop_charinfo[4];
                ierr = wrapper.Read1DNetworkNodes(ioncId, networkId, ref c_nodesX, ref c_nodesY,
                    nodesinfo, rnNodes);
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

                //5. Get the branch info and coordinates
                GridWrapper.interop_charinfo[] branchinfo = new GridWrapper.interop_charinfo[3];
                ierr = wrapper.Read1DNetworkBranches(ioncId, networkId, ref c_sourcenodeid,
                    ref c_targetnodeid,
                    ref c_branchlengths, branchinfo, ref c_nbranchgeometrypoints, rnBranches);
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

                //6. Get the 1d branch geometry
                ierr = wrapper.Read1DNetworkBranchesGeometry(ioncId, networkId, ref c_geopointsX,
                    ref c_geopointsY, rnGeometry);
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

                //7. Get the mesh name
                var rnetworkName = new StringBuilder(GridApiDataSet.GridConstants.MAXSTRLEN);
                ierr = wrapper.GetMeshName(ioncId, networkId, rnetworkName);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnetworkName.ToString().Trim(), Is.EqualTo(meshname));

                //8. Get the number of mesh points
                int rnmeshpoints = -1;
                ierr =
                    wrapper.Get1DMeshDiscretisationPointsCount(ioncId, networkId,
                        ref rnmeshpoints);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnmeshpoints, Is.EqualTo(nmeshpoints));

                //9. Get the coordinates of the mesh points
                GridWrapper.interop_charinfo[] meshpointsinfo = new GridWrapper.interop_charinfo[rnmeshpoints];
                ierr = wrapper.Read1DMeshDiscretisationPoints(ioncId, networkId, ref c_branchidx,
                    ref c_offset, meshpointsinfo, rnmeshpoints);
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

                //10. Get the number of links. 
                int linkmesh = 1;
                int r_nlinks = -1;
                ierr = wrapper.get_contacts_count(ref ioncId, ref linkmesh, ref r_nlinks);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(r_nlinks, Is.EqualTo(nlinks));
                GridWrapper.interop_charinfo[] linksinfo = new GridWrapper.interop_charinfo[nlinks];

                //11. Get the links values
                ierr = wrapper.get_mesh_contact(ref ioncId, ref linkmesh, ref c_mesh1indexes, ref c_mesh2indexes,
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

        private void check2dmesh(int ioncId, int meshId, ref GridWrapper wrapper)
        {
            IntPtr c_nodesX = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOf2DNodes);
            IntPtr c_nodesY = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOf2DNodes);
            IntPtr c_face_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfFaces * numberOfMaxFaceNodes);
            try
            {

            int nnodes = -1;
            int ierr = wrapper.GetNodeCount(ioncId, meshId, ref  nnodes);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(nnodes, Is.EqualTo(5));

            int nedge = -1;
            ierr = wrapper.GetEdgeCount(ioncId, meshId, ref  nedge);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(nedge, Is.EqualTo(6));

            int nface = -1;
            ierr = wrapper.GetFaceCount(ioncId, meshId, ref  nface);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(nface, Is.EqualTo(numberOfFaces));

            int maxfacenodes = -1;
            ierr = wrapper.GetMaxFaceNodes(ioncId, meshId, ref  maxfacenodes);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(maxfacenodes, Is.EqualTo(numberOfMaxFaceNodes));

            //get all node coordinates
            ierr = wrapper.GetNodeCoordinates(ioncId, meshId, ref c_nodesX, ref c_nodesY, nnodes);
            Assert.That(ierr, Is.EqualTo(0));

            double[] rc_nodeX = new double[numberOf2DNodes];
            double[] rc_nodeY = new double[numberOf2DNodes];
            Marshal.Copy(c_nodesX, rc_nodeX, 0, nnodes);
            Marshal.Copy(c_nodesY, rc_nodeY, 0, nnodes);
            for (int i = 0; i < nnodes; i++)
            {
                Assert.That(rc_nodeX[i], Is.EqualTo(mesh2d_nodesX[i]));
                Assert.That(rc_nodeY[i], Is.EqualTo(mesh2d_nodesY[i]));
            }
       
            //Check face nodes
            int fillvalue = -1;
            ierr = wrapper.GetFaceNodes(ioncId, meshId, ref c_face_nodes, nface,
                    maxfacenodes, ref fillvalue);
            Assert.That(ierr, Is.EqualTo(0));
            int[] rc_face_nodes = new int[nface * maxfacenodes];
            Marshal.Copy(c_face_nodes, rc_face_nodes, 0, nface * maxfacenodes);
            int ind = 0;
            for (int i = 0; i < nface; i++)
            {
                for (int j = 0; j < maxfacenodes; j++)
                {
                    Assert.That(rc_face_nodes[ind], Is.EqualTo(mesh2d_face_nodes[i,j]));
                    ind += 1;
                }
            }

            }
            finally
            {
                Marshal.FreeCoTaskMem(c_nodesX);
                Marshal.FreeCoTaskMem(c_nodesY);
                Marshal.FreeCoTaskMem(c_face_nodes);
            }
        }

        private void write1dnetworkandmesh(int ioncId, int networkId, ref GridWrapper wrapper)
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

                int ierr = -1;
                string tmpstring;
                //1. Write 1d network network nodes
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
                ierr = wrapper.Write1DNetworkNodes(ioncId, networkId, c_nodesX, c_nodesY,
                    nodesinfo, nNodes);
                Assert.That(ierr, Is.EqualTo(0));

                //2. Write 1d network branches
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
                ierr = wrapper.Write1DNetworkBranches(ioncId, networkId, c_sourcenodeid,
                    c_targetnodeid, branchinfo, c_branchlengths, c_nbranchgeometrypoints, nBranches);
                Assert.That(ierr, Is.EqualTo(0));

                //3. Write 1d network geometry
                Marshal.Copy(geopointsX, 0, c_geopointsX, nGeometry);
                Marshal.Copy(geopointsY, 0, c_geopointsY, nGeometry);
                ierr = wrapper.Write1DNetworkBranchesGeometry(ioncId, networkId, c_geopointsX,
                    c_geopointsY, nGeometry);
                Assert.That(ierr, Is.EqualTo(0));

                //4. Write the 1d mesh topology.
                //Assume mesh and network are saved in the same table 
                int meshId = networkId;
                ierr = wrapper.Create1DMesh(ioncId, networkId, ref meshId, meshname, nmeshpoints,
                    nmeshedges);
                Assert.That(ierr, Is.EqualTo(0));

                //5. Write the 1d mesh geometry
                Marshal.Copy(branchidx, 0, c_branchidx, nmeshpoints);
                Marshal.Copy(offset, 0, c_offset, nmeshpoints);
                GridWrapper.interop_charinfo[] meshpointsinfo = new GridWrapper.interop_charinfo[nmeshpoints];
                for (int i = 0; i < nmeshpoints; i++)
                {
                    tmpstring = meshpointsids[i];
                    tmpstring = tmpstring.PadRight(GridWrapper.idssize, ' ');
                    meshpointsinfo[i].ids = tmpstring.ToCharArray();
                    tmpstring = meshpointslongnames[i];
                    tmpstring = tmpstring.PadRight(GridWrapper.longnamessize, ' ');
                    meshpointsinfo[i].longnames = tmpstring.ToCharArray();
                }
                ierr = wrapper.Write1DMeshDiscretisationPoints(ioncId, meshId, c_branchidx,
                    c_offset, meshpointsinfo, nmeshpoints);
                Assert.That(ierr, Is.EqualTo(0));

                //6. Write links attributes
                Marshal.Copy(mesh1indexes, 0, c_mesh1indexes, nlinks);
                Marshal.Copy(mesh2indexes, 0, c_mesh2indexes, nlinks);
                int linkmesh = -1;
                ierr = wrapper.def_mesh_contact(ref ioncId, ref linkmesh, linkmeshname, ref nlinks, ref linkmesh1,
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

                //7. Write the mesh links
                ierr = wrapper.put_mesh_contact(ref ioncId, ref linkmesh, ref c_mesh1indexes, ref c_mesh2indexes,
                    linksinfo, ref nlinks);
                Assert.That(ierr, Is.EqualTo(0));
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

        private void addglobalattributes(int ioncId, ref GridWrapper wrapper)
        {
            string tmpstring;
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
            int ierr = wrapper.AddGlobalAttributes(ioncId, metadata);
            Assert.That(ierr, Is.EqualTo(0));
        }

        static Ugrid1DTests()
        {
            NativeLibrary.LoadNativeDll(GridApiDataSet.GRIDDLL_NAME, DimrApiDataSet.SharedDllPath);
        }

        //////create the netcdf files
        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void create1dUGRIDNetcdf()
        { 
            
                //1. Create a netcdf file 
                int ioncId = 0; //file variable 
                int mode = 1; //create in write mode
                var ierr = -1;
                string c_path = TestHelper.GetTestFilePath(@"ugrid\write1d.nc");
                c_path = TestHelper.CreateLocalCopy(c_path);
                FileUtils.DeleteIfExists(c_path);
                Assert.IsFalse(File.Exists(c_path));
                var wrapper = new GridWrapper();

                //2. Create the file, will not add any dataset
                ierr = wrapper.create(c_path, mode, ref ioncId);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.IsTrue(File.Exists(c_path));

                //3. For reading the grid later on we need to add metadata to the netcdf file. 
                //   The function AddGlobalAttributes adds to the netCDF file the UGRID convention
                addglobalattributes(ioncId, ref wrapper);

                //4. Create a 1d network
                int networkId = -1;
                ierr = wrapper.Create1DNetwork(ioncId, ref networkId, networkName, nNodes,
                    nBranches, nGeometry);
                Assert.That(ierr, Is.EqualTo(0));

                //5. Write the 1d network and mesh
                write1dnetworkandmesh(ioncId, networkId, ref wrapper);

                //6. Close the file
                wrapper.Close(ioncId);
        }

        ////// read the netcdf file created in the test above
        [Test]
        [Ignore("should be in unit test of io_netcdf kernel")]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void read1dUGRIDNetcdf()
        {
            //1. Open a netcdf file 
            string c_path = TestHelper.GetTestFilePath(@"ugrid\write1d.nc");
            c_path = TestHelper.CreateLocalCopy(c_path);
            Assert.IsTrue(File.Exists(c_path));
            int ioncId = 0; //file variable
            int mode = 0; //create in read mode
            var wrapper = new GridWrapper();
            var ierr = wrapper.Open(c_path, mode, ref ioncId, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            //2. Get the 1D network and mesh ids
            int networkId = -1;
            ierr = wrapper.Get1DNetworkId(ioncId, ref networkId);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(networkId, Is.EqualTo(1));
            int meshId = -1;
            ierr = wrapper.Get1DMeshId(ioncId, ref meshId);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(meshId, Is.EqualTo(1));

            //3. Check if all 1d data written in the file are correct
            check1dmesh(ioncId, networkId, ref wrapper);

            //4. Close the file
            ierr = wrapper.Close(ioncId);
        }

        // Deltashell creates a new file to write the 1d geometry and mesh as in the first test create1dUGRIDNetcdf
        // and clones the 2d mesh data read from a file produced by RGFgrid. 
        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void deltaShellClones2dMesh()
        {
            var wrapper = new GridWrapper();

            //1. RGF grid creates a 2d mesh. The info is in memory, here simulated by opening a file containing a mesh2d
            // and by reading all data in
            string sourcetwod_path = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            sourcetwod_path = TestHelper.CreateLocalCopy(sourcetwod_path);
            Assert.IsTrue(File.Exists(sourcetwod_path));
            int sourcetwodioncid = -1;   //file id 
            int sourcetwomode    =  0;   //read mode
            int ierr = wrapper.Open(sourcetwod_path, sourcetwomode, ref sourcetwodioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            //2. Now we create a new empty file where to save 1d and 2d meshes
            int targetioncid   = -1;  //file id  
            int targetmode     =  1;  //create in write mode
            string target_path = TestHelper.GetTestFilePath(@"ugrid\target.nc");
            target_path = TestHelper.CreateLocalCopy(target_path);
            FileUtils.DeleteIfExists(target_path);
            Assert.IsFalse(File.Exists(target_path));
            
            //4. Create the file
            ierr = wrapper.create(target_path, targetmode, ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.IsTrue(File.Exists(target_path));

            //3. Add global attributes in the file
            addglobalattributes(targetioncid, ref wrapper);

            //4. Get the id of the 2d mesh in the RGF grid file (Custom_Ugrid.nc)
            int sourcemesh2d = -1;
            ierr = wrapper.Get2DMeshId(ref sourcetwodioncid, ref  sourcemesh2d);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(sourcemesh2d, Is.EqualTo(1));

            //5. Create 1d geometry and mesh in the new file (target.nc)
            int networkId = -1;
            ierr = wrapper.Create1DNetwork(targetioncid, ref networkId, networkName, nNodes,
                nBranches, nGeometry);
            Assert.That(ierr, Is.EqualTo(0));

            //6. Write the 1d data in the new file (1d geometry, mesh and links)
            write1dnetworkandmesh(targetioncid, networkId, ref wrapper);

            //7. Clone the 2d mesh definitions in the new file
            int target2dmesh = -1;
            ierr = wrapper.clone_mesh_definition(ref sourcetwodioncid, ref targetioncid, ref sourcemesh2d, ref target2dmesh);
            Assert.That(ierr, Is.EqualTo(0));

            //8. Clone the 2d mesh data
            ierr = wrapper.clone_mesh_data(ref sourcetwodioncid, ref targetioncid, ref sourcemesh2d, ref target2dmesh);
            Assert.That(ierr, Is.EqualTo(0));

            //9. Close all files 
            ierr = wrapper.Close(sourcetwodioncid);
            Assert.That(ierr, Is.EqualTo(0));
            ierr = wrapper.Close(targetioncid);
            Assert.That(ierr, Is.EqualTo(0));

            //10. Now open the file with cloned meshes and check if the data written there are correct
            targetioncid = -1;  //file id  
            targetmode = 0;     //open in write mode
            ierr = wrapper.Open(target_path, targetmode, ref targetioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            //11. Check 2 meshes are present
            int nmesh = -1;
            ierr = wrapper.GetMeshCount(targetioncid, ref nmesh);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(nmesh, Is.EqualTo(2));

            //12. Get the mesh ids
            int source1dnetwork = -1;
            ierr = wrapper.Get1DNetworkId(targetioncid, ref source1dnetwork);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(source1dnetwork, Is.EqualTo(1));
            int source1dmesh = -1;
            ierr = wrapper.Get1DMeshId(targetioncid, ref source1dmesh);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(source1dmesh, Is.EqualTo(1));
            sourcemesh2d = -1;
            ierr = wrapper.Get2DMeshId(ref targetioncid, ref  sourcemesh2d);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(sourcemesh2d, Is.EqualTo(2));

            //13. Check all 1d and 2d data
            check1dmesh(targetioncid, source1dnetwork, ref wrapper);
            check2dmesh(targetioncid, sourcemesh2d, ref wrapper);

            //14. Close the file
            ierr = wrapper.Close(targetioncid);
            Assert.That(ierr, Is.EqualTo(0));
        }
    }
}

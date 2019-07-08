using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using DelftTools.TestUtils;
using DelftTools.Utils.Interop;
using DeltaShell.NGHS.IO.Grid;
using General.tests;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    [Ignore("should be in unit test of io_netcdf kernel")]
    public class NewUGridTests
    {
        //Constructor loads the library
        static NewUGridTests()
        {
            Dimr.DimrApiDataSet.SetSharedPath();

            string filename = Path.Combine(Dimr.DimrApiDataSet.SharedDllPath, "io_netcdf.dll");
            m_libnetcdf = NativeLibrary.LoadLibrary(filename);
            //we should chek the pointer is not null
            Assert.That(m_libnetcdf, Is.Not.Null);
            filename = Path.Combine(Dimr.DimrApiDataSet.SharedDllPath, "gridgeom.dll");
            m_libgridgeom = NativeLibrary.LoadLibrary(filename);
            Assert.That(m_libgridgeom, Is.Not.Null);
        }

        //pointer to the loaded dll
        public static IntPtr m_libnetcdf;
        public static IntPtr m_libgridgeom;

        //network name
        private string networkName = "network";

        //dimension info
        private int nNodes = 4;
        private int nBranches = 3;
        private int nGeometry = 9;
        private int startIndex = 1;

        //node info
        private double[] nodesX = { 1.0, 5.0, 5.0, 8.0 };
        private double[] nodesY = { 4.0, 4.0, 1.0, 4.0 };
        private string[] nodesids = { "node1", "node2", "node3", "node4" };
        private string[] nodeslongNames = { "nodelong1", "nodelong2", "nodelong3", "nodelong4" };
        private int[] sourcenodeid = { 1, 2, 2 };
        private int[] targetnodeid = { 2, 3, 4 };

        //branches info
        private double[] branchlengths = { 4.0, 3.0, 3.0 };
        private int[] nbranchgeometrypoints = { 3, 3, 3 };
        private string[] branchids = { "branch1", "branch2", "branch3" };
        private string[] branchlongNames = { "branchlong1", "branchlong2", "branchlong3" };
        private int[] branch_order = { -1, -1, -1 };

        //geometry info
        double[] geopointsX = { 1.0, 3.0, 5.0, 5.0, 5.0, 5.0, 5.0, 7.0, 8.0 };
        double[] geopointsY = { 4.0, 4.0, 4.0, 4.0, 2.0, 1.0, 4.0, 4.0, 4.0 };

        //mesh name
        private string meshname = "1dmesh";

        //mesh dimension
        private int nmeshpoints = 8;
        private int nedgenodes = 7; // nmeshpoints - 1 

        //mesh geometry
        private int nmesh1dPoints = 8;
        private int[] branchidx = { 1, 1, 1, 1, 2, 2, 3, 3 };
        private double[] offset = { 0.0, 2.0, 3.0, 4.0, 1.5, 3.0, 1.5, 3.0 };
        private double[] mesh1dCoordX = { 1, 1, 1, 1, 2, 2, 3, 3 };
        private double[] mesh1dCoordY = { 1, 1, 1, 1, 2, 2, 3, 3 };

        private string[] meshnodeids = { "node1_branch1", "node2_branch1", "node3_branch1", "node4_branch1", "node1_branch2",
            "node2_branch2", "node3_branch2", "node1_branch3", "node2_branch3", "node3_branch3" };

        private string[] meshnodelongnames = { "node1_branch1_long_name", "node2_branch1_long_name", "node3_branch1_long_name", "node4_branch1_long_name", "node1_branch2_long_name",
            "node2_branch2_long_name", "node3_branch2_long_name", "node1_branch3_long_name", "node2_branch3_long_name", "node3_branch3_long_name" };

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
        private int[] mesh1indexes = { 1, 2, 3 };
        private int[] mesh2indexes = { 1, 2, 3 };
        private string[] linksids = { "link1", "link2", "link3" };
        private string[] linkslongnames = { "linklong1", "linklong2", "linklong3" };
        private int[] contacttype = { 3, 3, 3 };

        // mesh2d
        private int numberOf2DNodes = 5;
        private int numberOfFaces = 2;
        private int numberOfMaxFaceNodes = 4;
        private double[] mesh2d_nodesX = { 0, 10, 15, 10, 5 };
        private double[] mesh2d_nodesY = { 0, 0, 5, 10, 5 };
        private double[,] mesh2d_face_nodes = { { 1, 2, 5, -999 }, { 2, 3, 4, 5 } };

        
        //function to check mesh1d data
        private void read1dnetwork
        (
            int ioncid,
            int networkid,
            ref IoNetcdfLibWrapper wrapper,
            ref int l_nnodes,
            ref int l_nbranches,
            ref int l_nGeometry,
            int l_startIndex,
            ref StringBuilder l_networkName,
            ref IntPtr nodeIds,
            ref IntPtr nodeLongnames,
            ref IntPtr branchIds,
            ref IntPtr branchLongnames,
            ref double[] l_nodesX,
            ref double[] l_nodesY,
            ref int[] l_sourcenodeid,
            ref int[] l_targetnodeid,
            ref double[] l_branchlengths,
            ref int[] l_nbranchgeometrypoints,
            ref double[] l_geopointsX,
            ref double[] l_geopointsY,
            ref int[] l_branch_order
        )
        {
            // Get the mesh name
            int ierr = -1;
            ierr = wrapper.ionc_get_1d_network_name(ref ioncid, ref networkid, l_networkName);
            Assert.That(ierr, Is.EqualTo(0));

            // Get the node count
            ierr = wrapper.ionc_get_1d_network_nodes_count(ref ioncid, ref networkid, ref l_nnodes);
            Assert.That(ierr, Is.EqualTo(0));
            IntPtr c_nodesX = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nnodes);
            IntPtr c_nodesY = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nnodes);

            // Get the number of branches
            ierr = wrapper.ionc_get_1d_network_branches_count(ref ioncid, ref networkid, ref l_nbranches);
            Assert.That(ierr, Is.EqualTo(0));
            IntPtr c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nbranches);
            IntPtr c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nbranches);
            IntPtr c_branchlengths = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nbranches);
            IntPtr c_nbranchgeometrypoints = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nbranches);
            IntPtr c_branch_order = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nbranches);

            // Get the number of geometry points
            ierr = wrapper.ionc_get_1d_network_branches_geometry_coordinate_count(ref ioncid, ref networkid, ref l_nGeometry);
            Assert.That(ierr, Is.EqualTo(0));
            IntPtr c_geopointsX = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nGeometry);
            IntPtr c_geopointsY = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nGeometry);

            // Get nodes info and coordinates
            ierr = wrapper.ionc_read_1d_network_nodes(ref ioncid, ref networkid, ref c_nodesX, ref c_nodesY, ref nodeIds, ref nodeLongnames, ref l_nnodes);
            Assert.That(ierr, Is.EqualTo(0));
            Marshal.Copy(c_nodesX, l_nodesX, 0, l_nodesX.Length);
            Marshal.Copy(c_nodesY, l_nodesY, 0, l_nodesY.Length);

            // Get the branch info and coordinates
            ierr = wrapper.ionc_get_1d_network_branches(ref ioncid, ref networkid, ref c_sourcenodeid, ref c_targetnodeid, ref c_branchlengths, ref branchIds, ref branchLongnames, ref c_nbranchgeometrypoints, ref l_nbranches, ref l_startIndex);
            Assert.That(ierr, Is.EqualTo(0));
            Marshal.Copy(c_targetnodeid, l_targetnodeid, 0, l_targetnodeid.Length);
            Marshal.Copy(c_sourcenodeid, l_sourcenodeid, 0, l_sourcenodeid.Length);
            Marshal.Copy(c_branchlengths, l_branchlengths, 0, l_branchlengths.Length);
            Marshal.Copy(c_nbranchgeometrypoints, l_nbranchgeometrypoints, 0, l_nbranchgeometrypoints.Length);

            // Get the 1d branch geometry
            ierr = wrapper.ionc_read_1d_network_branches_geometry(ref ioncid, ref networkid, ref c_geopointsX,
                ref c_geopointsY, ref l_nGeometry);
            Assert.That(ierr, Is.EqualTo(0));
            Marshal.Copy(c_geopointsX, l_geopointsX, 0, l_geopointsX.Length);
            Marshal.Copy(c_geopointsY, l_geopointsY, 0, l_geopointsY.Length);

            // Get the branch order 
            ierr = wrapper.ionc_get_1d_network_branchorder(ref ioncid, ref networkid, ref c_branch_order, ref nBranches);
            Assert.That(ierr, Is.EqualTo(0));

            Marshal.Copy(c_branch_order, l_branch_order, 0, l_branch_order.Length);

            //free all pointers
            Marshal.FreeCoTaskMem(c_nodesX);
            Marshal.FreeCoTaskMem(c_nodesY);
            Marshal.FreeCoTaskMem(c_sourcenodeid);
            Marshal.FreeCoTaskMem(c_targetnodeid);
            Marshal.FreeCoTaskMem(c_branchlengths);
            Marshal.FreeCoTaskMem(c_nbranchgeometrypoints);
            Marshal.FreeCoTaskMem(c_geopointsX);
            Marshal.FreeCoTaskMem(c_geopointsY);
            Marshal.FreeCoTaskMem(c_branch_order);
        }

        //function to check mesh1d data
        private void check1dmesh(int ioncid, int meshid, ref IoNetcdfLibWrapper wrapper)
        {
            //mesh variables
            IntPtr c_branchidx = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nmeshpoints);
            IntPtr c_offset = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nmeshpoints);
            //links variables
            IntPtr c_mesh1indexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nlinks);
            IntPtr c_mesh2indexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nlinks);
            IntPtr c_contacttype = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nlinks);
            try
            {
                // Get the mesh name
                int ierr = -1;
                var rmeshName = new StringBuilder(IoNetcdfLibWrapper.LibDetails.MAXSTRLEN);
                ierr = wrapper.ionc_get_mesh_name(ref ioncid, ref meshid, rmeshName);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rmeshName.ToString().Trim(), Is.EqualTo(meshname));

                // Get the number of mesh points
                int rnmeshpoints = -1;
                ierr = wrapper.ionc_get_1d_mesh_discretisation_points_count(ref ioncid, ref meshid, ref rnmeshpoints);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(rnmeshpoints, Is.EqualTo(nmeshpoints));

                // Get the coordinates of the mesh points
                var register = new UnmanagedMemoryRegister();
                var meshNodeIdsBuffer = StringBufferHandling.MakeStringBuffer(rnmeshpoints, IoNetcdfLibWrapper.idssize);
                var meshNodeLongNamesBuffer = StringBufferHandling.MakeStringBuffer(rnmeshpoints, IoNetcdfLibWrapper.longnamessize);
                IntPtr meshNodeIdsPtr = register.AddString(ref meshNodeIdsBuffer);
                IntPtr meshNodeLongNamesPtr = register.AddString(ref meshNodeLongNamesBuffer);

                ierr = wrapper.ionc_get_1d_mesh_discretisation_points(ref ioncid, ref meshid, ref c_branchidx,
                    ref c_offset, ref meshNodeIdsPtr, ref meshNodeLongNamesPtr, ref rnmeshpoints, ref startIndex);

                Assert.That(ierr, Is.EqualTo(0));

                var l_meshNodeIds = StringBufferHandling.ParseString(meshNodeIdsPtr, rnmeshpoints, IoNetcdfLibWrapper.idssize);
                var l_meshNodeLongNames = StringBufferHandling.ParseString(meshNodeLongNamesPtr, rnmeshpoints, IoNetcdfLibWrapper.longnamessize);

                for (int i = 0; i < l_meshNodeIds.Count(); i++)
                {
                    Assert.That(l_meshNodeIds[i].Trim(), Is.EqualTo(meshnodeids[i]));
                }

                for (int i = 0; i < l_meshNodeLongNames.Count(); i++)
                {
                    Assert.That(l_meshNodeLongNames[i].Trim(), Is.EqualTo(meshnodelongnames[i]));
                }

                int[] rc_branchidx = new int[rnmeshpoints];
                double[] rc_offset = new double[rnmeshpoints];
                Marshal.Copy(c_branchidx, rc_branchidx, 0, rnmeshpoints);
                Marshal.Copy(c_offset, rc_offset, 0, rnmeshpoints);
                for (int i = 0; i < rnmeshpoints; i++)
                {
                    Assert.That(rc_branchidx[i], Is.EqualTo(branchidx[i]));
                    Assert.That(rc_offset[i], Is.EqualTo(offset[i]));
                }

                //4. Get the number of links. TODO: add a mesh 2d for this to work!
                //int linkmesh = 1;
                //int r_nlinks = -1;
                //ierr = wrapper.ionc_get_contacts_count(ref ioncid, ref linkmesh, ref r_nlinks);
                //Assert.That(ierr, Is.EqualTo(0));
                //Assert.That(r_nlinks, Is.EqualTo(nlinks));
                //IoNetcdfLibWrapper.interop_charinfo[] linksinfo = new IoNetcdfLibWrapper.interop_charinfo[nlinks];

                ////5. Get the links values
                //ierr = wrapper.ionc_get_mesh_contact(ref ioncid, ref linkmesh, ref c_mesh1indexes, ref c_mesh2indexes, ref c_contacttype,
                //    linksinfo, ref nlinks, ref startIndex);
                //Assert.That(ierr, Is.EqualTo(0));
                //int[] rc_contacttype = new int[nlinks];
                //int[] rc_mesh1indexes = new int[nlinks];
                //int[] rc_mesh2indexes = new int[nlinks];
                //Marshal.Copy(c_mesh1indexes, rc_mesh1indexes, 0, nlinks);
                //Marshal.Copy(c_mesh2indexes, rc_mesh2indexes, 0, nlinks);
                //Marshal.Copy(c_contacttype, rc_contacttype, 0, nlinks);
                //for (int i = 0; i < nlinks; i++)
                //{
                //    string tmpstring = new string(linksinfo[i].ids);
                //    Assert.That(tmpstring.Trim(), Is.EqualTo(linksids[i]));
                //    tmpstring = new string(linksinfo[i].longnames);
                //    Assert.That(tmpstring.Trim(), Is.EqualTo(linkslongnames[i]));
                //    Assert.That(rc_mesh1indexes[i], Is.EqualTo(mesh1indexes[i]));
                //    Assert.That(rc_mesh2indexes[i], Is.EqualTo(mesh2indexes[i]));
                //    Assert.That(rc_contacttype[i], Is.EqualTo(contacttype[i]));
                //}

                //6. Get the written nodes ids
                //StringBuilder varname = new StringBuilder("node_ids");
                //IoNetcdfLibWrapper.interop_charinfo[] nodeidsvalues = new IoNetcdfLibWrapper.interop_charinfo[nmesh1dPoints];

                //ierr = wrapper.ionc_get_var_chars(ref ioncid, ref meshid, varname, nodeidsvalues, ref nmesh1dPoints);
                //Assert.That(ierr, Is.EqualTo(0));
                //for (int i = 0; i < nmesh1dPoints; i++)
                //{
                //    string tmpstring = new string(nodeidsvalues[i].ids);
                //    Assert.That(tmpstring.Trim(), Is.EqualTo(meshnodeids[i]));
                //}
            }
            finally
            {
                Marshal.FreeCoTaskMem(c_branchidx);
                Marshal.FreeCoTaskMem(c_offset);
                Marshal.FreeCoTaskMem(c_mesh1indexes);
                Marshal.FreeCoTaskMem(c_mesh2indexes);
            }
        }

        private void check2dmesh(int ioncid, int meshid, ref IoNetcdfLibWrapper wrapper)
        {
            IntPtr c_nodesX = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOf2DNodes);
            IntPtr c_nodesY = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * numberOf2DNodes);
            IntPtr c_face_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numberOfFaces * numberOfMaxFaceNodes);
            try
            {

                int nnodes = -1;
                int ierr = wrapper.ionc_get_node_count(ref ioncid, ref meshid, ref nnodes);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(nnodes, Is.EqualTo(5));

                int nedge = -1;
                ierr = wrapper.ionc_get_edge_count(ref ioncid, ref meshid, ref nedge);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(nedge, Is.EqualTo(6));

                int nface = -1;
                ierr = wrapper.ionc_get_face_count(ref ioncid, ref meshid, ref nface);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(nface, Is.EqualTo(numberOfFaces));

                int maxfacenodes = -1;
                ierr = wrapper.ionc_get_max_face_nodes(ref ioncid, ref meshid, ref maxfacenodes);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(maxfacenodes, Is.EqualTo(numberOfMaxFaceNodes));

                //get all node coordinates
                ierr = wrapper.ionc_get_node_coordinates(ref ioncid, ref meshid, ref c_nodesX, ref c_nodesY, ref nnodes);
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
                ierr = wrapper.ionc_get_face_nodes(ref ioncid, ref meshid, ref c_face_nodes, ref nface, ref maxfacenodes, ref fillvalue, ref startIndex);
                Assert.That(ierr, Is.EqualTo(0));
                int[] rc_face_nodes = new int[nface * maxfacenodes];
                Marshal.Copy(c_face_nodes, rc_face_nodes, 0, nface * maxfacenodes);
                int ind = 0;
                for (int i = 0; i < nface; i++)
                {
                    for (int j = 0; j < maxfacenodes; j++)
                    {
                        Assert.That(rc_face_nodes[ind], Is.EqualTo(mesh2d_face_nodes[i, j]));
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

        private void write1dmesh(
            int ioncid,
            ref StringBuilder l_networkName,
            ref StringBuilder l_meshname,
            ref IoNetcdfLibWrapper wrapper,
            ref IntPtr meshNodeIds,
            ref IntPtr meshNodesLongNames,
            int l_nmeshpoints,
            int l_nedgenodes,
            int l_nBranches,
            int l_nlinks,
            ref double[] l_branchoffset,
            ref double[] l_branchlength,
            ref int[] l_branchidx,
            ref int[] l_sourcenodeid,
            ref int[] l_targetnodeid,
            int l_startIndex,
            ref double[] l_mesh1dCoordX,
            ref double[] l_mesh1dCoordY
        )
        {
            // Mesh variables
            IntPtr c_branchidx = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nmeshpoints);
            IntPtr c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nBranches);
            IntPtr c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nBranches);
            IntPtr c_branchoffset = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nmeshpoints);
            IntPtr c_branchlength = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nBranches);
            IntPtr c_mesh1dCoordX = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nmeshpoints);
            IntPtr c_mesh1dCoordY = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nmeshpoints);
            // Links variables
            IntPtr c_mesh1indexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nlinks);
            IntPtr c_mesh2indexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nlinks);
            IntPtr c_contacttype = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nlinks);
            try
            {
                string tmpstring;
                int meshid = -1;
                int writexy = 1;

                //1. Create: the assumption here is that l_nedgenodes is known (we could move this calculation inside ionc_create_1d_mesh)
                int ierr = wrapper.ionc_create_1d_mesh_v1(ref ioncid, l_networkName.ToString(), ref meshid, l_meshname.ToString(), ref l_nmeshpoints, ref writexy);
                Assert.That(ierr, Is.EqualTo(0));

                //2. Create the edge nodes (the algorithm is in gridgeom.dll, not in ionetcdf.dll)
                Marshal.Copy(l_branchidx, 0, c_branchidx, l_nmeshpoints);
                Marshal.Copy(l_sourcenodeid, 0, c_sourcenodeid, l_nBranches);
                Marshal.Copy(l_targetnodeid, 0, c_targetnodeid, l_nBranches);
                Marshal.Copy(l_branchoffset, 0, c_branchoffset, l_nmeshpoints);
                Marshal.Copy(l_branchlength, 0, c_branchlength, l_nBranches);
                Marshal.Copy(l_mesh1dCoordX, 0, c_mesh1dCoordX, l_nmeshpoints);
                Marshal.Copy(l_mesh1dCoordY, 0, c_mesh1dCoordY, l_nmeshpoints);

                //3. Write the discretization points
                ierr = wrapper.ionc_put_1d_mesh_discretisation_points_v1(ref ioncid, ref meshid, ref c_branchidx, ref c_branchoffset, ref meshNodeIds, ref meshNodesLongNames, ref l_nmeshpoints, ref l_startIndex, ref c_mesh1dCoordX, ref c_mesh1dCoordY);
                Assert.That(ierr, Is.EqualTo(0));
            }
            finally
            {
                Marshal.FreeCoTaskMem(c_branchidx);
                Marshal.FreeCoTaskMem(c_branchoffset);
                Marshal.FreeCoTaskMem(c_mesh1indexes);
                Marshal.FreeCoTaskMem(c_mesh2indexes);
                Marshal.FreeCoTaskMem(c_sourcenodeid);
                Marshal.FreeCoTaskMem(c_targetnodeid);
                Marshal.FreeCoTaskMem(c_contacttype);
            }
        }

        public void write1d2dlinks(
            int ioncid,
            int l_linkmesh1,
            int l_linkmesh2,
            int l_locationType1,
            int l_locationType2,
            ref StringBuilder l_linkmeshname,
            ref IoNetcdfLibWrapper wrapper,
            int l_nlinks,
            ref int[] l_mesh1indexes,
            ref int[] l_mesh2indexes,
            ref int[] l_contacttype,
            int l_startIndex)
        {
            IntPtr c_mesh1indexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nlinks);
            IntPtr c_mesh2indexes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nlinks);
            IntPtr c_contacttype = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nlinks);
            try
            {
                // 1. Define the mesh with the contacts
                int linkmesh = -1;
                int ierr = wrapper.ionc_def_mesh_contact(ref ioncid, ref linkmesh, l_linkmeshname.ToString(), ref l_nlinks, ref l_linkmesh1,
                    ref l_linkmesh2, ref l_locationType1, ref l_locationType2);
                Assert.That(ierr, Is.EqualTo(0));

                // 2. Put the contacts in thge defined mesh
                Marshal.Copy(l_mesh1indexes, 0, c_mesh1indexes, l_nlinks);
                Marshal.Copy(l_mesh2indexes, 0, c_mesh2indexes, l_nlinks);
                Marshal.Copy(l_contacttype, 0, c_contacttype, l_nlinks);

                var register = new UnmanagedMemoryRegister();
                var linkIdsBuffer = StringBufferHandling.MakeStringBuffer(ref linksids, IoNetcdfLibWrapper.idssize);
                var linkLongNamesBuffer = StringBufferHandling.MakeStringBuffer(ref linkslongnames, IoNetcdfLibWrapper.longnamessize);
                IntPtr linkIdsPtr = register.AddString(ref linkIdsBuffer);
                IntPtr linkLongNamesPtr = register.AddString(ref linkLongNamesBuffer);

                ierr = wrapper.ionc_put_mesh_contact(ref ioncid, ref linkmesh, ref c_mesh1indexes, ref c_mesh2indexes, ref c_contacttype, ref linkIdsPtr, ref linkLongNamesPtr, ref l_nlinks, ref l_startIndex);
                Assert.That(ierr, Is.EqualTo(0));
            }
            finally
            {
                Marshal.FreeCoTaskMem(c_mesh1indexes);
                Marshal.FreeCoTaskMem(c_mesh2indexes);
                Marshal.FreeCoTaskMem(c_contacttype);
            }
        }


        // writes a network from the arrays 
        private void write1dnetwork(
            int ioncid,
            int networkid,
            ref IoNetcdfLibWrapper wrapper,
            int l_nnodes,
            int l_nbranches,
            int l_nGeometry,
            ref IntPtr nodeIds,
            ref IntPtr nodeLongNames,
            ref IntPtr branchIds,
            ref IntPtr branchLongNames,
            ref double[] l_nodesX,
            ref double[] l_nodesY,
            ref int[] l_sourcenodeid,
            ref int[] l_targetnodeid,
            ref double[] l_branchlengths,
            ref int[] l_nbranchgeometrypoints,
            ref double[] l_geopointsX,
            ref double[] l_geopointsY,
            ref int[] l_branch_order)
        {
            IntPtr c_nodesX = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nnodes);
            IntPtr c_nodesY = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nnodes);
            IntPtr c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nbranches);
            IntPtr c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nbranches);
            IntPtr c_branchlengths = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nbranches);
            IntPtr c_nbranchgeometrypoints = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nbranches);
            IntPtr c_geopointsX = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nGeometry);
            IntPtr c_geopointsY = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nGeometry);
            IntPtr c_branch_order = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nbranches);
            try
            {
                // Write 1d network network nodes
                Marshal.Copy(l_nodesX, 0, c_nodesX, l_nnodes);
                Marshal.Copy(l_nodesY, 0, c_nodesY, l_nnodes);

                int ierr = wrapper.ionc_write_1d_network_nodes(ref ioncid, ref networkid, ref c_nodesX, ref c_nodesY,
                    ref nodeIds, ref nodeLongNames, ref l_nnodes);
                Assert.That(ierr, Is.EqualTo(0));

                // Write 1d network branches
                Marshal.Copy(l_sourcenodeid, 0, c_sourcenodeid, l_nbranches);
                Marshal.Copy(l_targetnodeid, 0, c_targetnodeid, l_nbranches);
                Marshal.Copy(l_branchlengths, 0, c_branchlengths, l_nbranches);
                Marshal.Copy(l_nbranchgeometrypoints, 0, c_nbranchgeometrypoints, l_nbranches);
                ierr = wrapper.ionc_put_1d_network_branches(ref ioncid, ref networkid, ref c_sourcenodeid,
                    ref c_targetnodeid, ref branchIds, ref branchLongNames, ref c_branchlengths, ref c_nbranchgeometrypoints, ref l_nbranches, ref startIndex);
                Assert.That(ierr, Is.EqualTo(0));

                // Write 1d network geometry
                Marshal.Copy(l_geopointsX, 0, c_geopointsX, l_nGeometry);
                Marshal.Copy(l_geopointsY, 0, c_geopointsY, l_nGeometry);
                ierr = wrapper.ionc_write_1d_network_branches_geometry(ref ioncid, ref networkid, ref c_geopointsX,
                    ref c_geopointsY, ref l_nGeometry);
                Assert.That(ierr, Is.EqualTo(0));

                // Define the branch order 
                Marshal.Copy(l_branch_order, 0, c_branch_order, l_nbranches);
                ierr = wrapper.ionc_put_1d_network_branchorder(ref ioncid, ref networkid, ref c_branch_order, ref l_nbranches);
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
                Marshal.FreeCoTaskMem(c_branch_order);
            }
        }

        private void addglobalattributes(int ioncid, ref IoNetcdfLibWrapper wrapper)
        {
            string tmpstring;
            IoNetcdfLibWrapper.interop_metadata metadata;
            tmpstring = "Deltares";
            tmpstring = tmpstring.PadRight(IoNetcdfLibWrapper.metadatasize, ' ');
            metadata.institution = tmpstring.ToCharArray();
            tmpstring = "Unknown";
            tmpstring = tmpstring.PadRight(IoNetcdfLibWrapper.metadatasize, ' ');
            metadata.source = tmpstring.ToCharArray();
            tmpstring = "Unknown";
            tmpstring = tmpstring.PadRight(IoNetcdfLibWrapper.metadatasize, ' ');
            metadata.references = tmpstring.ToCharArray();
            tmpstring = "Unknown";
            tmpstring = tmpstring.PadRight(IoNetcdfLibWrapper.metadatasize, ' ');
            metadata.version = tmpstring.ToCharArray();
            tmpstring = "Unknown";
            tmpstring = tmpstring.PadRight(IoNetcdfLibWrapper.metadatasize, ' ');
            metadata.modelname = tmpstring.ToCharArray();
            int ierr = wrapper.ionc_add_global_attributes(ref ioncid, ref metadata);
            Assert.That(ierr, Is.EqualTo(0));
        }

        private void getNetworkid(int ioncid, ref int networkid, ref IoNetcdfLibWrapper wrapper)
        {
            //get the number of networks
            int nnumNetworks = -1;
            int ierr = wrapper.ionc_get_number_of_networks(ref ioncid, ref nnumNetworks);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(nnumNetworks, Is.EqualTo(1));
            // get the networks ids 
            IntPtr c_networksids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nnumNetworks);
            ierr = wrapper.ionc_get_network_ids(ref ioncid, ref c_networksids, ref nnumNetworks);
            Assert.That(ierr, Is.EqualTo(0));
            int[] rc_networksids = new int[nnumNetworks];
            Marshal.Copy(c_networksids, rc_networksids, 0, nnumNetworks);
            networkid = rc_networksids[0];
        }

        private void getMeshid(int ioncid, ref int meshid, int meshType, ref IoNetcdfLibWrapper wrapper)
        {
            // get the number meshes 
            int numMeshes = -1;
            int ierr = wrapper.ionc_get_number_of_meshes(ref ioncid, ref meshType, ref numMeshes);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(numMeshes, Is.EqualTo(1));
            // get the mesh id
            IntPtr c_meshids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * numMeshes);
            ierr = wrapper.ionc_ug_get_mesh_ids(ref ioncid, ref meshType, ref c_meshids, ref numMeshes);
            Assert.That(ierr, Is.EqualTo(0));
            int[] rc_meshids = new int[numMeshes];
            Marshal.Copy(c_meshids, rc_meshids, 0, numMeshes);
            meshid = rc_meshids[0];
        }

        // Create the netcdf files
        [Test]
        [NUnit.Framework.Category("UGRIDTests")]
        public void create1dUGridNetworkAndMeshNetcdf()
        {
            // Create a netcdf file 
            int ioncid = 0; //file variable 
            int mode = 1; //create in write mode
            var ierr = -1;
            string tmpstring; //temporary string for several operations
            string c_path = GridTestHelper.TestDirectoryPath() + @"\write1d.nc";
            GridTestHelper.DeleteIfExists(c_path);
            Assert.IsFalse(File.Exists(c_path));
            var wrapper = new IoNetcdfLibWrapper();

            //2. make a local copy of the variables 
            int l_nnodes = nNodes;
            int l_nbranches = nBranches;
            int l_nGeometry = nGeometry;
            double[] l_nodesX = nodesX;
            double[] l_nodesY = nodesY;
            int[] l_sourcenodeid = sourcenodeid;
            int[] l_targetnodeid = targetnodeid;
            double[] l_branchlengths = branchlengths;
            int[] l_nbranchgeometrypoints = nbranchgeometrypoints;
            double[] l_geopointsX = geopointsX;
            double[] l_geopointsY = geopointsY;
            int[] l_branch_order = branch_order;

            // Create the file, will not add any dataset 
            ierr = wrapper.ionc_create(c_path, ref mode, ref ioncid);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.IsTrue(File.Exists(c_path));

            // For reading the grid later on we need to add metadata to the netcdf file. 
            //   The function ionc_add_global_attributes adds to the netCDF file the UGRID convention
            addglobalattributes(ioncid, ref wrapper);

            // Create a 1d network
            int networkid = -1;
            StringBuilder l_networkName = new StringBuilder(networkName);
            ierr = wrapper.ionc_create_1d_network(ref ioncid, ref networkid, l_networkName.ToString(), ref nNodes, ref nBranches,
                ref nGeometry);
            Assert.That(ierr, Is.EqualTo(0));

            // Create the node branchidx, offsets, meshnodeidsinfo
            StringBuilder l_meshname = new StringBuilder(meshname);
            int l_nmeshpoints = nmeshpoints;
            int l_nedgenodes = nedgenodes;
            int l_nBranches = nBranches;
            int l_nlinks = nlinks;
            double[] l_branchoffset = offset;
            double[] l_branchlength = branchlengths;
            int[] l_branchidx = branchidx;
            int l_startIndex = startIndex;
            double[] l_mesh1dCoordX = mesh1dCoordX;
            double[] l_mesh1dCoordY = mesh1dCoordY;

            var register = new UnmanagedMemoryRegister();
            var networkNodeIdsBuffer = StringBufferHandling.MakeStringBuffer(ref nodesids, IoNetcdfLibWrapper.idssize);
            var networkNodeLongNamesBuffer = StringBufferHandling.MakeStringBuffer(ref nodeslongNames, IoNetcdfLibWrapper.longnamessize);
            IntPtr networkNodeIdsPtr = register.AddString(ref networkNodeIdsBuffer);
            IntPtr networkNodeLongNamesPtr = register.AddString(ref networkNodeLongNamesBuffer);

            var branchIdsStringBuffer = StringBufferHandling.MakeStringBuffer(ref branchids, IoNetcdfLibWrapper.idssize);
            var branchLongNamesStringBuffer = StringBufferHandling.MakeStringBuffer(ref branchlongNames, IoNetcdfLibWrapper.longnamessize);
            IntPtr branchIdsPtr = register.AddString(ref branchIdsStringBuffer);
            IntPtr branchLongNamesPtr = register.AddString(ref branchLongNamesStringBuffer);


            // Write 1d network and mesh
            write1dnetwork(ioncid,
                networkid,
                ref wrapper,
                l_nnodes,
                l_nbranches,
                l_nGeometry,
                ref networkNodeIdsPtr,
                ref networkNodeLongNamesPtr,
                ref branchIdsPtr,
                ref branchLongNamesPtr,
                ref l_nodesX,
                ref l_nodesY,
                ref l_sourcenodeid,
                ref l_targetnodeid,
                ref l_branchlengths,
                ref l_nbranchgeometrypoints,
                ref l_geopointsX,
                ref l_geopointsY,
                ref l_branch_order);


            var meshNodesIdsBuffer = StringBufferHandling.MakeStringBuffer(ref nodesids, IoNetcdfLibWrapper.idssize);
            var meshNodesLongNamesBuffer = StringBufferHandling.MakeStringBuffer(ref nodeslongNames, IoNetcdfLibWrapper.longnamessize);
            IntPtr meshNodesIdsPtr = register.AddString(ref meshNodesIdsBuffer);
            IntPtr meshNodeslongNamesPtr = register.AddString(ref meshNodesLongNamesBuffer);

            write1dmesh(
                ioncid,
                ref l_networkName,
                ref l_meshname,
                ref wrapper,
                ref meshNodesIdsPtr,
                ref meshNodeslongNamesPtr,
                l_nmeshpoints,
                l_nedgenodes,
                l_nBranches,
                l_nlinks,
                ref l_branchoffset,
                ref l_branchlength,
                ref l_branchidx,
                ref l_sourcenodeid,
                ref l_targetnodeid,
                l_startIndex,
                ref l_mesh1dCoordX,
                ref l_mesh1dCoordY);

            //8. Close the file
            ierr = wrapper.ionc_close(ref ioncid);
            Assert.That(ierr, Is.EqualTo(0));
        }

        ////// read the netcdf file created in the test above
        [Test]
        [NUnit.Framework.Category("UGRIDTests")]
        public void read1dUGRIDNetcdf()
        {
            IntPtr c_meshidsfromnetworkid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)));
            try
            {
                // Open a netcdf file 
                string c_path = GridTestHelper.TestFilesDirectoryPath() + @"\write1d.nc";
                Assert.IsTrue(File.Exists(c_path));
                int ioncid = 0; //file variable 
                int mode = 0; //create in read mode
                var wrapper = new IoNetcdfLibWrapper();
                var ierr = wrapper.ionc_open(c_path, ref mode, ref ioncid, ref iconvtype, ref convversion);
                Assert.That(ierr, Is.EqualTo(0));

                // Get the 1D network and mesh ids
                // network
                int networkid = -1;
                getNetworkid(ioncid, ref networkid, ref wrapper);
                Assert.That(networkid, Is.EqualTo(1));

                // 1d mesh mesh
                int meshType = 1;
                int meshid = -1;
                getMeshid(ioncid, ref meshid, meshType, ref wrapper);
                Assert.That(meshid, Is.EqualTo(1));

                ierr = wrapper.ionc_get_1d_mesh_id(ref ioncid, ref meshid);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(meshid, Is.EqualTo(1));

                // Create local variables
                int l_nnodes = -1;
                int l_nbranches = -1;
                int l_nGeometry = -1;
                int l_startIndex = startIndex;

                StringBuilder l_networkName = new StringBuilder(IoNetcdfLibWrapper.LibDetails.MAXSTRLEN);

                double[] l_nodesX = new double[nNodes];
                double[] l_nodesY = new double[nNodes];
                int[] l_sourcenodeid = new int[nBranches];
                int[] l_targetnodeid = new int[nBranches];
                double[] l_branchlengths = new double[nBranches];
                int[] l_nbranchgeometrypoints = new int[nBranches];
                double[] l_geopointsX = new double[nGeometry];
                double[] l_geopointsY = new double[nGeometry];
                int[] l_branch_order = new int[nBranches];

                var register = new UnmanagedMemoryRegister();
                var networkNodeIdsBuffer = StringBufferHandling.MakeStringBuffer(nNodes, IoNetcdfLibWrapper.idssize);
                var networkNodeLongNamesBuffer = StringBufferHandling.MakeStringBuffer(nNodes, IoNetcdfLibWrapper.longnamessize);
                IntPtr networkNodeIdsPtr = register.AddString(ref networkNodeIdsBuffer);
                IntPtr networkNodeLongNamesPtr = register.AddString(ref networkNodeLongNamesBuffer);

                var branchIdsStringBuffer = StringBufferHandling.MakeStringBuffer(nBranches, IoNetcdfLibWrapper.idssize);
                var branchLongNamesStringBuffer = StringBufferHandling.MakeStringBuffer(nBranches, IoNetcdfLibWrapper.longnamessize);
                IntPtr branchIdsPtr = register.AddString(ref branchIdsStringBuffer);
                IntPtr branchLongNamesPtr = register.AddString(ref branchLongNamesStringBuffer);

                read1dnetwork(
                    ioncid,
                    networkid,
                    ref wrapper,
                    ref l_nnodes,
                    ref l_nbranches,
                    ref l_nGeometry,
                    l_startIndex,
                    ref l_networkName,
                    ref networkNodeIdsPtr,
                    ref networkNodeLongNamesPtr,
                    ref branchIdsPtr,
                    ref branchLongNamesPtr,
                    ref l_nodesX,
                    ref l_nodesY,
                    ref l_sourcenodeid,
                    ref l_targetnodeid,
                    ref l_branchlengths,
                    ref l_nbranchgeometrypoints,
                    ref l_geopointsX,
                    ref l_geopointsY,
                    ref l_branch_order);

                // Check the values
                Assert.That(l_networkName.ToString().Trim(), Is.EqualTo(networkName));
                Assert.That(l_nnodes, Is.EqualTo(nNodes));
                Assert.That(l_nbranches, Is.EqualTo(nBranches));
                Assert.That(l_nGeometry, Is.EqualTo(nGeometry));

                var networkNodeIds = StringBufferHandling.ParseString(networkNodeIdsPtr, nNodes, IoNetcdfLibWrapper.idssize);
                var networkNodeLongNames = StringBufferHandling.ParseString(networkNodeLongNamesPtr, nNodes, IoNetcdfLibWrapper.longnamessize);
                var branchIds = StringBufferHandling.ParseString(branchIdsPtr, nBranches, IoNetcdfLibWrapper.idssize);
                var branchLongNames = StringBufferHandling.ParseString(branchLongNamesPtr, nBranches, IoNetcdfLibWrapper.longnamessize);

                for (int i = 0; i < nNodes; i++)
                {
                    Assert.That(networkNodeIds[i].Trim(), Is.EqualTo(nodesids[i]));
                    Assert.That(networkNodeLongNames[i].Trim(), Is.EqualTo(nodeslongNames[i]));
                    Assert.That(l_nodesX[i], Is.EqualTo(nodesX[i]));
                    Assert.That(l_nodesY[i], Is.EqualTo(nodesY[i]));
                }

                for (int i = 0; i < l_nbranches; i++)
                {
                    Assert.That(branchIds[i].Trim(), Is.EqualTo(branchids[i]));
                    Assert.That(branchLongNames[i].Trim(), Is.EqualTo(branchlongNames[i]));
                    Assert.That(l_targetnodeid[i], Is.EqualTo(targetnodeid[i]));
                    Assert.That(l_sourcenodeid[i], Is.EqualTo(sourcenodeid[i]));
                    Assert.That(l_branchlengths[i], Is.EqualTo(branchlengths[i]));
                    Assert.That(l_nbranchgeometrypoints[i], Is.EqualTo(nbranchgeometrypoints[i]));
                    Assert.That(l_branch_order[i], Is.EqualTo(branch_order[i]));
                }

                for (int i = 0; i < l_nGeometry; i++)
                {
                    Assert.That(l_geopointsX[i], Is.EqualTo(geopointsX[i]));
                    Assert.That(l_geopointsY[i], Is.EqualTo(geopointsY[i]));
                }

                // LC: REFACTOR NEEDED!
                check1dmesh(ioncid, meshid, ref wrapper);

                // Count the meshes associated with this network
                int nmeshids = -1;
                ierr = wrapper.ionc_count_mesh_ids_from_network_id(ref ioncid, ref networkid, ref nmeshids);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(nmeshids, Is.EqualTo(1));

                int[] meshidsfromnetworkid = new int[nmeshids];
                ierr = wrapper.ionc_get_mesh_ids_from_network_id(ref ioncid, ref networkid, ref nmeshids, ref c_meshidsfromnetworkid);
                Assert.That(ierr, Is.EqualTo(0));
                Marshal.Copy(c_meshidsfromnetworkid, meshidsfromnetworkid, 0, nmeshids);
                Assert.That(meshidsfromnetworkid[0], Is.EqualTo(1));

                // Get the network id from the mesh id
                networkid = -1;
                ierr = wrapper.ionc_get_network_id_from_mesh_id(ref ioncid, ref meshid, ref networkid);
                Assert.That(ierr, Is.EqualTo(0));
                Assert.That(networkid, Is.EqualTo(1));

                // Close the file
                ierr = wrapper.ionc_close(ref ioncid);
                Assert.That(ierr, Is.EqualTo(0));
            }
            finally
            {
                Marshal.FreeCoTaskMem(c_meshidsfromnetworkid);
            }
        }

        //// Deltashell creates a new file to write the 1d geometry and mesh as in the first test create1dUGRIDNetcdf
        //// and clones the 2d mesh data read from a file produced by RGFgrid. 
        [Test]
        [NUnit.Framework.Category("UGRIDTests")]
        public void Clones2dMesh()
        {
            var wrapper = new IoNetcdfLibWrapper();

            // RGF grid creates a 2d mesh. The info is in memory, here simulated by opening a file containing a mesh2d and by reading all data in
            string sourcetwod_path = TestHelper.CreateLocalCopy("Custom_Ugrid.nc");
            Assert.IsTrue(File.Exists(sourcetwod_path));
            int sourcetwodioncid = -1; //file id 
            int sourcetwomode = 0; //read mode
            int ierr = wrapper.ionc_open(sourcetwod_path, ref sourcetwomode, ref sourcetwodioncid, ref iconvtype,
                ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            // Now we create a new empty file where to save 1d and 2d meshes
            int targetioncid = -1; //file id  
            int targetmode = 1; //create in write mode
            string target_path = GridTestHelper.TestDirectoryPath() + "/target.nc";
            GridTestHelper.DeleteIfExists(target_path);
            Assert.IsFalse(File.Exists(target_path));

            // Create the file
            ierr = wrapper.ionc_create(target_path, ref targetmode, ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.IsTrue(File.Exists(target_path));

            // Add global attributes in the file
            addglobalattributes(targetioncid, ref wrapper);

            // Get the id of the 2d mesh in the RGF grid file (Custom_Ugrid.nc)
            int meshType = 2;
            int sourcemesh2d = -1;
            getMeshid(sourcetwodioncid, ref sourcemesh2d, meshType, ref wrapper);
            Assert.That(sourcemesh2d, Is.EqualTo(1));

            // Create 1d geometry and mesh in the new file (target.nc)
            int networkid = -1;
            ierr = wrapper.ionc_create_1d_network(ref targetioncid, ref networkid, networkName, ref nNodes,
                ref nBranches, ref nGeometry);
            Assert.That(ierr, Is.EqualTo(0));

            StringBuilder l_networkName = new StringBuilder(networkName);
            StringBuilder l_meshname = new StringBuilder(meshname);
            StringBuilder l_linkmeshname = new StringBuilder(linkmeshname);
            int l_nmeshpoints = nmeshpoints;
            int l_nedgenodes = nedgenodes;
            int l_nBranches = nBranches;
            int l_nlinks = nlinks;
            double[] l_branchoffset = offset;
            double[] l_branchlength = branchlengths;
            int[] l_branchidx = branchidx;
            int l_startIndex = startIndex;
            double[] l_mesh1dCoordX = mesh1dCoordX;
            double[] l_mesh1dCoordY = mesh1dCoordY;

            // Create the node branchidx, offsets, meshnodeidsinfo
            int l_nnodes = nNodes;
            int l_nbranches = nBranches;
            int l_nGeometry = nGeometry;
            double[] l_nodesX = nodesX;
            double[] l_nodesY = nodesY;
            int[] l_sourcenodeid = sourcenodeid;
            int[] l_targetnodeid = targetnodeid;
            double[] l_branchlengths = branchlengths;
            int[] l_nbranchgeometrypoints = nbranchgeometrypoints;
            double[] l_geopointsX = geopointsX;
            double[] l_geopointsY = geopointsY;
            int[] l_branch_order = branch_order;

            var register = new UnmanagedMemoryRegister();
            var networkNodeIdsBuffer = StringBufferHandling.MakeStringBuffer(ref nodesids, IoNetcdfLibWrapper.idssize);
            var networkNodeLongNamesBuffer = StringBufferHandling.MakeStringBuffer(ref nodesids, IoNetcdfLibWrapper.longnamessize);
            IntPtr networkNodeIdsPtr = register.AddString(ref networkNodeIdsBuffer);
            IntPtr networkNodeLongNamesPtr = register.AddString(ref networkNodeLongNamesBuffer);

            var branchIdsStringBuffer = StringBufferHandling.MakeStringBuffer(ref branchids, IoNetcdfLibWrapper.idssize);
            var branchLongNamesStringBuffer = StringBufferHandling.MakeStringBuffer(ref branchids, IoNetcdfLibWrapper.longnamessize);
            IntPtr branchIdsPtr = register.AddString(ref branchIdsStringBuffer);
            IntPtr branchLongNamesPtr = register.AddString(ref branchLongNamesStringBuffer);

            // Write the 1d data in the new file (1d geometry, mesh and links)
            write1dnetwork(targetioncid,
                networkid,
                ref wrapper,
                l_nnodes,
                l_nbranches,
                l_nGeometry,
                ref networkNodeIdsPtr,
                ref networkNodeLongNamesPtr,
                ref branchIdsPtr,
                ref branchLongNamesPtr,
                ref l_nodesX,
                ref l_nodesY,
                ref l_sourcenodeid,
                ref l_targetnodeid,
                ref l_branchlengths,
                ref l_nbranchgeometrypoints,
                ref l_geopointsX,
                ref l_geopointsY,
                ref l_branch_order);


            var meshNodeIdsWriteBuffer = StringBufferHandling.MakeStringBuffer(ref meshnodeids, IoNetcdfLibWrapper.idssize);
            var meshNodeLongNamesWriteBuffer = StringBufferHandling.MakeStringBuffer(ref meshnodeids, IoNetcdfLibWrapper.longnamessize);
            IntPtr meshNodeIdsWritePtr = register.AddString(ref meshNodeIdsWriteBuffer);
            IntPtr meshNodeLongNamesWritePtr = register.AddString(ref meshNodeLongNamesWriteBuffer);

            write1dmesh(
                targetioncid,
                ref l_networkName,
                ref l_meshname,
                ref wrapper,
                ref meshNodeIdsWritePtr,
                ref meshNodeLongNamesWritePtr,
                l_nmeshpoints,
                l_nedgenodes,
                l_nBranches,
                l_nlinks,
                ref l_branchoffset,
                ref l_branchlength,
                ref l_branchidx,
                ref l_sourcenodeid,
                ref l_targetnodeid,
                l_startIndex,
                ref l_mesh1dCoordX,
                ref l_mesh1dCoordY);

            // Clone the 2d mesh definitions in the new file
            int target2dmesh = -1;
            ierr = wrapper.ionc_clone_mesh_definition(ref sourcetwodioncid, ref targetioncid, ref sourcemesh2d,
                ref target2dmesh);
            Assert.That(ierr, Is.EqualTo(0));

            // Clone the 2d mesh data
            ierr = wrapper.ionc_clone_mesh_data(ref sourcetwodioncid, ref targetioncid, ref sourcemesh2d,
                ref target2dmesh);
            Assert.That(ierr, Is.EqualTo(0));

            // Close all files 
            ierr = wrapper.ionc_close(ref sourcetwodioncid);
            Assert.That(ierr, Is.EqualTo(0));
            ierr = wrapper.ionc_close(ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));

            // Now open the file with cloned meshes and check if the data written there are correct
            targetioncid = -1; //file id  
            targetmode = 0; //open in write mode
            ierr = wrapper.ionc_open(target_path, ref targetmode, ref targetioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            // Check 2 meshes are present
            int nmesh = -1;
            ierr = wrapper.ionc_get_mesh_count(ref targetioncid, ref nmesh);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(nmesh, Is.EqualTo(2));

            // Get the mesh  and network ids network
            int source1dnetwork = -1;
            getNetworkid(targetioncid, ref source1dnetwork, ref wrapper);
            Assert.That(networkid, Is.EqualTo(1));

            // 1d mesh mesh
            meshType = 1;
            int source1dmesh = -1;
            getMeshid(targetioncid, ref source1dmesh, meshType, ref wrapper);
            Assert.That(source1dmesh, Is.EqualTo(1));

            // 2d mesh mesh
            meshType = 2;
            int source2dmesh = -1;
            getMeshid(targetioncid, ref source2dmesh, meshType, ref wrapper);
            Assert.That(source2dmesh, Is.EqualTo(2));

            StringBuilder ll_networkName = new StringBuilder(IoNetcdfLibWrapper.LibDetails.MAXSTRLEN);

            var networkNodeIdsReadBuffer = StringBufferHandling.MakeStringBuffer(nNodes, IoNetcdfLibWrapper.idssize);
            var networkNodeLongNamesReadBuffer = StringBufferHandling.MakeStringBuffer(nNodes, IoNetcdfLibWrapper.longnamessize);
            IntPtr networkNodeIdsReadBufferPtr = register.AddString(ref networkNodeIdsReadBuffer);
            IntPtr networkNodeLongNamesReadBufferPtr = register.AddString(ref networkNodeLongNamesReadBuffer);

            var branchIdsReadBuffer = StringBufferHandling.MakeStringBuffer(nBranches, IoNetcdfLibWrapper.idssize);
            var branchLongNamesReadBuffer = StringBufferHandling.MakeStringBuffer(nBranches, IoNetcdfLibWrapper.longnamessize);
            IntPtr branchIdsReadBufferPtr = register.AddString(ref branchIdsReadBuffer);
            IntPtr branchLongNamesReadBufferPtr = register.AddString(ref branchLongNamesReadBuffer);

            // Check all 1d and 2d data
            read1dnetwork(
                targetioncid,
                networkid,
                ref wrapper,
                ref l_nnodes,
                ref l_nbranches,
                ref l_nGeometry,
                l_startIndex,
                ref ll_networkName,
                ref networkNodeIdsReadBufferPtr,
                ref networkNodeLongNamesReadBufferPtr,
                ref branchIdsReadBufferPtr,
                ref branchLongNamesReadBufferPtr,
                ref l_nodesX,
                ref l_nodesY,
                ref l_sourcenodeid,
                ref l_targetnodeid,
                ref l_branchlengths,
                ref l_nbranchgeometrypoints,
                ref l_geopointsX,
                ref l_geopointsY,
                ref l_branch_order);

            // Check the read values
            Assert.That(l_networkName.ToString().Trim(), Is.EqualTo(networkName));
            Assert.That(l_nnodes, Is.EqualTo(l_nnodes));
            Assert.That(l_nbranches, Is.EqualTo(nBranches));
            Assert.That(l_nGeometry, Is.EqualTo(nGeometry));

            var readNodeIds = StringBufferHandling.ParseString(networkNodeIdsReadBufferPtr, nNodes, IoNetcdfLibWrapper.idssize);
            var readNetworkNodeLongNames = StringBufferHandling.ParseString(networkNodeLongNamesReadBufferPtr, nNodes, IoNetcdfLibWrapper.longnamessize);
            var readBranchIds = StringBufferHandling.ParseString(branchIdsReadBufferPtr, nBranches, IoNetcdfLibWrapper.idssize);
            var readBranchLongNames = StringBufferHandling.ParseString(branchLongNamesReadBufferPtr, nBranches, IoNetcdfLibWrapper.longnamessize);

            for (int i = 0; i < nNodes; i++)
            {
                Assert.That(readNodeIds[i].Trim(), Is.EqualTo(nodesids[i]));
                Assert.That(readNetworkNodeLongNames[i].Trim(), Is.EqualTo(nodeslongNames[i]));
                Assert.That(l_nodesX[i], Is.EqualTo(nodesX[i]));
                Assert.That(l_nodesY[i], Is.EqualTo(nodesY[i]));
            }

            for (int i = 0; i < l_nbranches; i++)
            {
                Assert.That(readBranchIds[i].Trim(), Is.EqualTo(branchids[i]));
                Assert.That(readBranchLongNames[i].Trim(), Is.EqualTo(branchlongNames[i]));
                Assert.That(l_targetnodeid[i], Is.EqualTo(targetnodeid[i]));
                Assert.That(l_sourcenodeid[i], Is.EqualTo(sourcenodeid[i]));
                Assert.That(l_branchlengths[i], Is.EqualTo(branchlengths[i]));
                Assert.That(l_nbranchgeometrypoints[i], Is.EqualTo(nbranchgeometrypoints[i]));
                Assert.That(l_branch_order[i], Is.EqualTo(branch_order[i]));
            }

            for (int i = 0; i < l_nGeometry; i++)
            {
                Assert.That(l_geopointsX[i], Is.EqualTo(geopointsX[i]));
                Assert.That(l_geopointsY[i], Is.EqualTo(geopointsY[i]));
            }

            //LC refactor needed also for this part!
            check1dmesh(targetioncid, source1dmesh, ref wrapper);
            check2dmesh(targetioncid, source2dmesh, ref wrapper);

            // Close the file
            ierr = wrapper.ionc_close(ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));
        }

        // Create a standalone network
        [Test]
        [NUnit.Framework.Category("UGRIDTests")]
        public void create1dNetwork()
        {
            // Create a netcdf file 
            int ioncid = 0; //file variable 
            int mode = 1; //create in write mode
            var ierr = -1;
            string c_path = GridTestHelper.TestDirectoryPath() + @"\write1dNetwork.nc";
            GridTestHelper.DeleteIfExists(c_path);
            Assert.IsFalse(File.Exists(c_path));
            var wrapper = new IoNetcdfLibWrapper();

            // Make a local copy of the variables 
            int l_nnodes = nNodes;
            int l_nbranches = nBranches;
            int l_nGeometry = nGeometry;
            double[] l_nodesX = nodesX;
            double[] l_nodesY = nodesY;
            int[] l_sourcenodeid = sourcenodeid;
            int[] l_targetnodeid = targetnodeid;
            double[] l_branchlengths = branchlengths;
            int[] l_nbranchgeometrypoints = nbranchgeometrypoints;
            double[] l_geopointsX = geopointsX;
            double[] l_geopointsY = geopointsY;
            int[] l_branch_order = branch_order;

            var register = new UnmanagedMemoryRegister();
            var nodeIdsBuffer = StringBufferHandling.MakeStringBuffer(ref nodesids, IoNetcdfLibWrapper.idssize);
            var networkNodeLongNamesBuffer = StringBufferHandling.MakeStringBuffer(ref nodesids, IoNetcdfLibWrapper.longnamessize);
            IntPtr networkNodeIdsPtr = register.AddString(ref nodeIdsBuffer);
            IntPtr networkNodeLongNamesPtr = register.AddString(ref networkNodeLongNamesBuffer);

            var branchIdsStringWriteBuffer = StringBufferHandling.MakeStringBuffer(ref branchids, IoNetcdfLibWrapper.idssize);
            var branchLongNamesStringWriteBuffer = StringBufferHandling.MakeStringBuffer(ref branchids, IoNetcdfLibWrapper.longnamessize);
            IntPtr branchIdsPtr = register.AddString(ref branchIdsStringWriteBuffer);
            IntPtr branchLongNamesPtr = register.AddString(ref branchLongNamesStringWriteBuffer);

            // Create the file, will not add any dataset 
            ierr = wrapper.ionc_create(c_path, ref mode, ref ioncid);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.IsTrue(File.Exists(c_path));

            // For reading the grid later on we need to add metadata to the netcdf file. The function ionc_add_global_attributes adds to the netCDF file the UGRID convention
            addglobalattributes(ioncid, ref wrapper);

            // Create a 1d network
            int networkid = -1;
            ierr = wrapper.ionc_create_1d_network(ref ioncid, ref networkid, networkName, ref nNodes,
                ref nBranches, ref nGeometry);
            Assert.That(ierr, Is.EqualTo(0));

            // Write 1d network and mesh
            write1dnetwork(ioncid,
                networkid,
                ref wrapper,
                l_nnodes,
                l_nbranches,
                l_nGeometry,
                ref networkNodeIdsPtr,
                ref networkNodeLongNamesPtr,
                ref branchIdsPtr,
                ref branchLongNamesPtr,
                ref l_nodesX,
                ref l_nodesY,
                ref l_sourcenodeid,
                ref l_targetnodeid,
                ref l_branchlengths,
                ref l_nbranchgeometrypoints,
                ref l_geopointsX,
                ref l_geopointsY,
                ref l_branch_order);
            //6. Close the file
            ierr = wrapper.ionc_close(ref ioncid);
            Assert.That(ierr, Is.EqualTo(0));
        }

        // read the standalone network
        [Test]
        [NUnit.Framework.Category("UGRIDTests")]
        public void read1dNetwork()
        {
            // Open a netcdf file (file from test data, but actually created previously)
            string c_path = GridTestHelper.TestFilesDirectoryPath() + @"\write1dNetwork.nc";
            Assert.IsTrue(File.Exists(c_path));
            int ioncid = 0; //file variable 
            int mode = 0; //create in read mode
            var wrapper = new IoNetcdfLibWrapper();
            var ierr = wrapper.ionc_open(c_path, ref mode, ref ioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            // Get the 1D network and mesh ids
            int networkid = -1;
            ierr = wrapper.ionc_get_1d_network_id(ref ioncid, ref networkid);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(networkid, Is.EqualTo(1));
            int meshid = -1;
            ierr = wrapper.ionc_get_1d_mesh_id(ref ioncid, ref meshid);

            // Mesh should not be found!
            Assert.That(ierr, Is.EqualTo(-1));
            Assert.That(meshid, Is.EqualTo(-1));

            // Check if all 1d data written in the file are correct
            int l_nnodes = -1;
            int l_nbranches = -1;
            int l_nGeometry = -1;
            int l_startIndex = startIndex;
            StringBuilder l_networkName = new StringBuilder(IoNetcdfLibWrapper.LibDetails.MAXSTRLEN);

            double[] l_nodesX = new double[nNodes];
            double[] l_nodesY = new double[nNodes];
            int[] l_sourcenodeid = new int[nBranches];
            int[] l_targetnodeid = new int[nBranches];
            double[] l_branchlengths = new double[nBranches];
            int[] l_nbranchgeometrypoints = new int[nBranches];
            double[] l_geopointsX = new double[nGeometry];
            double[] l_geopointsY = new double[nGeometry];
            int[] l_branch_order = new int[nBranches];

            var register = new UnmanagedMemoryRegister();
            var networkNodeIdsBuffer = StringBufferHandling.MakeStringBuffer(nNodes, IoNetcdfLibWrapper.idssize);
            var networkNodeLongNamesBuffer = StringBufferHandling.MakeStringBuffer(nNodes, IoNetcdfLibWrapper.longnamessize);
            IntPtr networkNodeIdsPtr = register.AddString(ref networkNodeIdsBuffer);
            IntPtr networkNodeLongNamesPtr = register.AddString(ref networkNodeLongNamesBuffer);

            var branchIdsStringBuffer = StringBufferHandling.MakeStringBuffer(nBranches, IoNetcdfLibWrapper.idssize);
            var branchLongNamesStringBuffer = StringBufferHandling.MakeStringBuffer(nBranches, IoNetcdfLibWrapper.longnamessize);
            IntPtr branchIdsPtr = register.AddString(ref branchIdsStringBuffer);
            IntPtr branchLongNamesPtr = register.AddString(ref branchLongNamesStringBuffer);

            read1dnetwork(
                ioncid,
                networkid,
                ref wrapper,
                ref l_nnodes,
                ref l_nbranches,
                ref l_nGeometry,
                l_startIndex,
                ref l_networkName,
                ref networkNodeIdsPtr,
                ref networkNodeLongNamesPtr,
                ref branchIdsPtr,
                ref branchLongNamesPtr,
                ref l_nodesX,
                ref l_nodesY,
                ref l_sourcenodeid,
                ref l_targetnodeid,
                ref l_branchlengths,
                ref l_nbranchgeometrypoints,
                ref l_geopointsX,
                ref l_geopointsY,
                ref l_branch_order);

            // check the values
            Assert.That(l_networkName.ToString().Trim(), Is.EqualTo(networkName));
            Assert.That(l_nnodes, Is.EqualTo(l_nnodes));
            Assert.That(l_nbranches, Is.EqualTo(nBranches));
            Assert.That(l_nGeometry, Is.EqualTo(nGeometry));

            var readNodeIds = StringBufferHandling.ParseString(networkNodeIdsPtr, nNodes, IoNetcdfLibWrapper.idssize);
            var readNetworkNodeLongNames = StringBufferHandling.ParseString(networkNodeLongNamesPtr, nNodes, IoNetcdfLibWrapper.longnamessize);
            var readBranchIds = StringBufferHandling.ParseString(branchIdsPtr, nBranches, IoNetcdfLibWrapper.idssize);
            var readBranchLongNames = StringBufferHandling.ParseString(branchLongNamesPtr, nBranches, IoNetcdfLibWrapper.longnamessize);

            for (int i = 0; i < nNodes; i++)
            {
                Assert.That(readNodeIds[i].Trim(), Is.EqualTo(nodesids[i]));
                Assert.That(readNetworkNodeLongNames[i].Trim(), Is.EqualTo(nodeslongNames[i]));
                Assert.That(l_nodesX[i], Is.EqualTo(nodesX[i]));
                Assert.That(l_nodesY[i], Is.EqualTo(nodesY[i]));
            }

            for (int i = 0; i < l_nbranches; i++)
            {
                Assert.That(readBranchIds[i].Trim(), Is.EqualTo(branchids[i]));
                Assert.That(readBranchLongNames[i].Trim(), Is.EqualTo(branchlongNames[i]));
                Assert.That(l_targetnodeid[i], Is.EqualTo(targetnodeid[i]));
                Assert.That(l_sourcenodeid[i], Is.EqualTo(sourcenodeid[i]));
                Assert.That(l_branchlengths[i], Is.EqualTo(branchlengths[i]));
                Assert.That(l_nbranchgeometrypoints[i], Is.EqualTo(nbranchgeometrypoints[i]));
                Assert.That(l_branch_order[i], Is.EqualTo(branch_order[i]));
            }

            for (int i = 0; i < l_nGeometry; i++)
            {
                Assert.That(l_geopointsX[i], Is.EqualTo(geopointsX[i]));
                Assert.That(l_geopointsY[i], Is.EqualTo(geopointsY[i]));
            }

            // Close the file
            ierr = wrapper.ionc_close(ref ioncid);
        }

        /* 
        1)	Allocates the arrays defining the network (nodes, branches, geometry points, and all ids/longnames). Here I used 1000001 nodes, 1000000 branches, 3000000 geometry points 
        2)	Opens the first netcdf file “sewer_system.nc”
        3)	Creates and writes the network
        4)	Closes the file
        5)	Opens the file
        6)	Reads the arrays back in
        7)	Closes the file
        8)	Allocates the arrays for the augmented network(1000001 nodes, 1000002 branches, 3000003 geometry points)
        9)	Copies the values read from the “sewer_system.nc” in the new arrays
        10)	Adds the “stranger” node, branch and geometry points
        11)	Opens a second netcdf file “LargeSewerSystemSecondTest.nc”
        12)	Creates and writes the network “sewer_system_with_the_stranger”
        13)	Closes the file
        */
        [Test]
        [NUnit.Framework.Category("UGRIDTests")]
        public void LargeSewerSystem()
        {
            int stackSize = 1024 * 1024 * 16; //LC: in C# the default stack size is 1 MB, increase it to something larger for this test!
            Thread th = new Thread(() =>
                {
                    // Allocates the arrays defining the network 
                    int firstCaseNumberOfNodes = 100000; //5000 limit win64, without stack increase

                    int l_nnodes = firstCaseNumberOfNodes + 1;
                    int l_nbranches = firstCaseNumberOfNodes;
                    int l_nGeometry = firstCaseNumberOfNodes * 3;
                    double[] l_nodesX = new double[l_nnodes];
                    double[] l_nodesY = new double[l_nnodes];
                    double[] l_branchlengths = new double[l_nbranches];
                    int[] l_nbranchgeometrypoints = new int[l_nbranches];

                    int[] l_sourcenodeid = new int[l_nbranches];
                    int[] l_targetnodeid = new int[l_nbranches];
                    int[] l_branch_order = new int[l_nbranches];

                    double[] l_geopointsX = new double[l_nGeometry];
                    double[] l_geopointsY = new double[l_nGeometry];

                    var l_nodesIds = new string[l_nnodes];
                    var l_nodesLongNames = new string[l_nnodes];
                    var l_branchIds = new string[l_nbranches];
                    var l_branchLongNames = new string[l_nbranches];

                    string str;
                    for (int i = 0; i < l_nnodes; i++)
                    {
                        str = "node_id_" + i.ToString();
                        l_nodesIds[i] = str.PadRight(IoNetcdfLibWrapper.idssize, ' ');
                        str = "node_longname_" + i.ToString();
                        l_nodesLongNames[i] = str.PadRight(IoNetcdfLibWrapper.longnamessize, ' ');
                    }

                    for (int i = 0; i < l_nbranches; i++)
                    {
                        str = "branch_id_" + i.ToString();
                        l_branchIds[i] = str.PadRight(IoNetcdfLibWrapper.idssize, ' ');
                        str = "branch_longname_" + i.ToString();
                        l_branchLongNames[i] = str.PadRight(IoNetcdfLibWrapper.longnamessize, ' ');
                        l_branchlengths[i] = 1.414;
                        l_nbranchgeometrypoints[i] = 1;
                        l_sourcenodeid[i] = i;
                        l_targetnodeid[i] = i + 1;
                        l_branch_order[i] = i;
                    }

                    for (int i = 0; i < l_nGeometry; i++)
                    {
                        l_geopointsX[i] = i + 1.0 / 2.0;
                        l_geopointsX[i] = i + 1.0 / 2.0;
                    }

                    // Open file
                    int ioncid = 0;     // file variable 
                    int mode = 1;       // create in write mode
                    var ierr = -1;
                    string c_path = GridTestHelper.TestDirectoryPath() + @"\LargeSewerSystem.nc";
                    GridTestHelper.DeleteIfExists(c_path);
                    Assert.IsFalse(File.Exists(c_path));
                    var wrapper = new IoNetcdfLibWrapper();

                    ierr = wrapper.ionc_create(c_path, ref mode, ref ioncid);
                    Assert.That(ierr, Is.EqualTo(0));
                    Assert.IsTrue(File.Exists(c_path));

                    // The function ionc_add_global_attributes adds to the netCDF file the UGRID convention
                    addglobalattributes(ioncid, ref wrapper);

                    string l_network_name = "sewer_system";
                    int networkid = -1;
                    ierr = wrapper.ionc_create_1d_network(ref ioncid, ref networkid, l_network_name, ref l_nnodes,
                        ref l_nbranches, ref l_nGeometry);
                    Assert.That(ierr, Is.EqualTo(0));


                    var register = new UnmanagedMemoryRegister();
                    var networkNodeIdsBuffer = StringBufferHandling.MakeStringBuffer(l_nnodes, IoNetcdfLibWrapper.idssize);
                    var networkNodeLongNamesBuffer = StringBufferHandling.MakeStringBuffer(l_nnodes, IoNetcdfLibWrapper.longnamessize);
                    IntPtr networkNodeIdsPtr = register.AddString(ref networkNodeIdsBuffer);
                    IntPtr networkNodeLongNamesPtr = register.AddString(ref networkNodeLongNamesBuffer);

                    var branchIdsStringBuffer = StringBufferHandling.MakeStringBuffer(l_nbranches, IoNetcdfLibWrapper.idssize);
                    var branchLongNamesStringBuffer = StringBufferHandling.MakeStringBuffer(l_nbranches, IoNetcdfLibWrapper.longnamessize);
                    IntPtr branchIdsPtr = register.AddString(ref branchIdsStringBuffer);
                    IntPtr branchLongNamesPtr = register.AddString(ref branchLongNamesStringBuffer);


                    // Write 1d network and mesh
                    write1dnetwork(ioncid,
                        networkid,
                        ref wrapper,
                        l_nnodes,
                        l_nbranches,
                        l_nGeometry,
                        ref networkNodeIdsPtr,
                        ref networkNodeLongNamesPtr,
                        ref branchIdsPtr,
                        ref branchLongNamesPtr,
                        ref l_nodesX,
                        ref l_nodesY,
                        ref l_sourcenodeid,
                        ref l_targetnodeid,
                        ref l_branchlengths,
                        ref l_nbranchgeometrypoints,
                        ref l_geopointsX,
                        ref l_geopointsY,
                        ref l_branch_order);

                    // Close the file
                    ierr = wrapper.ionc_close(ref ioncid);
                    Assert.That(ierr, Is.EqualTo(0));

                    // Open the file in readmode
                    mode = 0; //read
                    ierr = wrapper.ionc_open(c_path, ref mode, ref ioncid, ref iconvtype, ref convversion);
                    Assert.That(ierr, Is.EqualTo(0));

                    // Get the 1D network id
                    getNetworkid(ioncid, ref networkid, ref wrapper);
                    Assert.That(networkid, Is.EqualTo(1));

                    // Create local variables
                    int l_startIndex = startIndex;
                    StringBuilder l_networkName = new StringBuilder(IoNetcdfLibWrapper.LibDetails.MAXSTRLEN);

                    var networkNodeIdsReadBuffer = StringBufferHandling.MakeStringBuffer(nNodes, IoNetcdfLibWrapper.idssize);
                    var networkNodeLongNamesReadBuffer = StringBufferHandling.MakeStringBuffer(nNodes, IoNetcdfLibWrapper.longnamessize);
                    IntPtr networkNodeIdsReadPtr = register.AddString(ref networkNodeIdsReadBuffer);
                    IntPtr networkNodeLongNamesReadPtr = register.AddString(ref networkNodeLongNamesReadBuffer);

                    var branchIdsStringReadBuffer = StringBufferHandling.MakeStringBuffer(nBranches, IoNetcdfLibWrapper.idssize);
                    var branchLongNamesReadBuffer = StringBufferHandling.MakeStringBuffer(nBranches, IoNetcdfLibWrapper.longnamessize);
                    IntPtr branchIdsReadPtr = register.AddString(ref branchIdsStringReadBuffer);
                    IntPtr branchLongNamesReadPtr = register.AddString(ref branchLongNamesReadBuffer);

                    // Read the arrays back in
                    read1dnetwork(
                        ioncid,
                        networkid,
                        ref wrapper,
                        ref l_nnodes,
                        ref l_nbranches,
                        ref l_nGeometry,
                        l_startIndex,
                        ref l_networkName,
                        ref networkNodeIdsReadPtr,
                        ref networkNodeLongNamesReadPtr,
                        ref branchIdsReadPtr,
                        ref branchLongNamesReadPtr,
                        ref l_nodesX,
                        ref l_nodesY,
                        ref l_sourcenodeid,
                        ref l_targetnodeid,
                        ref l_branchlengths,
                        ref l_nbranchgeometrypoints,
                        ref l_geopointsX,
                        ref l_geopointsY,
                        ref l_branch_order);

                    // Close the file
                    ierr = wrapper.ionc_close(ref ioncid);
                    Assert.That(ierr, Is.EqualTo(0));

                    // Allocate arrays for augmented network
                    int secondCaseNumberOfNodes = firstCaseNumberOfNodes + 1;

                    int sl_nnodes = secondCaseNumberOfNodes + 1;
                    int sl_nbranches = secondCaseNumberOfNodes;
                    int sl_nGeometry = secondCaseNumberOfNodes * 3;
                    double[] sl_nodesX = new double[sl_nnodes];
                    double[] sl_nodesY = new double[sl_nnodes];


                    double[] sl_branchlengths = new double[sl_nbranches];
                    int[] sl_nbranchgeometrypoints = new int[sl_nbranches];
                    int[] sl_sourcenodeid = new int[sl_nbranches];
                    int[] sl_targetnodeid = new int[sl_nbranches];
                    int[] sl_branch_order = new int[sl_nbranches];

                    double[] sl_geopointsX = new double[sl_nGeometry];
                    double[] sl_geopointsY = new double[sl_nGeometry];

                    var sl_nodesIds = new string[sl_nnodes];
                    var sl_nodesLongNames = new string[sl_nnodes];
                    var sl_branchIds = new string[sl_nbranches];
                    var sl_branchLongNames = new string[sl_nbranches];

                    // Copy the values of sewer_system in the arrays of the augmented network
                    for (int i = 0; i < l_nnodes; i++)
                    {
                        sl_nodesIds[i] = l_nodesIds[i];
                        sl_nodesLongNames[i] = l_nodesLongNames[i];
                    }

                    for (int i = 0; i < l_nbranches; i++)
                    {
                        sl_branchIds[i] = l_branchIds[i];
                        sl_branchLongNames[i] = l_branchLongNames[i];
                    }

                    Array.Copy(l_nodesX, sl_nodesX, l_nodesX.Length);
                    Array.Copy(l_nodesY, sl_nodesY, l_nodesY.Length);
                    Array.Copy(l_branchlengths, sl_branchlengths, l_branchlengths.Length);
                    Array.Copy(l_nbranchgeometrypoints, sl_nbranchgeometrypoints, l_nbranchgeometrypoints.Length);
                    Array.Copy(l_sourcenodeid, sl_sourcenodeid, l_sourcenodeid.Length);
                    Array.Copy(l_targetnodeid, sl_targetnodeid, l_targetnodeid.Length);
                    Array.Copy(l_branch_order, sl_branch_order, l_branch_order.Length);
                    Array.Copy(l_geopointsX, sl_geopointsX, l_geopointsX.Length);
                    Array.Copy(l_geopointsY, sl_geopointsY, l_geopointsY.Length);

                    //10. Add the stranger..
                    str = "i_am_the_stranger_nodeid";
                    sl_nodesIds[l_nnodes] = str.PadRight(IoNetcdfLibWrapper.idssize, ' ');
                    str = "i_am_the_stranger_nodelongname";
                    sl_nodesLongNames[l_nnodes] = str.PadRight(IoNetcdfLibWrapper.longnamessize, ' ');
                    sl_nodesX[l_nnodes] = -1.0;
                    sl_nodesY[l_nnodes] = -1.0;

                    str = "i_am_the_stranger_branchid";
                    sl_branchIds[l_nbranches] = str.PadRight(IoNetcdfLibWrapper.idssize, ' ');
                    str = "i_am_the_stranger_branchlongname";
                    sl_branchLongNames[l_nbranches] = str.PadRight(IoNetcdfLibWrapper.longnamessize, ' ');
                    sl_branchlengths[l_nbranches] = -1.0;
                    sl_nbranchgeometrypoints[l_nbranches] = 1;
                    sl_sourcenodeid[l_nbranches] = -1;
                    sl_targetnodeid[l_nbranches] = -1;
                    sl_branch_order[l_nbranches] = -1;

                    sl_geopointsX[l_nGeometry] = -1.0;
                    sl_geopointsY[l_nGeometry] = -1.0;

                    // Creates the second file, will not add any dataset 
                    c_path = GridTestHelper.TestDirectoryPath() + @"\LargeSewerSystemSecondTest.nc";
                    GridTestHelper.DeleteIfExists(c_path);
                    Assert.IsFalse(File.Exists(c_path));
                    mode = 1;       // create in write mode
                    ierr = wrapper.ionc_create(c_path, ref mode, ref ioncid);
                    Assert.That(ierr, Is.EqualTo(0));
                    Assert.IsTrue(File.Exists(c_path));
                    addglobalattributes(ioncid, ref wrapper);

                    // Creates and write 1d network
                    string sl_network_name = "sewer_system_with_the_stranger";
                    ierr = wrapper.ionc_create_1d_network(ref ioncid, ref networkid, sl_network_name, ref sl_nnodes,
                        ref sl_nbranches, ref sl_nGeometry);
                    Assert.That(ierr, Is.EqualTo(0));

                    var networkNodeIdsWriteBuffer = StringBufferHandling.MakeStringBuffer(ref sl_nodesIds, IoNetcdfLibWrapper.idssize);
                    var networkNodeLongNamesWriteBuffer = StringBufferHandling.MakeStringBuffer(ref sl_nodesLongNames, IoNetcdfLibWrapper.longnamessize);
                    IntPtr networkNodeIdsWritePtr = register.AddString(ref networkNodeIdsWriteBuffer);
                    IntPtr networkNodeLongNamesWritePtr = register.AddString(ref networkNodeLongNamesWriteBuffer);

                    var branchIdsStringWriteBuffer = StringBufferHandling.MakeStringBuffer(ref sl_branchIds, IoNetcdfLibWrapper.idssize);
                    var branchLongNamesStringWriteBuffer = StringBufferHandling.MakeStringBuffer(ref sl_branchLongNames, IoNetcdfLibWrapper.longnamessize);
                    IntPtr branchIdsWritePtr = register.AddString(ref branchIdsStringWriteBuffer);
                    IntPtr branchLongNamesWritePtr = register.AddString(ref branchLongNamesStringWriteBuffer);

                    write1dnetwork(ioncid,
                        networkid,
                        ref wrapper,
                        sl_nnodes,
                        sl_nbranches,
                        sl_nGeometry,
                        ref networkNodeIdsWritePtr,
                        ref networkNodeLongNamesWritePtr,
                        ref branchIdsWritePtr,
                        ref branchLongNamesWritePtr,
                        ref sl_nodesX,
                        ref sl_nodesY,
                        ref sl_sourcenodeid,
                        ref sl_targetnodeid,
                        ref sl_branchlengths,
                        ref sl_nbranchgeometrypoints,
                        ref sl_geopointsX,
                        ref sl_geopointsY,
                        ref sl_branch_order);

                    // Close the second file
                    ierr = wrapper.ionc_close(ref ioncid);
                    Assert.That(ierr, Is.EqualTo(0));
                },
                stackSize);
            th.Start();
            th.Join();

        }

        // Load 2D, create 1D, create links 1D-2D, save them to file for later processing
        [Test]
        [NUnit.Framework.Category("CreateNetInputFile")]
        public void CreateNetInputFile()
        {

            //1. Load 2d file 
            var wrapperNetcdf = new IoNetcdfLibWrapper();
            string sourcetwod_path = TestHelper.CreateLocalCopy("2d_net_river.nc");
            Assert.IsTrue(File.Exists(sourcetwod_path));
            int sourcetwodioncid = -1; //file id 
            int sourcetwomode = 0; //read mode
            int ierr = wrapperNetcdf.ionc_open(sourcetwod_path, ref sourcetwomode, ref sourcetwodioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            //2. Define the network
            int l_nnodes = 2;
            int l_nbranches = 1;
            int l_nGeometry = 25;
            double[] l_nodesX = { 293.78, 538.89 };
            double[] l_nodesY = { 27.48, 956.75 };
            int[] l_sourcenodeid = { 1 };
            int[] l_targetnodeid = { 2 };
            double[] l_branchlengths = { 1165.29 };
            int[] l_nbranchgeometrypoints = { 25 };
            double[] l_geopointsX =
            {
                293.78,
                278.97,
                265.31,
                254.17,
                247.44,
                248.30,
                259.58,
                282.24,
                314.61,
                354.44,
                398.94,
                445.00,
                490.60,
                532.84,
                566.64,
                589.08,
                600.72,
                603.53,
                599.27,
                590.05,
                577.56,
                562.97,
                547.12,
                530.67,
                538.89
            };
            double[] l_geopointsY =
            {
                27.48,
                74.87,
                122.59,
                170.96,
                220.12,
                269.67,
                317.89,
                361.93,
                399.39,
                428.84,
                450.76,
                469.28,
                488.89,
                514.78,
                550.83,
                594.93,
                643.09,
                692.60,
                742.02,
                790.79,
                838.83,
                886.28,
                933.33,
                980.17,
                956.75
            };
            int[] l_branch_order = { -1 };
            int l_startIndex = startIndex;

            var l_nodesIds = new string[l_nnodes];
            var l_nodesLongNames = new string[l_nnodes];
            var l_branchIds = new string[l_nbranches];
            var l_branchLongNames = new string[l_nbranches];

            string str = "";
            for (int i = 0; i < l_nnodes; i++)
            {
                str = "nodesids" + i;
                l_nodesIds[i] = str.PadRight(IoNetcdfLibWrapper.idssize, ' ');
                str = "nodeslongNames" + i;
                l_nodesLongNames[i] = str.PadRight(IoNetcdfLibWrapper.longnamessize, ' ');
            }

            for (int i = 0; i < l_nbranches; i++)
            {
                str = "branchids" + i;
                l_branchIds[i] = str.PadRight(IoNetcdfLibWrapper.idssize, ' ');
                str = "branchlongNames" + i;
                l_branchLongNames[i] = str.PadRight(IoNetcdfLibWrapper.longnamessize, ' ');
            }

            //3. Now we create a new empty file where to save 1d and 2d meshes
            int targetioncid = -1; //file id  
            int targetmode = 1;    //create in write mode
            string target_path = GridTestHelper.TestDirectoryPath() + "/river1_full_net.nc";
            GridTestHelper.DeleteIfExists(target_path);
            Assert.IsFalse(File.Exists(target_path));

            ierr = wrapperNetcdf.ionc_create(target_path, ref targetmode, ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.IsTrue(File.Exists(target_path));

            addglobalattributes(targetioncid, ref wrapperNetcdf);

            //4. Get the 2d mesh id from the existing file 
            int meshType = 2;
            int sourcemesh2d = -1;
            getMeshid(sourcetwodioncid, ref sourcemesh2d, meshType, ref wrapperNetcdf);
            Assert.That(sourcemesh2d, Is.EqualTo(1));

            //5. Create 1d geometry and mesh in the new file (target.nc)
            int networkid = -1;
            ierr = wrapperNetcdf.ionc_create_1d_network(ref targetioncid, ref networkid, networkName, ref l_nnodes, ref l_nbranches, ref l_nGeometry);
            Assert.That(ierr, Is.EqualTo(0));

            //6. define the network variables
            StringBuilder l_networkName = new StringBuilder(networkName);
            StringBuilder l_meshname = new StringBuilder(meshname);
            int l_nmeshpoints = 25;
            int l_nedgenodes = 24;
            int l_nlinks = 10;
            double[] l_branchoffset =
            {
                0.00,
                49.65,
                99.29,
                148.92,
                198.54,
                248.09,
                297.62,
                347.15,
                396.66,
                446.19,
                495.80,
                545.44,
                595.08,
                644.63,
                694.04,
                743.52,
                793.07,
                842.65,
                892.26,
                941.89,
                991.53,
                1041.17,
                1090.82,
                1140.46,
                1165.29
            };

            int[] l_branchidx =
            {
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1
            };

            double[] l_mesh1dCoordX = l_branchoffset;
            double[] l_mesh1dCoordY = l_branchoffset;

            var l_meshIds = new string[l_nmeshpoints];
            var l_meshLongNames = new string[l_nmeshpoints];
            for (int i = 0; i < l_nmeshpoints; i++)
            {
                str = "meshnodeids" + i;
                l_meshIds[i] = str.PadRight(IoNetcdfLibWrapper.idssize, ' ');
                str = "meshnodelongnames" + i;
                l_meshLongNames[i] = str.PadRight(IoNetcdfLibWrapper.longnamessize, ' ');
            }


            //7. Write the 1d data in the new file (1d geometry, mesh)
            var register = new UnmanagedMemoryRegister();
            var networkNodeIdsBuffer = StringBufferHandling.MakeStringBuffer(ref l_nodesIds, IoNetcdfLibWrapper.idssize);
            var networkNodeLongNamesBuffer = StringBufferHandling.MakeStringBuffer(ref l_nodesLongNames, IoNetcdfLibWrapper.longnamessize);
            IntPtr networkNodeIdsPtr = register.AddString(ref networkNodeIdsBuffer);
            IntPtr networkNodeLongNamesPtr = register.AddString(ref networkNodeLongNamesBuffer);

            var branchIdsStringBuffer = StringBufferHandling.MakeStringBuffer(ref l_branchIds, IoNetcdfLibWrapper.idssize);
            var branchLongNamesStringBuffer = StringBufferHandling.MakeStringBuffer(ref l_branchLongNames, IoNetcdfLibWrapper.longnamessize);
            IntPtr branchIdsPtr = register.AddString(ref branchIdsStringBuffer);
            IntPtr branchLongNamesPtr = register.AddString(ref branchLongNamesStringBuffer);


            write1dnetwork(targetioncid,
                networkid,
                ref wrapperNetcdf,
                l_nnodes,
                l_nbranches,
                l_nGeometry,
                ref networkNodeIdsPtr,
                ref networkNodeLongNamesPtr,
                ref branchIdsPtr,
                ref branchLongNamesPtr,
                ref l_nodesX,
                ref l_nodesY,
                ref l_sourcenodeid,
                ref l_targetnodeid,
                ref l_branchlengths,
                ref l_nbranchgeometrypoints,
                ref l_geopointsX,
                ref l_geopointsY,
                ref l_branch_order);

            var meshNodesIdsBuffer = StringBufferHandling.MakeStringBuffer(ref l_meshIds, IoNetcdfLibWrapper.idssize);
            var meshNodesLongNamesBuffer = StringBufferHandling.MakeStringBuffer(ref l_meshLongNames, IoNetcdfLibWrapper.longnamessize);
            IntPtr meshNodesIdsPtr = register.AddString(ref meshNodesIdsBuffer);
            IntPtr meshNodeslongNamesPtr = register.AddString(ref meshNodesLongNamesBuffer);

            write1dmesh(
                targetioncid,
                ref l_networkName,
                ref l_meshname,
                ref wrapperNetcdf,
                ref meshNodesIdsPtr,
                ref meshNodeslongNamesPtr,
                l_nmeshpoints,
                l_nedgenodes,
                l_nbranches,
                l_nlinks,
                ref l_branchoffset,
                ref l_branchlengths,
                ref l_branchidx,
                ref l_sourcenodeid,
                ref l_targetnodeid,
                l_startIndex,
                ref l_mesh1dCoordX,
                ref l_mesh1dCoordY);

            //8. Clone the 2d mesh definitions in the new file
            int target2dmesh = -1;
            ierr = wrapperNetcdf.ionc_clone_mesh_definition(ref sourcetwodioncid, ref targetioncid, ref sourcemesh2d,
                ref target2dmesh);
            Assert.That(ierr, Is.EqualTo(0));

            //9. Clone the 2d mesh data
            ierr = wrapperNetcdf.ionc_clone_mesh_data(ref sourcetwodioncid, ref targetioncid, ref sourcemesh2d,
                ref target2dmesh);
            Assert.That(ierr, Is.EqualTo(0));

            //10. Close all target and source files
            ierr = wrapperNetcdf.ionc_close(ref sourcetwodioncid);
            Assert.That(ierr, Is.EqualTo(0));
            ierr = wrapperNetcdf.ionc_close(ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));

            //----------------------------------------------------------------------//
            // Create the links from the file containing 2d and 1d ugrid meshes
            //----------------------------------------------------------------------//

            //11. Open file again
            int mode = 1; // weite mode (need to write the links)
            ierr = wrapperNetcdf.ionc_open(target_path, ref mode, ref targetioncid, ref iconvtype, ref convversion);

            //12. Get 2d mesh id
            meshType = 2;
            int mesh2d = -1;
            getMeshid(targetioncid, ref mesh2d, meshType, ref wrapperNetcdf);

            //13. Get 2d mesh dimensions
            var meshtwoddim = new GridWrapper.meshgeomdim();
            int l_networkid = 0;
            ierr = wrapperNetcdf.ionc_get_meshgeom_dim(ref targetioncid, ref mesh2d, ref l_networkid, ref meshtwoddim);
            Assert.That(ierr, Is.EqualTo(0));

            //14. Get the 2d mesh arrays
            var meshtwod = new GridWrapper.meshgeom();
            meshtwod.nodex = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshtwoddim.numnode);
            meshtwod.nodey = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshtwoddim.numnode);
            meshtwod.nodez = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * meshtwoddim.numnode);
            meshtwod.edge_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * meshtwoddim.numedge * 2);
            bool includeArrays = true;
            int start_index = 1;
            ierr = wrapperNetcdf.ionc_get_meshgeom(ref targetioncid, ref mesh2d, ref l_networkid, ref meshtwod, ref start_index, ref includeArrays);
            Assert.That(ierr, Is.EqualTo(0));

            //15. Using the existing arrays in memory (delta shell scenario), convert 1d into herman datastructure
            IntPtr c_meshXCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nmeshpoints);
            IntPtr c_meshYCoords = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nmeshpoints);
            IntPtr c_branchoffset = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nmeshpoints);
            IntPtr c_branchlength = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * l_nbranches);
            IntPtr c_branchids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nmeshpoints);
            IntPtr c_sourcenodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nbranches);
            IntPtr c_targetnodeid = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * l_nbranches);


            Marshal.Copy(l_geopointsX, 0, c_meshXCoords, l_nmeshpoints); // in this case geopoints == meshpoints
            Marshal.Copy(l_geopointsY, 0, c_meshYCoords, l_nmeshpoints);
            Marshal.Copy(l_branchoffset, 0, c_branchoffset, l_nmeshpoints);
            Marshal.Copy(l_branchlengths, 0, c_branchlength, l_nbranches);
            Marshal.Copy(l_branchidx, 0, c_branchids, l_nmeshpoints);
            Marshal.Copy(sourcenodeid, 0, c_sourcenodeid, l_nbranches);
            Marshal.Copy(targetnodeid, 0, c_targetnodeid, l_nbranches);

            //16. fill kn (Herman datastructure) for creating the links
            var wrapperGridgeom = new GridGeomLibWrapper();
            ierr = wrapperGridgeom.ggeo_convert_1d_arrays
            (
                ref c_meshXCoords,
                ref c_meshYCoords,
                ref c_branchoffset,
                ref c_branchlength,
                ref c_branchids,
                ref c_sourcenodeid,
                ref c_targetnodeid,
                ref l_nbranches,
                ref l_nmeshpoints,
                ref l_startIndex
            );
            Assert.That(ierr, Is.EqualTo(0));
            ierr = wrapperGridgeom.ggeo_convert(ref meshtwod, ref meshtwoddim, ref startIndex);
            Assert.That(ierr, Is.EqualTo(0));

            //17. make the links
            int c_npl = 0;
            IntPtr c_xpl = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 0);
            IntPtr c_ypl = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 0);
            IntPtr c_zpl = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * 0);
            int c_nOneDMask = 0;
            IntPtr c_oneDmask = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * 0);
            ierr = wrapperGridgeom.ggeo_make1D2Dinternalnetlinks(ref c_npl, ref c_xpl, ref c_ypl, ref c_zpl, ref c_nOneDMask, ref c_oneDmask);
            Assert.That(ierr, Is.EqualTo(0));

            //18. get the number of links
            int n2dl1dinks = 0;
            int linkType = 3;
            ierr = wrapperGridgeom.ggeo_get_links_count(ref n2dl1dinks, ref linkType);
            Assert.That(ierr, Is.EqualTo(0));

            //19. get the links: arrayfrom = 2d cell index, arrayto = 1d node index 
            IntPtr c_arrayfrom = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * n2dl1dinks); //2d cell number
            IntPtr c_arrayto = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * n2dl1dinks); //1d node
            ierr = wrapperGridgeom.ggeo_get_links(ref c_arrayfrom, ref c_arrayto, ref n2dl1dinks, ref linkType);
            Assert.That(ierr, Is.EqualTo(0));

            int[] rc_arrayfrom = new int[n2dl1dinks];
            int[] rc_arrayto = new int[n2dl1dinks];
            Marshal.Copy(c_arrayfrom, rc_arrayfrom, 0, n2dl1dinks);
            Marshal.Copy(c_arrayto, rc_arrayto, 0, n2dl1dinks);

            int l_linkmesh1 = mesh2d;
            int l_linkmesh2 = -1;
            getMeshid(targetioncid, ref l_linkmesh2, 1, ref wrapperNetcdf);
            int l_locationType1 = 1;
            int l_locationType2 = 1;
            StringBuilder l_linkmeshname = new StringBuilder("2d1dlinks");
            int[] contacttype = new int[n2dl1dinks];
            for (int i = 0; i < n2dl1dinks; i++)
            {
                contacttype[i] = 3;
            }

            //20. write 2d-1d links to file.
            write1d2dlinks(
                targetioncid,
                l_linkmesh1,
                l_linkmesh2,
                l_locationType1,
                l_locationType2,
                ref l_linkmeshname,
                ref wrapperNetcdf,
                n2dl1dinks,
                ref rc_arrayfrom,
                ref rc_arrayto,
                ref contacttype,
                l_startIndex);

            //21. Close all files 
            ierr = wrapperNetcdf.ionc_close(ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));

            //22. Free memory  
            //Free 2d arrays
            Marshal.FreeCoTaskMem(meshtwod.nodex);
            Marshal.FreeCoTaskMem(meshtwod.nodey);
            Marshal.FreeCoTaskMem(meshtwod.nodez);
            Marshal.FreeCoTaskMem(meshtwod.edge_nodes);
            //Free 1d arrays
            Marshal.FreeCoTaskMem(c_meshXCoords);
            Marshal.FreeCoTaskMem(c_meshYCoords);
            Marshal.FreeCoTaskMem(c_branchoffset);
            Marshal.FreeCoTaskMem(c_branchlength);
            Marshal.FreeCoTaskMem(c_branchids);
            Marshal.FreeCoTaskMem(c_sourcenodeid);
            Marshal.FreeCoTaskMem(c_targetnodeid);
            //Free links arrays
            Marshal.FreeCoTaskMem(c_arrayfrom);
            Marshal.FreeCoTaskMem(c_arrayto);
        }

        // Test: get only 1d network using get mesh geom
        [Test]
        [NUnit.Framework.Category("Read1dNetworkUsingGetMeshGeom")]
        public void Get1dNetworkUsingGetMeshGeom()
        {
            //1.Open a netcdf file
            string c_path = GridTestHelper.TestFilesDirectoryPath() + @"\write1dNetwork.nc";
            Assert.IsTrue(File.Exists(c_path));
            int ioncid = 0; //file variable 
            int mode = 0; //create in read mode
            var wrapper = new IoNetcdfLibWrapper();
            var ierr = wrapper.ionc_open(c_path, ref mode, ref ioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            //2. Get 1d mesh dimensions
            var networkdim = new GridWrapper.meshgeomdim();
            int l_meshid = 0; //set an invalid index
            int nnumNetworks = -1;
            ierr = wrapper.ionc_get_number_of_networks(ref ioncid, ref nnumNetworks);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.That(nnumNetworks, Is.EqualTo(1));
            IntPtr c_networkids = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nnumNetworks);

            //3. Get a valid networkid
            ierr = wrapper.ionc_get_network_ids(ref ioncid, ref c_networkids, ref nnumNetworks);
            Assert.That(ierr, Is.EqualTo(0));
            int[] l_networkid = new int[nnumNetworks];
            Marshal.Copy(c_networkids, l_networkid, 0, nnumNetworks);

            //4. Get network dimensions using meshgeom
            ierr = wrapper.ionc_get_meshgeom_dim(ref ioncid, ref l_meshid, ref l_networkid[0], ref networkdim);
            Assert.That(ierr, Is.EqualTo(0));

            //5. Allocate memory
            var network = new GridWrapper.meshgeom();
            MeshgeomMemoryManager.allocate(ref networkdim, ref network);

            var includeArrays = true;
            int start_index = 1;
            ierr = wrapper.ionc_get_meshgeom(ref ioncid, ref l_meshid, ref l_networkid[0], ref network, ref start_index, ref includeArrays);
            Assert.That(ierr, Is.EqualTo(0));

            //2. Define the network
            int l_nnodes = 2;
            int l_nbranches = 1;
            int l_nGeometry = 25;
            double[] l_nodesX = new double[networkdim.nnodes];
            double[] l_nodesY = new double[networkdim.nnodes];
            int[] l_nedge_nodes = new int[networkdim.nbranches];
            double[] l_nbranchlengths = new double[networkdim.nbranches];
            double[] l_ngeopointx = new double[networkdim.ngeometry];
            double[] l_ngeopointy = new double[networkdim.ngeometry];

            Marshal.Copy(network.nnodex, l_nodesX, 0, networkdim.nnodes);
            Marshal.Copy(network.nnodey, l_nodesY, 0, networkdim.nnodes);
            Marshal.Copy(network.nedge_nodes, l_nedge_nodes, 0, networkdim.nbranches);
            Marshal.Copy(network.nbranchlengths, l_nbranchlengths, 0, networkdim.nbranches);
            Marshal.Copy(network.ngeopointx, l_ngeopointx, 0, networkdim.ngeometry);
            Marshal.Copy(network.ngeopointy, l_ngeopointy, 0, networkdim.ngeometry);

            //8. Close the file
            ierr = wrapper.ionc_close(ref ioncid);
            Assert.That(ierr, Is.EqualTo(0));

        }

        // Test: get only 1d network using get mesh geom
        [Test]
        [NUnit.Framework.Category("Read1dNetworkUsingGetMeshGeom")]
        public void Put2dMeshUsingPutMeshGeom()
        {
            //1. Open a netcdf file
            string c_path = GridTestHelper.TestFilesDirectoryPath() + @"\Custom_Ugrid.nc";
            Assert.IsTrue(File.Exists(c_path));
            int ioncid = -1;
            int mode = 0; //read mode
            var wrapper = new IoNetcdfLibWrapper();
            var ierr = wrapper.ionc_open(c_path, ref mode, ref ioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            //2. Get the mesh dimensions in meshdim
            int existingMeshId = 1;
            int existingNetworkId = -1;
            var meshdim = new GridWrapper.meshgeomdim();
            ierr = wrapper.ionc_get_meshgeom_dim(ref ioncid, ref existingMeshId, ref existingNetworkId, ref meshdim);
            Assert.That(ierr, Is.EqualTo(0));

            //3. Allocate mesh
            var mesh = new GridWrapper.meshgeom();
            MeshgeomMemoryManager.allocate(ref meshdim, ref mesh);

            //4. Get mesh variables
            int start_index = 1; //arrays are 1 based
            bool includeArrays = true;
            ierr = wrapper.ionc_get_meshgeom(ref ioncid, ref existingMeshId, ref existingNetworkId, ref mesh, ref start_index, ref includeArrays);
            Assert.That(ierr, Is.EqualTo(0));

            //5. Close the file
            ierr = wrapper.ionc_close(ref ioncid);
            Assert.That(ierr, Is.EqualTo(0));

            //6. Create a new netcdf file
            int targetioncid = -1; //file id  
            int targetmode = 1; //create in write mode
            string target_path = GridTestHelper.TestDirectoryPath() + "/target.nc";
            GridTestHelper.DeleteIfExists(target_path);
            Assert.IsFalse(File.Exists(target_path));
            ierr = wrapper.ionc_create(target_path, ref targetmode, ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.IsTrue(File.Exists(target_path));

            //7. write a 2d mesh using ionc_put_meshgeom
            string meshname = "my_mesh";
            string networkname = ""; //empty string if mesh not available
            int newMeshId = -1;
            int newNetworkId = -1;
            ierr = wrapper.ionc_put_meshgeom(ref targetioncid, ref newMeshId, ref newNetworkId, ref mesh, ref meshdim, meshname, networkname, ref start_index);
            Assert.That(ierr, Is.EqualTo(0));
            //8. Close the file
            ierr = wrapper.ionc_close(ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));
        }

        // Test: get only 1d network using get mesh geom
        [Test]
        [NUnit.Framework.Category("Read1dNetworkUsingGetMeshGeom")]
        public void Put2dMeshUsingPutMeshGeomPutNodeZ()
        {
            //1. Open a netcdf file
            string c_path = GridTestHelper.TestFilesDirectoryPath() + @"\Custom_Ugrid.nc";
            Assert.IsTrue(File.Exists(c_path));
            int ioncid = -1;
            int mode = 0; //read mode
            var wrapper = new IoNetcdfLibWrapper();
            var ierr = wrapper.ionc_open(c_path, ref mode, ref ioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            //2. Get the mesh dimensions in meshdim
            int existingMeshId = 1;
            int existingNetworkId = -1;
            var meshdim = new GridWrapper.meshgeomdim();
            ierr = wrapper.ionc_get_meshgeom_dim(ref ioncid, ref existingMeshId, ref existingNetworkId, ref meshdim);
            Assert.That(ierr, Is.EqualTo(0));

            //3. Allocate mesh
            var mesh = new GridWrapper.meshgeom();
            MeshgeomMemoryManager.allocate(ref meshdim, ref mesh);

            //4. Get mesh variables
            int start_index = 1; //arrays are 1 based
            bool includeArrays = true;
            ierr = wrapper.ionc_get_meshgeom(ref ioncid, ref existingMeshId, ref existingNetworkId, ref mesh, ref start_index, ref includeArrays);
            Assert.That(ierr, Is.EqualTo(0));

            //5. Close the file
            ierr = wrapper.ionc_close(ref ioncid);
            Assert.That(ierr, Is.EqualTo(0));

            //6. Create a new netcdf file
            int targetioncid = -1; //file id  
            int targetmode = 1; //create in write mode
            string target_path = GridTestHelper.TestDirectoryPath() + "/target.nc";
            GridTestHelper.DeleteIfExists(target_path);
            Assert.IsFalse(File.Exists(target_path));
            ierr = wrapper.ionc_create(target_path, ref targetmode, ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));
            Assert.IsTrue(File.Exists(target_path));

            //7. write a 2d mesh using ionc_put_meshgeom
            string meshname = "my_mesh";
            string networkname = ""; //empty string if mesh not available
            int newMeshId = -1;
            int newNetworkId = -1;
            ierr = wrapper.ionc_put_meshgeom(ref targetioncid, ref newMeshId, ref newNetworkId, ref mesh, ref meshdim, meshname, networkname, ref start_index);
            Assert.That(ierr, Is.EqualTo(0));
            addglobalattributes(targetioncid, ref wrapper);
            //8. Close the file
            ierr = wrapper.ionc_close(ref targetioncid);
            Assert.That(ierr, Is.EqualTo(0));

            int myioncid = -1;
            ierr = wrapper.ionc_open(target_path, ref targetmode, ref myioncid, ref iconvtype, ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            int mymesh2dId = 0;
            ierr = wrapper.ionc_get_2d_mesh_id(ref myioncid, ref mymesh2dId);
            Assert.That(ierr, Is.EqualTo(0));

            ierr = WriteZCoordinateValues(wrapper, myioncid, mymesh2dId, 1, "node_z", "nodeess", new double[1] { 80.1 });
            Assert.That(ierr, Is.EqualTo(0));

            ierr = wrapper.ionc_close(ref myioncid);
            Assert.That(ierr, Is.EqualTo(0));


        }


        public int WriteZCoordinateValues(IoNetcdfLibWrapper wrapper, int ioncId, int meshId, int locationType, string varName, string longName, double[] zValues)
        {

            var nVal = zValues.Length;
            var nCalVal = 0;
            int ierr;
            ierr = wrapper.ionc_get_node_count(ref ioncId, ref meshId, ref nCalVal);

            IntPtr zPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(double)) * nCalVal);

            try
            {
                int varId = 0;
                wrapper.ionc_inq_varid_by_standard_name(ref ioncId, ref meshId, ref locationType, "altitude", ref varId);

                // Testing...
                wrapper.InqueryVariableId(ioncId, meshId, varName, ref varId);

                if (varId == -1) // does not exist
                {
                    wrapper.DefineVariable(ioncId, meshId, varId, (int)NetcdfDataType.nf90_double, locationType, varName,
                        "altitude", longName, "m", -999.0);
                }
                if (nVal == nCalVal)
                {
                    Marshal.Copy(zValues, 0, zPtr, nVal);

                    // Eventually the idea is to change PutVariable to use varId rather than varName
                    ierr = wrapper.PutVariable(ioncId, meshId, locationType, varName, zPtr, nVal);
                    return ierr;
                }
                var zCalValues = new double[nCalVal];
                for (int i = 0; i < nVal; i++)
                {
                    zCalValues[i] = zValues[i];
                }
                for (int i = nVal; i < nCalVal; i++)
                {
                    zCalValues[i] = -999.0;
                }
                Marshal.Copy(zCalValues, 0, zPtr, nCalVal);

                // Eventually the idea is to change PutVariable to use varId rather than varName
                ierr = wrapper.PutVariable(ioncId, meshId, locationType, varName, zPtr, nCalVal);
                return ierr;
            }
            catch
            {
                return 0;
            }

            finally
            {
                if (zPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(zPtr);
                zPtr = IntPtr.Zero;
            }
        }


        //open a file test
        [Test]
        [NUnit.Framework.Category("UGRIDTests")]
        public void openAfile()
        {
            var wrapper = new IoNetcdfLibWrapper();

            // Open a file 
            string c_path = GridTestHelper.TestFilesDirectoryPath() + @"\Custom_Ugrid_map.nc";
            Assert.IsTrue(File.Exists(c_path));
            int ioncid = -1; //file id 
            int sourcetwomode = 0; //read mode
            int ierr = wrapper.ionc_open(c_path, ref sourcetwomode, ref ioncid, ref iconvtype,
                ref convversion);
            Assert.That(ierr, Is.EqualTo(0));

            // test functions on costum file 
            int meshid = 0;
            ierr = wrapper.ionc_get_2d_mesh_id(ref ioncid, ref meshid);
            Assert.That(ierr, Is.EqualTo(0));
            int nmaxfacenodes = 0;

            int nnodes = -1;
            ierr = wrapper.ionc_get_node_count(ref ioncid, ref meshid, ref nnodes);
            Assert.That(ierr, Is.EqualTo(0));

            int nedge = -1;
            ierr = wrapper.ionc_get_edge_count(ref ioncid, ref meshid, ref nedge);
            Assert.That(ierr, Is.EqualTo(0));

            int nface = -1;
            ierr = wrapper.ionc_get_face_count(ref ioncid, ref meshid, ref nface);
            Assert.That(ierr, Is.EqualTo(0));

            int maxfacenodes = -1;
            ierr = wrapper.ionc_get_max_face_nodes(ref ioncid, ref meshid, ref maxfacenodes);
            Assert.That(ierr, Is.EqualTo(0));

            IntPtr c_face_nodes = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * nface * maxfacenodes);

            int fillvalue = -1;
            int startIndex = 0;
            ierr = wrapper.ionc_get_face_nodes(ref ioncid, ref meshid, ref c_face_nodes, ref nface, ref maxfacenodes, ref fillvalue, ref startIndex);
            Assert.That(ierr, Is.EqualTo(0));

            int[] rc_face_nodes = new int[nface * maxfacenodes];
            Marshal.Copy(c_face_nodes, rc_face_nodes, 0, nface * maxfacenodes);
        }

        // testing writing of coordinates for 1d ugrid mesh
        // int ierr = wrapper.ionc_create_1d_mesh_v2(ref ioncid, l_networkName.ToString(), ref meshid, l_meshname.ToString(), ref l_nmeshpoints, TRUE);
    }
}
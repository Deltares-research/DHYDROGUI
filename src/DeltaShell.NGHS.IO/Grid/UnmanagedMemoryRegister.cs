using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DeltaShell.NGHS.IO.Grid
{
    // Unmanaged memory bookeeping
    public class UnmanagedMemoryRegister : IDisposable
    {
        private readonly List<GCHandle> objectGarbageCollectHandles = new List<GCHandle>();

        public void Add(ref string str, ref IntPtr ptr)
        {
            Encoding ascii = Encoding.ASCII;
            Encoding unicode = Encoding.Unicode;

            byte[] unicodeArray = unicode.GetBytes(str.ToString());
            byte[] asciiArray = Encoding.Convert(unicode, ascii, unicodeArray);
            PinMemory(asciiArray);
            ptr = objectGarbageCollectHandles.Last().AddrOfPinnedObject();
        }

        public void Add(ref double[] arr, ref IntPtr ptr)
        {
            PinMemory(arr);
            ptr = objectGarbageCollectHandles.Last().AddrOfPinnedObject();
        }

        public void Add(ref int[] arr, ref IntPtr ptr)
        {
            PinMemory(arr);
            ptr = objectGarbageCollectHandles.Last().AddrOfPinnedObject();
        }


        public void Add<T>(int dim, ref IntPtr ptr)
        {
            T[] arr = new T[dim];
            PinMemory(arr);
            ptr = objectGarbageCollectHandles.Last().AddrOfPinnedObject();
        }

        public void Add(ref GridWrapper.meshgeomdim meshdim, ref GridWrapper.meshgeom mesh)
        {
            if (meshdim.numnode > 0)
            {
                Add<double>(meshdim.numnode, ref mesh.nodex);
                Add<double>(meshdim.numnode, ref mesh.nodey);
                Add<double>(meshdim.numnode, ref mesh.nodez);
                Add<int>(meshdim.numnode, ref mesh.branchidx);
                Add<double>(meshdim.numnode, ref mesh.branchoffsets);
                Add<char>(meshdim.numnode * GridWrapper.idssize, ref mesh.nodeids);
                Add<char>(meshdim.numnode * GridWrapper.longnamessize, ref mesh.nodelongnames);
            }

            if (meshdim.numedge > 0)
            {
                Add<int>(meshdim.numedge * 2, ref mesh.edge_nodes);
                Add<int>(meshdim.numedge * 2, ref mesh.edge_faces);
                Add<double>(meshdim.numedge, ref mesh.edgex);
                Add<double>(meshdim.numedge, ref mesh.edgey);
            }

            if (meshdim.numface > 0)
            {
                Add<int>(meshdim.maxnumfacenodes * meshdim.numface, ref mesh.face_nodes);
                Add<int>(meshdim.maxnumfacenodes * meshdim.numface, ref mesh.face_edges);
                Add<int>(meshdim.maxnumfacenodes * meshdim.numface, ref mesh.face_links);
                Add<double>(meshdim.numface, ref mesh.facex);
                Add<double>(meshdim.numface, ref mesh.facey);
            }

            //network part
            if (meshdim.nnodes > 0)
            {
                Add<double>(meshdim.nnodes, ref mesh.nnodex);
                Add<double>(meshdim.nnodes, ref mesh.nnodey);
                Add<char>(meshdim.nnodes * GridWrapper.idssize, ref mesh.nnodeids);
                Add<char>(meshdim.nnodes * GridWrapper.longnamessize, ref mesh.nnodelongnames);
            }

            if (meshdim.nbranches > 0)
            {
                Add<double>(meshdim.nbranches, ref mesh.nbranchlengths);
                Add<int>(meshdim.nbranches, ref mesh.nbranchgeometrynodes);
                Add<int>(meshdim.nbranches * 2, ref mesh.nedge_nodes);
                Add<int>(meshdim.nbranches, ref mesh.nbranchorder);
                Add<char>(meshdim.nbranches * GridWrapper.idssize, ref mesh.nbranchids);
                Add<char>(meshdim.nbranches * GridWrapper.longnamessize, ref mesh.nbranchlongnames);
            }

            if (meshdim.ngeometry > 0)
            {
                Add<double>(meshdim.ngeometry, ref mesh.ngeopointx);
                Add<double>(meshdim.ngeometry, ref mesh.ngeopointy);
            }
        }

        public void Dispose()
        {
            UnPinMemory();
        }

        private void UnPinMemory()
        {
            foreach (var handle in objectGarbageCollectHandles)
            {
                handle.Free();
            }

            objectGarbageCollectHandles.Clear();
        }

        private void PinMemory(object o)
        {
            // once pinned the object cannot be deleted by the garbage collector
            objectGarbageCollectHandles.Add(GCHandle.Alloc(o, GCHandleType.Pinned));
        }

        public IntPtr AddString(ref string str)
        {
            var ptr = IntPtr.Zero;
            Add(ref str, ref ptr);
            return ptr;
        }
    }
}
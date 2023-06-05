using DelftTools.Utils.Guards;
using Deltares.UGrid.Api;
using DeltaShell.NGHS.IO.Properties;
using log4net;

namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    /// <summary>
    /// Will be used to collect all objects needed to generate our 1D2D links from a (ugrid) file
    /// and the possibility to read the links too.
    /// </summary>
    public class GeneratedObjectsForLinks : ConvertedUgridFileObjects
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GeneratedObjectsForLinks));

        public Disposable1DMeshGeometry Mesh1d { get; set; }
        public Disposable2DMeshGeometry Mesh2d { get; set; }
        public DisposableNetworkGeometry NetworkGeometry { get; set; }
        public DisposableLinksGeometry LinksGeometry { get; set; }
        public int FillValueMesh2DFaceNodes { get; set; } = (int)UGridFileHelper.DefaultNoDataValue;

        /// <summary>
        /// Reads the set of 1d/2d links from the UGrid file
        /// </summary>
        /// <param name="path">Path to the UGrid file</param>
        /// <param name="api">Ugrid Api used to read mesh data from file <seealso cref="IUGridApi"/></param>
        /// <exception cref="IoNetCdfNativeError">This error is thrown when an error code is
        /// returned from a native function</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="api"/> is <c>null</c>.</exception>
        public void Read1D2DLinks(string path, IUGridApi api)
        {
            Ensure.NotNull(api, nameof(api));
            
            api.Open(path);

            if (!api.IsUGridFile())
            {
                Log.Error(string.Format(Resources.GeneratedObjectsForLinks_Read1D2DLinks_Could_not_load_links_from__0___This_is_not_a_UGrid_file_, path));
                Links1D2D?.Clear();
                return;
            }
            var meshIds2d = api.GetMeshIdsByMeshType(UGridMeshType.Mesh2D);
            if (NeedToReadMesh2D() && meshIds2d.Length != 0)
            {
                Mesh2d = api.GetMesh2D(meshIds2d[0]);
            }
            if(meshIds2d.Length != 0)
            {
                FillValueMesh2DFaceNodes = (int)api.GetVariableNoDataValue(Mesh2d.Name + "_face_nodes", meshIds2d[0], GridLocationType.None);
            }

            if (NeedToReadMesh1D())
            {
                var meshIds1d = api.GetMeshIdsByMeshType(UGridMeshType.Mesh1D);
                if (meshIds1d.Length != 0)
                {
                    Mesh1d = api.GetMesh1D(meshIds1d[0]);
                }
            }

            if (NeedToReadNetworkGeometry())
            {
                var networkIds = api.GetNetworkIds();
                if (networkIds.Length != 0)
                {
                    NetworkGeometry = api.GetNetworkGeometry(networkIds[0]);
                }
            }

            var linksId = api.GetLinksId();

            if (linksId != -1) // when api return -1 no link administration could be found in the file
            {
                LinksGeometry = api.GetLinks(linksId);
                Links1D2D?.SetLinks(this);
            }
            else
            {
                Links1D2D?.Clear();
            }
        }

        private bool NeedToReadNetworkGeometry()
        {
            return Discretization?.Network != null && NetworkGeometry == null;
        }

        private bool NeedToReadMesh1D()
        {
            return Grid != null && Mesh1d == null;
        }

        private bool NeedToReadMesh2D()
        {
            return Grid != null && Mesh2d == null;
        }
    }
}
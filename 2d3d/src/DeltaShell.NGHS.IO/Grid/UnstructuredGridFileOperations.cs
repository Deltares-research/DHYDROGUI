using System;
using System.IO;
using DeltaShell.NGHS.IO.Adapters;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.CoordinateSystems;
using log4net;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.NGHS.IO.Grid
{
    /// <summary>
    /// Class representing an unstructured grid file and the operations that can be performed on it.
    /// </summary>
    public class UnstructuredGridFileOperations
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(UnstructuredGridFileOperations));
        private readonly string filePath;

        /// <summary>
        /// Creates a new instance of <see cref="UnstructuredGridFileOperations"/>.
        /// </summary>
        /// <param name="filePath">The file path to the file containing an unstructured grid.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is <c>null</c>, empty or consists of whitespaces.</exception>
        public UnstructuredGridFileOperations(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(filePath));
            }

            if (!File.Exists(filePath) || Path.GetFileName(filePath) == null)
            {
                log.ErrorFormat("Could not find grid at \"{0}\"", filePath);
            }

            this.filePath = filePath;
            DataSetConventions = GetConvention(filePath);
        }

        /// <summary>
        /// Gets the <see cref="GridApiDataSet.DataSetConventions"/>.
        /// </summary>
        public GridApiDataSet.DataSetConventions DataSetConventions { get; }

        /// <summary>
        /// Gets the <see cref="UnstructuredGrid"/>.
        /// </summary>
        /// <param name="loadFlowLinksAndCells">Boolean indicator to load flow links and cells, defaults to <c>false</c>.</param>
        /// <param name="callCreateCells">Boolean indicator whether cells need to be created, defaults to <c>false</c>.</param>
        /// <returns>An <see cref="UnstructuredGrid"/>, <c>null</c> if the grid could not be read.</returns>
        /// <remarks>
        /// CreateCells will recalculate the cell centers using the kernel.
        /// This will ensure the correct cell centers will be used for spatial
        /// operations. This should be called for input grids that are used for
        /// spatial operations. This SHOULD NOT be called for output grids.
        /// CreateCells will reshuffle the indices. When this is called for output
        /// grids, the data associated with cells will be incorrect, if the indices
        /// are reshuffled.
        /// </remarks>
        public UnstructuredGrid GetGrid(bool loadFlowLinksAndCells = false,
                                        bool callCreateCells = false)
        {
            switch (DataSetConventions)
            {
                case GridApiDataSet.DataSetConventions.CONV_UGRID:
                    using (var fmUGridAdapter = new UGridToUnstructuredGridAdapter(filePath))
                    {
                        return fmUGridAdapter.GetUnstructuredGridFromUGridMeshId(1, callCreateCells: callCreateCells);
                    }
                case GridApiDataSet.DataSetConventions.CONV_OTHER:
                    return loadFlowLinksAndCells
                               ? NetFileImporter.ImportModelGrid(filePath)
                               : NetFileImporter.ImportGrid(filePath);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Performs an action when the grid is of type <see cref="GridApiDataSet.DataSetConventions.CONV_UGRID"/>.
        /// </summary>
        /// <param name="ugridAction">
        /// Performs an <see cref="Action"/> if the grid is of type
        /// <see cref="GridApiDataSet.DataSetConventions.CONV_UGRID"/>
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ugridAction"/> is <c>null</c>.</exception>
        public void DoIfUgrid(Action<UGridToUnstructuredGridAdapter> ugridAction)
        {
            if (ugridAction == null)
            {
                throw new ArgumentNullException(nameof(ugridAction));
            }

            if (DataSetConventions != GridApiDataSet.DataSetConventions.CONV_UGRID)
            {
                return;
            }

            using (var uGridAdaptor = new UGridToUnstructuredGridAdapter(filePath))
            {
                ugridAction(uGridAdaptor);
            }
        }

        /// <summary>
        /// Gets the coordinate system that is contained within the file.
        /// </summary>
        /// <returns>An <see cref="ICoordinateSystem"/>, <c>null</c> when the coordinate system could not be determined.</returns>
        public ICoordinateSystem GetCoordinateSystem()
        {
            switch (DataSetConventions)
            {
                case GridApiDataSet.DataSetConventions.CONV_UGRID:
                    using (var uGrid = new UGrid(filePath))
                    {
                        if (!uGrid.IsInitialized())
                        {
                            uGrid.Initialize();
                        }

                        return uGrid.CoordinateSystem;
                    }
                case GridApiDataSet.DataSetConventions.CONV_OTHER:
                    return NetFile.ReadCoordinateSystem(filePath);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Retrieves <see cref="GridApiDataSet.DataSetConventions"/> from the unstructured file.
        /// </summary>
        /// <param name="path">The file path to the file to retrieve the convention from.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when the convention could not be retrieved.</exception>
        private static GridApiDataSet.DataSetConventions GetConvention(string path)
        {
            IUGridApi gridApi = GridApiFactory.CreateNew();
            if (gridApi == null)
            {
                return GridApiDataSet.DataSetConventions.CONV_NULL;
            }

            using (gridApi)
            {
                GridApiDataSet.DataSetConventions convention;
                int ierr = gridApi.GetConvention(path, out convention);
                if (ierr != GridApiDataSet.GridConstants.NOERR)
                {
                    throw new ArgumentException("Couldn't get the grid convention because of error number: " + ierr);
                }

                return convention;
            }
        }
    }
}
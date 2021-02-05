using System;
using DeltaShell.NGHS.IO.Adapters;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.CoordinateSystems;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UnstructuredGridFileOperations
    {
        private readonly string filePath;
        private readonly GridApiDataSet.DataSetConventions dataSetConventions;
        
        public UnstructuredGridFileOperations(string filePath)
        {
            this.filePath = filePath;
            dataSetConventions = GetConvention(filePath);
        }

        public UnstructuredGrid GetGrid(bool loadFlowLinksAndCells = false,
                                        bool callCreateCells = false)
        {
            switch (dataSetConventions)
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
        
        public void DoIfUgrid(Action<UGridToUnstructuredGridAdapter> ugridAction)
        {
            if (dataSetConventions != GridApiDataSet.DataSetConventions.CONV_UGRID)
            {
                return;
            }

            using (var uGridAdaptor = new UGridToUnstructuredGridAdapter(filePath))
            {
                ugridAction(uGridAdaptor);
            }
        }
        
        public ICoordinateSystem GetCoordinateSystem()
        {
            switch (dataSetConventions)
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
                    throw new Exception("Couldn't get the grid convention because of error number: " + ierr);
                }

                return convention;
            }
        }
    }
}
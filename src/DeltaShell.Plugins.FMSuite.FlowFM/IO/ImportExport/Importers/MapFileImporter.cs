using System;
using System.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    //Todo : remove => only used in tests
    public static class MapFileImporter
    {
        public static UnstructuredGrid Import(string mduPath, string mapFile)
        {
            try
            {
                IFlexibleMeshModelApi api = FlexibleMeshModelApiFactory.CreateNew();
                if (api == null)
                {
                    return null;
                }

                using (api)
                {
                    api.Initialize(mduPath); // todo: get a more dedicated (quicker) call here
                }

                mapFile = Path.IsPathRooted(mapFile)
                              ? mapFile
                              : Path.Combine(Path.GetDirectoryName(mduPath), mapFile);

                // use the netcdf importer to read the updated map file
                return NetFileImporter.ImportModelGrid(mapFile);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
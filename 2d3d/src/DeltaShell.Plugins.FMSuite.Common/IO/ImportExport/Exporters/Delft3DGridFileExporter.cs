using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.IO.Writers;
using log4net;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Common.IO.ImportExport.Exporters
{
    public class Delft3DGridFileExporter : IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Delft3DGridFileExporter));

        public string Name => "Delft3D Grid";

        public string Category => "General";

        public string Description => string.Empty;

        public string FileFilter => "Delft3D Grid File (*.grd)|*.grd";

        public Bitmap Icon { get; private set; }

        public bool Export(object item, string path)
        {
            var grid = item as CurvilinearGrid;
            if (grid == null)
            {
                return false;
            }

            try
            {
                Delft3DGridFileWriter.Write(grid, path);
            }
            catch (Exception e)
            {
                Log.ErrorFormat("Failed to export grid: {0}", e.Message);
                return false;
            }

            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(CurvilinearGrid);
        }

        public bool CanExportFor(object item)
        {
            return true;
        }
    }
}
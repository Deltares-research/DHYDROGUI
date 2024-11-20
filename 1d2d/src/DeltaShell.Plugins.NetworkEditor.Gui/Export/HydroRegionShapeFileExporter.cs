using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Export
{
    public class HydroRegionShapeFileExporter : IFileExporter
    {
        private readonly IGui gui;
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroRegionShapeFileExporter));

        public HydroRegionShapeFileExporter(IGui gui)
        {
            this.gui = gui;
        }

        public string Name
        {
            get { return "HydroRegion to Esri Shapefile"; }
        }
        public string Description { get { return Name; } }
        public string Category { get { return "General"; } }

        public bool Export(object item, string path)
        {
            var hydroRegion = item as IHydroRegion;
            if (item == null)
            {
                return false;
            }

            var layer = MapLayerProviderHelper.CreateLayersRecursive(hydroRegion, null, gui.Plugins.Select(p => p.MapLayerProvider).ToList());
            var exporter = new ShapeFileExporter();

            if (!exporter.Export(layer, path))
            {
                log.Warn("Export to shapefile failed, or Hydro Region has no features");
                return false;
            }

            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(IHydroRegion);
        }

        public string FileFilter
        {
            get { return "Esri shapefiles (*.shp)|*.shp"; }
        }

        public Bitmap Icon { get; private set; }
        public bool CanExportFor(object item)
        {
            return true;
        }
    }
}

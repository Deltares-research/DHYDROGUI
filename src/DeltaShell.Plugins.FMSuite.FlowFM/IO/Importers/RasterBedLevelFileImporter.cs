using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using log4net;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class RasterBedLevelFileImporter : IFileImporter
    {
        

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find file {path}");
            }

            if (target == null)
            {
                return new DataItem { Value = new ImportedFMNetFile(path), Name = Path.GetFileName(path) };
            }
        
            return new RasterFile().Read(path);
        }


        public string Name
        {
            get { return "Raster Bed Level Importer"; }
        }

        public string Category
        {
            get {return "2D / 3D";}
        }

        public Bitmap Image { get; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(UnstructuredGrid); }
        }
        public bool CanImportOnRootLevel { get; }

        public string FileFilter
        {
            get { string fileFilter = "";
                fileFilter += "All supported raster formats|*.asc;*.bil;*.tif;*.tiff;*.map";
                fileFilter += "|" + "Arc/Info ASCII Grid (*.asc)|*.asc";
                fileFilter += "|" + "ESRI .hdr Labelled (*.bil)|*.bil";
                fileFilter += "|" + "TIF Tagget Image File Format (*.tif)|*.tif;*.tiff";
                fileFilter += "|" + "PCRaster raster file format (*.map)|*.map"; ;
                return fileFilter;
            }
           
        }

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get; }
    }
}

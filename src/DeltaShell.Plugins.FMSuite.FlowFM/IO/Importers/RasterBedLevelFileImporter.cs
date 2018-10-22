using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class RasterBedLevelFileImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RasterBedLevelFileImporter));
        //TODO: Implement limit in file size of 2GB.
        //private static double AscFileSizeErrorLimitInBytes = 2.0e9;

        public Func<UnstructuredGrid, WaterFlowFMModel> GetModelForGrid { get; set; }

        private static IRegularGridCoverage ImportAscFileToRegularGridCoverage(string ascFilePath)
        {
            var importer = new GdalFileImporter();
            var regularGrid = importer.ImportItem(ascFilePath) as IRegularGridCoverage;

            return regularGrid;
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        private static IList<PointValue> ConvertRegularGridToBedLevelValues(IRegularGridCoverage gridCoverage)
        {
            var xValues = gridCoverage.X.Values;
            var yValues = gridCoverage.Y.Values;
            
            //Insert values at the center of the cell
            var deltaX = gridCoverage.DeltaX / 2.0;
            var deltaY = gridCoverage.DeltaY / 2.0;

            var values = gridCoverage.GetValues<float>();

            var pointValueList = new List<PointValue>();

            try
            {
                for (var i = 0; i < yValues.Count; i++)
                {
                    for (var j = 0; j < xValues.Count; j++)
                    {
                        var pointValue = new PointValue
                        {
                            X = xValues[j] + deltaX,
                            Y = yValues[i] + deltaY,
                            Value = values[2 * i + j]
                        };
                        pointValueList.Add(pointValue);
                    }
                }
            }
            catch (Exception)
            {
                Log.Error("The file you are trying to import only contains integers. This is not yet supported. Please change a minimum of one value to a decimal number in the import file");
            }

            return pointValueList;
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
        
            var regularGridCoverage = ImportAscFileToRegularGridCoverage(path);
            var pointValuesList = ConvertRegularGridToBedLevelValues(regularGridCoverage);

            return pointValuesList;
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

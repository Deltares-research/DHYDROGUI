using System;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands.SpatialOperations;
using GeoAPI.Extensions.Coverages;
using SharpMap.Api.Layers;
using SharpMap.Api.SpatialOperations;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Ribbon
{
    public class ImportBedlevelFromFileCommand : SpatialOperationCommandBase
    {
       private string FileFilter
        {
            get
            {
                string fileFilter = "";
                fileFilter += "All supported raster formats|*.asc;*.bil;*.tif;*.tiff;*.map";
                fileFilter += "|" + "Arc/Info ASCII Grid (*.asc)|*.asc";
                fileFilter += "|" + "ESRI .hdr Labelled (*.bil)|*.bil";
                fileFilter += "|" + "TIF Tagget Image File Format (*.tif)|*.tif;*.tiff";
                fileFilter += "|" + "PCRaster raster file format (*.map)|*.map"; ;
                return fileFilter;
            }
        }

        protected override bool FeatureTypeIsSupported(Type type)
        {
            return base.FeatureTypeIsSupported(type) &&
                (typeof(IPointValue).IsAssignableFrom(type) ||
                (typeof(ICoverage).IsAssignableFrom(type) &&
                    !typeof(INetworkCoverage).IsAssignableFrom(type) &&
                    !typeof(IFeatureCoverage).IsAssignableFrom(type)));
        }

        protected override bool IsEnabledForLayer(ILayer layer)
        {
            return layer != null && FeatureTypeIsSupported(GetFeatureTypeForLayer(layer));
        }

        protected override string GetOperationPrefix()
        {
            return "Import Samples";
        }

        protected override ISpatialOperation CreateSpatialOperation(ILayer targetLayer)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = FileFilter,
                CheckFileExists = true,
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK) return null;

            var fileName = openFileDialog.FileName;

            var coordinateSystem = SourceLayer.CoordinateSystem;

            var coordinateSystemFactory = SharpMap.Map.CoordinateSystemFactory;

            var coordinateSystemDialog = new CoordinateConversionDialog(coordinateSystem, coordinateSystem,
                coordinateSystemFactory.SupportedCoordinateSystems,
                coordinateSystemFactory.CreateTransformation);

            if (coordinateSystemDialog.ShowDialog() != DialogResult.OK) return null;

            return new ImportRasterSamplesOperationImportData 
            {
                FilePath = fileName,
                SourceCoordinateSystem = coordinateSystemDialog.FromCS,
                TargetCoordinateSystem = coordinateSystemDialog.ToCS
            };
        }
    }
}
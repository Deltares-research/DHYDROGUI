using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters
{
    public class PointFileImporterExporter : Feature2DImportExportBase<Feature2DPoint>
    {
        public override string Category => "Feature geometries";

        public override string Description => string.Empty;

        public override Bitmap Image => Resources.Observation;

        public override string FileFilter => $"2D points file|*{FileConstants.XynFileExtension}";
        protected override string ImporterName => $"Points from {FileConstants.XynFileExtension} file";

        protected override string ExporterName => $"Points to {FileConstants.XynFileExtension} file";

        protected override IEnumerable<Feature2DPoint> Import(string path)
        {
            var reader = new ObsFile<Feature2DPoint>();
            return reader.Read(path);
        }

        protected override void Export(IEnumerable<Feature2DPoint> features, string path)
        {
            var writer = new ObsFile<Feature2DPoint>();
            writer.Write(path, features);
        }
    }
}
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro.GroupableFeatures;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters
{
    public class PolFileImporterExporter : Feature2DImportExportBase<GroupableFeature2DPolygon>
    {
        public override string Category => "Feature geometries";

        public override string Description => string.Empty;

        public override string FileFilter => $"Feature polygon file|*{FileConstants.PolylineFileExtension}";

        public override Bitmap Image => Resources.TextDocument;
        protected override string ExporterName => $"Features to {FileConstants.PolylineFileExtension} file";

        protected override string ImporterName => $"Features from {FileConstants.PolylineFileExtension} file";

        protected override IEnumerable<GroupableFeature2DPolygon> Import(string path)
        {
            var reader = new PolFile<GroupableFeature2DPolygon>();
            return reader.Read(path);
        }

        protected override void Export(IEnumerable<GroupableFeature2DPolygon> features, string path)
        {
            var writer = new PolFile<GroupableFeature2DPolygon>();
            writer.Write(path, features);
        }
    }
}
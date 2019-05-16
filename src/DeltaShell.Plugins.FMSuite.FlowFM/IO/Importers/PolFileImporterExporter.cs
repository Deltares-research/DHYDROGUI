using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class PolFileImporterExporter : Feature2DImportExportBase<GroupableFeature2DPolygon>
    {
        protected override string ExporterName => "Features to .pol file";

        protected override string ImporterName => "Features from .pol file";

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

        public override string Category => "Feature geometries";

        public override string Description => string.Empty;

        public override string FileFilter => "Feature polygon file|*.pol";

        public override Bitmap Image => Resources.TextDocument;
    }
}
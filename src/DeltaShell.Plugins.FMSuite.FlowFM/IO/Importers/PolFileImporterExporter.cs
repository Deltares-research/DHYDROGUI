using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class PolFileImporterExporter: Feature2DImportExportBase<Feature2DPolygon>
    {
        protected override string ExporterName
        {
            get { return "Features to .pol file"; }
        }

        protected override string ImporterName
        {
            get { return "Features from .pol file"; }
        }

        protected override IEnumerable<Feature2DPolygon> Import(string path)
        {
            var reader = new PolFile();
            return reader.Read(path);
        }

        protected override void Export(IEnumerable<Feature2DPolygon> features, string path)
        {
            var writer = new PolFile();
            writer.Write(path, features);
        }

        public override string Category
        {
            get { return "Feature geometries"; }
        }

        public override string FileFilter
        {
            get { return "Feature polygon file|*.pol"; }
        }

        public override Bitmap Image
        {
            get { return Properties.Resources.TextDocument; }
        }
    }
}

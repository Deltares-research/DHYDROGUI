using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.Common.IO;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class PolFileImporterExporter: Feature2DImportExportBase<GroupableFeature2DPolygon>
    {
        protected override string ExporterName
        {
            get { return "Features to .pol file"; }
        }

        protected override string ImporterName
        {
            get { return "Features from .pol file"; }
        }

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

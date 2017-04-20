using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class LdbFileImporterExporter : Feature2DImportExportBase<LandBoundary2D>
    {
        protected override IEnumerable<LandBoundary2D> Import(string path)
        {
            var reader = new LdbFile();
            return reader.Read(path);
        }

        protected override void Export(IEnumerable<LandBoundary2D> features, string path)
        {
            var writer = new LdbFile();
            writer.Write(path, features);
        }

        public override string Category
        {
            get { return "Feature geometries"; }
        }

        public override Bitmap Image
        {
            get { return Properties.Resources.TextDocument; }
        }

        public override string FileFilter
        {
            get { return "Land boundary files|*.ldb"; }
        }

        protected override string ImporterName
        {
            get { return "Land boundaries from .ldb file"; }
        }

        protected override string ExporterName
        {
            get { return "Land boundaries to .ldb file"; }
        }
    }
}

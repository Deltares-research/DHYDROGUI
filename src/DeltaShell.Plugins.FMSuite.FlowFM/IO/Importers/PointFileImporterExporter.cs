using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class PointFileImporterExporter : Feature2DImportExportBase<Feature2DPoint>
    {
        protected override string ImporterName
        {
            get { return "Points from .xyn file"; }
        }

        protected override string ExporterName
        {
            get { return "Points to .xyn file"; }
        }

        protected override IEnumerable<Feature2DPoint> Import(string path)
        {
            var reader = new ObsFile();
            return reader.Read(path);
        }

        protected override void Export(IEnumerable<Feature2DPoint> features, string path)
        {
            var writer = new ObsFile();
            writer.Write(path, features);
        }

        public override string Category
        {
            get { return "Feature geometries"; }
        }

        public override Bitmap Image
        {
            get { return Resources.Observation; }
        }

        public override string FileFilter
        {
            get { return "2D points file|*.xyn"; }
        }
    }
}

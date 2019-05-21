using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportersExporters
{
    public class ObsFileImporterExporter<T> : Feature2DImportExportBase<T> where T : Feature2DPoint, new()
    {
        public override string Category => "Observation points";

        public override string Description => string.Empty;

        public override string FileFilter => "Observation points|*.xyn";

        public override Bitmap Image => Resources.Observation;

        protected override string ExporterName => "Observation points to .xyn file";

        protected override string ImporterName => "Observation points from .xyn file";

        protected override IEnumerable<T> Import(string path)
        {
            var obsFile = new ObsFile<T>();
            return obsFile.Read(path);
        }

        protected override void Export(IEnumerable<T> features, string path)
        {
            var obsFile = new ObsFile<T>();
            obsFile.Write(path, features);
        }
    }
}
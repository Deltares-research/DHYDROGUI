using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class ObsFileImporterExporter<T> : Feature2DImportExportBase<T> where T : Feature2DPoint, new()
    {
        public override string Category
        {
            get { return "Observation points"; }
        }

        public override string FileFilter
        {
            get { return "Observation points|*.xyn"; }
        }

        public override Bitmap Image
        {
            get
            {
                return Resources.Observation;
            }
        }

        protected override string ExporterName
        {
            get { return "Observation points to .xyn file"; }
        }
        protected override string ImporterName
        {
            get { return "Observation points from .xyn file"; }
        }
        protected override IEnumerable<T> Import(string path)
        {
            var obsFile = new Feature2DPointFile<T>();
            return obsFile.Read(path);
        }

        protected override void Export(IEnumerable<T> features, string path)
        {
            var obsFile = new Feature2DPointFile<T>();
            obsFile.Write(path, features);
        }
    }
}
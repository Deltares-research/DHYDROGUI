using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters
{
    public class LdbFileImporterExporter : Feature2DImportExportBase<LandBoundary2D>
    {
        protected override IEnumerable<LandBoundary2D> Import(string path)
        {
            var reader = new LdbFile();
            IList<LandBoundary2D> landBoundaries = reader.Read(path);
            landBoundaries.ForEach(lb => FeatureGroupExtensions.TrySetGroupName(lb, path));
            return landBoundaries;
        }

        protected override void Export(IEnumerable<LandBoundary2D> features, string path)
        {
            var writer = new LdbFile();
            writer.Write(path, features);
        }

        public override string Category => "Feature geometries";

        public override string Description => string.Empty;

        public override Bitmap Image => Resources.TextDocument;

        public override string FileFilter => "Land boundary files|*.ldb";

        protected override string ImporterName => "Land boundaries from .ldb file";

        protected override string ExporterName => "Land boundaries to .ldb file";
    }
}
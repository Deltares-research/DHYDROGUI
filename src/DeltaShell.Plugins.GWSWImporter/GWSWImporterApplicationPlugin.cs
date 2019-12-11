using System.Collections.Generic;
using DelftTools.Shell.Core;
using Mono.Addins;

namespace DeltaShell.Plugins.ImportExport.Gwsw
{
    [Extension(typeof(IPlugin))]
    public class GWSWImporterApplicationPlugin : ApplicationPlugin
    {
        public override string Name
        {
            get { return "GWSWImporterApplicationPlugin"; }
        }

        public override string DisplayName
        {
            get { return "GWSW Importer Application Plugin"; }
        }
        public override string Description { get; }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "1.1.0.0"; }
        }

        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new GwswFileImporter();
        }
    }
}
using System.Collections.Generic;
using System.Reflection;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.Fews.Export;
using Mono.Addins;

namespace DeltaShell.Plugins.Fews
{
    [Extension(typeof(IPlugin))]
    public class FewsApplicationPlugin : ApplicationPlugin
    {

        public override string Name
        {
            get { return "DelftFEWS"; }
        }

        public override string DisplayName
        {
            get { return "FEWS Plugin"; }
        }

        public override string Description
        {
            get { return "Integration with DelftFEWS, adapter, import and export of FEWS-PI."; }
        }

        public override string Version
        {
            get { return GetType().Assembly.GetName().Version.ToString(); }
        }

        public override string FileFormatVersion
        {
            get { return "3.5.0.0"; }
        }
        
        public override IEnumerable<IFileImporter> GetFileImporters()
        {
            yield return new PiTimeSeriesImporter();
            yield return new PiTimeSeriesLateralSourceImporter();
            yield return new PiTimeSeriesTargetItemImporter();
        }

        public override IEnumerable<IFileExporter> GetFileExporters()
        {
            yield return new ShapeFileExporter();
            yield return new ModelExchangeItemExporter(base.Application);
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield break;
        }
    }
}

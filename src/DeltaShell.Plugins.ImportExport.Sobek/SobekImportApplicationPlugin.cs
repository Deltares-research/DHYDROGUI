using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.IO.Helpers;
using Mono.Addins;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    /// <summary>
    /// Plugin that provides functionality to import from sobek legacy format
    /// </summary>
    [Extension(typeof(IPlugin))]
    public class SobekImportApplicationPlugin : ApplicationPlugin
    {
        private Image image;

        static SobekImportApplicationPlugin()
        {
            Sobek2ModelImporters.RegisterSobek2Importer(new SobekModelToRainfallRunoffModelImporter());
        }
        public override string Name
        {
            get { return "Sobek Network import"; }
        }

        public override string DisplayName
        {
            get { return "SOBEK Network Import Plugin"; }
        }

        public override string Description
        {
            get { return "Plugin that provides functionality to import from the SOBEK2 Network format."; }
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
            yield return new SobekHydroModelImporter();
            yield return new SobekModelToWaterFlowFMImporter();
            yield return new SobekNetworkImporter();
            yield return new SobekNetworkToNetworkImporter();
            yield return new SobekModelToRainfallRunoffModelImporter();
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return GetType().Assembly;
        }
    }
}
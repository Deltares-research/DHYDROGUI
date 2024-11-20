using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekNetworkImporter : IFileImporter, IPartialSobekImporter
    {
        private object targetObject;
        private bool targetItemHasBeenSet;
        
        public SobekNetworkImporter()
        {
            targetItemHasBeenSet = false;
        }

        # region IFileImporter

        public string Name
        {
            get { return "SOBEK Network"; }
        }
        public string Description { get { return Name; } }
        public string Category
        {
            get { return ProductCategories.OneDTwoDDataImportCategory; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.sobek; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(HydroNetwork); }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public virtual bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public string FileFilter
        {
            get { return "All Supported Files|network.tp;deftop.1|Sobek 2.1* network files|network.tp|SobekRE network files|deftop.1"; }
        }

        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target = null)
        {
            // Configure the SobekPath of the IPartialSobekImporter part of the importer
            if (!string.IsNullOrEmpty(path))
            {
                PathSobek = Path.GetFullPath(path.Trim());
            }

            // Configure the TargetObject of the IPartialSobekImporter part of the importer
            var targetObjectInternal = target ?? TargetObject;

            if (ShouldCancel)
            {
                return null;
            }

            // Import by using the import logic of the IPartialSobekImporter part of the importer
            Import();

            if (ShouldCancel)
            {
                return null;
            }

            var hydroRegion = targetObjectInternal as HydroRegion;
            if (hydroRegion != null)
            {
                var network = (HydroNetwork) hydroRegion.SubRegions[0];
                if (network != null) network.MakeNamesUnique<ICompositeBranchStructure>();

                var basin = (IDrainageBasin) hydroRegion.SubRegions[1];

                // skip basin if it is empty
                if (!basin.AllHydroObjects.Any())
                {
                    return network;
                }
            }
            
            targetObject = null;
            targetItemHasBeenSet = false;
            return hydroRegion;
        }

        # endregion

        # region IPartialSobekImporter

        public string PathSobek { get; set; }

        public string DisplayName
        {
            get { return null; }
        }

        SobekImporterCategories IPartialSobekImporter.Category { get; } = SobekImporterCategories.WaterFlow1D;

        public object TargetObject
        {
            get
            {
                return targetObject ?? (targetObject = CreateHydroRegion()); 
            }
            set
            {
                targetObject = value;
                targetItemHasBeenSet = true;
            }
        }

        public IPartialSobekImporter PartialSobekImporter
        {
            get { return PartialSobekImporterBuilder.BuildPartialSobekImporter(PathSobek, TargetObject); } 
            set { }
        }

        public void Import()
        {
            PartialSobekImporter.Import();
        }

        public bool IsActive { get; set; }

        public bool IsVisible { get; set; }

        public Action<IPartialSobekImporter> AfterImport { get; set; }

        public Action<IPartialSobekImporter> BeforeImport { get; set; }

        # endregion

        public bool OpenViewAfterImport { get { return false; } }

        private HydroRegion CreateHydroRegion()
        {
            return new HydroRegion { SubRegions = { new HydroNetwork(), new DrainageBasin() } };
        }
    }
}

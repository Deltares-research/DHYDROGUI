using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness
{
    public class RoughnessFromCsvToSectionImporter : IFileImporter
    {      
        private readonly RoughnessBranchDataCsvReader csvReader = new RoughnessBranchDataCsvReader();
        
        public string Name
        {
            get { return "Roughness (CSV)"; }
        }
        public string Description { get { return Name; } }

        public string Category { get; private set; }
        
        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(RoughnessSection);
                yield return typeof(IList<RoughnessSection>);
            }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public string FileFilter
        {
            get { return "CSV Files (*.csv)|*.csv"; }
        }
        
        public bool OpenViewAfterImport { get { return false; } }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target)
        {
            var csvData = csvReader.GetBranchData(path);

            var roughnessSections = target is RoughnessSection
                                        ? new[] { (RoughnessSection) target }
                                        : (IList<RoughnessSection>) target;

            RoughnessBranchDataMerger.MergeIntoRoughnessSections(roughnessSections, csvData);

            return target;
        }
    }
}

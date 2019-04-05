using System;
using System.Collections.Generic;
using System.Drawing;

using DelftTools.Shell.Core;

using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class DataTableImporter : IFileImporter
    {
        public string Name { get { return "Data table importer"; } }

        public string Category { get { return "WAQ data tables"; } }
        public string Description
        {
            get { return string.Empty; }
        }

        public Bitmap Image {  get { return null; } }

        public IEnumerable<Type> SupportedItemTypes { get { yield return typeof(DataTableManager); } }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel { get { return false; } }

        public string FileFilter { get { return "WAQ data table (*.csv)|*.csv"; } }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get { return false; } }

        public object ImportItem(string path, object target = null)
        {
            var targetManager = target as DataTableManager;
            if (targetManager == null)
            {
                throw new NotSupportedException("Target of import must be an instance of DataTableManager.");
            }


            var readDataTableData = DataTableCsvFileReader.Read(path, WaqFileBasedPreProcessor.GetDataTableUseforsRelativeFolderPath(targetManager));

            targetManager.CreateNewDataTable(readDataTableData.Name, 
                readDataTableData.CreateDataTableDelwaqFormat(), 
                readDataTableData.GetSubstanceUseforFileName(), 
                readDataTableData.CreateDefaultSubstanceUseforContents());

            return targetManager;
        }
    }
}
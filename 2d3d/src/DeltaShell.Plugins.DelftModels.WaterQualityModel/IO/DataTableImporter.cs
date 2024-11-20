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
        public string FilePath { get; set; }
        public virtual string Name => "Data table importer";

        public string Category => "WAQ data tables";

        public string Description => string.Empty;

        public Bitmap Image => null;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(DataTableManager);
            }
        }

        public bool CanImportOnRootLevel => false;

        public string FileFilter => "WAQ data table (*.csv)|*.csv";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport => false;

        public virtual bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            var targetManager = target as DataTableManager;

            if (targetManager == null)
            {
                throw new NotSupportedException("Target of import must be an instance of DataTableManager.");
            }

            if (path == null)
            {
                path = FilePath;
            }

            DataTableCsvContents readDataTableData =
                DataTableCsvFileReader.Read(
                    path, WaqFileBasedPreProcessor.GetDataTableUseforsRelativeFolderPath(targetManager));

            targetManager.CreateNewDataTable(readDataTableData.Name,
                                             readDataTableData.CreateDataTableDelwaqFormat(),
                                             readDataTableData.GetSubstanceUseforFileName(),
                                             readDataTableData.CreateDefaultSubstanceUseforContents());

            return targetManager;
        }
    }
}
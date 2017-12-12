using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class GwswFeatureImporter: GwswBaseImporter, IFileImporter
    {
        public string definitionFilePath { get; set; }

        #region IFileImporter

        public string Name
        {
            get { return "GWSW Feature File importer"; }
        }

        public string Category
        {
            get { return "1D / 2D"; }
        }

        public Bitmap Image
        {
            get { return Resources.StructureFeatureSmall; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IWaterFlowFMModel);
            }
        }

        public bool CanImportOnRootLevel { get { return false; } }
        public string FileFilter { get { return "GWSW Csv Files (*.csv)|*.csv"; } }
        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get { return false; } }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        #endregion

        public object ImportItem(string path, object target = null)
        {
            var fmModel = target as IWaterFlowFMModel;

            if ( String.IsNullOrEmpty(definitionFilePath) )
                return null;
            ImportGwswDefinitionFile(definitionFilePath);

            return ImportGwswFeatureFile(path, fmModel?.Network);
        }
    }
}
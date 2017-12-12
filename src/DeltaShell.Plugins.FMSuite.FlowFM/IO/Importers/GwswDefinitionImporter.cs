using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class GwswDefinitionImporter: GwswBaseImporter, IFileImporter
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswDefinitionImporter));

        #region IFileImporter

        public object ImportItem(string path, object target = null)
        {
            var fmModel = target as IWaterFlowFMModel;

            ImportGwswDefinitionFile(path);

            Log.InfoFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Attributes_mapped__0_, GwswAttributesDefinition.Count);
            Log.Info(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Importing_sub_files_);

            return ImportFilesFromDefinitionFile(path, fmModel?.Network);

        }

        public string Name
        {
            get { return "GWSW Definition and Features importer"; }
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

        #region Gwsw Definition Importer

        /// <summary>
        /// First imports a Gwsw Definition file and all the files mentioned on it. When the definition file is imported, 
        /// a mapping per file is created. Each of the mappings contains a list of attributes, their types, and their default values (when applicable).
        /// After the mappings are created, the importer will look for all the files mentioned on it and will try to import
        /// them, using said mappings, into Network Features.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        private List<INetworkFeature> ImportFilesFromDefinitionFile(string path, IHydroNetwork network)
        {
            var uniqueFileList = GwswAttributesDefinition.GroupBy(i => i.FileName).Select(grp => grp.Key).ToList();
            var importedFeatureElements = new List<INetworkFeature>();

            //Read each one of the files.
            foreach (var fileName in uniqueFileList)
            {
                var directoryName = Path.GetDirectoryName(path);
                var elementFilePath = Path.Combine(directoryName, fileName);
                if (!File.Exists(elementFilePath))
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Could_not_find_file__0__, elementFilePath);
                    continue;
                }

                //Call the base importer.
                var importedElement = ImportGwswFeatureFile(elementFilePath, network);
                var elementAsFeature = importedElement as List<INetworkFeature>;
                if (elementAsFeature == null)
                {
                    Log.ErrorFormat(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_File__0__was_not_imported_correctly_, fileName);
                    continue;
                }

                importedFeatureElements.AddRange(elementAsFeature);
            }

            return importedFeatureElements;
        }

        #endregion
    }
}
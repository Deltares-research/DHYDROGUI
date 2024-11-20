using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using log4net;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class WaterFlowFMDataAccessListener : IDataAccessListener
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowFMDataAccessListener));

        private static readonly IDictionary<string, string> updatedDataItemNames = new Dictionary<string, string>() {{"Bathymetry", WaterFlowFMModelDefinition.BathymetryDataItemName}};

        public void SetProjectRepository(IProjectRepository repository)
        {
        }

        public IDataAccessListener Clone()
        {
            return new WaterFlowFMDataAccessListener();
        }

        public void OnPreLoad(object entity, object[] loadedState, string[] propertyNames)
        {
        }

        public void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            var model = entity as WaterFlowFMModel;
            if (model != null)
            {
                UpdateDataItemNames(model);
                FixImportFilePaths(model);

                // Update bathymetry coverage based on specified value in .mdu file
                WaterFlowFMProperty bedLevelTypeProperty =
                    model.ModelDefinition.Properties.FirstOrDefault(
                        p => p.PropertyDefinition.MduPropertyName.ToLower() == KnownProperties.BedlevType);
                if (bedLevelTypeProperty != null)
                {
                    model.UpdateBathymetryCoverage(
                        (UnstructuredGridFileHelper.BedLevelLocation) bedLevelTypeProperty.Value);
                }
            }
        }

        public bool OnPreUpdate(object entity, object[] state, string[] propertyNames)
        {
            return false;
        }

        public bool OnPreInsert(object entity, object[] state, string[] propertyNames)
        {
            return false;
        }

        public void OnPostUpdate(object entity, object[] state, string[] propertyNames)
        {
        }

        public void OnPostInsert(object entity, object[] state, string[] propertyNames)
        {
        }

        public bool OnPreDelete(object entity, object[] deletedState, string[] propertyNames)
        {
            return false;
        }

        public void OnPostDelete(object entity, object[] deletedState, string[] propertyNames)
        {
        }

        private static void FixImportFilePaths(WaterFlowFMModel model)
        {
            // check if ImportSamplesOperations of model have valid paths otherwise try to correct using mdu file directory
            // this is needed in for backward compatibility (previously FilePath used to be relative to mdu path)
            IEnumerable<ImportSamplesOperation> importSamplesOperationsWithoutValidPath = 
                model.DataItems
                     .Select(di => di.ValueConverter)
                     .OfType<SpatialOperationSetValueConverter>()
                     .SelectMany(vc => vc.SpatialOperationSet.GetOperationsRecursive())
                     .OfType<ImportSamplesOperation>()
                     .Where(o => !File.Exists(o.FilePath));

            string mduDirectory = Path.GetDirectoryName(model.ExtFilePath);
            if (mduDirectory == null)
            {
                return;
            }

            foreach (ImportSamplesOperation importSampleOperation in importSamplesOperationsWithoutValidPath)
            {
                string fileName = Path.GetFileName(importSampleOperation.FilePath);
                if (fileName == null)
                {
                    continue;
                }

                string newPath = Path.Combine(mduDirectory, fileName);
                if (File.Exists(newPath))
                {
                    importSampleOperation.FilePath = newPath;
                    Log.WarnFormat("Fixed path for operation : {0} for file : {1} to: {2}", importSampleOperation.Name,
                                   fileName, newPath);
                }
            }
        }

        private void UpdateDataItemNames(WaterFlowFMModel model)
        {
            foreach (IDataItem dataItem in model.AllDataItems)
            {
                string dataItemName = dataItem.Name;
                if (updatedDataItemNames.ContainsKey(dataItemName))
                {
                    dataItem.Name = updatedDataItemNames[dataItemName];
                }
            }
        }
    }
}
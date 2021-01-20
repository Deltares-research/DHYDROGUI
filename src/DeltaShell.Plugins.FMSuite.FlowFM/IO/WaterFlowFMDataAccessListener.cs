using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class WaterFlowFMDataAccessListener : DataAccessListenerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowFMDataAccessListener));

        private static readonly IDictionary<string, string> updatedDataItemNames = new Dictionary<string, string>() {{"Bathymetry", WaterFlowFMModelDefinition.BathymetryDataItemName}};

        public override object Clone()
        {
            return new WaterFlowFMDataAccessListener();
        }

        public override void OnPostLoad(object entity, object[] state, string[] propertyNames)
        {
            base.OnPostLoad(entity, state, propertyNames);

            var model = entity as WaterFlowFMModel;
            if (model != null)
            {
                UpdateDataItemNames(model);
                FixImportFilePaths(model);
                LoadSpatialData(model);

                // BedLevel dataitem value used to be exclusively UnstructuredGridVertexCoverages, now it needs to be more generic
                IDataItem bedLevelDataItem =
                    GetDataItemByName(model.DataItems, WaterFlowFMModelDefinition.BathymetryDataItemName);

                if (bedLevelDataItem != null)
                {
                    bedLevelDataItem.ValueType = typeof(UnstructuredGridCoverage);
                }

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

        private static void FixImportFilePaths(WaterFlowFMModel model)
        {
            // check if ImportSamplesOperations of model have valid paths otherwise try to correct using mdu file directory
            // this is needed in for backward compatibility (previously FilePath used to be relative to mdu path)
            IEnumerable<ImportSamplesOperation> importSamplesOperationsWithoutValidPath = model
                                                                                          .DataItems.Select(
                                                                                              di => di.ValueConverter)
                                                                                          .OfType<
                                                                                              SpatialOperationSetValueConverter
                                                                                          >()
                                                                                          .SelectMany(
                                                                                              vc => vc
                                                                                                    .SpatialOperationSet
                                                                                                    .GetOperationsRecursive())
                                                                                          .OfType<ImportSamplesOperation
                                                                                          >()
                                                                                          .Where(o => !File.Exists(
                                                                                                          o.FilePath));

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

        private static void SynchronizeGrid(WaterFlowFMModel model, string name)
        {
            IDataItem dataItem = model.DataItems.GetByName(name);
            if (!(dataItem.ValueConverter is SpatialOperationSetValueConverter spatialOperationSetValueConverter))
            {
                return;
            }

            ((UnstructuredGridCoverage) spatialOperationSetValueConverter.OriginalValue).Grid = model.Grid;
            spatialOperationSetValueConverter.SpatialOperationSet.SetDirty();
        }

        private static void LoadSpatialData(WaterFlowFMModel waterFlowFMModel)
        {
            // we do not want to import the spatial operations since the converted (z-) values are read from the net file
            ClearSpatialOperations(waterFlowFMModel, WaterFlowFMModelDefinition.BathymetryDataItemName);

            SynchronizeGrid(waterFlowFMModel, WaterFlowFMModelDefinition.BathymetryDataItemName);
            SynchronizeGrid(waterFlowFMModel, WaterFlowFMModelDefinition.RoughnessDataItemName);
            SynchronizeGrid(waterFlowFMModel, WaterFlowFMModelDefinition.InitialWaterLevelDataItemName);
            SynchronizeGrid(waterFlowFMModel, WaterFlowFMModelDefinition.ViscosityDataItemName);
            SynchronizeGrid(waterFlowFMModel, WaterFlowFMModelDefinition.DiffusivityDataItemName);
            SynchronizeGrid(waterFlowFMModel, WaterFlowFMModelDefinition.InitialTemperatureDataItemName);
            SynchronizeGrid(waterFlowFMModel, WaterFlowFMModelDefinition.InitialSalinityDataItemName);

            foreach (UnstructuredGridCellCoverage tracer in waterFlowFMModel.InitialTracers)
            {
                SynchronizeGrid(waterFlowFMModel, tracer.Name);
            }

            foreach (UnstructuredGridCellCoverage fraction in waterFlowFMModel.InitialFractions)
            {
                SynchronizeGrid(waterFlowFMModel, fraction.Name);
            }

            // update intermediate results in operation stack after loading project:
            ExecuteOperations(waterFlowFMModel.DataItems.GetByName(WaterFlowFMModelDefinition.RoughnessDataItemName));
            ExecuteOperations(waterFlowFMModel.DataItems.GetByName(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
            ExecuteOperations(waterFlowFMModel.DataItems.GetByName(WaterFlowFMModelDefinition.ViscosityDataItemName));
            ExecuteOperations(waterFlowFMModel.DataItems.GetByName(WaterFlowFMModelDefinition.DiffusivityDataItemName));
            ExecuteOperations(waterFlowFMModel.DataItems.GetByName(WaterFlowFMModelDefinition.InitialTemperatureDataItemName));
            ExecuteOperations(waterFlowFMModel.DataItems.GetByName(WaterFlowFMModelDefinition.InitialSalinityDataItemName));

            foreach (UnstructuredGridCellCoverage tracer in waterFlowFMModel.InitialTracers)
            {
                ExecuteOperations(waterFlowFMModel.GetDataItemByValue(tracer));
            }

            foreach (UnstructuredGridCellCoverage fraction in waterFlowFMModel.InitialFractions)
            {
                ExecuteOperations(waterFlowFMModel.GetDataItemByValue(fraction));
            }
        }

        /// <summary>
        /// Removes the spatial operations from a spatial data data item.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="dataItemName">Name of the data item.</param>
        private static void ClearSpatialOperations(WaterFlowFMModel model, string dataItemName)
        {
            IDataItem bedLevelDataItem = GetDataItemByName(model.DataItems, dataItemName);

            if (bedLevelDataItem != null)
            {
                bedLevelDataItem.ValueConverter = null;
            }
        }

        private static void ExecuteOperations(IDataItem dataItem)
        {
            if (dataItem == null)
            {
                return;
            }

            var sosvc = dataItem.ValueConverter as CoverageSpatialOperationValueConverter;
            if (sosvc == null || !sosvc.SpatialOperationSet.Dirty)
            {
                return;
            }

            // Enable event bubbling to trigger the copying of last values to ConvertedValue (SpatialOperationSetValueConverter.CopyValuesToConvertedValue)
            //(this will not be triggered during loading if EventSettings.BubblingEnabled is false)
            bool eventBubblingEnabled = EventSettings.BubblingEnabled;
            try
            {
                EventSettings.BubblingEnabled = true;
                sosvc.SpatialOperationSet.Execute();
            }
            finally
            {
                EventSettings.BubblingEnabled = eventBubblingEnabled;
            }
        }

        private static IDataItem GetDataItemByName(IEnumerable<IDataItem> dataItems, string dataItemName)
        {
            return dataItems.FirstOrDefault(di => di.Name == dataItemName);
        }
    }
}
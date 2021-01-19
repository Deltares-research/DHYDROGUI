using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Aop;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
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

        private static bool SynchronizeDataItemValue(WaterFlowFMModel model, string name, object value)
        {
            IDataItem dataItem = GetDataItemByName(model.DataItems, name);

            if (dataItem == null)
            {
                return false;
            }

            if (dataItem.ValueConverter is SpatialOperationSetValueConverter spatialOperationSetValueConverter)
            {
                var coverage = (ICoverage) spatialOperationSetValueConverter.OriginalValue;
                List<double> originalValues = coverage.Components[0].GetValues<double>().ToList();

                spatialOperationSetValueConverter.ConvertedValue = value;

                ((ICoverage) spatialOperationSetValueConverter.OriginalValue).SetValues(originalValues);

                // only do this when there are values or you will get an ArgumentException.
                // This happens when only a point cloud is loaded and there was no grid (TOOLS-21425)
                if (originalValues.Any(v => !Equals(v, coverage.Components[0].NoDataValue)) && model.Grid.FlowLinks.Count == originalValues.Count)
                {
                    coverage.SetValues(originalValues);
                }

                spatialOperationSetValueConverter.SpatialOperationSet.SetDirty();
            }
            else
            {
                dataItem.Value = value;
            }

            return true;
        }

        private static bool SynchronizeDataItemValues(WaterFlowFMModel model, string baseName,
                                                      CoverageDepthLayersList coverageDepthLayersList)
        {
            if (coverageDepthLayersList.Coverages.Count == 1)
            {
                return SynchronizeDataItemValue(model, baseName, coverageDepthLayersList.Coverages.First());
            }

            return !coverageDepthLayersList.Coverages.Where((t, i) => !SynchronizeDataItemValue(model, baseName + "_" + (i + 1), t)).Any();
        }

        private static void LoadSpatialData(WaterFlowFMModel waterFlowFMModel)
        {
            // we do not want to import the spatial operations since the converted (z-) values are read from the net file
            ClearSpatialOperations(waterFlowFMModel, WaterFlowFMModelDefinition.BathymetryDataItemName);

            SynchronizeDataItemValue(waterFlowFMModel, WaterFlowFMModelDefinition.BathymetryDataItemName,
                                     waterFlowFMModel.Bathymetry);
            SynchronizeDataItemValue(waterFlowFMModel, WaterFlowFMModelDefinition.RoughnessDataItemName,
                                     waterFlowFMModel.Roughness);
            SynchronizeDataItemValue(waterFlowFMModel, WaterFlowFMModelDefinition.InitialWaterLevelDataItemName,
                                     waterFlowFMModel.InitialWaterLevel);
            SynchronizeDataItemValue(waterFlowFMModel, WaterFlowFMModelDefinition.ViscosityDataItemName,
                                     waterFlowFMModel.Viscosity);
            SynchronizeDataItemValue(waterFlowFMModel, WaterFlowFMModelDefinition.DiffusivityDataItemName,
                                     waterFlowFMModel.Diffusivity);
            SynchronizeDataItemValue(waterFlowFMModel, WaterFlowFMModelDefinition.InitialTemperatureDataItemName,
                                     waterFlowFMModel.InitialTemperature);
            SynchronizeDataItemValues(waterFlowFMModel, WaterFlowFMModelDefinition.InitialSalinityDataItemName,
                                      waterFlowFMModel.InitialSalinity);

            foreach (UnstructuredGridCellCoverage tracer in waterFlowFMModel.InitialTracers)
            {
                SynchronizeDataItemValue(waterFlowFMModel, tracer.Name, tracer);
            }

            foreach (UnstructuredGridCellCoverage fraction in waterFlowFMModel.InitialFractions)
            {
                SynchronizeDataItemValue(waterFlowFMModel, fraction.Name, fraction);
            }

            waterFlowFMModel.ImportSpatialOperationsAfterLoading();

            // update intermediate results in operation stack after loading project:
            ExecuteOperations(waterFlowFMModel, waterFlowFMModel.Roughness);
            ExecuteOperations(waterFlowFMModel, waterFlowFMModel.InitialWaterLevel);
            ExecuteOperations(waterFlowFMModel, waterFlowFMModel.Viscosity);
            ExecuteOperations(waterFlowFMModel, waterFlowFMModel.Diffusivity);
            ExecuteOperations(waterFlowFMModel, waterFlowFMModel.InitialTemperature);
            foreach (ICoverage cov in waterFlowFMModel.InitialSalinity.Coverages)
            {
                ExecuteOperations(waterFlowFMModel, cov);
            }

            foreach (UnstructuredGridCellCoverage tracer in waterFlowFMModel.InitialTracers)
            {
                ExecuteOperations(waterFlowFMModel, tracer);
            }

            foreach (UnstructuredGridCellCoverage fraction in waterFlowFMModel.InitialFractions)
            {
                ExecuteOperations(waterFlowFMModel, fraction);
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

        private static void ExecuteOperations(IModel model, ICoverage coverage)
        {
            IDataItem di = model.GetDataItemByValue(coverage);
            if (di == null)
            {
                return;
            }

            var sosvc = di.ValueConverter as CoverageSpatialOperationValueConverter;
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
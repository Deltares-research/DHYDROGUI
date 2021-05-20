using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Spatial;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class WaterFlowFMDataAccessListener : DataAccessListenerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowFMDataAccessListener));

        private static readonly IDictionary<string, string> updatedDataItemNames = new Dictionary<string, string>()
            {
                {"Bathymetry", WaterFlowFMModelDefinition.BathymetryDataItemName}
            };

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
                if (ProjectRepository.IsLegacyProject(ProjectRepository.Path))
                {
                    model.ClearOutput();
                }

                // BedLevel dataitem value used to be exclusively UnstructuredGridVertexCoverages, now it needs to be more generic
                var bedLevelDataItem = model.DataItems.FirstOrDefault(di => di.Name == WaterFlowFMModelDefinition.BathymetryDataItemName);
                if (bedLevelDataItem != null) bedLevelDataItem.ValueType = typeof(UnstructuredGridCoverage);

                // Update bathymetry coverage based on specified value in .mdu file
                var bedLevelTypeProperty = model.ModelDefinition.Properties.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName.ToLower() == KnownProperties.BedlevType);
                if(bedLevelTypeProperty != null)
                    model.UpdateBathymetryCoverage((UGridFileHelper.BedLevelLocation) bedLevelTypeProperty.Value);
            }
        }

        private static void FixImportFilePaths(WaterFlowFMModel model)
        {
            // check if ImportSamplesOperations of model have valid paths otherwise try to correct using mdu file directory
            // this is needed in for backward compatibility (previously FilePath used to be relative to mdu path)
            var importSamplesOperationsWithoutValidPath = model.DataItems.Select(di => di.ValueConverter)
                .OfType<SpatialOperationSetValueConverter>()
                .SelectMany(vc => vc.SpatialOperationSet.GetOperationsRecursive())
                .OfType<ImportSamplesOperation>()
                .Where(o => !File.Exists(o.FilePath));

            var mduDirectory = Path.GetDirectoryName(model.ExtFilePath);
            if (mduDirectory == null) return;

            foreach (var importSampleOperation in importSamplesOperationsWithoutValidPath)
            {
                var fileName = Path.GetFileName(importSampleOperation.FilePath);
                if (fileName == null) continue;

                var newPath = Path.Combine(mduDirectory, fileName);
                if (File.Exists(newPath))
                {
                    importSampleOperation.FilePath = newPath;
                    Log.WarnFormat("Fixed path for operation : {0} for file : {1} to: {2}", importSampleOperation.Name, fileName, newPath);
                }
            }
        }

        private void UpdateDataItemNames(WaterFlowFMModel model)
        {
            foreach (var dataItem in model.AllDataItems)
            {
                var dataItemName = dataItem.Name;
                if (updatedDataItemNames.ContainsKey(dataItemName))
                {
                    dataItem.Name = updatedDataItemNames[dataItemName];
                }
            }
        }

        private static bool SynchronizeDataItemValue(WaterFlowFMModel model, string name, object value)
        {
            var dataItem = model.DataItems.FirstOrDefault(di => di.Name == name);

            if (dataItem == null) return false;

            var spatialOperationSetValueConverter = dataItem.ValueConverter as SpatialOperationSetValueConverter;
            if (spatialOperationSetValueConverter != null)
            {
                SpatialOperationHelper.MakeNamesUniquePerSet(spatialOperationSetValueConverter.SpatialOperationSet);
                
                var coverage = (ICoverage) spatialOperationSetValueConverter.OriginalValue;
                var originalValues =
                    coverage.Components[0].GetValues<double>();

                spatialOperationSetValueConverter.ConvertedValue = value;
                coverage.Clear();

                // only do this when there are values or you will get an ArgumentException.
                // This happens when only a point cloud is loaded and there was no grid (TOOLS-21425)
                if (originalValues.Any(v => !Equals(v, coverage.Components[0].NoDataValue)) && model.Grid.FlowLinks.Count == originalValues.Count)
                {
                    // TODO: remove this check, whenever we fix the flow link loading definitively...
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
            return
                !coverageDepthLayersList.Coverages.Where(
                    (t, i) => !SynchronizeDataItemValue(model, baseName + "_" + (i + 1), t)).Any();
        }

        private static void LoadSpatialData(WaterFlowFMModel waterFlowFMModel)
        {
            SynchronizeDataItemValue(waterFlowFMModel, WaterFlowFMModelDefinition.BathymetryDataItemName, waterFlowFMModel.Bathymetry);
            SynchronizeDataItemValue(waterFlowFMModel, WaterFlowFMModelDefinition.RoughnessDataItemName, waterFlowFMModel.Roughness);
            var initialWaterQuantityNameType = (InitialConditionQuantity)(int)waterFlowFMModel.ModelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D).Value;
            waterFlowFMModel.InitialWaterLevel.Name =
                initialWaterQuantityNameType == InitialConditionQuantity.WaterLevel
                    ? WaterFlowFMModelDefinition.InitialWaterLevelDataItemName
                    : WaterFlowFMModelDefinition.InitialWaterDepthDataItemName;
            SynchronizeDataItemValue(waterFlowFMModel, initialWaterQuantityNameType == InitialConditionQuantity.WaterLevel 
                ? WaterFlowFMModelDefinition.InitialWaterLevelDataItemName
                : WaterFlowFMModelDefinition.InitialWaterDepthDataItemName, waterFlowFMModel.InitialWaterLevel);
            SynchronizeDataItemValue(waterFlowFMModel, WaterFlowFMModelDefinition.ViscosityDataItemName, waterFlowFMModel.Viscosity);
            SynchronizeDataItemValue(waterFlowFMModel, WaterFlowFMModelDefinition.DiffusivityDataItemName, waterFlowFMModel.Diffusivity);
            SynchronizeDataItemValue(waterFlowFMModel, WaterFlowFMModelDefinition.InitialTemperatureDataItemName, waterFlowFMModel.InitialTemperature);
            SynchronizeDataItemValues(waterFlowFMModel, WaterFlowFMModelDefinition.InitialSalinityDataItemName, waterFlowFMModel.InitialSalinity);


            foreach (var tracer in waterFlowFMModel.InitialTracers)
            {
                SynchronizeDataItemValue(waterFlowFMModel, tracer.Name, tracer);
            }

            foreach (var fraction in waterFlowFMModel.InitialFractions)
            {
                SynchronizeDataItemValue(waterFlowFMModel, fraction.Name, fraction);
            }

            waterFlowFMModel.ImportSpatialOperationsAfterLoading();
            

            // update intermediate results in operation stack after loading project:
            ExecuteOperations(waterFlowFMModel, waterFlowFMModel.Bathymetry);
            ExecuteOperations(waterFlowFMModel, waterFlowFMModel.Roughness);
            ExecuteOperations(waterFlowFMModel, waterFlowFMModel.InitialWaterLevel);
            ExecuteOperations(waterFlowFMModel, waterFlowFMModel.Viscosity);
            ExecuteOperations(waterFlowFMModel, waterFlowFMModel.Diffusivity);
            ExecuteOperations(waterFlowFMModel, waterFlowFMModel.InitialTemperature);
            foreach (var cov in waterFlowFMModel.InitialSalinity.Coverages)
            {
                ExecuteOperations(waterFlowFMModel, cov);
            }

            foreach (var tracer in waterFlowFMModel.InitialTracers)
            {
                ExecuteOperations(waterFlowFMModel, tracer);
            }
            foreach (var fraction in waterFlowFMModel.InitialFractions)
            {
                ExecuteOperations(waterFlowFMModel, fraction);
            }
        }

        private static void ExecuteOperations(IModel model, ICoverage coverage)
        {
            var di = model.GetDataItemByValue(coverage);
            if (di == null) return;
            var sosvc = di.ValueConverter as CoverageSpatialOperationValueConverter;
            if (sosvc == null || !sosvc.SpatialOperationSet.Dirty) return;

            // Enable event bubbling to trigger the copying of last values to ConvertedValue (SpatialOperationSetValueConverter.CopyValuesToConvertedValue)
            //(this will not be triggered during loading if EventSettings.BubblingEnabled is false)
            var eventBubblingEnabled = EventSettings.BubblingEnabled;
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
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Filewriter for the initialFields.ini file.
    /// </summary>
    public static class InitialConditionInitialFieldsFileWriter
    {
        public static void WriteFile(string filename, WaterFlowFMModelDefinition modelDefinition, bool networkIsEmpty)
        {
            // [General]
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.InitialConditionDataMajorVersion,
                    GeneralRegion.InitialConditionDataMinorVersion,
                    GeneralRegion.FileTypeName.InitialFields),
            };
            if (!networkIsEmpty)
            {
                var globalInitialConditionQuantity1D = (InitialConditionQuantity) (int) modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D).Value;
                categories.Add(CreateInitialConditionQuantityCategory(globalInitialConditionQuantity1D));
            }
            categories.AddRange(CreateSpatialOperationQuantityCategory(filename,ExtForceQuantNames.FrictCoef, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName), true));
            var globalInitialConditionQuantity2D = (InitialConditionQuantity)(int)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D).Value;
            categories.AddRange(CreateSpatialOperationQuantityCategory(filename,globalInitialConditionQuantity2D == InitialConditionQuantity.WaterLevel 
                ? InitialFieldsFileConstants.WaterLevel 
                : InitialFieldsFileConstants.WaterDepth, modelDefinition.GetSpatialOperations(globalInitialConditionQuantity2D == InitialConditionQuantity.WaterLevel 
                ? WaterFlowFMModelDefinition.InitialWaterLevelDataItemName
                : WaterFlowFMModelDefinition.InitialWaterDepthDataItemName), false));

            if (File.Exists(filename)) File.Delete(filename);
            new IniFileWriter().WriteIniFile(categories, filename);
        }

        private static IEnumerable<DelftIniCategory> CreateSpatialOperationQuantityCategory(string filename, string quantity, IEnumerable<ISpatialOperation> spatialOperations, bool isParameter)
        {
            if (spatialOperations != null)
            {
                var forceFileItems = GetExtForceFileItems(filename, quantity, spatialOperations);

                foreach (var forceFileItem in forceFileItems)
                {
                    var category = new DelftIniCategory(isParameter
                        ? InitialConditionRegion.ParameterIniHeader
                        : InitialConditionRegion.InitialConditionIniHeader);
                    category.AddProperty(InitialConditionRegion.Quantity.Key, quantity);
                    category.AddProperty(InitialConditionRegion.DataFile.Key, forceFileItem.FileName);
                    category.AddProperty(InitialConditionRegion.DataFileType.Key, GetDataFileType(forceFileItem.FileType));
                    category.AddProperty(InitialConditionRegion.InterpolationMethod.Key, GetSpatialOperationMethod(forceFileItem.Method));
                    category.AddProperty(InitialConditionRegion.Operand.Key, forceFileItem.Operand);
                    category.AddProperty(InitialConditionRegion.LocationType.Key, "2d");
                    if (forceFileItem.Method == 6 && forceFileItem.ModelData != null)
                    {
                        if(forceFileItem.ModelData.TryGetValue(ExtForceFile.AveragingTypeKey, out var averagingType) && int.TryParse(averagingType.ToString(),out var averagingTypeInt))
                            category.AddProperty(InitialConditionRegion.AveragingType.Key, GetAveragingType(averagingTypeInt));
                        if (forceFileItem.ModelData.TryGetValue(ExtForceFile.RelSearchCellSizeKey, out var relSearchCellSize))
                            category.AddProperty(InitialConditionRegion.AveragingRelSize.Key, (double) relSearchCellSize, format: "G");
                        if (forceFileItem.ModelData.TryGetValue(ExtForceFile.MinSamplePointsKey, out var minSamplePoints))
                            category.AddProperty(InitialConditionRegion.AveragingNumMin.Key, minSamplePoints.ToString());
                    }

                    if (!Double.IsNaN(forceFileItem.Value) && forceFileItem.FileType == ExtForceQuantNames.FileTypes.InsidePolygon)
                    {
                        category.AddProperty(InitialConditionRegion.Value.Key, forceFileItem.Value);
                    }

                    yield return category;
                }
            }
        }

        private static IEnumerable<ExtForceFileItem> GetExtForceFileItems(string filename, string quantity, IEnumerable<ISpatialOperation> spatialOperations)
        {
            
            // if all ops are interpolations/set value within polygons, write them to the file
            foreach (var spatialOperation in spatialOperations)
            {
                var importSamplesOperation = spatialOperation as ImportSamplesSpatialOperationExtension;
                if (importSamplesOperation != null)
                {
                    yield return ExtForceFileHelper.WriteInitialConditionsSamples(filename, quantity,
                        importSamplesOperation, null, true);
                    continue;
                }

                var polygonOperation = spatialOperation as SetValueOperation;
                if (polygonOperation != null)
                {
                    yield return ExtForceFileHelper.WriteInitialConditionsPolygon(filename, quantity, 
                        polygonOperation, null, true);
                    continue;
                }

                var addSamplesOperation = spatialOperation as AddSamplesOperation;
                if (addSamplesOperation != null)
                {
                    yield return ExtForceFileHelper.WriteInitialConditionsUnsupported(filename, quantity, 
                        addSamplesOperation, true);
                    continue;
                }

                throw new NotImplementedException(
                    string.Format("Cannot serialize operation of type {0} to external forcings file",
                        spatialOperation.GetType()));
            }
        }

        private static string GetAveragingType(int averagingType)
        {
            switch (averagingType)
            {
                case (int)GridCellAveragingMethod.ClosestPoint: return "nearestNb";
                case (int)GridCellAveragingMethod.MaximumValue: return "max";
                case (int)GridCellAveragingMethod.MinimumValue: return "min";
                case (int)GridCellAveragingMethod.InverseWeightedDistance: return "invDist";
                case (int)GridCellAveragingMethod.MinAbs: return "minAbs";
            }
            return "mean";
        }

        private static string GetSpatialOperationMethod(int method)
        {
            switch (method)
            {
                case 5 : return "triangulation";
                case 6 : return "averaging";
            }

            return "constant";
        }

        private static string GetDataFileType(int fileType)
        {
            switch (fileType)
            {
                case ExtForceQuantNames.FileTypes.ArcInfo:
                    return "arcinfo";
                case ExtForceQuantNames.FileTypes.GeoTiff:
                    return "GeoTiff";
                case ExtForceQuantNames.FileTypes.Triangulation:
                case ExtForceQuantNames.FileTypes.TriangulationMagDir:
                    return "sample";
                case ExtForceQuantNames.FileTypes.InsidePolygon: 
                    return "polygon";
            }
            return string.Empty;
        }

        private static DelftIniCategory CreateInitialConditionQuantityCategory(
            InitialConditionQuantity globalInitialConditionQuantity)
        {
            var category = new DelftIniCategory(InitialConditionRegion.InitialConditionIniHeader);

            category.AddProperty(InitialConditionRegion.Quantity.Key, globalInitialConditionQuantity.ToString().ToLower());
            category.AddProperty(InitialConditionRegion.DataFile.Key, $"Initial{globalInitialConditionQuantity}.ini");
            category.AddProperty(InitialConditionRegion.DataFileType, "1dField");

            return category;
        }
    }
}

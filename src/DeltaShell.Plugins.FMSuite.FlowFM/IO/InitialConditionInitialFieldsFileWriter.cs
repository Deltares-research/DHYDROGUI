using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.Ini;
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
            var iniSections = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.InitialConditionDataMajorVersion,
                    GeneralRegion.InitialConditionDataMinorVersion,
                    GeneralRegion.FileTypeName.InitialFields),
            };
            if (!networkIsEmpty)
            {
                var globalInitialConditionQuantity1D = (InitialConditionQuantity) (int) modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D).Value;
                iniSections.Add(CreateInitialConditionQuantityIniSection(globalInitialConditionQuantity1D));
            }
            iniSections.AddRange(CreateSpatialOperationQuantityIniSection(filename,ExtForceQuantNames.FrictCoef, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName), true));
            iniSections.AddRange(CreateSpatialOperationQuantityIniSection(filename,InitialFieldsFile.Quantity.BedLevel, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.BathymetryDataItemName), false));
            iniSections.AddRange(CreateSpatialOperationQuantityIniSection(filename,InitialFieldsFile.Quantity.Infiltration, modelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.InfiltrationDataItemName), false));
            
            var globalInitialConditionQuantity2D = (InitialConditionQuantity)(int)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D).Value;
            iniSections.AddRange(CreateSpatialOperationQuantityIniSection(filename,globalInitialConditionQuantity2D == InitialConditionQuantity.WaterLevel 
                ? InitialFieldsFile.Quantity.WaterLevel 
                : InitialFieldsFile.Quantity.WaterDepth, modelDefinition.GetSpatialOperations(globalInitialConditionQuantity2D == InitialConditionQuantity.WaterLevel 
                                                                                                    ? WaterFlowFMModelDefinition.InitialWaterLevelDataItemName
                                                                                                    : WaterFlowFMModelDefinition.InitialWaterDepthDataItemName), false));

            if (File.Exists(filename)) File.Delete(filename);
            new IniFileWriter().WriteIniFile(iniSections, filename);
        }

        private static IEnumerable<IniSection> CreateSpatialOperationQuantityIniSection(string filename, string quantity, IEnumerable<ISpatialOperation> spatialOperations, bool isParameter)
        {
            if (spatialOperations != null)
            {
                var forceFileItems = GetExtForceFileItems(filename, quantity, spatialOperations);

                foreach (var forceFileItem in forceFileItems)
                {
                    var iniSection = new IniSection(isParameter
                        ? InitialConditionRegion.ParameterIniHeader
                        : InitialConditionRegion.InitialConditionIniHeader);
                    iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.Quantity.Key, quantity);
                    iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.DataFile.Key, forceFileItem.FileName);
                    iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.DataFileType.Key, GetDataFileType(forceFileItem.FileType));
                    iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.InterpolationMethod.Key, GetSpatialOperationMethod(forceFileItem.Method));
                    iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.Operand.Key, forceFileItem.Operand);
                    iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.LocationType.Key, "2d");
                    if (forceFileItem.Method == 6 && forceFileItem.ModelData != null)
                    {
                        if(forceFileItem.ModelData.TryGetValue(ExtForceFile.AveragingTypeKey, out var averagingType) && int.TryParse(averagingType.ToString(),out var averagingTypeInt))
                            iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.AveragingType.Key, GetAveragingType(averagingTypeInt));
                        if (forceFileItem.ModelData.TryGetValue(ExtForceFile.RelSearchCellSizeKey, out var relSearchCellSize))
                            iniSection.AddPropertyWithOptionalCommentAndFormat(InitialConditionRegion.AveragingRelSize.Key, (double) relSearchCellSize, format: "G");
                        if (forceFileItem.ModelData.TryGetValue(ExtForceFile.MinSamplePointsKey, out var minSamplePoints))
                            iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.AveragingNumMin.Key, minSamplePoints.ToString());
                    }

                    if (!Double.IsNaN(forceFileItem.Value) && forceFileItem.FileType == ExtForceQuantNames.FileTypes.InsidePolygon)
                    {
                        iniSection.AddPropertyWithOptionalCommentAndFormat(InitialConditionRegion.Value.Key, forceFileItem.Value);
                    }

                    yield return iniSection;
                }
            }
        }

        private static IEnumerable<ExtForceFileItem> GetExtForceFileItems(string filename, string quantity, IEnumerable<ISpatialOperation> spatialOperations)
        {
            
            // if all ops are interpolations/set value within polygons, write them to the file
            foreach (var spatialOperation in spatialOperations)
            {
                var importSamplesOperation = spatialOperation as ImportSamplesOperationImportData;
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
                    return InitialFieldsFile.DataType.ArcInfo;
                case ExtForceQuantNames.FileTypes.GeoTiff:
                    return InitialFieldsFile.DataType.GeoTiff;
                case ExtForceQuantNames.FileTypes.Triangulation:
                case ExtForceQuantNames.FileTypes.TriangulationMagDir:
                    return InitialFieldsFile.DataType.Sample;
                case ExtForceQuantNames.FileTypes.InsidePolygon: 
                    return InitialFieldsFile.DataType.Polygon;
            }
            return string.Empty;
        }

        private static IniSection CreateInitialConditionQuantityIniSection(
            InitialConditionQuantity globalInitialConditionQuantity)
        {
            var iniSection = new IniSection(InitialConditionRegion.InitialConditionIniHeader);

            iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.Quantity.Key, globalInitialConditionQuantity.ToString().ToLower());
            iniSection.AddPropertyWithOptionalComment(InitialConditionRegion.DataFile.Key, $"Initial{globalInitialConditionQuantity}.ini");
            iniSection.AddPropertyFromConfiguration(InitialConditionRegion.DataFileType, "1dField");

            return iniSection;
        }
    }
}

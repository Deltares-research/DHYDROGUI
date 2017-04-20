using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class WaterFlowModel1DBoundaryFileWriter
    {
        public static void WriteFile(string targetFile, WaterFlowModel1D waterFlowModel1D)
        {
            new F1DBoundaryFileWriter().WriteFile(targetFile,waterFlowModel1D);
        }
    }
    public class F1DBoundaryFileWriter: BoundaryFileWriter
    {
        public void WriteFile(string targetFile, WaterFlowModel1D waterFlowModel1D)
        {
            var categories = new List<IDelftIniCategory>()
            {
                GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.BoundaryConditionsMajorVersion, GeneralRegion.BoundaryConditionsMinorVersion, GeneralRegion.FileTypeName.BoundaryConditions)
            };

            var startTime = waterFlowModel1D.StartTime;

            var boundaryNodeData = waterFlowModel1D.BoundaryConditions.Where(bc => bc.DataType != WaterFlowModel1DBoundaryNodeDataType.None).ToList();
            categories.AddRange(boundaryNodeData.Select(data => GenerateBoundaryConditionDefinition(startTime, data)));
            categories.AddRange(waterFlowModel1D.LateralSourceData.Select(lateralSourceData => GenerateLateralDischargeDefinition(startTime, lateralSourceData)));

            if (waterFlowModel1D.UseSalt)
            {
                var salinityBoundaryNodeDatas = boundaryNodeData.Where(bnd => bnd.SaltConditionType != SaltBoundaryConditionType.None);
                categories.AddRange(salinityBoundaryNodeDatas.Select(data => GenerateBoundaryConditionDefinitionForSalt(startTime, data)));
                categories.AddRange(waterFlowModel1D.LateralSourceData.Select(lateralSourceData => GenerateLateralDischargeDefinitionForSalt(startTime, lateralSourceData)));
            }

            if (waterFlowModel1D.UseTemperature)
            {
                var temperatureBoundaryNodeDatas = boundaryNodeData.Where(bnd => bnd.TemperatureConditionType != TemperatureBoundaryConditionType.None);
                categories.AddRange(temperatureBoundaryNodeDatas.Select(data => GenerateBoundaryConditionDefinitionForTemperature(startTime, data)));
                categories.AddRange(waterFlowModel1D.LateralSourceData.Select(lateralSourceData => GenerateLateralDischargeDefinitionForTemperature(startTime, lateralSourceData)));
            }

            categories.AddRange(GenerateWindDefinitions(startTime, waterFlowModel1D.Wind));
            categories.AddRange(GenerateMeteoDataDefinitions(startTime, waterFlowModel1D.MeteoData));
            
            if (File.Exists(targetFile)) File.Delete(targetFile);
            new DelftBcWriter().WriteBcFile(categories, targetFile);
        }
        
        private IDelftBcCategory GenerateBoundaryConditionDefinition(DateTime startTime, WaterFlowModel1DBoundaryNodeData boundaryNodeData)
        {
            var functionType = BoundaryFileWriterHelper.GetFunctionString(boundaryNodeData.DataType);
            var interpolationType = (boundaryNodeData.InterpolationType == InterpolationType.Constant ?
                BoundaryRegion.TimeInterpolationStrings.BlockFrom : BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate);
            // From ModelApi: if interpolation type is anything other than Constant, then default to linear
            // From FM BcFile: the string representation of constant interpolation is "block"
            //                 whereas we make a distinction between "block-from" and "block-to"

            string periodic = GetTimeSeriesIsPeriodicProperty(boundaryNodeData.Data);

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var boundaryDefinition = definitionGenerator.CreateRegion(boundaryNodeData.Node.Name, functionType, interpolationType, periodic);

            switch (boundaryNodeData.DataType)
            {
                case WaterFlowModel1DBoundaryNodeDataType.FlowConstant:
                    boundaryDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterDischarge,
                        BoundaryRegion.UnitStrings.WaterDischarge, boundaryNodeData.Flow);
                    break;
                case WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries:
                    var waterDischargeData = new Dictionary<string, string>{ {BoundaryRegion.QuantityStrings.WaterDischarge, BoundaryRegion.UnitStrings.WaterDischarge} };
                    boundaryDefinition.Table = GenerateTableForTimeSeriesData(waterDischargeData, boundaryNodeData.Data, startTime);
                    break;
                case WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable:
                    boundaryDefinition.Table = GenerateTableForDischargeWaterLevelData(boundaryNodeData.Data);
                    break;
                case WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant:
                    boundaryDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterLevel,
                        BoundaryRegion.UnitStrings.WaterLevel, boundaryNodeData.WaterLevel);
                    break;
                case WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    var waterLevelData = new Dictionary<string, string> { { BoundaryRegion.QuantityStrings.WaterLevel, BoundaryRegion.UnitStrings.WaterLevel } };
                    boundaryDefinition.Table = GenerateTableForTimeSeriesData(waterLevelData, boundaryNodeData.Data, startTime);
                    break;
            }
            return boundaryDefinition;
        }
        
        private static IDelftBcCategory GenerateBoundaryConditionDefinitionForSalt(DateTime startTime, WaterFlowModel1DBoundaryNodeData boundaryNodeData)
        {
            var functionType = BoundaryFileWriterHelper.GetFunctionString(boundaryNodeData.SaltConditionType);
            var interpolationType = (boundaryNodeData.SaltInterpolationType == InterpolationType.Constant ?
            BoundaryRegion.TimeInterpolationStrings.BlockFrom : BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate);
            // From ModelApi: if interpolation type is anything other than Constant, then default to linear
            // From FM BcFile: the string representation of constant interpolation is "block"
            //                 whereas we make a distinction between "block-from" and "block-to"
            string periodic = GetTimeSeriesIsPeriodicProperty(boundaryNodeData.SaltConcentrationTimeSeries);

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var boundaryDefinition = definitionGenerator.CreateRegion(boundaryNodeData.Node.Name, functionType, interpolationType, periodic);

            switch (boundaryNodeData.SaltConditionType)
            {
                case SaltBoundaryConditionType.Constant:
                    boundaryDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterSalinity, 
                        BoundaryRegion.UnitStrings.SaltPpt, boundaryNodeData.SaltConcentrationConstant);
                    break;
                case SaltBoundaryConditionType.TimeDependent:
                    var waterSalinityData = new Dictionary<string, string> { { BoundaryRegion.QuantityStrings.WaterSalinity, BoundaryRegion.UnitStrings.SaltPpt } };
                    boundaryDefinition.Table = GenerateTableForTimeSeriesData(waterSalinityData, boundaryNodeData.SaltConcentrationTimeSeries, startTime);
                    break;
            }
            return boundaryDefinition;
        }

        private static IDelftBcCategory GenerateBoundaryConditionDefinitionForTemperature(DateTime startTime, WaterFlowModel1DBoundaryNodeData boundaryNodeData)
        {
            var functionType = BoundaryFileWriterHelper.GetFunctionString(boundaryNodeData.TemperatureConditionType);
            var interpolationType = (boundaryNodeData.TemperatureInterpolationType == InterpolationType.Constant ?
            BoundaryRegion.TimeInterpolationStrings.BlockFrom : BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate);
            // From ModelApi: if interpolation type is anything other than Constant, then default to linear
            // From FM BcFile: the string representation of constant interpolation is "block"
            //                 whereas we make a distinction between "block-from" and "block-to"
            var periodic = GetTimeSeriesIsPeriodicProperty(boundaryNodeData.TemperatureTimeSeries);

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var boundaryDefinition = definitionGenerator.CreateRegion(boundaryNodeData.Feature.Name, functionType, interpolationType, periodic);

            switch (boundaryNodeData.TemperatureConditionType)
            {
                case TemperatureBoundaryConditionType.Constant:
                    boundaryDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterTemperature,
                        BoundaryRegion.UnitStrings.WaterTemperature, boundaryNodeData.TemperatureConstant);
                    break;
                case TemperatureBoundaryConditionType.TimeDependent:
                    var waterTemperatureData = new Dictionary<string, string> { { BoundaryRegion.QuantityStrings.WaterTemperature, BoundaryRegion.UnitStrings.WaterTemperature } };
                    boundaryDefinition.Table = GenerateTableForTimeSeriesData(waterTemperatureData, boundaryNodeData.TemperatureTimeSeries, startTime);
                    break;
            }
            return boundaryDefinition;
        }

        private static IDelftBcCategory GenerateLateralDischargeDefinition(DateTime startTime, WaterFlowModel1DLateralSourceData lateralSourceData)
        {
            var functionType = BoundaryFileWriterHelper.GetFunctionString(lateralSourceData.DataType);
            var interpolationType = GetTimeSeriesInterpolationTypeProperty(lateralSourceData.Data);
            string periodic = GetTimeSeriesIsPeriodicProperty(lateralSourceData.Data);
            
            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(BoundaryRegion.BcLateralHeader);
            var lateralDefinition = definitionGenerator.CreateRegion(lateralSourceData.Feature.Name, functionType, interpolationType, periodic);
            
            switch (lateralSourceData.DataType)
            {
                case WaterFlowModel1DLateralDataType.FlowConstant:
                    lateralDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterDischarge,
                        BoundaryRegion.UnitStrings.WaterDischarge, lateralSourceData.Flow);
                    break;
                case WaterFlowModel1DLateralDataType.FlowTimeSeries:
                    var waterDischargeData = new Dictionary<string, string> { { BoundaryRegion.QuantityStrings.WaterDischarge, BoundaryRegion.UnitStrings.WaterDischarge } };
                    lateralDefinition.Table = GenerateTableForTimeSeriesData(waterDischargeData, lateralSourceData.Data, startTime);
                    break;
                case WaterFlowModel1DLateralDataType.FlowWaterLevelTable:
                    lateralDefinition.Table = GenerateTableForDischargeWaterLevelData(lateralSourceData.Data);
                    break;
            }
            return lateralDefinition;
        }

        private static IDelftBcCategory GenerateLateralDischargeDefinitionForSalt(DateTime startTime, WaterFlowModel1DLateralSourceData lateralSourceData)
        {
            var functionType = BoundaryFileWriterHelper.GetFunctionString(lateralSourceData.SaltLateralDischargeType);
            var interpolationType = BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate;
            if (lateralSourceData.SaltLateralDischargeType == SaltLateralDischargeType.ConcentrationTimeSeries)
                interpolationType = GetTimeSeriesInterpolationTypeProperty(lateralSourceData.SaltConcentrationTimeSeries);
            if (lateralSourceData.SaltLateralDischargeType == SaltLateralDischargeType.MassTimeSeries)
                interpolationType = GetTimeSeriesInterpolationTypeProperty(lateralSourceData.SaltMassTimeSeries);

            string periodic = GetTimeSeriesIsPeriodicProperty(lateralSourceData.SaltConcentrationTimeSeries);

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(BoundaryRegion.BcLateralHeader);
            var lateralDefinition = definitionGenerator.CreateRegion(lateralSourceData.Feature.Name, functionType, interpolationType, periodic); 
            
            switch (lateralSourceData.SaltLateralDischargeType)
            {
                case SaltLateralDischargeType.ConcentrationConstant:
                    lateralDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterSalinity, 
                        BoundaryRegion.UnitStrings.SaltPpt, lateralSourceData.SaltConcentrationDischargeConstant);
                    break;
                case SaltLateralDischargeType.ConcentrationTimeSeries:
                    var waterSalinityDataPpt = new Dictionary<string, string> { { BoundaryRegion.QuantityStrings.WaterSalinity, BoundaryRegion.UnitStrings.SaltPpt } };
                    lateralDefinition.Table = GenerateTableForTimeSeriesData(waterSalinityDataPpt, lateralSourceData.SaltConcentrationTimeSeries, startTime);
                    break;
                case SaltLateralDischargeType.MassConstant:
                    lateralDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterSalinity, 
                        BoundaryRegion.UnitStrings.SaltMass, lateralSourceData.SaltMassDischargeConstant);
                    break;
                case SaltLateralDischargeType.MassTimeSeries:
                    var waterSalinityDataMass = new Dictionary<string, string> { { BoundaryRegion.QuantityStrings.WaterSalinity, BoundaryRegion.UnitStrings.SaltMass } };
                    lateralDefinition.Table = GenerateTableForTimeSeriesData(waterSalinityDataMass, lateralSourceData.SaltMassTimeSeries, startTime);
                    break;
                case SaltLateralDischargeType.Default:
                    lateralDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterSalinity, 
                        BoundaryRegion.UnitStrings.SaltPpt, WaterFlowModel1DLateralSourceData.DefaultSalinity);
                    break;
            }
            return lateralDefinition;
        }

        private static IDelftBcCategory GenerateLateralDischargeDefinitionForTemperature(DateTime startTime, WaterFlowModel1DLateralSourceData lateralSourceData)
        {
            var functionType = BoundaryFileWriterHelper.GetFunctionString(lateralSourceData.TemperatureLateralDischargeType);
            var interpolationType = BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate;
            if (lateralSourceData.TemperatureLateralDischargeType == TemperatureLateralDischargeType.TimeDependent)
                interpolationType = GetTimeSeriesInterpolationTypeProperty(lateralSourceData.TemperatureTimeSeries);

            var periodic = GetTimeSeriesIsPeriodicProperty(lateralSourceData.TemperatureTimeSeries);

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(BoundaryRegion.BcLateralHeader);
            var lateralDefinition = definitionGenerator.CreateRegion(lateralSourceData.Feature.Name, functionType, interpolationType, periodic);

            switch (lateralSourceData.TemperatureLateralDischargeType)
            {
                case TemperatureLateralDischargeType.Constant:
                    lateralDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterTemperature, 
                        BoundaryRegion.UnitStrings.WaterTemperature, lateralSourceData.TemperatureConstant);
                    break;
                case TemperatureLateralDischargeType.TimeDependent:
                    var waterTemperatureData = new Dictionary<string, string> {{BoundaryRegion.QuantityStrings.WaterTemperature, BoundaryRegion.UnitStrings.WaterTemperature}};
                    lateralDefinition.Table = GenerateTableForTimeSeriesData(waterTemperatureData, lateralSourceData.TemperatureTimeSeries, startTime);
                    break;
            }
            return lateralDefinition;
        }

        private static IList<IDelftIniCategory> GenerateWindDefinitions(DateTime startTime, WindFunction wind)
        {
            var interpolationType = GetTimeSeriesInterpolationTypeProperty(wind);

            IList<IDelftIniCategory> definitions = new List<IDelftIniCategory>();

            if (wind.Arguments.Count > 0)
            {
                DefinitionGeneratorBoundary windSpeedBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
                var windSpeedDefinition = windSpeedBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                    BoundaryRegion.FunctionStrings.TimeSeries, interpolationType);

                // For historical reasons we split Wind into 2 separate functions in the BcFile
                var windSpeedFunction = (WindFunction)wind.Clone(true);
                windSpeedFunction.Components.RemoveAllWhere(c => !c.Name.ToLower().Contains("velocity"));

                var windSpeedData = new Dictionary<string, string> { { BoundaryRegion.QuantityStrings.WindSpeed, BoundaryRegion.UnitStrings.WindSpeed } };
                windSpeedDefinition.Table = GenerateTableForTimeSeriesData(windSpeedData, windSpeedFunction, startTime);
                definitions.Add(windSpeedDefinition);

                DefinitionGeneratorBoundary windDirBound = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
                var windDirDefinition = windDirBound.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                    BoundaryRegion.FunctionStrings.TimeSeries, interpolationType);

                // For historical reasons we split Wind into 2 separate functions in the BcFile
                var windDirectionFunction = (WindFunction)wind.Clone(true);
                windDirectionFunction.Components.RemoveAllWhere(c => !c.Name.ToLower().Contains("direction"));

                var windDirectionData = new Dictionary<string, string> { { BoundaryRegion.QuantityStrings.WindDirection, BoundaryRegion.UnitStrings.WindDirection } };
                windDirDefinition.Table = GenerateTableForTimeSeriesData(windDirectionData, windDirectionFunction, startTime);
                definitions.Add(windDirDefinition);
            }
            return definitions;
        }

        private static IList<IDelftIniCategory> GenerateMeteoDataDefinitions(DateTime startTime, MeteoFunction meteoData)
        {
            IList<IDelftIniCategory> definitions = new List<IDelftIniCategory>();

            if (meteoData.Arguments.Count > 0)
            {
                var interpolationType = GetTimeSeriesInterpolationTypeProperty(meteoData);

                // Currently, the EC module isn't able to handle timeSeries data with multiple components
                // Split Temperature into 3 separate functions
                
                // air temperature
                var airTemperatureBoundary = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
                var airTemperatureDefinition = airTemperatureBoundary.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                    BoundaryRegion.FunctionStrings.TimeSeries, interpolationType);

                var airTemperatureData = new Dictionary<string, string> { { BoundaryRegion.QuantityStrings.MeteoDataAirTemperature, BoundaryRegion.UnitStrings.MeteoDataAirTemperature } };

                var airTemperatureFunction = (MeteoFunction)meteoData.Clone(true);
                airTemperatureFunction.Components.RemoveAllWhere(c => c.Name != "Air temperature");

                airTemperatureDefinition.Table = GenerateTableForTimeSeriesData(airTemperatureData, airTemperatureFunction, startTime);
                definitions.Add(airTemperatureDefinition);

                // humidity
                var humidityBoundary = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
                var humidityDefinition = humidityBoundary.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                    BoundaryRegion.FunctionStrings.TimeSeries, interpolationType);

                var humidityData = new Dictionary<string, string> { { BoundaryRegion.QuantityStrings.MeteoDataHumidity, BoundaryRegion.UnitStrings.MeteoDataHumidity } };

                var humidityFunction = (MeteoFunction)meteoData.Clone(true);
                humidityFunction.Components.RemoveAllWhere(c => c.Name != "Relative humidity");

                humidityDefinition.Table = GenerateTableForTimeSeriesData(humidityData, humidityFunction, startTime);
                definitions.Add(humidityDefinition);

                // cloudiness
                var cloudinessBoundary = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
                var cloudinessDefinition = cloudinessBoundary.CreateRegion(FunctionAttributes.StandardFeatureNames.ModelWide,
                    BoundaryRegion.FunctionStrings.TimeSeries, interpolationType);

                var cloudinessData = new Dictionary<string, string> { { BoundaryRegion.QuantityStrings.MeteoDataCloudiness, BoundaryRegion.UnitStrings.MeteoDataCloudiness } };

                var cloudinessFunction = (MeteoFunction)meteoData.Clone(true);
                cloudinessFunction.Components.RemoveAllWhere(c => c.Name != "Cloudiness");

                cloudinessDefinition.Table = GenerateTableForTimeSeriesData(cloudinessData, cloudinessFunction, startTime);
                definitions.Add(cloudinessDefinition);
            }
            return definitions;           
        }

        private static string GetTimeSeriesInterpolationTypeProperty(IFunction timeSeries)
        {
            string periodic = null;
            if (timeSeries.Arguments != null && timeSeries.Arguments.Count > 0)
            {
                periodic = (timeSeries.Arguments[0].InterpolationType == InterpolationType.Constant)
                    ? BoundaryRegion.TimeInterpolationStrings.BlockFrom
                    : BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate;
            }
            return periodic;
        }

    }
}

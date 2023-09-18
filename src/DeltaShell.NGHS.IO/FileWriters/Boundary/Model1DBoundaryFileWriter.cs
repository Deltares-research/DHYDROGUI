using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Boundary
{
    public class Model1DBoundaryFileWriter: BoundaryFileWriter
    {
        public IEnumerable<DelftBcCategory> GenerateModel1DLateralSourceDataDelftBcCategories(DateTime startTime,
            IEnumerable<Model1DLateralSourceData> lateralSourcesData, bool useSalt, bool useTemperature,
            string bcForcingHeader)
        {
            var categories = new List<DelftBcCategory>();
            if (lateralSourcesData != null)
            {
                var model1DLateralSourceDatas = lateralSourcesData as Model1DLateralSourceData[] ?? lateralSourcesData.ToArray();
                categories.AddRange( model1DLateralSourceDatas.Select(lsd => GenerateLateralDischargeDefinition(startTime, lsd, bcForcingHeader)));
                if (useSalt)
                {
                    categories.AddRange(model1DLateralSourceDatas.Select(lsd => GenerateLateralDischargeDefinitionForSalt(startTime, lsd, bcForcingHeader)));
                }

                if (useTemperature)
                {
                    categories.AddRange(Enumerable.Select(model1DLateralSourceDatas, lsd => GenerateLateralDischargeDefinitionForTemperature(startTime, lsd, bcForcingHeader)));
                }
            }

            return categories;
        }

        public IEnumerable<DelftBcCategory> GenerateModel1DNodeBoundaryDelftBcCategories(DateTime startTime, IEnumerable<Model1DBoundaryNodeData> boundaryConditions1D, bool useSalt, bool useTemperature, string bcBoundaryHeader)
        {
            var categories = new List<DelftBcCategory>();
            if (boundaryConditions1D != null)
            {
                var boundaryNodeData = boundaryConditions1D.Where(bc => bc.DataType != Model1DBoundaryNodeDataType.None).ToList();
                categories.AddRange(boundaryNodeData.Select(data => GenerateBoundaryConditionDefinition(startTime, data, bcBoundaryHeader)));
            
                if (useSalt)
                {
                    var salinityBoundaryNodeDatas = boundaryNodeData.Where(bnd => bnd.SaltConditionType != SaltBoundaryConditionType.None);
                    categories.AddRange(salinityBoundaryNodeDatas.Select(data => GenerateBoundaryConditionDefinitionForSalt(startTime, data, bcBoundaryHeader)));
                }

                if (useTemperature)
                {
                    var temperatureBoundaryNodeDatas = boundaryNodeData.Where(bnd => bnd.TemperatureConditionType != TemperatureBoundaryConditionType.None);
                    categories.AddRange(temperatureBoundaryNodeDatas.Select(data => GenerateBoundaryConditionDefinitionForTemperature(startTime, data, bcBoundaryHeader)));
                }
            }

            return categories;
        }

        private DelftBcCategory GenerateBoundaryConditionDefinition(DateTime startTime, Model1DBoundaryNodeData boundaryNodeData, string bcBoundaryHeader)
        {
            var functionType = BoundaryFileWriterHelper.GetFunctionString(boundaryNodeData.DataType);
            var interpolationType = (boundaryNodeData.InterpolationType == InterpolationType.Constant ?
                BoundaryRegion.TimeInterpolationStrings.BlockFrom : BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate);
            // From ModelApi: if interpolation type is anything other than Constant, then default to linear
            // From FM BcFile: the string representation of constant interpolation is "block"
            //                 whereas we make a distinction between "block-from" and "block-to"

            string periodic = GetTimeSeriesIsPeriodicProperty(boundaryNodeData.Data);

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(bcBoundaryHeader);
            string name = string.Empty;
            var isManhole = false;
            if (boundaryNodeData.Node is Manhole manhole)
            {
                name = manhole.Compartments.OfType<OutletCompartment>().FirstOrDefault()?.Name;
                isManhole = true;
            }
            else
            {
                name = boundaryNodeData.Node.Name;
            }

            var boundaryDefinition = definitionGenerator.CreateRegion(name, functionType, interpolationType, periodic);
            if (isManhole)
            {
                boundaryDefinition.Section.AddPropertyWithOptionalComment("manHoleName", boundaryNodeData.Node.Name);
            }

            switch (boundaryNodeData.DataType)
            {
                case Model1DBoundaryNodeDataType.FlowConstant:
                    boundaryDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterDischarge,
                        BoundaryRegion.UnitStrings.WaterDischarge, boundaryNodeData.Flow);
                    break;
                case Model1DBoundaryNodeDataType.FlowTimeSeries:
                    var waterDischargeData = new QuantityUnitPair(BoundaryRegion.QuantityStrings.WaterDischarge, BoundaryRegion.UnitStrings.WaterDischarge);
                    boundaryDefinition.Table = GenerateTableForTimeSeriesData(waterDischargeData, boundaryNodeData.Data, startTime);
                    break;
                case Model1DBoundaryNodeDataType.FlowWaterLevelTable:
                    boundaryDefinition.Table = GenerateTableForDischargeWaterLevelData(boundaryNodeData.Data);
                    break;
                case Model1DBoundaryNodeDataType.WaterLevelConstant:
                    boundaryDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterLevel,
                        BoundaryRegion.UnitStrings.WaterLevel, boundaryNodeData.WaterLevel);
                    break;
                case Model1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    var waterLevelData = new QuantityUnitPair(BoundaryRegion.QuantityStrings.WaterLevel, BoundaryRegion.UnitStrings.WaterLevel);
                    boundaryDefinition.Table = GenerateTableForTimeSeriesData(waterLevelData, boundaryNodeData.Data, startTime);
                    break;
            }
            return boundaryDefinition;
        }
        
        private static DelftBcCategory GenerateBoundaryConditionDefinitionForSalt(DateTime startTime,
            Model1DBoundaryNodeData boundaryNodeData, string bcBoundaryHeader)
        {
            var functionType = BoundaryFileWriterHelper.GetFunctionString(boundaryNodeData.SaltConditionType);
            var interpolationType = (boundaryNodeData.SaltInterpolationType == InterpolationType.Constant ?
                BoundaryRegion.TimeInterpolationStrings.BlockFrom : BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate);
            // From ModelApi: if interpolation type is anything other than Constant, then default to linear
            // From FM BcFile: the string representation of constant interpolation is "block"
            //                 whereas we make a distinction between "block-from" and "block-to"
            string periodic = GetTimeSeriesIsPeriodicProperty(boundaryNodeData.SaltConcentrationTimeSeries);

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(bcBoundaryHeader);
            var boundaryDefinition = definitionGenerator.CreateRegion(boundaryNodeData.Node.Name, functionType, interpolationType, periodic);

            switch (boundaryNodeData.SaltConditionType)
            {
                case SaltBoundaryConditionType.Constant:
                    boundaryDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterSalinity, 
                        BoundaryRegion.UnitStrings.SaltPpt, boundaryNodeData.SaltConcentrationConstant);
                    break;
                case SaltBoundaryConditionType.TimeDependent:
                    var waterSalinityData = new QuantityUnitPair(BoundaryRegion.QuantityStrings.WaterSalinity, BoundaryRegion.UnitStrings.SaltPpt);
                    boundaryDefinition.Table = GenerateTableForTimeSeriesData(waterSalinityData, boundaryNodeData.SaltConcentrationTimeSeries, startTime);
                    break;
            }
            return boundaryDefinition;
        }

        private static DelftBcCategory GenerateBoundaryConditionDefinitionForTemperature(DateTime startTime,
            Model1DBoundaryNodeData boundaryNodeData, string bcBoundaryHeader)
        {
            var functionType = BoundaryFileWriterHelper.GetFunctionString(boundaryNodeData.TemperatureConditionType);
            var interpolationType = (boundaryNodeData.TemperatureInterpolationType == InterpolationType.Constant ?
                BoundaryRegion.TimeInterpolationStrings.BlockFrom : BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate);
            // From ModelApi: if interpolation type is anything other than Constant, then default to linear
            // From FM BcFile: the string representation of constant interpolation is "block"
            //                 whereas we make a distinction between "block-from" and "block-to"
            var periodic = GetTimeSeriesIsPeriodicProperty(boundaryNodeData.TemperatureTimeSeries);

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(bcBoundaryHeader);
            var boundaryDefinition = definitionGenerator.CreateRegion(boundaryNodeData.Feature.Name, functionType, interpolationType, periodic);

            switch (boundaryNodeData.TemperatureConditionType)
            {
                case TemperatureBoundaryConditionType.Constant:
                    boundaryDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterTemperature,
                        BoundaryRegion.UnitStrings.WaterTemperature, boundaryNodeData.TemperatureConstant);
                    break;
                case TemperatureBoundaryConditionType.TimeDependent:
                    var waterTemperatureData = new QuantityUnitPair(BoundaryRegion.QuantityStrings.WaterTemperature, BoundaryRegion.UnitStrings.WaterTemperature);
                    boundaryDefinition.Table = GenerateTableForTimeSeriesData(waterTemperatureData, boundaryNodeData.TemperatureTimeSeries, startTime);
                    break;
            }
            return boundaryDefinition;
        }

        private static DelftBcCategory GenerateLateralDischargeDefinition(DateTime startTime,
            Model1DLateralSourceData lateralSourceData, string bcForcingHeader)
        {
            var functionType = BoundaryFileWriterHelper.GetFunctionString(lateralSourceData.DataType);
            var interpolationType = GetTimeSeriesInterpolationTypeProperty(lateralSourceData.Data);
            string periodic = GetTimeSeriesIsPeriodicProperty(lateralSourceData.Data);
            
            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(bcForcingHeader);
            var lateralDefinition = definitionGenerator.CreateRegion(lateralSourceData.Feature.Name, functionType, interpolationType, periodic);
            
            switch (lateralSourceData.DataType)
            {
                case Model1DLateralDataType.FlowConstant:
                    lateralDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.LateralDischarge,
                        BoundaryRegion.UnitStrings.WaterDischarge, lateralSourceData.Flow);
                    break;
                case Model1DLateralDataType.FlowTimeSeries:
                    var waterDischargeData = new QuantityUnitPair(BoundaryRegion.QuantityStrings.LateralDischarge, BoundaryRegion.UnitStrings.WaterDischarge);
                    lateralDefinition.Table = GenerateTableForTimeSeriesData(waterDischargeData, lateralSourceData.Data, startTime);
                    break;
                case Model1DLateralDataType.FlowWaterLevelTable:
                    lateralDefinition.Table = GenerateTableForDischargeWaterLevelData(lateralSourceData.Data, BoundaryRegion.QuantityStrings.LateralDischarge);
                    break;
                case Model1DLateralDataType.FlowRealTime:
                    break;
            }
            return lateralDefinition;
        }

        private static DelftBcCategory GenerateLateralDischargeDefinitionForSalt(DateTime startTime,
            Model1DLateralSourceData lateralSourceData, string bcForcingHeader)
        {
            var functionType = BoundaryFileWriterHelper.GetFunctionString(lateralSourceData.SaltLateralDischargeType);
            var interpolationType = BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate;
            if (lateralSourceData.SaltLateralDischargeType == SaltLateralDischargeType.ConcentrationTimeSeries)
                interpolationType = GetTimeSeriesInterpolationTypeProperty(lateralSourceData.SaltConcentrationTimeSeries);
            if (lateralSourceData.SaltLateralDischargeType == SaltLateralDischargeType.MassTimeSeries)
                interpolationType = GetTimeSeriesInterpolationTypeProperty(lateralSourceData.SaltMassTimeSeries);

            string periodic = GetTimeSeriesIsPeriodicProperty(lateralSourceData.SaltConcentrationTimeSeries);

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(bcForcingHeader);
            var lateralDefinition = definitionGenerator.CreateRegion(lateralSourceData.Feature.Name, functionType, interpolationType, periodic); 
            
            switch (lateralSourceData.SaltLateralDischargeType)
            {
                case SaltLateralDischargeType.ConcentrationConstant:
                    lateralDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterSalinity, 
                        BoundaryRegion.UnitStrings.SaltPpt, lateralSourceData.SaltConcentrationDischargeConstant);
                    break;
                case SaltLateralDischargeType.ConcentrationTimeSeries:
                    var waterSalinityDataPpt = new QuantityUnitPair(BoundaryRegion.QuantityStrings.WaterSalinity, BoundaryRegion.UnitStrings.SaltPpt);
                    lateralDefinition.Table = GenerateTableForTimeSeriesData(waterSalinityDataPpt, lateralSourceData.SaltConcentrationTimeSeries, startTime);
                    break;
                case SaltLateralDischargeType.MassConstant:
                    lateralDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterSalinity, 
                        BoundaryRegion.UnitStrings.SaltMass, lateralSourceData.SaltMassDischargeConstant);
                    break;
                case SaltLateralDischargeType.MassTimeSeries:
                    var waterSalinityDataMass = new QuantityUnitPair(BoundaryRegion.QuantityStrings.WaterSalinity, BoundaryRegion.UnitStrings.SaltMass);
                    lateralDefinition.Table = GenerateTableForTimeSeriesData(waterSalinityDataMass, lateralSourceData.SaltMassTimeSeries, startTime);
                    break;
                case SaltLateralDischargeType.Default:
                    lateralDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterSalinity, 
                        BoundaryRegion.UnitStrings.SaltPpt, Model1DLateralSourceData.DefaultSalinity);
                    break;
            }
            return lateralDefinition;
        }

        private static DelftBcCategory GenerateLateralDischargeDefinitionForTemperature(DateTime startTime,
            Model1DLateralSourceData lateralSourceData, string bcForcingHeader)
        {
            var functionType = BoundaryFileWriterHelper.GetFunctionString(lateralSourceData.TemperatureLateralDischargeType);
            var interpolationType = BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate;
            if (lateralSourceData.TemperatureLateralDischargeType == TemperatureLateralDischargeType.TimeDependent)
                interpolationType = GetTimeSeriesInterpolationTypeProperty(lateralSourceData.TemperatureTimeSeries);

            var periodic = GetTimeSeriesIsPeriodicProperty(lateralSourceData.TemperatureTimeSeries);

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(bcForcingHeader);
            var lateralDefinition = definitionGenerator.CreateRegion(lateralSourceData.Feature.Name, functionType, interpolationType, periodic);

            switch (lateralSourceData.TemperatureLateralDischargeType)
            {
                case TemperatureLateralDischargeType.Constant:
                    lateralDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterTemperature, 
                        BoundaryRegion.UnitStrings.WaterTemperature, lateralSourceData.TemperatureConstant);
                    break;
                case TemperatureLateralDischargeType.TimeDependent:
                    var waterTemperatureData = new QuantityUnitPair(BoundaryRegion.QuantityStrings.WaterTemperature, BoundaryRegion.UnitStrings.WaterTemperature);
                    lateralDefinition.Table = GenerateTableForTimeSeriesData(waterTemperatureData, lateralSourceData.TemperatureTimeSeries, startTime);
                    break;
            }
            return lateralDefinition;
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
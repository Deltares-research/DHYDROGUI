using System;
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
        public BcIniSection GenerateBoundaryConditionDefinition(DateTime startTime, Model1DBoundaryNodeData boundaryNodeData, string bcBoundaryHeader)
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

        public static BcIniSection GenerateLateralDischargeDefinition(DateTime startTime,
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
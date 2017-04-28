using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    public class RainfallRunoffBoundaryDataFileWriter : BoundaryFileWriter
    {
        public void WriteFile(string targetFile, IRainfallRunoffModel rainfallRunoffModel)
        {
            var categories = new List<IDelftIniCategory>()
            {
                GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.BoundaryConditionsMajorVersion, GeneralRegion.BoundaryConditionsMinorVersion,
                    GeneralRegion.FileTypeName.BoundaryConditions)
            };

            var startTime = rainfallRunoffModel.StartTime;
            
            foreach (var boundaryData in rainfallRunoffModel.BoundaryData)
            {
                categories.Add(GenerateBoundaryConditionDefinition(startTime, boundaryData));
            }
            if (File.Exists(targetFile)) File.Delete(targetFile);
            new DelftBcWriter().WriteBcFile(categories, targetFile);
        }

        private IDelftIniCategory GenerateBoundaryConditionDefinition(DateTime startTime, RunoffBoundaryData boundaryData)
        {
            var functionType = boundaryData.Series.IsTimeSeries ? BoundaryRegion.FunctionStrings.TimeSeries : BoundaryRegion.FunctionStrings.Constant;
            
            var interpolationType = boundaryData.Series.IsTimeSeries
                ? boundaryData.Series.Data.Arguments[0].InterpolationType == InterpolationType.Constant
                    ? BoundaryRegion.TimeInterpolationStrings.BlockFrom
                    : BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate
                : null;

            string periodic = boundaryData.Series.IsTimeSeries ? GetTimeSeriesIsPeriodicProperty(boundaryData.Series.Data) : null;

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var boundaryDefinition = definitionGenerator.CreateRegion(boundaryData.Boundary.Name, functionType, interpolationType, periodic);
            if (boundaryData.Series.IsTimeSeries)
            {
                var waterLevelData = new Dictionary<string, string>{ {BoundaryRegion.QuantityStrings.WaterLevel, BoundaryRegion.UnitStrings.WaterLevel} };
                boundaryDefinition.Table = GenerateTableForTimeSeriesData(waterLevelData, boundaryData.Series.Data, startTime);
            }
            else
            {
                boundaryDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterLevel, BoundaryRegion.UnitStrings.WaterLevel, boundaryData.Series.Value);
            }
            
            return boundaryDefinition;
        }
    }
}
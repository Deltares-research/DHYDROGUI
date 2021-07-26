using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers;
using GeoAPI.Extensions.Feature;

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
            var rrBoundaries = new List<IFeature>();
            foreach (var boundaryData in rainfallRunoffModel.BoundaryData)
            {
                categories.Add(GenerateBoundaryConditionDefinition(startTime, boundaryData));
                rrBoundaries.Add(boundaryData.Boundary);
            }
            var rrModel = rainfallRunoffModel as RainfallRunoffModel;
            if (rrModel != null)
            {
                var linksFound = rrModel.GetAllModelData()
                    .SelectMany(md =>
                    {
                        var links = new List<ModelLink>();
                        RainfallRunoffModelController.AddLink(links, md.Catchment);
                        return links;
                    });

                foreach (var link in linksFound)
                {
                    if (link.ToFeature != null && !(link.ToFeature is RunoffBoundary || link.ToFeature is ILateralSource)) continue;

                    var boundary = link.ToFeature;
                    if (boundary == null)
                    {
                        boundary = link.FromFeature;
                    }

                    if (rrBoundaries.Contains(boundary))
                        continue; //already added

                    if (boundary is Catchment boundaryOnTheCatchment && Equals(boundaryOnTheCatchment.CatchmentType, CatchmentType.NWRW))
                        continue; //Nwrw catchments don't have catchment boundaries

                    rrBoundaries.Add(boundary);
                    var boundaryData = new RunoffBoundaryData(
                        new RunoffBoundary
                        {
                            Attributes = boundary.Attributes,
                            Geometry = boundary.Geometry,
                            Name = link.ToId ?? link.FromId,
                        });

                    //This method currently ads a value to the series generated, however it will always be 0, should we include this?
                    categories.Add(GenerateBoundaryConditionDefinition(startTime, boundaryData));
                }
            }

            if (File.Exists(targetFile)) File.Delete(targetFile);
            new DelftBcWriter().WriteBcFile(categories, targetFile);
        }

        private IDelftIniCategory GenerateBoundaryConditionDefinition(DateTime startTime, RunoffBoundaryData boundaryData)
        {
            var boundaryDefinition = GetBoundaryDefinition(boundaryData);
            if (boundaryData.Series.IsTimeSeries)
            {
                var waterLevelData = new Dictionary<string, string>{ {BoundaryRegion.QuantityStrings.WaterLevelQuantityInRR, BoundaryRegion.UnitStrings.WaterLevel} };
                boundaryDefinition.Table = GenerateTableForTimeSeriesData(waterLevelData, boundaryData.Series.Data, startTime);
            }
            else
            {
                boundaryDefinition.Table = GenerateTableForConstantData(BoundaryRegion.QuantityStrings.WaterLevelQuantityInRR, BoundaryRegion.UnitStrings.WaterLevel, boundaryData.Series.Value);
            }
            
            return boundaryDefinition;
        }

        private static IDelftBcCategory GetBoundaryDefinition(RunoffBoundaryData boundaryData)
        {
            var functionType = boundaryData.Series.IsTimeSeries
                ? BoundaryRegion.FunctionStrings.TimeSeries
                : BoundaryRegion.FunctionStrings.Constant;

            var interpolationType = boundaryData.Series.IsTimeSeries
                ? boundaryData.Series.Data.Arguments[0].InterpolationType == InterpolationType.Constant
                    ? BoundaryRegion.TimeInterpolationStrings.BlockFrom
                    : BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate
                : null;

            string periodic = boundaryData.Series.IsTimeSeries
                ? GetTimeSeriesIsPeriodicProperty(boundaryData.Series.Data)
                : null;

            IDefinitionGeneratorBoundary definitionGenerator = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            var boundaryDefinition =
                definitionGenerator.CreateRegion(boundaryData.Boundary.Name, functionType, interpolationType, periodic);
            return boundaryDefinition;
        }
    }
}
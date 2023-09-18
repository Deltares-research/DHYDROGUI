using System;
using System.Collections.Generic;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    /// <summary>
    /// Writer responsible for writing the boundary data in the D-Rainfall Runoff model to file.
    /// </summary>
    public class RainfallRunoffBoundaryDataFileWriter : BoundaryFileWriter
    {
        private static readonly QuantityUnitPair waterLevelQuantityUnitPair =
            new QuantityUnitPair(BoundaryRegion.QuantityStrings.WaterLevelQuantityInRR,
                                 BoundaryRegion.UnitStrings.WaterLevel);
        
        private readonly HashSet<string> handledBoundaries = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        private readonly IBcFileWriter bcFileWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="RainfallRunoffBoundaryDataFileWriter"/> class.
        /// </summary>
        /// <param name="bcFileWriter"> The BC file writer. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bcFileWriter"/> is <c>null</c>.
        /// </exception>
        public RainfallRunoffBoundaryDataFileWriter(IBcFileWriter bcFileWriter)
        {
            Ensure.NotNull(bcFileWriter, nameof(bcFileWriter));
            this.bcFileWriter = bcFileWriter;
        }

        /// <summary>
        /// Writes the boundary data in the provided D-Rainfall Runoff model to the target file.
        /// The following boundary data is written to file:
        /// - The rainfall runoff boundary data.
        /// - The boundary data of unpaved catchments that are not linked to a runoff boundary.
        /// - Default boundary data with a constant water level of 0 for catchments other than unpaved that are not linked to a
        /// runoff boundary. Default boundary data is required by the rainfall runoff kernel.
        /// If a file at the specified path already exists, it will be overwritten.
        /// </summary>
        /// <param name="filePath"> The file path to write the boundary data to. </param>
        /// <param name="rainfallRunoffModel"> The D-Rainfall Runoff model containing the boundary data. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="rainfallRunoffModel"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="filePath"/> is <c>null</c> or white space.
        /// </exception>
        public void WriteFile(string filePath, IRainfallRunoffModel rainfallRunoffModel)
        {
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNull(rainfallRunoffModel, nameof(rainfallRunoffModel));

            IList<DelftBcCategory> categories = CreateDelftBcCategories(rainfallRunoffModel);

            FileUtils.DeleteIfExists(filePath);
            bcFileWriter.WriteBcFile(categories, filePath);
        }

        private IList<DelftBcCategory> CreateDelftBcCategories(IRainfallRunoffModel rainfallRunoffModel)
        {
            var categories = new List<DelftBcCategory>
            {
                new DelftBcCategory(CreateGeneralRegion())
            };

            categories.AddRange(CreateRunoffBoundaryCategories(rainfallRunoffModel));
            categories.AddRange(CreateCatchmentBoundaryCategories(rainfallRunoffModel));

            return categories;
        }

        private static IniSection CreateGeneralRegion()
        {
            return GeneralRegionGenerator.GenerateGeneralRegion(
                GeneralRegion.BoundaryConditionsMajorVersion,
                GeneralRegion.BoundaryConditionsMinorVersion,
                GeneralRegion.FileTypeName.BoundaryConditions);
        }

        private IEnumerable<DelftBcCategory> CreateRunoffBoundaryCategories(IRainfallRunoffModel rainfallRunoffModel)
        {
            foreach (RunoffBoundaryData boundaryData in rainfallRunoffModel.BoundaryData)
            {
                yield return CreateDelftBcCategory(rainfallRunoffModel.StartTime,
                                                   boundaryData.Series,
                                                   boundaryData.Boundary.Name);
            }
        }

        private IEnumerable<DelftBcCategory> CreateCatchmentBoundaryCategories(IRainfallRunoffModel rainfallRunoffModel)
        {
            foreach (CatchmentModelData catchmentModelData in rainfallRunoffModel.ModelData)
            {
                ModelLink modelLink = RainfallRunoffModelController.CreateModelLink(catchmentModelData.Catchment);

                if (ShouldSkipBoundary(modelLink))
                {
                    continue;
                }

                string boundaryName = GetBoundaryName(modelLink);

                yield return catchmentModelData is UnpavedData unpavedData
                                 ? CreateDelftBcCategory(rainfallRunoffModel.StartTime, unpavedData.BoundarySettings.BoundaryData, boundaryName)
                                 : CreateDefaultDelftBcCategory(boundaryName);
            }
        }

        private static string GetBoundaryName(ModelLink modelLink)
        {
            return modelLink.ToId ?? modelLink.FromId;
        }

        private static DelftBcCategory CreateDefaultDelftBcCategory(string boundaryName)
        {
            DelftBcCategory category = CreateDelftBcDefinitionCategory(boundaryName, BoundaryRegion.FunctionStrings.Constant, null, null);
            category.Table = GenerateTableForConstantData(waterLevelQuantityUnitPair.Quantity, waterLevelQuantityUnitPair.Unit, 0);

            return category;
        }

        private DelftBcCategory CreateDelftBcCategory(DateTime startTime, RainfallRunoffBoundaryData rainfallRunoffBoundaryData, string boundaryName)
        {
            string function = GetFunction(rainfallRunoffBoundaryData);
            string interpolation = GetInterpolation(rainfallRunoffBoundaryData);
            string periodic = GetPeriodic(rainfallRunoffBoundaryData);

            DelftBcCategory category = CreateDelftBcDefinitionCategory(boundaryName, function, interpolation, periodic);
            category.Table = CreateTable(startTime, rainfallRunoffBoundaryData);
            
            handledBoundaries.Add(boundaryName);

            return category;
        }

        private static DelftBcCategory CreateDelftBcDefinitionCategory(string boundaryName, string function, string interpolation, string periodic)
        {
            IDefinitionGeneratorBoundary boundaryDefinitionGenerator = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            return boundaryDefinitionGenerator.CreateRegion(boundaryName, function, interpolation, periodic);
        }

        private static IList<IDelftBcQuantityData> CreateTable(DateTime startTime, RainfallRunoffBoundaryData rainfallRunoffBoundaryData)
        {
            return rainfallRunoffBoundaryData.IsTimeSeries
                       ? GenerateTableForTimeSeriesData(waterLevelQuantityUnitPair, rainfallRunoffBoundaryData.Data, startTime)
                       : GenerateTableForConstantData(waterLevelQuantityUnitPair.Quantity, waterLevelQuantityUnitPair.Unit, rainfallRunoffBoundaryData.Value);
        }

        private static string GetFunction(RainfallRunoffBoundaryData boundaryData)
        {
            return boundaryData.IsTimeSeries
                       ? BoundaryRegion.FunctionStrings.TimeSeries
                       : BoundaryRegion.FunctionStrings.Constant;
        }

        private static string GetInterpolation(RainfallRunoffBoundaryData boundaryData)
        {
            if (!boundaryData.IsTimeSeries)
            {
                return null;
            }

            return boundaryData.Data.Arguments[0].InterpolationType == InterpolationType.Constant
                       ? BoundaryRegion.TimeInterpolationStrings.BlockFrom
                       : BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate;
        }

        private static string GetPeriodic(RainfallRunoffBoundaryData boundaryData)
        {
            return boundaryData.IsTimeSeries
                       ? GetTimeSeriesIsPeriodicProperty(boundaryData.Data)
                       : null;
        }

        private bool ShouldSkipBoundary(ModelLink modelLink)
        {
            if (LinkHasInvalidLinkTarget(modelLink) || BoundaryHasAlreadyBeenHandled(GetBoundaryName(modelLink)))
            {
                return true;
            }

            IFeature boundary = modelLink.ToFeature ?? modelLink.FromFeature;
            return boundary is RunoffBoundary;
        }

        private static bool LinkHasInvalidLinkTarget(ModelLink modelLink)
        {
            return modelLink.ToFeature != null && !(modelLink.ToFeature is RunoffBoundary || modelLink.ToFeature is ILateralSource);
        }
        
        private bool BoundaryHasAlreadyBeenHandled(string boundaryName)
        {
            return handledBoundaries.Contains(boundaryName);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.Extensions;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers;
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

        private readonly IBcWriter bcWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="RainfallRunoffBoundaryDataFileWriter"/> class.
        /// </summary>
        /// <param name="bcWriter"> The BC file writer. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="bcWriter"/> is <c>null</c>.
        /// </exception>
        public RainfallRunoffBoundaryDataFileWriter(IBcWriter bcWriter)
        {
            Ensure.NotNull(bcWriter, nameof(bcWriter));
            this.bcWriter = bcWriter;
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

            IList<BcIniSection> sections = CreateBcSections(rainfallRunoffModel);

            FileUtils.DeleteIfExists(filePath);
            bcWriter.WriteBcFile(sections, filePath);
        }

        private IList<BcIniSection> CreateBcSections(IRainfallRunoffModel rainfallRunoffModel)
        {
            var sections = new List<BcIniSection>
            {
                new BcIniSection(CreateGeneralRegion())
            };

            sections.AddRange(CreateRunoffBoundarySections(rainfallRunoffModel));
            sections.AddRange(CreateWasteWaterTreatmentPlantsBoundarySections(rainfallRunoffModel.Basin.WasteWaterTreatmentPlants));
            sections.AddRange(CreateCatchmentBoundarySections(rainfallRunoffModel));

            return sections;
        }

        private static IniSection CreateGeneralRegion()
        {
            return GeneralRegionGenerator.GenerateGeneralRegion(
                GeneralRegion.BoundaryConditionsMajorVersion,
                GeneralRegion.BoundaryConditionsMinorVersion,
                GeneralRegion.FileTypeName.BoundaryConditions);
        }

        private IEnumerable<BcIniSection> CreateRunoffBoundarySections(IRainfallRunoffModel rainfallRunoffModel)
        {
            foreach (RunoffBoundaryData boundaryData in rainfallRunoffModel.BoundaryData)
            {
                yield return CreateBcSection(rainfallRunoffModel.StartTime,
                                             boundaryData.Series,
                                             boundaryData.Boundary.Name);
            }
        }

        private IEnumerable<BcIniSection> CreateWasteWaterTreatmentPlantsBoundarySections(IEnumerable<WasteWaterTreatmentPlant> wasteWaterTreatmentPlants)
        {
            foreach (WasteWaterTreatmentPlant wasteWaterTreatmentPlant in wasteWaterTreatmentPlants)
            {
                HydroLink link = GetOutgoingLink(wasteWaterTreatmentPlant);
                if (link is null || BoundaryHasAlreadyBeenHandled(link.Target.Name))
                {
                    continue;
                }
                yield return CreateDefaultBcSection($"{link.Target.Name}");
            }
        }

        private static HydroLink GetOutgoingLink(WasteWaterTreatmentPlant wasteWaterTreatmentPlant)
        {
            return wasteWaterTreatmentPlant.Links.FirstOrDefault(l => !l.Target.Equals(wasteWaterTreatmentPlant));
        }

        private IEnumerable<BcIniSection> CreateCatchmentBoundarySections(IRainfallRunoffModel rainfallRunoffModel)
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
                                 ? CreateBcSection(rainfallRunoffModel.StartTime, unpavedData.BoundarySettings.BoundaryData, boundaryName)
                                 : CreateDefaultBcSection(boundaryName);
            }
        }

        private static string GetBoundaryName(ModelLink modelLink)
        {
            return modelLink.ToId ?? modelLink.FromId;
        }

        private BcIniSection CreateDefaultBcSection(string boundaryName)
        {
            BcIniSection iniSection = CreateBcDefinitionSection(boundaryName, BoundaryRegion.FunctionStrings.Constant, null, null);
            iniSection.Table = GenerateTableForConstantData(waterLevelQuantityUnitPair.Quantity, waterLevelQuantityUnitPair.Unit, 0);

            handledBoundaries.Add(boundaryName);

            return iniSection;
        }

        private BcIniSection CreateBcSection(DateTime startTime, RainfallRunoffBoundaryData rainfallRunoffBoundaryData, string boundaryName)
        {
            string function = GetFunction(rainfallRunoffBoundaryData);
            string interpolation = GetInterpolation(rainfallRunoffBoundaryData);
            string periodic = GetPeriodic(rainfallRunoffBoundaryData);

            BcIniSection iniSection = CreateBcDefinitionSection(boundaryName, function, interpolation, periodic);
            iniSection.Table = CreateTable(startTime, rainfallRunoffBoundaryData);
            
            handledBoundaries.Add(boundaryName);

            return iniSection;
        }

        private static BcIniSection CreateBcDefinitionSection(string boundaryName, string function, string interpolation, string periodic)
        {
            IDefinitionGeneratorBoundary boundaryDefinitionGenerator = new DefinitionGeneratorBoundary(BoundaryRegion.BcBoundaryHeader);
            return boundaryDefinitionGenerator.CreateRegion(boundaryName, function, interpolation, periodic);
        }

        private static IList<IBcQuantityData> CreateTable(DateTime startTime, RainfallRunoffBoundaryData rainfallRunoffBoundaryData)
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    public static class WaqInitializationSettingsBuilder
    {
        /// <summary>
        /// Builds <see cref="WaqInitializationSettings"/> for the provided
        /// <param name="waterQualityModel"/>
        /// </summary>
        /// <param name="waterQualityModel"> Model to create <see cref="WaqInitializationSettings"/> for </param>
        public static WaqInitializationSettings BuildWaqInitializationSettings(WaterQualityModel waterQualityModel)
        {
            if (waterQualityModel.HydroData == null)
            {
                throw new NotSupportedException(
                    "Can not create initialization settings : no hydro dynamica specified.");
            }

            string hydFilePath = waterQualityModel.HydroData.FilePath;

            string hydFileRoot = Path.GetDirectoryName(hydFilePath);
            if (hydFileRoot == null)
            {
                throw new NotSupportedException("Could not find directory for relative paths");
            }

            string verticalDiffusionFile = null;
            if (!string.IsNullOrEmpty(waterQualityModel.VerticalDiffusionRelativeFilePath))
            {
                verticalDiffusionFile = Path.Combine(hydFileRoot, waterQualityModel.VerticalDiffusionRelativeFilePath);
            }

            return new WaqInitializationSettings
            {
                Settings = waterQualityModel.ModelSettings,
                ReferenceTime = waterQualityModel.ReferenceTime,
                SimulationStartTime = waterQualityModel.StartTime,
                SimulationStopTime = waterQualityModel.StopTime,
                SimulationTimeStep = waterQualityModel.TimeStep,
                InputFile = waterQualityModel.InputFile,
                SubstanceProcessLibrary = waterQualityModel.SubstanceProcessLibrary,
                ProcessCoefficients = waterQualityModel.ProcessCoefficients,
                Dispersion = waterQualityModel.Dispersion,
                InitialConditions = waterQualityModel.InitialConditions,
                UseRestart = waterQualityModel.UseRestart,
                SegmentsPerLayer = waterQualityModel.NumberOfDelwaqSegmentsPerHydrodynamicLayer,
                NumberOfLayers = waterQualityModel.NumberOfWaqSegmentLayers,
                GridFile = Path.Combine(hydFileRoot, waterQualityModel.GridRelativeFilePath),
                AttributesFile = Path.Combine(hydFileRoot, waterQualityModel.AttributesRelativeFilePath),
                VolumesFile = Path.Combine(hydFileRoot, waterQualityModel.VolumesRelativeFilePath),
                SurfacesFile = Path.Combine(hydFileRoot, waterQualityModel.SurfacesRelativeFilePath),
                VerticalDiffusionFile = verticalDiffusionFile,
                HorizontalExchanges = waterQualityModel.NumberOfHorizontalExchanges,
                VerticalExchanges = waterQualityModel.NumberOfVerticalExchanges,
                PointersFile = Path.Combine(hydFileRoot, waterQualityModel.PointersRelativeFilePath),
                HorizontalDispersion = waterQualityModel.HorizontalDispersion,
                VerticalDispersion = waterQualityModel.VerticalDispersion,
                UseAdditionalVerticalDiffusion = waterQualityModel.UseAdditionalHydrodynamicVerticalDiffusion,
                AreasFile = Path.Combine(hydFileRoot, waterQualityModel.AreasRelativeFilePath),
                FlowsFile = Path.Combine(hydFileRoot, waterQualityModel.FlowsRelativeFilePath),
                LengthsFile = Path.Combine(hydFileRoot, waterQualityModel.LengthsRelativeFilePath),
                VelocitiesFile = Path.Combine(hydFileRoot, waterQualityModel.VelocitiesFilePath),
                WidthsFile = Path.Combine(hydFileRoot, waterQualityModel.WidthsFilePath),
                ChezyCoefficientsFile = Path.Combine(hydFileRoot, waterQualityModel.ChezyCoefficientsFilePath),
                ModelWorkDirectory = waterQualityModel.ModelSettings.WorkDirectory,
                BoundaryDataManager = waterQualityModel.BoundaryDataManager,
                LoadsDataManager = waterQualityModel.LoadsDataManager,
                BoundaryNodeIds = waterQualityModel.BoundaryNodeIds,
                LoadAndIds = CreateLoadLocationInformation(waterQualityModel),
                OutputLocations = CreateOutputLocationInformation(waterQualityModel),
                BoundaryAliases =
                    CreateLocationAliases(waterQualityModel.Boundaries, waterQualityModel.BoundaryDataManager),
                LoadsAliases = CreateLocationAliases(waterQualityModel.Loads, waterQualityModel.LoadsDataManager)
            };
        }

        /// <summary>
        /// The boundary aliases are stored as
        /// boundary 1: measurement 1, measurement 2
        /// boundary 2: measurement 2
        /// But should be written in the input file as
        /// measurement 1: boundary 1
        /// measurement 2: boundary 1, boundary 2
        /// </summary>
        private static IDictionary<string, IList<string>> CreateLocationAliases(
            IEnumerable<IHasLocationAliases> locations, DataTableManager dataManager)
        {
            var result = new Dictionary<string, IList<string>>();

            // only write data if there is any data to be found
            if (dataManager.DataTables.Any())
            {
                foreach (IHasLocationAliases location in locations)
                {
                    List<string> aliassesToWrite = location.ParseLocationAliases();

                    if (aliassesToWrite.Count > 0)
                    {
                        foreach (string alias in aliassesToWrite)
                        {
                            // create a new entry in the dictionary if none was yet specified.
                            if (!result.ContainsKey(alias))
                            {
                                result.Add(alias, new List<string>());
                            }

                            // add the boundary to the list
                            result[alias].Add(location.Name);
                        }
                    }
                    else
                    {
                        // make a default entry
                        result.Add(location.Name, new List<string>() {location.Name});
                    }
                }
            }

            return result;
        }

        private static IDictionary<WaterQualityLoad, int> CreateLoadLocationInformation(
            WaterQualityModel waterQualityModel)
        {
            var loadsInformation = new Dictionary<WaterQualityLoad, int>(waterQualityModel.Loads.Count);
            foreach (WaterQualityLoad waterQualityLoad in waterQualityModel.Loads)
            {
                loadsInformation[waterQualityLoad] =
                    waterQualityModel.GetSegmentIndexForLocation(waterQualityLoad.Geometry.Coordinate);
            }

            return loadsInformation;
        }

        private static IDictionary<string, IList<int>> CreateOutputLocationInformation(
            WaterQualityModel waterQualityModel)
        {
            Dictionary<string, IList<int>> obsInformation = GetOutputLocationsForObservationAreas(waterQualityModel);
            AddOutputLocationsForObservationLocations(obsInformation, waterQualityModel);

            return obsInformation;
        }

        private static Dictionary<string, IList<int>> GetOutputLocationsForObservationAreas(
            WaterQualityModel waterQualityModel)
        {
            if (waterQualityModel.ModelSettings.MonitoringOutputLevel == MonitoringOutputLevel.None ||
                waterQualityModel.ModelSettings.MonitoringOutputLevel == MonitoringOutputLevel.Points)
            {
                return new Dictionary<string, IList<int>>();
            }

            Dictionary<string, IList<int>>
                obsInformation = waterQualityModel.ObservationAreas.GetOutputLocations(); // Get cell ID's for layer 0

            // Add remaining cell ID's for remaining layers [1..n]
            if (waterQualityModel.NumberOfWaqSegmentLayers > 1)
            {
                foreach (KeyValuePair<string, IList<int>> layer0ObservationAreaLocations in obsInformation.ToArray())
                {
                    IList<int> layer0CellIds = layer0ObservationAreaLocations.Value;
                    var remainingIdsToAdd =
                        new List<int>((waterQualityModel.NumberOfWaqSegmentLayers - 1) * layer0CellIds.Count);
                    for (var i = 1; i < waterQualityModel.NumberOfWaqSegmentLayers; i++)
                    {
                        int[] cellsOnLayerI = layer0CellIds
                                              .Select(id => id + (i * waterQualityModel
                                                                      .NumberOfDelwaqSegmentsPerHydrodynamicLayer))
                                              .ToArray();
                        remainingIdsToAdd.AddRange(cellsOnLayerI);
                    }

                    obsInformation[layer0ObservationAreaLocations.Key].AddRange(remainingIdsToAdd);
                }
            }

            return obsInformation;
        }

        private static void AddOutputLocationsForObservationLocations(IDictionary<string, IList<int>> obsInformation,
                                                                      WaterQualityModel waterQualityModel)
        {
            if (waterQualityModel.ModelSettings.MonitoringOutputLevel == MonitoringOutputLevel.None ||
                waterQualityModel.ModelSettings.MonitoringOutputLevel == MonitoringOutputLevel.Areas)
            {
                return;
            }

            foreach (WaterQualityObservationPoint obsPoint in waterQualityModel.ObservationPoints)
            {
                int cellId = waterQualityModel.GetSegmentIndexForLocation2D(new Coordinate(obsPoint.X, obsPoint.Y));

                ObservationPointType observationPointType = obsPoint.ObservationPointType;
                SetObservationPointData(obsInformation, waterQualityModel, observationPointType, obsPoint, cellId);
            }
        }

        private static void SetObservationPointData(IDictionary<string, IList<int>> obsInformation, WaterQualityModel waterQualityModel, 
                                                    ObservationPointType observationPointType, WaterQualityObservationPoint obsPoint, int cellId)
        {
            switch (observationPointType)
            {
                case ObservationPointType.SinglePoint:
                {
                    // single points are written as
                    // 'name' 1 segment_number
                    obsInformation[obsPoint.Name] = new[]
                    {
                        waterQualityModel.GetSegmentIndexForLocation(obsPoint.Geometry.Coordinate)
                    };
                }
                    break;
                case ObservationPointType.Average:
                {
                    // column average points are written as
                    // 'name' num_layers segment1 segment2 ...
                    var cellSegments = new int[waterQualityModel.NumberOfWaqSegmentLayers];

                    for (var l = 0; l < waterQualityModel.NumberOfWaqSegmentLayers; l++)
                    {
                        cellSegments[l] =
                            cellId + (l * waterQualityModel.NumberOfDelwaqSegmentsPerHydrodynamicLayer);
                    }

                    obsInformation[obsPoint.Name] = cellSegments;
                }
                    break;
                case ObservationPointType.OneOnEachLayer:
                {
                    // on each layer points are written as
                    // 'name_L1' 1 segment_number1
                    // 'name_L2' 1 segment_number2
                    // ...
                    for (var l = 0; l < waterQualityModel.NumberOfWaqSegmentLayers; l++)
                    {
                        var obsName = $"{obsPoint.Name}_L{l + 1}";
                        obsInformation[obsName] = new[]
                        {
                            cellId + (l * waterQualityModel.NumberOfDelwaqSegmentsPerHydrodynamicLayer)
                        };
                    }
                }
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(observationPointType));
            }
        }
    }
}
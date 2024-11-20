using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public static class PartialSobekImporterBuilder
    {
        /// <summary>
        /// Builds a chain of SOBEK importers (using <see cref="PartialSobekImporter.IPartialSobekImporter.PartialSobekImporter"/>  property)
        /// </summary>
        /// <param name="sobekPath">Path to the SOBEK case or network file</param>
        /// <param name="targetObject">Object to import to</param>
        /// <param name="partialSobekImporters">IPartialSobekImporters to chain (ordered first to last)</param>
        /// <returns>Top PartialSobekImporter containing all <paramref name="partialSobekImporters"/> (can be null)</returns>
        public static IPartialSobekImporter BuildPartialSobekImporter(string sobekPath, object targetObject, IEnumerable<IPartialSobekImporter> partialSobekImporters)
        {
            IPartialSobekImporter previousSobekImporter = null;
            
            foreach (var sobekImporter in partialSobekImporters)
            {
                sobekImporter.PartialSobekImporter = previousSobekImporter;
                sobekImporter.PathSobek = sobekPath;
                sobekImporter.TargetObject = targetObject;

                previousSobekImporter = sobekImporter;
            }

            return previousSobekImporter;
        }

        /// <summary>
        /// Builds a chain of SOBEK importers needed for <param name="obj"/> type
        /// </summary>
        /// <param name="networkFilePath">Path to the SOBEK case or network file</param>
        /// <param name="obj">Object to import to</param>
        /// <returns>PartialSobekImporter containing all required sub partialSobekImporters (can be null)</returns>
        public static IPartialSobekImporter BuildPartialSobekImporter(string networkFilePath, object obj)
        {
            var region = obj as HydroRegion;
            if (region != null)
            {
                return BuildHydroRegionImporter(networkFilePath, region);
            }

            var network = obj as HydroNetwork;
            if (network != null)
            {
                return BuildHydroNetworkImporter(networkFilePath, network);
            }

            var basin = obj as IDrainageBasin;
            if (basin != null)
            {
                return BuildDrainageBasinImporter(networkFilePath, basin);
            }

            var hydroModel = obj as HydroModel;
            if (hydroModel != null)
            {
                return BuildHydroModelImporter(networkFilePath, hydroModel);
            }

            var waterFlowFMModel = obj as WaterFlowFMModel;
            if (waterFlowFMModel != null)
            {
                return BuildWaterFlowFMModelImporter(networkFilePath, waterFlowFMModel);
            }

            var realTimeControlModel = obj as RealTimeControlModel;
            if (realTimeControlModel != null)
            {
                return BuildRealTimeControlModelImporter(networkFilePath, realTimeControlModel);
            }

            var rainfallRunoffModel = obj as RainfallRunoffModel;
            if (rainfallRunoffModel != null)
            {
                return BuildRainfallRunoffModelImporter(networkFilePath, rainfallRunoffModel);
            }

            throw new NotImplementedException(String.Format("No partial sobekimporter has been found for object type {0}.", obj.GetType()));
        }
        private static IPartialSobekImporter BuildDrainageBasinImporter(string sobekPath, IDrainageBasin basin)
        {
            var importers = new List<IPartialSobekImporter>
            {
                new SobekRRDrainageBasinImporter(),
            };

            return BuildPartialSobekImporter(sobekPath, basin, importers);
        }
        private static IPartialSobekImporter BuildHydroNetworkImporter(string sobekPath, HydroNetwork network)
        {
            return BuildPartialSobekImporter(sobekPath, network, GetHydroNetworkImporters());
        }

        private static List<IPartialSobekImporter> GetHydroNetworkImporters()
        {
            var importers = new List<IPartialSobekImporter>
                {
                    new SobekBranchesImporter(),
                    new SobekCrossSectionsImporter(),
                    new SobekStructuresImporter(),
                    new SobekLateralSourcesImporter(),
                    new SobekRetentionImporter(),
                    new SobekLinkageNodeImporter(),
                };
            return importers;
        }
        private static IPartialSobekImporter BuildHydroModelImporter(string sobekPath, HydroModel hydroModel)
        {
            var importers = new List<IPartialSobekImporter>();

            if (hydroModel.Activities.OfType<WaterFlowFMModel>().Any())
            {
                importers.AddRange(GetWaterFlowFMModelImporters());
            }
            else if (hydroModel.Region.SubRegions.OfType<HydroNetwork>().Any())
            {
                importers.AddRange(GetHydroNetworkImporters());
            }
            if (hydroModel.Activities.OfType<RealTimeControlModel>().Any())
            {
                importers.AddRange(GetRealTimeControlModelImporters());
            }
            if (hydroModel.Activities.OfType<RainfallRunoffModel>().Any())
            {
                importers.AddRange(GetRainfallRunoffModelImporters());
            }

            return BuildPartialSobekImporter(sobekPath, hydroModel,
                importers.Distinct(new ImporterTypeComparer()).
                    ToList());
        }

        private static IPartialSobekImporter BuildHydroRegionImporter(string sobekPath, HydroRegion region)
        {
            var importers = new List<IPartialSobekImporter>
                                {
                                    new SobekBranchesImporter(),
                                    new SobekCrossSectionsImporter(),
                                    new SobekStructuresImporter(),
                                    new SobekLateralSourcesImporter(),
                                    new SobekRetentionImporter(),
                                    new SobekLinkageNodeImporter(),
                                    new SobekRRDrainageBasinImporter(),
                                };

            return BuildPartialSobekImporter(sobekPath, region, importers);
        }

        private static IPartialSobekImporter BuildWaterFlowFMModelImporter(string sobekPath, WaterFlowFMModel waterFlowFMModel)
        {
            return BuildPartialSobekImporter(sobekPath, waterFlowFMModel, GetWaterFlowFMModelImporters());
        }

        private static IPartialSobekImporter BuildRealTimeControlModelImporter(string sobekPath, RealTimeControlModel realTimeControlModel)
        {
            return BuildPartialSobekImporter(sobekPath, realTimeControlModel, GetRealTimeControlModelImporters());
        }

        public static IEnumerable<IPartialSobekImporter> GetRealTimeControlModelImporters()
        {
            return new[] { new SobekControllersTriggersImporter() };
        }

        private static IPartialSobekImporter BuildRainfallRunoffModelImporter(string sobekPath, RainfallRunoffModel rainfallRunoffModel)
        {
            return BuildPartialSobekImporter(sobekPath, rainfallRunoffModel, GetRainfallRunoffModelImporters());
        }

        public static IEnumerable<IPartialSobekImporter> GetWaterFlowFMModelImporters()
        {
            return new List<IPartialSobekImporter>
            {
                new SobekBranchesImporter(),
                new SobekCrossSectionsImporter(),
                new SobekStructuresImporter(),
                new SobekLateralSourcesImporter(),
                new SobekMeasurementStationsImporter(),
                new SobekRoughnessImporter(),
                new SobekInitialConditionsImporter(),
                new SobekBoundaryConditionsImporter(),
                new SobekLateralSourcesDataImporter(),
                new SobekComputationalGridImporter(),
                //new SobekLinkageNodeImporter(),//niet ondersteunt, het model moet eerst naar een recentere versie moet worden geconverteerd.
                new SobekRetentionImporter(),
                //new SobekSaltImporter(),
                new SobekSettingsImporter()

            };
        }

        public static IEnumerable<IPartialSobekImporter> GetRainfallRunoffModelImporters()
        {
            return new List<IPartialSobekImporter>
            {
                new SobekRRDrainageBasinImporter(),
                new SobekRRSettingsImporter(),
                new SobekRRMeteoDataImporter(),
                new SobekRRPavedImporter(),
                new SobekRRUnpavedImporter(),
                new SobekRRGreenhouseImporter(),
                new SobekRRSacramentoImporter(),
                new SobekRRBoundaryConditionsImporter(),
                new SobekRRNwrwImporter(),
                new SobekRROpenWaterImporter(),
                new SobekRRKasInitImporter()
            };
        }
    }
    internal class ImporterTypeComparer : IEqualityComparer<IPartialSobekImporter>
    {
        public bool Equals(IPartialSobekImporter x, IPartialSobekImporter y)
        {
            return x.GetType() == y.GetType();
        }

        public int GetHashCode(IPartialSobekImporter obj)
        {
            return obj.GetType().GetHashCode();
        }
    }
}

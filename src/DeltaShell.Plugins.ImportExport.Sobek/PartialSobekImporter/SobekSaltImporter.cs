using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekSaltImporter: PartialSobekImporterBase
    {
        //FM1D2D-579

        private static readonly ILog log = LogManager.GetLogger(typeof(SobekSaltImporter));
        private IDictionary<string, IBranch> branches;
        private readonly Dictionary<string, string> saltBoundaryConditionToFeatureLookup = new Dictionary<string, string>();

        private string displayName = "Salt";
        public override string DisplayName
        {
            get { return displayName; }
        }

        protected override void PartialImport()
        {
            var boundaryConditionsPath = GetFilePath(SobekFileNames.SobekBoundaryConditionsLocationsFileName);
            if (!File.Exists(boundaryConditionsPath))
            {
                log.ErrorFormat("Cancelling salt import: Sobek boundary conditions file {0} does not exist.", boundaryConditionsPath);
                return;
            }

            branches = HydroNetwork.Branches.ToDictionary(b => b.Name, b => b);
            SetLookUpDictionary(boundaryConditionsPath);

            ImportSaltBoundaries();
            ImportSaltLateralBoundaries();
            ImportSaltInitialConcentration();
            ImportSaltDispersion();

            // when salt data is found, then salt will be enabled, hence:
            if (!enableSaltInModelHasBeenSet)
            {
                log.Warn("Sobek model does not contain salt data, nothing to import.");
            }
        }

        private bool enableSaltInModelHasBeenSet = false;
        private void EnableSaltInModel()
        {
            if (enableSaltInModelHasBeenSet) return;

            enableSaltInModelHasBeenSet = true;
        }

        private void SetLookUpDictionary(string path)
        {
            var sobekBoundaryLocationReader = new SobekBoundaryLocationReader { SobekType = SobekType };

            var nodeType = SobekType == SobekType.SobekRE
                               ? SobekBoundaryLocationType.SaltNode //these still exist in SobekRE model input file
                               : SobekBoundaryLocationType.Node;

            foreach (var sobekBoundaryLocation in sobekBoundaryLocationReader.Read(path).Where( b => b.SobekBoundaryLocationType == nodeType))
            {
                saltBoundaryConditionToFeatureLookup[sobekBoundaryLocation.Id] = nodeType == SobekBoundaryLocationType.Node
                                                                                     ? sobekBoundaryLocation.Id
                                                                                     : sobekBoundaryLocation.ConnectionId;
            }
        }

        private void ImportSaltLateralBoundaries()
        {
            string path = GetFilePath(SobekFileNames.SobekSaltLateralBoundaryFileName);
            var waterFlowFmModel = GetModel<WaterFlowFMModel>();

            if (!File.Exists(path))
            {
                log.InfoFormat("Import of lateral salt conditions skipped, file {0} does not exist.", path);
                return;
            }

            var saltLateralBoundaries = new SaltLateralBoundaryReader().Read(path);
            var lateralSourceDataMapping = new Dictionary<IFeature, Model1DLateralSourceData>();
            var createdLateralSourceFeatures = HydroNetwork.LateralSources.ToDictionary(ls => ls.Name, ls => ls);

            foreach (var sobekSaltBoundary in saltLateralBoundaries)
            {

                EnableSaltInModel();

                var lateralId = (sobekSaltBoundary.SaltBoundaryType == SaltBoundaryType.DrySubstance)
                                     ? sobekSaltBoundary.Id
                                     : sobekSaltBoundary.LateralId;

                if (createdLateralSourceFeatures.ContainsKey(lateralId))
                {
                    var lateralSource = createdLateralSourceFeatures[lateralId];
                    var lateralSourceData = lateralSourceDataMapping[lateralSource];
                    switch (sobekSaltBoundary.SaltStorageType)
                    {
                        case SaltStorageType.Constant:
                            if (sobekSaltBoundary.SaltBoundaryType == SaltBoundaryType.DrySubstance)
                            {
                                lateralSourceData.SaltLateralDischargeType = SaltLateralDischargeType.MassConstant;
                                lateralSourceData.SaltConcentrationDischargeConstant = sobekSaltBoundary.DryLoadConst;
                                lateralSourceData.DataType = Model1DLateralDataType.FlowConstant;
                                lateralSourceData.Flow = 0.0;
                            }
                            else
                            {
                                lateralSourceData.SaltLateralDischargeType =
                                    SaltLateralDischargeType.ConcentrationConstant;
                                lateralSourceData.SaltConcentrationDischargeConstant =
                                    sobekSaltBoundary.ConcentrationConst;
                            }
                            break;
                        case SaltStorageType.FunctionOfTime:
                            if (sobekSaltBoundary.SaltBoundaryType == SaltBoundaryType.DrySubstance)
                            {
                                lateralSourceData.SaltLateralDischargeType = SaltLateralDischargeType.MassTimeSeries;
                                DataTableHelper.SetTableToFunction(
                                    sobekSaltBoundary.DryLoadTable, lateralSourceData.SaltMassTimeSeries);
                                lateralSourceData.DataType = Model1DLateralDataType.FlowConstant;
                                lateralSourceData.Flow = 0.0;
                            }
                            else
                            {
                                lateralSourceData.SaltLateralDischargeType =
                                    SaltLateralDischargeType.ConcentrationTimeSeries;
                                DataTableHelper.SetTableToFunction(
                                    sobekSaltBoundary.ConcentrationTable, lateralSourceData.SaltConcentrationTimeSeries);
                            }
                            break;
                    }
                }
            }
        }

        private void ImportSaltInitialConcentration()
        {
            var initialPath = GetFilePath(SobekFileNames.SobekSaltInitialConditionsFileName);
            var waterFlowFmModel = GetModel<WaterFlowFMModel>();

            if (!File.Exists(initialPath))
            {
                log.InfoFormat("Initial condition file [{0}] not found; skipping...", initialPath);
                return;
            }

            var saltInitial = new NetworkCoverage
            {
                Network = HydroNetwork,
                DefaultValue = 0.0
            };

            var saltInitialConditions = new InitalSaltConditionsReader().Read(initialPath);
            if (!saltInitialConditions.Any())
            {
                log.WarnFormat("No initial conditions for salt found in file {0}.", initialPath);
                return;
            }

            foreach (var saltInitialCondition in saltInitialConditions)
            {
                EnableSaltInModel();

                // wikipedia.org
                // chlorine 35.453 g/mol
                // Sodium 22.98976928 g/mol
                // Sodium chloride 58.443 g/mol
                var saltCorrectionFactor = (saltInitialCondition.SaltConcentrationType == SaltConcentrationType.Choride)
                                               ? (58.443 / 35.453)
                                               : 1.0;
                if (saltInitialCondition.IsGlobalDefinition)
                {
                    if (!saltInitialCondition.Salt.IsConstant)
                    {
                        log.WarnFormat("Only constant support for initial salt concentration (GLIN record in {0}).", initialPath);
                    }
                    saltInitial.DefaultValue = saltInitialCondition.Salt.Constant;
                }
                else
                {
                    if (branches.ContainsKey(saltInitialCondition.BranchId))
                    {
                        var branch = branches[saltInitialCondition.BranchId];
                    }
                    else
                    {
                        log.WarnFormat("Branch {0} for initial salt concentration has not been found", saltInitialCondition.BranchId);
                    }
                }
            }
            
        }

        private void ImportSaltBoundaries()
        {
            string path = GetFilePath(SobekFileNames.SobekSaltNodeBoundaryFileName);
            var waterFlowFmModel = GetModel<WaterFlowFMModel>();

            if (!File.Exists(path))
            {
                log.InfoFormat("Import of salt boundary conditions skipped, file {0} does not exist.", path);
                return;
            }
            
            var nodes = HydroNetwork.Nodes.ToDictionary(n => n.Name, n => n);
            var saltNodeBoundaries = new SaltNodeBoundaryReader().Read(path);

            foreach (var sobekSaltNodeBoundary in saltNodeBoundaries)
            {
                EnableSaltInModel();

                var featureId = GetSobekBoundaryNodeId(sobekSaltNodeBoundary.Id);

                if (featureId == null)
                {
                    log.WarnFormat("Can't find node for salt boundary condition {0}, skipping node ...", sobekSaltNodeBoundary.Id);
                    continue;
                }
                if (!nodes.ContainsKey(featureId))
                {
                    log.WarnFormat("Unable to link salt boundary {0} to unknown node {1}.", sobekSaltNodeBoundary.Id, featureId);
                    continue;
                }

                var node = nodes[featureId];
                var boundaryNodeData = waterFlowFmModel.BoundaryConditions.FirstOrDefault(bc => bc.Feature == node);
                if (boundaryNodeData == null)
                {
                    log.ErrorFormat("No boundary data for node {0}", featureId);
                    continue;
                }

                if (SaltBoundaryNodeType.ZeroFlux == sobekSaltNodeBoundary.SaltBoundaryNodeType)
                {
                    //boundaryNodeData.SaltConditionType = SaltBoundaryConditionType.None;
                }
                else
                {
                    // switch (sobekSaltNodeBoundary.SaltStorageType)
                    // {
                    //     case SaltStorageType.Constant:
                    //         boundaryNodeData.SaltConcentrationConstant = sobekSaltNodeBoundary.ConcentrationConst;
                    //         break;
                    //     case SaltStorageType.FunctionOfTime:
                    //         boundaryNodeData.SaltConditionType = SaltBoundaryConditionType.TimeDependent;
                    //         DataTableHelper.SetTableToFunction(
                    //             sobekSaltNodeBoundary.ConcentrationTable, boundaryNodeData.SaltConcentrationTimeSeries);
                    //         break;
                    // }
                    //
                    // boundaryNodeData.ThatcherHarlemannCoefficient = sobekSaltNodeBoundary.TimeLag;
                }
            }
        }

        private string GetSobekBoundaryNodeId(string boundaryId)
        {
            return saltBoundaryConditionToFeatureLookup.ContainsKey(boundaryId) ? saltBoundaryConditionToFeatureLookup[boundaryId] : null;
        }

        private void ImportSaltDispersion()
        {
            var initialPath = GetFilePath(SobekFileNames.SobekSaltGlobalDispersionFileName);
            var waterFlowFmModel = GetModel<WaterFlowFMModel>();

            if (!File.Exists(initialPath))
            {
                log.InfoFormat("Salt global dispersion file [{0}] not found; skipping...", initialPath);
                return;
            }

            var globalDispersion = new SobekSaltGlobalDispersionReader().Read(initialPath).FirstOrDefault();
            if (globalDispersion == null)
            {
                log.WarnFormat("No global salt dispersion found in file {0}.", initialPath);
                return;
            }

            switch (globalDispersion.DispersionOptionType)
            {
                case DispersionOptionType.Option1:
                case DispersionOptionType.ThatcherHarlemann:
                    break;
                case DispersionOptionType.Option2:
                case DispersionOptionType.Empirical:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            initialPath = GetFilePath(SobekFileNames.SobekSaltLocalDispersionFileName);
            if (!File.Exists(initialPath))
            {
                log.WarnFormat("Salt local dispersion file [{0}] not found; skipping...", initialPath);
                return;
            }

            // the globalDispersion.DispersionOptionType is also used for local dispersion -> if not supported setting default is enough
            var localDispersionImproved = new SobekSaltLocalDispersionReader().Read(initialPath);
            if (!localDispersionImproved.Any())
            {
                log.WarnFormat("No local salt dispersion found in file {0}.", initialPath);
                return;
            }

            EnableSaltInModel();

            foreach (var sobekSaltLocalDispersion in localDispersionImproved)
            {
                if (!branches.ContainsKey(sobekSaltLocalDispersion.BranchId))
                {
                    log.ErrorFormat("Branch {0} not found, dispersion for this branch ignored.", sobekSaltLocalDispersion.BranchId);
                    continue;
                }
                var branch = branches[sobekSaltLocalDispersion.BranchId];

            }
        }
    }
}

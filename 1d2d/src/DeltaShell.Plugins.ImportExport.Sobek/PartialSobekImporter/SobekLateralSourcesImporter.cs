using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekLateralSourcesImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekLateralSourcesImporter));

        private string displayName = "Lateral sources";

        public override string DisplayName
        {
            get { return displayName; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

        protected override void PartialImport()
        {
            string path = GetFilePath(SobekFileNames.SobekBoundaryConditionsLocationsFileName);
            if (!File.Exists(path))
            {
                return;
            }

            var sobekBoundaryLocationReader = new SobekBoundaryLocationReader { SobekType = SobekType };
            var branches = HydroNetwork.Branches.ToDictionary(b => b.Name, b => b);
            var nodes = HydroNetwork.Nodes.ToDictionary(n => n.Name, n => n);

            var lateralsAtNode = new List<SobekBoundaryLocation>();
            var lateralSourcesToAdd = new List<LateralSource>();

            foreach (var sobekBoundaryLocation in sobekBoundaryLocationReader.Read(path))
            {
                // mapping from NETWORK.CN/DEFCND.1 file
                if ((sobekBoundaryLocation.SobekBoundaryLocationType == SobekBoundaryLocationType.Branch) ||
                    (sobekBoundaryLocation.SobekBoundaryLocationType == SobekBoundaryLocationType.SaltLateral) || // valid for Sobek RE only
                    (sobekBoundaryLocation.SobekBoundaryLocationType == SobekBoundaryLocationType.Diffuse))
                {
                    // FLBR record
                    if (!branches.ContainsKey(sobekBoundaryLocation.ConnectionId))
                    {
                        log.ErrorFormat("Unable to connect lateral boundary {0} to branch with id {1}; skipped.",
                                        sobekBoundaryLocation.Id, sobekBoundaryLocation.ConnectionId);
                        continue;
                    }
                    if (Comparer.AlmostEqual2sComplement(sobekBoundaryLocation.Offset, 9.9999e+009))
                    {
                        // lateral source has no location; used in SobekRe for salt concentration where the lateral added
                        // to an existing flow lateral.
                        // ignore, but do not log, it is correct behavior
                        continue;
                    }

                    IBranch branch = branches[sobekBoundaryLocation.ConnectionId];
                    var offset = sobekBoundaryLocation.Offset;

                    if (offset > branch.Length)
                    {
                        log.ErrorFormat("The chainage of lateral source '{0} - {1}' is outside of the branch length. The chainage has been set from {2} to {3}.", sobekBoundaryLocation.Id, sobekBoundaryLocation.Name, offset, branch.Length);
                        offset = branch.Length;
                    }

                    offset = MoveLateralToNodeForSewerSystems(branch, offset);

                    LateralSource lateralSource = new LateralSource
                    {
                        Branch = branch,
                        Name = sobekBoundaryLocation.Id,
                        LongName = sobekBoundaryLocation.Name,
                        Chainage = offset
                    };
                    //in SOBEK 2.12 a diffuse lateral source has lateral discharge over complete length of the reach 
                    if (sobekBoundaryLocation.SobekBoundaryLocationType == SobekBoundaryLocationType.Diffuse)
                    {
                        //diffuse lateral sources get a ls node per computational point of the branch
                        //it's handy to give the lateral source the branch name for two purposes:
                        //1. AddOrReplaceLateralSource will do a replace so we only have one diffuse lateral source and not for each calculation point
                        //2, The Lateral source data for diffuse laterals will match with the lateral source name (testbench 272)
                        lateralSource.Name = branch.Name;
                        lateralSource.Chainage = 0.0;
                        lateralSource.Length = lateralSource.Branch.Length;
                        lateralSource.Geometry = (IGeometry)lateralSource.Branch.Geometry.Clone();
                    }
                    else
                    {
                        lateralSource.Geometry = GeometryHelper.GetPointGeometry(branch, lateralSource.Chainage);
                    }
                    lateralSourcesToAdd.Add(lateralSource);
                }
                else if (sobekBoundaryLocation.SobekBoundaryLocationType == SobekBoundaryLocationType.LateralAtNode)
                {
                    // process these later
                    lateralsAtNode.Add(sobekBoundaryLocation);
                }
            }

            var lateralSources = HydroNetwork.LateralSources.ToDictionary(ls => ls.Name, ls => ls);
            
            var model = TryGetModel<WaterFlowFMModel>();
            model.DoWithPropertySet(nameof(model.DisableNetworkSynchronization), true, () =>
            {
                foreach (var lateralSource in lateralSourcesToAdd)
                {
                    AddOrReplaceLateralSource(lateralSource, lateralSources);
                }

                model.AddMissingLateralSourceData(HydroNetwork.LateralSources.OfType<LateralSource>());
            
                // now process the laterals at nodes; in SOBEK 3.x we do not use a separate type for this. However, in theory this could introduce multiple laterals with identical id
                // We give preference to importing 'laterals at branches'. If after importing these 'laterals at nodes' exist that have identical ids they are ignored
                var lateralsAdded = new List<LateralSource>();
                foreach (var lateralAtNode in lateralsAtNode)
                {
                    if (!nodes.ContainsKey(lateralAtNode.Id))
                    {
                        continue;
                    }

                    if (lateralSources.ContainsKey(lateralAtNode.Id))
                    {
                        log.ErrorFormat("Lateral source (at node '{0}') with id '{1}' and name '{2}' already exists and cannot be added.", nodes[lateralAtNode.Id], lateralAtNode.Id, lateralAtNode.Name);
                        continue;
                    }

                    INode node = nodes[lateralAtNode.Id];

                    var lateralSource = new LateralSource
                    {
                        Name = lateralAtNode.Id,
                        LongName = lateralAtNode.Name,
                        Network = node.Network,
                        Geometry = (IGeometry) node.Geometry.Clone()
                    };

                    if (node.OutgoingBranches.Count > 0)
                    {
                        NetworkHelper.AddBranchFeatureToBranch(lateralSource, node.OutgoingBranches[0], 0);
                    }
                    else if (node.IncomingBranches.Count > 0)
                    {
                        NetworkHelper.AddBranchFeatureToBranch(lateralSource, node.IncomingBranches[0], node.IncomingBranches[0].Length);
                    }

                    lateralSources[lateralSource.Name] = lateralSource;
                    lateralsAdded.Add(lateralSource);
                }

                model?.AddMissingLateralSourceData(lateralsAdded);
                NamingHelper.MakeNamesUnique(HydroNetwork.LateralSources);
            });
        }

        /// <summary>
        /// In sewer system we've laterals on the nodes
        /// If sewer system move node to the end of the pipe so it will treated as a lateral on the node
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private double MoveLateralToNodeForSewerSystems(IBranch branch, double offset)
        {
            return branch is IPipe ? branch.Length : offset;
        }

        private void AddOrReplaceLateralSource(ILateralSource lateralSource, Dictionary<string, ILateralSource> lateralSources)
        {
            var branch = lateralSource.Branch;

            if (lateralSources.ContainsKey(lateralSource.Name))
            {
                var targetLateralSource = lateralSources[lateralSource.Name];

                targetLateralSource.CopyFrom(lateralSource);
                targetLateralSource.Chainage = lateralSource.Chainage;
                targetLateralSource.Geometry = lateralSource.Geometry;

                if (targetLateralSource.Branch != branch)
                {
                    targetLateralSource.Branch.BranchFeatures.Remove(targetLateralSource);
                    NetworkHelper.AddBranchFeatureToBranch(targetLateralSource, branch, lateralSource.Chainage);
                }

                return;
            }

            NetworkHelper.AddBranchFeatureToBranch(lateralSource, branch, lateralSource.Chainage);
            lateralSources[lateralSource.Name] = lateralSource;
        }
    }
}

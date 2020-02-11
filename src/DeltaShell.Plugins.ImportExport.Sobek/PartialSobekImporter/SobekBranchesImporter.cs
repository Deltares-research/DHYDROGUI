using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.Sobek.Readers;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class  SobekBranchesImporter : PartialSobekImporterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekBranchesImporter));

        public override string DisplayName
        {
            get { return "Branches and nodes"; }
        }

        protected override void PartialImport()
        {
            var networkNetterPath = GetFilePath(SobekFileNames.SobekNetworkNetterFileName);
            var nodesPath = GetFilePath(SobekFileNames.SobekNodeFileName);
            var networkPath = GetFilePath(SobekFileNames.SobekNetworkFileName);
            var networkBranchGeometryPath = GetFilePath(SobekFileNames.SobekNetworkBrancheGeometryFileName);

            IDictionary<string,int> nodeTypes = new Dictionary<string, int>();
            IDictionary<string, int> branchTypes = new Dictionary<string, int>();

            if (File.Exists(networkNetterPath))
            {
                nodeTypes = SobekNetworkNetterReader.ReadNodeTypes(networkNetterPath);
                branchTypes = SobekNetworkNetterReader.ReadBranchTypes(networkNetterPath);
            }
            else
            {
                Log.WarnFormat("Couldn't find file {0} for detailed branches and nodes information.", networkNetterPath);
            }

            var nodeDataReader = new SobekRetentionsReader() { Sobek2Import = true };
            var nodeData = nodeDataReader.Read(nodesPath).Cast<Retention>()
                .ToDictionaryWithErrorDetails(nodesPath, c => c.Name);

            var createdNodes = new SobekNetworkNodeFileReader().Read(networkPath)
                .Concat(new SobekNetworkLinkageNodeFileReader().Read(networkPath).Cast<SobekNode>())
                .ToDictionaryWithErrorDetails(networkPath, n => n.ID, n => CreateHydroNodeOrManhole(n, nodeTypes, nodeData, HydroNetwork));

            var createdBranches = new SobekNetworkBranchFileReader().Read(networkPath)
                                .ToDictionaryWithErrorDetails(networkPath, b => b.TextID, b => CreateBranchOrPipe(b, createdNodes, branchTypes, HydroNetwork));

            ReadAndUpdateBranchGeometry(networkBranchGeometryPath, createdBranches, SobekType);

            var nodesLookup = HydroNetwork.Nodes.ToDictionary(n => n.Name);
            var branchesLookUp = HydroNetwork.Branches.ToDictionary(b => b.Name);

            AddToNetwork(createdNodes, nodesLookup, HydroNetwork);
            AddToNetwork(createdBranches, branchesLookUp, nodesLookup, HydroNetwork);


            ReadAndUpdateBranchOrderNumber(nodesPath, HydroNetwork, branchesLookUp);
        }

        private static void AddToNetwork(IDictionary<string, IBranch> createdChannels, IDictionary<string, IBranch> branchesLookUp, IDictionary<string, INode> nodesLookup, INetwork hydroNetwork)
        {
            foreach (var channel in createdChannels.Values)
            {
                if (branchesLookUp.ContainsKey(channel.Name))
                {
                    var existingBranch = branchesLookUp[channel.Name];
                    existingBranch.Geometry = channel.Geometry;
                    existingBranch.Length = channel.Length;
                    existingBranch.IsLengthCustom = channel.IsLengthCustom;
                    existingBranch.Source = nodesLookup[channel.Source.Name];
                    existingBranch.Target = nodesLookup[channel.Target.Name];
                }
                else
                {
                    branchesLookUp[channel.Name] = channel;
                    hydroNetwork.Branches.Add(channel);
                }
            }
        }

        private static void AddToNetwork(IDictionary<string, INode> createdNodes, IDictionary<string, INode> nodesLookup, INetwork hydroNetwork)
        {
            foreach (var node in createdNodes.Values)
            {
                if (nodesLookup.ContainsKey(node.Name))
                {
                    var existingNode = nodesLookup[node.Name];
                    existingNode.Geometry = node.Geometry;
                }
                else
                {
                    nodesLookup[node.Name] = node;
                    hydroNetwork.Nodes.Add(node);
                }
            }
        }

        /// <summary>
        /// Updates branches that where read from network.tp with geometry as stored in network.cp
        /// </summary>
        private static void ReadAndUpdateBranchGeometry(string filePath, IDictionary<string, IBranch> createdChannels, SobekType sobekType)
        {
            ThrowWhenFileNotExist(filePath);

            //update geometry of the branch
            var branchGeometryLookUp = new SobekCurvePointReader().Read(filePath).ToDictionaryWithErrorDetails(filePath,g => g.BranchID);

            foreach (var branchId in createdChannels.Keys)
            {
                var branch = createdChannels[branchId] as IChannel;

                if (branch == null) continue;//pipe has a geometry based on source and target node, so skip

                var branchGeometry = branchGeometryLookUp.ContainsKey(branchId)
                                         ? branchGeometryLookUp[branchId]
                                         : null;

                branch.Geometry = GeometryHelper.CalculateGeometry(sobekType == SobekType.Sobek212, branchGeometry, branch.Source, branch.Target);

                if (!branch.IsLengthCustom) continue;

                var difference = Math.Abs(branch.Length - branch.Geometry.Length);
                if (difference < BranchFeature.Epsilon) continue;

                var percentage = branch.Geometry.Length > 0 ? 100 * (difference / branch.Geometry.Length) : 100.0;
                if (percentage < 0.01) continue;

                Log.WarnFormat(
                    "Branch {0} - {1} has been imported with a difference of {2} between length {3} and geometry length {4} ({5} %)",
                    branch.Name, branch.LongName, difference.ToString("G4"), branch.Length.ToString("G4"),
                    branch.Geometry.Length.ToString("G4"), percentage.ToString("N2"));
            }
        }
    
        private static IBranch CreateBranchOrPipe(SobekBranch sobekBranch, Dictionary<string, INode> createdNodes,
            IDictionary<string, int> branchTypes, INetwork hydroNetwork)
        {
            var fromNode = createdNodes[sobekBranch.StartNodeID];
            var toNode = createdNodes[sobekBranch.EndNodeID];

            var fromManhole = fromNode as Manhole;
            var toManhole = toNode as Manhole;

            var netterBranchName = sobekBranch.StartNodeID + "-" + sobekBranch.EndNodeID;
            var bFoundNetterBranchName = branchTypes.ContainsKey(netterBranchName);
            if (!bFoundNetterBranchName) netterBranchName = sobekBranch.StartNodeID + "_" + sobekBranch.EndNodeID; // if nodeName contains "-", "_" is used

            if (bFoundNetterBranchName || branchTypes.ContainsKey(netterBranchName))
            {
                var branchType = branchTypes[netterBranchName];
                if (SobekNetworkNetterReader.IsPreasurePipe(branchType)) //is a preasure pipe / sewer connection 
                {
                    var sewerConnection = new SewerConnection(sobekBranch.TextID)
                    {
                        LongName = sobekBranch.Name,
                        Network = hydroNetwork,
                        IsLengthCustom = true,
                        Length = sobekBranch.Length,
                        Geometry = new LineString(
                            new[] {fromNode.Geometry.Coordinate, toNode.Geometry.Coordinate}),
                        Source = fromNode,
                        Target = toNode,
                        SourceCompartment = fromManhole?.Compartments?.FirstOrDefault(),
                        TargetCompartment = toManhole?.Compartments?.FirstOrDefault()
                    };
                    return sewerConnection;
                }

                if (SobekNetworkNetterReader.IsPipe(branchType)) //is a pipe 
                {
                    var pipe = new Pipe
                    {
                        Name = sobekBranch.TextID,
                        LongName = sobekBranch.Name,
                        Network = hydroNetwork,
                        IsLengthCustom = true,
                        Length = sobekBranch.Length,
                        Geometry = new LineString(new[] {fromNode.Geometry.Coordinate, toNode.Geometry.Coordinate}),
                        Material = SewerProfileMapping.SewerProfileMaterial.Unknown,
                        Source = fromNode,
                        Target = toNode,
                        SourceCompartment = fromManhole?.Compartments?.FirstOrDefault(),
                        TargetCompartment = toManhole?.Compartments?.FirstOrDefault()
                    };

                    if (SobekNetworkNetterReader.IsDryWeatherPipe(branchType))
                    {
                        pipe.WaterType = SewerConnectionWaterType.DryWater;
                    }
                    else if (SobekNetworkNetterReader.IsStormWeatherPipe(branchType))
                    {
                        pipe.WaterType = SewerConnectionWaterType.StormWater;
                    }
                    else
                    {
                        pipe.WaterType = SewerConnectionWaterType.Combined;
                    }

                    return pipe;
                }
            }

            return new Channel(fromNode, toNode, sobekBranch.Length)
            {
                Name = sobekBranch.TextID,
                LongName = sobekBranch.Name,
                Network = hydroNetwork,
                IsLengthCustom = true
            };
        }

        private static INode CreateHydroNodeOrManhole(SobekNode sobekNode, IDictionary<string, int> nodeTypes,
            Dictionary<string, Retention> nodeData,
            INetwork hydroNetwork)
        {

            //manholes??
            if (nodeTypes.ContainsKey(sobekNode.ID))
            {
                if (SobekNetworkNetterReader.IsManhole(nodeTypes[sobekNode.ID]))
                {
                    var manhole = CreateManholeWithOneCompartment(sobekNode);

                    if (nodeData.ContainsKey(sobekNode.ID))
                    {
                        UpdateManholeWithNodeData(manhole, nodeData[sobekNode.ID]);
                    }

                    return manhole;
                }
            }

            return new HydroNode
            {
                Name = sobekNode.ID,
                LongName = sobekNode.Name,
                Geometry = new Point(sobekNode.X, sobekNode.Y),
                Network = hydroNetwork
            };
        }

        private static IManhole CreateManholeWithOneCompartment(SobekNode sobekNode)
        {
            var compartment = new Compartment
            {
                Name = sobekNode.ID,
                Geometry = new Point(sobekNode.X, sobekNode.Y)
            };
            var manhole = new Manhole(sobekNode.ID);
            manhole.Geometry = new Point(sobekNode.X, sobekNode.Y);
            manhole.Compartments.Add(compartment);
            compartment.ParentManhole = manhole;

            return manhole;
        }
        private static void UpdateManholeWithNodeData(IManhole manhole, Retention retention)
        {
            var dimension = 0.0;
            if (retention.StorageArea > 0.0) dimension = Math.Pow(retention.StorageArea, 0.5);

            var compartment = manhole.Compartments.FirstOrDefault(c => c.Name == manhole.Name);
            if (compartment != null)
            {
                compartment.Name = retention.Name;
                compartment.ManholeLength = dimension;
                compartment.ManholeWidth = dimension;
                compartment.Shape = CompartmentShape.Square;
                compartment.BottomLevel = retention.BedLevel;
                compartment.FloodableArea = retention.StreetStorageArea;
                compartment.SurfaceLevel = retention.StreetLevel;
                //retention.StreetStorage,
                //retention.WellStorage,
            }
        }

        /// <summary>
        /// Updates ordernumber branches based on nodes.dat Sobek 212.4 - 213 files
        /// </summary>
        private static void ReadAndUpdateBranchOrderNumber(string filePath, INetwork hydroNetwork, IDictionary<string, IBranch> branchesLookup)
        {
            if (!File.Exists(filePath))
            {
                Log.ErrorFormat("Couldn't find file {0}.", filePath);
                return;
            }

            var nodesWithInterpolation = new SobekNodeFileReader().Read(filePath)
                                                        .Where(n => n.InterpolationOverNode)
                                                        .ToDictionaryWithErrorDetails(filePath, n => n.ID);

            var orderNumbers = hydroNetwork.Branches.Select(b => b.OrderNumber).Where(n => n > 0).ToList();
            var highestOrderNumber = orderNumbers.Any() ? orderNumbers.Max() : 0;

            var branchesPast = new HashSet<string>();

            foreach (var nodeId in nodesWithInterpolation.Keys)
            {
                var chainOfBranches = GetChainOfBranches(nodeId, branchesPast, nodesWithInterpolation, branchesLookup).ToList();
                if (chainOfBranches.Count <= 1) continue;

                highestOrderNumber++;
                foreach (var branch in chainOfBranches)
                {
                    branch.OrderNumber = highestOrderNumber;
                }
            }
        }

        private static IEnumerable<IBranch> GetChainOfBranches(string nodeId, HashSet<string> branchesPast, Dictionary<string, SobekNode> dicNodes, IDictionary<string, IBranch> dicBranches)
        {
            var node = dicNodes[nodeId];

            if (!branchesPast.Contains(node.InterpolationFrom) && dicBranches.ContainsKey(node.InterpolationFrom))
            {
                branchesPast.Add(node.InterpolationFrom);
                yield return dicBranches[node.InterpolationFrom];
                var previousNode = dicNodes.Values.FirstOrDefault(n => n.InterpolationTo == node.InterpolationFrom);
                if (previousNode != null)
                {
                    foreach (var branch in GetChainOfBranches(previousNode.ID, branchesPast, dicNodes, dicBranches))
                    {
                        yield return branch;
                    }
                }
            }

            if (!branchesPast.Contains(node.InterpolationTo) && dicBranches.ContainsKey(node.InterpolationTo))
            {
                branchesPast.Add(node.InterpolationTo);
                yield return dicBranches[node.InterpolationTo];

                var nextNode = dicNodes.Values.FirstOrDefault(n => n.InterpolationFrom == node.InterpolationTo);
                if (nextNode != null)
                {
                    foreach (var branch in GetChainOfBranches(nextNode.ID, branchesPast, dicNodes, dicBranches))
                    {
                        yield return branch;
                    }
                }
            }
        }
    }
}

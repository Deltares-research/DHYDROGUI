using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Actions;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DelftTools.Hydro.Helpers
{
    /// <summary>
    /// Utility class to work with networks
    /// </summary>
    public class HydroNetworkHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroNetworkHelper));

        public static void UpdateChannelNames(IChannel splittedChannel, IChannel newChannel) 
        {
            newChannel.Name = splittedChannel.Name + "_B";
            splittedChannel.Name = splittedChannel.Name + "_A";

            if (!String.IsNullOrEmpty(splittedChannel.LongName))
            {
                newChannel.LongName = splittedChannel.LongName + "_B";
                splittedChannel.LongName = splittedChannel.LongName + "_A";
            }
        }

        /// <summary>
        /// Splits a channel at the given chainage. 
        /// All channel features are updated. Renames using _A (for old) ,and _B for new
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="geometryOffset"></param>
        public static IHydroNode SplitChannelAtNode(IChannel channel, double geometryOffset)
        {
            if (geometryOffset != channel.Geometry.Length && geometryOffset != 0)
            {
                var channelSplitAction = new BranchSplitAction();
                channel.Network.BeginEdit(channelSplitAction);

                var result = NetworkHelper.SplitBranchAtNode(channel, geometryOffset);

                UpdateChannelNames(channel, (IChannel) result.NewBranch);

                //update the action before calling endedit..other entities might use the data.
                channelSplitAction.SplittedBranch = channel;
                channelSplitAction.NewBranch = result.NewBranch;

                channel.Network.EndEdit();
                return (IHydroNode) result.NewNode;
            }
            return null;
        }
        public static IManhole SplitPipeAtNode(IPipe pipe, double geometryOffset)
        {
            if (geometryOffset != pipe.Geometry.Length && geometryOffset != 0)
            {
                var channelSplitAction = new BranchSplitAction();
                pipe.Network.BeginEdit(channelSplitAction);

                var result = NetworkHelper.SplitBranchAtNode(pipe, geometryOffset);
                result.NewBranch.Name = pipe.Name + "_A";
                pipe.Name = pipe.Name + "_B";

                (result.NewBranch as IPipe)?.SetPipeProperties(pipe.Network as HydroNetwork);
                //update the action before calling endedit..other entities might use the data.
                channelSplitAction.SplittedBranch = pipe;
                channelSplitAction.NewBranch = result.NewBranch;

                pipe.Network.EndEdit();
                return (IManhole) result.NewNode;
            }
            return null;
        }
        
        public static Route AddNewRouteToNetwork(IHydroNetwork network)
        {
            var route = new Route
            {
                Name = "route_" + GetAvailableRouteNumber(network)
            };

            network.BeginEdit(new DefaultEditAction("Add new route to network"));
            network.Routes.Add(route);
            route.Network = network;
            network.EndEdit();

            return route;
        }

        public static void RemoveRoute(Route route)
        {
            var hydroNetwork = (IHydroNetwork)route.Network;
            hydroNetwork.BeginEdit(new DefaultEditAction("Delete feature " + route.Name));
            hydroNetwork.Routes.Remove(route);
            hydroNetwork.EndEdit();
        }

        private static int GetAvailableRouteNumber(IHydroNetwork network)
        {
            int lastNr = 0;

            foreach (var route in network.Routes.Reverse())
            {
                try
                {
                    lastNr = Int32.Parse(route.Name.Split('_')[1]);
                    break;
                }
                catch (Exception)
                {
                    //don't do anything: exception on split or on parse: non standard name
                }
            }

            return lastNr + 1;
        }

        /// <summary>
        /// Splits a branch at the given coordinate. 
        /// All branch features are updated
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="coordinate"></param>
        public static IHydroNode SplitChannelAtNode(IChannel branch, Coordinate coordinate)
        {
            var lengthIndexedLine = new LengthIndexedLine(branch.Geometry);
            double offset = lengthIndexedLine.Project(coordinate);
            return SplitChannelAtNode(branch, offset);
        }

        /// <summary>
        /// Splits a branch at the given node and connect resulting 2 branches to the node. 
        /// All related branch features are moved to the corresponding branch based on their geometry.
        /// </summary>
        /// <param name="network"></param>
        /// <param name="branch"></param>
        /// <param name="node"></param>
        public static IChannel SplitChannelAtNode(INetwork network, IChannel branch, INode node)
        {
            var channelSplitAction = new BranchSplitAction();

            var isEditing = branch.Network.IsEditing;
            if (!isEditing)
            {
                branch.Network.BeginEdit(channelSplitAction);
            }

            var newBranch = NetworkHelper.SplitBranchAtNode(network, branch, node);
            
            channelSplitAction.SplittedBranch = branch;
            channelSplitAction.NewBranch = newBranch;

            if (!isEditing)
            {
                branch.Network.EndEdit();
            }

            return (IChannel)newBranch;
        }

        /// <summary>
        /// Returns the number of networklocation in a coverage for a branch
        /// </summary>
        /// <param name="networkCoverage"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        private static int BranchLocationCount(INetworkCoverage networkCoverage, IChannel branch)
        {
            return networkCoverage.Locations.Values.Where(nl => nl.Branch == branch).Count();
        }

        /// <summary>
        /// This method generates cross sections out of the network pipes. We do this, such that we can use the static method
        /// LocationFileWriter.WriteFileCrossSectionLocations. Through the generated cross section, we can get to the pipes and
        /// generate two DelftIniCategory objects for every one of the pipes. One with chainage equal to zero and one with the
        /// chainage equal to the length of the pipe.
        /// </summary>
        /// <param name="network">The model network</param>
        /// <returns></returns>
        public static IEnumerable<CrossSection> GeneratePipeCrossSections(IHydroNetwork network)
        {
            return network.Pipes.Select(pipe => new CrossSection(pipe.CrossSection?.Definition) { Branch = pipe });
        }

        ///<summary>
        ///</summary>
        ///<param name="networkCoverage"></param>
        ///<param name="branch"></param>
        ///<param name="offsets"></param>
        public static void GenerateDiscretization(INetworkCoverage networkCoverage, IChannel branch, IEnumerable<double> offsets)
        {
            NetworkHelper.ClearLocations(networkCoverage, branch);
            var locations = offsets.Select(offset => new NetworkLocation(branch, offset)).ToList();
            networkCoverage.Locations.AddValues(locations);
        }

        public static void GenerateDiscretization(IDiscretization discretization,
                                                  bool overWriteSegments, bool eraseExisting,
                                                  double minimumCellLength, bool gridAtStructure,
                                                  double structureDistance, bool gridAtCrossSection,
                                                  bool gridAtLaterals,
                                                  bool gridAtFixedLength, double fixedLength,
                                                  IList<IChannel> selectedChannels=null)
        {
            if (!(discretization.Network is IHydroNetwork hydroNetwork))
            {
                log.Error("Could not find network to generate grid.");
                return;
            }

            discretization.Locations.SkipUniqueValuesCheck = true;
            discretization.SegmentGenerationMethod = SegmentGenerationMethod.None;

            selectedChannels = selectedChannels ?? hydroNetwork.Channels.ToList();
            
            foreach (var channel in selectedChannels)
            {
                if (BranchLocationCount(discretization, channel) > 1 && !overWriteSegments)
                {
                    continue;
                }

                if (eraseExisting)
                {
                    NetworkHelper.ClearLocations(discretization, channel);
                }
                else
                {
                    GenerateDiscretization(discretization, channel,
                                           minimumCellLength,
                                           gridAtStructure,
                                           structureDistance,
                                           gridAtCrossSection,
                                           gridAtLaterals,
                                           gridAtFixedLength,
                                           fixedLength);
                }
            }
            discretization.UpdateNetworkLocations(discretization.Locations.GetValues<INetworkLocation>().ToArray(), false);
            
            discretization.SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered;
            discretization.Locations.SkipUniqueValuesCheck = false;
        }

        /// <summary>
        /// Generates calculation grid cells for a branch, When the grid is generated at cross sections and at fixed 
        /// length each subbranch between two cross sections processed separately to prevent too small grid cells.
        /// </summary>
        /// <param name="discretization"></param>
        /// <param name="branch"></param>
        /// <param name="minimumCellLength"></param>
        /// <param name="gridAtStructure"></param>
        /// <param name="structureDistance"></param>
        /// <param name="gridAtCrossSection"></param>
        /// <param name="gridAtLaterals"></param>
        /// <param name="gridAtFixedLength"></param>
        /// <param name="fixedLength"></param>
        public static void GenerateDiscretization(IDiscretization discretization, IChannel branch, 
                                                  double minimumCellLength, bool gridAtStructure, double structureDistance, bool gridAtCrossSection,
                                                  bool gridAtLaterals, bool gridAtFixedLength, double fixedLength)
        {
            var offsets = new List<double> { 0.0 };

            // remember network locations the user has fixed.
            var existingLocations = discretization.Locations.Values.Where(nl => nl.Branch == branch).ToArray();
            var fixedOffsets = (from networkLocation in existingLocations
                                where discretization.IsFixedPoint(networkLocation)
                                select networkLocation.Chainage).ToList();
            offsets.AddRange(fixedOffsets);
            var length = branch.Length;
            offsets.Add(length);
            offsets = offsets.Distinct().ToList();

            if (gridAtStructure)
            {
                AddGridChainageForCompositeStructures(branch, minimumCellLength, structureDistance, offsets);
            }

            if (gridAtCrossSection)
            {
                AddGridChainagesForBranchFeatures(branch, minimumCellLength, offsets, branch.CrossSections.OfType<IBranchFeature>());
            }

            if (gridAtLaterals)
            {
                AddGridChainagesForBranchFeatures(branch, minimumCellLength, offsets, branch.BranchSources.OfType<IBranchFeature>());
            }

            AddGridChainagesAtFixedIntervals(offsets, gridAtFixedLength, fixedLength);
            GenerateDiscretization(discretization, branch, offsets);
            var networkLocations = discretization.Locations.Values.Where(loc => loc.Branch == branch && fixedOffsets.Contains(loc.Chainage)).ToArray();
            
            var wasSkipping = discretization.Locations.SkipUniqueValuesCheck;
            discretization.Locations.SkipUniqueValuesCheck = false;
            foreach (var networkLocation in networkLocations)
            {
                //set the points as fixed again.
                discretization.ToggleFixedPoint(networkLocation);
            }
            discretization.Locations.SkipUniqueValuesCheck = wasSkipping;
        }
        
        private static void AddGridChainagesAtFixedIntervals(List<double> chainages, bool gridAtFixedLength, double fixedLength)
        {
            // sort chainage and treat every cell now as separately when processing gridAtFixedLength
            chainages.Sort();
            if (gridAtFixedLength)
            {
                var fixedChainages = new List<double>();
                for (int i = 1; i < chainages.Count; i++)
                {
                    double segmentLength = chainages[i] - chainages[i - 1];
                    if (segmentLength > fixedLength)
                    {
                        var numberOfNewSegments = (int)Math.Ceiling(segmentLength / fixedLength);
                        for (int j = 1; j < numberOfNewSegments; j++)
                            fixedChainages.Add(chainages[i - 1] + (j * (segmentLength / numberOfNewSegments)));
                    }
                }

                chainages.AddRange(fixedChainages);
                chainages.Sort();
            }
        }
        
        private static string GetNameForType(Type type)
        {
            if (type == typeof(CrossSection))
                return "cross section";
            if (type == typeof(LateralSource))
                return "lateral source";
            return type.ToString();
        }

        private static void AddGridChainagesForBranchFeatures(IChannel branch, double minimumCellLength, List<double> chainages, IEnumerable<IBranchFeature> features)
        {
            var item = features.FirstOrDefault();
            var typeName = item != null ? GetNameForType(item.GetType()) : "<unknown>";

            double previous = 0.0;
            foreach (var feature in features.OrderBy(cs => cs.Chainage))
            {
                int i = 0;
                while ((i < chainages.Count) && (chainages[i] < feature.Chainage))
                {
                    previous = chainages[i];
                    i++;
                }
                // Is distance to predecessor large enough?
                if ((feature.Chainage - previous) >= minimumCellLength)
                {
                    // Is distance to successor too small?
                    if ((i < chainages.Count) && ((chainages[i] - feature.Chainage) < minimumCellLength))
                    {
                        log.Warn(string.Format("No grid point generated for {4} {0}:{1} at {2:f2} too close to point at {3:f2}.",
                            feature.Name, branch.Name, feature.Chainage, chainages[i], typeName));
                        continue;
                    }

                    if (branch.BranchFeatures.OfType<IStructure1D>().Any(bf => Math.Abs(bf.Chainage - feature.Chainage) < BranchFeature.Epsilon))
                    {
                        log.InfoFormat("No grid point generated for {3} {0}:{1} at {2:f2}. Grid point would overlap with structure.", feature.Name, branch.Name, feature.Chainage, typeName);
                        continue;
                    }

                    chainages.Insert(i, feature.Chainage);

                    previous = feature.Chainage;
                }
                else
                {
                    log.Warn(string.Format("No grid point generated for {4} {0}:{1} at {2:f2} too close to point at {3:f2}.",
                        feature.Name, branch.Name, feature.Chainage, previous, typeName));
                }
                // segment would be too small skip
            }
        }

        /// <summary>
        /// Adds gridpoints for locations where CompositeStructures are defined.
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="minimumCellLength"></param>
        /// <param name="structureDistance"></param>
        /// <param name="chainages"></param>
        private static void AddGridChainageForCompositeStructures(IChannel branch, double minimumCellLength, double structureDistance, List<double> chainages)
        {
            double step = structureDistance;

            var compositeStructures =
                branch.BranchFeatures.Where(bf => bf is ICompositeBranchStructure).OrderBy(bf => bf.Chainage).Select(
                    bf => new {bf.Name, Chainage = bf.Chainage});
            double previousChainage = 0.0;

            IList<double> structureChainages = new List<double>();
            int i = 0;
            bool previousIsStructure = false;

            foreach (var compositeStructure in compositeStructures)
            {
                while ((i < chainages.Count) && (chainages[i] < compositeStructure.Chainage))
                {
                    previousIsStructure = false;
                    previousChainage = chainages[i];
                    i++;
                }
                // add gridpoint before structure
                double beforeChainage = compositeStructure.Chainage - step;
                if ((beforeChainage - previousChainage) >= minimumCellLength)
                {
                    structureChainages.Add(beforeChainage);

                    previousChainage = beforeChainage;
                    previousIsStructure = true;
                }
                else if (previousIsStructure)
                {
                    // if predessor is also structure center the gridpoint between the 2 structures
                    structureChainages[structureChainages.Count - 1] = (beforeChainage + previousChainage) / 2;
                    previousChainage = structureChainages[structureChainages.Count - 1];
                }
                // add gridpoint after structure
                double afterChainage = compositeStructure.Chainage + step;
                if ((afterChainage - previousChainage) >= minimumCellLength)
                {
                    if ((i < chainages.Count) && ((chainages[i] - afterChainage) < minimumCellLength))
                    {
                        continue;
                    }
                    structureChainages.Add(afterChainage);

                    previousChainage = afterChainage;
                    previousIsStructure = true;
                }
            }
            chainages.AddRange(structureChainages);
            chainages.Sort();
        }
        /// <summary>
        /// Switches the direction of the branch
        /// </summary>
        /// <param name="branch"></param>
        public static void ReverseBranch(IBranch branch)
        {
            var branchReverseAction = new BranchReverseAction(branch);
            branch.Network.BeginEdit(branchReverseAction);

            var fromNode = branch.Source;
            var toNode = branch.Target;

            branch.Target = null; // Prevents IsConnectedToMultipleBranches from becoming true when false
            branch.Source = toNode;
            branch.Target = fromNode;

            // Reverse the linestring geometry
            var vertices = new List<Coordinate>();
            for (int i = branch.Geometry.Coordinates.Length - 1; i >= 0; i--)
            {
                vertices.Add(new Coordinate(branch.Geometry.Coordinates[i].X, branch.Geometry.Coordinates[i].Y));
            }
            branch.Geometry = new LineString(vertices.ToArray());

            ReverseBranchBranchFeatures(branch);

            branch.Network.EndEdit();
        }

        /// <summary>
        /// Update the offsets of the branchFeatures. The location on the map are not changed merely there offset
        /// relative to the start of the branch.
        /// </summary>
        /// <param name="branch"></param>
        private static void ReverseBranchBranchFeatures(IBranch branch)
        {
            var reversedBranchFeatures = branch.BranchFeatures.Reverse().ToArray();

            var length = branch.Length;
            foreach (var branchFeature in reversedBranchFeatures)
            {
                branchFeature.SetBeingMoved(true);
                branchFeature.Chainage = BranchFeature.SnapChainage(length, length - branchFeature.Chainage - branchFeature.Length);
            }

            branch.BranchFeatures.Clear();
            branch.BranchFeatures.AddRange(reversedBranchFeatures);

            foreach(var branchFeature in reversedBranchFeatures)
            {
                branchFeature.SetBeingMoved(false);
            }
        }

        /// <summary>
        /// Removes structureFeatures without structures. StructureFeatures are helper/container
        /// object that are created/deleted automatically.
        /// </summary>
        public static void RemoveUnusedCompositeStructures(IHydroNetwork network)
        {
            foreach (var structure in network.CompositeBranchStructures.Where(s => s.Structures.Count == 0).ToArray())
            {
                structure.Branch.BranchFeatures.Remove(structure);
            }
        }

        /// <summary>
        /// Sets the default name of a specific feature.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="feature"></param>
        /// <param name="checkIfNewNameIsNeeded"></param>
        public static string GetUniqueFeatureName(IHydroRegion region, IFeature feature, bool checkIfNewNameIsNeeded = false)
        {
            var featureName = feature.GetEntityType().Name;
            if (region == null) return featureName;

            var fullRegion = region.Parent as IHydroRegion ?? region;
            HashSet<string> names = null;
            lock (fullRegion)
            {
                var hydroObjectNames = fullRegion.AllHydroObjects.Where(f => f.GetEntityType().Name == featureName)
                    .Select(f => f.Name);
                var allLinkNames = fullRegion.AllRegions.OfType<IHydroRegion>().SelectMany(r => r.Links)
                    .Select(l => l.Name);
                var allNames = hydroObjectNames.Concat(allLinkNames);

                names = new HashSet<string>(allNames);
            }

            if (checkIfNewNameIsNeeded)
            {
                var nameProperty = feature.GetType().GetProperty("Name");
                if (nameProperty != null)
                {
                    var currentName = nameProperty.GetValue(feature, null);

                    if (!string.IsNullOrWhiteSpace(currentName as string) && !names.Contains(currentName.ToString())) return currentName.ToString();
                }
            }
            int i = 1;
            var uniqueName = featureName + "_1D_" + i;
            while (names.Contains(uniqueName))
            {
                i++;
                uniqueName = featureName + "_1D_" + i;
            }

            return uniqueName;
        }

        public static void AddStructureToComposite(ICompositeBranchStructure compositeBranchStructure, IStructure1D structure)
        {
            structure.Branch = compositeBranchStructure.Branch;
            structure.ParentStructure = compositeBranchStructure;
            structure.Chainage = compositeBranchStructure.Chainage;
            lock(compositeBranchStructure.Structures)
            {
                compositeBranchStructure.Structures.Add(structure);
            }

            if (null != compositeBranchStructure.Geometry)
            {
                structure.Geometry = (IGeometry)compositeBranchStructure.Geometry.Clone();
            }

            lock (structure.Branch.BranchFeatures)
            {
                structure.Branch.BranchFeatures.Add(structure);
            }
        }

        public static ICompositeBranchStructure AddStructureToExistingCompositeStructureOrToANewOne(IStructure1D structure, IBranch branch, bool generateUniqueName = true)
        {

            var compositeBranchStructure = GetCompositeBranchStructure(structure, branch);

            if(compositeBranchStructure == null)
            {
                if (structure.Geometry == null)
                    structure.Geometry = GetStructureGeometry(branch, structure.Chainage);
                compositeBranchStructure = new CompositeBranchStructure
                {
                    Branch = branch,
                    Network = branch.Network,
                    Chainage = structure.Chainage,
                    Geometry = (IGeometry)structure.Geometry?.Clone()
                };

                if (generateUniqueName)
                {
                    // make new composite structure names unique
                    compositeBranchStructure.Name = GetUniqueFeatureName(compositeBranchStructure.Network as HydroNetwork, compositeBranchStructure);
                }

                lock(branch.BranchFeatures)
                {
                    branch.BranchFeatures.Add(compositeBranchStructure);
                }
            }

            AddStructureToComposite(compositeBranchStructure, structure);
            return compositeBranchStructure;
        }

        public static IGeometry GetStructureGeometry(IBranch branch, double chainage)
        {
            if (branch?.Geometry == null) return null;
            var lengthIndexedLine = new LengthIndexedLine(branch.Geometry);
            var mapOffset = NetworkHelper.MapChainage(branch, chainage);
            return new Point(lengthIndexedLine.ExtractPoint(mapOffset).Copy());
        }

        private static ICompositeBranchStructure GetCompositeBranchStructure(IStructure1D structure, IBranch branch)
        {
            var compositeBranchStructures = branch.BranchFeatures
                                                  .OfType<ICompositeBranchStructure>()
                                                  .ToArray();

            // first try to find composite structure by using the tag
            // see CompositeBranchStructureDefinitionReader.ReadDefinition
            var composite = compositeBranchStructures.FirstOrDefault(f => f.Tag is string tagString 
                                                                          && tagString.Split(';')
                                                                                      .Any(i => string.Equals(i, structure.Name, StringComparison.CurrentCultureIgnoreCase)));

            return composite ?? compositeBranchStructures.FirstOrDefault(f => Math.Abs(f.Chainage - structure.Chainage) < 0.01);
        }

        ///<summary>
        /// Removes a structure from the hydro network
        ///</summary>
        ///<param name="structure"></param>
        public static void RemoveStructure(IStructure1D structure)
        {
            var channel = structure.Branch;
            if (channel == null) return; // Do nothing if structure is not on a branch

            channel.Network.BeginEdit("Delete " + structure.Name);
            
            channel.BranchFeatures.Remove(structure);
            RemoveFromChannel(structure, channel);

            channel.Network.EndEdit();
        }

        [EditAction]
        private static void RemoveFromChannel(IStructure1D structure, IBranch channel)
        {
            if (null == structure.ParentStructure)
            {
                return;
            }
            structure.ParentStructure.Structures.Remove(structure);
            structure.Branch = null;
            if (structure.ParentStructure.Structures.Count != 0)
            {
                return;
            }
            channel.BranchFeatures.Remove(structure.ParentStructure);
            structure.ParentStructure.Branch = null;
        }

        public static void AddSnakeHydroNetwork(IHydroNetwork network, params Point[] points)
        {
            AddSnakeNetwork(false, points, network);
        }

        public static IHydroNetwork GetSnakeHydroNetwork(params Point[] points)
        {
            return GetSnakeHydroNetwork(false, points);
        }

        public static IHydroNetwork GetSnakeHydroNetwork(bool generateIDs, params Point[] points)
        {
            var network = new HydroNetwork();
            AddSnakeNetwork(generateIDs, points, network);
            return network;
        }

        private static void AddSnakeNetwork(bool generateIDs, Point[] points, IHydroNetwork network)
        {
            var crossSectionType = new CrossSectionSectionType {Name = "FlutPleen"};
            network.CrossSectionSectionTypes.Add(crossSectionType);
            for (int i = 0; i < points.Length; i++)
            {
                var nodeName = "node" + (i + 1);

                network.Nodes.Add(new HydroNode(nodeName) {Geometry = points[i]});
            }
            for (int i = 1; i < points.Length; i++)
            {
                var lineGeometry = new LineString(new[]
                    {
                        new Coordinate(points[i - 1].X, points[i - 1].Y),
                        new Coordinate(points[i].X, points[i].Y)
                    });

                var branchName = "branch" + i;
                var branch = new Channel(branchName, network.Nodes[i - 1], network.Nodes[i])
                    {
                        Geometry = lineGeometry
                    };
                //setting id is optional ..needed for netcdf..but fatal for NHibernate (thinks it saved already)
                if (generateIDs)
                {
                    branch.Id = i;
                }
                network.Branches.Add(branch);
            }
        }

        public static IHydroNetwork GetSnakeHydroNetwork(int numberOfBranches)
        {
            return GetSnakeHydroNetwork(numberOfBranches, false);
        }

        /// <summary>
        /// Creates a random network with numberofBranches branches. 
        /// All branches are 100 long and the network is directed to the right.
        /// </summary>
        /// <param name="numberOfBranches"></param>
        /// <param name="generateIDs"></param>
        /// <returns></returns>
        public static IHydroNetwork GetSnakeHydroNetwork(int numberOfBranches, bool generateIDs)
        {
            IList<Point> points = new List<Point>();
            // create a random network by moving constantly right
            double currentX = 0;
            double currentY = 0;
            var random = new Random();
            int numberOfNodes = numberOfBranches + 1;
            for (int i = 0; i < numberOfNodes; i++)
            {
                // generate a network of branches of length 100 moving right by random angle.
                points.Add(new Point(currentX, currentY));
                //angle between -90 and +90
                double angle = random.Next(180) - 90;
                // x is cos between 0 < 1
                // y is sin between 1 and -1
                currentX += 100 * Math.Cos(DegreeToRadian(angle));// between 0 and 100
                currentY += 100 * Math.Sin(DegreeToRadian(angle));// between -100 and 100
            }
            return GetSnakeHydroNetwork(generateIDs, points.ToArray());
        }

        public static string CreateUniqueCompartmentNameInNetwork(IHydroNetwork network)
        {
            var compartments = new List<ICompartment>();
            if (network != null)
                compartments = network.Manholes.SelectMany(m => m.Compartments).ToList();

            return NetworkHelper.GetUniqueName("Compartment{0:D2}", compartments, "Compartment");
        }

        public static string GetUniqueManholeIdInNetwork(IHydroNetwork network)
        {
            var manholes = new List<IManhole>();
            if (network != null)
                lock (network.Nodes)
                {
                    manholes = network.Manholes.ToList();
                }
                

            return NetworkHelper.GetUniqueName("Manhole{0:D2}", manholes, "Manhole");
        }

        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
        
        public static ICrossSection AddCrossSectionDefinitionToBranch(IBranch branch, ICrossSectionDefinition crossSectionDefinition, double offset)
        {
            var branchFeature = new CrossSection(crossSectionDefinition);
            branchFeature.Name = "cross_section";
            NetworkHelper.AddBranchFeatureToBranch(branchFeature,branch, offset);
            return branchFeature;
        }

        public static IFeature SplitPipeAtCoordinate(IPipe pipe, Coordinate geometryCoordinate)
        {
            var lengthIndexedLine = new LengthIndexedLine(pipe.Geometry);
            double offset = lengthIndexedLine.Project(geometryCoordinate);
            return SplitPipeAtNode(pipe, offset);

        }
    }
}
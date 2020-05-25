using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro.CrossSections;
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
    /// todo: move calculation grid generation to DiscretizationHelper?
    /// </summary>
    // TODO: split into NetworkHelper and HydroNetworkHelper?
    public class HydroNetworkHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroNetworkHelper));
        
        /// <summary>
        /// </summary>
        /// <param name="networkCoverage"> </param>
        /// <param name="branch"> </param>
        /// <param name="offsets"> </param>
        public static void GenerateDiscretization(INetworkCoverage networkCoverage, IChannel branch,
                                                  IEnumerable<double> offsets)
        {
            NetworkHelper.ClearLocations(networkCoverage, branch);
            List<NetworkLocation> locations = offsets.Select(offset => new NetworkLocation(branch, offset)).ToList();
            networkCoverage.Locations.AddValues(locations);
        }

        public static void GenerateDiscretization(IDiscretization discretization,
                                                  bool overWriteSegments, bool eraseExisting,
                                                  double minimumCellLength, bool gridAtStructure,
                                                  double structureDistance, bool gridAtCrossSection,
                                                  bool gridAtLaterals,
                                                  bool gridAtFixedLength, double fixedLength,
                                                  IList<IChannel> selectedChannels = null)
        {
            discretization.Locations.SkipUniqueValuesCheck = true;

            selectedChannels = selectedChannels ?? discretization.Network.Branches.Cast<IChannel>().ToList();
            discretization.SegmentGenerationMethod = SegmentGenerationMethod.None;
            foreach (Channel channel in selectedChannels)
            {
                if (BranchLocationCount(discretization, channel) > 1)
                {
                    if (!overWriteSegments)
                    {
                        continue;
                    }
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

            discretization.SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered;

            discretization.Locations.SkipUniqueValuesCheck = false;
        }

        /// <summary>
        /// Generates calculation grid cells for a branch, When the grid is generated at cross sections and at fixed
        /// length each subbranch between two cross sections processed separately to prevent too small grid cells.
        /// todo add support for structures etc.
        /// </summary>
        /// <param name="discretization"> </param>
        /// <param name="branch"> </param>
        /// <param name="minimumCellLength"> </param>
        /// <param name="gridAtStructure"> </param>
        /// <param name="structureDistance"> </param>
        /// <param name="gridAtCrossSection"> </param>
        /// <param name="gridAtLaterals"> </param>
        /// <param name="gridAtFixedLength"> </param>
        /// <param name="fixedLength"> </param>
        public static void GenerateDiscretization(IDiscretization discretization, IChannel branch,
                                                  double minimumCellLength, bool gridAtStructure,
                                                  double structureDistance, bool gridAtCrossSection,
                                                  bool gridAtLaterals, bool gridAtFixedLength, double fixedLength)
        {
            var offsets = new List<double> {0.0};

            // remember network locations the user has fixed.
            INetworkLocation[] existingLocations =
                discretization.Locations.Values.Where(nl => nl.Branch == branch).ToArray();
            List<double> fixedOffsets = (from networkLocation in existingLocations
                                         where discretization.IsFixedPoint(networkLocation)
                                         select networkLocation.Chainage).ToList();
            offsets.AddRange(fixedOffsets);
            double length = branch.Length;
            offsets.Add(length);
            offsets = offsets.Distinct().ToList();

            if (gridAtStructure)
            {
                AddGridChainageForCompositeStructures(branch, minimumCellLength, structureDistance, offsets);
            }

            if (gridAtCrossSection)
            {
                AddGridChainagesForBranchFeatures(branch, minimumCellLength, offsets,
                                                  branch.CrossSections.OfType<IBranchFeature>());
            }

            if (gridAtLaterals)
            {
                AddGridChainagesForBranchFeatures(branch, minimumCellLength, offsets,
                                                  branch.BranchSources.OfType<IBranchFeature>());
            }

            AddGridChainagesAtFixedIntervals(offsets, gridAtFixedLength, fixedLength);
            GenerateDiscretization(discretization, branch, offsets);
            INetworkLocation[] networkLocations = discretization
                                                  .Locations.Values
                                                  .Where(loc => loc.Branch == branch &&
                                                                fixedOffsets.Contains(loc.Chainage)).ToArray();

            bool wasSkipping = discretization.Locations.SkipUniqueValuesCheck;
            discretization.Locations.SkipUniqueValuesCheck = false;
            foreach (INetworkLocation networkLocation in networkLocations)
            {
                //set the points as fixed again.
                discretization.ToggleFixedPoint(networkLocation);
            }

            discretization.Locations.SkipUniqueValuesCheck = wasSkipping;
        }

        /// <summary>
        /// Removes structureFeatures without structures. StructureFeatures are helper/container
        /// object that are created/deleted automatically.
        /// </summary>
        public static void RemoveUnusedCompositeStructures(IHydroNetwork network)
        {
            foreach (ICompositeBranchStructure structure in network
                                                            .CompositeBranchStructures
                                                            .Where(s => s.Structures.Count == 0).ToArray())
            {
                structure.Branch.BranchFeatures.Remove(structure);
            }
        }

        /// <summary>
        /// Sets the default name of a specific feature.
        /// </summary>
        /// <param name="region"> </param>
        /// <param name="feature"> </param>
        public static string GetUniqueFeatureName(IHydroRegion region, IFeature feature,
                                                  bool checkIfNewNameIsNeeded = false)
        {
            string featureName = feature.GetEntityType().Name;
            IHydroRegion fullRegion = region.Parent as IHydroRegion ?? region;
            IEnumerable<string> hydroObjectNames = fullRegion
                                                   .AllHydroObjects.Where(f => f.GetEntityType().Name == featureName)
                                                   .Select(f => f.Name);
            IEnumerable<string> allLinkNames =
                fullRegion.AllRegions.OfType<IHydroRegion>().SelectMany(r => r.Links).Select(l => l.Name);
            IEnumerable<string> allNames = hydroObjectNames.Concat(allLinkNames);
            var names = new HashSet<string>(allNames);

            if (checkIfNewNameIsNeeded)
            {
                PropertyInfo nameProperty = feature.GetType().GetProperty("Name");
                if (nameProperty != null)
                {
                    object currentName = nameProperty.GetValue(feature, null);

                    if (!string.IsNullOrWhiteSpace(currentName as string) && !names.Contains(currentName.ToString()))
                    {
                        return currentName.ToString();
                    }
                }
            }

            var i = 1;
            string uniqueName = featureName + i;
            while (names.Contains(uniqueName))
            {
                i++;
                uniqueName = featureName + i;
            }

            return uniqueName;
        }

        public static void AddStructureToComposite(ICompositeBranchStructure compositeBranchStructure,
                                                   IStructure1D structure)
        {
            structure.Branch = compositeBranchStructure.Branch;
            structure.ParentStructure = compositeBranchStructure;
            structure.Chainage = compositeBranchStructure.Chainage;
            compositeBranchStructure.Structures.Add(structure);

            if (null != compositeBranchStructure.Geometry)
            {
                structure.Geometry = (IGeometry) compositeBranchStructure.Geometry.Clone();
            }

            structure.Branch.BranchFeatures.Add(structure);
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

        public static IHydroNetwork GetSnakeHydroNetwork(int numberOfBranches)
        {
            return GetSnakeHydroNetwork(numberOfBranches, false);
        }

        /// <summary>
        /// Creates a random network with numberofBranches branches.
        /// All branches are 100 long and the network is directed to the right.
        /// </summary>
        /// <param name="numberOfBranches"> </param>
        /// <param name="generateIDs"> </param>
        /// <returns> </returns>
        public static IHydroNetwork GetSnakeHydroNetwork(int numberOfBranches, bool generateIDs)
        {
            IList<Point> points = new List<Point>();
            // create a random network by moving constantly right
            double currentX = 0;
            double currentY = 0;
            var random = new Random();
            int numberOfNodes = numberOfBranches + 1;
            for (var i = 0; i < numberOfNodes; i++)
            {
                // generate a network of branches of length 100 moving right by random angle.
                points.Add(new Point(currentX, currentY));
                //angle between -90 and +90
                double angle = random.Next(180) - 90;
                // x is cos between 0 < 1
                // y is sin between 1 and -1
                currentX += 100 * Math.Cos(DegreeToRadian(angle)); // between 0 and 100
                currentY += 100 * Math.Sin(DegreeToRadian(angle)); // between -100 and 100
            }

            return GetSnakeHydroNetwork(generateIDs, points.ToArray());
        }

        public static ICrossSection AddCrossSectionDefinitionToBranch(IBranch branch,
                                                                      ICrossSectionDefinition crossSectionDefinition,
                                                                      double offset)
        {
            var branchFeature = new CrossSection(crossSectionDefinition);
            branchFeature.Name = "cross_section";
            NetworkHelper.AddBranchFeatureToBranch(branchFeature, branch, offset);
            return branchFeature;
        }
        
        /// <summary>
        /// Returns the number of networklocation in a coverage for a branch
        /// </summary>
        /// <param name="networkCoverage"> </param>
        /// <param name="branch"> </param>
        /// <returns> </returns>
        private static int BranchLocationCount(INetworkCoverage networkCoverage, IChannel branch)
        {
            return networkCoverage.Locations.Values.Where(nl => nl.Branch == branch).Count();
        }

        private static void AddGridChainagesAtFixedIntervals(List<double> chainages, bool gridAtFixedLength,
                                                             double fixedLength)
        {
            // sort chainage and treat every cell now as separately when processing gridAtFixedLength
            chainages.Sort();
            if (gridAtFixedLength)
            {
                var fixedChainages = new List<double>();
                for (var i = 1; i < chainages.Count; i++)
                {
                    double segmentLength = chainages[i] - chainages[i - 1];
                    if (segmentLength > fixedLength)
                    {
                        var numberOfNewSegments = (int) Math.Ceiling(segmentLength / fixedLength);
                        for (var j = 1; j < numberOfNewSegments; j++)
                        {
                            fixedChainages.Add(chainages[i - 1] + (j * (segmentLength / numberOfNewSegments)));
                        }
                    }
                }

                chainages.AddRange(fixedChainages);
                chainages.Sort();
            }
        }

        private static string GetNameForType(Type type)
        {
            if (type == typeof(CrossSection))
            {
                return "cross section";
            }

            if (type == typeof(LateralSource))
            {
                return "lateral source";
            }

            return type.ToString();
        }

        private static void AddGridChainagesForBranchFeatures(IChannel branch, double minimumCellLength,
                                                              List<double> chainages,
                                                              IEnumerable<IBranchFeature> features)
        {
            IBranchFeature item = features.FirstOrDefault();
            string typeName = item != null ? GetNameForType(item.GetType()) : "<unknown>";

            var previous = 0.0;
            foreach (IBranchFeature feature in features.OrderBy(cs => cs.Chainage))
            {
                var i = 0;
                while (i < chainages.Count && chainages[i] < feature.Chainage)
                {
                    previous = chainages[i];
                    i++;
                }

                // Is distance to predecessor large enough?
                if (feature.Chainage - previous >= minimumCellLength)
                {
                    // Is distance to successor too small?
                    if (i < chainages.Count && chainages[i] - feature.Chainage < minimumCellLength)
                    {
                        log.Warn(string.Format(
                                     "No grid point generated for {4} {0}:{1} at {2:f2} too close to point at {3:f2}.",
                                     feature.Name, branch.Name, feature.Chainage, chainages[i], typeName));
                        continue;
                    }

                    if (branch.BranchFeatures.OfType<IStructure1D>()
                              .Any(bf => Math.Abs(bf.Chainage - feature.Chainage) < BranchFeature.Epsilon))
                    {
                        log.InfoFormat(
                            "No grid point generated for {3} {0}:{1} at {2:f2}. Grid point would overlap with structure.",
                            feature.Name, branch.Name, feature.Chainage, typeName);
                        continue;
                    }

                    chainages.Insert(i, feature.Chainage);

                    previous = feature.Chainage;
                }
                else
                {
                    log.Warn(string.Format(
                                 "No grid point generated for {4} {0}:{1} at {2:f2} too close to point at {3:f2}.",
                                 feature.Name, branch.Name, feature.Chainage, previous, typeName));
                }

                // segment would be too small skip
            }
        }

        /// <summary>
        /// Adds gridpoints for locations where CompositeStructures are defined.
        /// </summary>
        /// <param name="branch"> </param>
        /// <param name="minimumCellLength"> </param>
        /// <param name="structureDistance"> </param>
        /// <param name="chainages"> </param>
        private static void AddGridChainageForCompositeStructures(IChannel branch, double minimumCellLength,
                                                                  double structureDistance, List<double> chainages)
        {
            double step = structureDistance;

            var compositeStructures =
                branch.BranchFeatures.Where(bf => bf is ICompositeBranchStructure).OrderBy(bf => bf.Chainage).Select(
                    bf => new
                    {
                        bf.Name,
                        Chainage = bf.Chainage
                    });
            var previousChainage = 0.0;

            IList<double> structureChainages = new List<double>();
            var i = 0;
            var previousIsStructure = false;

            foreach (var compositeStructure in compositeStructures)
            {
                while (i < chainages.Count && chainages[i] < compositeStructure.Chainage)
                {
                    previousIsStructure = false;
                    previousChainage = chainages[i];
                    i++;
                }

                // add gridpoint before structure
                double beforeChainage = compositeStructure.Chainage - step;
                if (beforeChainage - previousChainage >= minimumCellLength)
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
                if (afterChainage - previousChainage >= minimumCellLength)
                {
                    if (i < chainages.Count && chainages[i] - afterChainage < minimumCellLength)
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
        /// Update the offsets of the branchFeatures. The location on the map are not changed merely there offset
        /// relative to the start of the branch.
        /// </summary>
        /// <param name="branch"> </param>
        private static void ReverseBranchBranchFeatures(IBranch branch)
        {
            IBranchFeature[] reversedBranchFeatures = branch.BranchFeatures.Reverse().ToArray();

            double length = branch.Length;
            foreach (IBranchFeature branchFeature in reversedBranchFeatures)
            {
                branchFeature.SetBeingMoved(true);
                branchFeature.Chainage =
                    BranchFeature.SnapChainage(length, length - branchFeature.Chainage - branchFeature.Length);
            }

            branch.BranchFeatures.Clear();
            branch.BranchFeatures.AddRange(reversedBranchFeatures);

            foreach (IBranchFeature branchFeature in reversedBranchFeatures)
            {
                branchFeature.SetBeingMoved(false);
            }
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

        private static void AddSnakeNetwork(bool generateIDs, Point[] points, IHydroNetwork network)
        {
            var crossSectionType = new CrossSectionSectionType {Name = "FlutPleen"};
            network.CrossSectionSectionTypes.Add(crossSectionType);
            for (var i = 0; i < points.Length; i++)
            {
                string nodeName = "node" + (i + 1);

                network.Nodes.Add(new HydroNode(nodeName) {Geometry = points[i]});
            }

            for (var i = 1; i < points.Length; i++)
            {
                var lineGeometry = new LineString(new[]
                {
                    new Coordinate(points[i - 1].X, points[i - 1].Y),
                    new Coordinate(points[i].X, points[i].Y)
                });

                string branchName = "branch" + i;
                var branch = new Channel(branchName, network.Nodes[i - 1], network.Nodes[i]) {Geometry = lineGeometry};
                //setting id is optional ..needed for netcdf..but fatal for NHibernate (thinks it saved already)
                if (generateIDs)
                {
                    branch.Id = i;
                }

                network.Branches.Add(branch);
            }
        }

        private static double DegreeToRadian(double angle)
        {
            return (Math.PI * angle) / 180.0;
        }
    }
}
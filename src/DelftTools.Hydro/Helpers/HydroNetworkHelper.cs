using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro.CrossSections;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.Helpers
{
    /// <summary>
    /// Utility class to work with networks
    /// todo: move calculation grid generation to DiscretizationHelper?
    /// </summary>
    // TODO: split into NetworkHelper and HydroNetworkHelper?
    public class HydroNetworkHelper
    {
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

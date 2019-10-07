using System;
using System.Globalization;
using DelftTools.Hydro;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.TestUtils
{
    public static class NetworkAndGridReaderTestHelper
    {
        #region Const TestValues

        public const double NODE1_X = 11.0;
        public const double NODE1_Y = 13.5;
        public const double NODE2_X = 31.5;
        public const double NODE2_Y = 37.0;

        public const int NUM_DISCRETIZATION_LOCATIONS = 10;

        #endregion

        public static IDiscretization GenerateDiscretization(IChannel branch, int numLocations)
        {
            const int decimalPlaces = 3;
            var sourceX = branch.Source.Geometry.Coordinate.X;
            var sourceY = branch.Source.Geometry.Coordinate.Y;
            var diffX = branch.Target.Geometry.Coordinate.X - sourceX;
            var diffY = branch.Target.Geometry.Coordinate.Y - sourceY;

            var discretization = new Discretization();
            for (var i = 0; i < numLocations; i++)
            {
                var chainage = Math.Round((double) (branch.Length / (numLocations - 1) * i), decimalPlaces);
                var x = Math.Round((double) (sourceX + (diffX / (numLocations - 1) * i)), decimalPlaces);
                var y = Math.Round((double) (sourceY + (diffY / (numLocations - 1) * i)), decimalPlaces);

                discretization.Locations.Values.Add(new NetworkLocation
                {
                    Branch = branch,
                    Chainage = chainage,
                    Geometry = new Point(x, y),
                    Name = string.Format("{0}_{1}", branch.Name, chainage.ToString("F3", CultureInfo.InvariantCulture))
                });
            }
            return discretization;
        }

        public static bool CompareNodes(IHydroNode node1, IHydroNode node2)
        {
            // Note: this comparison is not exhaustive
            var areEqual = true;

            areEqual &= node1.Name == node2.Name;
            areEqual &= node1.LongName == node2.LongName;
            areEqual &= node1.Geometry.EqualsExact(node2.Geometry);    
            
            areEqual &= node1.Description == node2.Description;
            areEqual &= node1.CanBeLinkSource == node2.CanBeLinkSource;
            areEqual &= node1.CanBeLinkTarget == node2.CanBeLinkTarget;
            areEqual &= node1.IsConnectedToMultipleBranches == node2.IsConnectedToMultipleBranches;
            areEqual &= node1.IsOnSingleBranch == node2.IsOnSingleBranch;
            areEqual &= node1.Attributes.Count == node2.Attributes.Count;
            
            areEqual &= node1.Network.Name == node2.Network.Name;

            areEqual &= node1.IncomingBranches.Count == node2.IncomingBranches.Count;
            for (var i = 0; i < node1.IncomingBranches.Count; i++)
            {
                areEqual &= node1.IncomingBranches[i].Name == node2.IncomingBranches[i].Name;
            }

            areEqual &= node1.OutgoingBranches.Count == node2.OutgoingBranches.Count;
            for (var i = 0; i < node1.OutgoingBranches.Count; i++)
            {
                areEqual &= node1.OutgoingBranches[i].Name == node2.OutgoingBranches[i].Name;
            }
           
            areEqual &= node1.Links.Count == node2.Links.Count;
            areEqual &= node1.NodeFeatures.Count == node2.NodeFeatures.Count;
            
            return areEqual;
        }

        public static bool CompareBranches(IChannel branch1, IChannel branch2)
        {
            // Note: this comparison is not exhaustive
            var areEqual = true;

            areEqual &= branch1.Name == branch2.Name;
            areEqual &= branch1.LongName == branch2.LongName;
            areEqual &= branch1.Description == branch2.Description;
            areEqual &= branch1.Source.Name == branch2.Source.Name;
            areEqual &= branch1.Source.Geometry.EqualsExact(branch2.Source.Geometry);
            areEqual &= branch1.Target.Name == branch2.Target.Name;
            areEqual &= branch1.Target.Geometry.EqualsExact(branch2.Target.Geometry);
            areEqual &= branch1.OrderNumber == branch2.OrderNumber;
            areEqual &= branch1.Attributes.Count == branch2.Attributes.Count;

            return areEqual;
        }

        public static bool CompareDiscretizationsValues(INetworkLocation location1, INetworkLocation location2)
        {
            // Note: this comparison is not exhaustive
            var areEqual = true;

            areEqual &= location1.Name == location2.Name;
            areEqual &= location1.LongName == location2.LongName;
            areEqual &= location1.Description == location2.Description;
            areEqual &= location1.Chainage.Equals(location2.Chainage);
            areEqual &= location1.Geometry.EqualsExact(location2.Geometry);
            areEqual &= location1.Branch.Name == location2.Branch.Name;
            
            return areEqual;
        }
    }
}

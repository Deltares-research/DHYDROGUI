using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.Link1d2d
{
    public static class ILink1D2DExtensions
    {
        public static int IndexOfNearest1D2DLink(this IEnumerable<ILink1D2D> links, Coordinate coordinate)
        {
            // get cell index at coordinate
            var closestDistance = Double.MaxValue;
            var bestFlowLinkIndex = -1;

            var point = new Point(coordinate);

            // cheap solution for now (may not give correct result):
            var flowLinks = links.ToList();
            for (int i = 0; i < flowLinks.Count; i++)
            {
                var flowLink = flowLinks[i];
                var line = new Point(flowLink.Geometry.Centroid.Coordinate);
                var distance = line.Distance(point);
                if (distance < closestDistance)
                {
                    bestFlowLinkIndex = i;
                    closestDistance = distance;
                }
            }

            return bestFlowLinkIndex;
        }
    }
}
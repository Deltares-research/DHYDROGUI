using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Networks;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.Fews.Tests.Assemblers
{
    public class RouteTestHelper
    {
        public static INetwork GetSnakeNetwork(bool generateIDs, int numberOfBranches)
        {
            IList<Point> points = new List<Point>();
            //create a random network by moving constantly right
            double currentX = 0;
            double currentY = 0;
            var random = new Random();
            int numberOfNodes = numberOfBranches + 1;
            for (int i = 0; i < numberOfNodes; i++)
            {
                //generate a network of branches of length 100 moving right by random angle.
                points.Add(new Point(currentX, currentY));
                //angle between -90 and +90
                double angle = random.Next(180) - 90;
                //x is cos between 0<1
                //y is sin between 1 and -1
                currentX += 100 * Math.Cos(DegreeToRadian(angle));//between 0 and 100
                currentY += 100 * Math.Sin(DegreeToRadian(angle));//between -100 and 100
            }
            return GetSnakeNetwork(generateIDs, points.ToArray());
        }

        public static INetwork GetSnakeNetwork(bool generateIDs, params Point[] points)
        {
            var network = new Network();
            for (int i = 0; i < points.Length; i++)
            {
                var nodeName = "node" + (i + 1);

                network.Nodes.Add(new Node(nodeName) { Geometry = points[i] });
            }
            for (int i = 1; i < points.Length; i++)
            {
                var lineGeometry = new LineString(new[]
                                                  {
                                                      new Coordinate(points[i-1].X, points[i-1].Y),
                                                      new Coordinate(points[i].X, points[i].Y)
                                                  });

                var branchName = "branch" + i;
                var branch = new Branch(network.Nodes[i - 1], network.Nodes[i], lineGeometry.Length)
                             {
                                 Geometry = lineGeometry,
                                 Name = branchName,

                             };
                //setting id is optional ..needed for netcdf..but fatal for nhibernate (thinks it saved already)
                if (generateIDs)
                {
                    branch.Id = i;
                }
                network.Branches.Add(branch);
            }
            return network;
        }

        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

    }
}
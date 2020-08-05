using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public static class EmbankmentGenerator
    {
        private const int DoLeft = 1;
        private const int DoRight = -1;
        private static readonly ILog Log = LogManager.GetLogger(typeof(EmbankmentGenerator));

        public static bool GenerateEmbankments(IList<Channel> branches, IList<Embankment> embankmentDefinitions,
                                               double constantDistance,
                                               bool generateLeftEmbankments, bool generateRightEmbankments,
                                               bool mergeAutomatically)
        {
            ChannelToLeftEmbankments = new Dictionary<string, Embankment>();
            ChannelToRightEmbankments = new Dictionary<string, Embankment>();

            bool result = GenerateEmbankmentsAtConstantDistance(branches, embankmentDefinitions, constantDistance,
                                                                generateLeftEmbankments, generateRightEmbankments);

            if (mergeAutomatically)
            {
                MergeAllEmbankments(branches, embankmentDefinitions);
            }

            return result;
        }

        public static bool GenerateEmbankmentsAtConstantDistance(IList<Channel> channels,
                                                                 IList<Embankment> embankmentDefinitions,
                                                                 double distance, bool generateLeftEmbankment,
                                                                 bool generateRightEmbankment)
        {
            if (distance <= 0.0)
            {
                Log.Warn("Given distance must be greater than zero. No embankments are generated.");
                return false;
            }

            if (distance > 10000.0d)
            {
                Log.Warn("Given distance to branch is too large. No embankments are generated.");
                return false;
            }

            foreach (Channel channel in channels)
            {
                var coordinatesLeft = new List<Coordinate>();
                var coordinatesRight = new List<Coordinate>();

                var leftEmbankment = new Embankment();
                var rightEmbankment = new Embankment();

                var radiansUp = 0.0;

                var prevRadians = 0.0;
                Coordinate prevPoint = channel.Geometry.Coordinates[0];
                var prevChainage = 0.0;
                var turningDirection = 0; // -1: Left, 1:Right, 0:Straight

                var skipLeftPoint = false;
                var skipRightPoint = false;

                for (var i = 0; i < channel.Geometry.Coordinates.Count(); i++)
                {
                    Coordinate point1;
                    Coordinate point2;
                    Coordinate refPoint;

                    double pointChainage;

                    if (i < channel.Geometry.Coordinates.Count() - 1)
                    {
                        point1 = channel.Geometry.Coordinates[i];
                        point2 = channel.Geometry.Coordinates[i + 1];
                        refPoint = point1;

                        pointChainage =
                            NetworkHelper.GetBranchFeatureChainageFromGeometry(channel, new Point(refPoint));
                    }
                    else
                    {
                        point1 = channel.Geometry.Coordinates[i - 1];
                        point2 = channel.Geometry.Coordinates[i];
                        refPoint = point2;

                        pointChainage = channel.Length;
                    }

                    double x = point2.X - point1.X;
                    double y = point2.Y - point1.Y;

                    double radiansDown = Math.Atan2(y, x);
                    if (radiansDown < 0.0)
                    {
                        radiansDown = (Math.PI * 2.0) + radiansDown;
                    }

                    if (i == channel.Geometry.Coordinates.Count() - 1)
                    {
                        // Skip if necessary according the Rodriguez Aguilera criterium (TOOLS-22145)
                        skipLeftPoint = false;
                        if (generateLeftEmbankment && turningDirection == -1)
                        {
                            double beta = (radiansDown - prevRadians) / 2.0;
                            skipLeftPoint = (pointChainage - prevChainage) / distance < Math.Tan(beta);
                        }

                        skipRightPoint = false;
                        if (generateRightEmbankment && turningDirection == 1)
                        {
                            double beta = (prevRadians - radiansDown) / 2.0;
                            skipRightPoint = (pointChainage - prevChainage) / distance < Math.Tan(beta);
                        }
                    }

                    if (i == 0 || i == channel.Geometry.Coordinates.Count() - 1)
                    {
                        radiansUp = radiansDown;
                    }

                    if (generateLeftEmbankment && !skipLeftPoint)
                    {
                        Coordinate leftPoint = GetPoint(radiansUp, radiansDown, refPoint, distance, DoLeft);
                        coordinatesLeft.Add(leftPoint);
                    }

                    if (generateRightEmbankment && !skipRightPoint)
                    {
                        Coordinate rightPoint = GetPoint(radiansUp, radiansDown, refPoint, distance, DoRight);
                        coordinatesRight.Add(rightPoint);
                    }

                    turningDirection = 0;
                    if (i == channel.Geometry.Coordinates.Count() - 2)
                    {
                        turningDirection = GetTurningDirection(prevPoint, point1, point2);
                    }

                    prevRadians = radiansUp;
                    radiansUp = radiansDown;
                    prevPoint = point1;
                    prevChainage = pointChainage;
                }

                if (generateLeftEmbankment)
                {
                    leftEmbankment.Geometry = new LineString(coordinatesLeft.ToArray());
                    leftEmbankment.Name =
                        NetworkHelper.GetUniqueName("Embankment{0:D2}", embankmentDefinitions, "Embankment");
                    embankmentDefinitions.Add(leftEmbankment);
                    ChannelToLeftEmbankments[channel.Name] = leftEmbankment;
                }

                if (generateRightEmbankment)
                {
                    rightEmbankment.Geometry = new LineString(coordinatesRight.ToArray());
                    rightEmbankment.Name =
                        NetworkHelper.GetUniqueName("Embankment{0:D2}", embankmentDefinitions, "Embankment");
                    embankmentDefinitions.Add(rightEmbankment);
                    ChannelToRightEmbankments[channel.Name] = rightEmbankment;
                }
            }

            return true;
        }

        private static Dictionary<string, Embankment> ChannelToLeftEmbankments { get; set; }
        private static Dictionary<string, Embankment> ChannelToRightEmbankments { get; set; }

        private static void MergeAllEmbankments(IList<Channel> branches, IList<Embankment> embankmentDefinitions)
        {
            var branchesMutableCopy = new List<Channel>(branches);
            foreach (Channel branch in branches)
            {
                int targetCount = (branch.Target.IncomingBranches?.Count ?? 0) +
                                  (branch.Target.OutgoingBranches?.Count ?? 0);
                int sourceCount = (branch.Source.IncomingBranches?.Count ?? 0) +
                                  (branch.Source.OutgoingBranches?.Count ?? 0);
                if (targetCount > 2 || sourceCount > 2)
                {
                    continue; // TODO: allow this situation
                }

                IEnumerable<Channel> connectedBranchesSameDirection =
                    branchesMutableCopy.Where(b => b.Source == branch.Target || b.Target == branch.Source);

                MergeBranchesToThisBranch(embankmentDefinitions, connectedBranchesSameDirection, branch);

                IEnumerable<Channel> connectedBranchesOppositeDirection =
                    branchesMutableCopy.Where(
                        b => b != branch &&
                             (b.Source == branch.Source || b.Target == branch.Target));

                MergeBranchesToThisBranch(embankmentDefinitions, connectedBranchesOppositeDirection, branch, true);

                // remove embankment from branchesCopy and from lookup dictionary
                branchesMutableCopy.Remove(branch);
                ChannelToLeftEmbankments.Remove(branch.Name);
                ChannelToRightEmbankments.Remove(branch.Name);
            }
        }

        private static void MergeBranchesToThisBranch(IList<Embankment> embankmentDefinitions,
                                                      IEnumerable<Channel> connectedBranches,
                                                      Channel branch,
                                                      bool connectedBranchesHaveOppositeDirection = false)
        {
            Embankment l1, l2, r1, r2;
            foreach (Channel cb in connectedBranches)
            {
                l1 = null;
                l2 = null;
                r1 = null;
                r2 = null;
                ChannelToLeftEmbankments.TryGetValue(branch.Name, out l1);
                ChannelToLeftEmbankments.TryGetValue(cb.Name, out l2);
                ChannelToRightEmbankments.TryGetValue(branch.Name, out r1);
                ChannelToRightEmbankments.TryGetValue(cb.Name, out r2);

                if (connectedBranchesHaveOppositeDirection)
                {
                    Embankment temp = l2;
                    l2 = r2;
                    r2 = temp;
                }

                if (l1 != null && l2 != null)
                {
                    Embankment mergedEmbankment =
                        EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, l1, l2);
                    if (mergedEmbankment != null)
                    {
                        embankmentDefinitions.Remove(l1);
                        embankmentDefinitions.Remove(l2);
                        embankmentDefinitions.Add(mergedEmbankment);
                    }

                    foreach (KeyValuePair<string, Embankment> c in ChannelToLeftEmbankments
                                                                   .Where(c => c.Value == l1).ToList())
                    {
                        if (ChannelToLeftEmbankments.ContainsKey(c.Key))
                        {
                            ChannelToLeftEmbankments[c.Key] = mergedEmbankment;
                        }
                    }

                    if (!connectedBranchesHaveOppositeDirection)
                    {
                        foreach (KeyValuePair<string, Embankment> c in ChannelToLeftEmbankments
                                                                       .Where(c => c.Value == l2).ToList())
                        {
                            if (ChannelToLeftEmbankments.ContainsKey(c.Key))
                            {
                                ChannelToLeftEmbankments[c.Key] = mergedEmbankment;
                            }
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, Embankment> c in ChannelToRightEmbankments
                                                                       .Where(c => c.Value == l2).ToList())
                        {
                            if (ChannelToRightEmbankments.ContainsKey(c.Key))
                            {
                                ChannelToRightEmbankments[c.Key] = mergedEmbankment;
                            }
                        }
                    }
                }

                if (r1 != null && r2 != null)
                {
                    Embankment mergedEmbankment =
                        EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, r1, r2);
                    if (mergedEmbankment != null)
                    {
                        embankmentDefinitions.Remove(r1);
                        embankmentDefinitions.Remove(r2);
                        embankmentDefinitions.Add(mergedEmbankment);
                    }

                    foreach (KeyValuePair<string, Embankment> c in ChannelToRightEmbankments
                                                                   .Where(c => c.Value == r1).ToList())
                    {
                        if (ChannelToRightEmbankments.ContainsKey(c.Key))
                        {
                            ChannelToRightEmbankments[c.Key] = mergedEmbankment;
                        }
                    }

                    if (!connectedBranchesHaveOppositeDirection)
                    {
                        foreach (KeyValuePair<string, Embankment> c in ChannelToRightEmbankments
                                                                       .Where(c => c.Value == r2).ToList())
                        {
                            if (ChannelToRightEmbankments.ContainsKey(c.Key))
                            {
                                ChannelToRightEmbankments[c.Key] = mergedEmbankment;
                            }
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<string, Embankment> c in ChannelToLeftEmbankments
                                                                       .Where(c => c.Value == r2).ToList())
                        {
                            if (ChannelToLeftEmbankments.ContainsKey(c.Key))
                            {
                                ChannelToLeftEmbankments[c.Key] = mergedEmbankment;
                            }
                        }
                    }
                }
            }
        }

        private static Coordinate GetPoint(double radiansUp, double radiansDn, Coordinate refPoint, double distance,
                                           int side)
        {
            var point = new Coordinate();
            point.Z = 0.0d; // Default value: NaN, which should be avoided. 

            double radiansSideUp = radiansUp + (Math.PI * side * 0.5);
            if (radiansSideUp < 0.0)
            {
                radiansSideUp = (Math.PI * 2.0) + radiansSideUp;
            }

            double radiansSideDn = radiansDn + (Math.PI * side * 0.5);
            if (radiansSideDn < 0.0)
            {
                radiansSideDn = (Math.PI * 2.0) + radiansSideDn;
            }

            double radiansSide = (radiansSideUp + radiansSideDn) / 2.0;

            double radians = Math.Abs(radiansSideUp - radiansSideDn);

            double length = distance / Math.Cos(radians / 2.0);

            point.X = (length * Math.Cos(radiansSide)) + refPoint.X;
            point.Y = (length * Math.Sin(radiansSide)) + refPoint.Y;

            return point;
        }

        private static int GetTurningDirection(Coordinate p1, Coordinate p2, Coordinate p3)
        {
            // See: http://en.wikipedia.org/wiki/Graham_scan
            // Three points are a counter-clockwise turn if ccw > 0, clockwise if
            // ccw < 0, and collinear if ccw = 0 because ccw is a determinant that
            // gives twice the signed  area of the triangle formed by p1, p2 and p3.
            // function ccw(p1, p2, p3):
            // return (p2.x - p1.x)*(p3.y - p1.y) - (p2.y - p1.y)*(p3.x - p1.x)

            double ccw = ((p2.X - p1.X) * (p3.Y - p1.Y)) - ((p2.Y - p1.Y) * (p3.X - p1.X));

            if (ccw > 0.0)
            {
                return -1;
            }

            if (ccw < 0.0)
            {
                return 1;
            }

            return 0;
        }

        private static Coordinate GetPointFromChainage(double startchainage, double chainage, Coordinate point1,
                                                       Coordinate point2)
        {
            var chainagePoint = new Coordinate();
            double factor = (chainage - startchainage) / point1.Distance(point2);

            if (Math.Abs(point2.X - point1.X) < 1.0E-8)
            {
                chainagePoint.X = point1.X;
            }
            else
            {
                chainagePoint.X = point1.X + ((point2.X - point1.X) * factor);
            }

            if (Math.Abs(point2.Y - point1.Y) < 1.0E-8)
            {
                chainagePoint.Y = point1.Y;
            }
            else
            {
                chainagePoint.Y = point1.Y + ((point2.Y - point1.Y) * factor);
            }

            return chainagePoint;
        }
    }
}
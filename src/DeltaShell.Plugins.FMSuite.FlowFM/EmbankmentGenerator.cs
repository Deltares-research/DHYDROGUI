using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public static class EmbankmentGenerator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EmbankmentGenerator));

        const int DoLeft = 1;
        const int DoRight = -1;

        private static Dictionary<string, Embankment> ChannelToLeftEmbankments { get; set; }
        private static Dictionary<string, Embankment> ChannelToRightEmbankments { get; set; }

        public static bool GenerateEmbankments(IList<Channel> branches, IList<Embankment> embankmentDefinitions, bool crossSectionBased, double constantDistance, bool generateLeftEmbankments, bool generateRightEmbankments, bool mergeAutomatically)
        {
            ChannelToLeftEmbankments = new Dictionary<string, Embankment>();
            ChannelToRightEmbankments = new Dictionary<string, Embankment>();

            bool result = false;
            if (!crossSectionBased)
            {
                result = GenerateEmbankmentsAtConstantDistance(branches, embankmentDefinitions, constantDistance, generateLeftEmbankments, generateRightEmbankments);
            }
            else
            {
                result = GenerateEmbankmentsBasedOnCrossSection(branches, embankmentDefinitions, generateLeftEmbankments, generateRightEmbankments);
            }
            if (mergeAutomatically)
            {
                MergeAllEmbankments(branches, embankmentDefinitions);
            }
            return result;

        }

        private static void MergeAllEmbankments(IList<Channel> branches, IList<Embankment> embankmentDefinitions)
        {
            var branchesMutableCopy = new List<Channel>(branches);
            foreach (var branch in branches)
            {
                var targetCount = (branch.Target.IncomingBranches == null ? 0 : branch.Target.IncomingBranches.Count) +
                                  (branch.Target.OutgoingBranches == null ? 0 : branch.Target.OutgoingBranches.Count);
                var sourceCount = (branch.Source.IncomingBranches == null ? 0 : branch.Source.IncomingBranches.Count) +
                                  (branch.Source.OutgoingBranches == null ? 0 : branch.Source.OutgoingBranches.Count);
                if (targetCount > 2 || sourceCount > 2)
                {
                    continue;
                }

                var connectedBranchesSameDirection =
                    branchesMutableCopy.Where(b => b.Source == branch.Target || b.Target == branch.Source);

                MergeBranchesToThisBranch(embankmentDefinitions, connectedBranchesSameDirection, branch);

                var connectedBranchesOppositeDirection =
                    branchesMutableCopy.Where(
                        b => b != branch &&
                            (b.Source == branch.Source|| b.Target == branch.Target));

                MergeBranchesToThisBranch(embankmentDefinitions, connectedBranchesOppositeDirection, branch, true);

                // remove embankment from branchesCopy and from lookup dictionary
                branchesMutableCopy.Remove(branch);
                ChannelToLeftEmbankments.Remove(branch.Name);
                ChannelToRightEmbankments.Remove(branch.Name);
            }
        }

        private static void MergeBranchesToThisBranch(IList<Embankment> embankmentDefinitions, IEnumerable<Channel> connectedBranches,
            Channel branch, bool connectedBranchesHaveOppositeDirection = false)
        {
            Embankment l1, l2, r1, r2;
            foreach (var cb in connectedBranches)
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
                    var mergedEmbankment = EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, l1, l2);
                    if (mergedEmbankment != null)
                    {
                        embankmentDefinitions.Remove(l1);
                        embankmentDefinitions.Remove(l2); 
                        embankmentDefinitions.Add(mergedEmbankment);
                    }

                    foreach (var c in ChannelToLeftEmbankments.Where(c => c.Value == l1).ToList())
                    {
                        if (ChannelToLeftEmbankments.ContainsKey(c.Key))
                        {
                            ChannelToLeftEmbankments[c.Key] = mergedEmbankment;
                        }
                    }
                    if (!connectedBranchesHaveOppositeDirection)
                    {
                        foreach (var c in ChannelToLeftEmbankments.Where(c => c.Value == l2).ToList())
                        {
                            if (ChannelToLeftEmbankments.ContainsKey(c.Key))
                            {
                                ChannelToLeftEmbankments[c.Key] = mergedEmbankment;
                            }
                        }
                    }
                    else
                    {
                        foreach (var c in ChannelToRightEmbankments.Where(c => c.Value == l2).ToList())
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
                    var mergedEmbankment = EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, r1, r2);
                    if (mergedEmbankment != null)
                    {
                        embankmentDefinitions.Remove(r1);
                        embankmentDefinitions.Remove(r2);
                        embankmentDefinitions.Add(mergedEmbankment);
                    }
                    foreach (var c in ChannelToRightEmbankments.Where(c => c.Value == r1).ToList())
                    {
                        if (ChannelToRightEmbankments.ContainsKey(c.Key))
                        {
                            ChannelToRightEmbankments[c.Key] = mergedEmbankment;
                        }
                    }
                    if (!connectedBranchesHaveOppositeDirection)
                    {
                        foreach (var c in ChannelToRightEmbankments.Where(c => c.Value == r2).ToList())
                        {
                            if (ChannelToRightEmbankments.ContainsKey(c.Key))
                            {
                                ChannelToRightEmbankments[c.Key] = mergedEmbankment;
                            }
                        }
                    }
                    else
                    {
                        foreach (var c in ChannelToLeftEmbankments.Where(c => c.Value == r2).ToList())
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

        public static bool GenerateEmbankmentsAtConstantDistance(IList<Channel> channels, IList<Embankment> embankmentDefinitions,
                                                           double distance, bool generateLeftEmbankment, bool generateRightEmbankment)
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

            foreach (var channel in channels)
            {
                var coordinatesLeft = new List<Coordinate>();
                var coordinatesRight = new List<Coordinate>();

                var leftEmbankment = new Embankment();
                var rightEmbankment = new Embankment();

                var radiansUp = 0.0;

                var prevRadians = 0.0;
                var prevPoint = channel.Geometry.Coordinates[0];
                var prevChainage = 0.0;
                var turningDirection = 0;  // -1: Left, 1:Right, 0:Straight

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

                        pointChainage = NetworkHelper.GetBranchFeatureChainageFromGeometry(channel, new Point(refPoint));
                    }
                    else
                    {
                        point1 = channel.Geometry.Coordinates[i - 1];
                        point2 = channel.Geometry.Coordinates[i];
                        refPoint = point2;

                        pointChainage = channel.Length;
                    }

                    var x = point2.X - point1.X;
                    var y = point2.Y - point1.Y;

                    var radiansDown = Math.Atan2(y, x);
                    if (radiansDown < 0.0) radiansDown = Math.PI * 2.0 + radiansDown;

                    if (i == channel.Geometry.Coordinates.Count() - 1)
                    {
                        // Skip if necessary according the Rodriguez Aguilera criterium (TOOLS-22145)
                        skipLeftPoint = false;
                        if (generateLeftEmbankment && turningDirection == -1)
                        {
                            var beta = (radiansDown - prevRadians) / 2.0;
                            skipLeftPoint = (pointChainage - prevChainage) / distance < Math.Tan(beta);
                        }

                        skipRightPoint = false;
                        if (generateRightEmbankment && turningDirection == 1)
                        {
                            var beta = (prevRadians - radiansDown) / 2.0;
                            skipRightPoint = (pointChainage - prevChainage) / distance < Math.Tan(beta);
                        }
                    }

                    if (i == 0 || i == channel.Geometry.Coordinates.Count() - 1)
                    {
                        radiansUp = radiansDown;
                    }

                    if (generateLeftEmbankment && !skipLeftPoint)
                    {
                        var leftPoint = GetPoint(radiansUp, radiansDown, refPoint, distance, DoLeft);
                        coordinatesLeft.Add(leftPoint);
                    }

                    if (generateRightEmbankment && !skipRightPoint)
                    {
                        var rightPoint = GetPoint(radiansUp, radiansDown, refPoint, distance, DoRight);
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
                    leftEmbankment.Name = NetworkHelper.GetUniqueName("Embankment{0:D2}", embankmentDefinitions, "Embankment");
                    embankmentDefinitions.Add(leftEmbankment);
                    ChannelToLeftEmbankments[channel.Name] = leftEmbankment;
                }
                if (generateRightEmbankment)
                {
                    rightEmbankment.Geometry = new LineString(coordinatesRight.ToArray());
                    rightEmbankment.Name = NetworkHelper.GetUniqueName("Embankment{0:D2}", embankmentDefinitions, "Embankment");
                    embankmentDefinitions.Add(rightEmbankment);
                    ChannelToRightEmbankments[channel.Name] = rightEmbankment;
                }
            }

            return true;
        }

        private static Coordinate GetPoint(double radiansUp, double radiansDn, Coordinate refPoint, double distance, int side)
        {

            Coordinate point = new Coordinate();
            point.Z = 0.0d;   // Default value: NaN, which should be avoided. 

            var radiansSideUp = radiansUp + (Math.PI * side * 0.5);
            if (radiansSideUp < 0.0) radiansSideUp = Math.PI * 2.0 + radiansSideUp;

            var radiansSideDn = radiansDn + (Math.PI * side * 0.5);
            if (radiansSideDn < 0.0) radiansSideDn = Math.PI * 2.0 + radiansSideDn;

            var radiansSide = (radiansSideUp + radiansSideDn) / 2.0;

            var radians = Math.Abs(radiansSideUp - radiansSideDn);

            var length = distance / Math.Cos(radians/2.0);

            point.X = length * Math.Cos(radiansSide) + refPoint.X;
            point.Y = length * Math.Sin(radiansSide) + refPoint.Y;

            return point;
        }

        private class CrossData
        {
            private double begChainage;
            private double endChainage;
            private int indexEndChain;
            private bool passedFirst;
            private bool lastReached;

            private List<double> leftSide;
            private List<double> rightSide;
            private List<double> leftHeight;
            private List<double> rightHeight;
            private List<double> chainage;

            public void Initialize(IChannel channel)
            {
                chainage = new List<double>();
                leftSide = new List<double>();
                rightSide = new List<double>();
                leftHeight = new List<double>();
                rightHeight = new List<double>();

                var sortedCrossSections = channel.CrossSections.Where(c => c.CrossSectionType == CrossSectionType.YZ ||
                                                                           c.CrossSectionType == CrossSectionType.ZW).OrderBy(c => c.Chainage).ToList();
                foreach (var cross in sortedCrossSections)
                {
                    chainage.Add(cross.Chainage);

                    leftSide.Add(Math.Abs(cross.Definition.Left - cross.Definition.Thalweg));
                    rightSide.Add(Math.Abs(cross.Definition.Left + cross.Definition.Width - cross.Definition.Thalweg));

                    leftHeight.Add(cross.Definition.LeftEmbankment);
                    rightHeight.Add(cross.Definition.RightEmbankment);
                }

                passedFirst= false;
                lastReached = false;
                begChainage = chainage.First();
                if (chainage.Count == 1)
                {
                    endChainage = begChainage;
                    indexEndChain = 0;
                    lastReached = true;
                }
                else
                {
                    endChainage = chainage.ElementAt(1);
                    indexEndChain = 1;
                    if (chainage.Count == 2)
                    {
                        lastReached = false;
                    }
                }

            }

            public bool GetValuesForChainage(ref double actualChainage, out double leftDistance, out double rightDistance, out double leftLevel, out double rightLevel)
            {

                // ExtraPolation before first Cross-Section
                if (actualChainage < chainage.First())
                {
                    leftDistance = leftSide.First();
                    rightDistance = rightSide.First();

                    leftLevel = leftHeight.First();
                    rightLevel = rightHeight.First();

                    return false;
                }

                // Cross-Section at begChainage
                if (Math.Abs(actualChainage - begChainage) < 1.0E-8)
                {
                    if (indexEndChain > 0)
                    {
                        leftDistance = leftSide.ElementAt(indexEndChain - 1);
                        rightDistance = rightSide.ElementAt(indexEndChain - 1);

                        leftLevel= leftHeight.ElementAt(indexEndChain - 1);
                        rightLevel = rightHeight.ElementAt(indexEndChain - 1);
                    }
                    else
                    {
                        leftDistance = leftSide.First();
                        rightDistance = rightSide.First();

                        leftLevel = leftHeight.First();
                        rightLevel = rightHeight.First();
                    }

                    passedFirst = true;
                    return false;
                }

                // First Pass begChainage
                if (actualChainage > begChainage && !passedFirst)
                {
                    actualChainage = begChainage;

                    leftDistance = leftSide.First();
                    rightDistance = rightSide.First();

                    leftLevel = leftHeight.First();
                    rightLevel = rightHeight.First();

                    passedFirst = true;
                    return true;
                }


                // Cross-Section at endChainage
                if (Math.Abs(actualChainage - endChainage) < 1.0E-8)
                {
                    leftDistance = leftSide.ElementAt(indexEndChain);
                    rightDistance = rightSide.ElementAt(indexEndChain);

                    leftLevel= leftHeight.ElementAt(indexEndChain);
                    rightLevel = rightHeight.ElementAt(indexEndChain);

                    if (indexEndChain == chainage.Count - 1)
                    {
                        lastReached = true;
                    }
                    return false;
                }

                // Extrapolation after last cross-section
                if (actualChainage > chainage.Last() && lastReached)
                {
                    leftDistance = leftSide.Last();
                    rightDistance = rightSide.Last();

                    leftLevel = leftHeight.Last();
                    rightLevel = rightHeight.Last();

                    return false;
                }

                // Move up te next cross-section
                if (actualChainage > endChainage  && !lastReached)
                {
                    // We passed a cross-section, add that to embankment
                    actualChainage = endChainage;
                    leftDistance = leftSide.ElementAt(indexEndChain);
                    rightDistance = rightSide.ElementAt(indexEndChain);

                    leftLevel= leftHeight.ElementAt(indexEndChain);
                    rightLevel = rightHeight.ElementAt(indexEndChain);

                    // Shift to next one if not at the end
                    if (indexEndChain < chainage.Count - 1)
                    {
                        begChainage = endChainage;
                        endChainage = chainage.ElementAt(++indexEndChain);
                    }
                    else
                    {
                        lastReached = true;
                    }

                     return true;
                }

                // Regular interpolation
                var fraction = 0.0;
                if (Math.Abs(endChainage - begChainage) > 1.0e-6)
                {
                    fraction = Math.Abs((actualChainage - begChainage) / (endChainage - begChainage));
                }

                leftDistance = leftSide.ElementAt(indexEndChain - 1) +
                                fraction * (leftSide.ElementAt(indexEndChain) - leftSide.ElementAt(indexEndChain - 1));

                rightDistance = rightSide.ElementAt(indexEndChain - 1) +
                                fraction * (rightSide.ElementAt(indexEndChain) - rightSide.ElementAt(indexEndChain - 1));

                leftLevel = leftHeight.ElementAt(indexEndChain - 1) +
                            fraction * (leftHeight.ElementAt(indexEndChain) - leftHeight.ElementAt(indexEndChain - 1));

                rightLevel = rightHeight.ElementAt(indexEndChain - 1) +
                                fraction * (rightHeight.ElementAt(indexEndChain) - rightHeight.ElementAt(indexEndChain - 1));

                return false;
            }

        }

        public static bool GenerateEmbankmentsBasedOnCrossSection(IList<Channel> channels, IList<Embankment> embankmentDefinitions,
                                                            bool generateLeftEmbankment, bool generateRightEmbankment)
        {

            foreach (var channel in channels)
            {

                var crossData = new CrossData();

                if (!channel.CrossSections.Any())
                {
                    Log.Warn(String.Format("Channel '{0}' has no cross-sections; no embankments created for this channel.", channel.Name));
                    continue;
                }

                var crossFound = channel.CrossSections.Any(c => c.CrossSectionType == CrossSectionType.YZ || c.CrossSectionType == CrossSectionType.ZW);

                if (!crossFound)
                {
                    Log.Warn(String.Format("No suitable cross-sectons found in channel '{0}'; no embankments created for this channel.", channel.Name));
                    continue;
                }


                var coordinatesLeft = new List<Coordinate>();
                var coordinatesRight = new List<Coordinate>();

                var leftEmbankment = new Embankment();
                var rightEmbankment = new Embankment();

                var radiansUp = 0.0;

                var prevRadians = 0.0;
                var prevPoint = channel.Geometry.Coordinates[0];
                var prevChainage = 0.0;
                var lastChainage = 0.0;
                var turningDirection = 0;  // -1: Left, 1:Right, 0:Straight

                crossData.Initialize(channel);

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
                        pointChainage = NetworkHelper.GetBranchFeatureChainageFromGeometry(channel, new Point(refPoint));
                    }
                    else
                    {
                        point1 = channel.Geometry.Coordinates[i - 1];
                        point2 = channel.Geometry.Coordinates[i];
                        refPoint = point2;
                        pointChainage = channel.Length;
                    }
                    
                    var x = point2.X - point1.X;
                    var y = point2.Y - point1.Y;

                    var radiansDown = Math.Atan2(y, x);
                    if (radiansDown < 0.0) radiansDown = Math.PI * 2.0 + radiansDown;

                    if (i == 0 || i == channel.Geometry.Coordinates.Count() - 1)
                    {
                        radiansUp = radiansDown;
                    }

                    var doAgain = true;

                    while (doAgain)
                    {
                        double leftDistance;
                        double leftHeight;
                        double rightDistance;
                        double rightHeight;

                        var actualChainage = pointChainage;
                        lastChainage = prevChainage;

                        doAgain = crossData.GetValuesForChainage(ref actualChainage, out leftDistance, out rightDistance,
                                                                 out leftHeight, out rightHeight);

                        // Skip if necessary according the Rodriguez Aguilera criterium (TOOLS-22145)
                        var skipLeftPoint = false;
                        if (generateLeftEmbankment && turningDirection == -1)
                        {
                            var beta = (radiansDown - prevRadians) / 2.0;
                            skipLeftPoint = (actualChainage - lastChainage) / leftDistance < Math.Tan(beta);
                        }

                        var skipRightPoint = false;
                        if (generateRightEmbankment && turningDirection == 1)
                        {
                            var beta = (prevRadians - radiansDown) / 2.0;
                            skipRightPoint = (actualChainage - lastChainage) / rightDistance < Math.Tan(beta);
                        }

                        if (doAgain)
                        {
                            var crossPoint = GetPointFromChainage(NetworkHelper.GetBranchFeatureChainageFromGeometry(channel, new Point(prevPoint)),
                                                                  actualChainage, prevPoint, refPoint);
                            
                            if (generateLeftEmbankment && !skipLeftPoint)
                            {
                                var leftPoint = GetPoint(radiansUp, radiansUp, crossPoint, leftDistance, DoLeft);
                                leftPoint.Z = leftHeight;
                                coordinatesLeft.Add(leftPoint);
                            }

                            if (generateRightEmbankment && !skipRightPoint)
                            {
                                var rightPoint = GetPoint(radiansUp, radiansUp, crossPoint, rightDistance, DoRight);
                                rightPoint.Z = rightHeight;
                                coordinatesRight.Add(rightPoint);
                            }

                            lastChainage = actualChainage;
                        }
                        else
                        {
                            if (generateLeftEmbankment && !skipLeftPoint)
                            {
                                var leftPoint = GetPoint(radiansUp, radiansDown, refPoint, leftDistance, DoLeft);
                                leftPoint.Z = leftHeight;
                                coordinatesLeft.Add(leftPoint);
                            }

                            if (generateRightEmbankment && !skipRightPoint)
                            {
                                var rightPoint = GetPoint(radiansUp, radiansDown, refPoint, rightDistance, DoRight);
                                rightPoint.Z = rightHeight;
                                coordinatesRight.Add(rightPoint);
                            }
                        }
                    }

                    turningDirection = 0;
                    if (i > 0 && i < channel.Geometry.Coordinates.Count() - 1)
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
                    leftEmbankment.Name = NetworkHelper.GetUniqueName("Embankment{0:D2}", embankmentDefinitions, "Embankment");
                    embankmentDefinitions.Add(leftEmbankment);
                }

                if (generateRightEmbankment)
                {
                    rightEmbankment.Geometry = new LineString(coordinatesRight.ToArray());
                    rightEmbankment.Name = NetworkHelper.GetUniqueName("Embankment{0:D2}", embankmentDefinitions, "Embankment");
                    embankmentDefinitions.Add(rightEmbankment);
                }
            }

            return true;
        }

        private static int GetTurningDirection(Coordinate p1, Coordinate p2, Coordinate p3)
        {
            // See: http://en.wikipedia.org/wiki/Graham_scan
            // Three points are a counter-clockwise turn if ccw > 0, clockwise if
            // ccw < 0, and collinear if ccw = 0 because ccw is a determinant that
            // gives twice the signed  area of the triangle formed by p1, p2 and p3.
            // function ccw(p1, p2, p3):
            // return (p2.x - p1.x)*(p3.y - p1.y) - (p2.y - p1.y)*(p3.x - p1.x)

            var ccw = (p2.X - p1.X)*(p3.Y - p1.Y) - (p2.Y - p1.Y)*(p3.X - p1.X);

            if (ccw > 0.0) return -1;

            if (ccw < 0.0) return 1;

            return 0;
        }

        private static Coordinate GetPointFromChainage(double startchainage, double chainage, Coordinate point1, Coordinate point2)
        {
            var chainagePoint = new Coordinate();
            var factor = (chainage - startchainage) / point1.Distance(point2);

            if (Math.Abs(point2.X - point1.X) < 1.0E-8)
            {
                chainagePoint.X = point1.X;
            }
            else
            {
                chainagePoint.X = point1.X + (point2.X - point1.X) * factor;
            }

            if (Math.Abs(point2.Y - point1.Y) < 1.0E-8)
            {
                chainagePoint.Y = point1.Y;
            }
            else
            {
                chainagePoint.Y = point1.Y + (point2.Y - point1.Y) * factor;
            }

            return chainagePoint;
        }
    }
}

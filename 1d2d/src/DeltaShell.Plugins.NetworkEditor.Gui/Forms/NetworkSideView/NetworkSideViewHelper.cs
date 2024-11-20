using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Functions;
using DelftTools.Functions.Binding;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Units;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    public static class NetworkSideViewHelper
    {
        public static bool GetReversed(Route route, IStructure1D structure)
        {
            INetworkSegment segment = RouteHelper.GetSegmentForNetworkLocation(route,
                                                                               new NetworkLocation(structure.Branch, structure.Chainage));
            return (segment != null) && (segment.EndChainage < segment.Chainage);
        }

        /// <summary>
        /// Makes sure we have something of a range even if max == min
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public static void UpdateMinMaxToEnsureVerticalResolution(ref double min, ref double max)
        {
            //we all ready have vertical resolution
            if (min != max)
            {
                return;
            }
            double fakeRange = min * 0.10;
            //unless this is 0..then we have to guess take five ;)
            if (fakeRange == 0)
                fakeRange = 5;
            min -= fakeRange;
            max += fakeRange;
        }

        public static ILineChartSeries GetLineSeries(IFunction function, 
                                                     IVariable xArgument, 
                                                     IVariable yComponent, 
                                                     FunctionBindingList functionBindingList, 
                                                     Color penColor)
        {
            ILineChartSeries lineSeries = ChartSeriesFactory.CreateLineSeries();

            lineSeries.DataSource = functionBindingList;

            // Set chart series data members and title
            lineSeries.Title = $"{function.Name} [{function.Components[0].Unit.Symbol}]";
            lineSeries.XValuesDataMember = xArgument.DisplayName; // x, double (offset along the route)
            lineSeries.YValuesDataMember = yComponent.DisplayName; // y, double (value)
            lineSeries.Color = penColor;
            lineSeries.PointerStyle = PointerStyles.Nothing;
            // areaSeries.PointerVisible: only visible effect is shapes are no longer filled with style brush
            // and legendline is thick
            lineSeries.CheckDataSource();
            lineSeries.UpdateASynchronously = true;
            return lineSeries;
        }

        public static IPointChartSeries GetPointSeries(IFunction function,
                                                       IVariable xArgument,
                                                       IVariable yComponent,
                                                       FunctionBindingList functionBindingList,
                                                       Color fillColor,
                                                       PointerStyles pointerStyles,
                                                       int pointerSize)
        {
            IPointChartSeries pointSeries = ChartSeriesFactory.CreatePointSeries();

            pointSeries.DataSource = functionBindingList;
            pointSeries.NoDataValues.Add(function.Components[0].NoDataValues.Count > 0 ? (double) function.Components[0].NoDataValues[0] : double.NaN);
            // Set chart series data members and title
            pointSeries.Title = $"{function.Name} [{function.Components[0].Unit.Symbol}]";
            pointSeries.XValuesDataMember = xArgument.DisplayName; // x, double (offset along the route)
            pointSeries.YValuesDataMember = yComponent.DisplayName; // y, double (value)
            pointSeries.Color = fillColor;
            pointSeries.LineColor = Color.Black;
            pointSeries.Style = pointerStyles;
            pointSeries.Size = pointerSize;
            // areaSeries.PointerVisible: only visible effect is shapes are no longer filled with style brush
            // and legendline is thick
            pointSeries.CheckDataSource();
            return pointSeries;
        }

        public static IAreaChartSeries GetAreaSeries(IFunction function, 
                                                     IVariable xArgument, 
                                                     IVariable yComponent, 
                                                     FunctionBindingList functionBindingList, 
                                                     Color fillColor)
        {
            IAreaChartSeries areaSeries = ChartSeriesFactory.CreateAreaSeries();

            // Set the data source
            
            areaSeries.DataSource = functionBindingList;

            // Set chart series data members and title
            areaSeries.Title = $"{function.Name} [{function.Components[0].Unit.Symbol}]";
            areaSeries.XValuesDataMember = xArgument.DisplayName; // x, double (offset along the route)
            areaSeries.YValuesDataMember = yComponent.DisplayName; // y, double (value)
            areaSeries.Color = fillColor;

            // <src-3.5.3700.30575 the upgrade to new version Teechart apparently changed the 
            //    default behaviour.>
            areaSeries.Color = fillColor;
            areaSeries.LineColor = TeeChartHelper.DarkenColor(fillColor, 60);
            // </src-3.5.3700.30575>

            areaSeries.PointerColor = fillColor;
            areaSeries.PointerStyle = PointerStyles.Nothing;
            // areaSeries.PointerVisible: only visible effect is shapes are no longer filled with style brush
            // and legendline is thick
            areaSeries.UpdateASynchronously = true;
            areaSeries.CheckDataSource();
            return areaSeries;
        }

        private static void ThrowWhenFunctionIsInvalid(IFunction function)
        {
            if (function == null)
            {
                throw new ArgumentException("Couldn't create the view because one of the functions is null / empty");
            }

            if (function.Arguments.Count == 0)
            {
                throw new ArgumentException("Couldn't create view because one of the functions doesnt contain arguments");
            }

            if (function.Components.Count == 0)
            {
                throw new ArgumentException("Couldn't create view because one of the functions doesnt contain components");
            }
        }

        private static void ThrowWhenFunctionVariableNamesAreInvalid(string argName, string compName)
        {
            if (string.IsNullOrEmpty(argName) || argName.Trim() == "")
            {
                throw new ArgumentException("Couldn't create view because one of the functions doesnt contain a valid argument name");
            }

            if (string.IsNullOrEmpty(compName) || compName.Trim() == "")
            {
                throw new ArgumentException("Couldn't create view because one of the functions doesnt contain a valid component name");
            }

            if (argName == compName)
            {
                throw new ArgumentException("Couldn't create view because one of the functions component name is the same as the argument name which is not allowed");
            }
        }

        public static void ValidateFunction(IFunction function)
        {
            ThrowWhenFunctionIsInvalid(function);

            IVariable xArgument = function.GetFirstArgumentVariableOfType<double>();
            if (xArgument == null)
            {
                throw new ArgumentException($"Couldn't create view because {nameof(function)} {function.Name} does not have a argument of type double.");
            }
            
            IVariable yComponent = function.GetFirstComponentVariableOfType<double>();
            if (yComponent == null)
            {
                throw new ArgumentException($"Couldn't create view because {nameof(function)} {function.Name} does not have a component of type double.");
            }

            ThrowWhenFunctionVariableNamesAreInvalid(xArgument.Name, yComponent.Name);
        }

        public static IEnumerable<Tuple<T, double>> GetNodesInRouteWithChainage<T>(Route route) where T : INode
        {
            var currentChainage = 0.0;

            for (int i = 1; i < route.Segments.Values.Count; i++)
            {
                INetworkSegment previousSegment = route.Segments.Values[i - 1];
                INetworkSegment currentSegment = route.Segments.Values[i];

                currentChainage += previousSegment.Length;

                if (previousSegment.Branch == currentSegment.Branch)
                    continue;

                INode previousNode = previousSegment.DirectionIsPositive
                                         ? previousSegment.Branch.Target
                                         : previousSegment.Branch.Source;

                INode currentNode = currentSegment.DirectionIsPositive
                                        ? currentSegment.Branch.Source
                                        : currentSegment.Branch.Target;

                if (currentNode == previousNode && currentNode is T typedNode)
                {
                    yield return new Tuple<T, double>(typedNode, currentChainage);
                }
            }
        }

        public static IEnumerable<IFunction> GetPipeSideViewFunctions(Route route)
        {
            if (route == null)
            {
                yield break;
            }

            var xValues = new List<double>();
            var yValuesTop = new List<double>();
            var yValuesBottom = new List<double>();

            var currentChainage = 0.0;
            IBranch previousPipe = null;

            for (int i = 0; i < route.Segments.Values.Count; i++)
            {
                INetworkSegment segment = route.Segments.Values[i];

                if (!(segment.Branch is ISewerConnection connection))
                    continue;

                if (previousPipe == connection)
                {
                    currentChainage += segment.Length;
                    continue;
                }

                previousPipe = connection;

                double levelSource;
                double levelTarget;

                if (connection is IPipe)
                {
                    levelSource = segment.DirectionIsPositive ? connection.LevelSource : connection.LevelTarget;
                    levelTarget = segment.DirectionIsPositive ? connection.LevelTarget : connection.LevelSource;
                }
                else
                {
                    levelSource = segment.DirectionIsPositive ? connection.SourceCompartment.BottomLevel : connection.TargetCompartment.BottomLevel;
                    levelTarget = segment.DirectionIsPositive ? connection.TargetCompartment.BottomLevel : connection.SourceCompartment.BottomLevel;
                }
                
                double pipeLength = connection.Length;

                if (i == 0)
                {
                    levelSource = GetLevelAtChainage(segment.Chainage, connection);
                    pipeLength = segment.DirectionIsPositive
                        ? pipeLength - segment.Chainage
                        : segment.Chainage;
                }

                if (i == route.Segments.Values.Count - 1)
                {
                    levelTarget = GetLevelAtChainage(segment.EndChainage, connection);
                    pipeLength = segment.DirectionIsPositive 
                        ? segment.EndChainage 
                        : pipeLength - segment.EndChainage;
                }

                double crossSectionHeight = connection is Pipe pipe ? (pipe.CrossSection?.Definition).HighestPoint : 0;

                xValues.Add(currentChainage);
                yValuesTop.Add(levelSource + crossSectionHeight);
                yValuesBottom.Add(levelSource);

                xValues.Add(currentChainage + pipeLength);
                yValuesTop.Add(levelTarget + crossSectionHeight);
                yValuesBottom.Add(levelTarget);

                currentChainage += segment.Length;
            }

            yield return CreateFunction(new Unit("meter", "m AD"), xValues, yValuesTop, "Pipe top");
            yield return CreateFunction(new Unit("meter", "m AD"), xValues, yValuesBottom, "Pipe bottom");
        }

        public static IFunction GetWaterLevelInPipeFunction(Route route, IFunction waterLevelInSideView)
        {
            var xValues = new List<double>();
            var yValues = new List<double>();

            IMultiDimensionalArray<double> relativeOffsets = waterLevelInSideView.Arguments[0].GetValues<double>();
            IMultiDimensionalArray<double> values = waterLevelInSideView.Components[0].GetValues<double>();
            
            SewerConnectionWaterLevelData previousData = null;

            for (int i = 0; i < relativeOffsets.Count; i++)
            {
                double relativeOffset = relativeOffsets[i];
                double value = values[i];

                Tuple<INetworkSegment, double> segmentChainage = GetBranchChainageFromRelativeLength(route, relativeOffset);
                INetworkSegment segment = segmentChainage?.Item1;
                double? chainage = segmentChainage?.Item2;

                if (segment == null || !(segment.Branch is ISewerConnection sewerConnection))
                    continue;

                double bottomLevelAtChainage = GetLevelAtChainage(chainage.Value, sewerConnection);
                var currentData = new SewerConnectionWaterLevelData(segment, bottomLevelAtChainage, value, relativeOffset);

                if (previousData != null)
                {
                    if (previousData.SewerConnection != currentData.SewerConnection)
                    {
                        double pipeBottomLevel = previousData.BranchSegment.DirectionIsPositive
                                                     ? previousData.SewerConnection.LevelTarget
                                                     : previousData.SewerConnection.LevelSource;

                        var dataEndPreviousPipe = new SewerConnectionWaterLevelData(previousData.BranchSegment, pipeBottomLevel, currentData.WaterLevel, currentData.RelativeOffset);

                        foreach (Coordinate coordinate in GetIntersectingCoordinates(previousData, dataEndPreviousPipe).OrderBy(c => c.X))
                        {
                            xValues.Add(coordinate.X);
                            yValues.Add(coordinate.Y);
                        }

                        xValues.Add(dataEndPreviousPipe.RelativeOffset);
                        yValues.Add(dataEndPreviousPipe.WaterLevelInSewerConnection);

                        previousData = dataEndPreviousPipe;
                    }
                    
                    IEnumerable<Coordinate> coordinatesToAdd = GetIntersectingCoordinates(previousData, currentData).Where(c => c != null);

                    foreach (var coordinate in coordinatesToAdd.OrderBy(c => c.X))
                    {
                        xValues.Add(coordinate.X);
                        yValues.Add(coordinate.Y);
                    }
                }

                xValues.Add(currentData.RelativeOffset);
                yValues.Add(currentData.WaterLevelInSewerConnection);

                previousData = currentData;
            }

            return CreateFunction(new Unit("meter", "m AD"), xValues, yValues, "Waterlevel in pipe");
        }

        internal static Tuple<INetworkSegment, double> GetBranchChainageFromRelativeLength(Route route, double relativeOffset)
        {
            var currentLength = 0.0;
            IMultiDimensionalArray<INetworkSegment> segments = route.Segments.Values;

            for (int i = 0; i < segments.Count; i++)
            {
                INetworkSegment segment = segments[i];

                double startSegment = currentLength;
                double endSegment = currentLength + segment.Length;

                bool onBeginSegment = Math.Abs(relativeOffset - startSegment) < 1e-8;
                bool onEndSegment = Math.Abs(relativeOffset - endSegment) < 1e-8;
                bool isOnSegment = relativeOffset >= startSegment && relativeOffset <= endSegment;

                if (!isOnSegment)
                {
                    currentLength += segment.Length;
                    continue;
                }

                if (onBeginSegment)
                {
                    return new Tuple<INetworkSegment, double>(segment, segment.Chainage);
                }

                if (onEndSegment)
                {
                    if (i == segments.Count - 1)
                    {
                        return new Tuple<INetworkSegment, double>(segment, segment.EndChainage);
                    }

                    currentLength += segment.Length;
                    segment = segments[i + 1];
                }

                double segmentOffset = relativeOffset - currentLength;
                double chainage = segment.DirectionIsPositive
                                      ? segment.Chainage + segmentOffset
                                      : segment.Chainage - segmentOffset;

                return new Tuple<INetworkSegment, double>(segment, chainage);

            }

            return null;
        }

        private static IEnumerable<Coordinate> GetIntersectingCoordinates(SewerConnectionWaterLevelData previousData, SewerConnectionWaterLevelData currentData)
        {
            ValueLocation previousLocation = previousData.ValueLocation;
            ValueLocation location = currentData.ValueLocation;

            var crossedTop = false;
            var crossedBottom = false;

            switch (previousLocation)
            {
                case ValueLocation.AboveSewerConnection when location != ValueLocation.AboveSewerConnection:
                    crossedTop = true;
                    crossedBottom = location == ValueLocation.BelowSewerConnection;
                    break;
                case ValueLocation.InsideSewerConnection when location != ValueLocation.InsideSewerConnection:
                    crossedTop = location == ValueLocation.AboveSewerConnection;
                    crossedBottom = location == ValueLocation.BelowSewerConnection;
                    break;
                case ValueLocation.BelowSewerConnection when location != ValueLocation.BelowSewerConnection:
                    crossedTop = location == ValueLocation.AboveSewerConnection;
                    crossedBottom = true;
                    break;
            }

            if (!crossedTop && !crossedBottom)
            {
                yield break;
            }

            var topLine = new LineString(new[]
            {
                new Coordinate(previousData.RelativeOffset, previousData.SewerConnectionTopLevel),
                new Coordinate(currentData.RelativeOffset, currentData.SewerConnectionTopLevel),
            });

            var bottomLine = new LineString(new[]
            {
                new Coordinate(previousData.RelativeOffset, previousData.SewerConnectionBottomLevel),
                new Coordinate(currentData.RelativeOffset, currentData.SewerConnectionBottomLevel),
            });

            var valueLine = new LineString(new[]
            {
                new Coordinate(previousData.RelativeOffset, previousData.WaterLevel),
                new Coordinate(currentData.RelativeOffset, currentData.WaterLevel),
            });

            if (crossedTop)
            {
                yield return topLine.Intersection(valueLine).Coordinate;
            }

            if (crossedBottom)
            {
                yield return bottomLine.Intersection(valueLine).Coordinate;
            }
        }

        public static void AddPipeSurfaceLevelsInRoute(Route route, INetworkCoverage coverage)
        {
            if (!route.Locations.Values.Any()) return;

            INetworkLocation[] existingLocations = coverage.Locations.Values.ToArray();
            
            foreach (IPipe pipe in route.Network.Branches.OfType<IPipe>())
            {
                if (pipe.Source is IManhole)
                {
                    var startLocationPipe = new NetworkLocation(pipe, 0);
                    SetLocationValue(coverage, startLocationPipe, existingLocations, pipe.SourceCompartment.SurfaceLevel);
                }

                if (pipe.Target is IManhole)
                {
                    var endLocationPipe = new NetworkLocation(pipe, pipe.Length);
                    SetLocationValue(coverage, endLocationPipe, existingLocations, pipe.TargetCompartment.SurfaceLevel);
                }
            }

            INetworkLocation startLocationRoute = route.Locations.Values[0];
            INetworkLocation endLocationRoute = route.Locations.Values.Last();

            if (startLocationRoute.Branch is IPipe startLocationRoutePipe)
            {
                coverage[startLocationRoute] = GetSurfaceLevelAtChainage(startLocationRoute.Chainage, startLocationRoutePipe);
            }

            if (endLocationRoute.Branch is IPipe endLocationRoutePipe)
            {
                coverage[endLocationRoute] = GetSurfaceLevelAtChainage(endLocationRoute.Chainage, endLocationRoutePipe);
            }
        }

        private static void SetLocationValue(IFunction coverage, INetworkLocation newLocation, IEnumerable<INetworkLocation> existingLocations, double newValue)
        {
            INetworkLocation[] locationsToReplace = existingLocations.Where(l => Equals(l, newLocation)).ToArray();

            // `coverage[newLocation] = newValue` will not replace an existing location but will add it again,
            // leading to duplicate locations in the coverage, which is invalid.
            if (locationsToReplace.Any())
            {
                IVariable locationVariable = coverage.Arguments[0];
                locationVariable.RemoveValues(new VariableValueFilter<INetworkLocation>(locationVariable, locationsToReplace));
            }

            coverage[newLocation] = newValue;
        }

        private static double GetSurfaceLevelAtChainage(double chainage, ISewerConnection connection)
        {
            // check for pipes between urban and rural networks
            if (connection.SourceCompartment == null || connection.TargetCompartment == null)
            {
                return double.NaN;
            }
            return GetPipeLevelAtChainage(connection, chainage, connection.SourceCompartment.SurfaceLevel, connection.TargetCompartment.SurfaceLevel);
        }

        private static double GetLevelAtChainage(double chainage, ISewerConnection connection)
        {
            return GetPipeLevelAtChainage(connection, chainage, connection.LevelSource, connection.LevelTarget);
        }

        private static double GetPipeLevelAtChainage(ISewerConnection connection, double chainage, double sourceLevel, double targetLevel)
        {
            double heightDiff = sourceLevel - targetLevel;
            double ratio = heightDiff / connection.Length;

            double newPipeLength = connection.Length - chainage;
            return (ratio * newPipeLength) + Math.Min(sourceLevel, targetLevel);
        }

        internal static IFunction CreateFunction(IUnit yUnit, List<double> xValues, List<double> yValues, string name)
        {
            var chainages = new Variable<double>("Chainage") {Unit = new Unit("Chainage", "m")};
            var yVar = new Variable<double>(name) {Unit = yUnit};

            FunctionHelper.SetValuesRaw<double>(chainages, xValues);
            FunctionHelper.SetValuesRaw<double>(yVar, yValues);

            IFunction function = new Function(name);
            
            function.Arguments.Add(chainages);
            function.Components.Add(yVar);
            
            return function;
        }
    }
}
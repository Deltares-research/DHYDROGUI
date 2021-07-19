using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Functions;
using DelftTools.Functions.Binding;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
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
        private enum ValueLocation
        {
            AbovePipe,
            InsidePipe,
            BelowPipe
        }

        private class PipeWaterLevelData
        {
            private double pipeBottomLevel;

            public IPipe Pipe { get; set; }

            public double RelativeOffset { get; set; }

            public ValueLocation ValueLocation { get; set; }

            public INetworkSegment Segment { get; set; }

            public double PipeBottomLevel
            {
                get { return pipeBottomLevel; }
                set
                {
                    pipeBottomLevel = value;
                    
                    if (Pipe != null)
                    {
                        PipeTopLevel = pipeBottomLevel + Pipe.CrossSectionDefinition.HighestPoint;
                    }
                }
            }

            public double PipeTopLevel { get; private set; }

            public double ValueInPipe { get; private set; }

            public double Value { get; set; }
            
            public void SetValueInPipe(double value)
            {
                if (value > PipeTopLevel)
                {
                    ValueLocation = ValueLocation.AbovePipe;
                    ValueInPipe = PipeTopLevel;
                    return;
                }

                if (value < PipeBottomLevel)
                {
                    ValueLocation = ValueLocation.BelowPipe;
                    ValueInPipe = pipeBottomLevel;
                    return;
                }

                ValueLocation = ValueLocation.InsidePipe;
                ValueInPipe = value;
            }
        }

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

        public static ILineChartSeries GetLineSeries(IFunction function, IVariable xArgument, IVariable yComponent, FunctionBindingList functionBindingList, Color penColor)
        {
            var lineSeries = ChartSeriesFactory.CreateLineSeries();

            lineSeries.DataSource = functionBindingList;

            // Set chart series data members and title
            lineSeries.Title = String.Format("{0} [{1}]", function.Name, function.Components[0].Unit.Symbol);
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

        public static IPointChartSeries GetPointSeries(IFunction function, IVariable xArgument, IVariable yComponent, FunctionBindingList functionBindingList, 
            Color fillColor, PointerStyles pointerStyles, int pointerSize)
        {
            var pointSeries = ChartSeriesFactory.CreatePointSeries();

            pointSeries.DataSource = functionBindingList;
            pointSeries.NoDataValues.Add(function.Components[0].NoDataValues.Count > 0 ? (double) function.Components[0].NoDataValues[0] : double.NaN);
            // Set chart series data members and title
            pointSeries.Title = String.Format("{0} [{1}]", function.Name, function.Components[0].Unit.Symbol);
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

        public static IAreaChartSeries GetAreaSeries(IFunction function, IVariable xArgument, IVariable yComponent, FunctionBindingList functionBindingList, Color fillColor)
        {
            var areaSeries = ChartSeriesFactory.CreateAreaSeries();

            // Set the data source
            
            areaSeries.DataSource = functionBindingList;

            // Set chart series data members and title
            areaSeries.Title = String.Format("{0} [{1}]", function.Name, function.Components[0].Unit.Symbol);
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

        public static void ThrowWhenFunctionIsInvalid(IFunction function)
        {
            if (function == null)
                throw new ArgumentException("Couldnt create the view because one of the functions is null / empty");

            if (function.Arguments.Count == 0)
                throw new ArgumentException("Couldnt create view because one of the functions doesnt contain arguments");

            if (function.Components.Count == 0)
                throw new ArgumentException("Couldnt create view because one of the functions doesnt contain components");
        }

        public static void ThrowWhenValueTypeIsNull(IVariable xArgument)
        {
            if (xArgument == null)
                throw new ArgumentException(
                    "Couldn't create view because argument is null.");
        }

        public static void ThrowWhenFunctionVariableNamesAreInvalid(string argName, string compName)
        {
            if (String.IsNullOrEmpty(argName) || argName.Trim() == "")
                throw new ArgumentException(
                    "Couldn't create view because one of the functions doesnt contain a valid argument name");

            if (String.IsNullOrEmpty(compName) || compName.Trim() == "")
                throw new ArgumentException(
                    "Couldn't create view because one of the functions doesnt contain a valid component name");

            if (argName == compName)
                throw new ArgumentException(
                    "Couldn't create view because one of the functions component name is the same as the argument name which is not allowed");
        }

        public static void ValidateFunction(IFunction function)
        {
            ThrowWhenFunctionIsInvalid(function);

            var xArgument = function.GetFirstArgumentVariableOfType<double>();
            if (xArgument == null)
            {
                throw new ArgumentException(
                    String.Format("Couldn't create view because function {0} does not have a argument of type double.",function.Name));
            }

            
            var yComponent = function.GetFirstComponentVariableOfType<double>();
            if (yComponent == null)
            {
                throw new ArgumentException(
                    String.Format("Couldn't create view because function {0} does not have a component of type double.",function.Name));
            }

            ThrowWhenFunctionVariableNamesAreInvalid(xArgument.Name, yComponent.Name);
        }

        public static IEnumerable<Tuple<T, double>> GetNodesInRouteWithChainage<T>(Route route) where T : INode
        {
            var currentChainage = 0.0;

            for (int i = 1; i < route.Segments.Values.Count; i++)
            {
                var previousSegment = route.Segments.Values[i - 1];
                var currentSegment = route.Segments.Values[i];

                currentChainage += previousSegment.Length;

                if (previousSegment.Branch == currentSegment.Branch)
                    continue;

                var previousNode = previousSegment.DirectionIsPositive
                    ? previousSegment.Branch.Target
                    : previousSegment.Branch.Source;

                var currentNode = currentSegment.DirectionIsPositive
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
                yield break;

            var xValues = new List<double>();
            var yValuesTop = new List<double>();
            var yValuesBottom = new List<double>();

            var currentChainage = 0.0;
            IBranch previousPipe = null;

            for (int i = 0; i < route.Segments.Values.Count; i++)
            {
                var segment = route.Segments.Values[i];

                if (!(segment.Branch is ISewerConnection connection))
                    continue;

                if (previousPipe == connection)
                {
                    currentChainage += segment.Length;
                    continue;
                }

                previousPipe = connection;

                var levelSource = segment.DirectionIsPositive ? connection.LevelSource : connection.LevelTarget;
                var levelTarget = segment.DirectionIsPositive ? connection.LevelTarget : connection.LevelSource;
                var pipeLength = connection.Length;

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

                var crossSectionHeight = connection is Pipe pipe ? pipe.CrossSectionDefinition.HighestPoint : 0;

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

            var relativeOffsets = waterLevelInSideView.Arguments[0].GetValues<double>();
            var values = waterLevelInSideView.Components[0].GetValues<double>();
            
            PipeWaterLevelData previousData = null;

            for (int i = 0; i < relativeOffsets.Count; i++)
            {
                var relativeOffset = relativeOffsets[i];
                var value = values[i];

                var segmentChainage = GetBranchChainageFromRelativeLength(route, relativeOffset);
                var segment = segmentChainage?.Item1;
                var chainage = segmentChainage?.Item2;

                if (segment == null || !(segment.Branch is IPipe pipe))
                    continue;

                var currentData = new PipeWaterLevelData
                {
                    Pipe = pipe,
                    Value = value,
                    RelativeOffset = relativeOffset,
                    Segment = segment,
                    PipeBottomLevel = GetLevelAtChainage(chainage.Value, pipe)
                };

                currentData.SetValueInPipe(value);

                if (previousData != null)
                {
                    if (previousData.Pipe != currentData.Pipe)
                    {
                        var pipeBottomLevel = previousData.Segment.DirectionIsPositive
                            ? previousData.Pipe.LevelTarget
                            : previousData.Pipe.LevelSource;

                        var dataEndPreviousPipe = new PipeWaterLevelData
                        {
                            Pipe = previousData.Pipe,
                            Value = currentData.Value,
                            Segment = previousData.Segment,
                            RelativeOffset = currentData.RelativeOffset,
                            PipeBottomLevel = pipeBottomLevel
                        };

                        dataEndPreviousPipe.SetValueInPipe(currentData.Value);

                        foreach (var coordinate in GetIntersectingCoordinates(previousData, dataEndPreviousPipe).OrderBy(c => c.X))
                        {
                            xValues.Add(coordinate.X);
                            yValues.Add(coordinate.Y);
                        }

                        xValues.Add(dataEndPreviousPipe.RelativeOffset);
                        yValues.Add(dataEndPreviousPipe.ValueInPipe);

                        previousData = dataEndPreviousPipe;
                    }
                    
                    var coordinatesToAdd = GetIntersectingCoordinates(previousData, currentData).Where(c => c != null);

                    foreach (var coordinate in coordinatesToAdd.OrderBy(c => c.X))
                    {
                        xValues.Add(coordinate.X);
                        yValues.Add(coordinate.Y);
                    }
                }

                xValues.Add(currentData.RelativeOffset);
                yValues.Add(currentData.ValueInPipe);

                previousData = currentData;
            }

            return CreateFunction(new Unit("meter", "m AD"), xValues, yValues, "Waterlevel in pipe");
        }

        internal static Tuple<INetworkSegment, double> GetBranchChainageFromRelativeLength(Route route, double relativeOffset)
        {
            var currentLength = 0.0;
            var segments = route.Segments.Values;

            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];

                var startSegment = currentLength;
                var endSegment = currentLength + segment.Length;

                var onBeginSegment = Math.Abs(relativeOffset - startSegment) < 1e-8;
                var onEndSegment = Math.Abs(relativeOffset - endSegment) < 1e-8;
                var isOnSegment = relativeOffset >= startSegment && relativeOffset <= endSegment;

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

                var segmentOffset = relativeOffset - currentLength;
                var chainage = segment.DirectionIsPositive
                    ? segment.Chainage + segmentOffset
                    : segment.Chainage - segmentOffset;

                return new Tuple<INetworkSegment, double>(segment, chainage);

            }

            return null;
        }

        private static IEnumerable<Coordinate> GetIntersectingCoordinates(PipeWaterLevelData previousData, PipeWaterLevelData currentData)
        {
            var previousLocation = previousData.ValueLocation;
            var location = currentData.ValueLocation;

            var crossedTop = false;
            var crossedBottom = false;

            switch (previousLocation)
            {
                case ValueLocation.AbovePipe when location != ValueLocation.AbovePipe:
                    crossedTop = true;
                    crossedBottom = location == ValueLocation.BelowPipe;
                    break;
                case ValueLocation.InsidePipe when location != ValueLocation.InsidePipe:
                    crossedTop = location == ValueLocation.AbovePipe;
                    crossedBottom = location == ValueLocation.BelowPipe;
                    break;
                case ValueLocation.BelowPipe when location != ValueLocation.BelowPipe:
                    crossedTop = location == ValueLocation.AbovePipe;
                    crossedBottom = true;
                    break;
            }

            if (crossedTop || crossedBottom)
            {
                var topLine = new LineString(new[]
                {
                    new Coordinate(previousData.RelativeOffset, previousData.PipeTopLevel),
                    new Coordinate(currentData.RelativeOffset, currentData.PipeTopLevel),
                });

                var bottomLine = new LineString(new[]
                {
                    new Coordinate(previousData.RelativeOffset, previousData.PipeBottomLevel),
                    new Coordinate(currentData.RelativeOffset, currentData.PipeBottomLevel),
                });

                var valueLine = new LineString(new[]
                {
                    new Coordinate(previousData.RelativeOffset, previousData.Value),
                    new Coordinate(currentData.RelativeOffset, currentData.Value),
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
        }

        public static void AddPipeSurfaceLevelsInRoute(Route route, INetworkCoverage coverage)
        {
            if (!route.Locations.Values.Any()) return;

            foreach (var pipe in route.Network.Branches.OfType<IPipe>())
            {
                if (pipe.Source is Manhole)
                {
                    var startLocationPipe = new NetworkLocation(pipe, 0);
                    coverage[startLocationPipe] = pipe.SourceCompartment.SurfaceLevel;
                }

                if (pipe.Target is Manhole)
                {
                    var endLocationPipe = new NetworkLocation(pipe, pipe.Length);
                    coverage[endLocationPipe] = pipe.TargetCompartment.SurfaceLevel;
                }
            }

            var startLocationRoute = route.Locations.Values[0];
            var endLocationRoute = route.Locations.Values[route.Locations.Values.Count - 1];

            if (startLocationRoute.Branch is IPipe startLocationRoutePipe)
                coverage[startLocationRoute] = GetSurfaceLevelAtChainage(startLocationRoute.Chainage, startLocationRoutePipe);

            if (endLocationRoute.Branch is IPipe endLocationRoutePipe)
                coverage[endLocationRoute] = GetSurfaceLevelAtChainage(endLocationRoute.Chainage, endLocationRoutePipe);
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
            var heightDiff = sourceLevel - targetLevel;
            var ratio = heightDiff / connection.Length;

            var newPipeLength = connection.Length - chainage;
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
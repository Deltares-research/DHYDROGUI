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
using NetTopologySuite.Extensions.Coverages;

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
            // areaSeries.PointerVisible = false;  
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
            // areaSeries.PointerVisible = false;  
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
            // areaSeries.PointerVisible = false;
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
            if (xArgument == null)
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

                if (!(segment.Branch is IPipe pipe))
                    continue;

                if (previousPipe == pipe)
                {
                    currentChainage += segment.Length;
                    continue;
                }

                previousPipe = pipe;

                var levelSource = segment.DirectionIsPositive ? pipe.LevelSource : pipe.LevelTarget;
                var levelTarget = segment.DirectionIsPositive ? pipe.LevelTarget : pipe.LevelSource;
                var pipeLength = pipe.Length;

                if (i == 0)
                {
                    levelSource = GetLevelAtChainage(segment.Chainage, pipe);
                    pipeLength = segment.DirectionIsPositive
                        ? pipeLength - segment.Chainage
                        : segment.Chainage;
                }

                if (i == route.Segments.Values.Count - 1)
                {
                    levelTarget = GetLevelAtChainage(segment.EndChainage, pipe);
                    pipeLength = segment.DirectionIsPositive 
                        ? segment.EndChainage 
                        : pipeLength - segment.EndChainage;
                }

                var crossSectionHeight = pipe.CrossSectionDefinition.HighestPoint;

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

        private static double GetLevelAtChainage(double chainage, IPipe pipe)
        {
            var heightDiff = pipe.LevelSource - pipe.LevelTarget;
            var ratio = heightDiff / pipe.Length;

            var newPipeLength = pipe.Length - chainage;
            return (ratio * newPipeLength) + Math.Min(pipe.LevelSource, pipe.LevelTarget);
        }

        private static IFunction CreateFunction(IUnit yUnit, List<double> xValues, List<double> yValues, string name)
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
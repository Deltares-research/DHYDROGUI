using System;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Functions;
using DelftTools.Functions.Binding;
using DelftTools.Hydro;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    public static class NetworkSideViewHelper
    {
        public static bool GetReversed(Route route, IStructure structure)
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
    }
}
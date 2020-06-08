using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.Link1d2d
{
    public class Links1D2DCoverage : FeatureCoverage
    {
        private IEventedList<ILink1D2D> links;
        private IDiscretization discretization;
        private UnstructuredGrid grid;
        private const string LinkIndexVariableName = "link_index";

        public IDiscretization Discretization
        {
            get { return discretization; }
            set { discretization = value; }
        }

        public UnstructuredGrid Grid
        {
            get { return grid; }
            set { grid = value; }
        }

        public virtual IEventedList<ILink1D2D> Links
        {
            get { return links; }
            set
            {
                links = value;
                AfterLinksSet();
            }
        }

        [EditAction]
        private void AfterLinksSet()
        {
            return;
        }
        protected Links1D2DCoverage() { }

        public Links1D2DCoverage(IEventedList<ILink1D2D> links, UnstructuredGrid grid, IDiscretization discretization, bool timeDependent)
        {
            Links = links;
            Grid = grid;
            Discretization = discretization;

            if (timeDependent)
            {
                Arguments.Add(new Variable<DateTime>(Name = "datetime")); // todo: add more info
                
            }
            var linkArgument = new Variable<int> { Name = LinkIndexVariableName };
            if (links != null)
            {
                linkArgument.SetValues(Enumerable.Range(0, links.Count()));
            }
            Arguments.Add(linkArgument);

            Components.Add(new Variable<double> { Name = "value" });
        }
        public override void Clear()
        {
            base.Clear();
            var linkArgument = Arguments.First(a => a.Name == LinkIndexVariableName);
            linkArgument.SetValues(Enumerable.Range(0, links?.Count() ?? 0));
        }
        public override object Evaluate(Coordinate coordinate)
        {
            return Evaluate(coordinate, null);
        }

        public override T Evaluate<T>(Coordinate coordinate)
        {
            return (T)Evaluate(coordinate, null);
        }

        public override T Evaluate<T>(double x, double y)
        {
            return (T)Evaluate(new Coordinate(x, y), null);
        }

        public IEnumerable<Coordinate> GetCoordinatesForLinks(IEnumerable<ILink1D2D> links)
        {
            return links == null
                ? Enumerable.Empty<Coordinate>()
                : Links.Select(l1d2d => l1d2d.Geometry.Centroid.Coordinate);
        }

        public virtual IEnumerable<Coordinate> Coordinates
        {
            get { return GetCoordinatesForLinks(Links); }
        }

        public override object Evaluate(Coordinate coordinate, DateTime? time)
        {
            var featureIndex = GetFeatureIndexAtCoordinate(coordinate);

            if (featureIndex == -1)
                return Components[0].NoDataValue;

            var featureFilter = new VariableValueFilter<int>(Arguments.Last(), featureIndex);

            if (IsTimeDependent && time.HasValue)
            {
                var timeFilter = new VariableValueFilter<DateTime>(Time, time.Value);
                var timeFilteredValues = GetValues(timeFilter, featureFilter);
                return timeFilteredValues == null || timeFilteredValues.Count == 0 ? Components[0].NoDataValue : timeFilteredValues[0];
            }

            var values = GetValues(featureFilter);
            return values == null || values.Count == 0 ? Components[0].NoDataValue : values[0];
        }
        public int GetFeatureIndexAtCoordinate(Coordinate coordinate)
        {
            if (Links == null || Grid == null || Discretization == null) return -1;
            var link1D2DIndex = Links.IndexOfNearest1D2DLink(coordinate);
            if (link1D2DIndex < 0) return -1;
            var link1d2d = Links.ElementAt(link1D2DIndex);
            if (coordinate.Equals2D(link1d2d.Geometry.Centroid.Coordinate)) return link1D2DIndex;
            if (link1d2d.FaceIndex < 0 || link1d2d.DiscretisationPointIndex < 0) return -1;
            var sourceCell = Grid.Cells[link1d2d.FaceIndex].ToPolygon(Grid);
            var cellIndexForCoordinate = Grid.GetCellIndexForCoordinate(Discretization.Locations.Values[link1d2d.DiscretisationPointIndex].Geometry.Coordinate);
            if (!cellIndexForCoordinate.HasValue) return -1;
            var targetCell = Grid.Cells[cellIndexForCoordinate.Value].ToPolygon(Grid);
            var point = new Point(coordinate);
            return point.Within(sourceCell) || point.Within(targetCell) ? link1D2DIndex : -1;
            
        }
        
        public override object Clone()
        {
            var clone = (Links1D2DCoverage)base.Clone();
            clone.Links = Links;
            clone.Grid = Grid;
            clone.Discretization = Discretization;
            return clone;
        }

        public override IFunction GetTimeSeries(Coordinate coordinate)
        {
            var featureIndexAtCoordinate = GetFeatureIndexAtCoordinate(coordinate);
            return GetTimeSeries(Links[featureIndexAtCoordinate].DiscretisationPointIndex, Links[featureIndexAtCoordinate].FaceIndex, featureIndexAtCoordinate, Links[featureIndexAtCoordinate].TypeOfLink, LinkIndexVariableName, (dIndex, fIndex) => links.First(l => l.DiscretisationPointIndex == dIndex && l.FaceIndex == fIndex).Link1D2DIndex);
        }

        public override IFunction GetTimeSeries(IFeature feature)
        {
            return GetTimeSeries(feature, LinkIndexVariableName, (dIndex, fIndex) => links.First(l => l.DiscretisationPointIndex == dIndex && l.FaceIndex == fIndex).Link1D2DIndex);
        }
        protected IFunction GetTimeSeries(int discretisationPointIndex, int faceIndex, int variableIndex, LinkStorageType featureType, string indexVariableName, Func<int, int, int> getVariableIndexFromGridFeatureIndex = null)
        {
            return Grid != null && Discretization != null && Links?.Count == 0
                ? GetTimeSeries(new Link1D2D(discretisationPointIndex, faceIndex, indexVariableName)
                {
                    Link1D2DIndex = variableIndex,
                    TypeOfLink = featureType
                }, indexVariableName, getVariableIndexFromGridFeatureIndex)
                : null;
        }

        protected IFunction GetTimeSeries(IFeature feature, string indexVariableName, Func<int, int, int> getVariableIndexFromDiscretizationPointIndexAndFaceIndex = null)
        {
            if (!IsTimeDependent || Time.Values.Count == 0) return null;

            var link1D2D = (ILink1D2D)feature;
            if (link1D2D.Link1D2DIndex < 0 || link1D2D.DiscretisationPointIndex < 0 || link1D2D.FaceIndex < 0 ) return null;

            var comp = Components[0];
            if (comp == null) return null;

            var timeSeries = new TimeSeries { Name = Name };

            var unit = (IUnit)(comp.Unit == null ? null : comp.Unit.Clone());
            timeSeries.Components.Add(new Variable<double>(comp.Name, unit));

            var indexVariable = Arguments.FirstOrDefault(a => a.Name == indexVariableName) ?? Arguments[1];
            var featureIdex = getVariableIndexFromDiscretizationPointIndexAndFaceIndex != null ? getVariableIndexFromDiscretizationPointIndexAndFaceIndex(link1D2D.DiscretisationPointIndex, link1D2D.FaceIndex) : link1D2D.Link1D2DIndex;
            var values = GetValues<double>(new VariableValueFilter<int>(indexVariable, featureIdex));

            timeSeries.Time.SetValues(Time.AllValues);
            timeSeries.SetValues(values);

            return timeSeries;
        }
    }
}

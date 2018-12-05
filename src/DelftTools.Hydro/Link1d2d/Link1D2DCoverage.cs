using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.Link1d2d
{
    public class Links1D2DCoverage : Coverage
    {
        private IEnumerable<ILink1D2D> links;
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

        public virtual IEnumerable<ILink1D2D> Links
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

        protected Links1D2DCoverage(IEnumerable<ILink1D2D> links, bool timeDependent)
        {
            Links = links;

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
            if (Links == null) return -1;
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
            return clone;
        }

    }
}

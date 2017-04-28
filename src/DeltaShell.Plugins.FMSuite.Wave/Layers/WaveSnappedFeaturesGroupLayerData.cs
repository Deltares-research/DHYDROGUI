using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Extensions.Features;
using SharpMap.Data.Providers;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.FMSuite.Wave.Layers
{
    public class WaveSnappedFeaturesGroupLayerData
    {
            private readonly WaveModel model;

            public WaveSnappedFeaturesGroupLayerData(WaveModel model)
            {
                this.model = model;
            }

            public IEnumerable<FeatureCollection> ChildData
            {
                get
                {
                    // todo: we pass the coordinate systme here once..that's a problem if the coordinate system changes along the way
                    //yield return new SnappedFeatureCollection(model, model.CoordinateSystem, (IList)model.Boundaries,
                                                              //WaveModelLayerStyles.BoundaryStyle, "Boundaries");
                    yield return new WaveBoundaryPointsFeatureCollection(model.Boundaries, model.BoundaryConditions, model.CoordinateSystem);
                }
            }
    }

    public class WaveBoundaryPointsFeatureCollection : FeatureCollection
    {
        private readonly IList<Feature2D> boundaries;
        private readonly IEventedList<WaveBoundaryCondition> boundaryConditions;

        public WaveBoundaryPointsFeatureCollection(IList<Feature2D> boundaries, IEventedList<WaveBoundaryCondition> boundaryConditions, ICoordinateSystem coordinateSystem)
        {
            this.boundaries = boundaries;
            this.boundaryConditions = boundaryConditions;
            this.CoordinateSystem = coordinateSystem;
            FeatureType = typeof (Feature2DPoint);
        }

        public override IList Features
        {
            get { return boundaries.OfType<Feature2D>().SelectMany(BoundaryToBoundaryPoints).ToList(); }
            set
            {
                throw new NotImplementedException();
            }
        }

        private IEnumerable<Feature2DPoint> BoundaryToBoundaryPoints(Feature2D boundary)
        {
            if (boundary.Geometry == null) yield break;
            
            var boundaryGeometry = (ILineString) boundary.Geometry;
            var indexed = new LengthIndexedLine(boundaryGeometry);

            var condition = boundaryConditions.FirstOrDefault(bc => Equals(bc.Feature, boundary));

            if (condition == null)
                yield break;

            int i = 0;
            foreach (var coord in boundaryGeometry.Coordinates)
            {
                var hasData = (condition.DataType != BoundaryConditionDataType.SpectrumFromFile &&
                               condition.GetDataAtPoint(i) != null)
                              || condition.SpectrumFiles.ContainsKey(i);
                var chainage = indexed.IndexOf(coord);
                var chainageString = hasData & !condition.IsHorizontallyUniform ? chainage.ToString("F2") : "";

                yield return new Feature2DPoint {Geometry = new Point(coord), Name = chainageString};
                i++;
            }
        }
    }
}
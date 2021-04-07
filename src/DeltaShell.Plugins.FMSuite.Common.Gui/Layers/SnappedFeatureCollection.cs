using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Layers
{
    public class SnappedFeatureCollection : FeatureCollection
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SnappedFeatureCollection));
        private bool dirty;

        private IList originalFeatures;

        /// <summary>
        /// A <see cref="FeatureCollection"/> for <see cref="IFeature"/> objects that are being
        /// snapped with <see cref="IGridOperationApi"/>.
        /// </summary>
        /// <param name="operationApi"> snap api </param>
        /// <param name="originalFeatures">
        /// Expected to be a collection of <see cref="IFeature"/>, where collection implements
        /// <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>.
        /// </param>
        /// <param name="originalFeaturesLayerStyle">
        /// Style of the layer from which <paramref name="originalFeatures"/> are coming
        /// from.
        /// </param>
        /// <param name="layerName"> Name of the layer. </param>
        /// <param name="snapApiFeatureType"> The feature type name for the snapping api </param>
        public SnappedFeatureCollection(IGridOperationApi operationApi, ICoordinateSystem coordinateSystem,
                                        IList originalFeatures, VectorStyle originalFeaturesLayerStyle,
                                        string layerName, string snapApiFeatureType)
        {
            OperationApi = operationApi;
            FeatureType = typeof(Feature2D);
            CoordinateSystem = coordinateSystem;

            OriginalFeatures = originalFeatures;
            OriginalFeaturesLayerStyle = originalFeaturesLayerStyle;
            LayerName = layerName;
            SnapApiFeatureType = snapApiFeatureType;
            SnappedFeatures = new List<Feature2D>();
            dirty = true;
        }

        public override IList Features
        {
            get
            {
                if (LayerIsShown && dirty)
                {
                    try
                    {
                        CalculateSnappedFeatures();
                        dirty = false;
                    }
                    catch (Exception)
                    {
                        // gulp
                        Layer.Visible = false;
                        dirty = true;
                    }
                }

                return SnappedFeatures;
            }
        }

        public string LayerName { get; private set; }
        public string SnapApiFeatureType { get; private set; }

        public VectorStyle SnappedLayerStyle
        {
            get
            {
                var snappedStyle = (VectorStyle)OriginalFeaturesLayerStyle.Clone();

                if (typeof(IPoint).IsAssignableFrom(snappedStyle.GeometryType))
                {
                    if (snappedStyle.HasCustomSymbol)
                    {
                        snappedStyle.Symbol = null; //reset
                    }

                    snappedStyle.GeometryType = typeof(ILineString);
                    snappedStyle.Line = new Pen(Color.Gray) { EndCap = LineCap.DiamondAnchor };
                }
                else if (typeof(IMultiPoint).IsAssignableFrom(snappedStyle.GeometryType))
                {
                    snappedStyle.GeometryType = typeof(IMultiLineString);
                    snappedStyle.Line = new Pen(Color.Gray) { EndCap = LineCap.Round };
                }
                else if (typeof(ILineString).IsAssignableFrom(snappedStyle.GeometryType))
                {
                    snappedStyle.Line = AdjustPenTransparency(snappedStyle.Line, 64);
                }
                else if (typeof(IPolygon).IsAssignableFrom(snappedStyle.GeometryType))
                {
                    snappedStyle.Outline = AdjustPenTransparency(snappedStyle.Outline, 128);
                    snappedStyle.Fill = AdjustBrushTransparency(snappedStyle.Fill, 128);
                }
                else
                {
                    throw new NotImplementedException();
                }

                return snappedStyle;
            }
        }

        public IGridOperationApi OperationApi { get; set; }

        public ILayer Layer { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Layer = null;
                OriginalFeatures = null;
            }

            base.Dispose(disposing);
        }

        private VectorStyle OriginalFeaturesLayerStyle { get; set; }
        private List<Feature2D> SnappedFeatures { get; set; }

        private IList OriginalFeatures
        {
            get => originalFeatures;
            set
            {
                if (originalFeatures != null)
                {
                    ((INotifyCollectionChanged)originalFeatures).CollectionChanged -=
                        OriginalFeaturesCollectionChanged;
                    ((INotifyPropertyChanged)originalFeatures).PropertyChanged -= OriginalFeaturesPropertyChanged;
                }

                originalFeatures = value;

                if (originalFeatures != null)
                {
                    ((INotifyCollectionChanged)originalFeatures).CollectionChanged +=
                        OriginalFeaturesCollectionChanged;
                    ((INotifyPropertyChanged)originalFeatures).PropertyChanged += OriginalFeaturesPropertyChanged;
                }
            }
        }

        private bool LayerIsShown =>
            Layer != null && Layer.Map != null && Layer.Map.GetAllVisibleLayers(false).Contains(Layer);

        private void OriginalFeaturesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (dirty)
            {
                return; //list already dirty, so don't update
            }

            if (e.PropertyName == "Geometry")
            {
                int indexInOriginal = OriginalFeatures.IndexOf(sender);
                if (indexInOriginal >= 0)
                {
                    SnappedFeatures.RemoveAt(indexInOriginal);
                    var originalFeature = (IFeature)OriginalFeatures[indexInOriginal];
                    Feature2D snappedFeature = GetSnappedFeature(originalFeature);
                    SnappedFeatures.Insert(indexInOriginal, snappedFeature);
                }

                FireFeaturesChanged();
            }
        }

        private Feature2D GetSnappedFeature(IFeature feature, IGeometry snappedGeometry = null)
        {
            if (snappedGeometry == null || snappedGeometry.IsEmpty)
            {
                try
                {
                    snappedGeometry = OperationApi.GetGridSnappedGeometry(SnapApiFeatureType, feature.Geometry);
                    if (snappedGeometry == null || snappedGeometry.IsEmpty)
                    {
                        Log.WarnFormat(
                            Resources
                                .SnappedFeatureCollection_GetSnappedFeature_No_snapped_geometry_was_generated_for_type__0__,
                            feature.Geometry.GeometryType);
                    }
                }
                catch (Exception e)
                {
                    Log.ErrorFormat(Common.Properties.Resources.SnappedFeatureCollection_Calculating_the_snapped_geometry_for_feature___0___failed___1_, feature, e.Message);
                }
            }

            var feature2D = new Feature2D();
            if (feature.Attributes != null)
            {
                feature2D.Attributes = (IFeatureAttributeCollection)feature.Attributes.Clone();
            }

            if (feature is INameable nameable)
            {
                feature2D.Name = nameable.Name;
            }

            if (snappedGeometry == null)
            {
                return feature2D;
            }

            if (feature.Geometry is IPoint)
            {
                //hack: line to snapped point
                snappedGeometry = GetSnappedGeometryForPoint(snappedGeometry, feature.Geometry.Coordinate);
            }

            if (snappedGeometry is MultiPoint points &&
                typeof(IMultiLineString).IsAssignableFrom(SnappedLayerStyle.GeometryType))
            {
                var auxGeom = new List<ILineString>();

                if (points.Count == 1)
                {
                    List<double> distances = feature.Geometry.Coordinates.Select(coord => coord.Distance(points.Coordinate)).ToList();

                    int smallestNumberIndex = distances.IndexOf(distances.Min());

                    auxGeom.Add((LineString)GetSnappedGeometryForPoint(points, feature.Geometry.Coordinates[smallestNumberIndex]));
                    snappedGeometry = new MultiLineString(auxGeom.ToArray());
                }

                if (points.Count == 2)
                {
                    auxGeom.Add((LineString)GetSnappedGeometryForPoint(points.FirstOrDefault(),
                                                                        feature.Geometry.Coordinates.FirstOrDefault()));
                    auxGeom.Add((LineString)GetSnappedGeometryForPoint(points.LastOrDefault(),
                                                                        feature.Geometry.Coordinates.LastOrDefault()));
                    snappedGeometry = new MultiLineString(auxGeom.ToArray());
                }
            }

            feature2D.Geometry = snappedGeometry;
            return feature2D;
        }

        private static IGeometry GetSnappedGeometryForPoint(IGeometry snappedGeometry, Coordinate featureCoordinate)
        {
            return snappedGeometry.IsEmpty
                       ? snappedGeometry
                       : new LineString(new[]
                       {
                           (Coordinate) featureCoordinate.Clone(),
                           snappedGeometry.Coordinate
                       });
        }

        private void OriginalFeaturesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (dirty)
            {
                return; //list already dirty, so don't update
            }

            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            IFeature feature = removedOrAddedItem is IFeatureData data
                                   ? data.Feature
                                   : removedOrAddedItem as IFeature;

            if (feature == null)
            {
                return;
            }

            int removedOrAddedIndex = e.GetRemovedOrAddedIndex();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    SnappedFeatures.Insert(removedOrAddedIndex, GetSnappedFeature(feature));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    SnappedFeatures.RemoveAt(removedOrAddedIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    SnappedFeatures[removedOrAddedIndex] = GetSnappedFeature(feature);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    dirty = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }

            FireFeaturesChanged();
        }

        private void CalculateSnappedFeatures()
        {
            SnappedFeatures.Clear();

            if (OriginalFeatures.Count <= 0)
            {
                return;
            }

            var originalGeometries = new List<IGeometry>();
            originalGeometries.AddRange(OriginalFeatures.OfType<IFeature>().Select(f => f.Geometry));
            //SourceSink is a FeatureData, so we need to extract the feature geometry from it.
            originalGeometries.AddRange(OriginalFeatures.OfType<IFeatureData>().Select(f => f.Feature.Geometry));

            IGeometry[] snappedGeometries =
                OperationApi.GetGridSnappedGeometry(SnapApiFeatureType, originalGeometries).ToArray();

            for (var i = 0; i < OriginalFeatures.Count; i++)
            {
                object feature = OriginalFeatures[i];
                if (feature is IFeatureData data)
                {
                    feature = data.Feature;
                }

                SnappedFeatures.Add(GetSnappedFeature((IFeature)feature, snappedGeometries[i]));
            }
        }

        private static Brush AdjustBrushTransparency(Brush originalBrush, int alpha)
        {
            return new SolidBrush(Color.FromArgb(alpha, ((SolidBrush)originalBrush).Color));
        }

        private static Pen AdjustPenTransparency(Pen originalPen, int alpha)
        {
            return new Pen(Color.FromArgb(alpha, originalPen.Color), originalPen.Width);
        }
    }
}
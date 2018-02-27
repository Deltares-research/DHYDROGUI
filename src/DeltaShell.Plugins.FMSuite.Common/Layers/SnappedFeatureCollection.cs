using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Common.Layers
{
    public class SnappedFeatureCollection : FeatureCollection
    {
        private VectorStyle OriginalFeaturesLayerStyle { get; set; }
        private List<Feature2D> SnappedFeatures { get; set; }
        private bool dirty;
        private bool snappedFeatureFailed;

        /// <summary>
        /// A <see cref="FeatureCollection"/> for <see cref="IFeature"/> objects that are being
        /// snapped with <see cref="IGridOperationApi"/>.
        /// </summary>
        /// <param name="operationApi">snap api</param>
        /// <param name="originalFeatures">Expected to be a collection of <see cref="IFeature"/>, where collection implements <see cref="INotifyCollectionChanged"/> and <see cref="INotifyPropertyChanged"/>.</param>
        /// <param name="originalFeaturesLayerStyle">Style of the layer from which <paramref name="originalFeatures"/> are coming from.</param>
        /// <param name="layerName">Name of the layer.</param>
        /// <param name="snapApiFeatureType">The feature type name for the snapping api</param>
        public SnappedFeatureCollection(IGridOperationApi operationApi, ICoordinateSystem coordinateSystem, IList originalFeatures, VectorStyle originalFeaturesLayerStyle, string layerName, string snapApiFeatureType)
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
            snappedFeatureFailed = false;
        }

        public override IList Features
        {
            get
            {
                if ( LayerIsShown && (dirty || snappedFeatureFailed ))
                {
                    try
                    {
                        snappedFeatureFailed = false; //Reset it.
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
                        snappedStyle.Symbol = null; //reset

                    snappedStyle.GeometryType = typeof (ILineString);
                    snappedStyle.Line = new Pen(Color.Gray) {EndCap = LineCap.DiamondAnchor};
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

        private IList originalFeatures;
        public IGridOperationApi OperationApi { get; set; }

        private IList OriginalFeatures
        {
            get { return originalFeatures; }
            set
            {
                if (originalFeatures != null)
                {
                    ((INotifyCollectionChanged)originalFeatures).CollectionChanged -= OriginalFeaturesCollectionChanged;
                    ((INotifyPropertyChanged)originalFeatures).PropertyChanged -= OriginalFeaturesPropertyChanged;
                }

                originalFeatures = value;

                if (originalFeatures != null)
                {
                    ((INotifyCollectionChanged)originalFeatures).CollectionChanged += OriginalFeaturesCollectionChanged;
                    ((INotifyPropertyChanged)originalFeatures).PropertyChanged += OriginalFeaturesPropertyChanged;
                }
            }
        }

        public ILayer Layer { get; set; }
        private bool LayerIsShown
        {
            get { return Layer != null && Layer.Map != null && Layer.Map.GetAllVisibleLayers(false).Contains(Layer); }
        }
        
        void OriginalFeaturesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (dirty)
                return; //list already dirty, so don't update

            if (e.PropertyName == "Geometry")
            {
                var indexInOriginal = OriginalFeatures.IndexOf(sender);
                if (indexInOriginal >= 0)
                {
                    SnappedFeatures.RemoveAt(indexInOriginal);
                    var originalFeature = (IFeature) OriginalFeatures[indexInOriginal];
                    var snappedFeature = GetSnappedFeature(originalFeature);
                    SnappedFeatures.Insert(indexInOriginal, snappedFeature);
                }

                FireFeaturesChanged();
            }
        }

        private Feature2D GetSnappedFeature(IFeature feature, IGeometry snappedGeometry=null)
        {
            if (snappedGeometry == null)
            {
                try
                {
                    snappedGeometry = OperationApi.GetGridSnappedGeometry(SnapApiFeatureType, feature.Geometry);;
                }
                catch (Exception)
                {
                    snappedFeatureFailed = true;
                }
            }

            var feature2D = new Feature2D();
            if (feature.Attributes != null)
                feature2D.Attributes = (IFeatureAttributeCollection) feature.Attributes.Clone();
            if (feature is INameable)
                feature2D.Name = ((INameable) feature).Name;

            if (feature.Geometry is IPoint)
            {
                //hack: line to snapped point
                snappedGeometry = snappedGeometry.IsEmpty
                                      ? snappedGeometry
                                      : new LineString(new[]
                                          {
                                              (Coordinate) feature.Geometry.Coordinate.Clone(),
                                              snappedGeometry.Coordinate
                                          });
            }
            feature2D.Geometry = snappedGeometry;
            return feature2D;
        }
        
        void OriginalFeaturesCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (dirty)
                return; //list already dirty, so don't update
            var feature = e.Item is IFeatureData ? ((IFeatureData)e.Item).Feature : (IFeature) e.Item;
            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    SnappedFeatures.Insert(e.Index, GetSnappedFeature(feature));
                    break;
                case NotifyCollectionChangeAction.Remove:
                    SnappedFeatures.RemoveAt(e.Index);
                    break;
                case NotifyCollectionChangeAction.Replace:
                    SnappedFeatures[e.Index] = GetSnappedFeature(feature);
                    break;
                case NotifyCollectionChangeAction.Reset:
                    dirty = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            FireFeaturesChanged();
        }

        private void CalculateSnappedFeatures()
        {
            SnappedFeatures.Clear();
            
            if (OriginalFeatures.Count <= 0) 
                return;

            var originalGeometries = new List<IGeometry>();
            originalGeometries.AddRange(OriginalFeatures.OfType<IFeature>().Select(f => f.Geometry));
            //SourceSink is a FeatureData, so we need to extract the feature geometry from it.
            originalGeometries.AddRange(OriginalFeatures.OfType<IFeatureData>().Select(f => f.Feature.Geometry));

            var snappedGeometries = OperationApi.GetGridSnappedGeometry(SnapApiFeatureType, originalGeometries).ToArray();

            for (var i = 0; i < OriginalFeatures.Count; i++)
            {
                var feature = OriginalFeatures[i];
                if (feature is IFeatureData)
                    feature = ((IFeatureData) feature).Feature;

                SnappedFeatures.Add(GetSnappedFeature((IFeature) feature, snappedGeometries[i]));
            }
        }

        private static Brush AdjustBrushTransparency(Brush originalBrush, int alpha)
        {
            return new SolidBrush(Color.FromArgb(alpha, ((SolidBrush) originalBrush).Color));
        }

        private static Pen AdjustPenTransparency(Pen originalPen, int alpha)
        {
            return new Pen(Color.FromArgb(alpha, originalPen.Color), originalPen.Width);
        }

        public override void Dispose()
        {
            Layer = null;
            OriginalFeatures = null;

            base.Dispose();
        }
    }
}
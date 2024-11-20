using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms.Integration;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.ModelFeatureCoordinateDataEditor
{

    /// <summary>
    /// Attached properties for WindowsFormsHost with a BoundaryGeometryPreview child item. 
    /// Allows for one way binding of properties in WPF controls
    /// </summary>
    public static class BoundaryGeometryPreviewWindowsFormsHostMap
    {
        public static readonly DependencyProperty FeatureGeometryProperty = DependencyProperty.RegisterAttached("FeatureGeometry", typeof(Geometry), typeof(BoundaryGeometryPreviewWindowsFormsHostMap), new PropertyMetadata(PropertyChanged));
        public static readonly DependencyProperty FeatureProperty = DependencyProperty.RegisterAttached("Feature", typeof(IFeature), typeof(BoundaryGeometryPreviewWindowsFormsHostMap), new PropertyMetadata(PropertyChanged));
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.RegisterAttached("SelectedIndex", typeof(int), typeof(BoundaryGeometryPreviewWindowsFormsHostMap), new PropertyMetadata(PropertyChanged));

        public static void SetFeatureGeometry(DependencyObject element, Geometry value)
        {
            element.SetValue(FeatureGeometryProperty, value);
        }

        public static Geometry GetFeatureGeometry(DependencyObject element)
        {
            return (Geometry)element.GetValue(FeatureGeometryProperty);
        }

        public static void SetSelectedIndex(DependencyObject element, int value)
        {
            element.SetValue(SelectedIndexProperty, value);
        }

        public static int GetSelectedIndex(DependencyObject element)
        {
            return (int) element.GetValue(SelectedIndexProperty);
        }

        public static void SetFeature(DependencyObject element, IFeature value)
        {
            element.SetValue(FeatureProperty, value);
        }

        public static IFeature GetFeature(DependencyObject element)
        {
            return (IFeature) element.GetValue(FeatureProperty);
        }

        private static void PropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var boundaryGeometryPreview = (sender as WindowsFormsHost).Child as BoundaryGeometryPreview;
            if (boundaryGeometryPreview == null) return;

            if (e.Property == FeatureProperty)
            {
                boundaryGeometryPreview.Feature = (IFeature) e.NewValue;
            }

            if (e.Property == FeatureGeometryProperty)
            {
                var featureGeometry = (Geometry)e.NewValue;

                boundaryGeometryPreview.FeatureGeometry = featureGeometry;

                if (featureGeometry == null) return;
                boundaryGeometryPreview.DataPoints = new EventedList<int>(Enumerable.Range(0, featureGeometry.Coordinates.Length));

                if (featureGeometry.Coordinates.Length > 0)
                {
                    boundaryGeometryPreview.SelectedPoints = new List<int> { 0 };
                }
            }

            if (e.Property == SelectedIndexProperty && e.NewValue is int)
            {
                var index = (int) e.NewValue;
                if (index == -1) return;
                boundaryGeometryPreview.SelectedPoints = new List<int> {index};
            }
        }
    }
}
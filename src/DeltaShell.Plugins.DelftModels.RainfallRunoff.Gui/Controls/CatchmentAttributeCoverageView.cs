using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api;
using SharpMap.Layers;
using SharpMap.Rendering;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public partial class CatchmentAttributeCoverageView : UserControl, ICompositeView, ITimeNavigatable
    {
        private readonly CoverageView coverageView;
        private IFeatureCoverageProvider data;
        private FeatureCoverageLayer primaryAttributeLayer;
        private FeatureCoverageLayer secondaryAttributeLayer;
        private readonly IEventedList<IView> childViews = new EventedList<IView>();

        public CatchmentAttributeCoverageView()
        {
            InitializeComponent();

            coverageView = new CoverageView {Dock = DockStyle.Fill};
            coverageView.TimesChanged += CoverageViewTimesChanged;
            Controls.Add(coverageView);
            coverageView.BringToFront();

            UpdateSecondaryLayerEnabled();
            childViews.Add(coverageView);
        }

        #region ICompositeView Members

        public IEventedList<IView> ChildViews
        {
            get { return childViews; }
        }

        public bool HandlesChildViews { get { return true; } }

        public void ActivateChildView(IView childView) { }

        #endregion

        #region ITimeNavigatable

        public DateTime? TimeSelectionStart
        {
            get
            {
                DateTime? secondaryTime = UseSecondaryAttribute() ? secondaryAttributeLayer.TimeSelectionStart : null;
                return EdgeTime(coverageView.TimeSelectionStart, secondaryTime, true);
            }
        }

        public DateTime? TimeSelectionEnd
        {
            get
            {
                DateTime? secondaryTime = UseSecondaryAttribute() ? secondaryAttributeLayer.TimeSelectionEnd : null;
                return EdgeTime(coverageView.TimeSelectionEnd, secondaryTime, false);
            }
        }

        public TimeNavigatableLabelFormatProvider CustomDateTimeFormatProvider
        {
            get { return coverageView.CustomDateTimeFormatProvider; }
        }

        public void SetCurrentTimeSelection(DateTime? start, DateTime? end)
        {
            coverageView.SetCurrentTimeSelection(start, end);
        }

        public event Action CurrentTimeSelectionChanged
        {
            add { coverageView.CurrentTimeSelectionChanged += value; }
            remove { coverageView.CurrentTimeSelectionChanged -= value; }
        }

        public IEnumerable<DateTime> Times
        {
            get { return GetTimes(); }
        }

        public event Action TimesChanged;

        public TimeSelectionMode SelectionMode
        {
            get { return coverageView.SelectionMode; }
        }

        public SnappingMode SnappingMode
        {
            get { return coverageView.SnappingMode; }
        }

        private static DateTime? EdgeTime(DateTime? timeOne, DateTime? timeTwo, bool minimum)
        {
            if (timeOne.HasValue)
            {
                if (timeTwo.HasValue)
                {
                    bool isLess = DateTime.Compare(timeOne.Value, timeTwo.Value) < 0;
                    return (isLess && minimum) ? timeOne : timeTwo;
                }
                return timeOne;
            }
            return timeTwo;
        }

        private bool UseSecondaryAttribute()
        {
            return secondaryAttributeLayer != null && secondaryAttributeLayer.Visible;
        }

        private IEnumerable<DateTime> GetTimes()
        {
            IEnumerable<DateTime> secondaryTimes = UseSecondaryAttribute() ? secondaryAttributeLayer.Times : null;
            return coverageView.Times.Concat(secondaryTimes ?? new DateTime[0]).Distinct().OrderBy(d => d).ToList();
        }

        #endregion

        #region IView<IFeatureCoverageProvider> Members

        public object Data
        {
            get { return data; }
            set
            {
                data = (IFeatureCoverageProvider) value;
                if (data != null)
                {
                    attributeComboBox.DataSource = data.FeatureCoverageNames.ToList();
                    secondaryAttributeComboBox.DataSource = data.FeatureCoverageNames.ToList();
                }
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion

        private void CoverageViewTimesChanged()
        {
            FireTimesChanged();
        }

        private void FireTimesChanged()
        {
            if (TimesChanged != null)
            {
                TimesChanged();
            }
        }

        private void UpdateSecondaryLayerEnabled()
        {
            bool visible = secondaryLayerEnabled.Checked;

            secondaryAttributeComboBox.Enabled = visible;
            lblSecondary.ForeColor = visible ? SystemColors.ControlText : SystemColors.GrayText;
            if (secondaryAttributeLayer != null)
            {
                secondaryAttributeLayer.Visible = visible;
            }
            FireTimesChanged();
        }

        private void AttributeComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedAttribute = (string) attributeComboBox.SelectedItem;
            IFeatureCoverage featureCoverage = data.GetFeatureCoverageByName(selectedAttribute);

            CreatePrimaryLayerIfNonExisting(featureCoverage);
            CreateSecondaryLayerIfNonExisting();

            //todo: figure out how to preserve zoom
            primaryAttributeLayer.SetName("[Primary] " + featureCoverage.Name);
            coverageView.Data = featureCoverage;
            FireTimesChanged();
        }

        private void CreatePrimaryLayerIfNonExisting(IFeatureCoverage featureCoverage)
        {
            if (primaryAttributeLayer == null)
            {
                IMap map = coverageView.ChildViews.OfType<MapView>().First().Map;
                coverageView.Data = featureCoverage;
                primaryAttributeLayer = map.Layers.OfType<FeatureCoverageLayer>().FirstOrDefault();
            }
        }

        private void CreateSecondaryLayerIfNonExisting()
        {
            IMap map = coverageView.ChildViews.OfType<MapView>().First().Map;

            if (secondaryAttributeLayer == null || !map.Layers.Contains(secondaryAttributeLayer))
            {
                var secondaryAttributeRenderer = new FeatureCoverageRenderer
                    {GeometryForFeatureDelegate = GetGeometryForSecondaryAttribute};
                secondaryAttributeLayer = new FeatureCoverageLayer(secondaryAttributeRenderer)
                    {
                        Selectable = false,
                        Visible = secondaryLayerEnabled.Checked
                    };
                map.Layers.Insert(0, secondaryAttributeLayer);
            }
        }

        private static IGeometry GetGeometryForSecondaryAttribute(IFeature feature)
        {
            if (feature is Catchment catchment)
            {
                return catchment.InteriorPoint;
            }
            return feature.Geometry; //or exception
        }

        private void SecondaryAttributeComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            CreateSecondaryLayerIfNonExisting();
            var selectedAttribute = (string) secondaryAttributeComboBox.SelectedItem;
            IFeatureCoverage featureCoverage = data.GetFeatureCoverageByName(selectedAttribute);
            secondaryAttributeLayer.FeatureCoverage = featureCoverage;
            secondaryAttributeLayer.Name = "[Secondary] " + featureCoverage.Name;
            FireTimesChanged();
        }

        private void SecondaryLayerEnabledCheckedChanged(object sender, EventArgs e)
        {
            UpdateSecondaryLayerEnabled();
        }
    }
}
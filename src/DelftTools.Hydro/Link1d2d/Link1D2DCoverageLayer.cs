using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils;
using DeltaShell.NGHS.Common.Gui.MapLayers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using SharpMap.Layers;
using SharpMap.Rendering.Thematics;
using SharpMap.Styles;

namespace DelftTools.Hydro.Link1d2d
{
    public class Link1D2DCoverageLayer: UnstructuredGridVectorCoverageLayerBase
    {
        private Links1D2DCoverage links1D2DCoverage;
        private bool updateThemeRequired;
        double min, max;
       
        public override ICoverage Coverage
        {
            get
            {
                return links1D2DCoverage;
            }
            set
            {
                if (links1D2DCoverage != null)
                {
                    links1D2DCoverage.ValuesChanged -= CoverageValuesChanged;
                    ((INotifyPropertyChange)links1D2DCoverage).PropertyChanged -= Coverage_PropertyChanged;
                }

                links1D2DCoverage = (Links1D2DCoverage)value;

                if (links1D2DCoverage != null)
                {
                    // update envelope
                    envelope = null;
                    links1D2DCoverage.ValuesChanged += CoverageValuesChanged;
                    ((INotifyPropertyChange)links1D2DCoverage).PropertyChanged += Coverage_PropertyChanged;

                    this.SetName(links1D2DCoverage.Name);

                    if (links1D2DCoverage.Components.Count > 0)
                    {
                        MissingValue = Convert.ToDouble(links1D2DCoverage.Components[0].NoDataValue);
                    }
                    
                    updateThemeRequired = true;
                    coverage = new UnstructuredGridFlowLinkCoverage(links1D2DCoverage.Grid, true)
                    {
                        Arguments = links1D2DCoverage.Arguments,
                        Components = links1D2DCoverage.Components,
                        Time = links1D2DCoverage.Time,
                    };
                }
            }
        }
        
        
        private void Coverage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // when the Coverage changes coordinate system, alter the transformation for rendering.
            if (Equals(e.PropertyName, nameof(ICoverage.CoordinateSystem)))
            {
                UpdateCoordinateTransformation();
            }

            if (Equals(e.PropertyName, nameof(ICoverage.IsEditing)) && !Coverage.IsEditing)
            {
                updateThemeRequired = true;
                RenderRequired = true;
            }
        }
        private void UpdateTheme()
        {
            if (Theme != null)
            {
                if (Theme is GradientTheme)
                {
                    var gradientTheme = theme as GradientTheme;

                    if (gradientTheme != null)
                    {
                        gradientTheme.NoDataValues = Coverage.Components.Last().NoDataValues;
                        GetMinMax(out min, out max);
                        gradientTheme.ScaleTo(min, max);
                    }
                }
                return;
            }

            if (!links1D2DCoverage.Grid.Vertices.Any())
            {
                return;
            }

            AutoUpdateThemeOnDataSourceChanged = true;

            GetMinMax(out min, out max);
            var defaultStyle = new VectorStyle { GeometryType = GetGeometryType() };
            var newTheme = ThemeFactory.CreateGradientTheme("value", defaultStyle, ColorBlend.Rainbow5,
                min, max, 1, 1, false, true, 12);
            newTheme.NoDataValues = Coverage.Components.Last().NoDataValues;
            Theme = newTheme;
        }
        public override ITheme Theme
        {
            get
            {
                if (updateThemeRequired)
                {
                    updateThemeRequired = false;
                    // auto turn on:
                    if (!OptimizeRendering)
                        OptimizeRendering = GetGrid().Vertices.Count > 200000; //200k
                    UpdateTheme();
                }

                return base.Theme;
            }
            set
            {
                base.Theme = value;

                if (base.Theme == null)
                {
                    updateThemeRequired = true;
                }
            }
        }
        
        private void CoverageValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            updateThemeRequired = true;
            RenderRequired = true;
        }


        protected override int GetPointCount(UnstructuredGrid grid)
        {
            return links1D2DCoverage.Links.Count;
        }
        
        protected override Coordinate GetPoint(UnstructuredGrid grid, int i)
        {
            return links1D2DCoverage.Links[i].GetCenter();
        }
    }
}

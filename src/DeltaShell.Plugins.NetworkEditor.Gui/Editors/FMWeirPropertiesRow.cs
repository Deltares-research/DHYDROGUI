using System;
using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors
{
    public class FMWeirPropertiesRow : IDisposable, INotifyPropertyChange, IFeatureRowObject
    {
        protected string CrestLevelTimeSeriesString = "Time series";
        private IWeir weir;
        private IWeirFormula formula;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        public FMWeirPropertiesRow(IWeir weir)
        {
            Weir = weir;
        }

        // weir properties
        [DisplayName("Name")]
        public string Name
        {
            get
            {
                return Weir.Name;
            }
            set
            {
                Weir.Name = value;
            }
        }

        [DynamicReadOnly]
        [DisplayName(GuiParameterNames.CrestLevel)]
        public string CrestLevel
        {
            get
            {
                if (weir.CanBeTimedependent && weir.UseCrestLevelTimeSeries)
                {
                    return CrestLevelTimeSeriesString;
                }

                return weir.CrestLevel.ToString("0.00", CultureInfo.CurrentCulture);
            }
            set
            {
                if (weir.CanBeTimedependent && weir.UseCrestLevelTimeSeries)
                {
                    throw new InvalidOperationException("Cannot set value from row when using time dependent crest level.");
                }

                weir.CrestLevel = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        [DynamicReadOnly]
        [DisplayName(GuiParameterNames.CrestWidth + " [m]")]
        public string CrestWidth
        {
            get
            {
                return weir.CrestWidth.ToString("0.00", CultureInfo.CurrentCulture);
            }
            set
            {
                weir.CrestWidth = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        [DisplayName("Use crest width")]
        public bool UseCrestWidth
        {
            get
            {
                return weir.CrestWidth > 0;
            }
            set
            {
                weir.CrestWidth = value ? weir.Geometry.Length : 0.0;
            }
        }

        [DisplayName("Lateral contraction coefficient")]
        [DisplayFormat("0.00")]
        public double SLateralContraction
        {
            get
            {
                var f = formula as SimpleWeirFormula;
                if (f != null)
                {
                    return f.LateralContraction;
                }

                return 0.0;
            }
            set
            {
                var f = formula as SimpleWeirFormula;
                if (f != null)
                {
                    f.LateralContraction = value;
                }
            }
        }

        [Browsable(false)]
        public bool HasParent { get; set; }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (weir == null)
            {
                return false;
            }

            if (propertyName == "CrestLevel")
            {
                return weir.UseCrestLevelTimeSeries;
            }

            if (propertyName == "CrestWidth")
            {
                return weir.CrestWidth <= 0.0;
            }

            return false;
        }

        public void Dispose()
        {
            Weir = null;
            PropertyChanged = null;
            PropertyChanging = null;
        }

        public IFeature GetFeature()
        {
            return weir;
        }

        private IWeir Weir
        {
            get
            {
                return weir;
            }
            set
            {
                if (weir != null)
                {
                    ((INotifyPropertyChanged) weir).PropertyChanged -= WeirPropertiesRowPropertyChanged;
                }

                weir = value;
                UpdateTimeSerieStrings();
                if (weir != null)
                {
                    formula = (IWeirFormula) weir.WeirFormula;
                    ((INotifyPropertyChanged) weir).PropertyChanged += WeirPropertiesRowPropertyChanged;
                }
            }
        }

        private void WeirPropertiesRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                UpdateTimeSerieStrings();
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        private void UpdateTimeSerieStrings()
        {
            if (weir != null)
            {
                CrestLevelTimeSeriesString = string.Format("{0}_{1}.tim", weir.Name, KnownStructureProperties.CrestLevel);
            }
        }
    }
}
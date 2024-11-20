using System;
using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors
{
    /// <summary>
    /// <see cref="FMWeirPropertiesRow"/> defines a single row within the
    /// multi-data editor of a single <see cref="IStructure"/>.
    /// </summary>
    /// <seealso cref="IDisposable" />
    /// <seealso cref="INotifyPropertyChange" />
    /// <seealso cref="IFeatureRowObject" />
    public class FMWeirPropertiesRow : IDisposable, INotifyPropertyChange, IFeatureRowObject
    {
        protected string CrestLevelTimeSeriesString = "Time series";
        private IStructure weir;

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// Creates a new <see cref="FMWeirPropertiesRow"/>.
        /// </summary>
        /// <param name="weir">The weir.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="weir"/> is <c>null</c>.
        /// </exception>
        public FMWeirPropertiesRow(IStructure weir)
        {
            Ensure.NotNull(weir, nameof(weir));
            Weir = weir;
        }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        [DisplayName("Group Name")]
        public string GroupName
        {
            get => Weir.GroupName;
            set => Weir.GroupName = value;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [DisplayName("Name")]
        public string Name
        {
            get => Weir.Name;
            set => Weir.Name = value;
        }

        /// <summary>
        /// Gets or sets the crest level.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <c>Pump.UseCrestLevelTimeSeries</c> is <c>true</c>.
        /// </exception>
        [DynamicReadOnly]
        [DisplayName(GuiParameterNames.CrestLevel + " [m AD]")]
        public string CrestLevel
        {
            get => weir.UseCrestLevelTimeSeries 
                       ? CrestLevelTimeSeriesString 
                       : weir.CrestLevel.ToString("0.00", CultureInfo.CurrentCulture);
            set
            {
                if (weir.UseCrestLevelTimeSeries)
                {
                    throw new InvalidOperationException("Cannot set value from row when using time dependent crest level.");
                }

                weir.CrestLevel = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets or sets the width of the crest.
        /// </summary>
        [DynamicReadOnly]
        [DisplayName(GuiParameterNames.CrestWidth + " [m]")]
        [DisplayFormat("0.00")]
        public string CrestWidth
        {
            get => weir.CrestWidth.ToString("0.00", CultureInfo.CurrentCulture);
            set => weir.CrestWidth = double.Parse(value, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use crest width].
        /// </summary>
        [DisplayName("Use crest width")]
        public bool UseCrestWidth
        {
            get => weir.CrestWidth > 0;
            set => weir.CrestWidth = value ? weir.Geometry.Length : 0.0;
        }

        [ReadOnly(true)]
        [DisplayName("Structure Type")]
        public string FormulaName => Weir.Formula.Name;

        [Browsable(false)]
        public bool HasParent { get; set; }

        /// <summary>
        /// Determines whether the specified <paramref name="propertyName"/>
        /// is currently read-only.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="propertyName"/> is currently read-only;
        /// otherwise, <c>false</c>.
        /// </returns>
        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (weir == null)
            {
                return false;
            }

            switch (propertyName)
            {
                case nameof(CrestLevel):
                    return weir.UseCrestLevelTimeSeries;
                case nameof(CrestWidth):
                    return weir.CrestWidth <= 0.0;
                default:
                    return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Weir = null;
            PropertyChanged = null;
            PropertyChanging = null;
        }

        /// <summary>
        /// Gets the underlying feature.
        /// </summary>
        /// <returns>
        /// The <see cref="IStructure"/> feature.
        /// </returns>
        public IFeature GetFeature() => Weir;

        private IStructure Weir
        {
            get => weir;
            set
            {
                if (weir != null)
                {
                    ((INotifyPropertyChanged) weir).PropertyChanged -= WeirPropertiesRowPropertyChanged;
                }

                weir = value;
                UpdateTimeSeriesStrings();

                if (weir != null)
                {
                    ((INotifyPropertyChanged) weir).PropertyChanged += WeirPropertiesRowPropertyChanged;
                }
            }
        }

        private void WeirPropertiesRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                UpdateTimeSeriesStrings();
            }

            PropertyChanged?.Invoke(this, e);
        }

        private void UpdateTimeSeriesStrings()
        {
            if (weir != null)
            {
                CrestLevelTimeSeriesString = $"{weir.Name}_{KnownStructureProperties.CrestLevel}.tim";
            }
        }
    }
}
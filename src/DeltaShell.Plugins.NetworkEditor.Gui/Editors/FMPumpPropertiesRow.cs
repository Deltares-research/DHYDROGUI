using System;
using System.ComponentModel;
using System.Globalization;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors
{
    public class FMPumpPropertiesRow : IDisposable, INotifyPropertyChange, IFeatureRowObject
    {
        private IPump pump;

        public FMPumpPropertiesRow(IPump pump)
        {
            Pump = pump;
        }

        private IPump Pump
        {
            get { return pump; }
            set
            {
                if (pump != null)
                {
                    ((INotifyPropertyChanged)pump).PropertyChanged -= PumpPropertiesRowPropertyChanged;
                }
                pump = value;
                if (pump != null)
                {
                    ((INotifyPropertyChanged)pump).PropertyChanged += PumpPropertiesRowPropertyChanged;
                }
            }
        }

        public string Name
        {
            get { return pump.Name; } 
            set { pump.Name = value; }
        }

        [DynamicReadOnly]
        public virtual string Capacity
        {
            get
            {
                if (pump.CanBeTimedependent && pump.UseCapacityTimeSeries)
                {
                    return String.Format("{0}_{1}.tim", pump.Name, KnownStructureProperties.Capacity);
                }
                return pump.Capacity.ToString(CultureInfo.CurrentCulture);
            }
            set
            {
                if (pump.CanBeTimedependent && pump.UseCapacityTimeSeries)
                {
                    throw new InvalidOperationException("Cannot set value from row when using time dependent pump capacity.");
                }
                pump.Capacity = double.Parse(value, CultureInfo.CurrentCulture);
            }
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (pump == null) return false;
            if (propertyName == "Capacity")
            {
                return pump.CanBeTimedependent && pump.UseCapacityTimeSeries;
            }
            return false;
        }

        public void Dispose()
        {
            Pump = null;
        }

        private void PumpPropertiesRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        #region INotificPropertyChange

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        [Browsable(false)]
        public bool HasParent { get; set; }
        public IFeature GetFeature()
        {
            return pump;
        }

        #endregion
    }
}
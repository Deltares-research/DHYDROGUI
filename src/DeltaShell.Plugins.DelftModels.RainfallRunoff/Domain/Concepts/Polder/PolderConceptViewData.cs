using System;
using System.Collections.Specialized;
using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Collections;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Polder
{
    public class PolderConceptViewData : IDisposable, INotifyPropertyChanged
    {
        private readonly PolderConcept polderConcept;
        private RainfallRunoffEnums.AreaUnit areaUnit;
        
        public PolderConceptViewData(PolderConcept polderConcept, RainfallRunoffEnums.AreaUnit areaUnit)
        {
            this.polderConcept = polderConcept;
            this.areaUnit = areaUnit;

            ((INotifyCollectionChange)polderConcept).CollectionChanged += OnCollectionChanged;
            ((INotifyPropertyChange)polderConcept).PropertyChanged += OnPropertyChanged;
            if (polderConcept.Catchment != null)
            {
                ((INotifyPropertyChange) polderConcept.Catchment).PropertyChanged += OnPropertyChanged; // Custom subscription to the catchment (aggregate).
            }
        }

        public Catchment Catchment
        {
            get { return polderConcept.Catchment; }
        }

        public double PavedPercentage
        {
            get { return GetPercentage(polderConcept.PavedArea); }
            set
            {
                if (PavedPercentage != value)
                {
                    polderConcept.PavedArea = CalculateAreaFromPercentage(value);
                }
            }
        }

        public double UnpavedPercentage
        {
            get { return GetPercentage(polderConcept.UnpavedArea); }
            set
            {
                if (UnpavedPercentage != value)
                {
                    polderConcept.UnpavedArea = CalculateAreaFromPercentage(value);
                }
            }
        }

        public double GreenhousePercentage
        {
            get { return GetPercentage(polderConcept.GreenhouseArea); }
            set
            {
                if (GreenhousePercentage != value)
                {
                    polderConcept.GreenhouseArea = CalculateAreaFromPercentage(value);
                }
            }
        }

        public double OpenwaterPercentage
        {
            get { return GetPercentage(polderConcept.OpenWaterArea); }
            set
            {
                if (OpenwaterPercentage != value)
                {
                    polderConcept.OpenWaterArea = CalculateAreaFromPercentage(value);
                }
            }
        }

        public bool HasPaved
        {
            get { return polderConcept.Paved != null; }
        }

        public bool HasGreenhouse
        {
            get { return polderConcept.Greenhouse != null; }
        }

        public bool HasOpenWater
        {
            get { return polderConcept.OpenWater != null; }
        }

        public bool HasUnpaved
        {
            get { return polderConcept.Unpaved != null; }
        }

        public bool HasNoPaved { get { return !HasPaved; } }
        public bool HasNoGreenhouse { get { return !HasGreenhouse; } }
        public bool HasNoUnpaved { get { return !HasUnpaved; }}
        public bool HasNoOpenWater { get { return !HasOpenWater; }}

        public double PavedArea
        {
            get => GetArea(polderConcept.PavedArea);
            set
            {
                polderConcept.PavedArea = GetConvertedArea(value);
                FirePropertyChanged(nameof(PavedPercentage));
            }
        }

        public double UnpavedArea
        {
            get => GetArea(polderConcept.UnpavedArea);
            set
            {
                polderConcept.UnpavedArea = GetConvertedArea(value);
                FirePropertyChanged(nameof(UnpavedPercentage));
            }
        }

        public double GreenhouseArea
        {
            get => GetArea(polderConcept.GreenhouseArea);
            set
            {
                polderConcept.GreenhouseArea = GetConvertedArea(value);
                FirePropertyChanged(nameof(GreenhousePercentage));
            }
        }

        /// <summary>
        /// Area unit from RainfallRunofArea
        /// </summary>
        public double OpenwaterArea
        {
            get => GetArea(polderConcept.OpenWaterArea);
            set
            {
                polderConcept.OpenWaterArea = GetConvertedArea(value);
                FirePropertyChanged(nameof(OpenwaterPercentage));
            }
        }

        public double SumAreas
        {
            get { return PavedArea + UnpavedArea + GreenhouseArea + OpenwaterArea; }
        }

        public double SumPercentages
        {
            get { return PavedPercentage + UnpavedPercentage + GreenhousePercentage + OpenwaterPercentage; }
        }

        public RainfallRunoffEnums.AreaUnit AreaUnit
        {
            get { return areaUnit; }
            set
            {
                if (areaUnit == value) return;
                areaUnit = value;
                FirePropertyChanged(nameof(AreaUnit));
            }
        }

        public double GeometryArea
        {
            get { return polderConcept.Catchment.AreaSize; }
        }

        public PolderConcept PolderConcept
        {
            get { return polderConcept; }
        }

        private double GetPercentage(double area)
        {
            return (100*area)/GeometryArea;
        }

        private double CalculateAreaFromPercentage(double percentage)
        {
            return GeometryArea*(percentage/100.0);
        }

        public void Dispose()
        {
            ((INotifyCollectionChange)polderConcept).CollectionChanged -= OnCollectionChanged;
            ((INotifyPropertyChange)polderConcept).PropertyChanged -= OnPropertyChanged;
            
            if(polderConcept.Catchment != null)
            {
                ((INotifyPropertyChange) polderConcept.Catchment).PropertyChanged -= OnPropertyChanged;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (!ReferenceEquals(sender, Catchment))
            {
                FirePropertyChanged(propertyChangedEventArgs.PropertyName);
            }
            else
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(sender, propertyChangedEventArgs);
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs propertyChangedEventArgs)
        {
            var catchmentModelData = propertyChangedEventArgs.GetRemovedOrAddedItem() as CatchmentModelData;
            if (catchmentModelData == null) return;

            if (catchmentModelData is PavedData)
            {
                FirePropertyChanged("HasPaved");
            }
            if (catchmentModelData is GreenhouseData)
            {
                FirePropertyChanged("HasGreenhouse");
            }
            if (catchmentModelData is OpenWaterData)
            {
                FirePropertyChanged("HasOpenWater");
            }
            if (catchmentModelData is UnpavedData)
            {
                FirePropertyChanged("HasUnpaved");
            }
        }

        protected virtual void FirePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        private double GetArea(double value) =>
            RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2, AreaUnit, value);

        private double GetConvertedArea(double value) =>
            RainfallRunoffUnitConverter.ConvertArea(AreaUnit, RainfallRunoffEnums.AreaUnit.m2, value);
    }
}
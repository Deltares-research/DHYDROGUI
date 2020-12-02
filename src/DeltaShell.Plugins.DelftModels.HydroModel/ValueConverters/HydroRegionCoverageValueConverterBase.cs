using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Coverages;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DeltaShell.Plugins.DelftModels.HydroModel.ValueConverters
{
    public abstract class HydroRegionCoverageValueConverterBase<TOrig, TConv> : Unique<long>, IHydroRegionValueConverter
        where TOrig : ICoverage
        where TConv : ICoverage
    {
        private TOrig originalValue;
        private TConv convertedValue;
        private IHydroRegion hydroRegion;
        private bool initialized;
        private DateTime lastTimeToBeAdded;
        private bool clearing;

        [Aggregation]
        public TOrig OriginalValue
        {
            get
            {
                return originalValue;
            }
            set
            {
                if (originalValue != null)
                {
                    ((INotifyPropertyChange) originalValue).PropertyChanged -= OriginalValuePropertyChanged;
                    originalValue.Arguments[1].ValuesChanged -= OriginalValueSecondArgumentValuesChanged;
                }

                originalValue = value;

                if (originalValue != null)
                {
                    ((INotifyPropertyChange) originalValue).PropertyChanged += OriginalValuePropertyChanged;
                    originalValue.Arguments[1].ValuesChanged += OriginalValueSecondArgumentValuesChanged;
                }

                Initialize();
            }
        }

        [Aggregation]
        public TConv ConvertedValue
        {
            get
            {
                return convertedValue;
            }
            set
            {
                if (convertedValue != null)
                {
                    ((INotifyPropertyChange) convertedValue).PropertyChanged -= ConvertedValuePropertyChanged;
                    convertedValue.CollectionChanged -= ConvertedValueCollectionChanged;
                    convertedValue.Arguments[0].ValuesChanged -= ConvertedValueTimeValuesChanged;

                    if (convertedValue.Arguments.Count > 1)
                    {
                        convertedValue.Arguments[1].ValuesChanged -= ConvertedValueSecondArgumentValuesChanged;
                    }
                }

                convertedValue = value;

                if (convertedValue != null)
                {
                    ((INotifyPropertyChange) convertedValue).PropertyChanged += ConvertedValuePropertyChanged;
                    convertedValue.CollectionChanged += ConvertedValueCollectionChanged;
                    convertedValue.Arguments[0].ValuesChanged += ConvertedValueTimeValuesChanged;
                    if (convertedValue.Arguments.Count > 1)
                    {
                        convertedValue.Arguments[1].ValuesChanged += ConvertedValueSecondArgumentValuesChanged;
                    }
                }

                Initialize();
            }
        }

        [Aggregation]
        public IHydroRegion HydroRegion
        {
            get
            {
                return hydroRegion;
            }
            set
            {
                hydroRegion = value;
                Initialize();
            }
        }

        [Aggregation]
        object IValueConverter.OriginalValue
        {
            get
            {
                return OriginalValue;
            }
            set
            {
                OriginalValue = (TOrig) value;
            }
        }

        [Aggregation]
        object IValueConverter.ConvertedValue
        {
            get
            {
                return ConvertedValue;
            }
            set
            {
                ConvertedValue = (TConv) value;
            }
        }

        public Type OriginalValueType
        {
            get
            {
                return typeof(TOrig);
            }
        }

        public Type ConvertedValueType
        {
            get
            {
                return typeof(TConv);
            }
        }

        public abstract object DeepClone();

        protected virtual void OnOriginalValueModified() {}

        protected virtual void ConvertedValueSecondArgumentValuesChanged(object sender, FunctionValuesChangingEventArgs e) {}

        protected virtual void Initialize()
        {
            if (hydroRegion == null || OriginalValue == null || ConvertedValue == null)
            {
                DeInitialize();
                return;
            }

            initialized = true;

            if (!EventSettings.BubblingEnabled)
            {
                return; //hack to make sure we don't call convert during save/load...
            }

            Convert();
        }

        protected abstract void Convert(DateTime dateTimeToUpdate = default(DateTime));

        protected DateTime[] SynchronizeTimeValues(DateTime dateTimeToUpdate)
        {
            bool updateOnlySpecificTimeSlice = dateTimeToUpdate != default(DateTime);

            if (updateOnlySpecificTimeSlice && ConvertedValue.Time.Values.Count - 1 == OriginalValue.Time.Values.Count) // we're only one time step off
            {
                if (!OriginalValue.Time.Values.Contains(dateTimeToUpdate))
                {
                    OriginalValue.Time.Values.Add(dateTimeToUpdate);
                }

                return new[]
                {
                    dateTimeToUpdate
                };
            }

            if (OriginalValue.Time.Values.Count > 0)
            {
                OriginalValue.Time.RemoveValues();
            }

            if (ConvertedValue.Time.Values.Count > 0)
            {
                OriginalValue.Time.Values.AddRange(ConvertedValue.Time.Values);
            }

            return ConvertedValue.Time.Values.ToArray();
        }

        protected static IHydroObject OtherSide(HydroLink hydroLink, IHydroObject me)
        {
            return Equals(hydroLink.Source, me) ? hydroLink.Target : hydroLink.Source;
        }

        protected static int GetActualOrPreviousTimeIndex(IVariable<DateTime> timeVariable, DateTime timeToUpdate)
        {
            IMultiDimensionalArray<DateTime> timeValues = timeVariable.Values;
            int timeIndex = timeValues.IndexOf(timeToUpdate);
            if (timeIndex == -1)
            {
                ArrayList adapter = ArrayList.Adapter(timeValues);
                // first index smaller:
                timeIndex = ~adapter.BinarySearch(timeToUpdate) - 1;
            }

            return timeIndex;
        }

        private bool IsInitialized
        {
            get
            {
                return initialized;
            }
        }

        private void OnConvertedValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // some (dangerous) performance shortcuts
            if (lastTimeToBeAdded != default(DateTime))
            {
                Convert(lastTimeToBeAdded);
                lastTimeToBeAdded = default(DateTime);
            }
        }

        private void OnConvertedValueCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Convert();
        }

        private void ConvertedValueCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            if (IsConvertedValueInEditMode())
            {
                return;
            }

            OnConvertedValueCollectionChanged(sender, e);
        }

        private void ConvertedValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            if (IsConvertedValueInEditMode())
            {
                return;
            }

            if (e.PropertyName == "IsEditing" && clearing)
            {
                Convert();
                clearing = false;
            }
            else
            {
                OnConvertedValuePropertyChanged(sender, e);
            }
        }

        private void OriginalValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            if (e.PropertyName == "IsEditing" && !OriginalValue.IsEditing && Equals(OriginalValue, sender))
            {
                OnOriginalValueModified();
            }
        }

        private void OriginalValueSecondArgumentValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            if (OriginalValue.IsEditing)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangeAction.Add)
            {
                OnOriginalValueModified();
                Convert();
            }
        }

        private void ConvertedValueTimeValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    lastTimeToBeAdded = (DateTime) e.Items[0];
                    break;
                case NotifyCollectionChangeAction.Remove:
                    if (!IsBackingArrayInEditMode(e.Function))
                    {
                        Convert();
                    }
                    else
                    {
                        clearing = true;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(e));
            }
        }

        private static bool IsBackingArrayInEditMode(IFunction function)
        {
            if (function is IVariable variable && variable.Values is IEditableObject editableObject)
            {
                return editableObject.IsEditing;
            }

            return false;
        }

        private bool IsConvertedValueInEditMode()
        {
            bool isEditing = ConvertedValue is IEditableObject editableObject && editableObject.IsEditing;
            return isEditing;
        }

        private void DeInitialize()
        {
            if (initialized)
            {
                initialized = false;
            }
        }
    }
}
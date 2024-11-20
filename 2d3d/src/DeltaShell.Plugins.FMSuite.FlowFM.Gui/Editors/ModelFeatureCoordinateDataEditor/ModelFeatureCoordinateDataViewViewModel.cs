using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.ModelFeatureCoordinateDataEditor
{
    /// <summary>
    /// View model for the <see cref="ModelFeatureCoordinateDataView"/>
    /// </summary>
    public class ModelFeatureCoordinateDataViewViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly List<PropertyDescriptor> coordinateDataRowPropertyDescriptors = new List<PropertyDescriptor>();
        private IModelFeatureCoordinateData modelFeatureCoordinateData;
        private ObservableCollection<CoordinateDataRow> coordinateDataRows;
        private int selectedCoordinateIndex;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// <see cref="IModelFeatureCoordinateData"/> that is wrapped by this view model
        /// </summary>
        public IModelFeatureCoordinateData ModelFeatureCoordinateData
        {
            get
            {
                return modelFeatureCoordinateData;
            }
            set
            {
                UnSubscribeToEvents();

                modelFeatureCoordinateData = value;

                SubscribeToEvents();

                UpdateColumnPropertyInformation();
                UpdateRowsForGeometry();

                OnPropertyChanged();
            }
        }

        public ObservableCollection<CoordinateDataRow> CoordinateDataRows
        {
            get
            {
                return coordinateDataRows;
            }
            set
            {
                coordinateDataRows = value;
                OnPropertyChanged();
            }
        }

        public int SelectedCoordinateIndex
        {
            get
            {
                return selectedCoordinateIndex;
            }
            set
            {
                selectedCoordinateIndex = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Function for adding dynamic property columns of <see cref="ModelFeatureCoordinateData"/>
        /// </summary>
        public Action<string, string, bool, string> AddColumn { get; set; }

        /// <summary>
        /// Function for clearing previous dynamic properties columns for <see cref="ModelFeatureCoordinateData"/>
        /// </summary>
        public Action ClearColumns { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnSubscribeToEvents();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SubscribeToEvents()
        {
            if (modelFeatureCoordinateData == null)
            {
                return;
            }

            ((INotifyCollectionChanged) modelFeatureCoordinateData).CollectionChanged += OnModelFeatureCoordinateDataCollectionChanged;
            ((INotifyPropertyChanged) modelFeatureCoordinateData).PropertyChanged += OnModelFeatureCoordinateDataPropertyChanged;

            if (modelFeatureCoordinateData.Feature != null)
            {
                ((INotifyPropertyChanged) modelFeatureCoordinateData.Feature).PropertyChanged += OnGeometryChanged;
            }
        }

        private void UnSubscribeToEvents()
        {
            if (modelFeatureCoordinateData == null)
            {
                return;
            }

            ((INotifyCollectionChanged) modelFeatureCoordinateData).CollectionChanged -= OnModelFeatureCoordinateDataCollectionChanged;
            ((INotifyPropertyChanged) modelFeatureCoordinateData).PropertyChanged -= OnModelFeatureCoordinateDataPropertyChanged;

            if (modelFeatureCoordinateData.Feature != null)
            {
                ((INotifyPropertyChanged) modelFeatureCoordinateData.Feature).PropertyChanged -= OnGeometryChanged;
            }
        }

        private void OnModelFeatureCoordinateDataPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var dataColumn = sender as IDataColumn;
            if (dataColumn == null || propertyChangedEventArgs.PropertyName != nameof(dataColumn.IsActive))
            {
                return;
            }

            UpdateColumnPropertyInformation();
        }

        private void OnGeometryChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(modelFeatureCoordinateData.Feature.Geometry))

            {
                UpdateRowsForGeometry();
            }
        }

        private void OnModelFeatureCoordinateDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender != modelFeatureCoordinateData.DataColumns)
            {
                return;
            }

            UpdateColumnPropertyInformation();
        }

        private void UpdateColumnPropertyInformation()
        {
            var defaultCoordinateDescriptors = new[]
            {
                new CoordinateDataRowGeometryPropertyDescriptor("X") {Type = GeometryPropertyDescriptorType.XValue},
                new CoordinateDataRowGeometryPropertyDescriptor("Y") {Type = GeometryPropertyDescriptorType.YValue}
            };

            IEnumerable<PropertyDescriptor> rowPropertyDescriptors = modelFeatureCoordinateData?.DataColumns
                                                                                               .Where(dc => dc.IsActive)
                                                                                               .Select((c, i) => new CoordinateDataRowPropertyDescriptor(GetPropertyName(c.Name), c.Name, c.DataType, i))
                                                                     ?? Enumerable.Empty<PropertyDescriptor>();

            coordinateDataRowPropertyDescriptors.Clear();
            coordinateDataRowPropertyDescriptors.AddRange(defaultCoordinateDescriptors.Concat(rowPropertyDescriptors));

            ClearColumns?.Invoke();

            foreach (PropertyDescriptor rowPropertyDescriptor in coordinateDataRowPropertyDescriptors)
            {
                bool isCoordinatePropertyDescriptor = rowPropertyDescriptor is CoordinateDataRowGeometryPropertyDescriptor;
                AddColumn?.Invoke(rowPropertyDescriptor.Name, rowPropertyDescriptor.DisplayName, isCoordinatePropertyDescriptor, isCoordinatePropertyDescriptor ? "{0:E3}" : null);
            }
        }

        private static string GetPropertyName(string name)
        {
            return name.Replace(" ", "_").Replace("[", "_").Replace("]", "_");
        }

        private void UpdateRowsForGeometry()
        {
            coordinateDataRows = new ObservableCollection<CoordinateDataRow>();

            if (modelFeatureCoordinateData?.Feature?.Geometry != null)
            {
                for (var index = 0; index < modelFeatureCoordinateData.Feature.Geometry.Coordinates.Length; index++)
                {
                    coordinateDataRows.Add(new CoordinateDataRow(modelFeatureCoordinateData, index, coordinateDataRowPropertyDescriptors));
                }
            }

            OnPropertyChanged(nameof(CoordinateDataRows));
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    [Entity]
    public class ModelFeatureCoordinateData<TFeature> : IModelFeatureCoordinateData where TFeature : IFeature
    {
        private TFeature feature;
        private IGeometry previousGeometry;

        public ModelFeatureCoordinateData()
        {
            DataColumns = new EventedList<IDataColumn>();
            DataColumns.CollectionChanged += DataColumnsCollectionChanged;
        }

        public TFeature Feature
        {
            get => feature;
            set
            {
                if (feature != null)
                {
                    ((INotifyPropertyChanged)feature).PropertyChanged -= GeometryChanged;
                }

                feature = value;

                if (feature != null)
                {
                    previousGeometry = feature.Geometry;
                    DataColumns.ForEach(SyncDataColumnValueList);
                    ((INotifyPropertyChanged)feature).PropertyChanged += GeometryChanged;
                }
                else
                {
                    DataColumns.ForEach(dc => dc.ValueList.Clear());
                }
            }
        }

        public IEventedList<IDataColumn> DataColumns { get; private set; }

        IFeature IModelFeatureCoordinateData.Feature
        {
            get => Feature;
            set => Feature = (TFeature)value;
        }

        public void Dispose()
        {
            Feature = default(TFeature);
        }

        private void DataColumnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    SyncDataColumnValueList((IDataColumn)e.GetRemovedOrAddedItem());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void SyncDataColumnValueList(IDataColumn dataColumn)
        {
            if (feature?.Geometry?.Coordinates == null)
            {
                return;
            }

            int length = feature.Geometry.Coordinates.Length;
            int valueListCount = dataColumn.ValueList.Count;
            int delta = Math.Abs(length - valueListCount);

            if (valueListCount < length)
            {
                for (var i = 0; i < delta; i++)
                {
                    dataColumn.ValueList.Add(dataColumn.DefaultValue);
                }
            }

            if (valueListCount > length)
            {
                for (int i = valueListCount - 1; i >= valueListCount - delta; i--)
                {
                    dataColumn.ValueList.RemoveAt(i);
                }
            }
        }

        private void GeometryChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Equals(sender, Feature) || e.PropertyName != nameof(Feature.Geometry))
            {
                return;
            }

            SynchronizeWithNewCoordinates();
        }

        private void SynchronizeWithNewCoordinates()
        {
            if (feature.Geometry.Coordinates.Length == previousGeometry.Coordinates.Length)
            {
                previousGeometry = feature.Geometry;
            }
            else
            {
                var coordinateComparison2D = new CoordinateComparison2D();
                List<Coordinate> geometryCoordinates = feature.Geometry.Coordinates.ToList();

                var pointerList = new List<int>(previousGeometry.Coordinates.Length);
                foreach (Coordinate previousGeometryCoordinate in previousGeometry.Coordinates)
                {
                    int toIndex = -1;
                    for (var i = 0; i < geometryCoordinates.Count; i++)
                    {
                        if (!coordinateComparison2D.Equals(geometryCoordinates[i], previousGeometryCoordinate))
                        {
                            continue;
                        }

                        toIndex = i;
                        break;
                    }

                    pointerList.Add(toIndex);
                }

                DataColumns.ForEach(dc => UpdateColumnValuesWithPointerTable(dc, pointerList));
                previousGeometry = feature.Geometry;
            }
        }

        private void UpdateColumnValuesWithPointerTable(IDataColumn dataColumn, List<int> pointerList)
        {
            IList originalList = dataColumn.ValueList;
            IList list = dataColumn.CreateDefaultValueList(feature.Geometry.Coordinates.Length);

            for (var i = 0; i < pointerList.Count; i++)
            {
                int toIndex = pointerList[i];
                if (toIndex == -1)
                {
                    continue;
                }

                list[toIndex] = originalList[i];
            }

            dataColumn.ValueList = list;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData
{
    public class ModelFeatureCoordinateData<TFeature> : IDisposable where TFeature : IFeature
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
            get { return feature; }
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

        public void Dispose()
        {
            Feature = default(TFeature);
        }

        private void DataColumnsCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    SyncDataColumnValueList((IDataColumn) e.Item);
                    break;
                case NotifyCollectionChangeAction.Remove:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void SyncDataColumnValueList(IDataColumn dataColumn)
        {
            if (feature?.Geometry?.Coordinates == null) return;

            var length = feature.Geometry.Coordinates.Length;
            var valueListCount = dataColumn.ValueList.Count;
            var delta = Math.Abs(length - valueListCount);

            if (valueListCount < length)
            {
                for (int i = 0; i < delta; i++)
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
            var coordinateComparison2D = new CoordinateComparison2D();
            var geometryCoordinates = feature.Geometry.Coordinates.ToList();

            // todo increase performance (Hashset ??)
            var pointerList = new List<int>(previousGeometry.Coordinates.Length);
            foreach (var previousGeometryCoordinate in previousGeometry.Coordinates)
            {
                var toIndex = -1;
                for (int i = 0; i < geometryCoordinates.Count; i++)
                {
                    if (!coordinateComparison2D.Equals(geometryCoordinates[i], previousGeometryCoordinate)) continue;

                    toIndex = i;
                    break;
                }

                pointerList.Add(toIndex);
            }

            DataColumns.ForEach(dc => UpdateColumnValuesWithPointerTable(dc, pointerList));
        }

        private void UpdateColumnValuesWithPointerTable(IDataColumn dataColumn, List<int> pointerList)
        {
            var originalList = dataColumn.ValueList;
            var list = dataColumn.CreateDefaultValueList(feature.Geometry.Coordinates.Length);

            for (int i = 0; i < pointerList.Count; i++)
            {
                var toIndex = pointerList[i];
                if (toIndex == -1) continue;

                list[toIndex] = originalList[i];
            }

            dataColumn.ValueList = list;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
   

namespace DelftTools.Hydro.Structures
{
    [Entity]
    public class ModelFeatureCoordinateData : IDisposable, IModelFeatureCoordinateData
    {
        private IModelDataColumnsFeature feature;
        private IGeometry previousGeometry;
        private object selector;
        
        public ModelFeatureCoordinateData(IModelDataColumnsFeature feature) 
        {
            DataColumns = new EventedList<IDataColumn>();
            Feature = feature;
        }

        public ModelFeatureCoordinateData(IModelDataColumnsFeature feature, object selector)
        {
            DataColumns = new EventedList<IDataColumn>();
            Selector = selector;
            Feature = feature;
        }

        public object Selector
        {
            get => selector;
            set
            {
                if (selector == null || !selector.Equals(value))
                {
                    selector = value;
                    UpdateDataColumns();
                }
            }
        }

        public IModelDataColumnsFeature Feature
        {
            get { return feature; }
            set
            {
                if (feature != null)
                {
                    ((INotifyPropertyChanged) feature).PropertyChanged -= GeometryChanged;
                    if (DataColumns != null) DataColumns.ForEach(dc => dc.ValueList.Clear());
                    DataColumns = new EventedList<IDataColumn>(); ;
                }

                feature = value;

                if (feature != null)
                {
                    previousGeometry = feature.Geometry;
                    ((INotifyPropertyChanged) feature).PropertyChanged += GeometryChanged;
                    var featureWeCanGenerateColumnsFor = feature as IModelDataColumnsFeature;
                    if (featureWeCanGenerateColumnsFor != null)
                    {
                        DataColumns = featureWeCanGenerateColumnsFor.GenerateDataColumns(this);
                        DataColumns.ForEach(SyncDataColumnValueList);
                        return;
                    }
                }
            }
        }

        public IEventedList<IDataColumn> DataColumns { get; private set; }
        
        public void Dispose()
        {
            Feature = null;
            DataColumns.ForEach(dc => dc.ValueList.Clear());
            DataColumns = null;
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

        private void UpdateDataColumns()
        {
            if (Feature != null)
            {
                Feature.UpdateDataColumns(this);
                DataColumns.ForEach(SyncDataColumnValueList);
            }
        }

        
        
    }
}
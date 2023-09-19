using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class MultiDimentionalArrayAdapter<T> : IMultiDimensionalArray<T>
    {
        private readonly IList<T> values;
        private object maxValue;
        private object minValue;

        public event EventHandler<MultiDimensionalArrayChangingEventArgs> CollectionChanging;

        public event EventHandler<MultiDimensionalArrayChangingEventArgs> CollectionChanged;

        public MultiDimentionalArrayAdapter(IList<T> values)
        {
            this.values = values;
        }

        public long Id { get; set; }

        public object MaxValue => maxValue ?? (maxValue = values.Max());

        public object MinValue => minValue ?? (minValue = values.Min());

        int IMultiDimensionalArray<T>.Count => values.Count;

        int IMultiDimensionalArray.Count => values.Count;

        int ICollection<T>.Count => values.Count;

        int ICollection.Count => values.Count;

        bool IMultiDimensionalArray.IsReadOnly
        {
            get => true;
            set {}
        }

        bool ICollection<T>.IsReadOnly => true;

        bool IList.IsReadOnly => true;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            CopyTo((T[]) array, index);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            values.CopyTo(array, arrayIndex);
        }

        public bool Contains(object value)
        {
            return values.Contains((T) value);
        }

        public bool Contains(T item)
        {
            return values.Contains(item);
        }

        public int IndexOf(object value)
        {
            return IndexOf((T) value);
        }

        public int IndexOf(T item)
        {
            return values.IndexOf(item);
        }

        public object Clone()
        {
            return new MultiDimentionalArrayAdapter<T>(values);
        }

        public Type GetEntityType()
        {
            return GetType();
        }

        T IMultiDimensionalArray<T>.this[params int[] indexes]
        {
            get
            {
                if (indexes.Length == 1)
                {
                    return values[indexes[0]];
                }

                throw new NotImplementedException();
            }
            set => throw new NotImplementedException();
        }

        object IMultiDimensionalArray.this[params int[] index]
        {
            get
            {
                if (index.Length == 1)
                {
                    return values[index[0]];
                }

                throw new NotImplementedException();
            }
            set => throw new NotImplementedException();
        }

        T IList<T>.this[int index]
        {
            get => values[index];
            set => throw new NotImplementedException();
        }

        object IList.this[int index]
        {
            get => values[index];
            set => throw new NotImplementedException();
        }

        #region Unused properties

        public object SyncRoot { get; private set; }

        public bool IsSynchronized { get; private set; }

        public object DefaultValue { get; set; }

        public int[] Shape { get; private set; }

        public int[] Stride { get; private set; }

        public int Rank { get; private set; }

        public bool FireEvents { get; set; }

        public bool IsAutoSorted { get; set; }

        public bool IsFixedSize => true;

        #endregion

        #region Unsupported functions

        public IMultiDimensionalArrayView<T> Select(int dimension, int[] indexes)
        {
            throw new NotImplementedException();
        }

        IMultiDimensionalArrayView IMultiDimensionalArray.Select(int[] start, int[] end)
        {
            return Select(start, end);
        }

        public IMultiDimensionalArrayView<T> Select(int dimension, int start, int end)
        {
            throw new NotImplementedException();
        }

        public IMultiDimensionalArrayView<T> Select(int[] start, int[] end)
        {
            throw new NotImplementedException();
        }

        IMultiDimensionalArrayView IMultiDimensionalArray.Select(int dimension, int start, int end)
        {
            return Select(dimension, start, end);
        }

        IMultiDimensionalArrayView IMultiDimensionalArray.Select(int dimension, int[] indexes)
        {
            return Select(dimension, indexes);
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void AddRange(IList values)
        {
            throw new NotImplementedException();
        }

        public void ReplaceValues(IList newValues)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public int InsertAt(int dimension, int index, int length, IList valuesToInsert)
        {
            throw new NotImplementedException();
        }

        public void InsertAt(int dimension, int index)
        {
            throw new NotImplementedException();
        }

        public void InsertAt(int dimension, int index, int length)
        {
            throw new NotImplementedException();
        }

        public void Move(int dimension, int index, int length, int newIndex)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void IMultiDimensionalArray.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void IMultiDimensionalArray<T>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int dimension, int index)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int dimension, int index, int length)
        {
            throw new NotImplementedException();
        }

        void IList.Clear()
        {
            throw new NotImplementedException();
        }

        void IMultiDimensionalArray.Clear()
        {
            throw new NotImplementedException();
        }

        void IMultiDimensionalArray<T>.Clear()
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotImplementedException();
        }

        public void Resize(params int[] newShape)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain
{
    [Serializable]
    [Entity(FireOnCollectionChange=false)]
    public class AreaDictionary<T> : EditableObjectUnique<long>, IDictionary<T, double>, ISerializable, ICloneable
    {
        private readonly IDictionary<T, double> internalDictionary;

        public AreaDictionary()
        {
            internalDictionary = new Dictionary<T, double>();
        }

        #region ICloneable Members

        public object Clone()
        {
            var clone = (AreaDictionary<T>) Activator.CreateInstance(GetType());
            foreach (T key in internalDictionary.Keys)
            {
                clone.Add(key, internalDictionary[key]);
            }
            return clone;
        }

        #endregion

        #region IDictionary<T,double> Members

        public void Add(T key, double value)
        {
            internalDictionary.Add(key, value);
            OnSumChanged();
        }

        [NoNotifyPropertyChange]
        public virtual double this[T key]
        {
            get { return internalDictionary[key]; }
            set
            {
                double currentValue;

                if (internalDictionary.TryGetValue(key, out currentValue) && !(Math.Abs(currentValue - value) > 0.00001))
                    return;

                internalDictionary[key] = value;
                OnSumChanged();
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return internalDictionary.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<T, double>> GetEnumerator()
        {
            return internalDictionary.GetEnumerator();
        }

        public void Add(KeyValuePair<T, double> item)
        {
            internalDictionary.Add(item);
            OnSumChanged();
        }

        public void Clear()
        {
            internalDictionary.Clear();
            OnSumChanged();
        }

        public bool Contains(KeyValuePair<T, double> item)
        {
            return internalDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<T, double>[] array, int arrayIndex)
        {
            internalDictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<T, double> item)
        {
            bool remove = internalDictionary.Remove(item);
            OnSumChanged();
            return remove;
        }

        public int Count
        {
            get { return internalDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return internalDictionary.IsReadOnly; }
        }

        public bool ContainsKey(T key)
        {
            return internalDictionary.ContainsKey(key);
        }

        public bool Remove(T key)
        {
            bool remove = internalDictionary.Remove(key);
            OnSumChanged();
            return remove;
        }

        public bool TryGetValue(T key, out double value)
        {
            return internalDictionary.TryGetValue(key, out value);
        }

        public ICollection<T> Keys
        {
            get { return internalDictionary.Keys; }
        }

        public ICollection<double> Values
        {
            get { return internalDictionary.Values; }
        }

        public double Sum
        {
            get
            {
                return Values.Sum();
            }
        }

        #endregion

        #region Serialization

        protected AreaDictionary(SerializationInfo info, StreamingContext context)
        {
            internalDictionary =
                (Dictionary<T, double>) info.GetValue("internalDictionary", typeof (Dictionary<T, double>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("internalDictionary", internalDictionary);
        }

        #endregion

        public void Reset(T resetType, double value)
        {
            foreach (var key in internalDictionary.Keys.ToList())
            {
                internalDictionary[key] = 0.0;
            }
            internalDictionary[resetType] = value;
        }

        public event EventHandler<AreaSumChangedEventArgs> SumChanged;


        protected virtual void OnSumChanged()
        {
            SumChanged?.Invoke(this, new AreaSumChangedEventArgs(Sum));
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Editing;

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
                
                BeginEdit(new AreaDictionaryModificationEditAction(this));
                internalDictionary[key] = value;
                EndEdit();
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
        }

        public void Clear()
        {
            internalDictionary.Clear();
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
            return internalDictionary.Remove(item);
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
            return internalDictionary.Remove(key);
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
            BeginEdit(new AreaDictionaryModificationEditAction(this));

            foreach (var key in internalDictionary.Keys.ToList())
            {
                internalDictionary[key] = 0.0;
            }
            internalDictionary[resetType] = value;

            EndEdit();
        }

        #region Undo/redo stuff

        private class AreaDictionaryModificationEditAction : EditActionBase
        {
            private readonly IDictionary<T, double> valuesBefore = new Dictionary<T, double>();
            private readonly AreaDictionary<T> instance;

            private AreaDictionaryModificationEditAction() : base("Area dictionary modification") { }

            public AreaDictionaryModificationEditAction(AreaDictionary<T> instance) : this()
            {
                this.instance = instance;

                //remember state
                valuesBefore = new Dictionary<T, double>(instance);
            }

            public override bool HandlesRestore { get { return true; } }
            
            public override void Restore()
            {
                instance.BeginEdit(new AreaDictionaryModificationEditAction(instance));

                foreach (var key in valuesBefore.Keys)
                {
                    instance.internalDictionary[key] = valuesBefore[key];
                }

                instance.EndEdit();
            }
        }

        #endregion
    }
}
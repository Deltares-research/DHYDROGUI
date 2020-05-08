using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Serialization;
using IEditableObject = System.ComponentModel.IEditableObject;

namespace DelftTools.Hydro.CrossSections.DataSets
{
    public abstract class LightDataTable<T> : LightDataTable, IEnumerable<T> where T : LightDataRow, new()
    {
        protected LightDataTable() {}

        protected LightDataTable(SerializationInfo info, StreamingContext context)
            : base(info, context) {}

        public new T this[int index] => Rows[index];

        public new LightBindingList<T> Rows { get; private set; }

        public override void Clear()
        {
            Rows.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        protected override void Initialize()
        {
            Rows = new LightBindingList<T>
            {
                AllowEdit = true,
                AllowNew = true,
                AllowRemove = true
            };
            Rows.ListChanged += RowsChanged;
            Rows.AddingNew += RowsAddingNew;
        }

        protected abstract SortOrder GetSortOrder();

        protected override IList GetRows()
        {
            return Rows;
        }

        protected override LightDataRow GetRow(int index)
        {
            return Rows[index];
        }

        protected override IEnumerator GetEnumeratorCore()
        {
            return GetEnumerator();
        }

        internal override void HandleRowChanged(LightDataRow row, double[] oldState, double[] newState)
        {
            ApplySorting(row);
            Rows.ResetItem(Rows.IndexOf((T) row));
            base.HandleRowChanged(row, oldState, newState);
        }

        private void RowsAddingNew(object sender, AddingNewEventArgs e)
        {
            var defaultRow = new T();
            EnsureUniqueAndAtEndOfTable(defaultRow);
            e.NewObject = defaultRow;
        }

        private void RowsChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                {
                    T row = Rows[e.NewIndex];
                    row.Table = this;
                    ApplySorting(row);
                    HandleRowAdded(row, Rows.IndexOf(row));
                    break;
                }
                case ListChangedType.ItemDeleted:
                    HandleRowRemoved(Rows.RemovedItem, e.NewIndex);
                    break;
                case ListChangedType.ItemMoved:
                    break; // sorting, do nothing
                case ListChangedType.ItemChanged:
                    break;
                case ListChangedType.Reset:
                    if (Rows.ItemsBeforeClear != null && Rows.ItemsBeforeClear.Count > 0)
                    {
                        HandleListCleared(Rows.ItemsBeforeClear.Cast<LightDataRow>().ToList());
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void EnsureUniqueAndAtEndOfTable(T row)
        {
            // so we aren't bothered with jumping rows
            double proposedValue = row[0];
            double[] existingValues = Rows.Where(r => r != row).Select(r => r[0]).ToArray();
            if (!existingValues.Contains(proposedValue))
            {
                return;
            }

            double value = existingValues.Any()
                               ? GetSortOrder() == SortOrder.Ascending
                                     ? existingValues.Max() + 1
                                     : existingValues.Min() - 1
                               : 0.0;
            row[0] = value;
        }

        private void ApplySorting(LightDataRow row)
        {
            var typedRow = (T) row;
            int sortedIndex = GetSortedIndex(typedRow);
            if (sortedIndex != Rows.IndexOf(typedRow))
            {
                Rows.Move(typedRow, sortedIndex);
            }
        }

        private int GetSortedIndex(LightDataRow row)
        {
            // get sorted index while ignoring self
            const int columnIndex = 0;
            double value = row[columnIndex];
            var index = 0;
            SortOrder direction = GetSortOrder();
            for (var i = 0; i < Rows.Count; i++)
            {
                T r = Rows[i];
                if (r == row)
                {
                    continue; // ignore self
                }

                if (direction == SortOrder.Descending && value >= r[columnIndex])
                {
                    return index;
                }

                if (direction == SortOrder.Ascending && value <= r[columnIndex])
                {
                    return index;
                }

                index++;
            }

            return index;
        }
    }

    [Serializable]
    public abstract class LightDataTable : EditableObjectUnique<long>, IEnumerable, IListSource, ISerializable
    {
        private bool enforceConstraints;

        public event EventHandler<LightDataRowChangeEventArgs> RowChanging;

        protected LightDataTable()
        {
            Initialize();
            EnforceConstraints = true;
        }

        protected LightDataTable(SerializationInfo info, StreamingContext context) : this()
        {
            var reader = new SerializationReader((byte[]) info.GetValue("data", typeof(byte[])));
            int count = reader.ReadInt32();

            EnforceConstraints = false;
            for (var i = 0; i < count; i++)
            {
                var values = new double[NumColumns];
                for (var j = 0; j < NumColumns; j++)
                {
                    values[j] = reader.ReadDouble();
                }

                AddByValues(values);
            }

            EnforceConstraints = true;
        }

        public LightDataRow this[int index] => GetRow(index);

        public bool HasErrors { get; private set; }

        public int Count => GetRows().Count;

        public IList<LightDataRow> Rows => GetRows().Cast<LightDataRow>().ToList().AsReadOnly();

        public bool EnforceConstraints
        {
            get => enforceConstraints;
            set
            {
                enforceConstraints = value;
                if (!enforceConstraints)
                {
                    return;
                }

                if (GetRows() != null)
                {
                    DoEnforceConstraints();
                }
            }
        }

        public bool ContainsListCollection => true;

        public void BeginLoadData() {}

        public void EndLoadData() {}

        public bool ContentEquals(LightDataTable table)
        {
            LightDataTable table1 = this;
            LightDataTable table2 = table;

            if (!(table1.GetType() == table2.GetType()))
            {
                return false;
            }

            if (table1.Rows.Count != table2.Rows.Count)
            {
                return false;
            }

            for (var i = 0; i < table1.Rows.Count; i++)
            {
                for (var j = 0; j < table1.NumColumns; j++)
                {
                    if (!table1.Rows[i][j].Equals(table2.Rows[i][j]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public abstract void Clear();

        public void Add(double[] itemArray)
        {
            AddByValues(itemArray);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorCore();
        }

        public IList GetList()
        {
            return GetRows();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var writer = new SerializationWriter();
            writer.Write(Count); // not really needed but very handy in deserialization.
            foreach (LightDataRow row in Rows)
            foreach (double item in row.ItemArray)
            {
                writer.Write(item);
            }

            info.AddValue("data", writer.ToArray());
        }

        protected abstract int NumColumns { get; }

        protected abstract void Initialize();

        protected virtual void DoEnforceConstraints() {}

        protected abstract IList GetRows();
        protected abstract LightDataRow GetRow(int index);
        protected abstract IEnumerator GetEnumeratorCore();

        protected abstract void AddByValues(double[] itemArray);

        private IList MutableRows => GetRows();

        private void OnRowChanging(LightDataRow row, DataRowAction action)
        {
            if (EnforceConstraints && (action == DataRowAction.Add || action == DataRowAction.Change))
            {
                DoEnforceConstraints();
            }

            if (RowChanging != null)
            {
                RowChanging(this, new LightDataRowChangeEventArgs
                {
                    Action = action,
                    Row = row
                });
            }
        }

        #region Undo/Redo

        [EditAction(typeof(ListClearedAction))]
        internal virtual void HandleListCleared(IList<LightDataRow> rows) {}

        [EditAction(typeof(RowChangeAction))]
        internal virtual void HandleRowChanged(LightDataRow row, double[] oldState, double[] newState)
        {
            OnRowChanging(row, DataRowAction.Change);
        }

        [EditAction(typeof(RowAddAction))]
        protected virtual void HandleRowAdded(LightDataRow row, int index)
        {
            OnRowChanging(row, DataRowAction.Add);
        }

        [EditAction(typeof(RowRemoveAction))]
        protected virtual void HandleRowRemoved(LightDataRow row, int index)
        {
            OnRowChanging(row, DataRowAction.Delete);
        }

        private class ListClearedAction : EditActionBase
        {
            public ListClearedAction()
                : base("List cleared") {}

            public override bool HandlesRestore => true;

            public override void Restore()
            {
                var table = (LightDataTable) Instance;
                var rows = (IList<LightDataRow>) Arguments[0];
                table.BeginEdit("Fill list"); // restore in multiple events :-(
                foreach (LightDataRow row in rows)
                {
                    table.MutableRows.Add(row);
                }

                table.EndEdit();
            }
        }

        private class RowAddAction : EditActionBase
        {
            public RowAddAction() : base("Add row") {}

            public override bool HandlesRestore => true;

            public override void Restore()
            {
                var table = (LightDataTable) Instance;
                var index = (int) Arguments[1];
                table.MutableRows.RemoveAt(index);
            }
        }

        private class RowChangeAction : EditActionBase
        {
            public RowChangeAction() : base("Change row") {}

            public override bool HandlesRestore => true;

            public override void Restore()
            {
                var row = (LightDataRow) Arguments[0];
                var itemArray = (double[]) Arguments[1];
                row.BeginEdit();
                for (var i = 0; i < itemArray.Length; i++)
                {
                    row[i] = itemArray[i];
                }

                row.EndEdit();
            }
        }

        private class RowRemoveAction : EditActionBase
        {
            public RowRemoveAction() : base("Remove row") {}

            public override bool HandlesRestore => true;

            public override void Restore()
            {
                var table = (LightDataTable) Instance;
                var row = (LightDataRow) Arguments[0];
                var index = (int) Arguments[1];
                table.MutableRows.Insert(index, row);
            }
        }

        #endregion
    }

    public class LightBindingList<T> : BindingList<T>
    {
        public T RemovedItem { get; private set; }
        public IList<T> ItemsBeforeClear { get; private set; }

        protected override void ClearItems()
        {
            ItemsBeforeClear = this.ToList();
            base.ClearItems();
            ItemsBeforeClear = null;
        }

        protected override void RemoveItem(int index)
        {
            bool wasAllowingRemove = AllowRemove;
            try
            {
                AllowRemove = true;

                RemovedItem = this[index];
                base.RemoveItem(index);
                RemovedItem = default(T);
            }
            finally
            {
                AllowRemove = wasAllowingRemove;
            }
        }

        internal void Move(T item, int index)
        {
            // index = index while ignoring self

            bool wasRaising = RaiseListChangedEvents;
            bool wasAllowingRemove = AllowRemove;
            try
            {
                RaiseListChangedEvents = false;
                AllowRemove = true;

                int oldIndex = IndexOf(item);
                Remove(item);
                InsertItem(index, item);
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemMoved, index, oldIndex));
            }
            finally
            {
                RaiseListChangedEvents = wasRaising;
                AllowRemove = wasAllowingRemove;
            }
        }
    }

    public abstract class LightDataRow : IEditableObject
    {
        private double[] oldState;
        private bool inTransaction;

        protected LightDataRow(int size)
        {
            ItemArray = new double[size];
        }

        public double this[int index]
        {
            get => ItemArray[index];
            set => Set(index, value);
        }

        [Browsable(false)]
        public double[] ItemArray { get; private set; }

        [Browsable(false)]
        public LightDataTable Table { get; internal set; }

        public void BeginEdit()
        {
            inTransaction = true;
        }

        public void CancelEdit()
        {
            if (oldState != null)
            {
                ItemArray = oldState;
            }

            oldState = null;
        }

        public void EndEdit()
        {
            inTransaction = false;

            if (oldState == null)
            {
                return; // nothing was changed
            }

            if (!oldState.SequenceEqual(ItemArray))
            {
                OnRowChanged();
            }

            oldState = null;
        }

        protected void Set(int index, double value)
        {
            BeginEditManually();
            ItemArray[index] = value;

            if (!inTransaction) // noone called BeginEdit on us first, so we have to handle this right now
            {
                OnRowChanged();
                oldState = null; // mini transaction
            }
        }

        private void BeginEditManually()
        {
            if (oldState == null)
            {
                oldState = (double[]) ItemArray.Clone();
            }
        }

        private void OnRowChanged()
        {
            try
            {
                if (Table != null) // can be null if not yet added to table
                {
                    Table.HandleRowChanged(this, oldState, ItemArray);
                }
            }
            catch (Exception)
            {
                CancelEdit();
                throw;
            }
        }
    }

    public class LightDataValueChangeEventArgs : EventArgs
    {
        public LightDataRow Row { get; set; }
        public double ProposedValue { get; set; }
    }

    public class LightDataRowChangeEventArgs : EventArgs
    {
        public LightDataRow Row { get; set; }
        public DataRowAction Action { get; set; }
    }

    public delegate void LightDataValueChangeEventHandler(object sender, LightDataValueChangeEventArgs args);
}
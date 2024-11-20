using System;
using System.Collections.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    public abstract class ZWProfileMutatorBase : ICrossSectionProfileMutator
    {
        protected CrossSectionDefinitionZW CrossSectionDefinition;

        protected IList<CrossSectionDataSet.CrossSectionZWRow> GetSortedRows()
        {
            return CrossSectionDefinition.ZWDataTable.Rows;
        }

        protected CrossSectionDataSet.CrossSectionZWRow GetRow(int profileIndex)
        {
            return GetSortedRows()[CrossSectionDefinition.GetRawDataTableIndex(profileIndex)];
        }

        protected void ThrowIfHeightChangeWouldChangeOrdering(CrossSectionDataSet.CrossSectionZWRow row, double zValue)
        {
            var rows = GetSortedRows();

            var previousRowIndex = rows.IndexOf(row) - 1;
            var previousLevel = previousRowIndex >= 0 ? rows[previousRowIndex].Z : double.MaxValue;
            var nextRowIndex = rows.IndexOf(row) + 1;
            var nextLevel = nextRowIndex < rows.Count ? rows[nextRowIndex].Z : double.MinValue;

            if (!(nextLevel < zValue && zValue < previousLevel)) //descending: (z.IsInRange(next, previous))
            {
                throw new ArgumentException("Change of level would change internal ordering of crossection values, which is not supported.");
            }
        }

        public abstract void MovePoint(int index, double y, double z);
        public abstract void AddPoint(double y, double z);
        public abstract void DeletePoint(int index);
        public abstract bool CanDelete { get; }
        public abstract bool CanAdd { get; }

        public bool CanMove
        {
            get { return true; }
        }

        public bool ClipHorizontal
        {
            get { return false; }
        }

        public bool ClipVertical
        {
            get { return true; }
        }
        
        public bool FixHorizontal
        {
            get { return false; }
        }

        public abstract bool FixVertical { get; }
    }
}
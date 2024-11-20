using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    public class YZProfileMutator : ICrossSectionProfileMutator
    {
        private readonly CrossSectionDefinitionYZ crossSectionDefinition;

        public YZProfileMutator(CrossSectionDefinitionYZ crossSectionDefinition)
        {
            this.crossSectionDefinition = crossSectionDefinition;
        }

        public void MovePoint(int index, double y, double z)
        {
            crossSectionDefinition.BeginEdit(CrossSectionDefinition.DefaultEditAction);

            try
            {
                var row = crossSectionDefinition.GetRow(index);
                row.Yq = y;
                row.Z = z;
            }
            finally
            {
                crossSectionDefinition.EndEdit();
            }
        }

        public void AddPoint(double y, double z)
        {
            if (crossSectionDefinition.YZDataTable.Rows.Select(r => Math.Round(r.Yq, 3)).Contains(Math.Round(y, 3)))
            {
                return;
            }

            var row = crossSectionDefinition.YZDataTable.AddCrossSectionYZRow(y, z);

            if (crossSectionDefinition.YZDataTable.Count > 2)
            {
                var sortedRows = crossSectionDefinition.YZDataTable.OrderBy(r => r.Yq).ToList();
                var rowIndex = sortedRows.IndexOf(row);

                double correctedStorageWidthValue;
                if (rowIndex == 0)
                {
                    correctedStorageWidthValue = sortedRows[rowIndex + 1].DeltaZStorage;
                }
                else if (rowIndex == crossSectionDefinition.YZDataTable.Count)
                {
                    correctedStorageWidthValue = sortedRows[rowIndex - 1].DeltaZStorage;
                }
                else
                {
                    var leftRow = sortedRows[rowIndex - 1];
                    var rightRow = sortedRows[rowIndex + 1];

                    // Linearly interpolate:
                    var y1 = leftRow.Yq;
                    var s1 = leftRow.DeltaZStorage;

                    var y2 = rightRow.Yq;
                    var s2 = rightRow.DeltaZStorage;

                    correctedStorageWidthValue = (s1 - s2) / (y1 - y2) * (y - y1) + s1;
                }

                row.DeltaZStorage = correctedStorageWidthValue;
            }
        }

        public void DeletePoint(int index)
        {
            var row = crossSectionDefinition.GetRow(index);
            if (row != null)
            {
                crossSectionDefinition.YZDataTable.RemoveCrossSectionYZRow(row);
            }
        }

        public bool CanDelete
        {
            get { return true; }
        }

        public bool CanAdd
        {
            get { return true; }
        }

        public bool CanMove
        {
            get { return true; }
        }

        public bool ClipHorizontal
        {
            get { return true; }
        }

        public bool ClipVertical
        {
            get { return false; }
        }

        public bool FixHorizontal
        {
            get { return false; }
        }

        public bool FixVertical
        {
            get { return false; }
        }
    }
}
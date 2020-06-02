using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    public class YZProfileMutator : ICrossSectionProfileMutator
    {
        private readonly CrossSectionDefinitionYZ crossSectionDefinition;

        public YZProfileMutator(CrossSectionDefinitionYZ crossSectionDefinition)
        {
            this.crossSectionDefinition = crossSectionDefinition;
        }

        public bool CanDelete
        {
            get
            {
                return true;
            }
        }

        public bool CanAdd
        {
            get
            {
                return true;
            }
        }

        public bool CanMove
        {
            get
            {
                return true;
            }
        }

        public bool ClipHorizontal
        {
            get
            {
                return true;
            }
        }

        public bool ClipVertical
        {
            get
            {
                return false;
            }
        }

        public bool FixHorizontal
        {
            get
            {
                return false;
            }
        }

        public bool FixVertical
        {
            get
            {
                return false;
            }
        }

        public void MovePoint(int index, double y, double z)
        {
            crossSectionDefinition.BeginEdit(CrossSectionDefinition.DefaultEditAction);

            try
            {
                CrossSectionDataSet.CrossSectionYZRow row = crossSectionDefinition.GetRow(index);
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

            var suggestedStorageWidth = 0.0;
            if (crossSectionDefinition.YZDataTable.Count == 1)
            {
                suggestedStorageWidth = crossSectionDefinition.YZDataTable[0].DeltaZStorage;
            }

            CrossSectionDataSet.CrossSectionYZRow row = crossSectionDefinition.YZDataTable.AddCrossSectionYZRow(y, z, suggestedStorageWidth);

            if (crossSectionDefinition.YZDataTable.Count > 2)
            {
                List<CrossSectionDataSet.CrossSectionYZRow> sortedRows = crossSectionDefinition.YZDataTable.OrderBy(r => r.Yq).ToList();
                int rowIndex = sortedRows.IndexOf(row);

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
                    CrossSectionDataSet.CrossSectionYZRow leftRow = sortedRows[rowIndex - 1];
                    CrossSectionDataSet.CrossSectionYZRow rightRow = sortedRows[rowIndex + 1];

                    // Linearly interpolate:
                    double y1 = leftRow.Yq;
                    double s1 = leftRow.DeltaZStorage;

                    double y2 = rightRow.Yq;
                    double s2 = rightRow.DeltaZStorage;

                    correctedStorageWidthValue = (((s1 - s2) / (y1 - y2)) * (y - y1)) + s1;
                }

                row.DeltaZStorage = correctedStorageWidthValue;
            }
        }

        public void DeletePoint(int index)
        {
            CrossSectionDataSet.CrossSectionYZRow row = crossSectionDefinition.GetRow(index);
            if (row != null)
            {
                crossSectionDefinition.YZDataTable.RemoveCrossSectionYZRow(row);
            }
        }
    }
}
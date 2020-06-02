using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    public class ZWProfileMutator : ZWProfileMutatorBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ZWProfileMutator));

        public ZWProfileMutator(CrossSectionDefinitionZW crossSectionDefinition)
        {
            CrossSectionDefinition = crossSectionDefinition;
        }

        public override bool CanDelete
        {
            get
            {
                return true;
            }
        }

        public override bool CanAdd
        {
            get
            {
                return true;
            }
        }

        public override bool FixVertical
        {
            get
            {
                return false;
            }
        }

        public override void MovePoint(int index, double y, double z)
        {
            CrossSectionDefinition.BeginEdit(DelftTools.Hydro.CrossSections.CrossSectionDefinition.DefaultEditAction);
            try
            {
                CrossSectionDataSet.CrossSectionZWRow row = GetRow(index);

                ThrowIfHeightChangeWouldChangeOrdering(row, z);

                row.Z = z;

                CrossSectionDataSet.CrossSectionZWRow lastRow = GetSortedRows().Last();

                if (row != lastRow)
                {
                    double newWidth = Math.Max(0.1, Math.Abs(y) * 2.0); //can't be zero if it's not the last row

                    if (row.StorageWidth > newWidth)
                    {
                        row.StorageWidth = newWidth;
                    }

                    row.Width = newWidth;
                }
            }
            catch (ArgumentException e)
            {
                Log.Error("Attempt to add invalid point to ZW-profile has been ignored.");
            }
            finally
            {
                CrossSectionDefinition.EndEdit();
            }
        }

        public override void AddPoint(double y, double z)
        {
            //don't add if already have a point for this z
            if (!CrossSectionDefinition.ZWDataTable.Any(r => r.Z == z))
            {
                var suggestedStorageWidth = 0.0;
                if (CrossSectionDefinition.ZWDataTable.Count == 1)
                {
                    // Use already defined storage:
                    suggestedStorageWidth = CrossSectionDefinition.ZWDataTable[0].StorageWidth;
                }

                CrossSectionDataSet.CrossSectionZWRow row = CrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(z, Math.Abs(y) * 2, suggestedStorageWidth);

                // Correct storage after add to make use of sorting to find neighbors
                if (CrossSectionDefinition.ZWDataTable.Count > 2)
                {
                    List<CrossSectionDataSet.CrossSectionZWRow> sortedRows = CrossSectionDefinition.ZWDataTable.OrderByDescending(r => r.Z).ToList();
                    int rowIndex = sortedRows.IndexOf(row);

                    double correctedStorageWidthValue;
                    if (rowIndex == 0)
                    {
                        correctedStorageWidthValue = sortedRows[rowIndex + 1].StorageWidth;
                    }
                    else if (rowIndex == CrossSectionDefinition.ZWDataTable.Count)
                    {
                        correctedStorageWidthValue = sortedRows[rowIndex - 1].StorageWidth;
                    }
                    else
                    {
                        CrossSectionDataSet.CrossSectionZWRow leftRow = sortedRows[rowIndex - 1];
                        CrossSectionDataSet.CrossSectionZWRow rightRow = sortedRows[rowIndex + 1];

                        // Linearly interpolate:
                        double z1 = leftRow.Z;
                        double s1 = leftRow.StorageWidth;

                        double z2 = rightRow.Z;
                        double s2 = rightRow.StorageWidth;

                        correctedStorageWidthValue = (((s1 - s2) / (z1 - z2)) * (z - z1)) + s1;
                    }

                    row.StorageWidth = correctedStorageWidthValue;
                }
            }
        }

        public override void DeletePoint(int index)
        {
            CrossSectionDefinition.ZWDataTable.RemoveCrossSectionZWRow(GetRow(index));
        }
    }
}
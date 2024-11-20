using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
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

        public override void MovePoint(int index, double y, double z)
        {
            CrossSectionDefinition.BeginEdit(DelftTools.Hydro.CrossSections.CrossSectionDefinition.DefaultEditAction);
            try
            {
                var row = GetRow(index);

                ThrowIfHeightChangeWouldChangeOrdering(row, z);

                row.Z = z;

                var lastRow = GetSortedRows().Last();

                if (row != lastRow)
                {
                    var newWidth = Math.Max(0.1, Math.Abs(y)*2.0); //can't be zero if it's not the last row

                    if (row.StorageWidth > newWidth)
                    {
                        row.StorageWidth = newWidth;
                    }

                    row.Width = newWidth;
                }
            }
            catch (ArgumentException)
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
                var row = CrossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(z, Math.Abs(y) * 2, suggestedStorageWidth);

                // Correct storage after add to make use of sorting to find neighbors
                if (CrossSectionDefinition.ZWDataTable.Count > 2)
                {
                    var sortedRows =  CrossSectionDefinition.ZWDataTable.OrderByDescending(r => r.Z).ToList();
                    var rowIndex = sortedRows.IndexOf(row);

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
                        var leftRow = sortedRows[rowIndex - 1];
                        var rightRow = sortedRows[rowIndex + 1];

                        // Linearly interpolate:
                        var z1 = leftRow.Z;
                        var s1 = leftRow.StorageWidth;

                        var z2 = rightRow.Z;
                        var s2 = rightRow.StorageWidth;

                        correctedStorageWidthValue = (s1 - s2) / (z1 - z2) * (z - z1) + s1;
                    }

                    row.StorageWidth = correctedStorageWidthValue;
                }
            }
        }

        public override void DeletePoint(int index)
        {
            CrossSectionDefinition.ZWDataTable.RemoveCrossSectionZWRow(GetRow(index));
        }

        public override bool CanDelete
        {
            get { return true; }
        }

        public override bool CanAdd
        {
            get { return true; }
        }

        public override bool FixVertical
        {
            get { return false; }
        }
    }
}

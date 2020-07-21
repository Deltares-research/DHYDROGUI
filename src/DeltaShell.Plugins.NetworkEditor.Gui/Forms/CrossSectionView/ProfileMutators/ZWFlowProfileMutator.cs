using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    public class ZWFlowProfileMutator : ZWProfileMutatorBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ZWFlowProfileMutator));

        public ZWFlowProfileMutator(CrossSectionDefinitionZW crossSectionDefinition)
        {
            CrossSectionDefinition = crossSectionDefinition;
        }

        public override bool CanDelete
        {
            get
            {
                return false;
            }
        }

        public override bool CanAdd
        {
            get
            {
                return false;
            }
        }

        public override bool FixVertical
        {
            get
            {
                return true;
            }
        }

        public override void MovePoint(int index, double y, double z)
        {
            CrossSectionDefinition.BeginEdit(DelftTools.Hydro.CrossSections.CrossSectionDefinition.DefaultEditAction);
            try
            {
                CrossSectionDataSet.CrossSectionZWRow row = GetRow(index);

                ThrowIfHeightChangeWouldChangeOrdering(row, z);

                double deltaStorage = Math.Max(0.0, (row.Width / 2.0) - Math.Abs(y));
                row.Z = z;
                row.StorageWidth = Math.Min(row.Width, deltaStorage * 2.0);
            }
            catch (ArgumentException e)
            {
                Log.Error("Attempt to add invalid point to ZW-flow profile has been ignored.");
            }
            finally
            {
                CrossSectionDefinition.EndEdit();
            }
        }

        public override void AddPoint(double y, double z)
        {
            throw new NotImplementedException("Cannot add point to storage profile, add point to normal profile instead");
        }

        public override void DeletePoint(int index)
        {
            throw new NotImplementedException("Cannot add point to storage profile, add point to normal profile instead");
        }
    }
}
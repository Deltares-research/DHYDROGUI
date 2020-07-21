using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    public class XYZFlowProfileMutator : ICrossSectionProfileMutator
    {
        private readonly CrossSectionDefinitionXYZ crossSectionDefinition;

        public XYZFlowProfileMutator(CrossSectionDefinitionXYZ crossSectionDefinition)
        {
            this.crossSectionDefinition = crossSectionDefinition;
        }

        public bool CanDelete
        {
            get
            {
                return false;
            }
        }

        public bool CanAdd
        {
            get
            {
                return false;
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
                return false;
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
                return true;
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
                CrossSectionDataSet.CrossSectionXYZRow row = crossSectionDefinition.XYZDataTable[index];
                double storage = z - row.Z;
                row.DeltaZStorage = Math.Max(0.0, storage);
            }
            finally
            {
                crossSectionDefinition.EndEdit();
            }
        }

        public void AddPoint(double y, double z)
        {
            throw new NotSupportedException("Cannot add point to storage profile, add point to normal profile instead");
        }

        public void DeletePoint(int index)
        {
            throw new NotSupportedException("Cannot delete point to storage profile, add point to normal profile instead");
        }
    }
}
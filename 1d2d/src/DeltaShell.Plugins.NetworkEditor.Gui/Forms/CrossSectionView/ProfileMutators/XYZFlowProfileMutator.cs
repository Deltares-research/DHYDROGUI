using System;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    public class XYZFlowProfileMutator : ICrossSectionProfileMutator
    {
        private readonly CrossSectionDefinitionXYZ crossSectionDefinition;

        public XYZFlowProfileMutator(CrossSectionDefinitionXYZ crossSectionDefinition)
        {
            this.crossSectionDefinition = crossSectionDefinition;
        }

        public void MovePoint(int index, double y, double z)
        {
            crossSectionDefinition.BeginEdit(CrossSectionDefinition.DefaultEditAction);
            try
            {
                var row = crossSectionDefinition.XYZDataTable[index];
                var storage = z - row.Z;
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

        public bool CanDelete
        {
            get { return false; }
        }

        public bool CanAdd
        {
            get { return false; }
        }

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
            get { return false;  }
        }

        public bool FixHorizontal
        {
            get { return true; }
        }

        public bool FixVertical
        {
            get { return false; }
        }
    }
}
using System;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    public class YZFlowProfileMutator : ICrossSectionProfileMutator
    {
        private CrossSectionDefinitionYZ crossSectionDefinition;

        public YZFlowProfileMutator(CrossSectionDefinitionYZ crossSectionDefinition)
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
                row.DeltaZStorage = Math.Max(0.0, z - row.Z);
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
            throw new NotSupportedException("Cannot delete point from storage profile, add point to normal profile instead");
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
            get { return true; }
        }

        public bool ClipVertical
        {
            get { return false; }
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
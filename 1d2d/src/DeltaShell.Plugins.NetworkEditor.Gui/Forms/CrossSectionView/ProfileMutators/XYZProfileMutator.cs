using System;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    public class XYZProfileMutator : ICrossSectionProfileMutator
    {
        private readonly CrossSectionDefinitionXYZ crossSectionDefinition;

        public XYZProfileMutator(CrossSectionDefinitionXYZ crossSectionDefinition)
        {
            this.crossSectionDefinition = crossSectionDefinition;
        }

        public void MovePoint(int index, double y, double z)
        {
            crossSectionDefinition.BeginEdit(CrossSectionDefinition.DefaultEditAction);
            try
            {
                crossSectionDefinition.Geometry.Coordinates[index].Z = z;
                crossSectionDefinition.Geometry = crossSectionDefinition.Geometry;
            }
            finally
            {
                crossSectionDefinition.EndEdit();
            }
        }

        public void AddPoint(double y, double z)
        {
            throw new NotSupportedException("Cannot add point to XYZ profile");
        }

        public void DeletePoint(int index)
        {
            throw new NotSupportedException("Cannot delete point from XYZ profile");
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
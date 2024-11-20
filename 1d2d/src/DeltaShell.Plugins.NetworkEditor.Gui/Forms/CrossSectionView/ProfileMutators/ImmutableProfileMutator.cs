using System;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    /// <summary>
    /// Profile mutator that can not mutate anything! Used for standard crosssections
    /// </summary>
    public class ImmutableProfileMutator : ICrossSectionProfileMutator
    {
        public void MovePoint(int index, double y, double z)
        {
            throw new NotImplementedException();
        }

        public void AddPoint(double y, double z)
        {
            throw new NotImplementedException();
        }

        public void DeletePoint(int index)
        {
            throw new NotImplementedException();
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
            get { return false; }
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
            get { return false; }
        }

        public bool FixVertical
        {
            get { return false; }
        }
    }
}
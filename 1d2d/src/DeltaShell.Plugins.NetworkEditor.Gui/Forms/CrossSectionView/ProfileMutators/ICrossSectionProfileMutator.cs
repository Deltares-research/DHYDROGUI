namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    public interface ICrossSectionProfileMutator
    {
        void MovePoint(int index, double y, double z);
        void AddPoint(double y, double z);
        void DeletePoint(int index);

        bool CanDelete { get; }
        bool CanAdd { get; }
        bool CanMove { get; }

        bool ClipHorizontal { get; }
        bool ClipVertical { get; }
        bool FixHorizontal { get; }
        bool FixVertical { get; }
    }
}
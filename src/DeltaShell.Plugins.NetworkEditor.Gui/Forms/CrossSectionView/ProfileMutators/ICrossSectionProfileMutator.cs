using System;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    public interface ICrossSectionProfileMutator
    {
        bool CanDelete { get; }
        bool CanAdd { get; }
        bool CanMove { get; }

        bool ClipHorizontal { get; }
        bool ClipVertical { get; }
        bool FixHorizontal { get; }
        bool FixVertical { get; }
        void MovePoint(int index, double y, double z);
        void AddPoint(double y, double z);
        void DeletePoint(int index);
    }
}
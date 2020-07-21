using System;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    public interface ICrossSectionHistoryCapableView
    {
        bool HistoryToolEnabled { get; set; }
    }
}
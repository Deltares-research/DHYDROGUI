using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public interface IDragAndDropStrategy
    {
        bool Validate();
        void Reposition();
        bool FindNewPosition(Canvas canvas, ContentPresenter contentPresenter, double leftOffset, double originalLeft);
    }
}
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public interface IDragAndDropStrategy
    {
        void DragStart(Canvas canvas, ContentPresenter contentPresenter);
        
        bool Validate();
        void Reposition();
        bool FindNewPosition(double horizontalOffset);
    }
}
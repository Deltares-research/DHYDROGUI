using System.Collections.ObjectModel;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public class ConnectionShapeDragAndDropStrategy : CompartmentShapeDragAndDropStrategy
    {
        public ConnectionShapeDragAndDropStrategy(ObservableCollection<IDrawingShape> shapes) : base(shapes)
        {
        }
    }
}
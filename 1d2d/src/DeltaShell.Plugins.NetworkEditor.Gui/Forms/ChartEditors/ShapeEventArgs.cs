using System;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    public delegate void SelectionChangedEventHandler(object sender, ShapeEventArgs e);

    public delegate void ShapeChangedEvendHandler(object sender, ShapeEventArgs e);

    public class ShapeEventArgs : EventArgs
    {
        public IShapeFeature ShapeFeature { get; set; }

        public ShapeEventArgs(IShapeFeature shapeFeature)
        {
            ShapeFeature = shapeFeature;
        }
    }
}
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    public interface IShapeLayerTool
    {
        ShapeModifyTool ShapeModifyTool { get; set; }
        bool IsActive { get; set; }
        bool IsBusy { get; set; }
        void Paint();
        void MouseEvent(ChartMouseEvent kind, MouseEventArgs e, Cursor c);
    }
}
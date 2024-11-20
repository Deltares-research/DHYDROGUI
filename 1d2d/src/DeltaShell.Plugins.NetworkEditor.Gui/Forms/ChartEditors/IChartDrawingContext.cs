using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    public interface IChartDrawingContext
    {
        object Graphics { get; }
        VectorStyle Style { get; set; }
        void Reset();
    }
}
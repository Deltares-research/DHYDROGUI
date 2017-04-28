using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid
{
    /// <summary>
    /// InterpolationType.None should not be supported
    /// InterpolationType.Constant should also not be supported because it makes the simulation unstable
    /// See tools TOOLS-4751
    /// </summary>
    public enum InterpolationTypeBoundaryQh
    {
        Linear = InterpolationType.Linear
    }
}
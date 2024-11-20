using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    /// <summary>
    /// InterpolationType.None should not be supported
    /// used for lateral Q(t)
    ///          boundary Q(t) and H(t)
    /// </summary>
    public enum InterpolationTypeBoundaryTime
    {
        Constant = InterpolationType.Constant,
        Linear = InterpolationType.Linear 
    }
}
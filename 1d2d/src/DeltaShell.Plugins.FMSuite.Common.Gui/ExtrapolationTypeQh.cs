using DelftTools.Functions.Generic;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    /// <summary>
    /// ExtrapolationType.Periodic should not be supported for Qh
    /// TOOLS-4691 comment Jan Noort support only Block
    /// </summary>
    public enum ExtrapolationTypeQh
    {
        Constant = ExtrapolationType.Constant
        // Linear = ExtrapolationType.Linear,
        // None = ExtrapolationType.None
    }
}
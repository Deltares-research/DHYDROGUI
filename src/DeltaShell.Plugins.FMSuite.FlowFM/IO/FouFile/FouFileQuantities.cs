using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    /// <summary>
    /// Defines the supported quantities for the statistical analysis configuration file (*.fou).
    /// </summary>
    public static class FouFileQuantities
    {
        private static readonly string[] quantities1D = { "fb", "vog", "wdog" };
        private static readonly string[] quantities1D2D = { "bs", "eh", "sul", "uxa", "uya", "wd", "wl" };
        private static readonly string[] quantities3D = { "q1", "ux", "uy", "uc" };
        private static readonly string[] unsupportedQuantities = { "cs", "ct", "cn", "ws" };

        /// <summary>
        /// Gets the supported fou file quantities.
        /// </summary>
        public static readonly IReadOnlyList<string> SupportedQuantities = quantities1D.Concat(quantities1D2D)
                                                                          .Concat(quantities3D)
                                                                          .OrderBy(q => q)
                                                                          .ToArray();

        /// <summary>
        /// Returns whether the specified quantity name is a 3D quantity.
        /// </summary>
        public static bool Is3DQuantity(string quantityName)
            => quantities3D.Contains(quantityName);

        /// <summary>
        /// Returns whether the specified quantity name is not supported.
        /// </summary>
        public static bool IsUnsupportedQuantity(string quantityName)
            => unsupportedQuantities.Contains(quantityName) || Regex.IsMatch(quantityName, @"\bc\d+\b");
    }
}
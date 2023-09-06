using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.FMSuite.Common.IO.BackwardCompatibility
{
    /// <summary>
    /// Class that contains the default behaviour for updating legacy properties.
    /// </summary>
    public class DefaultPropertyUpdater : IPropertyUpdater
    {
        /// <inheritdoc/>
        /// <remarks>
        /// The default behaviour for updating legacy properties is simply updating their name to the up to date version.
        /// </remarks>
        public void UpdateProperty(string oldPropertyKey,
                                   string newPropertyKey,
                                   IniSection section,
                                   ILogHandler logHandler)
        {
            Ensure.NotNull(oldPropertyKey, nameof(oldPropertyKey));
            Ensure.NotNull(newPropertyKey, nameof(newPropertyKey));
            Ensure.NotNull(section, nameof(section));
            Ensure.NotNull(logHandler, nameof(logHandler));

            logHandler.ReportWarningFormat(Resources.DelftIniBackwardsCompatibilityHelper_GetUpdatedKey_Backwards_Compatibility____0___has_been_updated_to___1__,
                                           oldPropertyKey,
                                           newPropertyKey);

            section.RenameProperties(oldPropertyKey, newPropertyKey);
        }
    }
}
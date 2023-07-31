using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
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
        public void UpdateProperty(DelftIniProperty legacyProperty,
                                   string newPropertyName,
                                   DelftIniCategory legacyPropertyCategory,
                                   ILogHandler logHandler)
        {
            Ensure.NotNull(legacyProperty, nameof(legacyProperty));
            Ensure.NotNull(newPropertyName, nameof(newPropertyName));
            Ensure.NotNull(logHandler, nameof(logHandler));

            logHandler.ReportWarningFormat(Resources.DelftIniBackwardsCompatibilityHelper_GetUpdatedName_Backwards_Compatibility____0___has_been_updated_to___1__,
                                           legacyProperty.Name,
                                           newPropertyName);

            legacyProperty.Name = newPropertyName;
        }
    }
}
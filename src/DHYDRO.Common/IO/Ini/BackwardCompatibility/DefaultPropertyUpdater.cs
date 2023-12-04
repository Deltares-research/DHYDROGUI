using DHYDRO.Common.Guards;
using DHYDRO.Common.Logging;
using DHYDRO.Common.Properties;

namespace DHYDRO.Common.IO.Ini.BackwardCompatibility
{
    /// <summary>
    /// Class that contains the default behaviour for updating legacy properties.
    /// </summary>
    public sealed class DefaultPropertyUpdater : IPropertyUpdater
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

            logHandler.ReportWarningFormat(Resources.Backwards_Compatibility_0_has_been_updated_to_1_,
                                           oldPropertyKey,
                                           newPropertyKey);

            section.RenameProperties(oldPropertyKey, newPropertyKey);
        }
    }
}
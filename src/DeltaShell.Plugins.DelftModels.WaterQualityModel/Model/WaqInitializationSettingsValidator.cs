using System.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    /// <summary>
    /// Validates the WaqInitializationSettings
    /// </summary>
    public static class WaqInitializationSettingsValidator
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaqInitializationSettingsValidator));

        /// <summary>
        /// Validates the specified <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">The waq initialization settings.</param>
        public static void Validate(WaqInitializationSettings settings)
        {
            ValidateGridFile(settings.GridFile);
        }

        private static void ValidateGridFile(string gridFilePath)
        {
            try
            {
                if (!NetCdfFileConventionChecker.HasSupportedConvention(gridFilePath))
                {
                    log.Warn(string.Format(
                                 Resources.WaqInitializationSettingsValidator_GridFile_does_not_meet_supported_UGRID_1_0,
                                 Path.GetFileName(gridFilePath)));
                }
            }
            catch (FileNotFoundException)
            {
                log.Error(string.Format(Resources.WaqInitializationSettingsValidator_Grid_file_was_not_found,
                                        gridFilePath));
            }
        }
    }
}
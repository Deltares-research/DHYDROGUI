using System.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Model
{
    /// <summary>
    /// Verifies the data specified in the <see cref="WaqInitializationSettings"/>.
    /// </summary>
    public static class WaqInitializationDataVerifier
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(WaqInitializationDataVerifier));

        /// <summary>
        /// Verifies the data specified in the <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings">The waq initialization settings.</param>
        public static void Verify(WaqInitializationSettings settings)
        {
            VerifyGridFile(settings.GridFile);
        }

        private static void VerifyGridFile(string gridFilePath)
        {
            try
            {
                if (!NetCdfFileConventionChecker.HasSupportedConvention(gridFilePath))
                {
                    log.Warn(string.Format(
                                 Resources.WaqInitializationDataVerifier_GridFile_does_not_meet_supported_UGRID_1_0,
                                 Path.GetFileName(gridFilePath)));
                }
            }
            catch (FileNotFoundException)
            {
                log.Error(string.Format(Resources.WaqInitializationDataVerifier_Grid_file_was_not_found,
                                        gridFilePath));
            }
        }
    }
}
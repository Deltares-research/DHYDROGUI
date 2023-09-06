using System;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders
{
    /// <summary>
    /// This <see cref="IniSectionFactory"/> provides a method to
    /// create a <see cref="IniSection"/>.
    /// </summary>
    public static class IniSectionFactory
    {
        /// <summary>
        /// Creates the boundary <see cref="IniSection"/> with the specified property values.
        /// </summary>
        /// <param name="quantity">The quantity.</param>
        /// <param name="locationFilePath">The location file path.</param>
        /// <param name="forcingFilePath">The forcing file path.</param>
        /// <param name="thatcherHarlemanTimeLag">The Thatcher-Harleman time lag.</param>
        /// <returns>
        /// The created <see cref="IniSection"/>.
        /// </returns>
        public static IniSection CreateBoundaryBlock(string quantity, 
                                                     string locationFilePath,
                                                     string forcingFilePath, 
                                                     TimeSpan thatcherHarlemanTimeLag)
        {
            var block = new IniSection(BndExtForceFileConstants.BoundaryBlockKey);
            if (quantity != null)
            {
                block.AddProperty(BndExtForceFileConstants.QuantityKey, quantity);
            }

            if (locationFilePath != null)
            {
                block.AddProperty(BndExtForceFileConstants.LocationFileKey, locationFilePath);
            }

            if (forcingFilePath != null)
            {
                block.AddProperty(BndExtForceFileConstants.ForcingFileKey, forcingFilePath);
            }

            if (thatcherHarlemanTimeLag != TimeSpan.Zero)
            {
                block.AddProperty(BndExtForceFileConstants.ThatcherHarlemanTimeLagKey, thatcherHarlemanTimeLag.TotalSeconds);
            }

            return block;
        }
    }
}
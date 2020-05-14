using System;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders
{
    public static class DelftIniCategoryFactory
    {
        private const double openBoundaryTolerance = 0.5;

        public static DelftIniCategory CreateBoundaryBlock(string quantity, string locationFilePath,
                                                            string forcingFilePath, TimeSpan thatcherHarlemanTimeLag,
                                                            bool isEmbankment = false)
        {
            var block = new DelftIniCategory(BndExtForceFileConstants.BoundaryBlockKey);
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

            if (isEmbankment)
            {
                block.AddProperty(BndExtForceFileConstants.OpenBoundaryToleranceKey, openBoundaryTolerance);
            }

            return block;
        }
    }
}

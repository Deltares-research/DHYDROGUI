using System;
using System.Linq;
using DelftTools.Hydro.Properties;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using log4net;

namespace DelftTools.Hydro
{
    public static class HydroNetworkExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroNetworkExtensions));
        public static void EnsureCompositeBranchStructureNamesAreUnique(this HydroNetwork network, bool enableLogging = true)
        {
            var previousNames = network.CompositeBranchStructures.Select(cbs => cbs.Name).ToList();
            if(previousNames.HasUniqueValues()) return;

            NamingHelper.MakeNamesUnique(network.CompositeBranchStructures);

            if (!enableLogging) return;

            var newNames = network.CompositeBranchStructures.Select(cbs => cbs.Name).ToList();
            var logMessage = Resources.HydroNetworkExtensions_EnsureCompositeBranchStructureNamesAreUnique_Composite_Structure_names_must_be_unique__the_following_Composite_Structures_have_been_renamed_;

            for (var i = 0; i < Math.Min(previousNames.Count, newNames.Count); i++)
            {
                var previousName = previousNames[i];
                var newName = newNames[i];

                if(previousName != newName)
                    logMessage += string.Format("{0}{1} has been renamed to {2}", Environment.NewLine, previousName, newName);
            }

            Log.Info(logMessage);
        }
    }
}

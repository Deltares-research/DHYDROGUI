using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DelftTools.Hydro
{
    public static class HydroNetworkExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroNetworkExtensions));

        /// <summary>
        /// Ensure that all <see cref="ICompositeBranchStructure"/> have a unique name
        /// </summary>
        /// <param name="network">Network to check</param>
        /// <param name="enableLogging">Add log message for changed <see cref="ICompositeBranchStructure"/> names</param>
        public static void MakeNamesUnique<T>(this IHydroNetwork network, bool enableLogging = true) where T : class, IBranchFeature
        {
            var networkNotifyPropertyChanging = network as INotifyPropertyChanging;
            var networkNotifyPropertyChanged = network as INotifyPropertyChanged;

            var shouldLog = enableLogging &&
                            networkNotifyPropertyChanged != null &&
                            networkNotifyPropertyChanging != null;

            var previousName = string.Empty;
            var messagesList = new List<string>();

            // function to catch previous name
            PropertyChangingEventHandler onNetworkPropertyChanging = (sender, e) =>
            {
                previousName = (sender as T)?.Name;
            };

            // function to add change log messages
            PropertyChangedEventHandler onNetworkPropertyChanged = (sender, e) =>
            {
                var compositeBranchStructure = sender as T;
                if (compositeBranchStructure == null) return;

                messagesList.Add(string.Format("{0} has been renamed to {1}", previousName, compositeBranchStructure.Name));
            };

            if (shouldLog)
            {
                networkNotifyPropertyChanging.PropertyChanging += onNetworkPropertyChanging;
                networkNotifyPropertyChanged.PropertyChanged += onNetworkPropertyChanged;
            }

            try
            {
                NamingHelper.MakeNamesUnique(network.BranchFeatures.OfType<T>());
            }
            finally
            {
                if (shouldLog)
                {
                    networkNotifyPropertyChanging.PropertyChanging -= onNetworkPropertyChanging;
                    networkNotifyPropertyChanged.PropertyChanged -= onNetworkPropertyChanged;

                    var logMessage = Resources.HydroNetworkExtensions_EnsureCompositeBranchStructureNamesAreUnique_Composite_Structure_names_must_be_unique__the_following_Composite_Structures_have_been_renamed_;
                    Log.Info($"{logMessage}{Environment.NewLine}" +
                             $"{string.Join(Environment.NewLine, messagesList)}");
                }
            }
        }
    }
}

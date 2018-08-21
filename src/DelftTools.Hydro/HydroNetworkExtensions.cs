using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Networks;
using log4net;
using SharpMap.CoordinateSystems.Transformations;

namespace DelftTools.Hydro
{
    public static class HydroNetworkExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroNetworkExtensions));

        public static void UpdateGeodeticDistancesOfChannels(this IHydroNetwork network)
        {
            if (network.CoordinateSystem == null)
            {
                network.Channels.ForEach(c => c.GeodeticLength = 0);
                return;
            }
            network.Channels.ForEach(c => c.GeodeticLength = GeodeticDistance.Length(network.CoordinateSystem, c.Geometry));
        }


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
                var branchFeature = sender as T;
                if (branchFeature == null) return;

                // we don't want to log messages for 'hidden' composite structures as this is confusing for the user
                var compositeBranchStructure = sender as ICompositeBranchStructure;
                if (compositeBranchStructure != null && compositeBranchStructure.Structures.Count < 2) return;

                messagesList.Add(string.Format("{0} has been renamed to {1}", previousName, branchFeature.Name));
            };

            if (shouldLog)
            {
                networkNotifyPropertyChanging.PropertyChanging += onNetworkPropertyChanging;
                networkNotifyPropertyChanged.PropertyChanged += onNetworkPropertyChanged;
            }

            try
            {
                var numberOfBranchFeatures = network.BranchFeatures.OfType<T>().Count();
                if (numberOfBranchFeatures > 1) NamingHelper.MakeNamesUnique(network.BranchFeatures.OfType<T>());
            }
            finally
            {
                if (shouldLog)
                {
                    networkNotifyPropertyChanging.PropertyChanging -= onNetworkPropertyChanging;
                    networkNotifyPropertyChanged.PropertyChanged -= onNetworkPropertyChanged;

                    if (messagesList.Any())
                    {
                        var logMessage = Resources.HydroNetworkExtensions_MakeNamesUnique_Branch_feature_names_must_be_unique__the_following_Branch_features_have_been_renamed_;
                        Log.Info($"{logMessage}{Environment.NewLine}" + $"{string.Join(Environment.NewLine, messagesList)}");
                    }
                }
            }
        }
    }
}

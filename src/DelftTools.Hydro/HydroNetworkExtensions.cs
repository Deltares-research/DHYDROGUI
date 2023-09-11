using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.Extensions;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.SewerFeatures;
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

        public static IManhole GetManhole(this IHydroNetwork hydroNetwork, ICompartment compartment)
        {
            return hydroNetwork.Manholes.FirstOrDefault(m => compartment.ParentManholeName != null // search for manhole via parent manhole name of this compartment
                                                             && m.Name.Equals(compartment.ParentManholeName, StringComparison.InvariantCultureIgnoreCase)) ??
                   hydroNetwork.Manholes.FirstOrDefault(m => m.Compartments.Contains(compartment)) ?? // search for manhole via this compartment
                   hydroNetwork.Manholes.FirstOrDefault(m => m.Compartments.Any(c => compartment.Name != null // search for manhole via compartments in manholes using this compartment name
                                                                                     && c.Name.Equals(compartment.Name, StringComparison.InvariantCultureIgnoreCase)));
        }

        public static void FindAndConnectManholesInNetwork(this IHydroNetwork hydroNetwork, ISewerConnection sewerConnection)
        {
            var connection = sewerConnection as SewerConnection;
            if (connection == null) return;
            if (!string.IsNullOrWhiteSpace(connection.SourceCompartmentName))
            {
                var i = 0;
                while (connection.Source == null && i < 5)
                {
                    if (connection.Source == null)
                    {
                        lock (hydroNetwork.Nodes)
                        {
                            var manholeContainingCompartment = hydroNetwork.Manholes.FirstOrDefault(m =>
                                                                                                        m.Compartments.Any(c =>
                                                                                                                               c.Name.Equals(connection.SourceCompartmentName,
                                                                                                                                             StringComparison.InvariantCultureIgnoreCase)));
                            connection.Source = manholeContainingCompartment;
                        }
                    }
                    Thread.Sleep(50);
                    i++;
                }
            }

            if (!string.IsNullOrWhiteSpace(connection.TargetCompartmentName))
            {
                var i = 0;
                while (connection.Target == null && i < 5)
                {
                    if (connection.Target == null)
                    {
                        lock (hydroNetwork.Nodes)
                        {
                            var manholeContainingCompartment = hydroNetwork.Manholes.FirstOrDefault(m =>
                                                                                                        m.Compartments.Any(c =>
                                                                                                                               c.Name.Equals(connection.TargetCompartmentName,
                                                                                                                                             StringComparison.InvariantCultureIgnoreCase)));
                            connection.Target = manholeContainingCompartment;
                        }
                    }

                    Thread.Sleep(50);
                    i++;
                }
            }

        }

        public static void UpdateGeodeticDistancesOfChannels(this IHydroNetwork network)
        {
            if (network.CoordinateSystem == null)
            {
                network.Channels.ForEach(c => c.GeodeticLength = double.NaN);
                return;
            }

            var geodeticDistance = new GeodeticDistance(network.CoordinateSystem);

            network.Channels.ForEach(c =>
            {
                var distance = 0.0;

                for (int index = 1; index < c.Geometry.Coordinates.Length; ++index)
                    distance += geodeticDistance.Distance(c.Geometry.Coordinates[index - 1],
                        c.Geometry.Coordinates[index]);

                c.GeodeticLength = distance;
            });
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

        public static IEnumerable<ICrossSectionDefinition> GetNetworkCrossSectionDefinitions(this IHydroNetwork network)
        {
            return network.CrossSections.Select(c => c.GetCrossSectionDefinition())
                .Concat(network.BridgeCrossSectionDefinitions())
                .Concat(network.CulvertCrossSectionDefinitions())
                .Concat(network.PipeCrossSectionDefinitions());
        }

        private static IEnumerable<ICrossSectionDefinition> BridgeCrossSectionDefinitions(this IHydroNetwork network)
        {
            return network.Bridges.Where(b => b.CrossSectionDefinition != null).Select(b => b.CrossSectionDefinition);
        }

        private static IEnumerable<ICrossSectionDefinition> CulvertCrossSectionDefinitions(this IHydroNetwork network)
        {
            return network.Culverts.Where(c => c.CrossSectionDefinition != null).Select(c => c.CrossSectionDefinition);
        }

        private static IEnumerable<ICrossSectionDefinition> PipeCrossSectionDefinitions(this IHydroNetwork network)
        {
            return network.Pipes.Where(p => p.CrossSection?.Definition != null).
                           Select(p => p.CrossSection?.Definition.GetBaseDefinition());
        }

        public static bool ContainsAnyCrossSectionDefinitions(this IHydroNetwork network)
        {
            return network.GetNetworkCrossSectionDefinitions().Any();
        }

        public static IDictionary<ICrossSectionDefinition, IEnumerable<IChannel>> GetChannelsPerCrossSectionDefinitionLookup(this IHydroNetwork network)
        {
            var channelsPerCrossSectionDefinitionLookup = new Dictionary<ICrossSectionDefinition, IEnumerable<IChannel>>();

            foreach (var crossSection in network.CrossSections)
            {
                var crossSectionDefinition = crossSection.GetCrossSectionDefinition();

                if (!channelsPerCrossSectionDefinitionLookup.ContainsKey(crossSectionDefinition))
                {
                    channelsPerCrossSectionDefinitionLookup.Add(crossSectionDefinition, new List<IChannel>());
                }

                ((List<IChannel>) channelsPerCrossSectionDefinitionLookup[crossSectionDefinition]).Add((IChannel) crossSection.Branch);
            }

            return channelsPerCrossSectionDefinitionLookup;
        }
    }
}

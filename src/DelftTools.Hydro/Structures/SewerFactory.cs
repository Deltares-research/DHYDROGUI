using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.Structures
{
    public static class SewerFactory
    {
        public static void AddDefaultPipeToNetwork(IPipe pipe, INetwork network)
        {
            var hydroNetwork = network as HydroNetwork;
            if (hydroNetwork == null) return;
            pipe.Network = network;
            AddDefaultPipeProfileToNetwork(hydroNetwork);
            SetPipeProperties(pipe, hydroNetwork);

            lock (network.Branches)
                network.Branches.Add(pipe);
            BranchOrderHelper.SetOrderForBranch(network, pipe);
        }

        private static void AddDefaultPipeProfileToNetwork(HydroNetwork hydroNetwork)
        {
            ICrossSectionDefinition defaultPipeProfile = GetDefaultPipeProfile(hydroNetwork);
            if (!defaultPipeProfile.Sections.Any())
            {
                var sewerCrossSectionType = hydroNetwork.CrossSectionSectionTypes.FirstOrDefault(csst => csst.Name.Equals(RoughnessDataSet.SewerSectionTypeName));
                if (sewerCrossSectionType == null) return;
                defaultPipeProfile.AddSection(sewerCrossSectionType, defaultPipeProfile.FlowWidth());
            }
        }

        public static void SetPipeProperties(this IPipe pipe, HydroNetwork hydroNetwork)
        {
            SetPhysicalProperties(pipe, hydroNetwork);
            SetPipeDefaultValues(pipe);
        }

        private static void SetPhysicalProperties(ISewerConnection sewerConnection, HydroNetwork hydroNetwork)
        {
            sewerConnection.Length = sewerConnection.Geometry.Length;
            SetSource(sewerConnection, hydroNetwork);
            SetTarget(sewerConnection, hydroNetwork);
        }

        private static void SetTarget(IBranch branch, HydroNetwork hydroNetwork)
        {
            if (branch.Target != null) return;
            branch.Target = GetExistingOrNewManholeFromNetwork(hydroNetwork, branch.Geometry.Coordinates.Last());
        }

        private static void SetSource(IBranch branch, HydroNetwork hydroNetwork)
        {
            if (branch.Source != null) return;
            branch.Source = GetExistingOrNewManholeFromNetwork(hydroNetwork, branch.Geometry.Coordinates.First());
        }

        private static void SetPipeDefaultValues(IPipe pipe)
        {
            SetSewerConnectionDefaultProperties(pipe);
            pipe.Material = SewerProfileMapping.SewerProfileMaterial.Concrete;
        }

        private static void SetSewerConnectionDefaultProperties(ISewerConnection sewerConnection)
        {
            sewerConnection.LevelSource = -2.0;
            sewerConnection.LevelTarget = -2.0;
            sewerConnection.WaterType = SewerConnectionWaterType.Combined;

            var sewerConnectionCrossSection = CrossSection.CreateDefault(CrossSectionType.Standard, sewerConnection, sewerConnection.Length / 2);
            if (sewerConnection.Network is IHydroNetwork hydroNetwork)
            {
                sewerConnectionCrossSection.Name = NamingHelper.GetUniqueName("SewerProfile_{0}", hydroNetwork.CrossSections, typeof(ICrossSection), true);
                var crossSectionDefinition = sewerConnection is IPipe ? GetDefaultPipeProfile(hydroNetwork) : GetDefaultPumpSewerStructureProfile(hydroNetwork);
                sewerConnectionCrossSection.UseSharedDefinition(crossSectionDefinition);
            }
            sewerConnection.CrossSection = sewerConnectionCrossSection;
        }

        private static INode GetExistingOrNewManholeFromNetwork(IHydroNetwork network, Coordinate coordinate)
        {
            var existingManhole = network.Manholes.FirstOrDefault(n => n.Geometry.Coordinate.X.IsEqualTo(coordinate.X) && n.Geometry.Coordinate.Y.IsEqualTo(coordinate.Y));
            return existingManhole ?? network.HydroNodes.FirstOrDefault(n => n.Geometry.Coordinate.X.IsEqualTo(coordinate.X) && n.Geometry.Coordinate.Y.IsEqualTo(coordinate.Y)) ?? CreateDefaultManholeAndAddToNetwork(network, coordinate);
        }

        public static INode CreateDefaultManholeAndAddToNetwork(IHydroNetwork network, Coordinate coordinate)
        {
            var uniqueName = NetworkHelper.GetUniqueName("Manhole{0:D3}", network.Manholes, "Manhole");
            var newManhole = new Manhole(uniqueName) {Geometry = new Point(coordinate)};

            var uniqueCompartmentName = NetworkHelper.GetUniqueName("Compartment{0:D3}",
                network.Manholes.SelectMany(m => m.Compartments), "Compartment");
            var newCompartment = new Compartment(uniqueCompartmentName);
            lock (newManhole.Compartments)
            {
                newManhole.Compartments.Add(newCompartment);
            }

            lock (network.Nodes)
            {
                network.Nodes.Add(newManhole);
            }
            return newManhole;
        }

        public static Compartment CreateNewCompartmentAndAddToManhole(IHydroNetwork network, Manhole parentManhole, string compartmentName = null)
        {
            var name = string.IsNullOrWhiteSpace(compartmentName) 
                ? GetUniqueCompartmentName(network)
                : compartmentName;

            var newCompartment = new Compartment
            {
                Name = name,
                ParentManhole = parentManhole,
                ManholeWidth = 1,
                BottomLevel = 0,
                SurfaceLevel = 1
            };
            lock (parentManhole.Compartments)
            {
                parentManhole.Compartments.Add(newCompartment);
            }
            return newCompartment;
        }

        public static ISewerConnection CreateNewInternalConnection(IManhole manhole)
        {
            var connection = new SewerConnection
            {
                Source = manhole,
                Target = manhole
            };

            return connection;
        }

        public static ISewerConnection CreateWeirConnection(IManhole manhole)
        {
            ISewerConnection sewerConnection = CreateNewInternalConnection(manhole);
            sewerConnection.AddStructureToBranch(new Weir());
            return sewerConnection;
        }

        public static ISewerConnection CreatePumpConnection(IManhole manhole)
        {
            ISewerConnection sewerConnection = CreateNewInternalConnection(manhole);
            sewerConnection.AddStructureToBranch(CreateNewPump());
            return sewerConnection;
        }

        public static ISewerConnection CreateOrificeConnection(IManhole manhole)
        {
            ISewerConnection sewerConnection = CreateNewInternalConnection(manhole);
            sewerConnection.AddStructureToBranch(new Orifice());
            return sewerConnection;
        }

        private static Pump CreateNewPump()
        {
            return new Pump
            {
                StartSuction = 0.5,
                StopSuction = -0.5,
                StartDelivery = 0.5,
                StopDelivery = -0.5,
            };
        }

        private static string GetUniqueCompartmentName(IHydroNetwork network)
        {
            var compartmentList = network.Manholes.SelectMany(m => m.Compartments); 
            return NetworkHelper.GetUniqueName("Compartment{0:D2}", compartmentList, "Compartment");
        }

        public static void AddDefaultSewerConnectionToNetwork(SewerConnection sewerConnection, INetwork network)
        {
            var hydroNetwork = network as HydroNetwork;
            if (hydroNetwork == null) return;
            sewerConnection.Network = network;

            SetSewerConnectionProperties(sewerConnection, hydroNetwork);

            lock (network.Branches)
                network.Branches.Add(sewerConnection);
            BranchOrderHelper.SetOrderForBranch(network, sewerConnection);
        }
        
        /// <summary>
        /// Gets or creates a default <see cref="ICrossSectionDefinition"/> for pipes.
        /// Newly created definitions are added to the <see cref="IHydroNetwork.SharedCrossSectionDefinitions"/>.
        /// </summary>
        /// <param name="network"> The hydro network. </param>
        /// <returns> The existing or created <see cref="ICrossSectionDefinition"/>.</returns>
        private static ICrossSectionDefinition GetDefaultPipeProfile(IHydroNetwork network)
        {
            return GetOrCreateCrossSectionDefinition(network,
                                                     SewerCrossSectionDefinitionFactory.DefaultPipeProfileName,
                                                     SewerCrossSectionDefinitionFactory.CreateDefaultPipeProfile);
        }

        /// <summary>
        /// Gets or creates a default <see cref="ICrossSectionDefinition"/> for pump sewer structures.
        /// Newly created definitions are added to the <see cref="IHydroNetwork.SharedCrossSectionDefinitions"/>.
        /// </summary>
        /// <param name="network"> The hydro network. </param>
        /// <returns> The existing or created <see cref="ICrossSectionDefinition"/>.</returns>
        public static ICrossSectionDefinition GetDefaultPumpSewerStructureProfile(IHydroNetwork network)
        {
            return GetOrCreateCrossSectionDefinition(network,
                                                     SewerCrossSectionDefinitionFactory.DefaultPumpSewerStructureProfileName,
                                                     SewerCrossSectionDefinitionFactory.CreateDefaultPumpSewerStructureProfile);
        }

        /// <summary>
        /// Gets or creates a default <see cref="ICrossSectionDefinition"/> for weir sewer structures.
        /// Newly created definitions are added to the <see cref="IHydroNetwork.SharedCrossSectionDefinitions"/>.
        /// </summary>
        /// <param name="network"> The hydro network. </param>
        /// <returns> The existing or created <see cref="ICrossSectionDefinition"/>.</returns>
        public static ICrossSectionDefinition GetDefaultWeirSewerStructureProfile(IHydroNetwork network)
        {
            return GetOrCreateCrossSectionDefinition(network,
                                                     SewerCrossSectionDefinitionFactory.DefaultWeirSewerStructureProfileName,
                                                     SewerCrossSectionDefinitionFactory.CreateDefaultWeirSewerStructureProfile);
        }

        private static ICrossSectionDefinition GetOrCreateCrossSectionDefinition(IHydroNetwork network, string name, Func<ICrossSectionDefinition> createDefinitionFunc)
        {
            ICrossSectionDefinition crossSectionDefinition = network.SharedCrossSectionDefinitions.FirstOrDefault(d => d.Name == name);
            if (crossSectionDefinition != null)
            {
                return crossSectionDefinition;
            }

            crossSectionDefinition = createDefinitionFunc();
            network.SharedCrossSectionDefinitions.Add(crossSectionDefinition);

            return crossSectionDefinition;
        }

        private static void SetSewerConnectionProperties(SewerConnection sewerConnection, HydroNetwork hydroNetwork)
        {
            SetPhysicalProperties(sewerConnection, hydroNetwork);
            SetSewerConnectionDefaultProperties(sewerConnection);
        }
    }
}
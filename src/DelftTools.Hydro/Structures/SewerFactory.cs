using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
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
        private const string DefaultProfileDefinitionName = "Default Sewer Profile";
        private const string DefaultSewerConnectionProfileDefinitionName = "Default Sewer Connection Profile";
        private static readonly CrossSectionDefinitionStandard DefaultSewerProfile = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle { Diameter = 0.4 })
        {
            Name = DefaultProfileDefinitionName
        };

        private static readonly CrossSectionDefinitionStandard DefaultSewerConnectionProfile = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle { Diameter = 0.1 })
        {
            Name = DefaultSewerConnectionProfileDefinitionName
        };

        private static readonly Dictionary<Type, Func<ISewerConnection, ISewerConnection>> SewerConnectionStructureCreators = new Dictionary<Type, Func<ISewerConnection, ISewerConnection>>
        {
            { typeof(Orifice), CreateOrificeConnection},
            { typeof(Pump), CreatePumpConnection },
            { typeof(Weir), CreateWeirConnection }
        };

        public static void AddDefaultPipeToNetwork(IPipe pipe, INetwork network)
        {
            var hydroNetwork = network as HydroNetwork;
            if (hydroNetwork == null) return;
            pipe.Network = network;
            AddDefaultSewerProfileToNetwork(hydroNetwork);
            SetPipeProperties(pipe, hydroNetwork);

            lock (network.Branches)
                network.Branches.Add(pipe);
            BranchOrderHelper.SetOrderForBranch(network, pipe);
        }

        private static void AddDefaultSewerProfileToNetwork(HydroNetwork hydroNetwork)
        {
            if (!DefaultSewerProfile.Sections.Any())
            {
                var sewerCrossSectionType = hydroNetwork.CrossSectionSectionTypes.FirstOrDefault(csst => csst.Name.Equals(RoughnessDataSet.SewerSectionTypeName));
                if (sewerCrossSectionType == null) return;
                DefaultSewerProfile.AddSection(sewerCrossSectionType, DefaultSewerProfile.FlowWidth());
            }

            var crossSectionDefinitionAlreadyPresentInNetwork = hydroNetwork.SharedCrossSectionDefinitions.Any(d => d.Name == DefaultProfileDefinitionName);
            if (!crossSectionDefinitionAlreadyPresentInNetwork)
            {
                hydroNetwork.SharedCrossSectionDefinitions.Add(DefaultSewerProfile);
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
                sewerConnectionCrossSection.Name = NamingHelper.GetUniqueName("SewerProfile_{0}", hydroNetwork.CrossSections, typeof(ICrossSection), true);
            sewerConnectionCrossSection.UseSharedDefinition(sewerConnection is IPipe ? DefaultSewerProfile : DefaultSewerConnectionProfile);
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

        public static ISewerConnection CreateConnectionWithStructure<T>(IManhole manhole)
        {
            if (!SewerConnectionStructureCreators.ContainsKey(typeof(T))) return null;

            var connection = CreateNewInternalConnection(manhole);
            var connectionWithStructure = SewerConnectionStructureCreators[typeof(T)]?.Invoke(connection);

            return connectionWithStructure;
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

        private static ISewerConnection CreateWeirConnection(ISewerConnection sewerConnection)
        {
            sewerConnection.AddStructureToBranch(CreateNewWeir());
            return sewerConnection;
        }

        private static ISewerConnection CreatePumpConnection(ISewerConnection sewerConnection)
        {
            sewerConnection.AddStructureToBranch(CreateNewPump());
            return sewerConnection;
        }

        private static ISewerConnection CreateOrificeConnection(ISewerConnection sewerConnection)
        {
            sewerConnection.AddStructureToBranch(CreateNewOrifice());
            return sewerConnection;
        }

        private static Weir CreateNewWeir()
        {
            return new Weir();
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

        private static Orifice CreateNewOrifice()
        {
            return new Orifice
            {
                
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

            if (hydroNetwork.SharedCrossSectionDefinitions.All(d => d.Name != DefaultSewerConnectionProfileDefinitionName))
            {
                hydroNetwork.SharedCrossSectionDefinitions.Add(DefaultSewerConnectionProfile);
            }

            SetSewerConnectionProperties(sewerConnection, hydroNetwork);

            lock (network.Branches)
                network.Branches.Add(sewerConnection);
            BranchOrderHelper.SetOrderForBranch(network, sewerConnection);
        }

        private static void SetSewerConnectionProperties(SewerConnection sewerConnection, HydroNetwork hydroNetwork)
        {
            SetPhysicalProperties(sewerConnection, hydroNetwork);
            SetSewerConnectionDefaultProperties(sewerConnection);
        }
    }
}
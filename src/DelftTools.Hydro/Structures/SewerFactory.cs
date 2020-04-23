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
        private static readonly CrossSectionDefinitionStandard DefaultSewerProfile = new CrossSectionDefinitionStandard(new CrossSectionStandardShapeCircle { Diameter = 0.4 })
        {
            Name = DefaultProfileDefinitionName
        };
        private static readonly ICrossSection DefaultSewerCrossSection = new CrossSection(DefaultSewerProfile);

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
            SetPipePhysicalProperties(pipe, hydroNetwork);
            SetPipeDefaultValues(pipe);
        }

        private static void SetPipePhysicalProperties(IPipe pipe, HydroNetwork hydroNetwork)
        {
            pipe.Length = pipe.Geometry.Length;
            SetSource(pipe, hydroNetwork);
            SetTarget(pipe, hydroNetwork);
        }

        private static void SetTarget(IPipe pipe, HydroNetwork hydroNetwork)
        {
            if (pipe.Target != null) return;
            pipe.Target = GetExistingOrNewManholeFromNetwork(hydroNetwork, pipe.Geometry.Coordinates.Last());
        }

        private static void SetSource(IPipe pipe, HydroNetwork hydroNetwork)
        {
            if (pipe.Source != null) return;
            pipe.Source = GetExistingOrNewManholeFromNetwork(hydroNetwork, pipe.Geometry.Coordinates.First());
        }

        private static void SetPipeDefaultValues(IPipe pipe)
        {
            pipe.LevelSource = -2.0;
            pipe.LevelTarget = -2.0;
            pipe.WaterType = SewerConnectionWaterType.Combined;
            pipe.Material = SewerProfileMapping.SewerProfileMaterial.Concrete;
            var pipeCrossSection = CrossSection.CreateDefault(CrossSectionType.Standard, pipe, pipe.Length / 2);
            if(pipe.Network is IHydroNetwork hydroNetwork)
                pipeCrossSection.Name = NamingHelper.GetUniqueName("SewerProfile_{0}", hydroNetwork.CrossSections, typeof(ICrossSection), true);
            pipeCrossSection.UseSharedDefinition(DefaultSewerProfile);
            pipe.CrossSection = pipeCrossSection;
            
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
            var newCompartment = new Compartment(uniqueCompartmentName)
            {
                SurfaceLevel = 0.0,
                BottomLevel = -2.0,
                FloodableArea = 100.0,
                ManholeLength = 0.64,
                ManholeWidth = 0.64
            };
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
                Target = manhole,
                /*Geometry = new LineString(new[]
                {
                    manhole.Geometry.Coordinate,
                    manhole.Geometry.Coordinate
                })*/
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
    }
}
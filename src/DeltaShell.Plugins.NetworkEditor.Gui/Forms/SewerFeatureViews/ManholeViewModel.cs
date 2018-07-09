using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class ManholeViewModel
    {
        private IHydroNetwork network;
        private Manhole manhole;

        public ManholeViewModel()
        {
            AddCompartmentCommand = new RelayCommand(AddCompartment, CanAddCompartment);
            AddPumpCommand = new RelayCommand(AddPump, CanAddPump);
            AddOrificeCommand = new RelayCommand(AddOrifice, CanAddOrifice);
            AddWeirCommand = new RelayCommand(AddWeir, CanAddWeir);
            RemoveCompartmentCommand = new RelayCommand(RemoveItem, CanRemoveItem);
        }

        public Manhole Manhole
        {
            get { return manhole; }
            set
            {
                if (value == null)
                {
                    network = null;
                    UnsubscribeEvents();
                    return;
                }

                if (manhole != null)
                {
                    if (manhole.Compartments != null)
                    {
                        UnsubscribeEvents();
                    }
                }

                manhole = value;

                if (manhole.Compartments != null)
                {
                    DetermineSurfaceAndBottomLevels();
                    SubscribeEvents();
                }

                network = manhole.Network as IHydroNetwork;
            }
        }

        public ICommand AddCompartmentCommand { get; set; }

        public ICommand AddOrificeCommand { get; set; }

        public ICommand AddPumpCommand { get; set; }

        public ICommand AddWeirCommand { get; set; }

        public ICommand RemoveCompartmentCommand { get; set; }

        public double SurfaceLevel { get; set; }

        public double BottomLevel { set; get; }

        public object SelectedItem { get; set; }

        private void DetermineSurfaceAndBottomLevels()
        {
            SurfaceLevel = CalculateSurfaceLevel();
            BottomLevel = CalculateBottomLevel();
        }

        private double CalculateSurfaceLevel()
        {
            var compartments = manhole?.Compartments;
            if (compartments != null && compartments.Any())
            {
                return compartments.Max(c => c.SurfaceLevel);
            }

            return 0;
        }

        private double CalculateBottomLevel()
        {
            var compartments = manhole?.Compartments;
            if (compartments != null && compartments.Any())
            {
                return compartments.Min(c => c.BottomLevel);
            }

            return 0;
        }

        private void EventedListCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            DetermineSurfaceAndBottomLevels();
        }

        private void SubscribeEvents()
        {
            manhole.Compartments.CollectionChanged += EventedListCollectionChanged;
        }

        private void UnsubscribeEvents()
        {
            manhole.Compartments.CollectionChanged -= EventedListCollectionChanged;
        }

        #region Add/remove methods

        private void RemoveItem(object obj)
        {
            var compartmentToRemove = SelectedItem as Compartment;
            if (compartmentToRemove != null)
            {
                // Remove compartment
                var sewerConnections = manhole.InternalConnections();
                foreach (var sewerConnection in sewerConnections)
                {
                    if (sewerConnection.SourceCompartment == compartmentToRemove)
                    {
                        sewerConnection.SourceCompartment = null;
                    }

                    if (sewerConnection.TargetCompartment == compartmentToRemove)
                    {
                        sewerConnection.TargetCompartment = null;
                    }
                }

                manhole.Compartments.Remove(compartmentToRemove);
            }

            var structure1D = SelectedItem as IStructure1D;
            if (structure1D != null)
            {
                var connectionsToRemove = manhole.InternalConnections().Where(connection => connection.BranchFeatures.Contains(structure1D)).ToList();
                foreach (var connection in connectionsToRemove)
                {
                    manhole.Network.Branches.Remove(connection);
                }
            }

            var orifice = SelectedItem as SewerConnectionOrifice;
            if (orifice != null)
            {
                manhole.Network.Branches.Remove(orifice);
            }
        }

        private bool CanRemoveItem(object obj)
        {
            return SelectedItem != null;
        }
        
        private void AddCompartment(object obj)
        {
            var newCompartment = SewerFactory.CreateNewCompartmentAndAddToManhole(network, manhole);
            TryConnectCompartmentToConnection(manhole, newCompartment);
        }

        private bool CanAddCompartment(object obj)
        {
            return Manhole?.Compartments != null;
        }

        private void AddPump(object obj)
        {
            AddNewSewerConnectionWithStructure<Pump>();
        }

        private bool CanAddPump(object obj)
        {
            return true;
        }

        private void AddOrifice(object obj)
        {
            AddNewSewerConnectionWithStructure<SewerConnectionOrifice>();
        }

        private bool CanAddOrifice(object obj)
        {
            return true;
        }

        private void AddWeir(object obj)
        {
            AddNewSewerConnectionWithStructure<Weir>();
        }

        private bool CanAddWeir(object obj)
        {
            return true;
        }

        private void AddNewSewerConnectionWithStructure<T>()
        {
            var connection = SewerFactory.CreateConnectionWithStructure<T>(manhole);
            if (connection == null) return;

            TryConnecTSewerConnectionToCompartments(connection, manhole);

            network.Branches.Add(connection);
        }

        private static void TryConnecTSewerConnectionToCompartments(ISewerConnection sewerConnection, Manhole manhole)
        {
            var currentConnections = manhole.InternalConnections().ToList();
 
            for (var i = 1; i < manhole.Compartments.Count; i++)
            {
                var sourceCompartment = manhole.Compartments[i - 1];
                var sourceAvailable = currentConnections.All(cc => cc.SourceCompartment != sourceCompartment);
                var targetCompartment = manhole.Compartments[i];
                var targetAvailable = currentConnections.All(cc => cc.SourceCompartment != targetCompartment);

                if (sourceAvailable && targetAvailable)
                {
                    sewerConnection.SourceCompartment = sourceCompartment;
                    sewerConnection.TargetCompartment = targetCompartment;
                    break;
                }
            }
        }

        private static void TryConnectCompartmentToConnection(Manhole parentManhole, Compartment newCompartment)
        {
            // Find sewer connection where this compartment can be a target
            var sewerConnections = parentManhole.InternalConnections().ToList();

            var sewerConnectionsWithoutTargetCompartments = sewerConnections.FirstOrDefault(sc => sc.TargetCompartment == null);
            var sewerConnectionWithoutSourceCompartment = sewerConnections.FirstOrDefault(sc => sc.SourceCompartment == null);
            if (sewerConnectionsWithoutTargetCompartments != null)
            {
                sewerConnectionsWithoutTargetCompartments.TargetCompartment = newCompartment;
            }
            if (sewerConnectionWithoutSourceCompartment == null) return;

            sewerConnectionWithoutSourceCompartment.SourceCompartment = newCompartment;
        }

        #endregion
    }

    public static class SewerFactory
    {
        private static readonly Dictionary<Type, Func<ISewerConnection, ISewerConnection>> SewerConnectionStructureCreators = new Dictionary<Type, Func<ISewerConnection, ISewerConnection>>
        {
            { typeof(SewerConnectionOrifice), CreateOrificeConnection},
            { typeof(Pump), CreatePumpConnection },
            { typeof(Weir), CreateWeirConnection }
        };

        public static Compartment CreateNewCompartmentAndAddToManhole(IHydroNetwork network, Manhole parentManhole)
        {
            var name = GetUniqueCompartmentName(network);

            var newCompartment = new Compartment
            {
                Name = name,
                ParentManhole = parentManhole,
                ManholeWidth = 1,
                BottomLevel = 0,
                SurfaceLevel = 1,
            }; 
            parentManhole.Compartments.Add(newCompartment);
            return newCompartment;
        }

        public static ISewerConnection CreateConnectionWithStructure<T>(Manhole manhole)
        {
            if (!SewerConnectionStructureCreators.ContainsKey(typeof(T))) return null;

            var connection = CreateNewInternalConnection(manhole);
            var connectionWithStructure = SewerConnectionStructureCreators[typeof(T)]?.Invoke(connection);

            return connectionWithStructure;
        }

        private static ISewerConnection CreateNewInternalConnection(Manhole manhole)
        {
            var connection = new SewerConnection
            {
                Source = manhole,
                Target = manhole,
                Geometry = new LineString(new[]
                {
                    manhole.Geometry.Coordinate,
                    manhole.Geometry.Coordinate
                })
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
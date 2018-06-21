using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class ManholeViewModel
    {
        private IHydroNetwork network;
        private Manhole manhole;
        private bool isSynchronising = false;
        private Compartment selectedCompartment;

        public ManholeViewModel()
        {
            AddCompartmentCommand = new RelayCommand(AddCompartment, CanAddCompartment);
            AddPumpCommand = new RelayCommand(AddPump, CanAddPump);
            AddOrificeCommand = new RelayCommand(AddOrifice, CanAddOrifice);
            AddWeirCommand = new RelayCommand(AddWeir, CanAddWeir);
            RemoveCompartmentCommand = new RelayCommand(RemoveCompartment, CanRemoveCompartment);
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
                    CompartmentsWrapper.Clear();
                    PipesInManholes.Clear();
                    return;
                }

                if (manhole != null)
                {
                    if (manhole.Compartments != null)
                    {
                        UnsubscribeEvents();
                    }
                }

                PipesInManholes.Clear();
                CompartmentsWrapper.Clear();
                manhole = value;

                if (manhole.Compartments != null)
                {
                    DetermineSurfaceAndBottomLevels();
                    CompartmentsWrapper.AddRange(manhole.Compartments);

                    SubscribeEvents();
                }

                network = manhole.Network as IHydroNetwork;

                if (network?.Pipes != null)
                {
                    var pipes = manhole.GetPipesConnectedToManhole(network.Pipes);
                    PipesInManholes.AddRange(pipes);
                }
            }
        }

        public ICommand AddCompartmentCommand { get; set; }

        public ICommand AddOrificeCommand { get; set; }

        public ICommand AddPumpCommand { get; set; }

        public ICommand AddWeirCommand { get; set; }

        public ICommand RemoveCompartmentCommand { get; set; }

        /* This collection is required because the collection changed event from the EventedList is not working correctly. 
         * This is an known issue and will be fixed with the upgrade to the framework version 1.4.
         * When that is done, remove CompartmentsWrapper and bind the Manhole.Compartments directly to the ItemsControl in the view. */
        public ObservableCollection<Compartment> CompartmentsWrapper { get; set; } = new ObservableCollection<Compartment>();

        public ObservableCollection<ISewerConnection> StructuresWrapper { get; set; } = new ObservableCollection<ISewerConnection>();

        public ObservableCollection<IPipe> PipesInManholes { get; set; } = new ObservableCollection<IPipe>();

        public Compartment SelectedCompartment
        {
            get { return selectedCompartment; }
            set
            {
                selectedCompartment = value;
                HasSelectedCompartment = selectedCompartment != null;
            }
        }

        public bool HasSelectedCompartment { get; set; }

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

        private void RemoveCompartment(object obj)
        {
            // First remove the internal connection, connected to that specific compartment

            // 
            // Remove selected item
            // if selectedItem is compartment:

            // if selectedItem is orifice:

            // if selectedItem is.. etc.
            
            var compartmentToRemove = SelectedItem as Compartment;
            if (compartmentToRemove != null)
            {
                // Remove compartment
                var sewerConnections = manhole.GetManholeInternalConnections();
                foreach (var sewerConnection in sewerConnections)
                {
                    if (sewerConnection.SourceCompartment == compartmentToRemove)
                    {
                        sewerConnection.ReplaceCompartmentsOnInternalConnection(null, sewerConnection.TargetCompartment);
                    }

                    if (sewerConnection.TargetCompartment == compartmentToRemove)
                    {
                        sewerConnection.ReplaceCompartmentsOnInternalConnection(sewerConnection.SourceCompartment, null);
                    }
                }

                manhole.Compartments.Remove(compartmentToRemove);
                // Remove from sewer connection source/target compartments. 
                // Reset source / target such that they stay connected


            }

            var pump = SelectedItem as Pump;
            if (pump != null)
            {
                // Remove pump connection
            }

            var orifice = SelectedItem as SewerConnectionOrifice;
            if (orifice != null)
            {
                // Remove orifice connection
            }

            CompartmentsWrapper.Remove(SelectedCompartment);
        }

        private bool CanRemoveCompartment(object obj)
        {
            return SelectedItem != null;
        }

        private void AddCompartment(object obj)
        {
            var name = GetUniqueCompartmentName(network);

            var newCompartment = new Compartment
            {
                Name = name,
                ParentManhole = Manhole,
                ManholeWidth = 1,
                BottomLevel = 0,
                SurfaceLevel = 1,
            };

            try
            {
                // Find sewer connection where this compartment can be a target
                var sewerConnections = manhole.GetManholeInternalConnections().ToList();

                var sewerConnectionsWithoutTargetCompartments = sewerConnections.FirstOrDefault(sc => sc.TargetCompartment == null);
                if (sewerConnectionsWithoutTargetCompartments != null)
                {
                    sewerConnectionsWithoutTargetCompartments.TargetCompartment = newCompartment;
                    return;
                }

                // Cant find a sewerconnection with an empty target. Try to find a connection with an empty source
                var sewerConnectionWithoutSourceCompartment = sewerConnections.FirstOrDefault(sc => sc.SourceCompartment == null);
                if (sewerConnectionWithoutSourceCompartment == null) return;

                sewerConnectionWithoutSourceCompartment.SourceCompartment = newCompartment;
            }
            finally
            {
                Manhole.Compartments.Add(newCompartment);
            }

            //CompartmentsWrapper.Add(newCompartment);
        }

        private readonly Dictionary<Key, Func<ISewerConnection>> CreateSewerConnectionDictionary = new Dictionary<Key, Func<ISewerConnection>>
        {
            { Key.Orifice, CreateOrificeConnection},
            { Key.Pump, CreatePumpConnection },
            { Key.Weir, CreateWeirConnection }
        };

        private static ISewerConnection CreateWeirConnection()
        {
            var connection = new SewerConnection();
            connection.BranchFeatures.Add(CreateNewWeir());
            return connection;
        }

        private static Weir CreateNewWeir()
        {
            return new Weir();
        }

        private static ISewerConnection CreatePumpConnection()
        {
            var connection = new SewerConnection();
            connection.BranchFeatures.Add(CreateNewPump());
            return connection;
        }

        private static ISewerConnection CreateOrificeConnection()
        {
            return new SewerConnectionOrifice
            {
                Bottom_Level = 1,
            };
        }

        private enum Key
        {
            Orifice,
            Pump,
            Weir
        }

        private void AddNewConnection(Key key)
        {
            var connection = CreateSewerConnection(key);
            if (connection == null) return;

            network.Branches.Add(connection);
        }

        private ISewerConnection CreateSewerConnection(Key key)
        {
            if (!CreateSewerConnectionDictionary.ContainsKey(key)) return null;

            var connection = CreateSewerConnectionDictionary[key]?.Invoke();

            if (connection == null) return null;

            connection.Source = Manhole;
            connection.Target = Manhole;
            TryConnectNewSewerConnectionSourceCompartment(connection, manhole);
            
            return connection;
        }

        private void AddOrifice(object obj)
        {
            AddNewConnection(Key.Orifice);
        }

        private static void TryConnectNewSewerConnectionSourceCompartment(ISewerConnection sewerConnection, Manhole manhole)
        {
            var currentConnections = manhole.GetManholeInternalConnections().ToList();

            var availableCompartment = manhole.Compartments.FirstOrDefault(compartment => currentConnections.All(cc => cc.SourceCompartment != compartment));

            if (availableCompartment != null)
            {
                sewerConnection.SourceCompartment = availableCompartment;
            }
        }

        private bool CanAddOrifice(object obj)
        {
            return true;
        }

        private void AddPump(object obj)
        {
            AddNewConnection(Key.Pump);
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

        private bool CanAddPump(object obj)
        {
            return true;
        }

        private void AddWeir(object obj)
        {
            AddNewConnection(Key.Weir);
        }

        private bool CanAddWeir(object obj)
        {
            return true;
        }

        private bool CanAddCompartment(object obj)
        {
            return CompartmentsWrapper != null && Manhole?.Compartments != null;
        }
      
        private void ObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (isSynchronising) return;

            isSynchronising = true;
            // If item is added, also add to evented list (if not already there)
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddItemsToCollection(e.NewItems, Manhole.Compartments);
            }

            // If item is removed, also remove from evented list
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                RemoveOldItemsFromCollection(e.OldItems, Manhole.Compartments);
            }
            isSynchronising = false;

        }

        private void EventedListCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (isSynchronising) return;

            isSynchronising = true;

            // If an item is added to the evented list, also add to the observable collection
            if (e.Action == NotifyCollectionChangeAction.Add)
            {
                AddItemToCollection(CompartmentsWrapper, e.Item);
            }

            // If an item is removed from the evented list, also remove from the observable collection
            if (e.Action == NotifyCollectionChangeAction.Remove)
            {
                RemoveItemsFromCollection(CompartmentsWrapper, e.Item);
            }

            isSynchronising = false;

            // Update surface and bottom levels
            DetermineSurfaceAndBottomLevels();

        }

        private void AddItemsToCollection<T>(IList items, IList<T> target)
        {
            foreach (var newItem in items)
            {
                AddItemToCollection(target, newItem);
            }
        }

        private static void AddItemToCollection<T>(IList<T> target, object newItem)
        {
            if (!(newItem is T) || target.Contains((T)newItem)) return;

            target.Add((T)newItem);
        }

        private void RemoveOldItemsFromCollection<T>(IList items, IList<T> target)
        {
            foreach (var item in items)
            {
                RemoveItemsFromCollection(target, item);
            }
        }

        private static void RemoveItemsFromCollection<T>(IList<T> target, object item)
        {
            if (!(item is T) || !target.Contains((T)item)) return;

            target.Remove((T)item);
        }

        private void SubscribeEvents()
        {
            manhole.Compartments.CollectionChanged += EventedListCollectionChanged;
            CompartmentsWrapper.CollectionChanged += ObservableCollectionChanged;
        }

        private void UnsubscribeEvents()
        {
            manhole.Compartments.CollectionChanged -= EventedListCollectionChanged;
            CompartmentsWrapper.CollectionChanged -= ObservableCollectionChanged;
        }

        private static string GetUniqueCompartmentName(IHydroNetwork network)
        {
            var compartmentList = network.Manholes.SelectMany(m => m.Compartments);
            return NetworkHelper.GetUniqueName("Compartment{0:D2}", compartmentList, "Compartment");
        }
    }
}
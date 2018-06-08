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
using NetTopologySuite.Extensions.Features;
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

            CompartmentsWrapper.Remove(SelectedCompartment);
        }

        private bool CanRemoveCompartment(object obj)
        {
            return SelectedCompartment != null;
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

            CompartmentsWrapper.Add(newCompartment);
        }

        private bool CanAddCompartment(object obj)
        {
            return CompartmentsWrapper != null && Manhole?.Compartments != null;
        }

        // Add/get the internal connections with structures
        private void GetInternalConnections()
        {
            var connections = manhole.GetManholeInternalConnections().ToList();

            var orificeConnections = connections.OfType<SewerConnectionOrifice>().ToList();
            var structureConnections = connections.Where(c => c.BranchFeatures.Any());

            StructuresWrapper.AddRange(orificeConnections);
            StructuresWrapper.AddRange(structureConnections);

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
            if (!(newItem is T) || target.Contains((T) newItem)) return;

            target.Add((T) newItem);
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
            if (!(item is T) || !target.Contains((T) item)) return;

            target.Remove((T) item);
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
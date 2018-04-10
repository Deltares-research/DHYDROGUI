using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                    Compartments.Clear();
                }

                if (manhole != null)
                {
                    if (manhole.Compartments != null)
                    {
                        manhole.Compartments.CollectionChanged -= CompartmentsOnCollectionChanged;
                    }
                }

                manhole = value;



                if (manhole != null)
                {
                    if (manhole.Compartments != null)
                    {
                        Compartments.Clear();
                        Compartments.AddRange(manhole.Compartments);

                        DetermineSurfaceAndBottomLevels();

                        manhole.Compartments.CollectionChanged += CompartmentsOnCollectionChanged;
                    }

                    network = manhole.Network as IHydroNetwork;

                    if (network?.Pipes != null)
                    {
                        var pipes = manhole.GetPipesConnectedToManhole(network.Pipes);
                        PipesInManholes.AddRange(pipes);
                    }
                }
            }
        }

        public ObservableCollection<Compartment> Compartments { get; set; } = new ObservableCollection<Compartment>();

        public ObservableCollection<IPipe> PipesInManholes { get; set; } = new ObservableCollection<IPipe>();

        public Compartment SelectedCompartment { get; set; }

        public double SurfaceLevel { get; set; }

        public double BottomLevel { set; get; }


        public ICommand AddCompartmentCommand { get; set; }

        public ICommand RemoveCompartmentCommand { get; set; }

        private void RemoveCompartment(object obj)
        {
            manhole?.Compartments?.Remove(SelectedCompartment);
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
            };

            manhole.Compartments.Add(newCompartment);
        }

        private bool CanAddCompartment(object obj)
        {
            return Manhole?.Compartments != null;
        }

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

        private void CompartmentsOnCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            var compartment = e.Item as Compartment;
            if (compartment == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangeAction.Add:
                    Compartments.Add(compartment);
                    break;
                case NotifyCollectionChangeAction.Remove:
                    Compartments.Remove(compartment);
                    break;
                case NotifyCollectionChangeAction.Replace:
                    break;
                case NotifyCollectionChangeAction.Reset:
                    break;
            }

            DetermineSurfaceAndBottomLevels();
        }
        
        private static string GetUniqueCompartmentName(IHydroNetwork network)
        {
            var compartmentList = network.Manholes.SelectMany(m => m.Compartments);
            return NetworkHelper.GetUniqueName("Compartment{0:D2}", compartmentList, "Compartment");
        }
    }
}
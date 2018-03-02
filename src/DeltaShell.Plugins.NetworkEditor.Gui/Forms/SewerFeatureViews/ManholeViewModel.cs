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
                        manhole.Compartments.CollectionChanged += CompartmentsOnCollectionChanged;
                    }
                    
                    network = manhole.Network as IHydroNetwork;

                    if (network?.Pipes != null)
                    {
                        GetPipesForManhole(network.Pipes);
                    }
                }
            }
        }

        public ObservableCollection<Compartment> Compartments { get; set; } = new ObservableCollection<Compartment>();

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

        }

        private void GetPipesForManhole(IEnumerable<IPipe> pipes)
        {
            var compartments = manhole.Compartments;

            foreach (var pipe in pipes)
            {
                if (compartments.Contains(pipe.SourceCompartment) || compartments.Contains(pipe.TargetCompartment))
                {
                    PipesInManholes.Add(pipe); 
                }
            }
        }
        
        public ObservableCollection<IPipe> PipesInManholes { get; set; } = new ObservableCollection<IPipe>();
        
        public Compartment SelectedCompartment { get; set; }
        
        public ICommand AddCompartmentCommand { get; set; }  

        public ICommand RemoveCompartmentCommand { get; set; } 

        private void RemoveCompartment(object obj)
        {
            manhole?.Compartments?.Remove(SelectedCompartment); 
            // TODO Sil set SelectedCompartment to null or is this done automatically?
        }

        private bool CanRemoveCompartment(object obj)
        {
            return SelectedCompartment != null;
        }

        private bool CanAddCompartment(object obj)
        {
            return Manhole?.Compartments != null;
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

        private static string GetUniqueCompartmentName(IHydroNetwork network)
        {
            var compartmentList = network.Manholes.SelectMany(m => m.Compartments);
            return NetworkHelper.GetUniqueName("Compartment{0:D2}", compartmentList, "Compartment");
        }
    }
}
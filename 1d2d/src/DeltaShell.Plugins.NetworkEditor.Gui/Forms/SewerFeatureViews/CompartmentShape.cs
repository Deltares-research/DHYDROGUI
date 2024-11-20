using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class CompartmentShape : DrawingShape
    {
        private ICompartment compartment;

        public ICompartment Compartment
        {
            get { return compartment; }
            set
            {
                if (compartment != null)
                {
                    ((INotifyPropertyChanged)compartment).PropertyChanged -= OnPropertyChanged;
                }
                compartment = value;

                if (compartment != null)
                {
                    ((INotifyPropertyChanged)compartment).PropertyChanged += OnPropertyChanged;
                    SetProperties();
                }
            }
        }

        public override object Source
        {
            get { return Compartment; }
            set { Compartment = value as ICompartment; }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            SetProperties();
        }

        private void SetProperties()
        {
            TopLevel = compartment.SurfaceLevel;
            BottomLevel = compartment.BottomLevel;
            Width = compartment.ManholeWidth;
            Height = compartment.SurfaceLevel - compartment.BottomLevel;
        }
    }
}
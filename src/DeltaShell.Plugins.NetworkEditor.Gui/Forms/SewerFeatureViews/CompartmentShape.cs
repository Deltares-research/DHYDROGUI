using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class CompartmentShape : DrawingShape
    {
        private Compartment compartment;

        public Compartment Compartment
        {
            get { return compartment; }
            set
            {
                compartment = value;

                if (compartment != null)
                {
                    SetProperties();
                }
            }
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
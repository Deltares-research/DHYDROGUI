using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class OrificeShape : InternalConnectionShape
    {
        public Orifice Orifice { get; set; }

        public override object Source
        {
            get { return Orifice; }
            set { Orifice = value as Orifice; }
        }
        
        public override double BottomLevel
        {
            get { return GetBottomLevelBasedOnCompartments(); }
            set { }
        }

        public override double TopLevel
        {
            get { return Orifice?.CrestLevel ?? double.NaN; }
            set { }
        }
    }
}
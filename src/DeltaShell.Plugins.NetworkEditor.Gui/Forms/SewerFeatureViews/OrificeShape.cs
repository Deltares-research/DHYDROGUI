using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class OrificeShape : ConnectionShape
    {
        public SewerConnectionOrifice Orifice { get; set; }

        public override double BottomLevel
        {
            get { return GetBottomLevelBasedOnCompartments(); }
            set { }
        }

        public override double TopLevel
        {
            get { return Orifice?.Bottom_Level ?? double.NaN; }
            set { }
        }
    }
}
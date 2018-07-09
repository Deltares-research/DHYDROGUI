using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class OrificeShape : ConnectionShape
    {
        public SewerConnectionOrifice Orifice { get; set; }

        public override object Source
        {
            get { return Orifice; }
            set { Orifice = value as SewerConnectionOrifice; }
        }

        public override ISewerConnection SewerConnection
        {
            get
            {
                return Orifice;
            }
            set
            {
                var orifice = value as SewerConnectionOrifice;
                if (orifice != null) Orifice = orifice;
            }
        }

        public override double BottomLevel
        {
            get { return GetBottomLevelBasedOnCompartments(); }
            set { }
        }

        public override double TopLevel
        {
            get { return ((SewerConnectionOrifice)SewerConnection)?.Bottom_Level ?? double.NaN; }
            set { }
        }
    }
}
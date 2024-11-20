using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class WeirShape : InternalConnectionShape
    {
        public Weir Weir { get; set; }

        public override object Source
        {
            get { return Weir; }
            set { Weir = value as Weir; }
        }

        public override double BottomLevel
        {
            get { return Weir?.CrestLevel ?? double.NaN; }
            set { }
        }

        public override double TopLevel
        {
            get { return GetTopLevelBasedOnCompartments(); }
            set { }
        }
    }
}
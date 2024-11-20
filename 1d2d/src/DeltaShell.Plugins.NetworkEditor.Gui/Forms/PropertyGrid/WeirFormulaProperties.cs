using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public abstract class WeirFormulaProperties
    {
        protected readonly IWeirFormula weirFormula;

        // This is clearly unfeasible, but necessary because the formula property setters of min/max flow direction
        // need to have knowledge about the weir's allowed flow direction (perhaps the latter should be in the definition?)

        protected readonly IWeir weir;

        protected WeirFormulaProperties(IWeirFormula weirFormula, IWeir weir)
        {
            this.weirFormula = weirFormula;
            this.weir = weir;
        }

        [Browsable(false)]
        public string Name
        {
            get { return weirFormula.Name; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Rectangular shape")]
        [Description("Freeform (false) /Rectangle (true)")]
        public bool IsRectangle
        {
            get { return weirFormula.IsRectangle; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Has flow direction")]
        public bool HasFlowDirection
        {
            get { return weirFormula.HasFlowDirection; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
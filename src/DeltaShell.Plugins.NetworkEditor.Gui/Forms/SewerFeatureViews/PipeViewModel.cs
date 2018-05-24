using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class PipeViewModel
    {
        private Pipe pipe;

        public Pipe Pipe
        {
            get { return pipe; }
            set
            {
                pipe = value;

                if (pipe == null) return;

                PipeSlope = pipe.Slope();
            }
        }

        public double PipeSlope { get; set; }
    }
}
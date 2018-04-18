using System;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class PipeViewModel
    {
        public Pipe Pipe { get; set; }

        private static double GetPipeDeltaX(IPipe pipe)
        {
            var length = pipe.Length;
            var dy = pipe.LevelTarget - pipe.LevelSource;

            var dx = Math.Sqrt(length * length - dy * dy);
            return dx;

        }

    }
}
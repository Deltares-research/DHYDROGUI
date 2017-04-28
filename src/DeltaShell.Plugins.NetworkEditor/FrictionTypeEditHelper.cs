using System.Drawing;
using DelftTools.Hydro;
using DelftTools.Units;

namespace DeltaShell.Plugins.NetworkEditor
{
    public class FrictionTypeEditHelper
    {
        public RoughnessType Type { get; set; }

        /// <summary>
        /// Name of friction
        /// </summary>
        
        public string Name { get; set; }
        /// <summary>
        /// ID (for now related to Sobek). Remove it ..we are not sobek. Use conversion to/form type
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Symbol and Unit
        /// </summary>
        public Unit Unit{ get; set; }
        /// <summary>
        /// Min value (used for style ranges)
        /// </summary>
        public double Min { get; set; }
        /// <summary>
        /// Max value(used for style ranges)
        /// </summary>
        public double Max { get; set; }
        /// <summary>
        /// DefaultValue
        /// </summary>
        public double DefaultValue { get; set; }

        /// <summary>
        /// Color to use as background
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// FrictionType: Type of friction name, symbol, unit and other characteristics
        /// </summary>
        public FrictionTypeEditHelper()
        {
            Type = RoughnessType.Chezy;
            Name = "Chezy";
            Id = 0;
            Unit = new Unit("Chezy", "m^1/2.s^1");
            Min = 20;
            Max = 100;
            DefaultValue = 45;
            BackgroundColor = Color.White;
        }
    }
}
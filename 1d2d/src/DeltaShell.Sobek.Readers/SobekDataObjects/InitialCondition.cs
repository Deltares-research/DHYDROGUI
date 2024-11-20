using System.Data;
using DelftTools.Functions.Generic;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class InitialCondition
    {
        public InitialCondition()
        {
            InterpolationNotSetValue = InterpolationType.None;
            Interpolation = InterpolationNotSetValue;
        }
        public bool IsConstant { get; set; }
        public double Constant { get; set; }
        public DataTable Data { get; set; }

        public InterpolationType Interpolation { get; set; }
        public static InterpolationType InterpolationNotSetValue { get; private set; }
    }
}
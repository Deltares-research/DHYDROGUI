using System;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekPidControllerProperties : ISobekControllerProperties
    {
        public double USminimum;
        public double USmaximum;
        public double USinitial; //Initial struacture state. Will not be used
        public double KFactorProportional;
        public double KFactorIntegral;
        public double KFactorDifferential;
        public double MaximumSpeed;
        public double ConstantSetPoint = double.NaN;
        public SobekType FromSobekType;
        public TimeSpan TimeStepModel;

    }
}
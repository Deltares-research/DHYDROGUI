namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekIntervalControllerProperties : ISobekControllerProperties
    {
        public double USminimum;
        public double USmaximum;
        public IntervalControllerIntervalType IntervalType;
        public double FixedInterval;
        public double ControlVelocity;
        public IntervalControllerDeadBandType DeadBandType;
        public double DeadBandFixedSize;
        public double DeadBandPecentage;
        public double DeadBandMin;
        public double DeadBandMax;
        public double ConstantSetPoint;
    }

    public enum IntervalControllerDeadBandType
    {
        Fixed = 0,
        PercentageDischarge = 1
    }

    public enum IntervalControllerIntervalType
    {
        Fixed = 0,
        Variable = 1
    }
}
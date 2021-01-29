namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekIntervalControllerProperties : ISobekControllerProperties
    {
        public double USminimum{ get; set; }
        public double USmaximum{ get; set; }
        public IntervalControllerIntervalType IntervalType{ get; set; }
        public double FixedInterval{ get; set; }
        public double ControlVelocity{ get; set; }
        public IntervalControllerDeadBandType DeadBandType{ get; set; }
        public double DeadBandFixedSize{ get; set; }
        public double DeadBandPecentage{ get; set; }
        public double DeadBandMin{ get; set; }
        public double DeadBandMax{ get; set; }
        public double ConstantSetPoint{ get; set; }
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
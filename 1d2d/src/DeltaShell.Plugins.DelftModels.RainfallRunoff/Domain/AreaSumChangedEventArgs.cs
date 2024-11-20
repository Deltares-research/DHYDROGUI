using System;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain
{
    public class AreaSumChangedEventArgs : EventArgs
    {
        
        public AreaSumChangedEventArgs(double sum)
        {
            Sum = sum;
        }

        public double Sum { get; }
    }
}
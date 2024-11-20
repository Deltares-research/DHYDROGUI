using System;

namespace DeltaShell.NGHS.Common.Gui.WPF.SettingsView
{
    public class DoubleWrapper
    {
        private double value;
        public Action<double> SetBackValue { get; set; }

        public double WrapperValue
        {
            get => value;
            set
            {
                this.value = value;
                SetBackValue?.Invoke(this.value);
            }
        }
    }
}
using System;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    public class DoubleWrapper
    {
        private double _value;
        public Action<double> SetBackValue { get; set; }

        public double WrapperValue
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                SetBackValue?.Invoke(_value);
            }
        }
    }
}
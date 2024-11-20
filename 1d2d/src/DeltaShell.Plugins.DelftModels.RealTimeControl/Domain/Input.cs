using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity(FireOnCollectionChange = false)]
    public class Input : ConnectionPoint, IInput
    {
        public override ConnectionType ConnectionType
        {
            get
            {
                return ConnectionType.Input;
            }
        }

        /// <summary>
        /// Value of Setpoint in generated xml; will always be regenerated during ToXmlInputReference process
        /// </summary>
        public string SetPoint { get; set; }

        public override object Clone()
        {
            var input = new Input();
            input.CopyFrom(this);
            return input;
        }

        public override void CopyFrom(object source)
        {
            var input = source as Input;
            if (input != null)
            {
                base.CopyFrom(source);
            }
        }
    }
}
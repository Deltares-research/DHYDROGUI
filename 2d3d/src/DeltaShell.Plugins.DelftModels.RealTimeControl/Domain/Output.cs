using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity(FireOnCollectionChange = false)]
    public class Output : ConnectionPoint
    {
        public override ConnectionType ConnectionType => ConnectionType.Output;
        public string IntegralPart { get; set; }
        public string DifferentialPart { get; set; }

        public override object Clone()
        {
            var output = new Output();
            output.CopyFrom(this);
            return output;
        }

        public override void CopyFrom(object source)
        {
            var output = source as Output;
            if (output != null)
            {
                base.CopyFrom(output);
                IntegralPart = output.IntegralPart;
            }
        }
    }
}
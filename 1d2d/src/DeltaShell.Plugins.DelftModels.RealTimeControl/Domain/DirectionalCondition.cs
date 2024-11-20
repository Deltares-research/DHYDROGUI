using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Converters;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Domain
{
    [Entity]
    public class DirectionalCondition : StandardCondition
    {
        public override string GetDescription()
        {
            return new DirectionalOperationConverter().OperationToString(Operation);
        }

        public override object Clone()
        {
            var directionalCondition = new DirectionalCondition();
            directionalCondition.CopyFrom(this);
            return directionalCondition;
        }

        public override void CopyFrom(object source)
        {
            var directionalCondition = source as DirectionalCondition;
            if (directionalCondition != null)
            {
                base.CopyFrom(source);
            }
        }
    }
}
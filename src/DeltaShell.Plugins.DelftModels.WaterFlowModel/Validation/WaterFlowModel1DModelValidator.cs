using DelftTools.Utils.Validation;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation
{
    public class WaterFlowModel1DModelValidator : IValidator<WaterFlowModel1D, WaterFlowModel1D>
    {
        public ValidationReport Validate(WaterFlowModel1D rootObject, WaterFlowModel1D target = null)
        {
            return new ValidationReport(rootObject.Name + " (Water Flow 1D Model)", new[]
            {
                WaterFlowModel1DHydroNetworkValidator.Validate(rootObject.Network),
                WaterFlowModel1DModelDataValidator.Validate(rootObject)
            });
        }
    }
}
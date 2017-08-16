using System.Linq;
using DeltaShell.Plugins.DelftModels.HydroModel.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Validation
{
    [TestFixture]
    public class WorkFlowTypeValidatorFactoryTest
    {
        [Test]
        public void WhenInstantiatingRainfallRunoffTwiceThenWorkFlowTypeValidatorFactoryContainsOneWorkFlowTypeValidator()
        {
            WorkFlowTypeValidatorFactory.WorkFlowTypeValidators.Clear();
            Assert.That(
                WorkFlowTypeValidatorFactory.WorkFlowTypeValidators.OfType<IWorkFlowTypeValidatorProvider>().Count(),
                Is.EqualTo(0));
            var rainfallRunoffModelJustToInjectWorkflowTypeValidator = new RainfallRunoffModel();
            Assert.That(
                WorkFlowTypeValidatorFactory.WorkFlowTypeValidators.OfType<IWorkFlowTypeValidatorProvider>().Count(),
                Is.EqualTo(1));
            var rainfallRunoffModelJustToInjectWorkflowTypeValidator2 = new RainfallRunoffModel();
            Assert.That(
                WorkFlowTypeValidatorFactory.WorkFlowTypeValidators.OfType<IWorkFlowTypeValidatorProvider>().Count(),
                Is.EqualTo(1));
        }
    }
}
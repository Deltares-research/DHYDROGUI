using System.Linq;
using DeltaShell.NGHS.Common.Validation;
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
            new RainfallRunoffModel();
            Assert.That(
                WorkFlowTypeValidatorFactory.WorkFlowTypeValidators.OfType<IWorkFlowTypeValidatorProvider>().Count(),
                Is.EqualTo(1));
            new RainfallRunoffModel();
            Assert.That(
                WorkFlowTypeValidatorFactory.WorkFlowTypeValidators.OfType<IWorkFlowTypeValidatorProvider>().Count(),
                Is.EqualTo(1));
        }
    }
}
using System.Linq;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Validation
{
    [TestFixture]
    public class WaterFlowModel1DModelDataValidatorTest
    {
        [Test]
        [TestCase("1", true)]
        [TestCase("2", true)]
        [TestCase("5", true)]
        [TestCase("0", false)]
        [TestCase("-5", false)]
        [TestCase("10", false)]
        public void ModelSettingIadvec1DShouldBeValidValue(string parameterValue, bool valid)
        {
            var model = new WaterFlowModel1D();
            var parameter = model.ParameterSettings.FirstOrDefault(p => p.Name == "Iadvec1D");
            Assert.NotNull(parameter);

            parameter.Value = parameterValue;

            var issues = model.Validate();
            var issue = issues.AllErrors.FirstOrDefault(i => i.Subject == "Iadvec1D");

            if (valid)
            {
                Assert.IsNull(issue, $"{parameterValue} should give no issue");
            }
            else
            {
                Assert.NotNull(issue, $"{parameterValue} should give issue");
                Assert.AreEqual("Numerical Parameter Iadvec1D must be 1,2 or 5. Given Value is: " + parameterValue, issue.Message);
            }
        }
    }
}
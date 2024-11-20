using System.Linq;
using DelftTools.Hydro;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Validation
{
    [TestFixture]
    public class GreenhouseDataValidatorTest
    {
        [Test]
        [TestCase(1912, true)]
        [TestCase(1999, true)]
        [TestCase(1962, false)]
        public void GivenGreenhouseDataValidator_CheckingGreenhouseData_ShouldIncludeGreenhouseYear(short greenhouseYear, bool expectError)
        {
            //Arrange
            var model = new RainfallRunoffModel
            {
                GreenhouseYear = greenhouseYear,
            };

            model.Basin.Catchments.Add(new Catchment{CatchmentType = CatchmentType.GreenHouse});
            
            // Act
            var validationReport = model.Validate();

            // Assert
            var validationIssue = validationReport.AllErrors
                                                  .Where(e => e.Subject is string)
                                                  .FirstOrDefault(e => (string)e.Subject == "Greenhouse year");

            if (expectError)
            {
                Assert.NotNull(validationIssue, $"Expected validation error for greenhouse year {greenhouseYear}");
            }
            else
            {
                Assert.Null(validationIssue, $"Did not expect validation error for greenhouse year {greenhouseYear}");
            }
        }
    }
}
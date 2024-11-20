using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.FouFile
{
    [TestFixture]
    public class FouFileDefinitionTest
    {
        private FouFileDefinition fouFileDefinition;
        private WaterFlowFMModelDefinition modelDefinition;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            modelDefinition = new WaterFlowFMModelDefinition();
        }
        
        [SetUp]
        public void SetUp()
        {
            fouFileDefinition = new FouFileDefinition();
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            Assert.That(fouFileDefinition.Variables, Is.Not.Empty);
            Assert.That(fouFileDefinition.ModelPropertyNames, Is.Not.Empty);
        }

        [Test]
        public void GetModelPropertyName_VariableIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => fouFileDefinition.GetModelPropertyName(null), Throws.ArgumentNullException);
        }

        [Test]
        public void GetModelPropertyName_UnknownVariable_ReturnsNull()
        {
            string propertyName = fouFileDefinition.GetModelPropertyName(new FouFileVariable());

            Assert.That(propertyName, Is.Null);
        }

        [Test]
        [TestCaseSource(nameof(GetSupportedFouFileQuantitiesTestCases))]
        public void GetModelPropertyName_WithSupportedQuantities_ReturnsPropertyNames(FouFileVariable fouFileVariable)
        {
            string modelPropertyName = fouFileDefinition.GetModelPropertyName(fouFileVariable);

            Assert.That(modelPropertyName, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(nameof(GetSupportedFouFileQuantitiesTestCases))]
        public void GetModelPropertyName_QuantityAndParameterUpperCase_ReturnsPropertyName(FouFileVariable fouFileVariable)
        {
            fouFileVariable.Quantity = fouFileVariable.Quantity.ToUpper();
            fouFileVariable.AnalysisType = fouFileVariable.AnalysisType?.ToUpper();

            string modelPropertyName = fouFileDefinition.GetModelPropertyName(fouFileVariable);

            Assert.That(modelPropertyName, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(nameof(GetSupportedFouFileQuantitiesTestCases))]
        public void GetModelPropertyName_WithSupportedQuantities_PropertyNameExistsInModelDefinition(FouFileVariable fouFileVariable)
        {
            string propertyName = fouFileDefinition.GetModelPropertyName(fouFileVariable);
            WaterFlowFMProperty modelProperty = modelDefinition.GetModelProperty(propertyName);

            Assert.That(modelProperty, Is.Not.Null, $"Model property '{propertyName}' does not exist in model definition.");
        }

        private static IEnumerable<TestCaseData> GetSupportedFouFileQuantitiesTestCases()
        {
            var fouFileDefinition = new FouFileDefinition();
            return fouFileDefinition.Variables.Select(v => new TestCaseData(v).SetName($"{v.Quantity} {v.AnalysisType}"));
        }
    }
}
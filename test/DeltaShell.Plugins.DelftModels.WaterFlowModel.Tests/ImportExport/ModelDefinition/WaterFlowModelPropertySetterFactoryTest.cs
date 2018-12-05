using System;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelPropertySetterFactoryTest
    {
        [Test]
        public void GivenDataModelWithTimeHeader_WhenGettingPropertySetterFromFactory_ThenTimePropertiesSetterIsReturned()
        {
            // Given
            var timeCategory = new DelftIniCategory(ModelDefinitionsRegion.TimeHeader);

            // When
            var propertySetter = WaterFlowModelPropertySetterFactory.GetPropertySetter(timeCategory);

            // Then
            Assert.That(propertySetter.GetType(), Is.EqualTo(typeof(WaterFlowModelTimePropertiesSetter)));
        }

        [Test]
        public void GivenDataModelWithUnkownHeader_WhenGettingPropertySetterFromFactory_ThenNotImplementedExceptionIsThrown()
        {
            // Given
            var unknownCategory = new DelftIniCategory("Unknown Header");

            // When
            TestDelegate getPropertySetter = () => WaterFlowModelPropertySetterFactory.GetPropertySetter(unknownCategory);

            // Then
            Assert.Throws<NotImplementedException>(getPropertySetter);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class DelftIniPropertyValidatorTest
    {
        private static DelftIniCategory Category;

        [SetUp]
        public void Initialize()
        {
            Category = CreateDelftIniCategory();
        }

        [Test]
        public void GivenDelftIniCategoryWithValidPropertyValues_WhenValidating_ThenNoErrorMessageIsReported()
        {
            //Given
            Category.ValidateProperty().Clear();
            Category = AddValidPropertyValues();

            //When
            var errorMessages = Category.ValidateProperty().ToList();

            //Then
            Assert.That(errorMessages, Is.Empty);
            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }    

        [Test]
        public void GivenDelftIniCategoryWithMissingPropertyValues_WhenValidating_ThenErrorMessageIsReported()
        {
            //Given
            Category.ValidateProperty().Clear();
            Category = AddMissingPropertyValues();

            //When
            var errorMessages = Category.ValidateProperty().ToList();

            //Then
            Assert.That(errorMessages, Is.Not.Empty);
            Assert.That(errorMessages.Count, Is.EqualTo(3));

            var property1 = Category.Properties.ElementAt(0);
            var property2 = Category.Properties.ElementAt(1);
            var property3 = Category.Properties.ElementAt(2);

            var expectedMessage1 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability__0__Property___1___on_line_number_is_missing____2___will_be_set_as_default, 
                                                property1.LineNumber, 
                                                property1.Name, 
                                                "0");
            var expectedMessage2 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability__0__Property___1___on_line_number_is_missing____2___will_be_set_as_default, 
                                                property2.LineNumber, 
                                                property2.Name, 
                                                DensityType.eckart_modified.ToString());
            var expectedMessage3 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability__0__Property___1___on_line_number_is_missing____2___will_be_set_as_default, 
                                                property3.LineNumber, 
                                                property3.Name,
                                                TemperatureModelType.Transport.ToString());

            Assert.That(errorMessages.ElementAt(0), Is.EqualTo(expectedMessage1));
            Assert.That(errorMessages.ElementAt(1), Is.EqualTo(expectedMessage2));
            Assert.That(errorMessages.ElementAt(2), Is.EqualTo(expectedMessage3));
        }

        [Test]
        public void GivenDelftIniCategoryWithInvalidPropertyValues_WhenValidating_ThenErrorMessageIsReported()
        {
            //Given
            Category.ValidateProperty().Clear();
            Category = AddInValidPropertyValues();

            //When
            var errorMessages = Category.ValidateProperty().ToList();

            //Then
            Assert.That(errorMessages, Is.Not.Empty);
            Assert.That(errorMessages.Count, Is.EqualTo(3));

            var property1 = Category.Properties.ElementAt(0);
            var property2 = Category.Properties.ElementAt(1);
            var property3 = Category.Properties.ElementAt(2);

            var expectedMessage1 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability__0__Property__1__on_line_number_is_invalid____2___will_be_set_as_default,
                property1.LineNumber,
                property1.Name,
                "0");
            var expectedMessage2 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability__0__Property__1__on_line_number_is_invalid____2___will_be_set_as_default,
                property2.LineNumber,
                property2.Name,
                DensityType.eckart_modified.ToString());
            var expectedMessage3 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability__0__Property__1__on_line_number_is_invalid____2___will_be_set_as_default,
                property3.LineNumber,
                property3.Name,
                TemperatureModelType.Transport.ToString());

            Assert.That(errorMessages.ElementAt(0), Is.EqualTo(expectedMessage1));
            Assert.That(errorMessages.ElementAt(1), Is.EqualTo(expectedMessage2));
            Assert.That(errorMessages.ElementAt(2), Is.EqualTo(expectedMessage3));
        }

        private DelftIniCategory AddMissingPropertyValues()
        {
            Category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, string.Empty);
            Category.AddProperty(ModelDefinitionsRegion.Density.Key, string.Empty);
            Category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, string.Empty);

            Assert.That(Category.Properties, Is.Not.Null);
            Assert.That(Category.Properties.Count, Is.EqualTo(3));
            return Category;
        }

        private static DelftIniCategory AddValidPropertyValues()
        {
            Category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "0");
            Category.AddProperty(ModelDefinitionsRegion.Density.Key, DensityType.eckart_modified.ToString());
            Category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, TemperatureModelType.Transport.ToString());

            Assert.That(Category.Properties, Is.Not.Null);
            Assert.That(Category.Properties.Count, Is.EqualTo(3));
            return Category;
        }

        private static DelftIniCategory AddInValidPropertyValues()
        {
            Category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "invalidValue");
            Category.AddProperty(ModelDefinitionsRegion.Density.Key, "invalidValue");
            Category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, "invalidValue");


            Assert.That(Category.Properties, Is.Not.Null);
            Assert.That(Category.Properties.Count, Is.EqualTo(3));
            return Category;
        }
        private static DelftIniCategory CreateDelftIniCategory()
        {
            var category = new DelftIniCategory(ModelDefinitionsRegion.TransportComputationValuesHeader);
            return category;
        }
    }
}

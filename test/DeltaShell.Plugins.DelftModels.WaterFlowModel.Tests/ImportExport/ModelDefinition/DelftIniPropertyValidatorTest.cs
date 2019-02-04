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
        public void GivenDelftIniCategoryWithValidDefaultPropertyValues_WhenValidating_ThenNoErrorMessageIsReported()
        {
            //Given
            Category.ValidateProperty().Clear();
            Category = AddValidDefaultPropertyValues();

            //When
            var errorMessages = Category.ValidateProperty().ToList();

            //Then
            Assert.That(errorMessages, Is.Empty);
            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }

        [Test]
        public void GivenDelftIniCategoryWithValidStandardPropertyValues_WhenValidating_ThenNoErrorMessageIsReported()
        {
            //Given
            Category.ValidateProperty().Clear();
            Category = AddValidStandardPropertyValues();

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

            var expectedMessage1 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability_Property_on_line_number_is_missing_will_be_set_as_default, 
                                                property1.LineNumber, 
                                                property1.Name, 
                                                "0");
            var expectedMessage2 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability_Property_on_line_number_is_missing_will_be_set_as_default, 
                                                property2.LineNumber, 
                                                property2.Name, 
                                                DensityType.eckart_modified.ToString());
            var expectedMessage3 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability_Property_on_line_number_is_missing_will_be_set_as_default, 
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

            var expectedMessage1 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability_Property_on_line_number_is_invalid_will_be_set_as_default,
                property1.LineNumber,
                property1.Name,
                "0");
            var expectedMessage2 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability_Property_on_line_number_is_invalid_will_be_set_as_default,
                property2.LineNumber,
                property2.Name,
                DensityType.eckart_modified.ToString());
            var expectedMessage3 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability_Property_on_line_number_is_invalid_will_be_set_as_default,
                property3.LineNumber,
                property3.Name,
                TemperatureModelType.Transport.ToString());

            Assert.That(errorMessages.ElementAt(0), Is.EqualTo(expectedMessage1));
            Assert.That(errorMessages.ElementAt(1), Is.EqualTo(expectedMessage2));
            Assert.That(errorMessages.ElementAt(2), Is.EqualTo(expectedMessage3));
        }

        [Test]
        public void GivenDelftIniCategoryWithOneInvalidOneStandardAndOneDefaultPropertyValue_WhenValidating_ThenOneErrorMessageIsReported()
        {
            //Given
            Category.ValidateProperty().Clear();
            Category = AddVariousPropertyValues1();

            //When
            var errorMessages = Category.ValidateProperty().ToList();

            //Then
            Assert.That(errorMessages, Is.Not.Empty);
            Assert.That(errorMessages.Count, Is.EqualTo(1));

            var property1 = Category.Properties.ElementAt(0);


            var expectedMessage1 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability_Property_on_line_number_is_invalid_will_be_set_as_default,
                property1.LineNumber,
                property1.Name,
                "0");

            Assert.That(errorMessages.ElementAt(0), Is.EqualTo(expectedMessage1));
        }

        [Test]
        public void GivenDelftIniCategoryWithOneInvalidOneMissingAndOneDefaultPropertyValue_WhenValidating_ThenTwoErrorMessagesAreReported()
        {
            //Given
            Category.ValidateProperty().Clear();
            Category = AddVariousPropertyValues2();

            //When
            var errorMessages = Category.ValidateProperty().ToList();

            //Then
            Assert.That(errorMessages, Is.Not.Empty);
            Assert.That(errorMessages.Count, Is.EqualTo(2));

            var property1 = Category.Properties.ElementAt(0);
            var property2 = Category.Properties.ElementAt(1);


            var expectedMessage1 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability_Property_on_line_number_is_invalid_will_be_set_as_default,
                property1.LineNumber,
                property1.Name,
                "0");

            var expectedMessage2 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability_Property_on_line_number_is_missing_will_be_set_as_default,
                property2.LineNumber,
                property2.Name,
                DensityType.eckart_modified.ToString());

            Assert.That(errorMessages.ElementAt(0), Is.EqualTo(expectedMessage1));
            Assert.That(errorMessages.ElementAt(1), Is.EqualTo(expectedMessage2));
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

        private static DelftIniCategory AddValidDefaultPropertyValues()
        {
            Category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "0");
            Category.AddProperty(ModelDefinitionsRegion.Density.Key, DensityType.eckart_modified.ToString());
            Category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, TemperatureModelType.Transport.ToString());

            Assert.That(Category.Properties, Is.Not.Null);
            Assert.That(Category.Properties.Count, Is.EqualTo(3));
            return Category;
        }
        private static DelftIniCategory AddValidStandardPropertyValues()
        {
            Category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "1");
            Category.AddProperty(ModelDefinitionsRegion.Density.Key, DensityType.eckart.ToString());
            Category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, TemperatureModelType.Composite.ToString());

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
        private static DelftIniCategory AddVariousPropertyValues1()
        {
            Category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "invalidValue");
            Category.AddProperty(ModelDefinitionsRegion.Density.Key, DensityType.unesco.ToString());
            Category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, TemperatureModelType.Transport.ToString());


            Assert.That(Category.Properties, Is.Not.Null);
            Assert.That(Category.Properties.Count, Is.EqualTo(3));
            return Category;
        }
        private static DelftIniCategory AddVariousPropertyValues2()
        {
            Category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "invalidValue");
            Category.AddProperty(ModelDefinitionsRegion.Density.Key, string.Empty);
            Category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, TemperatureModelType.Transport.ToString());


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

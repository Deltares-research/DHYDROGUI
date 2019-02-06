using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Reflection;
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
        private DelftIniCategory category;
        private List<string> errorMessages;

        [SetUp]
        public void Initialize()
        {
            category = new DelftIniCategory(ModelDefinitionsRegion.TransportComputationValuesHeader);
            category.ValidateProperties().Clear();
            errorMessages = new List<string>();
            CreateTransportComputationDelftIniCategory();
        }

        [Test]
        public void GivenDelftIniCategoryWithValidDefaultPropertyValues_WhenValidating_ThenNoErrorMessageIsReported()
        {
            //Given
            AddValidDefaultPropertyValues();

            //When
            errorMessages = category.ValidateProperties().ToList();

            //Then
            Assert.That(errorMessages, Is.Not.Null);
            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }

        [Test]
        public void GivenDelftIniCategoryWithValidStandardPropertyValues_WhenValidating_ThenNoErrorMessageIsReported()
        {
            //Given
            AddValidStandardPropertyValues();

            //When
            errorMessages = category.ValidateProperties().ToList();

            //Then
            Assert.That(errorMessages, Is.Not.Null);
            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }

        [Test]
        public void GivenDelftIniCategoryWithMissingPropertyValues_WhenValidating_ThenErrorMessageIsReported()
        {
            //Given
            AddMissingPropertyValues();

            //When
            errorMessages = category.ValidateProperties().ToList();

            //Then
            Assert.That(errorMessages, Is.Not.Null);
            Assert.That(errorMessages.Count, Is.EqualTo(3));

            var property1 = category.Properties.ElementAt(0);
            var property2 = category.Properties.ElementAt(1);
            var property3 = category.Properties.ElementAt(2);

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
            AddInValidPropertyValues();

            //When
            errorMessages = category.ValidateProperties().ToList();

            //Then
            Assert.That(errorMessages, Is.Not.Null);
            Assert.That(errorMessages.Count, Is.EqualTo(3));

            var property1 = category.Properties.ElementAt(0);
            var property2 = category.Properties.ElementAt(1);
            var property3 = category.Properties.ElementAt(2);

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
            AddVariousPropertyValues1();

            //When
            errorMessages = category.ValidateProperties().ToList();

            //Then
            Assert.That(errorMessages, Is.Not.Null);
            Assert.That(errorMessages.Count, Is.EqualTo(1));

            var property1 = category.Properties.ElementAt(0);


            var expectedMessage1 = string.Format(Resources.DelftIniPropertyValidator_CheckPropertyAvailability_Property_on_line_number_is_invalid_will_be_set_as_default,
                property1.LineNumber,
                property1.Name,
                "0");

            Assert.That(errorMessages.ElementAt(0), Is.EqualTo(expectedMessage1));
        }

        [Test]
        public void GivenDelftIniCategoryWithUnknownHeader_WhenValidating_ThenNoErrorMessagesAreReturned()
        {
            //Given
            AddCategoryWithUnknownHeader();

            //When
            errorMessages = category.ValidateProperties().ToList();

            //Then
            Assert.That(errorMessages, Is.Not.Null);
            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }

        [Test]
        public void GivenDelftIniCategoryWithOneInvalidOneMissingAndOneDefaultPropertyValue_WhenValidating_ThenTwoErrorMessagesAreReported()
        {
            //Given
            AddVariousPropertyValues2();

            //When
            errorMessages = category.ValidateProperties().ToList();

            //Then
            Assert.That(errorMessages, Is.Not.Null);
            Assert.That(errorMessages.Count, Is.EqualTo(2));

            var property1 = category.Properties.ElementAt(0);
            var property2 = category.Properties.ElementAt(1);


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

        private void AddMissingPropertyValues()
        {
            category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, string.Empty);
            category.AddProperty(ModelDefinitionsRegion.Density.Key, string.Empty);
            category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, string.Empty);

            Assert.That(category.Properties, Is.Not.Null);
            Assert.That(category.Properties.Count, Is.EqualTo(3));
        }

        private void AddValidDefaultPropertyValues()
        {
            category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "0");
            category.AddProperty(ModelDefinitionsRegion.Density.Key, DensityType.eckart_modified.ToString());
            category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, TemperatureModelType.Transport.ToString());

            Assert.That(category.Properties, Is.Not.Null);
            Assert.That(category.Properties.Count, Is.EqualTo(3));
        }

        private void AddValidStandardPropertyValues()
        {
            category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "1");
            category.AddProperty(ModelDefinitionsRegion.Density.Key, DensityType.eckart.ToString());
            category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, TemperatureModelType.Composite.ToString());

            Assert.That(category.Properties, Is.Not.Null);
            Assert.That(category.Properties.Count, Is.EqualTo(3));
        }

        private void AddInValidPropertyValues()
        {
            category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "2");
            category.AddProperty(ModelDefinitionsRegion.Density.Key, "invalidValue");
            category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, "invalidValue");


            Assert.That(category.Properties, Is.Not.Null);
            Assert.That(category.Properties.Count, Is.EqualTo(3));
        }

        private void AddVariousPropertyValues1()
        {
            category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "invalidValue");
            category.AddProperty(ModelDefinitionsRegion.Density.Key, DensityType.unesco.ToString());
            category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, TemperatureModelType.Transport.ToString());


            Assert.That(category.Properties, Is.Not.Null);
            Assert.That(category.Properties.Count, Is.EqualTo(3));
        }
        private void AddCategoryWithUnknownHeader()
        {
            TypeUtils.SetPrivatePropertyValue(category, "Name", "UnknownHeader");
            category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "invalidValue");
            category.AddProperty(ModelDefinitionsRegion.Density.Key, DensityType.unesco.ToString());
            category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, TemperatureModelType.Transport.ToString());


            Assert.That(category.Properties, Is.Not.Null);
            Assert.That(category.Properties.Count, Is.EqualTo(3));
        }

        private void AddVariousPropertyValues2()
        {
            category.AddProperty(ModelDefinitionsRegion.UseTemperature.Key, "invalidValue");
            category.AddProperty(ModelDefinitionsRegion.Density.Key, string.Empty);
            category.AddProperty(ModelDefinitionsRegion.HeatTransferModel.Key, TemperatureModelType.Transport.ToString());

            Assert.That(category.Properties, Is.Not.Null);
            Assert.That(category.Properties.Count, Is.EqualTo(3));
        }

        private void CreateTransportComputationDelftIniCategory()
        {
            category = new DelftIniCategory(ModelDefinitionsRegion.TransportComputationValuesHeader);

            Assert.That(category, Is.Not.Null);
        }
    }
}

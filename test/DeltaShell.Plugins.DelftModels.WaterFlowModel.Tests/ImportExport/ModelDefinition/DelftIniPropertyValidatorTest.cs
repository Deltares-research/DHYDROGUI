using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class DelftIniPropertyValidatorTest
    {
        [Test]
        public void GivenDelftIniCategoryWithValidPropertyValues_WhenValidating_ThenNoErrorMessageIsReported()
        {
            var category = AddValidProperties();
            var errorMessages = new List<string>();
            DelftIniPropertyValidator.ValidateProperties(category, errorMessages);

            Assert.That(errorMessages, Is.Empty);
            Assert.That(errorMessages.Count, Is.EqualTo(0));
        }    

        [Test]
        public void GivenDelftIniCategoryWithMissingPropertyValues_WhenValidating_ThenErrorMessageIsReported()
        {
            var category = AddPropertiesWithMissingValues();
            var errorMessages = new List<string>();
            DelftIniPropertyValidator.ValidateProperties(category, errorMessages);

            Assert.That(errorMessages, Is.Not.Empty);
            Assert.That(errorMessages.Count, Is.EqualTo(3));

            var property1 = category.Properties.ElementAt(0);
            var property2 = category.Properties.ElementAt(1);
            var property3 = category.Properties.ElementAt(2);

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
            var category = AddInValidProperties();
            var errorMessages = new List<string>();
            DelftIniPropertyValidator.ValidateProperties(category, errorMessages);

            Assert.That(errorMessages, Is.Not.Empty);
            Assert.That(errorMessages.Count, Is.EqualTo(3));

            var property1 = category.Properties.ElementAt(0);
            var property2 = category.Properties.ElementAt(1);
            var property3 = category.Properties.ElementAt(2);

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

        private DelftIniCategory AddPropertiesWithMissingValues()
        {
            var category = new DelftIniCategory("TransportComputation");
            category.AddProperty("Temperature", "");
            category.AddProperty("Density", "");
            category.AddProperty("HeatTransferModel", "");

            Assert.That(category.Properties, Is.Not.Null);
            Assert.That(category.Properties.Count, Is.EqualTo(3));
            return category;
        }

        private static DelftIniCategory AddValidProperties()
        {
            var category = new DelftIniCategory("TransportComputation");
            category.AddProperty("Temperature", "0");
            category.AddProperty("Density", DensityType.eckart_modified.ToString());
            category.AddProperty("HeatTransferModel", TemperatureModelType.Transport.ToString());

            Assert.That(category.Properties, Is.Not.Null);
            Assert.That(category.Properties.Count, Is.EqualTo(3));
            return category;
        }

        private static DelftIniCategory AddInValidProperties()
        {
            var category = new DelftIniCategory("TransportComputation");
            category.AddProperty("Temperature", "2");
            category.AddProperty("Density", "efoijwef");
            category.AddProperty("HeatTransferModel", "wqdjpdf");


            Assert.That(category.Properties, Is.Not.Null);
            Assert.That(category.Properties.Count, Is.EqualTo(3));
            return category;
        }
    }
}

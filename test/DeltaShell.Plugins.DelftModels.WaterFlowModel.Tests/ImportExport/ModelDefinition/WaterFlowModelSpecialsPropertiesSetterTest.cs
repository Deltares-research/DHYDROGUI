using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelSpecialsPropertiesSetterTest
    {
        private WaterFlowModelSpecialsPropertiesSetter specialsPropertiesSetter;
        private WaterFlowModel1D waterFlowModel1D;

        [SetUp]
        public void Initialize()
        {
            specialsPropertiesSetter = new WaterFlowModelSpecialsPropertiesSetter();
            waterFlowModel1D = new WaterFlowModel1D(){DesignFactorDlg = 0.0};
        }

        [Test]
        public void GivenCorrectSpecialsDataModel_WhenSettingModelProperties_ThenCorrectModelPropertiesAreSet()
        {
            // Given
            var category = GetCorrectSpecialsDataModel();

            // When
            specialsPropertiesSetter.SetProperties(category, waterFlowModel1D, new List<string>());

            // Then
            Assert.That(waterFlowModel1D.DesignFactorDlg, Is.EqualTo(1.0d));
        }

        [Test]
        public void GivenEmptySpecialsDataModel_WhenSettingModelProperties_ThenNoExceptionIsThrownAndDesignFactorDlgIsNotChanged()
        {
            // Given
            var specialsCategory = new DelftIniCategory(ModelDefinitionsRegion.SpecialsValuesHeader); // DelftIniCategory without properties

            Assert.That(waterFlowModel1D.DesignFactorDlg, Is.EqualTo(0.0));

            // When
            Assert.DoesNotThrow(() => specialsPropertiesSetter.SetProperties(specialsCategory, waterFlowModel1D, new List<string>()));

            // Then
            Assert.That(waterFlowModel1D.DesignFactorDlg, Is.EqualTo(0.0));
        }

        [Test]
        public void GivenCorrectSpecialsDataModelWithUnknownProperty_WhenSettingModelProperties_ThenUnknownPropertyIsSkippedAndErrorMessageIsReturned()
        {
            // Given
            var unknownPropertyName = "UnknownProperty";
            var category = GetCorrectSpecialsDataModel();
            category.AddProperty(unknownPropertyName, 1);

            // When
            var errorMessages = new List<string>();
            specialsPropertiesSetter.SetProperties(category, waterFlowModel1D, errorMessages);

            // Then
            Assert.That(waterFlowModel1D.DesignFactorDlg, Is.EqualTo(1.0d));

            Assert.That(errorMessages.Count, Is.EqualTo(1));
            var expectedMessage = string.Format(Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                0, unknownPropertyName);
            Assert.Contains(expectedMessage, errorMessages);
        }

        private static DelftIniCategory GetCorrectSpecialsDataModel()
        {
            var specialsCategory = new DelftIniCategory(ModelDefinitionsRegion.SpecialsValuesHeader);
            specialsCategory.AddProperty(ModelDefinitionsRegion.DesignFactorDlg.Key, "1.0");

            return specialsCategory;
        }
    }
}

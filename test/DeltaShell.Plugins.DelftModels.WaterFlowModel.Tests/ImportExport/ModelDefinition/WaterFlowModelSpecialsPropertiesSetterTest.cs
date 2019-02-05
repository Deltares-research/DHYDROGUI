using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
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
            waterFlowModel1D = new WaterFlowModel1D();
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

        private static DelftIniCategory GetCorrectSpecialsDataModel()
        {
            var specialsCategory = new DelftIniCategory(ModelDefinitionsRegion.SpecialsValuesHeader);
            specialsCategory.AddProperty(ModelDefinitionsRegion.DesignFactorDlg.Key, "1.0");

            return specialsCategory;
        }
    }
}

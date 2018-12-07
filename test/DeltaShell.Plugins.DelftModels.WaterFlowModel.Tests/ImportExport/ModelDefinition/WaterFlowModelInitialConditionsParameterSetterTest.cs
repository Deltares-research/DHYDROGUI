using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelInitialConditionsParameterSetterTest
    {
        [Test]
        public void
            GivenAnInitialConditionCategoryWithOneProperty_WhenSettingThisModelProperty_ThenThisParameterShouldBeSetInTheModel()
        {
            const string propertyName = "InitialEmptyWells";
           
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.InitialConditionsValuesHeader);
            category.AddProperty(propertyName, "1");

            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorReport = new List<string>();

            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add($"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            //When
            new WaterFlowModelInitialConditionsParameterSetter().SetProperties(category, model, CreateAndAddErrorReport);

            //Then
            Assert.AreEqual(0, errorReport.Count);

            var parameterSetting = model.ParameterSettings
                .FirstOrDefault(ps => ps.Name == propertyName);
            //ParameterSetting can never be null here, because in this situation the error report has also a message.
            Assert.NotNull(parameterSetting);
            Assert.AreEqual("True", parameterSetting.Value);
        }

        [Test]
        public void
            GivenANumericalParameterCategoryWithAnUnknownAndKnownProperty_WhenSettingTheseModelProperties_ThenOnlyTheKnownParameterShouldBeSetInTheModel()
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.InitialConditionsValuesHeader);

            category.AddProperty("bla", "2");
            category.AddProperty(ModelDefinitionsRegion.InitialEmptyWells.Key, 0);


            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorReport = new List<string>();

            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add($"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            //When
            new WaterFlowModelInitialConditionsParameterSetter().SetProperties(category, model, CreateAndAddErrorReport);

            //Then
            Assert.AreEqual(1, errorReport.Count);
            Assert.AreEqual(
                "An error occurred during reading the initial conditions of the md1d file::\r\n Parameter bla found in the md1d file. This parameter will not be imported, since it is not supported by the GUI",
                errorReport[0]);

            var parameterSetting = model.ParameterSettings
                .FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.InitialEmptyWells.Key);
            //ParameterSetting can never be null here, because in this situation the errorreport has also a message.
            Assert.NotNull(parameterSetting);
            Assert.AreEqual("False", parameterSetting.Value);
        }
    }
}

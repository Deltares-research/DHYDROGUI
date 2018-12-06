using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.ModelDefinition
{
    [TestFixture]
    public class WaterFlowModelNumericalParametersSetterTest
    {
        [TestCase("AccelerationTermFactor", "2")]
        [TestCase("AccurateVersusSpeed", "4")]
        [TestCase("CourantNumber", "0.9")]
        [TestCase("DtMinimum", "0.003")]
        [TestCase("EpsilonValueVolume", "0.0003")]
        [TestCase("EpsilonValueWaterDepth", "0.0002")]
        [TestCase("MaxDegree", "5")]
        [TestCase("MaxIterations", "9")]
        [TestCase("MinimumSurfaceatStreet", "0.3")]
        [TestCase("MinimumSurfaceinNode", "0.2")]
        [TestCase("MinimumLength", "1.4")]
        [TestCase("RelaxationFactor", "1.3")]
        [TestCase("Rho", "1001")]
        [TestCase("StructureInertiaDampingFactor", "1.2")]
        [TestCase("Theta", "1.1")]
        [TestCase("ThresholdValueFlooding", "0.02")]
        [TestCase("UseTimeStepReducerStructures", "1")]
       public void
            GivenANumericalParameterCategoryWithOneProperty_WhenSettingThisModelProperty_ThenThisParameterShouldBeSetInTheModel(string propertyName, string value)
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.NumericalParametersValuesHeader);

            category.AddProperty(propertyName, value);

            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorReport = new List<string>();

            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add($"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            //When
            (new WaterFlowModelNumericalParametersSetter()).SetProperties(category, model, CreateAndAddErrorReport);

            //Then

            Assert.AreEqual(0, errorReport.Count);

            var parameterSetting = model.ParameterSettings
                .FirstOrDefault(ps => ps.Name == propertyName);
            //ParameterSetting can never be null here, because in this situation the errorreport has also a message.
            Assert.NotNull(parameterSetting);
            Assert.AreEqual(value, parameterSetting.Value);
        }

        [Test]
        public void
            GivenANumericalParameterCategoryWithAnUnknownAndKnownProperty_WhenSettingTheseModelProperties_ThenOnlyTheKnownParameterShouldBeSetInTheModel()
        {
            //Given
            var category = new DelftIniCategory(ModelDefinitionsRegion.NumericalParametersValuesHeader);

            category.AddProperty("bla", "2");
            category.AddProperty(ModelDefinitionsRegion.AccelerationTermFactor.Key, 3);


            // Create ModelParameters
            var model = new WaterFlowModel1D();

            var errorReport = new List<string>();

            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add($"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            //When
            (new WaterFlowModelNumericalParametersSetter()).SetProperties(category, model, CreateAndAddErrorReport);

            //Then

            Assert.AreEqual(1, errorReport.Count);
            Assert.AreEqual(
                "An error occurred during reading the numerical parameters of the md1d file::\r\n Parameter bla found in the md1d file. This parameter will not be imported, since it is not supported by the GUI",
                errorReport[0]);

            var parameterSetting = model.ParameterSettings
                .FirstOrDefault(ps => ps.Name == ModelDefinitionsRegion.AccelerationTermFactor.Key);
            //ParameterSetting can never be null here, because in this situation the errorreport has also a message.
            Assert.NotNull(parameterSetting);
            Assert.AreEqual("3", parameterSetting.Value);
        }
    }
}
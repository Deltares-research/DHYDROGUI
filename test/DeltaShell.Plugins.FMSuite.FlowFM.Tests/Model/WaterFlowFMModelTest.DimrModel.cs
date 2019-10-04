using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        [Test]
        public void GetDataItemByItemString_ShouldReturnTheCorrectAreaDataItemForThatParameter()
        {
            // Given
            var fmModel = new WaterFlowFMModel();
            var gate = new Weir2D
            {
                Name = "structure01",
                WeirFormula = new GatedWeirFormula()
            };
            fmModel.Area.Weirs.Add(gate);

            // When
            var categoryComponents = new List<string>(new[]
            {
                KnownFeatureCategories.Gates,
                gate.Name,
                KnownStructureProperties.CrestLevel
            });
            string category = string.Join("/", categoryComponents);

            IDataItem dataItem = fmModel.GetDataItemByItemString(category);

            // Then
            Assert.AreEqual(gate, ((ParameterValueConverter) dataItem.ValueConverter).Location,
                            "The retrieved dataItem is not correct, since the features are not the same");
            Assert.AreEqual(gate.Name, dataItem.Name,
                            "The retrieved dataItem is not correct, since the features are not the same");
            Assert.AreEqual(KnownStructureProperties.CrestLevel,
                            ((ParameterValueConverter) dataItem.ValueConverter).ParameterName,
                            "The retrieved dataItem is not correct, since the parameters are not the same");
            Assert.AreEqual(KnownStructureProperties.CrestLevel, dataItem.Tag,
                            "The retrieved dataItem is not correct, since the parameters are not the same");
        }
    }
}
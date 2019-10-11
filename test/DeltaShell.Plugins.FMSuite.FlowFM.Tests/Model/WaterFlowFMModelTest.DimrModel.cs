using System;
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
        public void GetDataItemByItemString_ReturnsExpectedDataItem()
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
            var itemStringComponents = new List<string>(new[]
            {
                KnownFeatureCategories.Gates,
                gate.Name,
                KnownStructureProperties.CrestLevel
            });
            string itemString = string.Join("/", itemStringComponents);

            IDataItem dataItem = fmModel.GetDataItemByItemString(itemString);

            // Then
            string messageDifferentFeatureInDataItem =
                "The retrieved dataItem is not correct, since the features are not the same";

            string messageDifferentParameterInDataItem =
                "The retrieved dataItem is not correct, since the parameters are not the same";

            Assert.AreEqual(gate, ((ParameterValueConverter) dataItem.ValueConverter).Location,
                            messageDifferentFeatureInDataItem);
            Assert.AreEqual(gate.Name, dataItem.Name,
                            messageDifferentFeatureInDataItem);
            Assert.AreEqual(KnownStructureProperties.CrestLevel,
                            ((ParameterValueConverter) dataItem.ValueConverter).ParameterName,
                            messageDifferentParameterInDataItem);
            Assert.AreEqual(KnownStructureProperties.CrestLevel, dataItem.Tag,
                            messageDifferentParameterInDataItem);
        }

        [Test]
        public void GetDataItemByItemString_ForItemStringContainingOnly2Elements_ThrowArgumentException()
        {
            // Given
            var fmModel = new WaterFlowFMModel();

            // When
            var itemStringComponents = new List<string>(new[]
            {
                KnownFeatureCategories.Gates,
                KnownStructureProperties.CrestLevel
            });
            string itemString = string.Join("/", itemStringComponents);

            void Call() => fmModel.GetDataItemByItemString(itemString);

            // Then
            ArgumentException ex =
                Assert.Throws<ArgumentException>(Call);
            Assert.AreEqual($"{itemString} should contain a category, feature name and a parameter name.", ex.Message,
                            "The exception message is different than expected");
        }

        [Test]
        public void GetDataItemByItemString_ForItemStringContainingUnknownFeatureName_ThrowArgumentException()
        {
            // Given
            var fmModel = new WaterFlowFMModel();


            // When
            const string featureName = "NotExisting";

            var itemStringComponents = new List<string>(new[]
            {
                KnownFeatureCategories.Gates,
                featureName,
                KnownStructureProperties.CrestLevel
            });
            string itemString = string.Join("/", itemStringComponents);

            void Call() => fmModel.GetDataItemByItemString(itemString);

            // Then
            ArgumentException ex =
                Assert.Throws<ArgumentException>(Call);
            Assert.AreEqual($"feature {featureName} in {itemString} cannot be found in the FM model.", ex.Message,
                            "The exception message is different than expected");
        }

        [Test]
        public void GetDataItemByItemString_ForItemStringContainingUnknownParameterName_ThrowArgumentException()
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
            const string parameterName = "NotExisting";

            var itemStringComponents = new List<string>(new[]
            {
                KnownFeatureCategories.Gates,
                gate.Name,
                parameterName
            });
            string itemString = string.Join("/", itemStringComponents);

            void Call() => fmModel.GetDataItemByItemString(itemString);

            // Then
            ArgumentException ex =
                Assert.Throws<ArgumentException>(Call);
            Assert.AreEqual($"parameter name {parameterName} in {KnownFeatureCategories.Gates}/{gate.Name}/{parameterName} cannot be found in the FM model.",
                ex.Message, "The exception message is different than expected");
        }
    }
}
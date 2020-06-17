using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        [Test]
        public void GetDataItemsByItemString_ReturnsExpectedDataItem()
        {
            // Given
            var fmModel = new WaterFlowFMModel();
            const string gateName = "structure01";
            var gate1 = new Weir2D
            {
                Name = gateName,
                WeirFormula = new GatedWeirFormula()
            };
            fmModel.Area.Weirs.Add(gate1);
            var gate2 = new Weir2D
            {
                Name = gateName,
                WeirFormula = new GatedWeirFormula()
            };
            fmModel.Area.Weirs.Add(gate2);

            // When
            var itemStringComponents = new List<string>(new[]
            {
                KnownFeatureCategories.Gates,
                gateName,
                KnownStructureProperties.CrestLevel
            });
            string itemString = string.Join("/", itemStringComponents);

            IEnumerable<IDataItem> dataItems = fmModel.GetDataItemsByItemString(itemString).ToArray();

            // Then
            Assert.That(dataItems.Count(), Is.EqualTo(2));
            AssertDataItemIsGate(dataItems.ElementAt(0), gate1);
            AssertDataItemIsGate(dataItems.ElementAt(1), gate2);
        }

        private static void AssertDataItemIsGate(IDataItem dataItem, Weir2D gate)
        {
            const string messageDifferentFeatureInDataItem = "The retrieved dataItem is not correct, since the features are not the same";
            const string messageDifferentParameterInDataItem = "The retrieved dataItem is not correct, since the parameters are not the same";

            var dataItemParameterConverter = ((ParameterValueConverter) dataItem.ValueConverter);

            Assert.That(dataItemParameterConverter.Location, Is.EqualTo(gate), messageDifferentFeatureInDataItem);
            Assert.That(dataItem.Name, Is.EqualTo(gate.Name), messageDifferentFeatureInDataItem);
            Assert.That(dataItemParameterConverter.ParameterName, Is.EqualTo(KnownStructureProperties.CrestLevel), messageDifferentParameterInDataItem);
            Assert.That(dataItem.Tag, Is.EqualTo(KnownStructureProperties.CrestLevel), messageDifferentParameterInDataItem);
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

            void Call() => fmModel.GetDataItemsByItemString(itemString);

            // Then
            var ex = Assert.Throws<ArgumentException>(Call);
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

            void Call() => fmModel.GetDataItemsByItemString(itemString);

            // Then
            var ex = Assert.Throws<ArgumentException>(Call);
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

            void Call() => fmModel.GetDataItemsByItemString(itemString);

            // Then
            var ex = Assert.Throws<ArgumentException>(Call);
            Assert.AreEqual($"parameter name {parameterName} in {KnownFeatureCategories.Gates}/{gate.Name}/{parameterName} cannot be found in the FM model.",
                            ex.Message, "The exception message is different than expected");
        }

        [Test]
        public void PrepareForIntegratedModelRun_SetsCacheFileToTheCorrectExplicitWorkingDirectory()
        {
            const string explicitWorkingDirectory = @"Explicit\Working\Directory\dflowfm";
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                model.ExplicitWorkingDirectory = explicitWorkingDirectory;

                // Call
                model.PrepareForIntegratedModelRun();

                string expectedPath = Path.Combine(explicitWorkingDirectory,
                                                   Path.ChangeExtension(model.Name,
                                                                        FileConstants.CachingFileExtension));

                // Assert
                Assert.That(model.CacheFile.Path, Is.EqualTo(expectedPath), "Expected a different path:");
            }
        }
    }
}
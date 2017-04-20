using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors
{
    [TestFixture]
    public class EditTracersControlHelperTest
    {
        [Test]
        [TestCase("", null, false, "No name entered")]
        [TestCase("TracerTestName", null, true,"")]
        [TestCase("1T", null, false, "The name '1T' starts with a number")]
        [TestCase("T1", null, true, "")]
        [TestCase("DuplicateTest", null, true, "")]
        [TestCase("DuplicateTest", new[] { "DuplicateTest" }, false, "The name 'DuplicateTest' is already defined")]
        [TestCase("123/", null, false, "The name '123/' starts with a number\n\rThe name '123/', cannot contain spaces or (back-)slashes")]
        [TestCase("asdf\\", null, false, "The name 'asdf\\', cannot contain spaces or (back-)slashes")]
        [TestCase("-Name", null, true, "")]
        [TestCase(WaterFlowFMModelDefinition.BathymetryDataItemName, null, false, "The name 'Bed Level' cannot be a known default name\n\rThe name 'Bed Level', cannot contain spaces or (back-)slashes")]
        public void TracerNameValidationTest(string name, ICollection<string> definedNames, bool expectedResult, string expectedErrorMessage)
        {
            definedNames = definedNames ?? new Collection<string>();
            var testCase = new TracerDefinitionsEditor.TracerAddedEventArgs(name);

            string errorMessage;
            var isNameValid = EditTracersControlHelper.IsNameValid(testCase.Name, out errorMessage, WaterFlowFMModelDefinition.SpatialDataItemNames, definedNames);

            Assert.AreEqual(expectedResult, isNameValid);

            if (isNameValid) return;
            Assert.AreEqual(expectedErrorMessage, errorMessage);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class MathematicalExpressionTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var mathematicalExpression = new MathematicalExpression();

            // Assert
            Assert.That(mathematicalExpression.Inputs, Is.Not.Null);
            Assert.That(mathematicalExpression.Inputs, Is.Empty);
            Assert.That(mathematicalExpression.InputMapping, Is.Not.Null);
            Assert.That(mathematicalExpression.InputMapping, Is.Empty);
            Assert.That(mathematicalExpression.Expression, Is.EqualTo(string.Empty));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddInput_UpdatesInputMappingCorrectly()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();

            const string inputName = "input_name";
            IInput input = Substitute.For<IInput, INotifyPropertyChange>();
            input.Name = inputName;

            // Call
            mathematicalExpression.Inputs.Add(input);

            // Assert
            IReadOnlyDictionary<char, string> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(1));

            const char expectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(expectedKey));
            Assert.That(mapping[expectedKey], Is.EqualTo(inputName));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddSecondInput_UpdatesInputMappingCorrectly()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();

            const string firstInputName = "input_name_1";
            IInput firstInput = Substitute.For<IInput, INotifyPropertyChange>();
            firstInput.Name = firstInputName;

            mathematicalExpression.Inputs.Add(firstInput);

            const string secondInputName = "input_name_2";
            IInput secondInput = Substitute.For<IInput, INotifyPropertyChange>();
            secondInput.Name = secondInputName;

            // Call
            mathematicalExpression.Inputs.Add(secondInput);

            // Assert
            IReadOnlyDictionary<char, string> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(2));

            const char firstExpectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(firstExpectedKey));
            Assert.That(mapping[firstExpectedKey], Is.EqualTo(firstInputName));

            const char secondExpectedKey = 'B';
            Assert.That(existingKeys[1], Is.EqualTo(secondExpectedKey));
            Assert.That(mapping[secondExpectedKey], Is.EqualTo(secondInputName));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddMultipleInputs_UpdatesInputMappingCorrectly()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();

            IList<KeyValuePair<char, string>> expectedKeyValuePairs = new List<KeyValuePair<char, string>>();
            for (var i = 0; i <= 26; i++)
            {
                string inputName = $"input_name_{i}";
                IInput input = Substitute.For<IInput, INotifyPropertyChange>();
                input.Name = inputName;

                char newKey = ToChar(i);
                expectedKeyValuePairs.Add(new KeyValuePair<char, string>(newKey, inputName));

                // Call
                mathematicalExpression.Inputs.Add(input);

                // Assert
                CollectionAssert.AreEqual(mathematicalExpression.InputMapping, expectedKeyValuePairs);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddSecondInput_WithSameName_UpdatesInputMappingCorrectly()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();

            const string inputName = "input_name";
            IInput firstInput = Substitute.For<IInput, INotifyPropertyChange>();
            firstInput.Name = inputName;

            mathematicalExpression.Inputs.Add(firstInput);

            IInput secondInput = Substitute.For<IInput, INotifyPropertyChange>();
            secondInput.Name = inputName;

            // Call
            mathematicalExpression.Inputs.Add(secondInput);

            // Assert
            IReadOnlyDictionary<char, string> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(1));

            const char expectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(expectedKey));
            Assert.That(mapping[expectedKey], Is.EqualTo(inputName));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RemoveInput_UpdatesMappingCorrectly()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();

            const string firstInputName = "input_name_1";
            IInput firstInput = Substitute.For<IInput, INotifyPropertyChange>();
            firstInput.Name = firstInputName;

            const string secondInputName = "input_name_2";
            IInput secondInput = Substitute.For<IInput, INotifyPropertyChange>();
            secondInput.Name = secondInputName;

            mathematicalExpression.Inputs.Add(firstInput);
            mathematicalExpression.Inputs.Add(secondInput);

            // Call
            mathematicalExpression.Inputs.Remove(firstInput);

            // Assert
            IReadOnlyDictionary<char, string> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(1));

            const char expectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(expectedKey));
            Assert.That(mapping[expectedKey], Is.EqualTo(secondInputName));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RemoveInput_WhenSameNameStillExists_MappingShouldNotBeRemoved()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();

            const string inputName = "input_name";
            IInput firstInput = Substitute.For<IInput, INotifyPropertyChange>();
            firstInput.Name = inputName;

            IInput secondInput = Substitute.For<IInput, INotifyPropertyChange>();
            secondInput.Name = inputName;

            mathematicalExpression.Inputs.Add(firstInput);
            mathematicalExpression.Inputs.Add(secondInput);

            // Call
            mathematicalExpression.Inputs.Remove(secondInput);

            // Assert
            IReadOnlyDictionary<char, string> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(1));

            const char expectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(expectedKey));
            Assert.That(mapping[expectedKey], Is.EqualTo(inputName));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [TestCaseSource(nameof(GetInputsTestCases))]
        public void RenameInput_ToUniqueName_UpdatesMappingCorrectly(IInput firstInput)
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();

            const string originalInputName = "input_name";
            firstInput.Name = originalInputName;

            IInput secondInput = Substitute.For<IInput, INotifyPropertyChange>();
            secondInput.Name = originalInputName;

            mathematicalExpression.Inputs.Add(firstInput);
            mathematicalExpression.Inputs.Add(secondInput);

            const string newInputName = "new_input_name";

            // Call
            firstInput.Name = newInputName;

            // Assert
            IReadOnlyDictionary<char, string> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(2));

            const char firstExpectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(firstExpectedKey));
            Assert.That(mapping[firstExpectedKey], Is.EqualTo(newInputName));

            const char secondExpectedKey = 'B';
            Assert.That(existingKeys[1], Is.EqualTo(secondExpectedKey));
            Assert.That(mapping[secondExpectedKey], Is.EqualTo(originalInputName));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [TestCaseSource(nameof(GetInputsTestCases))]
        public void RenameInput_ToDuplicateName_UpdatesMappingCorrectly(IInput firstInput)
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();

            const string firstInputName = "input_name_1";
            firstInput.Name = firstInputName;

            const string secondInputName = "input_name_2";
            IInput secondInput = Substitute.For<IInput, INotifyPropertyChange>();
            secondInput.Name = secondInputName;

            mathematicalExpression.Inputs.Add(firstInput);
            mathematicalExpression.Inputs.Add(secondInput);

            // Call
            firstInput.Name = secondInputName;

            // Assert
            IReadOnlyDictionary<char, string> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(1));

            const char firstExpectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(firstExpectedKey));
            Assert.That(mapping[firstExpectedKey], Is.EqualTo(secondInputName));
        }

        [Test]
        public void CopyFrom_MathematicalExpression_ReturnsCorrectResult()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();

            const string sourceName = "expression_name";
            const string sourceLongName = "expression_long_name";
            var sourceExpression = new MathematicalExpression
            {
                Name = sourceName,
                LongName = sourceLongName
            };

            // Call
            mathematicalExpression.CopyFrom(sourceExpression);

            // Assert
            Assert.That(mathematicalExpression.Name, Is.EqualTo(sourceName));
            Assert.That(mathematicalExpression.LongName, Is.EqualTo(sourceLongName));
        }

        [Test]
        public void CopyFrom_NotMathematicalExpression_ReturnsCorrectResult()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();
            string originalName = mathematicalExpression.Name;
            string originalLongName = mathematicalExpression.LongName;

            var sourceObj = Substitute.For<RtcBaseObject>();
            sourceObj.Name = "expression_name";
            sourceObj.LongName = "expression_long_name";

            // Call
            mathematicalExpression.CopyFrom(sourceObj);

            // Assert
            Assert.That(mathematicalExpression.Name, Is.EqualTo(originalName));
            Assert.That(mathematicalExpression.LongName, Is.EqualTo(originalLongName));
        }

        [Test]
        public void Clone_ReturnsCorrectResult()
        {
            // Setup
            const string name = "expression_name";
            const string longName = "expression_long_name";
            var mathematicalExpression = new MathematicalExpression
            {
                Name = name,
                LongName = longName
            };

            // Call
            object result = mathematicalExpression.Clone();

            // Assert
            var clonedExpression = result as MathematicalExpression;
            Assert.That(clonedExpression, Is.Not.Null);
            Assert.That(clonedExpression, Is.Not.SameAs(mathematicalExpression));
            Assert.That(clonedExpression.Name, Is.EqualTo(name));
            Assert.That(clonedExpression.LongName, Is.EqualTo(longName));
        }

        private static IEnumerable<IInput> GetInputsTestCases()
        {
            yield return new Input();
            yield return new MathematicalExpression();
        }

        private static char ToChar(int i)
        {
            return Convert.ToChar(i + 65);
        }
    }
}
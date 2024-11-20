using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
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
        public void AddInput_UpdatesInputToCharMappingCorrectly()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();

            const string inputName = "input_name";
            IInput input = Substitute.For<IInput, INotifyPropertyChange>();
            input.Name = inputName;

            // Call
            mathematicalExpression.Inputs.Add(input);

            // Assert
            IReadOnlyDictionary<char, IInput> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(1));

            const char expectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(expectedKey));
            Assert.That(mapping[expectedKey].Name, Is.EqualTo(inputName));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddSecondInput_UpdatesInputToCharMappingCorrectly()
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
            IReadOnlyDictionary<char, IInput> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(2));

            const char firstExpectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(firstExpectedKey));
            Assert.That(mapping[firstExpectedKey].Name, Is.EqualTo(firstInputName));

            const char secondExpectedKey = 'B';
            Assert.That(existingKeys[1], Is.EqualTo(secondExpectedKey));
            Assert.That(mapping[secondExpectedKey].Name, Is.EqualTo(secondInputName));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddMultipleInputs_UpdatesInputToCharMappingCorrectly()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();

            IList<KeyValuePair<char, string>> expectedKeyValuePairs = new List<KeyValuePair<char, string>>();
            for (var i = 0; i <= 26; i++)
            {
                var inputName = $"input_name_{i}";
                IInput input = Substitute.For<IInput, INotifyPropertyChange>();
                input.Name = inputName;

                char newKey = ToChar(i);
                expectedKeyValuePairs.Add(new KeyValuePair<char, string>(newKey, inputName));

                // Call
                mathematicalExpression.Inputs.Add(input);

                // Assert
                IEnumerable<KeyValuePair<char, string>> keyValuePairs = mathematicalExpression.InputMapping.Select( im => new KeyValuePair<char, string>(im.Key, im.Value.Name));
                CollectionAssert.AreEqual(keyValuePairs, expectedKeyValuePairs);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddSecondInput_WithSameName_UpdatesInputToCharMappingCorrectly()
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
            IReadOnlyDictionary<char, IInput> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(2));

            const char firstExpectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(firstExpectedKey));
            Assert.That(mapping[firstExpectedKey], Is.EqualTo(firstInput));
            const char secondExpectedKey = 'B';
            Assert.That(existingKeys[1], Is.EqualTo(secondExpectedKey));
            Assert.That(mapping[secondExpectedKey], Is.EqualTo(secondInput));
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
            IReadOnlyDictionary<char, IInput> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(1));

            const char expectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(expectedKey));
            Assert.That(mapping[expectedKey].Name, Is.EqualTo(secondInputName));
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
            IReadOnlyDictionary<char, IInput> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(1));

            const char expectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(expectedKey));
            Assert.That(mapping[expectedKey].Name, Is.EqualTo(inputName));
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
            IReadOnlyDictionary<char, IInput> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(2));

            const char firstExpectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(firstExpectedKey));
            Assert.That(mapping[firstExpectedKey].Name, Is.EqualTo(newInputName));

            const char secondExpectedKey = 'B';
            Assert.That(existingKeys[1], Is.EqualTo(secondExpectedKey));
            Assert.That(mapping[secondExpectedKey].Name, Is.EqualTo(originalInputName));
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
            IReadOnlyDictionary<char, IInput> mapping = mathematicalExpression.InputMapping;
            char[] existingKeys = mapping.Keys.ToArray();
            Assert.That(existingKeys, Has.Length.EqualTo(2));

            const char firstExpectedKey = 'A';
            Assert.That(existingKeys[0], Is.EqualTo(firstExpectedKey));
            Assert.That(mapping[firstExpectedKey].Name, Is.EqualTo(secondInputName));
        }

        [Test]
        public void SetInputs_WithSameValue_ShouldNotReceiveNewSubscriptions()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();
            var inputs = Substitute.For<IEventedList<IInput>>();
            mathematicalExpression.Inputs = inputs;

            inputs.ClearReceivedCalls();

            // Call
            void Call() => mathematicalExpression.Inputs = inputs;

            // Assert
            Assert.DoesNotThrow(Call);
            inputs.DidNotReceiveWithAnyArgs().CollectionChanged += Arg.Any<NotifyCollectionChangedEventHandler>();
            inputs.DidNotReceiveWithAnyArgs().CollectionChanged -= Arg.Any<NotifyCollectionChangedEventHandler>();
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SetInputs_UpdatesInputToCharMappingCorrectly()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();

            const string inputName = "input_name";
            IInput input = Substitute.For<IInput, INotifyPropertyChange>();
            input.Name = inputName;

            // Call
            mathematicalExpression.Inputs = new EventedList<IInput>(new[]
            {
                input
            });

            // Assert
            Assert.That(mathematicalExpression.InputMapping, Has.Count.EqualTo(1));

            var expectedEntry = new KeyValuePair<char, IInput>('A', input);
            Assert.That(mathematicalExpression.InputMapping, Has.Member(expectedEntry));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [TestCaseSource(nameof(GetInputsTestCases))]
        public void RenameInput_AfterSetInputs_UpdatesInputToCharMappingCorrectly(IInput input)
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression
            {
                Name = "input_name_1",
                Inputs = new EventedList<IInput>(new[]
                {
                    input
                })
            };

            // Call
            const string secondInputName = "input_name_2";
            input.Name = secondInputName;

            // Assert
            Assert.That(mathematicalExpression.InputMapping, Has.Count.EqualTo(1));

            var expectedEntry = new KeyValuePair<char, string>('A', secondInputName);
            Assert.That(mathematicalExpression.InputMapping.Select( im => new KeyValuePair<char, string>(im.Key, im.Value.Name)), Has.Member(expectedEntry));
        }

        [Test]
        public void CopyFrom_MathematicalExpression_ReturnsCorrectResult()
        {
            // Setup
            var mathematicalExpression = new MathematicalExpression();
            const string mathExpression = "A * B";
            const string sourceName = "expression_name";
            const string sourceLongName = "expression_long_name";
            var sourceExpression = new MathematicalExpression
            {
                Name = sourceName,
                LongName = sourceLongName,
                Expression = mathExpression
            };

            // Call
            mathematicalExpression.CopyFrom(sourceExpression);

            // Assert
            Assert.That(mathematicalExpression.Name, Is.EqualTo(sourceName));
            Assert.That(mathematicalExpression.LongName, Is.EqualTo(sourceLongName));
            Assert.That(mathematicalExpression.Expression, Is.EqualTo(mathExpression));
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
            const string expression = "A * B";
            const string name = "expression_name";
            const string longName = "expression_long_name";
            var mathematicalExpression = new MathematicalExpression
            {
                Name = name,
                LongName = longName,
                Expression = expression
            };

            // Call
            object result = mathematicalExpression.Clone();

            // Assert
            var clonedExpression = result as MathematicalExpression;
            Assert.That(clonedExpression, Is.Not.Null);
            Assert.That(clonedExpression, Is.Not.SameAs(mathematicalExpression));
            Assert.That(clonedExpression.Name, Is.EqualTo(name));
            Assert.That(clonedExpression.LongName, Is.EqualTo(longName));
            Assert.That(clonedExpression.Expression, Is.EqualTo(expression));
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
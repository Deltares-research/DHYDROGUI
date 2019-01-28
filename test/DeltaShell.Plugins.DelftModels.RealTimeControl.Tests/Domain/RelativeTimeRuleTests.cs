using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class RelativeTimeRuleTests
    {
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";

        private const string Name = "Relative time rule";
        private Output output;
        private const string OutputParameterName = "output parameter";
        private const string OutputName = "output name";

        private Function tableFunction;

        [SetUp]
        public void SetUp()
        {
            tableFunction = RelativeTimeRule.DefineFunction();
            tableFunction[0.0] = 1.2;
            tableFunction[60.0] = 3.4;
            tableFunction[120.0] = 5.6;
            tableFunction[180.0] = 7.8;
            output = new Output
            {
                ParameterName = OutputParameterName,
                Feature = new RtcTestFeature { Name = OutputName },
            };
        }

        [Test]
        public void CheckXmlGenerationAbsolute()
        {
            var relativeTimeRule = new RelativeTimeRule
            {
                Name = Name,
                //Inputs = new EventedList<Input> { input },
                Outputs = new EventedList<Output> { output },
                FromValue = false,
                Function = tableFunction
            };
            var xmlAbsolute = "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<timeRelative id=\"[RelativeTimeRule]Relative time rule\">" +
                   "<mode>RETAINVALUEWHENINACTIVE</mode>" +
                   "<valueOption>ABSOLUTE</valueOption>" +  // RelativeTimeseries is ABSOLUTE; RelativeTimeseries is RELATIVE
                   "<maximumPeriod>0</maximumPeriod>" +
                   "<controlTable>" +
                   "<record time=\"0\" value=\"1.2\" />" +
                   "<record time=\"60\" value=\"3.4\" />" +
                   "<record time=\"120\" value=\"5.6\" />" +
                   "<record time=\"180\" value=\"7.8\" />" +
                   "<record time=\"181\" value=\"7.8\" />" + // see RelativeTimeRule::GetTable
                   "</controlTable>" +
                   "<output>" +
                   "<y>" + RtcXmlTag.Output + "output name/output parameter</y>" +
                   "<timeActive>Relative time rule</timeActive>" +
                   "</output>" +
                   "</timeRelative>" +
                   "</rule>";

            var xDocument = relativeTimeRule.ToXml(Fns, "");
            Assert.AreEqual(xmlAbsolute, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationRelativeFromValue()
        {
            var relativeTimeRule = new RelativeTimeRule
            {
                Name = Name,
                Outputs = new EventedList<Output> { output },
                FromValue = true,
                Function = tableFunction
            };
            var xmlRelative = "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<timeRelative id=\"[RelativeTimeRule]Relative time rule\">" +
                   "<mode>RETAINVALUEWHENINACTIVE</mode>" +
                   "<valueOption>RELATIVE</valueOption>" +  // RelativeTimeseries is ABSOLUTE; RelativeTimeseries is RELATIVE
                   "<maximumPeriod>0</maximumPeriod>" +
                   "<controlTable>" +
                   "<record time=\"0\" value=\"1.2\" />" +
                   "<record time=\"60\" value=\"3.4\" />" +
                   "<record time=\"120\" value=\"5.6\" />" +
                   "<record time=\"180\" value=\"7.8\" />" +
                   "<record time=\"181\" value=\"7.8\" />" + // see RelativeTimeRule::GetTable
                   "</controlTable>" +
                   "<input>" +
                   "<y>" + RtcXmlTag.Output + "output name/output parameter[AsInputFor]Relative time rule</y>" +
                   "</input>" +
                   "<output>" +
                   "<y>" + RtcXmlTag.Output + "output name/output parameter</y>" +
                   "<timeActive>Relative time rule</timeActive>" +
                   "</output>" +
                   "</timeRelative>" +
                   "</rule>";

            var xDocument = relativeTimeRule.ToXml(Fns, "");
            Assert.AreEqual(xmlRelative, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CopyFromAndClone()
        {
            var source = new RelativeTimeRule
            {
                Name = "test",
                FromValue = false,
                Interpolation = InterpolationType.Linear
            };

            var newRule = new RelativeTimeRule();
            var argumentValues = new[] { 60, 120.0, 360.0 };
            var componentValues = new[] { 8.0, 9.0, 10.0 };
            for (var i = 0; i < argumentValues.Count(); i++)
            {
                source.Function[argumentValues[i]] = componentValues[i];
            }
            newRule.CopyFrom(source);

            Assert.AreEqual(source.Name, newRule.Name);
            for (var i = 0; i < source.Function.Arguments[0].Values.Count; i++)
            {
                Assert.AreEqual(source.Function.Arguments[0].Values[i], newRule.Function.Arguments[0].Values[i]);
                Assert.AreEqual(source.Function.Components[0].Values[i], newRule.Function.Components[0].Values[i]);
            }
            Assert.AreEqual(source.FromValue, newRule.FromValue);
            Assert.AreEqual(source.Interpolation, newRule.Interpolation);
            
            var clone = (RelativeTimeRule)source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }

        /// <summary>
        /// GIVEN a RelativeTimeRule with a table containing no entries
        /// WHEN ToXml is called
        /// THEN the controlTable does not contain any entries
        /// </summary>
        [Test]
        public void GivenATableContainingNoEntries_WhenToXmlIsCalled_ThenTheControlTableDoesNotContainAnyEntries()
        {
            // Given
            var rule = new RelativeTimeRule();

            // When
            var resultDocument = rule.ToXml(Fns, "");

            // Then
            Assert.That(resultDocument, Is.Not.Null, "Expected result not to be null.");
            Assert.That(resultDocument.Elements().Any(), Is.True, "Expected at least on element of the result to exist.");
            var resultRule = resultDocument.Elements().FirstOrDefault();

            Assert.That(resultRule, Is.Not.Null, "Expected the rule element to exist.");
            var elements = resultRule.Elements().ToList();

            var xmlControlTable = elements.FirstOrDefault(e => e.Name.LocalName == "controlTable");
            Assert.That(xmlControlTable, Is.Not.Null, "Expected the rule to have a controlTable element.");
            Assert.That(xmlControlTable.HasElements, Is.False, "Expected the controlTable of the rule to be empty.");
        }

        /// <summary>
        /// GIVEN a RelativeTimeRule with a table containing a single entry
        /// WHEN ToXml is called
        /// THEN the controlTable contains the correct entries
        /// </summary>
        [Test]
        public void GivenATableContainingASingleEntry_WhenToXmlIsCalled_ThenTheControlTableContainsTheCorrectEntries()
        {
            // Given
            const double expectedArgVal = 5.0;
            const double expectedCompVal = 120.0;

            var rule = new RelativeTimeRule();
            rule.Function.Arguments[0].AddValues(new List<double>() { expectedArgVal });
            rule.Function.Components[0].Values[0] = expectedCompVal;

            // When
            var resultDocument = rule.ToXml(Fns, "");

            // Then
            Assert.That(resultDocument, Is.Not.Null, "Expected result not to be null.");
            Assert.That(resultDocument.Elements().Any(), Is.True, "Expected at least on element of the result to exist.");
            var resultRule = resultDocument.Elements().FirstOrDefault();

            Assert.That(resultRule, Is.Not.Null, "Expected the rule element to exist.");
            var elements = resultRule.Elements().ToList();

            var xmlControlTable = elements.FirstOrDefault(e => e.Name.LocalName == "controlTable");
            Assert.That(xmlControlTable, Is.Not.Null, "Expected the rule to have a controlTable element.");

            var tableElements = xmlControlTable.Elements();
            Assert.That(tableElements, Is.Not.Null, "Expected the controlTable of the rule to have elements.");
            var tableElementsList = tableElements.ToList();
            Assert.That(tableElementsList.Count, Is.EqualTo(2), "Expected the controlTable to have two elements.");

            AssertThatControlTableElementHasCorrectValues(tableElementsList[0], expectedArgVal, expectedCompVal);
            AssertThatControlTableElementHasCorrectValues(tableElementsList[1], expectedArgVal + 1.0, expectedCompVal);
        }

        private static void AssertThatControlTableElementHasCorrectValues(XElement element, double expectedArgVal, double expectedCompVal)
        {
            Assert.That(element, Is.Not.Null, 
                        "Expected the elements in controlTable to not be null.");
            var attributes = element.Attributes();
            Assert.That(attributes, Is.Not.Null, 
                        "Expected attributes of the elements of controlTable to not be null.");
            var attributesList = attributes.ToList();

            var timeAttribute = attributesList.FirstOrDefault(att => att.Name.LocalName == "time");
            Assert.That(timeAttribute, Is.Not.Null, 
                        "Expected elements to have a time attribute.");
            Assert.That(timeAttribute.Value, Is.EqualTo(expectedArgVal.ToString()),
                        "Expected time attribute to match with original value:");

            var valueAttribute = attributesList.FirstOrDefault(att => att.Name.LocalName == "value");
            Assert.That(valueAttribute, Is.Not.Null, 
                        "Expected elements to have a value attribute.");
            Assert.That(valueAttribute.Value, Is.EqualTo(expectedCompVal.ToString()),
                        "Expected value attribute to match with original value:");
        }

        /// <summary>
        /// GIVEN a RelativeTimeRule with a table containing multiple unique Y value entries
        /// WHEN ToXml is called
        /// THEN the controlTable contains the correct entries
        /// </summary>
        [Test]
        public void GivenATableContainingMultipleUniqueYValueEntries_WhenToXmlIsCalled_ThenTheControlTableContainsTheCorrectEntries()
        {
            // Given
            var expectedArgVals = new List<double>() {5.0, 10.0, 20.0, 40.0};
            var expectedCompVals = new List<double>() { 7.0, 12.0, 22.0, 42.0 };
          

            var rule = new RelativeTimeRule();
            rule.Function.Arguments[0].AddValues(expectedArgVals);

            for (var i = 0; i < expectedCompVals.Count; i++)
                rule.Function.Components[0].Values[i] = expectedCompVals[i];

            // When
            var resultDocument = rule.ToXml(Fns, "");

            // Then
            Assert.That(resultDocument, Is.Not.Null, "Expected result not to be null.");
            Assert.That(resultDocument.Elements().Any(), Is.True, "Expected at least on element of the result to exist.");
            var resultRule = resultDocument.Elements().FirstOrDefault();

            Assert.That(resultRule, Is.Not.Null, "Expected the rule element to exist.");
            var elements = resultRule.Elements().ToList();

            var xmlControlTable = elements.FirstOrDefault(e => e.Name.LocalName == "controlTable");
            Assert.That(xmlControlTable, Is.Not.Null, "Expected the rule to have a controlTable element.");

            var tableElements = xmlControlTable.Elements();
            Assert.That(tableElements, Is.Not.Null, "Expected the controlTable of the rule to have elements.");
            var tableElementsList = tableElements.ToList();

            var expectedNumberOfElements = expectedArgVals.Count + 1;
            Assert.That(tableElementsList.Count, Is.EqualTo(expectedNumberOfElements), "Expected the controlTable to have a different number of elements.");

            for (var i = 0; i < expectedArgVals.Count; i++)
                AssertThatControlTableElementHasCorrectValues(tableElementsList[i], 
                                                              expectedArgVals[i], 
                                                              expectedCompVals[i]);

            // Assert that last element is equal to the before last element.
            AssertThatControlTableElementHasCorrectValues(tableElementsList.Last(), 
                                                          expectedArgVals.Last() + 1,
                                                          expectedCompVals.Last());
        }

        /// <summary>
        /// GIVEN a RelativeTimeRule with a table containing two consecutive Y values entries
        /// WHEN ToXml is called
        /// THEN the controlTable contains the correct entries
        /// </summary>
        [TestCase(1.0)]
        [TestCase(100.0)]
        public void GivenATableContainingTwoConsecutiveYValuesEntries_WhenToXmlIsCalled_ThenTheControlTableContainsTheCorrectEntries(double timeBetweenYValues)
        {
            // Given
            var expectedArgVals = new List<double>() { 5.0, 10.0, 20.0, 40.0, 40.0 + timeBetweenYValues };
            var expectedCompVals = new List<double>() { 7.0, 12.0, 22.0, 42.0, 42.0 };


            var rule = new RelativeTimeRule();
            rule.Function.Arguments[0].AddValues(expectedArgVals);

            for (var i = 0; i < expectedCompVals.Count; i++)
                rule.Function.Components[0].Values[i] = expectedCompVals[i];

            // When
            var resultDocument = rule.ToXml(Fns, "");

            // Then
            Assert.That(resultDocument, Is.Not.Null, "Expected result not to be null.");
            Assert.That(resultDocument.Elements().Any(), Is.True, "Expected at least on element of the result to exist.");
            var resultRule = resultDocument.Elements().FirstOrDefault();

            Assert.That(resultRule, Is.Not.Null, "Expected the rule element to exist.");
            var elements = resultRule.Elements().ToList();

            var xmlControlTable = elements.FirstOrDefault(e => e.Name.LocalName == "controlTable");
            Assert.That(xmlControlTable, Is.Not.Null, "Expected the rule to have a controlTable element.");

            var tableElements = xmlControlTable.Elements();
            Assert.That(tableElements, Is.Not.Null, "Expected the controlTable of the rule to have elements.");
            var tableElementsList = tableElements.ToList();

            var expectedNumberOfElements = expectedArgVals.Count;
            Assert.That(tableElementsList.Count, Is.EqualTo(expectedNumberOfElements), "Expected the controlTable to have a different number of elements.");

            for (var i = 0; i < expectedArgVals.Count; i++)
                AssertThatControlTableElementHasCorrectValues(tableElementsList[i],
                                                              expectedArgVals[i],
                                                              expectedCompVals[i]);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Export
{
    [TestFixture]
    public class RelativeTimRuleSerializerTest
    {
        private const string name = "Relative time rule";
        private const string outputParameterName = "output parameter";
        private const string outputName = "output name";
        private static readonly XNamespace fns = "http://www.wldelft.nl/fews";
        private Output output;

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
                ParameterName = outputParameterName,
                Feature = new RtcTestFeature {Name = outputName}
            };
        }

        [Test]
        public void CheckXmlGenerationAbsolute()
        {
            var relativeTimeRule = new RelativeTimeRule
            {
                Name = name,
                Outputs = new EventedList<Output> {output},
                FromValue = false,
                Function = tableFunction
            };
            string xmlAbsolute = "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                                 "<timeRelative id=\"[RelativeTimeRule]Relative time rule\">" +
                                 "<mode>RETAINVALUEWHENINACTIVE</mode>" +
                                 "<valueOption>ABSOLUTE</valueOption>" + // RelativeTimeseries is ABSOLUTE; RelativeTimeseries is RELATIVE
                                 "<maximumPeriod>0</maximumPeriod>" +
                                 "<interpolationOption>BLOCK</interpolationOption>" +
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

            var serializer = new RelativeTimeRuleSerializer(relativeTimeRule);

            XElement xDocument = serializer.ToXml(fns, "").Single();
            Assert.AreEqual(xmlAbsolute, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationRelativeFromValue()
        {
            var relativeTimeRule = new RelativeTimeRule
            {
                Name = name,
                Outputs = new EventedList<Output> {output},
                FromValue = true,
                Function = tableFunction
            };
            const string xmlRelative = "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                                       "<timeRelative id=\"[RelativeTimeRule]Relative time rule\">" +
                                       "<mode>RETAINVALUEWHENINACTIVE</mode>" +
                                       "<valueOption>RELATIVE</valueOption>" + // RelativeTimeseries is ABSOLUTE; RelativeTimeseries is RELATIVE
                                       "<maximumPeriod>0</maximumPeriod>" +
                                       "<interpolationOption>BLOCK</interpolationOption>" +
                                       "<controlTable>" +
                                       "<record time=\"0\" value=\"1.2\" />" +
                                       "<record time=\"60\" value=\"3.4\" />" +
                                       "<record time=\"120\" value=\"5.6\" />" +
                                       "<record time=\"180\" value=\"7.8\" />" +
                                       "<record time=\"181\" value=\"7.8\" />" + // see RelativeTimeRule::GetTable
                                       "</controlTable>" +
                                       "<input>" +
                                       "<y>" + RtcXmlTag.Output +
                                       "output name/output parameter[AsInputFor]Relative time rule</y>" +
                                       "</input>" +
                                       "<output>" +
                                       "<y>" + RtcXmlTag.Output + "output name/output parameter</y>" +
                                       "<timeActive>Relative time rule</timeActive>" +
                                       "</output>" +
                                       "</timeRelative>" +
                                       "</rule>";

            var serializer = new RelativeTimeRuleSerializer(relativeTimeRule);

            XElement xDocument = serializer.ToXml(fns, "").Single();
            Assert.AreEqual(xmlRelative, xDocument.ToString(SaveOptions.DisableFormatting));
        }

        /// <summary>
        /// GIVEN a RelativeTimeRule with a table containing no entries
        /// WHEN ToXmlInputReference is called
        /// THEN the controlTable does not contain any entries
        /// </summary>
        [Test]
        public void GivenATableContainingNoEntries_WhenToXmlIsCalled_ThenTheControlTableDoesNotContainAnyEntries()
        {
            // Given
            var rule = new RelativeTimeRule();
            var serializer = new RelativeTimeRuleSerializer(rule);

            // When
            XElement resultDocument = serializer.ToXml(fns, "").Single();

            // Then
            Assert.That(resultDocument, Is.Not.Null, "Expected result not to be null.");
            Assert.That(resultDocument.Elements().Any(), Is.True,
                        "Expected at least on element of the result to exist.");
            XElement resultRule = resultDocument.Elements().FirstOrDefault();

            Assert.That(resultRule, Is.Not.Null, "Expected the rule element to exist.");
            List<XElement> elements = resultRule.Elements().ToList();

            XElement xmlControlTable = elements.FirstOrDefault(e => e.Name.LocalName == "controlTable");
            Assert.That(xmlControlTable, Is.Not.Null, "Expected the rule to have a controlTable element.");
            Assert.That(xmlControlTable.HasElements, Is.False, "Expected the controlTable of the rule to be empty.");
        }

        /// <summary>
        /// GIVEN a RelativeTimeRule with a table containing a single entry
        /// WHEN ToXmlInputReference is called
        /// THEN the controlTable contains two entries with subsequent argument values and equal component values
        /// </summary>
        [Test]
        public void GivenATableContainingASingleEntry_WhenToXmlIsCalled_ThenTheControlTableContainsTwoEntries()
        {
            // Given
            const double expectedArgVal = 5.0;
            const double expectedCompVal = 120.0;

            var rule = new RelativeTimeRule();
            rule.Function.Arguments[0].AddValues(new List<double>() {expectedArgVal});
            rule.Function.Components[0].Values[0] = expectedCompVal;

            var serializer = new RelativeTimeRuleSerializer(rule);

            // When
            XElement resultDocument = serializer.ToXml(fns, "").Single();

            // Then
            Assert.That(resultDocument, Is.Not.Null, "Expected result not to be null.");
            Assert.That(resultDocument.Elements().Any(), Is.True,
                        "Expected at least on element of the result to exist.");
            XElement resultRule = resultDocument.Elements().FirstOrDefault();

            Assert.That(resultRule, Is.Not.Null, "Expected the rule element to exist.");
            List<XElement> elements = resultRule.Elements().ToList();

            XElement xmlControlTable = elements.FirstOrDefault(e => e.Name.LocalName == "controlTable");
            Assert.That(xmlControlTable, Is.Not.Null, "Expected the rule to have a controlTable element.");

            IEnumerable<XElement> tableElements = xmlControlTable.Elements();
            Assert.That(tableElements, Is.Not.Null, "Expected the controlTable of the rule to have elements.");
            List<XElement> tableElementsList = tableElements.ToList();
            Assert.That(tableElementsList.Count, Is.EqualTo(2), "Expected the controlTable to have two elements.");

            AssertThatControlTableElementHasCorrectValues(tableElementsList[0], expectedArgVal, expectedCompVal);
            AssertThatControlTableElementHasCorrectValues(tableElementsList[1], expectedArgVal + 1.0, expectedCompVal);
        }

        /// <summary>
        /// GIVEN a RelativeTimeRule with a table containing multiple unique Y value entries
        /// WHEN ToXmlInputReference is called
        /// THEN the controlTable contains these entries and a subsequent entry with an equal component
        /// </summary>
        [Test]
        public void
            GivenATableContainingMultipleUniqueYValueEntries_WhenToXmlIsCalled_ThenTheControlTableContainsTheseEntriesAndASubsequentElement()
        {
            // Given
            var expectedArgVals = new List<double>()
            {
                5.0,
                10.0,
                20.0,
                40.0
            };
            var expectedCompVals = new List<double>()
            {
                7.0,
                12.0,
                22.0,
                42.0
            };

            var rule = new RelativeTimeRule();
            rule.Function.Arguments[0].AddValues(expectedArgVals);

            for (var i = 0; i < expectedCompVals.Count; i++)
            {
                rule.Function.Components[0].Values[i] = expectedCompVals[i];
            }

            var serializer = new RelativeTimeRuleSerializer(rule);

            // When
            XElement resultDocument = serializer.ToXml(fns, "").Single();

            // Then
            Assert.That(resultDocument, Is.Not.Null, "Expected result not to be null.");
            Assert.That(resultDocument.Elements().Any(), Is.True,
                        "Expected at least on element of the result to exist.");
            XElement resultRule = resultDocument.Elements().FirstOrDefault();

            Assert.That(resultRule, Is.Not.Null, "Expected the rule element to exist.");
            List<XElement> elements = resultRule.Elements().ToList();

            XElement xmlControlTable = elements.FirstOrDefault(e => e.Name.LocalName == "controlTable");
            Assert.That(xmlControlTable, Is.Not.Null, "Expected the rule to have a controlTable element.");

            IEnumerable<XElement> tableElements = xmlControlTable.Elements();
            Assert.That(tableElements, Is.Not.Null, "Expected the controlTable of the rule to have elements.");
            List<XElement> tableElementsList = tableElements.ToList();

            int expectedNumberOfElements = expectedArgVals.Count + 1;
            Assert.That(tableElementsList.Count, Is.EqualTo(expectedNumberOfElements),
                        "Expected the controlTable to have a different number of elements.");

            for (var i = 0; i < expectedArgVals.Count; i++)
            {
                AssertThatControlTableElementHasCorrectValues(tableElementsList[i],
                                                              expectedArgVals[i],
                                                              expectedCompVals[i]);
            }

            // Assert that last element is equal to the before last element.
            AssertThatControlTableElementHasCorrectValues(tableElementsList.Last(),
                                                          expectedArgVals.Last() + 1,
                                                          expectedCompVals.Last());
        }

        /// <summary>
        /// GIVEN a RelativeTimeRule with a table containing two consecutive Y values entries
        /// WHEN ToXmlInputReference is called
        /// THEN the controlTable contains only the table entries
        /// </summary>
        [TestCase(1.0)]
        [TestCase(100.0)]
        public void
            GivenATableContainingTwoConsecutiveYValuesEntries_WhenToXmlIsCalled_ThenTheControlTableContainsOnlyTheTableEntries(
                double timeBetweenYValues)
        {
            // Given
            var expectedArgVals = new List<double>()
            {
                5.0,
                10.0,
                20.0,
                40.0,
                40.0 + timeBetweenYValues
            };
            var expectedCompVals = new List<double>()
            {
                7.0,
                12.0,
                22.0,
                42.0,
                42.0
            };

            var rule = new RelativeTimeRule();
            rule.Function.Arguments[0].AddValues(expectedArgVals);

            for (var i = 0; i < expectedCompVals.Count; i++)
            {
                rule.Function.Components[0].Values[i] = expectedCompVals[i];
            }

            var serializer = new RelativeTimeRuleSerializer(rule);

            // When
            XElement resultDocument = serializer.ToXml(fns, "").Single();

            // Then
            Assert.That(resultDocument, Is.Not.Null, "Expected result not to be null.");
            Assert.That(resultDocument.Elements().Any(), Is.True,
                        "Expected at least on element of the result to exist.");
            XElement resultRule = resultDocument.Elements().FirstOrDefault();

            Assert.That(resultRule, Is.Not.Null, "Expected the rule element to exist.");
            List<XElement> elements = resultRule.Elements().ToList();

            XElement xmlControlTable = elements.FirstOrDefault(e => e.Name.LocalName == "controlTable");
            Assert.That(xmlControlTable, Is.Not.Null, "Expected the rule to have a controlTable element.");

            IEnumerable<XElement> tableElements = xmlControlTable.Elements();
            Assert.That(tableElements, Is.Not.Null, "Expected the controlTable of the rule to have elements.");
            List<XElement> tableElementsList = tableElements.ToList();

            int expectedNumberOfElements = expectedArgVals.Count;
            Assert.That(tableElementsList.Count, Is.EqualTo(expectedNumberOfElements),
                        "Expected the controlTable to have a different number of elements.");

            for (var i = 0; i < expectedArgVals.Count; i++)
            {
                AssertThatControlTableElementHasCorrectValues(tableElementsList[i],
                                                              expectedArgVals[i],
                                                              expectedCompVals[i]);
            }
        }

        #region TestHelpers

        /// <summary>
        /// Assert the that control table element <paramref name="element"/> has the expected arg
        /// value <paramref name="expectedAcfVal"/> and expected component value
        /// <paramref name="expectedCompVal"/>.
        /// </summary>
        /// <param name="element"> The element to be checked. </param>
        /// <param name="expectedArgVal"> The expected argument value. </param>
        /// <param name="expectedCompVal"> The expected component value. </param>
        private static void AssertThatControlTableElementHasCorrectValues(
            XElement element, double expectedArgVal, double expectedCompVal)
        {
            Assert.That(element, Is.Not.Null,
                        "Expected the elements in controlTable to not be null.");
            IEnumerable<XAttribute> attributes = element.Attributes();
            Assert.That(attributes, Is.Not.Null,
                        "Expected attributes of the elements of controlTable to not be null.");
            List<XAttribute> attributesList = attributes.ToList();

            XAttribute timeAttribute = attributesList.FirstOrDefault(att => att.Name.LocalName == "time");
            Assert.That(timeAttribute, Is.Not.Null,
                        "Expected elements to have a time attribute.");
            Assert.That(timeAttribute.Value, Is.EqualTo(expectedArgVal.ToString()),
                        "Expected time attribute to match with original value:");

            XAttribute valueAttribute = attributesList.FirstOrDefault(att => att.Name.LocalName == "value");
            Assert.That(valueAttribute, Is.Not.Null,
                        "Expected elements to have a value attribute.");
            Assert.That(valueAttribute.Value, Is.EqualTo(expectedCompVal.ToString()),
                        "Expected value attribute to match with original value:");
        }

        #endregion
    }
}
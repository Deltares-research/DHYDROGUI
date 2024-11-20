using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Gui.Editors
{
    public class MaxNameableWidthCalculatorTest
    {
        private MaxNameableWidthCalculator calculator;
        private List<INameable> listOfNameables;
        private Font font;

        [SetUp]
        public void SetUp()
        {
            calculator = new MaxNameableWidthCalculator();
            listOfNameables = new List<INameable>();
            font = new Font("Arial", 12f);
        }

        [Test]
        [TestCaseSource(nameof(ArgumentNull))]
        public void GivenParametersNull_WhenGetMaxNameableWidth_ThrowsArgumentNullException(List<INameable> testList, Font testFont, string expectedFieldDescription)
        {
            // Call
            void Call() => calculator.GetMaxNameableWidth(testList, testFont);

            // Assert
            Assert.That(Call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo(expectedFieldDescription));
        }

        [Test]
        [TestCaseSource(nameof(Nameables))]
        public void GivenListWithNameable_WhenGetMaxItemWidth_ThenReturnExpectedMaxWidth(string name, int expectedMaxWidth)
        {
            listOfNameables.Add(new NameableTestClass() { Name = name });
            int maxItemWidth = calculator.GetMaxNameableWidth(listOfNameables, font);
            Assert.That(maxItemWidth, Is.EqualTo(expectedMaxWidth));
        }

        [Test]
        public void GivenListWithNameables_WhenGetMaxItemWidth_ThenReturnExpectedMaxWidth()
        {
            const int expectedMaxWidth = 106;
            var calc = new MaxNameableWidthCalculator();
            listOfNameables.Add(new NameableTestClass() { Name = "Name" });
            listOfNameables.Add(new NameableTestClass() { Name = "LongerName" });
            listOfNameables.Add(new NameableTestClass() { Name = "LongestName" });
            int maxItemWidth = calc.GetMaxNameableWidth(listOfNameables, font);
            Assert.That(maxItemWidth, Is.EqualTo(expectedMaxWidth));
        }

        [Test]
        public void GivenEmptyListOfNameable_WhenGetMaxItemWidth_ThenReturnExpectedMaxWidthOf0()
        {
            const int expectedMaxWidth = 0;
            int maxItemWidth = calculator.GetMaxNameableWidth(listOfNameables, font);
            Assert.That(maxItemWidth, Is.EqualTo(expectedMaxWidth));
        }

        private static IEnumerable<TestCaseData> Nameables()
        {
            yield return new TestCaseData("Name", 50);
            yield return new TestCaseData("LongerName", 99);
            yield return new TestCaseData("LongestName", 106);
        }

        private static IEnumerable<TestCaseData> ArgumentNull()
        {
            yield return new TestCaseData(null, new Font("Arial", 12f), "items");
            yield return new TestCaseData(new List<INameable>(), null, "font");
        }

        private class NameableTestClass : INameable
        {
            public string Name { get; set; }
        }
    }
}
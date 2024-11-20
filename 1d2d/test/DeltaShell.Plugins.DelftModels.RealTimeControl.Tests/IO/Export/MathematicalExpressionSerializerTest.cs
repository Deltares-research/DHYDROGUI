using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Export
{
    [TestFixture]
    public class MathematicalExpressionSerializerTest
    {
        private static readonly XNamespace fns = "http://www.wldelft.nl/fews";

        [Test]
        public void GetXmlName_ShouldReturnExpressionName()
        {
            // Arrange
            var mathematicalExpression = new MathematicalExpression {Name = "test"};
            var serializer = new MathematicalExpressionSerializer(mathematicalExpression);

            // Act
            string xmlName = serializer.GetXmlName();

            // Assert
            Assert.AreEqual(mathematicalExpression.Name, xmlName);
        }

        [Test]
        public void ToXml_ForMathematicalExpressionWithConstantValueAsInput()
        {
            // Arrange
            var mathematicalExpression = new MathematicalExpression {Name = "f1"};

            var input1 = new Input
            {
                ParameterName = "waterlevel",
                Feature = new RtcTestFeature {Name = "feature1"}
            };

            mathematicalExpression.Inputs.Add(input1);

            mathematicalExpression.Expression = "A+6";

            var serializer = new MathematicalExpressionSerializer(mathematicalExpression);
            var prefix = "Control Group 1/";

            // Act
            var returnedXmlCode = serializer.ToXml(fns, prefix).Single().ToString(SaveOptions.DisableFormatting);

            // Assert
            string expectedXmlCode = "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                                     "<expression id=\"Control Group 1/f1\">" +
                                     "<x1Series ref=\"IMPLICIT\">[Input]feature1/waterlevel</x1Series>" +
                                     "<mathematicalOperator>+</mathematicalOperator>" +
                                     "<x2Value>6</x2Value>" +
                                     "<y>f1</y>" +
                                     "</expression>" +
                                     "</trigger>";

            Assert.AreEqual(expectedXmlCode, returnedXmlCode);
        }

        [Test]
        public void ToXml_MathematicalExpressionUsing3Parameters()
        {
            // Arrange
            var mathematicalExpression = new MathematicalExpression {Name = "f1"};

            var input1 = new Input
            {
                ParameterName = "waterlevel",
                Feature = new RtcTestFeature {Name = "feature1"}
            };

            var input2 = new Input
            {
                ParameterName = "waterlevel",
                Feature = new RtcTestFeature {Name = "feature2"}
            };

            var input3 = new Input
            {
                ParameterName = "waterlevel",
                Feature = new RtcTestFeature {Name = "feature3"}
            };

            mathematicalExpression.Inputs.Add(input1);
            mathematicalExpression.Inputs.Add(input2);
            mathematicalExpression.Inputs.Add(input3);

            mathematicalExpression.Expression = "A+B+C";

            var serializer = new MathematicalExpressionSerializer(mathematicalExpression);
            var prefix = "Control Group 1/";

            // Act
            IEnumerable<XElement> expressionsList = serializer.ToXml(fns, prefix);
            var returnedXmlCode1 = expressionsList.First().ToString(SaveOptions.DisableFormatting);
            var returnedXmlCode2 = expressionsList.Last().ToString(SaveOptions.DisableFormatting);

            // Assert
            string expectedXmlCode1 = "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                                      "<expression id=\"Control Group 1/f1/([Input]feature1/waterlevel + [Input]feature2/waterlevel)\">" +
                                      "<x1Series ref=\"IMPLICIT\">[Input]feature1/waterlevel</x1Series>" +
                                      "<mathematicalOperator>+</mathematicalOperator>" +
                                      "<x2Series ref=\"IMPLICIT\">[Input]feature2/waterlevel</x2Series>" +
                                      "<y>f1/([Input]feature1/waterlevel + [Input]feature2/waterlevel)</y>" +
                                      "</expression>" +
                                      "</trigger>";

            string expectedXmlCode2 = "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                                      "<expression id=\"Control Group 1/f1\">" +
                                      "<x1Series ref=\"IMPLICIT\">f1/([Input]feature1/waterlevel + [Input]feature2/waterlevel)</x1Series>" +
                                      "<mathematicalOperator>+</mathematicalOperator>" +
                                      "<x2Series ref=\"IMPLICIT\">[Input]feature3/waterlevel</x2Series>" +
                                      "<y>f1</y>" +
                                      "</expression>" +
                                      "</trigger>";

            Assert.AreEqual(expectedXmlCode1, returnedXmlCode1);
            Assert.AreEqual(expectedXmlCode2, returnedXmlCode2);
        }

        [Test]
        public void ToXml_MathematicalExpressionUsing2ParametersFromWhichOneIsAnotherMathematicalExpression()
        {
            // Arrange
            var mathematicalExpression = new MathematicalExpression {Name = "f1"};

            var input1 = new MathematicalExpression {Name = "f2"};

            var input2 = new Input
            {
                ParameterName = "waterlevel",
                Feature = new RtcTestFeature {Name = "feature2"}
            };

            mathematicalExpression.Inputs.Add(input1);
            mathematicalExpression.Inputs.Add(input2);

            mathematicalExpression.Expression = "A+B";

            var serializer = new MathematicalExpressionSerializer(mathematicalExpression);
            var prefix = "Control Group 1/";

            // Act
            var returnedXmlCode = serializer.ToXml(fns, prefix).Single().ToString(SaveOptions.DisableFormatting);

            // Assert
            string expectedXmlCode = "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                                     "<expression id=\"Control Group 1/f1\">" +
                                     "<x1Series ref=\"IMPLICIT\">f2</x1Series>" +
                                     "<mathematicalOperator>+</mathematicalOperator>" +
                                     "<x2Series ref=\"IMPLICIT\">[Input]feature2/waterlevel</x2Series>" +
                                     "<y>f1</y>" +
                                     "</expression>" +
                                     "</trigger>";

            Assert.AreEqual(expectedXmlCode, returnedXmlCode);
        }

        [Test]
        public void ToXml_MathematicalExpressionUsing3ParametersFromWhichOneIsAnotherMathematicalExpression()
        {
            // Arrange
            var mathematicalExpression = new MathematicalExpression {Name = "f1"};

            var input1 = new MathematicalExpression {Name = "f2"};

            var input2 = new Input
            {
                ParameterName = "waterlevel",
                Feature = new RtcTestFeature {Name = "feature2"}
            };

            var input3 = new Input
            {
                ParameterName = "waterlevel",
                Feature = new RtcTestFeature {Name = "feature3"}
            };

            mathematicalExpression.Inputs.Add(input1);
            mathematicalExpression.Inputs.Add(input2);
            mathematicalExpression.Inputs.Add(input3);

            mathematicalExpression.Expression = "A+B+C";

            var serializer = new MathematicalExpressionSerializer(mathematicalExpression);
            var prefix = "Control Group 1/";

            // Act
            IEnumerable<XElement> expressionsList = serializer.ToXml(fns, prefix);
            var returnedXmlCode1 = expressionsList.First().ToString(SaveOptions.DisableFormatting);
            var returnedXmlCode2 = expressionsList.Last().ToString(SaveOptions.DisableFormatting);

            // Assert
            string expectedXmlCode1 = "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                                      "<expression id=\"Control Group 1/f1/(f2 + [Input]feature2/waterlevel)\">" +
                                      "<x1Series ref=\"IMPLICIT\">f2</x1Series>" +
                                      "<mathematicalOperator>+</mathematicalOperator>" +
                                      "<x2Series ref=\"IMPLICIT\">[Input]feature2/waterlevel</x2Series>" +
                                      "<y>f1/(f2 + [Input]feature2/waterlevel)</y>" +
                                      "</expression>" +
                                      "</trigger>";

            string expectedXmlCode2 = "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                                      "<expression id=\"Control Group 1/f1\">" +
                                      "<x1Series ref=\"IMPLICIT\">f1/(f2 + [Input]feature2/waterlevel)</x1Series>" +
                                      "<mathematicalOperator>+</mathematicalOperator>" +
                                      "<x2Series ref=\"IMPLICIT\">[Input]feature3/waterlevel</x2Series>" +
                                      "<y>f1</y>" +
                                      "</expression>" +
                                      "</trigger>";

            Assert.AreEqual(expectedXmlCode1, returnedXmlCode1);
            Assert.AreEqual(expectedXmlCode2, returnedXmlCode2);
        }

        [Test]
        public void ToXml_WhenMathematicalExpressionContainsExpressionWithOneElement()
        {
            // Arrange
            var mathematicalExpression = new MathematicalExpression {Name = "f1"};

            var input1 = new Input
            {
                ParameterName = "waterlevel",
                Feature = new RtcTestFeature {Name = "feature1"}
            };

            mathematicalExpression.Inputs.Add(input1);

            mathematicalExpression.Expression = "A";

            var serializer = new MathematicalExpressionSerializer(mathematicalExpression);
            var prefix = "Control Group 1/";

            // Act and Assert
            var exception = Assert.Throws<InvalidOperationException>(() => serializer.ToXml(fns, prefix).ToList());
            Assert.AreEqual("Mathematical expression f1 contains invalid expression \"A\".", exception.Message);
        }

        [Test]
        public void ToXml_WhenMathematicalExpressionContainsExpressionWhichCannotBeParsed()
        {
            // Arrange
            var mathematicalExpression = new MathematicalExpression {Name = "f1"};

            mathematicalExpression.Expression = "test";

            var serializer = new MathematicalExpressionSerializer(mathematicalExpression);
            var prefix = "Control Group 1/";

            // Act and Assert
            var exception = Assert.Throws<ArgumentException>(() => serializer.ToXml(fns, prefix).ToList());
            Assert.AreEqual(
                "Error in Ln: 1 Col: 1\r\ntest\r\n^\r\nExpecting: floating-point number, parameter, '(', 'max' or 'min'\r\n",
                exception.Message);
        }

        [Test]
        public void GetDataConfigXmlElements_ForMathematicalExpressionsUsing2Parameters()
        {
            // Arrange
            var mathematicalExpression = new MathematicalExpression {Name = "f1"};

            var input1 = new Input();
            var input2 = new Input();

            mathematicalExpression.Inputs.Add(input1);
            mathematicalExpression.Inputs.Add(input2);

            mathematicalExpression.Expression = "A+B";

            var serializer = new MathematicalExpressionSerializer(mathematicalExpression);

            // Act
            var returnedXmlCode = serializer.GetDataConfigXmlElements(fns).Single().ToString(SaveOptions.DisableFormatting);

            // Assert
            var expectedXmlCode = "<timeSeries id=\"f1\" xmlns=\"http://www.wldelft.nl/fews\" />";

            Assert.AreEqual(expectedXmlCode, returnedXmlCode);
        }

        [Test]
        public void GetDataConfigXmlElements_ForMathematicalExpressionsUsing3Parameters()
        {
            // Arrange
            var mathematicalExpression = new MathematicalExpression {Name = "f1"};

            var input1 = new Input
            {
                ParameterName = "waterlevel",
                Feature = new RtcTestFeature {Name = "feature1"}
            };

            mathematicalExpression.Inputs.Add(input1);

            mathematicalExpression.Expression = "A+2+6";

            var serializer = new MathematicalExpressionSerializer(mathematicalExpression);

            // Act
            IEnumerable<XElement> dataConfigXmlElements = serializer.GetDataConfigXmlElements(fns);

            // Assert
            Assert.AreEqual(2, dataConfigXmlElements.Count());
            var returnedXmlCode1 = dataConfigXmlElements.First().ToString(SaveOptions.DisableFormatting);
            var returnedXmlCode2 = dataConfigXmlElements.Last().ToString(SaveOptions.DisableFormatting);

            var expectedXmlCode1 = "<timeSeries id=\"f1\" xmlns=\"http://www.wldelft.nl/fews\" />";
            var expectedXmlCode2 = "<timeSeries id=\"f1/([Input]feature1/waterlevel + 2)\" xmlns=\"http://www.wldelft.nl/fews\" />";

            Assert.AreEqual(expectedXmlCode1, returnedXmlCode1);
            Assert.AreEqual(expectedXmlCode2, returnedXmlCode2);
        }

        [TestCase("A+B", "+")]
        [TestCase("A-B", "-")]
        [TestCase("A*B", "*")]
        [TestCase("A/B", "/")]
        [TestCase("min(A,B)", "min")]
        [TestCase("max(A,B)", "max")]
        public void ToXml_ForMathematicalExpressionsUsing2Parameters(string expression, string operatorAsString)
        {
            // Arrange
            var mathematicalExpression = new MathematicalExpression {Name = "f1"};

            var input1 = new Input
            {
                ParameterName = "waterlevel",
                Feature = new RtcTestFeature {Name = "feature1"}
            };

            var input2 = new Input
            {
                ParameterName = "waterlevel",
                Feature = new RtcTestFeature {Name = "feature2"}
            };

            mathematicalExpression.Inputs.Add(input1);
            mathematicalExpression.Inputs.Add(input2);

            mathematicalExpression.Expression = expression;

            var serializer = new MathematicalExpressionSerializer(mathematicalExpression);
            var prefix = "Control Group 1/";

            // Act
            var returnedXmlCode = serializer.ToXml(fns, prefix).Single().ToString(SaveOptions.DisableFormatting);

            // Assert
            string expectedXmlCode = "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                                     "<expression id=\"Control Group 1/f1\">" +
                                     "<x1Series ref=\"IMPLICIT\">[Input]feature1/waterlevel</x1Series>" +
                                     "<mathematicalOperator>" + operatorAsString + "</mathematicalOperator>" +
                                     "<x2Series ref=\"IMPLICIT\">[Input]feature2/waterlevel</x2Series>" +
                                     "<y>f1</y>" +
                                     "</expression>" +
                                     "</trigger>";

            Assert.AreEqual(expectedXmlCode, returnedXmlCode);
        }
    }
}
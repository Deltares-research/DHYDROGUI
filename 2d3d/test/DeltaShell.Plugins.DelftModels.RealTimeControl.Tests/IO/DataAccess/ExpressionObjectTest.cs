using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeltaShell.Dimr.RtcXsd;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.DataAccess
{
    [TestFixture]
    public class ExpressionObjectTest
    {
        private static readonly Random random = new Random();

        [Test]
        public void Constructor_IdNull_ThrowsArgumentNullException()
        {
            // Setup
            var @operator = Operator.Max;

            // Call
            void Call() => new ExpressionObject(null, @operator,
                                                Substitute.For<IExpressionReference>(),
                                                Substitute.For<IExpressionReference>(),
                                                "y");

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("id"));
        }

        [Test]
        public void Constructor_OperatorNotDefined_ThrowsArgumentNullException()
        {
            // Setup
            const Operator @operator = (Operator) 100;

            // Call
            void Call() => new ExpressionObject("id", @operator,
                                                Substitute.For<IExpressionReference>(),
                                                Substitute.For<IExpressionReference>(),
                                                "y");

            // Assert
            var exception = Assert.Throws<InvalidEnumArgumentException>(Call);
            Assert.That(exception.Message, Is.EqualTo("operator"));
        }

        [TestCase(Operator.Add)]
        [TestCase(Operator.Subtract)]
        [TestCase(Operator.Multiply)]
        [TestCase(Operator.Divide)]
        [TestCase(Operator.Min)]
        [TestCase(Operator.Max)]
        public void Constructor_InitializesInstanceCorrectly(Operator @operator)
        {
            // Setup
            const string controlGroupName = "control_group_name";
            const string id = controlGroupName + "/expression_id";
            var firstReference = Substitute.For<IExpressionReference>();
            var secondReference = Substitute.For<IExpressionReference>();
            const string yValue = "y_value";

            // Call
            var expressionObject = new ExpressionObject(id, @operator,
                                                        firstReference, secondReference, yValue);

            // Assert
            Assert.That(expressionObject.Id, Is.EqualTo(id));
            Assert.That(expressionObject.ControlGroupName, Is.EqualTo(controlGroupName));
            Assert.That(expressionObject.Operator, Is.EqualTo(@operator));
            Assert.That(expressionObject.FirstReference, Is.SameAs(firstReference));
            Assert.That(expressionObject.SecondReference, Is.SameAs(secondReference));
            Assert.That(expressionObject.Y, Is.EqualTo(yValue));
        }

        [TestCaseSource(nameof(GetTestCases))]
        public void Constructor_ExpressionXml_InitializesInstanceCorrectly(ExpressionComplexType expressionXml,
                                                                           ExpressionObject expectedExpressionObject)
        {
            // Call
            var expressionObject = new ExpressionObject(expressionXml);

            // Assert
            Assert.That(expressionObject, Is.EqualTo(expectedExpressionObject)
                                            .Using(new ExpressionObjectEqualityComparer()));
        }

        private static IEnumerable<TestCaseData> GetTestCases()
        {
            const string controlGroupName = "control_group_name";
            const string id = controlGroupName + "/expression_id";
            const string yValue = "y_value";

            var constantValue = random.Next().ToString();
            const string inputReference = RtcXmlTag.Input + "input_name";
            const string expressionReference = "expression_name";

            foreach (Operator @operator in Enum.GetValues(typeof(Operator)))
            {
                ExpressionComplexType expressionXml = ExpressionComplexTypeBuilder.Create(id, @operator, yValue)
                                                                                  .WithConstantAsFirstReference(constantValue)
                                                                                  .AndConstantAsSecondReference(constantValue);
                var expectedResult = new ExpressionObject(id, @operator,
                                                          new ConstantLeafReference(constantValue),
                                                          new ConstantLeafReference(constantValue),
                                                          yValue);

                yield return new TestCaseData(expressionXml, expectedResult)
                    .SetName($" {@operator} - Constant - Constant");

                expressionXml = ExpressionComplexTypeBuilder.Create(id, @operator, yValue)
                                                            .WithConstantAsFirstReference(constantValue)
                                                            .AndInputAsSecondReference(inputReference);
                expectedResult = new ExpressionObject(id, @operator,
                                                      new ConstantLeafReference(constantValue),
                                                      new ParameterLeafReference(inputReference),
                                                      yValue);

                yield return new TestCaseData(expressionXml, expectedResult)
                    .SetName($" {@operator} - Constant - Input");

                expressionXml = ExpressionComplexTypeBuilder.Create(id, @operator, yValue)
                                                            .WithConstantAsFirstReference(constantValue)
                                                            .AndInputAsSecondReference(expressionReference);
                expectedResult = new ExpressionObject(id, @operator,
                                                      new ConstantLeafReference(constantValue),
                                                      new ExpressionReference(expressionReference),
                                                      yValue);

                yield return new TestCaseData(expressionXml, expectedResult)
                    .SetName($" {@operator} - Constant - Expression");

                expressionXml = ExpressionComplexTypeBuilder.Create(id, @operator, yValue)
                                                            .WithInputAsFirstReference(inputReference)
                                                            .AndConstantAsSecondReference(constantValue);
                expectedResult = new ExpressionObject(id, @operator,
                                                      new ParameterLeafReference(inputReference),
                                                      new ConstantLeafReference(constantValue),
                                                      yValue);

                yield return new TestCaseData(expressionXml, expectedResult)
                    .SetName($" {@operator} - Input - Constant");

                expressionXml = ExpressionComplexTypeBuilder.Create(id, @operator, yValue)
                                                            .WithInputAsFirstReference(inputReference)
                                                            .AndInputAsSecondReference(inputReference);
                expectedResult = new ExpressionObject(id, @operator,
                                                      new ParameterLeafReference(inputReference),
                                                      new ParameterLeafReference(inputReference),
                                                      yValue);

                yield return new TestCaseData(expressionXml, expectedResult)
                    .SetName($" {@operator} - Input - Input");

                expressionXml = ExpressionComplexTypeBuilder.Create(id, @operator, yValue)
                                                            .WithInputAsFirstReference(inputReference)
                                                            .AndInputAsSecondReference(expressionReference);
                expectedResult = new ExpressionObject(id, @operator,
                                                      new ParameterLeafReference(inputReference),
                                                      new ExpressionReference(expressionReference),
                                                      yValue);

                yield return new TestCaseData(expressionXml, expectedResult)
                    .SetName($" {@operator} - Input - Expression");

                expressionXml = ExpressionComplexTypeBuilder.Create(id, @operator, yValue)
                                                            .WithInputAsFirstReference(expressionReference)
                                                            .AndConstantAsSecondReference(constantValue);
                expectedResult = new ExpressionObject(id, @operator,
                                                      new ExpressionReference(expressionReference),
                                                      new ConstantLeafReference(constantValue),
                                                      yValue);

                yield return new TestCaseData(expressionXml, expectedResult)
                    .SetName($" {@operator} - Expression - Constant");

                expressionXml = ExpressionComplexTypeBuilder.Create(id, @operator, yValue)
                                                            .WithInputAsFirstReference(expressionReference)
                                                            .AndInputAsSecondReference(inputReference);
                expectedResult = new ExpressionObject(id, @operator,
                                                      new ExpressionReference(expressionReference),
                                                      new ParameterLeafReference(inputReference),
                                                      yValue);

                yield return new TestCaseData(expressionXml, expectedResult)
                    .SetName($" {@operator} - Expression - Input");

                expressionXml = ExpressionComplexTypeBuilder.Create(id, @operator, yValue)
                                                            .WithInputAsFirstReference(expressionReference)
                                                            .AndInputAsSecondReference(expressionReference);
                expectedResult = new ExpressionObject(id, @operator,
                                                      new ExpressionReference(expressionReference),
                                                      new ExpressionReference(expressionReference),
                                                      yValue);

                yield return new TestCaseData(expressionXml, expectedResult)
                    .SetName($" {@operator} - Expression - Expression");
            }
        }

        private class ExpressionObjectEqualityComparer : IEqualityComparer<ExpressionObject>
        {
            public bool Equals(ExpressionObject x, ExpressionObject y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                if (!(Equals(x.Id, y.Id) &&
                      Equals(x.ControlGroupName, y.ControlGroupName) &&
                      Equals(x.Operator, y.Operator) &&
                      Equals(x.Y, y.Y)))
                {
                    return false;
                }

                return EqualsReference(x.FirstReference, y.FirstReference) &&
                       EqualsReference(x.SecondReference, y.SecondReference);
            }

            public int GetHashCode(ExpressionObject obj)
            {
                throw new NotImplementedException();
            }

            private static bool EqualsReference(IExpressionReference x, IExpressionReference y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                if (x is ConstantLeafReference && !(y is ConstantLeafReference))
                {
                    return false;
                }

                if (x is ParameterLeafReference && !(y is ParameterLeafReference))
                {
                    return false;
                }

                if (x is ExpressionReference && !(y is ExpressionReference))
                {
                    return false;
                }

                return Equals(x.Value, y.Value);
            }
        }
    }
}
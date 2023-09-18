using System;
using System.Collections.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Dependency;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Dependency
{
    [TestFixture]
    public class AndOperatorExpressionTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void CanHandleExpressionTest()
        {
            var allExpressions = new List<DependencyExpressionBase>
                {
                    new BooleanIsTrueDependencyExpression(),
                    new ValueGreaterOrLesserThanDependencyExpression(),
                    new ValueEqualsDependencyExpression()
                };
            var expression = new AndOperatorExpression(allExpressions);
            allExpressions.Add(expression);

            Assert.IsFalse(expression.CanHandleExpression(""),
                "Should not handle empty expressions.");
            Assert.IsFalse(expression.CanHandleExpression(null),
                "Should not handle empty expressions.");
            Assert.IsFalse(expression.CanHandleExpression("sklGd_piOh49_Ps"),
                "Should not match any combinations of alpha-numeric characters and underscores.");

            Assert.IsTrue(expression.CanHandleExpression("  A &&  \t B "));
            Assert.IsFalse(expression.CanHandleExpression("  A ||  \t B "),
                "Not implemented this, as this would require setting up precedency rules.");

            Assert.IsTrue(expression.CanHandleExpression("A < 0 && B = 1"));
            Assert.IsFalse(expression.CanHandleExpression("A < 0 || B = 1"),
                "Not implemented this, as this would require setting up precedency rules.");

            Assert.IsFalse(expression.CanHandleExpression("&&"),
                "Should have a valid subexpressions left and right.");
            Assert.IsFalse(expression.CanHandleExpression("A&&"),
                "Should have a valid subexpressions left and right.");
            Assert.IsFalse(expression.CanHandleExpression("&&B"),
                "Should have a valid subexpressions left and right.");

            Assert.IsTrue(expression.CanHandleExpression("A && B && C"),
                "Allow nesting");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CompileExpressionWithBooleanIsTrueSubExpressionsTest()
        {
            var allExpressions = new List<DependencyExpressionBase>
                {
                    new BooleanIsTrueDependencyExpression()
                };
            var expression = new AndOperatorExpression(allExpressions);
            allExpressions.Add(expression);

            var properties = new List<ModelProperty>
                {
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyKey = "A",
                            DataType = typeof (bool)
                        }, "1"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyKey = "B",
                            DataType = typeof (bool)
                        }, "1"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyKey = "C",
                            DataType = typeof (bool)
                        }, "1"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyKey = "string",
                            DataType = typeof (string)
                        }, "1.2"),
                };
            
            var propertyA = properties[0];
            var propertyB = properties[1];
            var propertyC = properties[2];
            var propertyToBeComppiled = properties[3];

            Assert.Throws<FormatException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                "Throw FormatException when compiling for unhandleable dependency expression.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "A && B && C";
            var isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsTrue(isEnabledMethod(properties),
                "Should return true as A and B are boolean and true.");

            propertyA.Value = false;
            Assert.IsFalse(isEnabledMethod(properties),
                "Should return false as A is false.");

            propertyA.Value = true;
            propertyB.Value = false;
            Assert.IsFalse(isEnabledMethod(properties),
                "Should return false as B is false.");

            propertyB.Value = true;
            propertyC.Value = false;
            Assert.IsFalse(isEnabledMethod(properties),
                "Should return false as C is false.");

            propertyA.Value = false;
            propertyB.Value = false;
            Assert.IsFalse(isEnabledMethod(properties),
                "Should return false as A, B and C are false.");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CompileExpressionWithValueGreaterThenSubExpressionsTest()
        {
            var allExpressions = new List<DependencyExpressionBase>
                {
                    new ValueGreaterOrLesserThanDependencyExpression()
                };
            var expression = new AndOperatorExpression(allExpressions);
            allExpressions.Add(expression);

            var properties = new List<ModelProperty>
                {
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyKey = "A",
                            DataType = typeof(int)
                        }, "1"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyKey = "B",
                            DataType = typeof(int)
                        }, "6"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyKey = "string",
                            DataType = typeof(string)
                        }, "1.2"),
                };
            var propertyToBeComppiled = properties[2];
            var propertyA = properties[0];
            var propertyB = properties[1];

            Assert.Throws<FormatException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                "Throw FormatException when compiling for unhandleable dependency expression.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "A > 0 && A <= 5 && B > 5";
            var isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsTrue(isEnabledMethod(properties),
                "Should return true as A in range (0, 5] and B > 5 and both are numbers.");

            propertyA.Value = 0;
            Assert.IsFalse(isEnabledMethod(properties),
                "Should return false as A is 0, outside range (0, 5]");

            propertyA.Value = 6;
            Assert.IsFalse(isEnabledMethod(properties),
                "Should return false as A is 6, outside range (0, 5]");

            propertyA.Value = 5;
            Assert.IsTrue(isEnabledMethod(properties),
                "Should return true as A is 5, inside range (0, 5]");

            propertyB.Value = 5;
            Assert.IsFalse(isEnabledMethod(properties),
                "Should return false as B is not larger than 5.");

            propertyA.Value = 6;
            Assert.IsFalse(isEnabledMethod(properties),
                "Should return false as A and B are outside their ranges.");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CompileExpressionWithValueEqualsSubExpressionsTest()
        {
            var allExpressions = new List<DependencyExpressionBase>
                {
                    new ValueEqualsDependencyExpression()
                };
            var expression = new AndOperatorExpression(allExpressions);
            allExpressions.Add(expression);

            var properties = new List<ModelProperty>
                {
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyKey = "A",
                            DataType = typeof (int)
                        }, "1"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyKey = "B",
                            DataType = typeof (int)
                        }, "1"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyKey = "string",
                            DataType = typeof (string)
                        }, "1.2"),
                };
            var propertyToBeComppiled = properties[2];
            var propertyA = properties[0];
            var propertyB = properties[1];

            Assert.Throws<FormatException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                "Throw FormatException when compiling for unhandleable dependency expression.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "A = 1 && B = 1|2";
            var isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsTrue(isEnabledMethod(properties),
                "Should return true as A = 1 and B in range [1,2] and both are numbers.");

            propertyA.Value = 0;
            Assert.IsFalse(isEnabledMethod(properties),
                "Should return false as A is not 1");

            propertyA.Value = 1;
            propertyB.Value = 3;
            Assert.IsFalse(isEnabledMethod(properties),
                "Should return false as A is 6, outside range (0, 5]");

            propertyA.Value = 2;
            Assert.IsFalse(isEnabledMethod(properties),
                "Should return false as all requirements are not met");
        }
    }
}
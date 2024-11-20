using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.Dependency;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Dependency
{
    [TestFixture]
    public class BooleanIsTrueDependencyExpressionTest
    {
        [Test]
        public void CanHandleExpressionTest()
        {
            var expression = new BooleanIsTrueDependencyExpression();
            Assert.IsFalse(expression.CanHandleExpression(""),
                           "Should not handle empty expressions.");
            Assert.IsFalse(expression.CanHandleExpression(null),
                           "Should not handle empty expressions.");
            Assert.IsTrue(expression.CanHandleExpression("aSdFg_234567"),
                          "Should match any combinations of alpha-numeric characters and underscores.");
            Assert.IsTrue(expression.CanHandleExpression("7890_sdfgh_6t7y8u89"),
                          "Should match any combinations of alpha-numeric characters and underscores.");

            Assert.IsFalse(expression.CanHandleExpression("as23   4567yhjk"),
                           "Should not match if there are any white spaces");
            Assert.IsFalse(expression.CanHandleExpression("A<0"),
                           "Should not match comparisons");
        }

        [Test]
        public void CompileExpressionTest()
        {
            var expression = new BooleanIsTrueDependencyExpression();
            var properties = new List<ModelProperty>
            {
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "A",
                    EnabledDependencies = null,
                    DataType = typeof(string)
                }, "1"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "B",
                    DataType = typeof(bool)
                }, "1"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "C",
                    DataType = typeof(bool)
                }, "0"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "string",
                    DataType = typeof(string)
                }, "1.2")
            };
            ModelProperty propertyToBeComppiled = properties[0];

            Assert.Throws<FormatException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                                           "Throw FormatException when compiling for unhandleable dependency expression.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "B";
            Func<IEnumerable<ModelProperty>, bool> isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as B is boolean and true.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "C";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as C is boolean and not true.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "D";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as required property D is missing.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "string";
            Assert.Throws<ArgumentException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                                             "Should throw as property 'string' is not of boolean type.");
        }
    }
}
using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.Dependency;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Dependency
{
    [TestFixture]
    public class ValueEqualsDependencyExpressionTest
    {
        [Test]
        public void CanHandleExpressionTest()
        {
            var expression = new ValueEqualsDependencyExpression();

            Assert.IsFalse(expression.CanHandleExpression(""),
                "Should not handle empty expressions.");
            Assert.IsFalse(expression.CanHandleExpression(null),
                "Should not handle empty expressions.");

            Assert.IsFalse(expression.CanHandleExpression("sdfgh56789_"),
                "Should only handle comparisons.");

            Assert.IsTrue(expression.CanHandleExpression("KJhd9SOPf04_oihg0w_D  \t=123456789"),
                "Should handle equals to single integer");
            Assert.IsTrue(expression.CanHandleExpression("K8sdJhbf_sijd398= \t\t -1|2|-3|4|-5"),
                "Should handle equals to collection of integers");
            Assert.IsTrue(expression.CanHandleExpression("B = -1"),
                "Should handle equals to collection of integers");

            Assert.IsFalse(expression.CanHandleExpression("Fsjd9rh = 1.2"),
                "Implementation support for doubles in the future when required");
            Assert.IsFalse(expression.CanHandleExpression("Fsjd9rh = slkdjkYU98dfjk"),
                "Implementation support for strings in the future when required");
        }

        [Test]
        public void CompileTest()
        {
            var expression = new ValueEqualsDependencyExpression();
            var enumBCaption = "An enum:Minus one|Zero|One|Two";
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
                            DataType = DataTypeValueParser.GetClrType("B", "-1|0|1|2", ref enumBCaption, null, 0)
                        }, "-1"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyKey = "C",
                            DataType = typeof(int)
                        }, "5"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyKey = "string",
                            DataType = typeof(string)
                        }, "1"),
                };

            var propertyToBeComppiled = properties[0];
            var propertyB = properties[1];
            var propertyC = properties[2];

            Assert.Throws<FormatException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                "Throw FormatException when compiling for unhandleable dependency expression.");

            #region Checking for enums:

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "B = -1";
            var isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsTrue(isEnabledMethod(properties),
                "Support enum and compare on integer representation.");

            propertyB.SetValueAsString("2");
            Assert.IsFalse(isEnabledMethod(properties),
                "Support enum and compare on integer representation.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "B = -1|2";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsTrue(isEnabledMethod(properties),
                "Support enum and match to a set of integers.");
            propertyB.SetValueAsString("-1");
            Assert.IsTrue(isEnabledMethod(properties),
                "Support enum and match to a set of integers.");

            propertyB.SetValueAsString("0");
            Assert.IsFalse(isEnabledMethod(properties),
                "Support enum and match to a set of integers.");

            #endregion

            #region Checking for ints:

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "C = -9";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsFalse(isEnabledMethod(properties),
                "Support enum and compare on integer representation.");

            propertyC.Value = -9;
            Assert.IsTrue(isEnabledMethod(properties),
                "Support enum and compare on integer representation.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "C = -9|2";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsTrue(isEnabledMethod(properties),
                "Support enum and match to a set of integers.");
            propertyC.SetValueAsString("2");
            Assert.IsTrue(isEnabledMethod(properties),
                "Support enum and match to a set of integers.");

            propertyC.SetValueAsString("0");
            Assert.IsFalse(isEnabledMethod(properties),
                "Support enum and match to a set of integers.");

            #endregion

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "D = 0";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsFalse(isEnabledMethod(properties),
                "Should return false as property D is missing.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "string = 0";
            Assert.Throws<ArgumentException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                "Should throw as property 'string' is not of int or enum type.");
        }
    }
}
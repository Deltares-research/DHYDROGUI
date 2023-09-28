using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.Dependency;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Dependency
{
    [TestFixture]
    public class ValueGreaterOrLesserThanDependencyExpressionTest
    {
        [Test]
        public void CanHandleExpressionTest()
        {
            var supportedComparisonToken = new[]
            {
                ">",
                ">=",
                "<",
                "<="
            };

            var expression = new ValueGreaterOrLesserThanDependencyExpression();

            Assert.IsFalse(expression.CanHandleExpression(""),
                           "Should not handle empty expressions.");
            Assert.IsFalse(expression.CanHandleExpression(null),
                           "Should not handle empty expressions.");

            Assert.IsFalse(expression.CanHandleExpression("sdfgh56789_"),
                           "Should only handle comparisons.");
            foreach (string token in supportedComparisonToken)
            {
                Assert.IsTrue(expression.CanHandleExpression(string.Format("dfgh5678_{0} \t1.2", token)),
                              string.Format("Should handle '{0}' comparison with double", token));
                Assert.IsTrue(expression.CanHandleExpression(string.Format("_678sjJfuaisn\t {0}-3.4", token)),
                              string.Format("Should handle '{0}' comparison with negative double", token));
                Assert.IsTrue(expression.CanHandleExpression(string.Format("J7Jfs9_jmsfa_ \t \t {0}-3", token)),
                              string.Format("Should handle '{0}' comparison with int", token));
                Assert.IsTrue(expression.CanHandleExpression(string.Format("J7Jfs9_jmsfa_ {0} 6", token)),
                              string.Format("Should handle '{0}' comparison with int", token));

                Assert.IsFalse(expression.CanHandleExpression(string.Format("J7Jfs9_jmsfa_ {0} 9sdjan_smaio", token)),
                               string.Format("Should handle '{0}' comparison with other parameters", token));
                Assert.IsFalse(expression.CanHandleExpression(string.Format("1.2 {0} 9sdjan_smaio", token)),
                               string.Format("Should handle '{0}' comparison in wrong order", token));
            }

            Assert.IsFalse(expression.CanHandleExpression("dfgh5678_ = \t1.2"),
                           "Should not handle equals comparison with double");
            Assert.IsFalse(expression.CanHandleExpression("dfgh5678_=9"),
                           "Should not handle equals comparison with integer");
        }

        [Test]
        public void CompileGreaterThenExpressionTest()
        {
            var expression = new ValueGreaterOrLesserThanDependencyExpression();
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
                    DataType = typeof(double)
                }, "1.2"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "C",
                    DataType = typeof(int)
                }, "5"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "string",
                    DataType = typeof(string)
                }, "1.2")
            };
            ModelProperty propertyToBeComppiled = properties[0];
            ModelProperty propertyB = properties[1];
            ModelProperty propertyC = properties[2];

            Assert.Throws<FormatException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                                           "Throw FormatException when compiling for unhandleable dependency expression.");

            #region Checking for doubles:

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "B > 0.1";
            Func<IEnumerable<ModelProperty>, bool> isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as B is double and greater than 0.1");

            propertyB.Value = 0.1 + 1e-6; // Corner case (still greater than 0.1)
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as B is double and greater than 0.1");

            propertyB.Value = 0.1; // Corner case (no longer greater than 0.1)
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as B is double and equals 0.1");

            propertyB.Value = -2.3;
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as B is double and not greater than 0.1");

            #endregion

            #region Checking for ints:

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "C > 0";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as C is int and greater than 0");

            propertyC.Value = 1; // Corner case (still greater than 0)
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as C is int and greater than 0");

            propertyC.Value = 0; // Corner case (no longer greater than 0)
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as C is int and equals 0");

            propertyC.Value = -2;
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as C is int and not greater than 0");

            #endregion

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "D > 0";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as property D is missing.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "string > 0";
            Assert.Throws<ArgumentException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                                             "Should throw as property 'string' is not of double or int type.");
        }

        [Test]
        public void CompileGreaterThenOrEqualsExpressionTest()
        {
            var expression = new ValueGreaterOrLesserThanDependencyExpression();
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
                    DataType = typeof(double)
                }, "1.2"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "C",
                    DataType = typeof(int)
                }, "5"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "string",
                    DataType = typeof(string)
                }, "1.2")
            };
            ModelProperty propertyToBeComppiled = properties[0];
            ModelProperty propertyB = properties[1];
            ModelProperty propertyC = properties[2];

            Assert.Throws<FormatException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                                           "Throw FormatException when compiling for unhandleable dependency expression.");

            #region Checking for doubles:

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "B >= 0.1";
            Func<IEnumerable<ModelProperty>, bool> isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as B is double and greater than 0.1");

            propertyB.Value = 0.1; // Corner case
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as B is double and equals 0.1");

            propertyB.Value = 0.1 - 1e-6; // Corner case (no longer greater than 0.1)
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as B is double and not greater than 0.1");

            propertyB.Value = -2.3;
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as B is double and not greater than 0.1");

            #endregion

            #region Checking for ints:

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "C > 0";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return false as C is int and greater than 0");

            propertyC.Value = 1; // Corner case (still greater than 0)
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as C is int and greater than 0");

            propertyC.Value = 0; // Corner case (no longer greater than 0)
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as C is int and equals 0");

            propertyC.Value = -2;
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return true as C is int and not greater than 0");

            #endregion

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "D >= 0";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as property D is missing.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "string >= 0";
            Assert.Throws<ArgumentException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                                             "Should throw as property 'string' is not of double or int type.");
        }

        [Test]
        public void CompileLessThenExpressionTest()
        {
            var expression = new ValueGreaterOrLesserThanDependencyExpression();
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
                    DataType = typeof(double)
                }, "1.2"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "C",
                    DataType = typeof(int)
                }, "5"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "string",
                    DataType = typeof(string)
                }, "1.2")
            };
            ModelProperty propertyToBeComppiled = properties[0];
            ModelProperty propertyB = properties[1];
            ModelProperty propertyC = properties[2];

            Assert.Throws<FormatException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                                           "Throw FormatException when compiling for unhandleable dependency expression.");

            #region Checking for doubles:

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "B < 0.1";
            Func<IEnumerable<ModelProperty>, bool> isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as B is double and not less than 0.1");

            propertyB.Value = 0.1; // Corner case
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as B is double and equals 0.1");

            propertyB.Value = 0.1 - 1e-6; // Corner case (less than 0.1)
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as B is double and less than 0.1");

            propertyB.Value = -2.3;
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as B is double and less than 0.1");

            #endregion

            #region Checking for ints:

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "C < 0";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as C is int and not less than 0");

            propertyC.Value = 0; // Corner case
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as C is int and equals 0");

            propertyC.Value = -1; // Corner case (no longer greater than 0)
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as C is int and less than 0");

            propertyC.Value = -9999;
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as C is int and less than 0");

            #endregion

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "D < 0";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as property D is missing.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "string < 0";
            Assert.Throws<ArgumentException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                                             "Should throw as property 'string' is not of double or int type.");
        }

        [Test]
        public void CompileLessThenOrEqualsExpressionTest()
        {
            var expression = new ValueGreaterOrLesserThanDependencyExpression();
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
                    DataType = typeof(double)
                }, "1.2"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "C",
                    DataType = typeof(int)
                }, "5"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "string",
                    DataType = typeof(string)
                }, "1.2")
            };
            ModelProperty propertyToBeComppiled = properties[0];
            ModelProperty propertyB = properties[1];
            ModelProperty propertyC = properties[2];

            Assert.Throws<FormatException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                                           "Throw FormatException when compiling for unhandleable dependency expression.");

            #region Checking for doubles:

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "B <= 0.1";
            Func<IEnumerable<ModelProperty>, bool> isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as B is double and not less than 0.1");

            propertyB.Value = 0.1 + 1e-6; // Corner case
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as B is double and not less than 0.1");

            propertyB.Value = 0.1; // Corner case
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as B is double and equals 0.1");

            propertyB.Value = -2.3;
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as B is double and less than 0.1");

            #endregion

            #region Checking for ints:

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "C <= 0";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as C is int and not less than 0");

            propertyC.Value = 1; // Corner case
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as C is int and not less than 0");

            propertyC.Value = 0; // Corner case (no longer greater than 0)
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as C is int and equals 0");

            propertyC.Value = -9999;
            Assert.IsTrue(isEnabledMethod(properties),
                          "Should return true as C is int and less than 0");

            #endregion

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "D <= 0";
            isEnabledMethod = expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies);
            Assert.IsFalse(isEnabledMethod(properties),
                           "Should return false as property D is missing.");

            propertyToBeComppiled.PropertyDefinition.EnabledDependencies = "string <= 0";
            Assert.Throws<ArgumentException>(() => expression.CompileExpression(propertyToBeComppiled, properties, propertyToBeComppiled.PropertyDefinition.EnabledDependencies),
                                             "Should throw as property 'string' is not of double or int type.");
        }
    }
}
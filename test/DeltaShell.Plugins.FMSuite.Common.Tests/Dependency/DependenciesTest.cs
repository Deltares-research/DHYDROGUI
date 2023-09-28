using System.Collections.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Dependency;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Dependency
{
    [TestFixture]
    public class DependenciesTest
    {
        [Test]
        public void CompileAllDependencies()
        {
            var properties = new List<ModelProperty>
            {
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "A",
                    EnabledDependencies = "B",
                    DataType = typeof(bool)
                }, "0"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "B",
                    EnabledDependencies = "A",
                    DataType = typeof(bool)
                }, "1"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "C",
                    EnabledDependencies = "double < 1.0",
                    DataType = typeof(bool)
                }, "0"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "double",
                    DataType = typeof(double),
                    EnabledDependencies = "int = 3"
                }, "1.2"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "int",
                    DataType = typeof(int),
                    EnabledDependencies = ""
                }, "2")
            };

            // Expect no compilation messages:
            TestHelper.AssertLogMessagesCount(() => Dependencies.CompileEnabledDependencies(properties), 0);

            ModelProperty propertyA = properties[0];
            ModelProperty propertyB = properties[1];
            ModelProperty propertyC = properties[2];
            ModelProperty propertyDouble = properties[3];
            ModelProperty propertyint = properties[4];

            // No cyclic IsEnabled checking:
            Assert.IsTrue(propertyA.IsEnabled(properties));
            Assert.IsFalse(propertyB.IsEnabled(properties));

            // C is not enabled as 'double' is not less than 1.0
            Assert.IsFalse(propertyC.IsEnabled(properties));

            // double is not enabled as 'int' is not equal to 3
            Assert.IsFalse(propertyDouble.IsEnabled(properties));

            // No dependencies:
            Assert.IsTrue(propertyint.IsEnabled(properties));
        }

        [Test]
        public void DependenciesShouldHandleCompilationErrorsAndUseDefaults()
        {
            var properties = new List<ModelProperty>
            {
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "A",
                    EnabledDependencies = "string > 1", // This should not compile
                    DataType = typeof(string)
                }, "1"),
                new TestModelProperty(new TestModelPropertyDefinition
                {
                    FilePropertyKey = "string",
                    DataType = typeof(string)
                }, "1.2")
            };

            TestHelper.AssertLogMessageIsGenerated(() => Dependencies.CompileEnabledDependencies(properties),
                                                   "Cannot read dependencies for property 'A'; Reason: Model property 'string' should be have 'double' or 'integer' data type.", 1);

            ModelProperty propertyA = properties[0];
            ModelProperty propertyString = properties[1];

            Assert.IsTrue(propertyA.IsEnabled(properties),
                          "Use default isEnabled method");
            Assert.IsTrue(propertyString.IsEnabled(properties));
        }
    }
}
using System.Collections.Generic;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO;
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
                            FilePropertyName = "A",
                            EnabledDependencies = "B",
                            DataType = typeof(bool)
                        }, "0"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyName = "B",
                            EnabledDependencies = "A",
                            DataType = typeof(bool)
                        }, "1"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyName = "C",
                            EnabledDependencies = "double < 1.0",
                            DataType = typeof(bool)
                        }, "0"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyName = "double",
                            DataType = typeof(double),
                            EnabledDependencies = "int = 3"
                        }, "1.2"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyName = "int",
                            DataType = typeof(int),
                            EnabledDependencies = ""
                        }, "2")
                };

            // Expect no compilation messages:
            TestHelper.AssertLogMessagesCount(() => Dependencies.CompileEnabledDependencies(properties), 0);

            var propertyA = properties[0];
            var propertyB = properties[1];
            var propertyC = properties[2];
            var propertyDouble = properties[3];
            var propertyint = properties[4];

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
                            FilePropertyName = "A",
                            EnabledDependencies = "string > 1", // This should not compile
                            DataType = typeof (string)
                        }, "1"),
                    new TestModelProperty(new TestModelPropertyDefinition
                        {
                            FilePropertyName = "string",
                            DataType = typeof (string)
                        }, "1.2"),
                };

            TestHelper.AssertLogMessageIsGenerated(() => Dependencies.CompileEnabledDependencies(properties),
                "Cannot read dependencies for property 'A'; Reason: Model property 'string' should be have 'double' or 'integer' data type.", 1);

            var propertyA = properties[0];
            var propertyString = properties[1];

            Assert.IsTrue(propertyA.IsEnabled(properties),
                "Use default isEnabled method");
            Assert.IsTrue(propertyString.IsEnabled(properties));
        }

        [Test]
        public void DependenciesDefaultValueIndexerShouldLinkPropertiesAndUpdateDefaultValue()
        {
            //Arrange
            List<ModelProperty> allProperties = new List<ModelProperty>();
            ModelProperty dropDownProperty = GetDropDownProperty();
            ModelProperty doublePropertyWithLink = GetDoublePropertyWithLink(dropDownProperty);
            allProperties.Add(dropDownProperty);
            allProperties.Add(doublePropertyWithLink);
            
            //Act
            Dependencies.CompileDefaultValueIndexerDependencies(allProperties);
            
            //Assert
            Assert.That(dropDownProperty.LinkedModelProperty, Is.EqualTo(doublePropertyWithLink));
            Assert.That(doublePropertyWithLink.Value.ToString(), Is.EqualTo(doublePropertyWithLink.PropertyDefinition.DefaultValueAsStringArray[(int)dropDownProperty.Value]));
        }

        private static ModelProperty GetDoublePropertyWithLink(ModelProperty property)
        {
            ModelPropertyDefinition propertyDefinition = new TestModelPropertyDefinition();
            propertyDefinition.DefaultValueAsStringArray = new List<string>();
            propertyDefinition.DefaultValueAsStringArray.Add("10");
            propertyDefinition.DefaultValueAsStringArray.Add("11");
            const string propertyName = "UnifFrictCoefChannels";
            propertyDefinition.FilePropertyName = propertyName;
            propertyDefinition.DefaultsIndexer = property.PropertyDefinition.FilePropertyName;
            propertyDefinition.DataType = typeof(double);
            ModelProperty property2 = new TestModelProperty(propertyDefinition, "10");
            return property2;
        }

        private static ModelProperty GetDropDownProperty()
        {
            ModelPropertyDefinition propertyDefinition = new TestModelPropertyDefinition();
            string caption = "Uniform friction type:Chezy|Manning";
            const string typeField = "0|1";
            const string propertyName = "UnifFrictTypeChannels";
            propertyDefinition.FilePropertyName = propertyName;
            propertyDefinition.DataType = DataTypeValueParser.GetClrType(propertyName, typeField, ref caption, caption, 0);

            ModelProperty property = new TestModelProperty(propertyDefinition, "0");
            return property;
        }
    }
}
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ModelDefinition
{
    [TestFixture]
    public class WaterFlowFMPropertyDefinitionCreatorTest
    {
        [Test]
        public void CreateForUnknownProperty_ExpectedPropertyValues()
        {
            const string mduGroupName = "myGroupName";
            const string mduPropertyName = "myPropertyName";
            const string comment = "myComment";

            WaterFlowFMPropertyDefinition definition =
                WaterFlowFMPropertyDefinitionCreator.CreateForCustomProperty(
                    mduGroupName, mduPropertyName, comment);

            Assert.That(definition.Caption, Is.EqualTo(mduPropertyName));
            Assert.That(definition.MduPropertyName, Is.EqualTo(mduPropertyName));
            Assert.That(definition.FileSectionName, Is.EqualTo(mduGroupName));
            Assert.That(definition.FilePropertyKey, Is.EqualTo(mduPropertyName));
            Assert.That(definition.Category, Is.EqualTo("Miscellaneous"));
            Assert.IsNull(definition.SubCategory);
            Assert.That(definition.DataType, Is.EqualTo(typeof(string)));
            Assert.That(definition.DefaultValueAsString, Is.EqualTo(string.Empty));
            Assert.That(definition.EnabledDependencies, Is.EqualTo(string.Empty));
            Assert.That(definition.VisibleDependencies, Is.EqualTo(string.Empty));
            Assert.That(definition.Description, Is.EqualTo(comment));
            Assert.IsFalse(definition.IsDefinedInSchema);
            Assert.IsFalse(definition.IsFile);
            Assert.That(definition.UnknownPropertySource, Is.EqualTo(PropertySource.MduFile));
        }

        [TestCase(PropertySource.None)]
        [TestCase(PropertySource.MduFile)]
        [TestCase(PropertySource.MorphologyFile)]
        [TestCase(PropertySource.SedimentFile)]
        public void CreateForUnknownProperty_WithPropertySource_ThenUnknownPropertySourceIsSet(PropertySource source)
        {
            WaterFlowFMPropertyDefinition definition =
                WaterFlowFMPropertyDefinitionCreator.CreateForCustomProperty(
                    string.Empty, string.Empty, string.Empty, source);

            Assert.That(definition.UnknownPropertySource, Is.EqualTo(source));
        }
    }
}
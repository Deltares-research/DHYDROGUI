using System.Reflection;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.PropertyGrid;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms.Properties
{
    [TestFixture]
    public class HydraulicRulePropertiesTest
    {
        [Test, Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new HydraulicRuleProperties { Data = new HydraulicRule() });
        }

        [Test]
        [SetCulture("en-US")]
        public void ResourcesCategoryAttributeOfInterpolationProperty_ShouldHaveCorrectText()
        {
            string interpolationPropertyName = TypeUtils.GetMemberName<HydraulicRuleProperties>(p => p.Interpolation);
            PropertyInfo interpolationPropertyInfo = TypeUtils.GetPropertyInfo(typeof(HydraulicRuleProperties),
                                                                               interpolationPropertyName);

            string categoryPropertyName = TypeUtils.GetMemberName<ResourcesCategoryAttribute>(a => a.Category);
            Assert.That(interpolationPropertyInfo,
                        Has.Attribute<ResourcesCategoryAttribute>()
                           .Property(categoryPropertyName)
                           .EqualTo("Interpolation"));
        }
    }
}
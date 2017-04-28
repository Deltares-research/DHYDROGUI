using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms.PropertyGrid
{
    [TestFixture]
    public class WaterFlowModel1DPropertiesTest
    {
        [Test, Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new WaterFlowModel1DProperties { Data = new WaterFlowModel1D() });
        }


        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowRoughnessProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new WaterFlowModel1DRoughnessProperties(new WaterFlowModel1D()));
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [TestCase(false)]
        [TestCase(true)]
        public void ValidateDynamicAttributesForRoughnessProperties(bool useRoughness)
        {
            var model = new WaterFlowModel1D();
            var roughProperties = new WaterFlowModel1DRoughnessProperties(model);

            model.UseReverseRoughness = useRoughness;
            Assert.That(model.UseReverseRoughness, Is.EqualTo(useRoughness));

            Assert.That(roughProperties.ValidateDynamicAttributes("UseReverseRoughnessInCalculation"),
                Is.EqualTo(!useRoughness));
        }
    }
}
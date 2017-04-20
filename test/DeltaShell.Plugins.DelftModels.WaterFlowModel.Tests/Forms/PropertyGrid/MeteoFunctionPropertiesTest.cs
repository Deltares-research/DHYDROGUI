using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms.PropertyGrid
{
    [TestFixture]
    public class MeteoFunctionPropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowSalinityProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new WaterFlowModel1DSalinityProperties ( new WaterFlowModel1D() ));
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [TestCase(false, DispersionFormulationType.Constant, 0)]
        [TestCase(false, DispersionFormulationType.ThatcherHarleman, 0)]
        [TestCase(true, DispersionFormulationType.Constant, 0)]
        [TestCase(true, DispersionFormulationType.ThatcherHarleman, 0)]
        [TestCase(false, DispersionFormulationType.Constant, 23)]
        [TestCase(false, DispersionFormulationType.ThatcherHarleman, 23)]
        [TestCase(true, DispersionFormulationType.Constant, 23)]
        [TestCase(true, DispersionFormulationType.ThatcherHarleman, 23)]
        public void ValidateDynamicAttributesForSalinityProperties(bool useSalinity, DispersionFormulationType dispFormulationType, int dispCovValue)
        {
            var model = new WaterFlowModel1D();
            var saltProperties = new WaterFlowModel1DSalinityProperties(model);

            model.UseSalt = useSalinity;
            model.UseSaltInCalculation = useSalinity;
            Assert.That(model.UseSalt, Is.EqualTo(useSalinity));
            Assert.That(model.UseSaltInCalculation, Is.EqualTo(useSalinity));

            model.DispersionFormulationType = dispFormulationType;
            if (useSalinity)
            {
                model.DispersionCoverage.DefaultValue = dispCovValue;
                Assert.That(model.DispersionCoverage.DefaultValue, Is.EqualTo(dispCovValue));
            }

            Assert.That(saltProperties.ValidateDynamicAttributes("UseSaltInCalculation"), Is.EqualTo(!useSalinity));

            Assert.That(saltProperties.ValidateDynamicAttributes("SalinityPath"), Is.EqualTo(model.DispersionFormulationType == DispersionFormulationType.Constant));

            Assert.That(saltProperties.ValidateDynamicAttributes("DispersionFormulationType"), Is.EqualTo(model.DispersionCoverage == null || !useSalinity));

        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMorphologyProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new WaterFlowModel1DMorphologyProperties(new WaterFlowModel1D()));
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [TestCase(false)]
        [TestCase(true)]
        public void ValidateDynamicAttributesForMorphologyProperties(bool useMorphology)
        {
            var model = new WaterFlowModel1D();
            var morphologyProperties = new WaterFlowModel1DMorphologyProperties(model);

            model.UseMorphology = useMorphology;
            Assert.That(model.UseMorphology, Is.EqualTo(useMorphology));

            Assert.That(morphologyProperties.ValidateDynamicAttributes("MorphologyPath"),
                Is.EqualTo(!useMorphology));
            Assert.That(morphologyProperties.ValidateDynamicAttributes("SedimentPath"),
                Is.EqualTo(!useMorphology));
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowTemperatureProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new WaterFlowModel1DTemperatureProperties(new WaterFlowModel1D()));
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [TestCase(false, TemperatureModelType.Transport)]
        [TestCase(false, TemperatureModelType.Composite)]
        [TestCase(false, TemperatureModelType.Excess)]
        [TestCase(true, TemperatureModelType.Transport)]
        [TestCase(true, TemperatureModelType.Composite)]
        [TestCase(true, TemperatureModelType.Excess)]
        public void ValidateDynamicAttributesForTemperatureProperties(bool useTemperature, TemperatureModelType tempModelType)
        {
            var model = new WaterFlowModel1D();
            var tempProperties = new WaterFlowModel1DTemperatureProperties(model);

            model.UseTemperature = useTemperature;
            Assert.That(model.UseTemperature, Is.EqualTo(useTemperature));
            model.TemperatureModelType = tempModelType;
            Assert.That(model.TemperatureModelType, Is.EqualTo(tempModelType));

            if (!useTemperature)
            {
                Assert.That(tempProperties.ValidateDynamicAttributes("TemperatureModelType"), Is.EqualTo(true));
                Assert.That(tempProperties.ValidateDynamicAttributes("BackgroundTemperature"), Is.EqualTo(true));
                Assert.That(tempProperties.ValidateDynamicAttributes("SurfaceArea"), Is.EqualTo(true));
                Assert.That(tempProperties.ValidateDynamicAttributes("AtmosphericPressure"), Is.EqualTo(true));
                Assert.That(tempProperties.ValidateDynamicAttributes("HeatCapacityWater"), Is.EqualTo(true));
                Assert.That(tempProperties.ValidateDynamicAttributes("DaltonNumber"), Is.EqualTo(true));
                Assert.That(tempProperties.ValidateDynamicAttributes("StantonNumber"), Is.EqualTo(true));
            }
            else
            {
                Assert.That(tempProperties.ValidateDynamicAttributes("TemperatureModelType"), Is.EqualTo(false));
                Assert.That(tempProperties.ValidateDynamicAttributes("BackgroundTemperature"), Is.EqualTo(!(model.TemperatureModelType == TemperatureModelType.Composite || model.TemperatureModelType == TemperatureModelType.Excess)));

                Assert.That(tempProperties.ValidateDynamicAttributes("SurfaceArea"), Is.EqualTo(model.TemperatureModelType != TemperatureModelType.Composite));
                Assert.That(tempProperties.ValidateDynamicAttributes("AtmosphericPressure"), Is.EqualTo(model.TemperatureModelType != TemperatureModelType.Composite));
                Assert.That(tempProperties.ValidateDynamicAttributes("HeatCapacityWater"), Is.EqualTo(model.TemperatureModelType != TemperatureModelType.Composite));

                Assert.That(tempProperties.ValidateDynamicAttributes("StantonNumber"), Is.EqualTo(model.TemperatureModelType != TemperatureModelType.Composite));
                Assert.That(tempProperties.ValidateDynamicAttributes("DaltonNumber"), Is.EqualTo(model.TemperatureModelType != TemperatureModelType.Composite));
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowAdvancedOptions()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new WaterFlowModel1DAdvancedOptions(new WaterFlowModel1D()));
        }
    }
}
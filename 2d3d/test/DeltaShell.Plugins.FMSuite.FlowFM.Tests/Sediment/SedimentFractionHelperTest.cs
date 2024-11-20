using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Sediment
{
    [TestFixture]
    public class SedimentFractionHelperTest
    {
        [Test]
        public void GetSedimentationTypes_Always_ReturnsExpectedTypesAndValues()
        {
            // Call
            List<ISedimentType> sedimentationTypes = SedimentFractionHelper.GetSedimentationTypes();

            // Assert
            Assert.That(sedimentationTypes, Has.Count.EqualTo(3));
            Assert.That(sedimentationTypes, Is.All.InstanceOf<SedimentType>());

            AssertSandType(sedimentationTypes[0]);
            AssertMudType(sedimentationTypes[1]);
            AssertBedLoadType(sedimentationTypes[2]);
        }

        private static void AssertSandType(ISedimentType sedimentType)
        {
            Assert.That(sedimentType.Name, Is.EqualTo("Sand"));
            Assert.That(sedimentType.Key, Is.EqualTo("sand"));

            AssertSandProperties(sedimentType.Properties);
        }

        private static void AssertMudType(ISedimentType sedimentType)
        {
            Assert.That(sedimentType.Name, Is.EqualTo("Mud"));
            Assert.That(sedimentType.Key, Is.EqualTo("mud"));

            AssertMudProperties(sedimentType.Properties);
        }

        private static void AssertBedLoadType(ISedimentType sedimentType)
        {
            Assert.That(sedimentType.Name, Is.EqualTo("Bed-load"));
            Assert.That(sedimentType.Key, Is.EqualTo("bedload"));

            AssertBedLoadProperties(sedimentType.Properties);
        }

        private static void AssertSandProperties(IEventedList<ISedimentProperty> actualSandProperties)
        {
            Assert.That(actualSandProperties, Has.Count.EqualTo(7));

            AssertSpatiallyVaryingProperty("SedConc", 0, 0, false, double.MaxValue, true, "kg/m3", "Initial Concentration", true, false,
                                           actualSandProperties[0] as SpatiallyVaryingSedimentProperty<double>);
            AssertSpatiallyVaryingProperty("IniSedThick", 5, 0, false, double.MaxValue, true, "m", "Initial sediment layer thickness at bed", false, false,
                                           actualSandProperties[1] as SpatiallyVaryingSedimentProperty<double>);

            AssertSedimentProperty("FacDss", 1, 0.6, false, 1, false, "-", "Factor for suspended sediment diameter", true,
                                   actualSandProperties[2] as SedimentProperty<double>);
            AssertSedimentProperty("RhoSol", 2650, 0, true, 10000, true, "kg/m3", "Specific density", false,
                                   actualSandProperties[3] as SedimentProperty<double>);
            AssertSedimentProperty("TraFrm", -1, -2, false, 18, false, string.Empty, "Integer selecting the transport formula", true,
                                   actualSandProperties[4] as SedimentProperty<int>);
            AssertSedimentProperty("CDryB", 1600, 0, true, 10000, true, "kg/m3", "Dry bed density", false,
                                   actualSandProperties[5] as SedimentProperty<double>);
            AssertSedimentProperty("SedDia", 0.0002, 0.000063, false, double.MaxValue, false, "m", "Median sediment diameter (D50)", false,
                                   actualSandProperties[6] as SedimentProperty<double>);
        }

        private static void AssertMudProperties(IEventedList<ISedimentProperty> actualSandProperties)
        {
            Assert.That(actualSandProperties, Has.Count.EqualTo(9));

            AssertSpatiallyVaryingProperty("SedConc", 0, 0, false, double.MaxValue, true, "kg/m3", "Initial Concentration", true, false,
                                           actualSandProperties[0] as SpatiallyVaryingSedimentProperty<double>);
            AssertSpatiallyVaryingProperty("IniSedThick", 5, 0, false, double.MaxValue, true, "m", "Initial sediment layer thickness at bed", false, false,
                                           actualSandProperties[1] as SpatiallyVaryingSedimentProperty<double>);

            AssertSedimentProperty("FacDss", 1, 0.6, false, 1, false, "-", "Factor for suspended sediment diameter", true,
                                   actualSandProperties[2] as SedimentProperty<double>);
            AssertSedimentProperty("RhoSol", 2650, 0, true, 10000, true, "kg/m3", "Specific density", false,
                                   actualSandProperties[3] as SedimentProperty<double>);
            AssertSedimentProperty("TraFrm", -3, -3, false, -3, false, string.Empty, "Integer selecting the transport formula", true,
                                   actualSandProperties[4] as SedimentProperty<int>);
            AssertSedimentProperty("CDryB", 500, 0, true, 10000, true, "kg/m3", "Dry bed density", false,
                                   actualSandProperties[5] as SedimentProperty<double>);
            AssertSedimentProperty("SalMax", 31, 0.01, false, 391, true, "ppt", "Salinity for saline settling velocity", false,
                                   actualSandProperties[6] as SedimentProperty<double>);
            AssertSedimentProperty("WS0", 0.00025, 0, true, 1, true, "m/s", "Settling velocity fresh water", false,
                                   actualSandProperties[7] as SedimentProperty<double>);
            AssertSedimentProperty("WSM", 0.00025, 0, true, 1, true, "m/s", "Settling velocity saline water", false,
                                   actualSandProperties[8] as SedimentProperty<double>);
        }

        private static void AssertBedLoadProperties(IEventedList<ISedimentProperty> actualSandProperties)
        {
            Assert.That(actualSandProperties, Has.Count.EqualTo(5));

            AssertSpatiallyVaryingProperty("IniSedThick", 5, 0, false, double.MaxValue, true, "m", "Initial sediment layer thickness at bed", false, false,
                                           actualSandProperties[0] as SpatiallyVaryingSedimentProperty<double>);

            AssertSedimentProperty("RhoSol", 2650, 0, true, 10000, true, "kg/m3", "Specific density", false,
                                   actualSandProperties[1] as SedimentProperty<double>);
            AssertSedimentProperty("TraFrm", -1, -2, false, 18, false, string.Empty, "Integer selecting the transport formula", true,
                                   actualSandProperties[2] as SedimentProperty<int>);
            AssertSedimentProperty("CDryB", 1600, 0, true, 10000, true, "kg/m3", "Dry bed density", false,
                                   actualSandProperties[3] as SedimentProperty<double>);
            AssertSedimentProperty("SedDia", 0.0002, 0.000063, false, double.MaxValue, false, "m", "Median sediment diameter (D50)", false,
                                   actualSandProperties[4] as SedimentProperty<double>);
        }

        private static void AssertSpatiallyVaryingProperty<T>(string name,
                                                              T defaultValue, T minValue, bool minIsOpened, T maxValue, bool maxIsOpened,
                                                              string unit,
                                                              string description, bool isSpatiallyVarying, bool isMduOnly,
                                                              SpatiallyVaryingSedimentProperty<T> actualProperty)
        {
            AssertSedimentProperty(name, defaultValue, minValue, minIsOpened, maxValue, maxIsOpened, unit, description, isMduOnly, actualProperty);
            Assert.That(actualProperty.IsSpatiallyVarying, Is.EqualTo(isSpatiallyVarying));
        }

        private static void AssertSedimentProperty<T>(string name,
                                                      T defaultValue, T minValue, bool minIsOpened, T maxValue, bool maxIsOpened,
                                                      string unit,
                                                      string description, bool isMduOnly,
                                                      SedimentProperty<T> actualProperty)
        {
            Assert.That(actualProperty.Name, Is.EqualTo(name));
            Assert.That(actualProperty.DefaultValue, Is.EqualTo(defaultValue));
            Assert.That(actualProperty.MinValue, Is.EqualTo(minValue));
            Assert.That(actualProperty.MinIsOpened, Is.EqualTo(minIsOpened));
            Assert.That(actualProperty.MaxValue, Is.EqualTo(maxValue));
            Assert.That(actualProperty.MaxIsOpened, Is.EqualTo(maxIsOpened));
            Assert.That(actualProperty.Unit, Is.EqualTo(unit));
            Assert.That(actualProperty.Description, Is.EqualTo(description));
            Assert.That(actualProperty.MduOnly, Is.EqualTo(isMduOnly));
        }
    }
}
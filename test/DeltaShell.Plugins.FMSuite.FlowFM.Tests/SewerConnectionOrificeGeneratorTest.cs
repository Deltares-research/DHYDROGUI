using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class SewerConnectionOrificeGeneratorTest : SewerFeatureFactoryTestHelper
    {
        [Test]
        public void CreateSewerConnectionReturnsObjectWithExpectedValues()
        {
            #region Setting expected values
            var orificeId = "Obj123";
            var sourceCompartmentId = "cmp001";
            var targetCompartmentId = "cmp002";

            var connectionType = SewerConnectionMapping.ConnectionType.Orifice;
            var connectionTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(connectionType);

            var levelStart = 2.0;
            var levelEnd = 2.5;
            var length = 5.0;

            var waterType = SewerConnectionWaterType.DryWater;
            var waterTypeString = EnumDescriptionAttributeTypeConverter.GetEnumDescription(waterType);
            #endregion

            //Non value given
            var nvgString = string.Empty;
            var nvgDouble = 0.0;

            var connectionGwswElement = GetSewerConnectionGwswElement(orificeId, sourceCompartmentId, targetCompartmentId, connectionTypeString,
                levelStart, levelEnd, nvgString, length, nvgString, nvgString, waterTypeString, nvgDouble,
                nvgDouble, nvgDouble, nvgDouble);

            var orifice = new SewerOrificeGenerator().Generate(connectionGwswElement) as GwswConnectionOrifice;
            Assert.IsNotNull(orifice);

            Assert.That(orifice.Name, Is.EqualTo(orificeId));
            Assert.That(orifice.LevelSource, Is.EqualTo(levelStart));
            Assert.That(orifice.LevelTarget, Is.EqualTo(levelEnd));
            Assert.That(orifice.Length, Is.EqualTo(length));
            Assert.That(orifice.WaterType, Is.EqualTo(waterType));
            Assert.That(orifice.SourceCompartmentName, Is.EqualTo(sourceCompartmentId));
            Assert.That(orifice.TargetCompartmentName, Is.EqualTo(targetCompartmentId));
        }

        [Test]
        public void GenerateOrificeFromGwswStructureElementReturnsValidObject()
        {
            var orificeId = "orifice123";
            var bottomLevel = 30.0;
            var contractionCoef = 0.5;
            var maxDischarge = 1;
            var structureOrificeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, orificeId, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Orifice), string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.ContractionCoefficient, contractionCoef.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.MaxDischarge, maxDischarge.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble)
                }
            };

            var network = new HydroNetwork();
            
            var generator = new SewerOrificeGenerator();
            var createdOrifice = generator.Generate(structureOrificeGwswElement) as Orifice;
            Assert.IsNotNull(createdOrifice);
            Assert.AreEqual(orificeId, createdOrifice.Name);
            Assert.AreEqual(bottomLevel, createdOrifice.BottomLevel);
            Assert.AreEqual(contractionCoef, createdOrifice.ContractionCoefficent);
            Assert.AreEqual(maxDischarge, createdOrifice.MaxDischarge);
        }

        [Test]
        public void AfterAddingAConnectionOrificeYouCanExtendItsDefinitionWithTheStructure()
        {
            var orificeId = "orifice123";
            var bottomLevel = 30.0;
            var contractionCoef = 0.5;
            var maxDischarge = 1;
            var structureOrificeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, orificeId, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, EnumDescriptionAttributeTypeConverter.GetEnumDescription(SewerConnectionMapping.ConnectionType.Orifice), string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.ContractionCoefficient, contractionCoef.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.MaxDischarge, maxDischarge.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                }
            };
            
            var createdElement = new SewerOrificeGenerator().Generate(structureOrificeGwswElement) as Orifice;
            Assert.IsNotNull(createdElement);

            var createdOrifice = createdElement;
            Assert.IsNotNull(createdOrifice);
            Assert.AreEqual(orificeId, createdOrifice.Name);
            Assert.AreEqual(bottomLevel, createdOrifice.BottomLevel);
            Assert.AreEqual(contractionCoef, createdOrifice.ContractionCoefficent);
            Assert.AreEqual(maxDischarge, createdOrifice.MaxDischarge);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
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
            var connectionTypeString = connectionType.GetDescription();

            var levelStart = 2.0;
            var levelEnd = 2.5;
            var length = 5.0;

            var waterType = SewerConnectionWaterType.DryWater;
            var waterTypeString = waterType.GetDescription();
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
            var crestLevel = 30.0;
            var contractionCoef = 0.5;
            var maxDischarge = 1;
            var structureOrificeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, orificeId, string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, SewerConnectionMapping.ConnectionType.Orifice.GetDescription(), string.Empty),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.BottomLevel, crestLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.ContractionCoefficient, contractionCoef.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.MaxDischarge, maxDischarge.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble)
                }
            };

            var generator = new SewerOrificeGenerator();
            var createdOrifice = generator.Generate(structureOrificeGwswElement) as Orifice;
            
            Assert.IsNotNull(createdOrifice);
            Assert.That(createdOrifice.Name, Is.EqualTo(orificeId));
            Assert.That(createdOrifice.CrestLevel, Is.EqualTo(crestLevel));
            Assert.That(createdOrifice.MaxDischarge, Is.EqualTo(maxDischarge));

            var weirFormula = createdOrifice.WeirFormula as GatedWeirFormula;
            Assert.IsNotNull(weirFormula);
            Assert.That(weirFormula.ContractionCoefficient, Is.EqualTo(contractionCoef));
            Assert.That(weirFormula.UseMaxFlowPos, Is.True);
            Assert.That(weirFormula.UseMaxFlowNeg, Is.True);
        }
        
        [Test]
        public void GenerateOrificeFromGwswStructureElementWithoutOptionalAttributesReturnsValidObjectWithDefaultValues()
        {
            var structureOrificeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Structure.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, "orifice123", string.Empty),
                }
            };

            var generator = new SewerOrificeGenerator();
            var createdOrifice = generator.Generate(structureOrificeGwswElement) as Orifice;
            
            Assert.IsNotNull(createdOrifice);
            Assert.That(createdOrifice.CrestLevel, Is.EqualTo(1.0));
            Assert.That(createdOrifice.MaxDischarge, Is.EqualTo(0.0));

            var weirFormula = createdOrifice.WeirFormula as GatedWeirFormula;
            Assert.IsNotNull(weirFormula);
            Assert.That(weirFormula.ContractionCoefficient, Is.EqualTo(0.63));
            Assert.That(weirFormula.UseMaxFlowPos, Is.False);
            Assert.That(weirFormula.UseMaxFlowNeg, Is.False);
        }

        [Test]
        [TestCase("GSL", false, false, false)]
        [TestCase("OPN", true, true, false)]
        [TestCase("1_2", true, false, false)]
        [TestCase("2_1", false, true, false)]
        [TestCase("", false, false, true)]
        [TestCase(" ", false, false, true)]
        [TestCase("null", false, false, true)]
        [TestCase(null, false, false, true)]
        public void Generate_IsValidGwswSewerConnection_SetsExpectedFlowDirection(string flowDirectionString,
                                                                                  bool expectedPositiveFlow,
                                                                                  bool expectedNegativeFlow,
                                                                                  bool expectedLogMessagge)
        {
            // Setup
            const string randomString = "randomString";
            var random = new Random(80085);

            GwswElement gwswElement = GetSewerConnectionGwswElement(randomString, randomString, randomString, randomString,
                                                                    random.NextDouble(), random.NextDouble(), flowDirectionString, random.NextDouble(),
                                                                    randomString, randomString, randomString, random.NextDouble(),
                                                                    random.NextDouble(), random.NextDouble(), random.NextDouble());

            var generator = new SewerOrificeGenerator();

            // Call
            ISewerFeature orificeSewerFeature = null;
            if (expectedLogMessagge)
            {
                TestHelper.AssertAtLeastOneLogMessagesContains(()=> orificeSewerFeature = generator.Generate(gwswElement), string.Format(GWSW.Properties.Resources.SewerFeatureFactory_GetValueFromDescription_Type__0__is_not_recognized__please_check_the_syntax, flowDirectionString));
            }
            else
            {
                orificeSewerFeature = generator.Generate(gwswElement);
            }

            // Assert
            Assert.That(orificeSewerFeature, Is.TypeOf<GwswConnectionOrifice>());

            var orifice = (Orifice) orificeSewerFeature;
            Assert.That(orifice.AllowPositiveFlow, Is.EqualTo(expectedPositiveFlow));
            Assert.That(orifice.AllowNegativeFlow, Is.EqualTo(expectedNegativeFlow));
        }
    }
}
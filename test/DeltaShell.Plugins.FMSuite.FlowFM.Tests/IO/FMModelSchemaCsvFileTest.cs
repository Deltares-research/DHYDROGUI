using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FMModelSchemaCsvFileTest
    {
        [Test]
        public void LoadCharEnums()
        {
            var modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>("plugins\\DeltaShell.Plugins.FMSuite.FlowFM\\dflowfm-properties.csv", "MduGroup");
            Assert.Greater(modelPropertySchema.PropertyDefinitions.Count, 50);

            var tunitProperty = modelPropertySchema.PropertyDefinitions[KnownProperties.Tunit];
            var tunitEnum = tunitProperty.DataType;
            var values = GetEnumValues(tunitEnum);

            Assert.AreEqual("H", EnumDescriptionAttributeTypeConverter.GetEnumDisplayName(values[0]));
            Assert.AreEqual("S", tunitProperty.DefaultValueAsString);
        }

        [Test]
        public void LoadCharEnumsTestParser()
        {
            var modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>("plugins\\DeltaShell.Plugins.FMSuite.FlowFM\\dflowfm-properties.csv", "MduGroup");
            Assert.Greater(modelPropertySchema.PropertyDefinitions.Count, 50);

            Assert.Greater(modelPropertySchema.PropertyDefinitions.Count, 50);

            var tunitProperty = modelPropertySchema.PropertyDefinitions[KnownProperties.Tunit];
            var tunitEnum = tunitProperty.DataType;
            var values = GetEnumValues(tunitEnum);

            var hourValue = values[0];

            Assert.AreEqual("H", DataTypeValueParser.ToString(hourValue, tunitEnum));
            Assert.AreEqual(hourValue, DataTypeValueParser.FromString("H", tunitEnum));

        }

        [Test]
        public void LoadIntEnums()
        {
            var modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>("plugins\\DeltaShell.Plugins.FMSuite.FlowFM\\dflowfm-properties.csv", "MduGroup");
            Assert.Greater(modelPropertySchema.PropertyDefinitions.Count, 50);

            var convProperty = modelPropertySchema.PropertyDefinitions[KnownProperties.Conveyance2d];
            var convEnum = convProperty.DataType;
            var values = GetEnumValues(convEnum);

            Assert.AreEqual("-1", EnumDescriptionAttributeTypeConverter.GetEnumDisplayName(values[0]));
            Assert.AreEqual("R=HU", EnumDescriptionAttributeTypeConverter.GetEnumDescription(values[0]));
            Assert.AreEqual("-1", convProperty.DefaultValueAsString);
        }

        [Test]
        public void LoadConveyance2dEnumAndVerifyThatItHasNotChanged()
        {
            //if changed check fm validationrule 'WaterFlowFMModelDefinitionValidator'
            var modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>("plugins\\DeltaShell.Plugins.FMSuite.FlowFM\\dflowfm-properties.csv", "MduGroup");
            
            var convProperty = modelPropertySchema.PropertyDefinitions[KnownProperties.Conveyance2d];
            var convEnum = convProperty.DataType;
            var values = GetEnumValues(convEnum);
            Assert.AreEqual(5, values.Count, "The enum size of "+ KnownProperties.Conveyance2d + " has changed!");
            Assert.AreEqual("R=HU", EnumDescriptionAttributeTypeConverter.GetEnumDescription(values[0]));
            Assert.AreEqual(((int)Conveyance2DType.RisHU).ToString(), EnumDescriptionAttributeTypeConverter.GetEnumDisplayName(values[0]));
            Assert.AreEqual("R=H", EnumDescriptionAttributeTypeConverter.GetEnumDescription(values[1]));
            Assert.AreEqual(((int)Conveyance2DType.RisH).ToString(), EnumDescriptionAttributeTypeConverter.GetEnumDisplayName(values[1]));
            Assert.AreEqual("R=A/P", EnumDescriptionAttributeTypeConverter.GetEnumDescription(values[2]));
            Assert.AreEqual(((int)Conveyance2DType.RisAperP).ToString(), EnumDescriptionAttributeTypeConverter.GetEnumDisplayName(values[2]));
            Assert.AreEqual("K=analytic-1D conv", EnumDescriptionAttributeTypeConverter.GetEnumDescription(values[3]));
            Assert.AreEqual(((int)Conveyance2DType.Kisanalytic1Dconv).ToString(), EnumDescriptionAttributeTypeConverter.GetEnumDisplayName(values[3]));
            Assert.AreEqual("K=analytic-2D conv", EnumDescriptionAttributeTypeConverter.GetEnumDescription(values[4]));
            Assert.AreEqual(((int)Conveyance2DType.Kisanalytic2Dconv).ToString(), EnumDescriptionAttributeTypeConverter.GetEnumDisplayName(values[4]));
        }

        [Test]
        public void LoadIntEnumsNotStartingAtZero()
        {
            var modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>("plugins\\DeltaShell.Plugins.FMSuite.FlowFM\\dflowfm-properties.csv", "MduGroup");
            Assert.Greater(modelPropertySchema.PropertyDefinitions.Count, 50);

            var convProperty = modelPropertySchema.PropertyDefinitions["icgsolver"];
            var convEnum = convProperty.DataType;
            var values = GetEnumValues(convEnum);

            Assert.AreEqual("1", EnumDescriptionAttributeTypeConverter.GetEnumDisplayName(values[0]));
            Assert.AreEqual("sobekGS_OMP", EnumDescriptionAttributeTypeConverter.GetEnumDescription(values[0]));
            Assert.AreEqual("4", convProperty.DefaultValueAsString);
        }

        [Test]
        public void FixedWeirScheme()
        {
            var modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>("plugins\\DeltaShell.Plugins.FMSuite.FlowFM\\dflowfm-properties.csv", "MduGroup");
            Assert.Greater(modelPropertySchema.PropertyDefinitions.Count, 50);

            var convProperty = modelPropertySchema.PropertyDefinitions["fixedweirscheme"];

            Assert.AreEqual("9", convProperty.MaxValueAsString);
        }

        private static IList<Enum> GetEnumValues(Type enumType)
        {
            return Enum.GetValues(enumType).OfType<Enum>().OrderBy(o => o.ToString()).ToList();
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // trigger creation here, to make sure it's not triggered after these tests have ran.
            new WaterFlowFMModelDefinition();
        }
    }
}
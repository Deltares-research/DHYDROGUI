using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Logging;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class SewerCrossSectionDefinitionGeneratorTest : SewerFeatureFactoryTestHelper
    {
        private const string ProfileId = "PRO1";
        private readonly string unknownMaterialValue = SewerProfileMapping.SewerProfileMaterial.Unknown.GetDescription();
        private readonly string concreteMaterialValue = SewerProfileMapping.SewerProfileMaterial.Concrete.GetDescription();

        [TestCase(SewerFeatureType.Node)]
        [TestCase(SewerFeatureType.Connection)]
        [TestCase(SewerFeatureType.Discharge)]
        [TestCase(SewerFeatureType.Distribution)]
        [TestCase(SewerFeatureType.Meta)]
        [TestCase(SewerFeatureType.Runoff)]
        [TestCase(SewerFeatureType.Structure)]
        [TestCase(SewerFeatureType.Surface)]
        public void GivenGwswElementWithElementTypeNameUnequalToSewerProfile_WhenGeneratingSewerProfile_ThenReturnNull(SewerFeatureType type)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = type.ToString()
            };
            GenerateCrossSectionDefinitionAndCheckForNull<CircleCrossSectionShapeGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<RectangleCrossSectionShapeGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<EggCrossSectionShapeGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<TrapezoidCrossSectionShapeGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<ArchCrossSectionShapeGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<CunetteCrossSectionShapeGenerator>(profileGwswElement);
            GenerateCrossSectionDefinitionAndCheckForNull<DefaultCrossSectionShapeGenerator>(profileGwswElement);
        }

        #region Circle sewer profile shape
        
        [TestCase(SewerProfileMapping.SewerProfileMaterial.CastIron, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.Concrete, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.Hdpe, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.Masonry, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.Polyester, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.Polyvinylchlorid, 1.6)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.SheetMetal, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.Steel, 1.7)]
        [TestCase(SewerProfileMapping.SewerProfileMaterial.StoneWare, 1.7)]
        public void GivenCircleProfileGwswElement_WhenGeneratingSewerProfileCircle_ThenReturnCircleShapeWithCorrectDiameter(SewerProfileMapping.SewerProfileMaterial material, double expectedDiameter)
        {
            var materialString = material.GetDescription();
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1700", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, materialString, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CircleCrossSectionShapeGenerator, CrossSectionStandardShapeCircle>(logHandler,profileGwswElement, ProfileId, expectedDiameter, materialString);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithoutWidthDefined_WhenGeneratingSewerProfileCircle_ThenReturnDefaultCircleSewerProfileAndLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CircleCrossSectionShapeGenerator, CrossSectionStandardShapeCircle>(logHandler, profileGwswElement, ProfileId, 0.160d, unknownMaterialValue);
            logHandler.Received().ReportWarningFormat("Sewer profile '{0}' is missing its {1}. Default profile property values are used for this profile.",ProfileId,"width");
        }

        #endregion

        #region Rectangle sewer profile shape

        [Test]
        public void GivenGwswElement_WhenGeneratingSewerProfileRectangle_ThenReturnRectangleShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "2000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1200", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, concreteMaterialValue, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<RectangleCrossSectionShapeGenerator, CrossSectionStandardShapeRectangle>(logHandler, profileGwswElement, ProfileId, 2.0, 1.2, concreteMaterialValue);
        }

        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileWidth)]
        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileHeight)]
        [TestCase("fakeKey")]
        public void GivenSewerProfileGwswElementWithoutHeightOrWidthDefined_WhenGeneratingSewerProfileRectangle_ThenReturnDefaultRectangleSewerProfileAndLogMessage(string key)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(key, "2000", string.Empty, TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<RectangleCrossSectionShapeGenerator, CrossSectionStandardShapeRectangle>(logHandler, profileGwswElement, ProfileId, 1.0, 1.0, unknownMaterialValue);
            logHandler.Received().ReportWarningFormat("Sewer profile '{0}' is missing its {1}. Default profile property values are used for this profile.", ProfileId, "width and/or height");
        }

        #endregion

        #region Egg sewer profile shape

        [Test]
        public void GivenGwswElement_WhenGeneratingSewerProfileEgg_ThenReturnEggShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, concreteMaterialValue, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<EggCrossSectionShapeGenerator, CrossSectionStandardShapeEgg>(logHandler, profileGwswElement, ProfileId, 0.25, 0.375, concreteMaterialValue);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithoutWidthDefined_WhenGeneratingSewerProfileEgg_ThenReturnDefaultEggShapeAndLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<EggCrossSectionShapeGenerator, CrossSectionStandardShapeEgg>(logHandler, profileGwswElement, ProfileId, 2.0, 3.0, unknownMaterialValue);
            logHandler.Received().ReportWarningFormat("Sewer profile '{0}' is missing its {1}. Default profile property values are used for this profile.", ProfileId, "width");
        }

        [Test]
        public void GivenGwswElementWithWidthAndHeightInCorrectProportion_WhenGeneratingSewerProfileEgg_ThenReturnEggShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "375", string.Empty, TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<EggCrossSectionShapeGenerator, CrossSectionStandardShapeEgg>(logHandler, profileGwswElement, ProfileId, 0.25, 0.375, unknownMaterialValue);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenGeneratingSewerProfileEgg_ThenLogMessageIsGivenToUser()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "400", string.Empty, TypeDouble)
                }
            };

            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var generator = new EggCrossSectionShapeGenerator(logHandler);
            generator.Generate(profileGwswElement);
            logHandler.Received().ReportWarningFormat("The width and height of sewer profile '{0}' are not in the right proportion {1}. Width is now {2} mm and height is now {3} mm.", ProfileId, "(2:3)", 250d, 375d);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenGeneratingSewerProfileEgg_ThenHeightFromGwswElementIsIgnored()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "375", string.Empty, TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<EggCrossSectionShapeGenerator, CrossSectionStandardShapeEgg>(logHandler, profileGwswElement, ProfileId, 0.25, 0.375, unknownMaterialValue);
        }

        #endregion

        #region Cunette sewer profile shape

        [Test]
        public void GivenGwswElement_WhenGeneratingSewerProfileCunette_ThenReturnCunetteShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "2000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, concreteMaterialValue, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CunetteCrossSectionShapeGenerator, CrossSectionStandardShapeCunette>(logHandler, profileGwswElement, ProfileId, 2.0, 1.268, concreteMaterialValue);
        }

        [Test]
        public void GivenGwswElementWithWidthAndHeightInCorrectProportion_WhenGeneratingSewerProfileCunette_ThenReturnCunetteShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "2000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1268", string.Empty, TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CunetteCrossSectionShapeGenerator, CrossSectionStandardShapeCunette>(logHandler, profileGwswElement, ProfileId, 2.0, 1.268, unknownMaterialValue);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithoutWidthDefined_WhenGeneratingSewerProfileCunette_ThenReturnDefaultCunetteShapeAndLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CunetteCrossSectionShapeGenerator, CrossSectionStandardShapeCunette>(logHandler, profileGwswElement, ProfileId, 1.0, 0.634, unknownMaterialValue);
            logHandler.Received().ReportWarningFormat("Sewer profile '{0}' is missing its {1}. Default profile property values are used for this profile.", ProfileId, "width");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenGeneratingSewerProfileCunette_ThenLogMessageIsGivenToUser()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "400", string.Empty, TypeDouble)
                }
            };

            ILogHandler logHandler = Substitute.For<ILogHandler>();

            var cunetteGenerator = new CunetteCrossSectionShapeGenerator(logHandler);
            
            cunetteGenerator.Generate(profileGwswElement);
            logHandler.Received().ReportWarningFormat("The width and height of sewer profile '{0}' are not in the right proportion {1}. Width is now {2} mm and height is now {3} mm.", ProfileId, "(1:0.634)", 250d, 158.5);
        }

        [Test]
        public void GivenSewerProfileGwswElementWithWidthAndHeightNotInCorrectProportion_WhenGeneratingSewerProfileCunette_ThenHeightFromGwswElementIsIgnored()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "250", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "400", string.Empty, TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<CunetteCrossSectionShapeGenerator, CrossSectionStandardShapeCunette>(logHandler, profileGwswElement, ProfileId, 0.25, 0.1585, unknownMaterialValue);
        }

        #endregion

        #region Arch sewer profile shape

        [Test]
        public void GivenGwswElement_WhenGeneratingSewerProfileArch_ThenReturnArchShapeWithCorrectPropertyValues()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1200", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "2500", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, concreteMaterialValue, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<ArchCrossSectionShapeGenerator, CrossSectionStandardShapeArch>(logHandler, profileGwswElement, ProfileId, 1.2, 2.5, concreteMaterialValue, 2.5);
        }

        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileWidth)]
        [TestCase(SewerProfileMapping.PropertyKeys.SewerProfileHeight)]
        public void GivenGwswElementWithMissingValues_WhenGeneratingSewerProfileArch_ThenReturnDefaultArchShapeAndLogMessage(string key)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(key, "1000", string.Empty, TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<ArchCrossSectionShapeGenerator, CrossSectionStandardShapeArch>(logHandler, profileGwswElement, ProfileId, 1.0, 2.0, unknownMaterialValue, 1.0);
            logHandler.Received().ReportWarningFormat("Sewer profile '{0}' is missing its {1}. Default profile property values are used for this profile.", ProfileId, "width and/or height");
        }

        #endregion

        #region Trapezoid sewer profile shape

        [TestCase("2,5", "1,5")]
        [TestCase("2.5", "1.5")]
        public void GivenGwswElement_WhenGeneratingSewerProfileTrapezoid_ThenReturnTrapezoidShapeWithCorrectPropertyValues(string slope1, string slope2)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope1, slope1, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope2, slope2, string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileMaterial, concreteMaterialValue, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionShapeGenerator, CrossSectionStandardShapeTrapezium>(logHandler, profileGwswElement, ProfileId, 1.0, 2.0, 2.0, concreteMaterialValue);
        }

        [TestCase(SewerProfileMapping.PropertyKeys.Slope1)]
        [TestCase(SewerProfileMapping.PropertyKeys.Slope2)]
        public void GivenGwswElementWithOneMissingSlope_WhenGeneratingSewerProfileTrapezoid_ThenReturnTrapezoidWithPresentSlope(string slopeKey)
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(slopeKey, "2", string.Empty, TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionShapeGenerator, CrossSectionStandardShapeTrapezium>(logHandler, profileGwswElement, ProfileId, 1.0, 2.0, 2.0, unknownMaterialValue);

        }

        [Test]
        public void GivenSewerProfileGwswElementWithMissingWidth_WhenGeneratingSewerProfileTrapezoid_ThenReturnDefaultTrapezoidSewerProfileAndLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope1, "2,5", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope2, "1,5", string.Empty, TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionShapeGenerator, CrossSectionStandardShapeTrapezium>(logHandler, profileGwswElement, ProfileId, 10.0, 2.0, 20.0, unknownMaterialValue);
            logHandler.Received().ReportWarningFormat("Sewer profile '{0}' is missing its {1}. Default profile property values are used for this profile.", ProfileId, "width, height and/or slope");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithMissingHeight_WhenGeneratingSewerProfileTrapezoid_ThenReturnDefaultTrapezoidSewerProfile()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope1, "2,5", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope2, "1,5", string.Empty, TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionShapeGenerator, CrossSectionStandardShapeTrapezium>(logHandler, profileGwswElement, ProfileId, 10.0, 2.0, 20.0, unknownMaterialValue);
            logHandler.Received().ReportWarningFormat("Sewer profile '{0}' is missing its {1}. Default profile property values are used for this profile.", ProfileId, "width, height and/or slope");
        }

        [Test]
        public void GivenSewerProfileGwswElementWithMissingSlopes_WhenGeneratingSewerProfileTrapezoid_ThenReturnDefaultTrapezoidSewerProfile()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "1000", string.Empty, TypeDouble),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, "1000", string.Empty, TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateTrapezoidCrossSectionAndCheckShapeProperties<TrapezoidCrossSectionShapeGenerator, CrossSectionStandardShapeTrapezium>(logHandler, profileGwswElement, ProfileId, 10.0, 2.0, 20.0, unknownMaterialValue);
            logHandler.Received().ReportWarningFormat("Sewer profile '{0}' is missing its {1}. Default profile property values are used for this profile.",ProfileId,"width, height and/or slope");
        }

        #endregion

        #region Default sewer profile shape

        [Test]
        public void GivenDefaultDefinitionGenerator_WhenGeneratingSewerProfile_ThenReturnDefaultDefinitionAlways()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty),
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, "3000", "400", TypeDouble)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GenerateCrossSectionDefinitionAndCheckShapeProperties<DefaultCrossSectionShapeGenerator, CrossSectionStandardShapeCircle>(logHandler, profileGwswElement, ProfileId, 0.1, concreteMaterialValue);
        }

        [Test]
        public void GivenDefaultDefinitionGenerator_WhenGeneratingSewerProfile_ThenLogMessage()
        {
            var profileGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, ProfileId, string.Empty)
                }
            };
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var defaultGenerator = new DefaultCrossSectionShapeGenerator(logHandler);
            defaultGenerator.Generate(profileGwswElement);
            logHandler.Received().ReportWarningFormat(Properties.Resources.SewerFeatureFactory_CreateSewerProfile_Shape_was_not_defined_for_sewer_profile___0___in__Profiel_csv___A_default_round_profile_with_diameter_of_400_mm_is_used_for_this_profile_, ProfileId);
        }

        #endregion

        #region Test helpers

        private static void GenerateCrossSectionDefinitionAndCheckForNull<T>(GwswElement gwswElement) where T : ASewerCrossSectionShapeGenerator
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var generator = (T)Activator.CreateInstance(typeof(T), logHandler);
            var network = new HydroNetwork();
            generator.Generate(gwswElement);
            Assert.IsEmpty(network.SharedCrossSectionDefinitions);
        }

        private void GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(ILogHandler logHandler, GwswElement gwswElement, string expectedName, double expectedWidth, string expectedMaterial)
            where TGenerator : ASewerCrossSectionShapeGenerator
            where TShape : CrossSectionStandardShapeBase, new()
        {
            GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(logHandler, gwswElement, expectedName, expectedWidth, double.NaN, double.NaN, double.NaN, double.NaN, expectedMaterial);
        }

        private void GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(ILogHandler logHandler, GwswElement gwswElement, string expectedName, double expectedWidth, double expectedHeight, string expectedMaterial, double expectedArcHeight = double.NaN)
            where TGenerator : ASewerCrossSectionShapeGenerator
            where TShape : CrossSectionStandardShapeBase, new()
        {
            GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(logHandler, gwswElement, expectedName, expectedWidth, expectedHeight, expectedArcHeight, double.NaN, double.NaN, expectedMaterial);
        }

        private void GenerateTrapezoidCrossSectionAndCheckShapeProperties<TGenerator, TShape>(ILogHandler logHandler, GwswElement gwswElement, string expectedName, double expectedBottomWidth, double expectedSlope, double expectedMaxFlowWidth, string expectedMaterial)
            where TGenerator : ASewerCrossSectionShapeGenerator
            where TShape : CrossSectionStandardShapeBase, new()
        {
            GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(logHandler, gwswElement, expectedName, expectedBottomWidth, double.NaN, double.NaN, expectedSlope, expectedMaxFlowWidth, expectedMaterial);
        }

        private static void GenerateCrossSectionDefinitionAndCheckShapeProperties<TGenerator, TShape>(ILogHandler logHandler, GwswElement gwswElement, string expectedName, double expectedWidth, double expectedHeight, double expectedArcHeight, double expectedSlope, double expectedMaxFlowWidth, string expectedMaterial)
            where TGenerator : ASewerCrossSectionShapeGenerator
            where TShape : CrossSectionStandardShapeBase, new()
        {

            var createdShape = ((TGenerator)Activator.CreateInstance(typeof(TGenerator), logHandler)).Generate(gwswElement) as TShape;
            Assert.IsNotNull(createdShape);

            Assert.That(createdShape.Name, Is.EqualTo(expectedName));
            Assert.That(createdShape.MaterialName, Is.EqualTo(expectedMaterial));

            var csHeightWidthShape = createdShape as CrossSectionStandardShapeWidthHeightBase;
            if (csHeightWidthShape != null)
            {
                Assert.That(csHeightWidthShape.Width, Is.EqualTo(expectedWidth));
                Assert.That(csHeightWidthShape.Height, Is.EqualTo(expectedHeight));
            }

            if (typeof(TShape) == typeof(CrossSectionStandardShapeCircle))
            {
                var csRoundShape = createdShape as CrossSectionStandardShapeCircle;
                Assert.NotNull(csRoundShape);
                Assert.That(csRoundShape.Diameter, Is.EqualTo(expectedWidth));
            }
            else if (typeof(TShape) == typeof(CrossSectionStandardShapeArch))
            {
                var csArchShape = createdShape as CrossSectionStandardShapeArch;
                Assert.NotNull(csArchShape);
                Assert.That(csArchShape.ArcHeight, Is.EqualTo(expectedArcHeight));
            }
            else if (typeof(TShape) == typeof(CrossSectionStandardShapeTrapezium))
            {
                var csTrapezoidShape = createdShape as CrossSectionStandardShapeTrapezium;
                Assert.NotNull(csTrapezoidShape);
                Assert.That(csTrapezoidShape.BottomWidthB, Is.EqualTo(expectedWidth));
                Assert.That(csTrapezoidShape.Slope, Is.EqualTo(expectedSlope));
                Assert.That(csTrapezoidShape.MaximumFlowWidth, Is.EqualTo(expectedMaxFlowWidth));
            }
        }

        #endregion
    }
}
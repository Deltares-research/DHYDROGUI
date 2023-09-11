using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Reflection;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    public class SewerFeatureFactoryTestHelper
    {
        protected const string TypeDouble = "double";
        
        protected void CheckCompartmentPropertyValues(Compartment compartment, string uniqueId, double manholeLength, double manholeWidth, CompartmentShape shape, double floodableArea, double bottomLevel, double surfaceLevel)
        {
            Assert.NotNull(compartment.ParentManholeName);
            
            Assert.That(compartment.Name, Is.EqualTo(uniqueId));
            Assert.That(compartment.ManholeLength, Is.EqualTo(manholeLength / 1000.0)); //mm -> m
            Assert.That(compartment.ManholeWidth, Is.EqualTo(manholeWidth / 1000.0)); //mm -> m
            Assert.That(compartment.Shape, Is.EqualTo(shape));
            Assert.That(compartment.FloodableArea, Is.EqualTo(floodableArea));
            Assert.That(compartment.BottomLevel, Is.EqualTo(bottomLevel));
            Assert.That(compartment.SurfaceLevel, Is.EqualTo(surfaceLevel));
        }

        protected static GwswAttribute GetDefaultGwswAttribute(string attributeName, string attributeValue, string defaultValue, string attributeType = null)
        {
            if (attributeValue == null)
                attributeValue = string.Empty;
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            return new GwswAttribute
            {
                GwswAttributeType = GetGwswAttributeType("testFile", 5, "columnName", attributeType ?? "string", attributeName,
                    "unkownDefinition", "mandatoryMaybe", defaultValue, "noRemarks", logHandler),
                ValueAsString = attributeValue
            };
        }

        protected static GwswElement GetNodeGwswElement(string uniqueId, string manholeId, string nodeType, double xCoordinate, double yCoordinate, double nodeLength, double nodeWidth, string nodeShape, double floodableArea, double bottomLevel, double surfaceLevel)
        {
            GwswElement nodeGwswElement = new GwswElement();
            try
            {
                nodeGwswElement = new GwswElement
                {
                    ElementTypeName = SewerFeatureType.Node.ToString(),
                    GwswAttributeList =
                    {
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId, string.Empty),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId, string.Empty),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeType, nodeType, string.Empty),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.XCoordinate, xCoordinate.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.YCoordinate, yCoordinate.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeLength, nodeLength.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeWidth, nodeWidth.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeShape, nodeShape, string.Empty),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.FloodableArea, floodableArea.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.SurfaceLevel, surfaceLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    }
                };
            }
            catch (Exception e)
            {
                Assert.Fail("Gwsw element was not created, thus the test can't go ahead. {0}", e.Message);
            }

            Assert.IsNotNull(nodeGwswElement);

            return nodeGwswElement;
        }

        protected static GwswElement GetSewerConnectionGwswElement(string uniqueId, string startNode, string endNode, string sewerConnectionTypeString , double startLevel, double endLevel, string flowDirectionString, double length,
            string crossSectionDef, string pipeIndicator, string sewerConnectionWaterType, double inletLossStart, double inletLossEnd, double outletLossStart, double outletLossEnd)
        {
            GwswElement nodeGwswElement = new GwswElement();
            try
            {
                nodeGwswElement = new GwswElement
                {
                    ElementTypeName = SewerFeatureType.Connection.ToString(),
                    GwswAttributeList = new List<GwswAttribute>
                    {
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.UniqueId, uniqueId, string.Empty),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, startNode, string.Empty),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, endNode, string.Empty),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeType, sewerConnectionTypeString, string.Empty),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.LevelStart, startLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.LevelEnd, endLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.Length, length.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.FlowDirection, flowDirectionString, string.Empty),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.CrossSectionDefinitionId, crossSectionDef, string.Empty),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.PipeId, pipeIndicator, string.Empty),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.WaterType, sewerConnectionWaterType, string.Empty),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.InletLossStart, inletLossStart.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.InletLossEnd, inletLossEnd.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.OutletLossStart, outletLossStart.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerConnectionMapping.PropertyKeys.OutletLossEnd, outletLossEnd.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    }
                };
            }
            catch (Exception e)
            {
                Assert.Fail("Gwsw element was not created, thus the test can't go ahead. {0}", e.Message);
            }

            Assert.IsNotNull(nodeGwswElement);
            
            return nodeGwswElement;
        }

        protected static GwswElement GetStructureGwswElement(string uniqueId, string structureType, double pumpCapacity, double startLevelDownstreams, double stopLevelDownstreams, double startLevelUpstreams, double stopLevelUpstreams, double surfaceWaterLevel)
        {
            GwswElement nodeGwswElement = new GwswElement();
            try
            {
                nodeGwswElement = new GwswElement
                {
                    ElementTypeName = SewerFeatureType.Structure.ToString(),
                    GwswAttributeList = new List<GwswAttribute>
                    {
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.UniqueId, uniqueId, string.Empty),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StructureType, structureType, string.Empty),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.PumpCapacity, pumpCapacity.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StartLevelDownstreams, startLevelDownstreams.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StopLevelDownstreams, stopLevelDownstreams.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StartLevelUpstreams, startLevelUpstreams.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.StopLevelUpstreams, stopLevelUpstreams.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerStructureMapping.PropertyKeys.SurfaceWaterLevel, surfaceWaterLevel.ToString(CultureInfo.InvariantCulture), string.Empty, TypeDouble),
                    }
                };
            }
            catch (Exception e)
            {
                Assert.Fail("Gwsw element was not created, thus the test can't go ahead. {0}", e.Message);
            }

            Assert.IsNotNull(nodeGwswElement);

            return nodeGwswElement;
        }

        protected static GwswElement GetSewerProfileGwswElement(string profileId, string profileShape, string profileWidth, string profileHeight, string slope1, string slope2)
        {
            GwswElement nodeGwswElement = new GwswElement();
            try
            {
                nodeGwswElement = new GwswElement
                {
                    ElementTypeName = SewerFeatureType.Crosssection.ToString(),
                    GwswAttributeList = new List<GwswAttribute>
                    {
                        GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileId, profileId, string.Empty),
                        GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileShape, profileShape, string.Empty),
                        GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileWidth, profileWidth, string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.SewerProfileHeight, profileHeight, string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope1, slope1, string.Empty, TypeDouble),
                        GetDefaultGwswAttribute(SewerProfileMapping.PropertyKeys.Slope2, slope2, string.Empty, TypeDouble)
                    }
                };
            }
            catch (Exception e)
            {
                Assert.Fail("Gwsw element was not created, thus the test can't go ahead. {0}", e.Message);
            }

            Assert.IsNotNull(nodeGwswElement);

            return nodeGwswElement;
        }

        public static GwswAttributeType GetGwswAttributeType(string fileName, int lineNumber, string columnName,
            string typeField, string codeName, string definition, string mandatory, string defaultValue, string remarks, ILogHandler logHandler)
        {
            var gwswAttributeType = new GwswAttributeType(logHandler)
            {
                Name = columnName,
                Key = codeName,
                Definition = definition,
                Mandatory = mandatory,
                Remarks = remarks,
                FileName = fileName,
                DefaultValue = defaultValue
            };
            gwswAttributeType.AttributeType = gwswAttributeType.TryGetParsedValueType(columnName, typeField, definition, fileName, lineNumber);

            return gwswAttributeType;
        }

        protected static T CreateSewerFeature<T>(GwswElement gwswElement, ILogHandler logHandler = null) where T : class, ISewerFeature
        {
            SewerFeatureType elementType;
            if (!Enum.TryParse(gwswElement?.ElementTypeName, out elementType)) return null;

            var lu = new Dictionary<SewerFeatureType, GwswElement> {{elementType, gwswElement}}.ToLookup(l => l.Key, l => l.Value);
            var activityRunner = new ActivityRunner();
            var gwswImporter = new GwswFileImporter(new DefinitionsProvider(logHandler)) { ActivityRunner = activityRunner };
            TypeUtils.SetField(gwswImporter, "logHandler", logHandler);
            activityRunner.Activities.Add(new FileImportActivity(gwswImporter));

            var sewerEntities = SewerFeatureFactory.CreateSewerEntities(lu, gwswImporter);
            var sewerEntity = sewerEntities.FirstOrDefault() as T;
            return sewerEntity;
        }

        protected static void AddSewerFeatureToNetwork(ISewerFeature sewerFeature, IHydroNetwork network)
        {
            sewerFeature.AddToHydroNetwork(network, null);
        }
    }
}
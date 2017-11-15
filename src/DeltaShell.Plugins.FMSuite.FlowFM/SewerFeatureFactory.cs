using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerFeatureFactory
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerFeatureFactory));
        // For now, the types are: Node, Pipe, Structure, Surface, Runoff, Discharge, Distribution, Meta
        private static Dictionary<SewerFeatureType, Func<object, HydroNetwork, INetworkFeature>> CreateSewerFeature = new Dictionary<SewerFeatureType, Func<object, HydroNetwork, INetworkFeature>>
        {
            { SewerFeatureType.Node, CreateManhole },
            { SewerFeatureType.Pipe, CreatePipe }
        };

        public static IEnumerable<INetworkFeature> CreateMultipleInstances(List<GwswElement> listOfElements, HydroNetwork network = null)
        {
            var networkFeatures = new List<INetworkFeature>();
            foreach (var element in listOfElements)
            {
                networkFeatures.Add(CreateInstance(element, network));
            }
            return networkFeatures;
        }

        public static INetworkFeature CreateInstance(object element, HydroNetwork network = null)
        {
            SewerFeatureType elementType;
            var gwswElement = element as GwswElement;
            if (gwswElement != null && Enum.TryParse(gwswElement.ElementTypeName, out elementType))
            {
                if( CreateSewerFeature.ContainsKey(elementType))
                    return CreateSewerFeature[elementType](gwswElement, network);
            }

            SewerFeatureType elementListType;
            var gwswElementList = element as List<GwswElement>;
            if (gwswElementList != null && Enum.TryParse(gwswElementList.FirstOrDefault()?.ElementTypeName,
                    out elementListType))
            {
                if (CreateSewerFeature.ContainsKey(elementListType))
                    return CreateSewerFeature[elementListType](gwswElementList, network);
            }

            return null;
        }

        private static CompositeManholeNode CreateManhole(object element, HydroNetwork network = null)
        {
            var gwswElement = element as GwswElement;
            if(gwswElement != null)
                return CreateManholeNode(new List<GwswElement> { gwswElement });

            var gwswElementList = element as List<GwswElement>;
            if (gwswElementList != null)
                return CreateManholeNode(gwswElementList);

            return null;
        }

        private static CompositeManholeNode CreateManholeNode(IEnumerable<GwswElement> gwswElementList)
        {
            // Create dictionary with all attributes
            var elementValuesDictionary = gwswElementList.Select(gwswElement => gwswElement.GwswAttributeList)
                .Select(attributes => attributes.ToDictionary(attr => attr.GwswAttributeType.Key, attr => attr.ValueAsString))
                .ToList();

            CompositeManholeNode manholeNode;
            try
            {
                var manholeIds = elementValuesDictionary.Select(v => v[ManholePropertyKeys.ManholeId]).Distinct().ToList();
                if (manholeIds.Count != 1) return null;
                manholeNode = new CompositeManholeNode(manholeIds.FirstOrDefault());
            }
            catch
            {
                Log.Warn(Resources.SewerFeatureFactory_CreateManholeNode_There_are_lines_in__Knooppunt_csv__that_do_not_contain_a_Manhole_Id__These_lines_are_not_imported_);
                return null;
            }
            
            foreach (var elementValues in elementValuesDictionary)
            {
                try
                {
                    var manholeCompartment = CreateManHoleCompartment(elementValues);
                    manholeNode.Compartments.Add(manholeCompartment);
                }
                catch (Exception e)
                {
                    Log.Warn(e.Message);
                    return null;
                }
            }

            return manholeNode;
        }

        private static Manhole CreateManHoleCompartment(IReadOnlyDictionary<string, string> elementValues)
        {
            string manholeId;
            if (!elementValues.TryGetValue(ManholePropertyKeys.ManholeId, out manholeId))
                throw new Exception(Resources.SewerFeatureFactory_CreateManholeNode_There_are_lines_in__Knooppunt_csv__that_do_not_contain_a_Manhole_Id__These_lines_are_not_imported_);

            string uniqueId;
            if (!elementValues.TryGetValue(ManholePropertyKeys.UniqueId, out uniqueId))
                throw new Exception(string.Format(Resources.SewerFeatureFactory_CreateManHoleCompartment_Manhole_with_manhole_id___0___could_not_be_created__because_one_of_its_compartments_misses_its_unique_id_, manholeId));

            var manhole = new Manhole(uniqueId);
            // Set manhole value
            int intValue;
            if (TryGetIntValueElseThrowException(ManholePropertyKeys.NodeLength, elementValues, uniqueId, manholeId, out intValue)) manhole.ManholeLength = intValue;
            if (TryGetIntValueElseThrowException(ManholePropertyKeys.NodeWidth, elementValues, uniqueId, manholeId, out intValue)) manhole.ManholeWidth = intValue;

            double doubleValue;
            if (TryGetDoubleValueElseThrowException(ManholePropertyKeys.FloodableArea, elementValues, uniqueId, manholeId, out doubleValue)) manhole.FloodableArea = doubleValue;
            if (TryGetDoubleValueElseThrowException(ManholePropertyKeys.BottomLevel, elementValues, uniqueId, manholeId, out doubleValue)) manhole.BottomLevel = doubleValue;
            if (TryGetDoubleValueElseThrowException(ManholePropertyKeys.SurfaceLevel, elementValues, uniqueId, manholeId, out doubleValue)) manhole.SurfaceLevel = doubleValue;

            double yCoordinate;
            double xCoordinate;
            if (TryGetDoubleValueElseThrowException(ManholePropertyKeys.XCoordinate, elementValues, uniqueId, manholeId, out xCoordinate) 
                && TryGetDoubleValueElseThrowException(ManholePropertyKeys.YCoordinate, elementValues, uniqueId, manholeId, out yCoordinate))
                manhole.Coordinates = new Coordinate(xCoordinate, yCoordinate);

            // Set shape value of the manhole
            string nodeShape;
            if (!elementValues.TryGetValue(ManholePropertyKeys.NodeShape, out nodeShape)) return manhole;
            try
            {
                manhole.Shape = (ManholeShape)EnumDescriptionAttributeTypeConverter.GetEnumValue<ManholeShape>(nodeShape);
            }
            catch
            {
                ThrowException(uniqueId, "string", ManholePropertyKeys.NodeShape, nodeShape, manholeId);
            }

            return manhole;
        }

        private static bool TryGetIntValueElseThrowException(string columnKey, IReadOnlyDictionary<string, string> elementValues, string uniqueId, string manholeId, out int intValue)
        {
            string stringValue;
            if (!elementValues.TryGetValue(columnKey, out stringValue))
            {
                intValue = 0;
                return false;
            }

            if (!int.TryParse(stringValue, out intValue)) ThrowException(uniqueId, "integer", columnKey, stringValue, manholeId);
            return true;
        }

        private static bool TryGetDoubleValueElseThrowException(string columnKey, IReadOnlyDictionary<string, string> elementValues, string id, string manholeId, out double doubleValue)
        {
            string stringValue;
            if (!elementValues.TryGetValue(columnKey, out stringValue))
            {
                doubleValue = 0.0;
                return false;
            }
            if (!double.TryParse(ReplaceCommaWithPoint(stringValue), NumberStyles.Any, CultureInfo.InvariantCulture, out doubleValue))
                ThrowException(id, "double", columnKey, stringValue, manholeId);
            return true;
        }

        private static void ThrowException(string id, string dataType, string columnKey, string wrongValue, string manholeId)
        {
            throw new Exception(string.Format(Resources.SewerFeatureFactory_ThrowException_, id, dataType, columnKey, wrongValue, manholeId));
        }

        private static string ReplaceCommaWithPoint(string doubleString)
        {
            return doubleString.Replace(',', '.');
        }

        private static GwswAttribute GetAttributeFromList(GwswElement element, string attributeName)
        {
            var attribute = element.GwswAttributeList.FirstOrDefault(attr => attr.GwswAttributeType.Key.Equals(attributeName));
            if (attribute == null)
            {
                Log.WarnFormat(Resources.SewerFeatureFactory_GetAttributeFromList_Attribute__0__was_not_found_for_element__1_, attributeName, element.ElementTypeName);
                return null;
            }
            return attribute;
        }

        private static Pipe CreatePipe(object element, HydroNetwork network = null)
        {
            var gwswElement = element as GwswElement;
            if (gwswElement == null) return null;

            var newPipe = new Pipe();

            var nodeIdStart = GetAttributeFromList(gwswElement, "NODE_UNIQUE_ID_START");
            if (nodeIdStart != null)
            {
                //Find node;
                if ( nodeIdStart.ValueAsString != string.Empty && network != null)
                {
                    var foundNode = network.Nodes.FirstOrDefault(n => n.Name.Equals(nodeIdStart.ValueAsString));
                    if (foundNode == null)
                    {
                        //create node
                        foundNode = new Node(nodeIdStart.ValueAsString);
                        network.Nodes.Add(foundNode);
                    }
                    newPipe.Source = foundNode;
                }
            }
            var nodeIdEnd = GetAttributeFromList(gwswElement, "NODE_UNIQUE_ID_END");
            if (nodeIdEnd != null)
            {
                //Find node;                
                if (nodeIdEnd.ValueAsString != string.Empty && network != null)
                {
                    var foundNode = network.Nodes.FirstOrDefault(n => n.Name.Equals(nodeIdEnd.ValueAsString));
                    if (foundNode == null)
                    {
                        //create node
                        foundNode = new Node(nodeIdEnd.ValueAsString);
                        network.Nodes.Add(foundNode);
                    }
                    newPipe.Target = foundNode;
                }
            }
            var pipeTypeAttr = GetAttributeFromList(gwswElement, "PIPE_TYPE");
            if (pipeTypeAttr != null
                && pipeTypeAttr.ValueAsString != string.Empty)
            {
                //Find type;
                PipeType pipeType;
                if (Enum.TryParse(pipeTypeAttr.ValueAsString, out pipeType))
                {
                    newPipe.PipeType = pipeType;
                }
            }
            var levelStart = GetAttributeFromList(gwswElement, "LEVEL_START");
            if (levelStart != null)
            {
                var valueType = levelStart.GwswAttributeType.AttributeType;
                if (valueType == newPipe.LevelSource.GetType()
                    && levelStart.ValueAsString != string.Empty)
                {
                    newPipe.LevelSource = double.Parse(levelStart.ValueAsString);
                }
            }
            var levelEnd = GetAttributeFromList(gwswElement, "LEVEL_END");
            if (levelEnd != null)
            {
                var valueType = levelEnd.GwswAttributeType.AttributeType;
                if (valueType == newPipe.LevelTarget.GetType()
                    && levelEnd.ValueAsString != string.Empty)
                {
                    newPipe.LevelTarget = double.Parse(levelEnd.ValueAsString);
                }
            }
            var length = GetAttributeFromList(gwswElement, "LENGTH");
            if (length != null)
            {
                var valueType = length.GwswAttributeType.AttributeType;
                if (valueType == newPipe.Length.GetType()
                    && length.ValueAsString != string.Empty)
                {
                    newPipe.Length = double.Parse(length.ValueAsString);
                }
            }
            var profileDef = GetAttributeFromList(gwswElement, "CROSS_SECTION_DEF");
            if (profileDef != null)
            {
                //Find crossSectionDef;
                //Profiles are needed first.
                if (profileDef.ValueAsString != string.Empty && network != null)
                {
                    var foundCs = network.SewerProfiles.FirstOrDefault(n => n.Name.Equals(profileDef.ValueAsString));
                    if (foundCs == null)
                    {
                        foundCs = CrossSection.CreateDefault();
                        foundCs.Name = profileDef.ValueAsString;
                        network.SewerProfiles.Add(foundCs);
                    }
                    newPipe.CrossSectionShape = (CrossSection)foundCs;
                }
            }

            return newPipe;
        }
    }

    public static class ManholePropertyKeys
    {
        public const string ManholeId = "MANHOLE_ID";
        public const string UniqueId = "UNIQUE_ID";
        public const string NodeLength = "NODE_LENGTH";
        public const string NodeWidth = "NODE_WIDTH";
        public const string NodeShape = "NODE_SHAPE";
        public const string FloodableArea = "FLOODABLE_AREA";
        public const string BottomLevel = "BOTTOM_LEVEL";
        public const string SurfaceLevel = "SURFACE_LEVEL";
        public const string XCoordinate = "X_COORDINATE";
        public const string YCoordinate = "Y_COORDINATE";
    }
}

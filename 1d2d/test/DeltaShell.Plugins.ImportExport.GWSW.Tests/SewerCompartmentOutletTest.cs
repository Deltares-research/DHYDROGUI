using System.Collections.Generic;
using System.Globalization;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Reflection;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests
{
    [TestFixture]
    public class SewerCompartmentOutletTest : SewerFeatureFactoryTestHelper
    {
        #region Outlets

        [TestCase(false)]
        [TestCase(true)]
        public void GivenNodeGwswElementWithOutletNodeType_WhenCreatingCompartment_ThenCorrectOutletCompartmentIsCreated(bool useNetwork)
        {
            #region Create node Gwsw element
            var uniqueId = "outlet123";
            var manholeId = "man123";
            var typeDouble = "double";
            var xCoord = 30.0;
            var yCoord = 15.0;
            var nodeLength = 14.0;
            var nodeWidth = 13.0;
            var nodeShape = CompartmentShape.Round;
            var nodeShapeAsString = "rnd";
            var floodableArea = 11.0;
            var bottomLevel = 10.0;
            var surfaceLevel = 5.0;
            var nodeType = SewerStructureMapping.StructureType.Outlet.GetDescription(); // This makes it an outlet element!
            var nodeGwswElement = new GwswElement
            {
                ElementTypeName = SewerFeatureType.Node.ToString(),
                GwswAttributeList = new List<GwswAttribute>
                {
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.UniqueId, uniqueId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.ManholeId, manholeId, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.XCoordinate, xCoord.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.YCoordinate, yCoord.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeLength, nodeLength.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeWidth, nodeWidth.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeShape, nodeShapeAsString, string.Empty),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.FloodableArea, floodableArea.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.BottomLevel, bottomLevel.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.SurfaceLevel, surfaceLevel.ToString(CultureInfo.InvariantCulture), string.Empty, typeDouble),
                    GetDefaultGwswAttribute(ManholeMapping.PropertyKeys.NodeType, nodeType, string.Empty),
                }
            };

            #endregion

            var outletCompartment = CreateSewerFeature<OutletCompartment>(nodeGwswElement);
            Assert.IsNotNull(outletCompartment);
            
            CheckCompartmentPropertyValues(outletCompartment, uniqueId, nodeLength, nodeWidth, nodeShape, floodableArea, bottomLevel, surfaceLevel);
        }
        
        [Test]
        public void CreateOutletCompartmentFromGwswElementStructureType()
        {
            var uniqueId = "outlet123";
            var surfaceWaterLevel = 15.0;
            var structureType =
                SewerStructureMapping.StructureType.Outlet.GetDescription();

            var defaultDouble = 0.0;
            var structureGwswElement = GetStructureGwswElement(uniqueId, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);

            var outletCompartment = CreateSewerFeature<OutletCompartment>(structureGwswElement);
            Assert.IsNotNull(outletCompartment);
            Assert.That(outletCompartment.SurfaceWaterLevel, Is.EqualTo(surfaceWaterLevel));
        }
        
        [Test]
        public void CreateOutletCompartmentFromGwswElementStructureTypeWithoutNetwork()
        {
            var uniqueId = "outlet123";
            var surfaceWaterLevel = 15.0;
            var structureType =
                SewerStructureMapping.StructureType.Outlet.GetDescription();

            var defaultDouble = 0.0;
            var structureGwswElement = GetStructureGwswElement(uniqueId, structureType, defaultDouble, defaultDouble,
                defaultDouble, defaultDouble, defaultDouble, surfaceWaterLevel);

            var outletCompartment = CreateSewerFeature<OutletCompartment>(structureGwswElement);
            Assert.IsNotNull(outletCompartment);
            Assert.That(outletCompartment.SurfaceWaterLevel, Is.EqualTo(surfaceWaterLevel));
        }
        
        #endregion        
    }
}
using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.ImportExport.Sobek.Builders.HydroTypeInitializers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;
using NSubstitute;
using NUnit.Framework;
using BridgeType = DelftTools.Hydro.Structures.BridgeType;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Reflection;
using DelftTools.Hydro.CrossSections;
using GeoAPI.Geometries;
using System.Linq;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.Builders.HydroTypeInitializers
{
    [TestFixture]
    public class DefaultBridgeInitializerTests
    {
        private ILog mockLogger;
        private Dictionary<string, SobekCrossSectionDefinition> crossSectionDefinitions;
        private DefaultBridgeInitializer initializer;

        [SetUp]
        public void SetUp()
        {
            crossSectionDefinitions = new Dictionary<string, SobekCrossSectionDefinition>();
            initializer = new DefaultBridgeInitializer(crossSectionDefinitions);
            mockLogger = Substitute.For<ILog>();
            TypeUtils.SetStaticField(typeof(DefaultBridgeInitializer),"log", mockLogger);
        }

        [Test]
        public void Initialize_WithValidCrossSectionId_SetsGroundLayerAndBridgeType()
        {
            // Arrange
            var crossSectionId = "valid_id"; 
            var sobekBridge = new SobekBridge{ CrossSectionId = crossSectionId };
            var bridge = new Bridge();
            crossSectionDefinitions.Add(crossSectionId, new SobekCrossSectionDefinition { UseGroundLayer = true });

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Assert.IsTrue(bridge.GroundLayerEnabled);
            Assert.AreEqual(BridgeType.Tabulated, bridge.BridgeType);
        }
        
        [Test]
        public void Initialize_WithValidCrossSectionId_SetsGroundLayerThicknessAndBridgeType()
        {
            // Arrange
            var crossSectionId = "valid_id"; 
            var sobekBridge = new SobekBridge{ CrossSectionId = crossSectionId };
            var bridge = new Bridge();
            crossSectionDefinitions.Add(crossSectionId, new SobekCrossSectionDefinition { GroundLayerDepth = 80.1 });

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            
            Assert.AreEqual(80.1, bridge.GroundLayerThickness);
            Assert.AreEqual(BridgeType.Tabulated, bridge.BridgeType);
        }

        [Test]
        public void Initialize_WithInvalidCrossSectionId_SetsBridgeTypeToRectangle()
        {
            // Arrange
            var crossSectionId = "invalid_id"; 
            var sobekBridge = new SobekBridge { CrossSectionId = crossSectionId };
            var bridge = new Bridge();
            
            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Assert.AreEqual(BridgeType.Rectangle, bridge.BridgeType);
        }

        [Test]
        public void Initialize_WithTabulatedCrossSectionDefinition_SetsTabulatedProperties()
        {
            // Arrange
            var crossSectionId = "valid_id";
            var sobekBridge = new SobekBridge { CrossSectionId = crossSectionId };
            var bridge = new Bridge
            {
                Width = 80.1,
                Height = 2.7
            };
            
            SobekCrossSectionDefinition sobekCrossSectionDefinition = GenerateSobekCrossSectionDefinition();

            crossSectionDefinitions.Add(crossSectionId, sobekCrossSectionDefinition);

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Assert.AreEqual(80.1, bridge.Width);
            Assert.AreEqual(2.7, bridge.Height);
            Assert.AreEqual(30, bridge.TabulatedCrossSectionDefinition.Width);
            Assert.AreEqual(20, bridge.TabulatedCrossSectionDefinition.HighestPoint);
            Assert.AreEqual(BridgeType.Tabulated, bridge.BridgeType);
        }

        [Test]
        public void Initialize_WithValidTabulatedCrossSectionAndShift_SetsTabulatedPropertiesWithShiftApplied()
        {
            // Arrange
            var crossSectionId = "valid_id";
            var sobekBridge = new SobekBridge { CrossSectionId = crossSectionId };
            var bridge = new Bridge
            {
                Shift = 5,
                Width = 80.1,
                Height = 2.7
            }; 
            SobekCrossSectionDefinition sobekCrossSectionDefinition = GenerateSobekCrossSectionDefinition();
            initializer.SobekCrossSectionDefinitions.Add(crossSectionId, sobekCrossSectionDefinition);

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Assert.AreEqual(80.1, bridge.Width);
            Assert.AreEqual(2.7, bridge.Height);
            Assert.AreEqual(30, bridge.TabulatedCrossSectionDefinition.Width);
            Assert.AreEqual(25, bridge.TabulatedCrossSectionDefinition.HighestPoint);
            Assert.AreEqual(BridgeType.Tabulated, bridge.BridgeType);
        }

        [Test]
        public void Initialize_WithValidNonTabulatedCrossSection_SetsBridgeTypeToRectangleAndLogsWarning()
        {
            // Arrange
            var crossSectionId = "valid_id";
            var sobekBridge = new SobekBridge { CrossSectionId = crossSectionId };
            var bridge = new Bridge();
            
            initializer.SobekCrossSectionDefinitions.Add(crossSectionId, new SobekCrossSectionDefinition
            {
                Type = SobekCrossSectionDefinitionType.AsymmetricalTrapezoidal
            });

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Assert.AreEqual(BridgeType.Rectangle, bridge.BridgeType);
            mockLogger.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object>());
            mockLogger.Received().WarnFormat(Properties.Resources.DefaultBridgeInitializer_InitializeBridgeCrossSectionDefinition_Only_sobek2_bridge_geometric_profiles_of_type_tabular__0__supported_and_implemented_, SobekCrossSectionDefinitionType.Tabulated);

        }

        [Test]
        public void Initialize_WithNullCrossSectionId_SetsBridgeTypeToRectangle()
        {
            // Arrange
            var sobekBridge = new SobekBridge();
            var bridge = new Bridge();

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Assert.AreEqual(BridgeType.Rectangle, bridge.BridgeType);
        }

        [Test]
        public void Initialize_WithNullCrossSectionDefinition_DoesNotThrowException()
        {
            // Arrange
            var crossSectionId = "valid_id";
            var sobekBridge = new SobekBridge { CrossSectionId = crossSectionId };
            var bridge = new Bridge();
            
            initializer.SobekCrossSectionDefinitions.Add(crossSectionId, null);

            // Act & Assert
            Assert.DoesNotThrow(() => initializer.Initialize(sobekBridge, bridge));
        }

        [Test]
        public void Initialize_WithEmptyCrossSectionDefinitions_DoesNotThrowException()
        {
            // Arrange
            var sobekBridge = new SobekBridge();
            var bridge = new Bridge();

            // Act & Assert
            Assert.DoesNotThrow(() => initializer.Initialize(sobekBridge, bridge));
        }

        [Test]
        public void Initialize_WithNonexistentCrossSectionId_SetsBridgeTypeToRectangle()
        {
            // Arrange
            var crossSectionId = "nonexistent_id"; 
            var sobekBridge = new SobekBridge { CrossSectionId = crossSectionId };
            var bridge = new Bridge();
            

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Assert.AreEqual(BridgeType.Rectangle, bridge.BridgeType);
        }

        [Test]
        public void Initialize_WithNonTabulatedCrossSectionDefinition_LogsWarning()
        {
            // Arrange
            var crossSectionId = "valid_id";
            var sobekBridge = new SobekBridge { CrossSectionId = crossSectionId };
            var bridge = new Bridge();
            
            var nonTabulatedProfile = new SobekCrossSectionDefinition
            {
                Type = SobekCrossSectionDefinitionType.AsymmetricalTrapezoidal
            };
            initializer.SobekCrossSectionDefinitions.Add(crossSectionId, nonTabulatedProfile);

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            mockLogger.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object>());
        }

        [Test]
        public void Initialize_WithRectangleCrossSectionDefinition_SetsPropertiesForRectangleBridge()
        {
            // Arrange
            var crossSectionId = "valid_id";
            var sobekBridge = new SobekBridge{ CrossSectionId = crossSectionId };
            var bridge = new Bridge();
            
            var rectangleProfile = GenerateSobekCrossSectionDefinition();
            rectangleProfile.Name = "r_someName";
            initializer.SobekCrossSectionDefinitions.Add(crossSectionId, rectangleProfile);

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Assert.AreEqual(30, bridge.Width);
            Assert.AreEqual(10, bridge.Height);
            Assert.AreEqual(BridgeType.Rectangle, bridge.BridgeType);
        }

        [Test]
        public void Initialize_WithValidTabulatedCrossSection_SetsYzTabulatedProperties()
        {
            // Arrange
            var crossSectionId = "valid_id";
            var sobekBridge = new SobekBridge { CrossSectionId = crossSectionId };
            var bridge = new Bridge();
          
            SobekCrossSectionDefinition sobekCrossSectionDefinition = GenerateSobekCrossSectionDefinition();
            initializer.SobekCrossSectionDefinitions.Add(crossSectionId, sobekCrossSectionDefinition);

            // Act
            
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Coordinate[] coordinates = bridge.YZCrossSectionDefinition.GetProfile().ToArray();
            Assert.AreEqual(4, coordinates.Length);
            Assert.AreEqual(new Coordinate(-15, 20), coordinates[0]);
            Assert.AreEqual(new Coordinate(-10, 10), coordinates[1]);
            Assert.AreEqual(new Coordinate(10, 10), coordinates[2]);
            Assert.AreEqual(new Coordinate(15, 20), coordinates[3]);
        }

        [Test]
        public void Initialize_WithNonTabulatedCrossSectionDefinition_DoesNotSetYZProfile()
        {
            // Arrange
            var crossSectionId = "valid_id";
            var sobekBridge = new SobekBridge { CrossSectionId = crossSectionId };
            var bridge = new Bridge();
            var crossSectionDefinition = new SobekCrossSectionDefinition() { Type = SobekCrossSectionDefinitionType.ClosedCircle };
            initializer.SobekCrossSectionDefinitions.Add(crossSectionId, crossSectionDefinition);
            var defaultYzCrossSectionProfileFromRectangle = CrossSectionDefinitionYZ.CreateDefault();
            Coordinate[] expectedCoordinates = defaultYzCrossSectionProfileFromRectangle.GetProfile().ToArray();

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Coordinate[] coordinates = bridge.YZCrossSectionDefinition.GetProfile().ToArray();
            Assert.AreEqual(expectedCoordinates.Length, coordinates.Length);
            for (int i = 0; i < expectedCoordinates.Length; i++)
            {
                Assert.AreEqual(expectedCoordinates[i], coordinates[i]);
            }
        }[Test]
        public void Initialize_WithValidTabulatedCrossSection_SetsZwTabulatedProperties()
        {
            // Arrange
            var crossSectionId = "valid_id";
            var sobekBridge = new SobekBridge { CrossSectionId = crossSectionId };
            var bridge = new Bridge();
          
            SobekCrossSectionDefinition sobekCrossSectionDefinition = GenerateSobekCrossSectionDefinition();
            initializer.SobekCrossSectionDefinitions.Add(crossSectionId, sobekCrossSectionDefinition);

            // Act
            
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Coordinate[] coordinates = bridge.TabulatedCrossSectionDefinition.GetProfile().ToArray();
            Assert.AreEqual(4, coordinates.Length);
            Assert.AreEqual(new Coordinate(-15, 20), coordinates[0]);
            Assert.AreEqual(new Coordinate(-10, 10), coordinates[1]);
            Assert.AreEqual(new Coordinate(10, 10), coordinates[2]);
            Assert.AreEqual(new Coordinate(15, 20), coordinates[3]);
        }

        [Test]
        public void Initialize_WithNonTabulatedCrossSectionDefinition_DoesNotSetZWProfile()
        {
            // Arrange
            var crossSectionId = "valid_id";
            var sobekBridge = new SobekBridge { CrossSectionId = crossSectionId };
            var bridge = new Bridge();
            var crossSectionDefinition = new SobekCrossSectionDefinition() { Type = SobekCrossSectionDefinitionType.ClosedCircle };
            initializer.SobekCrossSectionDefinitions.Add(crossSectionId, crossSectionDefinition);
            var defaultZwCrossSectionProfile = CrossSectionDefinitionZW.CreateDefault();
            Coordinate[] expectedCoordinates = defaultZwCrossSectionProfile.GetProfile().ToArray();

            // Act
            initializer.Initialize(sobekBridge, bridge);

            // Assert
            Coordinate[] coordinates = bridge.TabulatedCrossSectionDefinition.GetProfile().ToArray();
            Assert.AreEqual(expectedCoordinates.Length, coordinates.Length);
            for (int i = 0; i < expectedCoordinates.Length; i++)
            {
                Assert.AreEqual(expectedCoordinates[i], coordinates[i]);
            }
        }
        private static SobekCrossSectionDefinition GenerateSobekCrossSectionDefinition()
        {
            var sobekCrossSectionDefinition = new SobekCrossSectionDefinition
            {
                Type = SobekCrossSectionDefinitionType.Tabulated,
            };
            sobekCrossSectionDefinition.TabulatedProfile.AddRange(new[]
            {
                new SobekTabulatedProfileRow
                {
                    Height = 10,
                    TotalWidth = 20
                },
                new SobekTabulatedProfileRow
                {
                    Height = 20,
                    TotalWidth = 30
                }
            });
            return sobekCrossSectionDefinition;
        }
    }
}
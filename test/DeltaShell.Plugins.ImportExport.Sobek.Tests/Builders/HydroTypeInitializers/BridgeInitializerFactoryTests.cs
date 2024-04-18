using System.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.ImportExport.Sobek.Builders.HydroTypeInitializers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;
using NSubstitute;
using NUnit.Framework;
using SobekBridgeType = DeltaShell.Sobek.Readers.SobekDataObjects.BridgeType;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.Builders.HydroTypeInitializers
{
    [TestFixture]
    public class BridgeInitializerFactoryTests
    {
        private ILog mockLogger;
        private IBridgeInitializer defaultInitializer;
        private BridgeInitializerFactory bridgeInitializerFactory;

        [SetUp]
        public void SetUp()
        {
            defaultInitializer = Substitute.For<IBridgeInitializer>();
            bridgeInitializerFactory = new BridgeInitializerFactory(defaultInitializer);
            mockLogger = Substitute.For<ILog>();
            TypeUtils.SetStaticField(typeof(BridgeInitializerFactory), "log", mockLogger);
        }

        [Test]
        public void GetBridgeInitializer_WithRegisteredBridgePillarInitializer_ReturnsPillarBridgeInitializer()
        {
            // Arrange
            
            // Act
            var result = bridgeInitializerFactory.GetBridgeInitializer(SobekBridgeType.PillarBridge);

            // Assert
            Assert.IsInstanceOf<PillarBridgeInitializer>(result);
        }

        [Test]
        public void GetBridgeInitializer_WithRegisteredInitializer_ReturnsInitializer()
        {
            // Arrange
            var initializer = Substitute.For<IBridgeInitializer>();
            bridgeInitializerFactory.RegisterBridgeInitializer(SobekBridgeType.FixedBed, initializer);

            // Act
            var result = bridgeInitializerFactory.GetBridgeInitializer(SobekBridgeType.FixedBed);

            // Assert
            Assert.AreEqual(initializer, result);
        }

        [Test]
        public void GetBridgeInitializer_WithUnregisteredInitializer_ReturnsDefaultInitializer()
        {
            // Arrange

            
            // Act
            var result = bridgeInitializerFactory.GetBridgeInitializer(SobekBridgeType.SoilBed);

            // Assert
            Assert.AreEqual(defaultInitializer, result);
        }

        [Test]
        public void GetBridgeInitializer_WithNullDefaultInitializer_ReturnsNull()
        {
            // Arrange
            BridgeInitializerFactory bridgeInitializerFactoryWithDefaultNullInitializer = new BridgeInitializerFactory(null);

            // Act
            var result = bridgeInitializerFactoryWithDefaultNullInitializer.GetBridgeInitializer(SobekBridgeType.Abutment);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void RegisterBridgeInitializer_WithOverridingInitializer_LogsWarning()
        {
            // Arrange
            var pillarBridgeInitializer = new PillarBridgeInitializer();
            var defaultBridgeInitializer = new DefaultBridgeInitializer(new Dictionary<string, SobekCrossSectionDefinition>());
            bridgeInitializerFactory.RegisterBridgeInitializer(SobekBridgeType.PillarBridge, pillarBridgeInitializer);

            // Act
            bridgeInitializerFactory.RegisterBridgeInitializer(SobekBridgeType.PillarBridge, defaultBridgeInitializer);

            // Assert
            mockLogger.Received().WarnFormat(Arg.Any<string>(), Arg.Any<object>());
            mockLogger.Received().WarnFormat(Properties.Resources.BridgeInitializerFactory_RegisterBridgeInitializer_Already_registered_sobek2_bridge_initializer_for_type__0___Overwriting_with_new_initializer, SobekBridgeType.PillarBridge);
        }
    }
}
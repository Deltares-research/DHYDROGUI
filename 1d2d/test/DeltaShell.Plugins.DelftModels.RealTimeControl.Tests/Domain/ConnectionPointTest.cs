using System;
using DelftTools.Utils;
using DelftTools.Utils.Data;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class ConnectionPointTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var connectionPoint = new TestConnectionPoint();

            // Assert
            Assert.That(connectionPoint, Is.InstanceOf<Unique<long>>());
            Assert.That(connectionPoint, Is.InstanceOf<INameable>());
            Assert.That(connectionPoint, Is.InstanceOf<ICopyFrom>());
            Assert.That(connectionPoint, Is.InstanceOf<IFeature>());

            Assert.That(connectionPoint.Feature, Is.Null);
            Assert.That(connectionPoint.Name, Is.EqualTo("[Not Set]"));
            Assert.That(connectionPoint.ParameterName, Is.Null);
            Assert.That(connectionPoint.IsConnected, Is.False);
            Assert.That(connectionPoint.UnitName, Is.Null);

            Assert.That(connectionPoint.LocationName, Is.Empty);
            Assert.That(connectionPoint.Geometry, Is.Null);
            Assert.That(connectionPoint.Attributes, Is.Null);
        }

        [Test]
        public void ConnectionPoint_SettingFeature_UpdatesProperties()
        {
            // Setup
            var attributeCollection = Substitute.For<IFeatureAttributeCollection>();
            var geometry = Substitute.For<IGeometry>();

            const string featureString = "Feature";
            var feature = new TestFeature(featureString)
            {
                Attributes = attributeCollection,
                Geometry = geometry
            };

            var point = new TestConnectionPoint();

            // Call
            point.Feature = feature;

            // Assert
            Assert.That(point.Attributes, Is.SameAs(attributeCollection));
            Assert.That(point.Geometry, Is.SameAs(geometry));
            Assert.That(point.LocationName, Is.EqualTo(featureString));
            Assert.That(point.Name, Is.EqualTo($"{featureString}_")); // Parameter name is null and therefore no suffix present
        }

        [Test]
        public void GivenConnectionPointWithFeature_SettingParameterName_UpdatesName()
        {
            // Setup
            const string parameterName = "ParameterName";
            const string featureString = "Feature";
            var feature = new TestFeature(featureString);

            var point = new TestConnectionPoint {Feature = feature};

            // Call
            point.ParameterName = parameterName;

            // Assert
            Assert.That(point.Name, Is.EqualTo($"{featureString}_{parameterName}"));
        }

        [Test]
        public void GivenConnectionPointWithFeature_WhenReset_ThenPropertiesReset()
        {
            // Given
            var feature = new TestFeature("Feature");
            var point = new TestConnectionPoint
            {
                Feature = feature,
                UnitName = "Unit",
                ParameterName = "Parameter"
            };

            // When
            point.Reset();

            // Then
            Assert.That(point.Feature, Is.Null);
            Assert.That(point.UnitName, Is.Empty);
            Assert.That(point.ParameterName, Is.Empty);

            Assert.That(point.Name, Is.EqualTo("[Not Set]"));
            Assert.That(point.IsConnected, Is.False);
        }

        [Test]
        public void GivenConnectionPointWithFeature_WhenSettingGeometry_ThenFeatureGeometrySet()
        {
            // Given
            var geometry = Substitute.For<IGeometry>();

            var feature = new TestFeature("Feature");
            var point = new TestConnectionPoint {Feature = feature};

            // When
            point.Geometry = geometry;

            // Then
            Assert.That(feature.Geometry, Is.SameAs(geometry));
        }

        [Test]
        public void GivenConnectionPointWithFeature_WhenSettingAttributes_ThenFeatureGeometrySet()
        {
            // Given
            var attributes = Substitute.For<IFeatureAttributeCollection>();

            var feature = new TestFeature("Feature");
            var point = new TestConnectionPoint {Feature = feature};

            // When
            point.Attributes = attributes;

            // Then
            Assert.That(feature.Attributes, Is.SameAs(attributes));
        }

        [Test]
        public void CopyFrom_Null_DoesNothing()
        {
            // Setup
            const string unitName = "Unit";
            const string parameterName = "Parameter";

            var random = new Random(21);
            double value = random.NextDouble();
            var feature = new TestFeature("Feature");
            var point = new TestConnectionPoint
            {
                Feature = feature,
                UnitName = unitName,
                ParameterName = parameterName,
                Value = value
            };

            // Call
            point.CopyFrom(null);

            // Assert
            Assert.That(point.Feature, Is.SameAs(feature));
            Assert.That(point.UnitName, Is.EqualTo(unitName));
            Assert.That(point.ParameterName, Is.EqualTo(parameterName));
            Assert.That(point.Value, Is.EqualTo(value));
        }

        [Test]
        public void CopyFrom_SourceNotConnectionPoint_DoesNothing()
        {
            // Setup
            const string unitName = "Unit";
            const string parameterName = "Parameter";

            var random = new Random(21);
            double value = random.NextDouble();
            var feature = new TestFeature("Feature");
            var point = new TestConnectionPoint
            {
                Feature = feature,
                UnitName = unitName,
                ParameterName = parameterName,
                Value = value
            };

            // Call
            point.CopyFrom(new object());

            // Assert
            Assert.That(point.Feature, Is.SameAs(feature));
            Assert.That(point.UnitName, Is.EqualTo(unitName));
            Assert.That(point.ParameterName, Is.EqualTo(parameterName));
            Assert.That(point.Value, Is.EqualTo(value));
        }

        [Test]
        public void CopyFrom_SourceIsConnectionPoint_TakesProperties()
        {
            // Setup
            const string unitName = "Unit";
            const string parameterName = "Parameter";

            var random = new Random(21);
            double value = random.NextDouble();
            var feature = new TestFeature("Feature");
            var point = new TestConnectionPoint
            {
                Feature = feature,
                UnitName = unitName,
                ParameterName = parameterName,
                Value = value
            };

            var sourcePoint = new TestConnectionPoint
            {
                Feature = new TestFeature("Other feature"),
                UnitName = "OtherUnit",
                ParameterName = "OtherParameter",
                Value = random.NextDouble()
            };

            // Precondition
            Assert.That(point.Feature, Is.Not.SameAs(sourcePoint.Feature));
            Assert.That(point.UnitName, Is.Not.EqualTo(sourcePoint.UnitName));
            Assert.That(point.ParameterName, Is.Not.EqualTo(sourcePoint.ParameterName));
            Assert.That(point.Value, Is.Not.EqualTo(sourcePoint.Value));

            // Call
            point.CopyFrom(sourcePoint);

            // Assert
            Assert.That(point.Feature, Is.SameAs(sourcePoint.Feature));
            Assert.That(point.UnitName, Is.EqualTo(sourcePoint.UnitName));
            Assert.That(point.ParameterName, Is.EqualTo(sourcePoint.ParameterName));
            Assert.That(point.Value, Is.EqualTo(sourcePoint.Value));
        }

        [Test]
        public void Clone_FromConnectionPointWithFeatures_ReturnsExpectedConnectionPoint()
        {
            // Setup
            const string unitName = "Unit";
            const string parameterName = "Parameter";

            var random = new Random(21);
            double value = random.NextDouble();
            var feature = new TestFeature("Feature");
            var point = new TestConnectionPoint
            {
                Feature = feature,
                UnitName = unitName,
                ParameterName = parameterName,
                Value = value
            };

            // Call
            object clonedPoint = point.Clone();

            // Assert
            var clonedConnectionPoint = clonedPoint as ConnectionPoint;
            Assert.That(clonedConnectionPoint, Is.Not.Null);
            Assert.That(clonedConnectionPoint.Feature, Is.SameAs(point.Feature));
            Assert.That(clonedConnectionPoint.UnitName, Is.EqualTo(point.UnitName));
            Assert.That(clonedConnectionPoint.ParameterName, Is.EqualTo(point.ParameterName));
            Assert.That(clonedConnectionPoint.Value, Is.EqualTo(point.Value));
        }

        [Test]
        public void GivenConnectionPointWithFeature_WhenFeatureUpdates_NameUpdates()
        {
            // Given
            var feature = new RtcTestFeature {Name = "f"};

            const string parameterName = "ParameterName";
            var point = new TestConnectionPoint
            {
                Feature = feature,
                ParameterName = parameterName
            };

            // Precondition
            Assert.That(point.Name, Is.EqualTo($"{feature.Name}_{parameterName}"));

            const string newFeatureName = "new Feature Name";

            // When
            feature.Name = newFeatureName;

            // Then
            Assert.That(point.Name, Is.EqualTo($"{newFeatureName}_{parameterName}"));
        }

        private class TestConnectionPoint : ConnectionPoint
        {
            public override ConnectionType ConnectionType { get; }
        }

        private class TestFeature : IFeature
        {
            private readonly string name;

            public TestFeature(string name)
            {
                this.name = name;
            }

            public long Id { get; set; }

            public IGeometry Geometry { get; set; }
            public IFeatureAttributeCollection Attributes { get; set; }

            public override string ToString()
            {
                return name;
            }

            public Type GetEntityType()
            {
                throw new NotImplementedException();
            }

            public object Clone()
            {
                throw new NotImplementedException();
            }
        }
    }
}
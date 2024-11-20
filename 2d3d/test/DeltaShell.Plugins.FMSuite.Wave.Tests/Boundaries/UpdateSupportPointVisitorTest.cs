using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries
{
    [TestFixture]
    public class UpdateSupportPointVisitorTest
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_ThrowsArgumentNullException_WhenNullParameterProvided()
        {
            void Call() => new SnapBoundariesToNewGrid.UpdateSupportPointVisitor(null);

            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void Constructor_ConstructionSuccessful_WhenEmptyCollectionProvided()
        {
            // Setup
            var toUpdate = new Dictionary<SupportPoint, SupportPoint>();

            // Call
            var visitor = new SnapBoundariesToNewGrid.UpdateSupportPointVisitor(toUpdate);

            // Assert
            Assert.IsNotNull(visitor);
            Assert.That(visitor, Is.InstanceOf<ISpatiallyDefinedDataComponentVisitor>());
        }

        [Test]
        public void Visit_UniformDataComponent_DoesNothing()
        {
            // Setup
            var toUpdate = new Dictionary<SupportPoint, SupportPoint> {{new SupportPoint(10, Substitute.For<IWaveBoundaryGeometricDefinition>()), new SupportPoint(20, Substitute.For<IWaveBoundaryGeometricDefinition>())}};

            var visitor = new SnapBoundariesToNewGrid.UpdateSupportPointVisitor(toUpdate);

            double height = random.NextDouble() * 1000;
            double period = random.NextDouble() * 1000;
            double direction = random.NextDouble() * 1000;
            double spreadingPower = random.NextDouble() * 1000;

            var spreading = new PowerDefinedSpreading {SpreadingPower = spreadingPower};

            var constantParameters = new ConstantParameters<PowerDefinedSpreading>(height, period, direction, spreading);
            var uniformDataComponent = new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(constantParameters);

            // Call
            visitor.Visit(uniformDataComponent);

            // Assert
            Assert.That(uniformDataComponent.Data, Is.SameAs(constantParameters));
            Assert.That(constantParameters.Height, Is.EqualTo(height));
            Assert.That(constantParameters.Period, Is.EqualTo(period));
            Assert.That(constantParameters.Direction, Is.EqualTo(direction));
            Assert.That(constantParameters.Spreading, Is.SameAs(spreading));
            Assert.That(spreading.SpreadingPower, Is.EqualTo(spreadingPower));
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponent_SupportPointReplaced()
        {
            // Setup
            var supportPointToReplace = new SupportPoint(10, Substitute.For<IWaveBoundaryGeometricDefinition>());
            var replacedWithSupportPoint = new SupportPoint(20, Substitute.For<IWaveBoundaryGeometricDefinition>());

            var fileBasedParameters = new FileBasedParameters("mock");

            var toVisit = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            toVisit.AddParameters(supportPointToReplace, fileBasedParameters);

            var toUpdate = new Dictionary<SupportPoint, SupportPoint> {{supportPointToReplace, replacedWithSupportPoint}};

            var visitor = new SnapBoundariesToNewGrid.UpdateSupportPointVisitor(toUpdate);

            // Call
            visitor.Visit(toVisit);

            // Assert
            Assert.IsFalse(toVisit.Data.ContainsKey(supportPointToReplace));
            Assert.IsTrue(toVisit.Data.ContainsKey(replacedWithSupportPoint));
            Assert.That(toVisit.Data[replacedWithSupportPoint], Is.SameAs(fileBasedParameters));
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponent_SupportPointRemoved()
        {
            // Setup
            var supportPointToReplace = new SupportPoint(10, Substitute.For<IWaveBoundaryGeometricDefinition>());

            var toVisit = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            toVisit.AddParameters(supportPointToReplace, new FileBasedParameters("mock"));

            var toUpdate = new Dictionary<SupportPoint, SupportPoint> {{supportPointToReplace, null}};

            var visitor = new SnapBoundariesToNewGrid.UpdateSupportPointVisitor(toUpdate);

            // Call
            visitor.Visit(toVisit);

            // Assert
            Assert.IsFalse(toVisit.Data.ContainsKey(supportPointToReplace));
            Assert.AreEqual(0, toVisit.Data.Count);
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponent_SupportPointIgnored()
        {
            // Setup
            var untouchedSupportPoint = new SupportPoint(10, Substitute.For<IWaveBoundaryGeometricDefinition>());
            var replacedWithSupportPoint = new SupportPoint(20, Substitute.For<IWaveBoundaryGeometricDefinition>());

            var fileBasedParameters = new FileBasedParameters("mock");

            var toVisit = new SpatiallyVaryingDataComponent<FileBasedParameters>();
            toVisit.AddParameters(untouchedSupportPoint, fileBasedParameters);

            var toUpdate = new Dictionary<SupportPoint, SupportPoint> {{new SupportPoint(10, Substitute.For<IWaveBoundaryGeometricDefinition>()), replacedWithSupportPoint}};

            var result = new SnapBoundariesToNewGrid.UpdateSupportPointVisitor(toUpdate);

            // Call
            result.Visit(toVisit);

            // Assert
            Assert.IsTrue(toVisit.Data.ContainsKey(untouchedSupportPoint));
            Assert.IsFalse(toVisit.Data.ContainsKey(replacedWithSupportPoint));
            Assert.AreEqual(1, toVisit.Data.Count);
            Assert.That(toVisit.Data[untouchedSupportPoint], Is.SameAs(fileBasedParameters));
        }
    }
}
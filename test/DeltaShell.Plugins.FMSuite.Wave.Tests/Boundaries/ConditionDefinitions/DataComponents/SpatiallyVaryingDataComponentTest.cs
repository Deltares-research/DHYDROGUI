using System;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.DataComponents
{
    [TestFixture(typeof(ConstantParameters))]
    public class SpatiallyVaryingDataComponentTest<T> where T : class, IBoundaryConditionParameters
    {

        [Test]
        public void Constructor_ConstantParameters_ExpectedResults()
        {
            // Call
            var dataComponent = new SpatiallyVaryingDataComponent<T>();

            // Assert
            Assert.That(dataComponent, Is.InstanceOf<IBoundaryConditionDataComponent>());
            Assert.That(dataComponent.Data, Is.Not.Null);
            Assert.That(dataComponent.Data, Is.Empty);
        }

        [Test]
        public void AddParameter_ValidData_ExpectedResults()
        {
            // Setup
            var geometricDef = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var supportPoint = new SupportPoint(20, geometricDef);

            var parameters = DataComponentTestUtils.ConstructParameters<T>();
            var dataComponent = new SpatiallyVaryingDataComponent<T>();

            // Call
            dataComponent.AddParameters(supportPoint, parameters);

            // Assert
            Assert.That(dataComponent.Data.ContainsKey(supportPoint),
                        "Expected the dictionary to contain the added key.");
            Assert.That(dataComponent.Data[supportPoint], Is.SameAs(parameters));
        }

        [Test]
        public void AddParameter_SupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var parameters = DataComponentTestUtils.ConstructParameters<T>();
            var dataComponent = new SpatiallyVaryingDataComponent<T>();

            // Call | Assert
            void Call() => dataComponent.AddParameters(null, parameters);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPoint"));
        }

        [Test]
        public void AddParameter_ParametersNull_ThrowsArgumentNullException()
        {
            // Setup
            var geometricDef = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var supportPoint = new SupportPoint(20, geometricDef);
            var dataComponent = new SpatiallyVaryingDataComponent<T>();

            // Call | Assert
            void Call() => dataComponent.AddParameters(supportPoint, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }

        [Test]
        public void AddParameter_SupportPointAlreadyExists_ThrowsInvalidOperationException()
        {
            // Setup
            var geometricDef = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var supportPoint = new SupportPoint(20, geometricDef);
            var parameters = DataComponentTestUtils.ConstructParameters<T>();
            var dataComponent = new SpatiallyVaryingDataComponent<T>();
            dataComponent.AddParameters(supportPoint, parameters);

            // Call | Assert
            void Call() => dataComponent.AddParameters(supportPoint, parameters);

            Assert.Throws<InvalidOperationException>(Call);
        }

        [Test]
        public void RemoveParameter_ValidData_ExpectedResults()
        {
            // Setup
            var geometricDef = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var supportPoint = new SupportPoint(20, geometricDef);

            var parameters = DataComponentTestUtils.ConstructParameters<T>();

            var dataComponent = new SpatiallyVaryingDataComponent<T>();
            dataComponent.AddParameters(supportPoint, parameters);

            Assert.That(dataComponent.Data.ContainsKey(supportPoint),
                        "Precondition failed.");

            // Call
            dataComponent.RemoveSupportPoint(supportPoint);

            // Assert
            Assert.That(!dataComponent.Data.ContainsKey(supportPoint),
                        "Expected the dictionary to not contain the key.");
        }

        [Test]
        public void ReplaceSupportPoint_ValidData_ExpectedResults()
        {
            // Setup
            var geometricDef = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var oldSupportPoint = new SupportPoint(20, geometricDef);

            var parameters = DataComponentTestUtils.ConstructParameters<T>();

            var dataComponent = new SpatiallyVaryingDataComponent<T>();
            dataComponent.AddParameters(oldSupportPoint, parameters);

            Assert.That(dataComponent.Data.ContainsKey(oldSupportPoint),
                        "Precondition failed.");

            var newSupportPoint = new SupportPoint(40.0, geometricDef);

            // Call
            dataComponent.ReplaceSupportPoint(oldSupportPoint, newSupportPoint);

            // Assert
            Assert.That(!dataComponent.Data.ContainsKey(oldSupportPoint), 
                        "Expected the old support point to be removed.");
            Assert.That(dataComponent.Data.ContainsKey(newSupportPoint),
                        "Expected the new support point to be added.");
            Assert.That(dataComponent.Data[newSupportPoint], Is.SameAs(parameters));
        }

        [Test]
        public void ReplaceSupportPoint_OldSupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var geometricDef = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var newSupportPoint = new SupportPoint(20, geometricDef);

            var dataComponent = new SpatiallyVaryingDataComponent<T>();

            // Call | Assert
            void Call() => dataComponent.ReplaceSupportPoint(null, newSupportPoint);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("oldSupportPoint"));
        }

        [Test]
        public void ReplaceSupportPoint_NewSupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var geometricDef = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var oldSupportPoint = new SupportPoint(20, geometricDef);

            var dataComponent = new SpatiallyVaryingDataComponent<T>();

            // Call | Assert
            void Call() => dataComponent.ReplaceSupportPoint(oldSupportPoint, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("newSupportPoint"));
        }

        [Test]
        public void ReplaceSupportPoint_OldSupportPointNotInData_ThrowsInvalidOperationException()
        {
            // Setup
            var geometricDef = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var oldSupportPoint = new SupportPoint(20, geometricDef);
            var newSupportPoint = new SupportPoint(50, geometricDef);

            var dataComponent = new SpatiallyVaryingDataComponent<T>();

            // Call | Assert
            void Call() => dataComponent.ReplaceSupportPoint(oldSupportPoint, newSupportPoint);

            Assert.Throws<InvalidOperationException>(Call);
        }

        [Test]
        public void ReplaceSupportPoint_NewSupportPointAlreadyInData_ThrowsInvalidOperationException()
        {
            // Setup
            var geometricDef = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var oldSupportPoint = new SupportPoint(20, geometricDef);
            var newSupportPoint = new SupportPoint(50, geometricDef);

            var oldParameters = DataComponentTestUtils.ConstructParameters<T>();
            var newParameters = DataComponentTestUtils.ConstructParameters<T>();

            var dataComponent = new SpatiallyVaryingDataComponent<T>();
            dataComponent.AddParameters(oldSupportPoint, oldParameters);
            dataComponent.AddParameters(newSupportPoint, newParameters);

            // Call | Assert
            void Call() => dataComponent.ReplaceSupportPoint(oldSupportPoint, newSupportPoint);

            Assert.Throws<InvalidOperationException>(Call);
        }

        [Test]
        public void Clear_RemovesAllElements()
        {
            // Setup
            var geometricDef = Substitute.For<IWaveBoundaryGeometricDefinition>();
            var dataComponent = new SpatiallyVaryingDataComponent<T>();

            const int nElements = 6;
            for (var i = 0; i < nElements; i++)
            {
                var supportPoint = new SupportPoint(20 * (i + 1), geometricDef);
                var parameters = DataComponentTestUtils.ConstructParameters<T>();
                
                dataComponent.AddParameters(supportPoint, parameters);
            }

            Assert.That(dataComponent.Data.Keys.Count(), Is.EqualTo(nElements), 
                        $"precondition failed, no {nElements} elements were added.");

            // Call
            dataComponent.Clear();

            Assert.That(dataComponent.Data, Is.Not.Null);
            Assert.That(dataComponent.Data, Is.Empty);
        }
    }
}
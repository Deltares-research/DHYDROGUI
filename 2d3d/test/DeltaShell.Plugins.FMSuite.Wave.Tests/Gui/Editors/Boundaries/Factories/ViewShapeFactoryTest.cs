using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Factories
{
    [TestFixture]
    public class ViewShapeFactoryTest
    {
        [Test]
        [TestCaseSource(nameof(GetConstructFromShapeValidData))]
        public void ConstructFromShape_ReturnsExpectedResult(IBoundaryConditionShape inputShape, Type expectedType)
        {
            // Setup
            var modelFactory = Substitute.For<IBoundaryConditionShapeFactory>();
            var viewFactory = new ViewShapeFactory(modelFactory);

            // Call
            IViewShape result = viewFactory.ConstructFromShape(inputShape);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf(expectedType));
            Assert.That(result.ObservedShape, Is.SameAs(inputShape));
        }

        [Test]
        [TestCaseSource(nameof(GetConstructFromShapeInvalidData))]
        public void ConstructFromShape_InvalidInput_ReturnsNull(IBoundaryConditionShape invalidInput)
        {
            // Setup
            var modelFactory = Substitute.For<IBoundaryConditionShapeFactory>();
            var viewFactory = new ViewShapeFactory(modelFactory);

            // Call
            IViewShape result = viewFactory.ConstructFromShape(invalidInput);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [TestCase(ViewShapeType.Gauss, typeof(GaussViewShape), typeof(GaussShape))]
        [TestCase(ViewShapeType.Jonswap, typeof(JonswapViewShape), typeof(JonswapShape))]
        [TestCase(ViewShapeType.PiersonMoskowitz, typeof(PiersonMoskowitzViewShape), typeof(PiersonMoskowitzShape))]
        public void ConstructFromType_ReturnsExpectedResult(ViewShapeType inputType,
                                                            Type expectedViewType,
                                                            Type expectedObservedShapeType)
        {
            // Setup
            var modelFactory = new BoundaryConditionShapeFactory();
            var viewFactory = new ViewShapeFactory(modelFactory);

            // Call
            IViewShape result = viewFactory.ConstructFromType(inputType);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf(expectedViewType));

            Assert.That(result.ObservedShape, Is.Not.Null);
            Assert.That(result.ObservedShape, Is.InstanceOf(expectedObservedShapeType));
        }

        [Test]
        public void ConstructFromType_InvalidType_ReturnsNull()
        {
            var modelFactory = Substitute.For<IBoundaryConditionShapeFactory>();
            var viewFactory = new ViewShapeFactory(modelFactory);

            const ViewShapeType t = (ViewShapeType) (-1);

            // Call | Assert
            void Call() => viewFactory.ConstructFromType(t);
            Assert.Throws<InvalidEnumArgumentException>(Call);
        }

        [Test]
        public void GetViewShapeTypesList_ExpectedResults()
        {
            // Setup
            var modelFactory = Substitute.For<IBoundaryConditionShapeFactory>();
            var viewFactory = new ViewShapeFactory(modelFactory);

            // Call
            IReadOnlyList<Type> shapeTypes = viewFactory.GetViewShapeTypesList();

            // Assert
            Assert.That(shapeTypes, Is.Not.Null);

            var expectedTypes = new List<Type>
            {
                typeof(GaussViewShape),
                typeof(JonswapViewShape),
                typeof(PiersonMoskowitzViewShape)
            };

            Assert.That(shapeTypes, Is.EquivalentTo(expectedTypes));
        }

        private static IEnumerable<TestCaseData> GetConstructFromShapeValidData()
        {
            yield return new TestCaseData(new GaussShape(), typeof(GaussViewShape));
            yield return new TestCaseData(new JonswapShape(), typeof(JonswapViewShape));
            yield return new TestCaseData(new PiersonMoskowitzShape(), typeof(PiersonMoskowitzViewShape));
        }

        private static IEnumerable<TestCaseData> GetConstructFromShapeInvalidData()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(Substitute.For<IBoundaryConditionShape>());
        }
    }
}
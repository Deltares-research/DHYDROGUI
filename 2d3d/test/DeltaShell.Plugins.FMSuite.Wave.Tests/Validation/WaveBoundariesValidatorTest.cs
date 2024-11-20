using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WaveBoundariesValidatorTest
    {
        private const double correctHeight = 1.0;
        private const double correctPeriod = 2.0;
        private const double correctDirection = 3.0;

        [Test]
        public void Validate_BoundariesNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => WaveBoundariesValidator.Validate(null, new DateTime());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("boundaries"));
        }

        [Test]
        public void Validate_NoBoundaries_ReturnsEmptyReport()
        {
            // Setup
            var boundaries = new EventedList<IWaveBoundary>();

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            Assert.AreEqual(0, report.GetAllIssuesRecursive().Count);
            Assert.AreEqual(0, report.SubReports.Count());
        }

        [Test]
        public void Visit_ConditionDefinitionNull_ThrowsArgumentNullException()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IBoundaryConditionVisitor>().Visit(null));

            // Call
            void Call() => WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveBoundaryConditionDefinition"));
        }

        [Test]
        public void Visit_ConditionDefinition_ShouldCallNextAcceptVisitorMethods()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var shape = Substitute.For<IBoundaryConditionShape>();
            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();

            IWaveBoundaryConditionDefinition waveBoundaryCondition = boundaries[0].ConditionDefinition;
            waveBoundaryCondition.Shape = shape;
            waveBoundaryCondition.DataComponent = dataComponent;

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IBoundaryConditionVisitor>().Visit(waveBoundaryCondition));

            // Call
            WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            shape.Received(1).AcceptVisitor(Arg.Any<IShapeVisitor>());
            dataComponent.Received(1).AcceptVisitor(Arg.Any<ISpatiallyDefinedDataComponentVisitor>());
        }

        [Test]
        public void Visit_JonswapShapeNull_ThrowsArgumentNullException()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IShapeVisitor>().Visit((JonswapShape) null));

            // Call
            void Call() => WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("jonswapShape"));
        }

        [Test]
        [TestCase(0.9, 1)]
        [TestCase(1.0, 0)]
        [TestCase(1.1, 0)]
        [TestCase(9.9, 0)]
        [TestCase(10.0, 0)]
        [TestCase(10.1, 1)]
        public void Visit_JonswapShape_ShouldValidateIfValueIsBetween1And10(double value, double expectedValidationIssuesNr)
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var shape = new JonswapShape {PeakEnhancementFactor = value};
            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IShapeVisitor>().Visit(shape));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(expectedValidationIssuesNr, allIssues.Count);
            if (expectedValidationIssuesNr >= 1)
            {
                Assert.IsTrue(allIssues.Any(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message == Resources.WaveBoundariesValidator_Validate_Peak_Enhancement_Factor_must_be_a_value_within_the_range_1_10_));
            }

            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        public void Visit_GaussShapeNull_DoesNotThrowArgumentNullException()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IShapeVisitor>().Visit((GaussShape) null));

            // Call
            void Call() => WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.DoesNotThrow(Call);
        }

        [Test]
        public void Visit_GaussShape_ShouldDoNothing()
        {
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var shape = new GaussShape();
            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IShapeVisitor>().Visit(shape));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            Assert.AreEqual(0, report.GetAllIssuesRecursive().Count);
            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        public void Visit_PiersonMoskowitzShapeShapeNull_DoesNotThrowArgumentNullException()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IShapeVisitor>().Visit((PiersonMoskowitzShape) null));

            // Call
            void Call() => WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.DoesNotThrow(Call);
        }

        [Test]
        public void Visit_PiersonMoskowitzShapeShape_ShouldDoNothing()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var shape = new PiersonMoskowitzShape();
            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IShapeVisitor>().Visit(shape));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            Assert.AreEqual(0, report.GetAllIssuesRecursive().Count);
            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        public void Visit_UniformDataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<ISpatiallyDefinedDataComponentVisitor>().Visit((UniformDataComponent<IForcingTypeDefinedParameters>) null));

            // Call
            void Call() => WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("uniformDataComponent"));
        }

        [Test]
        public void Visit_UniformConstantPowerDataComponent_AcceptVisitorShouldBeCalled()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var forcingParameters = Substitute.For<IForcingTypeDefinedParameters>();
            var uniformDataComponent = new UniformDataComponent<IForcingTypeDefinedParameters>(forcingParameters);

            IWaveBoundaryConditionDefinition waveBoundaryCondition = boundaries[0].ConditionDefinition;
            waveBoundaryCondition.DataComponent = uniformDataComponent;
            waveBoundaryCondition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                                 .Do(x => x.Arg<ISpatiallyDefinedDataComponentVisitor>().Visit(uniformDataComponent));

            // Call
            WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            uniformDataComponent.Data.Received(1).AcceptVisitor(Arg.Any<IForcingTypeDefinedParametersVisitor>());
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponentNull_ThrowsArgumentNullException()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<ISpatiallyDefinedDataComponentVisitor>().Visit((SpatiallyVaryingDataComponent<IForcingTypeDefinedParameters>) null));

            // Call
            void Call() => WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("spatiallyVaryingDataComponent"));
        }

        [Test]
        public void Visit_SpatiallyVaryingDataComponentWithInactiveSupportPoints_ShouldReturnValidationIssue()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            IWaveBoundaryGeometricDefinition geometryDefinition = boundaries[0].GeometricDefinition;
            IWaveBoundaryConditionDefinition waveBoundaryCondition = boundaries[0].ConditionDefinition;

            var supportPoint1 = new SupportPoint(0, geometryDefinition);
            var supportPoint2 = new SupportPoint(20, geometryDefinition);
            geometryDefinition.SupportPoints.Returns(new EventedList<SupportPoint>
            {
                supportPoint1,
                supportPoint2
            });

            var spatiallyVaryingDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>();

            waveBoundaryCondition.DataComponent = spatiallyVaryingDataComponent;
            waveBoundaryCondition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                                 .Do(x => x.Arg<ISpatiallyDefinedDataComponentVisitor>().Visit(spatiallyVaryingDataComponent));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(1, allIssues.Count);
            Assert.IsTrue(allIssues.Any(i =>
                                            i.Severity == ValidationSeverity.Info &&
                                            i.Message == Resources.WaveBoundariesValidator_Validate_Boundary_condition_contains_unactivated_support_points));
            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        public void Visit_SpatiallyVaryingConstantPowerDataComponent_AcceptVisitorShouldBeCalled()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            IWaveBoundaryGeometricDefinition geometryDefinition = boundaries[0].GeometricDefinition;
            IWaveBoundaryConditionDefinition waveBoundaryCondition = boundaries[0].ConditionDefinition;

            var supportPoint1 = new SupportPoint(0, geometryDefinition);
            var constantParameters1 = Substitute.For<IForcingTypeDefinedParameters>();

            var supportPoint2 = new SupportPoint(20, geometryDefinition);
            var constantParameters2 = Substitute.For<IForcingTypeDefinedParameters>();

            var spatiallyVaryingDataComponentDataComponent = new SpatiallyVaryingDataComponent<IForcingTypeDefinedParameters>();
            spatiallyVaryingDataComponentDataComponent.AddParameters(supportPoint1, constantParameters1);
            spatiallyVaryingDataComponentDataComponent.AddParameters(supportPoint2, constantParameters2);

            waveBoundaryCondition.DataComponent = spatiallyVaryingDataComponentDataComponent;
            waveBoundaryCondition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                                 .Do(x => x.Arg<ISpatiallyDefinedDataComponentVisitor>().Visit(spatiallyVaryingDataComponentDataComponent));

            // Call
            WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            constantParameters1.Received(1).AcceptVisitor(Arg.Any<IForcingTypeDefinedParametersVisitor>());
            constantParameters2.Received(1).AcceptVisitor(Arg.Any<IForcingTypeDefinedParametersVisitor>());
        }

        [Test]
        public void Visit_ConstantParametersNull_ThrowsArgumentNullException()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit((ConstantParameters<PowerDefinedSpreading>) null));

            // Call
            void Call() => WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("constantParameters"));
        }

        [Test]
        [TestCase(-0.1, 1)]
        [TestCase(0, 0)]
        [TestCase(0.1, 0)]
        [TestCase(24.9, 0)]
        [TestCase(25, 0)]
        [TestCase(25.1, 1)]
        public void Visit_ConstantParametersPowerSpreading_ShouldValidateTheHeightValue(double height, double expectedValidationIssuesNr)
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var constantParameters = new ConstantParameters<PowerDefinedSpreading>(height, correctPeriod, correctDirection, new PowerDefinedSpreading());
            SetupVisitingConstantParametersPowerSpreading(boundaries, constantParameters);

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);

            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(expectedValidationIssuesNr, report.GetAllIssuesRecursive().Count);
            if (expectedValidationIssuesNr >= 1)
            {
                Assert.IsTrue(allIssues.Any(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message == Resources.WaveBoundariesValidator_Validate_Parameter_Height_must_be_greater_than_0_and_smaller_or_equal_to_25_));
            }

            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        [TestCase(0.0, 1)]
        [TestCase(0.1, 0)]
        [TestCase(0.2, 0)]
        [TestCase(19.9, 0)]
        [TestCase(20, 0)]
        [TestCase(20.1, 1)]
        public void Visit_ConstantParametersPowerSpreading_ShouldValidateThePeriodValue(double period, double expectedValidationIssuesNr)
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var constantParameters = new ConstantParameters<PowerDefinedSpreading>(correctHeight, period, correctDirection, new PowerDefinedSpreading());
            SetupVisitingConstantParametersPowerSpreading(boundaries, constantParameters);

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(expectedValidationIssuesNr, report.GetAllIssuesRecursive().Count);
            if (expectedValidationIssuesNr >= 1)
            {
                Assert.IsTrue(allIssues.Any(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message == Resources.WaveBoundariesValidator_Validate_Parameter_Period_must_be_a_value_within_the_range_));
            }

            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        [TestCase(-360.1, 1)]
        [TestCase(-360.0, 0)]
        [TestCase(-359.9, 0)]
        [TestCase(359.9, 0)]
        [TestCase(360.0, 0)]
        [TestCase(360.1, 1)]
        public void Visit_ConstantParametersPowerSpreading_ShouldValidateTheDirectionValue(double direction, double expectedValidationIssuesNr)
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var constantParameters = new ConstantParameters<PowerDefinedSpreading>(correctHeight, correctPeriod, direction, new PowerDefinedSpreading());
            SetupVisitingConstantParametersPowerSpreading(boundaries, constantParameters);

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(expectedValidationIssuesNr, report.GetAllIssuesRecursive().Count);
            if (expectedValidationIssuesNr >= 1)
            {
                Assert.IsTrue(allIssues.Any(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message == Resources.WaveBoundariesValidator_Validate_Parameter_Direction_must_be_a_value_within_the_range_360_360_));
            }

            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        [TestCase(0.9, 1)]
        [TestCase(1.0, 0)]
        [TestCase(1.1, 0)]
        [TestCase(799.9, 0)]
        [TestCase(800.0, 0)]
        [TestCase(800.1, 1)]
        public void Visit_ConstantParametersPowerSpreading_ShouldAlsoValidateSpreadingValue(double spreadingPower, double expectedValidationIssuesNr)
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var constantParameters = new ConstantParameters<PowerDefinedSpreading>(correctHeight, correctPeriod, correctDirection, new PowerDefinedSpreading {SpreadingPower = spreadingPower});
            SetupVisitingConstantParametersPowerSpreading(boundaries, constantParameters);

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(expectedValidationIssuesNr, report.GetAllIssuesRecursive().Count);
            if (expectedValidationIssuesNr >= 1)
            {
                Assert.IsTrue(allIssues.Any(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message == Resources.WaveBoundariesValidator_Validate_Parameter_Spreading__must_be_a_value_within_the_range_1_800));
            }

            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        [TestCase(1.9, 1)]
        [TestCase(2.0, 0)]
        [TestCase(2.1, 0)]
        [TestCase(179.9, 0)]
        [TestCase(180, 0)]
        [TestCase(180.1, 1)]
        public void Visit_ConstantParametersDegreesSpreading_ShouldAlsoValidateSpreadingValue(double degreesSpreading, double expectedValidationIssuesNr)
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var constantParameters = new ConstantParameters<DegreesDefinedSpreading>(correctHeight, correctPeriod, correctDirection, new DegreesDefinedSpreading {DegreesSpreading = degreesSpreading});

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(constantParameters));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(expectedValidationIssuesNr, report.GetAllIssuesRecursive().Count);
            if (expectedValidationIssuesNr >= 1)
            {
                Assert.IsTrue(allIssues.Any(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message == Resources.WaveBoundariesValidator_Validate_Parameter_Spreading_must_be_a_value_within_the_range_2_180));
            }

            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        public void Visit_TimeDependentParametersNull_ThrowsArgumentNullException()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit((TimeDependentParameters<PowerDefinedSpreading>) null));

            // Call
            void Call() => WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("timeDependentParameters"));
        }

        [Test]
        [TestCase(-0.1, 1)]
        [TestCase(0, 0)]
        [TestCase(0.1, 0)]
        [TestCase(24.9, 0)]
        [TestCase(25, 0)]
        [TestCase(25.1, 1)]
        public void Visit_TimeDependentParametersPowerSpreading_ShouldValidateTheHeightValues(double height, double expectedValidationIssuesNr)
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            IWaveEnergyFunction<PowerDefinedSpreading> waveEnergyFunction = SetupVisitingTimeDependentParametersPowerSpreading(boundaries);
            IVariable<double> heightVariable = CreateVariableForTestDouble(height);
            waveEnergyFunction.HeightComponent.Returns(heightVariable);

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(expectedValidationIssuesNr, report.GetAllIssuesRecursive().Count);
            if (expectedValidationIssuesNr >= 1)
            {
                Assert.IsTrue(allIssues.Any(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message == Resources.WaveBoundariesValidator_Validate_Values_in_column_Hs_in_the_time_series_table_must_be_within_expected_range));
            }

            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        [TestCase(0.0, 1)]
        [TestCase(0.1, 0)]
        [TestCase(0.2, 0)]
        [TestCase(19.9, 0)]
        [TestCase(20.0, 0)]
        [TestCase(20.1, 1)]
        public void Visit_TimeDependentParametersPowerSpreading_ShouldValidateThePeriodValues(double period, double expectedValidationIssuesNr)
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            IWaveEnergyFunction<PowerDefinedSpreading> waveEnergyFunction = SetupVisitingTimeDependentParametersPowerSpreading(boundaries);
            IVariable<double> periodVariable = CreateVariableForTestDouble(period);
            waveEnergyFunction.PeriodComponent.Returns(periodVariable);

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(expectedValidationIssuesNr, report.GetAllIssuesRecursive().Count);
            if (expectedValidationIssuesNr >= 1)
            {
                Assert.IsTrue(allIssues.Any(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message == Resources.WaveBoundariesValidator_Validate_Values_in_column_Tp_in_the_time_series_table_must_be_within_expected_range));
            }

            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        [TestCase(-360.1, 1)]
        [TestCase(-360, 0)]
        [TestCase(-359.9, 0)]
        [TestCase(359.9, 0)]
        [TestCase(360.0, 0)]
        [TestCase(360.1, 1)]
        public void Visit_TimeDependentParametersPowerSpreading_ShouldValidateTheDirectionValues(double direction, double expectedValidationIssuesNr)
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            IWaveEnergyFunction<PowerDefinedSpreading> waveEnergyFunction = SetupVisitingTimeDependentParametersPowerSpreading(boundaries);
            IVariable<double> directionVariable = CreateVariableForTestDouble(direction);
            waveEnergyFunction.DirectionComponent.Returns(directionVariable);

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(expectedValidationIssuesNr, report.GetAllIssuesRecursive().Count);
            if (expectedValidationIssuesNr >= 1)
            {
                Assert.IsTrue(allIssues.Any(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message == Resources.WaveBoundariesValidator_Validate_Values_in_column_Direction_in_the_time_series_table_must_be_within_expected_range));
            }

            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        [TestCase(0.9, 1)]
        [TestCase(1.0, 0)]
        [TestCase(1.1, 0)]
        [TestCase(799.9, 0)]
        [TestCase(800, 0)]
        [TestCase(800.1, 1)]
        public void Visit_TimeDependentParametersPowerSpreading_ShouldValidateTheSpreadingValues(double spreading, double expectedValidationIssuesNr)
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            IWaveEnergyFunction<PowerDefinedSpreading> waveEnergyFunction = SetupVisitingTimeDependentParametersPowerSpreading(boundaries);
            IVariable<double> spreadingVariable = CreateVariableForTestDouble(spreading);
            waveEnergyFunction.SpreadingComponent.Returns(spreadingVariable);

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(expectedValidationIssuesNr, report.GetAllIssuesRecursive().Count);
            if (expectedValidationIssuesNr >= 1)
            {
                Assert.IsTrue(allIssues.Any(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message == Resources.WaveBoundariesValidator_Validate_Values_in_column_Spreading_in_the_time_series_table_must_be_a_value_within_the_range_1_800));
            }

            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        [TestCase(1.9, 1)]
        [TestCase(2.0, 0)]
        [TestCase(2.1, 0)]
        [TestCase(179.9, 0)]
        [TestCase(180, 0)]
        [TestCase(180.1, 1)]
        public void Visit_TimeDependentParametersDegreesSpreading_ShouldValidateTheSpreadingValues(double spreading, double expectedValidationIssuesNr)
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>();

            var variableSpreading = Substitute.For<IVariable<double>>();
            var valuesSpreading = new MultiDimensionalArray<double> {spreading};
            variableSpreading.Values.Returns(valuesSpreading);

            var variableTime = Substitute.For<IVariable<DateTime>>();
            var valuesTime = new MultiDimensionalArray<DateTime> {new DateTime(2020, 4, 20)};
            variableTime.Values.Returns(valuesTime);

            waveEnergyFunction.SpreadingComponent.Returns(variableSpreading);
            waveEnergyFunction.TimeArgument.Returns(variableTime);

            var timeDependentParameters = new TimeDependentParameters<DegreesDefinedSpreading>(waveEnergyFunction);

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(timeDependentParameters));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(expectedValidationIssuesNr, report.GetAllIssuesRecursive().Count);
            if (expectedValidationIssuesNr >= 1)
            {
                Assert.IsTrue(allIssues.Any(i =>
                                                i.Severity == ValidationSeverity.Error &&
                                                i.Message == Resources.WaveBoundariesValidator_Validate_Values_in_column_Spreading_in_the_time_series_table_must_be_a_value_within_the_range_2_180));
            }

            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        public void Visit_TimeDependentParametersPowerSpreading_ShouldValidateIfThereAreTimeArgumentValues()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();
            var timeDependentParameters = new TimeDependentParameters<PowerDefinedSpreading>(waveEnergyFunction);

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(timeDependentParameters));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(1, report.GetAllIssuesRecursive().Count);
            Assert.IsTrue(allIssues.Any(i =>
                                            i.Severity == ValidationSeverity.Error &&
                                            i.Message == Resources.WaveBoundariesValidator_Validate_Boundary_does_not_contain_any_valid_boundary_data));
            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        public void Visit_TimeDependentParametersPowerSpreadingWithTwoFunctions_ShouldValidateIfTimePointsAreSynchronized()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            IWaveEnergyFunction<PowerDefinedSpreading> waveEnergyFunction1 = CreateWaveEnergyFunctionWithOneTimeArgumentPowerSpreading();

            var waveEnergyFunction2 = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();
            var variable2 = Substitute.For<IVariable<DateTime>>();
            var values2 = new MultiDimensionalArray<DateTime>
            {
                new DateTime(2020, 4, 20),
                new DateTime(2020, 4, 21)
            };
            variable2.Values.Returns(values2);
            waveEnergyFunction2.TimeArgument.Returns(variable2);

            var timeDependentParameters1 = new TimeDependentParameters<PowerDefinedSpreading>(waveEnergyFunction1);
            var timeDependentParameters2 = new TimeDependentParameters<PowerDefinedSpreading>(waveEnergyFunction2);

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x =>
                         {
                             x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(timeDependentParameters1);
                             x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(timeDependentParameters2);
                         });

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(1, allIssues.Count);
            Assert.IsTrue(allIssues.Any(i =>
                                            i.Severity == ValidationSeverity.Error &&
                                            i.Message == string.Format(Resources.WaveBoundariesValidator_Validate_Time_points_are_not_synchronized_on_boundary__0__, boundaries[0].Name)));
            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        public void Visit_TimeDependentParametersPowerSpreading_ShouldReturnValidationErrorIfModelStartTimeIsAfterAllTimesInTimeSeries()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            IWaveEnergyFunction<PowerDefinedSpreading> waveEnergyFunction = CreateWaveEnergyFunctionWithOneTimeArgumentPowerSpreading();
            var timeDependentParameters = new TimeDependentParameters<PowerDefinedSpreading>(waveEnergyFunction);

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(timeDependentParameters));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime(2020, 4, 21));

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(1, allIssues.Count);
            Assert.IsTrue(allIssues.Any(i =>
                                            i.Severity == ValidationSeverity.Error &&
                                            i.Message == string.Format(
                                                Resources.WaveBoundariesValidator_Validate_ModelStartTime_Model_start_time_does_not_precede_any_of_Boundary_Condition_time_points_of__0__,
                                                boundaries[0].Name)));
            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        public void Visit_TimeDependentParametersPowerSpreading_ShouldNotReturnValidationErrorIfModelStartTimeIsNotAfterAllTimesInTimeSeries()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            IWaveEnergyFunction<PowerDefinedSpreading> waveEnergyFunction = CreateWaveEnergyFunctionWithOneTimeArgumentPowerSpreading();
            var timeDependentParameters = new TimeDependentParameters<PowerDefinedSpreading>(waveEnergyFunction);

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(timeDependentParameters));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime(2020, 4, 19));

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(0, allIssues.Count);
            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        public void Visit_FileBasedParameters_ShouldDoNothingIfFilePathIsNotNullAndNotEmpty()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            const string filePath = "NotNullAndNotEmpty";
            var fileBasedParameters = new FileBasedParameters(filePath);

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(fileBasedParameters));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            Assert.AreEqual(0, report.GetAllIssuesRecursive().Count);
            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        [TestCase("")]
        [TestCase("     ")]
        public void Visit_FileBasedParameters_ShouldReturnValidationErrorIfFilePathIsEmptyOrWhitespace(string filePath)
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var fileBasedParameters = new FileBasedParameters(filePath);

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(fileBasedParameters));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);

            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(1, allIssues.Count);
            Assert.IsTrue(allIssues.Any(i =>
                                            i.Severity == ValidationSeverity.Error &&
                                            i.Message == string.Format(
                                                Resources.WaveBoundariesValidator_Validate_FilePath_cannot_be_empty,
                                                boundaries[0].Name)));
            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        public void Visit_FileBasedParameters_ShouldReturnValidationErrorIfFilePathIsNull()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            var fileBasedParameters = new FileBasedParameters(string.Empty);
            fileBasedParameters.FilePath = null;

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(fileBasedParameters));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);

            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(1, allIssues.Count);
            Assert.IsTrue(allIssues.Any(i =>
                                            i.Severity == ValidationSeverity.Error &&
                                            i.Message == string.Format(
                                                Resources.WaveBoundariesValidator_Validate_FilePath_cannot_be_empty,
                                                boundaries[0].Name)));
            Assert.AreEqual(1, report.SubReports.Count());
        }

        [Test]
        public void Visit_FileBasedParametersNull_ThrowsArgumentNullException()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(null));

            // Call
            void Call() => WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("fileBasedParameters"));
        }

        [Test]
        public void Visit_DegreesDefinedSpreadingNull_ThrowsArgumentNullException()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<ISpreadingVisitor>().Visit((DegreesDefinedSpreading) null));

            // Call
            void Call() => WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("degreesDefinedSpreading"));
        }

        [Test]
        public void Visit_PowerDefinedSpreadingNull_ThrowsArgumentNullException()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<ISpreadingVisitor>().Visit((PowerDefinedSpreading) null));

            // Call
            void Call() => WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("powerDefinedSpreading"));
        }

        [Test]
        public void Visit_MultipleBoundaries_ShouldValidateAllBoundaries()
        {
            // Setup
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();
            var boundary2 = Substitute.For<IWaveBoundary>();
            boundaries.Add(boundary2);

            var shape = new JonswapShape {PeakEnhancementFactor = 15};

            boundaries[0].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IShapeVisitor>().Visit(shape));
            boundaries[1].ConditionDefinition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                         .Do(x => x.Arg<IShapeVisitor>().Visit(shape));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            // Assert
            Assert.AreEqual("Waves Model Boundaries", report.Category);
            IList<ValidationIssue> allIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(2, allIssues.Count);
            Assert.AreEqual(2, report.SubReports.Count());
        }

        [Test]
        public void Validate_SpatiallyVaryingConstantPowerBoundary_ShouldGiveValidationIssuesWithSupportPointNumberText()
        {
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            // Create constant parameters with only wrong values.
            var constantParameters1 = new ConstantParameters<PowerDefinedSpreading>(-500, -500, -500, new PowerDefinedSpreading {SpreadingPower = -500});
            var constantParameters2 = new ConstantParameters<PowerDefinedSpreading>(-500, -500, -500, new PowerDefinedSpreading {SpreadingPower = -500});

            IWaveBoundaryGeometricDefinition geometryDefinition = boundaries[0].GeometricDefinition;

            var supportPoint1 = new SupportPoint(0, geometryDefinition);
            var supportPoint2 = new SupportPoint(20, geometryDefinition);

            var spatiallyVaryingDataComponentDataComponent = new SpatiallyVaryingDataComponent<IForcingTypeDefinedParameters>();
            spatiallyVaryingDataComponentDataComponent.AddParameters(supportPoint1, constantParameters1);
            spatiallyVaryingDataComponentDataComponent.AddParameters(supportPoint2, constantParameters2);

            ValidateSpatiallyVaryingConstantParametersMessagesContainingSupportPointNumberText(boundaries, spatiallyVaryingDataComponentDataComponent);
        }

        [Test]
        public void Validate_SpatiallyVaryingConstantDegreesBoundary_ShouldGiveValidationIssuesWithSupportPointNumberText()
        {
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();

            // Create constant parameters with only wrong values.
            var constantParameters1 = new ConstantParameters<DegreesDefinedSpreading>(-500, -500, -500, new DegreesDefinedSpreading {DegreesSpreading = -500});
            var constantParameters2 = new ConstantParameters<DegreesDefinedSpreading>(-500, -500, -500, new DegreesDefinedSpreading {DegreesSpreading = -500});

            IWaveBoundaryGeometricDefinition geometryDefinition = boundaries[0].GeometricDefinition;

            var supportPoint1 = new SupportPoint(0, geometryDefinition);
            var supportPoint2 = new SupportPoint(20, geometryDefinition);

            var spatiallyVaryingDataComponentDataComponent = new SpatiallyVaryingDataComponent<IForcingTypeDefinedParameters>();
            spatiallyVaryingDataComponentDataComponent.AddParameters(supportPoint1, constantParameters1);
            spatiallyVaryingDataComponentDataComponent.AddParameters(supportPoint2, constantParameters2);

            ValidateSpatiallyVaryingConstantParametersMessagesContainingSupportPointNumberText(boundaries, spatiallyVaryingDataComponentDataComponent);
        }

        [Test]
        public void Validate_SpatiallyVaryingTimeDependentPowerBoundary_ShouldGiveValidationIssuesWithSupportPointNumberText()
        {
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();
            IWaveBoundaryGeometricDefinition geometryDefinition = boundaries[0].GeometricDefinition;

            var supportPoint1 = new SupportPoint(0, geometryDefinition);
            var supportPoint2 = new SupportPoint(20, geometryDefinition);

            // Create time dependent parameters with only wrong values.
            var waveEnergyFunction1 = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();
            IVariable<double> variable = CreateVariableForTestDouble(-500);
            waveEnergyFunction1.HeightComponent.Returns(variable);
            waveEnergyFunction1.PeriodComponent.Returns(variable);
            waveEnergyFunction1.DirectionComponent.Returns(variable);
            waveEnergyFunction1.SpreadingComponent.Returns(variable);
            var timeDependentParameters = new TimeDependentParameters<PowerDefinedSpreading>(waveEnergyFunction1);

            var spatiallyVaryingDataComponentDataComponent = new SpatiallyVaryingDataComponent<IForcingTypeDefinedParameters>();
            spatiallyVaryingDataComponentDataComponent.AddParameters(supportPoint1, timeDependentParameters);
            spatiallyVaryingDataComponentDataComponent.AddParameters(supportPoint2, timeDependentParameters);

            ValidateSpatiallyVaryingTimeDependentParametersMessagesContainingSupportPointNumberText(boundaries, spatiallyVaryingDataComponentDataComponent);
        }

        [Test]
        public void Validate_SpatiallyVaryingTimeDependentDegreesBoundary_ShouldGiveValidationIssuesWithSupportPointNumberText()
        {
            EventedList<IWaveBoundary> boundaries = CreateWaveBoundaryInList();
            IWaveBoundaryGeometricDefinition geometryDefinition = boundaries[0].GeometricDefinition;

            var supportPoint1 = new SupportPoint(0, geometryDefinition);
            var supportPoint2 = new SupportPoint(20, geometryDefinition);

            // Create time dependent parameters with only wrong values.
            var waveEnergyFunction1 = Substitute.For<IWaveEnergyFunction<DegreesDefinedSpreading>>();
            IVariable<double> variable = CreateVariableForTestDouble(-500);
            waveEnergyFunction1.HeightComponent.Returns(variable);
            waveEnergyFunction1.PeriodComponent.Returns(variable);
            waveEnergyFunction1.DirectionComponent.Returns(variable);
            waveEnergyFunction1.SpreadingComponent.Returns(variable);
            var timeDependentParameters = new TimeDependentParameters<DegreesDefinedSpreading>(waveEnergyFunction1);

            var spatiallyVaryingDataComponentDataComponent = new SpatiallyVaryingDataComponent<IForcingTypeDefinedParameters>();
            spatiallyVaryingDataComponentDataComponent.AddParameters(supportPoint1, timeDependentParameters);
            spatiallyVaryingDataComponentDataComponent.AddParameters(supportPoint2, timeDependentParameters);

            ValidateSpatiallyVaryingTimeDependentParametersMessagesContainingSupportPointNumberText(boundaries, spatiallyVaryingDataComponentDataComponent);
        }

        private static void ValidateSpatiallyVaryingConstantParametersMessagesContainingSupportPointNumberText(IEventedList<IWaveBoundary> boundaries, SpatiallyVaryingDataComponent<IForcingTypeDefinedParameters> spatiallyVaryingDataComponentDataComponent)
        {
            IWaveBoundaryConditionDefinition waveBoundaryCondition = boundaries[0].ConditionDefinition;
            waveBoundaryCondition.DataComponent = spatiallyVaryingDataComponentDataComponent;
            waveBoundaryCondition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                                 .Do(x => x.Arg<ISpatiallyDefinedDataComponentVisitor>().Visit(spatiallyVaryingDataComponentDataComponent));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime());

            IList<ValidationIssue> validationIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(8, validationIssues.Count);
            for (var i = 0; i < 4; i++)
            {
                Assert.IsTrue(validationIssues[i].Message.StartsWith("Point 1: "));
            }

            for (var i = 4; i < 8; i++)
            {
                Assert.IsTrue(validationIssues[i].Message.StartsWith("Point 2: "));
            }
        }

        private static void ValidateSpatiallyVaryingTimeDependentParametersMessagesContainingSupportPointNumberText(EventedList<IWaveBoundary> boundaries, SpatiallyVaryingDataComponent<IForcingTypeDefinedParameters> spatiallyVaryingDataComponentDataComponent)
        {
            IWaveBoundaryConditionDefinition waveBoundaryCondition = boundaries[0].ConditionDefinition;
            waveBoundaryCondition.DataComponent = spatiallyVaryingDataComponentDataComponent;
            waveBoundaryCondition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                                 .Do(x => x.Arg<ISpatiallyDefinedDataComponentVisitor>().Visit(spatiallyVaryingDataComponentDataComponent));

            // Call
            ValidationReport report = WaveBoundariesValidator.Validate(boundaries, new DateTime(2000, 1, 1));

            IList<ValidationIssue> validationIssues = report.GetAllIssuesRecursive();
            Assert.AreEqual(10, validationIssues.Count);
            for (var i = 0; i < 5; i++)
            {
                Assert.IsTrue(validationIssues[i].Message.StartsWith("Point 1: "));
            }

            for (var i = 5; i < 10; i++)
            {
                Assert.IsTrue(validationIssues[i].Message.StartsWith("Point 2: "));
            }
        }

        private static EventedList<IWaveBoundary> CreateWaveBoundaryInList()
        {
            var boundaries = new EventedList<IWaveBoundary>();
            var boundary = Substitute.For<IWaveBoundary>();
            boundaries.Add(boundary);
            return boundaries;
        }

        private static IVariable<double> CreateVariableForTestDouble(double value)
        {
            var variable = Substitute.For<IVariable<double>>();
            var values = new MultiDimensionalArray<double> {value};
            variable.Values.Returns(values);
            return variable;
        }

        private static void SetupVisitingConstantParametersPowerSpreading(EventedList<IWaveBoundary> boundaries, ConstantParameters<PowerDefinedSpreading> constantParameters)
        {
            IWaveBoundaryConditionDefinition waveBoundaryCondition = boundaries[0].ConditionDefinition;
            waveBoundaryCondition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                                 .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(constantParameters));
        }

        private static IWaveEnergyFunction<PowerDefinedSpreading> SetupVisitingTimeDependentParametersPowerSpreading(EventedList<IWaveBoundary> boundaries)
        {
            IWaveEnergyFunction<PowerDefinedSpreading> waveEnergyFunction = CreateWaveEnergyFunctionWithOneTimeArgumentPowerSpreading();

            IWaveBoundaryConditionDefinition waveBoundaryCondition = boundaries[0].ConditionDefinition;
            var timeDependentParameters = new TimeDependentParameters<PowerDefinedSpreading>(waveEnergyFunction);

            waveBoundaryCondition.When(x => x.AcceptVisitor(Arg.Any<IBoundaryConditionVisitor>()))
                                 .Do(x => x.Arg<IForcingTypeDefinedParametersVisitor>().Visit(timeDependentParameters));

            return waveEnergyFunction;
        }

        private static IWaveEnergyFunction<PowerDefinedSpreading> CreateWaveEnergyFunctionWithOneTimeArgumentPowerSpreading()
        {
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>();

            var variable1 = Substitute.For<IVariable<DateTime>>();
            var values1 = new MultiDimensionalArray<DateTime> {new DateTime(2020, 4, 20)};
            variable1.Values.Returns(values1);
            waveEnergyFunction.TimeArgument.Returns(variable1);
            return waveEnergyFunction;
        }
    }
}
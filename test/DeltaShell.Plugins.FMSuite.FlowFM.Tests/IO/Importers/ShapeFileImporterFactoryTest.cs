using System;
using System.Collections.Generic;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class ShapeFileImporterFactoryTest
    {
        /// <summary>
        /// WHEN ShapeFileImporterFactory Construct is called
        /// THEN no exception is thrown
        /// AND a new ShapeFileImporter is created
        /// </summary>
        [Test]
        public void WhenShapeFileImporterFactoryConstructNewShapeFileImporterIsCalled_ThenNoExceptionIsThrownAndANewShapeFileImporterIsCreated()
        {
            ShapeFileImporter<ILineString, IStructure> importer = null;
            Assert.DoesNotThrow(() => { importer = ShapeFileImporterFactory.Construct<ILineString, IStructure>(); },
                                "Expected no error to be thrown when constructing a new ShapeFileImporter.");

            Assert.That(importer, Is.Not.Null, "Expected a valid ShapeFileImporter to have been created.");
        }

        /// <summary>
        /// GIVEN a set of AfterFeatureCreateActions
        /// WHEN a Chain is created
        /// AND executed
        /// THEN every AfterFeatureCreateAction is executed in the order provided
        /// </summary>
        [Test]
        public void GivenASetOfAfterFeatureCreateActions_WhenAChainIsCreatedAndExecuted_ThenEveryAfterFeatureCreateActionIsExecutedInTheOrderProvided()
        {
            // Given
            var expectedSrcArg = MockRepository.GenerateStrictMock<IFeature>();
            var expectedDestArg = MockRepository.GenerateStrictMock<IFeature>();
            var expectedTargetsArg = MockRepository.GenerateStrictMock<IEnumerable<IFeature>>();

            var action1 = MockRepository.GenerateStrictMock<Action<IFeature, IFeature, IEnumerable<IFeature>>>();
            var action2 = MockRepository.GenerateStrictMock<Action<IFeature, IFeature, IEnumerable<IFeature>>>();
            var action3 = MockRepository.GenerateStrictMock<Action<IFeature, IFeature, IEnumerable<IFeature>>>();

            // Verify ordering
            action1.Expect(e => e.Invoke(expectedSrcArg, expectedDestArg, expectedTargetsArg))
                   .WhenCalled(w => action2.AssertWasNotCalled(f => f.Invoke(expectedSrcArg,
                                                                             expectedDestArg,
                                                                             expectedTargetsArg)))
                   .WhenCalled(w => action3.AssertWasNotCalled(f => f.Invoke(expectedSrcArg,
                                                                             expectedDestArg,
                                                                             expectedTargetsArg)))
                   .Repeat.Once();

            action2.Expect(e => e.Invoke(expectedSrcArg, expectedDestArg, expectedTargetsArg))
                   .WhenCalled(w => action1.AssertWasCalled(f => f.Invoke(expectedSrcArg,
                                                                          expectedDestArg,
                                                                          expectedTargetsArg)))
                   .WhenCalled(w => action3.AssertWasNotCalled(f => f.Invoke(expectedSrcArg,
                                                                             expectedDestArg,
                                                                             expectedTargetsArg)))
                   .Repeat.Once();

            action3.Expect(e => e.Invoke(expectedSrcArg, expectedDestArg, expectedTargetsArg))
                   .WhenCalled(w => action1.AssertWasCalled(f => f.Invoke(expectedSrcArg,
                                                                          expectedDestArg,
                                                                          expectedTargetsArg)))
                   .WhenCalled(w => action2.AssertWasCalled(f => f.Invoke(expectedSrcArg,
                                                                          expectedDestArg,
                                                                          expectedTargetsArg)))
                   .Repeat.Once();

            action1.Replay();
            action2.Replay();
            action3.Replay();

            Action<IFeature, IFeature, IEnumerable<IFeature>> chainAction =
                ShapeFileImporterFactory.AfterFeatureCreateActions.Chain(action1,
                                                                         action2,
                                                                         action3);

            // When
            chainAction.Invoke(expectedSrcArg,
                               expectedDestArg,
                               expectedTargetsArg);

            // Then
            action1.VerifyAllExpectations();
            action2.VerifyAllExpectations();
            action3.VerifyAllExpectations();
        }

        /// <summary>
        /// GIVEN an IFeature
        /// AND a targetFeature
        /// AND a set of targets not containing IFeature with the same name
        /// WHEN TryAddName is executed
        /// THEN targetFeature Name equals IFeature Attribute Name
        /// </summary>
        [Test]
        public void GivenAnIFeatureAndATargetFeatureAndASetOfTargetsNotContainingIFeatureWithTheSameName_WhenTryAddNameIsExecuted_ThenTargetFeatureNameEqualsIFeatureAttributeName()
        {
            // Given
            const string expectedName = "This is my step ladder.";
            var dstArg = new LandBoundary2D() {Name = "I never knew my real ladder."};

            var srcArg = MockRepository.GenerateStrictMock<IFeature>();
            srcArg.Expect(a => a.Attributes)
                  .Return(new DictionaryFeatureAttributeCollection() {{"Name", expectedName}})
                  .Repeat.Any();

            var targetsArg = new List<LandBoundary2D>();

            // When
            ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName(srcArg, dstArg, targetsArg);

            // Then
            Assert.That(dstArg.Name, Is.EqualTo(expectedName),
                        "Expected a different name after updating with action:");
        }

        /// <summary>
        /// GIVEN an IFeature
        /// AND a targetFeature
        /// AND a set of targets containing IFeature with the same name
        /// WHEN TryAddName is executed
        /// THEN targetFeature Name equals IFeature Attribute Name with an index
        /// </summary>
        [Test]
        public void GivenAnIFeatureAndATargetFeatureAndASetOfTargetsContainingIFeatureWithTheSameName_WhenTryAddNameIsExecuted_ThenTargetFeatureNameEqualsIFeatureAttributeNameWithAnIndex()
        {
            // Given
            const string expectedName = "I_Really_Should_Not_Be_Allowed_To_Test_With_Strings";
            var dstArg = new LandBoundary2D() {Name = "Old_Name"};

            var targetBnd = new LandBoundary2D() {Name = expectedName};

            var srcArg = MockRepository.GenerateStrictMock<IFeature>();
            srcArg.Expect(a => a.Attributes)
                  .Return(new DictionaryFeatureAttributeCollection() {{"Name", expectedName}})
                  .Repeat.Any();

            var targetsArg = new List<LandBoundary2D>() {targetBnd};

            // When
            ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName(srcArg, dstArg, targetsArg);

            // Then
            Assert.That(dstArg.Name, Is.EqualTo($"{expectedName}_1"),
                        "Expected a different name after updating with action:");
        }

        /// <summary>
        /// GIVEN an IFeature without a name
        /// AND a targetFeature
        /// AND a set of targets not containing the default name
        /// WHEN TryAddName is executed
        /// THEN a targetFeature Name equals the default name
        /// </summary>
        [Test]
        public void GivenAnIFeatureWithoutANameAndATargetFeatureAndASetOfTargetsNotContainingTheDefaultName_WhenTryAddNameIsExecuted_ThenATargetFeatureNameEqualsTheDefaultName()
        {
            // Given
            const string expectedName = "imported_feature";
            var dstArg = new LandBoundary2D() {Name = "Old_Name"};

            var srcArg = MockRepository.GenerateStrictMock<IFeature>();
            srcArg.Expect(a => a.Attributes)
                  .Return(new DictionaryFeatureAttributeCollection() {})
                  .Repeat.Any();

            var targetsArg = new List<LandBoundary2D>();

            // When
            ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName(srcArg, dstArg, targetsArg);

            // Then
            Assert.That(dstArg.Name, Is.EqualTo(expectedName),
                        "Expected a different name after updating with action:");
        }

        /// <summary>
        /// GIVEN an IFeature without a name
        /// AND a targetFeature
        /// AND a set of targets containing the default name
        /// WHEN TryAddName is executed
        /// THEN a targetFeature Name equals the default name with an index
        /// </summary>
        [Test]
        public void GivenAnIFeatureWithoutANameAndATargetFeatureAndASetOfTargetsContainingTheDefaultName_WhenTryAddNameIsExecuted_ThenATargetFeatureNameEqualsTheDefaultNameWithAnIndex()
        {
            // Given
            const string expectedName = "imported_feature";
            var dstArg = new LandBoundary2D() {Name = "Old_Name"};

            var targetBnd = new LandBoundary2D() {Name = expectedName};

            var srcArg = MockRepository.GenerateStrictMock<IFeature>();
            srcArg.Expect(a => a.Attributes)
                  .Return(new DictionaryFeatureAttributeCollection() {{"Name", expectedName}})
                  .Repeat.Any();

            var targetsArg = new List<LandBoundary2D>() {targetBnd};

            // When
            ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName(srcArg, dstArg, targetsArg);

            // Then
            Assert.That(dstArg.Name, Is.EqualTo($"{expectedName}_1"),
                        "Expected a different name after updating with action:");
        }

        /// <summary>
        /// GIVEN an IFeature with a crest width
        /// AND a targetFeature
        /// WHEN TryAddCrestWidth is executed
        /// THEN targetFeature CrestWidth equals IFeature crest width
        /// </summary>
        [Test]
        public void GivenAnIFeatureWithACrestWidthAndATargetFeature_WhenTryAddCrestWidthIsExecuted_ThenTargetFeatureCrestWidthEqualsIFeatureCrestWidth()
        {
            // Given
            const double expectedCrestWidth = 10.0;
            var dstArg = new Structure() {CrestWidth = -10.0};

            var srcArg = MockRepository.GenerateStrictMock<IFeature>();
            srcArg.Expect(a => a.Attributes)
                  .Return(new DictionaryFeatureAttributeCollection() {{"CrestWidth", expectedCrestWidth}})
                  .Repeat.Any();

            var targetsArg = new List<IStructure>();

            // When
            ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddCrestWidth(srcArg, dstArg, targetsArg);

            // Then
            Assert.That(dstArg.CrestWidth, Is.EqualTo(expectedCrestWidth),
                        "Expected a different crest width after updating with action:");
        }

        /// <summary>
        /// GIVEN an IFeature without a crest width
        /// AND a targetFeature
        /// WHEN TryAddCrestWidth is executed
        /// THEN targetFeature CrestWidth is unchanged
        /// </summary>
        [Test]
        public void GivenAnIFeatureWithoutACrestWidthAndATargetFeature_WhenTryAddCrestWidthIsExecuted_ThenTargetFeatureCrestWidthIsUnchanged()
        {
            // Given
            const double expectedCrestWidth = 10.0;
            var dstArg = new Structure() {CrestWidth = expectedCrestWidth};

            var srcArg = MockRepository.GenerateStrictMock<IFeature>();
            srcArg.Expect(a => a.Attributes)
                  .Return(new DictionaryFeatureAttributeCollection() {})
                  .Repeat.Any();

            var targetsArg = new List<IStructure>();

            // When
            ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddCrestWidth(srcArg, dstArg, targetsArg);

            // Then
            Assert.That(dstArg.CrestWidth, Is.EqualTo(expectedCrestWidth),
                        "Expected no change in crest width:");
        }

        /// <summary>
        /// GIVEN an IFeature with a crest level
        /// AND a targetFeature
        /// WHEN TryAddCrestLevel is executed
        /// THEN targetFeature CrestLevel equals IFeature crest level
        /// </summary>
        [Test]
        public void GivenAnIFeatureWithACrestLevelAndATargetFeature_WhenTryAddCrestLevelIsExecuted_ThenTargetFeatureCrestLevelEqualsIFeatureCrestLevel()
        {
            // Given
            const double expectedCrestLevel = 10.0;
            var dstArg = new Structure() {CrestLevel = -10.0};

            var srcArg = MockRepository.GenerateStrictMock<IFeature>();
            srcArg.Expect(a => a.Attributes)
                  .Return(new DictionaryFeatureAttributeCollection() {{"CrestLevel", expectedCrestLevel}})
                  .Repeat.Any();

            var targetsArg = new List<IStructure>();

            // When
            ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddCrestLevel(srcArg, dstArg, targetsArg);

            // Then
            Assert.That(dstArg.CrestLevel, Is.EqualTo(expectedCrestLevel),
                        "Expected a different crest level after updating with action:");
        }

        /// <summary>
        /// GIVEN an IFeature without a crest level
        /// AND a targetFeature
        /// WHEN TryAddCrestLevel is executed
        /// THEN targetFeature CrestLevel is unchanged
        /// </summary>
        [Test]
        public void GivenAnIFeatureWithoutACrestLevelAndATargetFeature_WhenTryAddCrestLevelIsExecuted_ThenTargetFeatureCrestLevelIsUnchanged()
        {
            // Given
            const double expectedCrestLevel = 10.0;
            var dstArg = new Structure() {CrestLevel = expectedCrestLevel};

            var srcArg = MockRepository.GenerateStrictMock<IFeature>();
            srcArg.Expect(a => a.Attributes)
                  .Return(new DictionaryFeatureAttributeCollection() {})
                  .Repeat.Any();

            var targetsArg = new List<IStructure>();

            // When
            ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddCrestLevel(srcArg, dstArg, targetsArg);

            // Then
            Assert.That(dstArg.CrestLevel, Is.EqualTo(expectedCrestLevel),
                        "Expected no change in crest level:");
        }

        /// <summary>
        /// GIVEN an IFeature without a FormulaName
        /// AND a targetFeature
        /// WHEN TryAddWeirFormula is executed
        /// THEN targetFeature WeirFormula Name is unchanged
        /// </summary>
        [Test]
        public void GivenAnIFeatureWithoutAFormulaNameAndATargetFeature_WhenTryAddWeirFormulaIsExecuted_ThenTargetFeatureWeirFormulaNameIsUnchanged()
        {
            // Given
            var dstArg = new Structure();
            IStructureFormula formula = dstArg.Formula;

            var srcArg = MockRepository.GenerateStrictMock<IFeature>();
            srcArg.Expect(a => a.Attributes)
                  .Return(new DictionaryFeatureAttributeCollection() {})
                  .Repeat.Any();

            var targetsArg = new List<IStructure>();

            // When
            ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddWeirFormula(srcArg, dstArg, targetsArg);

            // Then
            Assert.That(dstArg.Formula, Is.Not.Null,
                        "Expected a weir formula not to be null:");
            Assert.That(dstArg.Formula, Is.SameAs(formula),
                        "Expected no change in the weir formula updating with action:");
        }

        /// <summary>
        /// GIVEN an IFeature with a capacity
        /// AND a targetFeature
        /// WHEN TryAddCapacity is executed
        /// THEN targetFeature Capacity equals IFeature capacity
        /// </summary>
        [Test]
        public void GivenAnIFeatureWithACapacityAndATargetFeature_WhenTryAddCapacityIsExecuted_ThenTargetFeatureCapacityEqualsIFeatureCapacity()
        {
            // Given
            const double expectedCapacity = 10.0;
            var dstArg = new Pump() {Capacity = -10.0};

            var srcArg = MockRepository.GenerateStrictMock<IFeature>();
            srcArg.Expect(a => a.Attributes)
                  .Return(new DictionaryFeatureAttributeCollection() {{"Capacity", expectedCapacity}})
                  .Repeat.Any();

            var targetsArg = new List<IPump>();

            // When
            ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddCapacity(srcArg, dstArg, targetsArg);

            // Then
            Assert.That(dstArg.Capacity, Is.EqualTo(expectedCapacity),
                        "Expected a different capacity after updating with action:");
        }

        /// <summary>
        /// GIVEN an IFeature without a Capacity
        /// AND a targetFeature
        /// WHEN TryAddCapacity is executed
        /// THEN targetFeature Capacity is unchanged
        /// </summary>
        [Test]
        public void GivenAnIFeatureWithoutACapacityAndATargetFeature_WhenTryAddCapacityIsExecuted_ThenTargetFeatureCapacityIsUnchanged()
        {
            // Given
            const double expectedCapacity = 10.0;
            var dstArg = new Pump() {Capacity = expectedCapacity};

            var srcArg = MockRepository.GenerateStrictMock<IFeature>();
            srcArg.Expect(a => a.Attributes)
                  .Return(new DictionaryFeatureAttributeCollection() {})
                  .Repeat.Any();

            var targetsArg = new List<IPump>();

            // When
            ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddCapacity(srcArg, dstArg, targetsArg);

            // Then
            Assert.That(dstArg.Capacity, Is.EqualTo(expectedCapacity),
                        "Expected no change in capacity:");
        }

        /// <summary>
        /// GIVEN an IFeature with a FormulaName
        /// AND a targetFeature
        /// WHEN TryAddWeirFormula is executed
        /// THEN targetFeature WeirFormula Name equals IFeature FormulaName
        /// </summary>
        [TestCase("Simple Weir", typeof(SimpleWeirFormula))]
        [TestCase("Simple Gate", typeof(SimpleGateFormula))]
        [TestCase("General Structure", typeof(GeneralStructureFormula))]
        public void GivenAnIFeatureWithAFormulaNameAndATargetFeature_WhenTryAddWeirFormulaIsExecuted_ThenTargetFeatureWeirFormulaNameEqualsIFeatureFormulaName(string formulaName,
                                                                                                                                                               Type expectedType)
        {
            // Given
            var dstArg = new Structure() {Formula = null};

            var srcArg = MockRepository.GenerateStrictMock<IFeature>();
            srcArg.Expect(a => a.Attributes)
                  .Return(new DictionaryFeatureAttributeCollection() {{"FormulaName", formulaName}})
                  .Repeat.Any();

            var targetsArg = new List<IStructure>();

            // When
            ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddWeirFormula(srcArg, dstArg, targetsArg);

            // Then
            Assert.That(dstArg.Formula, Is.Not.Null,
                        "Expected a weir formula to be set after the action has been executed:");
            Assert.That(dstArg.Formula, Is.TypeOf(expectedType),
                        "Expected a different weir formula type after updating with action:");
        }
    }
}
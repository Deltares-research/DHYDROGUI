using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelValidatorTest
    {
        [Test]
        public void ValidateEmptyModel()
        {
            var model = new WaterQualityModel();
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            Assert.AreEqual(0, report.Issues.Count());
            Assert.AreEqual(2, report.AllErrors.Count()); // no substances and Hyd file
        }

        [Test]
        public void Validate_WhenHydFileDoesNotExist_ThenReportContainsExpectedValidationIssue()
        {
            // Set-up
            const string filePath = "path";
            var hydroData = MockRepository.GenerateStub<IHydroData>();
            hydroData.Stub(d => d.FilePath).Return(filePath);
            WaterQualityModel model = GetStubbedWaterQualityModel(hydroData);

            // Act
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // Assert
            ValidationIssue issue = report.AllErrors.SingleOrDefault(
                e => e.Message.Equals($"The hyd file with location {filePath} doesn't exist."));
            Assert.That(issue, Is.Not.Null, "Validation issue was not found");
            Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Error),
                        "Validation issue should have error severity.");
            Assert.That(issue.ViewData.GetType(), Is.EqualTo(typeof(HydFileImporter)),
                        $"View data of validation issue should be {typeof(HydFileImporter)}.");
        }

        [Test]
        [TestCase(@"segFileA.tau", true)]
        [TestCase(@"segFileB.tau", false)]
        [TestCase(@"", false)]
        public void ValidateModelWithProcessCoeficientsSegmentFunctionType(string segmentFunctionFile,
                                                                           bool validationExpected)
        {
            var model = new WaterQualityModel();
            string dataDir = Path.Combine(TestHelper.GetTestDataDirectory(), @"TestSegFunctionFiles");
            string segmentFunctionPath = segmentFunctionFile != "" ? Path.Combine(dataDir, segmentFunctionFile) : null;

            // setup (we don't really care for the content, just for the path being valid or not.
            SegmentFileFunction segFunc = WaterQualityFunctionFactory.CreateSegmentFunction("A", 1.2, "irrelevant", "g", "A",
                                                                                            segmentFunctionPath);
            model.ProcessCoefficients.Add(segFunc);

            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            Assert.AreEqual(0, report.Issues.Count());
            Assert.That(report.AllErrors.Count(), Is.EqualTo(2 + (validationExpected ? 0 : 1))); // no substances and no hyd file initially.
            if (!validationExpected)
            {
                string message = string.Format("Segmentation file for function: {0} not specified", segFunc.Name);
                if (segmentFunctionFile != "")
                {
                    message = string.Format("Could not find segmentation file for function: {0}", segFunc.Name);
                }

                var validationFailed = new ValidationIssue(segFunc, ValidationSeverity.Error, message);
                Assert.True(report.AllErrors.Contains(validationFailed));
            }
        }

        [Test]
        public void ModelWantingToUseTooManyThreads()
        {
            // Pretty sure this will be a valid 'too big' value for any machine available ^_^
            var model = new WaterQualityModel {ModelSettings = {NrOfThreads = int.MaxValue}};

            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            AssertExpectedValidationIssue(report, "This machine cannot use more threads than ", i => Equals(i.Subject, model) && i.Severity == ValidationSeverity.Error);
        }

        private static WaterQualityModel GetStubbedWaterQualityModel(IHydroData hydroData)
        {
            var model = MockRepository.GenerateStub<WaterQualityModel>();
            model.Stub(m => m.HydroData).Return(hydroData);
            model.Stub(m => m.ModelSettings).Return(new WaterQualityModelSettings());
            model.Stub(m => m.ObservationPoints).Return(new EventedList<WaterQualityObservationPoint>());
            model.Stub(m => m.ObservationAreas).Return(new WaterQualityObservationAreaCoverage(new UnstructuredGrid()));
            model.Stub(m => m.Loads).Return(new EventedList<WaterQualityLoad>());
            model.Stub(m => m.ProcessCoefficients).Return(new EventedList<IFunction>());
            return model;
        }

        private static void AssertExpectedValidationIssue(ValidationReport report, string text, Func<ValidationIssue, bool> filter)
        {
            Assert.That(report.GetAllIssuesRecursive().Where(filter).Any(i => i.Message.Contains(text)),
                        "Expected message:" + Environment.NewLine +
                        text + Environment.NewLine +
                        "Available validation messages:" + Environment.NewLine +
                        string.Join(Environment.NewLine,
                                    report.GetAllIssuesRecursive().Select(i => string.Format("{0}: {1}", i.Severity, i.Message))));
        }

        #region Observation Point / Areas

        [Test]
        public void ModelWithSameNamedObservationPointAndObservationAreaIsInvalid()
        {
            // setup
            var model = new WaterQualityModel();
            var data = new TestHydroDataStub();
            model.ImportHydroData(data);
            model.ObservationAreas.SetValuesAsLabels(new[]
            {
                "A",
                "B",
                "A",
                "A"
            });
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                Z = (model.ZTop + model.ZBot) / 2
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            const string expectedMessageA = "Observation point names should be unique and another observation area with name 'A' was already found.";
            AssertExpectedValidationIssue(report, expectedMessageA,
                                          i => Equals(i.Subject, model.ObservationPoints[0]) && i.Severity == ValidationSeverity.Error);
        }

        [Test]
        public void ModelWithNonUniqueNamedObservationPointsIsInvalid()
        {
            // setup
            var model = new WaterQualityModel();
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                Z = (model.ZTop + model.ZBot) / 2
            });
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "B",
                Z = (model.ZTop + model.ZBot) / 2
            });
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                Z = (model.ZTop + model.ZBot) / 2
            });
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "B",
                Z = (model.ZTop + model.ZBot) / 2
            });
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                Z = (model.ZTop + model.ZBot) / 2
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            const string expectedMessageA = "Observation point names should be unique and another observation point with name 'A' was already found.";
            AssertExpectedValidationIssue(report, expectedMessageA,
                                          i => Equals(i.Subject, model.ObservationPoints[2]) && i.Severity == ValidationSeverity.Error);
            AssertExpectedValidationIssue(report, expectedMessageA,
                                          i => Equals(i.Subject, model.ObservationPoints[4]) && i.Severity == ValidationSeverity.Error);

            const string expectedMessageB = "Observation point names should be unique and another observation point with name 'B' was already found.";
            AssertExpectedValidationIssue(report, expectedMessageB,
                                          i => Equals(i.Subject, model.ObservationPoints[3]) && i.Severity == ValidationSeverity.Error);
        }

        [Test]
        public void ModelWithObservationPointThatHaveNoDefinedZIsInvalid()
        {
            // setup
            var model = new WaterQualityModel();
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                ObservationPointType = ObservationPointType.SinglePoint,
                Z = double.NaN
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            const string expectedMessage = "Observation point 'A' has an undefined Z.";
            AssertExpectedValidationIssue(report, expectedMessage,
                                          i => Equals(i.Subject, model.ObservationPoints[0]) && i.Severity == ValidationSeverity.Error);

            string expectedIlligalZMessage = string.Format("Observation point 'A' has height of {0}, but is required to be in range [0, 1].",
                                                           double.NaN);
            Assert.IsFalse(report.GetAllIssuesRecursive().Any(i => i.Message.Contains(expectedIlligalZMessage)),
                           "If Z is NaN, there should be no message about Z being out of a certain range.");
        }

        [Test]
        public void ModelWithObservationPointsButLevelIsNone()
        {
            // setup
            var model = new WaterQualityModel();
            model.ModelSettings.MonitoringOutputLevel = MonitoringOutputLevel.None;
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                ObservationPointType = ObservationPointType.SinglePoint,
                Z = 0
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);
            var expectedMessage = "There are observation points available, but the monitoring output level excludes them from delwaq. Level: None";

            AssertExpectedValidationIssue(report, expectedMessage, i => Equals(i.Subject, model) && i.Severity == ValidationSeverity.Warning);
        }

        [Test]
        public void ModelWithObservationPointsButLevelIsAreas()
        {
            // setup
            var model = new WaterQualityModel();
            model.ModelSettings.MonitoringOutputLevel = MonitoringOutputLevel.Areas;
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                ObservationPointType = ObservationPointType.SinglePoint,
                Z = 0
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);
            var expectedMessage = "There are observation points available, but the monitoring output level excludes them from delwaq. Level: Areas";

            AssertExpectedValidationIssue(report, expectedMessage, i => Equals(i.Subject, model) && i.Severity == ValidationSeverity.Warning);
        }

        [Test]
        [Combinatorial]
        public void ModelWithObservationPointThatHaveNoDefinedZAndIsNotSinglePointIsValid(
            [Values(ObservationPointType.Average,
                    ObservationPointType.OneOnEachLayer)]
            ObservationPointType type)
        {
            // setup
            Assert.AreNotEqual(ObservationPointType.SinglePoint, type,
                               "Test precondition: do not evaluate for SinglePoint");

            var model = new WaterQualityModel();
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                ObservationPointType = type,
                Z = double.NaN
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            const string expectedMessage = "Observation point 'A' has an undefined Z.";
            Assert.IsFalse(report.GetAllIssuesRecursive().Any(i => i.Message.Contains(expectedMessage)),
                           "There should be no validation message about Z is NaN for non-SinglePoint types.");

            string expectedIlligalZMessage = string.Format("Observation point 'A' has height of {0}, but is required to be in range [0, 1].",
                                                           double.NaN);
            Assert.IsFalse(report.GetAllIssuesRecursive().Any(i => i.Message.Contains(expectedIlligalZMessage)),
                           "There should be no validation message about Z is NaN for non-SinglePoint types.");
        }

        [Test]
        [Combinatorial]
        public void SigmaModelWithObservationPointWithIllegalZIsInvalid(
            [Values(1.0 + 1e-6, 12.34, 0.0 - 1e-6, -34.56)]
            double z)
        {
            // setup
            var hydroData = new TestHydroDataStub {ModelType = HydroDynamicModelType.Unstructured};

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.AreEqual(HydroDynamicModelType.Unstructured, model.ModelType,
                            "Precondition: Model is set up as Sigma (unstructured) model.");

            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                ObservationPointType = ObservationPointType.SinglePoint,
                X = 5,
                Y = 5,
                Z = z
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            string expectedMessage = string.Format("Observation point 'A' has height of {0}, but is required to be in range [0, 1].",
                                                   z);
            AssertExpectedValidationIssue(report, expectedMessage, i => i.Severity == ValidationSeverity.Error);
        }

        [Test]
        [Combinatorial]
        public void SigmaModelWithObservationPointsWithlegalZIsValid(
            [Values(ObservationPointType.SinglePoint,
                    ObservationPointType.Average,
                    ObservationPointType.OneOnEachLayer)]
            ObservationPointType type)
        {
            // setup
            var hydroData = new TestHydroDataStub {ModelType = HydroDynamicModelType.Unstructured};

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.AreEqual(HydroDynamicModelType.Unstructured, model.ModelType,
                            "Precondition: Model is set up as Sigma (unstructured) model.");

            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                ObservationPointType = type,
                X = 5,
                Y = 5,
                Z = 1.0
            });
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "B",
                ObservationPointType = type,
                X = 5,
                Y = 5,
                Z = 0.34
            });
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "C",
                ObservationPointType = type,
                X = 5,
                Y = 5,
                Z = 0.0
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            Assert.AreEqual(0, report.Issues.Count());
        }

        [Test]
        [Combinatorial]
        public void ZlayerModelWithObservationPointWithIllegalZIsInvalid(
            [Values(12.34, -2.5 + 1e-6, -6.8 - 1e-6, -34.56)]
            double z)
        {
            // setup
            const double topLevel = -2.5;
            const double bottomLevel = -6.8;
            var hydroData = new TestHydroDataStub
            {
                LayerType = LayerType.ZLayer,
                Ztop = topLevel,
                Zbot = bottomLevel
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.AreEqual(LayerType.ZLayer, model.LayerType,
                            "Precondition: Model is set up as Z-layer (unstructured) model.");

            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                ObservationPointType = ObservationPointType.SinglePoint,
                X = 5,
                Y = 5,
                Z = z
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            string expectedMessage = string.Format("Observation point 'A' has height of {0}, but is required to be in range [{1}, {2}].",
                                                   z, bottomLevel, topLevel);
            AssertExpectedValidationIssue(report, expectedMessage, i => i.Severity == ValidationSeverity.Error);
        }

        [Test]
        [Combinatorial]
        public void ZLayerModelWithObservationPointWithlegalZIsValid(
            [Values(ObservationPointType.SinglePoint,
                    ObservationPointType.Average,
                    ObservationPointType.OneOnEachLayer)]
            ObservationPointType type)
        {
            // setup
            var hydroData = new TestHydroDataStub
            {
                LayerType = LayerType.ZLayer,
                Ztop = -2.5,
                Zbot = -6.8
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.AreEqual(LayerType.ZLayer, model.LayerType,
                            "Precondition: Model is set up as Z-layer (unstructured) model.");

            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                ObservationPointType = type,
                X = 5,
                Y = 5,
                Z = -2.5
            });
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "B",
                ObservationPointType = type,
                X = 5,
                Y = 5,
                Z = -4.69
            });
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "C",
                ObservationPointType = type,
                X = 5,
                Y = 5,
                Z = -6.8
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            Assert.AreEqual(0, report.Issues.Count());
        }

        [Test]
        [Combinatorial]
        public void ModelWithObservationPointsInsideInactiveCellGivesWarning(
            [Values(ObservationPointType.SinglePoint,
                    ObservationPointType.Average,
                    ObservationPointType.OneOnEachLayer)]
            ObservationPointType type)
        {
            // setup
            var hydroData = new TestHydroDataStub
            {
                LayerType = LayerType.ZLayer,
                Ztop = 3.0,
                Zbot = -3.0,
                ThirdCellIsInactive = true
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.IsTrue(model.HasHydroDataImported);

            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                ObservationPointType = type,
                X = 5,
                Y = 5,
                Z = 0.5
            });
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "B",
                ObservationPointType = type,
                X = 5,
                Y = 15,
                Z = 0.5
            });
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "C",
                ObservationPointType = type,
                X = 15,
                Y = 5,
                Z = 0.5
            });
            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "D",
                ObservationPointType = type,
                X = 15,
                Y = 15,
                Z = 0.5
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            const string expectedMessageB = "Observation point 'B' is inside an inactive cell.";
            AssertExpectedValidationIssue(report, expectedMessageB,
                                          i => Equals(i.Subject, model.ObservationPoints[1]) && i.Severity == ValidationSeverity.Warning);
        }

        [Test]
        public void ModelWithObservationPointOutsideGridIsInvalid([Values(0.0 - double.Epsilon, 20.0 + double.Epsilon)]
                                                                  double x,
                                                                  [Values(0.0 - double.Epsilon, 20.0 + double.Epsilon)]
                                                                  double y,
                                                                  [Values(ObservationPointType.SinglePoint, ObservationPointType.Average, ObservationPointType.OneOnEachLayer)]
                                                                  ObservationPointType type)
        {
            // setup
            var hydroData = new TestHydroDataStub
            {
                LayerType = LayerType.ZLayer,
                Ztop = 3.0,
                Zbot = -3.0
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.IsTrue(model.HasHydroDataImported);

            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "A",
                ObservationPointType = type,
                X = x,
                Y = y,
                Z = type == ObservationPointType.SinglePoint ? 0.5 : double.NaN
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            AssertExpectedValidationIssue(report,
                                          "Observation point 'A' is not within grid or has ambiguous location (on a grid edge or grid vertex).",
                                          i => Equals(i.Subject, model.ObservationPoints[0]) && i.Severity == ValidationSeverity.Error);
        }

        [Test]
        [Combinatorial]
        public void ModelWithObservationPointAtAmbiguousLocationIsInvalid(
            [Values(0.0, 10.0, 20)] double x,
            [Values(0.0, 10.0, 20)] double y,
            [Values(ObservationPointType.SinglePoint,
                    ObservationPointType.Average,
                    ObservationPointType.OneOnEachLayer)]
            ObservationPointType type)
        {
            // setup
            var hydroData = new TestHydroDataStub
            {
                LayerType = LayerType.ZLayer,
                Ztop = 3.0,
                Zbot = -3.0
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.IsTrue(model.HasHydroDataImported);

            model.ObservationPoints.Add(new WaterQualityObservationPoint
            {
                Name = "T",
                ObservationPointType = type,
                X = x,
                Y = y,
                Z = 0.5
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            AssertExpectedValidationIssue(report,
                                          "Observation point 'T' is not within grid or has ambiguous location (on a grid edge or grid vertex).",
                                          i => Equals(i.Subject, model.ObservationPoints[0]) && i.Severity == ValidationSeverity.Error);
        }

        #endregion

        #region Loads

        [Test]
        public void ModelWithMissingProcDefFileIsInvalid()
        {
            // setup
            var hydroData = new TestHydroDataStub
            {
                LayerType = LayerType.ZLayer,
                Ztop = 3.0,
                Zbot = -3.0
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.IsNotNull(model.SubstanceProcessLibrary);

            // set path to process definition files
            model.SubstanceProcessLibrary.ProcessDefinitionFilesPath = "thisPathDoesNotExist";

            // add a substance
            model.SubstanceProcessLibrary.Substances.Add(new WaterQualitySubstance {Name = "substance"});

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            Assert.IsTrue(report.AllErrors.Any(e => e.Subject is SubstanceProcessLibrary));
        }

        [Test]
        public void ModelWithNonUniqueNamedLoadsIsInvalid()
        {
            // setup
            var model = new WaterQualityModel();
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "A",
                Z = (model.ZTop + model.ZBot) / 2
            });
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "B",
                Z = (model.ZTop + model.ZBot) / 2
            });
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "A",
                Z = (model.ZTop + model.ZBot) / 2
            });
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "B",
                Z = (model.ZTop + model.ZBot) / 2
            });
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "A",
                Z = (model.ZTop + model.ZBot) / 2
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            const string expectedMessageA = "Load names should be unique and another load with name 'A' was already found.";
            AssertExpectedValidationIssue(report, expectedMessageA,
                                          i => Equals(i.Subject, model.Loads[2]) && i.Severity == ValidationSeverity.Error);
            AssertExpectedValidationIssue(report, expectedMessageA,
                                          i => Equals(i.Subject, model.Loads[4]) && i.Severity == ValidationSeverity.Error);

            const string expectedMessageB = "Load names should be unique and another load with name 'B' was already found.";
            AssertExpectedValidationIssue(report, expectedMessageB,
                                          i => Equals(i.Subject, model.Loads[3]) && i.Severity == ValidationSeverity.Error);
        }

        [Test]
        public void ModelWithLoadsThatHaveNoDefinedZIsInvalid()
        {
            // setup
            var model = new WaterQualityModel();
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "A",
                Z = double.NaN
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            const string expectedMessage = "Load 'A' has an undefined Z.";
            AssertExpectedValidationIssue(report, expectedMessage,
                                          i => Equals(i.Subject, model.Loads[0]) && i.Severity == ValidationSeverity.Error);

            string expectedIlligalZMessage = string.Format("Load 'A' has height of {0}, but is required to be in range [0, 1].",
                                                           double.NaN);
            Assert.IsFalse(report.GetAllIssuesRecursive().Any(i => i.Message.Contains(expectedIlligalZMessage)),
                           "If Z is NaN, there should be no message about Z being out of a certain range.");
        }

        [Test]
        [TestCase(1.0 + 1e-6)]
        [TestCase(12.34)]
        [TestCase(0.0 - 1e-6)]
        [TestCase(-34.56)]
        public void SigmaModelWithLoadWithIllegalZIsInvalid(double z)
        {
            // setup
            var hydroData = new TestHydroDataStub {ModelType = HydroDynamicModelType.Unstructured};

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.AreEqual(HydroDynamicModelType.Unstructured, model.ModelType,
                            "Precondition: Model is set up as Sigma (unstructured) model.");

            model.Loads.Add(new WaterQualityLoad
            {
                Name = "A",
                X = 5,
                Y = 5,
                Z = z
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            string expectedMessage = string.Format("Load 'A' has height of {0}, but is required to be in range [0, 1].", z);
            AssertExpectedValidationIssue(report, expectedMessage, i => i.Severity == ValidationSeverity.Error);
        }

        [Test]
        public void SigmaModelWithLoadWithlegalZIsValid()
        {
            // setup
            var hydroData = new TestHydroDataStub {ModelType = HydroDynamicModelType.Unstructured};

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.AreEqual(HydroDynamicModelType.Unstructured, model.ModelType,
                            "Precondition: Model is set up as Sigma (unstructured) model.");

            model.Loads.Add(new WaterQualityLoad
            {
                Name = "A",
                X = 5,
                Y = 5,
                Z = 1.0
            });
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "B",
                X = 5,
                Y = 5,
                Z = 0.34
            });
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "C",
                X = 5,
                Y = 5,
                Z = 0.0
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            Assert.AreEqual(0, report.Issues.Count());
        }

        [Test]
        [TestCase(12.34)]
        [TestCase(-2.5 + 1e-6)]
        [TestCase(-6.8 - 1e-6)]
        [TestCase(-34.56)]
        public void ZlayerModelWithLoadWithIllegalZIsInvalid(double z)
        {
            // setup
            const double topLevel = -2.5;
            const double bottomLevel = -6.8;
            var hydroData = new TestHydroDataStub
            {
                LayerType = LayerType.ZLayer,
                Ztop = topLevel,
                Zbot = bottomLevel
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.AreEqual(LayerType.ZLayer, model.LayerType,
                            "Precondition: Model is set up as Z-layer (unstructured) model.");

            model.Loads.Add(new WaterQualityLoad
            {
                Name = "A",
                X = 5,
                Y = 5,
                Z = z
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            string expectedMessage = string.Format("Load 'A' has height of {0}, but is required to be in range [{1}, {2}].",
                                                   z, bottomLevel, topLevel);
            AssertExpectedValidationIssue(report, expectedMessage, i => i.Severity == ValidationSeverity.Error);
        }

        [Test]
        public void ZLayerModelWithLoadWithlegalZIsValid()
        {
            // setup
            var hydroData = new TestHydroDataStub
            {
                LayerType = LayerType.ZLayer,
                Ztop = -2.5,
                Zbot = -6.8
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.AreEqual(LayerType.ZLayer, model.LayerType,
                            "Precondition: Model is set up as Z-layer (unstructured) model.");

            model.Loads.Add(new WaterQualityLoad
            {
                Name = "A",
                X = 5,
                Y = 5,
                Z = -2.5
            });
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "B",
                X = 5,
                Y = 5,
                Z = -4.69
            });
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "C",
                X = 5,
                Y = 5,
                Z = -6.8
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            Assert.AreEqual(0, report.Issues.Count());
        }

        [Test]
        public void ModelWithLoadInsideInactiveCellGivesWarning()
        {
            // setup
            var hydroData = new TestHydroDataStub
            {
                LayerType = LayerType.ZLayer,
                Ztop = 3.0,
                Zbot = -3.0,
                ThirdCellIsInactive = true
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.IsTrue(model.HasHydroDataImported);

            model.Loads.Add(new WaterQualityLoad
            {
                Name = "A",
                X = 5,
                Y = 5,
                Z = 0.5
            });
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "B",
                X = 5,
                Y = 15,
                Z = 0.5
            });
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "C",
                X = 15,
                Y = 5,
                Z = 0.5
            });
            model.Loads.Add(new WaterQualityLoad
            {
                Name = "D",
                X = 15,
                Y = 15,
                Z = 0.5
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            const string expectedMessageB = "Load 'B' is inside an inactive cell.";
            AssertExpectedValidationIssue(report, expectedMessageB,
                                          i => Equals(i.Subject, model.Loads[1]) && i.Severity == ValidationSeverity.Warning);
        }

        [Test]
        public void ModelWithLoadOutsideGridIsInvalid([Values(0.0 - double.Epsilon, 20.0 + double.Epsilon)]
                                                      double x,
                                                      [Values(0.0 - double.Epsilon, 20.0 + double.Epsilon)]
                                                      double y)
        {
            // setup
            var hydroData = new TestHydroDataStub
            {
                LayerType = LayerType.ZLayer,
                Ztop = 3.0,
                Zbot = -3.0
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.IsTrue(model.HasHydroDataImported);

            model.Loads.Add(new WaterQualityLoad
            {
                Name = "A",
                X = x,
                Y = y,
                Z = 0.5
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            AssertExpectedValidationIssue(report,
                                          "Load 'A' is not within grid or has ambiguous location (on a grid edge or grid vertex).",
                                          i => Equals(i.Subject, model.Loads[0]) && i.Severity == ValidationSeverity.Error);
        }

        [TestCase(19, 19)]
        [TestCase(1, 1)]
        public void ModelWithLoadInsideGridValid(double x, double y)
        {
            // setup
            var hydroData = new TestHydroDataStub
            {
                LayerType = LayerType.ZLayer,
                Ztop = 3.0,
                Zbot = -3.0
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.IsTrue(model.HasHydroDataImported);

            model.Loads.Add(new WaterQualityLoad
            {
                Name = "A",
                X = x,
                Y = y,
                Z = 0.5
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            Assert.That(report.Issues.Count(), Is.EqualTo(0));
        }

        [Test]
        [Combinatorial]
        public void ModelWithLoadAtAmbiguousLocationIsInvalid(
            [Values(0.0, 10.0, 20)] double x,
            [Values(0.0, 10.0, 20)] double y)
        {
            // setup
            var hydroData = new TestHydroDataStub
            {
                LayerType = LayerType.ZLayer,
                Ztop = 3.0,
                Zbot = -3.0
            };

            var model = new WaterQualityModel();
            model.ImportHydroData(hydroData);

            Assert.IsTrue(model.HasHydroDataImported);

            model.Loads.Add(new WaterQualityLoad
            {
                Name = "T",
                X = x,
                Y = y,
                Z = 0.5
            });

            // call
            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            // assert
            AssertExpectedValidationIssue(report,
                                          "Load 'T' is not within grid or has ambiguous location (on a grid edge or grid vertex).",
                                          i => Equals(i.Subject, model.Loads[0]) && i.Severity == ValidationSeverity.Error);
        }

        #endregion

        #region Output Timers

        private static string FormatTimeString(DateTime dateTime)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            string format = string.Format("yyyy{0}MM{0}dd HH{1}mm{1}ss", culture.DateTimeFormat.DateSeparator,
                                          culture.DateTimeFormat.TimeSeparator);

            return dateTime.ToString(format, culture);
        }

        private static object[] OutputTimersCase =
        {
            new object[]
            {
                0,
                0
            },
            new object[]
            {
                0,
                1
            },
            new object[]
            {
                1,
                0
            },
            new object[]
            {
                1,
                1
            },
            new object[]
            {
                0,
                -1
            },
            new object[]
            {
                -1,
                0
            },
            new object[]
            {
                -1,
                -1
            }
        };

        [Test]
        [TestCaseSource(nameof(OutputTimersCase))]
        public void Test_When_BalanceOutputTimers_DoNotMatch_ModelSimulationTimer_ValidationInfo_IsGiven(double startTime, double stopTime)
        {
            var model = new WaterQualityModel();
            var description = "balance output";

            DateTime referenceStartTime = model.StartTime;
            DateTime referenceStopTime = model.StopTime;
            string expectedMssg = string.Format(
                Resources.WaterQualityModelValidator_CheckTimers_Timers_for__0__are_not_equal_to_the_simulation_period_of_the_model___1____2____Please_verify_that_they_overlap_with_the_simulation_period_,
                description, FormatTimeString(referenceStartTime), FormatTimeString(referenceStopTime));

            //Set output timers:
            bool changeStartTime = startTime > 0 || startTime < 0;
            bool changeStopTime = stopTime > 0 || stopTime < 0;
            model.ModelSettings.BalanceStartTime = changeStartTime ? referenceStartTime.AddDays(startTime) : referenceStartTime;
            model.ModelSettings.BalanceStopTime = changeStopTime ? referenceStopTime.AddDays(stopTime) : referenceStopTime;

            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            Assert.GreaterOrEqual(1, report.InfoCount);
            bool timeChanged = changeStartTime || changeStopTime;
            Assert.AreEqual(timeChanged, report.GetAllIssuesRecursive().Any(i => i.Severity == ValidationSeverity.Info && i.Message == expectedMssg));
        }

        [Test]
        [TestCaseSource(nameof(OutputTimersCase))]
        public void Test_When_MonitoringLocations_OutputTimers_DoNotMatch_ModelSimulationTimer_ValidationInfo_IsGiven(double startTime, double stopTime)
        {
            var model = new WaterQualityModel();
            var description = "monitoring locations output";

            DateTime referenceStartTime = model.StartTime;
            DateTime referenceStopTime = model.StopTime;
            string expectedMssg = string.Format(
                Resources.WaterQualityModelValidator_CheckTimers_Timers_for__0__are_not_equal_to_the_simulation_period_of_the_model___1____2____Please_verify_that_they_overlap_with_the_simulation_period_,
                description, FormatTimeString(referenceStartTime), FormatTimeString(referenceStopTime));

            //Set output timers:
            bool changeStartTime = startTime > 0 || startTime < 0;
            bool changeStopTime = stopTime > 0 || stopTime < 0;
            model.ModelSettings.HisStartTime = changeStartTime ? referenceStartTime.AddDays(startTime) : referenceStartTime;
            model.ModelSettings.HisStopTime = changeStopTime ? referenceStopTime.AddDays(stopTime) : referenceStopTime;

            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            Assert.GreaterOrEqual(1, report.InfoCount);
            bool timeChanged = changeStartTime || changeStopTime;
            Assert.AreEqual(timeChanged, report.GetAllIssuesRecursive().Any(i => i.Severity == ValidationSeverity.Info && i.Message == expectedMssg));
        }

        [Test]
        [TestCaseSource(nameof(OutputTimersCase))]
        public void Test_When_Cells_OutputTimers_DoNotMatch_ModelSimulationTimer_ValidationInfo_IsGiven(double startTime, double stopTime)
        {
            var model = new WaterQualityModel();
            var description = "cells output";

            DateTime referenceStartTime = model.StartTime;
            DateTime referenceStopTime = model.StopTime;
            string expectedMssg = string.Format(
                Resources.WaterQualityModelValidator_CheckTimers_Timers_for__0__are_not_equal_to_the_simulation_period_of_the_model___1____2____Please_verify_that_they_overlap_with_the_simulation_period_,
                description, FormatTimeString(referenceStartTime), FormatTimeString(referenceStopTime));

            //Set output timers:
            bool changeStartTime = startTime > 0 || startTime < 0;
            bool changeStopTime = stopTime > 0 || stopTime < 0;
            model.ModelSettings.MapStartTime = changeStartTime ? referenceStartTime.AddDays(startTime) : referenceStartTime;
            model.ModelSettings.MapStopTime = changeStopTime ? referenceStopTime.AddDays(stopTime) : referenceStopTime;

            ValidationReport report = new WaterQualityModelValidator().Validate(model);

            Assert.GreaterOrEqual(1, report.InfoCount);
            bool timeChanged = changeStartTime || changeStopTime;
            Assert.AreEqual(timeChanged, report.GetAllIssuesRecursive().Any(i => i.Severity == ValidationSeverity.Info && i.Message == expectedMssg));
        }

        #endregion

        #region Process Coefficient rules

        [Test]
        [Category(TestCategory.Integration)]
        public void WaterQualityModelValidator_When_No_CoefficientsLoaded_NoIssues_Are_Generated()
        {
            var waqModel = new WaterQualityModel();
            Assert.IsNotNull(waqModel);

            var validator = new WaterQualityModelValidator();
            ValidationReport report = validator.Validate(waqModel);
            var categoryName = "Process coefficients";
            ValidationReport subReport = report.SubReports.FirstOrDefault(sr => sr.Category == categoryName);

            Assert.IsNotNull(subReport);
            Assert.IsFalse(subReport.Issues.Any());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void WaterQualityModelValidator_GeneratesValidationIssues_WhenNoRulesLoaded()
        {
            //Set up the model and the substances

            #region Set up model

            string modelFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\\Flow1D\\sobek.hyd");
            Assert.IsTrue(File.Exists(modelFilePath));

            var importer = new HydFileImporter();
            using (var waqModel = importer.ImportItem(modelFilePath) as WaterQualityModel)
            {
                Assert.IsNotNull(waqModel);

                string subsFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\\02b_Oxygen_bod_sediment.sub");
                Assert.IsTrue(File.Exists(subsFilePath));
                new SubFileImporter().Import(waqModel.SubstanceProcessLibrary, subsFilePath);
                Assert.IsNotNull(waqModel.SubstanceProcessLibrary);
                Assert.IsTrue(waqModel.SubstanceProcessLibrary.ActiveSubstances.Any());

                #endregion

                //Calling private method just to check this return.
                var validationResult = TypeUtils.CallPrivateStaticMethod(typeof(WaterQualityModelValidator),
                                                                         "ValidateProcessCoefficients",
                                                                         waqModel.SubstanceProcessLibrary,
                                                                         waqModel.ProcessCoefficients,
                                                                         new List<WaqProcessValidationRule>()
                                       ) as IEnumerable<ValidationIssue>;

                Assert.IsNotNull(validationResult);
                List<ValidationIssue> issues = validationResult.ToList();
                Assert.IsTrue(issues.Any());

                string expectedMssg = string.Format(Resources.WaterQualityModelValidator_ValidateProcessCoefficients_No_process_coefficient_rules_have_been_loaded__Therefore_they_cannot_be_validated_);
                Assert.IsTrue(issues.Any(iss => iss.Severity == ValidationSeverity.Warning && iss.Message.Contains(expectedMssg)));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void WaterQualityModelValidator_GeneratesValidationIssues_When_Library_DoesNot_Contain_Parameter()
        {
            //Set up the model and the substances

            #region Set up model

            string modelFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\\Flow1D\\sobek.hyd");
            Assert.IsTrue(File.Exists(modelFilePath));

            var importer = new HydFileImporter();
            using (var waqModel = importer.ImportItem(modelFilePath) as WaterQualityModel)
            {
                Assert.IsNotNull(waqModel);

                string subsFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\\02b_Oxygen_bod_sediment.sub");
                Assert.IsTrue(File.Exists(subsFilePath));
                new SubFileImporter().Import(waqModel.SubstanceProcessLibrary, subsFilePath);
                Assert.IsNotNull(waqModel.SubstanceProcessLibrary);
                Assert.IsTrue(waqModel.SubstanceProcessLibrary.ActiveSubstances.Any());

                #endregion

                //We know that the parameter swoxydem has a rule and is contained in the library
                //for the test data we are using. So let's remove it for this test.
                var swoxydem = "swoxydem";
                IFunction swoxyDemParam = waqModel.ProcessCoefficients.FirstOrDefault(pc => pc.Name.ToLower().Equals(swoxydem));
                Assert.IsNotNull(swoxyDemParam);

                //Calling private method just to check this return.
                var validationResult = TypeUtils.CallPrivateStaticMethod(typeof(WaterQualityModelValidator),
                                                                         "ValidateProcessCoefficients",
                                                                         new SubstanceProcessLibrary(),
                                                                         waqModel.ProcessCoefficients,
                                                                         waqModel.WaqProcessesRules
                                       ) as IEnumerable<ValidationIssue>;

                Assert.IsNotNull(validationResult);
                List<ValidationIssue> issues = validationResult.ToList();
                Assert.IsTrue(issues.Any());

                string expectedMssg = string.Format(Resources.WaterQualityModelValidator_ValidateProcessCoefficients_The_Substance_library_does_not_contain_the_given_parameter__0__, swoxyDemParam.Name);
                Assert.IsTrue(issues.Any(iss => iss.Severity == ValidationSeverity.Warning && iss.Message.Contains(expectedMssg)));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void WaterQualityModelValidator_GeneratesValidationIssues_BasedOnRules()
        {
            //Set up the model and the substances

            #region Set up model

            string modelFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\\Flow1D\\sobek.hyd");
            Assert.IsTrue(File.Exists(modelFilePath));

            var importer = new HydFileImporter();
            using (var waqModel = importer.ImportItem(modelFilePath) as WaterQualityModel)
            {
                Assert.IsNotNull(waqModel);

                string subsFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\\02b_Oxygen_bod_sediment.sub");
                Assert.IsTrue(File.Exists(subsFilePath));
                new SubFileImporter().Import(waqModel.SubstanceProcessLibrary, subsFilePath);
                Assert.IsNotNull(waqModel.SubstanceProcessLibrary);
                Assert.IsTrue(waqModel.SubstanceProcessLibrary.ActiveSubstances.Any());

                #endregion

                //Get one of the parameters
                var swoxydem = "swoxydem";
                IFunction swoxyDemParam = waqModel.ProcessCoefficients.FirstOrDefault(pc => pc.Name.ToLower().Equals(swoxydem));
                Assert.IsNotNull(swoxyDemParam);
                var invalidValue = 3;

                var validator = new WaterQualityModelValidator();
                ValidationReport report = validator.Validate(waqModel);
                var categoryName = "Process coefficients";
                ValidationReport subReport = report.SubReports.FirstOrDefault(sr => sr.Category == categoryName);

                Assert.IsNotNull(subReport);
                Assert.IsFalse(subReport.Issues.Any());

                string message = Resources.WaqValidationRulesExtension_GetWaqProcessValidationRuleAsString_Process_coefficient__0___value__1____2__3__;
                message = message.Replace(".", string.Empty); //small trick.
                string expectedMssg = string.Format(message, swoxyDemParam.Name, invalidValue, string.Empty, string.Empty);
                Assert.IsFalse(subReport.Issues.Any(iss => iss.Severity == ValidationSeverity.Warning && iss.Message.Contains(expectedMssg)));

                //Modify the parameter for which we know the validation will fail, let´s use:
                //SWOXYDEM,0,2,int,
                if (swoxyDemParam.Components != null && swoxyDemParam.Components.Any())
                {
                    swoxyDemParam.Components[0].DefaultValue = invalidValue;
                }

                report = validator.Validate(waqModel);
                subReport = report.SubReports.FirstOrDefault(sr => sr.Category == categoryName);

                Assert.IsNotNull(subReport);
                Assert.IsTrue(subReport.Issues.Any());
                //The message is now given.
                Assert.IsTrue(subReport.Issues.Any(iss => iss.Severity == ValidationSeverity.Warning && iss.Message.Contains(expectedMssg)));
            }
        }

        #endregion
    }
}
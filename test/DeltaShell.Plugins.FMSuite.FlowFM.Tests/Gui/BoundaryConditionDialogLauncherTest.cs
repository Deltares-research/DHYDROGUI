using System;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Services;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class BoundaryConditionDialogLauncherTest
    {
        private string timExtension;
        private string morphologyExtension;
        private string boundaryConditionExtension;
        private string cmpExtension;
        private string qhExtension;

        [SetUp]
        public void Initialize()
        {
            var timImporter = new TimFileImporter();
            timExtension = timImporter.FileFilter;
            var morphologyFileImporter = new BcmFileImporter();
            morphologyExtension = morphologyFileImporter.FileFilter;
            var boundaryConditionImporter = new BcFileImporter();
            boundaryConditionExtension = boundaryConditionImporter.FileFilter;
            var cmpFileImporter = new CmpFileImporter();
            cmpExtension = cmpFileImporter.FileFilter;
            var qhFileImporter = new QhFileImporter();
            qhExtension = qhFileImporter.FileFilter;
        }

        [Test]
        public void GivenFlowBoundaryConditionWhenImportedWithoutOpenFileDialogThenThrowException()
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed, BoundaryConditionDataType.TimeSeries);
            var wfmodel = new WaterFlowFMModel();

            Assert.That(() => BoundaryConditionDialogLauncher.LaunchImporterDialog(null, flowBoundaryCondition, 1, wfmodel.ReferenceTime),
                        Throws.ArgumentNullException);
        }

        [Test]
        public void GivenNoBoundaryConditionWhenImportedThenThrowException()
        {
            TestHelper.AssertAtLeastOneLogMessagesContains(() => BoundaryConditionDialogLauncher.LaunchImporterDialog(new FileDialogService(), null, 1, new DateTime()), "Boundary condition is not set");
        }

        [Test]
        public void GivenEmptyDateTimeWhenImportThenThrowException()
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed, BoundaryConditionDataType.TimeSeries);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => BoundaryConditionDialogLauncher.LaunchImporterDialog(new FileDialogService(), flowBoundaryCondition, 1, null), "Datetime is not set");
        }

        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed)]
        public void GivenAMorphologyFlowBoundaryQuantityTypeWithTimeSeriesWhenImportDialogIsOpenedThenValidExtentionsArePresent(FlowBoundaryQuantityType flowBoundaryQuantityType)
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, BoundaryConditionDataType.TimeSeries);
            var wfmodel = new WaterFlowFMModel();

            var fileDialogService = Substitute.For<IFileDialogService>();

            BoundaryConditionDialogLauncher.LaunchImporterDialog(fileDialogService, flowBoundaryCondition, 1, wfmodel.ReferenceTime);

            fileDialogService.Received().ShowOpenFileDialog(Arg.Is<FileDialogOptions>(options => options.FileFilter == $@"{timExtension}|{morphologyExtension}"));
        }

        [TestCaseSource(typeof(FlowBoundaryTestData), "TimeSeries")]
        public void GivenAFlowBoundaryQuantityTypeWithTimeSeriesWhenImportDialogIsOpenedThenValidExtentionsArePresent(FlowBoundaryQuantityType flowBoundaryQuantityType, BoundaryConditionDataType timeSeries)
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, timeSeries);
            var wfmodel = new WaterFlowFMModel();

            var fileDialogService = Substitute.For<IFileDialogService>();

            BoundaryConditionDialogLauncher.LaunchImporterDialog(fileDialogService, flowBoundaryCondition, 1, wfmodel.ReferenceTime);

            fileDialogService.Received().ShowOpenFileDialog(Arg.Is<FileDialogOptions>(options => options.FileFilter == $@"{boundaryConditionExtension}|{timExtension}"));
        }

        [TestCaseSource(typeof(FlowBoundaryTestData), "Harmonics")]
        [TestCaseSource(typeof(FlowBoundaryTestData), "HarmonicsCorrection")]
        [TestCaseSource(typeof(FlowBoundaryTestData), "Astronomical")]
        [TestCaseSource(typeof(FlowBoundaryTestData), "AstronomicalCorrection")]
        public void GivenAFlowBoundaryQuantityTypeWithHarmonicsCorrectionWhenImportDialogIsOpenedThenValidExtentionsArePresent(FlowBoundaryQuantityType flowBoundaryQuantityType, BoundaryConditionDataType harmonicsCorrection)
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, harmonicsCorrection);
            var wfmodel = new WaterFlowFMModel();

            var fileDialogService = Substitute.For<IFileDialogService>();

            BoundaryConditionDialogLauncher.LaunchImporterDialog(fileDialogService, flowBoundaryCondition, 1, wfmodel.ReferenceTime);

            fileDialogService.Received().ShowOpenFileDialog(Arg.Is<FileDialogOptions>(options => options.FileFilter == $@"{boundaryConditionExtension}|{cmpExtension}"));
        }

        [TestCaseSource(typeof(FlowBoundaryTestData), "Qh")]
        public void GivenAFlowBoundaryQuantityTypeWithQhWhenImportDialogIsOpenedThenValidExtentionsArePresent(FlowBoundaryQuantityType flowBoundaryQuantityType, BoundaryConditionDataType Qh)
        {
            var flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, Qh);
            var wfmodel = new WaterFlowFMModel();

            var fileDialogService = Substitute.For<IFileDialogService>();

            BoundaryConditionDialogLauncher.LaunchImporterDialog(fileDialogService, flowBoundaryCondition, 1, wfmodel.ReferenceTime);

            fileDialogService.Received().ShowOpenFileDialog(Arg.Is<FileDialogOptions>(options => options.FileFilter == $@"{boundaryConditionExtension}|{qhExtension}"));
        }
    }

    internal class FlowBoundaryTestData
    {
        private static readonly object[] TimeSeries =
        {
            new object[]
            {
                FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.TimeSeries
            },
            new object[]
            {
                FlowBoundaryQuantityType.Riemann,
                BoundaryConditionDataType.TimeSeries
            },
            new object[]
            {
                FlowBoundaryQuantityType.RiemannVelocity,
                BoundaryConditionDataType.TimeSeries
            },
            new object[]
            {
                FlowBoundaryQuantityType.Velocity,
                BoundaryConditionDataType.TimeSeries
            },
            new object[]
            {
                FlowBoundaryQuantityType.Neumann,
                BoundaryConditionDataType.TimeSeries
            },
            new object[]
            {
                FlowBoundaryQuantityType.Discharge,
                BoundaryConditionDataType.TimeSeries
            }
        };

        private static object[] Harmonics =
        {
            new object[]
            {
                FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.Harmonics
            },
            new object[]
            {
                FlowBoundaryQuantityType.Riemann,
                BoundaryConditionDataType.Harmonics
            },
            new object[]
            {
                FlowBoundaryQuantityType.RiemannVelocity,
                BoundaryConditionDataType.Harmonics
            },
            new object[]
            {
                FlowBoundaryQuantityType.Velocity,
                BoundaryConditionDataType.Harmonics
            },
            new object[]
            {
                FlowBoundaryQuantityType.Neumann,
                BoundaryConditionDataType.Harmonics
            },
            new object[]
            {
                FlowBoundaryQuantityType.Discharge,
                BoundaryConditionDataType.Harmonics
            }
        };

        private static object[] HarmonicsCorrection =
        {
            new object[]
            {
                FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.HarmonicCorrection
            },
            new object[]
            {
                FlowBoundaryQuantityType.Riemann,
                BoundaryConditionDataType.HarmonicCorrection
            },
            new object[]
            {
                FlowBoundaryQuantityType.RiemannVelocity,
                BoundaryConditionDataType.HarmonicCorrection
            },
            new object[]
            {
                FlowBoundaryQuantityType.Velocity,
                BoundaryConditionDataType.HarmonicCorrection
            },
            new object[]
            {
                FlowBoundaryQuantityType.Neumann,
                BoundaryConditionDataType.HarmonicCorrection
            },
            new object[]
            {
                FlowBoundaryQuantityType.Discharge,
                BoundaryConditionDataType.HarmonicCorrection
            }
        };

        private static object[] Astronomical =
        {
            new object[]
            {
                FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.AstroComponents
            },
            new object[]
            {
                FlowBoundaryQuantityType.Riemann,
                BoundaryConditionDataType.AstroComponents
            },
            new object[]
            {
                FlowBoundaryQuantityType.RiemannVelocity,
                BoundaryConditionDataType.AstroComponents
            },
            new object[]
            {
                FlowBoundaryQuantityType.Velocity,
                BoundaryConditionDataType.AstroComponents
            },
            new object[]
            {
                FlowBoundaryQuantityType.Neumann,
                BoundaryConditionDataType.AstroComponents
            },
            new object[]
            {
                FlowBoundaryQuantityType.Discharge,
                BoundaryConditionDataType.AstroComponents
            }
        };

        private static object[] AstronomicalCorrection =
        {
            new object[]
            {
                FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.AstroCorrection
            },
            new object[]
            {
                FlowBoundaryQuantityType.Riemann,
                BoundaryConditionDataType.AstroCorrection
            },
            new object[]
            {
                FlowBoundaryQuantityType.RiemannVelocity,
                BoundaryConditionDataType.AstroCorrection
            },
            new object[]
            {
                FlowBoundaryQuantityType.Velocity,
                BoundaryConditionDataType.AstroCorrection
            },
            new object[]
            {
                FlowBoundaryQuantityType.Neumann,
                BoundaryConditionDataType.AstroCorrection
            },
            new object[]
            {
                FlowBoundaryQuantityType.Discharge,
                BoundaryConditionDataType.AstroCorrection
            }
        };

        private static object[] Qh =
        {
            new object[]
            {
                FlowBoundaryQuantityType.WaterLevel,
                BoundaryConditionDataType.Qh
            },
            new object[]
            {
                FlowBoundaryQuantityType.Riemann,
                BoundaryConditionDataType.Qh
            },
            new object[]
            {
                FlowBoundaryQuantityType.RiemannVelocity,
                BoundaryConditionDataType.Qh
            },
            new object[]
            {
                FlowBoundaryQuantityType.Velocity,
                BoundaryConditionDataType.Qh
            },
            new object[]
            {
                FlowBoundaryQuantityType.Neumann,
                BoundaryConditionDataType.Qh
            },
            new object[]
            {
                FlowBoundaryQuantityType.Discharge,
                BoundaryConditionDataType.Qh
            }
        };
    }
}
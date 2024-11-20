using System;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class BoundaryConditionDialogLauncherTest
    {
        private MockRepository mocks;
        private OpenFileDialog fileDialogMock;
        private string timExtension;
        private string morphologyExtension;
        private string boundaryConditionExtension;
        private string cmpExtension;
        private string qhExtension;

        [SetUp]
        public void Initialize()
        {
            mocks = new MockRepository();
            fileDialogMock = mocks.DynamicMock<OpenFileDialog>();
            fileDialogMock.Filter = string.Empty;
            fileDialogMock.Expect(f => f.ShowDialog()).Return(DialogResult.OK);
            fileDialogMock.Replay();

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

        [TestCase]
        public void GivenFlowBoundaryConditionWhenImportedWithoutOpenFileDialogThenThrowException()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed, BoundaryConditionDataType.TimeSeries);
            WaterFlowFMModel wfmodel = new WaterFlowFMModel();
            Assert.Throws<ArgumentException>(() =>
            {
                BoundaryConditionDialogLauncher.LaunchImporterDialog(null, flowBoundaryCondition, 1, wfmodel.ReferenceTime);
            });
        }

        [TestCase()]
        public void GivenNoBoundaryConditionWhenImportedThenThrowException()
        {
            TestHelper.AssertAtLeastOneLogMessagesContains(()=> BoundaryConditionDialogLauncher.LaunchImporterDialog(new OpenFileDialog(), null, 1, new DateTime()), "Boundary condition is not set");
        }

        [TestCase()]
        public void GivenEmptyDateTimeWhenImportThenThrowException()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed, BoundaryConditionDataType.TimeSeries);
            
            TestHelper.AssertAtLeastOneLogMessagesContains(()=> BoundaryConditionDialogLauncher.LaunchImporterDialog(new OpenFileDialog(), flowBoundaryCondition, 1, null), "Datetime is not set");
        }

        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed)]
        public void GivenAMorphologyFlowBoundaryQuantityTypeWithTimeSeriesWhenImportDialogIsOpenedThenValidExtentionsArePresent(FlowBoundaryQuantityType flowBoundaryQuantityType)
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, BoundaryConditionDataType.TimeSeries);
            WaterFlowFMModel wfmodel = new WaterFlowFMModel();

            BoundaryConditionDialogLauncher.LaunchImporterDialog(fileDialogMock, flowBoundaryCondition, 1, wfmodel.ReferenceTime);

            fileDialogMock.AssertWasCalled(f => f.Filter = $@"{timExtension}|{morphologyExtension}");

        }

        [TestCaseSource(typeof(FlowBoundaryTestData), "TimeSeries")]  
        public void GivenAFlowBoundaryQuantityTypeWithTimeSeriesWhenImportDialogIsOpenedThenValidExtentionsArePresent(FlowBoundaryQuantityType flowBoundaryQuantityType, BoundaryConditionDataType timeSeries)
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, timeSeries);
            WaterFlowFMModel wfmodel = new WaterFlowFMModel();

            BoundaryConditionDialogLauncher.LaunchImporterDialog(fileDialogMock, flowBoundaryCondition, 1, wfmodel.ReferenceTime);
            
            fileDialogMock.AssertWasCalled(f => f.Filter = $@"{boundaryConditionExtension}|{timExtension}");
        }

        [TestCaseSource(typeof(FlowBoundaryTestData), "Harmonics")]  
        [TestCaseSource(typeof(FlowBoundaryTestData), "HarmonicsCorrection")]  
        [TestCaseSource(typeof(FlowBoundaryTestData), "Astronomical")]  
        [TestCaseSource(typeof(FlowBoundaryTestData), "AstronomicalCorrection")]  
        public void GivenAFlowBoundaryQuantityTypeWithHarmonicsCorrectionWhenImportDialogIsOpenedThenValidExtentionsArePresent(FlowBoundaryQuantityType flowBoundaryQuantityType, BoundaryConditionDataType harmonicsCorrection)
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, harmonicsCorrection);
            WaterFlowFMModel wfmodel = new WaterFlowFMModel();

            BoundaryConditionDialogLauncher.LaunchImporterDialog(fileDialogMock, flowBoundaryCondition, 1, wfmodel.ReferenceTime);
            
            fileDialogMock.AssertWasCalled(f => f.Filter = $@"{boundaryConditionExtension}|{cmpExtension}");
        }
        
        [TestCaseSource(typeof(FlowBoundaryTestData), "Qh")]
        public void GivenAFlowBoundaryQuantityTypeWithQhWhenImportDialogIsOpenedThenValidExtentionsArePresent(FlowBoundaryQuantityType flowBoundaryQuantityType, BoundaryConditionDataType Qh)
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, Qh);
            WaterFlowFMModel wfmodel = new WaterFlowFMModel();

            BoundaryConditionDialogLauncher.LaunchImporterDialog(fileDialogMock, flowBoundaryCondition, 1, wfmodel.ReferenceTime);

            fileDialogMock.AssertWasCalled(f => f.Filter = $@"{boundaryConditionExtension}|{qhExtension}");
                                                                                        
        }
    }

    class FlowBoundaryTestData
    {
        static readonly object[] TimeSeries =
        {
            new object[] {FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries},
            new object[] {FlowBoundaryQuantityType.Riemann, BoundaryConditionDataType.TimeSeries},
            new object[] {FlowBoundaryQuantityType.RiemannVelocity, BoundaryConditionDataType.TimeSeries},
            new object[] {FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.TimeSeries},
            new object[] {FlowBoundaryQuantityType.Neumann, BoundaryConditionDataType.TimeSeries},
            new object[] {FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.TimeSeries},
        
        };

        static object[] Harmonics =
        {
            new object[] {FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Harmonics},
            new object[] {FlowBoundaryQuantityType.Riemann, BoundaryConditionDataType.Harmonics},
            new object[] {FlowBoundaryQuantityType.RiemannVelocity, BoundaryConditionDataType.Harmonics},
            new object[] {FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.Harmonics},
            new object[] {FlowBoundaryQuantityType.Neumann, BoundaryConditionDataType.Harmonics},
            new object[] {FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.Harmonics},
        
        };

        static object[] HarmonicsCorrection =
        {
            new object[] {FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.HarmonicCorrection},
            new object[] {FlowBoundaryQuantityType.Riemann, BoundaryConditionDataType.HarmonicCorrection},
            new object[] {FlowBoundaryQuantityType.RiemannVelocity, BoundaryConditionDataType.HarmonicCorrection},
            new object[] {FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.HarmonicCorrection},
            new object[] {FlowBoundaryQuantityType.Neumann, BoundaryConditionDataType.HarmonicCorrection},
            new object[] {FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.HarmonicCorrection},
        };
        static object[] Astronomical =
        {
            new object[] {FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroComponents},
            new object[] {FlowBoundaryQuantityType.Riemann, BoundaryConditionDataType.AstroComponents},
            new object[] {FlowBoundaryQuantityType.RiemannVelocity, BoundaryConditionDataType.AstroComponents},
            new object[] {FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.AstroComponents},
            new object[] {FlowBoundaryQuantityType.Neumann, BoundaryConditionDataType.AstroComponents},
            new object[] {FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.AstroComponents},
        };
        static object[] AstronomicalCorrection =
        {
            new object[] {FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroCorrection},
            new object[] {FlowBoundaryQuantityType.Riemann, BoundaryConditionDataType.AstroCorrection},
            new object[] {FlowBoundaryQuantityType.RiemannVelocity, BoundaryConditionDataType.AstroCorrection},
            new object[] {FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.AstroCorrection},
            new object[] {FlowBoundaryQuantityType.Neumann, BoundaryConditionDataType.AstroCorrection},
            new object[] {FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.AstroCorrection},
        };
        static object[] Qh =
        {
            new object[] {FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Qh},
            new object[] {FlowBoundaryQuantityType.Riemann, BoundaryConditionDataType.Qh},
            new object[] {FlowBoundaryQuantityType.RiemannVelocity, BoundaryConditionDataType.Qh},
            new object[] {FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.Qh},
            new object[] {FlowBoundaryQuantityType.Neumann, BoundaryConditionDataType.Qh},
            new object[] {FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.Qh},
        };
    }
}

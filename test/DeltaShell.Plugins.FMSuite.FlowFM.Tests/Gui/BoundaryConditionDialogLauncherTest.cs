using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
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

        [SetUp]
        public void Initialize()
        {
            mocks = new MockRepository();
            fileDialogMock = mocks.DynamicMock<OpenFileDialog>();
            fileDialogMock.Filter = string.Empty;
            fileDialogMock.Expect(f => f.ShowDialog()).Return(DialogResult.OK);
            fileDialogMock.Replay();
        }

        [TestCase()]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenFlowBoundaryConditionWhenImportedWithoutOpenFileDialogThenThrowException()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed, BoundaryConditionDataType.TimeSeries);
            WaterFlowFMModel wfmodel = new WaterFlowFMModel();

            BoundaryConditionDialogLauncher.LaunchImporterDialog(null, flowBoundaryCondition, 1, wfmodel.ReferenceTime);
        }

        [TestCase()]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenNoBoundaryConditionWhenImportedThenThrowException()
        {
            BoundaryConditionDialogLauncher.LaunchImporterDialog(new OpenFileDialog(), null, 1, new DateTime());
        }

        [TestCase()]
        [ExpectedException(typeof(ArgumentException))]
        public void GivenEmptyDateTimeWhenImportThenThrowException()
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed, BoundaryConditionDataType.TimeSeries);

            BoundaryConditionDialogLauncher.LaunchImporterDialog(new OpenFileDialog(), flowBoundaryCondition, 1, null);
        }

        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed)]
        public void GivenAMorphologyFlowBoundaryQuantityTypeWithTimeSeriesWhenImportDialogIsOpenedThenValidExtentionsArePresent(FlowBoundaryQuantityType flowBoundaryQuantityType)
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, BoundaryConditionDataType.TimeSeries);
            WaterFlowFMModel wfmodel = new WaterFlowFMModel();

            BoundaryConditionDialogLauncher.LaunchImporterDialog(fileDialogMock, flowBoundaryCondition, 1, wfmodel.ReferenceTime);
            
            fileDialogMock.AssertWasCalled(f => f.Filter = @"Time series file (*.tim)|*.tim|Morphology boundary conditions file|*.bcm");

        }

        [TestCaseSource(typeof(FlowBoundaryTestData), "TimeSeries")]  
        public void GivenAFlowBoundaryQuantityTypeWithTimeSeriesWhenImportDialogIsOpenedThenValidExtentionsArePresent(FlowBoundaryQuantityType flowBoundaryQuantityType, BoundaryConditionDataType timeSeries)
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, timeSeries);
            WaterFlowFMModel wfmodel = new WaterFlowFMModel();

            BoundaryConditionDialogLauncher.LaunchImporterDialog(fileDialogMock, flowBoundaryCondition, 1, wfmodel.ReferenceTime);
            
            fileDialogMock.AssertWasCalled(f => f.Filter = @"Boundary conditions file|*.bc|Time series file (*.tim)|*.tim");
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
            
            fileDialogMock.AssertWasCalled(f => f.Filter = @"Boundary conditions file|*.bc|Harmonic series file|*.cmp");
        }
        
        [TestCaseSource(typeof(FlowBoundaryTestData), "Qh")]
        public void GivenAFlowBoundaryQuantityTypeWithQhWhenImportDialogIsOpenedThenValidExtentionsArePresent(FlowBoundaryQuantityType flowBoundaryQuantityType, BoundaryConditionDataType Qh)
        {
            FlowBoundaryCondition flowBoundaryCondition = new FlowBoundaryCondition(flowBoundaryQuantityType, Qh);
            WaterFlowFMModel wfmodel = new WaterFlowFMModel();

            BoundaryConditionDialogLauncher.LaunchImporterDialog(fileDialogMock, flowBoundaryCondition, 1, wfmodel.ReferenceTime);

            fileDialogMock.AssertWasCalled(f => f.Filter = @"Boundary conditions file|*.bc|Q-h series file|*.qh");
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
            new object[] {FlowBoundaryQuantityType.Riemann, BoundaryConditionDataType.TimeSeries},
            new object[] {FlowBoundaryQuantityType.RiemannVelocity, BoundaryConditionDataType.TimeSeries},
            new object[] {FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.Harmonics},
            new object[] {FlowBoundaryQuantityType.Neumann, BoundaryConditionDataType.Harmonics},
            new object[] {FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.Harmonics},
        
        };

        static object[] HarmonicsCorrection =
        {
            new object[] {FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.HarmonicCorrection},
            new object[] {FlowBoundaryQuantityType.Riemann, BoundaryConditionDataType.TimeSeries},
            new object[] {FlowBoundaryQuantityType.RiemannVelocity, BoundaryConditionDataType.TimeSeries},
            new object[] {FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.HarmonicCorrection},
            new object[] {FlowBoundaryQuantityType.Neumann, BoundaryConditionDataType.HarmonicCorrection},
            new object[] {FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.HarmonicCorrection},
        };
        static object[] Astronomical =
        {
            new object[] {FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroComponents},
            new object[] {FlowBoundaryQuantityType.Riemann, BoundaryConditionDataType.TimeSeries},
            new object[] {FlowBoundaryQuantityType.RiemannVelocity, BoundaryConditionDataType.TimeSeries},
            new object[] {FlowBoundaryQuantityType.Velocity, BoundaryConditionDataType.AstroComponents},
            new object[] {FlowBoundaryQuantityType.Neumann, BoundaryConditionDataType.AstroComponents},
            new object[] {FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.AstroComponents},
        };
        static object[] AstronomicalCorrection =
        {
            new object[] {FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroCorrection},
            new object[] {FlowBoundaryQuantityType.Riemann, BoundaryConditionDataType.TimeSeries},
            new object[] {FlowBoundaryQuantityType.RiemannVelocity, BoundaryConditionDataType.TimeSeries},
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

using System.Linq;
using DeltaShell.Dimr.Gui.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Dimr.Tests.ViewModels
{
    [TestFixture]
    public class DIMRConfigRibbonViewModelTests
    {
        
        [Test]
        public void DIMRConfigRibbonViewModelSetDebuggerLevelsTest()
        {
            // Check default values
            Assert.AreEqual(DimrApiDataSet.FeedbackLevel, DimrApiDataSet.DimrLoggingLevel.WARN);
            Assert.AreEqual(DimrApiDataSet.LogFileLevel, DimrApiDataSet.DimrLoggingLevel.LOG_DETAIL);

            var viewModel = new DIMRConfigRibbonViewModel();
            Assert.That(viewModel.Levels.Count(), Is.GreaterThan(0));

            // Set values on view model
            viewModel.CurrentFeedbackLevel = DimrApiDataSet.DimrLoggingLevel.MINOR;
            viewModel.CurrentLogfileLevel = DimrApiDataSet.DimrLoggingLevel.MAJOR;

            // Check values have been updated
            Assert.AreEqual(DimrApiDataSet.FeedbackLevel, DimrApiDataSet.DimrLoggingLevel.MINOR);
            Assert.AreEqual(DimrApiDataSet.LogFileLevel, DimrApiDataSet.DimrLoggingLevel.MAJOR);
        }
    }
}
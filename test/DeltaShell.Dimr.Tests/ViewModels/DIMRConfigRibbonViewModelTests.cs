using System.Linq;
using BasicModelInterface;
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
            Assert.AreEqual(DimrApiDataSet.FeedbackLevel, Level.Fatal);
            Assert.AreEqual(DimrApiDataSet.LogFileLevel, Level.Info);

            var viewModel = new DIMRConfigRibbonViewModel();
            Assert.That(viewModel.Levels.Count(), Is.GreaterThan(0));

            // Set values on view model
            viewModel.CurrentFeedbackLevel = Level.Debug;
            viewModel.CurrentLogfileLevel = Level.Fatal;

            // Check values have been updated
            Assert.AreEqual(DimrApiDataSet.FeedbackLevel, Level.Debug);
            Assert.AreEqual(DimrApiDataSet.LogFileLevel, Level.Fatal);
        }
    }
}
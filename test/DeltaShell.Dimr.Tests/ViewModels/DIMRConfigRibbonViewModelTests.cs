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
            Assert.AreEqual(DimrApiDataSet.FeedbackLevel, Level.Error);
            Assert.AreEqual(DimrApiDataSet.LogFileLevel, Level.Debug);

            var viewModel = new DIMRConfigRibbonViewModel();
            Assert.That(viewModel.Levels.Count(), Is.GreaterThan(0));

            // Set values on view model
            viewModel.CurrentFeedbackLevel = Level.Info;
            viewModel.CurrentLogfileLevel = Level.Fatal;

            // Check values have been updated
            Assert.AreEqual(DimrApiDataSet.FeedbackLevel, Level.Info);
            Assert.AreEqual(DimrApiDataSet.LogFileLevel, Level.Fatal);
        }
    }
}
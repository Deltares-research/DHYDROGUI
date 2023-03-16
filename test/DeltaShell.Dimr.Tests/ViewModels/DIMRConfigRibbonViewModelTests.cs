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
            Assert.AreEqual(Level.All, DimrApiDataSet.FeedbackLevel);
            Assert.AreEqual(Level.None, DimrApiDataSet.LogFileLevel);

            var viewModel = new DimrConfigRibbonViewModel();
            Assert.That(viewModel.Levels.Count(), Is.GreaterThan(0));

            // Set values on view model
            viewModel.CurrentFeedbackLevel = Level.Info;
            viewModel.CurrentLogfileLevel = Level.Fatal;

            // Check values have been updated
            Assert.AreEqual(Level.Info, DimrApiDataSet.FeedbackLevel);
            Assert.AreEqual(Level.Fatal, DimrApiDataSet.LogFileLevel);
        }
    }
}
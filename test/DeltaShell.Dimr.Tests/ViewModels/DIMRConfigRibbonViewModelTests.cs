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
            Assert.AreEqual(Level.All, DimrLogging.FeedbackLevel);
            Assert.AreEqual(Level.None, DimrLogging.LogFileLevel);

            var viewModel = new DIMRConfigRibbonViewModel();
            Assert.That(viewModel.Levels.Count(), Is.GreaterThan(0));

            // Set values on view model
            viewModel.CurrentFeedbackLevel = Level.Info;
            viewModel.CurrentLogfileLevel = Level.Fatal;

            // Check values have been updated
            Assert.AreEqual(Level.Info, DimrLogging.FeedbackLevel);
            Assert.AreEqual(Level.Fatal, DimrLogging.LogFileLevel);
        }
    }
}
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class HarmonicConditionsDialogViewModelTest
    {
        [Test]
        public void ConstructorTest()
        {
            var vm = new HarmonicConditionsDialogViewModel();
            Assert.AreEqual(1, vm.AmplitudeCorrection);
            Assert.AreEqual(false, vm.CorrectionsEnabled);
            Assert.AreEqual(0, vm.Frequency);
            Assert.AreEqual(0, vm.Amplitude);
            Assert.AreEqual(0, vm.Phase);
            Assert.AreEqual(0, vm.PhaseCorrection);
        }
    }
}
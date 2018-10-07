using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture]
    public class LeveeBreachViewModelTest
    {
        [Test]
        public void ViewModelWithLeveeBreach_GettingSelectedGrowthFormula_ShouldGiveExpectedResults()
        {
            var vm = new LeveeBreachViewModel
            {
                LeveeBreach = new LeveeBreach
                {
                    LeveeBreachFormula = LeveeBreachGrowthFormula.VerheijvdKnaap2002
                }
            };

            Assert.AreEqual(LeveeBreachGrowthFormula.VerheijvdKnaap2002, vm.SelectedGrowthFormula);

            vm.LeveeBreach.LeveeBreachFormula = LeveeBreachGrowthFormula.UserDefinedBreach;
            Assert.AreEqual(LeveeBreachGrowthFormula.UserDefinedBreach, vm.SelectedGrowthFormula);

            vm.LeveeBreach = null;
            Assert.AreEqual(LeveeBreachGrowthFormula.VerheijvdKnaap2002, vm.SelectedGrowthFormula);
        }

        [Test]
        public void ViewModelWithLeveeBreach_SettingSelectedGrowthFormula_ShouldSetFormulaInLeveeBreach()
        {
            var vm = new LeveeBreachViewModel
            {
                LeveeBreach = new LeveeBreach(),
                SelectedGrowthFormula = LeveeBreachGrowthFormula.UserDefinedBreach,
            };

            Assert.AreEqual(LeveeBreachGrowthFormula.UserDefinedBreach, vm.LeveeBreach.LeveeBreachFormula);
        }

        [Test]
        public void ViewModelWithoutLeveeBreach_SettingSelectedGrowthFormula_ShouldNotCauseCrash()
        {
            var vm = new LeveeBreachViewModel();
            vm.LeveeBreach = null;
            vm.SelectedGrowthFormula = LeveeBreachGrowthFormula.UserDefinedBreach;
        }

        [Test]
        public void ViewModelWithLeveeBreach_GettingBreahSettings_ShouldReturnExpected()
        {
            var vm = new LeveeBreachViewModel { LeveeBreach = new LeveeBreach {LeveeBreachFormula = LeveeBreachGrowthFormula.VerheijvdKnaap2002} };

            Assert.That(vm.LeveeBreach.LeveeBreachFormula == LeveeBreachGrowthFormula.VerheijvdKnaap2002);
            Assert.NotNull(vm.LeveeBreachSettings);
            Assert.That(vm.LeveeBreachSettings.GetType() == typeof(VerheijVdKnaap2002BreachSettings));
        }
    }
}
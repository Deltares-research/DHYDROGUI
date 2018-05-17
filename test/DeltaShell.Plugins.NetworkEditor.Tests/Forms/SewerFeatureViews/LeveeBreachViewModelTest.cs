using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture]
    public class LeveeBreachViewModelTest
    {
        [Test]
        public void LeveeBreachViewModel_CallingConstructor_ShouldSetCommands()
        {
            var vm = new LeveeBreachViewModel();
            Assert.NotNull(vm.ClosePopupCommand);
            Assert.NotNull(vm.GenerateTableCommand);
        }

        [Test]
        public void ViewModelWithLeveeBreach_GettingSelectedGrowthFormula_ShouldGiveExpectedResults()
        {
            var vm = new LeveeBreachViewModel
            {
                LeveeBreach = new LeveeBreach
                {
                    LeveeBreachFormula = LeveeBreachGrowthFormula.VerweijvdKnaap2002
                }
            };

            Assert.AreEqual(LeveeBreachGrowthFormula.VerweijvdKnaap2002, vm.SelectedGrowthFormula);

            vm.LeveeBreach.LeveeBreachFormula = LeveeBreachGrowthFormula.VdKnaap2000;
            Assert.AreEqual(LeveeBreachGrowthFormula.VdKnaap2000, vm.SelectedGrowthFormula);

            vm.LeveeBreach = null;
            Assert.AreEqual(LeveeBreachGrowthFormula.VdKnaap2000, vm.SelectedGrowthFormula);
        }

        [Test]
        public void ViewModelWithLeveeBreach_SettingSelectedGrowthFormula_ShouldSetFormulaInLeveeBreach()
        {
            var vm = new LeveeBreachViewModel
            {
                LeveeBreach = new LeveeBreach(),
                SelectedGrowthFormula = LeveeBreachGrowthFormula.VdKnaap2000,
            };

            Assert.AreEqual(LeveeBreachGrowthFormula.VdKnaap2000, vm.LeveeBreach.LeveeBreachFormula);
        }

        [Test]
        public void ViewModelWithoutLeveeBreach_SettingSelectedGrowthFormula_ShouldNotCauseCrash()
        {
            var vm = new LeveeBreachViewModel();
            vm.LeveeBreach = null;
            vm.SelectedGrowthFormula = LeveeBreachGrowthFormula.VdKnaap2000;
        }

        [Test]
        public void ViewModelWithLeveeBreach_SettingLeveeMaterial_ShouldSetMaterialInSettings()
        {
            var vm = new LeveeBreachViewModel { LeveeBreach = new LeveeBreach { LeveeBreachFormula = LeveeBreachGrowthFormula.VdKnaap2000 } };

            vm.UseSand = true;
            var settings = vm.LeveeBreach.GetLeveeBreachSettings() as LeveeBreachSettingsVdKnaap2000;
            Assert.NotNull(settings);
            Assert.IsFalse(vm.UseClay);
            Assert.AreEqual(LeveeMaterial.Sand, settings.LeveeMaterial);

            vm.UseClay = true;
            Assert.IsFalse(vm.UseSand);
            Assert.AreEqual(LeveeMaterial.Clay, settings.LeveeMaterial);
        }

        [Test]
        public void ViewModelWithLeveeBreach_ExecutingGenerateTableCommand_ShouldGenerateTable()
        {
            var vm = new LeveeBreachViewModel
            {
                LeveeBreach = new LeveeBreach { LeveeBreachFormula = LeveeBreachGrowthFormula.VdKnaap2000 }
            };
            vm.ShowGenerateTablePopup = true;
            vm.GenerateTableCommand.Execute(null);

            Assert.Fail("Not yet implemented");

            Assert.IsFalse(vm.ShowGenerateTablePopup);

        }
    }
}
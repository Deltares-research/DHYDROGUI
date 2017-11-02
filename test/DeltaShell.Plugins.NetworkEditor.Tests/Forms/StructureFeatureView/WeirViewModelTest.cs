using System.ComponentModel;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{
    [TestFixture]
    public class WeirViewModelTest
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void GivenWeirViewModel_WhenWeirIsNull_ThenHasWeirMethodReturnsFalse()
        {
            var viewModel = new WeirViewModel();
            Assert.IsFalse(viewModel.HasWeir);
        }

        [Test]
        public void GivenWeirViewModel_WhenWeirIsNotNull_ThenHasWeirReturnsTrue()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir()
            };
            Assert.IsTrue(viewModel.HasWeir);
        }

        #region GateOpeningHeight

        [Test]
        public void GivenWeirViewModelWithoutWeir_WhenGettingGateOpeningHeight_ThenReturnsZero()
        {
            var viewModel = new WeirViewModel();
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(0.0));
        }

        [Test]
        public void GivenWeirFormulaNotEqualToGeneralStructureWeirFormula_WhenGettingGateOpeningHeight_ThenReturnsZero()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir {WeirFormula = new SimpleWeirFormula()}
            };
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(0.0));
        }

        [Test]
        public void GivenGeneralStructureWeirFormula_WhenGettingGateOpeningHeight_ThenReturnGateOpeningValue()
        {
            const int returnValue = 123;
            var gsWeirFormula = mocks.DynamicMock<GeneralStructureWeirFormula>();
            gsWeirFormula.Expect(f => f.GateOpening).Return(returnValue).Repeat.Any();
            mocks.ReplayAll();

            var viewModel = new WeirViewModel
            {
                Weir = new Weir {WeirFormula = gsWeirFormula}
            };
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(returnValue));
        }

        [Test]
        public void GivennWeirViewModelWithoutWeir_WhenSettingGateOpeningHeight_ThenDoNotChangeValue()
        {
            var viewModel = new WeirViewModel();

            viewModel.GateOpeningHeight = 2;
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(0.0d));
        }

        [Test]
        public void GivenWeirWithWeirFormulaUnequalToGeneralStructureWeirFormula_WhenSettingGateOpeningHeight_ThenDoNotChangeValue()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir {WeirFormula = new SimpleWeirFormula()}
            };

            viewModel.GateOpeningHeight = 2;
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(0.0d));
        }

        [Test]
        public void GivenWeirWithGeneralStructureWeirFormula_WhenSettingGateOpeningHeight_ThenChangeValue()
        {
            var setValue = 2;
            var viewModel = new WeirViewModel
            {
                Weir = new Weir {WeirFormula = new GeneralStructureWeirFormula()}
            };

            viewModel.GateOpeningHeight = setValue;
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(2.0d));
        }

        [Test]
        public void GivenWeirWithGeneralStructureWeirFormula_WhenSettingGateOpeningHeight_ThenLowerEdgeLevelChanges()
        {
            var setValue = 2;
            var viewModel = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula
                    {
                        BedLevelStructureCentre = 3.5
                    }
                }
            };

            viewModel.GateOpeningHeight = setValue;
            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(5.5d));
        }

        #endregion

        #region SelectedWeirType

        [Test]
        public void GivenWeirViewModel_WhenSettingSelectedWeirTypeToSimpleWeir_ThenWeirFormulaIsSimpleWeirFormula()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir()
            };

            var count = 0;
            ((INotifyPropertyChanged) viewModel.Weir).PropertyChanged += (s, e) => count++;

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleWeir;
            Assert.That(viewModel.Weir.WeirFormula is SimpleWeirFormula);
            Assert.IsFalse(viewModel.GateGroupboxEnabled);
            Assert.That(viewModel.CrestLevelVisibility, Is.EqualTo(System.Windows.Visibility.Visible));
            Assert.AreEqual(1, count);
        }

        [Test]
        public void GivenWeirViewModel_WhenSettingSelectedWeirTypeToGeneralStructure_ThenWeirFormulaIsGeneralStructureWeirFormula()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir()
            };

            var count = 0;
            ((INotifyPropertyChanged) viewModel.Weir).PropertyChanged += (s, e) => count++;

            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;
            Assert.That(viewModel.Weir.WeirFormula is GeneralStructureWeirFormula);
            Assert.IsTrue(viewModel.GateGroupboxEnabled);
            Assert.That(viewModel.CrestLevelVisibility, Is.EqualTo(System.Windows.Visibility.Collapsed));
            Assert.AreEqual(1, count);
        }

        #endregion

        #region BedLevel

        [Test]
        public void GivenWeirViewModelWithoutWeirWhenGettingBedLevelStructureCentreThenReturnsZero()
        {
            var viewModel = new WeirViewModel();
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(0.0));
        }

        [Test]
        public void GivenWeirFormulaNotEqualToGeneralStructureWeirFormulaWhenGettingBedLevelStructureCentreThenReturnsZero()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir { WeirFormula = new SimpleWeirFormula() }
            };
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(0.0));
        }

        [Test]
        public void GivenWeirFormulaEqualToGeneralStructureWeirFormulaWhenGettingBedLevelStructureCentreThenReturnsDefaultValue()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir { WeirFormula = new GeneralStructureWeirFormula() }
            };
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(0.0));
        }

        #endregion

        #region LowerEdgeLevel

        [Test]
        public void GivenWeirViewModelWithGeneralStructureWeirFormulaWhenGettingLowerEdgeLevelThenReturnValue()
        {
            var vm = new WeirViewModel()
            {
                Weir = new Weir()
                {
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };
            vm.GateOpeningHeight = 123;
            ((GeneralStructureWeirFormula)vm.Weir.WeirFormula).BedLevelStructureCentre = 321;

            Assert.That(vm.LowerEdgeLevel, Is.EqualTo(444));
        }

        [Test]
        public void GivenWeirViewModelWithSimpleWeirFormulaWhenGettingLowerEdgeLevelThenReturnZero()
        {
            var vm = new WeirViewModel()
            {
                Weir = new Weir()
                {
                    WeirFormula = new SimpleWeirFormula()
                }
            };

            Assert.That(vm.LowerEdgeLevel, Is.EqualTo(0.0d));
        }

        [Test]
        public void GivenWeirViewModelWithoutWeirWhenSettingLowerEdgeLevelThenDoNotChangeValue()
        {
            var viewModel = new WeirViewModel();

            viewModel.LowerEdgeLevel = 2;
            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(0.0d));
        }

        [Test]
        public void GivenWeirWithWeirFormulaUnequalToGeneralStructureWeirFormulaWhenSettingLowerEdgeLevelThenDoNotChangeValue()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir { WeirFormula = new SimpleWeirFormula() }
            };

            viewModel.LowerEdgeLevel = 2;
            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(0.0d));
        }

        [Test]
        public void GivenWeirWithGeneralStructureWeirFormulaWhenSettingLowerEdgeLevelThenChangeValue()
        {
            var setValue = 2;
            var viewModel = new WeirViewModel
            {
                Weir = new Weir { WeirFormula = new GeneralStructureWeirFormula() }
            };

            viewModel.LowerEdgeLevel = setValue;
            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(2.0d));
        }

        [Test]
        public void GivenWeirWithGeneralStructureWeirFormulaWhenSettingLowerEdgeLevelThenGateOpeningHeightChanges()
        {
            var setValue = 2;
            var viewModel = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula
                    {
                        BedLevelStructureCentre = 3.5
                    }
                }
            };

            viewModel.LowerEdgeLevel = setValue;
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(-1.5d));
        }

        #endregion

        #region GeneralStructure relation between lower edge level, gate opening height and bed level

        [Test]
        public void GivenWeirViewModelWhenWeirFormulaIsGeneralStructureThenDetermineLowerEdgeLevel()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };

            var formula = (GeneralStructureWeirFormula)viewModel.Weir.WeirFormula;
            Assert.AreEqual(viewModel.LowerEdgeLevel, formula.GateOpening + formula.BedLevelStructureCentre);
        }

        [Test]
        public void GivenWeirViewModelWhenWeirFormulaIsGeneralStructureThenSetLowerEdgeLevelAndGateOpeningHeightChanges()
        {
            var setValue = 4;
            var viewModel = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };
            
            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(1.0d));
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(1.0d));
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(0.0d));

            viewModel.LowerEdgeLevel = setValue;

            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(4.0d));
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(4.0d));
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(0.0d));
        }

        [Test]
        public void GivenWeirViewModelWhenWeirFormulaIsGeneralStructureThenSetGateOpeningHeightAndLowerEdgeLevelChanges()
        {
            var setValue = 4;
            var viewModel = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };

            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(1.0d));
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(1.0d));
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(0.0d));

            viewModel.GateOpeningHeight = setValue;

            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(4.0d));
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(4.0d));
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(0.0d));
        }

        [Test]
        public void GivenWeirViewModelWhenWeirFormulaIsGeneralStructureThenSetBedLevelAndLowerEdgeLevelChanges()
        {
            var setValue = 4;
            var viewModel = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };

            var formula = (GeneralStructureWeirFormula)viewModel.Weir.WeirFormula;
            var count = 0;
            ((INotifyPropertyChanged)viewModel.Weir.WeirFormula).PropertyChanged += (s, e) => count++;
            
            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(1.0d));
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(1.0d));
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(0.0d));

            formula.BedLevelStructureCentre = setValue;

            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(5.0d));
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(1.0d));
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(4.0d));
            Assert.That(count == 1);
        }

        #endregion

        #region EnableAdvancedSettings

        [Test]
        public void GivenWeirViewModelWhenGettingEnableAdvanceSettingsThenReturnDefaultValueFalse()
        {
            var vm = new WeirViewModel();
            Assert.That(vm.EnableAdvancedSettings, Is.EqualTo(false));
        }

        [Test]
        public void GivenWeirViewModelWhenSettingEnableAdvanceSettingsThenPropertyChanged()
        {
            var vm = new WeirViewModel();

            var count = 0;
            ((INotifyPropertyChanged)vm).PropertyChanged += (s, e) => count++;
            vm.EnableAdvancedSettings = !vm.EnableAdvancedSettings;

            Assert.That(count, Is.EqualTo(1));
        }

        #endregion

    }
}
using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
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

        #region TimeSeriesEditor

        [Test]
        public void GivenWeirViewModel_WhenWeirIsTimeDependent_ViewShouldNotSetItToConstant()
        {
            var weir = new Weir(true){ UseCrestLevelTimeSeries = true};
            Assert.IsTrue(weir.UseCrestLevelTimeSeries);
            var viewModel = new WeirViewModel
            {
                Weir = weir,
            };
            
            Assert.IsTrue(weir.UseCrestLevelTimeSeries);
            Assert.IsFalse(viewModel.IsCrestLevelConstantTime);
            Assert.IsTrue(viewModel.EnableCrestLevelTimeSeries);
        }

        [Test]
        public void GivenWeirViewModel_WhenWeirIsSetToConstant_IsCrestLevelConstantTime_ShouldBeTrue()
        {
            var weir = new Weir(true) { UseCrestLevelTimeSeries = true };
            var viewModel = new WeirViewModel
            {
                Weir = weir,
            };
            Assert.IsTrue(weir.UseCrestLevelTimeSeries);
            Assert.IsFalse(viewModel.IsCrestLevelConstantTime);

            weir.UseCrestLevelTimeSeries = false;
            Assert.IsFalse(weir.UseCrestLevelTimeSeries);
            Assert.IsTrue(viewModel.IsCrestLevelConstantTime);
            Assert.IsFalse(viewModel.EnableCrestLevelTimeSeries);
        }

        [Test]
        public void GivenWeirViewModel_WhenWeirIsChangedToTimeDependent_ViewGetsRefreshedCorrectly()
        {
            var weir = new Weir(true);
            var viewModel = new WeirViewModel
            {
                Weir = weir
            };

            var count = 0;
            ((INotifyPropertyChanged)viewModel).PropertyChanged += (s, e) => count++;

            Assert.IsFalse(weir.UseCrestLevelTimeSeries);
            Assert.IsFalse(viewModel.EnableCrestLevelTimeSeries);
            Assert.IsTrue(viewModel.IsCrestLevelConstantTime);

            //Make it time dependent
            weir.UseCrestLevelTimeSeries = true;

            //Because the bubbling event tiggered the property change in the view model.
            //OnPropertyChanged(nameof(vm => vm.EnableCrestLevelTimeSeries));
            //OnPropertyChanged(nameof(vm => vm.IsCrestLevelConstantTime));
            Assert.IsTrue(count.Equals(2));

            Assert.IsTrue(weir.UseCrestLevelTimeSeries);
            Assert.IsTrue(viewModel.EnableCrestLevelTimeSeries);
            Assert.IsFalse(viewModel.IsCrestLevelConstantTime);
        }

        [Test]
        public void GivenWeirViewModel_WhenWeirIsNotTimeDependent_EnableTimeDependent_LogMessageIsGiven()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir("testName")
            };
            Assert.IsFalse(viewModel.Weir.CanBeTimedependent);

            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => viewModel.EnableCrestLevelTimeSeries = true,
                string.Format(Resources.WeirViewModel_EditTimeSeries_The_weir__0__does_not_support_Time_Series_, viewModel.Weir.Name));

            Assert.IsFalse(viewModel.EnableCrestLevelTimeSeries);
        }

        [Test]
        public void GivenWeirViewModel_WhenWeirIsTimeDependent_EnableTimeDependent_LogMessageIsNotGiven()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir("testName", true)
            };
            Assert.IsTrue(viewModel.Weir.CanBeTimedependent);
            Assert.Throws<AssertionException>(() => TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => viewModel.EnableCrestLevelTimeSeries = true,
                    string.Format(Resources.WeirViewModel_EditTimeSeries_The_weir__0__does_not_support_Time_Series_,
                        viewModel.Weir.Name)),
                "Expected no log messages, but at least one was found.");
        }

        [Test]
        public void GivenWeirViewModel_WhenWeirIsNotTimeDependent_OnEditTimeSeries_LogMessageIsGiven()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir("testName")
            };
            Assert.IsFalse(viewModel.Weir.CanBeTimedependent);
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => viewModel.OnEditCrestLevelTimeSeries.Execute(null),
                string.Format(Resources.WeirViewModel_EditTimeSeries_The_weir__0__does_not_support_Time_Series_, viewModel.Weir.Name));
        }

        [Test]
        public void GivenWeirViewModel_WhenWeirIsTimeDependent_OnEditTimeSeries_LogMessageIsNotGiven()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir("testName", true)
            };
            Assert.IsTrue(viewModel.Weir.CanBeTimedependent);
            Assert.Throws<AssertionException>(() => TestHelper.AssertAtLeastOneLogMessagesContains(
                () => viewModel.OnEditCrestLevelTimeSeries.Execute(null),
                string.Format(Resources.WeirViewModel_EditTimeSeries_The_weir__0__does_not_support_Time_Series_,
                    viewModel.Weir.Name)),
                    "Expected no log messages, but at least one was found.");
        }

        [Test]
        public void GivenWeirViewModel_WhenGeneralStructure_Set_UseCrestLevelTimeSeries_True_LogsErrorMessage()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(true)
            };
            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;

            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => viewModel.EnableCrestLevelTimeSeries = true,
                string.Format(Resources.WeirViewModel_EditTimeSeries_The_weir__0__does_not_support_Time_Series_, viewModel.Weir.Name));

            Assert.IsFalse(viewModel.EnableCrestLevelTimeSeries);
            Assert.IsFalse(viewModel.Weir.UseCrestLevelTimeSeries);
        }

        [Test]
        public void GivenWeirViewModel_WhenSimpleWeir_Set_UseCrestLevelTimeSeries_True_LogsErrorMessageIsNotGiven()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(true)
            };
            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleWeir;
            Assert.Throws<AssertionException>(
                () => TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => viewModel.EnableCrestLevelTimeSeries = true,
                    string.Format(Resources.WeirViewModel_EditTimeSeries_The_weir__0__does_not_support_Time_Series_, viewModel.Weir.Name)),
                "Expected no log messages, but at least one was found.");

            Assert.IsTrue(viewModel.EnableCrestLevelTimeSeries);
            Assert.IsTrue(viewModel.Weir.UseCrestLevelTimeSeries);
        }

        [Test]
        public void GivenWeirViewModel_WhenEditingTimeSeries_ModelGetsUpdated()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir("testWeir", true),
                GetTimeSeriesEditor = GenerateBasicTimeSeriesForIWeir
            };

            Assert.IsFalse(viewModel.Weir.CrestLevelTimeSeries.Time.Values.Any());
            viewModel.OnEditCrestLevelTimeSeries.Execute(null);

            //Check now there are values.
            Assert.IsTrue(viewModel.Weir.CrestLevelTimeSeries.Time.Values.Any());

            //Values From the method GenerateBasicTimeSeriesForIWeir
            var timeSeriesValues = GenerateBasicTimeSeriesForIWeir(new Weir());
            Assert.AreEqual(timeSeriesValues.Time.Values, viewModel.Weir.CrestLevelTimeSeries.Time.Values);
            Assert.AreEqual(timeSeriesValues.Components[0].Values, viewModel.Weir.CrestLevelTimeSeries.Components[0].Values);
        }

        private TimeSeries GenerateBasicTimeSeriesForIWeir(IWeir weir)
        {
            //We have IWeir in the header just for the signature in WeirViewModel.GetTimeSeriesEditor
            var timeSeries = new TimeSeries();
            timeSeries.Components.Add(new Variable<double>("value"));

            var dates = new[] { new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2003, 1, 1) };
            timeSeries.Time.SetValues(dates);

            var values = new[] { 0.0, 10.0, 20.0 };
            timeSeries.Components[0].SetValues(values);

            return timeSeries;
        }

        #endregion

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
            Assert.That(viewModel.SimpleWeirPropertiesVisibility, Is.EqualTo(System.Windows.Visibility.Visible));
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
            Assert.That(viewModel.SimpleWeirPropertiesVisibility, Is.EqualTo(System.Windows.Visibility.Collapsed));
            Assert.AreEqual(1, count);
        }

        [Test]
        public void GivenWeirViewModel_WhenSettingSelectedWeirTypeToGeneralStructure_ThenEnableTimeDependentIsFalse()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(true),
                EnableCrestLevelTimeSeries = true
            };

            Assert.IsTrue(viewModel.EnableCrestLevelTimeSeries);

            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;
            Assert.IsFalse(viewModel.EnableCrestLevelTimeSeries);
            Assert.IsFalse(viewModel.Weir.UseCrestLevelTimeSeries);
            Assert.IsTrue(viewModel.GateGroupboxEnabled);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GivenWeirViewModel_WhenSettingSelectedWeirType_FromGeneralStructure_ToSimpleWeir_ThenEnableTimeDependent_GetsItsPreviousValue(bool previousValue)
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(true),
                EnableCrestLevelTimeSeries = previousValue,
            };

            Assert.AreEqual(previousValue, viewModel.EnableCrestLevelTimeSeries);

            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;
            Assert.IsFalse(viewModel.EnableCrestLevelTimeSeries);

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleWeir;
            Assert.AreEqual(viewModel.EnableCrestLevelTimeSeries, previousValue);
            Assert.AreEqual(viewModel.Weir.UseCrestLevelTimeSeries, previousValue);
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
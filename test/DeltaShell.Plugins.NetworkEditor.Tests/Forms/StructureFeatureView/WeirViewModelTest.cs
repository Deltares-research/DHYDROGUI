using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Reflection;
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

        #region TimeSeriesEditor

        [Test]
        public void GivenWeirViewModel_WhenEditingTimeSeries_ModelGetsUpdated()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir("testWeir", true),
                GetTimeSeriesEditorForCrestLevel = GenerateBasicTimeSeriesForIWeir
            };

            Assert.IsFalse(viewModel.Weir.CrestLevelTimeSeries.Time.Values.Any());
            viewModel.OnEditCrestLevelTimeSeries.Execute(null);

            //Check now there are values.
            Assert.IsTrue(viewModel.Weir.CrestLevelTimeSeries.Time.Values.Any());

            //Values From the method GenerateBasicTimeSeriesForIWeir
            var timeSeriesValues = GenerateBasicTimeSeriesForIWeir(new Weir());
            Assert.AreEqual(timeSeriesValues.Time.Values, viewModel.Weir.CrestLevelTimeSeries.Time.Values);
            Assert.AreEqual(timeSeriesValues.Components[0].Values,
                viewModel.Weir.CrestLevelTimeSeries.Components[0].Values);
        }

        private TimeSeries GenerateBasicTimeSeriesForIWeir(IWeir weir)
        {
            //We have IWeir in the header just for the signature in WeirViewModel.GetTimeSeriesEditorForCrestLevel
            var timeSeries = new TimeSeries();
            timeSeries.Components.Add(new Variable<double>("value"));

            var dates = new[] {new DateTime(2000, 1, 1), new DateTime(2001, 1, 1), new DateTime(2003, 1, 1)};
            timeSeries.Time.SetValues(dates);

            var values = new[] {0.0, 10.0, 20.0};
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
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(-0.0));
        }

        [Test]
        public void GivenWeirWithGeneralStructureWeirFormula_WhenSettingBedLevelStructure_ThenCrestLevelIsUpdated()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula { }
                },

                BedLevelStructureCentre = 3.5d,
            };

            Assert.That(viewModel.Weir.CrestLevel, Is.EqualTo(3.5d));
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
            Assert.AreEqual(2, count,
                $"Expected 2 INotifyPropertyChanged.PropertyChanged events instead of {count} when setting the selected weir");
        }

        [Test]
        public void
            GivenWeirViewModel_WhenSettingSelectedWeirTypeToGeneralStructure_ThenWeirFormulaIsGeneralStructureWeirFormula()
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
            var crestLevelEnabled = (bool) TypeUtils.GetPropertyValue(viewModel, "CrestLevelEnabled");

            if (crestLevelEnabled && !viewModel.LowerEdgeLevelEnabled && !viewModel.HorizontalDoorOpeningWidthEnabled)
            {
                Assert.That(viewModel.SimpleWeirPropertiesVisibility, Is.EqualTo(System.Windows.Visibility.Visible));

            }

            Assert.AreEqual(4, count,
                $"Expected 4 INotifyPropertyChanged.PropertyChanged events instead of {count} when setting the selected weir");
        }

        [Test]
        public void GivenWeirViewModel_WhenSettingSelectedWeirTypeToSimpleGate_ThenWeirFormulaIsGatedWeirFormula()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir()
            };

            var count = 0;
            ((INotifyPropertyChanged) viewModel.Weir).PropertyChanged += (s, e) => count++;

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleGate;
            Assert.That(viewModel.Weir.WeirFormula is GatedWeirFormula);
            Assert.IsTrue(viewModel.GateGroupboxEnabled);
            var crestLevelEnabled = (bool) TypeUtils.GetPropertyValue(viewModel, "CrestLevelEnabled");

            if (crestLevelEnabled && !viewModel.LowerEdgeLevelEnabled && !viewModel.HorizontalDoorOpeningWidthEnabled)
            {
                Assert.That(viewModel.SimpleWeirPropertiesVisibility, Is.EqualTo(System.Windows.Visibility.Visible));

            }
            Assert.AreEqual(2, count,
                $"Expected 2 INotifyPropertyChanged.PropertyChanged events instead of {count} when setting the selected weir");
        }

        [Test]
        public void GivenWeirViewModel_WhenSettingSelectedWeirTypeToGeneralStructure_ThenEnableTimeDependentIsTrue()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(true),
                EnableCrestLevelTimeSeries = true
            };

            Assert.IsTrue(viewModel.EnableCrestLevelTimeSeries);

            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;
            Assert.IsTrue(viewModel.EnableCrestLevelTimeSeries);
            Assert.IsTrue(viewModel.Weir.UseCrestLevelTimeSeries);
            Assert.IsTrue(viewModel.GateGroupboxEnabled);
        }

        [Test]
        [TestCase(false)]
        public void
            GivenWeirViewModel_WhenSettingSelectedWeirType_FromGeneralStructure_ToSimpleWeir_WithValueFalse_ThenTimeSeriesNotEnabled(
                bool previousValue)
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

        [Test]
        [TestCase(true)]
        public void
            GivenWeirViewModel_WhenSettingSelectedWeirType_FromGeneralStructure_ToSimpleWeir_WithValueTrue_ThenTimeSeriesEnabled(
                bool previousValue)
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(true),
                EnableCrestLevelTimeSeries = previousValue,
            };

            Assert.AreEqual(previousValue, viewModel.EnableCrestLevelTimeSeries);

            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;
            Assert.IsTrue(viewModel.EnableCrestLevelTimeSeries);

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleWeir;
            Assert.AreEqual(viewModel.EnableCrestLevelTimeSeries, previousValue);
            Assert.AreEqual(viewModel.Weir.UseCrestLevelTimeSeries, previousValue);
        }

        [Test]
        public void
            GivenWeirViewModel_WhenAddingAStructureAndChangeToAnotherStructure_ThenTheCrestLevelAndCrestWidthArePersisted()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(true)
            };

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleWeir;
            Assert.AreEqual(0.0, viewModel.Weir.CrestLevel);
            Assert.AreEqual(0.0, viewModel.Weir.CrestWidth);

            viewModel.Weir.CrestLevel = 6.0;
            viewModel.Weir.CrestWidth = 5.0;

            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;
            Assert.AreEqual(6.0, viewModel.Weir.CrestLevel);
            Assert.AreEqual(5.0, viewModel.Weir.CrestWidth);

            viewModel.Weir.CrestLevel = 10.0;
            viewModel.Weir.CrestWidth = 4.0;

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleGate;
            Assert.AreEqual(10.0, viewModel.Weir.CrestLevel);
            Assert.AreEqual(4.0, viewModel.Weir.CrestWidth);

            viewModel.Weir.CrestLevel = 8.0;
            viewModel.Weir.CrestWidth = 7.0;

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleWeir;
            Assert.AreEqual(8.0, viewModel.Weir.CrestLevel);
            Assert.AreEqual(7.0, viewModel.Weir.CrestWidth);
        }

        [Test]
        public void
            GivenWeirViewModel_WhenChangingValuesOfGeneralStructure_SwitchToAnotherStructureAndBackToAGeneralStructure_ThenValuesArePersisted()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(true)
            };

            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;
            viewModel.Upstream1CrestLevel = 1.0;
            viewModel.Upstream2CrestLevel = 2.0;
            viewModel.Upstream1CrestWidth = 3.0;
            viewModel.Upstream2CrestWidth = 4.0;

            viewModel.Downstream1CrestLevel = 5.0;
            viewModel.Downstream2CrestLevel = 6.0;
            viewModel.Downstream1CrestWidth = 7.0;
            viewModel.Downstream2CrestWidth = 8.0;

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleGate;

            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;

            Assert.AreEqual(1.0, viewModel.Upstream1CrestLevel);
            Assert.AreEqual(2.0, viewModel.Upstream2CrestLevel);
            Assert.AreEqual(3.0, viewModel.Upstream1CrestWidth);
            Assert.AreEqual(4.0, viewModel.Upstream2CrestWidth);
            Assert.AreEqual(5.0, viewModel.Downstream1CrestLevel);
            Assert.AreEqual(6.0, viewModel.Downstream2CrestLevel);
            Assert.AreEqual(7.0, viewModel.Downstream1CrestWidth);
            Assert.AreEqual(8.0, viewModel.Downstream2CrestWidth);
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
        public void
            GivenWeirFormulaEqualToGeneralStructureWeirFormulaWhenGettingBedLevelStructureCentreThenReturnsDefaultValue()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir {WeirFormula = new GeneralStructureWeirFormula()}
            };
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(0.0));
        }

        #endregion

        #region LowerEdgeLevel

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
        public void
            GivenWeirWithWeirFormulaUnequalToGeneralStructureWeirFormulaWhenSettingLowerEdgeLevelThenDoNotChangeValue()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir {WeirFormula = new SimpleWeirFormula()}
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
                Weir = new Weir {WeirFormula = new GeneralStructureWeirFormula()}
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
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };

            var generalStructureFormula = viewModel.Weir.WeirFormula as GeneralStructureWeirFormula;
            generalStructureFormula.BedLevelStructureCentre = 3.5;

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

            var formula = (GeneralStructureWeirFormula) viewModel.Weir.WeirFormula;
            Assert.AreEqual(viewModel.LowerEdgeLevel, formula.LowerEdgeLevel);
        }

        [Test]
        public void
            GivenWeirViewModelWhenWeirFormulaIsGeneralStructureThenSetLowerEdgeLevelAndGateOpeningHeightChanges()
        {
            var setValue = 4;
            var viewModel = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };

            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(0.0d));
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(0.0d));
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(0.0d));

            viewModel.LowerEdgeLevel = setValue;

            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(4.0d));
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(4.0d));
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(0.0d));
        }

        [Test]
        public void GivenWeirViewModel_WhenWeirFormulaIsGeneralStructure_ThenSetBedLevelAndLowerEdgeLevelChanges()
        {
            var setValue = 4;
            var viewModel = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };

            var formula = (GeneralStructureWeirFormula) viewModel.Weir.WeirFormula;
            var count = 0;
            ((INotifyPropertyChanged) viewModel.Weir.WeirFormula).PropertyChanged += (s, e) => count++;

            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(0.0d));
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(0.0d));
            Assert.That(viewModel.BedLevelStructureCentre, Is.EqualTo(0.0d));

            viewModel.BedLevelStructureCentre = setValue;

            Assert.That(viewModel.LowerEdgeLevel, Is.EqualTo(0.0d));
            Assert.That(viewModel.GateOpeningHeight, Is.EqualTo(-4.0d));
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
            ((INotifyPropertyChanged) vm).PropertyChanged += (s, e) => count++;
            vm.EnableAdvancedSettings = !vm.EnableAdvancedSettings;

            Assert.That(count, Is.EqualTo(1));
        }

        #endregion

        #region

        [Test]
        public void
            GivenWeirViewModelsWhenGatedWeirFormulaAndGeneralStructuresWeirFormulaAreSetThenDetermineDoorHeight()
        {
            var viewModel1 = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GatedWeirFormula()
                }
            };

            var viewModel2 = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };

            var formula1 = (GatedWeirFormula) viewModel1.Weir.WeirFormula;
            Assert.AreEqual(viewModel1.DoorHeight, formula1.DoorHeight);
            var formula2 = (GeneralStructureWeirFormula) viewModel2.Weir.WeirFormula;
            Assert.AreEqual(viewModel2.DoorHeight, formula2.DoorHeight);
        }

        [Test]
        public void
            GivenWeirViewModelsWhenGatedWeirFormulaAndGeneralStructuresWeirFormulaAreSetThenDetermineGatedOpeningDirection()
        {
            var viewModel1 = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GatedWeirFormula()
                }
            };

            var viewModel2 = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };

            var formula1 = (GatedWeirFormula) viewModel1.Weir.WeirFormula;
            Assert.AreEqual(viewModel1.SelectedDoorOpeningHeightDirectionType, formula1.HorizontalDoorOpeningDirection);
            var formula2 = (GeneralStructureWeirFormula) viewModel2.Weir.WeirFormula;
            Assert.AreEqual(viewModel2.SelectedDoorOpeningHeightDirectionType, formula2.HorizontalDoorOpeningDirection);
        }

        [Test]
        public void GivenWeirViewModelWhenGeneralStructuresWeirFormulaIsSetThenDetermineUpstreamAndDownStream()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };

            var formula1 = (GeneralStructureWeirFormula) viewModel.Weir.WeirFormula;

            Assert.AreEqual(viewModel.Upstream1CrestWidth, formula1.WidthLeftSideOfStructure);
            Assert.AreEqual(viewModel.Upstream1CrestLevel, formula1.BedLevelLeftSideOfStructure);
            Assert.AreEqual(viewModel.Upstream2CrestWidth, formula1.WidthStructureLeftSide);
            Assert.AreEqual(viewModel.Upstream2CrestLevel, formula1.BedLevelLeftSideStructure);
            Assert.AreEqual(viewModel.Downstream1CrestWidth, formula1.WidthStructureRightSide);
            Assert.AreEqual(viewModel.Downstream1CrestLevel, formula1.BedLevelRightSideStructure);
            Assert.AreEqual(viewModel.Downstream2CrestWidth, formula1.WidthRightSideOfStructure);
            Assert.AreEqual(viewModel.Downstream2CrestLevel, formula1.BedLevelRightSideOfStructure);
        }

        [Test]
        public void GivenWeirViewModelWhenGatedWeirFormulaOrGeneralStructureWeirFormulaAreSetThenDetermineCrestLevel()
        {
            var viewModel1 = new WeirViewModel
            {
                Weir = new Weir
                {
                    CrestLevel = 1.0,
                    WeirFormula = new GatedWeirFormula()
                }
            };

            var viewModel2 = new WeirViewModel
            {
                Weir = new Weir
                {
                    CrestLevel = 2.0,
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };

            Assert.AreEqual(viewModel1.BedLevelStructureCentre, viewModel1.Weir.CrestLevel);
            Assert.AreEqual(viewModel2.BedLevelStructureCentre, viewModel2.Weir.CrestLevel);
        }

        [Test]
        public void GivenWeirViewModelWhenGatedWeirFormulaOrGeneralStructureWeirFormulaAreSetThenDetermineHorizontalDoorOpeningWidth()
        {
            var viewModel1 = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GatedWeirFormula()
                }
            };

            var viewModel2 = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };

            var formula1 = (GatedWeirFormula)viewModel1.Weir.WeirFormula;
            var formula2 = (GeneralStructureWeirFormula)viewModel2.Weir.WeirFormula;

            Assert.AreEqual(viewModel1.HorizontalDoorOpeningWidth, formula1.HorizontalDoorOpeningWidth);
            Assert.AreEqual(viewModel2.HorizontalDoorOpeningWidth, formula2.HorizontalDoorOpeningWidth);
        }

        [Test]
        public void GivenWeirViewModelWhenGeneralStructureWeirFormulaIsSetThenDetermineExtraResistance()
        {

            var viewModel = new WeirViewModel
            {
                Weir = new Weir
                {
                    WeirFormula = new GeneralStructureWeirFormula()
                }
            };

            var formula1 = (GeneralStructureWeirFormula)viewModel.Weir.WeirFormula;

            Assert.AreEqual(viewModel.ExtraResistance, formula1.ExtraResistance);
        }

        [Test]
        public void GivenWeirViewModel_WhenGeneralStructureWeirFormulaIsSet_ThenSimpleWeirVisibilityIsSet()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
            };

            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;
            Assert.That(viewModel.Weir.WeirFormula is GeneralStructureWeirFormula);
            Assert.AreEqual(viewModel.SimpleWeirPropertiesVisibility, System.Windows.Visibility.Visible);
            Assert.AreNotEqual(viewModel.SimpleWeirPropertiesVisibility,  System.Windows.Visibility.Collapsed);
        }

        [Test]
        public void GivenWeirViewModel_WhenSimpleWeirFormulaIsSet_ThenSimpleWeirVisibilityIsSet()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
            };

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleWeir;
            Assert.That(viewModel.Weir.WeirFormula is SimpleWeirFormula);
            Assert.AreEqual(viewModel.SimpleWeirPropertiesVisibility, System.Windows.Visibility.Visible);
            Assert.AreNotEqual(viewModel.SimpleWeirPropertiesVisibility,  System.Windows.Visibility.Collapsed);
        }

        [Test]
        public void GivenWeirViewModel_WhenGatedWeirFormulaIsSet_ThenSimpleWeirVisibilityIsSet()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
            };

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleGate;
            Assert.That(viewModel.Weir.WeirFormula is GatedWeirFormula);
            Assert.AreEqual(viewModel.SimpleWeirPropertiesVisibility, System.Windows.Visibility.Visible);
            Assert.AreNotEqual(viewModel.SimpleWeirPropertiesVisibility,  System.Windows.Visibility.Collapsed);
        }

        [Test]
        public void GivenWeirViewModel_WhenGatedWeirFormulaIsSet_ThenGeneralStructureVisibilityIsNotSet()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
            };

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleGate;
            Assert.That(viewModel.Weir.WeirFormula is GatedWeirFormula);
            Assert.AreEqual(viewModel.GeneralStructurePropertiesVisibility, System.Windows.Visibility.Collapsed);
            Assert.AreNotEqual(viewModel.GeneralStructurePropertiesVisibility,  System.Windows.Visibility.Visible);
        }

        [Test]
        public void GivenWeirViewModel_WhenSimpleWeirFormulaIsSet_ThenGeneralStructureVisibilityIsNotSet()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
            };

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleWeir;
            Assert.That(viewModel.Weir.WeirFormula is SimpleWeirFormula);
            Assert.AreEqual(viewModel.GeneralStructurePropertiesVisibility, System.Windows.Visibility.Collapsed);
            Assert.AreNotEqual(viewModel.GeneralStructurePropertiesVisibility,  System.Windows.Visibility.Visible);
        }

        [Test]
        public void GivenWeirViewModel_WhenGeneralStructureWeirFormulaIsSet_ThenGeneralStructureVisibilityIsSet()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
            };

            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;
            Assert.That(viewModel.Weir.WeirFormula is GeneralStructureWeirFormula);
            Assert.AreEqual(viewModel.GeneralStructurePropertiesVisibility, System.Windows.Visibility.Visible);
            Assert.AreNotEqual(viewModel.GeneralStructurePropertiesVisibility,  System.Windows.Visibility.Collapsed);
        }

        [Test]
        public void GivenWeirViewModel_WhenSimpleWeirFormulaIsSet_ThenGateGroupBoxIsDisabled()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
            };

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleWeir;
            Assert.That(viewModel.Weir.WeirFormula is SimpleWeirFormula);
            Assert.AreEqual(viewModel.GeneralStructurePropertiesVisibility, System.Windows.Visibility.Collapsed);
            Assert.AreEqual(viewModel.GateGroupboxEnabled, false);
            Assert.AreNotEqual(viewModel.GeneralStructurePropertiesVisibility,  System.Windows.Visibility.Visible);
        }

        [Test]
        public void GivenWeirViewModel_WhenGatedWeirFormulaIsSet_ThenGateGroupBoxIsEnabled()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
            };

            viewModel.SelectedWeirType = SelectableWeirFormulaType.SimpleGate;
            Assert.That(viewModel.Weir.WeirFormula is GatedWeirFormula);
            Assert.AreEqual(viewModel.GatedVisibility, System.Windows.Visibility.Visible);
            Assert.AreNotEqual(viewModel.GatedVisibility, System.Windows.Visibility.Collapsed);
            Assert.AreEqual(viewModel.GeneralStructurePropertiesVisibility, System.Windows.Visibility.Collapsed);
            Assert.AreEqual(viewModel.GateGroupboxEnabled, true);
        }

        [Test]
        public void GivenWeirViewModel_WhenGeneralStructureWeirFormulaIsSet_ThenGateGroupBoxIsEnabled()
        {
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
            };

            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;
            Assert.That(viewModel.Weir.WeirFormula is GeneralStructureWeirFormula);
            Assert.AreEqual(viewModel.GatedVisibility, System.Windows.Visibility.Visible);
            Assert.AreNotEqual(viewModel.GatedVisibility, System.Windows.Visibility.Collapsed);
            Assert.AreEqual(viewModel.GeneralStructurePropertiesVisibility, System.Windows.Visibility.Visible);
            Assert.AreEqual(viewModel.GateGroupboxEnabled, true);
        }

        #endregion


        #region Persistence 

        [Test]
        public void
            GivenAWeirViewModel_WhenSwitchingWeirFormula_ThenValuesShouldBePersisted()
        {
            // Given
            var viewModel = new WeirViewModel
            {
                Weir = new Weir(),
                SelectedWeirType = SelectableWeirFormulaType.SimpleGate,
                LowerEdgeLevel = 2.0,
                DoorHeight = 4.0,
                HorizontalDoorOpeningWidth = 3.0,
                SelectedDoorOpeningHeightDirectionType = GateOpeningDirection.FromRight
            };
            
            // When
            viewModel.SelectedWeirType = SelectableWeirFormulaType.GeneralStructure;

            // Then
            Assert.That(viewModel.LowerEdgeLevel == 2.0);
            Assert.That(viewModel.DoorHeight == 4.0);
            Assert.That(viewModel.HorizontalDoorOpeningWidth == 3.0);
            Assert.That(viewModel.SelectedDoorOpeningHeightDirectionType == GateOpeningDirection.FromRight);
        }
        
        #endregion

        [Test]
        public void GivenAWeirViewModel_WhenChangingTheCrestLevel_ThenTwoEventsShouldBeFiredForRefreshingTwoBoxesInTheView()
        {
            List<string> raisedEvents = new List<string>();
      
            var viewModel = new WeirViewModel()
            {
                Weir = new Weir2D{ WeirFormula = new SimpleWeirFormula() }
            };
            
            viewModel.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                raisedEvents.Add(e.PropertyName);
            };
            Assert.AreEqual(0, raisedEvents.Count);
            
            // Change property needed for firing the event
            viewModel.Weir.CrestLevel = 10;

            Assert.AreEqual(2, raisedEvents.Count,
                $"Expected 2 INotifyPropertyChanged.PropertyChanged events instead of {raisedEvents.Count} when setting the selected weir");
            Assert.That(raisedEvents.Contains("BedLevelStructureCentre"));
            Assert.That(raisedEvents.Contains("GateOpeningHeight"));

        }

        [Test]
        public void GivenAWeirViewModel_WhenChangingTheWeirFormulaToGatedWeir_ThenTwoEventsShouldBeFired()
        {
            var viewModel = new WeirViewModel()
            {
                Weir = new Weir2D {WeirFormula = new SimpleWeirFormula()}
            };

            var count = 0;
            ((INotifyPropertyChanged) viewModel.Weir).PropertyChanged += (s, e) => count++;

            Assert.AreEqual(0, count);

            // Change property needed for firing the event
            viewModel.Weir.WeirFormula = new GatedWeirFormula(true);

            Assert.AreEqual(2, count,
                $"Expected 2 INotifyPropertyChanged.PropertyChanged events instead of {count} when setting the selected weir");
        }

        [Test]
        public void GivenAWeirViewModel_WhenChangingTheWeirFormulaToGeneralStructure_ThenThreeEventsShouldBeFired()
        {
            var viewModel = new WeirViewModel()
            {
                Weir = new Weir2D { WeirFormula = new SimpleWeirFormula() }
            };

            var count = 0;
            ((INotifyPropertyChanged)viewModel.Weir).PropertyChanged += (s, e) => count++;

            Assert.AreEqual(0, count);

            // Change property needed for firing the event
            var generalStructureWeirFormula = new GeneralStructureWeirFormula
            {
                BedLevelStructureCentre = viewModel.Weir.CrestLevel,
                WidthStructureCentre = viewModel.Weir.CrestWidth,
            };

            viewModel.Weir.WeirFormula = generalStructureWeirFormula;

            Assert.AreEqual(3, count,
                $"Expected 3 INotifyPropertyChanged.PropertyChanged events instead of {count} when setting the selected weir");
        }
    }
}
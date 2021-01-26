using System;
using System.Collections.Generic;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.WeirFormulaViewModels;
using NUnit.Framework;
using Is = NUnit.Framework.Is;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Editors.Structures.ViewModels
{
    [TestFixture]
    public class StructureViewModelTest
    {
        public static IEnumerable<TestCaseData> GetConstructorData()
        {
            var simpleWeirFormula = new SimpleWeirFormula();
            var simpleWeir = new Structure {Formula = simpleWeirFormula};

            yield return new TestCaseData(simpleWeir, typeof(SimpleWeirViewModel));

            var gatedWeirFormula = new GatedWeirFormula();
            var gatedWeir = new Structure {Formula = gatedWeirFormula};

            yield return new TestCaseData(gatedWeir, typeof(GatedWeirViewModel));

            var generalStructureFormula = new GeneralStructureWeirFormula();
            var generalStructure = new Structure {Formula = generalStructureFormula};

            yield return new TestCaseData(generalStructure, typeof(GeneralStructureViewModel));
        }

        [Test]
        [TestCaseSource(nameof(GetConstructorData))]
        public void Constructor_ExpectedValuesSet(Structure weir, Type expectedViewModelType)
        {
            // Call
            using (var viewModel = new StructureViewModel(weir))
            {
                // Assert
                Assert.That(viewModel.FormulaType, Is.EqualTo(expectedViewModelType));
                Assert.That(viewModel.WeirViewModel, Is.Not.Null);
                Assert.That(viewModel.WeirViewModel, Is.InstanceOf(expectedViewModelType));
            }
        }

        [Test]
        public void Constructor_WeirNull_ThrowsArgumentNullException()
        {
            void Call() => new StructureViewModel(null);
            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("weir"));
        }

        [Test]
        public void FormulaTypeList_ExpectedValues()
        {
            // Setup
            var weir = new Structure();

            using (var viewModel = new StructureViewModel(weir))
            {
                // Call
                IReadOnlyList<Type> formulaTypes = viewModel.FormulaTypeList;

                // Assert
                Type[] expectedTypes =
                {
                    typeof(SimpleWeirViewModel), 
                    typeof(GatedWeirViewModel), 
                    typeof(GeneralStructureViewModel)
                };
                Assert.That(formulaTypes, Is.EqualTo(expectedTypes));
            }
        }

        public static IEnumerable<TestCaseData> GetFormulaTypeData()
        {
            var simpleWeir = new Structure {Formula = new SimpleWeirFormula()};
            var gatedWeir = new Structure {Formula = new GatedWeirFormula(true)};

            yield return new TestCaseData(gatedWeir,  typeof(SimpleWeirViewModel),       typeof(SimpleWeirFormula));
            yield return new TestCaseData(simpleWeir, typeof(GatedWeirViewModel),        typeof(GatedWeirFormula));
            yield return new TestCaseData(simpleWeir, typeof(GeneralStructureViewModel), typeof(GeneralStructureWeirFormula));
        }

        [Test]
        [TestCaseSource(nameof(GetFormulaTypeData))]
        public void FormulaType_SetDifferent_SetsValueCorrectly(Structure weir, 
                                                                Type newWeirViewModelType,
                                                                Type newFormulaType)
        {
            // Setup
            using (var viewModel = new StructureViewModel(weir))
            {
                var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.FormulaType = newWeirViewModelType;

                // Assert
                Assert.That(viewModel.FormulaType, Is.EqualTo(newWeirViewModelType));
                Assert.That(viewModel.WeirViewModel, Is.Not.Null);
                Assert.That(viewModel.WeirViewModel, Is.InstanceOf(newWeirViewModelType));

                Assert.That(weir.Formula, Is.InstanceOf(newFormulaType));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(2));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName, Is.EqualTo(nameof(viewModel.WeirViewModel)));
                Assert.That(propertyChangedObserver.Senders[1], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[1].PropertyName, Is.EqualTo(nameof(viewModel.FormulaType)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;
            }
        }

        public static IEnumerable<TestCaseData> GetFormulaTypeNoChangeData()
        {
            var simpleWeir = new Structure {Formula = new SimpleWeirFormula()};
            yield return new TestCaseData(simpleWeir,  typeof(SimpleWeirViewModel), typeof(SimpleWeirFormula));

            var gatedWeir = new Structure {Formula = new GatedWeirFormula(true)};
            yield return new TestCaseData(gatedWeir, typeof(GatedWeirViewModel), typeof(GatedWeirFormula));

            var generalStructure = new Structure {Formula = new GeneralStructureWeirFormula()};
            yield return new TestCaseData(generalStructure, typeof(GeneralStructureViewModel), typeof(GeneralStructureWeirFormula));
        }

        [Test]
        [TestCaseSource(nameof(GetFormulaTypeNoChangeData))]
        public void FormulaType_SetSame_DoesNotSetValue(Structure weir, 
                                                        Type newWeirViewModelType, 
                                                        Type expectedWeirFormulaType)
        {
            // Setup
            using (var viewModel = new StructureViewModel(weir))
            {
                var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
                viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

                // Call
                viewModel.FormulaType = newWeirViewModelType;

                // Assert
                Assert.That(viewModel.FormulaType, Is.EqualTo(newWeirViewModelType));
                Assert.That(viewModel.WeirViewModel, Is.Not.Null);
                Assert.That(viewModel.WeirViewModel, Is.InstanceOf(newWeirViewModelType));

                Assert.That(weir.Formula, Is.InstanceOf(expectedWeirFormulaType));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnPropertyChanged;
            }
        }

        [Test]
        public void FormulaType_SetToNonFormulaType_ThrowsArgumentException()
        {
            // Setup
            var weir = new Structure() {Formula = new SimpleWeirFormula()};

            using (var viewModel = new StructureViewModel(weir))
            {
                void Call() => viewModel.FormulaType = typeof(object);
                Assert.Throws<ArgumentException>(Call);
            }
        }
    }
}
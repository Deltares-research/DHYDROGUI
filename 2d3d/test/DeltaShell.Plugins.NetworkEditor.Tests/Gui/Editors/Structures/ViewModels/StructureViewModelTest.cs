using System;
using System.Collections.Generic;
using System.ComponentModel;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels;
using DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.ViewModels.StructureFormulaViewModels;
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

            yield return new TestCaseData(simpleWeir, typeof(SimpleWeirFormulaViewModel));

            var gatedWeirFormula = new SimpleGateFormula();
            var gatedWeir = new Structure {Formula = gatedWeirFormula};

            yield return new TestCaseData(gatedWeir, typeof(SimpleGateFormulaViewModel));

            var generalStructureFormula = new GeneralStructureFormula();
            var generalStructure = new Structure {Formula = generalStructureFormula};

            yield return new TestCaseData(generalStructure, typeof(GeneralStructureFormulaViewModel));
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
                Assert.That(viewModel.StructureFormulaViewModel, Is.Not.Null);
                Assert.That(viewModel.StructureFormulaViewModel, Is.InstanceOf(expectedViewModelType));
            }
        }

        [Test]
        public void Constructor_StructureNull_ThrowsArgumentNullException()
        {
            void Call() => new StructureViewModel(null);
            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("structure"));
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
                    typeof(SimpleWeirFormulaViewModel), 
                    typeof(SimpleGateFormulaViewModel), 
                    typeof(GeneralStructureFormulaViewModel)
                };
                Assert.That(formulaTypes, Is.EqualTo(expectedTypes));
            }
        }

        public static IEnumerable<TestCaseData> GetFormulaTypeData()
        {
            var simpleWeir = new Structure {Formula = new SimpleWeirFormula()};
            var gatedWeir = new Structure {Formula = new SimpleGateFormula(true)};

            yield return new TestCaseData(gatedWeir,  typeof(SimpleWeirFormulaViewModel),       typeof(SimpleWeirFormula));
            yield return new TestCaseData(simpleWeir, typeof(SimpleGateFormulaViewModel),        typeof(SimpleGateFormula));
            yield return new TestCaseData(simpleWeir, typeof(GeneralStructureFormulaViewModel), typeof(GeneralStructureFormula));
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
                var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.FormulaType = newWeirViewModelType;

                // Assert
                Assert.That(viewModel.FormulaType, Is.EqualTo(newWeirViewModelType));
                Assert.That(viewModel.StructureFormulaViewModel, Is.Not.Null);
                Assert.That(viewModel.StructureFormulaViewModel, Is.InstanceOf(newWeirViewModelType));

                Assert.That(weir.Formula, Is.InstanceOf(newFormulaType));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(2));
                Assert.That(propertyChangedObserver.Senders[0], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[0].PropertyName, Is.EqualTo(nameof(viewModel.StructureFormulaViewModel)));
                Assert.That(propertyChangedObserver.Senders[1], Is.SameAs(viewModel));
                Assert.That(propertyChangedObserver.EventArgses[1].PropertyName, Is.EqualTo(nameof(viewModel.FormulaType)));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;
            }
        }

        public static IEnumerable<TestCaseData> GetFormulaTypeNoChangeData()
        {
            var simpleWeir = new Structure {Formula = new SimpleWeirFormula()};
            yield return new TestCaseData(simpleWeir,  typeof(SimpleWeirFormulaViewModel), typeof(SimpleWeirFormula));

            var gatedWeir = new Structure {Formula = new SimpleGateFormula(true)};
            yield return new TestCaseData(gatedWeir, typeof(SimpleGateFormulaViewModel), typeof(SimpleGateFormula));

            var generalStructure = new Structure {Formula = new GeneralStructureFormula()};
            yield return new TestCaseData(generalStructure, typeof(GeneralStructureFormulaViewModel), typeof(GeneralStructureFormula));
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
                var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
                viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

                // Call
                viewModel.FormulaType = newWeirViewModelType;

                // Assert
                Assert.That(viewModel.FormulaType, Is.EqualTo(newWeirViewModelType));
                Assert.That(viewModel.StructureFormulaViewModel, Is.Not.Null);
                Assert.That(viewModel.StructureFormulaViewModel, Is.InstanceOf(newWeirViewModelType));

                Assert.That(weir.Formula, Is.InstanceOf(expectedWeirFormulaType));

                Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));

                // Clean up
                viewModel.PropertyChanged -= propertyChangedObserver.OnEventFired;
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
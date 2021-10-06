using System.Linq;
using System.Windows;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors
{
    [TestFixture]
    public class SedimentFractionsEditorViewModelTest
    {
        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestAddSedimentFractionInViewModelIsReflectedInObjectModel()
        {
            var numberOfSedimentFractions = 3;
            var objectModelSedimentFractions = SedimentFractionsEditorTestHelper.GetExampleSedimentFractions(numberOfSedimentFractions);

            var viewModel = new SedimentFractionsEditorViewModel() { ObjectModelSedimentFractions = objectModelSedimentFractions };
            Assert.AreEqual(numberOfSedimentFractions, viewModel.SedimentFractions.Count);

            viewModel.CurrentFractionName = "NewFraction";
            viewModel.OnAddCommand.Execute(null);
            Assert.AreEqual(numberOfSedimentFractions +1, objectModelSedimentFractions.Count);
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestRemoveSedimentFractionInViewModelIsReflectedInObjectModel()
        {
            var numberOfSedimentFractions = 3;
            var objectModelSedimentFractions = SedimentFractionsEditorTestHelper.GetExampleSedimentFractions(numberOfSedimentFractions);

            var viewModel = new SedimentFractionsEditorViewModel() { ObjectModelSedimentFractions = objectModelSedimentFractions };
            Assert.AreEqual(numberOfSedimentFractions, objectModelSedimentFractions.Count);

            viewModel.CurrentSedimentFraction = viewModel.SedimentFractions.Last();
            viewModel.OnRemoveCommand.Execute(null);
            Assert.AreEqual(numberOfSedimentFractions -1, objectModelSedimentFractions.Count);
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestAddSedimentFractionCreatesUniqueName()
        {
            var viewModel = new SedimentFractionsEditorViewModel();
            viewModel.CurrentFractionName = "NewFraction";
            viewModel.OnAddCommand.Execute(null);
            Assert.AreEqual(1, viewModel.ObjectModelSedimentFractions.Count);

            viewModel.CurrentFractionName = "NewFraction";
            viewModel.OnAddCommand.Execute(null);
            Assert.AreEqual(2, viewModel.ObjectModelSedimentFractions.Count);

            viewModel.CurrentFractionName = "NewFraction";
            viewModel.OnAddCommand.Execute(null);
            Assert.AreEqual(3, viewModel.ObjectModelSedimentFractions.Count);

            var fractionNames = viewModel.SedimentFractions.Select(f => f.Name).ToList();
            CollectionAssert.AllItemsAreUnique(fractionNames);
        }
        
        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestAddSedimentFractionUpdatesCurrentSedimentFraction()
        {
            var numberOfSedimentFractions = 3;
            var objectModelSedimentFractions = SedimentFractionsEditorTestHelper.GetExampleSedimentFractions(numberOfSedimentFractions);

            var viewModel = new SedimentFractionsEditorViewModel() { ObjectModelSedimentFractions = objectModelSedimentFractions };
            Assert.AreEqual(numberOfSedimentFractions, viewModel.SedimentFractions.Count);

            var originalCurrentSedimentFraction = viewModel.CurrentSedimentFraction;

            viewModel.CurrentFractionName = "NewFraction";
            viewModel.OnAddCommand.Execute(null);
            Assert.AreEqual(numberOfSedimentFractions + 1, viewModel.SedimentFractions.Count);

            var newCurrentSedimentFraction = viewModel.CurrentSedimentFraction;
            Assert.AreNotEqual(originalCurrentSedimentFraction.Name, newCurrentSedimentFraction.Name);
            Assert.AreEqual(viewModel.CurrentFractionName, newCurrentSedimentFraction.Name);
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestRemoveSedimentFractionUpdatesCurrentSedimentFraction()
        {
            var numberOfSedimentFractions = 3;
            var objectModelSedimentFractions = SedimentFractionsEditorTestHelper.GetExampleSedimentFractions(numberOfSedimentFractions);

            var viewModel = new SedimentFractionsEditorViewModel() { ObjectModelSedimentFractions = objectModelSedimentFractions };
            Assert.AreEqual(numberOfSedimentFractions, viewModel.SedimentFractions.Count);

            viewModel.CurrentSedimentFraction = viewModel.SedimentFractions.Last();

            var originalCurrentSedimentFraction = viewModel.CurrentSedimentFraction;
            viewModel.OnRemoveCommand.Execute(null);
            Assert.AreEqual(numberOfSedimentFractions - 1, viewModel.SedimentFractions.Count);

            var newCurrentSedimentFraction = viewModel.CurrentSedimentFraction;
            Assert.AreNotEqual(originalCurrentSedimentFraction.Name, newCurrentSedimentFraction.Name);
            Assert.AreEqual(viewModel.CurrentFractionName, newCurrentSedimentFraction.Name);
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestChangesToCurrentSedimentTypeAreReflectedInObjectModel()
        {
            var numberOfSedimentFractions = 3;
            var objectModelSedimentFractions = SedimentFractionsEditorTestHelper.GetExampleSedimentFractions(numberOfSedimentFractions);

            var viewModel = new SedimentFractionsEditorViewModel() { ObjectModelSedimentFractions = objectModelSedimentFractions };
            Assert.AreEqual(numberOfSedimentFractions, viewModel.SedimentFractions.Count);

            var sedimentFraction = viewModel.CurrentSedimentFraction;
            sedimentFraction.CurrentSedimentType = sedimentFraction.AvailableSedimentTypes.First(t => t.Name != sedimentFraction.CurrentSedimentType.Name);

            var objectModelSedimentFraction = objectModelSedimentFractions.First(f => f.Name == sedimentFraction.Name);
            Assert.AreEqual(objectModelSedimentFraction.CurrentSedimentType.Name, sedimentFraction.CurrentSedimentType.Name);
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestChangesToCurrentFormulaTypeAreReflectedInObjectModel()
        {
            var numberOfSedimentFractions = 3;
            var objectModelSedimentFractions = SedimentFractionsEditorTestHelper.GetExampleSedimentFractions(numberOfSedimentFractions);

            var viewModel = new SedimentFractionsEditorViewModel() { ObjectModelSedimentFractions = objectModelSedimentFractions };
            Assert.AreEqual(numberOfSedimentFractions, viewModel.SedimentFractions.Count);

            var sedimentFraction = viewModel.CurrentSedimentFraction;
            sedimentFraction.CurrentFormulaType = sedimentFraction.SupportedFormulaTypes.First(t => t.Name != sedimentFraction.CurrentFormulaType.Name);

            var objectModelSedimentFraction = objectModelSedimentFractions.First(f => f.Name == sedimentFraction.Name);
            Assert.AreEqual(objectModelSedimentFraction.CurrentFormulaType.Name, sedimentFraction.CurrentFormulaType.Name);
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestFractionsVisible()
        {
            var viewModel = new SedimentFractionsEditorViewModel();
            Assert.AreEqual(Visibility.Hidden, viewModel.FractionsVisible);

            viewModel.CurrentFractionName = "NewFraction";
            viewModel.OnAddCommand.Execute(null);
            Assert.AreEqual(Visibility.Visible, viewModel.FractionsVisible);

            viewModel.CurrentSedimentFraction = viewModel.SedimentFractions.Last();
            viewModel.OnRemoveCommand.Execute(null);
            Assert.AreEqual(Visibility.Hidden, viewModel.FractionsVisible);
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestFormulasVisible()
        {
            var viewModel = new SedimentFractionsEditorViewModel();
            Assert.AreEqual(Visibility.Hidden, viewModel.FormulasVisible);

            viewModel.CurrentFractionName = "NewFraction";
            viewModel.OnAddCommand.Execute(null);

            var sedimentFraction = viewModel.CurrentSedimentFraction;
            sedimentFraction.CurrentSedimentType = sedimentFraction.AvailableSedimentTypes.First(t => t.Name == "Sand");
            Assert.IsTrue(sedimentFraction.SupportedFormulaTypes.Any());

            Assert.AreEqual(Visibility.Visible, viewModel.FormulasVisible);

            sedimentFraction.CurrentSedimentType = sedimentFraction.AvailableSedimentTypes.First(t => t.Name == "Bed-load");
            Assert.IsTrue(sedimentFraction.SupportedFormulaTypes.Any());
            Assert.AreEqual(Visibility.Visible, viewModel.FormulasVisible);

            viewModel.OnRemoveCommand.Execute(null);
            Assert.AreEqual(Visibility.Hidden, viewModel.FractionsVisible);
            Assert.AreEqual(Visibility.Hidden, viewModel.FormulasVisible);
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void TestSedimentProperties_Gui_and_MduOnly()
        {
            // setup
            var exampleSedimentFractions = SedimentFractionsEditorTestHelper.GetExampleSedimentFractions(1);
            var objectModelSedimentFractions = exampleSedimentFractions;

            var viewModel = new SedimentFractionsEditorViewModel() { ObjectModelSedimentFractions = objectModelSedimentFractions };
            Assert.AreEqual(1, viewModel.SedimentFractions.Count);

            // set MduOnly to true
            var sedimentType = viewModel.CurrentSedimentType;
            sedimentType.Properties.ForEach(p => p.MduOnly = true);
            //exampleSedimentFractions.ForEach(sf => sf.CompileAndSetVisibilityAndIfEnabled());

            Assert.IsEmpty(viewModel.CurrentSedimentGuiProperties);

            // set MduOnly to false
            sedimentType.Properties.ForEach(p => p.MduOnly = false);
            exampleSedimentFractions.ForEach(sf => sf.CompileAndSetVisibilityAndIfEnabled());
            Assert.AreEqual(sedimentType.Properties.Count(), viewModel.CurrentSedimentGuiProperties.Count());
            
            // set Visibility to false
            sedimentType.Properties.Take(3).ForEach(p => p.Visible = list => false);
            exampleSedimentFractions.ForEach(sf => sf.CompileAndSetVisibilityAndIfEnabled());
            Assert.AreEqual(sedimentType.Properties.Count()-3, viewModel.CurrentSedimentGuiProperties.Count());

        }
    }
}

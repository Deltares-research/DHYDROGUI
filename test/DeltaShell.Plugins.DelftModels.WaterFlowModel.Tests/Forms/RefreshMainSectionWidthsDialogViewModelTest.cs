using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    public class RefreshMainSectionWidthsDialogViewModelTest
    {
        [Test]
        public void SettingCrossSectionsShouldResultInCrossSectionViewModels()
        {
            var crossSections = new List<ICrossSection>
            {
                new CrossSection(new CrossSectionDefinitionZW("Definition test 1")){Name = "Test 1"},
                new CrossSection(new CrossSectionDefinitionZW("Definition test 2")){Name = "Test 2"}
            };

            var viewModel = new RefreshMainSectionWidthsDialogViewModel();

            Assert.IsNull(viewModel.CrossSectionViewModels);

            viewModel.CrossSections = crossSections;

            Assert.IsNotNull(viewModel.CrossSectionViewModels);
            Assert.AreEqual(2, viewModel.CrossSectionViewModels.Count);
        }

        [Test]
        public void DeSelectAllCommandCommandShouldDeselectAllCrossSectionViewModels()
        {
            var viewModel = new RefreshMainSectionWidthsDialogViewModel
            {
                CrossSections = new List<ICrossSection>
                {
                    new CrossSection(new CrossSectionDefinitionZW("Definition test 1")) {Name = "Test 1"},
                    new CrossSection(new CrossSectionDefinitionZW("Definition test 2")) {Name = "Test 2"}
                }
            };

            var command = viewModel.DeSelectAllCommand;

            Assert.NotNull(command);
            Assert.IsTrue(command.CanExecute(null));

            Assert.AreEqual(2, viewModel.CrossSectionViewModels.Count(vm => vm.Selected));

            command.Execute(null);
            
            Assert.AreEqual(0, viewModel.CrossSectionViewModels.Count(vm => vm.Selected));

            Assert.IsFalse(command.CanExecute(null));
        }

        [Test]
        public void SelectAllCommandCommandShouldSelectAllCrossSectionViewModels()
        {
            var viewModel = new RefreshMainSectionWidthsDialogViewModel
            {
                CrossSections = new List<ICrossSection>
                {
                    new CrossSection(new CrossSectionDefinitionZW("Definition test 1")) {Name = "Test 1"},
                    new CrossSection(new CrossSectionDefinitionZW("Definition test 2")) {Name = "Test 2"}
                }
            };

            viewModel.CrossSectionViewModels.ForEach(c => c.Selected = false);

            var command = viewModel.SelectAllCommand;

            Assert.NotNull(command);
            Assert.IsTrue(command.CanExecute(null));

            Assert.AreEqual(0, viewModel.CrossSectionViewModels.Count(vm => vm.Selected));

            command.Execute(null);

            Assert.AreEqual(2, viewModel.CrossSectionViewModels.Count(vm => vm.Selected));

            Assert.IsFalse(command.CanExecute(null));
        }

        [Test]
        public void SelectedCrossSectionsAreFixedByFixSelectedCrossSectionsCommand()
        {
            var csDefZw = CrossSectionDefinitionZW.CreateDefault();
            csDefZw.Name = "Definition test 1";

            var mainSection = new CrossSectionSection()
            {
                SectionType = new CrossSectionSectionType() { Name = CrossSectionDefinition.MainSectionName },
                MinY = 0.0,
                MaxY = 30.0
            };

            csDefZw.Sections.Add(mainSection);
            
            var afterFixCount = 0;

            var viewModel = new RefreshMainSectionWidthsDialogViewModel
            {
                CrossSections = new List<ICrossSection>
                {
                    new CrossSection(csDefZw) {Name = "Test 1"}
                },
                AfterFix = () => afterFixCount++
            };

            var command = viewModel.FixSelectedCrossSectionsCommand;

            Assert.NotNull(command);
            Assert.IsTrue(command.CanExecute(null));

            Assert.AreNotEqual(csDefZw.SectionsTotalWidth(), csDefZw.FlowWidth());

            command.Execute(null);

            Assert.AreEqual(csDefZw.SectionsTotalWidth(), csDefZw.FlowWidth());

            Assert.AreEqual(1, afterFixCount);
        }
    }
}
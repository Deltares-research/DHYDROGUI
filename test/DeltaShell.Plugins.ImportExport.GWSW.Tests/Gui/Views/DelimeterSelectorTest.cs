using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.GWSW.ViewModels;
using DeltaShell.Plugins.ImportExport.GWSW.Views;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Views
{
    [TestFixture]
    public class DelimeterSelectorTest
    {
        [Category(TestCategory.WindowsForms)]
        [Test]
        [Apartment(System.Threading.ApartmentState.STA)]
        public void ShowDelimeterUserControl()
        {
            /*For some reason it crashes (sometimes) when closing it. For what I could read online it's due to the way we call the modal.
             It should be .Show, instead of .ShowModal*/
            var selector = new DelimeterSelector();
            selector.Data = '\t';
            WpfTestHelper.ShowModal(selector);
        }

        [Test]
        [TestCase(false, false, false, false, true, 'o', 'o')]
        [TestCase(false, false, false, true, false, ' ', ' ')]
        [TestCase(false, false, true, false, false, ' ', ';')]
        [TestCase(false, true, false, false, false, ' ', ',')]
        [TestCase(true, false, false, false, false, ' ', '\t')]
        public void OnUpdateDelimeter_Sets_SelectedDelimeter_FromCheckedBool(bool tab, bool comma, bool semicolon, bool space, bool other, char otherValue, char expectedDelimeter)
        {
            var viewModel = new DelimeterSelectorViewModel();
            viewModel.TabChecked = tab;
            viewModel.CommaChecked = comma;
            viewModel.SemicolonChecked = semicolon;
            viewModel.SpaceChecked = space;
            viewModel.OtherChecked = other;
            viewModel.OtherValue = otherValue;

            viewModel.OnUpdateDelimeter.Execute(null);

            Assert.AreEqual(expectedDelimeter, viewModel.SelectedDelimeter);
        }

        [Test]
        public void OnSetCommaDelimeter_Updates_Checked()
        {
            var viewModel = new DelimeterSelectorViewModel();

            viewModel.SelectedDelimeter = ',';
            viewModel.OnSetOptionChecked.Execute(null);

            Assert.IsFalse(viewModel.TabChecked);
            Assert.IsTrue(viewModel.CommaChecked);
            Assert.IsFalse(viewModel.SemicolonChecked);
            Assert.IsFalse(viewModel.SpaceChecked);
            Assert.IsFalse(viewModel.OtherChecked);
        }

        [Test]
        public void OnSetSemicolonDelimeter_Updates_Checked()
        {
            var viewModel = new DelimeterSelectorViewModel();

            viewModel.SelectedDelimeter = ';';
            viewModel.OnSetOptionChecked.Execute(null);

            Assert.IsFalse(viewModel.TabChecked);
            Assert.IsFalse(viewModel.CommaChecked);
            Assert.IsTrue(viewModel.SemicolonChecked);
            Assert.IsFalse(viewModel.SpaceChecked);
            Assert.IsFalse(viewModel.OtherChecked);
        }


        [Test]
        public void OnSetTabDelimeter_Updates_Checked()
        {
            var viewModel = new DelimeterSelectorViewModel();

            viewModel.SelectedDelimeter = '\t';
            viewModel.OnSetOptionChecked.Execute(null);

            Assert.IsTrue(viewModel.TabChecked);
            Assert.IsFalse(viewModel.CommaChecked);
            Assert.IsFalse(viewModel.SemicolonChecked);
            Assert.IsFalse(viewModel.SpaceChecked);
            Assert.IsFalse(viewModel.OtherChecked);
        }


        [Test]
        public void OnSetSpaceDelimeter_Updates_Checked()
        {
            var viewModel = new DelimeterSelectorViewModel();

            viewModel.SelectedDelimeter = ' ';
            viewModel.OnSetOptionChecked.Execute(null);

            Assert.IsFalse(viewModel.TabChecked);
            Assert.IsFalse(viewModel.CommaChecked);
            Assert.IsFalse(viewModel.SemicolonChecked);
            Assert.IsTrue(viewModel.SpaceChecked);
            Assert.IsFalse(viewModel.OtherChecked);
        }

        [Test]
        public void OnSetOtherDelimeter_Updates_Checked()
        {
            var viewModel = new DelimeterSelectorViewModel();

            viewModel.SelectedDelimeter = 'o';
            viewModel.OnSetOptionChecked.Execute(null);

            Assert.IsFalse(viewModel.TabChecked);
            Assert.IsFalse(viewModel.CommaChecked);
            Assert.IsFalse(viewModel.SemicolonChecked);
            Assert.IsFalse(viewModel.SpaceChecked);
            Assert.IsTrue(viewModel.OtherChecked);
        }
    }
}
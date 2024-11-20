using System.ComponentModel;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView.StandardCrossSections
{
    [TestFixture]
    public class CrossSectionStandardDataViewTest
    {
        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void Show()
        {
            var data = new CrossSectionDefinitionStandard() {LevelShift = 8.01};

            var view = new CrossSectionStandardDataView
                           {
                               Data = new CrossSectionDefinitionStandardViewModel(){Definition = data, IsOnChannel = true}
                           };

            ((INotifyPropertyChanged)data).PropertyChanged += (s,e)=>view.RefreshView();

            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}
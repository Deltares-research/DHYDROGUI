using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.PropertyBag.Dynamic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Concepts
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    class SacramentoDataPropertiesTest
    {
        [Test]
        public void ShowEmpty()
        {
            WindowsFormsTestHelper.ShowModal(new PropertyGrid
                {
                    SelectedObject = new DynamicPropertyBag(new SacramentoDataProperties
                        {
                            Data = new SacramentoData(new Catchment())
                        })
                });
        }
    }
}

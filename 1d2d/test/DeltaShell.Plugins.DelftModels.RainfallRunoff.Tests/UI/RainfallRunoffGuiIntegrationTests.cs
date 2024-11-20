using System.Threading;
using System.Windows.Controls;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class RainfallRunoffGuiIntegrationTests
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ShowWithGui()
        {
            using (var gui = RainfallRunoffIntegrationTestHelper.GetRunningGuiWithRRPlugins())
            {
                var model = new RainfallRunoffModel {Name = "model1"};
                model.Basin.Catchments.Add(new Catchment { Name = "Catchment001" });
                gui.Application.ProjectService.Project.RootFolder.Add(model);

                WpfTestHelper.ShowModal((Control) gui.MainWindow);
            }
        }
    }
}

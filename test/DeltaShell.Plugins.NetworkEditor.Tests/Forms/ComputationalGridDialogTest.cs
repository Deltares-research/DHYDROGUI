using System.Threading;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ComputationalGridDialogTest
    {
        private static double CGWMinimumCellLength = 0.5;
        private static bool CGWGridAtStructure = true;
        private static double CGWStructureDistance = 10.0;
        private static bool CGWGridAtCrossSection = true;
        private static bool CGWUseFixedLength = false;
        private static double CGWFixedLength = 100;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }


        ///<summary>
        ///Show cross section view with some default data
        ///</summary>
        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void ShowComputationalGridDialog()
        {
            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                var app = gui.Application;
                app.UserSettings["autosaveWindowLayout"] = false;
                var networkEditorPlugin = new NetworkEditorApplicationPlugin();
                app.Plugins.Add(networkEditorPlugin);
            
                gui.Run();

                app.CreateNewProject(); 
                
                IDiscretization defaultDiscretization = null;
                var network = new HydroNetwork();
                var channel = new Channel();

                network.Branches.Add(channel);

                app.Project.RootFolder.Add(network);

                var calculationGridWizard = new ComputationalGridDialog
                    {
                        HydroNetworks = { network },
                        UpdateDiscretization = defaultDiscretization,
                        MinimumCellLength = CGWMinimumCellLength,
                        GridAtStructure = CGWGridAtStructure,
                        StructureDistance = CGWStructureDistance,
                        GridAtCrossSection = CGWGridAtCrossSection,
                        UseFixedLength = CGWUseFixedLength,
                        FixedLength = CGWFixedLength,

                        // Don't use opacity during testing 
                        // see https://stackoverflow.com/questions/31835378/the-system-cannot-find-message-text-for-message-number-0x1-in-the-message-file
                        UseOpacity = false 
                    };

                WindowsFormsTestHelper.ShowModal(calculationGridWizard);
            }
        }
    }
}

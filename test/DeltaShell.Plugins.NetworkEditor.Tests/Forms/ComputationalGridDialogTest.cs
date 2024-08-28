using System.Threading;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils.Builders;
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

        ///<summary>
        ///Show cross section view with some default data
        ///</summary>
        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void ShowComputationalGridDialog()
        {
            using (var gui = new DHYDROGuiBuilder().Build())
            {
                var app = gui.Application;
                app.UserSettings["autosaveWindowLayout"] = false;
            
                gui.Run();

                Project project = app.ProjectService.CreateProject();
                
                IDiscretization defaultDiscretization = null;
                var network = new HydroNetwork();
                var channel = new Channel();

                network.Branches.Add(channel);

                project.RootFolder.Add(network);

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

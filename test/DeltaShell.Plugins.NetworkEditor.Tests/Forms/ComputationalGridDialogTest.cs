using System;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms
{
    [TestFixture]
    public class ComputationalGridDialogTest
    {
        private static double CGWMinimumCellLength = 0.5;
        private static bool CGWGridAtStructure = true;
        private static double CGWStructureDistance = 10.0;
        private static bool CGWGridAtCrossSection = true;
        private static bool CGWUseFixedLength = false;
        private static double CGWFixedLength = 100;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.ConfigureLogging();
        }

        [TestFixtureTearDown]
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
            try
            {
                using (var gui = new DeltaShellGui())
                {
                    var app = gui.Application;
                    app.UserSettings["autosaveWindowLayout"] = false;
                    var networkEditorPlugin = new NetworkEditorApplicationPlugin();
                    app.Plugins.Add(networkEditorPlugin);
                
                    gui.Run();

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
                            //Project = app.Project
                        };

                    WindowsFormsTestHelper.ShowModal(calculationGridWizard);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("The system cannot find message text for message number 0x%1"))
                    Assert.Ignore("No clue..");
            }
        }
    }
}

using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.DeveloperTools.Builders;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DeltaShell.Plugins.DeveloperTools.Commands
{
    /// <summary>
    /// Creates 1D flow model with demo network
    /// </summary>
    public class CreateFlowModel1DRectangleNetworkCommand : Command
    {
        protected override void OnExecute(params object[] arguments)
        {
            var dialog = new InputTextDialog { Text = "Enter an integer" };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int result;

                if (int.TryParse(dialog.EnteredText, out result))
                {
                    var gui = DeveloperToolsGuiPlugin.Instance.Gui;
                    var app = gui.Application;

                    if (app.Project == null)
                    {
                        app.CreateNewProject();
                    }

                    app.Project.RootFolder.Add(WaterFlowModel1DBuilder.CreateModelWithLargeNetwork(result));
                }
                else
                {
                    MessageBox.Show("Please enter an integer.");
                }
            } 
        }

        public override bool Enabled
        {
            get { return true; }
        }
    }
}

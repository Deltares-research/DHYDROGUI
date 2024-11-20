using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Views;
using SharpMap.Rendering.Thematics;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms
{
    partial class HydroModelSettings
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            AddNewActivityCallback = null;
            Layer = null;
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ModelsTimeSettingsView = new System.Windows.Forms.Integration.ElementHost();
            this.view = new HydroModelTimeSettingsView();
            this.SuspendLayout();
            // 
            // elementHost1
            // 
            this.ModelsTimeSettingsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ModelsTimeSettingsView.Location = new System.Drawing.Point(0, 0);
            this.ModelsTimeSettingsView.Name = "elementHost1";
            this.ModelsTimeSettingsView.Size = new System.Drawing.Size(757, 492);
            this.ModelsTimeSettingsView.TabIndex = 0;
            this.ModelsTimeSettingsView.Text = "elementHost1";
            this.ModelsTimeSettingsView.Child = view;
            // 
            // HydroModelSettings
            // 
            this.Controls.Add(this.ModelsTimeSettingsView);
            this.Name = "HydroModelSettings";
            this.Size = new System.Drawing.Size(757, 492);
            this.ResumeLayout(false);
        }

        #endregion
        
        private System.Windows.Forms.Integration.ElementHost ModelsTimeSettingsView;
        private Views.HydroModelTimeSettingsView view;
    }
}

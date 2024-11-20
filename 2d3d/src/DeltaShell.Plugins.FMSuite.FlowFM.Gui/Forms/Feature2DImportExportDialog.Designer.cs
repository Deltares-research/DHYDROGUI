using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Controls;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    partial class Feature2DImportExportDialog
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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            importFileNames = new string[0];
            
            this.SuspendLayout();
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileName = "saveFileDialog1";
            this.saveFileDialog.Title = "Save file";
            // 
            // FileImportExportDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "Feature2DImportExportDialog";
            this.Text = "Import/export features";
            this.ResumeLayout(false);

        }

        #endregion
        
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private string[] importFileNames;
    }
}
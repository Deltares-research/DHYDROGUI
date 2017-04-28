namespace DeltaShell.Plugins.Fews.Forms
{
    partial class SelectTimeSeriesWizardPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbTimeSeries = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // lbTimeSeries
            // 
            this.lbTimeSeries.FormattingEnabled = true;
            this.lbTimeSeries.Location = new System.Drawing.Point(17, 22);
            this.lbTimeSeries.Name = "lbTimeSeries";
            this.lbTimeSeries.Size = new System.Drawing.Size(244, 199);
            this.lbTimeSeries.TabIndex = 0;
            // 
            // SelectTimeSeriesWizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lbTimeSeries);
            this.Name = "SelectTimeSeriesWizardPage";
            this.Size = new System.Drawing.Size(281, 244);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lbTimeSeries;
    }
}

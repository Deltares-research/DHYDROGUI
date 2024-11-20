namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    partial class AreaDictionaryEditor
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
            this.components = new System.ComponentModel.Container();
            this.itemPanel = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.unitLabel = new System.Windows.Forms.Label();
            this.totalAreaTxt = new System.Windows.Forms.TextBox();
            this.totalAreaLabel = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.bottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // itemPanel
            // 
            this.itemPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemPanel.Location = new System.Drawing.Point(0, 0);
            this.itemPanel.Name = "itemPanel";
            this.itemPanel.Size = new System.Drawing.Size(418, 129);
            this.itemPanel.TabIndex = 0;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.unitLabel);
            this.bottomPanel.Controls.Add(this.totalAreaTxt);
            this.bottomPanel.Controls.Add(this.totalAreaLabel);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 129);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Padding = new System.Windows.Forms.Padding(10);
            this.bottomPanel.Size = new System.Drawing.Size(418, 35);
            this.bottomPanel.TabIndex = 1;
            // 
            // unitLabel
            // 
            this.unitLabel.AutoSize = true;
            this.unitLabel.Location = new System.Drawing.Point(245, 10);
            this.unitLabel.Name = "unitLabel";
            this.unitLabel.Size = new System.Drawing.Size(24, 13);
            this.unitLabel.TabIndex = 2;
            this.unitLabel.Text = "unit";
            // 
            // totalAreaTxt
            // 
            this.totalAreaTxt.Location = new System.Drawing.Point(148, 7);
            this.totalAreaTxt.Name = "totalAreaTxt";
            this.totalAreaTxt.ReadOnly = true;
            this.totalAreaTxt.Size = new System.Drawing.Size(91, 20);
            this.totalAreaTxt.TabIndex = 1;
            // 
            // totalAreaLabel
            // 
            this.totalAreaLabel.AutoSize = true;
            this.totalAreaLabel.Location = new System.Drawing.Point(10, 10);
            this.totalAreaLabel.Name = "totalAreaLabel";
            this.totalAreaLabel.Size = new System.Drawing.Size(55, 13);
            this.totalAreaLabel.TabIndex = 0;
            this.totalAreaLabel.Text = "Total area";
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // AreaDictionaryEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.itemPanel);
            this.Controls.Add(this.bottomPanel);
            this.Name = "AreaDictionaryEditor";
            this.Size = new System.Drawing.Size(418, 164);
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel itemPanel;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Label totalAreaLabel;
        private System.Windows.Forms.TextBox totalAreaTxt;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.Label unitLabel;

    }
}

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor
{
    partial class WaveBoundaryConditionPropertiesControl
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
            this.label3 = new System.Windows.Forms.Label();
            this.uniformityBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Spatial Definition:";
            // 
            // uniformityBox
            // 
            this.uniformityBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.uniformityBox.FormattingEnabled = true;
            this.uniformityBox.Location = new System.Drawing.Point(160, 64);
            this.uniformityBox.Name = "uniformityBox";
            this.uniformityBox.Size = new System.Drawing.Size(121, 21);
            this.uniformityBox.TabIndex = 15;
            // 
            // WaveBoundaryConditionPropertiesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uniformityBox);
            this.Controls.Add(this.label3);
            this.Name = "WaveBoundaryConditionPropertiesControl";
            this.Size = new System.Drawing.Size(329, 133);
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.uniformityBox, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox uniformityBox;
    }
}

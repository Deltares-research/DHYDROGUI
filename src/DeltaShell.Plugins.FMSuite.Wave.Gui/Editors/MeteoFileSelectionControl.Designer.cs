namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    partial class MeteoFileSelectionControl
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
            this.selMeteoBtn = new System.Windows.Forms.Button();
            this.meteoFileBox = new System.Windows.Forms.TextBox();
            this.fileLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // selMeteoBtn
            // 
            this.selMeteoBtn.Location = new System.Drawing.Point(211, 2);
            this.selMeteoBtn.Name = "selMeteoBtn";
            this.selMeteoBtn.Size = new System.Drawing.Size(30, 23);
            this.selMeteoBtn.TabIndex = 19;
            this.selMeteoBtn.Text = "...";
            this.selMeteoBtn.UseVisualStyleBackColor = true;
            this.selMeteoBtn.Click += new System.EventHandler(this.selMeteoBtn_Click);
            // 
            // meteoFileBox
            // 
            this.meteoFileBox.Location = new System.Drawing.Point(101, 4);
            this.meteoFileBox.Name = "meteoFileBox";
            this.meteoFileBox.ReadOnly = true;
            this.meteoFileBox.Size = new System.Drawing.Size(104, 20);
            this.meteoFileBox.TabIndex = 17;
            // 
            // fileLabel
            // 
            this.fileLabel.AutoSize = true;
            this.fileLabel.Location = new System.Drawing.Point(-2, 7);
            this.fileLabel.Name = "fileLabel";
            this.fileLabel.Size = new System.Drawing.Size(29, 13);
            this.fileLabel.TabIndex = 18;
            this.fileLabel.Text = "label";
            // 
            // MeteoFileSelectionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.selMeteoBtn);
            this.Controls.Add(this.meteoFileBox);
            this.Controls.Add(this.fileLabel);
            this.Margin = new System.Windows.Forms.Padding(2, 3, 3, 3);
            this.Name = "MeteoFileSelectionControl";
            this.Size = new System.Drawing.Size(249, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button selMeteoBtn;
        private System.Windows.Forms.TextBox meteoFileBox;
        private System.Windows.Forms.Label fileLabel;
    }
}

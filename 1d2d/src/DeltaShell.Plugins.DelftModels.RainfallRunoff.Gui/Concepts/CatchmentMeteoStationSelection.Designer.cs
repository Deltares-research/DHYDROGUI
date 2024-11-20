namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    partial class CatchmentMeteoStationSelection
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
            this.label1 = new System.Windows.Forms.Label();
            this.stationComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtAreaAdjustmentFactor = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Meteo station name";
            // 
            // stationComboBox
            // 
            this.stationComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.stationComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.stationComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.stationComboBox.FormattingEnabled = true;
            this.stationComboBox.Location = new System.Drawing.Point(133, 7);
            this.stationComboBox.Name = "stationComboBox";
            this.stationComboBox.Size = new System.Drawing.Size(121, 21);
            this.stationComboBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Area adjustment factor";
            // 
            // txtAreaAdjustmentFactor
            // 
            this.txtAreaAdjustmentFactor.Location = new System.Drawing.Point(133, 41);
            this.txtAreaAdjustmentFactor.Name = "txtAreaAdjustmentFactor";
            this.txtAreaAdjustmentFactor.Size = new System.Drawing.Size(68, 20);
            this.txtAreaAdjustmentFactor.TabIndex = 3;
            this.txtAreaAdjustmentFactor.Validated += new System.EventHandler(this.TxtAreaAdjustmentFactorValidated);
            // 
            // CatchmentMeteoStationSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtAreaAdjustmentFactor);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.stationComboBox);
            this.Controls.Add(this.label1);
            this.Name = "CatchmentMeteoStationSelection";
            this.Size = new System.Drawing.Size(262, 74);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox stationComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAreaAdjustmentFactor;
    }
}

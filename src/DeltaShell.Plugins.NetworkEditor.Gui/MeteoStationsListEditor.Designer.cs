namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    partial class MeteoStationsListEditor
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
            this.btnRemoveStation = new System.Windows.Forms.Button();
            this.btnAddStation = new System.Windows.Forms.Button();
            this.txtNewStationName = new System.Windows.Forms.TextBox();
            this.stationsList = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnRemoveStation
            // 
            this.btnRemoveStation.Location = new System.Drawing.Point(12, 262);
            this.btnRemoveStation.Name = "btnRemoveStation";
            this.btnRemoveStation.Size = new System.Drawing.Size(148, 20);
            this.btnRemoveStation.TabIndex = 1;
            this.btnRemoveStation.Text = "Remove";
            this.btnRemoveStation.UseVisualStyleBackColor = true;
            this.btnRemoveStation.Click += new System.EventHandler(this.BtnRemoveStationClick);
            // 
            // btnAddStation
            // 
            this.btnAddStation.Location = new System.Drawing.Point(109, 43);
            this.btnAddStation.Name = "btnAddStation";
            this.btnAddStation.Size = new System.Drawing.Size(51, 20);
            this.btnAddStation.TabIndex = 2;
            this.btnAddStation.Text = "Add";
            this.btnAddStation.UseVisualStyleBackColor = true;
            this.btnAddStation.Click += new System.EventHandler(this.BtnAddStationClick);
            // 
            // txtNewStationName
            // 
            this.txtNewStationName.Location = new System.Drawing.Point(12, 44);
            this.txtNewStationName.Name = "txtNewStationName";
            this.txtNewStationName.Size = new System.Drawing.Size(91, 20);
            this.txtNewStationName.TabIndex = 3;
            // 
            // stationsList
            // 
            this.stationsList.FormattingEnabled = true;
            this.stationsList.Location = new System.Drawing.Point(12, 70);
            this.stationsList.Name = "stationsList";
            this.stationsList.Size = new System.Drawing.Size(148, 186);
            this.stationsList.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 32);
            this.label1.TabIndex = 5;
            this.label1.Text = "Add / remove meteo stations here";
            // 
            // MeteoStationsList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnRemoveStation);
            this.Controls.Add(this.btnAddStation);
            this.Controls.Add(this.txtNewStationName);
            this.Controls.Add(this.stationsList);
            this.Name = "MeteoStationsListEditor";
            this.Size = new System.Drawing.Size(172, 291);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRemoveStation;
        private System.Windows.Forms.Button btnAddStation;
        private System.Windows.Forms.TextBox txtNewStationName;
        private System.Windows.Forms.ListBox stationsList;
        private System.Windows.Forms.Label label1;

    }
}

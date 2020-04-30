namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    partial class WaveTimePointEditor
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
            this.configPanel = new System.Windows.Forms.Panel();
            this.windComboBox = new System.Windows.Forms.ComboBox();
            this.hydroComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.windGroupBox = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.windDirectionBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.windSpeedBox = new System.Windows.Forms.TextBox();
            this.hydroGroupBox = new System.Windows.Forms.GroupBox();
            this.velocityYBox = new System.Windows.Forms.TextBox();
            this.velocityXBox = new System.Windows.Forms.TextBox();
            this.waterlevelBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.meteoBox = new System.Windows.Forms.GroupBox();
            this.waveMeteoDataEditor1 = new DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.WaveMeteoDataEditor();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tablePanel = new System.Windows.Forms.Panel();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.configPanel.SuspendLayout();
            this.windGroupBox.SuspendLayout();
            this.hydroGroupBox.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.meteoBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // configPanel
            // 
            this.configPanel.Controls.Add(this.windComboBox);
            this.configPanel.Controls.Add(this.hydroComboBox);
            this.configPanel.Controls.Add(this.label2);
            this.configPanel.Controls.Add(this.label1);
            this.configPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.configPanel.Location = new System.Drawing.Point(0, 0);
            this.configPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.configPanel.Name = "configPanel";
            this.configPanel.Size = new System.Drawing.Size(1254, 66);
            this.configPanel.TabIndex = 1;
            // 
            // windComboBox
            // 
            this.windComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.windComboBox.FormattingEnabled = true;
            this.windComboBox.Location = new System.Drawing.Point(484, 15);
            this.windComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.windComboBox.Name = "windComboBox";
            this.windComboBox.Size = new System.Drawing.Size(180, 28);
            this.windComboBox.TabIndex = 3;
            // 
            // hydroComboBox
            // 
            this.hydroComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.hydroComboBox.FormattingEnabled = true;
            this.hydroComboBox.Location = new System.Drawing.Point(164, 15);
            this.hydroComboBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.hydroComboBox.Name = "hydroComboBox";
            this.hydroComboBox.Size = new System.Drawing.Size(180, 28);
            this.hydroComboBox.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(405, 20);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Wind:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Hydrodynamics:";
            // 
            // windGroupBox
            // 
            this.windGroupBox.Controls.Add(this.label7);
            this.windGroupBox.Controls.Add(this.windDirectionBox);
            this.windGroupBox.Controls.Add(this.label8);
            this.windGroupBox.Controls.Add(this.windSpeedBox);
            this.windGroupBox.Location = new System.Drawing.Point(4, 210);
            this.windGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.windGroupBox.Name = "windGroupBox";
            this.windGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.windGroupBox.Size = new System.Drawing.Size(405, 158);
            this.windGroupBox.TabIndex = 1;
            this.windGroupBox.TabStop = false;
            this.windGroupBox.Text = "Wind (constant values)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(27, 95);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 20);
            this.label7.TabIndex = 11;
            this.label7.Text = "Direction:";
            // 
            // windDirectionBox
            // 
            this.windDirectionBox.Location = new System.Drawing.Point(189, 91);
            this.windDirectionBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.windDirectionBox.Name = "windDirectionBox";
            this.windDirectionBox.Size = new System.Drawing.Size(148, 26);
            this.windDirectionBox.TabIndex = 14;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(27, 45);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 20);
            this.label8.TabIndex = 10;
            this.label8.Text = "Speed:";
            // 
            // windSpeedBox
            // 
            this.windSpeedBox.Location = new System.Drawing.Point(189, 40);
            this.windSpeedBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.windSpeedBox.Name = "windSpeedBox";
            this.windSpeedBox.Size = new System.Drawing.Size(148, 26);
            this.windSpeedBox.TabIndex = 13;
            // 
            // hydroGroupBox
            // 
            this.hydroGroupBox.Controls.Add(this.velocityYBox);
            this.hydroGroupBox.Controls.Add(this.velocityXBox);
            this.hydroGroupBox.Controls.Add(this.waterlevelBox);
            this.hydroGroupBox.Controls.Add(this.label5);
            this.hydroGroupBox.Controls.Add(this.label4);
            this.hydroGroupBox.Controls.Add(this.label3);
            this.hydroGroupBox.Location = new System.Drawing.Point(4, 5);
            this.hydroGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.hydroGroupBox.Name = "hydroGroupBox";
            this.hydroGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.hydroGroupBox.Size = new System.Drawing.Size(405, 195);
            this.hydroGroupBox.TabIndex = 0;
            this.hydroGroupBox.TabStop = false;
            this.hydroGroupBox.Text = "Hydrodynamics (constant values)";
            // 
            // velocityYBox
            // 
            this.velocityYBox.Location = new System.Drawing.Point(189, 145);
            this.velocityYBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.velocityYBox.Name = "velocityYBox";
            this.velocityYBox.Size = new System.Drawing.Size(148, 26);
            this.velocityYBox.TabIndex = 9;
            // 
            // velocityXBox
            // 
            this.velocityXBox.Location = new System.Drawing.Point(189, 91);
            this.velocityXBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.velocityXBox.Name = "velocityXBox";
            this.velocityXBox.Size = new System.Drawing.Size(148, 26);
            this.velocityXBox.TabIndex = 8;
            // 
            // waterlevelBox
            // 
            this.waterlevelBox.Location = new System.Drawing.Point(189, 40);
            this.waterlevelBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.waterlevelBox.Name = "waterlevelBox";
            this.waterlevelBox.Size = new System.Drawing.Size(148, 26);
            this.waterlevelBox.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(27, 149);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 20);
            this.label5.TabIndex = 6;
            this.label5.Text = "Velocity Y:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 95);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 20);
            this.label4.TabIndex = 5;
            this.label4.Text = "Velocity X:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 45);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Water Level:";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.hydroGroupBox);
            this.flowLayoutPanel1.Controls.Add(this.windGroupBox);
            this.flowLayoutPanel1.Controls.Add(this.meteoBox);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(420, 683);
            this.flowLayoutPanel1.TabIndex = 0;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // meteoBox
            // 
            this.meteoBox.Controls.Add(this.waveMeteoDataEditor1);
            this.meteoBox.Location = new System.Drawing.Point(4, 378);
            this.meteoBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.meteoBox.Name = "meteoBox";
            this.meteoBox.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.meteoBox.Size = new System.Drawing.Size(405, 297);
            this.meteoBox.TabIndex = 2;
            this.meteoBox.TabStop = false;
            this.meteoBox.Text = "Wind Files";
            // 
            // waveMeteoDataEditor1
            // 
            this.waveMeteoDataEditor1.Data = null;
            this.waveMeteoDataEditor1.Location = new System.Drawing.Point(32, 34);
            this.waveMeteoDataEditor1.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.waveMeteoDataEditor1.Name = "waveMeteoDataEditor1";
            this.waveMeteoDataEditor1.Size = new System.Drawing.Size(369, 254);
            this.waveMeteoDataEditor1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 66);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tablePanel);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(this.flowLayoutPanel1);
            this.splitContainer1.Panel2MinSize = 0;
            this.splitContainer1.Size = new System.Drawing.Size(1254, 683);
            this.splitContainer1.SplitterDistance = 822;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 2;
            // 
            // tablePanel
            // 
            this.tablePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablePanel.Location = new System.Drawing.Point(0, 0);
            this.tablePanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tablePanel.Name = "tablePanel";
            this.tablePanel.Size = new System.Drawing.Size(822, 683);
            this.tablePanel.TabIndex = 1;
            // 
            // WaveTimePointEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.configPanel);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "WaveTimePointEditor";
            this.Size = new System.Drawing.Size(1254, 749);
            this.configPanel.ResumeLayout(false);
            this.configPanel.PerformLayout();
            this.windGroupBox.ResumeLayout(false);
            this.windGroupBox.PerformLayout();
            this.hydroGroupBox.ResumeLayout(false);
            this.hydroGroupBox.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.meteoBox.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel configPanel;
        private System.Windows.Forms.ComboBox windComboBox;
        private System.Windows.Forms.ComboBox hydroComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox windGroupBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox windDirectionBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox windSpeedBox;
        private System.Windows.Forms.GroupBox hydroGroupBox;
        private System.Windows.Forms.TextBox velocityYBox;
        private System.Windows.Forms.TextBox velocityXBox;
        private System.Windows.Forms.TextBox waterlevelBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Panel tablePanel;
        private System.Windows.Forms.GroupBox meteoBox;
        private WaveMeteoDataEditor waveMeteoDataEditor1;

    }
}

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    partial class WaveDomainEditor
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
            this.nDirBox = new System.Windows.Forms.TextBox();
            this.startDirBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.useDefaultDirSpaceCBox = new System.Windows.Forms.CheckBox();
            this.directionalPanel = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.endDirBox = new System.Windows.Forms.TextBox();
            this.endDirLabel = new System.Windows.Forms.Label();
            this.startDirLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.circleRBtn = new System.Windows.Forms.RadioButton();
            this.sectorRBtn = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.useDefaultFreqSpaceCBox = new System.Windows.Forms.CheckBox();
            this.frequencyPanel = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lowFreqBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.highFreqBox = new System.Windows.Forms.TextBox();
            this.nrOfFreqBox = new System.Windows.Forms.TextBox();
            this.hydroGroupBox = new System.Windows.Forms.GroupBox();
            this.useDefaultHydroCBox = new System.Windows.Forms.CheckBox();
            this.hydroPanel = new System.Windows.Forms.Panel();
            this.windBox = new System.Windows.Forms.ComboBox();
            this.velocityTypeBox = new System.Windows.Forms.ComboBox();
            this.velocityBox = new System.Windows.Forms.ComboBox();
            this.waterlevelBox = new System.Windows.Forms.ComboBox();
            this.bedlevelBox = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.useDefaultMeteoCBox = new System.Windows.Forms.CheckBox();
            this.waveMeteoPanel = new DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.WaveMeteoDataEditor();
            this.groupBox1.SuspendLayout();
            this.directionalPanel.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.frequencyPanel.SuspendLayout();
            this.hydroGroupBox.SuspendLayout();
            this.hydroPanel.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // nDirBox
            // 
            this.nDirBox.Location = new System.Drawing.Point(92, 55);
            this.nDirBox.Name = "nDirBox";
            this.nDirBox.Size = new System.Drawing.Size(76, 20);
            this.nDirBox.TabIndex = 0;
            // 
            // startDirBox
            // 
            this.startDirBox.Location = new System.Drawing.Point(92, 81);
            this.startDirBox.Name = "startDirBox";
            this.startDirBox.Size = new System.Drawing.Size(76, 20);
            this.startDirBox.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.useDefaultDirSpaceCBox);
            this.groupBox1.Controls.Add(this.directionalPanel);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(204, 203);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Directional Space";
            // 
            // useDefaultDirSpaceCBox
            // 
            this.useDefaultDirSpaceCBox.AutoSize = true;
            this.useDefaultDirSpaceCBox.Location = new System.Drawing.Point(12, 26);
            this.useDefaultDirSpaceCBox.Name = "useDefaultDirSpaceCBox";
            this.useDefaultDirSpaceCBox.Size = new System.Drawing.Size(109, 17);
            this.useDefaultDirSpaceCBox.TabIndex = 5;
            this.useDefaultDirSpaceCBox.Text = "use model default";
            this.useDefaultDirSpaceCBox.UseVisualStyleBackColor = true;
            // 
            // directionalPanel
            // 
            this.directionalPanel.Controls.Add(this.label4);
            this.directionalPanel.Controls.Add(this.endDirBox);
            this.directionalPanel.Controls.Add(this.endDirLabel);
            this.directionalPanel.Controls.Add(this.startDirBox);
            this.directionalPanel.Controls.Add(this.startDirLabel);
            this.directionalPanel.Controls.Add(this.nDirBox);
            this.directionalPanel.Controls.Add(this.label1);
            this.directionalPanel.Controls.Add(this.circleRBtn);
            this.directionalPanel.Controls.Add(this.sectorRBtn);
            this.directionalPanel.Location = new System.Drawing.Point(6, 55);
            this.directionalPanel.Name = "directionalPanel";
            this.directionalPanel.Size = new System.Drawing.Size(198, 137);
            this.directionalPanel.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Type:";
            // 
            // endDirBox
            // 
            this.endDirBox.Location = new System.Drawing.Point(92, 107);
            this.endDirBox.Name = "endDirBox";
            this.endDirBox.Size = new System.Drawing.Size(76, 20);
            this.endDirBox.TabIndex = 2;
            // 
            // endDirLabel
            // 
            this.endDirLabel.AutoSize = true;
            this.endDirLabel.Location = new System.Drawing.Point(2, 110);
            this.endDirLabel.Name = "endDirLabel";
            this.endDirLabel.Size = new System.Drawing.Size(72, 13);
            this.endDirLabel.TabIndex = 7;
            this.endDirLabel.Text = "End direction:";
            // 
            // startDirLabel
            // 
            this.startDirLabel.AutoSize = true;
            this.startDirLabel.Location = new System.Drawing.Point(2, 84);
            this.startDirLabel.Name = "startDirLabel";
            this.startDirLabel.Size = new System.Drawing.Size(75, 13);
            this.startDirLabel.TabIndex = 6;
            this.startDirLabel.Text = "Start direction:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Nr. of directions:";
            // 
            // circleRBtn
            // 
            this.circleRBtn.AutoSize = true;
            this.circleRBtn.Location = new System.Drawing.Point(92, 2);
            this.circleRBtn.Name = "circleRBtn";
            this.circleRBtn.Size = new System.Drawing.Size(51, 17);
            this.circleRBtn.TabIndex = 3;
            this.circleRBtn.TabStop = true;
            this.circleRBtn.Text = "Circle";
            this.circleRBtn.UseVisualStyleBackColor = true;
            // 
            // sectorRBtn
            // 
            this.sectorRBtn.AutoSize = true;
            this.sectorRBtn.Location = new System.Drawing.Point(92, 25);
            this.sectorRBtn.Name = "sectorRBtn";
            this.sectorRBtn.Size = new System.Drawing.Size(56, 17);
            this.sectorRBtn.TabIndex = 4;
            this.sectorRBtn.TabStop = true;
            this.sectorRBtn.Text = "Sector";
            this.sectorRBtn.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.useDefaultFreqSpaceCBox);
            this.groupBox2.Controls.Add(this.frequencyPanel);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox2.Location = new System.Drawing.Point(204, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(209, 203);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Frequency Space";
            // 
            // useDefaultFreqSpaceCBox
            // 
            this.useDefaultFreqSpaceCBox.AutoSize = true;
            this.useDefaultFreqSpaceCBox.Location = new System.Drawing.Point(12, 26);
            this.useDefaultFreqSpaceCBox.Name = "useDefaultFreqSpaceCBox";
            this.useDefaultFreqSpaceCBox.Size = new System.Drawing.Size(109, 17);
            this.useDefaultFreqSpaceCBox.TabIndex = 6;
            this.useDefaultFreqSpaceCBox.Text = "use model default";
            this.useDefaultFreqSpaceCBox.UseVisualStyleBackColor = true;
            // 
            // frequencyPanel
            // 
            this.frequencyPanel.Controls.Add(this.label7);
            this.frequencyPanel.Controls.Add(this.label6);
            this.frequencyPanel.Controls.Add(this.lowFreqBox);
            this.frequencyPanel.Controls.Add(this.label5);
            this.frequencyPanel.Controls.Add(this.highFreqBox);
            this.frequencyPanel.Controls.Add(this.nrOfFreqBox);
            this.frequencyPanel.Location = new System.Drawing.Point(6, 55);
            this.frequencyPanel.Name = "frequencyPanel";
            this.frequencyPanel.Size = new System.Drawing.Size(200, 84);
            this.frequencyPanel.TabIndex = 4;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(3, 4);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(94, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "Nr. of frequencies:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 56);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(79, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "End frequency:";
            // 
            // lowFreqBox
            // 
            this.lowFreqBox.Location = new System.Drawing.Point(101, 27);
            this.lowFreqBox.Name = "lowFreqBox";
            this.lowFreqBox.Size = new System.Drawing.Size(76, 20);
            this.lowFreqBox.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(82, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Start frequency:";
            // 
            // highFreqBox
            // 
            this.highFreqBox.Location = new System.Drawing.Point(101, 53);
            this.highFreqBox.Name = "highFreqBox";
            this.highFreqBox.Size = new System.Drawing.Size(76, 20);
            this.highFreqBox.TabIndex = 10;
            // 
            // nrOfFreqBox
            // 
            this.nrOfFreqBox.Location = new System.Drawing.Point(101, 1);
            this.nrOfFreqBox.Name = "nrOfFreqBox";
            this.nrOfFreqBox.Size = new System.Drawing.Size(76, 20);
            this.nrOfFreqBox.TabIndex = 11;
            // 
            // hydroGroupBox
            // 
            this.hydroGroupBox.Controls.Add(this.useDefaultHydroCBox);
            this.hydroGroupBox.Controls.Add(this.hydroPanel);
            this.hydroGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.hydroGroupBox.Location = new System.Drawing.Point(680, 0);
            this.hydroGroupBox.Name = "hydroGroupBox";
            this.hydroGroupBox.Size = new System.Drawing.Size(239, 203);
            this.hydroGroupBox.TabIndex = 4;
            this.hydroGroupBox.TabStop = false;
            this.hydroGroupBox.Text = "Hydrodynamics from Flow";
            // 
            // useDefaultHydroCBox
            // 
            this.useDefaultHydroCBox.AutoSize = true;
            this.useDefaultHydroCBox.Location = new System.Drawing.Point(13, 26);
            this.useDefaultHydroCBox.Name = "useDefaultHydroCBox";
            this.useDefaultHydroCBox.Size = new System.Drawing.Size(109, 17);
            this.useDefaultHydroCBox.TabIndex = 7;
            this.useDefaultHydroCBox.Text = "use model default";
            this.useDefaultHydroCBox.UseVisualStyleBackColor = true;
            // 
            // hydroPanel
            // 
            this.hydroPanel.Controls.Add(this.windBox);
            this.hydroPanel.Controls.Add(this.velocityTypeBox);
            this.hydroPanel.Controls.Add(this.velocityBox);
            this.hydroPanel.Controls.Add(this.waterlevelBox);
            this.hydroPanel.Controls.Add(this.bedlevelBox);
            this.hydroPanel.Controls.Add(this.label10);
            this.hydroPanel.Controls.Add(this.label9);
            this.hydroPanel.Controls.Add(this.label8);
            this.hydroPanel.Controls.Add(this.label3);
            this.hydroPanel.Controls.Add(this.label2);
            this.hydroPanel.Location = new System.Drawing.Point(6, 55);
            this.hydroPanel.Name = "hydroPanel";
            this.hydroPanel.Size = new System.Drawing.Size(225, 140);
            this.hydroPanel.TabIndex = 8;
            // 
            // windBox
            // 
            this.windBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.windBox.FormattingEnabled = true;
            this.windBox.Location = new System.Drawing.Point(96, 106);
            this.windBox.Name = "windBox";
            this.windBox.Size = new System.Drawing.Size(108, 21);
            this.windBox.TabIndex = 23;
            // 
            // velocityTypeBox
            // 
            this.velocityTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.velocityTypeBox.FormattingEnabled = true;
            this.velocityTypeBox.Location = new System.Drawing.Point(96, 80);
            this.velocityTypeBox.Name = "velocityTypeBox";
            this.velocityTypeBox.Size = new System.Drawing.Size(108, 21);
            this.velocityTypeBox.TabIndex = 22;
            // 
            // velocityBox
            // 
            this.velocityBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.velocityBox.FormattingEnabled = true;
            this.velocityBox.Location = new System.Drawing.Point(96, 53);
            this.velocityBox.Name = "velocityBox";
            this.velocityBox.Size = new System.Drawing.Size(108, 21);
            this.velocityBox.TabIndex = 21;
            // 
            // waterlevelBox
            // 
            this.waterlevelBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.waterlevelBox.FormattingEnabled = true;
            this.waterlevelBox.Location = new System.Drawing.Point(96, 27);
            this.waterlevelBox.Name = "waterlevelBox";
            this.waterlevelBox.Size = new System.Drawing.Size(108, 21);
            this.waterlevelBox.TabIndex = 20;
            // 
            // bedlevelBox
            // 
            this.bedlevelBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.bedlevelBox.FormattingEnabled = true;
            this.bedlevelBox.Location = new System.Drawing.Point(96, 1);
            this.bedlevelBox.Name = "bedlevelBox";
            this.bedlevelBox.Size = new System.Drawing.Size(108, 21);
            this.bedlevelBox.TabIndex = 19;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 109);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 13);
            this.label10.TabIndex = 18;
            this.label10.Text = "Wind:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 83);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(70, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "Velocity type:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(3, 58);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(47, 13);
            this.label8.TabIndex = 16;
            this.label8.Text = "Velocity:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Water level:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Bed level:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.waveMeteoPanel);
            this.groupBox3.Controls.Add(this.useDefaultMeteoCBox);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox3.Location = new System.Drawing.Point(413, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(267, 203);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Wind";
            // 
            // useDefaultMeteoCBox
            // 
            this.useDefaultMeteoCBox.AutoSize = true;
            this.useDefaultMeteoCBox.Location = new System.Drawing.Point(15, 26);
            this.useDefaultMeteoCBox.Name = "useDefaultMeteoCBox";
            this.useDefaultMeteoCBox.Size = new System.Drawing.Size(109, 17);
            this.useDefaultMeteoCBox.TabIndex = 9;
            this.useDefaultMeteoCBox.Text = "use model default";
            this.useDefaultMeteoCBox.UseVisualStyleBackColor = true;
            // 
            // waveMeteoPanel
            // 
            this.waveMeteoPanel.Data = null;
            this.waveMeteoPanel.Location = new System.Drawing.Point(7, 55);
            this.waveMeteoPanel.Name = "waveMeteoPanel";
            this.waveMeteoPanel.Size = new System.Drawing.Size(254, 142);
            this.waveMeteoPanel.TabIndex = 10;
            // 
            // WaveDomainEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.hydroGroupBox);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "WaveDomainEditor";
            this.Size = new System.Drawing.Size(919, 203);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.directionalPanel.ResumeLayout(false);
            this.directionalPanel.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.frequencyPanel.ResumeLayout(false);
            this.frequencyPanel.PerformLayout();
            this.hydroGroupBox.ResumeLayout(false);
            this.hydroGroupBox.PerformLayout();
            this.hydroPanel.ResumeLayout(false);
            this.hydroPanel.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox nDirBox;
        private System.Windows.Forms.TextBox startDirBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox endDirBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label endDirLabel;
        private System.Windows.Forms.Label startDirLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton sectorRBtn;
        private System.Windows.Forms.RadioButton circleRBtn;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox nrOfFreqBox;
        private System.Windows.Forms.TextBox highFreqBox;
        private System.Windows.Forms.TextBox lowFreqBox;
        private System.Windows.Forms.Panel directionalPanel;
        private System.Windows.Forms.Panel frequencyPanel;
        private System.Windows.Forms.GroupBox hydroGroupBox;
        private System.Windows.Forms.Panel hydroPanel;
        private System.Windows.Forms.ComboBox windBox;
        private System.Windows.Forms.ComboBox velocityTypeBox;
        private System.Windows.Forms.ComboBox velocityBox;
        private System.Windows.Forms.ComboBox waterlevelBox;
        private System.Windows.Forms.ComboBox bedlevelBox;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox useDefaultDirSpaceCBox;
        private System.Windows.Forms.CheckBox useDefaultFreqSpaceCBox;
        private System.Windows.Forms.CheckBox useDefaultHydroCBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private WaveMeteoDataEditor waveMeteoPanel;
        private System.Windows.Forms.CheckBox useDefaultMeteoCBox;
    }
}

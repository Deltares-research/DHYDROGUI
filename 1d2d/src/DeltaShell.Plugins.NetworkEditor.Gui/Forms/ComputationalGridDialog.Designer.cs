namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    partial class ComputationalGridDialog
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
            this.components = new System.ComponentModel.Container();
            this.groupBoxSelection = new System.Windows.Forms.GroupBox();
            this.radioSelectedBranches = new System.Windows.Forms.RadioButton();
            this.radioAllBranches = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioLeave = new System.Windows.Forms.RadioButton();
            this.radioOverwrite = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxNone = new System.Windows.Forms.CheckBox();
            this.checkBoxPreferred = new System.Windows.Forms.CheckBox();
            this.textBoxPreferredLength = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxMinimumDistance = new System.Windows.Forms.TextBox();
            this.textBoxStructureDistance = new System.Windows.Forms.TextBox();
            this.checkBoxStructure = new System.Windows.Forms.CheckBox();
            this.checkBoxLaterals = new System.Windows.Forms.CheckBox();
            this.checkBoxCrossSection = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBoxSelection.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxSelection
            // 
            this.groupBoxSelection.Controls.Add(this.radioSelectedBranches);
            this.groupBoxSelection.Controls.Add(this.radioAllBranches);
            this.groupBoxSelection.Location = new System.Drawing.Point(12, 4);
            this.groupBoxSelection.Name = "groupBoxSelection";
            this.groupBoxSelection.Size = new System.Drawing.Size(329, 65);
            this.groupBoxSelection.TabIndex = 0;
            this.groupBoxSelection.TabStop = false;
            this.groupBoxSelection.Text = "Create segments for:";
            // 
            // radioSelectedBranches
            // 
            this.radioSelectedBranches.AutoSize = true;
            this.radioSelectedBranches.Location = new System.Drawing.Point(6, 42);
            this.radioSelectedBranches.Name = "radioSelectedBranches";
            this.radioSelectedBranches.Size = new System.Drawing.Size(114, 17);
            this.radioSelectedBranches.TabIndex = 1;
            this.radioSelectedBranches.TabStop = true;
            this.radioSelectedBranches.Text = "&Selected branches";
            this.radioSelectedBranches.UseVisualStyleBackColor = true;
            // 
            // radioAllBranches
            // 
            this.radioAllBranches.AutoSize = true;
            this.radioAllBranches.Location = new System.Drawing.Point(6, 19);
            this.radioAllBranches.Name = "radioAllBranches";
            this.radioAllBranches.Size = new System.Drawing.Size(83, 17);
            this.radioAllBranches.TabIndex = 0;
            this.radioAllBranches.TabStop = true;
            this.radioAllBranches.Text = "&All branches";
            this.radioAllBranches.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioLeave);
            this.groupBox2.Controls.Add(this.radioOverwrite);
            this.groupBox2.Location = new System.Drawing.Point(12, 95);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(329, 88);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "If branch already has grid points";
            // 
            // radioLeave
            // 
            this.radioLeave.AutoSize = true;
            this.radioLeave.Location = new System.Drawing.Point(15, 52);
            this.radioLeave.Name = "radioLeave";
            this.radioLeave.Size = new System.Drawing.Size(133, 17);
            this.radioLeave.TabIndex = 1;
            this.radioLeave.TabStop = true;
            this.radioLeave.Text = "&Use existing grid points";
            this.radioLeave.UseVisualStyleBackColor = true;
            // 
            // radioOverwrite
            // 
            this.radioOverwrite.AutoSize = true;
            this.radioOverwrite.Location = new System.Drawing.Point(15, 29);
            this.radioOverwrite.Name = "radioOverwrite";
            this.radioOverwrite.Size = new System.Drawing.Size(143, 17);
            this.radioOverwrite.TabIndex = 0;
            this.radioOverwrite.TabStop = true;
            this.radioOverwrite.Text = "&Generate new grid points";
            this.radioOverwrite.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.checkBoxNone);
            this.groupBox3.Controls.Add(this.checkBoxPreferred);
            this.groupBox3.Controls.Add(this.textBoxPreferredLength);
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Location = new System.Drawing.Point(13, 191);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(412, 207);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Positions:";
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // checkBoxNone
            // 
            this.checkBoxNone.AutoSize = true;
            this.checkBoxNone.Location = new System.Drawing.Point(23, 23);
            this.checkBoxNone.Name = "checkBoxNone";
            this.checkBoxNone.Size = new System.Drawing.Size(180, 17);
            this.checkBoxNone.TabIndex = 0;
            this.checkBoxNone.Text = "&None (Remove grid from branch)";
            this.checkBoxNone.UseVisualStyleBackColor = true;
            this.checkBoxNone.CheckedChanged += new System.EventHandler(this.checkBoxNone_CheckedChanged);
            // 
            // checkBoxPreferred
            // 
            this.checkBoxPreferred.AutoSize = true;
            this.checkBoxPreferred.Location = new System.Drawing.Point(23, 47);
            this.checkBoxPreferred.Name = "checkBoxPreferred";
            this.checkBoxPreferred.Size = new System.Drawing.Size(104, 17);
            this.checkBoxPreferred.TabIndex = 1;
            this.checkBoxPreferred.Text = "&Preferred length:";
            this.checkBoxPreferred.UseVisualStyleBackColor = true;
            this.checkBoxPreferred.CheckedChanged += new System.EventHandler(this.checkBoxFixed_CheckedChanged);
            // 
            // textBoxPreferredLength
            // 
            this.textBoxPreferredLength.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPreferredLength.Location = new System.Drawing.Point(159, 45);
            this.textBoxPreferredLength.Name = "textBoxPreferredLength";
            this.textBoxPreferredLength.Size = new System.Drawing.Size(108, 20);
            this.textBoxPreferredLength.TabIndex = 2;
            this.textBoxPreferredLength.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxFixed_Validating);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.textBoxMinimumDistance);
            this.groupBox4.Controls.Add(this.textBoxStructureDistance);
            this.groupBox4.Controls.Add(this.checkBoxStructure);
            this.groupBox4.Controls.Add(this.checkBoxLaterals);
            this.groupBox4.Controls.Add(this.checkBoxCrossSection);
            this.groupBox4.Location = new System.Drawing.Point(10, 79);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(396, 122);
            this.groupBox4.TabIndex = 4;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Special locations:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(102, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Minimum &cell length:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(266, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "m. in front and behind";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(266, 96);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(18, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "m.";
            // 
            // textBoxMinimumDistance
            // 
            this.textBoxMinimumDistance.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMinimumDistance.Location = new System.Drawing.Point(149, 93);
            this.textBoxMinimumDistance.Name = "textBoxMinimumDistance";
            this.textBoxMinimumDistance.Size = new System.Drawing.Size(108, 20);
            this.textBoxMinimumDistance.TabIndex = 3;
            this.textBoxMinimumDistance.Validating += new System.ComponentModel.CancelEventHandler(this.tbStructureAfter_Validating);
            // 
            // textBoxStructureDistance
            // 
            this.textBoxStructureDistance.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStructureDistance.Location = new System.Drawing.Point(149, 67);
            this.textBoxStructureDistance.Name = "textBoxStructureDistance";
            this.textBoxStructureDistance.Size = new System.Drawing.Size(108, 20);
            this.textBoxStructureDistance.TabIndex = 2;
            this.textBoxStructureDistance.Validating += new System.ComponentModel.CancelEventHandler(this.tbStructureBefore_Validating);
            // 
            // checkBoxStructure
            // 
            this.checkBoxStructure.AutoSize = true;
            this.checkBoxStructure.Enabled = false;
            this.checkBoxStructure.Location = new System.Drawing.Point(23, 69);
            this.checkBoxStructure.Name = "checkBoxStructure";
            this.checkBoxStructure.Size = new System.Drawing.Size(69, 17);
            this.checkBoxStructure.TabIndex = 1;
            this.checkBoxStructure.Text = "S&tructure";
            this.checkBoxStructure.UseVisualStyleBackColor = true;
            this.checkBoxStructure.CheckedChanged += new System.EventHandler(this.checkBoxStructure_CheckedChanged);
            // 
            // checkBoxLaterals
            // 
            this.checkBoxLaterals.AutoSize = true;
            this.checkBoxLaterals.Location = new System.Drawing.Point(23, 46);
            this.checkBoxLaterals.Name = "checkBoxLaterals";
            this.checkBoxLaterals.Size = new System.Drawing.Size(100, 17);
            this.checkBoxLaterals.TabIndex = 0;
            this.checkBoxLaterals.Text = "&Lateral Sources";
            this.checkBoxLaterals.UseVisualStyleBackColor = true;
            // 
            // checkBoxCrossSection
            // 
            this.checkBoxCrossSection.AutoSize = true;
            this.checkBoxCrossSection.Location = new System.Drawing.Point(23, 23);
            this.checkBoxCrossSection.Name = "checkBoxCrossSection";
            this.checkBoxCrossSection.Size = new System.Drawing.Size(91, 17);
            this.checkBoxCrossSection.TabIndex = 0;
            this.checkBoxCrossSection.Text = "&Cross Section";
            this.checkBoxCrossSection.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(276, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(18, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "m.";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(355, 4);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 10;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(355, 33);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // ComputationalGridDialog
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(435, 409);
            this.Controls.Add(this.groupBoxSelection);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ComputationalGridDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generate Computational Grid";
            this.Load += new System.EventHandler(this.CalculationGridWizard_Load);
            this.MouseCaptureChanged += new System.EventHandler(this.CalculationGridWizard_MouseCaptureChanged);
            this.Move += new System.EventHandler(this.CalculationGridWizard_Move);
            this.groupBoxSelection.ResumeLayout(false);
            this.groupBoxSelection.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxSelection;
        private System.Windows.Forms.RadioButton radioSelectedBranches;
        private System.Windows.Forms.RadioButton radioAllBranches;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioLeave;
        private System.Windows.Forms.RadioButton radioOverwrite;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxPreferredLength;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox checkBoxStructure;
        private System.Windows.Forms.CheckBox checkBoxCrossSection;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox checkBoxPreferred;
        private System.Windows.Forms.CheckBox checkBoxNone;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxMinimumDistance;
        private System.Windows.Forms.TextBox textBoxStructureDistance;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBoxLaterals;

    }
}
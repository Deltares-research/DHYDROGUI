namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms
{
    partial class FormPasteBranchFeature
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
            this.labelPaste = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.labelChainage = new System.Windows.Forms.Label();
            this.labelShift = new System.Windows.Forms.Label();
            this.textBoxShift = new System.Windows.Forms.TextBox();
            this.textBoxChainage = new System.Windows.Forms.TextBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelPaste
            // 
            this.labelPaste.AutoSize = true;
            this.labelPaste.Location = new System.Drawing.Point(12, 15);
            this.labelPaste.Name = "labelPaste";
            this.labelPaste.Size = new System.Drawing.Size(106, 13);
            this.labelPaste.TabIndex = 0;
            this.labelPaste.Text = "Paste branch feature";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.CausesValidation = false;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(345, 41);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(345, 12);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 5;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // labelChainage
            // 
            this.labelChainage.AutoSize = true;
            this.labelChainage.Location = new System.Drawing.Point(12, 38);
            this.labelChainage.Name = "labelChainage";
            this.labelChainage.Size = new System.Drawing.Size(52, 13);
            this.labelChainage.TabIndex = 1;
            this.labelChainage.Text = "Chainage";
            // 
            // labelShift
            // 
            this.labelShift.AutoSize = true;
            this.labelShift.Location = new System.Drawing.Point(12, 65);
            this.labelShift.Name = "labelShift";
            this.labelShift.Size = new System.Drawing.Size(176, 13);
            this.labelShift.TabIndex = 3;
            this.labelShift.Text = "Perform Level shift at new chainage";
            this.labelShift.Visible = false;
            // 
            // textBoxShift
            // 
            this.textBoxShift.Enabled = false;
            this.textBoxShift.Location = new System.Drawing.Point(217, 65);
            this.textBoxShift.Name = "textBoxShift";
            this.textBoxShift.Size = new System.Drawing.Size(100, 20);
            this.textBoxShift.TabIndex = 4;
            this.textBoxShift.Visible = false;
            // 
            // textBoxChainage
            // 
            this.textBoxChainage.Location = new System.Drawing.Point(217, 38);
            this.textBoxChainage.Name = "textBoxChainage";
            this.textBoxChainage.Size = new System.Drawing.Size(100, 20);
            this.textBoxChainage.TabIndex = 2;
            this.textBoxChainage.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxChainage_Validating);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // FormPasteBranchFeature
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(433, 100);
            this.ControlBox = false;
            this.Controls.Add(this.textBoxChainage);
            this.Controls.Add(this.textBoxShift);
            this.Controls.Add(this.labelShift);
            this.Controls.Add(this.labelChainage);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.labelPaste);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormPasteBranchFeature";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Paste Branch Feature";
            this.Load += new System.EventHandler(this.FormPasteCrossSection_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelPaste;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Label labelChainage;
        internal System.Windows.Forms.Label labelShift;
        internal System.Windows.Forms.TextBox textBoxShift;
        private System.Windows.Forms.TextBox textBoxChainage;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}
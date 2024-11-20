namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    partial class GriddedWindView
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
            this.WindDataTextBox = new System.Windows.Forms.TextBox();
            this.filePathLabel = new System.Windows.Forms.Label();
            this.fileOpenButton = new System.Windows.Forms.Button();
            this.WindGridTextBox = new System.Windows.Forms.TextBox();
            this.gridFilePathLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // WindDataTextBox
            // 
            this.WindDataTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.WindDataTextBox.Enabled = false;
            this.WindDataTextBox.Location = new System.Drawing.Point(105, 5);
            this.WindDataTextBox.Name = "WindDataTextBox";
            this.WindDataTextBox.Size = new System.Drawing.Size(318, 20);
            this.WindDataTextBox.TabIndex = 0;
            // 
            // filePathLabel
            // 
            this.filePathLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.filePathLabel.AutoSize = true;
            this.filePathLabel.Location = new System.Drawing.Point(3, 9);
            this.filePathLabel.Name = "filePathLabel";
            this.filePathLabel.Size = new System.Drawing.Size(96, 13);
            this.filePathLabel.TabIndex = 1;
            this.filePathLabel.Text = "Wind data file path";
            // 
            // fileOpenButton
            // 
            this.fileOpenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fileOpenButton.Location = new System.Drawing.Point(429, 3);
            this.fileOpenButton.Name = "fileOpenButton";
            this.fileOpenButton.Size = new System.Drawing.Size(75, 23);
            this.fileOpenButton.TabIndex = 2;
            this.fileOpenButton.Text = "Browse...";
            this.fileOpenButton.UseVisualStyleBackColor = true;
            this.fileOpenButton.Click += new System.EventHandler(this.FileOpenButtonClick);
            // 
            // WindGridTextBox
            // 
            this.WindGridTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.WindGridTextBox.Enabled = false;
            this.WindGridTextBox.Location = new System.Drawing.Point(105, 36);
            this.WindGridTextBox.Name = "WindGridTextBox";
            this.WindGridTextBox.Size = new System.Drawing.Size(318, 20);
            this.WindGridTextBox.TabIndex = 3;
            // 
            // gridFilePathLabel
            // 
            this.gridFilePathLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.gridFilePathLabel.AutoSize = true;
            this.gridFilePathLabel.Location = new System.Drawing.Point(3, 40);
            this.gridFilePathLabel.Name = "gridFilePathLabel";
            this.gridFilePathLabel.Size = new System.Drawing.Size(92, 13);
            this.gridFilePathLabel.TabIndex = 4;
            this.gridFilePathLabel.Text = "Wind grid file path";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.filePathLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.fileOpenButton, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.WindGridTextBox, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.gridFilePathLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.WindDataTextBox, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(507, 62);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(513, 159);
            this.panel1.TabIndex = 6;
            // 
            // GriddedWindView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Name = "GriddedWindView";
            this.Size = new System.Drawing.Size(513, 159);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox WindDataTextBox;
        private System.Windows.Forms.Label filePathLabel;
        private System.Windows.Forms.Button fileOpenButton;
        private System.Windows.Forms.TextBox WindGridTextBox;
        private System.Windows.Forms.Label gridFilePathLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
    }
}

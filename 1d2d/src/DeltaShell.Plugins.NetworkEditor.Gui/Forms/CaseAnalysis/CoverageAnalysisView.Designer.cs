using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CaseAnalysis
{
    partial class CoverageAnalysisView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new Container();
            this.selectionPanel = new Panel();
            this.label2 = new Label();
            this.referenceValueWarningBox = new PictureBox();
            this.referenceValueTextBox = new TextBox();
            this.secondaryCoverageWarningBox = new PictureBox();
            this.label1 = new Label();
            this.applyButton = new Button();
            this.comboboxSecondary = new ComboBox();
            this.comboboxOperation = new ComboBox();
            this.comboboxPrimary = new ComboBox();
            this.warningToolTip = new ToolTip(this.components);
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.networkCoveragePanel = new Panel();
            this.selectionPanel.SuspendLayout();
            ((ISupportInitialize)(this.referenceValueWarningBox)).BeginInit();
            ((ISupportInitialize)(this.secondaryCoverageWarningBox)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // selectionPanel
            // 
            this.selectionPanel.AutoScroll = true;
            this.selectionPanel.Controls.Add(this.label2);
            this.selectionPanel.Controls.Add(this.referenceValueWarningBox);
            this.selectionPanel.Controls.Add(this.referenceValueTextBox);
            this.selectionPanel.Controls.Add(this.secondaryCoverageWarningBox);
            this.selectionPanel.Controls.Add(this.label1);
            this.selectionPanel.Controls.Add(this.applyButton);
            this.selectionPanel.Controls.Add(this.comboboxSecondary);
            this.selectionPanel.Controls.Add(this.comboboxOperation);
            this.selectionPanel.Controls.Add(this.comboboxPrimary);
            this.selectionPanel.Dock = DockStyle.Top;
            this.selectionPanel.Location = new Point(3, 3);
            this.selectionPanel.MinimumSize = new Size(620, 90);
            this.selectionPanel.Name = "selectionPanel";
            this.selectionPanel.Size = new Size(644, 90);
            this.selectionPanel.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new Point(326, 66);
            this.label2.Name = "label2";
            this.label2.Size = new Size(37, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Value:";
            // 
            // referenceValueWarningBox
            // 
            this.referenceValueWarningBox.Image = Resources.warning;
            this.referenceValueWarningBox.Location = new Point(600, 63);
            this.referenceValueWarningBox.Name = "referenceValueWarningBox";
            this.referenceValueWarningBox.Size = new Size(16, 16);
            this.referenceValueWarningBox.TabIndex = 6;
            this.referenceValueWarningBox.TabStop = false;
            this.referenceValueWarningBox.Visible = false;
            // 
            // referenceValueTextBox
            // 
            this.referenceValueTextBox.Location = new Point(393, 63);
            this.referenceValueTextBox.Name = "referenceValueTextBox";
            this.referenceValueTextBox.Size = new Size(194, 20);
            this.referenceValueTextBox.TabIndex = 5;
            this.referenceValueTextBox.TextChanged += new EventHandler(this.referenceValueTextBox_TextChanged);
            // 
            // secondaryCoverageWarningBox
            // 
            this.secondaryCoverageWarningBox.Image = Resources.warning;
            this.secondaryCoverageWarningBox.Location = new Point(600, 26);
            this.secondaryCoverageWarningBox.Name = "secondaryCoverageWarningBox";
            this.secondaryCoverageWarningBox.Size = new Size(16, 16);
            this.secondaryCoverageWarningBox.TabIndex = 4;
            this.secondaryCoverageWarningBox.TabStop = false;
            this.secondaryCoverageWarningBox.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new Point(236, 3);
            this.label1.Name = "label1";
            this.label1.Size = new Size(56, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Operation:";
            // 
            // applyButton
            // 
            this.applyButton.Location = new Point(15, 57);
            this.applyButton.Margin = new Padding(15);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new Size(81, 21);
            this.applyButton.TabIndex = 1;
            this.applyButton.Text = "Show";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new EventHandler(this.ApplyButtonClick);
            // 
            // comboboxSecondary
            // 
            this.comboboxSecondary.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboboxSecondary.Enabled = false;
            this.comboboxSecondary.FormattingEnabled = true;
            this.comboboxSecondary.Location = new Point(393, 23);
            this.comboboxSecondary.Margin = new Padding(15);
            this.comboboxSecondary.Name = "comboboxSecondary";
            this.comboboxSecondary.Size = new Size(194, 21);
            this.comboboxSecondary.TabIndex = 0;
            this.comboboxSecondary.SelectedValueChanged += new EventHandler(this.ComboboxesSelectedValueChanged);
            // 
            // comboboxOperation
            // 
            this.comboboxOperation.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboboxOperation.FormattingEnabled = true;
            this.comboboxOperation.Location = new Point(239, 23);
            this.comboboxOperation.Margin = new Padding(15);
            this.comboboxOperation.Name = "comboboxOperation";
            this.comboboxOperation.Size = new Size(124, 21);
            this.comboboxOperation.TabIndex = 0;
            this.comboboxOperation.SelectedValueChanged += new EventHandler(this.ComboboxOperationSelectedValueChanged);
            // 
            // comboboxPrimary
            // 
            this.comboboxPrimary.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboboxPrimary.FormattingEnabled = true;
            this.comboboxPrimary.Location = new Point(15, 23);
            this.comboboxPrimary.Margin = new Padding(15);
            this.comboboxPrimary.Name = "comboboxPrimary";
            this.comboboxPrimary.Size = new Size(194, 21);
            this.comboboxPrimary.TabIndex = 0;
            this.comboboxPrimary.SelectedValueChanged += new EventHandler(this.ComboboxesSelectedValueChanged);
            // 
            // warningToolTip
            // 
            this.warningToolTip.AutoPopDelay = 0;
            this.warningToolTip.InitialDelay = 500;
            this.warningToolTip.ReshowDelay = 100;
            this.warningToolTip.ToolTipIcon = ToolTipIcon.Warning;
            this.warningToolTip.ToolTipTitle = "Warning:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.selectionPanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.networkCoveragePanel, 0, 1);
            this.tableLayoutPanel1.Dock = DockStyle.Fill;
            this.tableLayoutPanel1.Location = new Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new Size(650, 480);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // networkCoveragePanel
            // 
            this.networkCoveragePanel.Dock = DockStyle.Fill;
            this.networkCoveragePanel.Location = new Point(3, 99);
            this.networkCoveragePanel.Name = "networkCoveragePanel";
            this.networkCoveragePanel.Size = new Size(644, 378);
            this.networkCoveragePanel.TabIndex = 0;
            // 
            // CoverageAnalysisView
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoScrollMinSize = new Size(650, 480);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "CoverageAnalysisView";
            this.Size = new Size(650, 480);
            this.selectionPanel.ResumeLayout(false);
            this.selectionPanel.PerformLayout();
            ((ISupportInitialize)(this.referenceValueWarningBox)).EndInit();
            ((ISupportInitialize)(this.secondaryCoverageWarningBox)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel selectionPanel;
        private ComboBox comboboxSecondary;
        private ComboBox comboboxOperation;
        private ComboBox comboboxPrimary;
        private Button applyButton;
        private Label label1;
        private PictureBox secondaryCoverageWarningBox;
        private ToolTip warningToolTip;
        private TextBox referenceValueTextBox;
        private PictureBox referenceValueWarningBox;
        private Label label2;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel networkCoveragePanel;
    }
}

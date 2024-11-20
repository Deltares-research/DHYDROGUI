using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    partial class DepthLayerControl
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
            this.components = new System.ComponentModel.Container();
            this.layersPanel = new System.Windows.Forms.TableLayoutPanel();
            this.picture = new System.Windows.Forms.PictureBox();
            this.layerTypeComboBox = new System.Windows.Forms.ComboBox();
            this.layerCountTextBox = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.layerCountLabel = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picture)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // layersPanel
            // 
            this.layersPanel.ColumnCount = 1;
            this.layersPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layersPanel.Location = new System.Drawing.Point(0, 31);
            this.layersPanel.Name = "layersPanel";
            this.layersPanel.RowCount = 2;
            this.layersPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layersPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layersPanel.Size = new System.Drawing.Size(124, 239);
            this.layersPanel.TabIndex = 1;
            // 
            // picture
            // 
            this.picture.Dock = System.Windows.Forms.DockStyle.Right;
            this.picture.Location = new System.Drawing.Point(124, 31);
            this.picture.Name = "picture";
            this.picture.Size = new System.Drawing.Size(40, 239);
            this.picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picture.TabIndex = 0;
            this.picture.TabStop = false;
            // 
            // layerTypeComboBox
            // 
            this.layerTypeComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.layerTypeComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.layerTypeComboBox.FormattingEnabled = true;
            this.layerTypeComboBox.Location = new System.Drawing.Point(3, 3);
            this.layerTypeComboBox.Name = "layerTypeComboBox";
            this.layerTypeComboBox.Size = new System.Drawing.Size(64, 21);
            this.layerTypeComboBox.TabIndex = 0;
            // 
            // layerCountTextBox
            // 
            this.layerCountTextBox.Location = new System.Drawing.Point(51, 1);
            this.layerCountTextBox.Name = "layerCountTextBox";
            this.layerCountTextBox.Size = new System.Drawing.Size(19, 20);
            this.layerCountTextBox.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.layerTypeComboBox);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(3);
            this.panel1.Size = new System.Drawing.Size(164, 31);
            this.panel1.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.layerCountLabel);
            this.panel2.Controls.Add(this.layerCountTextBox);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(67, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(94, 25);
            this.panel2.TabIndex = 3;
            // 
            // layerCountLabel
            // 
            this.layerCountLabel.AutoSize = true;
            this.layerCountLabel.Location = new System.Drawing.Point(3, 4);
            this.layerCountLabel.Name = "layerCountLabel";
            this.layerCountLabel.Size = new System.Drawing.Size(47, 13);
            this.layerCountLabel.TabIndex = 2;
            this.layerCountLabel.Text = "Number:";
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // DepthLayerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.layersPanel);
            this.Controls.Add(this.picture);
            this.Controls.Add(this.panel1);
            this.Name = "DepthLayerControl";
            this.Size = new System.Drawing.Size(164, 270);
            ((System.ComponentModel.ISupportInitialize)(this.picture)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel layersPanel;
        private ComboBox layerTypeComboBox;
        private TextBox layerCountTextBox;
        private Panel panel1;
        private Label layerCountLabel;
        private ErrorProvider errorProvider;
        private PictureBox picture;
        private int layerCount;
        private IList<double> layerThicknesses;
        private double totalThickness;
        private IList<Color> colors;
        private Panel panel2;
    }
}

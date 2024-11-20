using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Table;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    partial class VerticalProfileControl
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
            this.picture = new System.Windows.Forms.PictureBox();
            this.profileTypeComboBox = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableView = new DelftTools.Controls.Swf.Table.TableView();
            this.panel3 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picture)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableView)).BeginInit();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // picture
            // 
            this.picture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picture.Location = new System.Drawing.Point(0, 0);
            this.picture.Name = "picture";
            this.picture.Padding = new System.Windows.Forms.Padding(5);
            this.picture.Size = new System.Drawing.Size(56, 247);
            this.picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picture.TabIndex = 0;
            this.picture.TabStop = false;
            // 
            // profileTypeComboBox
            // 
            this.profileTypeComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.profileTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.profileTypeComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.profileTypeComboBox.FormattingEnabled = true;
            this.profileTypeComboBox.Location = new System.Drawing.Point(3, 3);
            this.profileTypeComboBox.Name = "profileTypeComboBox";
            this.profileTypeComboBox.Size = new System.Drawing.Size(316, 21);
            this.profileTypeComboBox.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.profileTypeComboBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(3);
            this.panel1.Size = new System.Drawing.Size(322, 31);
            this.panel1.TabIndex = 2;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.tableView);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(56, 31);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(266, 247);
            this.panel2.TabIndex = 3;
            // 
            // tableView
            // 
            this.tableView.AllowAddNewRow = true;
            this.tableView.AllowColumnFiltering = false;
            this.tableView.AllowColumnPinning = false;
            this.tableView.AllowColumnSorting = false;
            this.tableView.AllowDeleteRow = true;
            this.tableView.AutoGenerateColumns = true;
            this.tableView.ColumnAutoWidth = true;
            this.tableView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableView.EditButtons = true;
            this.tableView.HeaderHeigth = -1;
            this.tableView.IncludeHeadersOnCopy = false;
            this.tableView.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableView.IsEndEditOnEnterKey = true;
            this.tableView.Location = new System.Drawing.Point(0, 0);
            this.tableView.MultipleCellEdit = true;
            this.tableView.MultiSelect = true;
            this.tableView.Name = "tableView";
            this.tableView.ReadOnly = false;
            this.tableView.ReadOnlyCellBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.tableView.ReadOnlyCellForeColor = System.Drawing.Color.Black;
            this.tableView.RowHeight = -1;
            this.tableView.RowSelect = false;
            this.tableView.RowValidator = null;
            this.tableView.ShowImportExportToolbar = false;
            this.tableView.ShowRowNumbers = true;
            this.tableView.Size = new System.Drawing.Size(266, 247);
            this.tableView.TabIndex = 0;
            this.tableView.TabStop = false;
            this.tableView.UseCenteredHeaderText = false;
            this.tableView.ViewInfo = null;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.picture);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(0, 31);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(56, 247);
            this.panel3.TabIndex = 4;
            // 
            // VerticalProfileControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Name = "VerticalProfileControl";
            this.Size = new System.Drawing.Size(322, 278);
            ((System.ComponentModel.ISupportInitialize)(this.picture)).EndInit();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableView)).EndInit();
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComboBox profileTypeComboBox;
        private Panel panel1;
        private ErrorProvider errorProvider;
        private PictureBox picture;
        private Panel panel2;
        private TableView tableView;
        private Panel panel3;
    }
}

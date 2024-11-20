using System;

using DelftTools.Controls;
using DelftTools.Utils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.DataSetManager
{
    partial class DataTableManagerView
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBoxSubstanceUseFor = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBoxDataFile = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableView1 = new DelftTools.Controls.Swf.Table.TableView();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableView1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxSubstanceUseFor);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(496, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(7);
            this.groupBox1.Size = new System.Drawing.Size(487, 271);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Substances use for";
            // 
            // textBoxSubstanceUseFor
            // 
            this.textBoxSubstanceUseFor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxSubstanceUseFor.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSubstanceUseFor.Location = new System.Drawing.Point(7, 20);
            this.textBoxSubstanceUseFor.Multiline = true;
            this.textBoxSubstanceUseFor.Name = "textBoxSubstanceUseFor";
            this.textBoxSubstanceUseFor.ReadOnly = true;
            this.textBoxSubstanceUseFor.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxSubstanceUseFor.Size = new System.Drawing.Size(473, 244);
            this.textBoxSubstanceUseFor.TabIndex = 0;
            this.textBoxSubstanceUseFor.WordWrap = false;
            this.textBoxSubstanceUseFor.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxSubstanceUseForValidating);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBoxDataFile);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(7);
            this.groupBox2.Size = new System.Drawing.Size(986, 360);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Data file";
            // 
            // textBoxDataFile
            // 
            this.textBoxDataFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxDataFile.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxDataFile.Location = new System.Drawing.Point(7, 20);
            this.textBoxDataFile.Multiline = true;
            this.textBoxDataFile.Name = "textBoxDataFile";
            this.textBoxDataFile.ReadOnly = true;
            this.textBoxDataFile.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxDataFile.Size = new System.Drawing.Size(972, 333);
            this.textBoxDataFile.TabIndex = 0;
            this.textBoxDataFile.WordWrap = false;
            this.textBoxDataFile.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDataFileValidating);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Size = new System.Drawing.Size(986, 641);
            this.splitContainer1.SplitterDistance = 277;
            this.splitContainer1.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableView1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(986, 277);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // tableView1
            // 
            this.tableView1.AllowAddNewRow = false;
            this.tableView1.AllowColumnFiltering = false;
            this.tableView1.AllowColumnPinning = false;
            this.tableView1.AllowColumnSorting = false;
            this.tableView1.AllowDeleteRow = true;
            this.tableView1.AutoGenerateColumns = false;
            this.tableView1.AutoSizeRows = false;
            this.tableView1.ColumnAutoWidth = false;
            this.tableView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableView1.EditButtons = false;
            this.tableView1.HeaderHeigth = -1;
            this.tableView1.IncludeHeadersOnCopy = false;
            this.tableView1.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableView1.IsEndEditOnEnterKey = false;
            this.tableView1.Location = new System.Drawing.Point(3, 3);
            this.tableView1.MultipleCellEdit = true;
            this.tableView1.MultiSelect = true;
            this.tableView1.Name = "tableView1";
            this.tableView1.ReadOnly = false;
            this.tableView1.ReadOnlyCellBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.tableView1.ReadOnlyCellForeColor = System.Drawing.Color.Black;
            this.tableView1.RowHeight = -1;
            this.tableView1.RowSelect = false;
            this.tableView1.RowValidator = null;
            this.tableView1.ShowImportExportToolbar = false;
            this.tableView1.ShowRowNumbers = true;
            this.tableView1.Size = new System.Drawing.Size(487, 271);
            this.tableView1.TabIndex = 1;
            this.tableView1.UseCenteredHeaderText = false;
            this.tableView1.ViewInfo = null;
            this.tableView1.FocusedRowChanged += new System.EventHandler(this.TableView1OnFocusedRowChanged);
            this.tableView1.RowDeleteHandler += RowDeleteHandler;
            this.tableView1.ColumnFilterChanged += TableView1OnColumnFilterChanged;
            // 
            // DataTableManagerView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "DataTableManagerView";
            this.Size = new System.Drawing.Size(986, 641);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DelftTools.Controls.Swf.Table.TableView tableView1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxSubstanceUseFor;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBoxDataFile;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    partial class ControlGroupLayerEditorView
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
            OpenViewAction = null;
            RtcModel = null;

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
            this.tableViewRules = new DelftTools.Controls.Swf.Table.TableView();
            this.tableViewConditions = new DelftTools.Controls.Swf.Table.TableView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.tableViewRules)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableViewConditions)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableViewRules
            // 
            this.tableViewRules.AllowAddNewRow = false;
            this.tableViewRules.AllowColumnPinning = true;
            this.tableViewRules.AllowColumnSorting = true;
            this.tableViewRules.AllowDeleteRow = false;
            this.tableViewRules.AutoGenerateColumns = true;
            this.tableViewRules.ColumnAutoWidth = false;
            this.tableViewRules.DisplayCellFilter = null;
            this.tableViewRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableViewRules.HeaderHeigth = -1;
            this.tableViewRules.InputValidator = null;
            this.tableViewRules.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableViewRules.InvalidCellFilter = null;
            this.tableViewRules.IsEndEditOnEnterKey = false;
            this.tableViewRules.Location = new System.Drawing.Point(5, 18);
            this.tableViewRules.MultipleCellEdit = true;
            this.tableViewRules.MultiSelect = true;
            this.tableViewRules.Name = "tableViewRules";
            this.tableViewRules.ReadOnly = false;
            this.tableViewRules.ReadOnlyCellBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.tableViewRules.ReadOnlyCellFilter = null;
            this.tableViewRules.ReadOnlyCellForeColor = System.Drawing.Color.Black;
            this.tableViewRules.RowHeight = -1;
            this.tableViewRules.RowSelect = false;
            this.tableViewRules.RowValidator = null;
            this.tableViewRules.EditButtons = false;
            this.tableViewRules.ShowRowNumbers = false;
            this.tableViewRules.Size = new System.Drawing.Size(459, 340);
            this.tableViewRules.TabIndex = 0;
            this.tableViewRules.UseCenteredHeaderText = false;
            // 
            // tableViewConditions
            // 
            this.tableViewConditions.AllowAddNewRow = false;
            this.tableViewConditions.AllowColumnPinning = true;
            this.tableViewConditions.AllowColumnSorting = true;
            this.tableViewConditions.AllowDeleteRow = false;
            this.tableViewConditions.AutoGenerateColumns = true;
            this.tableViewConditions.ColumnAutoWidth = false;
            this.tableViewConditions.DisplayCellFilter = null;
            this.tableViewConditions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableViewConditions.HeaderHeigth = -1;
            this.tableViewConditions.InputValidator = null;
            this.tableViewConditions.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableViewConditions.InvalidCellFilter = null;
            this.tableViewConditions.IsEndEditOnEnterKey = false;
            this.tableViewConditions.Location = new System.Drawing.Point(5, 18);
            this.tableViewConditions.MultipleCellEdit = true;
            this.tableViewConditions.MultiSelect = true;
            this.tableViewConditions.Name = "tableViewConditions";
            this.tableViewConditions.ReadOnly = false;
            this.tableViewConditions.ReadOnlyCellBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.tableViewConditions.ReadOnlyCellFilter = null;
            this.tableViewConditions.ReadOnlyCellForeColor = System.Drawing.Color.Black;
            this.tableViewConditions.RowHeight = -1;
            this.tableViewConditions.RowSelect = false;
            this.tableViewConditions.RowValidator = null;
            this.tableViewConditions.EditButtons = false;
            this.tableViewConditions.ShowRowNumbers = false;
            this.tableViewConditions.Size = new System.Drawing.Size(459, 340);
            this.tableViewConditions.TabIndex = 1;
            this.tableViewConditions.UseCenteredHeaderText = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableViewRules);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox1.Size = new System.Drawing.Size(469, 363);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Rules";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableViewConditions);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(478, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox2.Size = new System.Drawing.Size(469, 363);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Conditions";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(950, 369);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // ControlGroupLayerEditorView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ControlGroupLayerEditorView";
            this.Size = new System.Drawing.Size(950, 369);
            ((System.ComponentModel.ISupportInitialize)(this.tableViewRules)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableViewConditions)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DelftTools.Controls.Swf.Table.TableView tableViewRules;
        private DelftTools.Controls.Swf.Table.TableView tableViewConditions;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

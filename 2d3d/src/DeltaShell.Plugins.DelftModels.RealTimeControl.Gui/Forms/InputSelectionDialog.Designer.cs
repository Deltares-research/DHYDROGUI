namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    partial class InputSelectionDialog
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.tableViewLocations = new DelftTools.Controls.Swf.Table.TableView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableViewDataItems = new DelftTools.Controls.Swf.Table.TableView();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableViewLocations)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableViewDataItems)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Controls.Add(this.buttonOk);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 441);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(434, 40);
            this.panel1.TabIndex = 0;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(347, 10);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Enabled = false;
            this.buttonOk.Location = new System.Drawing.Point(266, 10);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // tableViewLocations
            // 
            this.tableViewLocations.AllowAddNewRow = false;
            this.tableViewLocations.AllowColumnPinning = true;
            this.tableViewLocations.AllowColumnSorting = true;
            this.tableViewLocations.AllowDeleteRow = false;
            this.tableViewLocations.AutoGenerateColumns = false;
            this.tableViewLocations.ColumnAutoWidth = false;
            this.tableViewLocations.DisplayCellFilter = null;
            this.tableViewLocations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableViewLocations.EditableObject = null;
            this.tableViewLocations.HeaderHeigth = -1;
            this.tableViewLocations.InputValidator = null;
            this.tableViewLocations.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableViewLocations.InvalidCellFilter = null;
            this.tableViewLocations.IsEndEditOnEnterKey = false;
            this.tableViewLocations.Location = new System.Drawing.Point(3, 3);
            this.tableViewLocations.MultipleCellEdit = true;
            this.tableViewLocations.MultiSelect = true;
            this.tableViewLocations.Name = "tableViewLocations";
            this.tableViewLocations.ReadOnly = true;
            this.tableViewLocations.ReadOnlyCellBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.tableViewLocations.ReadOnlyCellFilter = null;
            this.tableViewLocations.ReadOnlyCellForeColor = System.Drawing.Color.Black;
            this.tableViewLocations.RowDeleteHandler = null;
            this.tableViewLocations.RowHeight = -1;
            this.tableViewLocations.RowSelect = true;
            this.tableViewLocations.RowValidator = null;
            this.tableViewLocations.EditButtons = false;
            this.tableViewLocations.ShowRowNumbers = false;
            this.tableViewLocations.Size = new System.Drawing.Size(254, 435);
            this.tableViewLocations.TabIndex = 1;
            this.tableViewLocations.UseCenteredHeaderText = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.Controls.Add(this.tableViewLocations, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableViewDataItems, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(434, 441);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // tableViewDataItems
            // 
            this.tableViewDataItems.AllowAddNewRow = false;
            this.tableViewDataItems.AllowColumnPinning = true;
            this.tableViewDataItems.AllowColumnSorting = true;
            this.tableViewDataItems.AllowDeleteRow = false;
            this.tableViewDataItems.AutoGenerateColumns = false;
            this.tableViewDataItems.ColumnAutoWidth = false;
            this.tableViewDataItems.DisplayCellFilter = null;
            this.tableViewDataItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableViewDataItems.EditableObject = null;
            this.tableViewDataItems.HeaderHeigth = -1;
            this.tableViewDataItems.InputValidator = null;
            this.tableViewDataItems.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableViewDataItems.InvalidCellFilter = null;
            this.tableViewDataItems.IsEndEditOnEnterKey = false;
            this.tableViewDataItems.Location = new System.Drawing.Point(263, 3);
            this.tableViewDataItems.MultipleCellEdit = true;
            this.tableViewDataItems.MultiSelect = true;
            this.tableViewDataItems.Name = "tableViewDataItems";
            this.tableViewDataItems.ReadOnly = true;
            this.tableViewDataItems.ReadOnlyCellBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(244)))), ((int)(((byte)(244)))));
            this.tableViewDataItems.ReadOnlyCellFilter = null;
            this.tableViewDataItems.ReadOnlyCellForeColor = System.Drawing.Color.Black;
            this.tableViewDataItems.RowDeleteHandler = null;
            this.tableViewDataItems.RowHeight = -1;
            this.tableViewDataItems.RowSelect = true;
            this.tableViewDataItems.RowValidator = null;
            this.tableViewDataItems.EditButtons = false;
            this.tableViewDataItems.ShowRowNumbers = false;
            this.tableViewDataItems.Size = new System.Drawing.Size(168, 435);
            this.tableViewDataItems.TabIndex = 2;
            this.tableViewDataItems.UseCenteredHeaderText = false;
            // 
            // InputSelectionDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(434, 481);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputSelectionDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select input";
            this.Load += new System.EventHandler(this.InputSelectionDialogLoad);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableViewLocations)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableViewDataItems)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private DelftTools.Controls.Swf.Table.TableView tableViewLocations;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DelftTools.Controls.Swf.Table.TableView tableViewDataItems;
    }
}
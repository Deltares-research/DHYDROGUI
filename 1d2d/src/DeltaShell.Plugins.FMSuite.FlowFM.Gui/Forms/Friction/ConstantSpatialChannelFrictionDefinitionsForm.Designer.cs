namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms.Friction
{
    partial class ConstantSpatialChannelFrictionDefinitionsForm
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
            this.tableView = new DelftTools.Controls.Swf.Table.TableView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.orderByChainageButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tableView)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableView
            // 
            this.tableView.AllowAddNewRow = true;
            this.tableView.AllowColumnFiltering = false;
            this.tableView.AllowColumnPinning = true;
            this.tableView.AllowColumnSorting = true;
            this.tableView.AllowDeleteRow = true;
            this.tableView.AutoGenerateColumns = false;
            this.tableView.AutoSizeRows = false;
            this.tableView.ColumnAutoWidth = false;
            this.tableView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableView.EditButtons = true;
            this.tableView.HeaderHeigth = -1;
            this.tableView.IncludeHeadersOnCopy = false;
            this.tableView.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableView.IsEndEditOnEnterKey = false;
            this.tableView.Location = new System.Drawing.Point(4, 5);
            this.tableView.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.tableView.ShowRowNumbers = false;
            this.tableView.Size = new System.Drawing.Size(623, 440);
            this.tableView.TabIndex = 0;
            this.tableView.UseCenteredHeaderText = false;
            this.tableView.ViewInfo = null;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.tableView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(800, 450);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.orderByChainageButton, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(631, 2);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0, 2, 3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(166, 445);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.okButton);
            this.flowLayoutPanel1.Controls.Add(this.cancelButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(166, 407);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.okButton.Location = new System.Drawing.Point(3, 3);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(160, 32);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OnOkButtonClick);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.cancelButton.Location = new System.Drawing.Point(3, 41);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(160, 32);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.OnCancelButtonClick);
            // 
            // orderByChainageButton
            // 
            this.orderByChainageButton.Location = new System.Drawing.Point(3, 410);
            this.orderByChainageButton.Name = "orderByChainageButton";
            this.orderByChainageButton.Size = new System.Drawing.Size(160, 32);
            this.orderByChainageButton.TabIndex = 1;
            this.orderByChainageButton.Text = "Order by chainage";
            this.orderByChainageButton.UseVisualStyleBackColor = true;
            this.orderByChainageButton.Click += new System.EventHandler(this.OnOrderByChainageButtonClick);
            // 
            // ConstantSpatialChannelFrictionDefinitionsForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConstantSpatialChannelFrictionDefinitionsForm";
            this.Text = "ConstantSpatialChannelFrictionDefinitionsForm";
            ((System.ComponentModel.ISupportInitialize)(this.tableView)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DelftTools.Controls.Swf.Table.TableView tableView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button orderByChainageButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
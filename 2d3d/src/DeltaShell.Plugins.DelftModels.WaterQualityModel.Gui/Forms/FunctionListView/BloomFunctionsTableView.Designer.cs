using DelftTools.Controls;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.FunctionListView
{
    partial class BloomFunctionsTableView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableView = new DelftTools.Controls.Swf.Table.TableView();
            ((System.ComponentModel.ISupportInitialize)(this.tableView)).BeginInit();
            this.SuspendLayout();
            // 
            // tableView
            // 
            this.tableView.AllowAddNewRow = true;
            this.tableView.AllowColumnFiltering = false;
            this.tableView.AllowColumnPinning = true;
            this.tableView.AllowColumnSorting = true;
            this.tableView.AllowDeleteRow = true;
            this.tableView.AutoGenerateColumns = true;
            this.tableView.AutoSizeRows = false;
            this.tableView.ColumnAutoWidth = false;
            this.tableView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableView.EditButtons = true;
            this.tableView.HeaderHeigth = -1;
            this.tableView.IncludeHeadersOnCopy = false;
            this.tableView.InvalidCellBackgroundColor = System.Drawing.Color.Tomato;
            this.tableView.IsEndEditOnEnterKey = false;
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
            this.tableView.ShowRowNumbers = false;
            this.tableView.Size = new System.Drawing.Size(718, 465);
            this.tableView.TabIndex = 0;
            this.tableView.UseCenteredHeaderText = false;
            this.tableView.ViewInfo = null;
            // 
            // BloomFunctionsTableView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableView);
            this.Name = "BloomFunctionsTableView";
            this.Size = new System.Drawing.Size(718, 465);
            ((System.ComponentModel.ISupportInitialize)(this.tableView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DelftTools.Controls.Swf.Table.TableView tableView;
        partial void Dispose();
    }
}

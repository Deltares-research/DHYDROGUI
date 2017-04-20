using DeltaShell.Plugins.SharpMapGis.Gui.Forms.MapLegendView;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    partial class FMModelInspectionWindow
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
            if (disposing)
            {
                OnDispose();
            }

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
            this.mapControl = new SharpMap.UI.Forms.MapControl();
            this.btnSingleStep = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnStep100 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkSync = new System.Windows.Forms.CheckBox();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnPlayPause = new System.Windows.Forms.Button();
            this.lblNoMapFile = new System.Windows.Forms.Label();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanelSide = new System.Windows.Forms.TableLayoutPanel();
            this.mapLegendView = new MapLegendView(gui);
            this.panel1.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.tableLayoutPanelMain.SuspendLayout();
            this.tableLayoutPanelSide.SuspendLayout();
            this.SuspendLayout();
            // 
            // mapControl
            // 
            this.mapControl.AllowDrop = true;
            this.mapControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapControl.Location = new System.Drawing.Point(227, 3);
            this.mapControl.Name = "mapControl";
            this.mapControl.Size = new System.Drawing.Size(893, 669);
            this.mapControl.TabIndex = 0;
            this.mapControl.Text = "mapControl";
            // 
            // btnSingleStep
            // 
            this.btnSingleStep.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSingleStep.Location = new System.Drawing.Point(3, 3);
            this.btnSingleStep.Name = "btnSingleStep";
            this.btnSingleStep.Size = new System.Drawing.Size(64, 22);
            this.btnSingleStep.TabIndex = 1;
            this.btnSingleStep.Text = "Step";
            this.btnSingleStep.UseVisualStyleBackColor = true;
            this.btnSingleStep.Click += new System.EventHandler(this.btnSingleStepClick);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(1036, 6);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 22);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnCloseClick);
            // 
            // btnStep100
            // 
            this.btnStep100.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnStep100.Location = new System.Drawing.Point(73, 3);
            this.btnStep100.Name = "btnStep100";
            this.btnStep100.Size = new System.Drawing.Size(64, 22);
            this.btnStep100.TabIndex = 3;
            this.btnStep100.Text = "Step 100";
            this.btnStep100.UseVisualStyleBackColor = true;
            this.btnStep100.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.chkSync);
            this.panel1.Controls.Add(this.panelButtons);
            this.panel1.Controls.Add(this.btnPlayPause);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 675);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1123, 34);
            this.panel1.TabIndex = 4;
            // 
            // chkSync
            // 
            this.chkSync.AutoSize = true;
            this.chkSync.Checked = true;
            this.chkSync.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSync.Location = new System.Drawing.Point(641, 10);
            this.chkSync.Name = "chkSync";
            this.chkSync.Size = new System.Drawing.Size(85, 17);
            this.chkSync.TabIndex = 7;
            this.chkSync.Text = "Sync redraw";
            this.chkSync.UseVisualStyleBackColor = true;
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.btnSingleStep);
            this.panelButtons.Controls.Add(this.btnStep100);
            this.panelButtons.Location = new System.Drawing.Point(494, 3);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(141, 27);
            this.panelButtons.TabIndex = 6;
            // 
            // btnPlayPause
            // 
            this.btnPlayPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPlayPause.Location = new System.Drawing.Point(427, 6);
            this.btnPlayPause.Name = "btnPlayPause";
            this.btnPlayPause.Size = new System.Drawing.Size(64, 22);
            this.btnPlayPause.TabIndex = 4;
            this.btnPlayPause.Text = "Play";
            this.btnPlayPause.UseVisualStyleBackColor = true;
            this.btnPlayPause.Click += new System.EventHandler(this.btnPlayPauseClick);
            // 
            // MapLegendView
            // 
            this.mapLegendView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom)));
            // 
            // lblNoMapFile
            // 
            this.lblNoMapFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNoMapFile.AutoSize = true;
            this.lblNoMapFile.Location = new System.Drawing.Point(795, 15);
            this.lblNoMapFile.Name = "lblNoMapFile";
            this.lblNoMapFile.Size = new System.Drawing.Size(313, 13);
            this.lblNoMapFile.TabIndex = 6;
            this.lblNoMapFile.Text = "Warning: Some quanties are not available due to missing map file";
            this.lblNoMapFile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblNoMapFile.Visible = false;
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.ColumnCount = 2;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelSide, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.mapControl, 1, 0);
            this.tableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 1;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(1123, 675);
            this.tableLayoutPanelMain.TabIndex = 7;
            // 
            // tableLayoutPanelSide
            // 
            this.tableLayoutPanelSide.ColumnCount = 1;
            this.tableLayoutPanelSide.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSide.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelSide.Controls.Add(this.mapLegendView, 0, 0);
            this.tableLayoutPanelSide.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelSide.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanelSide.Name = "tableLayoutPanelSide";
            this.tableLayoutPanelSide.RowCount = 2;
            this.tableLayoutPanelSide.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelSide.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelSide.Size = new System.Drawing.Size(218, 669);
            this.tableLayoutPanelSide.TabIndex = 0;
            // 
            // FMModelInspectionWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1123, 709);
            this.Controls.Add(this.lblNoMapFile);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Controls.Add(this.panel1);
            this.KeyPreview = true;
            this.Name = "FMModelInspectionWindow";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "--";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FMModelInspectionWindow_FormClosed);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FMModelInspectionWindow_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FMModelInspectionWindow_KeyUp);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelSide.ResumeLayout(false);
            this.tableLayoutPanelSide.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SharpMap.UI.Forms.MapControl mapControl;
        private System.Windows.Forms.Button btnSingleStep;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnStep100;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnPlayPause;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.CheckBox chkSync;
        private System.Windows.Forms.Label lblNoMapFile;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSide;
        private MapLegendView mapLegendView;
    }
}
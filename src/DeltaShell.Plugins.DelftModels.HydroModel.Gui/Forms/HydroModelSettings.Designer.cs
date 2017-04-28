using System.Windows.Forms.Integration;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Views;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms
{
    partial class HydroModelSettings
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
            this.label7 = new System.Windows.Forms.Label();
            this.buttonRun = new System.Windows.Forms.Button();
            this.buttonAddActivity = new System.Windows.Forms.Button();
            this.listBoxActivities = new System.Windows.Forms.ListBox();
            this.buttonDeleteActivity = new System.Windows.Forms.Button();
            this.bindingSourceHydroModel = new System.Windows.Forms.BindingSource(this.components);
            this.label6 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.workflowEditorControl = new DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.WorkflowEditorControl();
            this.panel2 = new System.Windows.Forms.Panel();
            this.userControl = new HydroModelTimeSettingsUserControl();
            this.WpfElementHost = new ElementHost();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceHydroModel)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label7.Location = new System.Drawing.Point(203, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(72, 15);
            this.label7.TabIndex = 0;
            this.label7.Text = "Workflows";
            // 
            // buttonRun
            // 
            this.buttonRun.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonRun.Location = new System.Drawing.Point(0, 0);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 22);
            this.buttonRun.TabIndex = 11;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // buttonAddActivity
            // 
            this.buttonAddActivity.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonAddActivity.Location = new System.Drawing.Point(0, 0);
            this.buttonAddActivity.Name = "buttonAddActivity";
            this.buttonAddActivity.Size = new System.Drawing.Size(57, 22);
            this.buttonAddActivity.TabIndex = 7;
            this.buttonAddActivity.Text = "Add ...";
            this.buttonAddActivity.UseVisualStyleBackColor = true;
            this.buttonAddActivity.Click += new System.EventHandler(this.buttonAddActivity_Click);
            // 
            // listBoxActivities
            // 
            this.listBoxActivities.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBoxActivities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxActivities.FormattingEnabled = true;
            this.listBoxActivities.Location = new System.Drawing.Point(3, 23);
            this.listBoxActivities.Name = "listBoxActivities";
            this.listBoxActivities.Size = new System.Drawing.Size(194, 184);
            this.listBoxActivities.TabIndex = 6;
            this.listBoxActivities.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxActivities_KeyDown);
            // 
            // buttonDeleteActivity
            // 
            this.buttonDeleteActivity.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonDeleteActivity.Location = new System.Drawing.Point(57, 0);
            this.buttonDeleteActivity.Name = "buttonDeleteActivity";
            this.buttonDeleteActivity.Size = new System.Drawing.Size(57, 22);
            this.buttonDeleteActivity.TabIndex = 8;
            this.buttonDeleteActivity.Text = "Delete";
            this.buttonDeleteActivity.UseVisualStyleBackColor = true;
            this.buttonDeleteActivity.Click += new System.EventHandler(this.buttonDeleteActivity_Click);
            // 
            // bindingSourceHydroModel
            // 
            this.bindingSourceHydroModel.RaiseListChangedEvents = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.Location = new System.Drawing.Point(3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 15);
            this.label6.TabIndex = 0;
            this.label6.Text = "Models";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.workflowEditorControl, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.listBoxActivities, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label7, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(247, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(637, 238);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.buttonRun);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(203, 213);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(431, 22);
            this.panel3.TabIndex = 12;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonDeleteActivity);
            this.panel1.Controls.Add(this.buttonAddActivity);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 213);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(194, 22);
            this.panel1.TabIndex = 0;
            // 
            // workflowEditorControl
            // 
            this.workflowEditorControl.AutoScroll = true;
            this.workflowEditorControl.CurrentWorkflow = null;
            this.workflowEditorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workflowEditorControl.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.workflowEditorControl.Location = new System.Drawing.Point(203, 23);
            this.workflowEditorControl.Margin = new System.Windows.Forms.Padding(3, 3, 15, 3);
            this.workflowEditorControl.Name = "workflowEditorControl";
            this.workflowEditorControl.Size = new System.Drawing.Size(419, 184);
            this.workflowEditorControl.TabIndex = 12;
            this.workflowEditorControl.Workflows = null;
            this.workflowEditorControl.CurrentWorkflowChanged += new System.EventHandler<System.EventArgs>(this.workflowEditorControl_CurrentWorkflowChanged);
            this.workflowEditorControl.SelectedActivityChanged += new System.EventHandler<DelftTools.Utils.EventArgs<DelftTools.Shell.Core.Workflow.IActivity>>(this.WorkflowEditorControlSelectedActivityChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.WpfElementHost);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(247, 238);
            this.panel2.TabIndex = 14;
            // 
            // WpfElementHost
            // 
            this.WpfElementHost.Location = new System.Drawing.Point(0, 0);
            this.WpfElementHost.Name = "WpfElementHost";
            this.WpfElementHost.Size = new System.Drawing.Size(247, 238);
            this.WpfElementHost.TabIndex = 12;
            this.WpfElementHost.Text = "WpfElementHost";
            this.WpfElementHost.Child = this.userControl;
            // 
            // HydroModelSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoScrollMinSize = new System.Drawing.Size(875, 230);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel2);
            this.Name = "HydroModelSettings";
            this.Size = new System.Drawing.Size(884, 238);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceHydroModel)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            //this.userControl.ViewModel.PropertyChanged += this.ViewModelPropertyChanged;
            this.ResumeLayout(false);
        }

        #endregion
        
        private System.Windows.Forms.BindingSource bindingSourceHydroModel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Button buttonAddActivity;
        private System.Windows.Forms.ListBox listBoxActivities;
        private System.Windows.Forms.Button buttonDeleteActivity;
        private WorkflowEditorControl workflowEditorControl;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Integration.ElementHost WpfElementHost;
        private Views.HydroModelTimeSettingsUserControl userControl;
    }
}

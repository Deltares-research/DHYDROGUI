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
            this.bindingSourceHydroModel = new System.Windows.Forms.BindingSource(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.workflowEditorControl = new DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.WorkflowEditorControl();
            this.ModelsTimeSettingsView = new System.Windows.Forms.Integration.ElementHost();
            this.view = new DeltaShell.Plugins.DelftModels.HydroModel.Gui.Views.HydroModelTimeSettingsView();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceHydroModel)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label7.Location = new System.Drawing.Point(3, 0);
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
            // bindingSourceHydroModel
            // 
            this.bindingSourceHydroModel.RaiseListChangedEvents = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 384F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel3, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.workflowEditorControl, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label7, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(500, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(537, 238);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.buttonRun);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 185);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(378, 22);
            this.panel3.TabIndex = 12;
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
            // ModelsTimeSettingsView
            // 
            this.ModelsTimeSettingsView.Dock = System.Windows.Forms.DockStyle.Left;
            this.ModelsTimeSettingsView.Location = new System.Drawing.Point(0, 0);
            this.ModelsTimeSettingsView.Name = "ModelsTimeSettingsView";
            this.ModelsTimeSettingsView.Size = new System.Drawing.Size(500, 230);
            this.ModelsTimeSettingsView.TabIndex = 12;
            this.ModelsTimeSettingsView.Text = "ModelsTimeSettingsView";
            this.ModelsTimeSettingsView.Child = this.view;
            // 
            // HydroModelSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoScrollMinSize = new System.Drawing.Size(875, 230);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.ModelsTimeSettingsView);
            this.Name = "HydroModelSettings";
            this.Size = new System.Drawing.Size(666, 204);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceHydroModel)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        
        private System.Windows.Forms.BindingSource bindingSourceHydroModel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonRun;
        private WorkflowEditorControl workflowEditorControl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Integration.ElementHost ModelsTimeSettingsView;
        private Views.HydroModelTimeSettingsView view;
    }
}

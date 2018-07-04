using System.Windows.Forms;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    partial class BoundaryConditionListView
    {
        private const string FeaturePropertyDescription = "Boundary";
        private const string QuantityPropertyDescription = "Quantity";
        private const string ForcingTypePropertyDescription = "Forcing type";
        private const string FactorPropertyDescription = "Factor";
        private const string OffsetPropertyDescription = "Offset";

        private static readonly string FeaturePropertyName =
           TypeUtils.GetMemberName<FlowBoundaryCondition>(bc => bc.FeatureName);

        private static readonly string QuantityPropertyName =
            TypeUtils.GetMemberName<FlowBoundaryCondition>(bc => bc.VariableName);

        private static readonly string ForcingTypePropertyName =
            TypeUtils.GetMemberName<FlowBoundaryCondition>(bc => bc.DataType);

        private static readonly string FactorPropertyName =
            TypeUtils.GetMemberName<FlowBoundaryCondition>(bc => bc.Factor);

        private static readonly string OffsetPropertyName =
            TypeUtils.GetMemberName<FlowBoundaryCondition>(bc => bc.Offset);

        private static readonly string ThatcherHarlemanPropertyName =
            TypeUtils.GetMemberName<FlowBoundaryCondition>(bc => bc.ThatcherHarlemanTimeLag);

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
            this.SuspendLayout();
            // 
            // BoundaryConditionListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "BoundaryConditionListView";
            this.Size = new System.Drawing.Size(214, 196);
            this.ResumeLayout(false);
        }

        #endregion
    }
}

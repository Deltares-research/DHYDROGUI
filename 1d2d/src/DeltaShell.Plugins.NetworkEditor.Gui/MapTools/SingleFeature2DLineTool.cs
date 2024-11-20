using System;
using System.Drawing;
using System.Windows.Forms;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Geometries;
using log4net;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    /// <summary>
    /// SingleFeature2DLineTool works much the same as Feature2DLineTool
    /// Except! the number of features allowed on the targetLayer is limited to 1
    /// In the event that a feature already exists on the targetLayer the user is warned and given the opportunity to cancel
    /// </summary>
    public class SingleFeature2DLineTool : Feature2DLineTool
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SingleFeature2DLineTool));
        private bool warningGiven = false;
        public SingleFeature2DLineTool(string targetLayerName, string name, Bitmap icon) : base(targetLayerName, name, icon)
        {

        }

        /// <summary>
        /// Override IsActive to warn the user when activating the tool
        /// </summary>
        public override bool IsActive
        {
            get
            {
                return base.IsActive;
            }
            set
            {
                if (!value)
                {
                    warningGiven = false; // Reset warning flag
                }
                else if(HasExistingFeatures())
                {
                    LogWarning();
                }

                base.IsActive = value;
            }
        }
        
        /// <summary>
        /// Override OnMouseDown to warn the user when the tool is already active
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="e"></param>
        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            if (!warningGiven && HasExistingFeatures())
                LogWarning();

            base.OnMouseDown(worldPosition, e);
        }

        /// <summary>
        /// Override OnMouseDoubleClick to clear existing features when the user adds a new feature
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (HasExistingFeatures())
            {
                VectorLayer.DataSource.Features.Clear();
            }
            base.OnMouseDoubleClick(sender, e);
        }


        private void LogWarning()
        {
            Log.WarnFormat(Resources.SingleFeature2DLineTool_LogWarning_only_one_feature_supported, 
                           Name, LayerName, Environment.NewLine);

            warningGiven = true;
        }

        private bool HasExistingFeatures()
        {
            if (VectorLayer != null &&
                VectorLayer.DataSource != null &&
                VectorLayer.DataSource.Features != null)
            {
                return VectorLayer.DataSource.Features.Count > 0;
            }
            return false;
        }

    }
}

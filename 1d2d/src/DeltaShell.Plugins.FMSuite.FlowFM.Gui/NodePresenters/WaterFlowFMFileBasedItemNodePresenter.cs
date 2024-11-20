using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class WaterFlowFMFileBasedItemNodePresenter: FMSuiteNodePresenterBase<FileBasedModelItem>
    {
        protected override string GetNodeText(FileBasedModelItem data)
        {
            return data.FileName;
        }

        protected override Image GetNodeImage(FileBasedModelItem data)
        {
            if (data.PropertyName == WaterFlowFMFileBasedItemFactory.MduFileProperty)
            {
                return WaterFlowFMModelNodePresenter.UnstrucModelIcon;
            }
            if (data.PropertyName == WaterFlowFMFileBasedItemFactory.MapFileProperty)
            {
                return Resources.unstrucWater;
            }
            if (data.PropertyName == WaterFlowFMFileBasedItemFactory.HisFileProperty)
            {
                return Resources.TimeSeries;
            }
            if (Model?.ModelDefinition != null)
            {
                if (MatchProperty(data, KnownProperties.ExtForceFile))
                {
                    return Common.Gui.Properties.Resources.folder_wrench;
                }
                if (MatchProperty(data, KnownProperties.BndExtForceFile))
                {
                    return Common.Gui.Properties.Resources.folder_wrench;
                }
                if (MatchProperty(data,KnownProperties.LandBoundaryFile))
                {
                    return WaterFlowFMModelNodePresenter.LandBoundaryIcon;
                }
                if (MatchProperty(data, KnownProperties.ThinDamFile))
                {
                    return WaterFlowFMModelNodePresenter.ThinDamIcon;
                }
                if (MatchProperty(data, KnownProperties.FixedWeirFile))
                {
                    return WaterFlowFMModelNodePresenter.FixedWeirIcon;
                }
                if (MatchProperty(data, KnownProperties.ObsFile))
                {
                    return WaterFlowFMModelNodePresenter.ObsIcon;
                }
                if (MatchProperty(data, KnownProperties.ObsCrsFile))
                {
                    return WaterFlowFMModelNodePresenter.ObsCSIcon;
                }
                if (MatchProperty(data, KnownProperties.StructuresFile))
                {
                    return Resources.Pump;       
                }
                if (MatchProperty(data, KnownProperties.NetFile))
                {
                    return WaterFlowFMModelNodePresenter.UnstrucIcon;
                }
                if (MatchProperty(data, KnownProperties.DryPointsFile))
                {
                    return WaterFlowFMModelNodePresenter.DryPointIcon;
                }
                if (MatchProperty(data, KnownProperties.RoofAreaFile))
                {
                    return WaterFlowFMModelNodePresenter.RoofAreaIcon;
                }
                if (MatchProperty(data, KnownProperties.GulliesFile))
                {
                    return WaterFlowFMModelNodePresenter.GullyIcon;
                }
                if (MatchProperty(data.Parent, KnownProperties.ExtForceFile) ||
                    MatchProperty(data.Parent, KnownProperties.BndExtForceFile))
                {
                    var extension = Path.GetExtension(data.FileName);

                    if (extension == null) return null;

                    var fileExtension = extension.TrimStart('.');

                    if (fileExtension == ExtForceQuantNames.PliFileExtension)
                    {
                        return WaterFlowFMModelNodePresenter.LandBoundaryIcon;
                    }
                    if (fileExtension == ExtForceQuantNames.PolFileExtension)
                    {
                        return Common.Gui.Properties.Resources.polygon;
                    }
                    if (fileExtension == ExtForceQuantNames.TimFileExtension || fileExtension == ExtForceQuantNames.T3DFileExtension)
                    {
                        return Resources.BoundaryType_TimeSeries;
                    }
                    if (fileExtension == ExtForceQuantNames.CmpFileExtension)
                    {
                        return Resources.BoundaryType_Harmonics;
                    }
                    if (fileExtension == ExtForceQuantNames.QhFileExtension)
                    {
                        return Resources.BoundaryType_Qh;
                    }
                    if (fileExtension == BcFile.Extension.TrimStart('.'))
                    {
                        return Common.Gui.Properties.Resources.boundary;
                    }
                    if (fileExtension == "spw")
                    {
                        return Common.Gui.Properties.Resources.hurricane2;
                    }
                    if (fileExtension == "wnd" || fileExtension == "amu" || fileExtension == "amv" ||
                        fileExtension == "amp" || fileExtension == "apwxwy")
                    {
                        return Resources.Wind1;
                    }
                }
            }
            return null;
        }

        private bool MatchProperty(FileBasedModelItem data, string propertyKey)
        {
            var modelProperty = Model.ModelDefinition.GetModelProperty(propertyKey);
            var modelPropertyCaption = modelProperty.PropertyDefinition.Caption;

            return modelPropertyCaption == data.PropertyName;
        }

        public override IEnumerable GetChildNodeObjects(FileBasedModelItem parentNodeData, ITreeNode node)
        {
            return parentNodeData.DirectChildren;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            if (nodeData is FileBasedModelItem && ((FileBasedModelItem) nodeData).FileExists)
            {
                var contextMenuStrip = new ContextMenuStrip();
                var toolStripMenuItem = new ClonableToolStripMenuItem
                    {
                        Text = Resources.WaterFlowFMFileBasedItemNodePresenter_GetContextMenu_Show_in_Windows_Explorer___, 
                        Tag = nodeData
                    };
                toolStripMenuItem.Click += ContextMenuStripOnClick;
                contextMenuStrip.Items.Add(toolStripMenuItem);
                return new MenuItemContextMenuStripAdapter(contextMenuStrip);
            }
            return null;
        }

        private void ContextMenuStripOnClick(object sender, EventArgs eventArgs)
        {
            var fileBasedModelItem = (FileBasedModelItem) ((ToolStripMenuItem) sender).Tag;
            var args = string.Format("/Select, {0}", fileBasedModelItem.FilePath);
            Process.Start(new ProcessStartInfo("Explorer.exe", args));
        }

        public WaterFlowFMModel Model { get; set; }
    }
}
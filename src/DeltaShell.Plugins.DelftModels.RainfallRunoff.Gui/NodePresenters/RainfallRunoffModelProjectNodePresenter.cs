using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.NodePresenters
{
    public class RainfallRunoffModelProjectNodePresenter : ModelNodePresenterBase<RainfallRunoffModel>
    {
        public const string CatchmentDataFolderName = "Catchment Data";

        public RainfallRunoffModelProjectNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        public override IEnumerable GetChildNodeObjects(RainfallRunoffModel parentNodeData, ITreeNode node)
        {
            foreach (var inputItem in GetInputItems(parentNodeData))
            {
                yield return inputItem;
            }
            
            yield return new TreeFolder(parentNodeData, GetOutputItems(parentNodeData), "Output", FolderImageType.Output);
        }

        private IEnumerable GetInputItems(RainfallRunoffModel model)
        {
            yield return model.GetDataItemByValue(model.Basin);
            yield return new TreeFolder(model, GetMeteoDataItems(model), "Meteorological Data", FolderImageType.None);
            yield return new TreeFolder(model, GetInitialConditions(model), "Initial Conditions", FolderImageType.None);
            yield return new CatchmentModelDataTreeFolder(model, model.ModelData, CatchmentDataFolderName, FolderImageType.None);
            yield return new TreeFolder(model, GetNwrwDataItems(model), "Nwrw", FolderImageType.None);
        }

        private IEnumerable GetNwrwDataItems(RainfallRunoffModel model)
        {
            yield return new DataItem(model.NwrwDryWeatherFlowDefinitions, "Dryweather Flow Definitions");
            yield return new DataItem(model.NwrwDefinitions, "Surface Settings");
        }
        
        private IEnumerable GetMeteoDataItems(RainfallRunoffModel model)
        {
            yield return model.GetDataItemByValue(model.Precipitation);
            yield return model.GetDataItemByValue(model.Evaporation);
            yield return model.GetDataItemByValue(model.Temperature);
        }
        
        private IEnumerable GetInitialConditions(RainfallRunoffModel model)
        {
            yield return
                new DataItem(new RRInitialConditionsWrapper
                    {
                        Model = model,
                        Name = "Unpaved",
                        Type = RRInitialConditionsWrapper.InitialConditionsType.Unpaved,
                    }, DataItemRole.Input) {Owner = model};
            yield return
                new DataItem(new RRInitialConditionsWrapper
                    {
                        Model = model,
                        Name = "Paved",
                        Type = RRInitialConditionsWrapper.InitialConditionsType.Paved,
                    }, DataItemRole.Input) { Owner = model };
            yield return
                new DataItem(new RRInitialConditionsWrapper
                    {
                        Model = model,
                        Name = "Greenhouse",
                        Type = RRInitialConditionsWrapper.InitialConditionsType.Greenhouse,
                    }, DataItemRole.Input) { Owner = model };
        }

        private IEnumerable GetOutputItems(RainfallRunoffModel model)
        {
            foreach (var outputDataItem in model.DataItems.Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output))
            {
                yield return model.GetDataItemByValue(outputDataItem.Value);
            }

            foreach (var outputDataItem in model.OutputDataItems.Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output))
            {
                yield return outputDataItem;
            }
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var menu = base.GetContextMenu(sender, nodeData);
            var model = nodeData as RainfallRunoffModel;

            if (model == null) return menu;

            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.Add(CreateValidationMenuItem(model));
            contextMenu.Items.Add(CreateOpenWorkDirMenuItem(model));
            contextMenu.Items.Add(CreateInitialConditionsMenuItem(model));

            var rrModelMenu = new MenuItemContextMenuStripAdapter(contextMenu);

            if (menu != null)
            {
                menu.Add(rrModelMenu);
            }
            else
            {
                return rrModelMenu;
            }
            return menu;
        }

        private static ClonableToolStripMenuItem CreateOpenWorkDirMenuItem(RainfallRunoffModel model)
        {
            var workingDirItem = new ClonableToolStripMenuItem
            {
                Text = Resources.RainfallRunoffModelProjectNodePresenter_CreateOpenWorkDirMenuItem_Open_Last_Working_Directory___, 
                Tag = model, 
                Image = Resources.workingdirectory
            };
            workingDirItem.Click += WorkingDirClicked;
            return workingDirItem;
        }

        private ClonableToolStripMenuItem CreateValidationMenuItem(RainfallRunoffModel model)
        {
            var validateItem = new ClonableToolStripMenuItem
            {
                Text = Resources.RainfallRunoffModelProjectNodePresenter_CreateValidationMenuItem_Validate___, 
                Tag = model, 
                Image = Resources.validation
            };
            validateItem.Click += ValidateClicked;
            return validateItem;
        }

        private static ClonableToolStripMenuItem CreateInitialConditionsMenuItem(RainfallRunoffModel model)
        {
            var topItem = new ClonableToolStripMenuItem
            {
                Text = Resources.RainfallRunoffModelProjectNodePresenter_CreateInitialConditionsMenuItem_Use_Previous_Output_as_Initial_Condition, 
                Image = Resources.arrow_return
            };

            var chooseItem = new ClonableToolStripMenuItem
            {
                Text = Resources.RainfallRunoffModelProjectNodePresenter_CreateInitialConditionsMenuItem_Choose_Time_Step___
            };
            chooseItem.Click += (s, e) => new InitialConditionsFromOutputTimePicker(
                                              model.SetInitialConditionsFromPreviousOutput) { Model = model }
                                              .ShowDialog();

            var lastItem = new ClonableToolStripMenuItem
            {
                Text = Resources.RainfallRunoffModelProjectNodePresenter_CreateInitialConditionsMenuItem_Use_Last_Time_Step
            };
            lastItem.Click += (s, e) => model.SetInitialConditionsFromPreviousOutput(model.StopTime);

            topItem.DropDownItems.Add(chooseItem);
            topItem.DropDownItems.Add(lastItem);

            topItem.Enabled = model.OutputCoverages.Any() && model.OutputCoverages.First().Time.Values.Count > 0;

            return topItem;
        }
        
        private static void WorkingDirClicked(object sender, EventArgs e)
        {
            var model = (RainfallRunoffModel) ((ToolStripItem) sender).Tag;
            string workingDir = model.ModelController.WorkingDirectory;
            if (String.IsNullOrEmpty(workingDir))
            {
                MessageBox.Show(Resources.RainfallRunoffModelProjectNodePresenter_WorkingDirClicked_Working_directory_not_created_yet__Model_must_run_first_,
                                Resources.RainfallRunoffModelProjectNodePresenter_WorkingDirClicked_Cannot_open_working_directory, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Process.Start(workingDir); //open directory in explorer
            }
        }

        private void ValidateClicked(object sender, EventArgs args)
        {
            var model = (RainfallRunoffModel) ((ToolStripItem) sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof (ValidationView));
        }
    }
}
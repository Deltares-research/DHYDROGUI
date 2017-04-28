using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.ProjectExplorer
{
    public class WaterFlowModel1DNodePresenter : ModelNodePresenterBase<WaterFlowModel1D>
    {
        public WaterFlowModel1DNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        public override IEnumerable GetChildNodeObjects(WaterFlowModel1D parentNodeData, ITreeNode node)
        {
            yield return new TreeFolder(parentNodeData, GetInputItems(parentNodeData), WaterFlowModelConstants.InputFolderName, FolderImageType.Input);
            yield return new TreeFolder(parentNodeData, GetOutputItems(parentNodeData), WaterFlowModelConstants.OutputFolderName, FolderImageType.Output);
        }

        private static IEnumerable GetOutputItems(WaterFlowModel1D data)
        {
            yield return GetRestartFolder(data);

            foreach (var outputDataItem in data.DataItems.Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output &&
                !IsOutputRestartFile(di)))
            {
                yield return data.GetDataItemByValue(outputDataItem.Value);
            }
        }

        private static object GetRestartFolder(WaterFlowModel1D data)
        {
            return new TreeFolder(data, GetRestartStates(data), "States", FolderImageType.None);
        }

        private static IEnumerable GetRestartStates(WaterFlowModel1D data)
        {
            var restartStates = data.DataItems.Where(IsOutputRestartFile);
            foreach (var restartState in restartStates)
            {
                yield return restartState;
            }
        }

        private static bool IsOutputRestartFile(IDataItem dataItem)
        {
            return dataItem.Value is FileBasedRestartState && dataItem.Role == DataItemRole.Output;
        }

        private static IEnumerable GetInputItems(WaterFlowModel1D data)
        {
            yield return data.GetDataItemByValue(data.Network);
            yield return data.GetDataItemByValue(data.NetworkDiscretization);
            yield return data.BoundaryConditionsDataItemSet;
            yield return data.LateralSourcesDataItemSet;
            yield return data.GetDataItemByValue(data.RoughnessSections);
            yield return new TreeFolder(data, GetInitialConditionsItems(data), "Initial Conditions", FolderImageType.None);
            yield return data.GetDataItemByValue(data.WindShielding);
            yield return new TreeFolder(data, GetMeteoItems(data), "Meteo data", FolderImageType.None);
            yield return new TreeFolder(data, GetDispersionItems(data), "Dispersion", FolderImageType.None);
        }

        private static IEnumerable GetMeteoItems(WaterFlowModel1D data)
        {
            yield return data.GetDataItemByValue(data.Wind);
            if (data.UseTemperature && data.TemperatureModelType == TemperatureModelType.Composite)
            {
                yield return data.GetDataItemByValue(data.MeteoData);
            }
        }

        private static IEnumerable GetDispersionItems(WaterFlowModel1D data)
        {
            if (data.UseSalt)
            {
                yield return data.GetDataItemByValue(data.DispersionCoverage);
                if (data.DispersionFormulationType != DispersionFormulationType.Constant)
                {
                    yield return data.GetDataItemByValue(data.DispersionF3Coverage);
                    yield return data.GetDataItemByValue(data.DispersionF4Coverage);
                }
            }
        }

        private static IEnumerable GetInitialConditionsItems(WaterFlowModel1D data)
        {
            yield return data.GetDataItemByValue(data.InitialConditions);
            yield return data.GetDataItemByValue(data.InitialFlow);

            if (data.InitialSaltConcentration != null)
            {
                yield return data.GetDataItemByValue(data.InitialSaltConcentration);
            }

            if (data.UseTemperature)
            {
                yield return data.GetDataItemByValue(data.InitialTemperature);
            }

            yield return data.GetDataItemByValue(data.RestartInput);
        }

        // Note: Why do this?
        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var menu = base.GetContextMenu(sender, nodeData);
            var model = nodeData as WaterFlowModel1D;
            
            if (model == null) return menu;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(CreateValidationMenuItem(model));
            contextMenu.Items.Add(CreateInitialConditionsMenuItem(model));

            var flowMenu = new MenuItemContextMenuStripAdapter(contextMenu);

            if (menu != null)
                menu.Add(flowMenu);
            else
                return flowMenu;
            
            return menu;
        }

        private static ClonableToolStripMenuItem CreateInitialConditionsMenuItem(WaterFlowModel1D model)
        {
            var topItem = new ClonableToolStripMenuItem
            {
                Text = Resources.WaterFlowModel1DNodePresenter_CreateInitialConditionsMenuItem_Use_Previous_Output_as_Initial_Condition,
                Image = Resources.arrow_return
            };

            var chooseItem = new ClonableToolStripMenuItem
            {
                Text = Resources.WaterFlowModel1DNodePresenter_CreateInitialConditionsMenuItem_Choose_Time_Step___
            };
            chooseItem.Click += (s, e) => new InitialConditionsFromOutputTimePicker(
                                              model.SetInitialConditionsFromPreviousOutput) {Model = model}
                                              .ShowDialog();
            var lastItem = new ClonableToolStripMenuItem
            {
                Text = Resources.WaterFlowModel1DNodePresenter_CreateInitialConditionsMenuItem_Use_Last_Time_Step
            };
            lastItem.Click += (s, e) => model.SetInitialConditionsFromPreviousOutput(model.StopTime);

            topItem.DropDownItems.Add(chooseItem);
            topItem.DropDownItems.Add(lastItem);

            topItem.Enabled = model.OutputFunctions.Any() &&
                              model.OutputFunctions.First()
                                  .Arguments.First(a => a.Name.Equals("Time", StringComparison.InvariantCultureIgnoreCase))
                                  .Values.Count > 0;

            return topItem;
        }

        private ClonableToolStripMenuItem CreateValidationMenuItem(WaterFlowModel1D model)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = Resources.WaterFlowModel1DNodePresenter_CreateValidationMenuItem_Validate___, 
                Tag = model, 
                Image = Resources.validation
            };
            item.Click += OnValidateClicked;
            return item;
        }

        private void OnValidateClicked(object sender, EventArgs args)
        {
            //do this differently in the future:
            var model = (WaterFlowModel1D)((ToolStripItem)sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof (ValidationView));
        }
    }
}
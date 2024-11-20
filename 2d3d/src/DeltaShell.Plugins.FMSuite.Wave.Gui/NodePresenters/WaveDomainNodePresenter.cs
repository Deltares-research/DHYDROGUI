using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Controls.Swf.TreeViewControls;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters
{
    public class WaveDomainNodePresenter : TreeViewNodePresenterBase<WaveDomainData>
    {
        private static readonly Bitmap DomainFolderImage = Resources.folder_domain;
        private static readonly Bitmap GridImage = Resources.Grid2D;
        private static readonly Bitmap BathymetryImage = Common.Gui.Properties.Resources.bathymetry;

        private readonly Func<WaveDomainData, WaveModel> getModelForDomain;

        private readonly string gridMemberName = nameof(WaveDomainData.Grid);
        private readonly string bathymetryMemberName = nameof(WaveDomainData.Bathymetry);

        public WaveDomainNodePresenter(Func<WaveDomainData, WaveModel> getModel)
        {
            getModelForDomain = getModel;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WaveDomainData nodeData)
        {
            node.Text = nodeData.Name;
            node.Image = DomainFolderImage;
        }

        public override IEnumerable GetChildNodeObjects(WaveDomainData parentNodeData, ITreeNode node)
        {
            WaveModel model = getModelForDomain(parentNodeData);
            yield return new WaveModelTreeShortcut(parentNodeData.Grid.Name, GridImage, model, parentNodeData.Grid,
                                                   ShortCutType.Grid);
            yield return new WaveModelTreeShortcut(parentNodeData.Bathymetry.Name, BathymetryImage, model,
                                                   parentNodeData.Bathymetry, ShortCutType.SpatialCoverageWithView);

            foreach (IWaveDomainData domain in parentNodeData.SubDomains)
            {
                yield return domain;
            }
        }

        public override DragOperations CanDrop(object item, ITreeNode sourceNode, ITreeNode targetNode,
                                               DragOperations validOperations)
        {
            return DragOperations.Move;
        }

        public override DragOperations CanDrag(WaveDomainData nodeData)
        {
            return nodeData.SuperDomain != null
                       ? DragOperations.Move
                       : DragOperations.None;
        }

        public override void OnDragDrop(object item, object sourceParentNodeData, WaveDomainData target,
                                        DragOperations operation, int position)
        {
            if (operation != DragOperations.Move)
            {
                throw new NotImplementedException("No operations other than 'move' expected");
            }

            if (sourceParentNodeData.Equals(target))
            {
                return;
            }

            var domain = item as WaveDomainData;
            var oldParent = sourceParentNodeData as WaveDomainData;
            WaveModel model = getModelForDomain(domain);

            model.BeginEdit("Move domain...");
            model.DeleteSubDomain(oldParent, domain);
            model.AddSubDomain(target, domain);
            model.EndEdit();
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var waveDomain = nodeData as WaveDomainData;
            WaveModel model = getModelForDomain(waveDomain);
            if (model != null && waveDomain != null)
            {
                var contextMenu = new ContextMenuStrip();
                if (waveDomain.SuperDomain == null)
                {
                    contextMenu.Items.Add(CreateAddSuperDomainMenuItem(model, waveDomain));
                }

                contextMenu.Items.Add(CreateAddDomainMenuItem(model, waveDomain));
                contextMenu.Items.Add(CreateDeleteDomainMenuItem(model, waveDomain));

                var domainMenu = new MenuItemContextMenuStripAdapter(contextMenu);
                return domainMenu;
            }

            return null;
        }

        protected override void OnPropertyChanged(WaveDomainData item, ITreeNode node, PropertyChangedEventArgs e)
        {
            if (node == null)
            {
                return;
            }

            if (e.PropertyName == gridMemberName ||
                e.PropertyName == bathymetryMemberName)
            {
                node.Update();
            }

            base.OnPropertyChanged(item, node, e);
        }

        protected override bool CanRemove(WaveDomainData nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, WaveDomainData nodeData)
        {
            WaveModel model = getModelForDomain(nodeData);
            DeleteDomain(model, nodeData);
            return true;
        }

        private ToolStripItem CreateAddSuperDomainMenuItem(WaveModel model, WaveDomainData waveDomain)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = Resources.WaveDomainNodePresenter_CreateAddSuperDomainMenuItem_Add_Exterior_Domain___,
                Tag = model
            };
            item.Click += (s, a) => AddSuperDomainOnClick(model, waveDomain);
            return item;
        }

        private ToolStripItem CreateAddDomainMenuItem(WaveModel model, WaveDomainData waveDomain)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = Resources.WaveDomainNodePresenter_CreateAddDomainMenuItem_Add_Interior_Domain___,
                Tag = model
            };
            item.Click += (s, a) => AddSubDomainOnClick(model, waveDomain);
            return item;
        }

        private ToolStripItem CreateDeleteDomainMenuItem(WaveModel model, WaveDomainData nodeData)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = Resources.WaveDomainNodePresenter_CreateDeleteDomainMenuItem_Delete_Domain,
                Tag = model
            };
            item.Click += (s, a) => DeleteDomain(model, nodeData);
            item.Image = Common.Gui.Properties.Resources.DeleteHS;
            return item;
        }

        private void DeleteDomain(WaveModel model, WaveDomainData nodeData)
        {
            if (nodeData.SuperDomain != null)
            {
                model.DeleteSubDomain(nodeData.SuperDomain, nodeData);
            }
            else if (nodeData.SubDomains.Count == 1)
            {
                // here we know what to do
                model.BeginEdit("Delete outer domain ...");
                IWaveDomainData newOuterDomain = model.OuterDomain.SubDomains[0];
                newOuterDomain.SuperDomain = null;    // First set this to update domain specific view if open before removing subdomains otherwise view will not be updated
                model.OuterDomain.SubDomains.Clear(); // disconnect
                model.OuterDomain = newOuterDomain;
                model.EndEdit();
            }
            else
            {
                MessageBox.Show("Cannot delete domain: the root level may contain one and only one domain",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddSuperDomainOnClick(WaveModel model, WaveDomainData waveDomain)
        {
            string name = PromptForValidDomainName(model);

            if (name != null)
            {
                var newDomain = new WaveDomainData(name);
                model.SyncWithModelDefaults(newDomain);
                if (CancelOnExistingFile(model, newDomain))
                {
                    return;
                }

                model.BeginEdit("Add exterior domain ...");
                model.OuterDomain = newDomain;
                model.AddSubDomain(newDomain, waveDomain);
                model.EndEdit();
            }
        }

        private void AddSubDomainOnClick(WaveModel model, WaveDomainData waveDomain)
        {
            string name = PromptForValidDomainName(model);

            if (name != null)
            {
                var newDomain = new WaveDomainData(name);
                model.SyncWithModelDefaults(newDomain);
                if (CancelOnExistingFile(model, newDomain))
                {
                    return;
                }

                model.AddSubDomain(waveDomain, newDomain);
            }
        }

        private static string PromptForValidDomainName(WaveModel model)
        {
            var dialog = new InputTextDialog
            {
                Text = "Enter domain name...",
                InitialText = "",
                ValidationMethod = s => WaveDomainHelper.IsValidDomainName(s, model),
                ValidationErrorMsg = "Please enter a unique and valid name (to be used as filename)"
            };

            return dialog.ShowDialog() == DialogResult.OK ? dialog.EnteredText : null;
        }

        private bool CancelOnExistingFile(WaveModel model, WaveDomainData domain)
        {
            string gridFile = Path.Combine(Path.GetDirectoryName(model.MdwFilePath), domain.GridFileName);
            string bedlevelFile = Path.Combine(Path.GetDirectoryName(model.MdwFilePath), domain.BedLevelFileName);

            if (File.Exists(gridFile))
            {
                DialogResult result = PromptOnUsingExistingFile(gridFile);
                if (result == DialogResult.Cancel)
                {
                    return true;
                }

                if (result == DialogResult.No)
                {
                    FileUtils.DeleteIfExists(gridFile);
                }
            }

            if (File.Exists(bedlevelFile))
            {
                DialogResult result = PromptOnUsingExistingFile(bedlevelFile);
                if (result == DialogResult.Cancel)
                {
                    return true;
                }

                if (result == DialogResult.No)
                {
                    FileUtils.DeleteIfExists(bedlevelFile);
                }
            }

            return false;
        }

        private DialogResult PromptOnUsingExistingFile(string filePath)
        {
            var msg = "File {0} already exists in the model directory, do you want to use its content?";
            DialogResult result = MessageBox.Show(string.Format(msg, filePath), "Use existing file?",
                                                  MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                                  MessageBoxDefaultButton.Button1);

            return result;
        }
    }
}
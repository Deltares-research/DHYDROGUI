using System;
using System.Diagnostics;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils;
using DeltaShell.NGHS.Common.Gui.NodePresenters;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters
{
    public abstract class FMSuiteNodePresenterBase<T> : TreeViewNodePresenterBaseForPluginGui<T> where T : class
    {
        private Image lastImageForEmptyData;
        private Image emptyDataImage;

        private bool firstUpdate = true;

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, T nodeData)
        {
            CheckNodeImageResourceLeak(nodeData);

            node.Text = GetNodeText(nodeData);
            Image image = GetNodeImage(nodeData);
            if (IsDataEmpty())
            {
                if (!ReferenceEquals(image, lastImageForEmptyData)) // create new empty image
                {
                    lastImageForEmptyData = image;
                    if (emptyDataImage != null)
                    {
                        emptyDataImage.Dispose();
                    }

                    emptyDataImage = CreateEmptyDataImage(image);
                }

                image = emptyDataImage;
            }

            node.Image = image;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            IMenuItem menuBase = base.GetContextMenu(sender, nodeData);
            IMenuItem menu = NodePresenterHelper.GetContextMenuFromPluginGuis(Gui, sender, nodeData);
            if (menuBase != null)
            {
                menu.Add(menuBase);
            }

            object data = GetContextMenuData((T) nodeData);
            menu.Add(new MenuItemContextMenuStripAdapter(ContextMenuFactory.CreateMenuFor(data, Gui, this, sender)));

            return menu;
        }

        protected abstract string GetNodeText(T data);
        protected abstract Image GetNodeImage(T data);

        protected virtual bool IsDataEmpty()
        {
            return false;
        }

        protected virtual object GetContextMenuData(T data)
        {
            return data;
        }

        protected void ResetGuiSelection()
        {
            Gui.Selection = null;
        }

        private static Image CreateEmptyDataImage(Image image)
        {
            var semiTransparentyImage = new Bitmap(image);
            using (Graphics graphics = Graphics.FromImage(semiTransparentyImage))
            {
                graphics.Clear(Color.Transparent);
                GraphicsUtils.DrawImageTransparent(graphics, image, 0.5f);
            }

            return semiTransparentyImage;
        }

        [Conditional("DEBUG")]
        private void CheckNodeImageResourceLeak(T data)
        {
            if (!firstUpdate)
            {
                return;
            }

            firstUpdate = false;
            Image image1 = GetNodeImage(data);
            Image image2 = GetNodeImage(data);
            if (!ReferenceEquals(image1, image2))
            {
                throw new InvalidOperationException(
                    string.Format("Apparent resource leak in {0}.GetNodeImage(): a new bitmap is returned each time, please reuse the bitmap",
                                  GetType().Name));
            }
        }
    }
}
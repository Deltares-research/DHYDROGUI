using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    public abstract class ModelTreeShortcut : IValueContainer
    {
        protected ModelTreeShortcut(string text, Bitmap image, IModel model, object value, ShortCutType shortCutType = ShortCutType.SettingsTab, IEnumerable<object> childObjects = null)
        {
            Text = text;
            Image = image;
            Model = model;
            Value = value;
            ChildObjects = childObjects;
            ShortCutType = shortCutType;
        }

        /// <summary>
        /// Text to show for the node
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Image to show for the node
        /// </summary>
        public Bitmap Image { get; }

        /// <summary>
        /// Model that this shortcut belongs to
        /// </summary>
        public IModel Model { get; }

        /// <summary>
        /// Sub-shortcuts for this shortcut
        /// </summary>
        public IEnumerable<object> ChildObjects { get; }

        /// <summary>
        /// Type of shortcut
        /// </summary>
        public ShortCutType ShortCutType { get; }

        /// <summary>
        /// Data value of the shortcut (object being wrapped)
        /// ShortCutType.SettingsTab = > name of the tab
        /// </summary>
        public object Value { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class EditTracersControlHelper : ICustomControlHelper
    {
        private TracerDefinitionsEditor editor;
        private IEventedList<string> data; 
        private IEventedList<string> Data
        {
            set
            {
                if (data != null)
                {
                    data.CollectionChanged -= Tracers_CollectionChanged;
                }

                data = value;
                editor.Data = data;

                if (data != null)
                {
                    data.CollectionChanged += Tracers_CollectionChanged;
                }
            }
        }

        private void Tracers_CollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            editor.UpdateListBox();
        }

        public Control CreateControl()
        {
            editor = new TracerDefinitionsEditor();
            editor.TracerAdded += Editor_TracerAdded;
            editor.TracerRemoved += Editor_TracerRemoved;
            return editor;
        }

        private void Editor_TracerRemoved(string s)
        {
            data.Remove(s);
        }

        private void Editor_TracerAdded(TracerDefinitionsEditor.TracerAddedEventArgs e)
        {
            string errorMessage;
            var nameValid = IsNameValid(e.Name, out errorMessage, WaterFlowFMModelDefinition.SpatialDataItemNames, data);

            if (nameValid)
            {
                data.Add(e.Name);
            }
            else
            {
                e.Cancelled = true;
                e.ErrorMessage = errorMessage;
            }
        }

        public static bool IsNameValid(string name, out string errorMessage, string[] defaultNames = null, ICollection<string> definedNames = null)
        {
            // check for empty name string
            if (string.IsNullOrEmpty(name))
            {
                errorMessage = "No name entered";
                return false;
            }

            var errorMessages = new List<string>();

            // check for default names first
            if (defaultNames != null && defaultNames.Any(n => n.StartsWith(name)))
            {
                errorMessages.Add(string.Format("The name '{0}' cannot be a known default name", name));
            }

            // check if name starts with number
            if (name.Length > 0 && Regex.IsMatch(name, @"^\d"))
            {
                errorMessages.Add(string.Format("The name '{0}' starts with a number", name));
            }

            // check if name is already defined
            if (definedNames != null && definedNames.Contains(name))
            {
                errorMessages.Add(string.Format("The name '{0}' is already defined", name));
            }

            // don't allow white spaces, slashes and back slashes in names
            var regex = new Regex(@"^[^\s\/\\]+$", RegexOptions.Multiline); 
            if (!regex.IsMatch(name))
            {
                errorMessages.Add(string.Format("The name '{0}', cannot contain spaces or (back-)slashes", name));
            }

            errorMessage = errorMessages.Count > 0 ? string.Join("\n\r",errorMessages) : null;

            return errorMessages.Count == 0;
        }

        public void SetData(Control control, object rootObject, object propertyValue)
        {
            var model = rootObject as WaterFlowFMModel;
            Data = model != null ? model.TracerDefinitions : null;
        }

        public bool HideCaptionAndUnitLabel()
        {
            return true;
        }
    }
}
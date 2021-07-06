using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Globalization;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public class AreaDictionaryEditorController<T>
    {
        private const int RowHeight = 22;
        private readonly AreaDictionaryEditor editor;
        private RainfallRunoffEnums.AreaUnit areaUnit;

        private AreaDictionary<T> data;
        private bool filling;
        private int internalEdits;

        public AreaDictionaryEditorController(AreaDictionaryEditor editor)
        {
            this.editor = editor;
        }

        public AreaDictionary<T> Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    ((INotifyPropertyChanged) data).PropertyChanged -= AreaDictionaryPropertyChanged;
                }

                data = value;

                if (data != null)
                {
                    BuildControls();
                    ((INotifyPropertyChanged) data).PropertyChanged += AreaDictionaryPropertyChanged;
                }
            }
        }

        public RainfallRunoffEnums.AreaUnit AreaUnit
        {
            get { return areaUnit; }
            set
            {
                areaUnit = value;
                TypeConverter converter = TypeDescriptor.GetConverter(typeof (RainfallRunoffEnums.AreaUnit));
                editor.UnitLabel = converter.ConvertToString(areaUnit);
                FillControls();
            }
        }

        #region Subscription

        private void SubscribeControls()
        {
            foreach (TextBox textbox in editor.ItemPanel.Controls.OfType<TextBox>())
            {
                textbox.Validating += TextboxValidating;
                textbox.TextChanged += TextboxTextChanged;
                textbox.Validated += TextboxValidated;
            }
        }

        private void UnsubscribeControls()
        {
            foreach (TextBox textbox in editor.ItemPanel.Controls.OfType<TextBox>())
            {
                textbox.Validating -= TextboxValidating;
                textbox.TextChanged -= TextboxTextChanged;
                textbox.Validated -= TextboxValidated;
            }
        }

        #endregion

        #region Validation and Events

        private void TextboxValidated(object sender, EventArgs e)
        {
            if (filling)
                return;

            var textBox = (TextBox) sender;

            if (textBox.Text == "")
            {
                textBox.Text = "0";
            }

            double value;
            if (Double.TryParse(textBox.Text, out value))
            {
                internalEdits++;
                Data[(T) textBox.Tag] = RainfallRunoffUnitConverter.ConvertArea(AreaUnit,
                                                                                RainfallRunoffEnums.AreaUnit.m2, value);
            }
        }

        private void TextboxValidating(object sender, CancelEventArgs e)
        {
            var textBox = (TextBox) sender;
            e.Cancel = !ValidateTextBox(textBox);
        }

        private bool ValidateTextBox(TextBox textBox)
        {
            if (filling)
                return true;

            if (textBox.Text == "")
            {
                return true;
            }

            double value;
            if (!Double.TryParse(textBox.Text, out value))
            {
                editor.ErrorProvider.SetError(textBox, "Cannot parse text to value");
                return false;
            }
            editor.ErrorProvider.SetError(textBox, "");
            return true;
        }


        private void TextboxTextChanged(object sender, EventArgs e)
        {
            ValidateTextBox((TextBox) sender);
            CalculateTotalArea();
        }

        private void CalculateTotalArea()
        {
            double sum = 0.0;
            foreach (TextBox textBox in editor.ItemPanel.Controls.OfType<TextBox>())
            {
                double value;
                if (Double.TryParse(textBox.Text, out value))
                {
                    sum += value;
                }
            }
            editor.TotalAreaText.Text = sum.ToString(RegionalSettingsManager.RealNumberFormat);
        }

        #endregion

        #region Fill control

        #region Delegates

        public delegate void ItemPanelResizeHandler(int desiredHeight);

        #endregion

        private void BuildControls()
        {
            UnsubscribeControls();

            editor.SuspendLayout();

            editor.ItemPanel.Controls.Clear();

            int index = 0;
            foreach (T item in data.Keys)
            {
                GenerateItem(item,
                             RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2, AreaUnit,
                                                                     data[item]), index);
                index++;
            }
            var numRows = (int) Math.Ceiling(index/2.0);
            int desiredHeight = numRows*(RowHeight + 1);
            editor.ItemPanel.Height = desiredHeight;
            editor.Height = editor.EmptyHeight + desiredHeight + 20;

            if (ItemPanelResized != null)
            {
                ItemPanelResized(desiredHeight);
            }

            SubscribeControls();
            CalculateTotalArea();

            editor.ResumeLayout();

            filling = false;
        }

        private void FillControls()
        {
            if (filling)
                return;

            filling = true;

            int index = 0;
            foreach (T item in data.Keys)
            {
                FillItem(editor.ItemPanel.Controls.OfType<TextBox>().ElementAt(index),
                         RainfallRunoffUnitConverter.ConvertArea(RainfallRunoffEnums.AreaUnit.m2, AreaUnit, data[item]));
                index++;
            }

            filling = false;
        }

        private void FillItem(TextBox textBox, double value)
        {
            textBox.Text = value.ToString(RegionalSettingsManager.RealNumberFormat);
        }

        public event ItemPanelResizeHandler ItemPanelResized;

        private void GenerateItem(T item, double value, int index)
        {
            const int margin = 10;
            int row = index/2;
            bool isLeft = index%2 == 0;

            int left = isLeft ? 0 : 250; //distance between lefts of columns
            int top = margin + row*RowHeight;

            TypeConverter typeConverter = TypeDescriptor.GetConverter(item.GetType()); //use nice string if available
            string itemString = (typeConverter != null) ? typeConverter.ConvertToString(item) : item.ToString();

            var label = new Label {Text = itemString, Width = 130, Left = left, Top = 3 + top, TextAlign = ContentAlignment.MiddleRight};
            var txtBox = new TextBox {Width = 100, Left = label.Right + margin, Top = top, Text = value.ToString(), Tag = item};

            editor.ItemPanel.Controls.Add(txtBox);
            editor.ItemPanel.Controls.Add(label);
        }

        #endregion

        private void AreaDictionaryPropertyChanged(object sender, EventArgs e)
        {
            if (internalEdits > 0)
            {
                internalEdits = 0;
                return;
            }
            FillControls();
        }
    }
}
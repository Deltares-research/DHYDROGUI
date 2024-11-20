using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Utils.Binding;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public partial class DepthLayerControl : UserControl
    {
        private DepthLayerType selectedDepthLayerType;
        private Keys lastKey;
        private DepthLayerDefinition depthLayerDefinition;

        public DepthLayerControl() : this(Enum.GetValues(typeof(DepthLayerType)).Cast<DepthLayerType>().ToList()) {}

        public DepthLayerControl(IList<DepthLayerType> supportedDepthLayerTypes)
        {
            List<KeyValuePair<DepthLayerType, string>> bindingSource =
                EnumBindingHelper.ToList<DepthLayerType>().Where(kvp => supportedDepthLayerTypes.Contains(kvp.Key)).ToList();

            CanSpecifyThicknesses = true;

            InitializeComponent();

            layerTypeComboBox.DataSource = bindingSource;
            layerTypeComboBox.DisplayMember = "Value";
            layerTypeComboBox.ValueMember = "Key";
            layerTypeComboBox.DataBindings.Add(new Binding("SelectedValue", this, "SelectedDepthLayerType", false,
                                                           DataSourceUpdateMode.OnPropertyChanged));
            layerCountTextBox.DataBindings.Add(new Binding("Text", this, "LayerCount", true,
                                                           DataSourceUpdateMode.OnValidation));
            layerCountTextBox.Enabled = false;
            layerCountTextBox.PreviewKeyDown += TextBoxPreviewKeyDown;
            layerCountTextBox.Validating += LayerCountTextBoxValidating;
            layerCountTextBox.Validated += LayerCountTextBoxValidated;
            picture.Paint += PicturePaint;

            layerThicknesses = new List<double>();
            selectedDepthLayerType = DepthLayerType.Single;
            layerCount = 1;
        }

        public Action<DepthLayerDefinition> AfterDepthLayerDefinitionCreated { private get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DepthLayerDefinition DepthLayerDefinition
        {
            get
            {
                UpdateDepthLayerDefinition();
                return depthLayerDefinition;
            }
            set
            {
                depthLayerDefinition = value;
                if (depthLayerDefinition == null)
                {
                    selectedDepthLayerType = DepthLayerType.Single;
                    layerThicknesses = new List<double>(new[]
                    {
                        1.0
                    });
                    layerCount = 1;
                    layerTypeComboBox.Enabled = false;
                }
                else
                {
                    selectedDepthLayerType = depthLayerDefinition.Type;
                    CopyLayerThicknesses();
                    layerTypeComboBox.Enabled = true;
                }

                RefreshView();
            }
        }

        public bool CanSpecifyThicknesses { get; set; }

        // needs to be public for data binding
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int LayerCount
        {
            get
            {
                return layerCount;
            }
            set
            {
                if (layerCount != value && value > 0)
                {
                    layerCount = value;
                    GenerateLayerThicknesses();
                    UpdateDepthLayerDefinition();
                    RefreshView();
                }
            }
        }

        // needs to be public for data binding
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DepthLayerType SelectedDepthLayerType
        {
            get
            {
                return selectedDepthLayerType;
            }
            set
            {
                if (value != selectedDepthLayerType)
                {
                    selectedDepthLayerType = value;
                    UpdateControl();
                    UpdateDepthLayerDefinition();
                }
            }
        }

        public static Color BlueShade(int i, int layers)
        {
            return ChangeColorBrightness(Color.DeepSkyBlue, 1 - ((2 * (float) (i + 1)) / (layers + 1)));
        }

        private DepthLayerDefinition CreateDepthLayerDefinition
        {
            get
            {
                switch (selectedDepthLayerType)
                {
                    case DepthLayerType.Single:
                        return new DepthLayerDefinition(DepthLayerType.Single, Enumerable.Empty<double>());
                    case DepthLayerType.Sigma:
                        return new DepthLayerDefinition(DepthLayerType.Sigma, layerThicknesses.Select(t => t / totalThickness));
                    case DepthLayerType.Z:
                        return new DepthLayerDefinition(DepthLayerType.Z, layerThicknesses.ToList());

                    default:
                        throw new ArgumentException("Depth layer type not supported");
                }
            }
        }

        private IEnumerable<TextBox> TextBoxes
        {
            get
            {
                return layersPanel.Controls.OfType<Control>().SelectMany(c => c.Controls.OfType<TextBox>());
            }
        }

        private void CopyLayerThicknesses()
        {
            switch (selectedDepthLayerType)
            {
                case DepthLayerType.Single:
                    layerThicknesses = new List<double>(new[]
                    {
                        1.0
                    });
                    layerCount = 1;
                    break;
                case DepthLayerType.Sigma:
                    layerThicknesses =
                        depthLayerDefinition.LayerThicknesses.Select(d => 100 * d).ToList();
                    layerCount = depthLayerDefinition.NumLayers;
                    break;
                case DepthLayerType.Z:
                    layerThicknesses = depthLayerDefinition.LayerThicknesses.ToList();
                    layerCount = depthLayerDefinition.NumLayers;
                    break;
            }
        }

        private void UpdateDepthLayerDefinition()
        {
            depthLayerDefinition = CreateDepthLayerDefinition;
            if (AfterDepthLayerDefinitionCreated != null)
            {
                AfterDepthLayerDefinitionCreated(depthLayerDefinition);
            }
        }

        private void UpdateControl()
        {
            int layers = SelectedDepthLayerType == DepthLayerType.Single ? 1 : 2;
            bool layersChanged = layers != LayerCount;
            LayerCount = layers;
            if (!layersChanged && layerCount > 0)
            {
                GenerateLayerThicknesses();
                UpdateDepthLayerDefinition();
                RefreshView();
            }
        }

        private void RefreshView()
        {
            layerCountTextBox.Enabled = SelectedDepthLayerType != DepthLayerType.Single;
            AutoScrollMinSize = new Size(165, 25 * (LayerCount + 1));
            FillDepthLayersPanel();
            UpdateTotalSum();
        }

        private void GenerateLayerThicknesses()
        {
            switch (SelectedDepthLayerType)
            {
                case DepthLayerType.Single:
                    layerThicknesses = new List<double>(new[]
                    {
                        0.0
                    });
                    break;
                case DepthLayerType.Sigma:
                    double fraction = 100.0 / LayerCount;
                    layerThicknesses.Clear();
                    for (var i = 0; i < LayerCount; ++i)
                    {
                        layerThicknesses.Add(fraction);
                    }

                    break;
                case DepthLayerType.Z:
                    layerThicknesses.Clear();
                    for (var i = 0; i < LayerCount; ++i)
                    {
                        layerThicknesses.Add(1.0);
                    }

                    break;
                default:
                    throw new ArgumentException("Depth layer type is not supported");
            }
        }

        private void GenerateColors()
        {
            colors = new List<Color>();
            if (SelectedDepthLayerType == DepthLayerType.Sigma || SelectedDepthLayerType == DepthLayerType.Z)
            {
                for (var i = 0; i < LayerCount; ++i)
                {
                    colors.Add(BlueShade(i, LayerCount));
                }
            }
        }

        private static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            var red = (float) color.R;
            var green = (float) color.G;
            var blue = (float) color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = ((255 - red) * correctionFactor) + red;
                green = ((255 - green) * correctionFactor) + green;
                blue = ((255 - blue) * correctionFactor) + blue;
            }

            return Color.FromArgb(color.A, (int) red, (int) green, (int) blue);
        }

        private Control CreateLayerEntry(int i, double thickness, string unit)
        {
            var control = new Control();

            var textBox = new TextBox
            {
                Location = new Point(55, 5),
                Width = 30,
                Text = Math.Round(thickness, 2).ToString(CultureInfo.InvariantCulture),
                CausesValidation = true,
                BorderStyle = BorderStyle.None,
                TabIndex = i + 2
            };
            textBox.PreviewKeyDown += TextBoxPreviewKeyDown;
            textBox.Validating += TextBoxValidating;
            textBox.Validated += TextBoxValidated;

            control.Controls.Add(textBox);
            control.Controls.Add(new Label
            {
                Text = unit,
                Location = new Point(85, 5)
            });
            control.Controls.Add(new Label
            {
                Text = "Layer " + i,
                Location = new Point(5, 5)
            });

            control.Enabled = CanSpecifyThicknesses;

            return control;
        }

        private void FillDepthLayersPanel()
        {
            foreach (TextBox textBox in layersPanel.Controls.OfType<TextBox>())
            {
                textBox.Validating -= TextBoxValidating;
                textBox.Validated -= TextBoxValidated;
                textBox.Dispose();
            }

            Controls.Remove(layersPanel);
            layersPanel = new TableLayoutPanel {Dock = DockStyle.Fill};
            Controls.Add(layersPanel);
            layersPanel.BringToFront();

            if (SelectedDepthLayerType == DepthLayerType.Single)
            {
                layersPanel.Visible = false;
                return;
            }

            layersPanel.Visible = true;

            layersPanel.SuspendLayout();
            int rowCount = LayerCount + 1;
            layersPanel.RowCount = rowCount;

            float height = (float) 100 / rowCount;

            for (var i = 0; i < rowCount; i++)
            {
                layersPanel.RowStyles.Add(new RowStyle(SizeType.Percent, height));
            }

            layersPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (var i = 0; i < LayerCount; i++)
            {
                Control control = CreateLayerEntry(LayerCount - i, layerThicknesses[i],
                                                   SelectedDepthLayerType == DepthLayerType.Sigma ? "%" : "m");
                control.Dock = DockStyle.Fill;
                layersPanel.Controls.Add(control, 0, i);
            }

            var totalSumLabel = new Label
            {
                Name = "totalSumLabel",
                Text = "Total: 100 " + (SelectedDepthLayerType == DepthLayerType.Sigma ? "%" : "m"),
                Padding = new Padding(5)
            };

            layersPanel.Controls.Add(totalSumLabel, 0, LayerCount);

            layersPanel.ResumeLayout();
        }

        private void PicturePaint(object sender, PaintEventArgs e)
        {
            if (SelectedDepthLayerType == DepthLayerType.Single)
            {
                return;
            }

            GenerateColors();
            Graphics graphics = e.Graphics;
            var picture = (PictureBox) sender;
            graphics.Clear(picture.BackColor);
            var offset = 0.0;
            var j = 0;
            foreach (double layerThickness in layerThicknesses.Reverse())
            {
                double height = (picture.Height * layerThickness) / totalThickness;
                graphics.FillRectangle(new SolidBrush(colors[j++]), 0, (float) offset,
                                       picture.Width, (float) height);
                offset += height;
            }
        }

        private void TextBoxPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            lastKey = e.KeyCode;
            if (e.KeyCode == Keys.Enter)
            {
                if (ReferenceEquals(sender, layerCountTextBox))
                {
                    ValidateChildren();
                }
                else
                {
                    var box = sender as TextBox;
                    if (box != null)
                    {
                        box.Parent.Focus();
                        box.Focus();
                    }
                }

                ((TextBox) sender).Select(0, ((TextBox) sender).Text.Length);
            }
        }

        private void LayerCountTextBoxValidating(object sender, CancelEventArgs e)
        {
            var textBox = (TextBox) sender;
            string text = textBox.Text;
            int result;
            if (!int.TryParse(text, out result))
            {
                e.Cancel = true;
                textBox.Select(0, text.Length);
                errorProvider.SetError(textBox, "value is not a valid number of layers");
                return;
            }

            if (result < 1 || result > 99)
            {
                e.Cancel = true;
                textBox.Select(0, text.Length);
                errorProvider.SetError(textBox, "value is not a valid number of layers");
                return;
            }

            errorProvider.SetError(textBox, string.Empty);
            e.Cancel = false;
            LayerCount = result;

            if (lastKey == Keys.Tab)
            {
                SelectNextControl(textBox, true, true, true, true);
            }
        }

        private void TextBoxValidating(object sender, CancelEventArgs e)
        {
            var textBox = (TextBox) sender;
            string text = textBox.Text;
            double result;
            if (double.TryParse(text, out result))
            {
                int max = selectedDepthLayerType == DepthLayerType.Sigma ? 100 : 1000000;

                if (layerThicknesses.Count == 1 || result > 0 && result < max)
                {
                    e.Cancel = false;
                    errorProvider.SetError(textBox, string.Empty);
                    int index = TextBoxes.ToList().IndexOf(textBox);
                    layerThicknesses[index] = result;
                    textBox.Text = Math.Round(result, 2).ToString(CultureInfo.InvariantCulture);
                    return;
                }
            }

            e.Cancel = true;
            textBox.Select(0, text.Length);
            if (SelectedDepthLayerType == DepthLayerType.Sigma)
            {
                errorProvider.SetError(textBox, "value is not a valid percentage");
            }

            if (SelectedDepthLayerType == DepthLayerType.Z)
            {
                errorProvider.SetError(textBox, "value is not a valid thickness");
            }
        }

        private void TextBoxValidated(object sender, EventArgs e)
        {
            totalThickness = 0;
            UpdateTotalSum();

            if (picture != null)
            {
                picture.Invalidate();
            }
        }

        private void UpdateTotalSum()
        {
            totalThickness = SelectedDepthLayerType == DepthLayerType.Sigma
                                 ? Math.Round(layerThicknesses.Sum())
                                 : layerThicknesses.Sum();

            Label totalSumLabel =
                layersPanel.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "totalSumLabel");
            if (totalSumLabel == null)
            {
                return;
            }

            totalSumLabel.Text = "Total: " + totalThickness + (SelectedDepthLayerType == DepthLayerType.Sigma ? "%" : " m");

            if (SelectedDepthLayerType == DepthLayerType.Sigma)
            {
                var integerSum = (int) totalThickness;
                totalSumLabel.ForeColor = integerSum != 100 ? Color.Red : Color.Black;
            }
        }

        private void LayerCountTextBoxValidated(object sender, EventArgs e)
        {
            layersPanel.Invalidate(true);
        }
    }
}
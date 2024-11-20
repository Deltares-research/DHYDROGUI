using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Functions.DelftTools.Utils.Tuples;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Unpaved
{
    public partial class ErnstZeeuwHellingaDrainageControl : UserControl
    {
        private readonly IList<CheckBox> checkBoxes;
        private readonly IList<TextBox> fromBoxes;
        private readonly IList<Panel> panels;
        private readonly IList<TextBox> toBoxes;
        private readonly IList<TextBox> valueBoxes;

        private ErnstDeZeeuwHellingaDrainageFormulaBase data;

        public ErnstZeeuwHellingaDrainageControl()
        {
            InitializeComponent();

            toBoxes = new List<TextBox>(new[] {txtLevelOneTo, txtLevelTwoTo, txtLevelThreeTo});
            fromBoxes = new List<TextBox>(new[] {txtLevelOneFrom, txtLevelTwoFrom, txtLevelThreeFrom, txtLevelFourFrom});
            checkBoxes = new List<CheckBox>(new[] {levelOneCkbox, levelTwoCkbox, levelThreeCkbox});
            panels = new List<Panel>(new[] {levelOnePanel, levelTwoPanel, levelThreePanel, levelFourPanel});
            valueBoxes = new List<TextBox>(new[] {levelOneValue, levelTwoValue, levelThreeValue, levelFourValue});

            SetMaskToTextboxes(0, false);
            SetMaskToTextboxes(1, false);
            SetMaskToTextboxes(2, false);
        }

        public ErnstDeZeeuwHellingaDrainageFormulaBase Data
        {
            get { return data; }
            set
            {
                data = value;

                if (data != null)
                {
                    ernstDeZeeuwHellingaDrainageFormulaBaseBindingSource.DataSource = data;
                    SetQuantity(data.IsErnst ? "Drainage resistance: [day]" : "Reaction factor: [1/day]");
                }
            }
        }

        /// <summary>
        /// Switch between Ernst / DeZeeuwHellinga
        /// </summary>
        /// <param name="quantity"></param>
        private void SetQuantity(string quantity)
        {
            reactionFactorLbl.Text = quantity;
        }

        private void ToTextBoxValidating(object sender, CancelEventArgs e)
        {
            var textBox = ((TextBox) sender);
            string text = textBox.Text;
            int level = toBoxes.IndexOf(textBox);

            double from, to;

            if (!Double.TryParse(text, out to))
            {
                errorProvider.SetError(textBox, "Invalid value");
                e.Cancel = true;
                return;
            }

            TextBox fromBox = fromBoxes[level];
            if (Double.TryParse(fromBox.Text, out @from) && @from >= to)
            {
                errorProvider.SetError(textBox, String.Format("Must be larger than {0:0.##}", to));
                return;
            }

            errorProvider.SetError(textBox, "");
        }

        private void ToTextBoxValidated(object sender, EventArgs e)
        {
            UpdateView();
        }

        private void CheckBoxCheckedChanged(object sender, EventArgs e)
        {
            var checkBox = (CheckBox) sender;
            int index = checkBoxes.IndexOf(checkBox);
            int nextIndex = index + 1;
            int previousIndex = index - 1;

            CheckBox previousCheckBox = previousIndex >= 0 ? checkBoxes[previousIndex] : null;
            CheckBox nextCheckBox = nextIndex < checkBoxes.Count ? checkBoxes[nextIndex] : null;

            if (nextCheckBox != null)
            {
                nextCheckBox.Enabled = checkBox.Checked;
            }
            if (previousCheckBox != null)
            {
                previousCheckBox.Enabled = !checkBox.Checked;
            }
        }

        private void PanelEnabledChanged(object sender, EventArgs e)
        {
            var panel = (Panel) sender;
            int level = panels.IndexOf(panel);

            if (!panel.Enabled)
            {
                errorProvider.SetError(toBoxes[level], ""); //clear any errors
            }

            SetMaskToTextboxes(level, panel.Enabled);

            UpdateView();
        }

        private void SetMaskToTextboxes(int level, bool enabled)
        {
            char mask = enabled ? (char) 0 : ' ';
            toBoxes[level].PasswordChar = mask;
            fromBoxes[level].PasswordChar = mask;
            valueBoxes[level].PasswordChar = mask;
        }

        private void UpdateInfiniteBox()
        {
            if (data == null)
                return;

            double maxToValue = 0.0;
            if (data.LevelOneEnabled)
                maxToValue = Math.Max(maxToValue, data.LevelOneTo);
            if (data.LevelTwoEnabled)
                maxToValue = Math.Max(maxToValue, data.LevelTwoTo);
            if (data.LevelThreeEnabled)
                maxToValue = Math.Max(maxToValue, data.LevelThreeTo);

            txtLevelFourFrom.Text = maxToValue.ToString();
        }

        private void ValuesChanged(object sender, EventArgs e)
        {
            UpdateView();
        }

        private void UpdateView()
        {
            UpdateInfiniteBox();
            UpdateImage();
        }

        private void ErnstZeeuwHellingaDrainageControlLoad(object sender, EventArgs e)
        {
            UpdateView();
        }

        #region Rendering

        private void UpdateImage()
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
                pictureBox.Image = null;
            }

            if (data == null)
                return;

            var smallFont = new Font(new FontFamily("Arial"), 8f);
            var runoffPen = new Pen(Color.Green, 3f);
            var bedPen = new Pen(Color.DarkRed, 3f);
            var waterPen = new Pen(Color.DarkBlue, 3f);
            int w = pictureBox.Width;
            int h = pictureBox.Height;

            var bitmap = new Bitmap(w, h);
            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            float textMargin = 0.07f;
            float margin = 0.1f;
            float waterMargin = (0.1f + margin);
            float levelsMargin = (0.05f + margin);
            float lineMargin = 0.25f;
            float lineWidth = lineMargin*w;
            float waterTop = (1f - waterMargin)*h;
            float waterBottom = (1f - margin)*h;

            float baseOffset = ((lineMargin + margin)*w);

            float dy = (1f - 2*margin);
            float dx = (1f - (2*(margin + lineMargin)));
            float alfa = dy/dx;
            float beta = margin*h;

            float waterSurfaceLeft = ((waterTop - beta)/alfa) + baseOffset;
            float waterSurfaceLeftBottom = ((waterBottom - beta)/alfa) + baseOffset;

            var leftTop = new PointF(margin*w, margin*h);
            var rightTop = new PointF(leftTop.X + lineWidth, leftTop.Y);

            var waterRightTop = new PointF((1f - margin)*w, waterTop);
            var waterRightBottom = new PointF((1f - margin)*w, (1f - margin)*h);
            var waterLeftTop = new PointF(waterSurfaceLeft, waterRightTop.Y);
            var waterLeftBottom = new PointF(waterSurfaceLeftBottom, (1f - margin)*h);

            graphics.DrawLines(waterPen, new[] {waterLeftTop, waterRightTop});
            graphics.DrawLines(waterPen, new[] {waterLeftTop, waterLeftBottom});
            graphics.DrawLines(waterPen, new[] {waterLeftBottom, waterRightBottom});
            graphics.DrawLines(bedPen, new[] {leftTop, rightTop, waterLeftTop});

            double surface;
            if (Double.TryParse(surfaceValue.Text, out surface))
            {
                graphics.DrawString(String.Format("Surface R = {0:0.#####} >>", surface), smallFont, Brushes.Black,
                                    textMargin*w, 2);
            }

            List<Pair<double, double>> levelsAndValues = GetLevelsAndValues().Reverse().ToList();
            if (levelsAndValues.Count > 0)
            {
                float previousRatio = 1f - margin;
                double maxLevel = levelsAndValues.Max(p => p.First);
                foreach (var pair in levelsAndValues)
                {
                    double newRatio = (pair.First/maxLevel) - levelsMargin;
                    var effectiveRatio = (float) Math.Max(margin, Math.Min(previousRatio - textMargin, newRatio));
                        //make sure they are nicely spaced
                    float height = effectiveRatio*h;
                    float x = ((height - beta)/alfa) + baseOffset;
                    graphics.DrawLines(runoffPen, new[] {new PointF(textMargin*w, height), new PointF(x, height)});
                    graphics.DrawString(String.Format("R = {0:0.#####} >>", pair.Second), smallFont, Brushes.Black,
                                        textMargin*w, ((effectiveRatio - textMargin)*h));
                    previousRatio = effectiveRatio;
                }
            }
            double infinity;
            if (Double.TryParse(levelFourValue.Text, out infinity))
            {
                graphics.DrawString(String.Format("R = {0:0.#####} >>", infinity), smallFont, Brushes.Black,
                                    textMargin*w, h*(1f - levelsMargin));
            }
            double inflow;
            if (Double.TryParse(horizontalInflowValue.Text, out inflow))
            {
                string inflowText = String.Format("<< R = {0:0.#####}", inflow);
                SizeF measure = graphics.MeasureString(inflowText, smallFont);
                graphics.DrawString(inflowText, smallFont, Brushes.Black, ((1f - margin)*w) - measure.Width,
                                    3 + (h*(1f - (waterMargin))));
            }

            graphics.Dispose();
            bedPen.Dispose();

            pictureBox.Image = bitmap;
        }

        private IEnumerable<Pair<double, double>> GetLevelsAndValues()
        {
            for (int i = 0; i < toBoxes.Count; i++)
            {
                TextBox box = toBoxes[i];
                if (box.Enabled)
                {
                    double level;
                    double value;
                    if (Double.TryParse(box.Text, out level) && 
                        Double.TryParse(valueBoxes[i].Text, out value))
                    {
                        yield return new Pair<double, double>(level, value);
                    }
                }
                else
                {
                    yield break;
                }
            }
        }

        #endregion
    }
}
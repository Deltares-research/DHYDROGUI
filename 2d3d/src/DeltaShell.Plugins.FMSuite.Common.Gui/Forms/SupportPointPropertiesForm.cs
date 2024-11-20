using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.CoordinateSystems.Transformations;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    public partial class SupportPointPropertiesForm : Form
    {
        private readonly BoundaryConditionSet boundaryConditionSet;
        private readonly int index;
        private readonly IList<string> otherSupportPointNames;
        private bool canceling;

        public SupportPointPropertiesForm(BoundaryConditionSet boundaryConditionSet, int index, ICoordinateSystem coordinateSystem)
        {
            this.boundaryConditionSet = boundaryConditionSet;
            this.index = index;
            otherSupportPointNames = new List<string>(boundaryConditionSet.SupportPointNames);
            otherSupportPointNames.RemoveAt(index);

            InitializeComponent();

            errorProvider.SetIconAlignment(textBox1, ErrorIconAlignment.BottomRight);

            IGeometry geometry = boundaryConditionSet.Feature.Geometry;

            Coordinate coordinate = geometry.Coordinates[index];

            XCoordinateLabel.Text = coordinate.X.ToString("F2");
            YCoordinateLabel.Text = coordinate.Y.ToString("F2");

            double distanceToStartingPoint;
            if (coordinateSystem != null)
            {
                distanceToStartingPoint = 0;
                for (var i = 1; i < index; ++i)
                {
                    distanceToStartingPoint += GeodeticDistance.Distance(coordinateSystem, geometry.Coordinates[i],
                                                                         geometry.Coordinates[i - 1]);
                }
            }
            else
            {
                distanceToStartingPoint = GeometryHelper.LineStringGetDistance(geometry as LineString, index);
            }

            ChainageLabel.Text = distanceToStartingPoint.ToString("F2");

            string name = boundaryConditionSet.SupportPointNames[index];

            textBox1.Text = name;
            textBox1.CausesValidation = true;
            textBox1.Validating += TextBoxValidating;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                canceling = true;
            }

            return base.ProcessDialogKey(keyData);
        }

        private void TextBoxValidating(object sender, CancelEventArgs e)
        {
            if (canceling)
            {
                canceling = false;
                e.Cancel = false;
                return;
            }

            if (string.IsNullOrEmpty(textBox1.Text))
            {
                errorProvider.SetError(textBox1, "Invalid support point name entered");
                e.Cancel = true;
            }
            else if (otherSupportPointNames.Contains(textBox1.Text))
            {
                errorProvider.SetError(textBox1, "Support point name already taken");
                e.Cancel = true;
            }
            else
            {
                errorProvider.SetError(textBox1, "");
            }
        }

        private void OkButtonClick(object sender, EventArgs e)
        {
            boundaryConditionSet.SupportPointNames[index] = textBox1.Text;
            DialogResult = DialogResult.OK;
        }

        private void CancelButtonClick(object sender, EventArgs e)
        {
            canceling = true;
            DialogResult = DialogResult.Cancel;
        }

        private void defaultNameButton_Click(object sender, EventArgs e)
        {
            textBox1.Text = BoundaryConditionSet.DefaultLocationName(boundaryConditionSet.Feature, index);
        }
    }
}
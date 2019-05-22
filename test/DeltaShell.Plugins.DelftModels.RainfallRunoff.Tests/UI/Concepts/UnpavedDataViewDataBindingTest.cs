using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Concepts
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class UnpavedDataViewDataBindingTest
    {
        [Test]
        [Ignore("A bit too fragile for the build server, useful for local debugging")]
        public void BruteForceDatabindingCheck()
        {
            var unpaved = new UnpavedData(new Catchment());
            var properties = ReflectionTestHelper.GetPublicInstanceProperties(unpaved);
            var originalValues = properties.Select(p => p.GetValue(unpaved, null)).ToList();
            var unpavedDataView = new UnpavedDataView { Data = unpaved };

            WindowsFormsTestHelper.ShowModal(unpavedDataView,
                                             f =>
                                                 {
                                                     var controls =
                                                         unpavedDataView.Controls.OfType<Control>().SelectMany(
                                                             c => c.Controls.OfType<Control>());

                                                     RandomFillTextBoxes(controls);
                                                     CycleComboBoxes(controls);
                                                     ToggleCheckBoxes(controls);
                                                     RandomFillTextBoxes(controls);
                                                     CycleComboBoxes(controls);
                                                     RandomFillTextBoxes(controls);
                                                     ToggleRadioButtons(controls);
                                                     RandomFillTextBoxes(controls);
                                                     ToggleRadioButtons(controls);
                                                     RandomFillTextBoxes(controls);

                                                     unpavedDataView.Focus();

                                                     var newValues = properties.Select(p => p.GetValue(unpaved, null)).ToList();

                                                     var failing = new List<string>();
                                                     var succeeding = new List<string>();

                                                     for (var i = 0; i < originalValues.Count; i++)
                                                     {
                                                         var property = properties.ElementAt(i).ToString();
                                                         if (!originalValues[i].Equals(newValues[i]))
                                                         {
                                                             succeeding.Add(property);
                                                         }
                                                         else
                                                         {
                                                             failing.Add(property);
                                                         }
                                                     }

                                                     Console.WriteLine("Succeeding:");
                                                     succeeding.ForEach(prop => Console.WriteLine("\t" + prop));

                                                     Console.WriteLine("Failing:");
                                                     failing.ForEach(prop => Console.WriteLine("\t" + prop));

                                                     Assert.AreEqual(0, failing.Count);
                                                 });
        }

        private static void ToggleRadioButtons(IEnumerable<Control> controls)
        {
            foreach (var radioGroup in controls.OfType<RadioButton>().Select(r => r.Parent).Distinct())
            {
                var radioButtons = radioGroup.Controls.OfType<RadioButton>().OrderBy(c => c.Top);

                var checkedFound = false;
                foreach (var radio in radioButtons)
                {
                    if (radio.Checked)
                    {
                        checkedFound = true;
                    }
                    else if (checkedFound)
                    {
                        radio.Checked = true;
                        break;
                    }
                }
            }
        }

        private static void CycleComboBoxes(IEnumerable<Control> controls)
        {
            foreach (var combo in controls.OfType<ComboBox>())
            {
                combo.Focus();

                if (!combo.Enabled)
                {
                    Console.WriteLine(combo.Name + " skipped");
                    continue;
                }
                if (combo.SelectedIndex < combo.Items.Count - 1)
                {
                    combo.SelectedIndex++;
                }
            }
        }

        private static void ToggleCheckBoxes(IEnumerable<Control> controls)
        {
            foreach (var checkBox in controls.OfType<CheckBox>())
            {
                checkBox.Focus();

                checkBox.Checked = !checkBox.Checked;
            }
        }

        private static void RandomFillTextBoxes(IEnumerable<Control> controls)
        {
            foreach (var control in controls.OfType<TextBox>())
            {
                control.Focus();
                control.Text = "555";
            }
        } 
    }
}
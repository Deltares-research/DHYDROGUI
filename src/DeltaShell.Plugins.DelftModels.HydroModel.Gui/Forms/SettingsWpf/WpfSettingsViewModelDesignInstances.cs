using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Creates a dummy design instance to help layout all controls for the <see cref="WpfSettingsView"/>
    /// </summary>
    public static class WpfSettingsViewModelDesignInstances
    {
        public static readonly WpfSettingsViewModel AllTypes = MakeDesignInstance();

        private static WpfSettingsViewModel MakeDesignInstance()
        {
            var wpfSettingsViewModel = new WpfSettingsViewModel
            {
                SettingsCategories = new ObservableCollection<WpfGuiCategory>
                {
                    new WpfGuiCategory("Category 1", new List<FieldUIDescription>
                    {
                        new FieldUIDescription((o) => 1, (o, o1) => {})
                        {
                            Name = "Test parameter 1 (int)",
                            Category = "Test category 1",
                            ValueType = typeof(int),
                            Label = "Label Test parameter 1 (int)",
                            SubCategory = "Sub category 1",
                            UnitSymbol = "abc def ghi"
                        },
                        new FieldUIDescription((o) => 2.0, (o, o1) => {})
                        {
                            Name = "Test parameter 2 (double)",
                            Category = "Test category 1",
                            ValueType = typeof(double),
                            Label = "Label Test parameter 2 (double)",
                            SubCategory = "Sub category 1",
                            UnitSymbol = "abc def"
                        },
                        new FieldUIDescription((o) => true, (o, o1) => {})
                        {
                            Name = "Test parameter 3 (bool)",
                            Category = "Test category 1",
                            ValueType = typeof(bool),
                            Label = "Label Test parameter 3 (bool)",
                            SubCategory = "Sub category 2"
                        },
                        new FieldUIDescription((o) => DateTime.Now, (o, o1) => {})
                        {
                            Name = "Test parameter 4 (DateTime)",
                            Category = "Test category 1",
                            ValueType = typeof(DateTime),
                            Label = "Label Test parameter 4 (DateTime)",
                            SubCategory = "Sub category 2"
                        },
                        new FieldUIDescription((o) => new TimeSpan(1, 13, 52, 12), (o, o1) => {})
                        {
                            Name = "Test parameter 5 (TimeSpan)",
                            Category = "Test category 1",
                            ValueType = typeof(TimeSpan),
                            Label = "Label Test parameter 5 (TimeSpan)",
                            SubCategory = "Sub category 2"
                        },
                        new FieldUIDescription((o) => "abc", (o, o1) => {})
                        {
                            Name = "Test parameter 6 (String)",
                            Category = "Test category 1",
                            ValueType = typeof(string),
                            Label = "Label Test parameter 6 (String)",
                            SubCategory = "Sub category 2"
                        },
                        new FieldUIDescription((o) => "abc", (o, o1) => {})
                        {
                            Name = "Test parameter 7 (String)",
                            Category = "Test category 1",
                            ValueType = typeof(string),
                            Label = "A very long label for testing text wrapping etc.",
                            SubCategory = "Sub category 2"
                        },
                        new FieldUIDescription((o) => ModelGroup.FMWaveRtcModels, (o, o1) => {})
                        {
                            Name = "Test parameter 8 (ModelGroup/ enum)",
                            Category = "Test category 1",
                            ValueType = typeof(ModelGroup),
                            Label = "Label Test parameter 8 (ModelGroup/ enum)",
                            SubCategory = "Sub category 1"
                        },
                        new FieldUIDescription((o) => new List<double>(new[]
                        {
                            1.0,
                            2.0,
                            3.0,
                            4.0
                        }), (o, o1) => {})
                        {
                            Name = "Test parameter 9 (List<double>)",
                            Category = "Test category 1",
                            ValueType = typeof(IList<double>),
                            Label = "Label Test parameter 9 (List<double>)",
                            SubCategory = "Sub category 1"
                        },
                        new FieldUIDescription((o) => "abc", (o, o1) => {})
                        {
                            Name = "Test parameter 10 (String)",
                            Category = "Test category 1",
                            ValueType = typeof(string),
                            Label = "Label Test parameter 10 (String)",
                            SubCategory = "Sub category 3"
                        },
                        new FieldUIDescription((o) => "abc", (o, o1) => {})
                        {
                            Name = "Test parameter 11 (String)",
                            Category = "Test category 1",
                            ValueType = typeof(string),
                            Label = "Label Test parameter 11 (String)",
                            SubCategory = "Sub category 3"
                        },
                        new FieldUIDescription((o) => "abc", (o, o1) => {})
                        {
                            Name = "Test parameter 12 (String)",
                            Category = "Test category 1",
                            ValueType = typeof(string),
                            Label = "Label Test parameter 12 (String)",
                            SubCategory = "Sub category 4"
                        }
                    }),
                    new WpfGuiCategory("Category 2", new List<FieldUIDescription>
                    {
                        new FieldUIDescription((o) => 1, (o, o1) => {})
                        {
                            Name = "Test parameter 1 (int)",
                            Category = "Test category 2",
                            ValueType = typeof(int),
                            Label = "Label Test parameter 1 (int)",
                            SubCategory = "Sub category 1"
                        }
                    })
                }
            };

            // add a command for a property
            wpfSettingsViewModel.SettingsCategories[0].SubCategories[2].Properties.ElementAt(1).CustomCommand = new CommandHelper(() => {});

            // add a sub category with custom control

            wpfSettingsViewModel.SettingsCategories[0].SubCategories[3].CustomControl = new UserControl
            {
                Content = new TextBlock
                {
                    Text = "Custom sub category user control",
                    Background = new SolidColorBrush(Colors.Orange)
                }
            };

            // add a category with custom control
            var wpfGuiCategory = new WpfGuiCategory("Custom category", new List<FieldUIDescription>())
            {
                CustomControl = new UserControl
                {
                    Content = new TextBlock
                    {
                        Text = "Custom category user control",
                        Background = new SolidColorBrush(Colors.Brown)
                    }
                }
            };

            wpfSettingsViewModel.SettingsCategories.Add(wpfGuiCategory);

            return wpfSettingsViewModel;
        }
    }
}
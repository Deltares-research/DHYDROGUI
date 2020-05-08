using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.FMSuite.Common.Toolboxes;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors.Buttons
{
    public abstract class ToolboxesPanel : ICustomControlHelper
    {
        private static ILog log = LogManager.GetLogger(typeof(ToolboxesPanel));

        public Control CreateControl()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            try
            {
                string toolboxPath = GetToolBoxesDirectory();
                // load commands from disk (cache?)
                foreach (ScriptCommand command in ToolboxCommands.LoadFrom(toolboxPath))
                {
                    ScriptCommand localCommand = command;
                    var button = new Button
                    {
                        Image = command.Image,
                        TextImageRelation = TextImageRelation.ImageAboveText,
                        TextAlign = ContentAlignment.BottomCenter,
                        Text = command.Title,
                        Width = 120,
                        Height = 60
                    };

                    button.Click += (s, e) =>
                    {
                        Cursor oldCursor = panel.Cursor;
                        panel.Cursor = Cursors.WaitCursor;

                        RunCommand(localCommand);

                        panel.Cursor = oldCursor;
                    };
                    panel.Controls.Add(button);
                }
            }
            catch (Exception e)
            {
                panel.Controls.Add(new Label
                {
                    AutoSize = true,
                    Text = string.Format("Unable to load toolboxes: {0}", e.Message)
                });
            }

            return panel;
        }

        public void SetData(Control control, object rootObject, object propertyValue)
        {
            Model = (IModel) rootObject;
        }

        public bool HideCaptionAndUnitLabel()
        {
            return true;
        }

        protected IModel Model { get; set; }
        protected abstract string GetToolBoxesDirectory();
        protected abstract Dictionary<string, object> GetScriptPredefinedVariables();

        private void RunCommand(ScriptCommand command)
        {
            var logger = new ScriptLogger();

            var errors = new List<string>();
            try
            {
                // run the command (script)
                command.Execute(logger, GetScriptPredefinedVariables());

                // forward any log messages to the message window / log file
                foreach (string info in logger.Infos)
                {
                    log.Info(info);
                }

                foreach (string error in logger.Errors)
                {
                    log.Error(error);
                }

                errors.AddRange(logger.Errors.ToList());
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
            }

            // if any errors / exceptions occurred, show a message box with those errors
            if (errors.Any())
            {
                MessageBox.Show(string.Join(Environment.NewLine, errors), "Errors while running command",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private class ScriptLogger : IScriptLogger
        {
            public readonly List<string> Infos = new List<string>();
            public readonly List<string> Errors = new List<string>();

            public void Info(string format, params string[] args)
            {
                Infos.Add(string.Format(format, args));
            }

            public void Error(string format, params string[] args)
            {
                Errors.Add(string.Format(format, args));
            }
        }
    }
}
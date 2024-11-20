using System.Collections.Specialized;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors
{
    [TestFixture]
    public class TracerDefinitionsEditorWpfViewModelTest
    {
        [Test]
        public void UpdatesOfTracersIsSyncedWithTracerList()
        {
            var tracersList = new EventedList<string>
            {
                "abc",
                "def"
            };

            var addCount = 0;
            var removeCount = 0;

            tracersList.CollectionChanged += (s, a) =>
            {
                switch (a.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        addCount++;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        removeCount++;
                        break;
                }
            };

            var viewmodel = new TracerDefinitionsEditorWpfViewModel {TracersList = tracersList};

            Assert.AreEqual(viewmodel.TracersList, viewmodel.Tracers);

            viewmodel.Tracers.Add("ghi");

            Assert.AreEqual(viewmodel.TracersList, viewmodel.Tracers);
            Assert.AreEqual(1, addCount);

            viewmodel.Tracers.Remove("abc");

            Assert.AreEqual(viewmodel.TracersList, viewmodel.Tracers);
            Assert.AreEqual(1, removeCount);
        }

        [Test]
        [TestCase("", false, "No name entered")]
        [TestCase("1T", false, "The name '1T' starts with a number")]
        [TestCase("T1", true, null)]
        [TestCase("DuplicateTest", false, "The name 'DuplicateTest' is already defined")]
        [TestCase("123/", false, "The name '123/' starts with a number\n\rThe name '123/', cannot contain spaces or (back-)slashes")]
        [TestCase("asdf\\", false, "The name 'asdf\\', cannot contain spaces or (back-)slashes")]
        [TestCase("-Name", true, null)]
        [TestCase(WaterFlowFMModelDefinition.BathymetryDataItemName, false, "The name 'Bed Level' cannot be a known default name\n\rThe name 'Bed Level', cannot contain spaces or (back-)slashes")]
        public void AddTracerCommandCanExecuteShouldCheckParameterValidity(string parameter, bool canExecute, string message)
        {
            var viewmodel = new TracerDefinitionsEditorWpfViewModel {TracersList = new EventedList<string> {"DuplicateTest"}};

            bool canExecuteResult = viewmodel.AddTracerCommand.CanExecute(parameter);

            Assert.AreEqual(canExecute, canExecuteResult);
            Assert.AreEqual(viewmodel.CanAddMessage, message);
        }

        [Test]
        public void AddTracerCommandAddsATracer()
        {
            var viewmodel = new TracerDefinitionsEditorWpfViewModel {TracersList = new EventedList<string> {"abc"}};

            viewmodel.AddTracerCommand.Execute("def");

            Assert.AreEqual(2, viewmodel.TracersList.Count);
        }

        [Test]
        public void RemoveTracerCommandShouldCallMayRemoveFunction()
        {
            var viewmodel = new TracerDefinitionsEditorWpfViewModel
            {
                TracersList = new EventedList<string>
                {
                    "abc",
                    "def",
                    "ghi"
                }
            };

            // no MayRemove set
            viewmodel.RemoveTracerCommand.Execute("abc");
            Assert.AreEqual(2, viewmodel.TracersList.Count);

            // MayRemove canceling
            var mayRemoveCount = 0;
            viewmodel.MayRemove = (s) =>
            {
                mayRemoveCount++;
                return false;
            };

            viewmodel.RemoveTracerCommand.Execute("def");

            Assert.AreEqual(2, viewmodel.TracersList.Count);
            Assert.AreEqual(mayRemoveCount, 1);

            // MayRemove continuing
            viewmodel.MayRemove = (s) =>
            {
                mayRemoveCount++;
                return true;
            };

            viewmodel.RemoveTracerCommand.Execute("def");

            Assert.AreEqual(1, viewmodel.TracersList.Count);
            Assert.AreEqual(mayRemoveCount, 2);
        }
    }
}
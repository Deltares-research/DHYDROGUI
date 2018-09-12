using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.BoundaryConditionEditor;
using NUnit.Framework;
using System;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors
{
    [TestFixture]
    class WaveBoundaryConditionDataViewTest
    {
        [Test]
        public void GivenAWaveModel_WhenGettingTheStartAndStopTimeForTimeSeriesDialog_ThenStartTimeIsEqualToModelReferenceDate()
        {
            var view = new WaveBoundaryConditionDataView();
            view.Model = new WaveModel();

            var startTime = TypeUtils.GetPropertyValue(view, "StartTime");
            var stopTime = TypeUtils.GetPropertyValue(view, "StopTime");

            Assert.AreEqual(startTime, view.Model.ModelDefinition.ModelReferenceDateTime);
            Assert.AreEqual(stopTime, ((DateTime)startTime).AddDays(1));
        }
    }
}

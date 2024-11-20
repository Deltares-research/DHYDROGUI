using System.Threading;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class HydroModelSettingsTest
    {
        [Test] 
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void Show()
        {
            var flow = new HydroModelTest.SimpleModel { Name = "water flow model" };
            var rr = new HydroModelTest.SimpleModel { Name = "rainfall-runoff model" };
            var waq = new HydroModelTest.SimpleModel { Name = "water quality model" };
            var rtc = new HydroModelTest.SimpleModel { Name = "real-time control model" };

            var w1 = new SequentialActivity
                         {
                             Name = "flow only", 
                             Activities = { new ActivityWrapper { Activity = flow } }
                         }; 
            
            var w2 = new SequentialActivity { Name = "flow and rr (parallel) + waq", 
                Activities = {
                    new ParallelActivity { Name = "parallel", Activities = { new ActivityWrapper { Activity = flow }, new ActivityWrapper { Activity = rtc } } },
                    new ActivityWrapper { Activity = waq }
                    }
            };

            var w3 = new SequentialActivity
            {
                Name = "rr + flow and rtc (parallel) + waq",
                Activities = {
                    new ActivityWrapper { Activity = rr },
                    new ParallelActivity { Name = "parallel", Activities = { new ActivityWrapper { Activity = flow }, new ActivityWrapper { Activity = rtc } } },
                    new ActivityWrapper { Activity = waq }
                    }
            };
            
            var model = new HydroModel { Activities = { rr, flow, rtc, waq }, Workflows = { w1, w2, w3 }, CurrentWorkflow = w1 };
            
            var form = new HydroModelSettings { Data = model };
            
            WindowsFormsTestHelper.ShowModal(form);
        }
    }
}
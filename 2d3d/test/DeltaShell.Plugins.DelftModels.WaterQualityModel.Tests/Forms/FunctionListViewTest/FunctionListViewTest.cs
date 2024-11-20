using System;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.FunctionListView;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Forms.FunctionListViewTest
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class FunctionListViewTest
    {
        [Test]
        public void TestShowFunctionListViewEmpty()
        {
            var functionListView = new FunctionListView();

            WindowsFormsTestHelper.ShowModal(functionListView);
        }

        [Test]
        public void TestShowFunctionListViewWithoutData()
        {
            var functionListView = new FunctionListView {Data = null};

            WindowsFormsTestHelper.ShowModal(functionListView);
        }

        [Test]
        public void TestShowFunctionListViewWithData()
        {
            var functionListView = new FunctionListView {Data = CreateFunctionList()};

            Assert.IsFalse(functionListView.ShowArguments);
            Assert.IsFalse(functionListView.ShowComponents);
            Assert.IsTrue(functionListView.ShowNamesReadOnly);
            Assert.IsTrue(functionListView.ShowUnitsReadOnly);
            Assert.IsTrue(functionListView.ShowEditButtons);

            WindowsFormsTestHelper.ShowModal(functionListView);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void TestShowFunctionListViewWithDataAndFunctionCreators()
        {
            using (var gui = new DHYDROGuiBuilder().Build())
            {
                gui.Run();

                var mockRep = new MockRepository();
                var functionCreator = mockRep.StrictMock<IFunctionTypeCreator>();

                SetupResult.For(functionCreator.FunctionTypeName).Return("Function type name");
                Expect.Call(functionCreator.IsThisFunctionType(new Function())).IgnoreArguments().Return(true).Repeat.Any();
                Expect.Call(functionCreator.TransformToFunctionType(new Function())).IgnoreArguments().Return(new Function("New function")).Repeat.Any();
                Expect.Call(functionCreator.GetDefaultValueForFunction(new Function())).IgnoreArguments().Return(3.0).Repeat.Any();
                Expect.Call(functionCreator.GetUnitForFunction(new Function())).IgnoreArguments().Return("mg/L").Repeat.Any();
                Expect.Call(functionCreator.GetUrlForFunction(new Function())).IgnoreArguments().Return(string.Empty).Repeat.Any();
                Expect.Call(functionCreator.IsAllowed(new Function())).IgnoreArguments().Return(false).Repeat.Any();
                mockRep.ReplayAll();

                var functionListView = new FunctionListView
                {
                    Gui = gui,
                    Data = CreateFunctionList(),
                    ShowArguments = true,
                    ShowComponents = true
                };

                functionListView.FunctionCreators.Add(functionCreator);
                gui.DocumentViewsResolver.DefaultViewTypes.Add(typeof(Function), typeof(FunctionView));
                WindowsFormsTestHelper.ShowModal(functionListView, delegate
                {
                    ((IEventedList<IFunction>) functionListView.Data).Add(new Function("function 5")); // Add a function after the view is created
                    TypeUtils.CallPrivateMethod(functionListView, "OpenViewForFunction");              // Open a view for the selected function

                    functionListView.ShowEditButtons = false; // Hide the edit buttons
                });
            }
        }

        [Test]
        public void TestExcludeNames()
        {
            var functionListView = new FunctionListView
            {
                ShowArguments = true,
                ShowComponents = true
            };

            functionListView.ExcludeList.Add("function 2");
            functionListView.Data = CreateFunctionList();

            WindowsFormsTestHelper.ShowModal(functionListView);

            // check manually that function 2 is excluded from the list
        }

        [Test]
        public void TestFunctionListView_SetInitialValueColumn_True_UpdatesDefaultValueColumnCaption()
        {
            var listView = new FunctionListView {UseInitialValueColumn = true};
            ITableViewColumn column = TypeUtils.GetField<FunctionListView, ITableViewColumn>(listView, "defaultValueColumn");
            Assert.AreEqual(Resources.FunctionListView_GetDefaultValueColumnName_Initial_value, column.Caption);

            listView.UseInitialValueColumn = false;
            Assert.AreEqual(Resources.FunctionListView_InitializeTableView_Default_value, column.Caption);
        }

        [Test]
        public void TestFunctionListView_SetInitialValueColumn_False_UpdatesDefaultValueColumnCaption()
        {
            var listView = new FunctionListView {UseInitialValueColumn = false};
            ITableViewColumn column = TypeUtils.GetField<FunctionListView, ITableViewColumn>(listView, "defaultValueColumn");
            Assert.AreEqual(Resources.FunctionListView_InitializeTableView_Default_value, column.Caption);

            listView.UseInitialValueColumn = true;
            Assert.AreEqual(Resources.FunctionListView_GetDefaultValueColumnName_Initial_value, column.Caption);
        }

        private static EventedList<IFunction> CreateFunctionList()
        {
            return new EventedList<IFunction>
            {
                new Function("function 1")
                {
                    Arguments = new EventedList<IVariable>
                    {
                        new Variable<DateTime>
                        {
                            Name = "Argument",
                            Unit = new Unit("Unit", "u")
                        }
                    },
                    Components = new EventedList<IVariable>
                    {
                        new Variable<double>
                        {
                            Name = "Component",
                            Unit = new Unit("Unit", "u")
                        }
                    }
                },
                new Function("function 2")
                {
                    Arguments = new EventedList<IVariable>
                    {
                        new Variable<DateTime>
                        {
                            Name = "Argument",
                            Unit = new Unit("Unit", "u")
                        }
                    }
                },
                new Function("function 3")
                {
                    Components = new EventedList<IVariable>
                    {
                        new Variable<double>
                        {
                            Name = "Component",
                            Unit = new Unit("Unit", "u")
                        }
                    }
                },
                new Function("function 4")
            };
        }
    }
}
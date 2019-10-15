using DelftTools.Functions;
using DelftTools.Functions.Binding;
using DelftTools.Functions.Generic;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class FunctionArgumentSyncerTest
    {
        [Test]
        public void SyncWithFunctionBindingList()
        {
            var f1 = new Function("f1");
            f1.Arguments.Add(new Variable<int>("arg"));
            f1.Components.Add(new Variable<int>("comp"));
            f1.Arguments[0].IsAutoSorted = true;

            var f2 = new Function("f2");
            f2.Arguments.Add(new Variable<int>("arg"));
            f2.Components.Add(new Variable<int>("comp"));
            f2.Arguments[0].IsAutoSorted = true;

            var list = new FunctionBindingList(f1);

            var syncer = new FunctionArgumentSyncer<int>();
            syncer.Functions.Add(f1);
            syncer.Functions.Add(f2);

            // update through list
            list.AddNew();
            var row = list[0];
            row[0] = 10;
            row[1] = 4;
            row.EndEdit();

            list.AddNew();
            row = list[1];
            row[0] = 11;
            row[1] = 45;
            row.EndEdit();

            list.AddNew();
            row = list[2];
            row[0] = 3;
            row[1] = 9;
            row.EndEdit();

            Assert.AreEqual(f1.Arguments[0].Values, f2.Arguments[0].Values);
            
            // vice versa
            f2[20] = 99;
            f2[30] = 130;

            Assert.AreEqual(f1.Arguments[0].Values, f2.Arguments[0].Values);
        }

    }
}
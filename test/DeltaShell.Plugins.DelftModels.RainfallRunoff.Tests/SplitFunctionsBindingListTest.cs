using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Editing;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class SplitFunctionsBindingListTest
    {
        [Test]
        public void GivenSplitFunctionsBindingList_HavingNoFunctionsSet_ShouldWork()
        {
            //Arrange
            var bindingList = new SplitFunctionsBindingList(Enumerable.Empty<IFunction>());

            // Act & Assert
            bindingList.BeginEdit(new DefaultEditAction("Test"));
            Assert.False(bindingList.IsEditing, "Not in edit state, because there are no functions to put in edit state");
            Assert.IsNull(bindingList.CurrentEditAction, "No action should be active");
            
            bindingList.CancelEdit();
            Assert.False(bindingList.EditWasCancelled, "Canceling edit state does nothing if there are no functions present");

            bindingList.EndEdit();
        }
    }
}
using System.ComponentModel;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Tests
{
    [TestFixture]
    [NUnit.Framework.Category(TestCategory.Integration)]
    public class RtcShapesIntegrationTests
    {
        [Test]
        public void AddingNameableTagChangesTextOfShape()
        {
            var testObject = new NameableObject {Name = "test1"};
            var shape = new InputItemShape {Tag = testObject};
            Assert.AreEqual(testObject.Name, shape.Title);
        }

        [Test]
        public void ChangingPropertyChangeableObjectInTagChangesTextOfShape()
        {
            var testObject = new NameableObject {Name = "test1"};
            var shape = new InputItemShape {Tag = testObject};
            Assert.AreEqual(testObject.Name, shape.Title);
            testObject.Name = "test2";
            Assert.AreEqual(testObject.Name, shape.Title);
        }
    }

    public class NameableObject : INameable, INotifyPropertyChanged
    {
        private string name;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        private void OnPropertyChanged(string nm)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(nm));
            }
        }
    }
}
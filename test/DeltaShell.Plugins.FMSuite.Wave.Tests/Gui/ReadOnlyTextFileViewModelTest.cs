using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class ReadOnlyTextFileViewModelTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            const string name = "someName.txt";
            const string content = "Some content that goes into the text.";

            var data = new ReadOnlyTextFileData(name, content);

            // Call
            var viewModel = new ReadOnlyTextFileViewModel(data);

            // Assert
            Assert.That(viewModel, Is.InstanceOf<TextDocumentBase>());
            Assert.That(viewModel.Data, Is.SameAs(data));
            Assert.That(viewModel.Name, Is.EqualTo(name));
            Assert.That(viewModel.Content, Is.EqualTo(content));
        }

        [Test]
        public void Constructor_DataNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => new ReadOnlyTextFileViewModel(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("data"));
        }

        [Test]
        public void Name_Set_ThrowsNotSupportedException()
        {
            // Setup
            const string name = "someName.txt";
            const string content = "Some content that goes into the text.";

            var data = new ReadOnlyTextFileData(name, content);
            var viewModel = new ReadOnlyTextFileViewModel(data);

            // Call
            void Call() => viewModel.Name = "somethingElse";

            // Assert
            Assert.Throws<System.NotSupportedException>(Call);
        }
        
        [Test]
        public void Content_Set_ThrowsNotSupportedException()
        {
            // Setup
            const string name = "someName.txt";
            const string content = "Some content that goes into the text.";

            var data = new ReadOnlyTextFileData(name, content);
            var viewModel = new ReadOnlyTextFileViewModel(data);

            // Call
            void Call() => viewModel.Content = "somethingElse";

            // Assert
            Assert.Throws<System.NotSupportedException>(Call);
        }
    }
}
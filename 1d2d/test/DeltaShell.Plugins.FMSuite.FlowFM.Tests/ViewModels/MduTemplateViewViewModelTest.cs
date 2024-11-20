using DeltaShell.Plugins.FMSuite.FlowFM.Gui.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ViewModels
{
    [TestFixture]
    public class MduTemplateViewViewModelTest
    {
        [Test]
        public void MduTemplateViewViewModelConstructorTest()
        {
            var viewModel = new MduTemplateViewViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.BrowseCommand);
            Assert.IsNotNull(viewModel.CancelCommand);
            Assert.IsNotNull(viewModel.ImportCommand);
            Assert.IsNull(viewModel.Cancel);
            Assert.IsNull(viewModel.GetFilePath);
            Assert.IsNull(viewModel.FilePath);
        }

        [Test]
        public void ImportCommandTest()
        {
            const string helloWorld = "Hello world";
            var viewModel = new MduTemplateViewViewModel() { FilePath = helloWorld };
            viewModel.ExecuteProjectTemplate = o => Assert.That(o, Is.EqualTo(helloWorld));
            viewModel.ImportCommand?.Execute("Anything else");

        }
        
        [Test]
        public void CancelCommandTest()
        {
            var viewModel = new MduTemplateViewViewModel();
            var i = 0;
            viewModel.Cancel = () => i++;
            viewModel.CancelCommand?.Execute(null);
            Assert.That(i, Is.EqualTo(1));
        }
        
        [Test]
        public void PropertyChangedEventedTest()
        {
            const string helloWorld = "Hello world"; 
            var viewModel = new MduTemplateViewViewModel();
            var i = 0;
            viewModel.PropertyChanged += (sender, args) => i++ ;
            viewModel.FilePath = helloWorld;
            Assert.That(i, Is.EqualTo(1));
        }

        [Test]
        public void BrowseCommandTest()
        {
            const string helloWorld = "Hello world";
            var viewModel = new MduTemplateViewViewModel();
            viewModel.GetFilePath = () => helloWorld;
            viewModel.ExecuteProjectTemplate = o => Assert.That(o, Is.EqualTo(helloWorld)); 
            viewModel.BrowseCommand?.Execute(null);
            Assert.IsTrue(viewModel.ImportCommand?.CanExecute(null));
        }

    }
}

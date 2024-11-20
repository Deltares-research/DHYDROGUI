using System.Collections.Specialized;
using System.ComponentModel;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Views;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.Views
{
    [TestFixture]
    [NUnit.Framework.Category(TestCategory.Wpf)]
    public class MainDomainSpecificDataViewTest
    {
        [Test]
        public void Dispose_DomainsViewModelAreUnsubscribed()
        {
            INotifyPropertyChange domain = Substitute.For<INotifyPropertyChange, IWaveDomainData>();
            var rootDomain = (IWaveDomainData) domain;
            var subDomains = Substitute.For<IEventedList<IWaveDomainData>>();
            rootDomain.SubDomains.Returns(subDomains);

            var viewModel = new MainDomainSpecificDataViewModel(rootDomain);
            var view = new MainDomainSpecificDataView(viewModel);

            // Call
            view.Dispose();

            // Assert
            domain.ReceivedWithAnyArgs().PropertyChanged -= Arg.Any<PropertyChangedEventHandler>();
            subDomains.ReceivedWithAnyArgs().CollectionChanged -= Arg.Any<NotifyCollectionChangedEventHandler>();
        }
    }
}
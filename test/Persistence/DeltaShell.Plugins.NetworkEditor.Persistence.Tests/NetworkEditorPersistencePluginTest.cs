using System;
using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Persistence.Tests
{
    [TestFixture]
    public class NetworkEditorPersistencePluginTest
    {
        [Test]
        public void Constructor_InitializesCorrectValues()
        {
            var plugin = new NetworkEditorPersistencePlugin();

            Assert.That(plugin, Is.AssignableTo<PersistencePlugin>());
            Assert.That(plugin, Is.AssignableTo<INHibernatePluginExtensions>());

            Assert.That(plugin.Name, Is.EqualTo("Network editor persistence plugin"));
            Assert.That(plugin.DisplayName, Is.EqualTo("Network editor persistence plugin"));
            Assert.That(plugin.Description, Is.EqualTo("Plugin for persisting the network editor"));
            Assert.That(plugin.Version, Is.EqualTo("0.1.0.0"));
            Assert.That(plugin.FileFormatVersion, Is.EqualTo("3.5.2.0"));
            Assert.That(plugin.PluginNameBeforeNHibernateMigration, Is.EqualTo("Network"));
        }

        [Test]
        public void PluginIsExtensionPoint()
        {
            Type pluginType = typeof(NetworkEditorPersistencePlugin);
            Assert.That(pluginType, Has.Attribute<ExtensionAttribute>()
                                       .With.Property(nameof(ExtensionAttribute.Type))
                                       .EqualTo(typeof(IPlugin)));
        }
    }
}
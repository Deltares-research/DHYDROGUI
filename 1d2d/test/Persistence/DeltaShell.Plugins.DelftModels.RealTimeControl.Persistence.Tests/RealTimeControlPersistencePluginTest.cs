using System;
using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Persistence.Tests
{
    [TestFixture]
    public class RealTimeControlPersistencePluginTest
    {
        [Test]
        public void Constructor_InitializesCorrectValues()
        {
            var plugin = new RealTimeControlPersistencePlugin();

            Assert.That(plugin, Is.AssignableTo<PersistencePlugin>());
            Assert.That(plugin, Is.AssignableTo<INHibernatePluginExtensions>());

            Assert.That(plugin.Name, Is.EqualTo("D-Real Time Control domain persistence plugin"));
            Assert.That(plugin.DisplayName, Is.EqualTo("D-Real Time Control domain persistence plugin"));
            Assert.That(plugin.Description, Is.EqualTo("Plugin for persisting the D-Real Time Control domain"));
            Assert.That(plugin.Version, Is.EqualTo("0.1.0.0"));
            Assert.That(plugin.FileFormatVersion, Is.EqualTo("3.8.0.0"));
            Assert.That(plugin.PluginNameBeforeNHibernateMigration, Is.EqualTo("Real-Time Control"));
        }

        [Test]
        public void PluginIsExtensionPoint()
        {
            Type pluginType = typeof(RealTimeControlPersistencePlugin);
            Assert.That(pluginType, Has.Attribute<ExtensionAttribute>()
                                       .With.Property(nameof(ExtensionAttribute.Type))
                                       .EqualTo(typeof(IPlugin)));
        }
    }
}
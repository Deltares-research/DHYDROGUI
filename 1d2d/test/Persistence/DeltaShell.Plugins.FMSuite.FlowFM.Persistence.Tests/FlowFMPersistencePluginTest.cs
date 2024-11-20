using System;
using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Persistence.Tests
{
    [TestFixture]
    public class FlowFMPersistencePluginTest
    {
        [Test]
        public void Constructor_InitializesCorrectValues()
        {
            var plugin = new FlowFMPersistencePlugin();

            Assert.That(plugin, Is.AssignableTo<PersistencePlugin>());
            Assert.That(plugin, Is.AssignableTo<INHibernatePluginExtensions>());

            Assert.That(plugin.Name, Is.EqualTo("D-Flow FM domain persistence plugin"));
            Assert.That(plugin.DisplayName, Is.EqualTo("D-Flow FM domain persistence plugin"));
            Assert.That(plugin.Description, Is.EqualTo("Plugin for persisting the D-Flow FM domain"));
            Assert.That(plugin.Version, Is.EqualTo("0.1.0.0"));
            Assert.That(plugin.FileFormatVersion, Is.EqualTo("1.4.0.0"));
            Assert.That(plugin.PluginNameBeforeNHibernateMigration, Is.EqualTo("Delft3D FM"));
        }

        [Test]
        public void PluginIsExtensionPoint()
        {
            Type pluginType = typeof(FlowFMPersistencePlugin);
            Assert.That(pluginType, Has.Attribute<ExtensionAttribute>()
                                       .With.Property(nameof(ExtensionAttribute.Type))
                                       .EqualTo(typeof(IPlugin)));
        }
    }
}
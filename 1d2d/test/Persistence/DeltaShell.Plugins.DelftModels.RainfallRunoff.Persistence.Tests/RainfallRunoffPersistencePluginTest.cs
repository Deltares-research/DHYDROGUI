using System;
using DelftTools.Shell.Core;
using DelftTools.Shell.Persistence;
using DeltaShell.Plugins.Persistence.NHibernate;
using Mono.Addins;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Persistence.Tests
{
    [TestFixture]
    public class RainfallRunoffPersistencePluginTest
    {
        [Test]
        public void Constructor_InitializesCorrectValues()
        {
            var plugin = new RainfallRunoffPersistencePlugin();

            Assert.That(plugin, Is.AssignableTo<PersistencePlugin>());
            Assert.That(plugin, Is.AssignableTo<INHibernatePluginExtensions>());

            Assert.That(plugin.Name, Is.EqualTo("D-Rainfall Runoff domain persistence plugin"));
            Assert.That(plugin.DisplayName, Is.EqualTo("D-Rainfall Runoff domain persistence plugin"));
            Assert.That(plugin.Description, Is.EqualTo("Plugin for persisting the D-Rainfall Runoff domain"));
            Assert.That(plugin.Version, Is.EqualTo("0.1.0.0"));
            Assert.That(plugin.FileFormatVersion, Is.EqualTo("3.7.0.0"));
            Assert.That(plugin.PluginNameBeforeNHibernateMigration, Is.EqualTo("rainfall runoff model"));
        }

        [Test]
        public void PluginIsExtensionPoint()
        {
            Type pluginType = typeof(RainfallRunoffPersistencePlugin);
            Assert.That(pluginType, Has.Attribute<ExtensionAttribute>()
                                       .With.Property(nameof(ExtensionAttribute.Type))
                                       .EqualTo(typeof(IPlugin)));
        }
    }
}
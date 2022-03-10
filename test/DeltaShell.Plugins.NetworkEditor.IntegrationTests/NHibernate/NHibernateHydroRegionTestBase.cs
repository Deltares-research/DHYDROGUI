using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests.NHibernate
{
    public class NHibernateHydroRegionTestBase : NHibernateIntegrationTestBase
    {
        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new NetworkEditorGuiPlugin());
        }

        /// <summary>
        /// Saves and retrieves an object by wrapping it in a dataitem in the rootfolder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="path">Project path used to save / load</param>
        /// <returns></returns>
        protected T SaveLoadObject<T>(T o, string path)
        {
            ProjectRepository.Create(path);

            var project = new Project();
            project.RootFolder.Add(o);

            ProjectRepository.SaveOrUpdate(project);

            ProjectRepository.Close();
            ProjectRepository.Dispose();

            // read it back
            ProjectRepository = factory.CreateNew();
            ProjectRepository.Open(path);

            var project2 = ProjectRepository.GetProject();
            return (T)((DataItem)project2.RootFolder.Items[0]).Value;
        }

        protected static IChannel CreateChannel(INode fromNode, INode toNode)
        {
            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(1000, 1000), 
                                   new Coordinate(1000, 1500)
                               };

            return new Channel(fromNode, toNode)
                       {
                           Geometry = new LineString(vertices.ToArray())
                       };
        }
    }
}
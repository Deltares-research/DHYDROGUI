using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.IntegrationTestUtils.NHibernate;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests.NHibernate
{
    public class NHibernateHydroRegionTestBase : NHibernateIntegrationTestBase
    {
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
            ProjectRepository = CreateProjectRepository();
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
        
        protected override NHibernateProjectRepository CreateProjectRepository()
        {
            return new DHYDRONHibernateProjectRepositoryBuilder().Build();
        }
    }
}
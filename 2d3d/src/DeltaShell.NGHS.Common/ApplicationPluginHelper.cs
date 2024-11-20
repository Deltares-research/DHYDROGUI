using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.NGHS.Common
{
    public static class ApplicationPluginHelper
    {
        /// <summary>
        /// Finds the parent project item.
        /// </summary>
        /// <param name="rootFolder"> The rootfolder containing all models</param>
        /// <param name="owner"> Gui selection </param>
        /// <returns> Parent IProjectItem </returns>
        public static IProjectItem FindParentProjectItemInsideProject(Folder rootFolder, object owner)
        {
            if (rootFolder == null || owner == null)
            {
                return null;
            }

            switch (owner)
            {
                case Folder folder when folder == rootFolder:
                    return folder;
                case ICompositeActivity compositeActivity when !compositeActivity.ReadOnly:
                    return compositeActivity;
            }

            List<ICompositeActivity> compositeActivities = rootFolder.GetAllModelsRecursive().OfType<ICompositeActivity>().ToList();
            var treeFolderParentActivity = owner.GetType().GetProperty("Parent")?.GetMethod.Invoke(owner, new object[]
                                                                                                       {}) as ICompositeActivity;

            return compositeActivities.FirstOrDefault(a =>
            {
                if (owner is IActivity activity)
                {
                    return a.Activities.Contains(activity);
                }

                return a == treeFolderParentActivity;
            });
        }
    }
}
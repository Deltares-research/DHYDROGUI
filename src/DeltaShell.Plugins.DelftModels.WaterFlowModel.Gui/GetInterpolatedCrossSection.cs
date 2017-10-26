using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui
{
    public class GetInterpolatedCrossSection : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GetInterpolatedCrossSection));

        private static IModelApi ModelApi = WaterFlowModelApiFactory.CreateApi();

        private static WeakReference networkReference;
        private static WeakReference modelReference;
        public static GetInterpolatedCrossSection Instance {get; private set;}
        
        static GetInterpolatedCrossSection()
        {
            Instance = new GetInterpolatedCrossSection();
        }


        public static void DisposeInstance()
        {
            if (Instance != null)
            {
                Instance.Dispose();
                Instance = null;
            }
        }

        public static IHydroNetwork HydroNetwork
        {
            get
            {
                if (networkReference != null)
                {
                    var target = (IHydroNetwork) networkReference.Target;
                    if (target != null)
                        return target;
                }
                networkReference = null;
                return null;
            }
            set { networkReference = value != null ? new WeakReference(value) : null; }
        }

        public static WaterFlowModel1D WaterFlowModel1D
        {
            get
            {
                if (modelReference != null)
                {
                    var target = (WaterFlowModel1D)modelReference.Target;
                    if (target != null)
                        return target;
                }
                modelReference = null;
                return null;
            }
            set { modelReference = value != null ? new WeakReference(value) : null; }
        }

        public static IFeature GetInterpolatedCrossSectionAt(IPoint point)
        {
            // TODO: ModelApi functions were removed, this needs to be re-implemented at some point
            Log.Error("Get Interpolated CrossSection feature has been disabled");
            return null;
        }

        /// <summary>
        /// Find nearest crosssection on connected branch having same ordernumber, starting in direction as inidicated by parameter
        /// inSourceNodeDirection. In no such crosssection can be found the return value will hold a value less than zero as its 
        /// first item.
        /// </summary>
        /// <param name="c">The channel from which to start the search</param>
        /// <param name="chainage">The chainage at which to start</param>
        /// <param name="inSourceNodeDirection">When true the search starts toward the source node of the channel, otherwise it starts
        /// toward the target node of the channel</param>
        /// <param name="visited">a list to hold all channels already visited (it is a recursive call). This prevents the algorithm
        /// from getting stuck in a loop.</param>
        /// <returns>If a crosssection was found the returned values specify the distance to the crosssection, and a reference to the
        /// crosssection itself.
        /// If no crosssection was found the first value will be less than 0</returns>
        public static DelftTools.Utils.Tuple<double, ICrossSection> FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber(Channel c, double chainage, bool inSourceNodeDirection, List<Channel> visited = null)
        {
            // is crosssection on current branch
            ICrossSection fcs;
            if (inSourceNodeDirection)
            {
                fcs = c.BranchFeatures.Where(cs => !(cs.Chainage > chainage)).OfType<ICrossSection>().FirstOrDefault();
                if (fcs != null)
                {
                    return new DelftTools.Utils.Tuple<double, ICrossSection>(chainage - fcs.Chainage, fcs);
                }
            }
            else
            {
                fcs = c.BranchFeatures.Where(cs => !(cs.Chainage < chainage)).OfType<ICrossSection>().FirstOrDefault();
                if (fcs != null)
                {
                    return new DelftTools.Utils.Tuple<double, ICrossSection>(fcs.Chainage - chainage, fcs);
                }
            }

            // if not, do recursive call
            var connectedBranches = (inSourceNodeDirection
                                           ? c.Source.IncomingBranches.Concat(c.Source.OutgoingBranches)
                                           : c.Target.IncomingBranches.Concat(c.Target.OutgoingBranches)).ToList();
            foreach (var i in connectedBranches)
            {
                if (visited == null) // apparently we have just started the recursive search
                {
                    visited = new List<Channel>();
                }
                if (!ReferenceEquals(i, c) && i.OrderNumber == c.OrderNumber && !visited.Contains(i as Channel))
                {
                    visited.Add(c);
                    var rec = i.Target == c.Source ? FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber((Channel)i, i.Length, true,  visited)
                                                   : FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber((Channel)i,       0d, false, visited);
                    return rec.First < 0 ? rec : new DelftTools.Utils.Tuple<double, ICrossSection>(
                        (inSourceNodeDirection ? chainage : (c.Length - chainage)) + rec.First, rec.Second);
                }
            }
            return new DelftTools.Utils.Tuple<double, ICrossSection>(-1d, CrossSection.CreateDefault());
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            WaterFlowModelApiFactory.Cleanup(true, ModelApi);
            ModelApi = null;
        }

        #endregion
    }
}

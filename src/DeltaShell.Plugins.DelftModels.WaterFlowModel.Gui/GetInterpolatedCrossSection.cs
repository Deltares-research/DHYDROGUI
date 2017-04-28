using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Networks;

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
            //Do validation
            if (HydroNetwork == null)
            {
                Log.Error("There is no network available, can not interpolate");
                return null;
            }

            var channel = NetworkHelper.GetNearestBranch(HydroNetwork.Branches, point, 10.0) as Channel;

            if (channel == null)
            {
                Log.Error("There is no branch available, can not interpolate");
                return null;
            }

            var issues = WaterFlowModel1DHydroNetworkValidator.GetCrossSectionValidationIssues(channel, HydroNetwork,
                                                                                               new HashSet<string>());

            if (issues.Any())
            {                
                foreach (var issue in issues.ToList())
                {
                    Log.Error(issue.Message);
                }
                return null;
            }

            var chainage = NetworkHelper.GetBranchFeatureChainageFromGeometry(channel, point);

            var visitedBranches = new List<Channel>();
            var cs1 = FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber(channel, chainage, true, visitedBranches);
            var cs2 = FindNearestCrossSectionOnConnectedBranchWithSameOrderNumber(channel, chainage, false, visitedBranches);

            if (!ValidateNearestCrossSections(cs1, cs2))
            {
                return null;
            }

            ICrossSection cs = null;

            // these two cases handle the extrapolation-case: if just one crosssection is found use that
            if (cs1.First < 0)
            {
                cs = (ICrossSection)cs2.Second.Clone();
            }
            if (cs2.First < 0)
            {
                cs = (ICrossSection)cs1.Second.Clone();
            }
            
            if (cs1.Second.Definition.IsProxy)
            {
                cs1.Second.MakeDefinitionLocal();
            }

            if (cs2.Second.Definition.IsProxy)
            {
                cs2.Second.MakeDefinitionLocal();
            }

            var distanceBetweenCrossSections = cs1.First + cs2.First;
            var distanceToCrossSectionNr1 = cs1.First;

            Form form;
            if (cs != null)
            {
                cs.Branch = channel; //Because clone does not copy branch references.
            }
            else
            {
                if (cs1.Second.CrossSectionType == CrossSectionType.ZW ||
                    cs1.Second.CrossSectionType == CrossSectionType.Standard)
                {
                    cs = getInterpolatedZWCrossSection(channel, chainage, cs1.Second, cs2.Second, distanceBetweenCrossSections, distanceToCrossSectionNr1);
                }
                else
                {
                    if (WaterFlowModel1D == null)
                    {
                        Log.Error("There's no model available for a conveyance table calculation");
                        return null;
                    }
                    cs = getInterpolatedYZCrossSection(channel, chainage, cs1.Second, cs2.Second, distanceBetweenCrossSections, distanceToCrossSectionNr1, WaterFlowModel1D);
                }
            }

            if (cs.CrossSectionType == CrossSectionType.ZW || cs.CrossSectionType == CrossSectionType.Standard)
            {

                form = GetCrossSectionForm(cs, HydroNetwork);
            }
            else
            {
                if (WaterFlowModel1D == null)
                {
                    Log.Error("There's no model available for a conveyance table calculation");
                    return null;
                }
                form = GetConveyanceForm(cs, WaterFlowModel1D);
            }

            if (form != null && form.ShowDialog() == DialogResult.OK)
            {
                return cs;
            }

            return null;
        }

        private static DialogForm DialogForm
        {
            get
            {
                var form = new DialogForm()
                {
                    Size = new Size(1200, 600),
                    MinimizeBox = false,
                    StartPosition = FormStartPosition.CenterScreen,
                    ShowInTaskbar = false,
                    TopMost = true,
                    Text = "Interpolated cross-section view"
                };
                return form;
            }
        }

        public static DialogForm GetCrossSectionForm(ICrossSection cs, IHydroNetwork hydroNetwork)
        {
            var form = DialogForm;

            var pnlView = form.Controls.Find("pnlView", true).First();

            pnlView.Controls.Add(new CrossSectionDefinitionView
                {
                    Data = cs.Definition,
                    ViewModel = new CrossSectionDefinitionViewModel(true, "", 10, 0, "", "", true)
                        {
                            CrossSectionSectionTypes = hydroNetwork.CrossSectionSectionTypes
                        }
                });

            return form;
        }

        public static DialogForm GetConveyanceForm(ICrossSection cs, WaterFlowModel1D waterFlowModel1D)
        {
            var form = DialogForm;

            var pnlView = form.Controls.Find("pnlView", true).First();
            var btnAdd = form.Controls.Find("btnAdd", true).First();

            var waterFlowModel1DConveyanceCalculator = new WaterFlowModel1DConveyanceCalculator(waterFlowModel1D);
            var functionView = new FunctionView
                {
                    Data = waterFlowModel1DConveyanceCalculator.GetConveyance(cs),
                    Dock = DockStyle.Fill
                };
            functionView.ChartView.Chart.Legend.ShowCheckBoxes = true;
            form.Shown += (o, args) => functionView.TableView.BestFitColumns(false);
            pnlView.Controls.Add(functionView);
            btnAdd.Enabled = false;

            return form;
        }

        public static ICrossSection getInterpolatedZWCrossSection(Channel c, double chainage, ICrossSection cs1, ICrossSection cs2, double distanceBetweenCrossSections, double distanceToCrossSectionNr1)
        {
            var crossSectionModelApiController = new CrossSectionModelApiController(ModelApi, null); // roughness not needed for following calls

            int csDefinition1 = crossSectionModelApiController.SetProfileInModelApi(cs1, cs1.Definition, false);
            int crossSectionNr1 = ModelApi.NetworkSetCS(0, 0.0, csDefinition1, 0.0);                

            int csDefinition2 = crossSectionModelApiController.SetProfileInModelApi(cs2, cs2.Definition, false);
            int crossSectionNr2 = ModelApi.NetworkSetCS(0, 0.0, csDefinition2, 0.0);

            // create arrays to hold the resulting interpolated cross section
            double[] levels_r = new double[0];
            double[] flowWidth_r = new double[0];
            double[] totalWidth_r = new double[0];
            double[] plains_r = new double[0];
            double levelCrest_r = 0.0;
            double bedLevel_r = 0.0;
            double flowArea_r = 0.0;
            double totalArea_r = 0.0;
            double groundlayer_r = 0.0;
            bool groundlayerUsed_r = false;

            int levelCount;
            double bedLevelShift;

            ModelApi.GetInterpolatedZWCrossSection(crossSectionNr1, crossSectionNr2, distanceBetweenCrossSections, distanceToCrossSectionNr1,
                                                    out levelCount, out bedLevelShift, ref levels_r,
                                                    ref flowWidth_r, ref totalWidth_r, ref plains_r, ref levelCrest_r,
                                                    ref bedLevel_r, ref flowArea_r, ref totalArea_r,
                                                    ref groundlayerUsed_r, ref groundlayer_r);

            var newcs = CrossSection.CreateDefault(CrossSectionType.ZW, c, chainage);

            // fill in newcs values
            var newcsdef = (CrossSectionDefinitionZW)newcs.Definition;
            newcsdef.ZWDataTable.Clear();
            for (int i = 0; i < levelCount; i++)
            {
                newcsdef.ZWDataTable.AddCrossSectionZWRow(levels_r[i] + bedLevelShift, totalWidth_r[i],
                                                            totalWidth_r[i] - flowWidth_r[i]);
            }
            newcsdef.SummerDike.Active = levelCrest_r > 0d;
            newcsdef.SummerDike.CrestLevel = levelCrest_r;
            newcsdef.SummerDike.FloodPlainLevel = bedLevel_r;
            newcsdef.SummerDike.FloodSurface = flowArea_r;

            ModelApi.Finalize();

            return newcs;
        }

        public static ICrossSection getInterpolatedYZCrossSection(Channel c, double chainage, ICrossSection cs1, ICrossSection cs2, double distanceBetweenCrossSections, double distanceToCrossSectionNr1, WaterFlowModel1D waterFlowModel1D)
        {
            var crossSectionModelApiController = new CrossSectionModelApiController(ModelApi, waterFlowModel1D.RoughnessSections);

            var firstCrossSection = cs1;
            int csDefinition1 = crossSectionModelApiController.SetProfileInModelApi(firstCrossSection,
                                                                                    cs1.Definition,
                                                                                    false);
            int crossSectionNr1 = ModelApi.NetworkSetCS(0, 0.0, csDefinition1, 0.0);

            var secondCrossSection = cs2;
            int csDefinition2 = crossSectionModelApiController.SetProfileInModelApi(secondCrossSection,
                                                                                    cs2.Definition,
                                                                                    false);
            int crossSectionNr2 = ModelApi.NetworkSetCS(0, 0.0, csDefinition2, 0.0);

            // create arrays to hold the resulting interpolated cross section
            double[] y = new double[0];
            double[] z = new double[0];

            ModelApi.GetInterpolatedYZCrossSection(crossSectionNr1, crossSectionNr2, distanceBetweenCrossSections,
                                                   distanceToCrossSectionNr1, ref y, ref z);

            var newcs = CrossSection.CreateDefault(CrossSectionType.YZ, c, chainage);

            // fill in newcs values

            var newcsdef = (CrossSectionDefinitionYZ)newcs.Definition;
            newcsdef.YZDataTable.Clear();
            for (var i = 0; i < y.Count(); i++)
            {
                newcsdef.YZDataTable.AddCrossSectionYZRow(y[i], z[i], 0d);
            }

            ModelApi.Finalize();

            return newcs;
        }

        private static bool ValidateNearestCrossSections(DelftTools.Utils.Tuple<double, ICrossSection> cs1, DelftTools.Utils.Tuple<double, ICrossSection> cs2)
        {
            if (cs1.First < 0 && cs2.First < 0)
            {
                {
                    return false;
                }
            }

            // these two cases handle the extrapolation-case: if just one crosssection is found use that
            if (cs1.First < 0)
            {
                {
                    return true;
                }
            }
            if (cs2.First < 0)
            {
                {
                    return true;
                }
            }

            var cst1 = cs1.Second.CrossSectionType;
            var cst2 = cs2.Second.CrossSectionType;

            if (cst1 != cst2)
            {
                if ((cst1 == CrossSectionType.ZW && cst2 != CrossSectionType.Standard) ||
                    (cst2 == CrossSectionType.ZW && cst1 != CrossSectionType.Standard))
                {
                    Log.ErrorFormat("Crosssection types ({0}, {1}) do not match, cannot interpolate", cst1, cst2);
                    {
                        return false;
                    }
                }

                if ((cst1 == CrossSectionType.YZ && cst2 != CrossSectionType.GeometryBased) ||
                    (cst2 == CrossSectionType.YZ && cst1 != CrossSectionType.GeometryBased))
                {
                    Log.ErrorFormat("Crosssection types ({0}, {1}) do not match, cannot interpolate", cst1, cst2);
                    {
                        return false;
                    }
                }
            }
            return true;
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

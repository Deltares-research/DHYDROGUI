using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Data;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Actions;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DelftTools.Hydro.Roughness
{
    [Entity]
    public class RoughnessSection : Unique<long>, ICloneable, ICopyFrom, INameable, IItemContainer, IEditableObject
    {
        private const double epsilon = 1.0e-7;

        private static readonly ILog Log = LogManager.GetLogger(typeof(RoughnessSection));
        private readonly Stack<IEditAction> editActions = new Stack<IEditAction>();
        //cannot use a dictionary because hashcode is cached. Once a location is set, when the chainage changes the location is no longer found
        private readonly IList<Utils.Tuple<INetworkLocation, Utils.Tuple<IBranch, double>>> previousChainages = new List<Utils.Tuple<INetworkLocation, Utils.Tuple<IBranch, double>>>();
        private const int ChainageArgumentIndex = 0;
        private const int QorHArgumentIndex = 1;

        private INetwork network;
        private bool functionOfQPerBranchInitialized;
        private bool functionOfHPerBranchInitialized;
        private IDictionary<IBranch, IFunction> functionOfQPerBranch;
        private IDictionary<IBranch, IFunction> functionOfHPerBranch;
        private IList<IFunction> roughnessFunctionOfQ;
        private IList<IFunction> roughnessFunctionOfH;
        private CrossSectionSectionType crossSectionSectionType;
        protected bool isInKnownEditAction;
        private RoughnessNetworkCoverage roughnessNetworkCoverage;

        protected RoughnessSection()
        {
            RoughnessNetworkCoverage = new RoughnessNetworkCoverage("dummy", false, "");
        }

        public RoughnessSection(CrossSectionSectionType crossSectionSectionType, INetwork network)
            : this()
        {
            CrossSectionSectionType = crossSectionSectionType;
            
            RoughnessNetworkCoverage = new RoughnessNetworkCoverage(Name, false);
            // change to constant
            RoughnessNetworkCoverage.Locations.InterpolationType = InterpolationType.Linear;
            RoughnessNetworkCoverage.Locations.ExtrapolationType = ExtrapolationType.Constant;

            Network = network;
        }

        [Aggregation]
        public virtual CrossSectionSectionType CrossSectionSectionType
        {
            get { return crossSectionSectionType; }
            set
            {
                if (crossSectionSectionType is INotifyPropertyChanged notifyPropertyChange)
                {
                    notifyPropertyChange.PropertyChanged -= CrossSectionSectionTypePropertyChanged;
                }

                crossSectionSectionType = value;

                if (crossSectionSectionType is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged += CrossSectionSectionTypePropertyChanged;
                }
            }
        }

        [Aggregation]
        public INetwork Network
        {
            get { return network; }
            set
            {
                if (network == value)
                {
                    return;
                }
                if (network != null)
                {
                    ResetNetworkInRoughnessCoverage();
                    ((INotifyCollectionChanged)network).CollectionChanged -= NetworkCollectionChanged;

                    // Try to convert existing functions to the new network if possible
                    // needed for Hydro model clone, otherwise functions get lost on relinking dataitems (SOBEK3-705)
                    functionOfHPerBranch = ConvertExistingFunctionsToNewNetwork(functionOfHPerBranch, value);
                    functionOfQPerBranch = ConvertExistingFunctionsToNewNetwork(functionOfQPerBranch, value);
                }
                network = value;
                if (value != null)
                {
                    SetNetworkInRoughnessCoverage();
                    ((INotifyCollectionChanged)network).CollectionChanged += NetworkCollectionChanged;
                }
            }
        }

        public string Name
        {
            get
            {
                if (Reversed)
                {
                    return CrossSectionSectionType.Name + " (Reversed)";
                }
                return CrossSectionSectionType.Name;
            }
            set { }
        }
        
        /// <summary>
        /// Indicates if this RoughnessSection is used to describe reverse-flow roughness
        /// </summary>
        public virtual bool Reversed
        {
            get { return false; }
        }

        public virtual RoughnessNetworkCoverage RoughnessNetworkCoverage
        {
            get { return roughnessNetworkCoverage; }
            private set
            {
                if (roughnessNetworkCoverage != null)
                {
                    roughnessNetworkCoverage.ValuesChanged -= RoughnessNetworkCoverageValueChanged;
                    ((INotifyPropertyChange)roughnessNetworkCoverage).PropertyChanging -= RoughnessNetworkCoveragePropertyChanging;
                    ((INotifyPropertyChange)roughnessNetworkCoverage).PropertyChanged -= RoughnessNetworkCoveragePropertyChanged;
                }
                roughnessNetworkCoverage = value;
                if (roughnessNetworkCoverage != null)
                {
                    roughnessNetworkCoverage.ValuesChanged += RoughnessNetworkCoverageValueChanged;
                    ((INotifyPropertyChange)roughnessNetworkCoverage).PropertyChanging += RoughnessNetworkCoveragePropertyChanging;
                    ((INotifyPropertyChange)roughnessNetworkCoverage).PropertyChanged += RoughnessNetworkCoveragePropertyChanged;
                }
            }
        }

        // flags are required since RoughnessFunctionOfQ and RoughnessFunctionOfH mapped to properties which require that network is already loaded, this is not the case with lazy
        private IDictionary<IBranch, IFunction> FunctionOfQPerBranch
        {
            get
            {
                if (!functionOfQPerBranchInitialized)
                {
                    if (roughnessFunctionOfQ == null)
                    {
                        roughnessFunctionOfQ = new List<IFunction>();
                    }
                    functionOfQPerBranch = ListToDictionary(roughnessFunctionOfQ, network);
                    functionOfQPerBranchInitialized = true;
                }

                return functionOfQPerBranch;
            }
        }

        private IDictionary<IBranch, IFunction> FunctionOfHPerBranch
        {
            get
            {
                if(!functionOfHPerBranchInitialized)
                {
                    if (roughnessFunctionOfH == null)
                    {
                        roughnessFunctionOfH = new List<IFunction>();
                    }
                    functionOfHPerBranch = ListToDictionary(roughnessFunctionOfH, network);
                    functionOfHPerBranchInitialized = true;
                }

                return functionOfHPerBranch;
            }
        }

        public IFunction AddQRoughnessFunctionToBranch(IBranch branch)
        {
            return AddQRoughnessFunctionToBranch(branch, DefineFunctionOfQ());
        }

        public IFunction AddQRoughnessFunctionToBranch(IBranch branch, IFunction waterFlowRoughnessFunction)
        {
            FunctionOfQPerBranch[branch] = waterFlowRoughnessFunction;
            return waterFlowRoughnessFunction;
        }

        public IFunction AddHRoughnessFunctionToBranch(IBranch branch)
        {
            return AddHRoughnessFunctionToBranch(branch, DefineFunctionOfH());
        }

        public IFunction AddHRoughnessFunctionToBranch(IBranch branch, IFunction waterLevelRoughnessFunction)
        {
            FunctionOfHPerBranch[branch] = waterLevelRoughnessFunction;
            return waterLevelRoughnessFunction;
        }

        public void Clear()
        {
            RoughnessNetworkCoverage.Clear();
            FunctionOfQPerBranch.Clear();
            FunctionOfHPerBranch.Clear();
        }

        public static IFunction DefineFunctionOfQ()
        {
            var chainage = new Variable<double>("Chainage") { ExtrapolationType = ExtrapolationType.Constant };
            var q = new Variable<double>("Q");
            var roughness = new Variable<double>("Roughness");

            return new Function { Name = "FunctionOfQ", Arguments = { chainage, q }, Components = { roughness } };
        }

        public static IFunction DefineFunctionOfH()
        {
            var chainage = new Variable<double>("Chainage") { ExtrapolationType = ExtrapolationType.Constant };
            var h = new Variable<double>("H");
            var roughness = new Variable<double>("Roughness");

            return new Function { Name = "FunctionOfH", Arguments = { chainage, h }, Components = { roughness } };
        }

        public virtual RoughnessType EvaluateRoughnessType(INetworkLocation location)
        {
            return RoughnessNetworkCoverage.EvaluateRoughnessType(location);
        }

        public virtual double EvaluateRoughnessValue(INetworkLocation location)
        {
            return RoughnessNetworkCoverage.EvaluateRoughnessValue(location);
        }

        public virtual RoughnessFunction GetRoughnessFunctionType(IBranch branch)
        {
            if (FunctionOfHPerBranch.ContainsKey(branch))
            {
                return RoughnessFunction.FunctionOfH;
            }
            if (FunctionOfQPerBranch.ContainsKey(branch))
            {
                return RoughnessFunction.FunctionOfQ;
            }
            return RoughnessFunction.Constant;
        }

        public virtual IFunction FunctionOfQ(IBranch branch)
        {
            return FunctionOfQPerBranch[branch];
        }

        public virtual IFunction FunctionOfH(IBranch branch)
        {
            return FunctionOfHPerBranch[branch];
        }

        public void RemoveRoughnessFunctionsForBranch(IBranch branch)
        {
            if (FunctionOfQPerBranch.ContainsKey(branch))
            {
                FunctionOfQPerBranch.Remove(branch);
            }
            if (FunctionOfHPerBranch.ContainsKey(branch))
            {
                FunctionOfHPerBranch.Remove(branch);
            }
        }

        public void ChangeBranchFunction(IBranch branch, RoughnessFunction roughnessFunction)
        {
            BeginEdit(string.Format("Roughness dependency ({0}): {1} -> {2}", branch, GetRoughnessFunctionType(branch), roughnessFunction));

            var chainages = RoughnessNetworkCoverage.Locations.Values.Where(l => l.Branch == branch).Select(l => l.Chainage);

            RemoveRoughnessFunctionsForBranch(branch);

            switch (roughnessFunction)
            {
                case RoughnessFunction.Constant: break;
                case RoughnessFunction.FunctionOfQ:
                case RoughnessFunction.FunctionOfH:
                    var function = roughnessFunction == RoughnessFunction.FunctionOfH
                                       ? AddHRoughnessFunctionToBranch(branch)
                                       : AddQRoughnessFunctionToBranch(branch);
                    function.Arguments[0].SetValues(chainages);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("roughnessFunction");
            }

            EndEdit();
        }

        public IEnumerable<object> GetDirectChildren()
        {
            yield return RoughnessNetworkCoverage;
        }

        public virtual RoughnessType GetDefaultRoughnessType()
        {
            return roughnessNetworkCoverage.DefaultRoughnessType;
        }

        public virtual void SetDefaultRoughnessType(RoughnessType type)
        {
            roughnessNetworkCoverage.DefaultRoughnessType = type;
        }

        public virtual double GetDefaultRoughnessValue()
        {
            return roughnessNetworkCoverage.DefaultValue;
        }

        public virtual void SetDefaultRoughnessValue(double value)
        {
            roughnessNetworkCoverage.DefaultValue = value;
        }

        public virtual void SetDefaults(RoughnessType defaultType, double defaultValue)
        {
            RoughnessNetworkCoverage.DefaultValue = defaultValue;
            RoughnessNetworkCoverage.DefaultRoughnessType = defaultType;
        }

        public void UpdateCoverageForFunction(IBranch branch, IFunction roughnessFunction, RoughnessType roughnessType)
        {
            foreach (double chainage in roughnessFunction.Arguments[0].Values)
            {
                if (chainage > branch.Length)
                {
                    Log.ErrorFormat("Invalid chainage '{0}' for branch '{1}'; skipped.", chainage, branch.Name);
                    continue;
                }

                var location = new NetworkLocation(branch, chainage);
                var res = roughnessFunction[chainage];

                var value = (res is IMultiDimensionalArray)
                                ? ((MultiDimensionalArray) (roughnessFunction[chainage])).MinValue
                                : res;

                RoughnessNetworkCoverage[location] = new[] { value, (int) roughnessType };
            }
        }

        private void ResetNetworkInRoughnessCoverage()
        {
            RoughnessNetworkCoverage.Network = null;
        }

        private void SetNetworkInRoughnessCoverage()
        {
            RoughnessNetworkCoverage.Network = Network;
        }

        private void HandleBranchSplit(BranchSplitAction branchSplitAction)
        {
            //update function of Q or H after branch. The roughnesscoverage should be already be update.
            //find the function for the splitted branch
            var function = GetRoughnessFunction(branchSplitAction.SplittedBranch);
            if (function == null)
                return;//done

            //create a similar function on the new branch..
            var newFunction = AddSimilarFunctionToNewBranch(branchSplitAction.SplittedBranch, branchSplitAction.NewBranch);

            //evaluate the previous values at the split
            IList<double> values = new List<double>();
            var splitChainage = branchSplitAction.SplittedBranch.Length;
            foreach (double qOrH in function.Arguments[QorHArgumentIndex].Values)
            {
                double rougnessValue = EvaluateRoughnessFunction(function, splitChainage, qOrH);
                values.Add(rougnessValue);
            }
            //add these value to both functions 
            for (int i = 0; i < function.Arguments[QorHArgumentIndex].Values.Count; i++)
            {
                var qOrH = (double)function.Arguments[QorHArgumentIndex].Values[i];
                function[splitChainage, qOrH] = values[i];
                newFunction[0.0d, qOrH] = values[i];
            }

            //last step..migrate values from 'old' function to new if the chainage exceeds the split chainage
            var chainageList = ((IMultiDimensionalArray<double>)function.Arguments[ChainageArgumentIndex].Values).ToList();
            foreach (var chainage in chainageList.Where(o => o > splitChainage))
            {
                //get values from the old and remove them there
                var roughnessValues = function.GetValues(new VariableValueFilter<double>(function.Arguments[ChainageArgumentIndex], chainage));
                if (roughnessValues.Count == 0)
                {
                    continue;
                }
                //apply a shift 
                double newChainage = chainage - splitChainage;
                //add em to the new function
                newFunction.SetValues(roughnessValues, new VariableValueFilter<double>(newFunction.Arguments[ChainageArgumentIndex], newChainage));
                //remove from old (cannot do this earlier because we use a view)
                function.RemoveValues(new VariableValueFilter<double>(function.Arguments[ChainageArgumentIndex], chainage));
            }
        }

        /// <param name="oldBranch">Old branch for which function is defined</param>
        /// <param name="newBranch">New branch for which to define a function</param>
        /// <returns></returns>
        private IFunction AddSimilarFunctionToNewBranch(IBranch oldBranch, IBranch newBranch)
        {
            var oldFunction = GetRoughnessFunction(oldBranch);
            if (oldFunction == null)
            {
                throw new InvalidOperationException(string.Format("No function defined for branch {0}. Cannot copy.",
                                                                  oldBranch));
            }
            var newFunction = GetRoughnessFunctionType(oldBranch) == RoughnessFunction.FunctionOfH
                                        ? AddHRoughnessFunctionToBranch(newBranch)
                                        : AddQRoughnessFunctionToBranch(newBranch);
            //copy Q or H 
            newFunction.Arguments[QorHArgumentIndex].Values.AddRange(oldFunction.Arguments[QorHArgumentIndex].Values);
            return newFunction;
        }

        private static double EvaluateRoughnessFunction(IFunction function, double splitChainage, double q)
        {
            return function.Evaluate<double>(new VariableValueFilter<double>(function.Arguments[ChainageArgumentIndex], splitChainage),
                                             new VariableValueFilter<double>(function.Arguments[QorHArgumentIndex], q));
        }

        private void HandleBranchOrChainageChanged(INetworkLocation networkLocation)
        {
            var keyValuePair = previousChainages.FirstOrDefault(k => Equals(k.First, networkLocation));
            if (keyValuePair == null)
            {
                throw new InvalidOperationException(string.Format("Unknown network location {0}. Cannot change the chainage/branch.", networkLocation));
            }
            var key = keyValuePair.First;
            if (key == null)
            {
                throw new InvalidOperationException(string.Format("Unknown network location {0}. Cannot change the chainage/branch.", networkLocation));
            }
            UpdateRougnessFunction(key);
            StoreNetworkLocationChainage(key);
        }

        private void StoreNetworkLocationChainage(INetworkLocation networkLocation)
        {
            var existingEntry = previousChainages.FirstOrDefault(k => Equals(k.First, networkLocation));
            var chainage = new DelftTools.Utils.Tuple<IBranch, double>(networkLocation.Branch, networkLocation.Chainage);

            if (existingEntry != null)
            {
                existingEntry.Second = chainage;
            }
            else
            {
                var newEntry = new DelftTools.Utils.Tuple<INetworkLocation, DelftTools.Utils.Tuple<IBranch, double>>(networkLocation, chainage);
                previousChainages.Add(newEntry);
            }
        }

        private void UpdateRougnessFunction(INetworkLocation networkLocation)
        {
            //sorry for this ...cannot use a dictionary because hashcode changes when chainage change...don't want to 'name' the DelftTools.Utils.Tuple..seem overkill?
            var previousBranch = previousChainages.First(t => t.First == networkLocation).Second.First;
            var previousChainage = previousChainages.First(t => t.First == networkLocation).Second.Second;

            //handle network changed
            if (previousBranch != null && previousBranch.Network != networkLocation.Branch.Network)
            {
                return;
            }

            //handle branch changed
            if (previousBranch != networkLocation.Branch)
            {
                var previousBranchRoughnessFunction = GetRoughnessFunction(previousBranch);
                if (previousBranchRoughnessFunction != null)
                {
                    previousBranchRoughnessFunction.Arguments[ChainageArgumentIndex].Values.Remove(previousChainage);
                }
                var roughnessFunction = GetRoughnessFunction(networkLocation.Branch);
                if (roughnessFunction != null)
                {
                    roughnessFunction.Arguments[ChainageArgumentIndex].Values.Add(networkLocation.Chainage);
                }
                return;
            }

            //handle chainage changed
            if (Math.Abs(previousChainage - networkLocation.Chainage) >= epsilon)
            {
                var roughnessFunction = GetRoughnessFunction(networkLocation.Branch);
                if (roughnessFunction != null)
                {
                    //change chainage value of the previous chainage.
                    IMultiDimensionalArray multiDimensionalArray = roughnessFunction.Arguments[ChainageArgumentIndex].Values;
                    var index = multiDimensionalArray.IndexOf(previousChainage);

                    if (index == -1)
                    {
                        index = IndexOfUsingEpsilon(multiDimensionalArray, previousChainage, 0.00001);
                    }

                    multiDimensionalArray[index] = networkLocation.Chainage;
                }
            }
        }

        private int IndexOfUsingEpsilon(IMultiDimensionalArray doubleArray, double value, double eps)
        {
            for(int i = 0; i < doubleArray.Count; i++)
            {
                if (Math.Abs(value-(double)doubleArray[i]) < eps)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the roughness function for a branch.F(x,Q) F(x,H) or null if none defined.
        /// </summary>
        /// <param name="branch"></param>
        /// <returns></returns>
        private IFunction GetRoughnessFunction(IBranch branch)
        {
            switch (GetRoughnessFunctionType(branch))
            {
                case RoughnessFunction.Constant:
                    return null;
                case RoughnessFunction.FunctionOfQ:
                    return FunctionOfQ(branch);
                case RoughnessFunction.FunctionOfH:
                    return FunctionOfH(branch);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Dictionary<IBranch, IFunction> ListToDictionary(IEnumerable<IFunction> value, INetwork network)
        {
            var dictionary = new Dictionary<IBranch, IFunction>();
            if (value == null)
            {
                return null;
            }
            if (network != null)
            {
                foreach (var function in value)
                {
                    var branch = network.Branches.FirstOrDefault(b => b.Name == function.Name);
                    if (branch != null)
                    {
                        dictionary[branch] = function;
                    }
                    else
                    {
                        dictionary.RemoveAllWhere(kv => kv.Value == function);
                    }
                }
            }
            return dictionary;
        }

        private static IList<IFunction> BranchDictionaryToList(IDictionary<IBranch, IFunction> dictionary)
        {
            var result = new List<IFunction>();
            foreach (var branch in dictionary.Keys)
            {
                dictionary[branch].Name = branch.Name;
                result.Add(dictionary[branch]);
            }
            return result;
        }

        private static IDictionary<IBranch, IFunction> ConvertExistingFunctionsToNewNetwork(IDictionary<IBranch, IFunction> functionPerBranch, INetwork newNetwork)
        {
            if (functionPerBranch == null
                || newNetwork == null) return null;

            var convertedFunctionPerBranch = new Dictionary<IBranch, IFunction>();
            foreach (var function in functionPerBranch)
            {
                var matchingBranch = newNetwork.Branches.FirstOrDefault(b => b.Name == function.Key.Name);
                if (matchingBranch != null) convertedFunctionPerBranch[matchingBranch] = function.Value;
            }
            return convertedFunctionPerBranch;
        }

        #region Clone and Copy

        public object Clone()
        {
            var roughnessSection = (RoughnessSection)Activator.CreateInstance(GetType(), true);
            roughnessSection.CopyFrom(this);
            return roughnessSection;
        }

        public virtual void CopyFrom(object source)
        {
            var sourceSection = (RoughnessSection)source;
            RoughnessNetworkCoverage = (RoughnessNetworkCoverage)sourceSection.RoughnessNetworkCoverage.Clone();
            CrossSectionSectionType = sourceSection.CrossSectionSectionType;

            if (network == null)
            {
                return;
            }
            // only update network related data if it has been set. eg when cloning dataitem
            CopyFunctionOfFromRoughnessSection(sourceSection, network);
        }

        public void CopyFunctionOfFromRoughnessSection(RoughnessSection sourceSection, INetwork sourceNetwork)
        {
            foreach (var branch in sourceSection.FunctionOfQPerBranch.Keys.ToArray())
            {
                var targetBranch = sourceNetwork.Branches.First(b => b.Name == branch.Name);
                FunctionOfQPerBranch[targetBranch] = (IFunction)sourceSection.FunctionOfQPerBranch[branch].Clone();
            }
            foreach (var branch in sourceSection.FunctionOfHPerBranch.Keys.ToArray())
            {
                var targetBranch = sourceNetwork.Branches.First(b => b.Name == branch.Name);
                FunctionOfHPerBranch[targetBranch] = (IFunction)sourceSection.FunctionOfHPerBranch[branch].Clone();
            }
        }

        #endregion

        #region Events

        public event RoughnessTypeChangedHandler RoughnessTypeChanged;

        void NetworkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //changes should be handled by EditAction handler
            if (isInKnownEditAction)
                return;

            if (Equals(sender, Network.Branches))
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        // default is constant; thus already ok
                        break;
                    case NotifyCollectionChangedAction.Remove:

                        if (FunctionOfHPerBranch.ContainsKey((IBranch)e.GetRemovedOrAddedItem()))
                        {
                            FunctionOfHPerBranch.Remove((IBranch)e.GetRemovedOrAddedItem());
                        }
                        if (FunctionOfQPerBranch.ContainsKey((IBranch)e.GetRemovedOrAddedItem()))
                        {
                            FunctionOfQPerBranch.Remove((IBranch)e.GetRemovedOrAddedItem());
                        }
                        break;
                    default:
                        throw new NotImplementedException("Branch collection action is not implemented yet: " + e.Action);
                }
            }
        }

        void RoughnessNetworkCoveragePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if we change the default, any branch without a location implicitly changes type. we need to communicate that
            if (!Reversed && Equals(sender, roughnessNetworkCoverage.Components[1]) && e.PropertyName == "DefaultValue")
            {
                var newRoughnessType = GetDefaultRoughnessType();
                var branchesWithoutLocations =
                    RoughnessNetworkCoverage.Network.Branches.Where(
                        b => RoughnessNetworkCoverage.Locations.Values.All(loc => loc.Branch != b)).ToList();

                foreach (var branch in branchesWithoutLocations)
                {
                    OnRoughnessTypeChanged(branch, newRoughnessType);
                }
            }

            if (Equals(sender, roughnessNetworkCoverage) && e.PropertyName == "IsEditing")
            {
                IEditAction coverageEditAction = roughnessNetworkCoverage.CurrentEditAction;
                if (coverageEditAction is BranchSplitAction branchSplitAction)
                {
                    if (roughnessNetworkCoverage.IsEditing)
                    {
                        isInKnownEditAction = true;
                    }
                    else
                    {
                        HandleBranchSplit(branchSplitAction);
                        isInKnownEditAction = false;
                    }
                }

                if (NotifyRoughnessTypeChangedForEmptyBranchesAfterEndEdit && 
                    !roughnessNetworkCoverage.IsEditing)
                    NotifyRoughnessTypeChangedForEmptyBranches();
            }

            if ((sender is INetworkLocation location) && (e.PropertyName == "Chainage" || e.PropertyName == "Branch") && !isInKnownEditAction)
            {
                HandleBranchOrChainageChanged(location);
            }
        }

        private bool NotifyRoughnessTypeChangedForEmptyBranchesAfterEndEdit = false;

        void RoughnessNetworkCoveragePropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if ((sender is INetworkLocation) && (e.PropertyName == "Chainage" || e.PropertyName == "Branch"))
            {
                StoreNetworkLocationChainage((INetworkLocation)sender);
            }
        }

        void CrossSectionSectionTypePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                roughnessNetworkCoverage.UpdateCoverageName(Name);
                //'HACK' make sure a property changed event is fired..so our containing dataitem gets updated
                Name = crossSectionSectionType.Name;
            }
        }

        /// <summary>
        /// If a networklocation is added/removed/modified and the roughness is function of than the function should 
        /// reflect this change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void RoughnessNetworkCoverageValueChanged(object sender, MultiDimensionalArrayChangingEventArgs e)
        {
            if (isInKnownEditAction)
                return;

            //if we change the roughness type in the normal section, send an event so the reverse section can update
            if (!Reversed && Equals(sender, RoughnessNetworkCoverage.RoughnessTypeComponent))
            {
                if (RoughnessTypeChanged == null)
                    return;

                if (e.Action != NotifyCollectionChangeAction.Remove)
                {
                    var location = RoughnessNetworkCoverage.Locations.Values[e.Index];
                    var newType = (RoughnessType)e.Items[0];
                    OnRoughnessTypeChanged(location.Branch, newType);
                }
                else
                {
                    if (RoughnessNetworkCoverage.IsEditing)
                        NotifyRoughnessTypeChangedForEmptyBranchesAfterEndEdit = true;
                    else
                        NotifyRoughnessTypeChangedForEmptyBranches();
                }
                return;
            }

            if (!Equals(sender, RoughnessNetworkCoverage.Locations)) return;

            if (e.Action == NotifyCollectionChangeAction.Replace)
            {
                HandleRoughnessCoverageValueReplace(e);
            }
            else
            {
                //assumes a single value change..
                var networkLocation = (NetworkLocation)e.Items[0];

                var roughnessFunction = GetRoughnessFunction(networkLocation.Branch);
                if (roughnessFunction != null)
                {
                    SyncFunctionWithCoverage(roughnessFunction, e);
                }
            }
        }

        private void NotifyRoughnessTypeChangedForEmptyBranches()
        {
            NotifyRoughnessTypeChangedForEmptyBranchesAfterEndEdit = false;

            //On data removed, if a branch just loses a location, there's nothing to update. If a branch 
            //loses all locations however, implicitly the roughnessType goes back to coverage default.
            var branchesWithoutLocations = roughnessNetworkCoverage.Network.Branches.ToDictionary(b => b, b => true);
            foreach (var loc in RoughnessNetworkCoverage.Locations.Values)
                branchesWithoutLocations[loc.Branch] = false; // has a location, so false
            
            foreach (var branch in branchesWithoutLocations.Keys.Where(b => branchesWithoutLocations[b]))
            {
                OnRoughnessTypeChanged(branch, RoughnessNetworkCoverage.DefaultRoughnessType); //back to default
            }
        }

        private void HandleRoughnessCoverageValueReplace(MultiDimensionalArrayChangingEventArgs e)
        {
            var networkLocation = (NetworkLocation)e.Items[0];
            var coverageLocations = RoughnessNetworkCoverage.Locations.Values;
            //replace is a bit special because we don't know WHAT got replaced. so we do a look of of any missing value 
            //and assume that is the one that got replace

            foreach (var branch in FunctionOfQPerBranch.Keys)
            {
                IBranch branch1 = branch;
                //if the function has a value that is not in the coverage it is the 'missing' value
                var chainages = (IMultiDimensionalArray<double>)FunctionOfQPerBranch[branch].Arguments[0].Values;
                var missingChainage = chainages
                    .Where(c => !coverageLocations.Contains(new NetworkLocation(branch1, c)))
                    .DefaultIfEmpty(double.NaN).First();

                if (!double.IsNaN(missingChainage))
                {
                    var function = FunctionOfQPerBranch[branch1];
                    var index = chainages.IndexOf(missingChainage);

                    //the chainage is missing..is it replaced by a value on same branch? 
                    //->update chainage. Otherwise delete it
                    if (networkLocation.Branch == branch1)
                    {
                        //replace the old chainage with the new
                        function.Arguments[0].Values[index] = networkLocation.Chainage;
                    }
                }
            }

            //sync all functions with the coverage.location might have migrated to other branch. then we lose the value but have to update the functions
            foreach (var kvp in FunctionOfQPerBranch.Concat(FunctionOfHPerBranch))
            {
                var chainages = (IMultiDimensionalArray<double>)kvp.Value.Arguments[0].Values;
                var branch = kvp.Key;
                //remove anything we can't find in coverage
                var removedChainages = chainages.Where(c => !coverageLocations.Contains(new NetworkLocation(branch, c))).ToList();
                foreach (var c in removedChainages)
                {
                    chainages.Remove(c);
                }

                //add chainages for locations not found in the function.
                //incorrect! might by Q(h) or F(q)
                var addedChainages = coverageLocations.Where(c => c.Branch == branch).Select(l => l.Chainage)
                                                      .Where(o => !chainages.Contains(o));
                foreach (var c in addedChainages)
                {
                    chainages.Add(c);
                }
            }
        }

        public delegate void RoughnessTypeChangedHandler(IBranch branch, RoughnessType newType);

        private void SyncFunctionWithCoverage(IFunction functionOf, MultiDimensionalArrayChangingEventArgs e)
        {
            if (RoughnessNetworkCoverage.IsSorting)
                return;

            double chainage = ((NetworkLocation)e.Items[0]).Chainage;
            if (e.Action == NotifyCollectionChangeAction.Add && 
                !functionOf.Arguments[ChainageArgumentIndex].Values.Contains(chainage))
            {
                //check might be expensive...but needed because importer add values to coverage AFTER the function is set
                functionOf.Arguments[ChainageArgumentIndex].Values.Add(chainage);
            }

            if (e.Action == NotifyCollectionChangeAction.Remove)
            {
                functionOf.Arguments[ChainageArgumentIndex].Values.Remove(chainage);
            }
            if (e.Action == NotifyCollectionChangeAction.Replace)
            {
                //we know something got replaced but do not now the old value..
            }
        }

        private void OnRoughnessTypeChanged(IBranch branch, RoughnessType newType)
        {
            if (RoughnessTypeChanged != null)
            {
                RoughnessTypeChanged(branch, newType);
            }
        }

        #endregion

        #region NHibernate

        /// used for for NHibernate mapping to database
        private IList<IFunction> RoughnessFunctionOfQ
        {
            get { return BranchDictionaryToList(FunctionOfQPerBranch); }
            set
            {
                roughnessFunctionOfQ = value;
                functionOfQPerBranchInitialized = false;
            }
        }

        /// <summary>
        /// used for for NHibernate mapping to database
        /// </summary>
        private IList<IFunction> RoughnessFunctionOfH
        {
            get { return BranchDictionaryToList(FunctionOfHPerBranch); }
            set
            {
                roughnessFunctionOfH = value;
                functionOfHPerBranchInitialized = false;
            }
        }
        #endregion

        #region IEditableObject

        [NoNotifyPropertyChange]
        public virtual IEditAction CurrentEditAction { get { return (editActions.Count > 0) ? editActions.Peek() : null; } }

        [NoNotifyPropertyChange]
        public virtual bool EditWasCancelled { get; protected set; }

        public virtual bool IsEditing { get; protected set; }

        public virtual void BeginEdit(string action)
        {
            BeginEdit(new DefaultEditAction(action));
        }

        public virtual void BeginEdit(IEditAction action)
        {
            editActions.Push(action);
            EditWasCancelled = false;
            IsEditing = true;
        }

        public virtual void CancelEdit()
        {
            EditWasCancelled = true;
            IsEditing = false;
            editActions.Pop();
        }

        public virtual void EndEdit()
        {
            IsEditing = false;
            editActions.Pop();
        }

        #endregion
    }
}
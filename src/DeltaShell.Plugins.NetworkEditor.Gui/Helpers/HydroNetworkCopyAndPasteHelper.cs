using System;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Helpers
{
    public static class HydroNetworkCopyAndPasteHelper
    {
        private const string clipBoardFormat = "CopiedNetworkFeature";
        private const string clipBoardText = "Copied network feature";

        private static INetworkFeature copiedNetworkFeature;

        /// <summary>
        /// Sets <see cref="copiedNetworkFeature"/> to null.
        /// </summary>
        public static void ReleaseCopiedNetworkFeature()
        {
            if (copiedNetworkFeature != null)
            {
                copiedNetworkFeature = null;
                Clipboard.Clear();
            }
        }

        /// <summary>
        /// Sets <paramref name="networkFeature"/> to the clipboard.
        /// </summary>
        /// <param name="networkFeature">The network feature that must be set to the clipboard.</param>
        public static void SetNetworkFeatureToClipBoard(INetworkFeature networkFeature)
        {
            ReleaseCopiedNetworkFeature();

            if (networkFeature == null) return;

            Clipboard.SetData(clipBoardFormat, clipBoardText);
            copiedNetworkFeature = networkFeature;
        }

        /// <summary>
        /// Gets the channel that is set to the clipboard.
        /// </summary>
        /// <returns>The channel that is set to the clipboard, or null if no channel is set to the clipboard.</returns>
        public static IChannel GetChannelFromClipBoard()
        {
            return IsChannelSetToClipBoard() ? copiedNetworkFeature as IChannel : null;
        }

        /// <summary>
        /// Gets the branch feature that is set to the clipboard.
        /// </summary>
        /// <returns>The branch feature that is set to the clipboard, or null if no branch feature is set to the clipboard.</returns>
        public static IBranchFeature GetBranchFeatureFromClipBoard()
        {
            return IsBranchFeatureSetToClipBoard() ? copiedNetworkFeature as IBranchFeature : null;
        }

        /// <summary>
        /// Checks whether a channel is set to the clipboard or not.
        /// </summary>
        /// <returns>If a channel is set to the clipboard.</returns>
        public static bool IsChannelSetToClipBoard()
        {
            return IsNetworkFeatureSetToClipBoard() && copiedNetworkFeature is IChannel;
        }

        /// <summary>
        /// Checks whether a branch feature is set to the clipboard or not.
        /// </summary>
        /// <returns>If a branch feature is set to the clipboard.</returns>
        public static bool IsBranchFeatureSetToClipBoard()
        {
            return IsNetworkFeatureSetToClipBoard() && copiedNetworkFeature is IBranchFeature;
        }

        /// <summary>
        /// Checks whether a network feature is set to the clipboard or not.
        /// </summary>
        /// <returns>If a network feature is set to the clipboard.</returns>
        private static bool IsNetworkFeatureSetToClipBoard()
        {
            var clipBoardFeature = Clipboard.GetData(clipBoardFormat);
            
            return clipBoardFeature != null
                   && clipBoardFeature.ToString().Equals(clipBoardText)
                   && copiedNetworkFeature != null;
        }

        /// <summary>
        /// Checks whether <paramref name="crossSection"/> is pastable in <paramref name="network"/> (according to the specified CrossSectionSectionTypes).
        /// </summary>
        /// <param name="network">The network used for evaluation.</param>
        /// <param name="crossSection">The cross section used for evaluation.</param>
        /// <param name="errorMessage">An error message that might result from the evaluation.</param>
        /// <returns>If <paramref name="crossSection"/> is pastable in <paramref name="network"/>.</returns>
        /// <exception cref="ArgumentException">If an argument is null.</exception>
        public static bool IsCrossSectionPastableInNetwork(IHydroNetwork network, ICrossSection crossSection, out string errorMessage)
        {
            if (network == null) throw new ArgumentException("network is null");
            if (crossSection== null) throw new ArgumentException("crossSection is null");

            
            var definition = crossSection.Definition;
            if ((definition.IsProxy) && (crossSection.Network != network))
            {
                errorMessage =
                    string.Format(
                        "Can not paste cross section with name \"{0}\" because it uses a shared definition of other network.",
                        crossSection.Name);
                return false;
            }

            if (definition.GeometryBased)
            {
                errorMessage =
                    string.Format(
                        "Can not paste cross section with name \"{0}\" because it is geometry based.",
                        crossSection.Name);
                return false;
            }

            var sourceCrossSectionSectionTypeNames = definition.Sections.Select(s => s.SectionType.Name).Distinct();
            var networkCrossSectionSectionTypeNames = network.CrossSectionSectionTypes.Select(st => st.Name).Distinct();
            var undefinedCrossSectionSectionTypeNames = sourceCrossSectionSectionTypeNames.Where(n => !networkCrossSectionSectionTypeNames.Contains(n));
            var undefinedCrossSectionSectionTypeNamesNo = undefinedCrossSectionSectionTypeNames.Count();

            errorMessage = "";

            if (undefinedCrossSectionSectionTypeNamesNo != 0)
            {
                if (undefinedCrossSectionSectionTypeNamesNo == 1)
                {
                    errorMessage = String.Format("Cannot paste the cross section with name \"{0}\" in this network because a SectionType with the following name is missing: {1}",
                            definition.Name, undefinedCrossSectionSectionTypeNames.ElementAt(0));
                }
                else
                {
                    errorMessage = String.Format("Cannot paste the cross section with name \"{0}\" in this network because SectionTypes with the following name are missing: ",
                        definition.Name);

                    for (var i = 0; i < undefinedCrossSectionSectionTypeNamesNo; i++)
                    {
                        errorMessage += undefinedCrossSectionSectionTypeNames.ElementAt(i);

                        if (i != undefinedCrossSectionSectionTypeNamesNo - 1)
                        {
                            errorMessage += ", ";
                        }
                    }
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Adapts <paramref name="crossSection"/> to <paramref name="targetNetwork"/> (when it comes to the specified CrossSectionSectionTypes).
        /// </summary>
        /// <param name="targetNetwork">The network to adapt <paramref name="crossSection"/> to.</param>
        /// <param name="crossSection">The cross section to adapt.</param>
        /// <returns>If <paramref name="crossSection"/> is successfully adapted to <paramref name="targetNetwork"/>.</returns>
        /// <exception cref="ArgumentException">If an argument is null.</exception>
        /// <remarks>If false is returned, <param name="crossSection"/> is left unchanged.</remarks>
        public static bool AdaptCrossSectionBeforePastingInNetwork(IHydroNetwork targetNetwork, ICrossSection crossSection)
        {
            if (targetNetwork == null) throw new ArgumentException("network is null");
            if (crossSection == null) throw new ArgumentException("crossSection is null");

            if (crossSection.Definition.IsProxy)
            {
                var innerDefinition = ((CrossSectionDefinitionProxy)crossSection.Definition).InnerDefinition;
                if (!targetNetwork.SharedCrossSectionDefinitions.Contains(innerDefinition))
                {
                    crossSection.MakeDefinitionLocal();
                }
            }

            var crossSectionDefinition = crossSection.Definition;
            var sectionTypeNames = crossSectionDefinition.Sections.Select(s => s.SectionType.Name).Distinct();

            // Check whether the crossSection is adaptable or not (according to the specified CrossSectionSectiontypes)
            var networkSectionTypeNames = targetNetwork.CrossSectionSectionTypes.Select(st => st.Name).Distinct();
            if (!sectionTypeNames.All(networkSectionTypeNames.Contains)) return false;

            var nameDictionary = sectionTypeNames.ToDictionary(name => name,
                                                               name =>
                                                               targetNetwork.CrossSectionSectionTypes.First(
                                                                   st => st.Name == name));

            foreach (var section in crossSectionDefinition.Sections)
            {
                section.SectionType = nameDictionary[section.SectionType.Name];
            }

            return true;
        }

        /// <summary>
        /// Pastes the channel on the clipboard to <paramref name="network"/>.
        /// </summary>
        /// <param name="network">The network to paste the clipboard channel to.</param>
        /// <param name="errorMessage">An error message that might result from the paste action.</param>
        /// <returns>If the clipboard channel is successfully pasted to <paramref name="network"/>.</returns>
        /// <exception cref="ArgumentException">If an argument is null.</exception>
        /// <remarks>The method checks/adapts the cross sections of the clipboard channel. If a cross section cannot be pasted to <paramref name="network"/>, the channel as a whole will not be pasted to the network.</remarks>        
        public static bool PasteChannelToNetwork(IHydroNetwork network, out string errorMessage)
        {
            if (network == null) throw new ArgumentException("network is null");
            errorMessage = "";

            var channel = GetChannelFromClipBoard();
            if (channel == null)
            {
                errorMessage = "No branch is copied";
                return false;
            }

            var clonedChannel = (IChannel) channel.Clone();

            // Adapt the cross sections to the target network
            foreach (var localCrossSection in clonedChannel.CrossSections)
            {
                var crossSection = localCrossSection;
                if (AdaptCrossSectionBeforePastingInNetwork(network, crossSection)) continue;

                IsCrossSectionPastableInNetwork(network, crossSection, out errorMessage);
                errorMessage = "Cannot paste the channel because one of its features cannot be pasted:\n\n" + errorMessage;
                return false;
            }

            // If pasting into the same network, move the pasted channels a bit, so the new channels can be distinguished. 
            // If pasting into another network, the coordinates should remain as they were. 
            int delta = (channel.Network == network) ? 100 : 0; 

            // Change the geometry of all branch features
            foreach (var branchFeature in clonedChannel.BranchFeatures)
            {
                for (var i = 0; i < branchFeature.Geometry.Coordinates.Count(); i++)
                {
                    GeometryHelper.MoveCoordinate(branchFeature.Geometry, i, delta, delta);
                }

                branchFeature.Geometry = branchFeature.Geometry.Coordinates.Count() == 1
                                             ? (IGeometry) new Point(branchFeature.Geometry.Coordinate)
                                             : new LineString(branchFeature.Geometry.Coordinates);
            }

            // Change the geometry of the channel itself
            for (var i = 0; i < clonedChannel.Geometry.Coordinates.Count(); i++)
            {
                GeometryHelper.MoveCoordinate(clonedChannel.Geometry, i, delta, delta);
            }

            clonedChannel.Geometry = new LineString(clonedChannel.Geometry.Coordinates);
            
            // Update the name of the channel
            clonedChannel.Name = HydroNetworkHelper.GetUniqueFeatureName(network, clonedChannel, true);

            // Update the name of all branch features
            foreach (var branchFeature in clonedChannel.BranchFeatures)
            {
                branchFeature.Name =  HydroNetworkHelper.GetUniqueFeatureName(network, branchFeature, true);
            }

            // Use HydroNetworkHelper to add the channel to the network (this way a source node and a target node are generated automatically)
            var wasAlreadyEditing = network.IsEditing;
            try
            {
                if (!wasAlreadyEditing)
                {
                    network.BeginEdit("Paste channel");
                }

                NetworkHelper.AddChannelToHydroNetwork(network, clonedChannel);

                if (!wasAlreadyEditing)
                {
                    network.EndEdit();
                }
            }
            catch (Exception exception)
            {
                if (network.IsEditing)
                {
                    network.CancelEdit();
                }
                errorMessage = String.Format("Unable to paste channel: {0}", exception.Message);
                return false;
            } 

            return true;
        }

        /// <summary>
        /// Pastes the branch feature on the clipboard to <paramref name="channel"/>.
        /// </summary>
        /// <param name="channel">The channel to paste the clipboard branch feature to.</param>
        /// <param name="chainage"></param>
        /// <param name="errorMessage">An error message that might result from the paste action.</param>
        /// <returns>If the clipboard branch feature is successfully pasted to <paramref name="channel"/>.</returns>
        /// <exception cref="ArgumentException">If an argument is null.</exception>
        /// <remarks>The method checks/adapts cross sections. If a cross section cannot be pasted to the network of <paramref name="channel"/>, the paste action as a whole is aborted and <paramref name="errorMessage"/> is set.</remarks>
        public static bool PasteBranchFeatureFromClipboardToBranch(IChannel channel, double chainage, out string errorMessage)
        {
            if (channel == null) throw new ArgumentException("channel is null");
            

            var branchFeature = GetBranchFeatureFromClipBoard();
            if (branchFeature == null)
            {
                errorMessage = "No branch feature in clipboard";
                return false;
            }

            return PastBranchFeatureToBranch(channel, branchFeature, chainage,  out errorMessage);
        }

        public static bool PastBranchFeatureToBranch(IChannel channel, IBranchFeature branchFeature, double chainage, out string errorMessage)
        {
            errorMessage = "";

            //we are we not just cloning here?!?!
            var newBranchFeature = (IBranchFeature) Activator.CreateInstance(branchFeature.GetEntityType());
            newBranchFeature.Branch = channel;
            var targetNetwork = channel.HydroNetwork;
            newBranchFeature.Network = targetNetwork;
            newBranchFeature.Chainage = chainage;
            newBranchFeature.CopyFrom(branchFeature);
            
            //update name only if needed
            if (targetNetwork.BranchFeatures.Any(bf => bf.Name == newBranchFeature.Name) || string.IsNullOrEmpty(newBranchFeature.Name))
            {
                newBranchFeature.Name = HydroNetworkHelper.GetUniqueFeatureName(targetNetwork, newBranchFeature);
            }
            var lengthIndexedLine = new LengthIndexedLine(channel.Geometry);
            var newPointGeometry = new Point((Coordinate) lengthIndexedLine.ExtractPoint(chainage).Clone());

            if (newBranchFeature is ICrossSection)
            {
                var newCrossSection = (ICrossSection)newBranchFeature;

                if(!AdaptCrossSectionBeforePastingInNetwork(targetNetwork, newCrossSection))
                {
                    IsCrossSectionPastableInNetwork(targetNetwork, newCrossSection, out errorMessage);
                    return false;
                }
                
                channel.BranchFeatures.Add(newCrossSection);
                return true;
            }
            if (newBranchFeature is LateralSource || newBranchFeature is IObservationPoint || newBranchFeature is IRetention)
            {
                newBranchFeature.Geometry = newPointGeometry;
                channel.BranchFeatures.Add(newBranchFeature);
                return true;
            }
            if (newBranchFeature is CompositeBranchStructure)
            {
                var newCompositeBranchStructure = (CompositeBranchStructure)newBranchFeature;
                newCompositeBranchStructure.Geometry = newPointGeometry;
                channel.BranchFeatures.Add(newCompositeBranchStructure);
                PasteStructuresFromSourceToNewCompositeStructure(branchFeature, channel, newCompositeBranchStructure);
                return true;
            }
            if (newBranchFeature is IStructure1D)
            {
                var newStructure = (IStructure1D) newBranchFeature;
                newStructure.Geometry = newPointGeometry;
                HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(newStructure, newStructure.Branch);
                return true;
            }
            errorMessage = "Pasting failed; unknown feature";
            return false;
        }

        /// <summary>
        /// Pastes the branch feature on the clipboard into <paramref name="branchFeature"/>.
        /// </summary>
        /// <param name="branchFeature">The branch feature to paste the clipboard branch feature into.</param>
        /// <param name="errorMessage">An error message that might result from the paste into action.</param>
        /// <returns>If the clipboard branch feature is successfully pasted into <paramref name="branchFeature"/>.</returns>
        /// <exception cref="ArgumentException">If an argument is null.</exception>
        /// <remarks>The method checks/adapts cross sections. If a cross section cannot be pasted to the network of <paramref name="branchFeature"/>, the paste action as a whole is aborted and <paramref name="errorMessage"/> is set.</remarks>
        public static bool PasteBranchFeatureIntoBranchFeature(IBranchFeature branchFeature, out string errorMessage)
        {
            if (branchFeature == null) throw new ArgumentException("branchFeature is null");
            errorMessage = "";

            var sourceFeature = GetBranchFeatureFromClipBoard();
            if (sourceFeature == null)
            {
                errorMessage = "No branch feature is copied";
                return false;
            }

            if (sourceFeature is ICrossSection)
            {
                var crossSectionClone = (ICrossSection)sourceFeature.Clone();

                if (!AdaptCrossSectionBeforePastingInNetwork((IHydroNetwork) branchFeature.Network, crossSectionClone))
                {
                    IsCrossSectionPastableInNetwork((IHydroNetwork)branchFeature.Network, crossSectionClone, out errorMessage);
                    return false;
                }

                sourceFeature = crossSectionClone;
            }

            if (branchFeature.GetEntityType() == sourceFeature.GetEntityType())
            {
                branchFeature.CopyFrom(sourceFeature);
            }
            else
            {
                errorMessage = "The copied branch feature is of a different type as the target branch feature";
                return false;
            }

            return true;
        }

        public static void PasteCompositeStructureToBranch(IBranchFeature source, IFeature newBranchFeature)
        {
            var newCompositeBranchStructure = (CompositeBranchStructure) newBranchFeature;
            PasteStructuresFromSourceToNewCompositeStructure(source, newCompositeBranchStructure.Branch, newCompositeBranchStructure);
        }

        private static void PasteStructuresFromSourceToNewCompositeStructure(IBranchFeature source, IBranch branch, CompositeBranchStructure newCompositeBranchStructure)
        {
            foreach (var structure in ((CompositeBranchStructure) source).Structures)
            {
                var newStructure = (IBranchFeature)Activator.CreateInstance(structure.GetEntityType());
                newStructure.Branch = branch;
                newStructure.Network = branch.Network;
                newStructure.CopyFrom(structure);

                newStructure.Name = HydroNetworkHelper.GetUniqueFeatureName((IHydroNetwork) branch.Network, newStructure, true);
                
                if (newStructure.Geometry == null)
                {
                    newStructure.Geometry = newCompositeBranchStructure.Geometry;
                }
                
                HydroNetworkHelper.AddStructureToComposite(newCompositeBranchStructure, (IStructure1D)newStructure);
            }
        }
    }
}

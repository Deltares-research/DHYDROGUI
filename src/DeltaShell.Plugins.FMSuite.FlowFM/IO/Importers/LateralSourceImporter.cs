using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    /// <summary>
    /// <see cref="LateralSourceImporter"/> extends the <see cref="TimeSeriesCsvImporter"/>
    /// such that it can be used for lateral sources.
    /// </summary>
    /// <seealso cref="TimeSeriesCsvImporter"/>
    public class LateralSourceImporter : TimeSeriesCsvImporter
    {
        private readonly ILog log;

        /// <summary>
        /// Creates a new default <see cref="LateralSourceImporter"/>.
        /// </summary>
        public LateralSourceImporter() 
            : this(new LateralSourceFileImporter(), LogManager.GetLogger(typeof(LateralSourceImporter))) { }

        /// <summary>
        /// Creates a new <see cref="LateralSourceImporter"/> with the given <paramref name="fileImporter"/> and
        /// <paramref name="logger"/>.
        /// </summary>
        /// <param name="fileImporter">The file importer used within this class.</param>
        /// <param name="logger">The logger used within this class.</param>
        public LateralSourceImporter(LateralSourceFileImporter fileImporter, ILog logger)
        {
            // Note that we explicitly require the fileImporter to be a LateralSourceFileImporter,
            // in order to ensure the LateralSourceFileImporter property works as expected.

            Ensure.NotNull(fileImporter, nameof(fileImporter));
            Ensure.NotNull(logger, nameof(logger));

            FileImporter = fileImporter;
            log = logger;
        }

        public override string Name => "Flow1D CSV Importer";
        
        // Unfortunately the FileImporter property is not overridable, as such we cannot forward it to a
        // new more specific property. As such, we are casting the FileImporter to a LateralSourceFileImporter,
        // This is possible because the FileImporter is always initialized as a LateralSourceFileImporter in the
        // constructor.
        private LateralSourceFileImporter LateralSourceFileImporter => FileImporter as LateralSourceFileImporter;

        public override IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IEventedList<Model1DLateralSourceData>);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="BoundaryRelationType"/> of this <see cref="LateralSourceImporter"/>.
        /// </summary>
        public BoundaryRelationType BoundaryRelationType
        {
            get => LateralSourceFileImporter.BoundaryRelationType;
            set => LateralSourceFileImporter.BoundaryRelationType = value;
        }

        /// <summary>
        /// This function is called when the CSV import dialog has finished selecting the data to import. It will call the
        /// <see cref="Importers.LateralSourceFileImporter"/> to import and format the CSV data, and replace existing values with imported values
        /// </summary>
        /// <param name="path">A custom path value passed to the base.ImportItem() call. Note: if no <see cref="path"/> is provided,
        /// the <see cref="TimeSeriesCsvImporter.FilePath"/> property of the base class is used</param>
        /// <param name="target">The target object to import the new LateralSources in to</param>
        /// <returns>The updated <see cref="target"/> object with imported data.
        /// Produces an Warning log if there was no data to be overwritten.
        /// Produces an Error log if the target is of no suitable type</returns>
        public override object ImportItem(string path, object target)
        {
            // base.ImportItem() will call the LateralSourceFileImporter and will read and format the CSV data
            List<IFunction> functionList = ((IEnumerable<IFunction>) base.ImportItem(path, target)).ToList();

            if (!(target is IEventedList<Model1DLateralSourceData> targetAsLateralList))
            {
                log.ErrorFormat(Resources.LateralSourceImporter_Error_Occured_While_Setting__0__to_target__1_, functionList, target);
                return target;
            }
            
            foreach (IFunction function in functionList)
            {
                var overwritten = false;
                foreach (Model1DLateralSourceData lateralSourceDataItem in targetAsLateralList)   
                {
                    if (function.Name == lateralSourceDataItem.Feature.Name)
                    {
                        UpdateLateralSourceData(lateralSourceDataItem, function);
                        overwritten = true;
                        break;
                    }
                }

                if (!overwritten)
                {
                    log.WarnFormat(Resources.LateralSourceImporter_Could_Not_Find_Suitable_Target_For__0_, function.Name);
                }
            }
        
            return target;
        }

        private void UpdateLateralSourceData(Model1DLateralSourceData existingData, IFunction newData)
        {
            switch (LateralSourceFileImporter.BoundaryRelationType)
            {
                case BoundaryRelationType.Q:
                    existingData.DataType = Model1DLateralDataType.FlowConstant;
                    existingData.Flow = (double) newData.Components[0].Values[0];
                    break;
                case BoundaryRelationType.Qh:
                case BoundaryRelationType.Qt:
                    existingData.Data = newData;
                    // The lateral source data will automatically recognise whether it is Q(h) or Q(t), depending on the datatype of the argument (double or DateTime)
                    // It will change the existingData.DataType accordingly. 
                    break;
                case BoundaryRelationType.H:
                case BoundaryRelationType.Ht:
                default:
                    break;
            } 
        }
    }
}
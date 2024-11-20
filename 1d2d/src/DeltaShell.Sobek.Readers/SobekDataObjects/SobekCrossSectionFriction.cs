using System;
using System.Collections.Generic;
using DelftTools.Hydro;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class SobekFrictionSegment
    {
        public double Start { get; set; }
        public double End { get; set; }
        public double Friction { get; set; }
        public RoughnessType FrictionType { get; set; }
    }


    /// <summary>
    /// Negative Friction not supported TOOLS-1565
    /// </summary>
    public class SobekCrossSectionFriction
    {
        private readonly IList<SobekFrictionSegment> friction;

        private int addFrictionValueIndex;

        public SobekCrossSectionFriction()
        {
            friction = new List<SobekFrictionSegment>();
        }

        public string ID { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Id of the cross section DEFINITION the friction data applies
        /// </summary>
        public string CrossSectionID { get; set; }

        public IList<SobekFrictionSegment> Segments
        {
            get { return friction; }
        }

        /// <summary>
        /// row = double as type, double as value 
        /// </summary>
        /// <param name="row"></param>
        public void AddFrictionValues(object row)
        {
            int flowdirection = 0;
            object[] frictionTypeAndValue = ((System.Data.DataRow)(row)).ItemArray;
            string type =  ((double) frictionTypeAndValue[0]).ToString();
            if (flowdirection == 0)
            {
                friction[addFrictionValueIndex].FrictionType = (RoughnessType)Enum.Parse(typeof(RoughnessType), type);
                friction[addFrictionValueIndex].Friction = (double)frictionTypeAndValue[1]; 
            }
            addFrictionValueIndex++;
        }

        /// <summary>
        /// row = double as start, double as end
        /// </summary>
        /// <param name="row"></param>
        public void AddYSections(object row)
        {
            object[] array = ((System.Data.DataRow)(row)).ItemArray;
            friction.Add(new SobekFrictionSegment
            {
                Start = (double)array[0],
                End = (double)array[1]
            });
        }

        /// <summary>
        /// Used for administration: if no CRFR for yz-section available the main friction of the branch will be used
        /// </summary>
        public bool IsSameAsMainFriction { get; set; }

       

        
    }
}